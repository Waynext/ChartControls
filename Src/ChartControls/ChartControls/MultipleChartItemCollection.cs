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
#else
using System.Windows.Media;
#endif
namespace ChartControls
{
    /// <summary>
    /// 多值数据项。
    /// </summary>
    public class MultipleChartItem : ChartItem
    {
        /// <summary>
        /// 构造函数。
        /// </summary>
        public MultipleChartItem()
        {
        }

        /// <summary>
        /// 值列表。
        /// </summary>
        public List<double> Values
        {
            get;
            set;
        }

        /// <summary>
        /// 值变动百分比列表。
        /// </summary>
        public List<double> ValueChanges
        {
            get;
            set;
        }

        public override string ToString()
        {
            return string.Format("{0}|{1:F2}|{2:P2}|{3}|{4}", Date.ToString(ConstStrings.DateTimeFormat), Value, ValueChange,
                                    string.Join("-", Values.Select(v => v.ToString("F2"))), string.Join("-", ValueChanges.Select(v => v.ToString("P2"))));
        }
    }

    /// <summary>
    /// 多值数据项包装。
    /// </summary>
    public class MultipleChartItemWrap : ChartItemWrap
    {
        /// <summary>
        /// 坐标点数组。
        /// </summary>
        public Point[] Points
        {
            get;
            set;
        }
    }

    /// <summary>
    /// 多值数据项集合。
    /// </summary>
    public class MultipleChartItemCollection : ChartItemCollection
    {
        private int iValuePointLength = -1;
        private Point[][] valuePoints;

        /// <summary>
        /// 画笔数组，对应多值数据项的每个数据。
        /// </summary>
        public IPen[] Pens
        {
            get;
            private set;
        }

        /// <summary>
        /// 构造函数。
        /// </summary>
        /// <param name="id">ID。</param>
        /// <param name="items">多值数据项列表。<see cref="MultipleChartItem"/></param>
        /// <param name="pens">画笔数组。<see cref="IPen"/> <see cref="DrawingObjectFactory.CreatePen(Brush, double)"/></param>
        /// <param name="isItemConnected">数据集合中点之间的连接线是否需要绘制。缺省值是true，表示绘制。</param>
        /// <param name="isItemDynamic">是否动态加载数据集合中的数据项。缺省值是false，表示不动态加载。</param>
        public MultipleChartItemCollection(CollectionId id, IEnumerable<MultipleChartItem> items, IPen[] pens, bool isItemConnected = true, bool isItemDynamic = false)
            : base(id, items, (pens != null && pens.Length != 0 ? pens[0] : null), null, isItemConnected, isItemDynamic)
        {
            if (pens != null && pens.Length > 1)
            {
                Pens = new IPen[pens.Length - 1];

                for (int i = 1; i < pens.Length; i++)
                {
                    Pens[i - 1] = CopyPen(pens[i], PenLineCap.Round);
                }
            }
        }

        private int iMaxPositionInner = -1;
        private int iMinPositionInner = -1;
        private void GetValuePointsLength()
        {
            if (iValuePointLength == -1)
            {
                var firstItem = (Items[0] as MultipleChartItem);
                iValuePointLength = firstItem.Values.Count;
            }
        }

        /// <summary>
        /// <see cref="ChartItemCollection.CalculteCollectionPointsX"/>     
        /// </summary>
        protected override void CalculteCollectionPointsX()
        {
            if (IsEmpty)
                return;

            GetValuePointsLength();

            double xDis = 0;
            for (int i = 0; i < points.Length; i++)
            {
                points[i] = new Point();
                points[i].X = xDis + collectRect.Left;

                for (int j = 0; j < iValuePointLength; j++)
                {
                    valuePoints[j][i].X = xDis + collectRect.Left; 
                }

                xDis += ItemXDistance + ItemXSpan;
            }
        }

