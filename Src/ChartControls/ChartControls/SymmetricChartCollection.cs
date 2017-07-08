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
#if USINGCANVAS
using Windows.UI.Xaml.Media;
#else
using System.Windows.Media;
using System.Windows;
#endif
namespace ChartControls
{
    /// <summary>
    /// 时刻，时间段，交易数设置。
    /// </summary>
    public sealed class SymmetricCommonSettings
    {
        /// <summary>
        /// 中国整点时刻组
        /// </summary>
        public readonly static TimeSpan[] CNTimes = new TimeSpan[] { new TimeSpan(9, 30, 0), new TimeSpan(10, 30, 0), new TimeSpan(11, 30, 0), new TimeSpan(14, 0, 0), new TimeSpan(15, 0, 0) };
        /// <summary>
        /// 香港整点时刻组
        /// </summary>
        public readonly static TimeSpan[] HKTimes = new TimeSpan[] { new TimeSpan(9, 30, 0), new TimeSpan(10, 0, 0), new TimeSpan(11, 0, 0), new TimeSpan(12, 0, 0),
            new TimeSpan(14, 0, 0), new TimeSpan(15, 0, 0) , new TimeSpan(16, 0, 0) };
        /// <summary>
        /// 美国整点时刻组
        /// </summary>
        public readonly static TimeSpan[] USTimes = new TimeSpan[] { new TimeSpan(9, 30, 0), new TimeSpan(10, 0, 0), new TimeSpan(11, 0, 0), new TimeSpan(12, 0, 0),
            new TimeSpan(13, 0, 0), new TimeSpan(14, 0, 0), new TimeSpan(15, 0, 0) , new TimeSpan(16, 0, 0) };

        /// <summary>
        /// 中国时间段比例组
        /// </summary>
        public readonly static int[] CNRangeRatios = new int[] { 2, 2, 2, 2 };

        /// <summary>
        /// 香港时间段比例组
        /// </summary>
        public readonly static int[] HKRangeRatios = new int[] { 1, 2, 2, 2, 2, 2 };

        /// <summary>
        /// 美国时间段比例组
        /// </summary>
        public readonly static int[] USRangeRatios = new int[] { 1, 2, 2, 2, 2, 2, 2 };

        /// <summary>
        /// 中国总分钟数
        /// </summary>
        public const int CNTradingCount = 242;
        /// <summary>
        /// 香港总分钟数
        /// </summary>
        public const int HKTradingCount = 332;
        /// <summary>
        /// 美国总分钟数
        /// </summary>
        public const int USTradingCount = 391;

        public const int CNTradingCount2 = 241;

        /// <summary>
        /// 整点时刻组
        /// </summary>
        public TimeSpan[] Times
        {
            get;
            set;
        }

        /// <summary>
        /// 时间段比例组
        /// </summary>
        public int[] RangeRatios
        {
            get;
            set;
        }

        /// <summary>
        /// 总分钟数
        /// </summary>
        public int TradingCount;

        /// <summary>
        /// 中国常用设置
        /// </summary>
        public readonly static SymmetricCommonSettings CNSettings = new SymmetricCommonSettings()
        {
            Times = CNTimes,
            RangeRatios = CNRangeRatios,
            TradingCount = CNTradingCount
        };

        /// <summary>
        /// 中国常用设置
        /// </summary>
        public readonly static SymmetricCommonSettings CNSettings2 = new SymmetricCommonSettings()
        {
            Times = CNTimes,
            RangeRatios = CNRangeRatios,
            TradingCount = CNTradingCount2
        };

        /// <summary>
        /// 香港常用设置
        /// </summary>
        public readonly static SymmetricCommonSettings HKSettings = new SymmetricCommonSettings()
        {
            Times = HKTimes,
            RangeRatios = HKRangeRatios,
            TradingCount = HKTradingCount
        };

        /// <summary>
        /// 美国常用设置
        /// </summary>
        public readonly static SymmetricCommonSettings USSettings = new SymmetricCommonSettings()
        {
            Times = USTimes,
            RangeRatios = USRangeRatios,
            TradingCount = USTradingCount
        };
    }

    /// <summary>
    /// 分时图数据集合
    /// </summary>
    public class SymmetricChartItemCollection : ChartItemCollection
    {
        /// <summary>
        /// 构造函数。
        /// </summary>
        /// <param name="id">集合ID。</param>
        /// <param name="items">数据集合列表。</param>
        /// <param name="pen">画笔</param>
        /// <param name="fill">填充颜色，暂时不用</param>
        /// <param name="settings">时刻，时间段，交易数设置。</param>
        public SymmetricChartItemCollection(CollectionId id, IEnumerable<ChartItem> items, IPen pen, Brush fill, SymmetricCommonSettings settings)
            : base(id, items, pen, fill)
        {
            sValue = valueNA;
            FixedVisibleCount = settings.TradingCount;
            IsScalesOptimized = false;

            StartDate = DateTime.Now;

            Settings = settings;
        }

