using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.LinkedData.Kasabi
{
    public class KasabiException
        : RdfException
    {
        public KasabiException(String message, Exception cause)
            : base(message, cause) { }

        public KasabiException(String message)
            : base(message) { }
    }
}
