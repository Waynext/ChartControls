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
using Windows.UI.Xaml.Media;
using Windows.Foundation;
#else
using System.Windows.Media;
#endif

namespace ChartControls
{
    /// <summary>
    /// 成交量图表样式。
    /// </summary>
    public enum VolumnItemStyle
    {
        /// <summary>
        /// 条。
        /// </summary>
        Fat,
        /// <summary>
        /// 竖线。
        /// </summary>
        Slim,
        /// <summary>
        /// 曲线。
        /// </summary>
        Linear
    }

    /// <summary>
    /// 成交量数据项。
    /// </summary>
    public class VolumnItem : ChartItem
    {
        /// <summary>
        /// 成交量。
        /// </summary>
        public double Volumn
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
        /// 成交额。
        /// </summary>
        public double Turnover
        {
            get;
            set;
        }

        /// <summary>
        /// 换手率。
        /// </summary>
        public double ExchangeRate
        {
            get;
            set;
        }

        /// <summary>
        /// 是否上升。
        /// </summary>
        public bool IsRaise
        {
            get;
            set;
        }

        public override string ToString()
        {
            return string.Format("{0}-V{1:N2}-T{2:N2}", Date.ToString(ConstStrings.DateTimeFormat), Volumn, Turnover);
        }
    }

