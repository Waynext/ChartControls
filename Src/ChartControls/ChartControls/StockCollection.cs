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
#if USINGCANVAS
using Windows.UI.Xaml.Media;
using Windows.Foundation;
#else
using System.Windows.Media;
#endif

namespace ChartControls
{
    /// <summary>
    /// K线样式。
    /// </summary>
    public enum StockItemStyle
    {
        /// <summary>
        /// 蜡烛。
        /// </summary>
        Candle,
        /// <summary>
        /// 美国线。
        /// </summary>
        America,

        /// <summary>
        /// 线性。
        /// </summary>
        Linear
    }

    /// <summary>
    /// k线数据项。
    /// </summary>
    public class StockItem : ChartItem
    {
        /// <summary>
        /// 最高价格。
        /// </summary>
        public double High
        {
            get;
            set;
        }

        /// <summary>
        /// 最低价格。
        /// </summary>
        public double Low
        {
            get;
            set;
        }

        /// <summary>
        /// 开盘价格。
        /// </summary>
        public double Open
        {
            get;
            set;
        }

        /// <summary>
        /// 收盘价格。
        /// </summary>
        public double Close
        {
            get
            {
                return Value;
            }
            set
            {
                Value = value;
            }
        }

        /// <summary>
        /// 收盘价变动百分比。
        /// </summary>
        public double CloseChange
        {
            get
            {
                return ValueChange;
            }
            set
            {
                ValueChange = value;
            }
        }

        public override string ToString()
        {
            return string.Format("{0}-H{1:F2},O{2:F2},L{3:F2},C{4:F2},{5:P2}", Date.ToString(ConstStrings.DateTimeFormat),
                                High, Open, Low, Close, CloseChange);
        }
    }

    /// <summary>
    /// K线坐标结构。
    /// </summary>
    public struct VerticalLine
    {
        /// <summary>
        /// 最高点坐标。
        /// </summary>
        public double YHigh;
        /// <summary>
        /// 最低点坐标。
        /// </summary>
        public double YLow;
        /// <summary>
        /// 开盘点坐标。
        /// </summary>
        public double YOpen;
    }

    /// <summary>
    /// k线数据集合。
    /// </summary>
    public class StockItemCollection : ChartItemCollection
    {
        /// <summary>
        /// <see cref="GetItemValue(int, string)"/>
        /// </summary>
        public const string HighName = "High";
        /// <summary>
        /// <see cref="GetItemValue(int, string)"/>
        /// </summary>
        public const string LowName = "Low";
        /// <summary>
        /// <see cref="GetItemValue(int, string)"/>
        /// </summary>
        public const string CloseName = "Close";
        /// <summary>
        /// <see cref="GetItemValue(int, string)"/>
        /// </summary>
        public const string OpenName = "Open";
        /// <summary>
        /// <see cref="GetItemValue(int, string)"/>
        /// </summary>
        public const string CloseChangeName = "CloseChange";

        /// <summary>
        /// 构造函数。
        /// </summary>
        /// <param name="id">集合ID。</param>
        /// <param name="items">数据项列表。</param>
        /// <param name="penRaise">上升画笔。<see cref="DrawingObjectFactory.CreatePen(Brush, double)"/></param>
        /// <param name="penFall">下降画笔。<see cref="DrawingObjectFactory.CreatePen(Brush, double)"/></param>
        /// <param name="fill">填充画刷。</param>
        /// <param name="isItemDynamic">是否动态加载数据集合中的数据项。缺省值是false，表示不动态加载。</param>
        public StockItemCollection(CollectionId id, IEnumerable<StockItem> items, IPen penRaise, IPen penFall, Brush fill, bool isItemDynamic = false)
            : base(id, items, penRaise, fill, false, isItemDynamic)
        {
            if (penFall == null)
                throw new ArgumentNullException("penFall");

            RaisePen = penRaise;
            FallPen = penFall;
        }

        private IPen lineRaisePen;
        private IPen lineFallPen;
        private IPen rectRaisePen;
        private IPen rectFallPen;

        /// <summary>
        /// 上升线的画笔。
        /// </summary>
        public IPen RaisePen
        {
            get
            {
                return Pen;
            }
            set
            {
                Pen = value;

                lineRaisePen = CopyPen(value, PenLineCap.Square);
                rectRaisePen = CopyPen(value, PenLineCap.Flat);
            }
        }

