using VDS.RDF.Query.Builder.Expressions;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Functions.Sparql.Boolean;

namespace VDS.RDF.Query.Builder
{
    public static class ExpressionBuilderFunctionalFormsExtensions
    {
        public static BooleanExpression Bound(this ExpressionBuilder eb, VariableExpression var)
        {
            return new BooleanExpression(new BoundFunction(var.Expression));
        }

        public static BooleanExpression Bound(this ExpressionBuilder eb, string var)
        {
            return Bound(eb, eb.Variable(var));
        }
    }
}