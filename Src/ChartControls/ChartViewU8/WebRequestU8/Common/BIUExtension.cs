
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace WebRequest.Common
{
    static class BIUExtension
    {
        public static bool IsNull(this object obj)
        {
            bool isTrue = obj == null;
            return isTrue;
        }

        public static bool IsNullOrEmpty(this string str)
        {
            bool isTrue = string.IsNullOrEmpty(str);
            return isTrue;
        }

        public static bool IsNullOrEmpty<T>(this IEnumerable<T> list)
        {
            bool isEmpty = list.IsNull() || list.Count() == 0;
            return isEmpty;
        }

        public static bool IsEmpty<T>(this IEnumerable<T> list)
        {
            bool isEmpty = list.Count() == 0;
            return isEmpty;
        }

        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> list)
        {
            if (list.IsNullOrEmpty())
            {
                throw new ArgumentNullException();
            }

            ObservableCollection<T> result = new ObservableCollection<T>();
            foreach (T item in list)
            {
                result.Add(item);
            }
            return result;
        }

        public static bool IsValidPassword(this string pwd)
        {
            return Regex.IsMatch(pwd, @"^[A-Za-z0-9!$#%]+$");
        }

        public static bool IsValidEmail(this string email)
        {
            return Regex.IsMatch(email, @"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*");
        }

        public static bool IsNullOrEmptyOrWhiteSpace(this string str)
        {
            bool isTrue = string.IsNullOrEmpty(str) || string.IsNullOrWhiteSpace(str);
            return isTrue;
        }

        public static long DateTimeToFileTimeUtc(this DateTime dt)
        {
            long ftUtc = dt.ToFileTimeUtc();
            return ftUtc;
        }

        static DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        /// <summary>
        /// How do you convert Unix epoch time into real time in C#? (Epoch beginning 1/1/1970)
        /// </summary>
        /// <param name="ftUtc"></param>
        /// <returns></returns>
        public static DateTime FromUnixTime(this long unixTime)
        {
            DateTime dt = epoch.AddMilliseconds(unixTime);
            return dt;
        }

        public static long GetMinUnixTime()
        {
            return 0;
        }

        public static long ToUnixTime(this DateTime date)
        {
            return Convert.ToInt64((date - epoch).TotalSeconds);
        }

        public static long ToUnixTimeMilliseconds(this DateTime date)
        {
            return Convert.ToInt64((date - epoch).TotalMilliseconds);
        }

        public static long GetStreamLen(this Stream stream)
        {
            long len = 0;
            if (stream.IsNull())
            {
                return len;
            }
            len = stream.Seek(0, SeekOrigin.End);
            stream.Seek(0, SeekOrigin.Begin);
            return len;
        }

        public static string ConvertStorgeUnit(this ulong byteLen)
        {
            string result = string.Empty;
            double resultValue = 0;
            const double unitSize = 1000.0;

            if ((resultValue = byteLen / Math.Pow(unitSize, 3)) >= 1)
            {
                resultValue = Math.Truncate(resultValue * unitSize) / unitSize;
                result = string.Format("{0} GB", resultValue);
            }
            else if ((resultValue = byteLen / Math.Pow(unitSize, 2)) >= 1)
            {
                resultValue = Math.Truncate(resultValue * unitSize) / unitSize;
                result = string.Format("{0:F2} MB", resultValue);

            }
            else if ((resultValue = byteLen / unitSize) >= 1)
            {
                resultValue = Math.Truncate(resultValue * unitSize) / unitSize;
                result = string.Format("{0:F2} KB", resultValue);
            }
            else
            {
                result = string.Format("{0} Byte(s)", byteLen);
            }

            return result;
        }
    }
}
