using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebRequest.Common
{
    public enum ReadyState
    {
        Succeeded,
        Failed,
        Cancelled
    }

    sealed class HttpConst
    {
        public const string HttpMethod_Get = "get";
        public const string HttpMethod_Post = "post";
        public const string HttpMethod_Put = "put";
        public const string HttpMethod_Delete = "delete";
        public const string HttpMethod_Patch = "patch";
    }
}
