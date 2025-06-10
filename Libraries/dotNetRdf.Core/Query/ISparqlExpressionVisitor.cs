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

// unset

using VDS.RDF.Query.Expressions.Arithmetic;
using VDS.RDF.Query.Expressions.Comparison;
using VDS.RDF.Query.Expressions.Conditional;
using VDS.RDF.Query.Expressions.Functions;
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
using VDS.RDF.Query.Expressions.Functions.XPath.Numeric;
using VDS.RDF.Query.Expressions.Primary;
using XPath=VDS.RDF.Query.Expressions.Functions.XPath;
using AbsFunction = VDS.RDF.Query.Expressions.Functions.Sparql.Numeric.AbsFunction;
using ArqFunctions = VDS.RDF.Query.Expressions.Functions.Arq;
using ConcatFunction = VDS.RDF.Query.Expressions.Functions.Sparql.String.ConcatFunction;
using ContainsFunction = VDS.RDF.Query.Expressions.Functions.Sparql.String.ContainsFunction;
using EncodeForUriFunction = VDS.RDF.Query.Expressions.Functions.Sparql.String.EncodeForUriFunction;
using FloorFunction = VDS.RDF.Query.Expressions.Functions.Sparql.Numeric.FloorFunction;
using LeviathanNumeric = VDS.RDF.Query.Expressions.Functions.Leviathan.Numeric;
using LeviathanHash = VDS.RDF.Query.Expressions.Functions.Leviathan.Hash;
using ReplaceFunction = VDS.RDF.Query.Expressions.Functions.Sparql.String.ReplaceFunction;
using RoundFunction = VDS.RDF.Query.Expressions.Functions.Sparql.Numeric.RoundFunction;

namespace VDS.RDF.Query;

/// <summary>
/// The interface to be implemented by an object that can be accepted by an ISparqlExpression instance.
/// </summary>
/// <typeparam name="T">The type returned by the visit methods of the visitor.</typeparam>
public interface ISparqlExpressionVisitor<out T>
{
    // Primary
    /// <summary>
    /// Visit a <see cref="AggregateTerm"/>.
    /// </summary>
    /// <param name="aggregate"></param>
    /// <returns></returns>
    T VisitAggregateTerm(AggregateTerm aggregate);

    /// <summary>
    /// Visit a <see cref="AllModifier"/>.
    /// </summary>
    /// <param name="all"></param>
    /// <returns></returns>
    T VisitAllModifier(AllModifier all);

    /// <summary>
    /// Visit a <see cref="ConstantTerm"/>.
    /// </summary>
    /// <param name="constant"></param>
    /// <returns></returns>
    T VisitConstantTerm(ConstantTerm constant);

    /// <summary>
    /// Visit a <see cref="DistinctModifier"/>.
    /// </summary>
    /// <param name="distinct"></param>
    /// <returns></returns>
    T VisitDistinctModifier(DistinctModifier distinct);

    /// <summary>
    /// Visit a <see cref="GraphPatternTerm"/>.
    /// </summary>
    /// <param name="graphPattern"></param>
    /// <returns></returns>
    T VisitGraphPatternTerm(GraphPatternTerm graphPattern);

    /// <summary>
    /// Visit a <see cref="VariableTerm"/>.
    /// </summary>
    /// <param name="variable"></param>
    /// <returns></returns>
    T VisitVariableTerm(VariableTerm variable);

    // Arithmetic

    /// <summary>
    /// Visit a <see cref="AdditionExpression"/>.
    /// </summary>
    /// <param name="expr"></param>
    /// <returns></returns>
    T VisitAdditionExpression(AdditionExpression expr);

    /// <summary>
    /// Visit a <see cref="DivisionExpression"/>.
    /// </summary>
    /// <param name="divisionExpression"></param>
    /// <returns></returns>
    T VisitDivisionExpression(DivisionExpression divisionExpression);

    /// <summary>
    /// Visit a <see cref="MultiplicationExpression"/>.
    /// </summary>
    /// <param name="multiplicationExpression"></param>
    /// <returns></returns>
    T VisitMultiplicationExpression(MultiplicationExpression multiplicationExpression);

    /// <summary>
    /// Visit a <see cref="SubtractionExpression"/>.
    /// </summary>
    /// <param name="subtractionExpression"></param>
    /// <returns></returns>
    T VisitSubtractionExpression(SubtractionExpression subtractionExpression);

    /// <summary>
    /// Visit a <see cref="MinusExpression"/>.
    /// </summary>
    /// <param name="minusExpression"></param>
    /// <returns></returns>
    T VisitMinusExpression(MinusExpression minusExpression);

    /// <summary>
    /// Visit a <see cref="EqualsExpression"/>.
    /// </summary>
    /// <param name="equals"></param>
    /// <returns></returns>
    T VisitEqualsExpression(EqualsExpression equals);

