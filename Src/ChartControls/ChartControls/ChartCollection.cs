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
using Windows.Foundation;
using Windows.UI.Xaml.Media;
#else
using System.Windows.Media;
#endif

namespace ChartControls
{
    /// <summary>
    /// 图表数据项。它是其他类型数据项的基类。
    /// </summary>
    public class ChartItem
    {
        /// <summary>
        /// 主要值。
        /// </summary>
        public double Value
        {
            get;
            set;
        }

        /// <summary>
        /// 主要值较上一图表数据项的主要值的变动。
        /// </summary>
        public double ValueChange
        {
            get;
            set;
        }

        /// <summary>
        /// 主要值的时间。
        /// </summary>
        public DateTime Date
        {
            get;
            set;
        }

        /// <summary>
        /// 额外数据。 <see cref="ExtraData"/>
        /// </summary>
        public ExtraData ExtraData
        {
            get;
            set;
        }

        public override string ToString()
        {
            return string.Format("{0}|{1:F2}|{2:P2}", Date.ToString(ConstStrings.DateTimeFormat), Value, ValueChange);
        }
    }

    /// <summary>
    /// 图表数据项比较器，通过时间比较。
    /// </summary>
    public class ChartItemComparer : IComparer<ChartItem>
    {
        public int Compare(ChartItem x, ChartItem y)
        {
            return x.Date.CompareTo(y.Date);
        }
    }

    /// <summary>
    /// 额外数据字典，可以用存储分红，除权数据等等。
    /// </summary>
    public class ExtraData
    {
        private Dictionary<string, object> dict = new Dictionary<string,object>();

        /// <summary>
        /// 添加额外数据项。
        /// </summary>
        public void Add(string key, object value)
        {
            dict.Add(key, value);
        }
        /// <summary>
        /// 删除额外数据项。
        /// </summary>
        public object Get(string key)
        {
            object ret = null;

            dict.TryGetValue(key, out ret);
            return ret;
        }
    }

    /// <summary>
    /// 图表数据包装，被用于交互事件中。
    /// </summary>
    public class ChartItemWrap
    {
        /// <summary>
        /// 图表数据项。
        /// </summary>
        public ChartItem ChartItem
        {
            get;
            set;
        }

        /// <summary>
        /// 交互位置，比如鼠标位置，触碰位置。
        /// </summary>
        public Point Point
        {
            get;
            set;
        }
    }

    /// <summary>
    /// 刻度。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Scale<T>
    {
        /// <summary>
        /// 构造函数。
        /// </summary>
        /// <param name="pos">刻度位置。</param>
        /// <param name="value">刻度值。</param>
        public Scale(double pos, T value)
        {
            Pos = pos;
            Value = value;
        }

        /// <summary>
        /// 刻度位置。
        /// </summary>
        public double Pos
        {
            get;
            set;
        }

        /// <summary>
        /// 刻度值。
        /// </summary>
        public T Value
        {
            get;
            set;
        }

        /// <summary>
        /// 辅助刻度值。
        /// </summary>
        public T AssistValue
        {
            get;
            set;
        }
    }

    /// <summary>
    /// 插入数据位置。
    /// </summary>
    public enum AddLocation {
        /// <summary>
        /// 头部。
        /// </summary>
        Head,
        /// <summary>
        /// 尾部。
        /// </summary>
        Tail
    };

    /// <summary>
    /// 数据集合ID。
    /// </summary>
    public class CollectionId
    {
        private static int idSeed = 0;

        /// <summary>
        /// 构造函数。
        /// </summary>
        /// <param name="displayId">显示ID，如股票代码。</param>
        /// <param name="displayName">显示名字，如公司简称，或者指标名称。</param>
        /// <param name="marketId">市场ID。</param>
        public CollectionId(string displayId, string displayName = null, string marketId = null)
        {
            if (displayId == null)
                throw new ArgumentNullException("displayId");

            Id = idSeed++; ;
            DisplayId = displayId;
            MarketId = marketId;
            Name = displayName;
        }
        
        /// <summary>
        /// ID。
        /// </summary>
        public int Id
        {
            get;
            private set;
        }

        /// <summary>
        /// 显示ID，如股票代码。
        /// </summary>
        public string DisplayId
        {
            get;
            set;
        }

        /// <summary>
        /// 市场ID。
        /// </summary>
        public string MarketId
        {
            get;
            set;
        }

        /// <summary>
        /// 显示名字，如公司简称，或者指标名称。
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        public override bool Equals(object obj)
        {
            var other = obj as CollectionId;
            if(other == null)
            {
                return false;
            }
            else
            {
                return other.Id == Id || other.DisplayId == DisplayId;
            }
        }

        public override int GetHashCode()
        {
            return DisplayId.GetHashCode();
        }

        public override string ToString()
        {
            return DisplayId;
        }

    }

    /// <summary>
    /// 图表数据集合，代表的图表是曲线。
    /// 它是其他数据集合的基类。
    /// </summary>
    public class ChartItemCollection : IPoint2ValuePoint
    {
        /// <summary>
        /// <see cref="GetItemValue(int, string)"/>
        /// </summary>
        public const string ValueName = "Value";
        /// <summary>
        /// <see cref="GetItemValue(int, string)"/>
        /// </summary>
        public const string ValueChangeName = "ValueChange";

        private const double ratio = 1.0 / 5.0;

        /// <summary>
        /// 构造函数。
        /// </summary>
        /// <param name="id">数据集合ID。</param>
        /// <param name="items">数据项列表。</param>
        /// <param name="pen">画笔，通过画图工厂创建。<see cref="DrawingObjectFactory.CreatePen(Brush, double)"/></param>
        /// <param name="fill">填充画刷</param>
        /// <param name="isItemConnected">数据集合中点之间的连接线是否需要绘制。缺省值是true，表示绘制。</param>
        /// <param name="isItemDynamic">是否动态加载数据集合中的数据项。缺省值是false，表示不动态加载。</param>
        public ChartItemCollection(CollectionId id, IEnumerable<ChartItem> items, IPen pen, Brush fill, bool isItemConnected = true, bool isItemDynamic = false)
        {
            if (pen == null)
                throw new ArgumentNullException("pen");

            if (items != null)
                Items = new List<ChartItem>(items);
            else
                Items = new List<ChartItem>();

            Pen = pen;

            Fill = fill;
            IsItemsConnected = isItemConnected;

            MaxItemXDistance = 100;
            MinItemXDistance = 0.01;

            ItemXDistance = 5.0;

            ItemXSpan = ItemXDistance * ratio;

            collId = id;

            if (isItemDynamic)
            {
                isHeadEnd = false;
                isTailEnd = false;
            }

            FixedVisibleCount = -1;

            IsScalesOptimized = true;
        }

        /// <summary>
        /// 未定义值。如果数据集合中任何未定义的数据，请务必值此赋值。
        /// </summary>
        public const double valueNA = double.PositiveInfinity;

        private CollectionId collId;

        /// <summary>
        /// 数据集合Id。
        /// </summary>
        public CollectionId Id
        {
            get
            {
                return collId;
            }
        }

        private IPen linePen;
        private IPen pen;

        /// <summary>
        /// 画笔。
        /// </summary>
        public IPen Pen
        {
            get
            {
                return pen;
            }
            set
            {
                pen = value;
                linePen = CopyPen(pen, PenLineCap.Round);
            }
        }

        /// <summary>
        /// 填充颜色。
        /// </summary>
        public Brush Fill
        {
            get;
            set;
        }

        /// <summary>
        /// 数据项列表。
        /// </summary>
        public List<ChartItem> Items
        {
            get;
            set;
        }

        /// <summary>
        /// 数据集合中点之间的连接线是否需要绘制。
        /// </summary>
        public bool IsItemsConnected
        {
            get;
            set;
        }

        /// <summary>
        /// 数据项的间距。
        /// </summary>
        public double ItemXSpan
        {
            get;
            protected set;
        }

        /// <summary>
        /// 数据项的最大宽度。
        /// </summary>
        public double MaxItemXDistance
        {
            get;
            set;
        }

        /// <summary>
        /// 数据项的最小宽度。
        /// </summary>
        public double MinItemXDistance
        {
            get;
            set;
        }

        /// <summary>
        /// 数据项的宽度。
        /// </summary>
        public double ItemXDistance
        {
            get;
            protected set;
        }

        /// <summary>
        /// Y轴方向每个单位的距离。
        /// </summary>
        public double ItemYDistance
        {
            get;
            protected set;
        }

        /// <summary>
        /// 第一个可见的数据项的索引。
        /// </summary>
        public int FirstVisibleItemIndex
        {
            get
            {
                return iStartPosition;
            }
        }

