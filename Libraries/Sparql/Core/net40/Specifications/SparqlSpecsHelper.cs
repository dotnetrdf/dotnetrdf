/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using VDS.Common.References;
using VDS.RDF.Nodes;
using VDS.RDF.Parsing;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Specifications
{
    /// <summary>
    /// Class containing Helper information and methods pertaining to the Sparql Query Language for RDF
    /// </summary>
    public static class SparqlSpecsHelper
    {
        static SparqlSpecsHelper()
        {
            // TODO May want to use a different node formatter here
            Formatter = new AlgebraFormatter();
        }

        #region Keywords and Constants

        /// <summary>
        /// Namespace Uri for SPARQL Namespace
        /// </summary>
        public const String SparqlNamespace = "http://www.w3.org/2005/sparql-results#";
        /// <summary>
        /// Namespace Uri for the RDF serialization of a SPARQL Result Set
        /// </summary>
        public const String SparqlRdfResultsNamespace = "http://www.w3.org/2001/sw/DataAccess/tests/result-set#";

        /// <summary>
        /// Keywords in Sparql
        /// </summary>
        public const String SparqlKeywordBase = "BASE",
                      SparqlKeywordPrefix = "PREFIX",
                      SparqlKeywordSelect = "SELECT",
                      SparqlKeywordConstruct = "CONSTRUCT",
                      SparqlKeywordDescribe = "DESCRIBE",
                      SparqlKeywordAsk = "ASK",
                      SparqlKeywordOrder = "ORDER",
                      SparqlKeywordBy = "BY",
                      SparqlKeywordOrderBy = "ORDER BY",
                      SparqlKeywordLimit = "LIMIT",
                      SparqlKeywordOffset = "OFFSET",
                      SparqlKeywordDistinct = "DISTINCT",
                      SparqlKeywordReduced = "REDUCED",
                      SparqlKeywordFrom = "FROM",
                      SparqlKeywordNamed = "NAMED",
                      SparqlKeywordFromNamed = "FROM NAMED",
                      SparqlKeywordWhere = "WHERE",
                      SparqlKeywordGraph = "GRAPH",
                      SparqlKeywordOptional = "OPTIONAL",
                      SparqlKeywordUnion = "UNION",
                      SparqlKeywordFilter = "FILTER",
                      SparqlKeywordStr = "STR",
                      SparqlKeywordLang = "LANG",
                      SparqlKeywordLangMatches = "LANGMATCHES",
                      SparqlKeywordDataType = "DATATYPE",
                      SparqlKeywordBound = "BOUND",
                      SparqlKeywordSameTerm = "SAMETERM",
                      SparqlKeywordIsUri = "ISURI",
                      SparqlKeywordIsIri = "ISIRI",
                      SparqlKeywordIsLiteral = "ISLITERAL",
                      SparqlKeywordIsBlank = "ISBLANK",
                      SparqlKeywordRegex = "REGEX",
                      SparqlKeywordTrue = "true",
                      SparqlKeywordFalse = "false",
                      SparqlKeywordAsc = "ASC",
                      SparqlKeywordDesc = "DESC",
                      SparqlKeywordCount = "COUNT",
                      SparqlKeywordSum = "SUM",
                      SparqlKeywordAvg = "AVG",
                      SparqlKeywordMin = "MIN",
                      SparqlKeywordMax = "MAX",
                      SparqlKeywordGroupConcat = "GROUP_CONCAT",
                      SparqlKeywordSample = "SAMPLE",
                      SparqlKeywordNMin = "NMIN",
                      SparqlKeywordNMax = "NMAX",
                      SparqlKeywordMedian = "MEDIAN",
                      SparqlKeywordMode = "MODE",
                      SparqlKeywordAs = "AS",
                      SparqlKeywordGroupBy = "GROUP BY",
                      SparqlKeywordGroup = "GROUP",
                      SparqlKeywordHaving = "HAVING",
                      SparqlKeywordExists = "EXISTS",
                      SparqlKeywordNot = "NOT",
                      SparqlKeywordNotExists = "NOT EXISTS",
                      SparqlKeywordUnsaid = "UNSAID",
                      SparqlKeywordLet = "LET",
                      SparqlKeywordBind = "BIND",
                      SparqlKeywordSeparator = "SEPARATOR",
                      SparqlKeywordLength = "LENGTH",
                      SparqlKeywordStrLang = "STRLANG",
                      SparqlKeywordStrDt = "STRDT",
                      SparqlKeywordIri = "IRI",
                      SparqlKeywordUri = "URI",
                      SparqlKeywordBNode = "BNODE",
                      SparqlKeywordIn = "IN",
                      SparqlKeywordNotIn = "NOT IN",
                      SparqlKeywordCoalesce = "COALESCE",
                      SparqlKeywordIf = "IF",
                      SparqlKeywordInsert = "INSERT",
                      SparqlKeywordDelete = "DELETE",
                      SparqlKeywordClear = "CLEAR",
                      SparqlKeywordLoad = "LOAD",
                      SparqlKeywordData = "DATA",
                      SparqlKeywordInto = "INTO",
                      SparqlKeywordSilent = "SILENT",
                      SparqlKeywordCreate = "CREATE",
                      SparqlKeywordDrop = "DROP",
                      SparqlKeywordWith = "WITH",
                      SparqlKeywordUsing = "USING",
                      SparqlKeywordDefault = "DEFAULT",
                      SparqlKeywordAll = "ALL",
                      SparqlKeywordMinus = "MINUS",
                      SparqlKeywordService = "SERVICE",
                      SparqlKeywordBindings = "BINDINGS",
                      SparqlKeywordValues = "VALUES",
                      SparqlKeywordUndef = "UNDEF",
                      SparqlKeywordIsNumeric = "ISNUMERIC",
                      SparqlKeywordStrLen = "STRLEN",
                      SparqlKeywordSubStr = "SUBSTR",
                      SparqlKeywordUCase = "UCASE",
                      SparqlKeywordLCase = "LCASE",
                      SparqlKeywordStrStarts = "STRSTARTS",
                      SparqlKeywordStrEnds = "STRENDS",
                      SparqlKeywordStrBefore = "STRBEFORE",
                      SparqlKeywordStrAfter = "STRAFTER",
                      SparqlKeywordReplace = "REPLACE",
                      SparqlKeywordContains = "CONTAINS",
                      SparqlKeywordEncodeForUri = "ENCODE_FOR_URI",
                      SparqlKeywordConcat = "CONCAT",
                      SparqlKeywordAbs = "ABS",
                      SparqlKeywordRound = "ROUND",
                      SparqlKeywordCeil = "CEIL",
                      SparqlKeywordFloor = "FLOOR",
                      SparqlKeywordRand = "RAND",
                      SparqlKeywordNow = "NOW",
                      SparqlKeywordYear = "YEAR",
                      SparqlKeywordMonth = "MONTH",
                      SparqlKeywordDay = "DAY",
                      SparqlKeywordHours = "HOURS",
                      SparqlKeywordMinutes = "MINUTES",
                      SparqlKeywordSeconds = "SECONDS",
                      SparqlKeywordTimezone = "TIMEZONE",
                      SparqlKeywordTz = "TZ",
                      SparqlKeywordMD5 = "MD5",
                      SparqlKeywordSha1 = "SHA1",
                      SparqlKeywordSha256 = "SHA256",
                      SparqlKeywordSha384 = "SHA384",
                      SparqlKeywordSha512 = "SHA512",
                      SparqlKeywordAny = "ANY",
                      SparqlKeywordNone = "NONE",
                      SparqlKeywordAdd = "ADD",
                      SparqlKeywordCopy = "COPY",
                      SparqlKeywordMove = "MOVE",
                      SparqlKeywordTo = "TO",
                      SparqlKeywordUUID = "UUID",
                      SparqlKeywordStrUUID = "STRUUID",
                      SparqlKeywordCall = "CALL"
                      ;

        /// <summary>
        /// Set of SPARQL Keywords that are Non-Query Keywords
        /// </summary>
        public static String[] NonQueryKeywords = {   
                                                      SparqlKeywordOrder, 
                                                      SparqlKeywordBy, 
                                                      SparqlKeywordLimit, 
                                                      SparqlKeywordOffset, 
                                                      SparqlKeywordDistinct, 
                                                      SparqlKeywordReduced, 
                                                      SparqlKeywordFrom, 
                                                      SparqlKeywordNamed, 
                                                      SparqlKeywordWhere, 
                                                      SparqlKeywordOptional, 
                                                      SparqlKeywordUnion, 
                                                      SparqlKeywordFilter, 
                                                      SparqlKeywordGraph, 
                                                      SparqlKeywordGroup, 
                                                      SparqlKeywordGroupBy, 
                                                      SparqlKeywordHaving, 
                                                      SparqlKeywordLet, 
                                                      SparqlKeywordBind,
                                                      SparqlKeywordExists, 
                                                      SparqlKeywordNotExists, 
                                                      SparqlKeywordUnsaid, 
                                                      SparqlKeywordNot,
                                                      SparqlKeywordLength,
                                                      SparqlKeywordMinus,
                                                      SparqlKeywordService,
                                                      SparqlKeywordBindings,
                                                      SparqlKeywordValues,
                                                      SparqlKeywordUndef,
                                                      SparqlKeywordDefault,
                                                  };
        /// <summary>
        /// Set of SPARQL Keywords that are Function Keywords
        /// </summary>
        public static String[] FunctionKeywords = {   
                                                      SparqlKeywordAbs,
                                                      SparqlKeywordBNode,
                                                      SparqlKeywordBound,
                                                      SparqlKeywordCall,
                                                      SparqlKeywordCeil,
                                                      SparqlKeywordCoalesce,
                                                      SparqlKeywordConcat,
                                                      SparqlKeywordDataType, 
                                                      SparqlKeywordDay,
                                                      SparqlKeywordEncodeForUri,
                                                      SparqlKeywordFloor,
                                                      SparqlKeywordHours,
                                                      SparqlKeywordIf,
                                                      SparqlKeywordIn,
                                                      SparqlKeywordIri,
                                                      SparqlKeywordIsBlank, 
                                                      SparqlKeywordIsIri, 
                                                      SparqlKeywordIsLiteral, 
                                                      SparqlKeywordIsNumeric,
                                                      SparqlKeywordIsUri, 
                                                      SparqlKeywordLang,
                                                      SparqlKeywordLangMatches, 
                                                      SparqlKeywordLCase,
                                                      SparqlKeywordMD5,
                                                      SparqlKeywordMinutes,
                                                      SparqlKeywordMonth,
                                                      SparqlKeywordNotIn,
                                                      SparqlKeywordNow,
                                                      SparqlKeywordRand,
                                                      SparqlKeywordRegex, 
                                                      SparqlKeywordRound,
                                                      SparqlKeywordSameTerm, 
                                                      SparqlKeywordSeconds,
                                                      SparqlKeywordSha1,
                                                      SparqlKeywordSha256,
                                                      SparqlKeywordSha384,
                                                      SparqlKeywordSha512,
                                                      SparqlKeywordStr,
                                                      SparqlKeywordContains,
                                                      SparqlKeywordReplace,
                                                      SparqlKeywordStrAfter,
                                                      SparqlKeywordStrBefore,
                                                      SparqlKeywordStrDt,
                                                      SparqlKeywordStrEnds,
                                                      SparqlKeywordStrLang,
                                                      SparqlKeywordStrLen,
                                                      SparqlKeywordStrStarts,
                                                      SparqlKeywordStrUUID,
                                                      SparqlKeywordSubStr,
                                                      SparqlKeywordTimezone,
                                                      SparqlKeywordTz,
                                                      SparqlKeywordUCase,
                                                      SparqlKeywordUri,
                                                      SparqlKeywordUUID,
                                                      SparqlKeywordYear
                                                  };

        /// <summary>
        /// Set of SPARQL Keywords that are Aggregate Keywords
        /// </summary>
        /// <remarks>
        /// Unlike <see cref="SparqlSpecsHelper.AggregateFunctionKeywords">AggregateFunctionKeywords[]</see> this includes keywords related to aggregates (like DISTINCT) and those for Leviathan extension aggregates which are not standard SPARQL 1.1 syntax
        /// </remarks>
        public static String[] AggregateKeywords = {   
                                                       SparqlKeywordAll,
                                                       SparqlKeywordAny,
                                                       SparqlKeywordAvg, 
                                                       SparqlKeywordCount, 
                                                       SparqlKeywordDistinct,
                                                       SparqlKeywordMax, 
                                                       SparqlKeywordMedian,
                                                       SparqlKeywordMin, 
                                                       SparqlKeywordMode,
                                                       SparqlKeywordNone,
                                                       SparqlKeywordNMin, 
                                                       SparqlKeywordNMax, 
                                                       SparqlKeywordSum, 
                                                       SparqlKeywordAs,
                                                       SparqlKeywordGroupConcat,
                                                       SparqlKeywordSample,
                                                       SparqlKeywordSeparator
                                                   };

        /// <summary>
        /// Set of SPARQL Keywords that are built in SPARQL Aggregate Functions
        /// </summary>
        public static String[] AggregateFunctionKeywords = {
                                                                SparqlKeywordAvg,
                                                                SparqlKeywordCount,
                                                                SparqlKeywordGroupConcat,
                                                                SparqlKeywordMax,
                                                                SparqlKeywordMin,
                                                                SparqlKeywordSum,
                                                                SparqlKeywordSample
                                                           };

        /// <summary>
        /// Set of IRIs for supported Cast Functions
        /// </summary>
        public static String[] SupportedCastFunctions = {   
                                                            XmlSpecsHelper.XmlSchemaDataTypeBoolean, 
                                                            XmlSpecsHelper.XmlSchemaDataTypeDateTime, 
                                                            XmlSpecsHelper.XmlSchemaDataTypeDecimal, 
                                                            XmlSpecsHelper.XmlSchemaDataTypeDouble, 
                                                            XmlSpecsHelper.XmlSchemaDataTypeFloat, 
                                                            XmlSpecsHelper.XmlSchemaDataTypeInteger, 
                                                            XmlSpecsHelper.XmlSchemaDataTypeString 
                                                        };

        /// <summary>
        /// Set of Keywords for SPARQL Query 1.0
        /// </summary>
        public static String[] SparqlQuery10Keywords = {
                                                        SparqlKeywordAsc,
                                                        SparqlKeywordAsk,
                                                        SparqlKeywordBase,
                                                        SparqlKeywordBound,
                                                        SparqlKeywordBy,
                                                        SparqlKeywordConstruct,
                                                        SparqlKeywordDataType,
                                                        SparqlKeywordDesc,
                                                        SparqlKeywordDescribe,
                                                        SparqlKeywordDistinct,
                                                        SparqlKeywordFilter,
                                                        SparqlKeywordFrom,
                                                        SparqlKeywordFromNamed,
                                                        SparqlKeywordGraph,
                                                        SparqlKeywordIsBlank,
                                                        SparqlKeywordIsIri,
                                                        SparqlKeywordIsLiteral,
                                                        SparqlKeywordIsUri,
                                                        SparqlKeywordLang,
                                                        SparqlKeywordLangMatches,
                                                        SparqlKeywordLimit,
                                                        SparqlKeywordNamed,
                                                        SparqlKeywordOffset,
                                                        SparqlKeywordOptional,
                                                        SparqlKeywordOrder,
                                                        SparqlKeywordOrderBy,
                                                        SparqlKeywordPrefix,
                                                        SparqlKeywordReduced,
                                                        SparqlKeywordRegex,
                                                        SparqlKeywordSameTerm,
                                                        SparqlKeywordSelect,
                                                        SparqlKeywordStr,
                                                        SparqlKeywordUnion,
                                                        SparqlKeywordWhere,
                                                       };

        /// <summary>
        /// Set of additional Keywords for SPARQL Query 1.1
        /// </summary>
        public static String[] SparqlQuery11Keywords = {
                                                        SparqlKeywordAbs,
                                                        SparqlKeywordAs,
                                                        SparqlKeywordAvg,
                                                        SparqlKeywordBNode,
                                                        SparqlKeywordCeil,
                                                        SparqlKeywordCoalesce,
                                                        SparqlKeywordConcat,
                                                        SparqlKeywordCount,
                                                        SparqlKeywordDay,
                                                        SparqlKeywordDefault,
                                                        SparqlKeywordEncodeForUri,
                                                        SparqlKeywordExists,
                                                        SparqlKeywordFloor,
                                                        SparqlKeywordGroup,
                                                        SparqlKeywordGroupBy,
                                                        SparqlKeywordGroupConcat,
                                                        SparqlKeywordHaving,
                                                        SparqlKeywordHours,
                                                        SparqlKeywordIf,
                                                        SparqlKeywordIn,
                                                        SparqlKeywordIri,
                                                        SparqlKeywordIsNumeric,
                                                        SparqlKeywordLCase,
                                                        SparqlKeywordMax,
                                                        SparqlKeywordMD5,
                                                        SparqlKeywordMin,
                                                        SparqlKeywordMinutes,
                                                        SparqlKeywordMinus,
                                                        SparqlKeywordMonth,
                                                        SparqlKeywordNot,
                                                        SparqlKeywordNotExists,
                                                        SparqlKeywordNotIn,
                                                        SparqlKeywordNow,
                                                        SparqlKeywordRand,
                                                        SparqlKeywordRound,
                                                        SparqlKeywordSample,
                                                        SparqlKeywordSeconds,
                                                        SparqlKeywordService,
                                                        SparqlKeywordSha1,
                                                        SparqlKeywordSha256,
                                                        SparqlKeywordSha384,
                                                        SparqlKeywordSha512,
                                                        SparqlKeywordContains,
                                                        SparqlKeywordReplace,
                                                        SparqlKeywordStrAfter,
                                                        SparqlKeywordStrBefore,
                                                        SparqlKeywordStrDt,
                                                        SparqlKeywordStrLang,
                                                        SparqlKeywordStrLen,
                                                        SparqlKeywordStrEnds,
                                                        SparqlKeywordStrStarts,
                                                        SparqlKeywordStrUUID,
                                                        SparqlKeywordSubStr,
                                                        SparqlKeywordSum,
                                                        SparqlKeywordTimezone,
                                                        SparqlKeywordTz,
                                                        SparqlKeywordUCase,
                                                        SparqlKeywordUndef,
                                                        SparqlKeywordUri,
                                                        SparqlKeywordUUID,
                                                        SparqlKeywordValues,
                                                        SparqlKeywordYear
                                                       };

        /// <summary>
        /// Set of SPARQL Keywords that are Update Keywords
        /// </summary>
        public static String[] UpdateKeywords = {   
                                                    SparqlKeywordAdd,
                                                    SparqlKeywordAll,
                                                    SparqlKeywordClear,
                                                    SparqlKeywordCopy,
                                                    SparqlKeywordCreate,
                                                    SparqlKeywordData,
                                                    SparqlKeywordDefault,
                                                    SparqlKeywordDelete,
                                                    SparqlKeywordDrop,
                                                    SparqlKeywordInsert,
                                                    SparqlKeywordInto,
                                                    SparqlKeywordLoad,
                                                    SparqlKeywordMove,
                                                    SparqlKeywordSilent,
                                                    SparqlKeywordTo,
                                                    SparqlKeywordUsing,
                                                    SparqlKeywordWith
                                                };

        /// <summary>
        /// Set of Keywords for SPARQL Update 1.1
        /// </summary>
        public static String[] SparqlUpdate11Keywords = {
                                                          SparqlKeywordAdd,
                                                          SparqlKeywordAll,
                                                          SparqlKeywordBase,
                                                          SparqlKeywordClear,
                                                          SparqlKeywordCopy,
                                                          SparqlKeywordCreate,
                                                          SparqlKeywordData,
                                                          SparqlKeywordDefault,
                                                          SparqlKeywordDelete,
                                                          SparqlKeywordDrop,
                                                          SparqlKeywordGraph,
                                                          SparqlKeywordInsert,
                                                          SparqlKeywordInto,
                                                          SparqlKeywordLoad,
                                                          SparqlKeywordMove,
                                                          SparqlKeywordPrefix,
                                                          SparqlKeywordSilent,
                                                          SparqlKeywordTo,
                                                          SparqlKeywordUsing,
                                                          SparqlKeywordWhere,
                                                          SparqlKeywordWith                                                        
                                                       };
                                                    

        /// <summary>
        /// Regular Expression Pattern for Valid Integers in Sparql
        /// </summary>
        public static Regex SparqlInteger = new Regex(TurtleSpecsHelper.ValidIntegerPattern);
        /// <summary>
        /// Regular Expression Pattern for Valid Decimals in Sparql
        /// </summary>
        public static Regex SparqlDecimal = new Regex(TurtleSpecsHelper.ValidDecimalPattern);
        /// <summary>
        /// Regular Expression Pattern for Valid Doubles in Sparql
        /// </summary>
        public static Regex SparqlDouble = new Regex(TurtleSpecsHelper.ValidDoublePattern);

        public static INodeFormatter Formatter { get; private set; }

        #endregion

        #region Keyword Test Functions

        /// <summary>
        /// Checks whether a given Keyword is a SPARQL Query Keyword
        /// </summary>
        /// <param name="keyword">Keyword to check</param>
        /// <returns></returns>
        public static bool IsQueryKeyword(String keyword)
        {
            keyword = keyword.ToUpper();
            if (keyword.Equals(SparqlKeywordAsk) || keyword.Equals(SparqlKeywordConstruct) || keyword.Equals(SparqlKeywordDescribe) || keyword.Equals(SparqlKeywordSelect))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Checks whether a given Keyword is a SPARQL Non-Query Keyword
        /// </summary>
        /// <param name="keyword">Keyword to check</param>
        /// <returns></returns>
        public static bool IsNonQueryKeyword(String keyword)
        {
            return NonQueryKeywords.Contains(keyword, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Checks whether a given Keyword is a SPARQL Function Verb
        /// </summary>
        /// <param name="keyword">Keyword to check</param>
        /// <returns></returns>
        public static bool IsFunctionKeyword(String keyword)
        {
            return FunctionKeywords.Contains(keyword, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Checks whether a given Keyword is a SPARQL 1.1 Function Verb
        /// </summary>
        /// <param name="keyword">Keyword to check</param>
        /// <returns></returns>
        public static bool IsFunctionKeyword11(String keyword)
        {
            return IsFunctionKeyword(keyword) && SparqlQuery11Keywords.Contains(keyword, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Checks whether a given Keyword is a SPARQL Aggregate Keyword (includes keywords related to aggregates like DISTINCT, AS and Leviathan extension aggregate keywords)
        /// </summary>
        /// <param name="keyword">Keyword to check</param>
        /// <returns></returns>
        public static bool IsAggregateKeyword(String keyword)
        {
            return AggregateKeywords.Contains(keyword, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Checks whether a given Keyword is a SPARQL Aggregate Function Keyword (only keywords for the SPARQL built-in aggregate functions)
        /// </summary>
        /// <param name="keyword">Keyword to check</param>
        /// <returns></returns>
        public static bool IsAggregateFunctionKeyword(String keyword)
        {
            return AggregateFunctionKeywords.Contains(keyword, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Checks whether a given Keyword is a SPARQL Update Keyword
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public static bool IsUpdateKeyword(String keyword)
        {
            return UpdateKeywords.Contains(keyword, StringComparer.OrdinalIgnoreCase);
        }

        #endregion

        #region Plan Literal Validation

        /// <summary>
        /// Checks whether the given value is a valid Numeric Literal in Sparql
        /// </summary>
        /// <param name="value">Value to test</param>
        /// <returns></returns>
        public static bool IsValidNumericLiteral(String value)
        {
            return (SparqlInteger.IsMatch(value) || SparqlDecimal.IsMatch(value) || SparqlDouble.IsMatch(value));
        }

        /// <summary>
        /// Checks whether the given value is a valid Integer Literal in Sparql
        /// </summary>
        /// <param name="value">Value to test</param>
        /// <returns></returns>
        public static bool IsInteger(String value)
        {
            return SparqlInteger.IsMatch(value);
        }

        /// <summary>
        /// Checks whether the given value is a valid Decimal Literal in Sparql
        /// </summary>
        /// <param name="value">Value to test</param>
        /// <returns></returns>
        public static bool IsDecimal(String value)
        {
            return SparqlDecimal.IsMatch(value) || SparqlInteger.IsMatch(value);
        }

        /// <summary>
        /// Checks whether the given value is a valid Float Literal in Sparql
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsFloat(String value)
        {
            return IsDouble(value);
        }

        /// <summary>
        /// Checks whether the given value is a valid Double Literal in Sparql
        /// </summary>
        /// <param name="value">Value to test</param>
        /// <returns></returns>
        public static bool IsDouble(String value)
        {
            return SparqlDouble.IsMatch(value) || SparqlDecimal.IsMatch(value) || SparqlInteger.IsMatch(value);
        }

        #endregion

        #region Numeric Type determination

        /// <summary>
        /// Determines the Sparql Numeric Type for a Literal based on its Data Type Uri
        /// </summary>
        /// <param name="dtUri">Data Type Uri</param>
        /// <returns></returns>
        public static EffectiveNumericType GetNumericTypeFromDataTypeUri(Uri dtUri)
        {
            return GetNumericTypeFromDataTypeUri(dtUri.AbsoluteUri);
        }

        /// <summary>
        /// Determines the Sparql Numeric Type for a Literal based on its Data Type Uri
        /// </summary>
        /// <param name="dtUri">Data Type Uri as a String</param>
        /// <returns></returns>
        public static EffectiveNumericType GetNumericTypeFromDataTypeUri(String dtUri)
        {
            if (dtUri.Equals(XmlSpecsHelper.XmlSchemaDataTypeDouble))
            {
                return EffectiveNumericType.Double;
            }
            if (dtUri.Equals(XmlSpecsHelper.XmlSchemaDataTypeFloat))
            {
                return EffectiveNumericType.Float;
            }
            if (dtUri.Equals(XmlSpecsHelper.XmlSchemaDataTypeDecimal))
            {
                return EffectiveNumericType.Decimal;
            }
            return XmlSpecsHelper.IntegerDataTypes.Contains(dtUri) ? EffectiveNumericType.Integer : EffectiveNumericType.NaN;
        }

        #endregion

        /// <summary>
        /// Unescapes local name escapes from QNames
        /// </summary>
        /// <param name="value">Value to unescape</param>
        /// <returns></returns>
        public static String UnescapeQName(String value)
        {
            if (value.Contains('\\') || value.Contains('%'))
            {
                StringBuilder output = new StringBuilder();
                output.Append(value.Substring(0, value.IndexOf(':')));
                char[] cs = value.ToCharArray();
                for (int i = output.Length; i < cs.Length; i++)
                {
                    if (cs[i] == '\\')
                    {
                        if (i == cs.Length - 1) throw new RdfParseException("Invalid backslash to start an escape at the end of the Local Name, expecting a single character after the backslash");
                        char esc = cs[i + 1];
                        switch (esc)
                        {
                            case '_':
                            case '~':
                            case '-':
                            case '.':
                            case '!':
                            case '$':
                            case '&':
                            case '\'':
                            case '(':
                            case ')':
                            case '*':
                            case '+':
                            case ',':
                            case ';':
                            case '=':
                            case '/':
                            case '?':
                            case '#':
                            case '@':
                            case '%':
                                output.Append(esc);
                                i++;
                                break;
                            default:
                                throw new RdfParseException("Invalid character after a backslash, a backslash can only be used to escape a limited set (_~-.|$&\\()*+,;=/?#@%) of characters in a Local Name");
                        }
                    }
                    else if (cs[i] == '%')
                    {
                        //Remember that we are supposed to preserve precent encoded characters as-is
                        //Simply need to validate that they are valid encoding
                        if (i > cs.Length - 2)
                        {
                            throw new RdfParseException("Invalid % to start a percent encoded character in a Local Name, two hex digits are required after a %, use \\% to denote a percent character directly");
                        }
                        else
                        {
                            if (!SparqlGrammarHelper.IsHex(cs[i + 1]) || !SparqlGrammarHelper.IsHex(cs[i + 2]))
                            {
                                throw new RdfParseException("Invalid % encoding, % character was not followed by two hex digits, use \\% to denote a percent character directly");
                            }
                            else
                            {
                                output.Append(cs, i, 3);
                                i += 2;
                            }
                        }
                    }
                    else
                    {
                        output.Append(cs[i]);
                    }
                }
                return output.ToString();
            }
            else
            {
                return value;
            }
        }

        /// <summary>
        /// Calculates the Effective Boolean Value of a given Node according to the Sparql specification
        /// </summary>
        /// <param name="n">Node to computer EBV for</param>
        /// <returns></returns>
        public static bool EffectiveBooleanValue(INode n)
        {
            if (n == null)
            {
                //Nulls give Type Error
                throw new NodeValueException("Cannot calculate the Effective Boolean Value of a null value");
            }
            else
            {
                if (n.NodeType == NodeType.Literal)
                {
                    INode lit = (INode)n;

                    if (lit.DataType == null)
                    {
                        if (lit.Value == String.Empty)
                        {
                            //Empty String Literals have EBV of False
                            return false;
                        }
                        else
                        {
                            //Non-Empty String Literals have EBV of True
                            return true;
                        }
                    }
                    else
                    {
                        //EBV is dependent on the Data Type for Typed Literals
                        String dt = lit.DataType.ToString();

                        if (dt.Equals(XmlSpecsHelper.XmlSchemaDataTypeBoolean))
                        {
                            //Boolean Typed Literal
                            bool b = false;
                            if (Boolean.TryParse(lit.Value, out b))
                            {
                                //Valid Booleans have EBV of their value
                                return b;
                            }
                            else
                            {
                                //Invalid Booleans have EBV of false
                                return false;
                            }
                        }
                        else if (dt.Equals(XmlSpecsHelper.XmlSchemaDataTypeString))
                        {
                            //String Typed Literal
                            if (lit.Value == String.Empty)
                            {
                                //Empty String Literals have EBV of False
                                return false;
                            }
                            else
                            {
                                //Non-Empty String Literals have EBV of True
                                return true;
                            }
                        }
                        else
                        {
                            //Is it a Number?
                            EffectiveNumericType numType = GetNumericTypeFromDataTypeUri(dt);
                            switch (numType)
                            {
                                case EffectiveNumericType.Decimal:
                                    //Should be a decimal
                                    Decimal dec;
                                    if (Decimal.TryParse(lit.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out dec))
                                    {
                                        if (dec == Decimal.Zero)
                                        {
                                            //Zero gives EBV of false
                                            return false;
                                        }
                                        else
                                        {
                                            //Non-Zero gives EBV of true
                                            return true;
                                        }
                                    }
                                    else
                                    {
                                        //Invalid Numerics have EBV of false
                                        return false;
                                    }

                                case EffectiveNumericType.Float:
                                case EffectiveNumericType.Double:
                                    //Should be a double
                                    Double dbl;
                                    if (Double.TryParse(lit.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out dbl))
                                    {
                                        if (dbl == 0.0d || Double.IsNaN(dbl))
                                        {
                                            //Zero/NaN gives EBV of false
                                            return false;
                                        }
                                        else
                                        {
                                            //Non-Zero gives EBV of true
                                            return true;
                                        }
                                    }
                                    else
                                    {
                                        //Invalid Numerics have EBV of false
                                        return false;
                                    }

                                case EffectiveNumericType.Integer:
                                    //Should be an Integer
                                    long l;
                                    if (Int64.TryParse(lit.Value, out l))
                                    {
                                        if (l == 0)
                                        {
                                            //Zero gives EBV of false
                                            return false;
                                        }
                                        else
                                        {
                                            //Non-Zero gives EBV of true
                                            return true;
                                        }
                                    }
                                    else
                                    {
                                        //Invalid Numerics have EBV of false
                                        return false;
                                    }

                                case EffectiveNumericType.NaN:
                                    //If not a Numeric Type then Type error
                                    throw new NodeValueException("Unable to compute an Effective Boolean Value for a Literal Typed <" + dt + ">");

                                default:
                                    //Shouldn't hit this case but included to keep compiler happy
                                    throw new NodeValueException("Unable to compute an Effective Boolean Value for a Literal Typed <" + dt + ">");
                            }
                        }
                    }
                }
                else
                {
                    //Non-Literal Nodes give type error
                    throw new NodeValueException("Cannot calculate the Effective Boolean Value of a non-literal RDF Term");
                }
            }
        }
       
        #region Equality/Inequality

        /// <summary>
        /// Implements Node Equality with SPARQL Semantics
        /// </summary>
        /// <param name="x">Node</param>
        /// <param name="y">Node</param>
        /// <returns></returns>
        public static bool Equality(INode x, INode y)
        {
            if (x == null || y == null)
            {
                //Nulls can't be equal to each other
                throw new NodeValueException("Cannot evaluate equality when one/both arguments are null");
            }
            else if (x.NodeType != y.NodeType)
            {
                //Different Type Nodes are never equal to each other
                return false;
            }
            else if (x.NodeType == NodeType.Literal)
            {
                //Do they have supported Data Types?
                String xtype, ytype;
                try 
                {
                    xtype = XmlSpecsHelper.GetSupportedDataType(x);
                    ytype = XmlSpecsHelper.GetSupportedDataType(y);
                } 
                catch (RdfException) 
                {
                    //Can't determine a Data Type for one/both of the Nodes so use RDF Term equality instead
                    return x.Equals(y);
                }

                if (xtype.Equals(String.Empty) || ytype.Equals(String.Empty))
                {
                    //One/both has an unknown type
                    if (x.Equals(y))
                    {
                        //If RDF Term equality returns true then we return true;
                        return true;
                    }
                    else
                    {
                        //If RDF Term equality returns false then we error
                        throw new NodeValueException("Unable to determine equality since one/both arguments has an Unknown Type");
                    }
                }
                else
                {
                    //Both have known types
                    EffectiveNumericType numtype = (EffectiveNumericType)Math.Max((int)GetNumericTypeFromDataTypeUri(xtype), (int)GetNumericTypeFromDataTypeUri(ytype));
                    if (numtype != EffectiveNumericType.NaN)
                    {
                        //Both are Numeric so use Numeric equality
                        try
                        {
                            return NumericEquality(x, y, numtype);
                        }
                        catch (FormatException)
                        {
                            return x.Equals(y);
                        }
                        catch (NodeValueException)
                        {
                            //If this errors try RDF Term equality since 
                            return x.Equals(y);
                        }
                    }
                    else if (xtype.Equals(ytype))
                    {
                        switch (xtype) 
                        {
                            case XmlSpecsHelper.XmlSchemaDataTypeDate:
                                return DateEquality(x, y);
                            case XmlSpecsHelper.XmlSchemaDataTypeDateTime:
                                return DateTimeEquality(x, y);
                            case XmlSpecsHelper.XmlSchemaDataTypeDuration:
                                return TimeSpanEquality(x, y);
                            case XmlSpecsHelper.XmlSchemaDataTypeString:
                                //Both Strings so use Lexical string equality
                                return ((INode)x).Value.Equals(((INode)y).Value);
                            default:
                                //Use value equality
                                return (x.CompareTo(y) == 0);
                        }
                    }
                    else
                    {
                        String commontype = XmlSpecsHelper.GetCompatibleSupportedDataType(xtype, ytype);
                        if (commontype.Equals(String.Empty))
                        {
                            return false;
                        }
                        else
                        {
                            switch (commontype)
                            {
                                case XmlSpecsHelper.XmlSchemaDataTypeDateTime:
                                    return DateTimeEquality(x, y);
                                default:
                                    return false;
                            }
                        }
                    }
                }
            }
            else
            {
                //For any other Node types equality is RDF Term equality
                return x.Equals(y);
            }
        }

        /// <summary>
        /// Implements Node Inequality with SPARQL Semantics
        /// </summary>
        /// <param name="x">Node</param>
        /// <param name="y">Node</param>
        /// <returns></returns>
        public static bool Inequality(INode x, INode y)
        {
            if (x == null || y == null)
            {
                //Nulls can't be equal to each other
                throw new NodeValueException("Cannot evaluate inequality when one/both arguments are null");
            }
            else if (x.NodeType != y.NodeType)
            {
                //Different Type Nodes are never equal to each other
                return true;
            }
            else if (x.NodeType == NodeType.Literal)
            {
                //Do they have supported Data Types?
                String xtype, ytype;
                try
                {
                    xtype = XmlSpecsHelper.GetSupportedDataType(x);
                    ytype = XmlSpecsHelper.GetSupportedDataType(y);
                }
                catch (RdfException)
                {
                    //Can't determine a Data Type for one/both of the Nodes so use RDF Term equality instead
                    return !x.Equals(y);
                }

                if (xtype.Equals(String.Empty) || ytype.Equals(String.Empty))
                {
                    //One/both has an unknown type
                    if (x.Equals(y))
                    {
                        //If RDF Term equality returns true then we return false
                        return false;
                    }
                    else
                    {
                        //If RDF Term equality returns false then we error
                        throw new NodeValueException("Unable to determine inequality since one/both arguments has an Unknown Type");
                    }
                }
                else
                {
                    //Both have known types
                    EffectiveNumericType xnumtype = GetNumericTypeFromDataTypeUri(xtype);
                    EffectiveNumericType ynumtype = GetNumericTypeFromDataTypeUri(ytype);
                    EffectiveNumericType numtype = (EffectiveNumericType)Math.Max((int)xnumtype, (int)ynumtype);
                    if (numtype != EffectiveNumericType.NaN)
                    {
                        if (xnumtype == EffectiveNumericType.NaN || ynumtype == EffectiveNumericType.NaN)
                        {
                            //If one is non-numeric then we can't assume non-equality
                            return false;
                        }

                        //Both are Numeric so use Numeric equality
                        try
                        {
                            return !NumericEquality(x, y, numtype);
                        }
                        catch (FormatException)
                        {
                            if (x.Equals(y)) return false;
                            return false;
                        }
                        catch (NodeValueException)
                        {
                            //If this errors try RDF Term equality since 
                            return !x.Equals(y);
                        }
                    }
                    else if (xtype.Equals(ytype))
                    {
                        switch (xtype)
                        {
                            case XmlSpecsHelper.XmlSchemaDataTypeDate:
                                try
                                {
                                    return !DateEquality(x, y);
                                }
                                catch (NodeValueException)
                                {
                                    return true;
                                }
                            case XmlSpecsHelper.XmlSchemaDataTypeDateTime:
                                try
                                {
                                    return !DateTimeEquality(x, y);
                                }
                                catch (NodeValueException)
                                {
                                    return true;
                                }
                            case XmlSpecsHelper.XmlSchemaDataTypeDuration:
                                try
                                {
                                    return !TimeSpanEquality(x, y);
                                }
                                catch (NodeValueException)
                                {
                                    return true;
                                }
                            case XmlSpecsHelper.XmlSchemaDataTypeString:
                                //Both Strings so use Lexical string equality
                                return !((INode)x).Value.Equals(((INode)y).Value);
                            default:
                                //Use value equality
                                return (x.CompareTo(y) != 0);
                        }
                    }
                    else
                    {
                        String commontype = XmlSpecsHelper.GetCompatibleSupportedDataType(xtype, ytype);
                        if (commontype.Equals(String.Empty))
                        {
                            return true;
                        }
                        else
                        {
                            switch (commontype)
                            {
                                case XmlSpecsHelper.XmlSchemaDataTypeDate:
                                    try
                                    {
                                        return !DateEquality(x, y);
                                    }
                                    catch (NodeValueException)
                                    {
                                        return true;
                                    }
                                case XmlSpecsHelper.XmlSchemaDataTypeDateTime:
                                    try
                                    {
                                        return !DateTimeEquality(x, y);
                                    }
                                    catch (NodeValueException)
                                    {
                                        return true;
                                    }
                                default:
                                    return true;
                            }
                        }
                    }
                }
            }
            else
            {
                //For any other Node types equality is RDF Term equality
                return !x.Equals(y);
            }
        }

        /// <summary>
        /// Implements Numeric Equality with SPARQL Semantics
        /// </summary>
        /// <param name="x">Node</param>
        /// <param name="y">Node</param>
        /// <param name="type">SPARQL Numeric Tyoe</param>
        /// <returns></returns>
        public static bool NumericEquality(INode x, INode y, EffectiveNumericType type)
        {
            if (x == null || y == null) throw new NodeValueException("Cannot evaluate numeric equality when one or both arguments are Null");
            if (type == EffectiveNumericType.NaN) throw new NodeValueException("Cannot evaluate numeric equality when the Numeric Type is NaN");

            IValuedNode a = x.AsValuedNode();
            IValuedNode b = y.AsValuedNode();

            switch (type)
            {
                case EffectiveNumericType.Decimal:
                    return a.AsDecimal().Equals(b.AsDecimal());
                case EffectiveNumericType.Double:
                    return a.AsDouble().Equals(b.AsDouble());
                case EffectiveNumericType.Float:
                    return a.AsFloat().Equals(b.AsFloat());
                case EffectiveNumericType.Integer:
                    return a.AsInteger().Equals(b.AsInteger());
                default:
                    throw new NodeValueException("Cannot evaluate numeric equality since of the arguments is not numeric");
            }
        }

        /// <summary>
        /// Implements Date Time Equality with SPARQL Semantics
        /// </summary>
        /// <param name="x">Node</param>
        /// <param name="y">Node</param>
        /// <returns></returns>
        public static bool DateTimeEquality(INode x, INode y)
        {
            if (x == null || y == null) throw new NodeValueException("Cannot evaluate date equality when one or both arguments are Null");
            try
            {
                DateTime c = x.AsValuedNode().AsDateTime();
                DateTime d = y.AsValuedNode().AsDateTime();

                switch (c.Kind)
                {
                    case DateTimeKind.Unspecified:
                        if (d.Kind != DateTimeKind.Unspecified)
                        {
                            // If non-equal kinds and either is unespecified kind then non-comparable
                            throw new NodeValueException("Dates are incomparable, one specifies time zone information while the other does not");
                        }
                        // Both unspecified so compare
                        return c.Equals(d);
                    case DateTimeKind.Local:
                        // This case should be impossible since AsValuedNode() normalizes DateTime to UTC but cover it for programmatic use
                        // Adjust to UTC and compare
                        c = c.ToUniversalTime();
                        if (d.Kind == DateTimeKind.Unspecified)
                            throw new NodeValueException(
                                "Dates are incomparable, one specifies time zone information while the other does not");
                        if (d.Kind == DateTimeKind.Local) d = d.ToUniversalTime();
                        goto default;
                    default:
                        // Covers UTC based comparison
                        if (d.Kind == DateTimeKind.Unspecified)
                            throw new NodeValueException(
                                "Dates are incomparable, one specifies time zone information while the other does not");
                        if (d.Kind == DateTimeKind.Local) d = d.ToUniversalTime();

                        return c.Equals(d);
                }
            }
            catch (FormatException)
            {
                throw new NodeValueException("Cannot evaluate date equality since one of the arguments does not have a valid lexical value for a Date");
            }
        }

        /// <summary>
        /// Implements Date Equality with SPARQL Semantics
        /// </summary>
        /// <param name="x">Node</param>
        /// <param name="y">Node</param>
        /// <returns></returns>
        public static bool DateEquality(INode x, INode y)
        {
            if (x == null || y == null) throw new NodeValueException("Cannot evaluate date equality when one or both arguments are Null");
            try
            {
                IValuedNode a = x.AsValuedNode();
                IValuedNode b = y.AsValuedNode();

                bool strictEquals = (a.EffectiveType != b.EffectiveType);

                DateTime c = a.AsDateTime();
                DateTime d = b.AsDateTime();

                switch (c.Kind)
                {
                    case DateTimeKind.Unspecified:
                        if (d.Kind != DateTimeKind.Unspecified && strictEquals)
                            throw new NodeValueException(
                                "Dates are incomparable, one specifies time zone information while the other does not");
                        // One/Both unspecified so just compare
                        return (c.Year == d.Year && c.Month == d.Month && c.Day == d.Day);
                    case DateTimeKind.Local:
                        // This case should be impossible since AsValuedNode() normalizes DateTime to UTC but cover it for programmatic use
                        if (d.Kind == DateTimeKind.Unspecified && strictEquals)
                            throw new NodeValueException(
                                "Dates are incomparable, one specifies time zone information while the other does not");
                        // Adjust to UTC and compare
                        c = c.ToUniversalTime();
                        if (d.Kind == DateTimeKind.Local) d = d.ToUniversalTime();
                        goto default;
                    default:
                        // Covers UTC based comparison
                        if (d.Kind == DateTimeKind.Unspecified && strictEquals)
                            throw new NodeValueException(
                                "Dates are incomparable, one specifies time zone information while the other does not");
                        // Adjust to UTC and compare
                        if (d.Kind == DateTimeKind.Local) d = d.ToUniversalTime();

                        return (c.Year == d.Year && c.Month == d.Month && c.Day == d.Day);
                }
            }
            catch (FormatException)
            {
                throw new NodeValueException("Cannot evaluate date equality since one of the arguments does not have a valid lexical value for a Date");
            }
        }

        /// <summary>
        /// Implements Time Span Equality with SPARQL Semantics
        /// </summary>
        /// <param name="x">Node</param>
        /// <param name="y">Node</param>
        /// <returns></returns>
        public static bool TimeSpanEquality(INode x, INode y)
        {
            if (x == null || y == null) throw new NodeValueException("Cannot evaluate time span equality when one or both arguments are Null");
            try
            {
                TimeSpan c = x.AsValuedNode().AsTimeSpan();
                TimeSpan d = y.AsValuedNode().AsTimeSpan();

                return c.Equals(d);
            }
            catch (FormatException)
            {
                throw new NodeValueException("Cannot evaluate time span equality since one of the arguments does not have a valid lexical value for a Time Span");
            }
        }

        /// <summary>
        /// Converts a Literal Node to a Decimal
        /// </summary>
        /// <param name="n">Literal Node</param>
        /// <returns></returns>
        [Obsolete("Use AsValuedNode().AsDecimal() instead", true)]
        public static Decimal ToDecimal(INode n)
        {
            if (n.DataType == null) throw new NodeValueException("Cannot convert an untyped Literal to a Decimal");
            return Decimal.Parse(n.Value);
        }

        /// <summary>
        /// Converts a Literal Node to a Double
        /// </summary>
        /// <param name="n">Literal Node</param>
        /// <returns></returns>
        [Obsolete("Use AsValuedNode().AsDouble() instead", true)]
        public static Double ToDouble(INode n)
        {
            if (n.DataType == null) throw new NodeValueException("Cannot convert an untyped Literal to a Double");
            return Double.Parse(n.Value);
        }

        /// <summary>
        /// Converts a Literal Node to a Float
        /// </summary>
        /// <param name="n">Literal Node</param>
        /// <returns></returns>
        [Obsolete("Use AsValuedNode().AsFloat() instead", true)]
        public static Single ToFloat(INode n)
        {
            if (n.DataType == null) throw new NodeValueException("Cannot convert an untyped Literal to a Float");
            return Single.Parse(n.Value);
        }

        /// <summary>
        /// Converts a Literal Node to an Integer
        /// </summary>
        /// <param name="n">Literal Node</param>
        /// <returns></returns>
        [Obsolete("Use AsValuedNode().AsInteger() instead", true)]
        public static Int64 ToInteger(INode n)
        {
            if (n.DataType == null) throw new NodeValueException("Cannot convert an untyped Literal to an Integer");
            return Int64.Parse(n.Value);
        }

        /// <summary>
        /// Converts a Literal Node to a Date Time
        /// </summary>
        /// <param name="n">Literal Node</param>
        /// <returns></returns>
        [Obsolete("Use AsValuedNode().AsDateTime() instead", true)]
        public static DateTime ToDateTime(INode n)
        {
            if (n.DataType == null) throw new NodeValueException("Cannot convert an untyped Literal to a Date Time");
            return DateTime.Parse(n.Value, null, DateTimeStyles.AssumeUniversal);
        }

        /// <summary>
        /// Converts a Literal Node to a Date Time Offset
        /// </summary>
        /// <param name="n">Literal Node</param>
        /// <returns></returns>
        [Obsolete("Use AsValuedNode().AsDateTimeOffset() instead", true)]
        public static DateTimeOffset ToDateTimeOffset(INode n)
        {
            if (n.DataType == null) throw new NodeValueException("Cannot convert an untyped Literal to a Date Time");
            return DateTimeOffset.Parse(n.Value, null, DateTimeStyles.AssumeUniversal);
        }

        /// <summary>
        /// Converts a Literal Node to a Time Span
        /// </summary>
        /// <param name="n">Literal Node</param>
        /// <returns></returns>
        [Obsolete("Use AsValuedNode().AsTimeSpan() instead", true)]
        public static TimeSpan ToTimeSpan(INode n)
        {
            if (n.DataType == null) throw new NodeValueException("Cannot convert an untyped Literal to a Time Span");
            return TimeSpan.Parse(n.Value);
        }

        #endregion

        /// <summary>
        /// Determines whether the Arguments are valid
        /// </summary>
        /// <param name="stringLit">String Literal</param>
        /// <param name="argLit">Argument Literal</param>
        /// <returns></returns>
        public static bool IsValidStringArgumentPair(INode stringLit, INode argLit)
        {
            if (stringLit.HasLanguage)
            {
                // 1st Argument has a language tag
                if (argLit.HasDataType && !argLit.HasLanguage)
                {
                    // If 2nd Argument is typed then must be xsd:string
                    // to be valid
                    return argLit.DataType.AbsoluteUri.Equals(XmlSpecsHelper.XmlSchemaDataTypeString);
                }
                if (!argLit.HasLanguage || stringLit.Language.Equals(argLit.Language))
                {
                    // 2nd Argument must have no Language Tag OR same Language Tag in order to be valid
                    return true;
                }
                //Otherwise Invalid
                return false;
            }
            if (stringLit.HasDataType)
            {
                // 1st argument has a data type

                // The data type must be an xsd:string or not valid
                if (!stringLit.DataType.AbsoluteUri.Equals(XmlSpecsHelper.XmlSchemaDataTypeString)) return false;

                if (argLit.HasDataType && !argLit.HasLanguage)
                {
                    // If 2nd argument has a DataType it must also be an xsd:string or not valid
                    return argLit.DataType.AbsoluteUri.Equals(XmlSpecsHelper.XmlSchemaDataTypeString);
                }
                // 2nd argument must have no language tag to be valid
                return !argLit.HasLanguage;
            }

            // 1st argument is a plain literal
            if (argLit.HasDataType)
            {
                // So 2nd argument must be xsd:string if typed
                return argLit.DataType.AbsoluteUri.Equals(XmlSpecsHelper.XmlSchemaDataTypeString);
            }
            // 2nd literal cannot have a language tag to be valid
            return !argLit.HasLanguage;
        }
    }
}
