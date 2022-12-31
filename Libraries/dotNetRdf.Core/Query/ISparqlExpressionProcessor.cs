/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2021 dotNetRDF Project (http://dotnetrdf.org/)
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

namespace VDS.RDF.Query
{
    public interface ISparqlExpressionProcessor<out TResult, in TContext, in TBinding>
    {

        // Primary
        TResult ProcessAggregateTerm(AggregateTerm aggregate, TContext context, TBinding binding);
        TResult ProcessAllModifier(AllModifier all, TContext context, TBinding binding);
        TResult ProcessConstantTerm(ConstantTerm constant, TContext context, TBinding binding);
        TResult ProcessDistinctModifier(DistinctModifier distinct, TContext context, TBinding binding);
        TResult ProcessGraphPatternTerm(GraphPatternTerm graphPattern, TContext context, TBinding binding);
        TResult ProcessVariableTerm(VariableTerm variable, TContext context, TBinding binding);
        TResult ProcessTripleNodeTerm(TripleNodeTerm tripleNodeTerm, TContext context, TBinding binding);
        TResult ProcessAdditionExpression(AdditionExpression addition, TContext context, TBinding binding);

        TResult ProcessDivisionExpression(DivisionExpression division, TContext context, TBinding binding);

        TResult ProcessMinusExpression(MinusExpression minus, TContext context, TBinding binding);

        TResult ProcessMultiplicationExpression(MultiplicationExpression multiplication, TContext context,
            TBinding binding);

        TResult ProcessSubtractionExpression(SubtractionExpression subtraction, TContext context, TBinding binding);

        TResult ProcessEqualsExpression(EqualsExpression equals, TContext context, TBinding binding);
        TResult ProcessGreaterThanExpression(GreaterThanExpression gt, TContext context, TBinding binding);

        TResult ProcessGreaterThanOrEqualToExpression(GreaterThanOrEqualToExpression gte, TContext context,
            TBinding binding);

        TResult ProcessLessThanExpression(LessThanExpression lt, TContext context, TBinding binding);

        TResult ProcessLessThanOrEqualToExpression(LessThanOrEqualToExpression lte, TContext context,
            TBinding binding);

        TResult ProcessNotEqualsExpression(NotEqualsExpression ne, TContext context, TBinding binding);

        TResult ProcessAndExpression(AndExpression and, TContext context, TBinding binding);
        TResult ProcessNotExpression(NotExpression not, TContext context, TBinding binding);
        TResult ProcessOrExpression(OrExpression or, TContext context, TBinding binding);

        // ARQ Functions
        TResult ProcessArqBNodeFunction(BNodeFunction bNode, TContext context, TBinding binding);
        TResult ProcessEFunction(EFunction e, TContext context, TBinding binding);
        TResult ProcessLocalNameFunction(LocalNameFunction localName, TContext context, TBinding binding);
        TResult ProcessMaxFunction(MaxFunction max, TContext context, TBinding binding);
        TResult ProcessMinFunction(MinFunction min, TContext context, TBinding binding);
        TResult ProcessNamespaceFunction(NamespaceFunction ns, TContext context, TBinding binding);
        TResult ProcessNowFunction(NowFunction now, TContext context, TBinding binding);
        TResult ProcessPiFunction(PiFunction pi, TContext context, TBinding binding);
        TResult ProcessSha1Function(Sha1Function sha1, TContext context, TBinding binding);
        TResult ProcessStringJoinFunction(StringJoinFunction stringJoin, TContext context, TBinding binding);
        TResult ProcessSubstringFunction(SubstringFunction substring, TContext context, TBinding binding);

        // Leviathan Functions
        TResult ProcessLeviathanMD5HashFunction(Expressions.Functions.Leviathan.Hash.MD5HashFunction md5, TContext context, TBinding binding);
        TResult ProcessLeviathanSha256HashFunction(Expressions.Functions.Leviathan.Hash.Sha256HashFunction sha256, TContext context, TBinding binding);
        TResult ProcessCosecantFunction(CosecantFunction cosec, TContext context, TBinding binding);
        TResult ProcessCosineFunction(CosineFunction cos, TContext context, TBinding binding);
        TResult ProcessCotangentFunction(CotangentFunction cot, TContext context, TBinding binding);
        TResult ProcessDegreesToRadiansFunction(DegreesToRadiansFunction degToRad, TContext context, TBinding binding);
        TResult ProcessRadiansToDegreesFunction(RadiansToDegreesFunction radToDeg, TContext context, TBinding binding);
        TResult ProcessSecantFunction(SecantFunction sec, TContext context, TBinding binding);
        TResult ProcessSineFunction(SineFunction sin, TContext context, TBinding binding);
        TResult ProcessTangentFunction(TangentFunction tan, TContext context, TBinding binding);

        TResult ProcessCartesianFunction(CartesianFunction cart, TContext context, TBinding binding);
        TResult ProcessCubeFunction(CubeFunction cube, TContext context, TBinding binding);
        TResult ProcessLeviathanEFunction(Expressions.Functions.Leviathan.Numeric.EFunction eFunction, TContext context, TBinding binding);
        TResult ProcessFactorialFunction(FactorialFunction factorial, TContext context, TBinding binding);
        TResult ProcessLogFunction(LogFunction log, TContext context, TBinding binding);
        TResult ProcessNaturalLogFunction(LeviathanNaturalLogFunction logn, TContext context, TBinding binding);
        TResult ProcessPowerFunction(PowerFunction pow, TContext context, TBinding binding);
        TResult ProcessPythagoreanDistanceFunction(PythagoreanDistanceFunction pythagoreanDistance, TContext context, TBinding binding);
        TResult ProcessRandomFunction(RandomFunction rand, TContext context, TBinding binding);
        TResult ProcessReciprocalFunction(ReciprocalFunction reciprocal, TContext context, TBinding binding);
        TResult ProcessRootFunction(RootFunction root, TContext context, TBinding binding);
        TResult ProcessSquareFunction(SquareFunction square, TContext context, TBinding binding);
        TResult ProcessSquareRootFunction(SquareRootFunction sqrt, TContext context, TBinding binding);
        TResult ProcessTenFunction(TenFunction ten, TContext context, TBinding binding);

        // SPARQL functions
        TResult ProcessBoundFunction(BoundFunction bound, TContext context, TBinding binding);
        TResult ProcessExistsFunction(ExistsFunction exists, TContext context, TBinding binding);
        TResult ProcessIsBlankFunction(IsBlankFunction isBlank, TContext context, TBinding binding);
        TResult ProcessIsIriFunction(IsIriFunction isIri, TContext context, TBinding binding);
        TResult ProcessIsLiteralFunction(IsLiteralFunction isLiteral, TContext context, TBinding binding);
        TResult ProcessIsNumericFunction(IsNumericFunction isNumeric, TContext context, TBinding binding);
        TResult ProcessIsTripleFunction(IsTripleFunction isTripleFunction, TContext context, TBinding binding);
        TResult ProcessLangMatchesFunction(LangMatchesFunction langMatches, TContext context, TBinding binding);
        TResult ProcessRegexFunction(RegexFunction regex, TContext context, TBinding binding);
        TResult ProcessSameTermFunction(SameTermFunction sameTerm, TContext context, TBinding binding);
        TResult ProcessBNodeFunction(Expressions.Functions.Sparql.Constructor.BNodeFunction bNode, TContext context, TBinding binding);
        TResult ProcessIriFunction(IriFunction iri, TContext context, TBinding binding);
        TResult ProcessStrDtFunction(StrDtFunction strDt, TContext context, TBinding binding);
        TResult ProcessStrLangFunction(StrLangFunction strLang, TContext context, TBinding binding);
        TResult ProcessDayFunction(DayFunction day, TContext context, TBinding binding);
        TResult ProcessHoursFunction(HoursFunction hours, TContext context, TBinding binding);
        TResult ProcessMinutesFunction(MinutesFunction minutes, TContext context, TBinding binding);
        TResult ProcessMonthFunction(MonthFunction month, TContext context, TBinding binding);
        TResult ProcessNowFunction(Expressions.Functions.Sparql.DateTime.NowFunction now, TContext context, TBinding binding);
        TResult ProcessSecondsFunction(SecondsFunction seconds, TContext context, TBinding binding);
        TResult ProcessTimezoneFunction(TimezoneFunction timezone, TContext context, TBinding binding);
        TResult ProcessTZFunction(TZFunction tz, TContext context, TBinding binding);
        TResult ProcessYearFunction(YearFunction year, TContext context, TBinding binding);
        TResult ProcessMd5HashFunction(MD5HashFunction md5, TContext context, TBinding binding);
        TResult ProcessSha1HashFunction(Sha1HashFunction sha1, TContext context, TBinding binding);
        TResult ProcessSha256HashFunction(Sha256HashFunction sha256, TContext context, TBinding binding);
        TResult ProcessSha384HashFunction(Sha384HashFunction sha384, TContext context, TBinding binding);
        TResult ProcessSha512HashFunction(Sha512HashFunction sha512, TContext context, TBinding binding);
        TResult ProcessAbsFunction(AbsFunction abs, TContext context, TBinding binding);
        TResult ProcessCeilFunction(CeilFunction ceil, TContext context, TBinding binding);
        TResult ProcessFloorFunction(FloorFunction floor, TContext context, TBinding binding);
        TResult ProcessRandFunction(RandFunction rand, TContext context, TBinding binding);
        TResult ProcessRoundFunction(RoundFunction round, TContext context, TBinding binding);
        TResult ProcessInFunction(InFunction inFn, TContext context, TBinding binding);
        TResult ProcessNotInFunction(NotInFunction notIn, TContext context, TBinding binding);
        TResult ProcessConcatFunction(ConcatFunction concat, TContext context, TBinding binding);
        TResult ProcessContainsFunction(ContainsFunction contains, TContext context, TBinding binding);
        TResult ProcessDataTypeFunction(DataTypeFunction dataType, TContext context, TBinding binding);
        TResult ProcessDataType11Function(DataType11Function dataType, TContext context, TBinding binding);
        TResult ProcessEncodeForUriFunction(EncodeForUriFunction encodeForUri, TContext context, TBinding binding);
        TResult ProcessLangFunction(LangFunction lang, TContext context, TBinding binding);
        TResult ProcessLCaseFunction(LCaseFunction lCase, TContext context, TBinding binding);
        TResult ProcessReplaceFunction(ReplaceFunction replace, TContext context, TBinding binding);
        TResult ProcessStrAfterFunction(StrAfterFunction strAfter, TContext context, TBinding binding);
        TResult ProcessStrBeforeFunction(StrBeforeFunction strBefore, TContext context, TBinding binding);
        TResult ProcessStrEndsFunction(StrEndsFunction strEnds, TContext context, TBinding binding);
        TResult ProcessStrFunction(StrFunction str, TContext context, TBinding binding);
        TResult ProcessStrLenFunction(StrLenFunction strLen, TContext context, TBinding binding);
        TResult ProcessStrStartsFunction(StrStartsFunction strStarts, TContext context, TBinding binding);
        TResult ProcessSubStrFunction(SubStrFunction subStr, TContext context, TBinding binding);
        TResult ProcessUCaseFunction(UCaseFunction uCase, TContext context, TBinding binding);
        TResult ProcessUuidFunction(UUIDFunction uuid, TContext context, TBinding binding);
        TResult ProcessStrUuidFunction(StrUUIDFunction uuid, TContext context, TBinding binding);
        TResult ProcessCallFunction(CallFunction call, TContext context, TBinding binding);
        TResult ProcessCoalesceFunction(CoalesceFunction coalesce, TContext context, TBinding binding);
        TResult ProcessIfElseFunction(IfElseFunction ifElse, TContext context, TBinding binding);

        // Triple Node Functions
        TResult ProcessSubjectFunction(SubjectFunction subjectFunction, TContext context, TBinding binding);
        TResult ProcessPredicateFunction(PredicateFunction predicateFunction, TContext context, TBinding binding);
        TResult ProcessObjectFunction(ObjectFunction objectFunction, TContext context, TBinding binding);


        // XPath Functions
        TResult ProcessBooleanCast(BooleanCast cast, TContext context, TBinding binding);
        TResult ProcessDateTimeCast(DateTimeCast cast, TContext context, TBinding binding);
        TResult ProcessDecimalCast(DecimalCast cast, TContext context, TBinding binding);
        TResult ProcessDoubleCast(DoubleCast cast, TContext context, TBinding binding);
        TResult ProcessFloatCast(FloatCast cast, TContext context, TBinding binding);
        TResult ProcessIntegerCast(IntegerCast cast, TContext context, TBinding binding);
        TResult ProcessStringCast(StringCast cast, TContext context, TBinding binding);
        TResult ProcessDayFromDateTimeFunction(DayFromDateTimeFunction day, TContext context, TBinding binding);
        TResult ProcessHoursFromDateTimeFunction(HoursFromDateTimeFunction hours, TContext context, TBinding binding);
        TResult ProcessMinutesFromDateTimeFunction(MinutesFromDateTimeFunction minutes, TContext context, TBinding binding);
        TResult ProcessMonthFromDateTimeFunction(MonthFromDateTimeFunction month, TContext context, TBinding binding);
        TResult ProcessSecondsFromDateTimeFunction(SecondsFromDateTimeFunction seconds, TContext context, TBinding binding);
        TResult ProcessTimezoneFromDateTimeFunction(TimezoneFromDateTimeFunction timezone, TContext context, TBinding binding);
        TResult ProcessYearsFromDateTimeFunction(YearFromDateTimeFunction years, TContext context, TBinding binding);
        TResult ProcessAbsFunction(XPath.Numeric.AbsFunction abs, TContext context, TBinding binding);
        TResult ProcessCeilFunction(XPath.Numeric.CeilingFunction ceil, TContext context, TBinding binding);
        TResult ProcessFloorFunction(XPath.Numeric.FloorFunction floor, TContext context, TBinding binding);
        TResult ProcessRoundFunction(Expressions.Functions.XPath.Numeric.RoundFunction round, TContext context, TBinding binding);
        TResult ProcessRoundHalfToEvenFunction(Expressions.Functions.XPath.Numeric.RoundHalfToEvenFunction round, TContext context, TBinding binding);
        TResult ProcessCompareFunction(CompareFunction compare, TContext context, TBinding binding);
        TResult ProcessConcatFunction(Expressions.Functions.XPath.String.ConcatFunction concat, TContext context, TBinding binding);
        TResult ProcessContainsFunction(Expressions.Functions.XPath.String.ContainsFunction contains, TContext context, TBinding binding);
        TResult ProcessEncodeForUriFunction(Expressions.Functions.XPath.String.EncodeForUriFunction encodeForUri, TContext context, TBinding binding);
        TResult ProcessEndsWithFunction(Expressions.Functions.XPath.String.EndsWithFunction endsWith, TContext context, TBinding binding);
        TResult ProcessEscapeHtmlUriFunction(Expressions.Functions.XPath.String.EscapeHtmlUriFunction escape, TContext context, TBinding binding);
        TResult ProcessLowerCaseFunction(Expressions.Functions.XPath.String.LowerCaseFunction lCase, TContext context, TBinding binding);
        TResult ProcessNormalizeSpaceFunction(Expressions.Functions.XPath.String.NormalizeSpaceFunction normalize, TContext context, TBinding binding);
        TResult ProcessNormalizeUnicodeFunction(Expressions.Functions.XPath.String.NormalizeUnicodeFunction normalize, TContext context, TBinding binding);
        TResult ProcessReplaceFunction(Expressions.Functions.XPath.String.ReplaceFunction replace, TContext context, TBinding binding);
        TResult ProcessStartsWithFunction(Expressions.Functions.XPath.String.StartsWithFunction startsWith, TContext context, TBinding binding);
        TResult ProcessStringLengthFunction(Expressions.Functions.XPath.String.StringLengthFunction strLen, TContext context, TBinding binding);
        TResult ProcessSubstringAfterFunction(Expressions.Functions.XPath.String.SubstringAfterFunction substringAfter, TContext context, TBinding binding);
        TResult ProcessSubstringBeforeFunction(Expressions.Functions.XPath.String.SubstringBeforeFunction substringBefore, TContext context, TBinding binding);
        TResult ProcessSubstringFunction(Expressions.Functions.XPath.String.SubstringFunction substring, TContext context, TBinding binding);
        TResult ProcessUpperCaseFunction(Expressions.Functions.XPath.String.UpperCaseFunction uCase, TContext context, TBinding binding);
        TResult ProcessBooleanFunction(BooleanFunction boolean, TContext context, TBinding binding);

        // Generic unrecognized function
        TResult ProcessUnknownFunction(UnknownFunction unknownFunction, TContext context, TBinding binding);
    }
}
