using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Query.Builder.Expressions
{
    /// <summary>
    /// Represents a RDF term expression (IRI, literal or blank node)
    /// </summary>
    public class RdfTermExpression : SparqlExpression
    {
        internal RdfTermExpression(ISparqlExpression expression) : base(expression)
        {
        }
    }
}