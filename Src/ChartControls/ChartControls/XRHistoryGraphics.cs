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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

#if USINGCANVAS
using Windows.Foundation;
using Windows.UI.Xaml.Media;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
#else
using System.Windows.Controls;
using System.Windows.Media;
#endif


namespace ChartControls
{
    public class XRHistoryGraphics : IExtraDataGraphics, IInteractive
    {
        public XRHistoryGraphics()
        {
            ExitedForground = Brushes.Goldenrod;
            NonexitedForground = Brushes.Silver;
        }

        public Brush ExitedForground
        {
            get;
            set;
        }

        public Brush NonexitedForground
        {
            get;
            set;
        }

        private Brush Forground
        {
            get
            {
                return HasExitedRight ? ExitedForground : NonexitedForground;
            }
        }
        public bool HasExitedRight
        {
            get;
            set;
        }

        struct XRItem
        {
            public double X;
            public int Index;
            public string toolTip;
        }

        private double startY = -1;
        private double height = 8;
        private double radius = 3;
        private List<XRItem> XRItems;
        private int internalIndex = -1;

        public void DrawExtraData(ChartControl chartSource, IDrawingContext dc)
        {
            var dataSource = chartSource.MainCollection;
            if (dataSource == null)
            {
                return;
            }

            var items = dataSource.Items;

            int iStart = dataSource.FirstVisibleItemIndex;
            int count = dataSource.VisiableItemCount;

            IPen pen = DrawingObjectFactory.CreatePen(Forground, 1);

            var xMid = dataSource.ItemXDistance / 2;

            startY = -1;
            internalIndex = -1;
            XRItems = null;
            for (int i = iStart; i < iStart + count; i++)
            {
                var item = dataSource.Items[i];
                if (item.ExtraData == null)
                {
                    continue;
                }

                var xhHistory = item.ExtraData.Get(ExtraDataNames.XRName) as ExitRight;
                if (xhHistory != null)
                {
                    var x = dataSource.GetItemPositionX(i, ChartItemCollection.ValueName);

                    if (startY == -1)
                    {
                        startY = dataSource.ChartRegion.Bottom - height;
                        XRItems = new List<XRItem>();
                    }
                    XRItems.Add(new XRItem() {
                        X = x,
                        Index = i,
                        toolTip = xhHistory.ToString()
                    });

                    x = PointSnapper.SnapValue(x + xMid);

                    var point1 = new Point(x, dataSource.ChartRegion.Bottom);
                    var point2 = new Point(x, dataSource.ChartRegion.Bottom - 2);

                    dc.DrawLine(pen, point1, point2);

                    point1 = new Point(x, dataSource.ChartRegion.Bottom - 5);

                    dc.DrawEllipse(Forground, null, point1, radius, radius);
                }
            }
        }

        public bool HasTooltip
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

        private object tooltip;
        public object ToolTip
        {
            get
            {
                if (internalIndex != -1)
                {
                    if (tooltip == null)
                    {
                        return XRItems[internalIndex].toolTip;
                    }
                    else
                    {
                        var tt = tooltip as ToolTip;
                        if (tt == null)
                        {
                            return tooltip;
                        }
                        else
                        {
                            tt.Content = XRItems[internalIndex].toolTip;
                            return tt;
                        }
                    }
                }

                return null;
            }
            set
            {
                tooltip = value;
            }
        }

#if USINGCANVAS
        public PopupMenu ContextMenu
        {
            get;
            set;
        }
#else
        public ContextMenu ContextMenu
        {
            get;
            set;
        }
#endif

        public bool IsPointInRegion(Point point)
        {
            throw new NotSupportedException();
        }

        public bool CanSelect
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

        public bool IsSelected
        {
            get
            {
                throw new NotSupportedException();
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public bool CanChange
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

        public int GetNodeIndex(Point point)
        {
            internalIndex = -1;
            int index = -1;

            if (startY == -1)
                return index;

            if (point.Y >= startY && point.Y < startY + height)
            {
                for (int i = 0; i < XRItems.Count; i++)
                {
                    var XRItem = XRItems[i];
                    if (point.X >= XRItem.X - radius && point.X < XRItem.X + radius)
                    {
                        internalIndex = i;
                        index = XRItem.Index;
                        break;
                    }
                }
            }

            return index;
        }

        public void TranformPosition(Transform transform)
        {
            throw new NotSupportedException();
        }

        public void UpdateNodePosition(int nodeIndex, Point newPosition)
        {
            throw new NotSupportedException();
        }

    }
}
