using VDS.RDF.Query.Builder.Expressions;
using VDS.RDF.Query.Expressions.Functions.Sparql.Boolean;
using VDS.RDF.Query.Expressions.Functions.Sparql.String;

namespace VDS.RDF.Query.Builder
{
    /// <summary>
    /// Provides methods for creating SPARQL functions, which operate on strings
    /// </summary>
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

        /// <summary>
        /// Creates a call to the STRLEN function with a variable parameter
        /// </summary>
        /// <param name="eb"> </param>
        /// <param name="str">a SPARQL variable</param>
        public static NumericExpression<int> StrLen(this ExpressionBuilder eb, VariableExpression str)
        {
            return new NumericExpression<int>(new StrLenFunction(str.Expression));
        }

        /// <summary>
        /// Creates a call to the STRLEN function with a variable parameter
        /// </summary>
        /// <param name="eb"> </param>
        /// <param name="str">a string literal parameter</param>
        public static NumericExpression<int> StrLen(this ExpressionBuilder eb, TypedLiteralExpression<string> str)
        {
            return new NumericExpression<int>(new StrLenFunction(str.Expression));
        }
    }
}