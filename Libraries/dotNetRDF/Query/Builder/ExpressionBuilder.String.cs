/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/

using VDS.RDF.Query.Builder.Expressions;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Functions.Sparql.Boolean;
using VDS.RDF.Query.Expressions.Functions.Sparql.String;

namespace VDS.RDF.Query.Builder
{
    /// <inheritdoc />
    internal partial class ExpressionBuilder
    {
        /// <inheritdoc />
        public BooleanExpression Regex(VariableExpression text, string pattern)
        {
            return Regex(text.Expression, pattern.ToSimpleLiteral(), null);
        }

        /// <inheritdoc />
        public BooleanExpression Regex(VariableExpression text, VariableExpression pattern)
        {
            return Regex(text.Expression, pattern.Expression, null);
        }

        /// <inheritdoc />
        public BooleanExpression Regex(LiteralExpression text, string pattern)
        {
            return Regex(text.Expression, pattern.ToSimpleLiteral(), null);
        }

        /// <inheritdoc />
        public BooleanExpression Regex(LiteralExpression text, LiteralExpression pattern)
        {
            return Regex(text.Expression, pattern.Expression, null);
        }

        /// <inheritdoc />
        public BooleanExpression Regex(LiteralExpression text, VariableExpression pattern)
        {
            return Regex(text.Expression, pattern.Expression, null);
        }

        /// <inheritdoc />
        public BooleanExpression Regex(SparqlExpression text, string pattern, string flags)
        {
            return Regex(text.Expression, Constant(pattern).Expression, flags.ToConstantTerm());
        }

        /// <inheritdoc />
        public BooleanExpression Regex(VariableExpression text, VariableExpression pattern, string flags)
        {
            return Regex(text.Expression, pattern.Expression, flags.ToConstantTerm());
        }

        /// <inheritdoc />
        public BooleanExpression Regex(LiteralExpression text, string pattern, string flags)
        {
            return Regex(text.Expression, pattern.ToSimpleLiteral(), flags.ToConstantTerm());
        }

        /// <inheritdoc />
        public BooleanExpression Regex(LiteralExpression text, LiteralExpression pattern, string flags)
        {
            return Regex(text.Expression, pattern.Expression, flags.ToConstantTerm());
        }

        /// <inheritdoc />
        public BooleanExpression Regex(LiteralExpression text, VariableExpression pattern, string flags)
        {
            return Regex(text.Expression, pattern.Expression, flags.ToConstantTerm());
        }

        private static BooleanExpression Regex(ISparqlExpression text, ISparqlExpression pattern, ISparqlExpression flags)
        {
            RegexFunction regex = flags == null ? new RegexFunction(text, pattern) : new RegexFunction(text, pattern, flags);

            return new BooleanExpression(regex);
        }

        /// <inheritdoc />
        public NumericExpression<int> StrLen(VariableExpression str)
        {
            return new NumericExpression<int>(new StrLenFunction(str.Expression));
        }

        /// <inheritdoc />
        public NumericExpression<int> StrLen(TypedLiteralExpression<string> str)
        {
            return new NumericExpression<int>(new StrLenFunction(str.Expression));
        }

        private static TypedLiteralExpression<string> Substr(ISparqlExpression str, ISparqlExpression startingLoc, ISparqlExpression length)
        {
            var subStrFunction = length == null ? new SubStrFunction(str, startingLoc) : new SubStrFunction(str, startingLoc, length);
            return new TypedLiteralExpression<string>(subStrFunction);
        }

        /// <inheritdoc />
        public TypedLiteralExpression<string> Substr(TypedLiteralExpression<string> str, NumericExpression<int> startingLoc)
        {
            return Substr(str.Expression, startingLoc.Expression, null);
        }

        /// <inheritdoc />
        public TypedLiteralExpression<string> Substr(TypedLiteralExpression<string> str, VariableExpression startingLoc)
        {
            return Substr(str.Expression, startingLoc.Expression, null);
        }

        /// <inheritdoc />
        public TypedLiteralExpression<string> Substr(TypedLiteralExpression<string> str, int startingLoc)
        {
            return Substr(str.Expression, startingLoc.ToConstantTerm(), null);
        }

        /// <inheritdoc />
        public TypedLiteralExpression<string> Substr(VariableExpression str, NumericExpression<int> startingLoc)
        {
            return Substr(str.Expression, startingLoc.Expression, null);
        }

        /// <inheritdoc />
        public TypedLiteralExpression<string> Substr(VariableExpression str, int startingLoc)
        {
            return Substr(str.Expression, startingLoc.ToConstantTerm(), null);
        }

        /// <inheritdoc />
        public TypedLiteralExpression<string> Substr(VariableExpression str, VariableExpression startingLoc)
        {
            return Substr(str.Expression, startingLoc.Expression, null);
        }

        /// <inheritdoc />
        public TypedLiteralExpression<string> Substr(TypedLiteralExpression<string> str, NumericExpression<int> startingLoc, int length)
        {
            return Substr(str.Expression, startingLoc.Expression, length.ToConstantTerm());
        }

        /// <inheritdoc />
        public TypedLiteralExpression<string> Substr(TypedLiteralExpression<string> str, VariableExpression startingLoc, int length)
        {
            return Substr(str.Expression, startingLoc.Expression, length.ToConstantTerm());
        }

