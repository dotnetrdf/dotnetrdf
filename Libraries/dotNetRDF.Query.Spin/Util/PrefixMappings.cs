using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Query.Spin.Util
{
    public class PrefixMapping : Dictionary<String, String>
    {
        public PrefixMapping()
            : base(StringComparer.Ordinal)
        {
        }
    }
}