        /// <summary>
        /// 可见数据项的数目。
        /// </summary>
        public int VisiableItemCount
        {
            get
            {
                return points != null ? points.Length : 0;
            }
        }

        /// <summary>
        /// 可见数据项集合中最大值的索引减去第一个可见的数据项的索引。
        /// </summary>
        public int MaxVisiableItemIndexDiff
        {
            get
            {
                return iMaxPosition;
            }
        }

        /// <summary>
        /// 可见数据项集合中最小值的索引减去第一个可见的数据项的索引。
        /// </summary>
        public int MinVisiableItemIndexDiff
        {
            get
            {
                return iMinPosition;
            }
        }

        /// <summary>
        /// 数据集合的可见区域。
        /// </summary>
        public Rect ChartRegion
        {
            get
            {
                return collectRect;
            }
        }

        /// <summary>
        /// 设置固定数目的可见项。
        /// </summary>
        protected int FixedVisibleCount
        {
            get;
            set;
        }

        /// <summary>
        /// 刻度划分是否是取整的。缺省是true，表示是取整的。false，表示是刻度平均分配的。
        /// </summary>
        internal bool IsScalesOptimized
        {
            get;
            set;
        }
        /// <summary>
        /// Y轴刻度数量。
        /// </summary>
        internal int YColumnCount
        {
            get;
            set;
        }

        /// <summary>
        /// X轴刻度数量，缺省值是4。
        /// </summary>
        internal int XColumnCount
        {
            get;
            set;
        }
        /// <summary>
        /// 取得索引位置上的数据项时间
        /// </summary>
        /// <param name="index">数据集合索引</param>
        /// <returns></returns>
        public DateTime GetDate(int index)
        {
            return Items[index].Date;
        }

        /// <summary>
        /// 取数据项中某值。
        /// </summary>
        /// <param name="index">索引。</param>
        /// <param name="valueName">值的名称。</param>
        /// <returns></returns>
        public virtual double GetItemValue(int index, string valueName)
        {
            if (valueName == ValueName)
                return Items[index].Value;
            else if (valueName == ValueChangeName)
            {
                return Items[index].ValueChange;
            }
            else
            {
                return valueNA;
            }
        }

        /// <summary>
        /// 取数据项中某值的Y坐标。
        /// </summary>
        /// <param name="index">索引。</param>
        /// <param name="valueName">值的名称。<see cref="ValueName"/> <see cref="ValueChangeName"/></param>
        /// <returns></returns>
        public virtual double GetItemPositionY(int index, string valueName)
        {
            return points[index - iStartPosition].Y;
        }

        /// <summary>
        /// 取数据项中某值的X坐标。
        /// </summary>
        /// <param name="index">索引。</param>
        /// <param name="valueName">值的名称。</param>
        /// <returns></returns>
        public virtual double GetItemPositionX(int index, string valueName)
        {
            return points[index - iStartPosition].X;
        }

        /// <summary>
        /// 第一个可见项的索引。
        /// </summary>
        protected int iStartPosition = -1;

        /// <summary>
        /// 最初的可见项数目。
        /// </summary>
        protected int originalPointLength;

        /// <summary>
        /// 可见项集合的坐标数组。
        /// </summary>
        protected Point[] points;

        /// <summary>
        /// 当前项的索引。
        /// </summary>
        protected int iCurrentPosition = -1;

        /// <summary>
        /// 可见项集合中最小值的索引。
        /// </summary>
        protected int iMinPosition = -1;

        /// <summary>
        /// 可见项集合中最大值的索引。
        /// </summary>
        protected int iMaxPosition = -1;

        /// <summary>
        /// 数据集合的可见区域。
        /// </summary>
        protected Rect collectRect;

        private bool isHeadEnd = true;
        private bool isTailEnd = true;

        /// <summary>
        /// 本集合从属的图表数据集合。
        /// </summary>
        protected ChartItemCollection masterCollection;

        /// <summary>
        /// 坐标类型 <see cref="CoordinateType"/>
        /// </summary>
        protected CoordinateType coordinateType = CoordinateType.Linear;

        /// <summary>
        /// 计算可见集合及它们的位置信息。在重绘前被调用。
        /// 如果需要重新定义计算方法，可以重载此函数。但是通常只需要重载被该方法调用的子方法即可，如果仍无法满足，再考虑重载。
        /// </summary>
        /// <param name="rect">可见区域</param>
        public virtual void CalculatePosition(Rect rect)
        {
            if (masterCollection == null)
            {
                collectRect = rect;

                double width = rect.Width, height = rect.Height;

                if (iStartPosition == -1)
                {
                    int itemCount = 0;

                    int containsCount = GetContainsCount();
                    iStartPosition = Items.Count - containsCount;
                    if (iStartPosition <= 0)
                    {
                        iStartPosition = 0;

                        itemCount = Math.Min(CalculateItemWidthAndCount(width, Items.Count), Items.Count);
                    }
                    else
                    {
                        itemCount = containsCount;
                    }

                    originalPointLength = itemCount;
                    RearrayPointCollection(itemCount);

                    if(IsEmpty)
                    {
                        iStartPosition = -1;
                    }
                }

                CalculteCollectionPointsX();

                FindMaxAndMinIndex();

                CalculateYDistance();
            }
            else
            {
                CopyFromMaster();
            }

            CalculateCollectionPoints();
        }

        /// <summary>
        /// 计算可见集合项的X坐标集合，填充<see cref="points"/>数组的X坐标。
        /// <see cref="CalculatePosition"/>会调用此函数。
        /// 如果数据项有更多X坐标需要计算，可重载此函数。      
        /// </summary>
        protected virtual void CalculteCollectionPointsX()
        {
            double xDis = 0;
            double xSpan = ItemXDistance + ItemXSpan;
            for (int i = 0; i < points.Length; i++)
            {
                points[i] = new Point();
                points[i].X = xDis + collectRect.Left;

                xDis += xSpan;
            }
        }

        /// <summary>
        /// 寻找可见项集合中最大最小值的索引，设置<see cref="iMaxPosition"/>和<see cref="iMinPosition"/>。
        /// <see cref="CalculatePosition"/>会调用此函数。
        /// 如果数据项中有更多数据用于最大最小值比较，可重载此函数。 
        /// </summary>
        protected virtual void FindMaxAndMinIndex()
        {
            double max = double.MinValue, min = double.MaxValue;

            for (int i = iStartPosition; i < iStartPosition + points.Length; i++)
            {
                if (!IsItemValid(Items[i]))
                    continue;

                if (Items[i].Value > max)
                {
                    iMaxPosition = i - iStartPosition;
                    max = Items[i].Value;
                }
                if (Items[i].Value < min)
                {
                    iMinPosition = i - iStartPosition;
                    min = Items[i].Value;
                }
            }
        }

        /// <summary>
        /// 计算Y轴方向上每个单位的距离，设置<see cref="ItemYDistance"/>。
        /// <see cref="CalculatePosition"/>会调用此函数。
        /// 有需要，可重载此函数。 
        /// </summary>
        protected virtual void CalculateYDistance()
        {
            double height = collectRect.Height - 1;

            double max = GetMaxValue();
            double min = GetMinValue();

            ItemYDistance = 0.0;
            ItemYDistance = height / (max - min);
        }

        /// <summary>
        /// 计算可见集合项的Y坐标集合，填充<see cref="points"/>数组的Y坐标。
        /// <see cref="CalculatePosition"/>会调用此函数。
        /// 如有更多Y坐标有需要计算，可重载此函数。      
        /// </summary>
        protected virtual void CalculateCollectionPoints()
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
                if (iStartPosition + i < Items.Count)
                {
                    var itemTemp = Items[i + iStartPosition];
                    if (IsItemValid(itemTemp))
                        points[i].Y = ItemYDistance * (max - itemTemp.Value) + collectRect.Top;
                    else
                        points[i].Y = valueNA;
                }
                else
                {
                    points[i].Y = valueNA;
                }
            }
        }

        /// <summary>
        /// 取最大值。
        /// 有需要，可重载此函数。
        /// </summary>
        /// <returns>最大值。</returns>
        public virtual double GetMaxValue()
        {
            if (IsEmpty)
                return 0f;

            return Items[iStartPosition + iMaxPosition].Value;
        }

        /// <summary>
        /// 取最小值。
        /// 有需要，可重载此函数。
        /// </summary>
        /// <returns>最小值。</returns>
        public virtual double GetMinValue()
        {
            if (IsEmpty)
                return 0f;
            return Items[iStartPosition + iMinPosition].Value;
        }

        /// <summary>
        /// 复制主人集合的绘制参数，省得去自己计算。
        /// </summary>
        protected virtual void CopyFromMaster()
        {
            ItemXDistance = masterCollection.ItemXDistance;
            ItemYDistance = masterCollection.ItemYDistance;
            MaxItemXDistance = masterCollection.MaxItemXDistance;
            MinItemXDistance = masterCollection.MinItemXDistance;
            ItemXSpan = masterCollection.ItemXSpan;

            iStartPosition = masterCollection.iStartPosition;
            originalPointLength = masterCollection.originalPointLength;
            points = new Point[masterCollection.points.Length];
            Array.Copy(masterCollection.points, points, points.Length);
            iCurrentPosition = masterCollection.iCurrentPosition;

            iMinPosition = masterCollection.iMinPosition;
            iMaxPosition = masterCollection.iMaxPosition;
            collectRect = masterCollection.collectRect;

            FixedVisibleCount = masterCollection.FixedVisibleCount;
        }

