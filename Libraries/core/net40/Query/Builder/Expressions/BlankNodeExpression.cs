using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Query.Builder.Expressions
{
    /// <summary>
    /// Represents a blank node RDF term expression
    /// </summary>
    public class BlankNodeExpression : RdfTermExpression
    {
        internal BlankNodeExpression(ISparqlExpression expression) : base(expression)
        {
        }
    }
}