        /// <summary>
        /// <see cref="ChartItemCollection.FindMaxAndMinIndex"/>
        /// </summary>
        protected override void FindMaxAndMinIndex()
        {
            if (coordinateType != CoordinateType.Percentage)
            {
                double max = double.MinValue, min = double.MaxValue;

                for (int i = iStartPosition; i < iStartPosition + points.Length; i++)
                {
                    MultipleChartItem item = Items[i] as MultipleChartItem;

                    if (!IsItemValid(Items[i]))
                        continue;

                    if (IsValueValid(item.Value))
                    {
                        if (item.Value > max)
                        {
                            iMaxPosition = i - iStartPosition;
                            iMaxPositionInner = -1;
                            max = item.Value;
                        }
                        if (item.Value < min)
                        {
                            iMinPosition = i - iStartPosition;
                            iMinPositionInner = -1;
                            min = item.Value;
                        }
                    }


                    for (int j = 0; j < item.Values.Count; j++)
                    {
                        var value = item.Values[j];
                        if (IsValueValid(value))
                        {
                            if (value > max)
                            {
                                iMaxPosition = i - iStartPosition;
                                iMaxPositionInner = j;
                                max = value;
                            }
                            if (value < min)
                            {
                                iMinPosition = i - iStartPosition;
                                iMinPositionInner = j;
                                min = value;
                            }
                        }
                    }
                }
            }
            else
            {
                FindMaxMin4PercentageCoordiate();
            }
        }

        private void FindMaxMin4PercentageCoordiate()
        {
            int iMax = -1;
            double max = double.MinValue, min = double.MaxValue;
            int[] iMaxs = new int[iValuePointLength];
            for (int i = 0; i < iMaxs.Length; i++) { iMaxs[i] = -1; }
            double[] maxs = new double[iValuePointLength];
            for (int i = 0; i < maxs.Length; i++) { maxs[i] = double.MinValue; }

            int iMin = -1;
            int[] iMins = new int[iValuePointLength];
            for (int i = 0; i < iMins.Length; i++) { iMins[i] = -1; }
            double[] mins = new double[iValuePointLength];
            for (int i = 0; i < mins.Length; i++) { mins[i] = double.MaxValue; }

            for (int i = iStartPosition; i < iStartPosition + points.Length; i++)
            {
                MultipleChartItem item = Items[i] as MultipleChartItem;

                if (!IsItemValid(Items[i]))
                    continue;

                if (IsValueValid(item.Value))
                {
                    if (item.Value > max)
                    {
                        iMax = i - iStartPosition;
                        max = item.Value;
                    }
                    if (item.Value < min)
                    {
                        iMin = i - iStartPosition;
                        min = item.Value;
                    }
                }

                
                for (int j = 0; j < item.Values.Count; j ++)
                {
                    var value = item.Values[j];
                    if (IsValueValid(value))
                    {
                        if (value > maxs[j])
                        {
                            iMaxs[j] = i - iStartPosition;
                            maxs[j] = value;
                        }
                        if (value < mins[j])
                        {
                            iMins[j] = i - iStartPosition;
                            mins[j] = value;
                        }
                    }
                }
            }

            var itemStart = GetStartItem(-1);
            if(itemStart != null)
            {
                max = (max - itemStart.Value) / itemStart.Value;
                min = (min - itemStart.Value) / itemStart.Value;

                iMaxPosition = iMax;
                iMaxPositionInner = -1;
                iMinPosition = iMin;
                iMinPositionInner = -1;
            }

            for (int j = 0; j < iValuePointLength; j++)
            {
                itemStart = GetStartItem(j);

                var jMax = (maxs[j] - itemStart.Values[j]) / itemStart.Values[j];
                var jMin = (mins[j] - itemStart.Values[j]) / itemStart.Values[j];

                if (jMax > max)
                {
                    max = jMax;

                    iMaxPosition = iMaxs[j];
                    iMaxPositionInner = j;
                    
                }

                if (jMin < min)
                {
                    min = jMin;

                    iMinPosition = iMins[j];
                    iMinPositionInner = j;
                }
            }
        }

        /// <summary>
        /// <see cref="ChartItemCollection.CalculateYDistance"/>
        /// </summary>
        protected override void CalculateYDistance()
        {
            double height = collectRect.Height - 1;
            double max;
            double min;
            if (coordinateType != CoordinateType.Percentage)
            {
                max = GetMaxValue();
                min = GetMinValue();
            }
            else
            {
                max = GetValue4PercentageCoordiate(iMaxPosition, iMaxPositionInner);
                min = GetValue4PercentageCoordiate(iMinPosition, iMinPositionInner);
            }

            ItemYDistance = 0.0;
            ItemYDistance = height / (max - min);
        }

