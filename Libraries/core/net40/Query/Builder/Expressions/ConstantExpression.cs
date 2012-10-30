using VDS.RDF.Query.Builder.Expressions;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query.Builder
{
    public class ConstantExpression : TypedSparqlExpression<ConstantTerm>
    {
        public ConstantExpression(ILiteralNode literalNode)
            : base(new ConstantTerm(literalNode))
        {
        }
    }
}