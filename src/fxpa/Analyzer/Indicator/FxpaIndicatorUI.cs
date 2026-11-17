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
    public class FxpaIndicatorUI : BasicIndicatorUI
    {
        public new FxpaIndicator Indicator
        {
            get { return (FxpaIndicator)base.Indicator; }
        }

        /// <summary>
        /// 
        /// </summary>
        public FxpaIndicatorUI(FxpaIndicator indicator)
            : base(indicator)
        {
            Console.WriteLine(indicator.Name + "," + (indicator.DataProvider != null).ToString());
        }

        public override string[] GetPropertiesNames()
        {
            List<string> results = new List<string>();
            for (int i = 0; i < Indicator.IntputParameters.Count; i++)
            {
                results.Add(Indicator.IntputParameters[i].Name);
            }

            results.AddRange(base.GetPropertiesNames());

            return results.ToArray();
        }

        public override object GetPropertyValue(string name)
        {
            for (int i = 0; i < Indicator.IntputParameters.Count; i++)
            {
                if (Indicator.IntputParameters[i].Name == name)
                {
                    return Indicator.InputParametersValues[i];
                }
            }

            return base.GetPropertyValue(name);
        }

        public override Type GetPropertyType(string name)
        {
            for (int i = 0; i < Indicator.IntputParameters.Count; i++)
            {
                if (Indicator.IntputParameters[i].Name == name)
                {
                    return Indicator.IntputParameters[i].ParameterType;
                }
            }

            return base.GetPropertyType(name);
        }

        public override bool SetPropertyValue(string name, object value)
        {
            for (int i = 0; i < Indicator.IntputParameters.Count; i++)
            {
                if (Indicator.IntputParameters[i].Name == name)
                {
                    Indicator.InputParametersValues[i] = value;
                    return true;
                }
            }

            return base.SetPropertyValue(name, value);
        }
    }
}