        private MultipleChartItem GetStartItem(int j)
        {
            MultipleChartItem resukltItem = null;
            if (j == -1)
            {
                for (int i = iStartPosition; i < iStartPosition + points.Length; i++)
                {
                    MultipleChartItem item = Items[i] as MultipleChartItem;

                    if (!IsItemValid(Items[i]))
                        continue;

                    if (IsValueValid(item.Value))
                    {
                        resukltItem = item;
                        break;
                    }
                }
            }
            else
            {
                for (int i = iStartPosition; i < iStartPosition + points.Length; i++)
                {
                    MultipleChartItem item = Items[i] as MultipleChartItem;

                    if (!IsItemValid(Items[i]))
                        continue;

                    if(IsValueValid(item.Values[j]))
                    {
                        resukltItem = item;
                        break;
                    }
                }
            }

            return resukltItem;
        }

        /// <summary>
        /// <see cref="ChartItemCollection.CalculateCollectionPoints"/>
        /// </summary>
        protected override void CalculateCollectionPoints()
        {
            if (coordinateType != CoordinateType.Percentage)
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

                GetValuePointsLength();
                for (int i = 0; i < points.Length; i++)
                {
                    var itemTemp = Items[i + iStartPosition] as MultipleChartItem;
                    if (IsItemValid(itemTemp))
                    {
                        if (IsValueValid(itemTemp.Value))
                            points[i].Y = ItemYDistance * (max - itemTemp.Value) + collectRect.Top;
                        else
                        {
                            points[i].Y = valueNA;
                        }

                        for (int j = 0; j < iValuePointLength; j++)
                        {
                            var value = itemTemp.Values[j];

                            if (IsValueValid(value))
                                valuePoints[j][i].Y = ItemYDistance * (max - value) + collectRect.Top;
                            else
                            {
                                valuePoints[j][i].Y = valueNA;
                            }
                        }
                    }
                    else
                    {
                        points[i].Y = valueNA;
                        for (int j = 0; j < iValuePointLength; j++)
                        {
                            valuePoints[j][i].Y = valueNA;
                        }
                    }
                }
            }
            else
            {
                CalculatePointYs4PercentageCoordiate();
            }
        }

        private void CalculatePointYs4PercentageCoordiate()
        {
            double max = 0;
            if (masterCollection == null)
            {
                max = GetValue4PercentageCoordiate(iMaxPosition, iMaxPositionInner);
            }
            else
            {
                max = masterCollection.GetMaxValue();
            }

            GetValuePointsLength();

            double startValue = 0f;
            double[] startValues = new double[iValuePointLength];

            var startItem = GetStartItem(-1);
            if(startItem != null)
            {
                startValue = startItem.Value;
            }

            for (int j = 0; j < iValuePointLength; j++)
            {
                startItem = GetStartItem(j);
                if (startItem != null)
                {
                    startValues[j] = startItem.Values[j];
                }
            }

            for (int i = 0; i < points.Length; i++)
            {
                var itemTemp = Items[i + iStartPosition] as MultipleChartItem;
                if (IsItemValid(itemTemp))
                {
                    if (IsValueValid(itemTemp.Value))
                    {
                        points[i].Y = ItemYDistance * (max - (itemTemp.Value - startValue) / startValue) + collectRect.Top;
                    }
                    else
                    {
                        points[i].Y = valueNA;
                    }

                    for (int j = 0; j < iValuePointLength; j++)
                    {
                        var value = itemTemp.Values[j];

                        if (IsValueValid(value))
                            valuePoints[j][i].Y = ItemYDistance * (max - (value - startValues[j]) / startValues[j]) + collectRect.Top;
                        else
                        {
                            valuePoints[j][i].Y = valueNA;
                        }
                    }
                }
                else
                {
                    points[i].Y = valueNA;
                    for (int j = 0; j < iValuePointLength; j++)
                    {
                        valuePoints[j][i].Y = valueNA;
                    }
                }
            }
        }

        /// <summary>
        /// <see cref="ChartItemCollection.GetStartValue"/>
        /// </summary>
        protected override double GetStartValue()
        {
            var startItem = GetStartItem(-1);

            return startItem.Value;
        }


        /// <summary>
        /// 获取最大值。
        /// </summary>
        protected double GetMaxValueRaw()
        {
            double max = 0f;
            var mItem = Items[iStartPosition + iMaxPosition] as MultipleChartItem;

            if (iMaxPositionInner == -1)
            {
                max = mItem.Value;
            }
            else
            {
                if (coordinateType == CoordinateType.Percentage)
                {
                    var startItemInner = GetStartItem(iMaxPositionInner);
                    var startItem = GetStartItem(-1);

                    var change = (mItem.Values[iMaxPositionInner] - startItemInner.Values[iMaxPositionInner]) / startItemInner.Values[iMaxPositionInner];

                    max = startItem.Value + startItem.Value * change;
                }
                else
                {
                    max = mItem.Values[iMaxPositionInner];
                }
            }

            return max;
        }

