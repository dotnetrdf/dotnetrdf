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

namespace VDS.RDF.Query.Builder
{
    /// <summary>
    /// Provides methods for building SPARQL expressions, but not aggregates
    /// </summary>
    public interface INonAggregateExpressionBuilder
    {
        /// <summary>
        /// Creates a call to the REGEX function
        /// </summary>
        BooleanExpression Regex(VariableExpression text, string pattern);

        /// <summary>
        /// Creates a call to the REGEX function
        /// </summary>
        BooleanExpression Regex(VariableExpression text, VariableExpression pattern);

        /// <summary>
        /// Creates a call to the REGEX function
        /// </summary>
        BooleanExpression Regex(LiteralExpression text, string pattern);

        /// <summary>
        /// Creates a call to the REGEX function
        /// </summary>
        BooleanExpression Regex(LiteralExpression text, LiteralExpression pattern);

        /// <summary>
        /// Creates a call to the REGEX function
        /// </summary>
        BooleanExpression Regex(LiteralExpression text, VariableExpression pattern);

        /// <summary>
        /// Creates a call to the REGEX function
        /// </summary>
        BooleanExpression Regex(SparqlExpression text, string pattern, string flags);

        /// <summary>
        /// Creates a call to the REGEX function
        /// </summary>
        BooleanExpression Regex(VariableExpression text, VariableExpression pattern, string flags);

        /// <summary>
        /// Creates a call to the REGEX function
        /// </summary>
        BooleanExpression Regex(LiteralExpression text, string pattern, string flags);

        /// <summary>
        /// Creates a call to the REGEX function
        /// </summary>
        BooleanExpression Regex(LiteralExpression text, LiteralExpression pattern, string flags);

        /// <summary>
        /// Creates a call to the REGEX function
        /// </summary>
        BooleanExpression Regex(LiteralExpression text, VariableExpression pattern, string flags);

        /// <summary>
        /// Creates a call to the STRLEN function with a variable parameter
        /// </summary>
        /// <param name="str">a SPARQL variable</param>
        NumericExpression<int> StrLen(VariableExpression str);

        /// <summary>
        /// Creates a call to the STRLEN function with a string literal parameter
        /// </summary>
        /// <param name="str">a string literal parameter</param>
        NumericExpression<int> StrLen(TypedLiteralExpression<string> str);

        /// <summary>
        /// Creates a call to the SUBSTR function with a string literal and variable parameters
        /// </summary>
        /// <param name="str">a string literal parameter</param>
        /// <param name="startingLoc">1-based start index</param>
        TypedLiteralExpression<string> Substr(TypedLiteralExpression<string> str, NumericExpression<int> startingLoc);

        /// <summary>
        /// Creates a call to the SUBSTR function with a string literal and interger expression parameters
        /// </summary>
        /// <param name="str">a string literal parameter</param>
        /// <param name="startingLoc">a SPARQL variable</param>
        TypedLiteralExpression<string> Substr(TypedLiteralExpression<string> str, VariableExpression startingLoc);

        /// <summary>
        /// Creates a call to the SUBSTR function with a string literal and interger parameters
        /// </summary>
        /// <param name="str">a string literal parameter</param>
        /// <param name="startingLoc">1-based start index</param>
        TypedLiteralExpression<string> Substr(TypedLiteralExpression<string> str, int startingLoc);

        /// <summary>
        /// Creates a call to the SUBSTR function with a variable and interger expression parameters
        /// </summary>
        /// <param name="str">a SPARQL variable</param>
        /// <param name="startingLoc">1-based start index</param>
        TypedLiteralExpression<string> Substr(VariableExpression str, NumericExpression<int> startingLoc);

        /// <summary>
        /// Creates a call to the SUBSTR function with a variable and interger parameters
        /// </summary>
        /// <param name="str">a SPARQL variable</param>
        /// <param name="startingLoc">1-based start index</param>
        TypedLiteralExpression<string> Substr(VariableExpression str, int startingLoc);