    /// <summary>
    /// Visit a <see cref="GreaterThanExpression"/>.
    /// </summary>
    /// <param name="gt"></param>
    /// <returns></returns>
    T VisitGreaterThanExpression(GreaterThanExpression gt);

    /// <summary>
    /// Visit a <see cref="GreaterThanOrEqualToExpression"/>.
    /// </summary>
    /// <param name="gte"></param>
    /// <returns></returns>
    T VisitGreaterThanOrEqualToExpression(GreaterThanOrEqualToExpression gte);

    /// <summary>
    /// Visit a <see cref="LessThanExpression"/>.
    /// </summary>
    /// <param name="lt"></param>
    /// <returns></returns>
    T VisitLessThanExpression(LessThanExpression lt);

    /// <summary>
    /// Visit a <see cref="LessThanOrEqualToExpression"/>.
    /// </summary>
    /// <param name="lte"></param>
    /// <returns></returns>
    T VisitLessThanOrEqualToExpression(LessThanOrEqualToExpression lte);

    /// <summary>
    /// Visit a <see cref="NotEqualsExpression"/>.
    /// </summary>
    /// <param name="ne"></param>
    /// <returns></returns>
    T VisitNotEqualsExpression(NotEqualsExpression ne);

    /// <summary>
    /// Visit an <see cref="AndExpression"/>.
    /// </summary>
    /// <param name="and"></param>
    /// <returns></returns>
    T VisitAndExpression(AndExpression and);

    /// <summary>
    /// Visit a <see cref="NotExpression"/>.
    /// </summary>
    /// <param name="not"></param>
    /// <returns></returns>
    T VisitNotExpression(NotExpression not);

    /// <summary>
    /// Visit an <see cref="OrExpression"/>.
    /// </summary>
    /// <param name="or"></param>
    /// <returns></returns>
    T VisitOrExpression(OrExpression or);

    // ARQ Functions
    /// <summary>
    /// Visit an ARQ <see cref="ArqFunctions.BNodeFunction"/>.
    /// </summary>
    /// <param name="bNode"></param>
    /// <returns></returns>
    T VisitArqBNodeFunction(ArqFunctions.BNodeFunction bNode);

    /// <summary>
    /// Visit an ARQ <see cref="ArqFunctions.EFunction"/>.
    /// </summary>
    /// <param name="e"></param>
    /// <returns></returns>
    T VisitEFunction(ArqFunctions.EFunction e);

    /// <summary>
    /// Visit an ARQ <see cref="ArqFunctions.LocalNameFunction"/>.
    /// </summary>
    /// <param name="localName"></param>
    /// <returns></returns>
    T VisitLocalNameFunction(ArqFunctions.LocalNameFunction localName);

    /// <summary>
    /// Visit an ARQ <see cref="ArqFunctions.MaxFunction"/>.
    /// </summary>
    /// <param name="max"></param>
    /// <returns></returns>
    T VisitMaxFunction(ArqFunctions.MaxFunction max);

    /// <summary>
    /// Visit an ARQ <see cref="ArqFunctions.MinFunction"/>.
    /// </summary>
    /// <param name="min"></param>
    /// <returns></returns>
    T VisitMinFunction(ArqFunctions.MinFunction min);

    /// <summary>
    /// Visit an ARQ <see cref="ArqFunctions.NamespaceFunction"/>.
    /// </summary>
    /// <param name="ns"></param>
    /// <returns></returns>
    T VisitNamespaceFunction(ArqFunctions.NamespaceFunction ns);

    /// <summary>
    /// Visit an ARQ <see cref="ArqFunctions.NowFunction"/>.
    /// </summary>
    /// <param name="now"></param>
    /// <returns></returns>
    T VisitNowFunction(ArqFunctions.NowFunction now);

    /// <summary>
    /// Visit an ARQ <see cref="ArqFunctions.PiFunction"/>.
    /// </summary>
    /// <param name="pi"></param>
    /// <returns></returns>
    T VisitPiFunction(ArqFunctions.PiFunction pi);

    /// <summary>
    /// Visit an ARQ <see cref="ArqFunctions.Sha1Function"/>.
    /// </summary>
    /// <param name="sha1"></param>
    /// <returns></returns>
    T VisitSha1Function(ArqFunctions.Sha1Function sha1);

    /// <summary>
    /// Visit an ARQ <see cref="ArqFunctions.StringJoinFunction"/>.
    /// </summary>
    /// <param name="stringJoin"></param>
    /// <returns></returns>
    T VisitStringJoinFunction(ArqFunctions.StringJoinFunction stringJoin);

    /// <summary>
    /// Visit an ARQ <see cref="ArqFunctions.SubstringFunction"/>.
    /// </summary>
    /// <param name="substring"></param>
    /// <returns></returns>
    T VisitSubstringFunction(ArqFunctions.SubstringFunction substring);

