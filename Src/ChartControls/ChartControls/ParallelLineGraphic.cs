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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ChartControls.Drawing;

#if USINGCANVAS
using Windows.Foundation;
#else
using System.Windows.Media;
using System.Windows.Controls;

#endif

namespace ChartControls
{
    /// <summary>
    /// 并行线图形。
    /// </summary>
    public class ParallelLineGraphics : ICustomGraphics, IInteractive
    {
        public ParallelLineGraphics()
        {
            Points = new Point[pointCount];
            ValuePoints = new ValuePoint[pointCount];
        }
        private GeometryGroup regionGeo;
        private List<RectangleGeometry> selectionGeoList;
        private int iPoint = 0;

        private const int pointCount = 3;
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
            Points = new Point[3];
            Points[iPoint] = pt;
            iPoint = 1;
        }

        public bool NextDraw(Point pt, IDrawingContext dc, bool isFinal)
        {
            if (iPoint == pointCount)
                return true;

            Points[iPoint] = pt;

            var p0 = PointSnapper.SnapPoint(Points[0]);
            var p1 = PointSnapper.SnapPoint(Points[1]);
            dc.DrawLine(Pen, p0, p1);
            
            if (iPoint == 2)
            {
                var p2 = PointSnapper.SnapPoint(Points[2]);
                var diffX = p1.X - p2.X;
                var diffY = p1.Y - p2.Y;

                Point pt00 = new Point(p0.X - diffX, p0.Y - diffY);
                dc.DrawLine(Pen, pt00, p2);
            }
            if (isFinal)
            {
                iPoint++;
            }
            return iPoint == PointCount;
        }

        public void Draw(IDrawingContext dc)
        {
            if (iPoint != pointCount)
                return;

            var pts = new Point[pointCount];
            for (int i = 0; i < pointCount; i++)
            {
                pts[i] = PointSnapper.SnapPoint(Points[i]);
            }

            dc.DrawLine(Pen, pts[0], pts[1]);

            var diffX = pts[1].X - pts[2].X;
            var diffY = pts[1].Y - pts[2].Y;

            Point pt00 = new Point(pts[0].X - diffX, pts[0].Y - diffY);
            dc.DrawLine(Pen, pt00, pts[2]);

            if (IsSelected)
            {
                selectionGeoList = new List<RectangleGeometry>();

                for (int i = 0; i < PointCount; i++)
                {
                    var selectionGeo = new RectangleGeometry(new Rect(PointSnapper.RoundValue(pts[i].X - 3), PointSnapper.RoundValue(pts[i].Y - 3),
                        6, 6));

                    dc.DrawGeometry(Pen.Brush, null, selectionGeo);

                    selectionGeoList.Add(selectionGeo);
                }
            }

            CreateRegion(pts);
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
                    isIn = selectionGeoList.Any(geo => geo.FillContains(point));
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
                    for (int i = 0; i < selectionGeoList.Count; i++)
                    {
                        if (selectionGeoList[i].FillContains(point))
                        {
                            index = i;
                            break;
                        }
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

        private void CreateRegion(Point[] ptSnapped)
        {
            regionGeo = new GeometryGroup();

            var line1Geo = new StreamGeometry();
            using (var gdc = line1Geo.Open())
            {
                var ptTemp = new Point(ptSnapped[0].X, ptSnapped[0].Y - 3);

                gdc.BeginFigure(ptTemp, true, true);
                ptTemp = new Point(ptSnapped[1].X, ptSnapped[1].Y - 3);
                gdc.LineTo(ptTemp, false, false);
                ptTemp = new Point(ptSnapped[1].X, ptSnapped[1].Y + 3);
                gdc.LineTo(ptTemp, false, false);
                ptTemp = new Point(ptSnapped[0].X, ptSnapped[0].Y + 3);
                gdc.LineTo(ptTemp, false, false);
            }
            regionGeo.Children.Add(line1Geo);

            var line2Geo = new StreamGeometry();
            using (var gdc = line2Geo.Open())
            {
                var diffX = ptSnapped[1].X - ptSnapped[2].X;
                var diffY = ptSnapped[1].Y - ptSnapped[2].Y;

                var pt20 = new Point(ptSnapped[0].X - diffX, ptSnapped[0].Y - diffY);

                var ptTemp = new Point(pt20.X, pt20.Y - 3);
                gdc.BeginFigure(ptTemp, true, true);
                ptTemp = new Point(ptSnapped[2].X, ptSnapped[2].Y - 3);
                gdc.LineTo(ptTemp, false, false);
                ptTemp = new Point(ptSnapped[2].X, ptSnapped[2].Y + 3);
                gdc.LineTo(ptTemp, false, false);
                ptTemp = new Point(ptTemp.X, ptTemp.Y + 3);
                gdc.LineTo(ptTemp, false, false);
            }

            regionGeo.Children.Add(line2Geo);
        }
    }
}