        /// <summary>
        /// Creates a call to the SUBSTR function with two variable parameters
        /// </summary>
        /// <param name="str">a SPARQL variable</param>
        /// <param name="startingLoc">a SPARQL variable</param>
        TypedLiteralExpression<string> Substr(VariableExpression str, VariableExpression startingLoc);

        /// <summary>
        /// Creates a call to the SUBSTR function with a string literal and variable parameters
        /// </summary>
        /// <param name="str">a string literal parameter</param>
        /// <param name="startingLoc">1-based start index</param>
        /// <param name="length">substring length </param>
        TypedLiteralExpression<string> Substr(TypedLiteralExpression<string> str, NumericExpression<int> startingLoc, int length);

        /// <summary>
        /// Creates a call to the SUBSTR function with a string literal and interger expression parameters
        /// </summary>
        /// <param name="str">a string literal parameter</param>
        /// <param name="startingLoc">a SPARQL variable</param>
        /// <param name="length">substring length </param>
        TypedLiteralExpression<string> Substr(TypedLiteralExpression<string> str, VariableExpression startingLoc, int length);

        /// <summary>
        /// Creates a call to the SUBSTR function with a string literal and interger parameters
        /// </summary>
        /// <param name="str">a string literal parameter</param>
        /// <param name="startingLoc">1-based start index</param>
        /// <param name="length">substring length </param>
        TypedLiteralExpression<string> Substr(TypedLiteralExpression<string> str, int startingLoc, int length);

        /// <summary>
        /// Creates a call to the SUBSTR function with a variable and interger expression parameters
        /// </summary>
        /// <param name="str">a SPARQL variable</param>
        /// <param name="startingLoc">1-based start index</param>
        /// <param name="length">substring length </param>
        TypedLiteralExpression<string> Substr(VariableExpression str, NumericExpression<int> startingLoc, int length);

        /// <summary>
        /// Creates a call to the SUBSTR function with a variable and interger parameters
        /// </summary>
        /// <param name="str">a SPARQL variable</param>
        /// <param name="startingLoc">1-based start index</param>
        /// <param name="length">substring length </param>
        TypedLiteralExpression<string> Substr(VariableExpression str, int startingLoc, int length);

        /// <summary>
        /// Creates a call to the SUBSTR function with two variable parameters
        /// </summary>
        /// <param name="str">a SPARQL variable</param>
        /// <param name="startingLoc">a SPARQL variable</param>
        /// <param name="length">substring length </param>
        TypedLiteralExpression<string> Substr(VariableExpression str, VariableExpression startingLoc, int length);

        /// <summary>
        /// Creates a call to the SUBSTR function with a string literal and two integer expressions parameters
        /// </summary>
        /// <param name="str">a string literal parameter</param>
        /// <param name="startingLoc">1-based start index</param>
        /// <param name="length">substring length </param>
        TypedLiteralExpression<string> Substr(TypedLiteralExpression<string> str, NumericExpression<int> startingLoc, NumericExpression<int> length);

        /// <summary>
        /// Creates a call to the SUBSTR function with a string literal, variable and interger expression parameters
        /// </summary>
        /// <param name="str">a string literal parameter</param>
        /// <param name="startingLoc">a SPARQL variable</param>
        /// <param name="length">substring length </param>
        TypedLiteralExpression<string> Substr(TypedLiteralExpression<string> str, VariableExpression startingLoc, NumericExpression<int> length);

        /// <summary>
        /// Creates a call to the SUBSTR function with a string literal, interger and integer expression parameters
        /// </summary>
        /// <param name="str">a string literal parameter</param>
        /// <param name="startingLoc">1-based start index</param>
        /// <param name="length">substring length </param>
        TypedLiteralExpression<string> Substr(TypedLiteralExpression<string> str, int startingLoc, NumericExpression<int> length);