    // Leviathan Functions
    /// <summary>
    /// Visit a Leviathan <see cref="LeviathanHash.MD5HashFunction"/>.
    /// </summary>
    /// <param name="md5"></param>
    /// <returns></returns>
    T VisitLeviathanMD5HashFunction(LeviathanHash.MD5HashFunction md5);

    /// <summary>
    /// Visit a Leviathan <see cref="LeviathanHash.Sha256HashFunction"/>.
    /// </summary>
    /// <param name="sha256"></param>
    /// <returns></returns>
    T VisitLeviathanSha256HashFunction(LeviathanHash.Sha256HashFunction sha256);

    /// <summary>
    /// Visit a <see cref="CosecantFunction"/>.
    /// </summary>
    /// <param name="cosec"></param>
    /// <returns></returns>
    T VisitCosecantFunction(CosecantFunction cosec);

    /// <summary>
    /// Visit a <see cref="CosineFunction"/>.
    /// </summary>
    /// <param name="cos"></param>
    /// <returns></returns>
    T VisitCosineFunction(CosineFunction cos);

    /// <summary>
    /// Visit a <see cref="CotangentFunction"/>.
    /// </summary>
    /// <param name="cot"></param>
    /// <returns></returns>
    T VisitCotangentFunction(CotangentFunction cot);

    /// <summary>
    /// Visit a <see cref="DegreesToRadiansFunction"/>.
    /// </summary>
    /// <param name="degToRad"></param>
    /// <returns></returns>
    T VisitDegreesToRadiansFunction(DegreesToRadiansFunction degToRad);

    /// <summary>
    /// Visit a <see cref="RadiansToDegreesFunction"/>.
    /// </summary>
    /// <param name="radToDeg"></param>
    /// <returns></returns>
    T VisitRadiansToDegreesFunction(RadiansToDegreesFunction radToDeg);

    /// <summary>
    /// Visit a <see cref="SecantFunction"/>.
    /// </summary>
    /// <param name="sec"></param>
    /// <returns></returns>
    T VisitSecantFunction(SecantFunction sec);

    /// <summary>
    /// Visit a <see cref="SineFunction"/>.
    /// </summary>
    /// <param name="sin"></param>
    /// <returns></returns>
    T VisitSineFunction(SineFunction sin);

    /// <summary>
    /// Visit a <see cref="TangentFunction"/>.
    /// </summary>
    /// <param name="tan"></param>
    /// <returns></returns>
    T VisitTangentFunction(TangentFunction tan);

    /// <summary>
    /// Visit a <see cref="CartesianFunction"/>.
    /// </summary>
    /// <param name="cart"></param>
    /// <returns></returns>
    T VisitCartesianFunction(CartesianFunction cart);

    /// <summary>
    /// Visit a <see cref="CubeFunction"/>.
    /// </summary>
    /// <param name="cube"></param>
    /// <returns></returns>
    T VisitCubeFunction(CubeFunction cube);

    /// <summary>
    /// Visit a <see cref="EFunction"/>.
    /// </summary>
    /// <param name="eFunction"></param>
    /// <returns></returns>
    T VisitLeviathanEFunction(EFunction eFunction);

    /// <summary>
    /// Visit a <see cref="FactorialFunction"/>.
    /// </summary>
    /// <param name="factorial"></param>
    /// <returns></returns>
    T VisitFactorialFunction(FactorialFunction factorial);

    /// <summary>
    /// Visit a <see cref="LogFunction"/>.
    /// </summary>
    /// <param name="log"></param>
    /// <returns></returns>
    T VisitLogFunction(LogFunction log);

    /// <summary>
    /// Visit a <see cref="LeviathanNaturalLogFunction"/>.
    /// </summary>
    /// <param name="logn"></param>
    /// <returns></returns>
    T VisitNaturalLogFunction(LeviathanNaturalLogFunction logn);

    /// <summary>
    /// Visit a <see cref="PowerFunction"/>.
    /// </summary>
    /// <param name="pow"></param>
    /// <returns></returns>
    T VisitPowerFunction(PowerFunction pow);

    /// <summary>
    /// Visit a <see cref="PythagoreanDistanceFunction"/>.
    /// </summary>
    /// <param name="pythagoreanDistance"></param>
    /// <returns></returns>
    T VisitPythagoreanDistanceFunction(PythagoreanDistanceFunction pythagoreanDistance);

    /// <summary>
    /// Visit a <see cref="RandomFunction"/>.
    /// </summary>
    /// <param name="rand"></param>
    /// <returns></returns>
    T VisitRandomFunction(RandomFunction rand);

