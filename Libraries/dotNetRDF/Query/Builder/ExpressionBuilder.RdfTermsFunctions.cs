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

using System;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Builder.Expressions;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Functions.Sparql.Boolean;
using VDS.RDF.Query.Expressions.Functions.Sparql.Constructor;
using VDS.RDF.Query.Expressions.Functions.Sparql.String;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query.Builder
{
    internal partial class ExpressionBuilder
    {
        public BooleanExpression IsIRI(SparqlExpression term)
        {
            var isIri = new IsIriFunction(term.Expression);
            return new BooleanExpression(isIri);
        }

        public BooleanExpression IsIRI(string variableName)
        {
            return IsIRI(Variable(variableName));
        }

        public BooleanExpression IsBlank(SparqlExpression term)
        {
            var isBlank = new IsBlankFunction(term.Expression);
            return new BooleanExpression(isBlank);
        }

        public BooleanExpression IsBlank(string variableName)
        {
            return IsBlank(Variable(variableName));
        }

        public BooleanExpression IsLiteral(SparqlExpression term)
        {
            var isLiteral = new IsLiteralFunction(term.Expression);
            return new BooleanExpression(isLiteral);
        }

        public BooleanExpression IsLiteral(string variableName)
        {
            return IsLiteral(Variable(variableName));
        }

        public BooleanExpression IsNumeric(SparqlExpression term)
        {
            var isNumeric = new IsNumericFunction(term.Expression);
            return new BooleanExpression(isNumeric);
        }

        public BooleanExpression IsNumeric(string variableName)
        {
            return IsNumeric(Variable(variableName));
        }

        public LiteralExpression Str(VariableExpression variable)
        {
            return Str(variable.Expression);
        }

        public LiteralExpression Str(LiteralExpression literal)
        {
            return Str(literal.Expression);
        }

        public LiteralExpression Str(IriExpression iriTerm)
        {
            return Str(iriTerm.Expression);
        }

        private static LiteralExpression Str(ISparqlExpression expression)
        {
            return new LiteralExpression(new StrFunction(expression));
        }

        public LiteralExpression Lang(VariableExpression variable)
        {
            return new LiteralExpression(new LangFunction(variable.Expression));
        }

        public LiteralExpression Lang(LiteralExpression literal)
        {
            return new LiteralExpression(new LangFunction(literal.Expression));
        }

        public IriExpression Datatype<TExpression>(PrimaryExpression<TExpression> literal) where TExpression : ISparqlExpression
        {
            return Datatype(literal.Expression);
        }

        private IriExpression Datatype(ISparqlExpression expression)
        {
            var dataTypeFunction = SparqlVersion == SparqlQuerySyntax.Sparql_1_0
                                       ? new DataTypeFunction(expression)
                                       : new DataType11Function(expression);
            return new IriExpression(dataTypeFunction);
        }

        public BlankNodeExpression BNode()
        {
            return new BlankNodeExpression(new BNodeFunction());
        }

        public BlankNodeExpression BNode(LiteralExpression simpleLiteral)
        {
            return new BlankNodeExpression(new BNodeFunction(simpleLiteral.Expression));
        }

        public BlankNodeExpression BNode(TypedLiteralExpression<string> stringLiteral)
        {
            return new BlankNodeExpression(new BNodeFunction(stringLiteral.Expression));
        }

        private static LiteralExpression StrDt(ISparqlExpression lexicalForm, ISparqlExpression datatypeIri)
        {
            return new LiteralExpression(new StrDtFunction(lexicalForm, datatypeIri));
        }

        public LiteralExpression StrDt(LiteralExpression lexicalForm, IriExpression datatypeIri)
        {
            return StrDt(lexicalForm.Expression, datatypeIri.Expression);
        }

        public LiteralExpression StrDt(LiteralExpression lexicalForm, Uri datatypeIri)
        {
            return StrDt(lexicalForm.Expression, new ConstantTerm(new UriNode(null, datatypeIri)));
        }

        public LiteralExpression StrDt(LiteralExpression lexicalForm, VariableExpression datatypeIri)
        {
            return StrDt(lexicalForm.Expression, datatypeIri.Expression);
        }

        public LiteralExpression StrDt(string lexicalForm, IriExpression datatypeIri)
        {
            return StrDt(lexicalForm.ToConstantTerm(), datatypeIri.Expression);
        }

        public LiteralExpression StrDt(string lexicalForm, VariableExpression datatypeIri)
        {
            return StrDt(lexicalForm.ToConstantTerm(), datatypeIri.Expression);
        }

        public LiteralExpression StrDt(string lexicalForm, Uri datatypeIri)
        {
            return StrDt(lexicalForm.ToConstantTerm(), new ConstantTerm(new UriNode(null, datatypeIri)));
        }

        public LiteralExpression StrDt(VariableExpression lexicalForm, Uri datatypeIri)
        {
            return StrDt(lexicalForm.Expression, new ConstantTerm(new UriNode(null, datatypeIri)));
        }

        public LiteralExpression StrDt(VariableExpression lexicalForm, VariableExpression datatypeIri)
        {
            return StrDt(lexicalForm.Expression, datatypeIri.Expression);
        }

        public LiteralExpression StrDt(VariableExpression lexicalForm, IriExpression datatypeIri)
        {
            return StrDt(lexicalForm.Expression, datatypeIri.Expression);
        }

        public IriExpression UUID()
        {
            return new IriExpression(new UUIDFunction());
        }

        public LiteralExpression StrUUID()
        {
            return new LiteralExpression(new StrUUIDFunction());
        }
    }
}