        private double sValue;
        /// <summary>
        /// 上一个交易日的收盘价。
        /// </summary>
        public double StartValue
        {
            get
            {
                return sValue;
            }
            set
            {
                sValue = value;
                InitMax = (sValue * 1.1) * 100;
                InitMax = (int)(InitMax) / 100.0;

            }
        }

        /// <summary>
        ///  开始时间。
        /// </summary>
        public DateTime StartDate
        {
            get;
            set;
        }

        /// <summary>
        /// 初始最大值。
        /// </summary>
        public double InitMax
        {
            get;
            set;
        }

        /// <summary>
        /// 时刻，时间段，交易数设置。 <see cref="ChartControls.SymmetricCommonSettings"/>
        /// </summary>
        public SymmetricCommonSettings Settings
        {
            get;
            set;
        }

        private double startValue
        {
            get
            {
                return StartValue != valueNA ? StartValue : Items[iStartPosition].Value;
            }
        }

        /// <summary>
        /// <see cref="ChartItemCollection.GetMaxValue"/>
        /// </summary>
        public override double GetMaxValue()
        {
            var start = startValue;
            double max, min;
            if (Items.Any())
            {
                max = Items[iStartPosition + iMaxPosition].Value;
                min = Items[iStartPosition + iMinPosition].Value;

                return Math.Max(max - start, start - min) + start;
            }
            else
            {
                if(sValue == valueNA)
                {
                    throw new NotSupportedException("StartValue is invalid");
                }

                max = InitMax;
                return max;
            }

            
        }

        /// <summary>
        /// <see cref="ChartItemCollection.GetMinValue"/>
        /// </summary>
        public override double GetMinValue()
        {

            var start = startValue;
            double max, min;
            if (Items.Any())
            {
                max = Items[iStartPosition + iMaxPosition].Value;
                min = Items[iStartPosition + iMinPosition].Value;
                return start - Math.Max(max - start, start - min);
            }
            else
            {
                if (sValue == valueNA)
                {
                    throw new NotSupportedException("StartValue is invalid");
                }

                min = sValue - (InitMax - sValue);
                return min;
            }

            
        }

        /// <summary>
        /// <see cref="ChartItemCollection.GetYAxisScales(CoordinateType)"/>
        /// </summary>
        public override IList<Scale<double>> GetYAxisScales(CoordinateType coordinateType)
        {
            if (coordinateType != CoordinateType.Linear)
                throw new ArgumentException("Only Linear coordiate is supproed");

            int columnCount = YColumnCount;

            List<Scale<double>> scales = null;

            var max = GetMaxValue();
            var min = GetMinValue();

            var vDis = (max - min) / columnCount;

            scales = new List<Scale<double>>();

            var valueTemp = max;
            do
            {
                double y = ItemYDistance * (max - valueTemp) + collectRect.Top;

                Scale<double> scale = new Scale<double>(y, valueTemp);
                scale.AssistValue = (valueTemp - startValue) / startValue;

                scales.Add(scale);
                valueTemp -= vDis;
            } while (valueTemp > min || IsValueEqual(valueTemp, min)); 
                
            return scales;
        }

        
        private DateTime[] dates;

        /// <summary>
        /// <see cref="ChartItemCollection.GetXAxisScales"/>
        /// </summary>
        /// <returns></returns>
        public override IList<Scale<DateTime>> GetXAxisScales()
        {
            var dis = ItemXDistance + ItemXSpan;

            var sumRangeRatio = Settings.RangeRatios.Sum();
            var countPerColumn = (FixedVisibleCount - 1) / sumRangeRatio;

            IList<Scale<DateTime>> scales = new List<Scale<DateTime>>(Settings.Times.Length);
            if (dates == null)
            {
                dates = Settings.Times.Select(time => new DateTime(StartDate.Year, StartDate.Month, StartDate.Day, time.Hours, time.Minutes, time.Seconds)).ToArray();
            }

            scales.Add(new Scale<DateTime>(1.5, dates[0]));
            int sum = 0;
            for (int i = 0; i < Settings.RangeRatios.Length; i++)
            {
                sum += Settings.RangeRatios[i];
                scales.Add(new Scale<DateTime>(dis * (countPerColumn * sum) + ItemXDistance + collectRect.Left, dates[i + 1]));
            }

            return scales;
        }
        /// <summary>
        /// <see cref="ChartItemCollection.CopyFromMaster"/>
        /// </summary>
        protected override void CopyFromMaster()
        {
            base.CopyFromMaster();

            var sColl = masterCollection as SymmetricChartItemCollection;
            if (sColl != null)
            {
                StartValue = sColl.StartValue;
                InitMax = sColl.InitMax;
                Settings = sColl.Settings;
                StartDate = sColl.StartDate;
            }
            else
            {
                var mColl = masterCollection as SymmetricMultipleChartItemCollection;
                StartValue = mColl.StartValue;
                InitMax = mColl.InitMax;
                Settings = mColl.Settings;
                StartDate = mColl.StartDate;
            }
        }

