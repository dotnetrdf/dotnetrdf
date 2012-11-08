using VDS.RDF.Query.Builder.Expressions;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Functions.Sparql.Boolean;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query.Builder
{
    public static class ExpressionBuilderRegexStringExtensions
    {
        public static BooleanExpression Regex(this ExpressionBuilder eb, SparqlExpression text, string pattern)
        {
            return new BooleanExpression(new RegexFunction(text.Expression, eb.Constant(pattern).Expression));
        }

        public static BooleanExpression Regex(this ExpressionBuilder eb, SparqlExpression text, string pattern, string flags)
        {
            return new BooleanExpression(new RegexFunction(text.Expression, eb.Constant(pattern).Expression, eb.Constant(flags).Expression));
        }
    }
}