// -----
// MIT License
//
// Copyright (c) 2005-2026 Harry Ho
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// -----

using System;
using System.Collections.Generic;
using System.Text;




namespace fxpa
{
    public class RemoteSimulationDataProvider : RemoteDataProvider, ITimeControl
    {
        int _initialAvailablePeriodsCount = 12;
        public int InitialAvailablePeriodsCount
        {
            get { lock (this) { return _initialAvailablePeriodsCount; } }
            set { lock (this) { _initialAvailablePeriodsCount = value; } }
        }

        List<BarData> _totalUnits = new List<BarData>();

        #region ITimeControl Members

        /// <summary>
        /// The current object implements time control, extending the default remote data provider.
        /// </summary>
        public override ITimeControl TimeControl
        {
            get { lock (this) { return this; } }
        }

        public int PeriodsCount
        {
            get { lock (this) { return _totalUnits.Count; } }
        }

        public int CurrentPeriod
        {
            get { lock (this) { return _dataUnits.Count; } }
        }

        public TimeSpan Interval
        {
            get { return base.TimeInterval; }
        }

        #endregion

        public override double Ask
        {
            get
            {
                lock (this)
                {
                    if (CurrentDataUnit == null)
                    {
                        return double.NaN;
                    }

                    return CurrentDataUnit.Value.Close;
                }
            }
        }

        public override double Bid
        {
            get
            {
                lock (this)
                {
                    if (CurrentDataUnit == null)
                    {
                        return double.NaN;
                    }

                    return CurrentDataUnit.Value.Close - Spread;
                }
            }
        }

        double _spread = 0;
        public override double Spread
        {
            get
            {
                lock (this)
                {
                    return _spread;
                }
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public RemoteSimulationDataProvider()
        {
        }

        bool DoStepForward(int stepsRemaining)
        {
            lock (this)
            {
                if (_dataUnits.Count == _totalUnits.Count)
                {
                    return false;
                }

                _dataUnits.Add(_totalUnits[_dataUnits.Count]);
            }

            RaiseValuesUpdateEvent(UpdateType.NewBar, 1, stepsRemaining);
            return true;
        }

        public bool StepForward()
        {
            return DoStepForward(0);
        }

        public bool StepBack()
        {
            lock (this)
            {
                return false;
            }
        }

        public bool StepTo(int index)
        {
            if (index < CurrentPeriod)
            {
                return false;
            }

            // This is before the start.
            int current = CurrentPeriod;
            for (int i = current; i < index; i++)
            {
                // Last step is not marked as multi step to indicate stepping is over.
                if (DoStepForward(index - 1 - i) == false)
                {
                    return false;
                }
            }
            return true;
        }

        //[MessageReceiver]
        //protected override void Receive(TradingValuesUpdateMessage message)
        //{
        //    BarData[] inputDataUnits = message.GenerateDataUnits();
        //    double ask = message.Ask;
        //    double bid = message.Bid;

        //    int updatedItemsCount = 1;
        //    UpdateType updateType = UpdateType.Quote;
        //    lock(this)
        //    {
        //        if (_totalUnits.Count == 0)
        //        {
        //            updateType = UpdateType.Initial;
        //            updatedItemsCount = inputDataUnits.Length;
        //        }

        //        _lastUpdateDateTime = DateTime.Now;
        //        if (inputDataUnits.Length > 0)
        //        {// In this scenario spread is a fixed percentage of the initial bar close price.
        //            int symbolsCount = 5;
        //            if (inputDataUnits[0].Close < 1)
        //            {
        //                symbolsCount = 4;
        //            }
        //            double roundValue1 = MathHelper.RoundToSymbolsCount(inputDataUnits[0].Close, symbolsCount);
        //            double roundValue2 = MathHelper.RoundToSymbolsCount(inputDataUnits[0].Close * 1.0005, symbolsCount);
        //            _spread = roundValue2 - roundValue1;
        //        }
        //        else
        //        {
        //            _spread = 0;
        //        }
        //        //_ask = ask;
        //        //_bid = bid;

        //        for (int i = 0; i < inputDataUnits.Length; i++)
        //        {
        //            if (_totalUnits.Count == 0 || inputDataUnits[i].DateTime > _totalUnits[_totalUnits.Count - 1].DateTime)
        //            {
        //                if (updateType == UpdateType.Quote)
        //                {
        //                    updateType = UpdateType.NewBar;
        //                }

        //                if (updatedItemsCount != inputDataUnits.Length)
        //                {
        //                    updatedItemsCount++;
        //                }

        //                _totalUnits.Add(inputDataUnits[i]);
        //            }
        //        }

        //        // Also check the last 5 units for any requotes that might have been sent,
        //        // this happens when price changes and we get updates for the last unit.
        //        for (int i = 0; i < 5 && inputDataUnits.Length - 1 - i > 0 && _totalUnits.Count - 1 - i > 0; i++)
        //        {
        //            if (inputDataUnits[inputDataUnits.Length - 1 - i].DateTime == _totalUnits[_totalUnits.Count - 1 - i].DateTime
        //                && inputDataUnits[inputDataUnits.Length - 1 - i].Equals(_totalUnits[_totalUnits.Count - 1 - i]) == false)
        //            {
        //                _totalUnits[_totalUnits.Count - 1 - i] = inputDataUnits[inputDataUnits.Length - 1 - i];
        //                if (updateType == UpdateType.Quote)
        //                {
        //                    updateType = UpdateType.BarsUpdated;
        //                }

        //                if (updatedItemsCount != inputDataUnits.Length)
        //                {
        //                    updatedItemsCount++;
        //                }
        //            }
        //        }
        //    } // lock(this)

        //    if (updateType == UpdateType.Initial)
        //    {// Start initially with _initialAvailablePeriods periods count.
        //        lock (this)
        //        {
        //            for (int i = 0; i < _initialAvailablePeriodsCount && i < _totalUnits.Count; i++)
        //            {
        //                _dataUnits.Add(_totalUnits[i]);
        //            }
        //        }

        //        if (_operationalState == OperationalStateEnum.Initialized)
        //        {
        //            _operationalState = OperationalStateEnum.Operational;
        //            RaiseOperationalStatusChangedEvent();
        //        }

        //        RaiseValuesUpdateEvent(UpdateType.Initial, _dataUnits.Count, 0);
        //    }
            
        //}

    }
}
