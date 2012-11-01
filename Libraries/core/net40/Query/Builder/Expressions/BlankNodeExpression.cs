using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Query.Builder.Expressions
{
    public class BlankNodeExpression : RdfTermExpression
    {
        internal BlankNodeExpression(ISparqlExpression expression) : base(expression)
        {
        }
    }
}