        public override bool AddLatestChartItem(ChartItem latestItem)
        {
            Items.Add(latestItem);
            iStartPosition = -1;

            return true;
        }
    }

    /// <summary>
    /// 分时图多数据集合。
    /// </summary>
    public class SymmetricMultipleChartItemCollection : MultipleChartItemCollection
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="id">集合ID。</param>
        /// <param name="items">数据列表。</param>
        /// <param name="pens">画笔数组。</param>
        /// <param name="settings">时刻，时间段，交易数设置。</param>
        public SymmetricMultipleChartItemCollection(CollectionId id, IEnumerable<MultipleChartItem> items, IPen[] pens, SymmetricCommonSettings settings)
            : base(id, items, pens)
        {
            sValue = valueNA;
            FixedVisibleCount = settings.TradingCount;
            IsScalesOptimized = false;

            Settings = settings;
        }

        private double sValue;
        /// <summary>
        /// 上一个交易日的收盘价。
        /// </summary>
        public double StartValue
        {
            get
            {
                return sValue;
            }
            set
            {
                sValue = value;
                InitMax = (int)((sValue * 1.1) * 100) / 100;

            }
        }

        /// <summary>
        ///  开始时间。
        /// </summary>
        public DateTime StartDate
        {
            get;
            set;
        }

        /// <summary>
        /// 初始最大值。
        /// </summary>
        public double InitMax
        {
            get;
            set;
        }

        /// <summary>
        /// 时刻，时间段，交易数设置。 <see cref="ChartControls.SymmetricCommonSettings"/>
        /// </summary>
        public SymmetricCommonSettings Settings
        {
            get;
            set;
        }

        private double startValue
        {
            get
            {
                return StartValue != valueNA ? StartValue : Items[iStartPosition].Value;
            }
        }

        /// <summary>
        /// <see cref="ChartItemCollection.GetMaxValue"/>
        /// </summary>
        public override double GetMaxValue()
        {
            var start = startValue;
            
            if (Items.Any())
            {
                double max, min;
                max = GetMaxValueRaw();
                min = GetMinValueRaw();

                return Math.Max(max - start, start - min) + start;
            }
            else
            {
                if (sValue == valueNA)
                {
                    throw new NotSupportedException("StartValue is invalid");
                }

                return InitMax;
            }

            
        }

        /// <summary>
        /// <see cref="ChartItemCollection.GetMinValue"/>
        /// </summary>
        public override double GetMinValue()
        {
            var start = startValue;
            
            if (Items.Any())
            {
                double max, min;
                min = GetMinValueRaw();
                max = GetMaxValueRaw();

                return Math.Max(max - start, start - min) + start;
            }
            else
            {
                if (sValue == valueNA)
                {
                    throw new NotSupportedException("StartValue is invalid");
                }
                return sValue - (InitMax - sValue);
            }

            
            
        }

        /// <summary>
        /// <see cref="ChartItemCollection.GetYAxisScales(CoordinateType)"/>
        /// </summary>
        public override IList<Scale<double>> GetYAxisScales(CoordinateType coordinateType)
        {
            if (coordinateType != CoordinateType.Linear)
                throw new ArgumentException("Only Linear coordiate is supproed");

            int columnCount = YColumnCount;

            List<Scale<double>> scales = null;

            if (!IsEmpty)
            {
                var max = GetMaxValue();
                var min = GetMinValue();

                var vDis = (max - min) / columnCount;

                scales = new List<Scale<double>>();

                var valueTemp = max;
                do
                {
                    double y = ItemYDistance * (max - valueTemp) + collectRect.Top;

                    Scale<double> scale = new Scale<double>(y, (valueTemp - startValue) / startValue);
                    scale.AssistValue = valueTemp;

                    scales.Add(scale);
                    valueTemp -= vDis;
                } while (valueTemp >= min);
            }
            return scales;
        }