        /// <summary>
        /// <see cref="ChartItemCollection.GetMaxValue"/>
        /// </summary>
        public override double GetMaxValue()
        {
            if (IsEmpty)
                return 0f;

            var max = GetMaxValueRaw();
            return max;
        }

        /// <summary>
        /// 获取最小值。
        /// </summary>
        protected double GetMinValueRaw()
        {
            var mItem = Items[iStartPosition + iMinPosition] as MultipleChartItem;
            double min = 0f;
            if (iMinPositionInner == -1)
            {
                min = mItem.Value;
            }
            else
            {
                if (coordinateType == CoordinateType.Percentage)
                {
                    var startItemInner = GetStartItem(iMinPositionInner);
                    var startItem = GetStartItem(-1);

                    var change = (mItem.Values[iMinPositionInner] - startItemInner.Values[iMinPositionInner]) / startItemInner.Values[iMinPositionInner];

                    min = startItem.Value + startItem.Value * change;
                }
                else
                {
                    min = mItem.Values[iMinPositionInner];
                }
            }

            return min;
        }

        /// <summary>
        /// <see cref="ChartItemCollection.GetMinValue"/>
        /// </summary>
        public override double GetMinValue()
        {
            if (IsEmpty)
                return 0f;

            var min = GetMinValueRaw();
            return min;
        }

        private double GetValue4PercentageCoordiate(int index, int indexInner)
        {
            var mItem = Items[iStartPosition + index] as MultipleChartItem;

            var sItem = GetStartItem(indexInner);
            if (indexInner == -1)
            {
                return (mItem.Value - sItem.Value) / sItem.Value;
            }
            else
            {
                return (mItem.Values[indexInner] - sItem.Values[indexInner]) / sItem.Values[indexInner];
            }

        }

        /// <summary>
        /// <see cref="ChartItemCollection.CopyFromMaster"/>
        /// </summary>
        protected override void CopyFromMaster()
        {
            base.CopyFromMaster();

            GetValuePointsLength();

            for (int j = 0; j < iValuePointLength; j++)
            {
                valuePoints[j] = new Point[points.Length];
            }
        }

        /// <summary>
        /// <see cref="ChartItemCollection.RearrayPointCollection(int)"/>
        /// </summary>
        /// <param name="length"></param>
        protected override void RearrayPointCollection(int length)
        {
            GetValuePointsLength();

            base.RearrayPointCollection(length);

            if (valuePoints == null)
            {
                valuePoints = new Point[iValuePointLength][];
            }

            for (int j = 0; j < iValuePointLength; j++)
            {
                valuePoints[j] = new Point[length];
            }
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

            DrawLines(dc, points, Pen);

            for (int j = 0; j < iValuePointLength; j++)
            {
                DrawLines(dc, valuePoints[j], Pens[j]);
            }
            
        }

        private void DrawLines(IDrawingContext dc, Point[] pointArrays, IPen pen)
        {
            if (ItemXDistance + ItemXSpan >= 1)
            {
                DrawLooseChart(dc, pointArrays, pen);
            }
            else
            {
                DrawDenseChart(dc, pointArrays, pen);
            }
        }

        /// <summary>
        /// <see cref="ChartItemCollection.CreateChartItemWrap(int)"/>
        /// </summary>
        protected override ChartItemWrap CreateChartItemWrap(int iPoint)
        {
            return new MultipleChartItemWrap()
            {
                ChartItem = Items[iStartPosition + iPoint],
                Point = GetPointCurrentItemAfterAdjust(iPoint),
                Points = GetPointsCurrentItemAfterAdjust(iPoint)
            };
        }

        private bool IsPointYValid(Point pt)
        {
            return pt.Y != valueNA;
        }

        private Point GetPoint(Point pt, bool isMin = false)
        {
            Point point;
            if (IsPointYValid(pt))
            {
                point = AdjustPoint(pt, isMin);
            }
            else
            {
                point = pt;
            }

            return point;
        }

        private Point GetPointCurrentItemAfterAdjust(int iPoint)
        {
            Point point;
            if (iPoint != iMinPosition || iMinPositionInner != -1)
            {
                point = GetPoint(points[iPoint]);
            }
            else
            {
                point = GetPoint(points[iPoint], true);
            }

            return point;
        }