        /// <summary>
        /// Creates a call to the SUBSTR function with a variable, interger expression and integer expression parameters
        /// </summary>
        /// <param name="str">a SPARQL variable</param>
        /// <param name="startingLoc">1-based start index</param>
        /// <param name="length">substring length </param>
        TypedLiteralExpression<string> Substr(VariableExpression str, NumericExpression<int> startingLoc, NumericExpression<int> length);

        /// <summary>
        /// Creates a call to the SUBSTR function with a variable, interger and a numeric expression parameters
        /// </summary>
        /// <param name="str">a SPARQL variable</param>
        /// <param name="startingLoc">1-based start index</param>
        /// <param name="length">substring length </param>
        TypedLiteralExpression<string> Substr(VariableExpression str, int startingLoc, NumericExpression<int> length);

        /// <summary>
        /// Creates a call to the SUBSTR function with two variable parameters
        /// </summary>
        /// <param name="str">a SPARQL variable</param>
        /// <param name="startingLoc">a SPARQL variable</param>
        /// <param name="length">substring length </param>
        TypedLiteralExpression<string> Substr(VariableExpression str, VariableExpression startingLoc, NumericExpression<int> length);

        /// <summary>
        /// Creates a call to the SUBSTR function with a string literal, interger expression and a numeric expression parameters
        /// </summary>
        /// <param name="str">a string literal parameter</param>
        /// <param name="startingLoc">1-based start index</param>
        /// <param name="length">substring length </param>
        TypedLiteralExpression<string> Substr(TypedLiteralExpression<string> str, NumericExpression<int> startingLoc, VariableExpression length);

        /// <summary>
        /// Creates a call to the SUBSTR function with a string literal, interger expression and a variable parameters
        /// </summary>
        /// <param name="str">a string literal parameter</param>
        /// <param name="startingLoc">a SPARQL variable</param>
        /// <param name="length">substring length </param>
        TypedLiteralExpression<string> Substr(TypedLiteralExpression<string> str, VariableExpression startingLoc, VariableExpression length);

        /// <summary>
        /// Creates a call to the SUBSTR function with a string literal, interger and a variable parameters
        /// </summary>
        /// <param name="str">a string literal parameter</param>
        /// <param name="startingLoc">1-based start index</param>
        /// <param name="length">substring length </param>
        TypedLiteralExpression<string> Substr(TypedLiteralExpression<string> str, int startingLoc, VariableExpression length);

        /// <summary>
        /// Creates a call to the SUBSTR function with a variable, interger expression and a variable parameters
        /// </summary>
        /// <param name="str">a SPARQL variable</param>
        /// <param name="startingLoc">1-based start index</param>
        /// <param name="length">substring length </param>
        TypedLiteralExpression<string> Substr(VariableExpression str, NumericExpression<int> startingLoc, VariableExpression length);

        /// <summary>
        /// Creates a call to the SUBSTR function with a variable, interger and a variable parameters
        /// </summary>
        /// <param name="str">a SPARQL variable</param>
        /// <param name="startingLoc">1-based start index</param>
        /// <param name="length">substring length </param>
        TypedLiteralExpression<string> Substr(VariableExpression str, int startingLoc, VariableExpression length);

        /// <summary>
        /// Creates a call to the SUBSTR function with three variable parameters
        /// </summary>
        /// <param name="str">a SPARQL variable</param>
        /// <param name="startingLoc">a SPARQL variable</param>
        /// <param name="length">substring length </param>
        TypedLiteralExpression<string> Substr(VariableExpression str, VariableExpression startingLoc, VariableExpression length);

        /// <summary>
        /// Creates a call to the LANGMATCHES function
        /// </summary>
        BooleanExpression LangMatches(LiteralExpression languageTag, string languageRange);

        /// <summary>
        /// Creates a call to the LANGMATCHES function
        /// </summary>
        BooleanExpression LangMatches(VariableExpression languageTag, string languageRange);

        /// <summary>
        /// Creates a call to the LANGMATCHES function
        /// </summary>
        BooleanExpression LangMatches(LiteralExpression languageTag, LiteralExpression languageRange);

