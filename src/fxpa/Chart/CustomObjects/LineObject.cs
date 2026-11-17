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

using System.Drawing.Drawing2D;

namespace fxpa
{
    public class LineObject : DynamicCustomObject
    {
        public enum ModeEnum
        {
            LineSegment,
            Line,
            HorizontalLine,
            VerticalLine
        }

        ModeEnum _mode;
        public ModeEnum Mode
        {
            get { return _mode; }
        }

        Pen _pen = Pens.White;
        public Pen Pen
        {
            get { return _pen; }
            set { _pen = value; }
        }

        public override bool IsBuilding
        {
            get 
            {
                if (_mode == ModeEnum.Line || _mode == ModeEnum.LineSegment)
                {
                    return _controlPoints.Count < 2;
                }
                else
                {
                    return _controlPoints.Count < 1;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public LineObject(string name, ModeEnum mode)
            : base(name)
        {
            _mode = mode;
        }

        public override RectangleF GetContainingRectangle(RectangleF drawingSpace)
        {
            if (_controlPoints.Count < 2)
            {
                return RectangleF.Empty;
            }

            if (_mode == ModeEnum.LineSegment)
            {// Base method relies on control points and is good in this scenario.
                return base.GetContainingRectangle(drawingSpace);
            }
            else if (_mode == ModeEnum.Line)
            {
                PointF point1 = _controlPoints[0];
                PointF point2 = _controlPoints[1];
                StretchSegmentToDrawingSpace(ref point1, ref point2, drawingSpace);
                return new RectangleF(point1.X, point1.Y, point2.X - point1.X, point2.Y - point1.Y);
            }

            return RectangleF.Empty;
        }

        public override bool TrySelect(Matrix transformationMatrix, PointF drawingSpaceSelectionPoint, float absoluteSelectionMargin, bool canLoseSelection)
        {
            if (IsBuilding)
            {
                return false;
            }

            PointF point1 = _controlPoints[0];
            PointF point2;

            if (_mode == ModeEnum.Line || _mode == ModeEnum.LineSegment)
            {
                point2 = _controlPoints[1];
            }
            else if (_mode == ModeEnum.HorizontalLine)
            {
                point2 = new PointF(point1.X + 1, point1.Y);
            }
            else if (_mode == ModeEnum.VerticalLine)
            {
                point2 = new PointF(point1.X, point1.Y + 1);
            }
            else
            {
                // SystemMonitor.Throw("Unhandled case.");
                return false;
            }

            if (TrySelectControlPoints(transformationMatrix, drawingSpaceSelectionPoint, absoluteSelectionMargin) > 0
                || TrySelectLine(transformationMatrix, _mode == ModeEnum.LineSegment, point1, point2, drawingSpaceSelectionPoint, absoluteSelectionMargin))
            {
                Selected = true;
                return true;
            }

            if (canLoseSelection)
            {
                Selected = false;
            }
            return false;
        }

        public override bool AddBuildingPoint(PointF point)
        {
            _controlPoints.Add(point);
            return !IsBuilding;
        }



        public override void Draw(GraphicsWrapper g, PointF? mousePosition, RectangleF clippingRectangle, RectangleF drawingSpace)
        {
            if (Visible == false)
            {
                return;
            }

            if (_controlPoints.Count < 1)
            {
                return;
            }

            if (_mode == ModeEnum.HorizontalLine || _mode == ModeEnum.VerticalLine)
            {
                PointF point1 = _controlPoints[0];
                PointF point2 = new PointF(drawingSpace.X + drawingSpace.Width, _controlPoints[0].Y);

                if (_mode == ModeEnum.VerticalLine)
                {
                    point2 = new PointF(_controlPoints[0].X, drawingSpace.Y + drawingSpace.Height);
                }

                StretchSegmentToDrawingSpace(ref point1, ref point2, drawingSpace);
                g.DrawLine(_pen, point1, point2);

                if (Selected)
                {
                    DrawControlPoints(g);
                }
            }
            else
            if (_controlPoints.Count == 1 && mousePosition.HasValue)
            {
                PointF point1 = _controlPoints[0];
                PointF point2 = mousePosition.Value;

                if (_mode == ModeEnum.Line)
                {
                    StretchSegmentToDrawingSpace(ref point1, ref point2, drawingSpace);
                }
                g.DrawLine(_pen, point1, point2);
            }
            else if (_controlPoints.Count == 2)
            {
                PointF point1 = _controlPoints[0];
                PointF point2 = _controlPoints[1];

                if (_mode == ModeEnum.Line)
                {
                    StretchSegmentToDrawingSpace(ref point1, ref point2, drawingSpace);
                }
                
                // Clipping opitmization.
                RectangleF rectangle = new RectangleF(
                    Math.Min(point1.X, point2.X), Math.Min(point1.Y, point2.Y),
                    Math.Abs(point2.X - point1.X), Math.Abs(point2.Y - point1.Y));
                if (rectangle.IntersectsWith(clippingRectangle) == false)
                {
                    return;
                }

                //if (_isSegment == false)
                //{// Draw the central line piece separately.
                    //g.DrawLine(_pen, _controlPoints[0], _controlPoints[1]);
                //}

                g.DrawLine(_pen, point1, point2);
                if (Selected)
                {
                    DrawControlPoints(g);
                }

            }

        }
    }
}
