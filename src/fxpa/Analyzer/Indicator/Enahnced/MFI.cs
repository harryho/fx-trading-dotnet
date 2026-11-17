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

//using System;
//using System.Collections.Generic;
//using System.Text;
//using CommonSupport;
//using CommonFinancial;


namespace fxpa
{
    [CustomNameAttribute("Money Flow Index")]
    public class MFI : Indicator
    {
        int _timePeriod = 14;
        public int TimePeriod
        {
            get { return _timePeriod; }
            set { _timePeriod = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public MFI()
            : base(true, false, new string[] { "MFI" })
        {

        }

        public enum MFIStates
        {
            Default = 0,
            Under20Up = 1,
            Under20Down = 2,
            Over80Up = 3,
            Over80Down = 4
        }

        //public override Type StateEnumType
        //{
        //    get { return typeof(MFIStates); }
        //}


        protected override void OnCalculate(int startingIndex, int indecesCount)
        {
            double[] high = DataProvider.GetDataValues(BarData.DataValueSourceEnum.High, startingIndex, indecesCount);
            double[] low = DataProvider.GetDataValues(BarData.DataValueSourceEnum.Low, startingIndex, indecesCount);
            double[] close = DataProvider.GetDataValues(BarData.DataValueSourceEnum.Close, startingIndex, indecesCount);
            double[] volume = DataProvider.GetDataValues(BarData.DataValueSourceEnum.Volume, startingIndex, indecesCount);

            double[] result = new double[high.Length];

            int beginIndex, number;

            TicTacTec.TA.Library.Core.RetCode code =
            TicTacTec.TA.Library.Core.Mfi(0, indecesCount - 1, high, low, close, volume, TimePeriod, out beginIndex, out number, result);

            System.Diagnostics.Debug.Assert(code == TicTacTec.TA.Library.Core.RetCode.Success);

            Results.SetResultSetValues("MFI", beginIndex, number, result);
        }

        public override float OnResultAnalysisCrossingFound(int line1index, double line1value, int line2index, double line2value, bool direction, double currentSignalPositionValue)
        {
            if (line1index == 0 && line2index == 1)
            {
                if (direction)
                {
                    return (int)MFIStates.Over80Up;
                }
                else
                {
                    return (int)MFIStates.Over80Down;
                }
            }
            else if (line1index == 0 && line2index == 2)
            {
                if (direction)
                {
                    return (int)MFIStates.Under20Up;
                }
                else
                {
                    return (int)MFIStates.Under20Down;
                }
            }

            return (int)MFIStates.Default;

        }

        protected override double[][] ProvideSignalAnalysisLines()
        {
            return new double[][] { Results.ResultSets[0].Values, Results.CreateFixedLineResultLength(20), Results.CreateFixedLineResultLength(80) };
        }

    }
}
