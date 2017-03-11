/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2013 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using VDS.RDF.Query.Builder.Expressions;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Functions.Sparql.Boolean;
using VDS.RDF.Query.Expressions.Functions.Sparql.String;

namespace VDS.RDF.Query.Builder
{
    /// <summary>
    /// Provides methods for creating SPARQL functions, which operate on strings
    /// </summary>
    public partial class ExpressionBuilder
    {
        /// <summary>
        /// Creates a call to the REGEX function
        /// </summary>
        public BooleanExpression Regex(VariableExpression text, string pattern)
        {
            return Regex(text.Expression, pattern.ToSimpleLiteral(), null);
        }

        /// <summary>
        /// Creates a call to the REGEX function
        /// </summary>
        public BooleanExpression Regex(VariableExpression text, VariableExpression pattern)
        {
            return Regex(text.Expression, pattern.Expression, null);
        }

        /// <summary>
        /// Creates a call to the REGEX function
        /// </summary>
        public BooleanExpression Regex(LiteralExpression text, string pattern)
        {
            return Regex(text.Expression, pattern.ToSimpleLiteral(), null);
        }

        /// <summary>
        /// Creates a call to the REGEX function
        /// </summary>
        public BooleanExpression Regex(LiteralExpression text, LiteralExpression pattern)
        {
            return Regex(text.Expression, pattern.Expression, null);
        }

        /// <summary>
        /// Creates a call to the REGEX function
        /// </summary>
        public BooleanExpression Regex(LiteralExpression text, VariableExpression pattern)
        {
            return Regex(text.Expression, pattern.Expression, null);
        }

        /// <summary>
        /// Creates a call to the REGEX function
        /// </summary>
        public BooleanExpression Regex(SparqlExpression text, string pattern, string flags)
        {
            return Regex(text.Expression, Constant(pattern).Expression, flags.ToConstantTerm());
        }

        /// <summary>
        /// Creates a call to the REGEX function
        /// </summary>
        public BooleanExpression Regex(VariableExpression text, VariableExpression pattern, string flags)
        {
            return Regex(text.Expression, pattern.Expression, flags.ToConstantTerm());
        }

        /// <summary>
        /// Creates a call to the REGEX function
        /// </summary>
        public BooleanExpression Regex(LiteralExpression text, string pattern, string flags)
        {
            return Regex(text.Expression, pattern.ToSimpleLiteral(), flags.ToConstantTerm());
        }

        /// <summary>
        /// Creates a call to the REGEX function
        /// </summary>
        public BooleanExpression Regex(LiteralExpression text, LiteralExpression pattern, string flags)
        {
            return Regex(text.Expression, pattern.Expression, flags.ToConstantTerm());
        }

        /// <summary>
        /// Creates a call to the REGEX function
        /// </summary>
        public BooleanExpression Regex(LiteralExpression text, VariableExpression pattern, string flags)
        {
            return Regex(text.Expression, pattern.Expression, flags.ToConstantTerm());
        }

        private static BooleanExpression Regex(ISparqlExpression text, ISparqlExpression pattern, ISparqlExpression flags)
        {
            RegexFunction regex = flags == null ? new RegexFunction(text, pattern) : new RegexFunction(text, pattern, flags);

            return new BooleanExpression(regex);
        }

        /// <summary>
        /// Creates a call to the STRLEN function with a variable parameter
        /// </summary>
        /// <param name="str">a SPARQL variable</param>
        public NumericExpression<int> StrLen(VariableExpression str)
        {
            return new NumericExpression<int>(new StrLenFunction(str.Expression));
        }

        /// <summary>
        /// Creates a call to the STRLEN function with a string literal parameter
        /// </summary>
        /// <param name="str">a string literal parameter</param>
        public NumericExpression<int> StrLen(TypedLiteralExpression<string> str)
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
        /// <param name="str">a string literal parameter</param>
        /// <param name="startingLoc">1-based start index</param>
        public TypedLiteralExpression<string> Substr(TypedLiteralExpression<string> str, NumericExpression<int> startingLoc)
        {
            return Substr(str.Expression, startingLoc.Expression, null);
        }

