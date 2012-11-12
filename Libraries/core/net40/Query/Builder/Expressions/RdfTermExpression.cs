using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Query.Builder.Expressions
{
    /// <summary>
    /// Represents a RDF term expression (IRI, literal or blank node)
    /// </summary>
    public class RdfTermExpression : SparqlExpression
    {
        /// <summary>
        /// Wraps the <paramref name="expression"/> as an RDF term expression
        /// </summary>
        public RdfTermExpression(ISparqlExpression expression) : base(expression)
        {
        }
    }
}