using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF;
using VDS.RDF.Writing.Formatting;

namespace VDS.Alexandria.Documents.Adaptors
{
    class NQuadsAdaptor : NTriplesAdaptor
    {
        public NQuadsAdaptor()
            : base(new NQuadsFormatter()) { }
    }
}
