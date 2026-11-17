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

using System.Reflection;

using System.Collections.ObjectModel;

namespace fxpa
{
    public sealed class AnalyzerSession
    {
        Symbol symbol;

        public Symbol Symbol
        {
            get { return symbol; }
            set { symbol = value; }
        }

        SessionInfo _sessionInfo;
        public SessionInfo SessionInfo
        {
            get { return _sessionInfo; }
        }

        IDataProvider _dataProvider;
        public IDataProvider DataProvider
        {
            get { return _dataProvider; }
        }

        IOrderExecutionProvider _orderExecutionProvider;
        public IOrderExecutionProvider OrderExecutionProvider
        {
            get { return _orderExecutionProvider; }
        }

        ListEx<Indicator> _indicators = new ListEx<Indicator>();
        public ReadOnlyCollection<Indicator> Indicators
        {
            get { return _indicators.AsReadOnly(); }
        }

        public bool IsInitialized
        {
            get { return _dataProvider != null; }
        }

        #region IOperational Members

        OperationalStateEnum _operationalState = OperationalStateEnum.UnInitialized;
        
        public OperationalStateEnum OperationalState
        {
            get { lock (this) { return _operationalState; } }
        }

        //public event GeneralHelper.GenericDelegate<IOperational> OperationalStatusChangedEvent;

        #endregion

        public delegate void IndicatorUpdateDelegate(AnalyzerSession session, Indicator indicator);
        public event IndicatorUpdateDelegate IndicatorAddedEvent;
        public event IndicatorUpdateDelegate IndicatorRemovedEvent;

        /// <summary>
        /// 
        /// </summary>
        public AnalyzerSession(SessionInfo sessionInfo)
        {
            _sessionInfo = sessionInfo;
            symbol = AppUtil.ParseSymbol(sessionInfo.Symbol);
        }

        public bool Initialize(IDataProvider dataProvider, IOrderExecutionProvider orderExectioner)
        {
            lock (this)
            {
                _dataProvider = dataProvider;

                _operationalState = OperationalStateEnum.Initialized;
            }

            //GeneralHelper.SafeEventRaise(OperationalStatusChangedEvent, this);
            return true;
        }

        public bool Initialize(IDataProvider dataProvider)
        {
            lock (this)
            {
                _dataProvider = dataProvider;
                ////_dataProvider.OperationalStatusChangedEvent += new GeneralHelper.GenericDelegate<IOperational>(_subItem_OperationalStatusChangedEvent);
                _operationalState = OperationalStateEnum.Initialized;
            }
            return true;
        }

        public void UnInitialize()
        {
            lock (this)
            {
                _operationalState = OperationalStateEnum.UnInitialized;

                if (_dataProvider != null)
                {
                    //_dataProvider.OperationalStatusChangedEvent -= new GeneralHelper.GenericDelegate<IOperational>(_subItem_OperationalStatusChangedEvent);
                    _dataProvider = null;
                }

                if (_orderExecutionProvider != null)
                {
                    //_orderExecutionProvider.OperationalStatusChangedEvent -= new GeneralHelper.GenericDelegate<IOperational>(_subItem_OperationalStatusChangedEvent);
                    _orderExecutionProvider = null;
                }
            }
            //GeneralHelper.SafeEventRaise(OperationalStatusChangedEvent, this);
        }

        public void Add(Indicator indicator)
        {
            indicator.Calculate();
            if (_indicators.Add(indicator) && IndicatorAddedEvent != null)
            {
                IndicatorAddedEvent(this, indicator);
            }
        }

        public void Remove(Indicator indicator)
        {
            if (_indicators.Remove(indicator) && IndicatorRemovedEvent != null)
            {
                IndicatorRemovedEvent(this, indicator);
            }
        }


    }
}