    /// <summary>
    /// Visit a <see cref="ReciprocalFunction"/>.
    /// </summary>
    /// <param name="reciprocal"></param>
    /// <returns></returns>
    T VisitReciprocalFunction(ReciprocalFunction reciprocal);

    /// <summary>
    /// Visit a <see cref="RootFunction"/>.
    /// </summary>
    /// <param name="root"></param>
    /// <returns></returns>
    T VisitRootFunction(RootFunction root);

    /// <summary>
    /// Visit a <see cref="SquareFunction"/>.
    /// </summary>
    /// <param name="square"></param>
    /// <returns></returns>
    T VisitSquareFunction(SquareFunction square);

    /// <summary>
    /// Visit a <see cref="SquareRootFunction"/>.
    /// </summary>
    /// <param name="sqrt"></param>
    /// <returns></returns>
    T VisitSquareRootFunction(SquareRootFunction sqrt);

    /// <summary>
    /// Visit a <see cref="TenFunction"/>.
    /// </summary>
    /// <param name="ten"></param>
    /// <returns></returns>
    T VisitTenFunction(TenFunction ten);

    // SPARQL functions
    /// <summary>
    /// Process a <see cref="BoundFunction"/>.
    /// </summary>
    /// <param name="bound"></param>
    /// <returns></returns>
    T VisitBoundFunction(BoundFunction bound);

    /// <summary>
    /// Process a <see cref="ExistsFunction"/>.
    /// </summary>
    /// <param name="exists"></param>
    /// <returns></returns>
    T VisitExistsFunction(ExistsFunction exists);

    /// <summary>
    /// Process a <see cref="IsBlankFunction"/>.
    /// </summary>
    /// <param name="isBlank"></param>
    /// <returns></returns>
    T VisitIsBlankFunction(IsBlankFunction isBlank);

    /// <summary>
    /// Process a <see cref="IsIriFunction"/>.
    /// </summary>
    /// <param name="isIri"></param>
    /// <returns></returns>
    T VisitIsIriFunction(IsIriFunction isIri);

    /// <summary>
    /// Process a <see cref="IsLiteralFunction"/>.
    /// </summary>
    /// <param name="isLiteral"></param>
    /// <returns></returns>
    T VisitIsLiteralFunction(IsLiteralFunction isLiteral);

    /// <summary>
    /// Process a <see cref="IsNumericFunction"/>.
    /// </summary>
    /// <param name="isNumeric"></param>
    /// <returns></returns>
    T VisitIsNumericFunction(IsNumericFunction isNumeric);

    /// <summary>
    /// Process a <see cref="IsTripleFunction"/>.
    /// </summary>
    /// <param name="isTripleFunction"></param>
    /// <returns></returns>
    T VisitIsTripleFunction(IsTripleFunction isTripleFunction);

    /// <summary>
    /// Process a <see cref="LangMatchesFunction"/>.
    /// </summary>
    /// <param name="langMatches"></param>
    /// <returns></returns>
    T VisitLangMatchesFunction(LangMatchesFunction langMatches);

    /// <summary>
    /// Process a <see cref="RegexFunction"/>.
    /// </summary>
    /// <param name="regex"></param>
    /// <returns></returns>
    T VisitRegexFunction(RegexFunction regex);

    /// <summary>
    /// Process a <see cref="SameTermFunction"/>.
    /// </summary>
    /// <param name="sameTerm"></param>
    /// <returns></returns>
    T VisitSameTermFunction(SameTermFunction sameTerm);

    /// <summary>
    /// Process a <see cref="BNodeFunction"/>.
    /// </summary>
    /// <param name="bNode"></param>
    /// <returns></returns>
    T VisitBNodeFunction(BNodeFunction bNode);

    /// <summary>
    /// Process an <see cref="IriFunction"/>.
    /// </summary>
    /// <param name="iri"></param>
    /// <returns></returns>
    T VisitIriFunction(IriFunction iri);

    /// <summary>
    /// Process a <see cref="StrDtFunction"/>.
    /// </summary>
    /// <param name="strDt"></param>
    /// <returns></returns>
    T VisitStrDtFunction(StrDtFunction strDt);

    /// <summary>
    /// Process a <see cref="StrLangFunction"/>.
    /// </summary>
    /// <param name="strLang"></param>
    /// <returns></returns>
    T VisitStrLangFunction(StrLangFunction strLang);

    /// <summary>
    /// Process a <see cref="DayFunction"/>.
    /// </summary>
    /// <param name="day"></param>
    /// <returns></returns>
    T VisitDayFunction(DayFunction day);

    /// <summary>
    /// Process a <see cref="HoursFunction"/>.
    /// </summary>
    /// <param name="hours"></param>
    /// <returns></returns>
    T VisitHoursFunction(HoursFunction hours);

