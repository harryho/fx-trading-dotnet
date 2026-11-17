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
//using CommonSupport;
//using CommonFinancial;

namespace fxpa
{
    /// <summary>
    /// This indicator is not tradeable since it changes its position and requres future looking.
    /// Indicator written manually no external lib used.
    /// </summary>
    [CustomNameAttribute("Shadow Trace Custom")]
    public class ShadowTraceCustom : Indicator
    {
        int _lookBackLength = 20;

        public int LookBackLength
        {
            get { return _lookBackLength; }
            set { _lookBackLength = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public ShadowTraceCustom()
            : base(false, false, new string[] { "Shadow", "ShadowTop", "ShadowBottom", "ShadowCombined" })
        {
        }



        public enum ShadowTraceStates
        {
            Default = 0,
            CrossUp = 1,
            CrossDown = 2
        }

        //public override Type StateEnumType
        //{
        //    get { return typeof(ShadowTraceStates); }
        //}

        protected override void OnCalculate(int startingIndex, int indecesCount)
        {
            double[] results = new double[indecesCount];
            double[] resultsTop = new double[indecesCount];
            double[] resultsBottom = new double[indecesCount];
            double[] resultsCombined = new double[indecesCount];

            for (int i = startingIndex; i < startingIndex + indecesCount; i++)
            {
                double topSum = 0;
                double bottomSum = 0;

                for (int j = i; j >= 0 && j > i - LookBackLength; j--)
                {
                    topSum += DataProvider.DataUnits[j].BarTopShadowLength;
                    bottomSum += DataProvider.DataUnits[j].BarBottomShadowLength;
                }

                results[i - startingIndex] = topSum - bottomSum;
                resultsTop[i - startingIndex] = topSum;
                resultsBottom[i - startingIndex] = bottomSum;
                resultsCombined[i - startingIndex] = topSum + bottomSum;

            }


            Results.SetResultSetValues("Shadow", startingIndex, indecesCount, results);
            Results.SetResultSetValues("ShadowTop", startingIndex, indecesCount, resultsTop);
            Results.SetResultSetValues("ShadowBottom", startingIndex, indecesCount, resultsBottom);
            Results.SetResultSetValues("ShadowCombined", startingIndex, indecesCount, resultsCombined);
        }


        public override float OnResultAnalysisCrossingFound(int line1index, double line1value, int line2index, double line2value, bool direction, double currentPositionSignalValue)
        {
            //if (line1index == 0)
            //{
            //    return (float)ShadowTraceStates.CrossUp;
            //}
            //else
            //{
            //    return (float)ShadowTraceStates.CrossDown;
            //}
            return 0;
        }

        protected override double[][] ProvideSignalAnalysisLines()
        {
            return new double[][] { Results.CreateFixedLineResultLength(0), Results.ResultSets[0].Values };
        }

    }
}
