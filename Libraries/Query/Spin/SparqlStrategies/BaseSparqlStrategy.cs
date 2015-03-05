using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Query.Spin.SparqlStrategies
{
    /// <summary>
    /// A base strategy object for SPARQL rewriting
    /// </summary>
    public abstract class BaseSparqlRewriteStrategy
    {

        public abstract bool IsRequiredBy(Connection context);

        //public event SparqlQueryEvent DefaultGraphAdded;
        //public event SparqlQueryEvent NamedGraphAdded;
        //public event SparqlQueryEvent GraphPatternAdded;

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