#if USINGCANVAS
        /// <summary>
        /// 数据项间距大于等于1时，调用绘制。
        /// </summary>
        /// <param name="dc">绘制上下文。<see cref="IDrawingContext"/></param>
        /// <param name="pointArray">可见项坐标数组。</param>
        /// <param name="pen">画笔。<see cref="IPen"/></param>
        protected void DrawLooseChart(IDrawingContext dc, Point[] pointArray, IPen pen)
        {
            int i = -1;

            do
            {
                i++;
                if (i == pointArray.Length)
                    return;
            } while (!IsPointValid(pointArray[i]));

            var xMid = ItemXDistance / 2;

            List<Point> PtList = new List<Point>();
            var x = PointSnapper.SnapValue(pointArray[i].X + 1);
            var y = PointSnapper.SnapValue(pointArray[i].Y);
            var tempPt = new Point(x, y);
            Point startPoint = tempPt;
            i++;

            PathSegmentCollection lines = new PathSegmentCollection();

            for (; i < pointArray.Length; i++)
            {
                if (!IsPointValid(pointArray[i]))
                    continue;

                x = PointSnapper.SnapValue(pointArray[i].X + xMid);
                y = PointSnapper.SnapValue(pointArray[i].Y);
                tempPt = new Point(x, y);
                lines.Add(new LineSegment() { Point = tempPt });
            }

            PathFigure figure = new PathFigure()
            {
                StartPoint = startPoint,
                IsFilled = false,
                IsClosed = false,
                Segments = lines
            };
            PathGeometry geo = new PathGeometry();
            geo.Figures.Add(figure);

            dc.DrawGeometry(null, pen, geo);
        }

        /// <summary>
        /// 数据项间距小于1时，调用绘制。
        /// </summary>
        /// <param name="dc">绘制上下文。<see cref="IDrawingContext"/></param>
        /// <param name="pointArray">可见项坐标数组。</param>
        /// <param name="pen">画笔。<see cref="IPen"/></param>
        protected void DrawDenseChart(IDrawingContext dc, Point[] pointArray, IPen pen)
        {
            int i = -1;

            do
            {
                i++;
                if (i == pointArray.Length)
                    return;
            } while (!IsPointValid(pointArray[i]));

            var xMid = ItemXDistance / 2;
            
            List<Point> PtList = new List<Point>();
            var x = PointSnapper.SnapValue(pointArray[i].X);
            var yMin = PointSnapper.SnapValue(pointArray[i].Y);
            var iMin = i;
            var yMax = yMin;
            var iMax = i;
            i++;

            Point startPoint;

            while (i < pointArray.Length)
            {
                if (IsPointValid(pointArray[i]))
                {
                    var tempX = PointSnapper.SnapValue(pointArray[i].X);
                    if (tempX != x)
                    {
                        break;
                    }
                    else
                    {
                        var tempY = PointSnapper.SnapValue(pointArray[i].Y);
                        if (tempY < yMin)
                        {
                            yMin = tempY;
                            iMin = i;
                        }
                        else if (tempY > yMax)
                        {
                            yMax = tempY;
                            iMax = i;
                        }
                    }
                }
                i++;
            }

            if (iMin < iMax)
            {
                var tempPt = new Point(x, yMin);
                startPoint = tempPt;
                if (yMin != yMax)
                {
                    tempPt = new Point(tempPt.X, yMax);
                    PtList.Add(tempPt);
                }
            }
            else
            {
                var tempPt = new Point(x, yMax);
                startPoint = tempPt;
                if (yMin != yMax)
                {
                    tempPt = new Point(tempPt.X, yMin);
                    PtList.Add(tempPt);
                }
            }

            PathSegmentCollection lines = new PathSegmentCollection();
            for (; i < pointArray.Length; )
            {
                if (!IsPointValid(pointArray[i]))
                {
                    i++;
                    continue;
                }

                x = PointSnapper.SnapValue(pointArray[i].X + xMid);
                yMin = yMax = PointSnapper.SnapValue(pointArray[i].Y);

                i++;

                if (i != pointArray.Length)
                {

                    while (i < pointArray.Length)
                    {
                        if (IsPointValid(pointArray[i]))
                        {
                            var tempX = PointSnapper.SnapValue(pointArray[i].X);
                            if (tempX != x)
                            {
                                break;
                            }
                            else
                            {
                                var tempY = PointSnapper.SnapValue(pointArray[i].Y);
                                if (tempY < yMin)
                                {
                                    yMin = tempY;
                                    iMin = i;
                                }
                                else if (tempY > yMax)
                                {
                                    yMax = tempY;
                                    iMax = i;
                                }
                            }
                        }
                        i++;
                    }
                }

                if (iMin < iMax)
                {
                    var tempPt = new Point(x, yMin);
                    PtList.Add(tempPt);
                    if (yMin != yMax)
                    {
                        tempPt = new Point(x, yMax);
                        lines.Add(new LineSegment() { Point = tempPt });
                    }
                }
                else
                {
                    var tempPt = new Point(x, yMax);
                    PtList.Add(tempPt);
                    if (yMin != yMax)
                    {
                        tempPt = new Point(x, yMin);
                        lines.Add(new LineSegment() { Point = tempPt });
                    }
                }
            }

            PathFigure figure = new PathFigure()
            {
                StartPoint = startPoint,
                IsFilled = false,
                IsClosed = false,
                Segments = lines
            };

            PathGeometry geo = new PathGeometry();
            geo.Figures.Add(figure);
            
            dc.DrawGeometry(null, pen, geo);
        }