        /// <summary>
        /// Creates a call to the SUBSTR function with a string literal and interger expression parameters
        /// </summary>
        /// <param name="str">a string literal parameter</param>
        /// <param name="startingLoc">a SPARQL variable</param>
        public TypedLiteralExpression<string> Substr(TypedLiteralExpression<string> str, VariableExpression startingLoc)
        {
            return Substr(str.Expression, startingLoc.Expression, null);
        }

        /// <summary>
        /// Creates a call to the SUBSTR function with a string literal and interger parameters
        /// </summary>
        /// <param name="str">a string literal parameter</param>
        /// <param name="startingLoc">1-based start index</param>
        public TypedLiteralExpression<string> Substr(TypedLiteralExpression<string> str, int startingLoc)
        {
            return Substr(str.Expression, startingLoc.ToConstantTerm(), null);
        }

        /// <summary>
        /// Creates a call to the SUBSTR function with a variable and interger expression parameters
        /// </summary>
        /// <param name="str">a SPARQL variable</param>
        /// <param name="startingLoc">1-based start index</param>
        public TypedLiteralExpression<string> Substr(VariableExpression str, NumericExpression<int> startingLoc)
        {
            return Substr(str.Expression, startingLoc.Expression, null);
        }

        /// <summary>
        /// Creates a call to the SUBSTR function with a variable and interger parameters
        /// </summary>
        /// <param name="str">a SPARQL variable</param>
        /// <param name="startingLoc">1-based start index</param>
        public TypedLiteralExpression<string> Substr(VariableExpression str, int startingLoc)
        {
            return Substr(str.Expression, startingLoc.ToConstantTerm(), null);
        }

        /// <summary>
        /// Creates a call to the SUBSTR function with two variable parameters
        /// </summary>
        /// <param name="str">a SPARQL variable</param>
        /// <param name="startingLoc">a SPARQL variable</param>
        public TypedLiteralExpression<string> Substr(VariableExpression str, VariableExpression startingLoc)
        {
            return Substr(str.Expression, startingLoc.Expression, null);
        }

        /// <summary>
        /// Creates a call to the SUBSTR function with a string literal and variable parameters
        /// </summary>
        /// <param name="str">a string literal parameter</param>
        /// <param name="startingLoc">1-based start index</param>
        /// <param name="length">substring length </param>
        public TypedLiteralExpression<string> Substr(TypedLiteralExpression<string> str, NumericExpression<int> startingLoc, int length)
        {
            return Substr(str.Expression, startingLoc.Expression, length.ToConstantTerm());
        }

        /// <summary>
        /// Creates a call to the SUBSTR function with a string literal and interger expression parameters
        /// </summary>
        /// <param name="str">a string literal parameter</param>
        /// <param name="startingLoc">a SPARQL variable</param>
        /// <param name="length">substring length </param>
        public TypedLiteralExpression<string> Substr(TypedLiteralExpression<string> str, VariableExpression startingLoc, int length)
        {
            return Substr(str.Expression, startingLoc.Expression, length.ToConstantTerm());
        }

        /// <summary>
        /// Creates a call to the SUBSTR function with a string literal and interger parameters
        /// </summary>
        /// <param name="str">a string literal parameter</param>
        /// <param name="startingLoc">1-based start index</param>
        /// <param name="length">substring length </param>
        public TypedLiteralExpression<string> Substr(TypedLiteralExpression<string> str, int startingLoc, int length)
        {
            return Substr(str.Expression, startingLoc.ToConstantTerm(), length.ToConstantTerm());
        }

        /// <summary>
        /// Creates a call to the SUBSTR function with a variable and interger expression parameters
        /// </summary>
        /// <param name="str">a SPARQL variable</param>
        /// <param name="startingLoc">1-based start index</param>
        /// <param name="length">substring length </param>
        public TypedLiteralExpression<string> Substr(VariableExpression str, NumericExpression<int> startingLoc, int length)
        {
            return Substr(str.Expression, startingLoc.Expression, length.ToConstantTerm());
        }

        /// <summary>
        /// Creates a call to the SUBSTR function with a variable and interger parameters
        /// </summary>
        /// <param name="str">a SPARQL variable</param>
        /// <param name="startingLoc">1-based start index</param>
        /// <param name="length">substring length </param>
        public TypedLiteralExpression<string> Substr(VariableExpression str, int startingLoc, int length)
        {
            return Substr(str.Expression, startingLoc.ToConstantTerm(), length.ToConstantTerm());
        }

