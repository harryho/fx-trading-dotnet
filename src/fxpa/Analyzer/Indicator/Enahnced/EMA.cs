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
    /// 
    /// </summary>
    [CustomNameAttribute("Exponential Moving Average")]
    public class EMA : Indicator
    {
        int _timePeriod = 17;
        public int TimePeriod
        {
            get { return _timePeriod; }
            set { _timePeriod = value; }
        }

        double[] _closeResultValues;

        /// <summary>
        /// 
        /// </summary>
        public EMA()
            : base(true, true, new string[] { "EMA" })
        {
        }

        public enum EMAStates
        {
            Default = 0,
            DownUpCross = 1,
            UpDownCross = 2,
        }

        //public override Type StateEnumType
        //{
        //    get { return typeof(EMAStates); }
        //}

        protected override void OnCalculate(int startingIndex, int indecesCount)
        {
            _closeResultValues = DataProvider.GetDataValues(BarData.DataValueSourceEnum.Close, startingIndex, indecesCount);

            double[] emaResults = new double[indecesCount];
            int beginIndex = 0;
            int numberElements = 0;

            TicTacTec.TA.Library.Core.RetCode code =
            TicTacTec.TA.Library.Core.Ema(0, indecesCount - 1, _closeResultValues, TimePeriod,
                out beginIndex, out numberElements, emaResults);

            System.Diagnostics.Debug.Assert(code == TicTacTec.TA.Library.Core.RetCode.Success);

            Results.SetResultSetValues("EMA", beginIndex, numberElements, emaResults);
        }

        public override float OnResultAnalysisCrossingFound(int line1index, double line1value, int line2index, double line2value,
            bool direction, double currentSignalPositionValue)
        {
            if (line1index == 0 && line2index == 1)
            {
                if (direction)
                {
                    return (int)EMAStates.DownUpCross;
                }
                else
                {
                    return (int)EMAStates.UpDownCross;
                }
            }

            return (int)EMAStates.Default;
        }

        protected override double[][] ProvideSignalAnalysisLines()
        {
            return new double[][] { Results.ResultSets[0].Values, _closeResultValues };
        }

    }
}