        /// <inheritdoc />
        public TypedLiteralExpression<string> Substr(TypedLiteralExpression<string> str, int startingLoc, int length)
        {
            return Substr(str.Expression, startingLoc.ToConstantTerm(), length.ToConstantTerm());
        }

        /// <inheritdoc />
        public TypedLiteralExpression<string> Substr(VariableExpression str, NumericExpression<int> startingLoc, int length)
        {
            return Substr(str.Expression, startingLoc.Expression, length.ToConstantTerm());
        }

        /// <inheritdoc />
        public TypedLiteralExpression<string> Substr(VariableExpression str, int startingLoc, int length)
        {
            return Substr(str.Expression, startingLoc.ToConstantTerm(), length.ToConstantTerm());
        }

        /// <inheritdoc />
        public TypedLiteralExpression<string> Substr(VariableExpression str, VariableExpression startingLoc, int length)
        {
            return Substr(str.Expression, startingLoc.Expression, length.ToConstantTerm());
        }

        /// <inheritdoc />
        public TypedLiteralExpression<string> Substr(TypedLiteralExpression<string> str, NumericExpression<int> startingLoc, NumericExpression<int> length)
        {
            return Substr(str.Expression, startingLoc.Expression, length.Expression);
        }

        /// <inheritdoc />
        public TypedLiteralExpression<string> Substr(TypedLiteralExpression<string> str, VariableExpression startingLoc, NumericExpression<int> length)
        {
            return Substr(str.Expression, startingLoc.Expression, length.Expression);
        }

        /// <inheritdoc />
        public TypedLiteralExpression<string> Substr(TypedLiteralExpression<string> str, int startingLoc, NumericExpression<int> length)
        {
            return Substr(str.Expression, startingLoc.ToConstantTerm(), length.Expression);
        }

        /// <inheritdoc />
        public TypedLiteralExpression<string> Substr(VariableExpression str, NumericExpression<int> startingLoc, NumericExpression<int> length)
        {
            return Substr(str.Expression, startingLoc.Expression, length.Expression);
        }

        /// <inheritdoc />
        public TypedLiteralExpression<string> Substr(VariableExpression str, int startingLoc, NumericExpression<int> length)
        {
            return Substr(str.Expression, startingLoc.ToConstantTerm(), length.Expression);
        }

        /// <inheritdoc />
        public TypedLiteralExpression<string> Substr(VariableExpression str, VariableExpression startingLoc, NumericExpression<int> length)
        {
            return Substr(str.Expression, startingLoc.Expression, length.Expression);
        }

        /// <inheritdoc />
        public TypedLiteralExpression<string> Substr(TypedLiteralExpression<string> str, NumericExpression<int> startingLoc, VariableExpression length)
        {
            return Substr(str.Expression, startingLoc.Expression, length.Expression);
        }

        /// <inheritdoc />
        public TypedLiteralExpression<string> Substr(TypedLiteralExpression<string> str, VariableExpression startingLoc, VariableExpression length)
        {
            return Substr(str.Expression, startingLoc.Expression, length.Expression);
        }

        /// <inheritdoc />
        public TypedLiteralExpression<string> Substr(TypedLiteralExpression<string> str, int startingLoc, VariableExpression length)
        {
            return Substr(str.Expression, startingLoc.ToConstantTerm(), length.Expression);
        }

        /// <inheritdoc />
        public TypedLiteralExpression<string> Substr(VariableExpression str, NumericExpression<int> startingLoc, VariableExpression length)
        {
            return Substr(str.Expression, startingLoc.Expression, length.Expression);
        }

        /// <inheritdoc />
        public TypedLiteralExpression<string> Substr(VariableExpression str, int startingLoc, VariableExpression length)
        {
            return Substr(str.Expression, startingLoc.ToConstantTerm(), length.Expression);
        }

        /// <inheritdoc />
        public TypedLiteralExpression<string> Substr(VariableExpression str, VariableExpression startingLoc, VariableExpression length)
        {
            return Substr(str.Expression, startingLoc.Expression, length.Expression);
        }

        private static BooleanExpression LangMatches(ISparqlExpression languageTag, ISparqlExpression languageRange)
        {
            return new BooleanExpression(new LangMatchesFunction(languageTag, languageRange));
        }

        /// <inheritdoc />
        public BooleanExpression LangMatches(LiteralExpression languageTag, string languageRange)
        {
            return LangMatches(languageTag.Expression, languageRange.ToConstantTerm());
        }

        /// <inheritdoc />
        public BooleanExpression LangMatches(VariableExpression languageTag, string languageRange)
        {
            return LangMatches(languageTag.Expression, languageRange.ToConstantTerm());
        }

        /// <inheritdoc />
        public BooleanExpression LangMatches(LiteralExpression languageTag, LiteralExpression languageRange)
        {
            return LangMatches(languageTag.Expression, languageRange.Expression);
        }

        /// <inheritdoc />
        public BooleanExpression LangMatches(VariableExpression languageTag, LiteralExpression languageRange)
        {
            return LangMatches(languageTag.Expression, languageRange.Expression);
        }

        /// <inheritdoc />
        public BooleanExpression LangMatches(LiteralExpression languageTag, VariableExpression languageRange)
        {
            return LangMatches(languageTag.Expression, languageRange.Expression);
        }

        /// <inheritdoc />
        public BooleanExpression LangMatches(VariableExpression languageTag, VariableExpression languageRange)
        {
            return LangMatches(languageTag.Expression, languageRange.Expression);
        }
    }
}