        /// <summary>
        /// Creates a call to the SUBSTR function with two variable parameters
        /// </summary>
        /// <param name="str">a SPARQL variable</param>
        /// <param name="startingLoc">a SPARQL variable</param>
        /// <param name="length">substring length </param>
        public TypedLiteralExpression<string> Substr(VariableExpression str, VariableExpression startingLoc, int length)
        {
            return Substr(str.Expression, startingLoc.Expression, length.ToConstantTerm());
        }

        /// <summary>
        /// Creates a call to the SUBSTR function with a string literal and two integer expressions parameters
        /// </summary>
        /// <param name="str">a string literal parameter</param>
        /// <param name="startingLoc">1-based start index</param>
        /// <param name="length">substring length </param>
        public TypedLiteralExpression<string> Substr(TypedLiteralExpression<string> str, NumericExpression<int> startingLoc, NumericExpression<int> length)
        {
            return Substr(str.Expression, startingLoc.Expression, length.Expression);
        }

        /// <summary>
        /// Creates a call to the SUBSTR function with a string literal, variable and interger expression parameters
        /// </summary>
        /// <param name="str">a string literal parameter</param>
        /// <param name="startingLoc">a SPARQL variable</param>
        /// <param name="length">substring length </param>
        public TypedLiteralExpression<string> Substr(TypedLiteralExpression<string> str, VariableExpression startingLoc, NumericExpression<int> length)
        {
            return Substr(str.Expression, startingLoc.Expression, length.Expression);
        }

        /// <summary>
        /// Creates a call to the SUBSTR function with a string literal, interger and integer expression parameters
        /// </summary>
        /// <param name="str">a string literal parameter</param>
        /// <param name="startingLoc">1-based start index</param>
        /// <param name="length">substring length </param>
        public TypedLiteralExpression<string> Substr(TypedLiteralExpression<string> str, int startingLoc, NumericExpression<int> length)
        {
            return Substr(str.Expression, startingLoc.ToConstantTerm(), length.Expression);
        }

        /// <summary>
        /// Creates a call to the SUBSTR function with a variable, interger expression and integer expression parameters
        /// </summary>
        /// <param name="str">a SPARQL variable</param>
        /// <param name="startingLoc">1-based start index</param>
        /// <param name="length">substring length </param>
        public TypedLiteralExpression<string> Substr(VariableExpression str, NumericExpression<int> startingLoc, NumericExpression<int> length)
        {
            return Substr(str.Expression, startingLoc.Expression, length.Expression);
        }

        /// <summary>
        /// Creates a call to the SUBSTR function with a variable, interger and a numeric expression parameters
        /// </summary>
        /// <param name="str">a SPARQL variable</param>
        /// <param name="startingLoc">1-based start index</param>
        /// <param name="length">substring length </param>
        public TypedLiteralExpression<string> Substr(VariableExpression str, int startingLoc, NumericExpression<int> length)
        {
            return Substr(str.Expression, startingLoc.ToConstantTerm(), length.Expression);
        }

        /// <summary>
        /// Creates a call to the SUBSTR function with two variable parameters
        /// </summary>
        /// <param name="str">a SPARQL variable</param>
        /// <param name="startingLoc">a SPARQL variable</param>
        /// <param name="length">substring length </param>
        public TypedLiteralExpression<string> Substr(VariableExpression str, VariableExpression startingLoc, NumericExpression<int> length)
        {
            return Substr(str.Expression, startingLoc.Expression, length.Expression);
        }

        /// <summary>
        /// Creates a call to the SUBSTR function with a string literal, interger expression and a numeric expression parameters
        /// </summary>
        /// <param name="str">a string literal parameter</param>
        /// <param name="startingLoc">1-based start index</param>
        /// <param name="length">substring length </param>
        public TypedLiteralExpression<string> Substr(TypedLiteralExpression<string> str, NumericExpression<int> startingLoc, VariableExpression length)
        {
            return Substr(str.Expression, startingLoc.Expression, length.Expression);
        }