        /// <summary>
        /// Creates a call to the LANGMATCHES function
        /// </summary>
        BooleanExpression LangMatches(VariableExpression languageTag, LiteralExpression languageRange);

        /// <summary>
        /// Creates a call to the LANGMATCHES function
        /// </summary>
        BooleanExpression LangMatches(LiteralExpression languageTag, VariableExpression languageRange);

        /// <summary>
        /// Creates a call to the LANGMATCHES function
        /// </summary>
        BooleanExpression LangMatches(VariableExpression languageTag, VariableExpression languageRange);

        /// <summary>
        /// Creates a call to the isIRI function with an expression parameter
        /// </summary>
        /// <param name="term">any SPARQL expression</param>
        BooleanExpression IsIRI(SparqlExpression term);

        /// <summary>
        /// Creates a call to the isIRI function with a variable parameter
        /// </summary>
        /// <param name="variableName">name of variable to check</param>
        BooleanExpression IsIRI(string variableName);

        /// <summary>
        /// Creates a call to the isBlank function with an expression parameter
        /// </summary>
        /// <param name="term">any SPARQL expression</param>
        BooleanExpression IsBlank(SparqlExpression term);

        /// <summary>
        /// Creates a call to the isBlank function with a variable parameter
        /// </summary>
        /// <param name="variableName">name of variable to check</param>
        BooleanExpression IsBlank(string variableName);

        /// <summary>
        /// Creates a call to the isLiteral function with an expression parameter
        /// </summary>
        /// <param name="term">any SPARQL expression</param>
        BooleanExpression IsLiteral(SparqlExpression term);

        /// <summary>
        /// Creates a call to the isLiteral function with a variable parameter
        /// </summary>
        /// <param name="variableName">name of variable to check</param>
        BooleanExpression IsLiteral(string variableName);

        /// <summary>
        /// Creates a call to the isNumeric function with an expression parameter
        /// </summary>
        /// <param name="term">any SPARQL expression</param>
        BooleanExpression IsNumeric(SparqlExpression term);

        /// <summary>
        /// Creates a call to the isNumeric function with a variable parameter
        /// </summary>
        /// <param name="variableName">name of variable to check</param>
        BooleanExpression IsNumeric(string variableName);

        /// <summary>
        /// Creates a call to the STR function with a variable parameter
        /// </summary>
        /// <param name="variable">a SPARQL variable</param>
        LiteralExpression Str(VariableExpression variable);

        /// <summary>
        /// Creates a call to the STR function with a literal expression parameter
        /// </summary>
        /// <param name="literal">a SPARQL literal expression</param>
        LiteralExpression Str(LiteralExpression literal);

        /// <summary>
        /// Creates a call to the STR function with an variable parameter
        /// </summary>
        /// <param name="iriTerm">an RDF IRI term</param>
        LiteralExpression Str(IriExpression iriTerm);

        /// <summary>
        /// Creates a call to the LANG function with a variable parameter
        /// </summary>
        /// <param name="variable">a SPARQL variable</param>
        LiteralExpression Lang(VariableExpression variable);

        /// <summary>
        /// Creates a call to the LANG function with a literal expression parameter
        /// </summary>
        /// <param name="literal">a SPARQL literal expression</param>
        LiteralExpression Lang(LiteralExpression literal);

        /// <summary>
        /// Creates a call to the DATATYPE function with a literal expression parameter
        /// </summary>
        /// <param name="literal">a SPARQL literal expression</param>
        /// <remarks>depending on <see cref="ExpressionBuilder.SparqlVersion"/> will use a different flavour of datatype function</remarks>
        IriExpression Datatype<TExpression>(PrimaryExpression<TExpression> literal) where TExpression : ISparqlExpression;

        /// <summary>
        /// Creates a parameterless call to the BNODE function
        /// </summary>
        BlankNodeExpression BNode();

        /// <summary>
        /// Creates a call to the BNODE function with a simple literal parameter
        /// </summary>
        /// <param name="simpleLiteral">a SPARQL simple literal</param>
        BlankNodeExpression BNode(LiteralExpression simpleLiteral);

