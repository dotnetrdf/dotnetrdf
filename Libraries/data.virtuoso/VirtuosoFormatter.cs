using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Writing;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Storage
{
    /// <summary>
    /// Formatter for use in writing data to Virtuoso
    /// </summary>
    /// <remarks>
    /// Uses Virtuoso function calls for communicating BNodes back to Virtuoso
    /// </remarks>
    class VirtuosoFormatter
        : SparqlFormatter
    {
        /// <summary>
        /// Formats a Blank Node by using the <strong>bif:rdf_make_iid_of_qname()</strong> function
        /// </summary>
        /// <param name="b">Blank Node</param>
        /// <param name="segment">Triple Segment</param>
        /// <returns></returns>
        protected override string FormatBlankNode(IBlankNode b, TripleSegment? segment)
        {
            //Use the bif:rdf_make_iid_of_qname('nodeID://bnode') function
            return "`bif:rdf_make_iid_of_qname('nodeID://" + b.InternalID + "')`";
        }
    }
}
