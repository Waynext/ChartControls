using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebRequest.Common
{
    class RangeHelper
    {
        static readonly char[] splitor = new char[] {'-'};
        public long Start
        {
            get;
            set;
        }

        public long End
        {
            get;
            set;
        }

        public RangeHelper()
        {
        }

        public RangeHelper(long start, long end)
        {
            Start = start;
            End = end;
        }

        public override string ToString()
        {
            return string.Format("{0}{2}{1}", Start, End, splitor[0]);
        }

        public static RangeHelper Parse(string range)
        {
            RangeHelper rangeHelper = null;

            var rangePair = range.Trim().Split(splitor, StringSplitOptions.RemoveEmptyEntries);
            if (rangePair.Length == 2)
            {
                long s, e;
                if(long.TryParse(rangePair[0], out s))
                {
                    if (long.TryParse(rangePair[1], out e))
                    {
                        rangeHelper = new RangeHelper();
                        rangeHelper.Start = s;
                        rangeHelper.End = e;
                        
                    }
                }
                
            }

            return rangeHelper;
        }
    }

    class ContentRangeHelper
    {
        public RangeHelper Range
        {
            get;
            private set;
        }

        public long Length
        {
            get;
            set;
        }
        public ContentRangeHelper(RangeHelper rangeHelper, long length)
        {
            Range = rangeHelper;
            Length = length;
        }

        private const string dataType = "bytes";
        private const string starMark = "*";
        private static readonly char[] splitor = new char[]{'/'};
        public override string ToString()
        {
            return string.Format(dataType + " {0}{1}{2}",
                Range != null ? Range.ToString() : starMark, splitor[0], Length);
        }

        public static ContentRangeHelper Parse(string contentRange)
        {
            if (!string.IsNullOrEmpty(contentRange))
            {

                contentRange.Trim();
                if (contentRange.StartsWith(dataType))
                {
                    contentRange = contentRange.Substring(dataType.Length).Trim();
                    var ranges = contentRange.Split(splitor, StringSplitOptions.RemoveEmptyEntries);
                    if (ranges.Length == 2)
                    {
                        RangeHelper range = null;
                        long length = 0;
                        if (ranges[1] != starMark)
                        {
                            range = RangeHelper.Parse(ranges[0]);
                        }

                        if (long.TryParse(ranges[1], out length))
                        {
                            return new ContentRangeHelper(range, length);
                        }
                    }
                }
            }

            return null;
            
        }
    }
}