#else
        /// <summary>
        /// 数据项间距小于1时，调用绘制。
        /// </summary>
        /// <param name="dc">绘制上下文。<see cref="IDrawingContext"/></param>
        /// <param name="pointArray">可见项坐标数组。</param>
        /// <param name="pen">画笔。<see cref="IPen"/></param>
        protected void DrawDenseChart(IDrawingContext dc, Point[] pointArray, IPen pen)
        {
            int i = -1;

            do
            {
                i++;
                if (i == pointArray.Length)
                    return;
            } while (!IsPointValid(pointArray[i]));

            var xMid = ItemXDistance / 2;
            StreamGeometry geo = new StreamGeometry();

            using (var geoContext = geo.Open())
            {
                List<Point> PtList = new List<Point>();
                var x = PointSnapper.SnapValue(pointArray[i].X);
                var yMin = PointSnapper.SnapValue(pointArray[i].Y);
                var iMin = i;
                var yMax = yMin;
                var iMax = i;
                i++;
                while (i < pointArray.Length)
                {
                    if (IsPointValid(pointArray[i]))
                    {
                        var tempX = PointSnapper.SnapValue(pointArray[i].X);
                        if (tempX != x)
                        {
                            break;
                        }
                        else
                        {
                            var tempY = PointSnapper.SnapValue(pointArray[i].Y);
                            if (tempY < yMin)
                            {
                                yMin = tempY;
                                iMin = i;
                            }
                            else if (tempY > yMax)
                            {
                                yMax = tempY;
                                iMax = i;
                            }
                        }
                    }
                    i++;
                }

                if (iMin < iMax)
                {
                    var tempPt = new Point(x, yMin);
                    geoContext.BeginFigure(tempPt, false, false);
                    if (yMin != yMax)
                    {
                        tempPt = new Point(tempPt.X, yMax);
                        PtList.Add(tempPt);
                    }
                }
                else
                {
                    var tempPt = new Point(x, yMax);
                    geoContext.BeginFigure(tempPt, false, false);
                    if (yMin != yMax)
                    {
                        tempPt = new Point(tempPt.X, yMin);
                        PtList.Add(tempPt);
                    }
                }


                for (; i < pointArray.Length;)
                {
                    if (!IsPointValid(pointArray[i]))
                    {
                        i++;
                        continue;
                    }

                    x = PointSnapper.SnapValue(pointArray[i].X + xMid);
                    yMin = yMax = PointSnapper.SnapValue(pointArray[i].Y);

                    i++;

                    if (i != pointArray.Length)
                    {

                        while (i < pointArray.Length)
                        {
                            if (IsPointValid(pointArray[i]))
                            {
                                var tempX = PointSnapper.SnapValue(pointArray[i].X);
                                if (tempX != x)
                                {
                                    break;
                                }
                                else
                                {
                                    var tempY = PointSnapper.SnapValue(pointArray[i].Y);
                                    if (tempY < yMin)
                                    {
                                        yMin = tempY;
                                        iMin = i;
                                    }
                                    else if (tempY > yMax)
                                    {
                                        yMax = tempY;
                                        iMax = i;
                                    }
                                }
                            }
                            i++;
                        }
                    }

                    if (iMin < iMax)
                    {
                        var tempPt = new Point(x, yMin);
                        PtList.Add(tempPt);
                        if (yMin != yMax)
                        {
                            tempPt = new Point(x, yMax);
                            PtList.Add(tempPt);
                        }
                    }
                    else
                    {
                        var tempPt = new Point(x, yMax);
                        PtList.Add(tempPt);
                        if (yMin != yMax)
                        {
                            tempPt = new Point(x, yMin);
                            PtList.Add(tempPt);
                        }
                    }
                }

                geoContext.PolyLineTo(PtList, true, false);
            }

            geo.Freeze();
            dc.DrawGeometry(null, pen, geo);
        }

        /// <summary>
        /// 数据项间距大于等于1时，调用绘制。
        /// </summary>
        /// <param name="dc">绘制上下文。<see cref="IDrawingContext"/></param>
        /// <param name="pointArray">可见项坐标数组。</param>
        /// <param name="pen">画笔。<see cref="IPen"/></param>
        protected void DrawLooseChart(IDrawingContext dc, Point[] pointArray, IPen pen)
        {
            int i = -1;

            do
            {
                i++;
                if (i == pointArray.Length)
                    return;
            } while (!IsPointValid(pointArray[i]));

            var xMid = ItemXDistance / 2;

            StreamGeometry geo = new StreamGeometry();

            using (var geoContext = geo.Open())
            {
                List<Point> PtList = new List<Point>();
                var x = PointSnapper.SnapValue(pointArray[i].X);
                var y = PointSnapper.SnapValue(pointArray[i].Y);
                var tempPt = new Point(x, y);
                geoContext.BeginFigure(tempPt, false, false);

                i++;

                for (; i < pointArray.Length; i++)
                {
                    if (!IsPointValid(pointArray[i]))
                        continue;

                    x = PointSnapper.SnapValue(pointArray[i].X + xMid);
                    y = PointSnapper.SnapValue(pointArray[i].Y);
                    tempPt = new Point(x, y);
                    PtList.Add(tempPt);
                }

                geoContext.PolyLineTo(PtList, true, false);
            }

            geo.Freeze();
            dc.DrawGeometry(null, pen, geo);
        }
