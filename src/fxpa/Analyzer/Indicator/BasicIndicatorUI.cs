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
using System.Drawing;


namespace fxpa
{
    /// <summary>
    /// Holds information regarding the indicator UI.
    /// </summary>
    public class BasicIndicatorUI : IPropertyContainer
    {
        bool _enabled = true;
        public bool Enabled
        {
            get { return _enabled; }
            set { _enabled = value; }
        }

        Dictionary<string, Pen> _outputResultSetsPens = new Dictionary<string, Pen>();
        public Dictionary<string, Pen> OutputResultSetsPens
        {
            get { return _outputResultSetsPens; }
            set { _outputResultSetsPens = value; }
        }

        BasicIndicator _indicator;
        public BasicIndicator Indicator
        {
            get { return _indicator; }
        }

        public delegate void IndicatorUIUpdatedDelegate(BasicIndicatorUI ui);
        public event IndicatorUIUpdatedDelegate IndicatorUIUpdatedEvent;

        /// <summary>
        /// 
        /// </summary>
        public BasicIndicatorUI(BasicIndicator indicator)
        {
            Console.WriteLine(indicator.Name + "," + (indicator.DataProvider != null).ToString());

            _indicator = indicator;
            foreach (string name in indicator.ResultSetNames)
            {
                _outputResultSetsPens.Add(name, null);
            }
        }

        #region IDynamicPropertyContainer Members

        public virtual string[] GetPropertiesNames()
        {
            return GeneralHelper.EnumerableToArray<string>(_outputResultSetsPens.Keys);
        }

        public virtual object GetPropertyValue(string name)
        {
            return _outputResultSetsPens[name];
        }

        public virtual Type GetPropertyType(string name)
        {
            return _outputResultSetsPens[name].GetType();
        }

        public virtual bool SetPropertyValue(string name, object value)
        {
            _outputResultSetsPens[name] = (Pen)value;
            return true;
        }

        public void PropertyChanged()
        {
            if (IndicatorUIUpdatedEvent != null)
            {
                IndicatorUIUpdatedEvent(this);
            }
        }



        #endregion

    }
}
