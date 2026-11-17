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
    /// A simple fast line class to help basic drawing calculations.
    /// Basic equation for a line: Ax+By=C
    /// </summary>
    public class SimpleLine
    {
        PointF _point1;
        PointF _point2;

        /// <summary>
        /// A = y2-y1
        /// </summary>
        public float A
        {
            get { return _point2.Y - _point1.Y; }
        }

        /// <summary>
        /// B = x1-x2
        /// </summary>
        public float B
        {
            get { return _point1.X - _point2.X; }
        }

        /// <summary>
        /// C = A*x1+B*y1
        /// </summary>
        public float C
        {
            get { return A * _point1.X + B * _point1.Y; }
        }

        /// <summary>
        /// 
        /// </summary>
        public SimpleLine(PointF point1, PointF point2)
        {
            _point1 = point1;
            _point2 = point2;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="otherLine"></param>
        /// <returns>Returns null to indicate lines are invalid or parralel.</returns>
        public PointF? Intersection(SimpleLine otherLine)
        {
            float det = this.A * otherLine.B - otherLine.A * this.B;
            if(det == 0)
            {
                return null;
            }

            return new PointF( (otherLine.B * this.C - this.B * otherLine.C) / det, 
                (this.A * otherLine.C - otherLine.A * this.C) / det);
        }

        ///// <summary>
        ///// Helper.
        ///// </summary>
        //public static bool GetLinePointDistance(PointF linePoint1, PointF linePoint2, PointF location,
        //    out float lineSegmentLocation, out float distance, out PointF closestIntersectionPoint)
        //{

        //}

        /// <summary>
        /// Formula used as shown here: http://local.wasp.uwa.edu.au/~pbourke/geometry/pointline/
        /// u = ( (x3 - x1)(x2 - x1) + (y3 - y1)(y2 - y1) )/ sqr||p2 - p1||
        /// x = x1 + u(x2 - x1);
        /// y = y1 + u(y2 - y1);
        /// </summary>
        public float? DistanceToPoint(PointF point, out float lineSegmentLocation, out PointF intersectionPoint)
        {
            if (A == 0 && B == 0)
            {// No line.
                lineSegmentLocation = 0;
                intersectionPoint = new PointF();
                return null;
            }

            lineSegmentLocation = (point.X - _point1.X) * (_point2.X - _point1.X) + (point.Y - _point1.Y) * (_point2.Y - _point1.Y);
            float xSquare = (float)Math.Pow(_point2.X - _point1.X, 2);
            float ySquare = (float)Math.Pow(_point2.Y - _point1.Y, 2);

            lineSegmentLocation = lineSegmentLocation / (xSquare + ySquare);

            float intersectionPointX = _point1.X + lineSegmentLocation * (_point2.X - _point1.X);
            float intersectionPointY = _point1.Y + lineSegmentLocation * (_point2.Y - _point1.Y);
            intersectionPoint = new PointF(intersectionPointX, intersectionPointY);

            return (float)Math.Sqrt(Math.Pow(point.X - intersectionPoint.X, 2) + Math.Pow(point.Y - intersectionPoint.Y, 2));
        }
    }
}
