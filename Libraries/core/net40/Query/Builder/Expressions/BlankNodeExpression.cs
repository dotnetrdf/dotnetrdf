using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Query.Builder.Expressions
{
    /// <summary>
    /// Represents a blank node RDF term expression
    /// </summary>
    public class BlankNodeExpression : RdfTermExpression
    {
        /// <summary>
        /// Wraps the <paramref name="expression"/> as a blank node expression
        /// </summary>
        public BlankNodeExpression(ISparqlExpression expression) : base(expression)
        {
        }
    }
}