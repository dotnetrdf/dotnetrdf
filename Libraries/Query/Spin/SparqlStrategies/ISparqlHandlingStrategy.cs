using VDS.RDF.Query.Spin.Core;
using VDS.RDF.Query.Spin.SparqlUtil;

namespace VDS.RDF.Query.Spin.SparqlStrategies
{
    /// <summary>
    /// A base strategy object for SPARQL commands rewriting and evaluation handling
    /// </summary>
    // TODO! CRITICAL Ensure that whenever rewriting is performed, the placement of bind assignments is preserved according to the original query
    //          We maybe should then not use the GraphPattern class but create another class that do not use UnplacedFilters and UnplacedAssignments collections
    internal interface ISparqlHandlingStrategy
        : ISparqlSDPlugin
    {

        void Handle(SparqlCommandUnit command);

    }

}