        private IPen fallPen;
        /// <summary>
        /// 下跌线的画笔。
        /// </summary>
        public IPen FallPen
        {
            get
            {
                return fallPen;
            }
            set
            {
                fallPen = value;

                lineFallPen = CopyPen(value, PenLineCap.Square);
                rectFallPen = CopyPen(value, PenLineCap.Flat);
            }
        }

        private StockItemStyle itemStyle = StockItemStyle.Candle;

        /// <summary>
        /// k线样式。<see cref="StockItemStyle"/>
        /// </summary>
        public StockItemStyle ItemStyle
        {
            get
            {
                return itemStyle;
            }
            set
            {
                itemStyle = value;
                IsItemsConnected = itemStyle == StockItemStyle.Linear;
            }
        }

        /// <summary>
        /// <see cref="ChartItemCollection.GetItemValue(int, string)"/>
        /// </summary>
        public override double GetItemValue(int index, string valueName)
        {
            var sItem = Items[index] as StockItem;

            if (valueName == HighName)
                return sItem.High;
            if (valueName == LowName)
                return sItem.Low;
            if (valueName == CloseName)
                return sItem.Value;
            if (valueName == OpenName)
                return sItem.Open;
            else if (valueName == CloseChangeName)
            {
                return Items[index].ValueChange;
            }
            else
            {
                return base.GetItemValue(index, valueName);
            }
        }

        /// <summary>
        /// <see cref="ChartItemCollection.GetItemPositionY(int, string)"/>
        /// </summary>
        public override double GetItemPositionY(int index, string valueName)
        {
            int i = index - iStartPosition;
            if (valueName == HighName)
                return verticalLines[i].YHigh;
            if (valueName == LowName)
                return verticalLines[i].YLow;
            if (valueName == CloseName)
                return points[i].Y;
            if (valueName == OpenName)
                return verticalLines[i].YOpen;
            else
            {
                return base.GetItemPositionY(index, valueName);
            }
        }

        /// <summary>
        /// <see cref="ChartItemCollection.GetItemPositionX(int, string)"/>
        /// </summary>
        public override double GetItemPositionX(int index, string valueName)
        {
            return points[index - iStartPosition].X;
        }

        /// <summary>
        /// k线坐标数组。
        /// </summary>
        protected VerticalLine[] verticalLines;

        /// <summary>
        /// <see cref="ChartItemCollection.FindMaxAndMinIndex"/>
        /// </summary>
        protected override void FindMaxAndMinIndex()
        {
            if (IsEmpty)
                return;

            double max = double.MinValue, min = double.MaxValue;

            for (int i = iStartPosition; i < iStartPosition + points.Length; i++)
            {
                var item = Items[i] as StockItem;

                if (!IsItemValid(item))
                    continue;

                if (item.High > max)
                {
                    iMaxPosition = i - iStartPosition;
                    max = item.High;
                }
                if (item.Low < min)
                {
                    iMinPosition = i - iStartPosition;
                    min = item.Low;
                }
            }
        }

        /// <summary>
        /// <see cref="ChartItemCollection.CalculateCollectionPoints"/>
        /// </summary>
        protected override void CalculateCollectionPoints()
        {
            double max = 0;
            if (masterCollection == null)
            {
                max = GetMaxValue();
            }
            else
            {
                max = masterCollection.GetMaxValue();
            }

            for (int i = 0; i < points.Length; i++)
            {
                var itemTemp = Items[i + iStartPosition] as StockItem;
                if (IsItemValid(itemTemp))
                {
                    points[i].Y = ItemYDistance * (max - itemTemp.Value) + collectRect.Top;
                    verticalLines[i].YHigh = ItemYDistance * (max - itemTemp.High) + collectRect.Top;
                    verticalLines[i].YLow = ItemYDistance * (max - itemTemp.Low) + collectRect.Top;
                    verticalLines[i].YOpen = ItemYDistance * (max - itemTemp.Open) + collectRect.Top;
                }
                else
                {
                    points[i].Y = valueNA;
                    verticalLines[i].YHigh = valueNA;
                    verticalLines[i].YLow = valueNA;
                    verticalLines[i].YOpen = valueNA;
                }
            }
        }

        /// <summary>
        /// <see cref="ChartItemCollection.GetMaxValue"/>
        /// </summary>
        public override double GetMaxValue()
        {
            if (IsEmpty)
                return 0f;

            return (Items[iStartPosition + iMaxPosition] as StockItem).High;
        }

