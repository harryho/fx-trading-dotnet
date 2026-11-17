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
    class ImagesChartSeries : SimpleChartSeries
    {
        PointF _imagesDisplacement = new PointF();
        /// <summary>
        /// What is the default displacement for images - allowing to render them besides/above etc. the graphics.
        /// </summary>
        public PointF ImagesDisplacement
        {
            get
            {
                lock (this)
                {
                    return _imagesDisplacement;
                }
            }
            set
            {
                lock (this)
                {
                    _imagesDisplacement = value;
                }
            }
        }

        Dictionary<float, Image> _images = new Dictionary<float, Image>();
        public Dictionary<float, Image> Images
        {
            get
            {
                lock (this)
                {
                    return _images;
                }
            }
        }

        public override void Draw(GraphicsWrapper g, int unitsUnification, RectangleF clippingRectangle, float itemWidth, float itemMargin)
        {
            PointF drawingPoint = new PointF();

            lock (this)
            {
                Image image;
                foreach (float[] valueSet in ValueSets)
                {
                    foreach (float value in valueSet)
                    {// Images mode does not apply unit unification
                        if (double.IsNaN(value) == false && drawingPoint.X >= clippingRectangle.X && drawingPoint.X <= clippingRectangle.X + clippingRectangle.Width
                            && _images.ContainsKey((int)value))
                        {
                            image = _images[(int)value];
                            g.DrawImage(image, drawingPoint.X + _imagesDisplacement.X - image.Width / 2, drawingPoint.Y + _imagesDisplacement.Y + (float)value);
                        }

                        drawingPoint.X += itemMargin + itemWidth;
                    }
                }
            }
        }

    }
}
