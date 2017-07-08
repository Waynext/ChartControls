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

namespace ChartControls
{
    /// <summary>
    /// 请求后的操作。
    /// </summary>
    public enum ActionResult { Succeeded, QueryMore, NoChange };

    /// <summary>
    /// 请求数据。
    /// </summary>
    public class QueryData
    {
        private static int idSeed = 0;

        /// <summary>
        /// 构造函数。
        /// </summary>
        public QueryData()
        {
            QueryId = idSeed++;
        }
         
        /// <summary>
        /// 请求ID。
        /// </summary>
        public int QueryId
        {
            get;
            set;
        }

        /// <summary>
        /// 数据集合ID。
        /// </summary>
        public CollectionId CollectionId
        {
            get;
            set;
        }

        /// <summary>
        /// 头部时间。不需要，可为空。
        /// </summary>
        public DateTime? HeadDate
        {
            get;
            set;
        }

        /// <summary>
        /// 头部数据项请求数量。
        /// </summary>
        public int? HeadCount
        {
            get;
            set;
        }

        /// <summary>
        /// 尾部时间。不需要可为空。
        /// </summary>
        public DateTime? TailDate
        {
            get;
            set;
        }

        /// <summary>
        /// 尾部数据项请求数量。
        /// </summary>
        public int? TailCount
        {
            get;
            set;
        }
    }

    /// <summary>
    /// 请求结果数据项。
    /// </summary>
    public class QueryItem
    {
        public DateTime Date
        {
            get;
            set;
        }

        public double? High
        {
            get;
            set;
        }

        public double? Low
        {
            get;
            set;
        }

        public double? Open
        {
            get;
            set;
        }

        public double? Close
        {
            get;
            set;
        }

        public double? Volumn
        {
            get;
            set;
        }

        public double? Turnover
        {
            get;
            set;
        }

        public double? ExchangeRate
        {
            get;
            set;
        }

        public double[] Values
        {
            get;
            set;
        }
    }

    /// <summary>
    /// 请求结果。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class QueryDataResult<T>
    {
        /// <summary>
        /// 请求ID。
        /// </summary>
        public int QueryId
        {
            get;
            set;
        }

        /// <summary>
        /// 数据集合ID。
        /// </summary>
        public CollectionId CollectionId
        {
            get;
            set;
        }
        /// <summary>
        /// 请求是否成功。
        /// </summary>
        public bool IsSucceeded
        {
            get;
            set;
        }

        /// <summary>
        /// 头部是否有数据。
        /// </summary>
        public bool IsHeadIncluded
        {
            get;
            set;
        }

        /// <summary>
        /// 头部数据集合
        /// </summary>
        public List<T> HeadItems
        {
            get;
            set;
        }

        /// <summary>
        /// 头部是否还有数据。
        /// </summary>
        public bool? IsHeadEnd
        {
            get;
            set;
        }

        /// <summary>
        /// 尾部是否有数据。
        /// </summary>
        public bool IsTailIncluded
        {
            get;
            set;
        }

        /// <summary>
        /// 尾部数据集合
        /// </summary>
        public List<T> TailItems
        {
            get;
            set;
        }

        /// <summary>
        /// 尾部是否还有数据。
        /// </summary>
        public bool? IsTailEnd
        {
            get;
            set;
        }
    }
}
