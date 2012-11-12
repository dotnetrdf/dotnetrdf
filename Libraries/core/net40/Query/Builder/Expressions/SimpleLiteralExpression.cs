using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Query.Builder.Expressions
{
    /// <summary>
    /// Represents a literal expression without a datatype or language tag
    /// </summary>
    public class SimpleLiteralExpression:LiteralExpression
    {
        internal SimpleLiteralExpression(ISparqlExpression expression) : base(expression)
        {
        }
    }
}