        /// <summary>
        /// Creates a call to the BNODE function with a string literal parameter
        /// </summary>
        /// <param name="stringLiteral">a SPARQL string literal</param>
        BlankNodeExpression BNode(TypedLiteralExpression<string> stringLiteral);

        /// <summary>
        /// Creates a call to the STRDT function with a simple literal and a IRI expression parameters
        /// </summary>
        /// <param name="lexicalForm">a SPARQL simple literal</param>
        /// <param name="datatypeIri">datatype IRI</param>
        LiteralExpression StrDt(LiteralExpression lexicalForm, IriExpression datatypeIri);

        /// <summary>
        /// Creates a call to the STRDT function with a simple literal and a <see cref="Uri"/> parameters
        /// </summary>
        /// <param name="lexicalForm">a SPARQL simple literal</param>
        /// <param name="datatypeIri">datatype IRI</param>
        LiteralExpression StrDt(LiteralExpression lexicalForm, Uri datatypeIri);

        /// <summary>
        /// Creates a call to the STRDT function with a simple literal and a variable parameters
        /// </summary>
        /// <param name="lexicalForm">a SPARQL simple literal</param>
        /// <param name="datatypeIri">datatype IRI</param>
        LiteralExpression StrDt(LiteralExpression lexicalForm, VariableExpression datatypeIri);

        /// <summary>
        /// Creates a call to the STRDT function with a simple literal and a IRI expression parameters
        /// </summary>
        /// <param name="lexicalForm">a literal</param>
        /// <param name="datatypeIri">datatype IRI</param>
        LiteralExpression StrDt(string lexicalForm, IriExpression datatypeIri);

        /// <summary>
        /// Creates a call to the STRDT function with a simple literal and a IRI expression parameters
        /// </summary>
        /// <param name="lexicalForm">a literal</param>
        /// <param name="datatypeIri">datatype IRI</param>
        LiteralExpression StrDt(string lexicalForm, VariableExpression datatypeIri);

        /// <summary>
        /// Creates a call to the STRDT function with a simple literal and a <see cref="Uri"/> parameters
        /// </summary>
        /// <param name="lexicalForm">a literal</param>
        /// <param name="datatypeIri">datatype IRI</param>
        LiteralExpression StrDt(string lexicalForm, Uri datatypeIri);

        /// <summary>
        /// Creates a call to the STRDT function with a variable and a <see cref="Uri"/> parameters
        /// </summary>
        /// <param name="lexicalForm">a literal</param>
        /// <param name="datatypeIri">datatype IRI</param>
        LiteralExpression StrDt(VariableExpression lexicalForm, Uri datatypeIri);

        /// <summary>
        /// Creates a call to the STRDT function with a variable and a <see cref="Uri"/> parameters
        /// </summary>
        /// <param name="lexicalForm">a literal</param>
        /// <param name="datatypeIri">datatype IRI</param>
        LiteralExpression StrDt(VariableExpression lexicalForm, VariableExpression datatypeIri);

        /// <summary>
        /// Creates a call to the STRDT function with a variable and a IRI expression parameters
        /// </summary>
        /// <param name="lexicalForm">a literal</param>
        /// <param name="datatypeIri">datatype IRI</param>
        LiteralExpression StrDt(VariableExpression lexicalForm, IriExpression datatypeIri);

        /// <summary>
        /// Creates a call to the UUID function
        /// </summary>
        IriExpression UUID();

        /// <summary>
        /// Creates a call to the StrUUID function
        /// </summary>
        LiteralExpression StrUUID();

        /// <summary>
        /// Creates a call to the BOUND function with a variable parameter
        /// </summary>
        /// <param name="var">a SPARQL variable</param>
        BooleanExpression Bound(VariableExpression var);

        /// <summary>
        /// Creates a call to the BOUND function with a variable parameter
        /// </summary>
        /// <param name="var">a SPARQL variable name</param>
        BooleanExpression Bound(string var);

