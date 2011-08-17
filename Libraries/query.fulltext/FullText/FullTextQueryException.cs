using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Query.FullText
{
    public class FullTextQueryException
        : RdfQueryException
    {
        public FullTextQueryException(String message, Exception cause)
            : base(message, cause) { }

        public FullTextQueryException(String message)
            : base(message) { }
    }

    public class FullTextIndexException
        : RdfQueryException
    {
        public FullTextIndexException(String message, Exception cause)
            : base(message, cause) { }

        public FullTextIndexException(String message)
            : base(message) { }
    }
}