        /// <summary>
        /// <see cref="ChartItemCollection.GetMinValue"/>
        /// </summary>
        public override double GetMinValue()
        {
            if (IsEmpty)
                return 0f;

            return (Items[iStartPosition + iMinPosition] as StockItem).Low;
        }

        /// <summary>
        /// <see cref="ChartItemCollection.CopyFromMaster"/>
        /// </summary>
        protected override void CopyFromMaster()
        {
            base.CopyFromMaster();
            verticalLines = new VerticalLine[(masterCollection as StockItemCollection).verticalLines.Length];
            Array.Copy((masterCollection as StockItemCollection).verticalLines, verticalLines, verticalLines.Length);
        }

        /// <summary>
        /// <see cref="ChartItemCollection.Draw(IDrawingContext)"/>
        /// </summary>
        public override void Draw(IDrawingContext dc)
        {
            if (points == null || !points.Any())
            {
                return;
            }

            if (ItemStyle == StockItemStyle.Linear)
            {
                base.Draw(dc);
                return;
            }

            double halfItemWidth = ItemXDistance / 2;
            if (halfItemWidth + ItemXSpan < 1)
            {
                DrawDenseChart(dc);
                return;
            }

            int i = -1;

            do
            {
                i++;
            } while (i < points.Length && !IsPointValid(points[i]));

            GeometryGroup groupLineRaise = new GeometryGroup(), groupLineFall = new GeometryGroup();
            GeometryGroup groupRectRaise = new GeometryGroup(), groupRectFall = new GeometryGroup();

            Point p1 = new Point();
            Point p2 = new Point();
            List<Point> PtList = new List<Point>();

            for (; i < points.Length; i++)
            {
                if (!IsPointValid(points[i]))
                    continue;
                var x = points[i].X;
                var yC = points[i].Y;
                var yH = verticalLines[i].YHigh;
                var yL = verticalLines[i].YLow;
                var yO = verticalLines[i].YOpen;
                bool isRaise = yC <= yO;
                if (yC == yO)
                {
                    isRaise = Items[i + iStartPosition].ValueChange >= 0;
                }
                GeometryGroup groupLine, groupRect = null;

                var xMid = PointSnapper.SnapValue(x + halfItemWidth);
                if (ItemStyle == StockItemStyle.Candle)
                {
                    if (ItemXDistance >= 3)
                    {                   
                        p1.X = p2.X = xMid;
                        p1.Y = PointSnapper.SnapValue(yH);

                        double yNext = 0;
                        if (isRaise)
                        {
                            p2.Y = PointSnapper.SnapValue(yC);
                            groupLine = groupLineRaise;
                            groupRect = groupRectRaise;
                            yNext = PointSnapper.SnapValue(yO);
                        }
                        else
                        {
                            p2.Y = PointSnapper.SnapValue(yO);
                            groupLine = groupLineFall;
                            groupRect = groupRectFall;
                            yNext = PointSnapper.SnapValue(yC);
                        }

                        if (p1.Y != p2.Y)
                        {
                            LineGeometry line1 = new LineGeometry()
                            {
                                StartPoint = p1,
                                EndPoint = p2
                            };

#if USINGCANVAS            
#else
                            line1.Freeze();
#endif
                            groupLine.Children.Add(line1);
                        }

                        p1.X = PointSnapper.SnapValue(x);
                        p1.Y = p2.Y;
                        p2.X = PointSnapper.SnapValue(x + ItemXDistance) - 1;
                        p2.Y = yNext;

                        if (p1.Y == p2.Y)
                        {
                            LineGeometry rLine = new LineGeometry()
                            {
                                StartPoint = p1,
                                EndPoint = p2
                            };
#if USINGCANVAS            
#else
                            rLine.Freeze();
#endif
                            groupLine.Children.Add(rLine);
                        }
                        else
                        {
                            RectangleGeometry rect = new RectangleGeometry()
                            {
                                Rect = new Rect(p1, p2)
                            };
#if USINGCANVAS            
#else
                            rect.Freeze();
#endif
                            groupRect.Children.Add(rect);
                        }
                        

                        p1.X = p2.X = xMid;
                        p1.Y = yNext;
                        p2.Y = PointSnapper.SnapValue(yL);

                        if (p1.Y != p2.Y)
                        {
                            LineGeometry line2 = new LineGeometry()
                            {
                                StartPoint = p1,
                                EndPoint = p2
                            };
#if USINGCANVAS            
#else
                            line2.Freeze();
#endif
                            groupLine.Children.Add(line2);
                        }
                    }
                    else
                    {
                        if (isRaise)
                        {
                            groupLine = groupLineRaise;
                        }
                        else
                        {
                            groupLine = groupLineFall;
                        }


                        p1.X = p2.X = xMid;
                        p1.Y = PointSnapper.SnapValue(yH);
                        p2.Y = PointSnapper.SnapValue(yL);
                        if (p1.Y == p2.Y)
                        {
                            p2.Y++;
                        }

                        LineGeometry line1 = new LineGeometry
                        {
                            StartPoint = p1,
                            EndPoint = p2
                        };
#if USINGCANVAS            
#else
                        line1.Freeze();
#endif
                        groupLine.Children.Add(line1);
                    }
                }
                else
                {
                    if (isRaise)
                    {
                        groupLine = groupLineRaise;
                    }
                    else
                    {
                        groupLine = groupLineFall;
                    }

                    p1.X = p2.X = xMid;
                    p1.Y = PointSnapper.SnapValue(yH);
                    p2.Y = PointSnapper.SnapValue(yL);
                    if (p1.Y == p2.Y)
                    {
                        p2.Y++;
                    }

                    LineGeometry line1 = new LineGeometry()
                    {
                        StartPoint = p1,
                        EndPoint = p2
                    };

#if USINGCANVAS            
#else
                    line1.Freeze();
#endif
                    groupLine.Children.Add(line1);

                    p1.X = PointSnapper.SnapValue(x);
                    p1.Y = p2.Y = PointSnapper.SnapValue(yO);
                    p2.X = xMid;

                    if (p1.X != p2.X)
                    {
                        LineGeometry line2 = new LineGeometry()
                        {
                            StartPoint = p1,
                            EndPoint = p2
                        };
#if USINGCANVAS            
#else
                        line2.Freeze();
#endif
                        groupLine.Children.Add(line2);
                    }
                    

                    p1.X = p2.X;
                    p1.Y = p2.Y = PointSnapper.SnapValue(yC);
                    p2.X = PointSnapper.SnapValue(x + ItemXDistance);
                    if (p1.X != p2.X)
                    {
                        LineGeometry line3 = new LineGeometry()
                        {
                            StartPoint = p1,
                            EndPoint = p2
                        };
#if USINGCANVAS            
#else
                        line3.Freeze();
#endif
                        groupLine.Children.Add(line3);
                    }
        
                }
            }

#if USINGCANVAS            
#else
            groupLineRaise.Freeze();
            groupLineFall.Freeze();
            groupRectRaise.Freeze();
            groupRectFall.Freeze();
#endif
            dc.DrawGeometry(null, lineRaisePen, groupLineRaise);
            dc.DrawGeometry(null, lineFallPen, groupLineFall);
            dc.DrawGeometry(null, rectRaisePen, groupRectRaise);
            dc.DrawGeometry(rectFallPen.Brush, rectFallPen, groupRectFall);
        }

