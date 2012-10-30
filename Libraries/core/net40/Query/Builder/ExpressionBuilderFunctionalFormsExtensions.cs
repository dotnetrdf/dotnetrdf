using VDS.RDF.Query.Expressions.Functions.Sparql.Boolean;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query.Builder
{
    public static class ExpressionBuilderFunctionalFormsExtensions
    {
        public static BooleanExpression Bound(this ExpressionBuilder eb, VariableTerm var)
        {
            return new BooleanExpression(new BoundFunction(var));
        }

        public static BooleanExpression Bound(this ExpressionBuilder eb, string var)
        {
            return new BooleanExpression(new BoundFunction(eb.Variable(var)));
        }
    }
}