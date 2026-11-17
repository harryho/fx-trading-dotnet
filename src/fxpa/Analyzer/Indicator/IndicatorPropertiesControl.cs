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
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

using System.Reflection;


namespace fxpa
{
    public partial class IndicatorPropertiesControl : CustomPropertiesControl
    {
        public new BasicIndicator SelectedObject
        {
            get
            {
                return Indicator;
            }

            set
            {
                Indicator = value;
            }
        }

        public BasicIndicator Indicator
        {
            get
            {
                if (base.SelectedObject == null)
                {
                    return null;
                }
                else
                {
                    return ((BasicIndicatorUI)base.SelectedObject).Indicator;
                }
            }

            set 
            {
                if (value == null)
                {
                    //TracerHelper.Trace("Null");
                    base.SelectedObject = null;
                }
                else
                {
                    //TracerHelper.Trace(value.Name + ", " + (value.DataProvider != null).ToString());
                    base.SelectedObject = value.UI;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public IndicatorPropertiesControl()
        {
            InitializeComponent();
        }

        private void IndicatorControl_Load(object sender, EventArgs e)
        {
            UpdateUI();

            base.FilteringPropertiesNames.AddRange( new string[] { "Description"} );
            base._startingYLocation = textBoxDescription.Bottom + InterControlMargin;
        }
        
        /// <summary>
        /// Main UI logic function.
        /// </summary>
        protected override void OnUpdateUI(int startingYValue)
        {
            textBoxDescription.Visible = Indicator != null;
            labelDesription.Visible = Indicator != null;

            if (Indicator == null)
            {
                return;
            }
            
            textBoxDescription.Text = Indicator.Description;

            int lastYValue = startingYValue;

        }

    }
}