        /// <summary>
        /// Creates a call to the SUBSTR function with a string literal, interger expression and a variable parameters
        /// </summary>
        /// <param name="str">a string literal parameter</param>
        /// <param name="startingLoc">a SPARQL variable</param>
        /// <param name="length">substring length </param>
        public TypedLiteralExpression<string> Substr(TypedLiteralExpression<string> str, VariableExpression startingLoc, VariableExpression length)
        {
            return Substr(str.Expression, startingLoc.Expression, length.Expression);
        }

        /// <summary>
        /// Creates a call to the SUBSTR function with a string literal, interger and a variable parameters
        /// </summary>
        /// <param name="str">a string literal parameter</param>
        /// <param name="startingLoc">1-based start index</param>
        /// <param name="length">substring length </param>
        public TypedLiteralExpression<string> Substr(TypedLiteralExpression<string> str, int startingLoc, VariableExpression length)
        {
            return Substr(str.Expression, startingLoc.ToConstantTerm(), length.Expression);
        }

        /// <summary>
        /// Creates a call to the SUBSTR function with a variable, interger expression and a variable parameters
        /// </summary>
        /// <param name="str">a SPARQL variable</param>
        /// <param name="startingLoc">1-based start index</param>
        /// <param name="length">substring length </param>
        public TypedLiteralExpression<string> Substr(VariableExpression str, NumericExpression<int> startingLoc, VariableExpression length)
        {
            return Substr(str.Expression, startingLoc.Expression, length.Expression);
        }

        /// <summary>
        /// Creates a call to the SUBSTR function with a variable, interger and a variable parameters
        /// </summary>
        /// <param name="str">a SPARQL variable</param>
        /// <param name="startingLoc">1-based start index</param>
        /// <param name="length">substring length </param>
        public TypedLiteralExpression<string> Substr(VariableExpression str, int startingLoc, VariableExpression length)
        {
            return Substr(str.Expression, startingLoc.ToConstantTerm(), length.Expression);
        }

        /// <summary>
        /// Creates a call to the SUBSTR function with three variable parameters
        /// </summary>
        /// <param name="str">a SPARQL variable</param>
        /// <param name="startingLoc">a SPARQL variable</param>
        /// <param name="length">substring length </param>
        public TypedLiteralExpression<string> Substr(VariableExpression str, VariableExpression startingLoc, VariableExpression length)
        {
            return Substr(str.Expression, startingLoc.Expression, length.Expression);
        }

        private static BooleanExpression LangMatches(ISparqlExpression languageTag, ISparqlExpression languageRange)
        {
            return new BooleanExpression(new LangMatchesFunction(languageTag, languageRange));
        }

        /// <summary>
        /// Creates a call to the LANGMATCHES function
        /// </summary>
        public BooleanExpression LangMatches(LiteralExpression languageTag, string languageRange)
        {
            return LangMatches(languageTag.Expression, languageRange.ToConstantTerm());
        }

        /// <summary>
        /// Creates a call to the LANGMATCHES function
        /// </summary>
        public BooleanExpression LangMatches(VariableExpression languageTag, string languageRange)
        {
            return LangMatches(languageTag.Expression, languageRange.ToConstantTerm());
        }

        /// <summary>
        /// Creates a call to the LANGMATCHES function
        /// </summary>
        public BooleanExpression LangMatches(LiteralExpression languageTag, LiteralExpression languageRange)
        {
            return LangMatches(languageTag.Expression, languageRange.Expression);
        }

        /// <summary>
        /// Creates a call to the LANGMATCHES function
        /// </summary>
        public BooleanExpression LangMatches(VariableExpression languageTag, LiteralExpression languageRange)
        {
            return LangMatches(languageTag.Expression, languageRange.Expression);
        }

        /// <summary>
        /// Creates a call to the LANGMATCHES function
        /// </summary>
        public BooleanExpression LangMatches(LiteralExpression languageTag, VariableExpression languageRange)
        {
            return LangMatches(languageTag.Expression, languageRange.Expression);
        }

        /// <summary>
        /// Creates a call to the LANGMATCHES function
        /// </summary>
        public BooleanExpression LangMatches(VariableExpression languageTag, VariableExpression languageRange)
        {
            return LangMatches(languageTag.Expression, languageRange.Expression);
        }
    }
}