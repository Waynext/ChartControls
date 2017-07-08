using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ULog;

namespace WebRequest.Common
{
    class DiagnoseHelper
    {
        public static void CheckReference(Object reference, string errorMsg)
        {
            if (reference == null)
            {
                LogHelper.OnlineLogger.Error(errorMsg);
                throw new NullReferenceException(errorMsg);
            }
        }

        public static void CheckArgument(Object arg, string errorMsg)
        {
            if (arg == null)
            {
                string errMsg = errorMsg + " can not be null";
                LogHelper.OnlineLogger.Error(errMsg);
                throw new ArgumentException(errMsg);
            }
        }

        public static void CheckStringIgnoreCase(string content, string target, string errorMsg)
        {
            if (!content.Equals(target, StringComparison.OrdinalIgnoreCase))
            {
                LogHelper.OnlineLogger.Error(errorMsg);
                throw new InvalidOperationException(errorMsg);
            }
        }

        public static void CheckString(String text, string errorMsg)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrWhiteSpace(text))
            {
                LogHelper.OnlineLogger.Error(errorMsg);
                throw new InvalidOperationException(errorMsg);
            }
        }

        public static void CheckCondition(bool isTrue, string errorMsg)
        {
            if (!isTrue)
            {
                LogHelper.OnlineLogger.Error(errorMsg);
                throw new InvalidOperationException(errorMsg);
            }
        }
    }
}