    /// <summary>
    /// Process a <see cref="MinutesFunction"/>.
    /// </summary>
    /// <param name="minutes"></param>
    /// <returns></returns>
    T VisitMinutesFunction(MinutesFunction minutes);

    /// <summary>
    /// Process a <see cref="MonthFunction"/>.
    /// </summary>
    /// <param name="month"></param>
    /// <returns></returns>
    T VisitMonthFunction(MonthFunction month);

    /// <summary>
    /// Process a <see cref="NowFunction"/>.
    /// </summary>
    /// <param name="now"></param>
    /// <returns></returns>
    T VisitNowFunction(NowFunction now);

    /// <summary>
    /// Process a <see cref="SecondsFunction"/>.
    /// </summary>
    /// <param name="seconds"></param>
    /// <returns></returns>
    T VisitSecondsFunction(SecondsFunction seconds);

    /// <summary>
    /// Process a <see cref="TimezoneFunction"/>.
    /// </summary>
    /// <param name="timezone"></param>
    /// <returns></returns>
    T VisitTimezoneFunction(TimezoneFunction timezone);

    /// <summary>
    /// Process a <see cref="TZFunction"/>.
    /// </summary>
    /// <param name="tz"></param>
    /// <returns></returns>
    T VisitTZFunction(TZFunction tz);

    /// <summary>
    /// Process a <see cref="YearFunction"/>.
    /// </summary>
    /// <param name="year"></param>
    /// <returns></returns>
    T VisitYearFunction(YearFunction year);

    /// <summary>
    /// Process a <see cref="MD5HashFunction"/>.
    /// </summary>
    /// <param name="md5"></param>
    /// <returns></returns>
    T VisitMd5HashFunction(MD5HashFunction md5);

    /// <summary>
    /// Process a <see cref="Sha1HashFunction"/>.
    /// </summary>
    /// <param name="sha1"></param>
    /// <returns></returns>
    T VisitSha1HashFunction(Sha1HashFunction sha1);

    /// <summary>
    /// Process a <see cref="Sha256HashFunction"/>.
    /// </summary>
    /// <param name="sha256"></param>
    /// <returns></returns>
    T VisitSha256HashFunction(Sha256HashFunction sha256);

    /// <summary>
    /// Process a <see cref="Sha384HashFunction"/>.
    /// </summary>
    /// <param name="sha384"></param>
    /// <returns></returns>
    T VisitSha384HashFunction(Sha384HashFunction sha384);

    /// <summary>
    /// Process a <see cref="Sha512HashFunction"/>.
    /// </summary>
    /// <param name="sha512"></param>
    /// <returns></returns>
    T VisitSha512HashFunction(Sha512HashFunction sha512);

    /// <summary>
    /// Process an <see cref="AbsFunction"/>.
    /// </summary>
    /// <param name="abs"></param>
    /// <returns></returns>
    T VisitAbsFunction(AbsFunction abs);

    /// <summary>
    /// Process a <see cref="CeilFunction"/>.
    /// </summary>
    /// <param name="ceil"></param>
    /// <returns></returns>
    T VisitCeilFunction(CeilFunction ceil);

    /// <summary>
    /// Process a <see cref="FloorFunction"/>.
    /// </summary>
    /// <param name="floor"></param>
    /// <returns></returns>
    T VisitFloorFunction(FloorFunction floor);

    /// <summary>
    /// Process a <see cref="RandFunction"/>.
    /// </summary>
    /// <param name="rand"></param>
    /// <returns></returns>
    T VisitRandFunction(RandFunction rand);

    /// <summary>
    /// Process a <see cref="RandFunction"/>.
    /// </summary>
    /// <param name="round"></param>
    /// <returns></returns>
    T VisitRoundFunction(RoundFunction round);

    /// <summary>
    /// Process a <see cref="InFunction"/>.
    /// </summary>
    /// <param name="inFn"></param>
    /// <returns></returns>
    T VisitInFunction(InFunction inFn);

    /// <summary>
    /// Process a <see cref="NotInFunction"/>.
    /// </summary>
    /// <param name="notIn"></param>
    /// <returns></returns>
    T VisitNotInFunction(NotInFunction notIn);

    /// <summary>
    /// Process a <see cref="ConcatFunction"/>.
    /// </summary>
    /// <param name="concat"></param>
    /// <returns></returns>
    T VisitConcatFunction(ConcatFunction concat);

    /// <summary>
    /// Process a <see cref="ContainsFunction"/>.
    /// </summary>
    /// <param name="contains"></param>
    /// <returns></returns>
    T VisitContainsFunction(ContainsFunction contains);

    /// <summary>
    /// Process a <see cref="DataTypeFunction"/>.
    /// </summary>
    /// <param name="dataType"></param>
    /// <returns></returns>
    T VisitDataTypeFunction(DataTypeFunction dataType);

