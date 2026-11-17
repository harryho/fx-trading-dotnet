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
using System.Windows.Forms;



namespace fxpa
{
    /// <summary>
    /// This class brings together the DataProvider and the ChartSeries, allowing the data to be rendered.
    /// </summary>
    public class IndicatorChartSeries : SimpleChartSeries
    {
        BasicIndicator _indicator;

        public BasicIndicator Indicator
        {
            get { return _indicator; }
        }

        /// <summary>
        /// 
        /// </summary>
        public IndicatorChartSeries(string name, BasicIndicator indicator)
        {
            Name = name;

            Initialize(indicator);
        }

        protected void Initialize(BasicIndicator indicator)
        {
            _indicator = indicator;
            _indicator.IndicatorCalculatedEvent += new Indicator.IndicatorCalculatedDelegate(_indicator_IndicatorCalculatedEvent);
            _indicator.UI.IndicatorUIUpdatedEvent += new BasicIndicatorUI.IndicatorUIUpdatedDelegate(UI_IndicatorUIUpdatedEvent);
            _indicator.ics = this;
        }

        void UI_IndicatorUIUpdatedEvent(BasicIndicatorUI ui)
        {
            UpdateValues();
        }

        protected override void OnAddedToChart()
        {
            UpdateValues();
        }

        protected override void OnRemovedFromChart()
        {
            if (_indicator != null)
            {
                _indicator.IndicatorCalculatedEvent -= new Indicator.IndicatorCalculatedDelegate(_indicator_IndicatorCalculatedEvent);
                _indicator = null;
            }
        }

      public   void _indicator_IndicatorCalculatedEvent(Indicator indicator)
        {
            UpdateValues();
        }

        void UpdateValues()
        {
            base.ClearValues();
            foreach (IndicatorResultSet resultSet in _indicator.Results.ResultSets)
            {
                Console.WriteLine(" n=> " + resultSet.Name + " v => " + resultSet.Values);
                AddValueSet(GeneralHelper.DoublesToFloats(resultSet.Values), _indicator.UI.OutputResultSetsPens[resultSet.Name]);
            }

            Console.WriteLine("  chart pane "+this.GetType());
            //if (charPane != null)
            //{
            //    Console.WriteLine("  chart pane ");
            //    ChartPaneUpdateDelegate d = new ChartPaneUpdateDelegate(charPane.series_SeriesUpdatedEvent);
            //    d.Invoke(this, true);
            //}
            //else
            //{
                base.RaiseValuesUpdated(true);
            //}
        }

        delegate void ChartPaneUpdateDelegate(ChartSeries series, bool isUpdate);

        public ChartPane charPane = null;


        public override void SaveToFile(string fileName)
        {
        }

        public override void Draw(GraphicsWrapper g, int unitsUnification, System.Drawing.RectangleF clippingRectangle, float itemWidth, float itemMargin)
        {
            base.Draw(g, unitsUnification, clippingRectangle, itemWidth, itemMargin);
        }

    }
}
