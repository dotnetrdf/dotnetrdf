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

namespace VDS.RDF.Query
{
    /// <summary>
    /// The interface to be implemented by an object that can be accepted by an ISparqlExpression instance.
    /// </summary>
    /// <typeparam name="T">The type returned by the visit methods of the visitor.</typeparam>
    public interface ISparqlExpressionVisitor<out T>
    {
        // Primary
        T VisitAggregateTerm(AggregateTerm aggregate);
        T VisitAllModifier(AllModifier all);
        T VisitConstantTerm(ConstantTerm constant);
        T VisitDistinctModifier(DistinctModifier distinct);
        T VisitGraphPatternTerm(GraphPatternTerm graphPattern);
        T VisitVariableTerm(VariableTerm variable);

        // Arithmetic
        T VisitAdditionExpression(AdditionExpression expr);
        T VisitDivisionExpression(DivisionExpression divisionExpression);
        T VisitMultiplicationExpression(MultiplicationExpression multiplicationExpression);
        T VisitSubtractionExpression(SubtractionExpression subtractionExpression);
        T VisitMinusExpression(MinusExpression minusExpression);

        T VisitEqualsExpression(EqualsExpression equals);
        T VisitGreaterThanExpression(GreaterThanExpression gt);

        T VisitGreaterThanOrEqualToExpression(GreaterThanOrEqualToExpression gte);

        T VisitLessThanExpression(LessThanExpression lt);

        T VisitLessThanOrEqualToExpression(LessThanOrEqualToExpression lte);

        T VisitNotEqualsExpression(NotEqualsExpression ne);

        T VisitAndExpression(AndExpression and);
        T VisitNotExpression(NotExpression not);
        T VisitOrExpression(OrExpression or);

        // ARQ Functions
        T VisitArqBNodeFunction(ArqFunctions.BNodeFunction bNode);
        T VisitEFunction(ArqFunctions.EFunction e);
        T VisitLocalNameFunction(ArqFunctions.LocalNameFunction localName);
        T VisitMaxFunction(ArqFunctions.MaxFunction max);
        T VisitMinFunction(ArqFunctions.MinFunction min);
        T VisitNamespaceFunction(ArqFunctions.NamespaceFunction ns);
        T VisitNowFunction(ArqFunctions.NowFunction now);
        T VisitPiFunction(ArqFunctions.PiFunction pi);
        T VisitSha1Function(ArqFunctions.Sha1Function sha1);
        T VisitStringJoinFunction(ArqFunctions.StringJoinFunction stringJoin);
        T VisitSubstringFunction(ArqFunctions.SubstringFunction substring);

        // Leviathan Functions
        T VisitLeviathanMD5HashFunction(LeviathanHash.MD5HashFunction md5);
        T VisitLeviathanSha256HashFunction(LeviathanHash.Sha256HashFunction sha256);
        T VisitCosecantFunction(CosecantFunction cosec);
        T VisitCosineFunction(CosineFunction cos);
        T VisitCotangentFunction(CotangentFunction cot);
        T VisitDegreesToRadiansFunction(DegreesToRadiansFunction degToRad);
        T VisitRadiansToDegreesFunction(RadiansToDegreesFunction radToDeg);
        T VisitSecantFunction(SecantFunction sec);
        T VisitSineFunction(SineFunction sin);
        T VisitTangentFunction(TangentFunction tan);

        T VisitCartesianFunction(CartesianFunction cart);
        T VisitCubeFunction(CubeFunction cube);
        T VisitLeviathanEFunction(LeviathanNumeric.EFunction eFunction);
        T VisitFactorialFunction(FactorialFunction factorial);
        T VisitLogFunction(LogFunction log);
        T VisitNaturalLogFunction(LeviathanNaturalLogFunction logn);
        T VisitPowerFunction(PowerFunction pow);
        T VisitPythagoreanDistanceFunction(PythagoreanDistanceFunction pythagoreanDistance);
        T VisitRandomFunction(RandomFunction rand);
        T VisitReciprocalFunction(ReciprocalFunction reciprocal);
        T VisitRootFunction(RootFunction root);
        T VisitSquareFunction(SquareFunction square);
        T VisitSquareRootFunction(SquareRootFunction sqrt);
        T VisitTenFunction(TenFunction ten);

