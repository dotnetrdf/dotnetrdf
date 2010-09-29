using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;

namespace Alexandria.Documents.Adaptors
{
    public class NTriplesAdaptor : RdfAdaptor
    {
        public NTriplesAdaptor()
            : base(new NTriplesParser(), new NTriplesWriter()) { }

    }
}
