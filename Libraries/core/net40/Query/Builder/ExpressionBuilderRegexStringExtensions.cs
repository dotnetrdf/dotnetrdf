using VDS.RDF.Query.Builder.Expressions;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Functions.Sparql.Boolean;
using VDS.RDF.Query.Expressions.Functions.Sparql.String;
using VDS.RDF.Query.Expressions.Primary;

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
        /// Creates a call to the STRLEN function with a string literal parameter
        /// </summary>
        /// <param name="eb"> </param>
        /// <param name="str">a string literal parameter</param>
        public static NumericExpression<int> StrLen(this ExpressionBuilder eb, TypedLiteralExpression<string> str)
        {
            return new NumericExpression<int>(new StrLenFunction(str.Expression));
        }

        private static TypedLiteralExpression<string> Substr(ISparqlExpression str, ISparqlExpression startingLoc, ISparqlExpression length)
        {
            var subStrFunction = length == null ? new SubStrFunction(str, startingLoc) : new SubStrFunction(str, startingLoc, length);
            return new TypedLiteralExpression<string>(subStrFunction);
        }

        /// <summary>
        /// Creates a call to the SUBSTR function with a string literal and variable parameters
        /// </summary>
        /// <param name="eb"> </param>
        /// <param name="str">a string literal parameter</param>
        /// <param name="startingLoc">1-based start index</param>
        public static TypedLiteralExpression<string> Substr(this ExpressionBuilder eb, TypedLiteralExpression<string> str, NumericExpression<int> startingLoc)
        {
            return Substr(str.Expression, startingLoc.Expression, null);
        }

        /// <summary>
        /// Creates a call to the SUBSTR function with a string literal and interger expression parameters
        /// </summary>
        /// <param name="eb"> </param>
        /// <param name="str">a string literal parameter</param>
        /// <param name="startingLoc">a SPARQL variable</param>
        public static TypedLiteralExpression<string> Substr(this ExpressionBuilder eb, TypedLiteralExpression<string> str, VariableExpression startingLoc)
        {
            return Substr(str.Expression, startingLoc.Expression, null);
        }

        /// <summary>
        /// Creates a call to the SUBSTR function with a string literal and interger parameters
        /// </summary>
        /// <param name="eb"> </param>
        /// <param name="str">a string literal parameter</param>
        /// <param name="startingLoc">1-based start index</param>
        public static TypedLiteralExpression<string> Substr(this ExpressionBuilder eb, TypedLiteralExpression<string> str, int startingLoc)
        {
            return Substr(str.Expression, startingLoc.ToConstantTerm(), null);
        }

        /// <summary>
        /// Creates a call to the SUBSTR function with a variable and interger expression parameters
        /// </summary>
        /// <param name="eb"> </param>
        /// <param name="str">a SPARQL variable</param>
        /// <param name="startingLoc">1-based start index</param>
        public static TypedLiteralExpression<string> Substr(this ExpressionBuilder eb, VariableExpression str, NumericExpression<int> startingLoc)
        {
            return Substr(str.Expression, startingLoc.Expression, null);
        }

        /// <summary>
        /// Creates a call to the SUBSTR function with a variable and interger parameters
        /// </summary>
        /// <param name="eb"> </param>
        /// <param name="str">a SPARQL variable</param>
        /// <param name="startingLoc">1-based start index</param>
        public static TypedLiteralExpression<string> Substr(this ExpressionBuilder eb, VariableExpression str, int startingLoc)
        {
            return Substr(str.Expression, startingLoc.ToConstantTerm(), null);
        }

        /// <summary>
        /// Creates a call to the SUBSTR function with two variable parameters
        /// </summary>
        /// <param name="eb"> </param>
        /// <param name="str">a SPARQL variable</param>
        /// <param name="startingLoc">a SPARQL variable</param>
        public static TypedLiteralExpression<string> Substr(this ExpressionBuilder eb, VariableExpression str, VariableExpression startingLoc)
        {
            return Substr(str.Expression, startingLoc.Expression, null);
        }

        /// <summary>
        /// Creates a call to the SUBSTR function with a string literal and variable parameters
        /// </summary>
        /// <param name="eb"> </param>
        /// <param name="str">a string literal parameter</param>
        /// <param name="startingLoc">1-based start index</param>
        /// <param name="length">substring length </param>
        public static TypedLiteralExpression<string> Substr(this ExpressionBuilder eb, TypedLiteralExpression<string> str, NumericExpression<int> startingLoc, int length)
        {
            return Substr(str.Expression, startingLoc.Expression, length.ToConstantTerm());
        }

        /// <summary>
        /// Creates a call to the SUBSTR function with a string literal and interger expression parameters
        /// </summary>
        /// <param name="eb"> </param>
        /// <param name="str">a string literal parameter</param>
        /// <param name="startingLoc">a SPARQL variable</param>
        /// <param name="length">substring length </param>
        public static TypedLiteralExpression<string> Substr(this ExpressionBuilder eb, TypedLiteralExpression<string> str, VariableExpression startingLoc, int length)
        {
            return Substr(str.Expression, startingLoc.Expression, length.ToConstantTerm());
        }

        /// <summary>
        /// Creates a call to the SUBSTR function with a string literal and interger parameters
        /// </summary>
        /// <param name="eb"> </param>
        /// <param name="str">a string literal parameter</param>
        /// <param name="startingLoc">1-based start index</param>
        /// <param name="length">substring length </param>
        public static TypedLiteralExpression<string> Substr(this ExpressionBuilder eb, TypedLiteralExpression<string> str, int startingLoc, int length)
        {
            return Substr(str.Expression, startingLoc.ToConstantTerm(), length.ToConstantTerm());
        }

        /// <summary>
        /// Creates a call to the SUBSTR function with a variable and interger expression parameters
        /// </summary>
        /// <param name="eb"> </param>
        /// <param name="str">a SPARQL variable</param>
        /// <param name="startingLoc">1-based start index</param>
        /// <param name="length">substring length </param>
        public static TypedLiteralExpression<string> Substr(this ExpressionBuilder eb, VariableExpression str, NumericExpression<int> startingLoc, int length)
        {
            return Substr(str.Expression, startingLoc.Expression, length.ToConstantTerm());
        }

        /// <summary>
        /// Creates a call to the SUBSTR function with a variable and interger parameters
        /// </summary>
        /// <param name="eb"> </param>
        /// <param name="str">a SPARQL variable</param>
        /// <param name="startingLoc">1-based start index</param>
        /// <param name="length">substring length </param>
        public static TypedLiteralExpression<string> Substr(this ExpressionBuilder eb, VariableExpression str, int startingLoc, int length)
        {
            return Substr(str.Expression, startingLoc.ToConstantTerm(), length.ToConstantTerm());
        }

        /// <summary>
        /// Creates a call to the SUBSTR function with two variable parameters
        /// </summary>
        /// <param name="eb"> </param>
        /// <param name="str">a SPARQL variable</param>
        /// <param name="startingLoc">a SPARQL variable</param>
        /// <param name="length">substring length </param>
        public static TypedLiteralExpression<string> Substr(this ExpressionBuilder eb, VariableExpression str, VariableExpression startingLoc, int length)
        {
            return Substr(str.Expression, startingLoc.Expression, length.ToConstantTerm());
        }

        /// <summary>
        /// Creates a call to the SUBSTR function with a string literal and two integer expressions parameters
        /// </summary>
        /// <param name="eb"> </param>
        /// <param name="str">a string literal parameter</param>
        /// <param name="startingLoc">1-based start index</param>
        /// <param name="length">substring length </param>
        public static TypedLiteralExpression<string> Substr(this ExpressionBuilder eb, TypedLiteralExpression<string> str, NumericExpression<int> startingLoc, NumericExpression<int> length)
        {
            return Substr(str.Expression, startingLoc.Expression, length.Expression);
        }

        /// <summary>
        /// Creates a call to the SUBSTR function with a string literal, variable and interger expression parameters
        /// </summary>
        /// <param name="eb"> </param>
        /// <param name="str">a string literal parameter</param>
        /// <param name="startingLoc">a SPARQL variable</param>
        /// <param name="length">substring length </param>
        public static TypedLiteralExpression<string> Substr(this ExpressionBuilder eb, TypedLiteralExpression<string> str, VariableExpression startingLoc, NumericExpression<int> length)
        {
            return Substr(str.Expression, startingLoc.Expression, length.Expression);
        }

        /// <summary>
        /// Creates a call to the SUBSTR function with a string literal, interger and integer expression parameters
        /// </summary>
        /// <param name="eb"> </param>
        /// <param name="str">a string literal parameter</param>
        /// <param name="startingLoc">1-based start index</param>
        /// <param name="length">substring length </param>
        public static TypedLiteralExpression<string> Substr(this ExpressionBuilder eb, TypedLiteralExpression<string> str, int startingLoc, NumericExpression<int> length)
        {
            return Substr(str.Expression, startingLoc.ToConstantTerm(), length.Expression);
        }

        /// <summary>
        /// Creates a call to the SUBSTR function with a variable, interger expression and integer expression parameters
        /// </summary>
        /// <param name="eb"> </param>
        /// <param name="str">a SPARQL variable</param>
        /// <param name="startingLoc">1-based start index</param>
        /// <param name="length">substring length </param>
        public static TypedLiteralExpression<string> Substr(this ExpressionBuilder eb, VariableExpression str, NumericExpression<int> startingLoc, NumericExpression<int> length)
        {
            return Substr(str.Expression, startingLoc.Expression, length.Expression);
        }

        /// <summary>
        /// Creates a call to the SUBSTR function with a variable, interger and a numeric expression parameters
        /// </summary>
        /// <param name="eb"> </param>
        /// <param name="str">a SPARQL variable</param>
        /// <param name="startingLoc">1-based start index</param>
        /// <param name="length">substring length </param>
        public static TypedLiteralExpression<string> Substr(this ExpressionBuilder eb, VariableExpression str, int startingLoc, NumericExpression<int> length)
        {
            return Substr(str.Expression, startingLoc.ToConstantTerm(), length.Expression);
        }

        /// <summary>
        /// Creates a call to the SUBSTR function with two variable parameters
        /// </summary>
        /// <param name="eb"> </param>
        /// <param name="str">a SPARQL variable</param>
        /// <param name="startingLoc">a SPARQL variable</param>
        /// <param name="length">substring length </param>
        public static TypedLiteralExpression<string> Substr(this ExpressionBuilder eb, VariableExpression str, VariableExpression startingLoc, NumericExpression<int> length)
        {
            return Substr(str.Expression, startingLoc.Expression, length.Expression);
        }

        /// <summary>
        /// Creates a call to the SUBSTR function with a string literal, interger expression and a numeric expression parameters
        /// </summary>
        /// <param name="eb"> </param>
        /// <param name="str">a string literal parameter</param>
        /// <param name="startingLoc">1-based start index</param>
        /// <param name="length">substring length </param>
        public static TypedLiteralExpression<string> Substr(this ExpressionBuilder eb, TypedLiteralExpression<string> str, NumericExpression<int> startingLoc, VariableExpression length)
        {
            return Substr(str.Expression, startingLoc.Expression, length.Expression);
        }

        /// <summary>
        /// Creates a call to the SUBSTR function with a string literal, interger expression and a variable parameters
        /// </summary>
        /// <param name="eb"> </param>
        /// <param name="str">a string literal parameter</param>
        /// <param name="startingLoc">a SPARQL variable</param>
        /// <param name="length">substring length </param>
        public static TypedLiteralExpression<string> Substr(this ExpressionBuilder eb, TypedLiteralExpression<string> str, VariableExpression startingLoc, VariableExpression length)
        {
            return Substr(str.Expression, startingLoc.Expression, length.Expression);
        }

        /// <summary>
        /// Creates a call to the SUBSTR function with a string literal, interger and a variable parameters
        /// </summary>
        /// <param name="eb"> </param>
        /// <param name="str">a string literal parameter</param>
        /// <param name="startingLoc">1-based start index</param>
        /// <param name="length">substring length </param>
        public static TypedLiteralExpression<string> Substr(this ExpressionBuilder eb, TypedLiteralExpression<string> str, int startingLoc, VariableExpression length)
        {
            return Substr(str.Expression, startingLoc.ToConstantTerm(), length.Expression);
        }

        /// <summary>
        /// Creates a call to the SUBSTR function with a variable, interger expression and a variable parameters
        /// </summary>
        /// <param name="eb"> </param>
        /// <param name="str">a SPARQL variable</param>
        /// <param name="startingLoc">1-based start index</param>
        /// <param name="length">substring length </param>
        public static TypedLiteralExpression<string> Substr(this ExpressionBuilder eb, VariableExpression str, NumericExpression<int> startingLoc, VariableExpression length)
        {
            return Substr(str.Expression, startingLoc.Expression, length.Expression);
        }

        /// <summary>
        /// Creates a call to the SUBSTR function with a variable, interger and a variable parameters
        /// </summary>
        /// <param name="eb"> </param>
        /// <param name="str">a SPARQL variable</param>
        /// <param name="startingLoc">1-based start index</param>
        /// <param name="length">substring length </param>
        public static TypedLiteralExpression<string> Substr(this ExpressionBuilder eb, VariableExpression str, int startingLoc, VariableExpression length)
        {
            return Substr(str.Expression, startingLoc.ToConstantTerm(), length.Expression);
        }

        /// <summary>
        /// Creates a call to the SUBSTR function with three variable parameters
        /// </summary>
        /// <param name="eb"> </param>
        /// <param name="str">a SPARQL variable</param>
        /// <param name="startingLoc">a SPARQL variable</param>
        /// <param name="length">substring length </param>
        public static TypedLiteralExpression<string> Substr(this ExpressionBuilder eb, VariableExpression str, VariableExpression startingLoc, VariableExpression length)
        {
            return Substr(str.Expression, startingLoc.Expression, length.Expression);
        } 
    }
}