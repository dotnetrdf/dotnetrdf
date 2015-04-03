using VDS.RDF.Query.Spin.Core;

namespace VDS.RDF.Query.Spin.SparqlStrategies
{
    /// <summary>
    /// A base strategy object for SPARQL rewriting
    /// </summary>
    public abstract class BaseSparqlRewriteStrategy
    {

        public abstract bool IsRequiredBy(Connection context);

        internal abstract void Rewrite(SparqlCommandUnit command);

    }

    /// <summary>
    /// A base strategy object for SPARQL evaluation
    /// </summary>
    /// <remarks>This should normaly be only used for custom function extensions (Spin functions without body) evaluation</remarks>
    /// <remarks>TODO discover what extension functions the underlying storage supports to avoid local evaluation</remarks>
    public abstract class BaseSparqlEvaluationStrategy
    {

    }
}
