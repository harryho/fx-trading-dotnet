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

//namespace fxpa
//{
//    public class RVI : TechnicalIndicator
//    {
//        public int TimePeriod = 14;

//        /// <summary>
//        /// 
//        /// </summary>
//        public RVI(DataProvider dataProvider)
//            : base(dataProvider)
//        {
//        }

//        //public enum RSIStates
//        //{
//        //    Default = 0,
//        //    Under30Up = 1,
//        //    Under30Down = 2,
//        //    Over70Up = 3,
//        //    Over70Down = 4
//        //}

//        public override string FullName
//        {
//            get { return "Relative Strength Index"; }
//        }

//        public override Type StateEnumType
//        {
//            get { return typeof(RSIStates); }
//        }

//        protected override void OnCalculate(int startingIndex, int indecesCount)
//        {// Calculate result.
//            double[] closes = _dataProvider.GetDataValues(startingIndex, indecesCount, DataProvider.DataValueSourceEnum.Close);
//            double[] result = new double[closes.Length];

//            int beginIndex, number;

//            TicTacTec.TA.Library.Core.RetCode code =
//            TicTacTec.TA.Library.Core.Rsi(0, indecesCount - 1, closes, TimePeriod,
//                    out beginIndex, out number, result);

//            System.Diagnostics.Debug.Assert(code == TicTacTec.TA.Library.Core.RetCode.Success);

//            AddResultSet(beginIndex, number, result);
//        }


//        protected override double[][] ProvideSignalAnalysisLines()
//        {
//            return new double[][] { ResultSets[0], CreateFixedLineResultLength(30), CreateFixedLineResultLength(70) };
//        }


//        /// <summary>
//        /// Line indeces always come from lower to biger.
//        /// </summary>
//        protected override float OnResultAnalysisCrossingFound(int line1index, double line1value, int line2index, double line2value, bool direction, float currentSignalPositionValue)
//        {// Calculating the result signals.

//            if ((line1index == 0 && line2index == 1))
//            {
//                if (direction)
//                {
//                    return (int)RSIStates.Under30Up;
//                }
//                else
//                {
//                    return (int)RSIStates.Under30Down;
//                }
//            }

//            if ((line1index == 0 && line2index == 2))
//            {
//                if (direction)
//                {
//                    return (int)RSIStates.Over70Up;
//                }
//                else
//                {
//                    return (int)RSIStates.Over70Down;
//                }
//            }

//            return (int)RSIStates.Default;
//        }


//        protected override float OnResultAnalysisExtremumFound(int lineIndex, double lineValue, bool direction, float currentPositionSignalValue)
//        {
//            throw new Exception("The method or operation is not implemented.");
//        }

//        protected override void RequiredAnalysis(out bool crossingAnalysis, out bool extremumAnalysis)
//        {
//            crossingAnalysis = true;
//            extremumAnalysis = false;
//        }

//    }
//}
