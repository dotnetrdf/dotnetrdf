using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Linq
{
    public class LinqToRdfException : RdfException
    {
        public LinqToRdfException(String errorMsg)
            : base(errorMsg) { }

        public LinqToRdfException(String errorMsg, Exception cause)
            : base(errorMsg, cause) { }
    }
}