        private void DrawDenseChart(IDrawingContext dc)
        {
            int i = -1;

            do
            {
                i++;
            } while (i < points.Length && !IsPointValid(points[i]));

            GeometryGroup groupRaise = new GeometryGroup(), groupFall = new GeometryGroup();

            Point p1 = new Point(ChartItemCollection.valueNA, ChartItemCollection.valueNA);
            Point p2 = new Point(ChartItemCollection.valueNA, ChartItemCollection.valueNA);
            double open = ChartItemCollection.valueNA;
            double close = open;

            List<Point> PtList = new List<Point>();

            double halfItemWidth = ItemXDistance / 2;

            int iStart = i;


            for (; i < points.Length; i++)
            {
                if (!IsPointValid(points[i]))
                    continue;

                var x = points[i].X;
                var yC = points[i].Y;
                var yH = verticalLines[i].YHigh;
                var yL = verticalLines[i].YLow;
                var yO = verticalLines[i].YOpen;
                
                GeometryGroup group = null;

                var xMid = PointSnapper.SnapValue(x + halfItemWidth);

                if (p1.X != xMid)
                {
                    if (i != iStart)
                    {
                        if (p1.Y == p2.Y)
                        {
                            p2.Y++;
                        }

                        bool isRaise = close <= open;

                        if (isRaise)
                        {
                            group = groupRaise;
                        }
                        else
                        {
                            group = groupFall;
                        }

                        LineGeometry line1 = new LineGeometry()
                        {
                            StartPoint = p1,
                            EndPoint = p2
                        };
#if USINGCANVAS            
#else
                        line1.Freeze();
#endif
                        group.Children.Add(line1);
                    }

                    open = yO;
                    close = yC;
                    p1.X = p2.X = xMid;
                    p1.Y = PointSnapper.SnapValue(yH);
                    p2.Y = PointSnapper.SnapValue(yL);
                }
                else
                {
                    var YH = PointSnapper.SnapValue(yH);
                    if (YH < p1.Y)
                    {
                        p1.Y = YH;
                    }
                    var YL = PointSnapper.SnapValue(yL);
                    if (YL > p2.Y)
                    {
                        p2.Y = YL;
                    }
                    close = yC;
                } 
            }
#if USINGCANVAS            
#else
            groupRaise.Freeze();
            groupFall.Freeze();
#endif
            dc.DrawGeometry(null, lineRaisePen, groupRaise);
            dc.DrawGeometry(null, lineFallPen, groupFall);

        }

