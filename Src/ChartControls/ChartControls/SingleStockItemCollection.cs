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
    /// K线和值数据项
    /// </summary>
    public class StockValuesItem : StockItem
    {
        public StockValuesItem()
        {
        }

        public List<double> Values
        {
            get;
            set;
        }

        public List<double> ValueChanges
        {
            get;
            set;
        }

        public override string ToString()
        {
            return string.Format("{0}-H{1:F2},O{2:F2},L{3:F2},C{4:F2},{5:P2},{6}", Date.ToString(ConstStrings.DateTimeFormat),
                                 High, Open, Low, Close, CloseChange, string.Join("-", Values.Select(v => v.ToString("F2"))));
        }
    }

    /// <summary>
    /// K线和值数据集合
    /// </summary>
    public class StockValuesItemCollection : StockItemCollection
    {
        private int iValuePointLength = -1;
        private Point[][] valuePoints;

        public IPen[] Pens
        {
            get;
            private set;
        }

        public StockValuesItemCollection(CollectionId id, IEnumerable<StockValuesItem> items, IPen penRaise, IPen penFall, Brush fill, IPen[] pens, bool isItemDynamic = false)
            : base(id, items, penRaise, penFall, fill, isItemDynamic)
        {
            if (pens != null && pens.Length >= 1)
            {
                Pens = new IPen[pens.Length];

                for (int i = 0; i < pens.Length; i++)
                {
                    Pens[i] = CopyPen(pens[i], PenLineCap.Round);
                }
            }
        }

        private int iMaxPositionInner = -1;
        private int iMinPositionInner = -1;
        private void GetValuePointsLength()
        {
            if (iValuePointLength == -1)
            {
                var firstItem = (Items[0] as StockValuesItem);
                iValuePointLength = firstItem.Values.Count;
            }
        }

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

        protected override void FindMaxAndMinIndex()
        {
            if (coordinateType != CoordinateType.Percentage)
            {
                double max = double.MinValue, min = double.MaxValue;

                for (int i = iStartPosition; i < iStartPosition + points.Length; i++)
                {
                    StockValuesItem item = Items[i] as StockValuesItem;

                    if (!IsItemValid(Items[i]))
                        continue;

                    if (IsValueValid(item.Value))
                    {
                        if (item.High > max)
                        {
                            iMaxPosition = i - iStartPosition;
                            iMaxPositionInner = -1;
                            max = item.High;
                        }
                        if (item.Low < min)
                        {
                            iMinPosition = i - iStartPosition;
                            iMinPositionInner = -1;
                            min = item.Low;
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
                StockValuesItem item = Items[i] as StockValuesItem;

                if (!IsItemValid(Items[i]))
                    continue;

                if (IsValueValid(item.Value))
                {
                    if (item.High > max)
                    {
                        iMax = i - iStartPosition;
                        max = item.High;
                    }
                    if (item.Low < min)
                    {
                        iMin = i - iStartPosition;
                        min = item.Low;
                    }
                }


                for (int j = 0; j < item.Values.Count; j++)
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
            if (itemStart != null)
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

        private StockValuesItem GetStartItem(int j)
        {
            StockValuesItem resukltItem = null;
            if (j == -1)
            {
                for (int i = iStartPosition; i < iStartPosition + points.Length; i++)
                {
                    StockValuesItem item = Items[i] as StockValuesItem;

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
                    StockValuesItem item = Items[i] as StockValuesItem;

                    if (!IsItemValid(Items[i]))
                        continue;

                    if (IsValueValid(item.Values[j]))
                    {
                        resukltItem = item;
                        break;
                    }
                }
            }

            return resukltItem;
        }

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
                max = GetMaxValue4PercentageCoordiate();
                min = GetMinValue4PercentageCoordiate();
            }

            ItemYDistance = 0.0;
            ItemYDistance = height / (max - min);
        }

        private double GetMaxValue4PercentageCoordiate()
        {
            var ssItem = Items[iStartPosition + iMaxPosition] as StockValuesItem;

            var startItem = GetStartItem(iMaxPositionInner);
            if (iMaxPositionInner == -1)
            {
                return (ssItem.High - startItem.Value) / startItem.Value;
            }
            else
            {
                return (ssItem.Values[iMaxPositionInner] - startItem.Values[iMaxPositionInner]) / startItem.Values[iMaxPositionInner];
            }

        }

        private double GetMinValue4PercentageCoordiate()
        {
            var ssItem = Items[iStartPosition + iMinPosition] as StockValuesItem;

            var startItem = GetStartItem(iMinPositionInner);
            if (iMinPositionInner == -1)
            {
                return (ssItem.Low - startItem.Value) / startItem.Value;
            }
            else
            {
                return (ssItem.Values[iMinPositionInner] - startItem.Values[iMinPositionInner]) / startItem.Values[iMinPositionInner];
            }

        }

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
                    var itemTemp = Items[i + iStartPosition] as StockValuesItem;
                    if (IsItemValid(itemTemp))
                    {
                        if (IsValueValid(itemTemp.Value))
                        {
                            points[i].Y = ItemYDistance * (max - itemTemp.Value) + collectRect.Top;
                            verticalLines[i].YHigh = ItemYDistance * (max - itemTemp.High) + collectRect.Top;
                            verticalLines[i].YLow = ItemYDistance * (max - itemTemp.Low) + collectRect.Top;
                            verticalLines[i].YOpen = ItemYDistance * (max - itemTemp.Open) + collectRect.Top;
                        }
                        else
                        {
                            points[i].Y = verticalLines[i].YHigh = verticalLines[i].YLow = verticalLines[i].YOpen = valueNA;
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
                        points[i].Y = verticalLines[i].YHigh = verticalLines[i].YLow = verticalLines[i].YOpen = valueNA;

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
                max = GetMaxValue4PercentageCoordiate();
            }
            else
            {
                max = masterCollection.GetMaxValue();
            }

            GetValuePointsLength();

            double startValue = 0f;
            double[] startValues = new double[iValuePointLength];

            var startItem = GetStartItem(-1);
            if (startItem != null)
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
                var itemTemp = Items[i + iStartPosition] as StockValuesItem;
                if (IsItemValid(itemTemp))
                {
                    if (IsValueValid(itemTemp.Value))
                    {
                        points[i].Y = ItemYDistance * (max - (itemTemp.Value - startValue) / startValue) + collectRect.Top;
                        verticalLines[i].YHigh = ItemYDistance * (max - (itemTemp.High- startValue) / startValue) + collectRect.Top;
                        verticalLines[i].YLow = ItemYDistance * (max - (itemTemp.Low - startValue) / startValue) + collectRect.Top;
                        verticalLines[i].YOpen = ItemYDistance * (max - (itemTemp.Open- startValue) / startValue) + collectRect.Top;
                    }
                    else
                    {
                        points[i].Y = verticalLines[i].YHigh = verticalLines[i].YLow = verticalLines[i].YOpen = valueNA;
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

        protected override double GetStartValue()
        {
            var startItem = GetStartItem(-1);
            return startItem.Value;
        }

        protected override double GetYScaleDiffValue()
        {
            return 1;
        }

        public override double GetMaxValue()
        {
            if (IsEmpty)
                return 0f;

            var mItem = Items[iStartPosition + iMaxPosition] as StockValuesItem;

            if (iMaxPositionInner == -1)
            {
                return mItem.High;
            }
            else
            {
                if (coordinateType == CoordinateType.Percentage)
                {
                    var startItemInner = GetStartItem(iMaxPositionInner);
                    var startItem = GetStartItem(-1);

                    var change = (mItem.Values[iMaxPositionInner] - startItemInner.Values[iMaxPositionInner]) / startItemInner.Values[iMaxPositionInner];

                    return startItem.Value + startItem.Value * change;
                }
                else
                {
                    return mItem.Values[iMaxPositionInner];
                }
            }
        }

        public override double GetMinValue()
        {
            if (IsEmpty)
                return 0f;

            var mItem = Items[iStartPosition + iMinPosition] as StockValuesItem;

            if (iMinPositionInner == -1)
            {
                return mItem.Low;
            }
            else
            {
                if (coordinateType == CoordinateType.Percentage)
                {
                    var startItemInner = GetStartItem(iMinPositionInner);
                    var startItem = GetStartItem(-1);

                    var change = (mItem.Values[iMinPositionInner] - startItemInner.Values[iMinPositionInner]) / startItemInner.Values[iMinPositionInner];

                    return startItem.Value + startItem.Value * change;
                }
                else
                {
                    return mItem.Values[iMinPositionInner];
                }
            }
            

        }

        protected override void CopyFromMaster()
        {
            base.CopyFromMaster();

            GetValuePointsLength();

            for (int j = 0; j < iValuePointLength; j++)
            {
                valuePoints[j] = new Point[points.Length];
            }
        }

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
        public override void Draw(IDrawingContext dc)
        {
            if (points == null || !points.Any())
            {
                return;
            }

            base.Draw(dc);

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

        protected override void UpdateConnectedItem(ChartItem connectItem, ChartItem preItem)
        {
            var ssCItem = connectItem as StockValuesItem;
            var ssPItem = preItem as StockValuesItem;

            if (IsValueValid(ssCItem.Value))
                ssCItem.ValueChange = (connectItem.Value - ssPItem.Value) / ssPItem.Value;

            if (ssCItem.ValueChanges != null)
            {
                for (int j = 0; j < iValuePointLength; j++)
                {
                    ssCItem.ValueChanges[j] = (ssCItem.Values[j] - ssPItem.Values[j]) / ssPItem.Values[j];
                }
            }
        }

        protected override void ConvertChartItemLog10(ChartItem item)
        {
            StockValuesItem sItem = item as StockValuesItem;

            if (IsValueValid(sItem.Value))
            {
                sItem.Value = Math.Log10(sItem.Value);

                sItem.High = Math.Log10(sItem.High);
                sItem.Low = Math.Log10(sItem.Low);
                sItem.Open = Math.Log10(sItem.Open);
            }
            for (int j = 0; j < iValuePointLength; j++)
            {
                if (IsValueValid(sItem.Values[j]))
                    sItem.Values[j] = Math.Log10(sItem.Values[j]);
            }
        }

        protected override void ConvertChartItemPow10(ChartItem item)
        {
            StockValuesItem sItem = item as StockValuesItem;

            if (IsValueValid(sItem.Value))
            {
                sItem.Value = Math.Pow(10, sItem.Value);
                sItem.High = Math.Pow(10, sItem.High);
                sItem.Low = Math.Pow(10, sItem.Low);
                sItem.Open = Math.Pow(10, sItem.Open);
            }

            for (int j = 0; j < iValuePointLength; j++)
            {
                if (IsValueValid(sItem.Values[j]))
                    sItem.Values[j] = Math.Pow(10, sItem.Values[j]);
            }
        }

        public override ChartItem CopyAndAdjustItemValue(ChartItem item, CoordinateType coordinateType)
        {
            StockValuesItem sItem = item as StockValuesItem;

            if (sItem == null)
            {
                return sItem;
            }

            StockValuesItem copyItem = null;
            if ((coordinateType == CoordinateType.Log10 && IsValueValid(sItem.Value)))
            {
                copyItem = new StockValuesItem()
                {
                    Value = Math.Pow(10, sItem.Value),
                    High = Math.Pow(10, sItem.High),
                    Low = Math.Pow(10, sItem.Low),
                    Open = Math.Pow(10, sItem.Open),
                    CloseChange = sItem.CloseChange,
                    Date = sItem.Date,
                    ExtraData = sItem.ExtraData,
                    Values = new List<double>(iValuePointLength),
                    ValueChanges = sItem.ValueChanges != null ? new List<double>(iValuePointLength) : null

                };
            }
            else
            {
                copyItem = new StockValuesItem()
                {
                    Value = sItem.Value,
                    High = sItem.High,
                    Low = sItem.Low,
                    Open = sItem.Open,
                    CloseChange = sItem.CloseChange,
                    Date = sItem.Date,
                    ExtraData = sItem.ExtraData,
                    Values = new List<double>(iValuePointLength),
                    ValueChanges = sItem.ValueChanges != null ? new List<double>(iValuePointLength) : null

                };
            }

            for (int j = 0; j < iValuePointLength; j++)
            {
                copyItem.Values.Add((coordinateType == CoordinateType.Log10 && IsValueValid(sItem.Values[j])) ? Math.Pow(10, sItem.Values[j]) : sItem.Values[j]);
                if (sItem.ValueChanges != null)
                    copyItem.ValueChanges.Add(sItem.ValueChanges[j]);
            }

            return copyItem;
        }

        public override ChartItem ConvertFrom(QueryItem queryItem)
        {
            StockValuesItem item = new StockValuesItem()
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
    }
}