        private DateTime[] dates;
        /// <summary>
        /// <see cref="ChartItemCollection.GetXAxisScales"/>
        /// </summary>
        public override IList<Scale<DateTime>> GetXAxisScales()
        {
            var dis = ItemXDistance + ItemXSpan;

            var sumRangeRatio = Settings.RangeRatios.Sum();
            var countPerColumn = (FixedVisibleCount - 1) / sumRangeRatio;

            IList<Scale<DateTime>> scales = new List<Scale<DateTime>>(Settings.Times.Length);
            if (dates == null)
            {
                dates = Settings.Times.Select(time => new DateTime(StartDate.Year, StartDate.Month, StartDate.Day, time.Hours, time.Minutes, time.Seconds)).ToArray();
            }

            scales.Add(new Scale<DateTime>(1.5, dates[0]));
            int sum = 0;
            for (int i = 0; i < Settings.RangeRatios.Length; i++)
            {
                sum += Settings.RangeRatios[i];
                scales.Add(new Scale<DateTime>(dis * (countPerColumn * sum) + ItemXDistance + collectRect.Left, dates[i + 1]));
            }

            return scales;
        }

        /// <summary>
        /// <see cref="ChartItemCollection.CopyFromMaster"/>
        /// </summary>
        protected override void CopyFromMaster()
        {
            base.CopyFromMaster();

            var sColl = masterCollection as SymmetricChartItemCollection;
            if (sColl != null)
            {
                StartValue = sColl.StartValue;
                InitMax = sColl.InitMax;
                Settings = sColl.Settings;
                StartDate = sColl.StartDate;
            }
            else
            {
                var mColl = masterCollection as SymmetricMultipleChartItemCollection;
                StartValue = mColl.StartValue;
                InitMax = mColl.InitMax;
                Settings = mColl.Settings;
                StartDate = mColl.StartDate;
            }
        }

        public override bool AddLatestChartItem(ChartItem latestItem)
        {
            Items.Add(latestItem);
            iStartPosition = -1;

            return true;
        }
    }

    /// <summary>
    /// 分时成交量数据集合。
    /// </summary>
    public class SymmetricVolumnItemCollection : VolumnItemCollection
    {
        /// <summary>
        /// 构造函数。
        /// </summary>
        /// <param name="id">集合ID。</param>
        /// <param name="items">数据集合列表。</param>
        /// <param name="penRaise">上升画笔。<see cref="DrawingObjectFactory.CreatePen(Brush, double)"></see></param>
        /// <param name="penFall">下降画笔。<see cref="DrawingObjectFactory.CreatePen(Brush, double)"></see></param>
        /// <param name="settings">时刻，时间段，交易数设置。</param>
        public SymmetricVolumnItemCollection(CollectionId id, IEnumerable<VolumnItem> items, IPen penRaise, IPen penFall, SymmetricCommonSettings settings)
            : base(id, items, penRaise, penFall, false)
        {
            FixedVisibleCount = settings.TradingCount;
            VolumnItemStyle = VolumnItemStyle.Slim;

            IsScalesOptimized = false;

            Settings = settings;
        }

        /// <summary>
        ///  开始时间。
        /// </summary>
        public DateTime StartDate
        {
            get;
            set;
        }

        /// <summary>
        /// 时刻，时间段，交易数设置。 <see cref="ChartControls.SymmetricCommonSettings"/>
        /// </summary>
        public SymmetricCommonSettings Settings
        {
            get;
            set;
        }

        private DateTime[] dates;
        /// <summary>
        /// <see cref="ChartItemCollection.GetXAxisScales"/>
        /// </summary>
        /// <returns></returns>
        public override IList<Scale<DateTime>> GetXAxisScales()
        {
            var dis = ItemXDistance + ItemXSpan;

            var sumRangeRatio = Settings.RangeRatios.Sum();
            var countPerColumn = (FixedVisibleCount - 1) / sumRangeRatio;

            IList<Scale<DateTime>> scales = new List<Scale<DateTime>>(Settings.Times.Length);
            if (dates == null)
            {
                dates = Settings.Times.Select(time => new DateTime(StartDate.Year, StartDate.Month, StartDate.Day, time.Hours, time.Minutes, time.Seconds)).ToArray();
            }

            scales.Add(new Scale<DateTime>(1.5, dates[0]));
            int sum = 0;
            for (int i = 0; i < Settings.RangeRatios.Length; i++)
            {
                sum += Settings.RangeRatios[i];
                scales.Add(new Scale<DateTime>(dis * (countPerColumn * sum) + ItemXDistance + collectRect.Left, dates[i+1]));
            }

            return scales;
        }

        /// <summary>
        /// <see cref="ChartItemCollection.CopyFromMaster"/>
        /// </summary>
        protected override void CopyFromMaster()
        {
            base.CopyFromMaster();

            var sColl = masterCollection as SymmetricVolumnItemCollection;
            if (sColl != null)
            {
                StartDate = sColl.StartDate;
                Settings = sColl.Settings;
            }
        }

        /// <summary>
        /// 添加最新数据
        /// </summary>
        /// <param name="latestItem">数据项</param>
        /// <returns>是否添加成功。</returns>
        public override bool AddLatestChartItem(ChartItem latestItem)
        {
            Items.Add(latestItem);
            iStartPosition = -1;

            return true;
        }
    }
}
