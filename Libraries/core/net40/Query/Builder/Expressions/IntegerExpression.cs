using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Query.Builder.Expressions
{
    class IntegerExpression : NumericExpression<int>
    {
        public IntegerExpression(ISparqlExpression expression)
            : base(expression)
        {
        }
    }
}