        private Point[] GetPointsCurrentItemAfterAdjust(int iPoint)
        {
            Point[] pts = new Point[iValuePointLength];

            for (int j = 0; j < iValuePointLength; j++)
            {
                if (iPoint != iMinPosition || iMinPositionInner != iPoint)
                {
                    pts[j] = GetPoint(valuePoints[j][iPoint]);
                }
                else
                {
                    pts[j] = GetPoint(valuePoints[j][iPoint], true);
                }
            }
            return pts;
        }

        /// <summary>
        /// <see cref="ChartItemCollection.UpdateConnectedItem(ChartItem, ChartItem)"/>
        /// </summary>
        protected override void UpdateConnectedItem(ChartItem connectItem, ChartItem preItem)
        {
            var multiCItem = connectItem as MultipleChartItem;
            var multiPItem = preItem as MultipleChartItem;

            if(IsValueValid(multiCItem.Value))
                multiCItem.ValueChange = (connectItem.Value - multiPItem.Value) / multiPItem.Value;

            if (multiCItem.ValueChanges != null)
            {
                for (int j = 0; j < iValuePointLength; j++)
                {
                    multiCItem.ValueChanges[j] = (multiCItem.Values[j] - multiPItem.Values[j]) / multiPItem.Values[j];
                }
            }
        }

        /// <summary>
        /// <see cref="ChartItemCollection.ConvertChartItemLog10(ChartItem)"/>
        /// </summary>
        protected override void ConvertChartItemLog10(ChartItem item)
        {
            MultipleChartItem mItem = item as MultipleChartItem;
            if (IsValueValid(mItem.Value))
                mItem.Value = Math.Log10(mItem.Value);
            for (int j = 0; j < iValuePointLength; j++)
            {
                if (IsValueValid(mItem.Values[j]))
                    mItem.Values[j] = Math.Log10(mItem.Values[j]);
            }
        }
        /// <summary>
        /// <see cref="ChartItemCollection.ConvertChartItemPow10(ChartItem)"/>
        /// </summary>
        protected override void ConvertChartItemPow10(ChartItem item)
        {
            MultipleChartItem mItem = item as MultipleChartItem;

            if (IsValueValid(mItem.Value))
                mItem.Value = Math.Pow(10, mItem.Value);

            for (int j = 0; j < iValuePointLength; j++)
            {
                if (IsValueValid(mItem.Values[j]))
                    mItem.Values[j] = Math.Pow(10, mItem.Values[j]);
            }
        }

        /// <summary>
        /// <see cref="ChartItemCollection.CopyAndAdjustItemValue(ChartItem, CoordinateType)"/>
        /// </summary>
        public override ChartItem CopyAndAdjustItemValue(ChartItem item, CoordinateType coordinateType)
        {
            MultipleChartItem mItem = item as MultipleChartItem;

            if (mItem == null)
            {
                return mItem;
            }

            var copyItem = new MultipleChartItem()
            {
                Value = (coordinateType == CoordinateType.Log10 && IsValueValid(mItem.Value)) ? Math.Pow(10, mItem.Value) : mItem.Value,
                ValueChange = mItem.ValueChange,
                Date = mItem.Date,
                ExtraData = mItem.ExtraData,
                Values = new List<double>(iValuePointLength),
                ValueChanges = mItem.ValueChanges != null ? new List<double>(iValuePointLength) : null

            };

            for (int j = 0; j < iValuePointLength; j++)
            {
                copyItem.Values.Add((coordinateType == CoordinateType.Log10 && IsValueValid(mItem.Values[j])) ? Math.Pow(10, mItem.Values[j]) : mItem.Values[j]);
                if(mItem.ValueChanges != null)
                    copyItem.ValueChanges.Add(mItem.ValueChanges[j]);
            }

            return copyItem;
        }

        /// <summary>
        /// <see cref="ChartItemCollection.ConvertFrom(QueryItem)"/>
        /// </summary>
        public override ChartItem ConvertFrom(QueryItem queryItem)
        {
            MultipleChartItem item = new MultipleChartItem()
            {
                Date = queryItem.Date
            };

            if (queryItem.Close != null)
            {
                item.Value = queryItem.Close.Value;
            }

            if (queryItem.Values != null)
            {
                item.Values = new List<double>(queryItem.Values.Length);
                item.Values.AddRange(queryItem.Values);
            }

            return item;
        }

        /// <summary>
        /// <see cref="ChartItemCollection.GetYScaleDiffValue"/>
        /// </summary>
        protected override double GetYScaleDiffValue()
        {
            return 1;
        }
    }

    
}
