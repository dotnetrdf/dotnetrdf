using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Writing.Formatting
{
    public class CsvFormatter : DeliminatedLineFormatter
    {
        public CsvFormatter()
            : base("CSV", ',', '\\', null, null, null, '"', false) { }
    }
}