    /// <summary>
    /// Process a <see cref="DataType11Function"/>.
    /// </summary>
    /// <param name="dataType"></param>
    /// <returns></returns>
    T VisitDataType11Function(DataType11Function dataType);

    /// <summary>
    /// Process a <see cref="EncodeForUriFunction"/>.
    /// </summary>
    /// <param name="encodeForUri"></param>
    /// <returns></returns>
    T VisitEncodeForUriFunction(EncodeForUriFunction encodeForUri);

    /// <summary>
    /// Process a <see cref="LangFunction"/>.
    /// </summary>
    /// <param name="lang"></param>
    /// <returns></returns>
    T VisitLangFunction(LangFunction lang);

    /// <summary>
    /// Process a <see cref="LCaseFunction"/>.
    /// </summary>
    /// <param name="lCase"></param>
    /// <returns></returns>
    T VisitLCaseFunction(LCaseFunction lCase);

    /// <summary>
    /// Process a <see cref="ReplaceFunction"/>.
    /// </summary>
    /// <param name="replace"></param>
    /// <returns></returns>
    T VisitReplaceFunction(ReplaceFunction replace);

    /// <summary>
    /// Process a <see cref="StrAfterFunction"/>.
    /// </summary>
    /// <param name="strAfter"></param>
    /// <returns></returns>
    T VisitStrAfterFunction(StrAfterFunction strAfter);

    /// <summary>
    /// Process a <see cref="StrBeforeFunction"/>.
    /// </summary>
    /// <param name="strBefore"></param>
    /// <returns></returns>
    T VisitStrBeforeFunction(StrBeforeFunction strBefore);

    /// <summary>
    /// Process a <see cref="StrEndsFunction"/>.
    /// </summary>
    /// <param name="strEnds"></param>
    /// <returns></returns>
    T VisitStrEndsFunction(StrEndsFunction strEnds);

    /// <summary>
    /// Process a <see cref="StrFunction"/>.
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    T VisitStrFunction(StrFunction str);

    /// <summary>
    /// Process a <see cref="StrLenFunction"/>.
    /// </summary>
    /// <param name="strLen"></param>
    /// <returns></returns>
    T VisitStrLenFunction(StrLenFunction strLen);

    /// <summary>
    /// Process a <see cref="StrStartsFunction"/>.
    /// </summary>
    /// <param name="strStarts"></param>
    /// <returns></returns>
    T VisitStrStartsFunction(StrStartsFunction strStarts);

    /// <summary>
    /// Process a <see cref="SubStrFunction"/>.
    /// </summary>
    /// <param name="subStr"></param>
    /// <returns></returns>
    T VisitSubStrFunction(SubStrFunction subStr);

    /// <summary>
    /// Process a <see cref="UCaseFunction"/>.
    /// </summary>
    /// <param name="uCase"></param>
    /// <returns></returns>
    T VisitUCaseFunction(UCaseFunction uCase);

    /// <summary>
    /// Process a <see cref="UUIDFunction"/>.
    /// </summary>
    /// <param name="uuid"></param>
    /// <returns></returns>
    T VisitUuidFunction(UUIDFunction uuid);

    /// <summary>
    /// Process a <see cref="StrUUIDFunction"/>.
    /// </summary>
    /// <param name="uuid"></param>
    /// <returns></returns>
    T VisitStrUuidFunction(StrUUIDFunction uuid);

    /// <summary>
    /// Process a <see cref="CallFunction"/>.
    /// </summary>
    /// <param name="call"></param>
    /// <returns></returns>
    T VisitCallFunction(CallFunction call);

    /// <summary>
    /// Process a <see cref="CoalesceFunction"/>.
    /// </summary>
    /// <param name="coalesce"></param>
    /// <returns></returns>
    T VisitCoalesceFunction(CoalesceFunction coalesce);

    /// <summary>
    /// Process a <see cref="IfElseFunction"/>.
    /// </summary>
    /// <param name="ifElse"></param>
    /// <returns></returns>
    T VisitIfElseFunction(IfElseFunction ifElse);

    // Triple Node Functions
    /// <summary>
    /// Process a <see cref="SubjectFunction"/>.
    /// </summary>
    /// <param name="subjectFunction"></param>
    /// <returns></returns>
    T VisitSubjectFunction(SubjectFunction subjectFunction);

    /// <summary>
    /// Process a <see cref="PredicateFunction"/>.
    /// </summary>
    /// <param name="predicateFunction"></param>
    /// <returns></returns>
    T VisitPredicateFunction(PredicateFunction predicateFunction);

    /// <summary>
    /// Process an <see cref="ObjectFunction"/>.
    /// </summary>
    /// <param name="objectFunction"></param>
    /// <returns></returns>
    T VisitObjectFunction(ObjectFunction objectFunction);