        // SPARQL functions
        T VisitBoundFunction(BoundFunction bound);
        T VisitExistsFunction(ExistsFunction exists);
        T VisitIsBlankFunction(IsBlankFunction isBlank);
        T VisitIsIriFunction(IsIriFunction isIri);
        T VisitIsLiteralFunction(IsLiteralFunction isLiteral);
        T VisitIsNumericFunction(IsNumericFunction isNumeric);
        T VisitIsTripleFunction(IsTripleFunction isTripleFunction);
        T VisitLangMatchesFunction(LangMatchesFunction langMatches);
        T VisitRegexFunction(RegexFunction regex);
        T VisitSameTermFunction(SameTermFunction sameTerm);
        T VisitBNodeFunction(BNodeFunction bNode);
        T VisitIriFunction(IriFunction iri);
        T VisitStrDtFunction(StrDtFunction strDt);
        T VisitStrLangFunction(StrLangFunction strLang);
        T VisitDayFunction(DayFunction day);
        T VisitHoursFunction(HoursFunction hours);
        T VisitMinutesFunction(MinutesFunction minutes);
        T VisitMonthFunction(MonthFunction month);
        T VisitNowFunction(NowFunction now);
        T VisitSecondsFunction(SecondsFunction seconds);
        T VisitTimezoneFunction(TimezoneFunction timezone);
        T VisitTZFunction(TZFunction tz);
        T VisitYearFunction(YearFunction year);
        T VisitMd5HashFunction(MD5HashFunction md5);
        T VisitSha1HashFunction(Sha1HashFunction sha1);
        T VisitSha256HashFunction(Sha256HashFunction sha256);
        T VisitSha384HashFunction(Sha384HashFunction sha384);
        T VisitSha512HashFunction(Sha512HashFunction sha512);
        T VisitAbsFunction(AbsFunction abs);
        T VisitCeilFunction(CeilFunction ceil);
        T VisitFloorFunction(FloorFunction floor);
        T VisitRandFunction(RandFunction rand);
        T VisitRoundFunction(RoundFunction round);
        T VisitInFunction(InFunction inFn);
        T VisitNotInFunction(NotInFunction notIn);
        T VisitConcatFunction(ConcatFunction concat);
        T VisitContainsFunction(ContainsFunction contains);
        T VisitDataTypeFunction(DataTypeFunction dataType);
        T VisitDataType11Function(DataType11Function dataType);
        T VisitEncodeForUriFunction(EncodeForUriFunction encodeForUri);
        T VisitLangFunction(LangFunction lang);
        T VisitLCaseFunction(LCaseFunction lCase);
        T VisitReplaceFunction(ReplaceFunction replace);
        T VisitStrAfterFunction(StrAfterFunction strAfter);
        T VisitStrBeforeFunction(StrBeforeFunction strBefore);
        T VisitStrEndsFunction(StrEndsFunction strEnds);
        T VisitStrFunction(StrFunction str);
        T VisitStrLenFunction(StrLenFunction strLen);
        T VisitStrStartsFunction(StrStartsFunction strStarts);
        T VisitSubStrFunction(SubStrFunction subStr);
        T VisitUCaseFunction(UCaseFunction uCase);
        T VisitUuidFunction(UUIDFunction uuid);
        T VisitStrUuidFunction(StrUUIDFunction uuid);
        T VisitCallFunction(CallFunction call);
        T VisitCoalesceFunction(CoalesceFunction coalesce);
        T VisitIfElseFunction(IfElseFunction ifElse);

        // Triple Node Functions
        T VisitSubjectFunction(SubjectFunction subjectFunction);
        T VisitPredicateFunction(PredicateFunction predicateFunction);
        T VisitObjectFunction(ObjectFunction objectFunction);

        // XPath Functions
        T VisitBooleanCast(BooleanCast cast);
        T VisitDateTimeCast(DateTimeCast cast);
        T VisitDecimalCast(DecimalCast cast);
        T VisitDoubleCast(DoubleCast cast);
        T VisitFloatCast(FloatCast cast);
        T VisitIntegerCast(IntegerCast cast);
        T VisitStringCast(StringCast cast);
        T VisitDayFromDateTimeFunction(DayFromDateTimeFunction day);
        T VisitHoursFromDateTimeFunction(HoursFromDateTimeFunction hours);
        T VisitMinutesFromDateTimeFunction(MinutesFromDateTimeFunction minutes);
        T VisitMonthFromDateTimeFunction(MonthFromDateTimeFunction month);
        T VisitSecondsFromDateTimeFunction(SecondsFromDateTimeFunction seconds);
        T VisitTimezoneFromDateTimeFunction(TimezoneFromDateTimeFunction timezone);
        T VisitYearsFromDateTimeFunction(YearFromDateTimeFunction years);
        T VisitAbsFunction(XPath.Numeric.AbsFunction abs);
        T VisitCeilFunction(CeilingFunction ceil);
        T VisitFloorFunction(XPath.Numeric.FloorFunction floor);
        T VisitRoundFunction(XPath.Numeric.RoundFunction round);
        T VisitRoundHalfToEvenFunction(XPath.Numeric.RoundHalfToEvenFunction round);
        T VisitCompareFunction(XPath.String.CompareFunction compare);
        T VisitConcatFunction(XPath.String.ConcatFunction concat);
        T VisitContainsFunction(XPath.String.ContainsFunction contains);
        T VisitEncodeForUriFunction(XPath.String.EncodeForUriFunction encodeForUri);
        T VisitEndsWithFunction(XPath.String.EndsWithFunction endsWith);
        T VisitEscapeHtmlUriFunction(XPath.String.EscapeHtmlUriFunction escape);
        T VisitLowerCaseFunction(XPath.String.LowerCaseFunction lCase);
        T VisitNormalizeSpaceFunction(XPath.String.NormalizeSpaceFunction normalize);
        T VisitNormalizeUnicodeFunction(XPath.String.NormalizeUnicodeFunction normalize);
        T VisitReplaceFunction(XPath.String.ReplaceFunction replace);
        T VisitStartsWithFunction(XPath.String.StartsWithFunction startsWith);
        T VisitStringLengthFunction(XPath.String.StringLengthFunction strLen);
        T VisitSubstringAfterFunction(XPath.String.SubstringAfterFunction substringAfter);
        T VisitSubstringBeforeFunction(XPath.String.SubstringBeforeFunction substringBefore);
        T VisitSubstringFunction(XPath.String.SubstringFunction substring);
        T VisitUpperCaseFunction(XPath.String.UpperCaseFunction uCase);
        T VisitBooleanFunction(BooleanFunction boolean);

        // Generic unrecognized function
        T VisitUnknownFunction(UnknownFunction unknownFunction);

    }

}