using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Writing.Formatting
{
    public class TsvFormatter : DeliminatedLineFormatter
    {
        public TsvFormatter()
            : base("TSV", '\t', '\\', '<', '>', '"', '"', null, true) { }
    }
}
