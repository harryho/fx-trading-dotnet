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
    /// <summary>
    /// Average True Range (ATR) indicator measures a security's volatility.
    /// </summary>
    [CustomNameAttribute("Average True Range")]
    public class ATR : Indicator
    {
        // The range for the last day or so.
        int _timePeriod = 14;
        public int TimePeriod
        {
            get { return _timePeriod; }
            set { _timePeriod = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public ATR()
            : base(true, false, new string[] { "ATR" })
        {
        }

        public enum ATRStates
        {
            Default = 0,
            MaxExtreme = 1,
            MinExtreme = 2
        }

        //public override Type StateEnumType
        //{
        //    get { return typeof(ATRStates); }
        //}

        protected override void OnCalculate(int startingIndex, int indecesCount)
        {
            double[] high = DataProvider.GetDataValues(BarData.DataValueSourceEnum.High, startingIndex, indecesCount);
            double[] low = DataProvider.GetDataValues(BarData.DataValueSourceEnum.Low, startingIndex, indecesCount);
            double[] close = DataProvider.GetDataValues(BarData.DataValueSourceEnum.Close, startingIndex, indecesCount);

            double[] result = new double[high.Length];

            int beginIndex, number;

            TicTacTec.TA.Library.Core.RetCode code = TicTacTec.TA.Library.Core.Atr(0, indecesCount - 1, high, low, close, TimePeriod,
                    out beginIndex, out number, result);

            System.Diagnostics.Debug.Assert(code == TicTacTec.TA.Library.Core.RetCode.Success);

            Results.SetResultSetValues("ATR", beginIndex, number, result);
        }

        public override float OnResultAnalysisExtremumFound(int lineIndex, double lineValue, bool direction, double currentPositionSignalValue)
        {
            System.Diagnostics.Debug.Assert(lineIndex == 0);

            if (direction)
            {
                return (int)ATRStates.MaxExtreme;
            }
            else
            {
                return (int)ATRStates.MinExtreme;
            }
        }

    }
}