    // XPath Functions
    /// <summary>
    /// Process a <see cref="BooleanCast"/>.
    /// </summary>
    /// <param name="cast"></param>
    /// <returns></returns>
    T VisitBooleanCast(BooleanCast cast);
    /// <summary>
    /// Process a <see cref="DateTimeCast"/>.
    /// </summary>
    /// <param name="cast"></param>
    /// <returns></returns>
    T VisitDateTimeCast(DateTimeCast cast);

    /// <summary>
    /// Process a <see cref="DecimalCast"/>.
    /// </summary>
    /// <param name="cast"></param>
    /// <returns></returns>
    T VisitDecimalCast(DecimalCast cast);

    /// <summary>
    /// Process a <see cref="DoubleCast"/>.
    /// </summary>
    /// <param name="cast"></param>
    /// <returns></returns>
    T VisitDoubleCast(DoubleCast cast);

    /// <summary>
    /// Process a <see cref="FloatCast"/>.
    /// </summary>
    /// <param name="cast"></param>
    /// <returns></returns>
    T VisitFloatCast(FloatCast cast);

    /// <summary>
    /// Process a <see cref="IntegerCast"/>.
    /// </summary>
    /// <param name="cast"></param>
    /// <returns></returns>
    T VisitIntegerCast(IntegerCast cast);

    /// <summary>
    /// Process a <see cref="StringCast"/>.
    /// </summary>
    /// <param name="cast"></param>
    /// <returns></returns>
    T VisitStringCast(StringCast cast);

    /// <summary>
    /// Process a <see cref="DayFromDateTimeFunction"/>.
    /// </summary>
    /// <param name="day"></param>
    /// <returns></returns>
    T VisitDayFromDateTimeFunction(DayFromDateTimeFunction day);

    /// <summary>
    /// Process a <see cref="HoursFromDateTimeFunction"/>.
    /// </summary>
    /// <param name="hours"></param>
    /// <returns></returns>
    T VisitHoursFromDateTimeFunction(HoursFromDateTimeFunction hours);

    /// <summary>
    /// Process a <see cref="MinutesFromDateTimeFunction"/>.
    /// </summary>
    /// <param name="minutes"></param>
    /// <returns></returns>
    T VisitMinutesFromDateTimeFunction(MinutesFromDateTimeFunction minutes);

    /// <summary>
    /// Process a <see cref="MonthFromDateTimeFunction"/>.
    /// </summary>
    /// <param name="month"></param>
    /// <returns></returns>
    T VisitMonthFromDateTimeFunction(MonthFromDateTimeFunction month);

    /// <summary>
    /// Process a <see cref="SecondsFromDateTimeFunction"/>.
    /// </summary>
    /// <param name="seconds"></param>
    /// <returns></returns>
    T VisitSecondsFromDateTimeFunction(SecondsFromDateTimeFunction seconds);

    /// <summary>
    /// Process a <see cref="TimezoneFromDateTimeFunction"/>.
    /// </summary>
    /// <param name="timezone"></param>
    /// <returns></returns>
    T VisitTimezoneFromDateTimeFunction(TimezoneFromDateTimeFunction timezone);

    /// <summary>
    /// Process a <see cref="YearFromDateTimeFunction"/>.
    /// </summary>
    /// <param name="years"></param>
    /// <returns></returns>
    T VisitYearsFromDateTimeFunction(YearFromDateTimeFunction years);

    /// <summary>
    /// Process an XPath <see cref="XPath.Numeric.AbsFunction"/>.
    /// </summary>
    /// <param name="abs"></param>
    /// <returns></returns>
    T VisitAbsFunction(XPath.Numeric.AbsFunction abs);

    /// <summary>
    /// Process an XPath <see cref="CeilFunction"/>.
    /// </summary>
    /// <param name="ceil"></param>
    /// <returns></returns>
    T VisitCeilFunction(CeilingFunction ceil);

    /// <summary>
    /// Process an XPath <see cref="XPath.Numeric.FloorFunction"/>.
    /// </summary>
    /// <param name="floor"></param>
    /// <returns></returns>
    T VisitFloorFunction(XPath.Numeric.FloorFunction floor);

    /// <summary>
    /// Process an XPath <see cref="XPath.Numeric.RoundFunction"/>.
    /// </summary>
    /// <param name="round"></param>
    /// <returns></returns>
    T VisitRoundFunction(XPath.Numeric.RoundFunction round);

    /// <summary>
    /// Process an XPath <see cref="RoundHalfToEvenFunction"/>.
    /// </summary>
    /// <param name="round"></param>
    /// <returns></returns>
    T VisitRoundHalfToEvenFunction(RoundHalfToEvenFunction round);

    /// <summary>
    /// Process an XPath <see cref="XPath.String.CompareFunction"/>.
    /// </summary>
    /// <param name="compare"></param>
    /// <returns></returns>
    T VisitCompareFunction(XPath.String.CompareFunction compare);