        /// <summary>
        /// Creates a call to the IF function with an expression for the first parameter
        /// </summary>
        /// <param name="ifExpression">conditional clause expression</param>
        IfThenPart If(BooleanExpression ifExpression);

        /// <summary>
        /// Creates a call to the IF function with a variable for the first parameter
        /// </summary>
        /// <param name="ifExpression">conditional clause variable expression</param>
        IfThenPart If(VariableExpression ifExpression);

        /// <summary>
        /// Creates a call of the COALESCE function with a variable number of expression parameters
        /// </summary>
        /// <param name="expressions">SPARQL expressions</param>
        RdfTermExpression Coalesce(params SparqlExpression[] expressions);

        /// <summary>
        /// Creates a call of the EXISTS function
        /// </summary>
        /// <param name="buildExistsPattern">a function, which will create the graph pattern parameter</param>
        BooleanExpression Exists(Action<IGraphPatternBuilder> buildExistsPattern);

        /// <summary>
        /// Creates a call of the SAMETERM function with two expression parameters
        /// </summary>
        /// <param name="left">a SPARQL expression</param>
        /// <param name="right">a SPARQL expression</param>
        BooleanExpression SameTerm(SparqlExpression left, SparqlExpression right);

        /// <summary>
        /// Creates a call of the SAMETERM function with variable and expression parameters
        /// </summary>
        /// <param name="left">a variable name</param>
        /// <param name="right">a SPARQL expression</param>
        BooleanExpression SameTerm(string left, SparqlExpression right);

        /// <summary>
        /// Creates a call of the SAMETERM function with expression and variable parameters
        /// </summary>
        /// <param name="left">a SPARQL expression</param>
        /// <param name="right">a variable name</param>
        BooleanExpression SameTerm(SparqlExpression left, string right);

        /// <summary>
        /// Creates a call of the SAMETERM function with two variable parameters
        /// </summary>
        /// <param name="left">a variable name</param>
        /// <param name="right">a variable name</param>
        BooleanExpression SameTerm(string left, string right);

        /// <summary>
        /// SPARQL syntax verions to use when creating expressions
        /// </summary>
        SparqlQuerySyntax SparqlVersion { get; set; }

        /// <summary>
        /// Creates a SPARQL variable
        /// </summary>
        VariableExpression Variable(string variable);

        /// <summary>
        /// Creates a string constant 
        /// </summary>
        TypedLiteralExpression<string> Constant(string value);

        /// <summary>
        /// Creates a numeric constant 
        /// </summary>
        NumericExpression<int> Constant(int value);

        /// <summary>
        /// Creates a numeric constant 
        /// </summary>
        NumericExpression<decimal> Constant(decimal value);

        /// <summary>
        /// Creates a numeric constant 
        /// </summary>
        NumericExpression<float> Constant(float value);

        /// <summary>
        /// Creates a numeric constant 
        /// </summary>
        NumericExpression<double> Constant(double value);

        /// <summary>
        /// Creates a boolean constant 
        /// </summary>
        TypedLiteralExpression<bool> Constant(bool value);

        /// <summary>
        /// Creates a numeric constant 
        /// </summary>
        NumericExpression<byte> Constant(byte value);

        /// <summary>
        /// Creates a numeric constant 
        /// </summary>
        NumericExpression<sbyte> Constant(sbyte value);

        /// <summary>
        /// Creates a numeric constant 
        /// </summary>
        NumericExpression<short> Constant(short value);

        /// <summary>
        /// Creates a datetime constant 
        /// </summary>
        TypedLiteralExpression<DateTime> Constant(DateTime value);

        /// <summary>
        /// Creates an IRI constant 
        /// </summary>
        RdfTermExpression Constant(Uri value);

        /// <summary>
        /// Builds a SPARQL constructor function call
        /// </summary>
        SparqlCastBuilder Cast(SparqlExpression castedExpression);
    }
}