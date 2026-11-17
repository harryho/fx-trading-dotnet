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
    [CustomNameAttribute("Moving Average Convergence/Divergence")]
    public class MACD : Indicator
    {
        private int _fastPeriod = 12;
        public int FastPeriod
        {
            get { return _fastPeriod; }
            set { _fastPeriod = value; }
        }

        private int _slowPeriod = 26;
        public int SlowPeriod
        {
            get { return _slowPeriod; }
            set { _slowPeriod = value; }
        }

        private int _signal = 9;
        public int Signal
        {
            get { return _signal; }
            set { _signal = value; }
        }

        public enum MACDStates
        {
            Default = 0,
            Cross0Up = 1,
            Cross0Down = 2
        }

        /// <summary>
        /// 
        /// </summary>
        public MACD()
            : base(true, false, new string[] { "MACD", "MACDHistory", "MACDSignal" })
        {
        }

        //public override Type StateEnumType
        //{
        //    get { return typeof(MACDStates); }
        //}

        protected override void OnCalculate(int startingIndex, int indecesCount)
        {
            double[] averages = DataProvider.GetDataValues(BarData.DataValueSourceEnum.Average, startingIndex, indecesCount);

            double[] macd = new double[averages.Length];
            double[] macdSignal = new double[averages.Length];
            double[] macdHistory = new double[averages.Length];

            int beginIndex, number;
            TicTacTec.TA.Library.Core.RetCode code =
                TicTacTec.TA.Library.Core.Macd(0, indecesCount - 1, averages,
                    FastPeriod, SlowPeriod, Signal,
                    out beginIndex, out number, macd, macdSignal, macdHistory);

            System.Diagnostics.Debug.Assert(code == TicTacTec.TA.Library.Core.RetCode.Success);

            Results.SetResultSetValues("MACD", beginIndex, number, macd);
            Results.SetResultSetValues("MACDHistory", beginIndex, number, macdHistory);
            Results.SetResultSetValues("MACDSignal", beginIndex, number, macdSignal);
        }

        public override float OnResultAnalysisCrossingFound(int line1index, double line1value, int line2index, double line2value, bool direction, double currentSignalPositionValue)
        {
            if (line1index == 0 && line2index == 1)
            {
                if (direction)
                {
                    return (int)MACDStates.Cross0Up;
                }
                else
                {
                    return (int)MACDStates.Cross0Down;
                }
            }

            return (int)MACDStates.Default;
        }

        protected override double[][] ProvideSignalAnalysisLines()
        {
            return new double[][] { Results.ResultSets[2].Values, Results.CreateFixedLineResultLength(0) };
        }

    }
}
