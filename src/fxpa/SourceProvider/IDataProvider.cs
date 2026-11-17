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
    public enum UpdateType
    {// In order of importance.
        Initial, 
        NewBar,
        BarsUpdated,
        Quote
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="updateType"></param>
    /// <param name="count"></param>
    /// <param name="stepsRemaining">Multi step mode occurs when many steps are done one after the other in fast succession. Usefull for UI not to update itself.</param>
    public delegate void ValuesUpdatedDelegate(IDataProvider provider, UpdateType updateType, int count, int stepsRemaining);

    public interface IDataProvider 
    {
        string Name { get; }

        double Ask { get; }
        double Bid { get; }
        double Spread{ get; }

        int DataUnitCount { get; }

        /// <summary>
        /// This is where time control how be exercised on a provider. 
        /// If provider does not support time control, member is null.
        /// </summary>
        ITimeControl TimeControl { get; }

        DateTime FirstBarDateTime{ get; }
        DateTime LastBarDateTime{ get; }
        IList<BarData> DataUnits{ get; }

        DateTime CurrentSourceDateTime{ get; }
        BarData? CurrentDataUnit { get; }

        TimeSpan TimeInterval { get; }

        /// <summary>
        /// Update type, updated items count.
        /// </summary>
        event ValuesUpdatedDelegate ValuesUpdateEvent;
        //Harry--Modified
        //void RequestValuesUpdate();

        double[] GetDataValues(BarData.DataValueSourceEnum valueEnum);
        double[] GetDataValues(BarData.DataValueSourceEnum valueEnum, int startingIndex, int indexCount);

        int GetBarIndexAtTime(DateTime time);

        void UnInitialize();
    }
}
