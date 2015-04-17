using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Query.Spin.SparqlUtil
{
    interface ISparqlSDPlugin
    {
        INode Resource { get; }

        IGraph SparqlSDContribution { get; }
    }
}
