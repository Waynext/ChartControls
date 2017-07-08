#region License
// Copyright (c) 2015 Wayne Gu
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion
using ChartControls.Drawing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ChartControls
{
    /// <summary>
    /// 直线自定义图形。
    /// </summary>
    public class LineGraphics : ICustomGraphics, IInteractive
    {
        public LineGraphics()
        {
            Points = new Point[pointCount];
            ValuePoints = new ValuePoint[pointCount];
        }

        private StreamGeometry regionGeo;
        private RectangleGeometry selectionGeo1;
        private RectangleGeometry selectionGeo2;

        private const int pointCount = 2;
        public int PointCount
        {
            get
            {
                return pointCount;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public Point[] Points
        {
            get;
            set;
        }

        public ValuePoint[] ValuePoints
        {
            get;
            set;
        }

        public IPen Pen
        {
            get;
            set;
        }

        public void StartDraw(Point pt)
        {
            Points = new Point[2];
            Points[0] = pt;
        }

        public bool NextDraw(Point pt, IDrawingContext dc, bool isFinal)
        {
            Points[1] = pt;

            var p0 = PointSnapper.SnapPoint(Points[0]);
            var p1 = PointSnapper.SnapPoint(Points[1]);
            dc.DrawLine(Pen, p0, p1);

            if (isFinal)
            {
                CreateRegion();
            }
            return isFinal;
        }

        public void Draw(IDrawingContext dc)
        {
            var p0 = PointSnapper.SnapPoint(Points[0]);
            var p1 = PointSnapper.SnapPoint(Points[1]);

            dc.DrawLine(Pen, p0, p1);

            if (IsSelected)
            {
                selectionGeo1 = new RectangleGeometry(new Rect(PointSnapper.RoundValue(p0.X - 3), PointSnapper.RoundValue(p0.Y - 3),
                    6, 6));
                selectionGeo2 = new RectangleGeometry(new Rect(PointSnapper.RoundValue(p1.X - 3), PointSnapper.RoundValue(p1.Y - 3),
                    6, 6));


                dc.DrawGeometry(Pen.Brush, null, selectionGeo1);
                dc.DrawGeometry(Pen.Brush, null, selectionGeo2);
            }
            
            CreateRegion();
            
        }

        public bool HasTooltip
        {
            get
            {
                return false;
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public object ToolTip
        {
            get
            {
                return null;
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public ContextMenu ContextMenu
        {
            get
            {
                return null;
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public bool IsPointInRegion(Point point)
        {
            bool isIn = false;
            if (regionGeo != null)
            {
                isIn = regionGeo.FillContains(point);

                if (!isIn && IsSelected)
                {
                    isIn = selectionGeo1.FillContains(point) || selectionGeo2.FillContains(point);
                }
            }
            return isIn;
        }

        public bool CanSelect
        {
            get
            {
                return true;
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public bool IsSelected
        {
            get;
            set;
        }

        public bool CanChange
        {
            get;
            set;
        }

        public int GetNodeIndex(Point point)
        {
            int index = InteractiveConst.nodeIndexOutofTarget;
            if (IsPointInRegion(point))
            {
                index = InteractiveConst.nodeIndexInTarget;

                if (IsSelected)
                {
                    if (selectionGeo1.FillContains(point))
                    {
                        index = 0;
                    }
                    else if (selectionGeo2.FillContains(point))
                    {
                        index = 1;
                    }
                }
            }
            return index;
        }

        public void UpdateNodePosition(int nodeIndex, Point newPosition)
        {
            if (nodeIndex >= 0 && nodeIndex < PointCount)
            {
                Points[nodeIndex] = newPosition;

                regionGeo = null;
            }
        }

        public void TranformPosition(Transform transform)
        {
            for (int i = 0; i < PointCount; i++)
            {
                Points[i] = transform.Transform(Points[i]);
            }

            regionGeo = null;

        }

        
        private void CreateRegion()
        {
            regionGeo = new StreamGeometry();

            var p0 = PointSnapper.SnapPoint(Points[0]);
            var p1 = PointSnapper.SnapPoint(Points[1]);

            using (var gdc = regionGeo.Open())
            {
                var ptTemp = new Point(p0.X, p0.Y - 3);

                gdc.BeginFigure(ptTemp, true, true);
                ptTemp = new Point(p1.X, p1.Y - 3);
                gdc.LineTo(ptTemp, false, false);
                ptTemp = new Point(p1.X, p1.Y + 3);
                gdc.LineTo(ptTemp, false, false);
                ptTemp = new Point(p0.X, p0.Y + 3);
                gdc.LineTo(ptTemp, false, false);
            }
        }
    }
}
