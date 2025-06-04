/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2025 dotNetRDF Project (http://dotnetrdf.org/)
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

using VDS.RDF.Query.Expressions.Arithmetic;
using VDS.RDF.Query.Expressions.Comparison;
using VDS.RDF.Query.Expressions.Conditional;
using VDS.RDF.Query.Expressions.Functions;
using VDS.RDF.Query.Expressions.Functions.Arq;
using VDS.RDF.Query.Expressions.Functions.Leviathan.Numeric;
using VDS.RDF.Query.Expressions.Functions.Leviathan.Numeric.Trigonometry;
using VDS.RDF.Query.Expressions.Functions.Sparql;
using VDS.RDF.Query.Expressions.Functions.Sparql.Boolean;
using VDS.RDF.Query.Expressions.Functions.Sparql.Constructor;
using VDS.RDF.Query.Expressions.Functions.Sparql.DateTime;
using VDS.RDF.Query.Expressions.Functions.Sparql.Hash;
using VDS.RDF.Query.Expressions.Functions.Sparql.Numeric;
using VDS.RDF.Query.Expressions.Functions.Sparql.Set;
using VDS.RDF.Query.Expressions.Functions.Sparql.String;
using VDS.RDF.Query.Expressions.Functions.Sparql.TripleNode;
using VDS.RDF.Query.Expressions.Functions.XPath;
using VDS.RDF.Query.Expressions.Functions.XPath.Cast;
using VDS.RDF.Query.Expressions.Functions.XPath.DateTime;
using VDS.RDF.Query.Expressions.Functions.XPath.String;
using VDS.RDF.Query.Expressions.Primary;
using XPath=VDS.RDF.Query.Expressions.Functions.XPath; 
using AbsFunction = VDS.RDF.Query.Expressions.Functions.Sparql.Numeric.AbsFunction;
using BNodeFunction = VDS.RDF.Query.Expressions.Functions.Arq.BNodeFunction;
using ConcatFunction = VDS.RDF.Query.Expressions.Functions.Sparql.String.ConcatFunction;
using ContainsFunction = VDS.RDF.Query.Expressions.Functions.Sparql.String.ContainsFunction;
using EFunction = VDS.RDF.Query.Expressions.Functions.Arq.EFunction;
using EncodeForUriFunction = VDS.RDF.Query.Expressions.Functions.Sparql.String.EncodeForUriFunction;
using FloorFunction = VDS.RDF.Query.Expressions.Functions.Sparql.Numeric.FloorFunction;
using NowFunction = VDS.RDF.Query.Expressions.Functions.Arq.NowFunction;
using ReplaceFunction = VDS.RDF.Query.Expressions.Functions.Sparql.String.ReplaceFunction;
using RoundFunction = VDS.RDF.Query.Expressions.Functions.Sparql.Numeric.RoundFunction;
using SubstringFunction = VDS.RDF.Query.Expressions.Functions.Arq.SubstringFunction;

namespace VDS.RDF.Query;

/// <summary>
/// The interface to be implemented by a class that can process SPARQL expressions.
/// </summary>
/// <typeparam name="TResult"></typeparam>
/// <typeparam name="TContext"></typeparam>
/// <typeparam name="TBinding"></typeparam>
public interface ISparqlExpressionProcessor<out TResult, in TContext, in TBinding>
{

    // Primary
    /// <summary>
    /// Process an AggregateTerm.
    /// </summary>
    /// <param name="aggregate"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessAggregateTerm(AggregateTerm aggregate, TContext context, TBinding binding);
    /// <summary>
    /// Process an AllModifier.
    /// </summary>
    /// <param name="all"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessAllModifier(AllModifier all, TContext context, TBinding binding);
    /// <summary>
    /// Process a <see cref="ConstantTerm"/>.
    /// </summary>
    /// <param name="constant"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessConstantTerm(ConstantTerm constant, TContext context, TBinding binding);
    /// <summary>
    /// Process a <see cref="DistinctModifier"/>.
    /// </summary>
    /// <param name="distinct"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessDistinctModifier(DistinctModifier distinct, TContext context, TBinding binding);
    /// <summary>
    /// Process a <see cref="GraphPatternTerm"/>.
    /// </summary>
    /// <param name="graphPattern"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessGraphPatternTerm(GraphPatternTerm graphPattern, TContext context, TBinding binding);
    /// <summary>
    /// Process a <see cref="VariableTerm"/>.
    /// </summary>
    /// <param name="variable"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessVariableTerm(VariableTerm variable, TContext context, TBinding binding);
    /// <summary>
    /// Process a <see cref="TripleNodeTerm"/>.
    /// </summary>
    /// <param name="tripleNodeTerm"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessTripleNodeTerm(TripleNodeTerm tripleNodeTerm, TContext context, TBinding binding);
    /// <summary>
    /// Process an <see cref="AdditionExpression"/>.
    /// </summary>
    /// <param name="addition"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessAdditionExpression(AdditionExpression addition, TContext context, TBinding binding);

    /// <summary>
    /// Process a <see cref="DivisionExpression"/>.
    /// </summary>
    /// <param name="division"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessDivisionExpression(DivisionExpression division, TContext context, TBinding binding);

    /// <summary>
    /// Process a <see cref="MinusExpression"/>.
    /// </summary>
    /// <param name="minus"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessMinusExpression(MinusExpression minus, TContext context, TBinding binding);

    /// <summary>
    /// Process a <see cref="MultiplicationExpression"/>.
    /// </summary>
    /// <param name="multiplication"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessMultiplicationExpression(MultiplicationExpression multiplication, TContext context,
        TBinding binding);

    /// <summary>
    /// Process a <see cref="SubtractionExpression"/>.
    /// </summary>
    /// <param name="subtraction"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessSubtractionExpression(SubtractionExpression subtraction, TContext context, TBinding binding);

    /// <summary>
    /// Process a <see cref="EqualsExpression"/>.
    /// </summary>
    /// <param name="equals"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessEqualsExpression(EqualsExpression equals, TContext context, TBinding binding);
    
    /// <summary>
    /// Process a <see cref="GreaterThanExpression"/>.
    /// </summary>
    /// <param name="gt"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessGreaterThanExpression(GreaterThanExpression gt, TContext context, TBinding binding);

    /// <summary>
    /// Process a <see cref="GreaterThanOrEqualToExpression"/>.
    /// </summary>
    /// <param name="gte"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessGreaterThanOrEqualToExpression(GreaterThanOrEqualToExpression gte, TContext context,
        TBinding binding);

    /// <summary>
    /// Process a <see cref="LessThanExpression"/>.
    /// </summary>
    /// <param name="lt"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessLessThanExpression(LessThanExpression lt, TContext context, TBinding binding);

    /// <summary>
    /// Process a <see cref="LessThanOrEqualToExpression"/>.
    /// </summary>
    /// <param name="lte"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessLessThanOrEqualToExpression(LessThanOrEqualToExpression lte, TContext context,
        TBinding binding);

    /// <summary>
    /// Process <see cref="NotEqualsExpression"/>.
    /// </summary>
    /// <param name="ne"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessNotEqualsExpression(NotEqualsExpression ne, TContext context, TBinding binding);

    /// <summary>
    /// Process an <see cref="AndExpression"/>.
    /// </summary>
    /// <param name="and"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessAndExpression(AndExpression and, TContext context, TBinding binding);
    /// <summary>
    /// Process a <see cref="NotExpression"/>.
    /// </summary>
    /// <param name="not"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessNotExpression(NotExpression not, TContext context, TBinding binding);

    /// <summary>
    /// Process an <see cref="OrExpression"/>.
    /// </summary>
    /// <param name="or"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessOrExpression(OrExpression or, TContext context, TBinding binding);

    // ARQ Functions
    /// <summary>
    /// Process a <see cref="BNodeFunction"/> .
    /// </summary>
    /// <param name="bNode"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessArqBNodeFunction(BNodeFunction bNode, TContext context, TBinding binding);
    /// <summary>
    /// Process an <see cref="EFunction"/>.
    /// </summary>
    /// <param name="e"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessEFunction(EFunction e, TContext context, TBinding binding);
    /// <summary>
    /// Process a <see cref="LocalNameFunction"/>.
    /// </summary>
    /// <param name="localName"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessLocalNameFunction(LocalNameFunction localName, TContext context, TBinding binding);
    /// <summary>
    /// Process a <see cref="MaxFunction"/>.
    /// </summary>
    /// <param name="max"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessMaxFunction(MaxFunction max, TContext context, TBinding binding);
    /// <summary>
    /// Process a <see cref="MinFunction"/>.
    /// </summary>
    /// <param name="min"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessMinFunction(MinFunction min, TContext context, TBinding binding);

    /// <summary>
    /// Process a <see cref="NamespaceFunction"/>.
    /// </summary>
    /// <param name="ns"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessNamespaceFunction(NamespaceFunction ns, TContext context, TBinding binding);
    /// <summary>
    /// Process a <see cref="NowFunction"/>.
    /// </summary>
    /// <param name="now"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessNowFunction(NowFunction now, TContext context, TBinding binding);
    /// <summary>
    /// Process a <see cref="PiFunction"/>.
    /// </summary>
    /// <param name="pi"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessPiFunction(PiFunction pi, TContext context, TBinding binding);
    /// <summary>
    /// Process a <see cref="Sha1Function"/>.
    /// </summary>
    /// <param name="sha1"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessSha1Function(Sha1Function sha1, TContext context, TBinding binding);
    /// <summary>
    /// Process a <see cref="StringJoinFunction"/>.
    /// </summary>
    /// <param name="stringJoin"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessStringJoinFunction(StringJoinFunction stringJoin, TContext context, TBinding binding);
    /// <summary>
    /// Process a <see cref="SubstringFunction"/>.
    /// </summary>
    /// <param name="substring"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessSubstringFunction(SubstringFunction substring, TContext context, TBinding binding);

    // Leviathan Functions
    /// <summary>
    /// Process a Leviathan <see cref="Expressions.Functions.Leviathan.Hash.MD5HashFunction"/> function.
    /// </summary>
    /// <param name="md5"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessLeviathanMD5HashFunction(Expressions.Functions.Leviathan.Hash.MD5HashFunction md5, TContext context, TBinding binding);

    /// <summary>
    /// Process a Leviathan <see cref="Expressions.Functions.Leviathan.Hash.Sha256HashFunction"/> function.
    /// </summary>
    /// <param name="sha256"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessLeviathanSha256HashFunction(Expressions.Functions.Leviathan.Hash.Sha256HashFunction sha256, TContext context, TBinding binding);
    
    /// <summary>
    /// Process a <see cref="CosecantFunction"/>.
    /// </summary>
    /// <param name="cosec"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessCosecantFunction(CosecantFunction cosec, TContext context, TBinding binding);
    
    /// <summary>
    /// Process a <see cref="CosineFunction"/>.
    /// </summary>
    /// <param name="cos"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessCosineFunction(CosineFunction cos, TContext context, TBinding binding);
    
    /// <summary>
    /// Process a <see cref="CotangentFunction"/>.
    /// </summary>
    /// <param name="cot"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    
    TResult ProcessCotangentFunction(CotangentFunction cot, TContext context, TBinding binding);
    /// <summary>
    /// Process a <see cref="DegreesToRadiansFunction"/>.
    /// </summary>
    /// <param name="degToRad"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    
    TResult ProcessDegreesToRadiansFunction(DegreesToRadiansFunction degToRad, TContext context, TBinding binding);
    /// <summary>
    /// Process a <see cref="RadiansToDegreesFunction"/>.
    /// </summary>
    /// <param name="radToDeg"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessRadiansToDegreesFunction(RadiansToDegreesFunction radToDeg, TContext context, TBinding binding);
    /// <summary>
    /// Process a <see cref="SecantFunction"/>.
    /// </summary>
    /// <param name="sec"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessSecantFunction(SecantFunction sec, TContext context, TBinding binding);
    /// <summary>
    /// Process a <see cref="SineFunction"/>.
    /// </summary>
    /// <param name="sin"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessSineFunction(SineFunction sin, TContext context, TBinding binding);
    /// <summary>
    /// Process a <see cref="TangentFunction"/>.
    /// </summary>
    /// <param name="tan"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessTangentFunction(TangentFunction tan, TContext context, TBinding binding);

    /// <summary>
    /// Process a <see cref="CartesianFunction"/>.
    /// </summary>
    /// <param name="cart"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    
    TResult ProcessCartesianFunction(CartesianFunction cart, TContext context, TBinding binding);
    /// <summary>
    /// Process a <see cref="CubeFunction"/>.
    /// </summary>
    /// <param name="cube"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessCubeFunction(CubeFunction cube, TContext context, TBinding binding);

    /// <summary>
    /// Process a <see cref="Expressions.Functions.Leviathan.Numeric.EFunction"/>.
    /// </summary>
    /// <param name="eFunction"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessLeviathanEFunction(Expressions.Functions.Leviathan.Numeric.EFunction eFunction, TContext context, TBinding binding);

    /// <summary>
    /// Process a <see cref="FactorialFunction"/>.
    /// </summary>
    /// <param name="factorial"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessFactorialFunction(FactorialFunction factorial, TContext context, TBinding binding);

    /// <summary>
    /// Process a <see cref="LogFunction"/>.
    /// </summary>
    /// <param name="log"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessLogFunction(LogFunction log, TContext context, TBinding binding);

    /// <summary>
    /// Process a <see cref="LeviathanNaturalLogFunction"/>.
    /// </summary>
    /// <param name="logn"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessNaturalLogFunction(LeviathanNaturalLogFunction logn, TContext context, TBinding binding);

    /// <summary>
    /// Process a <see cref="PowerFunction"/>.
    /// </summary>
    /// <param name="pow"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessPowerFunction(PowerFunction pow, TContext context, TBinding binding);

    /// <summary>
    /// Process a <see cref="PythagoreanDistanceFunction"/>.
    /// </summary>
    /// <param name="pythagoreanDistance"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessPythagoreanDistanceFunction(PythagoreanDistanceFunction pythagoreanDistance, TContext context, TBinding binding);

    /// <summary>
    /// Process a <see cref="RandomFunction"/>.
    /// </summary>
    /// <param name="rand"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessRandomFunction(RandomFunction rand, TContext context, TBinding binding);

    /// <summary>
    /// Process a <see cref="ReciprocalFunction"/>.
    /// </summary>
    /// <param name="reciprocal"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessReciprocalFunction(ReciprocalFunction reciprocal, TContext context, TBinding binding);

    /// <summary>
    /// Process a <see cref="RootFunction"/>.
    /// </summary>
    /// <param name="root"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessRootFunction(RootFunction root, TContext context, TBinding binding);

    /// <summary>
    /// Process a <see cref="SquareFunction"/>.
    /// </summary>
    /// <param name="square"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessSquareFunction(SquareFunction square, TContext context, TBinding binding);

    /// <summary>
    /// Process a <see cref="SquareRootFunction"/>.
    /// </summary>
    /// <param name="sqrt"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessSquareRootFunction(SquareRootFunction sqrt, TContext context, TBinding binding);

    /// <summary>
    /// Process a <see cref="TenFunction"/>.
    /// </summary>
    /// <param name="ten"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessTenFunction(TenFunction ten, TContext context, TBinding binding);

    // SPARQL functions
    /// <summary>
    /// Process a SPARQL <see cref="BoundFunction"/>.
    /// </summary>
    /// <param name="bound"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessBoundFunction(BoundFunction bound, TContext context, TBinding binding);

    /// <summary>
    /// Process a SPARQL <see cref="ExistsFunction"/>.
    /// </summary>
    /// <param name="exists"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessExistsFunction(ExistsFunction exists, TContext context, TBinding binding);

    /// <summary>
    /// Process a SPARQL <see cref="IsBlankFunction"/>.
    /// </summary>
    /// <param name="isBlank"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessIsBlankFunction(IsBlankFunction isBlank, TContext context, TBinding binding);

    /// <summary>
    /// Process a SPARQL <see cref="IsIriFunction"/>.
    /// </summary>
    /// <param name="isIri"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessIsIriFunction(IsIriFunction isIri, TContext context, TBinding binding);

    /// <summary>
    /// Process a SPARQL <see cref="IsLiteralFunction"/>.
    /// </summary>
    /// <param name="isLiteral"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessIsLiteralFunction(IsLiteralFunction isLiteral, TContext context, TBinding binding);

    /// <summary>
    /// Process a SPARQL <see cref="IsNumericFunction"/>.
    /// </summary>
    /// <param name="isNumeric"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessIsNumericFunction(IsNumericFunction isNumeric, TContext context, TBinding binding);

    /// <summary>
    /// Process a SPARQL <see cref="IsTripleFunction"/>.
    /// </summary>
    /// <param name="isTripleFunction"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessIsTripleFunction(IsTripleFunction isTripleFunction, TContext context, TBinding binding);

    /// <summary>
    /// Process a SPARQL <see cref="LangMatchesFunction"/>.
    /// </summary>
    /// <param name="langMatches"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessLangMatchesFunction(LangMatchesFunction langMatches, TContext context, TBinding binding);

    /// <summary>
    /// Process a SPARQL <see cref="RegexFunction"/>.
    /// </summary>
    /// <param name="regex"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessRegexFunction(RegexFunction regex, TContext context, TBinding binding);

    /// <summary>
    /// Process a SPARQL <see cref="SameTermFunction"/>.
    /// </summary>
    /// <param name="sameTerm"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessSameTermFunction(SameTermFunction sameTerm, TContext context, TBinding binding);

    /// <summary>
    /// Process a SPARQL <see cref="BNodeFunction"/>.
    /// </summary>
    /// <param name="bNode"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessBNodeFunction(Expressions.Functions.Sparql.Constructor.BNodeFunction bNode, TContext context, TBinding binding);

    /// <summary>
    /// Process a SPARQL <see cref="IriFunction"/>.
    /// </summary>
    /// <param name="iri"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessIriFunction(IriFunction iri, TContext context, TBinding binding);

    /// <summary>
    /// Process a SPARQL <see cref="StrDtFunction"/>.
    /// </summary>
    /// <param name="strDt"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessStrDtFunction(StrDtFunction strDt, TContext context, TBinding binding);

    /// <summary>
    /// Process a SPARQL <see cref="StrLangFunction"/>.
    /// </summary>
    /// <param name="strLang"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessStrLangFunction(StrLangFunction strLang, TContext context, TBinding binding);

    /// <summary>
    /// Process a SPARQL <see cref="DayFunction"/>.
    /// </summary>
    /// <param name="day"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessDayFunction(DayFunction day, TContext context, TBinding binding);

    /// <summary>
    /// Process a SPARQL <see cref="HoursFunction"/>.
    /// </summary>
    /// <param name="hours"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessHoursFunction(HoursFunction hours, TContext context, TBinding binding);

    /// <summary>
    /// Process a SPARQL <see cref="MinutesFunction"/>.
    /// </summary>
    /// <param name="minutes"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessMinutesFunction(MinutesFunction minutes, TContext context, TBinding binding);

    /// <summary>
    /// Process a SPARQL <see cref="MonthFunction"/>.
    /// </summary>
    /// <param name="month"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessMonthFunction(MonthFunction month, TContext context, TBinding binding);

    /// <summary>
    /// Process a SPARQL <see cref="Expressions.Functions.Sparql.DateTime.NowFunction"/>.
    /// </summary>
    /// <param name="now"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessNowFunction(Expressions.Functions.Sparql.DateTime.NowFunction now, TContext context, TBinding binding);

    /// <summary>
    /// Process a SPARQL <see cref="SecondsFunction"/>.
    /// </summary>
    /// <param name="seconds"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessSecondsFunction(SecondsFunction seconds, TContext context, TBinding binding);

    /// <summary>
    /// Process a SPARQL <see cref="TimezoneFunction"/>.
    /// </summary>
    /// <param name="timezone"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessTimezoneFunction(TimezoneFunction timezone, TContext context, TBinding binding);

    /// <summary>
    /// Process a SPARQL <see cref="TZFunction"/>.
    /// </summary>
    /// <param name="tz"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessTZFunction(TZFunction tz, TContext context, TBinding binding);

    /// <summary>
    /// Process a SPARQL <see cref="YearFunction"/>.
    /// </summary>
    /// <param name="year"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessYearFunction(YearFunction year, TContext context, TBinding binding);

    /// <summary>
    /// Process a SPARQL <see cref="MD5HashFunction"/>.
    /// </summary>
    /// <param name="md5"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessMd5HashFunction(MD5HashFunction md5, TContext context, TBinding binding);

    /// <summary>
    /// Process a SPARQL <see cref="Sha1HashFunction"/>.
    /// </summary>
    /// <param name="sha1"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessSha1HashFunction(Sha1HashFunction sha1, TContext context, TBinding binding);

    /// <summary>
    /// Process a SPARQL <see cref="Sha256HashFunction"/>.
    /// </summary>
    /// <param name="sha256"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessSha256HashFunction(Sha256HashFunction sha256, TContext context, TBinding binding);

    /// <summary>
    /// Process a SPARQL <see cref="Sha384HashFunction"/>.
    /// </summary>
    /// <param name="sha384"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessSha384HashFunction(Sha384HashFunction sha384, TContext context, TBinding binding);

    /// <summary>
    /// Process a SPARQL <see cref="Sha512HashFunction"/>.
    /// </summary>
    /// <param name="sha512"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessSha512HashFunction(Sha512HashFunction sha512, TContext context, TBinding binding);

    /// <summary>
    /// Process a SPARQL <see cref="AbsFunction"/>.
    /// </summary>
    /// <param name="abs"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessAbsFunction(AbsFunction abs, TContext context, TBinding binding);

    /// <summary>
    /// Process a SPARQL <see cref="CeilFunction"/>.
    /// </summary>
    /// <param name="ceil"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessCeilFunction(CeilFunction ceil, TContext context, TBinding binding);

    /// <summary>
    /// Process a SPARQL <see cref="FloorFunction"/>.
    /// </summary>
    /// <param name="floor"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessFloorFunction(FloorFunction floor, TContext context, TBinding binding);

    /// <summary>
    /// Process a SPARQL <see cref="RandFunction"/>.
    /// </summary>
    /// <param name="rand"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessRandFunction(RandFunction rand, TContext context, TBinding binding);

    /// <summary>
    /// Process a SPARQL <see cref="RoundFunction"/>.
    /// </summary>
    /// <param name="round"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessRoundFunction(RoundFunction round, TContext context, TBinding binding);

    /// <summary>
    /// Process a SPARQL <see cref="InFunction"/>.
    /// </summary>
    /// <param name="inFn"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessInFunction(InFunction inFn, TContext context, TBinding binding);

    /// <summary>
    /// Process a SPARQL <see cref="NotInFunction"/>.
    /// </summary>
    /// <param name="notIn"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessNotInFunction(NotInFunction notIn, TContext context, TBinding binding);

    /// <summary>
    /// Process a SPARQL <see cref="ConcatFunction"/>.
    /// </summary>
    /// <param name="concat"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessConcatFunction(ConcatFunction concat, TContext context, TBinding binding);

    /// <summary>
    /// Process a SPARQL <see cref="ContainsFunction"/>.
    /// </summary>
    /// <param name="contains"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessContainsFunction(ContainsFunction contains, TContext context, TBinding binding);

    /// <summary>
    /// Process a SPARQL 1.0 <see cref="DataTypeFunction"/>.
    /// </summary>
    /// <param name="dataType"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessDataTypeFunction(DataTypeFunction dataType, TContext context, TBinding binding);

    /// <summary>
    /// Process a SPARQL 1.1 <see cref="DataTypeFunction"/>.
    /// </summary>
    /// <param name="dataType"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessDataType11Function(DataType11Function dataType, TContext context, TBinding binding);

    /// <summary>
    /// Process a SPARQL <see cref="EncodeForUriFunction"/>.
    /// </summary>
    /// <param name="encodeForUri"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessEncodeForUriFunction(EncodeForUriFunction encodeForUri, TContext context, TBinding binding);

    /// <summary>
    /// Process a SPARQL <see cref="LangFunction"/>.
    /// </summary>
    /// <param name="lang"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessLangFunction(LangFunction lang, TContext context, TBinding binding);

    /// <summary>
    /// Process a SPARQL <see cref="LCaseFunction"/>.
    /// </summary>
    /// <param name="lCase"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessLCaseFunction(LCaseFunction lCase, TContext context, TBinding binding);

    /// <summary>
    /// Process a SPARQL <see cref="ReplaceFunction"/>.
    /// </summary>
    /// <param name="replace"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessReplaceFunction(ReplaceFunction replace, TContext context, TBinding binding);

    /// <summary>
    /// Process a SPARQL <see cref="StrAfterFunction"/>.
    /// </summary>
    /// <param name="strAfter"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessStrAfterFunction(StrAfterFunction strAfter, TContext context, TBinding binding);

    /// <summary>
    /// Process a SPARQL <see cref="StrBeforeFunction"/>.
    /// </summary>
    /// <param name="strBefore"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessStrBeforeFunction(StrBeforeFunction strBefore, TContext context, TBinding binding);

    /// <summary>
    /// Process a SPARQL <see cref="StrEndsFunction"/>.
    /// </summary>
    /// <param name="strEnds"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessStrEndsFunction(StrEndsFunction strEnds, TContext context, TBinding binding);

    /// <summary>
    /// Process a SPARQL <see cref="StrFunction"/>.
    /// </summary>
    /// <param name="str"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessStrFunction(StrFunction str, TContext context, TBinding binding);

    /// <summary>
    /// Process a SPARQL <see cref="StrLenFunction"/>.
    /// </summary>
    /// <param name="strLen"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessStrLenFunction(StrLenFunction strLen, TContext context, TBinding binding);

    /// <summary>
    /// Process a SPARQL <see cref="StrStartsFunction"/>.
    /// </summary>
    /// <param name="strStarts"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessStrStartsFunction(StrStartsFunction strStarts, TContext context, TBinding binding);

    /// <summary>
    /// Process a SPARQL <see cref="SubStrFunction"/>.
    /// </summary>
    /// <param name="subStr"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessSubStrFunction(SubStrFunction subStr, TContext context, TBinding binding);

    /// <summary>
    /// Process a SPARQL <see cref="UCaseFunction"/>.
    /// </summary>
    /// <param name="uCase"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessUCaseFunction(UCaseFunction uCase, TContext context, TBinding binding);

    /// <summary>
    /// Process a SPARQL <see cref="UUIDFunction"/>.
    /// </summary>
    /// <param name="uuid"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessUuidFunction(UUIDFunction uuid, TContext context, TBinding binding);

    /// <summary>
    /// Process a SPARQL <see cref="StrUUIDFunction"/>.
    /// </summary>
    /// <param name="uuid"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessStrUuidFunction(StrUUIDFunction uuid, TContext context, TBinding binding);

    /// <summary>
    /// Process a SPARQL <see cref="CallFunction"/>.
    /// </summary>
    /// <param name="call"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessCallFunction(CallFunction call, TContext context, TBinding binding);

    /// <summary>
    /// Process a SPARQL <see cref="CoalesceFunction"/>.
    /// </summary>
    /// <param name="coalesce"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessCoalesceFunction(CoalesceFunction coalesce, TContext context, TBinding binding);

    /// <summary>
    /// Process a SPARQL <see cref="IfElseFunction"/>.
    /// </summary>
    /// <param name="ifElse"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessIfElseFunction(IfElseFunction ifElse, TContext context, TBinding binding);

    // Triple Node Functions
    /// <summary>
    /// Process a <see cref="SubjectFunction"/>.
    /// </summary>
    /// <param name="subjectFunction"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessSubjectFunction(SubjectFunction subjectFunction, TContext context, TBinding binding);

    /// <summary>
    /// Process a <see cref="PredicateFunction"/>.
    /// </summary>
    /// <param name="predicateFunction"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessPredicateFunction(PredicateFunction predicateFunction, TContext context, TBinding binding);

    /// <summary>
    /// Process a <see cref="ObjectFunction"/>.
    /// </summary>
    /// <param name="objectFunction"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessObjectFunction(ObjectFunction objectFunction, TContext context, TBinding binding);


    // XPath Functions
    /// <summary>
    /// Process a <see cref="BooleanCast"/>.
    /// </summary>
    /// <param name="cast"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessBooleanCast(BooleanCast cast, TContext context, TBinding binding);

    /// <summary>
    /// Process a <see cref="DateTimeCast"/>.
    /// </summary>
    /// <param name="cast"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessDateTimeCast(DateTimeCast cast, TContext context, TBinding binding);
    
    /// <summary>
    /// Process a <see cref="DecimalCast"/>.
    /// </summary>
    /// <param name="cast"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessDecimalCast(DecimalCast cast, TContext context, TBinding binding);

    /// <summary>
    /// Process a <see cref="DoubleCast"/>.
    /// </summary>
    /// <param name="cast"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessDoubleCast(DoubleCast cast, TContext context, TBinding binding);

    /// <summary>
    /// Process a <see cref="FloatCast"/>.
    /// </summary>
    /// <param name="cast"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessFloatCast(FloatCast cast, TContext context, TBinding binding);

    /// <summary>
    /// Process a <see cref="IntegerCast"/>.
    /// </summary>
    /// <param name="cast"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessIntegerCast(IntegerCast cast, TContext context, TBinding binding);

    /// <summary>
    /// Process a <see cref="StringCast"/>.
    /// </summary>
    /// <param name="cast"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessStringCast(StringCast cast, TContext context, TBinding binding);

    /// <summary>
    /// Process a <see cref="DayFromDateTimeFunction"/>.
    /// </summary>
    /// <param name="day"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessDayFromDateTimeFunction(DayFromDateTimeFunction day, TContext context, TBinding binding);

    /// <summary>
    /// Process a <see cref="HoursFromDateTimeFunction"/>.
    /// </summary>
    /// <param name="hours"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessHoursFromDateTimeFunction(HoursFromDateTimeFunction hours, TContext context, TBinding binding);

    /// <summary>
    /// Process a <see cref="MinutesFromDateTimeFunction"/>.
    /// </summary>
    /// <param name="minutes"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessMinutesFromDateTimeFunction(MinutesFromDateTimeFunction minutes, TContext context, TBinding binding);

    /// <summary>
    /// Process a <see cref="MonthFromDateTimeFunction"/>.
    /// </summary>
    /// <param name="month"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessMonthFromDateTimeFunction(MonthFromDateTimeFunction month, TContext context, TBinding binding);

    /// <summary>
    /// Process a <see cref="SecondsFromDateTimeFunction"/>.
    /// </summary>
    /// <param name="seconds"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessSecondsFromDateTimeFunction(SecondsFromDateTimeFunction seconds, TContext context, TBinding binding);

    /// <summary>
    /// Process a <see cref="TimezoneFromDateTimeFunction"/>.
    /// </summary>
    /// <param name="timezone"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessTimezoneFromDateTimeFunction(TimezoneFromDateTimeFunction timezone, TContext context, TBinding binding);

    /// <summary>
    /// Process a <see cref="YearFromDateTimeFunction"/>.
    /// </summary>
    /// <param name="years"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessYearsFromDateTimeFunction(YearFromDateTimeFunction years, TContext context, TBinding binding);

    /// <summary>
    /// Process an XPath <see cref="XPath.Numeric.AbsFunction"/>.
    /// </summary>
    /// <param name="abs"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessAbsFunction(XPath.Numeric.AbsFunction abs, TContext context, TBinding binding);

    /// <summary>
    /// Process an XPath <see cref="XPath.Numeric.CeilingFunction"/>.
    /// </summary>
    /// <param name="ceil"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessCeilFunction(XPath.Numeric.CeilingFunction ceil, TContext context, TBinding binding);

    /// <summary>
    /// Process an XPath <see cref="XPath.Numeric.FloorFunction"/>.
    /// </summary>
    /// <param name="floor"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessFloorFunction(XPath.Numeric.FloorFunction floor, TContext context, TBinding binding);

    /// <summary>
    /// Process an XPath <see cref="XPath.Numeric.RoundFunction"/>.
    /// </summary>
    /// <param name="round"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessRoundFunction(Expressions.Functions.XPath.Numeric.RoundFunction round, TContext context, TBinding binding);

    /// <summary>
    /// Process an XPath <see cref="XPath.Numeric.RoundHalfToEvenFunction"/>.
    /// </summary>
    /// <param name="round"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessRoundHalfToEvenFunction(XPath.Numeric.RoundHalfToEvenFunction round, TContext context, TBinding binding);

    /// <summary>
    /// Process a XPath <see cref="CompareFunction"/>.
    /// </summary>
    /// <param name="compare"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessCompareFunction(CompareFunction compare, TContext context, TBinding binding);

    /// <summary>
    /// Process an XPath <see cref="XPath.String.ConcatFunction"/>.
    /// </summary>
    /// <param name="concat"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessConcatFunction(Expressions.Functions.XPath.String.ConcatFunction concat, TContext context, TBinding binding);

    /// <summary>
    /// Process an XPath <see cref="XPath.String.ContainsFunction"/>.
    /// </summary>
    /// <param name="contains"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessContainsFunction(Expressions.Functions.XPath.String.ContainsFunction contains, TContext context, TBinding binding);

    /// <summary>
    /// Process an XPath <see cref="XPath.String.EncodeForUriFunction"/>.
    /// </summary>
    /// <param name="encodeForUri"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessEncodeForUriFunction(XPath.String.EncodeForUriFunction encodeForUri, TContext context, TBinding binding);

    /// <summary>
    /// Process an XPath <see cref="EndsWithFunction"/>.
    /// </summary>
    /// <param name="endsWith"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessEndsWithFunction(EndsWithFunction endsWith, TContext context, TBinding binding);

    /// <summary>
    /// Process an XPath <see cref="EscapeHtmlUriFunction"/>.
    /// </summary>
    /// <param name="escape"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessEscapeHtmlUriFunction(EscapeHtmlUriFunction escape, TContext context, TBinding binding);

    /// <summary>
    /// Process an XPath <see cref="LowerCaseFunction"/>.
    /// </summary>
    /// <param name="lCase"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessLowerCaseFunction(LowerCaseFunction lCase, TContext context, TBinding binding);

    /// <summary>
    /// Process an XPath <see cref="NormalizeSpaceFunction"/>.
    /// </summary>
    /// <param name="normalize"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessNormalizeSpaceFunction(NormalizeSpaceFunction normalize, TContext context, TBinding binding);

    /// <summary>
    /// Process an XPath <see cref="NormalizeUnicodeFunction"/>.
    /// </summary>
    /// <param name="normalize"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessNormalizeUnicodeFunction(NormalizeUnicodeFunction normalize, TContext context, TBinding binding);

    /// <summary>
    /// Process an XPath <see cref="XPath.String.ReplaceFunction"/>.
    /// </summary>
    /// <param name="replace"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessReplaceFunction(Expressions.Functions.XPath.String.ReplaceFunction replace, TContext context, TBinding binding);

    /// <summary>
    /// Process an XPath <see cref="XPath.String.StartsWithFunction"/>.
    /// </summary>
    /// <param name="startsWith"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessStartsWithFunction(StartsWithFunction startsWith, TContext context, TBinding binding);

    /// <summary>
    /// Process an XPath <see cref="StringLengthFunction"/>.
    /// </summary>
    /// <param name="strLen"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessStringLengthFunction(StringLengthFunction strLen, TContext context, TBinding binding);

    /// <summary>
    /// Process an XPath <see cref="SubstringAfterFunction"/>.
    /// </summary>
    /// <param name="substringAfter"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessSubstringAfterFunction(SubstringAfterFunction substringAfter, TContext context, TBinding binding);

    /// <summary>
    /// Process an XPath <see cref="SubstringBeforeFunction"/>.
    /// </summary>
    /// <param name="substringBefore"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessSubstringBeforeFunction(SubstringBeforeFunction substringBefore, TContext context, TBinding binding);

    /// <summary>
    /// Process an XPath <see cref="XPath.String.SubstringFunction"/>.
    /// </summary>
    /// <param name="substring"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessSubstringFunction(Expressions.Functions.XPath.String.SubstringFunction substring, TContext context, TBinding binding);

    /// <summary>
    /// Process an XPath <see cref="UpperCaseFunction"/>.
    /// </summary>
    /// <param name="uCase"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessUpperCaseFunction(UpperCaseFunction uCase, TContext context, TBinding binding);

    /// <summary>
    /// Process an XPath <see cref="BooleanFunction"/>.
    /// </summary>
    /// <param name="boolean"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessBooleanFunction(BooleanFunction boolean, TContext context, TBinding binding);

    /// <summary>
    /// Handle an unrecognized function call.
    /// </summary>
    /// <param name="unknownFunction"></param>
    /// <param name="context"></param>
    /// <param name="binding"></param>
    /// <returns></returns>
    TResult ProcessUnknownFunction(UnknownFunction unknownFunction, TContext context, TBinding binding);
}
