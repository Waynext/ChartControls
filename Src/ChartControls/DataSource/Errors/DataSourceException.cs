using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DataSource.Errors
{
    class DataSourceException : Exception
    {
        public HttpStatusCode ResponseStatus
        {
            get;
            private set;
        }

        public Error Error
        {
            get;
            private set;
        }

        public DataSourceException(Error e)
            : base(e.Message)
        {
            Error = e;
            GetStatusCode();
        }

        public DataSourceException(string message, Exception innerEx)
            : base(message, innerEx)
        {
            GetStatusCode();
        }

        private void GetStatusCode()
        {
        }

    }
}
