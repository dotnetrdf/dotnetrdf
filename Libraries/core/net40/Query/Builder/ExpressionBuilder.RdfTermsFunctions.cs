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
    /// <summary>
    /// Provides methods for creating SPARQL functions, which operate on RDF Terms
    /// </summary>
    public partial class ExpressionBuilder
    {
        /// <summary>
        /// Creates a call to the isIRI function with an expression parameter
        /// </summary>
        /// <param name="term">any SPARQL expression</param>
        public BooleanExpression IsIRI(SparqlExpression term)
        {
            var isIri = new IsIriFunction(term.Expression);
            return new BooleanExpression(isIri);
        }

        /// <summary>
        /// Creates a call to the isIRI function with a variable parameter
        /// </summary>
        /// <param name="variableName">name of variable to check</param>
        public BooleanExpression IsIRI(string variableName)
        {
            return IsIRI(Variable(variableName));
        }

        /// <summary>
        /// Creates a call to the isBlank function with an expression parameter
        /// </summary>
        /// <param name="term">any SPARQL expression</param>
        public BooleanExpression IsBlank(SparqlExpression term)
        {
            var isBlank = new IsBlankFunction(term.Expression);
            return new BooleanExpression(isBlank);
        }

        /// <summary>
        /// Creates a call to the isBlank function with a variable parameter
        /// </summary>
        /// <param name="variableName">name of variable to check</param>
        public BooleanExpression IsBlank(string variableName)
        {
            return IsBlank(Variable(variableName));
        }

        /// <summary>
        /// Creates a call to the isLiteral function with an expression parameter
        /// </summary>
        /// <param name="term">any SPARQL expression</param>
        public BooleanExpression IsLiteral(SparqlExpression term)
        {
            var isLiteral = new IsLiteralFunction(term.Expression);
            return new BooleanExpression(isLiteral);
        }

        /// <summary>
        /// Creates a call to the isLiteral function with a variable parameter
        /// </summary>
        /// <param name="variableName">name of variable to check</param>
        public BooleanExpression IsLiteral(string variableName)
        {
            return IsLiteral(Variable(variableName));
        }

        /// <summary>
        /// Creates a call to the isNumeric function with an expression parameter
        /// </summary>
        /// <param name="term">any SPARQL expression</param>
        public BooleanExpression IsNumeric(SparqlExpression term)
        {
            var isNumeric = new IsNumericFunction(term.Expression);
            return new BooleanExpression(isNumeric);
        }

        /// <summary>
        /// Creates a call to the isNumeric function with a variable parameter
        /// </summary>
        /// <param name="variableName">name of variable to check</param>
        public BooleanExpression IsNumeric(string variableName)
        {
            return IsNumeric(Variable(variableName));
        }

        /// <summary>
        /// Creates a call to the STR function with a variable parameter
        /// </summary>
        /// <param name="variable">a SPARQL variable</param>
        public LiteralExpression Str(VariableExpression variable)
        {
            return Str(variable.Expression);
        }

        /// <summary>
        /// Creates a call to the STR function with a literal expression parameter
        /// </summary>
        /// <param name="literal">a SPARQL literal expression</param>
        public LiteralExpression Str(LiteralExpression literal)
        {
            return Str(literal.Expression);
        }

        /// <summary>
        /// Creates a call to the STR function with an variable parameter
        /// </summary>
        /// <param name="iriTerm">an RDF IRI term</param>
        public LiteralExpression Str(IriExpression iriTerm)
        {
            return Str(iriTerm.Expression);
        }

        private static LiteralExpression Str(ISparqlExpression expression)
        {
            return new LiteralExpression(new StrFunction(expression));
        }

        /// <summary>
        /// Creates a call to the LANG function with a variable parameter
        /// </summary>
        /// <param name="variable">a SPARQL variable</param>
        public LiteralExpression Lang(VariableExpression variable)
        {
            return new LiteralExpression(new LangFunction(variable.Expression));
        }

        /// <summary>
        /// Creates a call to the LANG function with a literal expression parameter
        /// </summary>
        /// <param name="literal">a SPARQL literal expression</param>
        public LiteralExpression Lang(LiteralExpression literal)
        {
            return new LiteralExpression(new LangFunction(literal.Expression));
        }

        /// <summary>
        /// Creates a call to the DATATYPE function with a literal expression parameter
        /// </summary>
        /// <param name="literal">a SPARQL literal expression</param>
        /// <remarks>depending on <see cref="ExpressionBuilder.SparqlVersion"/> will use a different flavour of datatype function</remarks>
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

        /// <summary>
        /// Creates a parameterless call to the BNODE function
        /// </summary>
        public BlankNodeExpression BNode()
        {
            return new BlankNodeExpression(new BNodeFunction());
        }

        /// <summary>
        /// Creates a call to the BNODE function with a simple literal parameter
        /// </summary>
        /// <param name="simpleLiteral">a SPARQL simple literal</param>
        public BlankNodeExpression BNode(LiteralExpression simpleLiteral)
        {
            return new BlankNodeExpression(new BNodeFunction(simpleLiteral.Expression));
        }

        /// <summary>
        /// Creates a call to the BNODE function with a string literal parameter
        /// </summary>
        /// <param name="stringLiteral">a SPARQL string literal</param>
        public BlankNodeExpression BNode(TypedLiteralExpression<string> stringLiteral)
        {
            return new BlankNodeExpression(new BNodeFunction(stringLiteral.Expression));
        }

        private static LiteralExpression StrDt(ISparqlExpression lexicalForm, ISparqlExpression datatypeIri)
        {
            return new LiteralExpression(new StrDtFunction(lexicalForm, datatypeIri));
        }

        /// <summary>
        /// Creates a call to the STRDT function with a simple literal and a IRI expression parameters
        /// </summary>
        /// <param name="lexicalForm">a SPARQL simple literal</param>
        /// <param name="datatypeIri">datatype IRI</param>
        public LiteralExpression StrDt(LiteralExpression lexicalForm, IriExpression datatypeIri)
        {
            return StrDt(lexicalForm.Expression, datatypeIri.Expression);
        }

        /// <summary>
        /// Creates a call to the STRDT function with a simple literal and a <see cref="Uri"/> parameters
        /// </summary>
        /// <param name="lexicalForm">a SPARQL simple literal</param>
        /// <param name="datatypeIri">datatype IRI</param>
        public LiteralExpression StrDt(LiteralExpression lexicalForm, Uri datatypeIri)
        {
            return StrDt(lexicalForm.Expression, new ConstantTerm(new UriNode(null, datatypeIri)));
        }

        /// <summary>
        /// Creates a call to the STRDT function with a simple literal and a variable parameters
        /// </summary>
        /// <param name="lexicalForm">a SPARQL simple literal</param>
        /// <param name="datatypeIri">datatype IRI</param>
        public LiteralExpression StrDt(LiteralExpression lexicalForm, VariableExpression datatypeIri)
        {
            return StrDt(lexicalForm.Expression, datatypeIri.Expression);
        }

        /// <summary>
        /// Creates a call to the STRDT function with a simple literal and a IRI expression parameters
        /// </summary>
        /// <param name="lexicalForm">a literal</param>
        /// <param name="datatypeIri">datatype IRI</param>
        public LiteralExpression StrDt(string lexicalForm, IriExpression datatypeIri)
        {
            return StrDt(lexicalForm.ToConstantTerm(), datatypeIri.Expression);
        }

        /// <summary>
        /// Creates a call to the STRDT function with a simple literal and a IRI expression parameters
        /// </summary>
        /// <param name="lexicalForm">a literal</param>
        /// <param name="datatypeIri">datatype IRI</param>
        public LiteralExpression StrDt(string lexicalForm, VariableExpression datatypeIri)
        {
            return StrDt(lexicalForm.ToConstantTerm(), datatypeIri.Expression);
        }

        /// <summary>
        /// Creates a call to the STRDT function with a simple literal and a <see cref="Uri"/> parameters
        /// </summary>
        /// <param name="lexicalForm">a literal</param>
        /// <param name="datatypeIri">datatype IRI</param>
        public LiteralExpression StrDt(string lexicalForm, Uri datatypeIri)
        {
            return StrDt(lexicalForm.ToConstantTerm(), new ConstantTerm(new UriNode(null, datatypeIri)));
        }

        /// <summary>
        /// Creates a call to the STRDT function with a variable and a <see cref="Uri"/> parameters
        /// </summary>
        /// <param name="lexicalForm">a literal</param>
        /// <param name="datatypeIri">datatype IRI</param>
        public LiteralExpression StrDt(VariableExpression lexicalForm, Uri datatypeIri)
        {
            return StrDt(lexicalForm.Expression, new ConstantTerm(new UriNode(null, datatypeIri)));
        }

        /// <summary>
        /// Creates a call to the STRDT function with a variable and a <see cref="Uri"/> parameters
        /// </summary>
        /// <param name="lexicalForm">a literal</param>
        /// <param name="datatypeIri">datatype IRI</param>
        public LiteralExpression StrDt(VariableExpression lexicalForm, VariableExpression datatypeIri)
        {
            return StrDt(lexicalForm.Expression, datatypeIri.Expression);
        }

        /// <summary>
        /// Creates a call to the STRDT function with a variable and a IRI expression parameters
        /// </summary>
        /// <param name="lexicalForm">a literal</param>
        /// <param name="datatypeIri">datatype IRI</param>
        public LiteralExpression StrDt(VariableExpression lexicalForm, IriExpression datatypeIri)
        {
            return StrDt(lexicalForm.Expression, datatypeIri.Expression);
        }

        /// <summary>
        /// Creates a call to the UUID function
        /// </summary>
        public IriExpression UUID()
        {
            return new IriExpression(new UUIDFunction());
        }

        /// <summary>
        /// Creates a call to the StrUUID function
        /// </summary>
        public LiteralExpression StrUUID()
        {
            return new LiteralExpression(new StrUUIDFunction());
        }
    }
}