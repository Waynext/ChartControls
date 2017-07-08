using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DataSource.Errors
{
    class Error
    {
        public ErrorCode Code
        {
            get;
            set;
        }

        public string Message
        {
            get;
            set;
        }

        public string Description
        {
            get;
            set;
        }
    }

    enum ErrorCode
    {
        NoError = 0,
        InternalError
    }
}