    /// <summary>
    /// 成交量数据集合。
    /// </summary>
    public class VolumnItemCollection : ChartItemCollection
    {
        /// <summary>
        /// 构造函数。
        /// </summary>
        /// <param name="id">集合ID。</param>
        /// <param name="items">数据集合列表。</param>
        /// <param name="penRaise">上升画笔。<see cref="DrawingObjectFactory.CreatePen(Brush, double)"></see></param>
        /// <param name="penFall">下降画笔。<see cref="DrawingObjectFactory.CreatePen(Brush, double)"></see></param>
        /// <param name="isItemDynamic">是否动态加载数据集合中的数据项。缺省值是false，表示不动态加载。</param>
        public VolumnItemCollection(CollectionId id, IEnumerable<VolumnItem> items, IPen penRaise, IPen penFall, bool isItemDynamic = false)
            : base(id, items, penRaise, null, false, isItemDynamic)
        {
            if (penFall == null)
                throw new ArgumentNullException("penFall");

            FallPen = penFall;
        }

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
            }
        }

        /// <summary>
        /// 下跌线的画笔。
        /// </summary>
        public IPen FallPen
        {
            get;
            set;
        }

        private VolumnItemStyle volumnItemStyle = VolumnItemStyle.Fat;
        /// <summary>
        /// 成交量图表样式。
        /// </summary>
        public VolumnItemStyle VolumnItemStyle
        {
            get
            {
                return volumnItemStyle;
            }
            set
            {
                volumnItemStyle = value;
                IsItemsConnected = volumnItemStyle == VolumnItemStyle.Linear;
            }
        }

        /// <summary>
        /// <see cref="ChartItemCollection.Draw(IDrawingContext)"/>
        /// </summary>
        /// <param name="dc"></param>
        public override void Draw(IDrawingContext dc)
        {
            if (points == null || !points.Any())
            {
                return;
            }

            if (volumnItemStyle == VolumnItemStyle.Fat || volumnItemStyle == VolumnItemStyle.Slim)
            {
                DrawBar(dc);
            }
            else
            {
                base.Draw(dc);
            }

            
        }

        private void DrawBar(IDrawingContext dc)
        {
            int i = -1;

            do
            {
                i++;
            } while (!IsPointValid(points[i]));

            GeometryGroup groupRaise = new GeometryGroup(), groupFall = new GeometryGroup();

            Point p1 = new Point();
            Point p2 = new Point();
            List<Point> PtList = new List<Point>();

            double halfItemWidth = ItemXDistance / 2;
            for (; i < points.Length; i++)
            {
                if (!IsPointValid(points[i]))
                    continue;

                var x = points[i].X;
                var y = points[i].Y;

                GeometryGroup group = null;

                bool isRaise = (Items[iStartPosition + i] as VolumnItem).IsRaise;
                if (isRaise)
                {
                    group = groupRaise;
                }
                else
                {
                    group = groupFall;
                }

                if (ItemXDistance >= 3 && volumnItemStyle == VolumnItemStyle.Fat)
                {
                    p1.X = PointSnapper.SnapValue(x);
                    p1.Y = PointSnapper.SnapValue(y);
                    p2.X = PointSnapper.SnapValue(x + ItemXDistance) - 1;
                    p2.Y = collectRect.Bottom;

                    RectangleGeometry rect = new RectangleGeometry()
                    {
                        Rect = new Rect(p1, p2)
                    };
#if USINGCANVAS
#else
                        rect.Freeze();
#endif
                    group.Children.Add(rect);
                }
                else
                {
                    p1.X = p2.X = PointSnapper.SnapValue(x + halfItemWidth);
                    p1.Y = PointSnapper.SnapValue(y);
                    p2.Y = collectRect.Bottom;
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
            }

#if USINGCANVAS
#else
            (RaisePen as DrawingPen).Freeze();
            (FallPen as DrawingPen).Freeze();
            groupRaise.Freeze();
            groupFall.Freeze();
#endif
            
            dc.DrawGeometry(null, RaisePen, groupRaise);
            dc.DrawGeometry(FallPen.Brush, FallPen, groupFall);
        }

        /// <summary>
        /// <see cref="ChartItemCollection.GetMinValue"/>
        /// </summary>
        public override double GetMinValue()
        {
            return 0;
        }

        /// <summary>
        /// <see cref="ChartItemCollection.CopyAndAdjustItemValue(ChartItem, CoordinateType)"/>
        /// </summary>
        public override ChartItem CopyAndAdjustItemValue(ChartItem item, CoordinateType coordinateType)
        {
            if (item == null)
            {
                return item;
            }

            VolumnItem vItem = item as VolumnItem;

            return new VolumnItem()
            {
                Value = coordinateType == CoordinateType.Log10 ? Math.Pow(10, vItem.Value) : vItem.Value,
                Turnover = vItem.Turnover,
                ExchangeRate = vItem.ExchangeRate,
                IsRaise = vItem.IsRaise,
                Date = item.Date,
                ExtraData = item.ExtraData
            };
        }

        /// <summary>
        /// <see cref="ChartItemCollection.ConvertFrom(QueryItem)"/>
        /// </summary>
        public override ChartItem ConvertFrom(QueryItem queryItem)
        {
            VolumnItem item = new VolumnItem()
            {
                Date = queryItem.Date
            };

            if (queryItem.Volumn != null)
            {
                item.Volumn = queryItem.Volumn.Value;
            }

            if (queryItem.Turnover != null)
            {
                item.Turnover = queryItem.Turnover.Value;
            }

            if (queryItem.ExchangeRate != null)
            {
                item.ExchangeRate = queryItem.ExchangeRate.Value;
            }
            return item;
        }

        /// <summary>
        /// <see cref="ChartItemCollection.GetYAxisScales(CoordinateType)"/>
        /// </summary>
        public override IList<Scale<double>> GetYAxisScales(CoordinateType coordinateType)
        {
            int column = YColumnCount;

            List<Scale<double>> scales = new List<Scale<double>>();

            if (!IsEmpty)
            {
                var max = GetMaxValue();
                var min = GetMinValue();

                if (coordinateType == CoordinateType.Log10)
                {
                    max = Math.Pow(10, max);
                    min = Math.Pow(10, min);
                }

                Scale<double> scaleTop = new Scale<double>(collectRect.Top, max);
                scales.Add(scaleTop);

                Scale<double> scaleBottom = new Scale<double>(min, min);
                scales.Add(scaleBottom);
            }

            return scales;
        }
    }
}
