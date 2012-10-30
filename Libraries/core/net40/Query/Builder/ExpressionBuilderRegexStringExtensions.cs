using VDS.RDF.Query.Builder.Expressions;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Functions.Sparql.Boolean;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query.Builder
{
    public static class ExpressionBuilderRegexStringExtensions
    {
        public static BooleanExpression Regex(this ExpressionBuilder eb, TypedSparqlExpression<VariableTerm> text, string pattern)
        {
            return new BooleanExpression(new RegexFunction(text.Expression, eb.Constant(pattern).Expression));
        }
    }
}