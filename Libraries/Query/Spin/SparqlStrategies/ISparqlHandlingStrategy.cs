using VDS.RDF.Query.Spin.Core;
using VDS.RDF.Query.Spin.SparqlUtil;

namespace VDS.RDF.Query.Spin.SparqlStrategies
{
    /// <summary>
    /// Interface for classes that rewrite SPARQL commands and monitor SPARQL execution
    /// </summary>
    internal interface ISparqlHandlingStrategy
        : ISparqlSDPlugin
    {
        void Handle(SparqlCommandUnit command);
    }
}