#endif
        /// <summary>
        /// 绘制数据集合图形。
        /// 如需绘制曲线以外样式的图形，可重载。
        /// </summary>
        /// <param name="dc">绘制上下文</param>
        public virtual void Draw(IDrawingContext dc)
        {
            if (points == null || !points.Any() || !IsItemsConnected)
            {
                return;
            }

            if (ItemXDistance + ItemXSpan >= 1)
            {
                DrawLooseChart(dc, points, linePen);
            }
            else
            {
                DrawDenseChart(dc, points, linePen);
            }
        }

        class PointComparer : IComparer<Point>
        {
            public static PointComparer Instance = new PointComparer();
            public int Compare(Point x, Point y)
            {
                return x.X.CompareTo(y.X);
            }
        }

        /// <summary>
        /// 依据坐标查找当前数据项。
        /// </summary>
        /// <param name="pt">坐标。</param>
        /// <returns>数据项包装。</returns>
        public virtual ChartItemWrap LocateCurrentChartItem(Point pt)
        {
            if (IsEmpty)
                return null;

            bool isChanged = false;

            if (masterCollection == null)
            {
                if (iStartPosition != -1)
                {
                    var iOldPosition = iCurrentPosition;

                    iCurrentPosition = Array.BinarySearch<Point>(points, pt, PointComparer.Instance);

                    isChanged = iOldPosition != iCurrentPosition;

                    if (isChanged)
                    {
                        if (iCurrentPosition < 0)
                        {
                            iCurrentPosition = Math.Max(0, ~iCurrentPosition - 1);

                            if (iCurrentPosition == points.Length)
                            {
                                iCurrentPosition = points.Length - 1;
                            }
                        }
                    }
                }
            }
            else
            {
                var iOldPosition = iCurrentPosition;

                iCurrentPosition = masterCollection.iCurrentPosition;

                isChanged = iOldPosition != iCurrentPosition;
            }
            ChartItemWrap currentItemWrap = null;
            if (iCurrentPosition != -1 && isChanged)
            {
                currentItemWrap = CreateChartItemWrap(iCurrentPosition);
            }

            return currentItemWrap;
        }

        /// <summary>
        /// 创建数据项包装。
        /// </summary>
        /// <param name="iPoint">数据项索引。</param>
        /// <returns>数据项包装</returns>
        protected virtual ChartItemWrap CreateChartItemWrap(int iPoint)
        {
            if (iStartPosition + iPoint < Items.Count)
            {
                return new ChartItemWrap()
                {
                    ChartItem = Items[iStartPosition + iPoint],
                    Point = GetPointCurrentItemAfterAdjust(iPoint)
                };
            }
            else
            {
                return null;
            }

        }

        /// <summary>
        /// 改变当前数据项。
        /// </summary>
        /// <param name="steps">向左向右单位移动的单位。</param>
        /// <returns></returns>
        public virtual ChartItemWrap LocateNearItem(int steps)
        {
            if (IsEmpty)
                return null;

            ChartItemWrap itemWp = null;

            if (masterCollection == null)
            {
                if (iStartPosition != -1)
                {
                    if (steps > 0)
                    {
                        iCurrentPosition = Math.Min(points.Length - 1, iCurrentPosition + steps);
                    }
                    else
                    {
                        iCurrentPosition = Math.Max(0, iCurrentPosition + steps);
                    }

                    Debug.WriteLine("iCurrentPosition=" + iCurrentPosition);
                }
            }
            else
            {
                iCurrentPosition = masterCollection.iCurrentPosition;

            }

            if (iCurrentPosition != -1)
            {
                itemWp = CreateChartItemWrap(iCurrentPosition);
            }
            return itemWp;
        }

        /// <summary>
        /// 取当前数据项包装。
        /// </summary>
        public ChartItemWrap CurrentItem
        {
            get
            {
                if (IsEmpty)
                    return null;

                if (masterCollection != null)
                {
                    iCurrentPosition = masterCollection.iCurrentPosition;
                }

                ChartItemWrap itemWp = null;
                if (iCurrentPosition != -1)
                {
                    itemWp = CreateChartItemWrap(iCurrentPosition);
                }
                return itemWp;
            }
        }

        private Point GetPointCurrentItemAfterAdjust(int iPoint)
        {
            Point point;
            if (iPoint != iMinPosition)
            {
                point = AdjustPoint(points[iPoint]);
            }
            else
            {
                point = AdjustPoint(points[iPoint], true);
            }

            return point;
        }

        /// <summary>
        /// 微调坐标位置。
        /// </summary>
        /// <param name="pt">坐标点。</param>
        /// <param name="isMinY">检查是否是最低点。</param>
        /// <returns>调整后的坐标。</returns>
        protected Point AdjustPoint(Point pt, bool isMinY = false)
        {
            var ptRet = new Point();

            var xMid = ItemXDistance / 2;

            ptRet.X = PointSnapper.SnapValue(pt.X + xMid);
            ptRet.Y = PointSnapper.SnapValue(pt.Y);

            if (isMinY)
            {
                if (ptRet.Y > collectRect.Bottom)
                {
                    ptRet.Y = (int)collectRect.Bottom - 0.5;
                }
            }

            return ptRet;
        }

        private int GetRealStepsAfterMove(int steps)
        {
            int realSteps = 0;

            int containsCount = GetContainsCount();

            int iNewStartPosition = iStartPosition + steps;
            if (steps < 0)
            {
                if (iNewStartPosition < 0)
                {
                    iNewStartPosition = 0;
                }
                realSteps = iNewStartPosition - iStartPosition;
                
                /*changes.ExitCount = Math.Max(0, points.Length - changes.RealSteps - containsCount);
                if (iMaxPosition > points.Length - changes.ExitCount)
                {
                    changes.Changes |= MaxMinChanges.Max;
                }
                if (iMinPosition > points.Length - changes.ExitCount)
                {
                    changes.Changes |= MaxMinChanges.Min;
                }*/
            }
            else if (steps > 0)
            {
                if (iNewStartPosition > Items.Count - containsCount)
                {
                    iNewStartPosition = Math.Max(iStartPosition, Items.Count - containsCount);
                }

                realSteps = iNewStartPosition - iStartPosition;
                /*changes.ExitCount = Math.Max(0, points.Length + changes.RealSteps - containsCount);

                if (iNewStartPosition > iMaxPosition + iStartPosition)
                {
                    changes.Changes |= MaxMinChanges.Max;
                }
                if (iNewStartPosition > iMinPosition + iStartPosition)
                {
                    changes.Changes |= MaxMinChanges.Min;
                }*/
            }
            else
            {
                Debug.Assert(false, "steps should not be zero");
            }

            return realSteps;
        }

        private const int QueryCount = 256;

        private QueryData CreateQueryDataFrom(int steps, int realSteps)
        {
            QueryData queryData = null;

            if (steps == realSteps)
                return null;
            else if (steps < 0)
            {
                if (!isHeadEnd)
                {
                    queryData = new QueryData()
                    {
                        CollectionId = collId
                    };
                    queryData.HeadDate = Items[0].Date;
                    queryData.HeadCount = Math.Max(QueryCount, steps - realSteps);
                }
            }
            else
            {
                if (!isTailEnd)
                {
                    queryData = new QueryData()
                    { CollectionId = collId };
                    queryData.TailDate = Items[Items.Count - 1].Date;
                    queryData.TailCount = Math.Max(QueryCount, steps - realSteps);
                }
            }

            return queryData;

        }

        /// <summary>
        /// 调整数据可见项集合。
        /// </summary>
        /// <param name="steps">正数向右调整，负数向左调整。</param>
        /// <param name="queryData">如果调整到现有数据集合的末端，请求额外数据。数据集合中的数据是动态请求的，参见构造函数参数isItemDynamic。</param>
        /// <returns></returns>
        public virtual ActionResult Move(int steps, out QueryData queryData)
        {
            queryData = null;

            if (IsEmpty)
                return ActionResult.NoChange;

            if (masterCollection == null)
            {
                if (steps == 0)
                    return ActionResult.NoChange;
                
                int realSteps = GetRealStepsAfterMove(steps);

                queryData = CreateQueryDataFrom(steps, realSteps);
                if (queryData != null)
                {
                    return ActionResult.QueryMore;
                }

                if (realSteps == 0)
                    return ActionResult.NoChange;

                iStartPosition += realSteps;

                var containsCount = GetContainsCount();

                originalPointLength = (points.Length + Math.Abs(realSteps) < containsCount) ? points.Length + Math.Abs(realSteps) : containsCount;
                RearrayPointCollection(originalPointLength);
            }
            else
            {
                iStartPosition = masterCollection.iStartPosition;
                RearrayPointCollection(masterCollection.points.Length);
            }
            return ActionResult.Succeeded;
        }

        /// <summary>
        /// 放大或者缩小可见数据集合。
        /// </summary>
        /// <param name="times">放大或者缩小倍速。</param>
        /// <param name="qData">如果调整到现有数据集合的末端，请求额外数据。数据集合中的数据是动态请求的，参见构造函数参数isItemDynamic。</param>
        /// <returns></returns>
        public virtual ActionResult Zoom(double times, out QueryData qData, bool autoAdjust)
        {
            qData = null;

            if (IsEmpty || FixedVisibleCount != -1)
                return ActionResult.NoChange;

            if (masterCollection == null)
            {
                var plusTimes = times > 1 ? 1.1 : 0.9;
                var oldItemXDistance = ItemXDistance;
                var oldItemXSpan = ItemXSpan;

                while (true)
                {
                    if (!CalculateItemWidth(times * (oldItemXDistance + oldItemXSpan)))
                    {
                        return ActionResult.NoChange;
                    }

                    if (oldItemXDistance == ItemXDistance && oldItemXSpan == ItemXSpan)
                    {
                        if (autoAdjust)
                        {
                            times = times * plusTimes;
                        }
                        else
                        {
                            return ActionResult.NoChange;
                        }

                    }
                    else
                    {
                        break;
                    }
                }

                int containsCount = GetContainsCount();

                var iNewStartPosition = iStartPosition + points.Length - containsCount;

                if (iNewStartPosition < 0)
                {
                    if (!isHeadEnd)
                    {
                        qData = new QueryData();
                        qData.CollectionId = collId;
                        qData.HeadDate = Items[0].Date;
                        qData.HeadCount = Math.Max(QueryCount, -iNewStartPosition);
                    }

                    iNewStartPosition = 0;
                }

                if (iNewStartPosition + containsCount > Items.Count)
                {
                    if (!isTailEnd)
                    {
                        if (qData == null)
                            qData = new QueryData();
                        qData.TailDate = Items[Items.Count - 1].Date;
                        qData.TailCount = Math.Max(QueryCount, iNewStartPosition + containsCount - Items.Count);
                    }

                    containsCount = Items.Count - iNewStartPosition;
                }

                if (qData != null)
                {
                    ItemXSpan = oldItemXSpan;
                    ItemXDistance = oldItemXDistance;
                    return ActionResult.QueryMore;
                }
                originalPointLength = containsCount;
                iStartPosition = iNewStartPosition;
                RearrayPointCollection(containsCount);
                iCurrentPosition = containsCount - 1;
            }
            else
            {
                ItemXDistance = masterCollection.ItemXDistance;
                ItemXSpan = masterCollection.ItemXSpan;
                iStartPosition = masterCollection.iStartPosition;
                RearrayPointCollection(masterCollection.points.Length);
                iCurrentPosition = masterCollection.iCurrentPosition;
            }

            return ActionResult.Succeeded;
        }

        /// <summary>
        /// 重新分配可见数据坐标数组。
        /// 如需分配更多数据，可重载此函数。
        /// </summary>
        /// <param name="length"></param>
        protected virtual void RearrayPointCollection(int length)
        {
            points = new Point[length];
        }

        /// <summary>
        /// 调整数据集合显示区域。
        /// </summary>
        /// <param name="newSize">新区域的大小</param>
        public virtual void Resize(Size newSize)
        {
            if (IsEmpty)
                return;

            if (masterCollection == null)
            {
                int newCount = CalculateItemWidthAndCount(newSize.Width, originalPointLength);

                if (points != null && points.Length != newCount)
                {
                    iStartPosition = iStartPosition - (newCount - points.Length);
                    if (iStartPosition < 0)
                    {
                        newCount += iStartPosition;
                        iStartPosition = 0;
                    }

                    RearrayPointCollection(newCount);
                }
            }
            else
            {
                ItemXDistance = masterCollection.ItemXDistance;
                ItemXSpan = masterCollection.ItemXSpan;
                if (points != null && masterCollection.points != null && points.Length != masterCollection.points.Length)
                {
                    RearrayPointCollection(masterCollection.points.Length);
                }
            }
        }

        /// <summary>
        /// 显示一定区域内的数据项列表。
        /// </summary>
        /// <param name="startPt">起始点。</param>
        /// <param name="endPt">终止点。</param>
        /// <returns>是否成功显示。</returns>
        public virtual bool ShowRegion(Point startPt, Point endPt)
        {
            if (IsEmpty || FixedVisibleCount != -1)
                return false;

            if (masterCollection == null)
            {
                int iStart = Array.BinarySearch<Point>(points, startPt, PointComparer.Instance);
                if (iStart < 0)
                {
                    iStart = ~iStart;
                }

                int iEnd = Array.BinarySearch<Point>(points, endPt, PointComparer.Instance);
                if (iEnd < 0)
                {
                    iEnd = ~iEnd;
                }

                if (iStart > iEnd)
                {
                    var iTemp = iStart;
                    iStart = iEnd;
                    iEnd = iTemp;
                }

                if (iEnd - iStart < 1)
                {
                    return false;
                }

                int count = iEnd - iStart;

                var iNewStartPosition = iStartPosition + iStart;

                var width = collectRect.Width;

                var newCount = CalculateItemWidthAndCount(width, count);
                if (newCount != originalPointLength)
                {
                    iStartPosition = iNewStartPosition;
                    originalPointLength = Math.Min(Items.Count - iStartPosition, newCount);

                    RearrayPointCollection(originalPointLength);
                }
                else
                {
                    return false;
                }
            }
            else
            {
                iStartPosition = masterCollection.iStartPosition;
                originalPointLength = masterCollection.originalPointLength;
                RearrayPointCollection(masterCollection.points.Length);
            }
            return true;
        }

        /// <summary>
        /// 取可见项列表中，一定区域内的数据项列表。
        /// </summary>
        /// <param name="startPt">起始点。</param>
        /// <param name="endPt">终止点。</param>
        /// <returns>数据项列表。</returns>
        public virtual IList<ChartItem> GetChartItems(Point startPt, Point endPt)
        {
            if (IsEmpty)
                return null;

            int iStart = Array.BinarySearch<Point>(points, startPt, PointComparer.Instance);
            if (iStart < 0)
            {
                iStart = ~iStart;
            }

            int iEnd = Array.BinarySearch<Point>(points, endPt, PointComparer.Instance);
            if (iEnd < 0)
            {
                iEnd = ~iEnd;
            }

            if (iStart > iEnd)
            {
                var iTemp = iStart;
                iStart = iEnd;
                iEnd = iTemp;
            }

            if (iEnd - iStart < 1)
            {
                return null;
            }

            int count = iEnd - iStart;
            return Items.GetRange(iStart + iStartPosition, count);
        }

        /// <summary>
        /// 坐标点是否可用。等于<see cref="valueNA"/>表示不可用。
        /// </summary>
        protected bool IsPointValid(Point pt)
        {
            return pt.X != valueNA && pt.Y != valueNA /*&& pt.Y >= collectRect.Top && pt.Y < collectRect.Bottom*/;
        }

        /// <summary>
        /// 判读数据项是否可用。等于空表示不可用。
        /// </summary>
        protected static bool IsItemValid(ChartItem item)
        {
            return item != null;
        }

        /// <summary>
        /// 判读值是否可用。等于<see cref="valueNA"/>表示不可用。
        /// </summary>
        protected static bool IsValueValid(double value)
        {
            return value != valueNA;
        }

        /// <summary>
        /// 设置本集合要辅助的集合。
        /// </summary>
        /// <param name="source">需要辅助数据集合。</param>
        /// <param name="isIndependent">本集合坐标计算是否独立，缺省值是false，表示不独立计算。</param>
        public void AssistTo(ChartItemCollection source, bool isIndependent = false)
        {
            if (!isIndependent)
                masterCollection = source;
            int j = 0;

            List<ChartItem> dataItems = new List<ChartItem>(source.Items.Count);

            for (int i = 0; i < source.Items.Count; i++)
            {
                var itemSource = source.Items[i];

                for (; j < Items.Count;)
                {
                    if (itemSource.Date == Items[j].Date)
                    {
                        dataItems.Add(Items[j]);
                        j++;
                        break;
                    }
                    else if (itemSource.Date < Items[j].Date)
                    {
                        dataItems.Add(null);
                        break;
                    }
                    else
                    {
                        j++;
                    }
                }

                if (j == Items.Count)
                {
                    j++;
                }
                else if (j > Items.Count)
                {
                    dataItems.Add(null);
                }
            }

            Items = dataItems;
        }

        /// <summary>
        /// y坐标转换数据值。
        /// </summary>
        /// <param name="y">y坐标。</param>
        /// <returns>数据值。</returns>
        public virtual double GetValueFromPosition(double y)
        {
            if (y < collectRect.Top)
            {
                y = collectRect.Top;
            }
            else if (y > collectRect.Bottom)
            {
                y = collectRect.Bottom;
            }

            var max = GetMaxValue();
            var min = GetMinValue();

            double v = max - (max - min) * (y - collectRect.Top) / collectRect.Height;
            return v;
        }

        class ScaleComparer : IComparer<Scale<double>>
        {
            public static ScaleComparer Instance = new ScaleComparer();
            public int Compare(Scale<double> x, Scale<double> y)
            {
                double diff = x.Value - y.Value;
                if (diff > 0.001)
                {
                    return 1;
                }
                else if (diff > -0.001)
                {
                    return 0;
                }
                else
                {
                    return -1;
                }
            }
        }

        /// <summary>
        /// 取第一个可见项的值。
        /// </summary>
        /// <returns>可见项的值<see cref="ChartItem.Value"/></returns>
        protected virtual double GetStartValue()
        {
            var startItem = Items[iStartPosition];
            if (startItem != null)
            {
                return Items[iStartPosition].Value;
            }
            else
            {
                return valueNA;
            }
        }

        /// <summary>
        /// 取百分比坐标单位值。
        /// </summary>
        protected virtual double GetYScaleDiffValue()
        {
            return GetStartValue();
        }

        /// <summary>
        /// 计算Y轴的刻度列表。
        /// </summary>
        /// <param name="coordinateType">坐标类型<see cref="CoordinateType"/></param>
        /// <returns>Y轴的刻度列表。</returns>
        public virtual IList<Scale<double>> GetYAxisScales(CoordinateType coordinateType)
        {
            int columnCount = YColumnCount;

            List<Scale<double>> scales = null;

            if (!IsEmpty)
            {
                var max = GetMaxValue();
                var min = GetMinValue();

                if (coordinateType == CoordinateType.Log10)
                {
                    max = Math.Pow(10, max);
                    min = Math.Pow(10, min);
                }
                else if (coordinateType == CoordinateType.Percentage)
                {
                    var start = GetStartValue();
                    max = (max - start) / start;
                    min = (min - start) / start;
                }

                if (columnCount != 0)
                {
                    var vDis = (max - min) / columnCount;

                    if (IsScalesOptimized)
                        scales = GetOptimizedYScales(vDis, max, min);
                    else
                    {
                        scales = new List<Scale<double>>();

                        var startValue = max;
                        do
                        {
                            double y = 0;
                            if (coordinateType == CoordinateType.Log10)
                            {
                                y = ItemYDistance * (Math.Log10(max) - Math.Log10(startValue)) + collectRect.Top;
                            }
                            else if (coordinateType == CoordinateType.Percentage)
                            {
                                y = ItemYDistance * ((max - startValue) * GetYScaleDiffValue()) + collectRect.Top;
                            }
                            else
                            {
                                y = ItemYDistance * (max - startValue) + collectRect.Top;
                            }

                            Scale<double> scale = new Scale<double>(y, startValue);
                            scales.Add(scale);
                            startValue -= vDis;
                        } while (startValue >= min);

                        var lastScale = scales.Last();
                        if (lastScale.Value != min)
                        {
                            var y = ItemYDistance * (max - min) + collectRect.Top;
                            Scale<double> scale = new Scale<double>(y, min);
                            scales.Add(scale);

                        }
                    }
                }
            }
            return scales;
        }

        private List<Scale<double>> GetOptimizedYScales(double vDis, double max, double min)
        {
            List<Scale<double>> scales = new List<Scale<double>>();

            if (vDis >= 1)
            {
                int iDisUnit = (int)Math.Pow(10, (int)Math.Log10(vDis));
                int iDis = (int)vDis / iDisUnit * iDisUnit;

                var startValue = (int)max / iDis * iDis;
                if (!IsValueEqual(max, startValue))
                {
                    Scale<double> scale = new Scale<double>(collectRect.Top, max);
                    scale.AssistValue = ChartItemCollection.valueNA;
                    scales.Add(scale);
                }
                do
                {
                    double y = 0;
                    if (coordinateType == CoordinateType.Log10)
                    {
                        y = ItemYDistance * (Math.Log10(max) - Math.Log10(startValue)) + collectRect.Top;
                    }
                    else if (coordinateType == CoordinateType.Percentage)
                    {
                        y = ItemYDistance * ((max - startValue) * GetYScaleDiffValue()) + collectRect.Top;
                    }
                    else
                    {
                        y = ItemYDistance * (max - startValue) + collectRect.Top;
                    }

                    Scale<double> scale = new Scale<double>(y, startValue);
                    scale.AssistValue = ChartItemCollection.valueNA;
                    scales.Add(scale);
                    startValue -= iDis;
                } while (startValue > min);

                if (!IsValueEqual(min, startValue))
                {
                    Scale<double> scale = new Scale<double>(collectRect.Bottom, min);
                    scale.AssistValue = ChartItemCollection.valueNA;
                    scales.Add(scale);
                }
            }
            else if (vDis < 1)
            {
                int iLogDis = (int)(-Math.Log10(vDis)) + 1;
                int iDisUnit = (int)Math.Pow(10, iLogDis);
                //int iDis1Ten = (iDis / 10);
                int iDis = (int)(vDis * iDisUnit);
                if (iDis == 0)
                    return scales;

                var minValue = min * iDisUnit;
                var startValue = (int)(max * iDisUnit);

                var realValue = (double)startValue / iDisUnit;
                if (!IsValueEqual(realValue, max))
                {
                    var scale = new Scale<double>(collectRect.Top, max);
                    scale.AssistValue = ChartItemCollection.valueNA;
                    scales.Add(scale);
                }
                do
                {
                    double valueDiff = max - realValue;
                    if (coordinateType == CoordinateType.Percentage)
                    {
                        valueDiff *= GetYScaleDiffValue();
                    }
                    Scale<double> scale = new Scale<double>(ItemYDistance * valueDiff + collectRect.Top, realValue);
                    scale.AssistValue = ChartItemCollection.valueNA;
                    scales.Add(scale);
                    startValue -= iDis;
                    realValue = (double)startValue / iDisUnit;

                } while (startValue >= minValue);

                if (!IsValueEqual(realValue, min))
                {
                    var scale = new Scale<double>(collectRect.Bottom, min);
                    scale.AssistValue = ChartItemCollection.valueNA;
                    scales.Add(scale);
                }
            }

            if (coordinateType == CoordinateType.Percentage)
            {
                for (int i = 0; i < scales.Count; i++)
                {
                    if (scales[i].Value > minDiff)
                    {
                        continue;
                    }
                    else if (scales[i].Value > -minDiff)
                    {
                        break;
                    }
                    else
                    {
                        double valueDiff = max * GetYScaleDiffValue();
                        Scale<double> scale = new Scale<double>(ItemYDistance * valueDiff + collectRect.Top, 0);
                        scale.AssistValue = ChartItemCollection.valueNA;
                        scales.Insert(i, scale);
                        break;
                    }
                }
            }

            return scales;
        }

        internal const double minDiff = 0.000001;
        protected bool IsValueEqual(double v, double t)
        {
            var diff = v - t;
            if (diff > 0)
            {
                return diff < minDiff;
            }
            else
            {
                return diff > -minDiff;
            }
        }

        /// <summary>
        /// 取得X轴的刻度列表。
        /// </summary>
        /// <returns></returns>
        public virtual IList<Scale<DateTime>> GetXAxisScales()
        {
            List<Scale<DateTime>> scales = new List<Scale<DateTime>>();
            if (!IsEmpty)
            {
                if (XColumnCount != 0)
                {
                    double startX = collectRect.Left;
                    var div = (collectRect.Width - startX) / XColumnCount;

                    scales.Add(new Scale<DateTime>(startX, Items[iStartPosition].Date));

                    bool isEnd = false;
                    for(int i = 0; i < XColumnCount; i ++)
                    {
                        startX += div;

                        var startPt = new Point(startX, 0);
    
                        int iStart = Array.BinarySearch<Point>(points, startPt, PointComparer.Instance);
                        if (iStart < 0)
                        {
                            iStart = ~iStart;
                        }

                        if (iStart == points.Length)
                        {
                            if (!isEnd)
                            {
                                scales.Add(new Scale<DateTime>(startX, Items[iStart + iStartPosition - 1].Date));
                                isEnd = true;
                            }
                            else
                            {
                                scales.Add(new Scale<DateTime>(startX, DateTime.MinValue));
                            }
                        }
                        else {
                            scales.Add(new Scale<DateTime>(startX, Items[iStart + iStartPosition].Date));
                        }

                    }
                }
            }

            return scales;
        }

        private int CalculateItemWidthAndCount(double width, int count)
        {
            double itemWidth;
            if (FixedVisibleCount != -1)
                itemWidth = (width - 1) / (FixedVisibleCount - 1);
            else if(count != 0)
                itemWidth = width / count;
            else
            {
                return 0;
            }

            CalculateItemWidth(itemWidth);

            return FixedVisibleCount != -1 ? count : (int)(width / (ItemXSpan + ItemXDistance));
        }

        /// <summary>
        /// 依据请求宽度，计算合理的数据项宽度。
        /// </summary>
        /// <param name="itemWidth">请求宽度。</param>
        /// <returns>合理的数据项宽度。</returns>
        protected bool CalculateItemWidth(double itemWidth)
        {
            if (FixedVisibleCount != -1)
            {
                ItemXDistance = Math.Min(itemWidth, 1);
                ItemXSpan = itemWidth - ItemXDistance;
                return true;
            }

            var itemXSpan = itemWidth / 5;
            double itemXDistance = 0;
            if ((int)itemXSpan == 0)
            {
                if (itemWidth <= 1.0)
                {
                    itemXDistance = itemWidth - itemXSpan;
                }
                else if (itemWidth < 4.0)
                {
                    itemXDistance = 1;
                    itemXSpan = (int)(itemWidth - itemXDistance);
                }
                else
                {
                    itemXDistance = 3;
                    itemXSpan = 1;
                }
            }
            else
            {
                itemXSpan = (int)itemXSpan;
                var itemXDis = (int)(itemWidth - itemXSpan);
                if ((itemXDis & 1) == 0)
                {
                    itemXDis -= 1;
                }
                itemXDistance = itemXDis;
            }

            if (itemXSpan + itemXDistance >= MinItemXDistance && itemXSpan + itemXDistance < MaxItemXDistance)
            {
                ItemXSpan = itemXSpan;
                ItemXDistance = itemXDistance;
                return true;
            }
            else
            {
                return false;
            }

        }

        /// <summary>
        /// 计算可见项数目。
        /// </summary>
        /// <returns>可见项数目</returns>
        protected int GetContainsCount()
        {
            if (FixedVisibleCount == -1)
                return (int)((collectRect.Width + ItemXSpan) / (ItemXDistance + ItemXSpan));
            else
            {
                return FixedVisibleCount;
            }
        }

        /// <summary>
        /// 请求结果项转换为数据项。
        /// </summary>
        /// <param name="queryItem">请求项。<see cref="QueryItem"/></param>
        /// <returns>数据结果项。<see cref="ChartItem"/></returns>
        public virtual ChartItem ConvertFrom(QueryItem queryItem)
        {
            ChartItem item = new ChartItem()
            {
                Date = queryItem.Date
            };

            if (queryItem.Close != null)
            {
                item.Value = queryItem.Close.Value;
            }

            return item;
        }

        ChartItemComparer comparer = new ChartItemComparer();

        /// <summary>
        /// 插入数据项集合。动态请求发出后，需要条用此函数插入请求结果。
        /// <see cref="QueryData"/>
        /// <see cref="Move(int, out QueryData)"/>
        /// <see cref="Zoom(double, out QueryData, bool)"/>
        /// </summary>
        /// <param name="items">数据项集合。</param>
        /// <param name="location">插入位置。</param>
        /// <param name="isEnd">是否还有更多数据可以请求。false表示，向左或者向右方向上已经到头。</param>
        public void AddChartItems(IList<ChartItem> items, AddLocation location, bool isEnd)
        {
            switch (location)
            {
                case AddLocation.Head:
                    if (items != null && items.Any())
                    {
                        var lastNewItem = items[items.Count - 1];
                        int iItem = Items.BinarySearch(lastNewItem, comparer);
                        if (iItem < 0)
                        {
                            iItem = ~iItem;
                        }
                        else
                        {
                            iItem++;
                        }

                        var newItems = new List<ChartItem>(items.Count + Count - iItem);

                        newItems.AddRange(items);
                        if (!IsEmpty)
                        {
                            newItems.AddRange(Items.GetRange(iItem, Items.Count - iItem));

                            UpdateConnectedItem(Items[iItem], items[items.Count - 1]);
                        }

                        Items = newItems;

                        UpdatePositions(items.Count - iItem);
                    }
                    isHeadEnd = isEnd;
                    break;
                case AddLocation.Tail:
                    if (items != null && items.Any())
                    {
                        if (IsEmpty)
                        {
                            Items = new List<ChartItem>();
                            Items.AddRange(items);
                        }
                        else
                        {
                            var firstNewItem = items[0];
                            int iItem = Items.BinarySearch(firstNewItem, comparer);
                            if (iItem < 0)
                            {
                                iItem = ~iItem;
                            }

                            if (iItem == Items.Count)
                            {
                                Items.AddRange(items);
                            }
                            else
                            {
                                var newItems = new List<ChartItem>(items.Count + Count - iItem);
                                newItems.AddRange(Items.GetRange(0, iItem));
                                newItems.AddRange(items);

                                UpdateConnectedItem(items[0], Items[iItem]);

                                Items = newItems;

                            }
                        }

                    }
                    isTailEnd = isEnd;
                    break;
                default:
                    throw new ArgumentException("Unknown location");
            }
        }

        /// <summary>
        /// 设置数据项中的变动项<see cref="ChartItem.ValueChange"/>。
        /// 如果需要计算更多变动项，可以重载。
        /// </summary>
        /// <param name="connectItem">数据项。</param>
        /// <param name="preItem">前一个数据项。</param>
        protected virtual void UpdateConnectedItem(ChartItem connectItem, ChartItem preItem)
        {
            connectItem.ValueChange = (connectItem.Value - preItem.Value) / preItem.Value;
        }

        private void UpdatePositions(int iCount)
        {
            if (iStartPosition != -1)
            {
                iStartPosition += iCount;
            }

            if (iCurrentPosition != -1)
            {
                iCurrentPosition += iCount;
                if(iCurrentPosition >= points.Length)
                {
                    iCurrentPosition = -1;
                }
            }
        }

        /// <summary>
        /// 数据集合是否为空。
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return Items == null || !Items.Any();
            }
        }

        /// <summary>
        /// 数据集合中数据项的数量。
        /// </summary>
        protected int Count
        {
            get
            {
                return Items != null ? Items.Count : 0;
            }
        }

        /// <summary>
        /// 根据坐标类型，转换数据项集合中所有数据。
        /// </summary>
        /// <param name="coordinateTypeTo">坐标类型<see cref="CoordinateType"/>。</param>
        public void TransferCoordinate(CoordinateType coordinateTypeTo)
        {
            if (Items != null)
            {
                foreach (var item in Items)
                {
                    if (IsItemValid(item))
                        AdjustItemValue(item, coordinateType, coordinateTypeTo);
                }
            }

            coordinateType = coordinateTypeTo;
        }

        /// <summary>
        /// 转换数据项的值，Log10(X)。
        /// </summary>
        /// <param name="item">数据项<see cref="ChartItem"/>。</param>
        protected virtual void ConvertChartItemLog10(ChartItem item)
        {
            item.Value = Math.Log10(item.Value);
        }

        /// <summary>
        /// 转换数据项的值，Pow10(X)。
        /// </summary>
        /// <param name="item">数据项<see cref="ChartItem"/>。</param>
        protected virtual void ConvertChartItemPow10(ChartItem item)
        {
            item.Value = Math.Pow(10, item.Value);
        }

        /// <summary>
        /// 根据坐标类型转换数据项的值。
        /// </summary>
        /// <param name="item">数据项<see cref="ChartItem"/>。</param>
        /// <param name="coordinateTypeFrom">源坐标类型<see cref="CoordinateType"/>。</param>
        /// <param name="coordinateTypeTo">目标坐标类型<see cref="CoordinateType"/>。</param>
        public void AdjustItemValue(ChartItem item, CoordinateType coordinateTypeFrom, CoordinateType coordinateTypeTo)
        {
            if (coordinateTypeFrom == coordinateTypeTo)
                return;

            if (coordinateTypeFrom == CoordinateType.Linear)
            {
                if (coordinateTypeTo == CoordinateType.Log10)
                {
                    ConvertChartItemLog10(item);
                }
                else if (coordinateTypeTo == CoordinateType.Percentage)
                { }
                else
                {
                    throw new NotSupportedException(string.Format("{0}=>{1}", coordinateTypeFrom, coordinateTypeTo));
                }
            }
            else if (coordinateTypeFrom == CoordinateType.Log10)
            {
                if (coordinateTypeTo == CoordinateType.Linear || coordinateTypeTo == CoordinateType.Percentage)
                {
                    ConvertChartItemPow10(item);
                }
                else
                {
                    throw new NotSupportedException(string.Format("{0}=>{1}", coordinateTypeFrom, coordinateTypeTo));
                }
            }
            else if (coordinateTypeFrom == CoordinateType.Percentage)
            {
                if (coordinateTypeTo == CoordinateType.Log10)
                {
                    ConvertChartItemLog10(item);
                }
                else if (coordinateTypeTo == CoordinateType.Linear)
                { }
                else
                {
                    throw new NotSupportedException(string.Format("{0}=>{1}", coordinateTypeFrom, coordinateTypeTo));
                }
            }
            else
            {
                throw new NotSupportedException(string.Format("{0}=>{1}", coordinateTypeFrom, coordinateTypeTo));
            }
        }

        /// <summary>
        /// 复制数据项并且根据坐标类型转换它的值。
        /// </summary>
        /// <param name="item">数据项<see cref="ChartItem"/>。</param>
        /// <param name="coordinateType">坐标类型<see cref="CoordinateType"/>。</param>
        /// <returns></returns>
        public virtual ChartItem CopyAndAdjustItemValue(ChartItem item, CoordinateType coordinateType)
        {
            if (item == null)
            {
                return item;
            }

            return new ChartItem()
            {
                Value = coordinateType == CoordinateType.Log10 ? Math.Pow(10, item.Value) : item.Value,
                ValueChange = item.ValueChange,
                Date = item.Date,
                ExtraData = item.ExtraData
            };

        }

        /// <summary>
        /// 坐标点转化为时间和值的组合。
        /// </summary>
        /// <param name="pt">坐标。</param>
        /// <returns>时间和值的组合。</returns>
        public ValuePoint ConvertFromPoint(Point pt)
        {
            ValuePoint vp = new ValuePoint();

            int iStart = Array.BinarySearch<Point>(points, pt, PointComparer.Instance);
            if (iStart < 0)
            {
                iStart = ~iStart;

                if (iStart == points.Count())
                {
                    iStart = points.Count() - 1;
                }
            }

            vp.Date = Items[iStartPosition + iStart].Date;
            vp.Deviation = (points[iStart].X - pt.X) / (ItemXDistance + ItemXSpan);

            var max = GetMaxValue();
            vp.Value = max - (pt.Y - collectRect.Top) / ItemYDistance;

            return vp;
        }

        /// <summary>
        /// 时间和值的组合转化为坐标。
        /// </summary>
        /// <param name="vp">时间和值。</param>
        /// <returns>坐标</returns>
        public Point ConvertFromValuePoint(ValuePoint vp)
        {
            Point pt = new Point(-1, -1);

            ChartItem item = new ChartItem()
            {
                Date = vp.Date
            };

            int iIndex = Items.BinarySearch(item, comparer);
            if (iIndex >= 0)
            {
                var idiff = iIndex - iStartPosition;
                pt.X = points[0].X + (idiff * (ItemXDistance + ItemXSpan)) - vp.Deviation * (ItemXDistance + ItemXSpan);
                var max = GetMaxValue();
                pt.Y = (max - vp.Value) * ItemYDistance + collectRect.Top;
            }
            return pt;
        }

        /// <summary>
        /// 复制画笔。
        /// </summary>
        /// <param name="pen">源。</param>
        /// <param name="lineCap">线头形状。</param>
        /// <returns></returns>
        public static IPen CopyPen(IPen pen, PenLineCap lineCap)
        {
            var pen1 = pen.Clone();

            pen1.StartLineCap = lineCap;
            pen1.EndLineCap = lineCap;

#if USINGCANVAS
#else
            var dPen = pen1 as DrawingPen;
            dPen.Freeze();
#endif
            return pen1;
        }

        /// <summary>
        /// 添加最新数据
        /// </summary>
        /// <param name="latestItem">数据项</param>
        /// <returns>是否添加成功。</returns>
        public virtual bool AddLatestChartItem(ChartItem latestItem)
        {
            Items.Add(latestItem);
            if (Items.Any())
            {
                if (iStartPosition != -1)
                {
                    if ((points.Length + 1) * (ItemXDistance + ItemXSpan) < collectRect.Width)
                    {
                        originalPointLength = points.Length + 1;
                        RearrayPointCollection(points.Length + 1);
                    }
                    else
                    {
                        iStartPosition++;
                    }
                    
                }
            }

            return true;
        }

        /// <summary>
        /// 更新最新数据。
        /// </summary>
        /// <param name="latestItem">数据项</param>
        /// <returns>是否更新成功。</returns>
        public virtual bool UpdateLatestChartItem(ChartItem latestItem)
        {
            if (Items.Any())
            {
                Items[Items.Count - 1] = latestItem;
                return true;
            }
            else
            {
                return false;
            }
            
        }
    }
}
