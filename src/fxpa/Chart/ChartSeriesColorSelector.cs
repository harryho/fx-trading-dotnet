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
    public class ChartSeriesColorSelector
    {
        int _seriesIndex = 0;
        public int SeriesIndex
        {
            get { return _seriesIndex; }
            set { _seriesIndex = value; }
        }

        public ChartSeriesColorSelector()
        {
        }

        public void SetupSeries(ChartSeries inputSeries)
        {
            if (inputSeries is TradeChartSeries)
            {
                TradeChartSeries series = (TradeChartSeries)inputSeries;
                switch (_seriesIndex)
                {
                    case 0:
                        {
                            series.DefaultPen = Pens.DarkSeaGreen;
                            series.RisingBarPen = Pens.DarkSeaGreen;
                            series.RisingBarFill = null;
                            series.FallingBarPen = Pens.IndianRed;
                            series.FallingBarFill = Brushes.IndianRed;
                        }
                        break;
                    case 1:
                        {
                            series.DefaultPen = Pens.LightBlue;
                            series.RisingBarPen = Pens.LightBlue;
                            series.RisingBarFill = null;
                            series.FallingBarPen = Pens.LightBlue;
                            series.FallingBarFill = Brushes.LightBlue;
                        }
                        break;
                    case 2:
                        {
                            series.DefaultPen = Pens.LightSlateGray;
                            series.RisingBarPen = Pens.LightSlateGray;
                            series.RisingBarFill = null;
                            series.FallingBarPen = Pens.LightSlateGray;
                            series.FallingBarFill = Brushes.LightSlateGray;
                        }
                        break;
                    case 3:
                        {
                            series.DefaultPen = Pens.Lavender;
                            series.RisingBarPen = Pens.Lavender;
                            series.RisingBarFill = null;
                            series.FallingBarPen = Pens.Lavender;
                            series.FallingBarFill = Brushes.Lavender;
                        }
                        break;
                    case 4:
                        {
                            series.DefaultPen = Pens.OrangeRed;
                            series.RisingBarPen = Pens.OrangeRed;
                            series.RisingBarFill = null;
                            series.FallingBarPen = Pens.OrangeRed;
                            series.FallingBarFill = Brushes.OrangeRed;
                        }
                        break;
                    case 5:
                        {
                            series.DefaultPen = Pens.Gold;
                            series.RisingBarPen = Pens.Gold;
                            series.RisingBarFill = null;
                            series.FallingBarPen = Pens.Gold;
                            series.FallingBarFill = Brushes.Gold;
                        }
                        break;
                    case 6:
                        {
                            series.DefaultPen = Pens.GreenYellow;
                            series.RisingBarPen = Pens.GreenYellow;
                            series.RisingBarFill = null;
                            series.FallingBarPen = Pens.GreenYellow;
                            series.FallingBarFill = Brushes.GreenYellow;
                        }
                        break;
                    case 7:
                        {
                            series.DefaultPen = Pens.BlueViolet;
                            series.RisingBarPen = Pens.BlueViolet;
                            series.RisingBarFill = null;
                            series.FallingBarPen = Pens.BlueViolet;
                            series.FallingBarFill = Brushes.BlueViolet;
                        }
                        break;

                }
            }

            _seriesIndex++;
        }
    }
}