    /// <summary>
    /// Process an XPath <see cref="XPath.String.ConcatFunction "/>.
    /// </summary>
    /// <param name="concat"></param>
    /// <returns></returns>
    T VisitConcatFunction(XPath.String.ConcatFunction concat);

    /// <summary>
    /// Process an XPath <see cref="XPath.String.ContainsFunction"/>.
    /// </summary>
    /// <param name="contains"></param>
    /// <returns></returns>
    T VisitContainsFunction(XPath.String.ContainsFunction contains);

    /// <summary>
    /// Process an XPath <see cref="XPath.String.EncodeForUriFunction"/>.
    /// </summary>
    /// <param name="encodeForUri"></param>
    /// <returns></returns>
    T VisitEncodeForUriFunction(XPath.String.EncodeForUriFunction encodeForUri);

    /// <summary>
    /// Process an XPath <see cref="XPath.String.EndsWithFunction"/>.
    /// </summary>
    /// <param name="endsWith"></param>
    /// <returns></returns>
    T VisitEndsWithFunction(XPath.String.EndsWithFunction endsWith);

    /// <summary>
    /// Process an XPath <see cref="XPath.String.EscapeHtmlUriFunction"/>.
    /// </summary>
    /// <param name="escape"></param>
    /// <returns></returns>
    T VisitEscapeHtmlUriFunction(XPath.String.EscapeHtmlUriFunction escape);

    /// <summary>
    /// Process an XPath <see cref="XPath.String.LowerCaseFunction"/>.
    /// </summary>
    /// <param name="lCase"></param>
    /// <returns></returns>
    T VisitLowerCaseFunction(XPath.String.LowerCaseFunction lCase);

    /// <summary>
    /// Process an XPath <see cref="XPath.String.NormalizeSpaceFunction"/>.
    /// </summary>
    /// <param name="normalize"></param>
    /// <returns></returns>
    T VisitNormalizeSpaceFunction(XPath.String.NormalizeSpaceFunction normalize);

    /// <summary>
    /// Process an XPath <see cref="XPath.String.NormalizeUnicodeFunction"/>.
    /// </summary>
    /// <param name="normalize"></param>
    /// <returns></returns>
    T VisitNormalizeUnicodeFunction(XPath.String.NormalizeUnicodeFunction normalize);

    /// <summary>
    /// Process an XPath <see cref="XPath.String.ReplaceFunction"/>.
    /// </summary>
    /// <param name="replace"></param>
    /// <returns></returns>
    T VisitReplaceFunction(XPath.String.ReplaceFunction replace);

    /// <summary>
    /// Process an XPath <see cref="XPath.String.StartsWithFunction"/>.
    /// </summary>
    /// <param name="startsWith"></param>
    /// <returns></returns>
    T VisitStartsWithFunction(XPath.String.StartsWithFunction startsWith);

    /// <summary>
    /// Process an XPath <see cref="XPath.String.StringLengthFunction"/>.
    /// </summary>
    /// <param name="strLen"></param>
    /// <returns></returns>
    T VisitStringLengthFunction(XPath.String.StringLengthFunction strLen);

    /// <summary>
    /// Process an XPath <see cref="XPath.String.SubstringAfterFunction"/>.
    /// </summary>
    /// <param name="substringAfter"></param>
    /// <returns></returns>
    T VisitSubstringAfterFunction(XPath.String.SubstringAfterFunction substringAfter);

    /// <summary>
    /// Process an XPath <see cref="XPath.String.SubstringBeforeFunction"/>.
    /// </summary>
    /// <param name="substringBefore"></param>
    /// <returns></returns>
    T VisitSubstringBeforeFunction(XPath.String.SubstringBeforeFunction substringBefore);

    /// <summary>
    /// Process an XPath <see cref="XPath.String.SubstringFunction"/>.
    /// </summary>
    /// <param name="substring"></param>
    /// <returns></returns>
    T VisitSubstringFunction(XPath.String.SubstringFunction substring);

    /// <summary>
    /// Process an XPath <see cref="XPath.String.UpperCaseFunction"/>.
    /// </summary>
    /// <param name="uCase"></param>
    /// <returns></returns>
    T VisitUpperCaseFunction(XPath.String.UpperCaseFunction uCase);

    /// <summary>
    /// Process an XPath <see cref="BooleanFunction"/>.
    /// </summary>
    /// <param name="boolean"></param>
    /// <returns></returns>
    T VisitBooleanFunction(BooleanFunction boolean);

    /// <summary>
    /// Handle an unrecognized function.
    /// </summary>
    /// <param name="unknownFunction"></param>
    /// <returns></returns>
    T VisitUnknownFunction(UnknownFunction unknownFunction);

}