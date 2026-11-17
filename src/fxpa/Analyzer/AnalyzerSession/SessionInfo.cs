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
    [Serializable]
    public struct SessionInfo
    {
        Guid _guid;
        public Guid Guid
        {
            get { return _guid; }
        }

        string _id;
        public string Id
        {
            get { return _id; }
        }

        /// <summary>
        /// TODO : this needs to be updated.
        /// </summary>
        public string BaseCurrency
        {
            get { return _symbol.Substring(0, 3); }
        }

        string _symbol;
        public string Symbol
        {
          get { return _symbol; }
        }

        TimeSpan _timeInterval;
        /// <summary>
        /// How long is one period of this session.
        /// If the value is zero, Tick based is accepted (each time the value changes).
        /// </summary>
        public TimeSpan TimeInterval
        {
            get { return _timeInterval; }
        }

        double _lotSize;
        public double LotSize
        {
            get { return _lotSize; }
        }

        double _pointDigits;
        public double PointDigits
        {
            get { return _pointDigits; }
        }


        public override bool Equals(object obj)
        {
            return ((SessionInfo)obj).Symbol == this.Symbol;
        }
        /// <summary>
        /// 
        /// </summary>
        public SessionInfo(string id, string symbol, Guid guid, TimeSpan timeInterval, double lotSize, double pointDigits)
        {
            _pointDigits = pointDigits;
            _lotSize = lotSize;
            _id = id;
            _symbol = symbol;
            _timeInterval = timeInterval;
            _guid = guid;
        }
    }
}
