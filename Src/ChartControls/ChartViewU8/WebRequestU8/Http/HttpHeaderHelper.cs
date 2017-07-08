using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
namespace WebRequest.Http
{
    public class HttpHeaderHelper
    {
        public const string cookieKey = "Cookie";
        private static readonly char[] splitor = ":".ToCharArray();
        private static readonly char[] splitor2 = "=".ToCharArray();
        static public KeyValuePair<string, string>? SplitHeader(string header)
        {
            var headerParts = header.Split(splitor, StringSplitOptions.RemoveEmptyEntries);
            if (headerParts.Length == 2)
            {
                return new KeyValuePair<string, string>(headerParts[0].Trim(), headerParts[1].Trim());
            }

            return null;
        }

        static public bool IsHeaderCookie(string header)
        {
            return header.TrimStart().StartsWith(cookieKey);
        }

        static public Cookie CreateCookie(string cookie)
        {
            var cookieParts = cookie.Split(splitor2, StringSplitOptions.RemoveEmptyEntries);

            if (cookieParts.Length == 2)
            {
                return new Cookie(cookieParts[0], cookieParts[1]);
            }

            return null;
        }
    }
}
