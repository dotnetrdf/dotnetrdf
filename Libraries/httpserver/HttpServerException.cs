using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.Web
{
    public class HttpServerException : Exception
    {
        public HttpServerException(String message)
            : base(message) { }

        public HttpServerException(String message, Exception cause)
            : base(message, cause) { }
    }

    public class NoHandlerException : HttpServerException
    {
        public NoHandlerException(String message)
            : base(message) { }

        public NoHandlerException(String message, Exception cause)
            : base(message, cause) { }
    }
}
