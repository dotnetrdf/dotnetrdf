using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Query.Builder.Expressions
{
    /// <summary>
    /// Represents a literal expression without a datatype or language tag
    /// </summary>
    public class SimpleLiteralExpression:LiteralExpression
    {
        /// <summary>
        /// Wraps the <paramref name="expression"/> as a simple literal expression
        /// </summary>
        public SimpleLiteralExpression(ISparqlExpression expression) : base(expression)
        {
        }
    }
}