        /// <summary>
        /// <see cref="ChartItemCollection.RearrayPointCollection(int)"/>
        /// </summary>
        protected override void RearrayPointCollection(int length)
        {
            points = new Point[length];
            verticalLines = new VerticalLine[length];
        }

        /// <summary>
        /// <see cref="ChartItemCollection.ConvertChartItemLog10(ChartItem)"/>
        /// </summary>
        protected override void ConvertChartItemLog10(ChartItem item)
        {
            StockItem sItem = item as StockItem;
            sItem.Value = Math.Log10(sItem.Value);
            sItem.High = Math.Log10(sItem.High);
            sItem.Low = Math.Log10(sItem.Low);
            sItem.Open = Math.Log10(sItem.Open);
        }

        /// <summary>
        /// <see cref="ChartItemCollection.ConvertChartItemPow10(ChartItem)"/>
        /// </summary>
        protected override void ConvertChartItemPow10(ChartItem item)
        {
            StockItem sItem = item as StockItem;
            sItem.Value = Math.Pow(10, sItem.Value);
            sItem.High = Math.Pow(10, sItem.High);
            sItem.Low = Math.Pow(10, sItem.Low);
            sItem.Open = Math.Pow(10, sItem.Open);
        }

        /// <summary>
        /// <see cref="CopyAndAdjustItemValue(ChartItem, CoordinateType)"/>
        /// </summary>
        public override ChartItem CopyAndAdjustItemValue(ChartItem item, CoordinateType coordinateType)
        {
            if (item == null)
                return null;

            StockItem sItemRet = null;
            if (coordinateType == CoordinateType.Log10)
            {
                StockItem sItem = item as StockItem;

                sItemRet = new StockItem()
                {
                    Value = Math.Pow(10, sItem.Value),
                    High = Math.Pow(10, sItem.High),
                    Low = Math.Pow(10, sItem.Low),
                    Open = Math.Pow(10, sItem.Open),
                    CloseChange = sItem.CloseChange,
                    Date = sItem.Date,
                    ExtraData = sItem.ExtraData
                };
            }
            else
            {
                StockItem sItem = item as StockItem;

                sItemRet = new StockItem()
                {
                    Value = sItem.Value,
                    High = sItem.High,
                    Low = sItem.Low,
                    Open = sItem.Open,
                    CloseChange = sItem.CloseChange,
                    Date = sItem.Date,
                    ExtraData = sItem.ExtraData
                };
            }

            return sItemRet;
        }

        /// <summary>
        /// <see cref="ChartItemCollection.ConvertFrom(QueryItem)"/>
        /// </summary>
        public override ChartItem ConvertFrom(QueryItem queryItem)
        {
            StockItem item = new StockItem()
            {
                Date = queryItem.Date
            };

            if (queryItem.Close != null)
            {
                item.Close = queryItem.Close.Value;
            }
            if (queryItem.High != null)
            {
                item.High = queryItem.High.Value;
            }
            if (queryItem.Low != null)
            {
                item.Low = queryItem.Low.Value;
            }
            if (queryItem.Open != null)
            {
                item.Open = queryItem.Open.Value;
            }
            return item;
        }
    }
}
