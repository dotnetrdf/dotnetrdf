using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Query.Spin
{
    public class SpinException : RdfQueryException
    {
        public SpinException(String message)
            : base(message) { }

        public SpinException(String message, Exception cause)
            : base(message, cause) { }
    }
}
