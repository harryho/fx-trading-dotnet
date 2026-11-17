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
//    /// <summary>
//    /// INTERPETATION : 
//    /// The most popular method of interpreting a moving average is to compare the relationship between a 
//    /// moving average of the security's closing price and the security's closing price itself. A sell signal 
//    /// is generated when the security's price falls below its moving average and a buy signal is generated 
//    /// when the security's price rises above its moving average.
//    /// </summary>
//    public class TEMA : Indicator
//    {
//        const int TimePeriod = 20;

//        /// <summary>
//        /// 
//        /// </summary>
//        public TEMA(DataProvider dataProvider)
//            : base(dataProvider)
//        {
//        }

//        public enum EMAStates
//        {
//            UpDownCross = -1,
//            Default = 0,
//            DownUpCross = 1
//        }

//        public string FullName
//        {
//            get { return "Triple Exponential Moving Average"; }
//        }

//        public Type StateEnumType
//        {
//            get { return typeof(EMAStates); }
//        }

//        protected override void Calculate(int startingIndex, int indecesCount)
//        {
//            base.Clear();

//            double[] closeValues = _dataProvider.GetDataValues(startingIndex, indecesCount, DataProvider.DataValueSourceEnum.Close);

//            double[] temaResult = new double[indecesCount];
//            int beginIndex = 0;
//            int numberElements = 0;
//            TicTacTec.TA.Library.Core.Tema(/*startingIndex*/ 0, /*startingIndex +*/ indecesCount - 1, closeValues, TimePeriod,
//                out beginIndex, out numberElements, temaResult);

//            AddResultSet(beginIndex, numberElements, temaResult);

//            // Calculate states - WE NEED TO USE THE RESULT SET HERE, as it is different to the temaResult!!
//            float[] resultSignals = CalculateDefaultCrossingSignal(beginIndex, ResultSets[0], closeValues);
//            SetResultSignals(resultSignals);
//        }

//    }
//}
