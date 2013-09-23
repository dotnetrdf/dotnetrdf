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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using VDS.RDF.Nodes;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Patterns;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Specifications;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Specifications
{
    /// <summary>
    /// Class containing Helper information and methods pertaining to the Sparql Query Language for RDF
    /// </summary>
    public static class SparqlSpecsHelper
    {

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
        /// Set of XML Schema Data Types which are derived from Integer and can be treated as Integers by SPARQL
        /// </summary>
        public static String[] IntegerDataTypes = {   
                                                      XmlSpecsHelper.XmlSchemaDataTypeByte, 
                                                      XmlSpecsHelper.XmlSchemaDataTypeInt,
                                                      XmlSpecsHelper.XmlSchemaDataTypeInteger, 
                                                      XmlSpecsHelper.XmlSchemaDataTypeLong, 
                                                      XmlSpecsHelper.XmlSchemaDataTypeNegativeInteger, 
                                                      XmlSpecsHelper.XmlSchemaDataTypeNonNegativeInteger, 
                                                      XmlSpecsHelper.XmlSchemaDataTypeNonPositiveInteger, 
                                                      XmlSpecsHelper.XmlSchemaDataTypePositiveInteger, 
                                                      XmlSpecsHelper.XmlSchemaDataTypeShort, 
                                                      XmlSpecsHelper.XmlSchemaDataTypeUnsignedByte, 
                                                      XmlSpecsHelper.XmlSchemaDataTypeUnsignedInt, 
                                                      XmlSpecsHelper.XmlSchemaDataTypeUnsignedLong, 
                                                      XmlSpecsHelper.XmlSchemaDataTypeUnsignedShort 
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
        public static SparqlNumericType GetNumericTypeFromDataTypeUri(Uri dtUri)
        {
            return GetNumericTypeFromDataTypeUri(dtUri.AbsoluteUri);
        }

        /// <summary>
        /// Determines the Sparql Numeric Type for a Literal based on its Data Type Uri
        /// </summary>
        /// <param name="dtUri">Data Type Uri as a String</param>
        /// <returns></returns>
        public static SparqlNumericType GetNumericTypeFromDataTypeUri(String dtUri)
        {
            if (dtUri.Equals(XmlSpecsHelper.XmlSchemaDataTypeDouble))
            {
                return SparqlNumericType.Double;
            }
            else if (dtUri.Equals(XmlSpecsHelper.XmlSchemaDataTypeFloat))
            {
                return SparqlNumericType.Float;
            }
            else if (dtUri.Equals(XmlSpecsHelper.XmlSchemaDataTypeDecimal))
            {
                return SparqlNumericType.Decimal;
            }
            else if (IntegerDataTypes.Contains(dtUri))
            {
                return SparqlNumericType.Integer;
            }
            else
            {
                return SparqlNumericType.NaN;
            }
        }

        #endregion

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
                throw new RdfQueryException("Cannot calculate the Effective Boolean Value of a null value");
            }
            else
            {
                if (n.NodeType == NodeType.Literal)
                {
                    ILiteralNode lit = (ILiteralNode)n;

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
                            SparqlNumericType numType = GetNumericTypeFromDataTypeUri(dt);
                            switch (numType)
                            {
                                case SparqlNumericType.Decimal:
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

                                case SparqlNumericType.Float:
                                case SparqlNumericType.Double:
                                    //Should be a double
                                    Double dbl;
                                    if (Double.TryParse(lit.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out dbl))
                                    {
                                        if (dbl == 0.0d || dbl == Double.NaN)
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

                                case SparqlNumericType.Integer:
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

                                case SparqlNumericType.NaN:
                                    //If not a Numeric Type then Type error
                                    throw new RdfQueryException("Unable to compute an Effective Boolean Value for a Literal Typed <" + dt + ">");

                                default:
                                    //Shouldn't hit this case but included to keep compiler happy
                                    throw new RdfQueryException("Unable to compute an Effective Boolean Value for a Literal Typed <" + dt + ">");
                            }
                        }
                    }
                }
                else
                {
                    //Non-Literal Nodes give type error
                    throw new RdfQueryException("Cannot calculate the Effective Boolean Value of a non-literal RDF Term");
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
                throw new RdfQueryException("Cannot evaluate equality when one/both arguments are null");
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
                        throw new RdfQueryException("Unable to determine equality since one/both arguments has an Unknown Type");
                    }
                }
                else
                {
                    //Both have known types
                    SparqlNumericType numtype = (SparqlNumericType)Math.Max((int)GetNumericTypeFromDataTypeUri(xtype), (int)GetNumericTypeFromDataTypeUri(ytype));
                    if (numtype != SparqlNumericType.NaN)
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
                        catch (RdfQueryException)
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
                                return ((ILiteralNode)x).Value.Equals(((ILiteralNode)y).Value);
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
                throw new RdfQueryException("Cannot evaluate inequality when one/both arguments are null");
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
                        throw new RdfQueryException("Unable to determine inequality since one/both arguments has an Unknown Type");
                    }
                }
                else
                {
                    //Both have known types
                    SparqlNumericType xnumtype = GetNumericTypeFromDataTypeUri(xtype);
                    SparqlNumericType ynumtype = GetNumericTypeFromDataTypeUri(ytype);
                    SparqlNumericType numtype = (SparqlNumericType)Math.Max((int)xnumtype, (int)ynumtype);
                    if (numtype != SparqlNumericType.NaN)
                    {
                        if (xnumtype == SparqlNumericType.NaN || ynumtype == SparqlNumericType.NaN)
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
                        catch (RdfQueryException)
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
                                catch (RdfQueryException)
                                {
                                    return true;
                                }
                            case XmlSpecsHelper.XmlSchemaDataTypeDateTime:
                                try
                                {
                                    return !DateTimeEquality(x, y);
                                }
                                catch (RdfQueryException)
                                {
                                    return true;
                                }
                            case XmlSpecsHelper.XmlSchemaDataTypeDuration:
                                try
                                {
                                    return !TimeSpanEquality(x, y);
                                }
                                catch (RdfQueryException)
                                {
                                    return true;
                                }
                            case XmlSpecsHelper.XmlSchemaDataTypeString:
                                //Both Strings so use Lexical string equality
                                return !((ILiteralNode)x).Value.Equals(((ILiteralNode)y).Value);
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
                                    catch (RdfQueryException)
                                    {
                                        return true;
                                    }
                                case XmlSpecsHelper.XmlSchemaDataTypeDateTime:
                                    try
                                    {
                                        return !DateTimeEquality(x, y);
                                    }
                                    catch (RdfQueryException)
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
        public static bool NumericEquality(INode x, INode y, SparqlNumericType type)
        {
            if (x == null || y == null) throw new RdfQueryException("Cannot evaluate numeric equality when one or both arguments are Null");
            if (type == SparqlNumericType.NaN) throw new RdfQueryException("Cannot evaluate numeric equality when the Numeric Type is NaN");

            try
            {
                ILiteralNode a = (ILiteralNode)x;
                ILiteralNode b = (ILiteralNode)y;

                switch (type)
                {
                    case SparqlNumericType.Decimal:
                        return ToDecimal(a).Equals(ToDecimal(b));
                    case SparqlNumericType.Double:
                        return ToDouble(a).Equals(ToDouble(b));
                    case SparqlNumericType.Float:
                        return ToFloat(a).Equals(ToFloat(b));
                    case SparqlNumericType.Integer:
                        return ToInteger(a).Equals(ToInteger(b));
                    default:
                        throw new RdfQueryException("Cannot evaluate numeric equality since of the arguments is not numeric");
                }
            }
            catch (FormatException)
            {
                throw;// new RdfQueryException("Cannot evaluate numeric equality since one of the arguments does not have a valid lexical value for the given type");
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
            if (x == null || y == null) throw new RdfQueryException("Cannot evaluate date equality when one or both arguments are Null");
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
                            throw new RdfQueryException("Dates are incomparable, one specifies time zone information while the other does not");
                        }
                        // Both unspecified so compare
                        return c.Equals(d);
                    case DateTimeKind.Local:
                        // This case should be impossible since AsValuedNode() normalizes DateTime to UTC but cover it for programmatic use
                        // Adjust to UTC and compare
                        c = c.ToUniversalTime();
                        if (d.Kind == DateTimeKind.Unspecified)
                            throw new RdfQueryException(
                                "Dates are incomparable, one specifies time zone information while the other does not");
                        if (d.Kind == DateTimeKind.Local) d = d.ToUniversalTime();
                        goto default;
                    default:
                        // Covers UTC based comparison
                        if (d.Kind == DateTimeKind.Unspecified)
                            throw new RdfQueryException(
                                "Dates are incomparable, one specifies time zone information while the other does not");
                        if (d.Kind == DateTimeKind.Local) d = d.ToUniversalTime();

                        return c.Equals(d);
                }
            }
            catch (FormatException)
            {
                throw new RdfQueryException("Cannot evaluate date equality since one of the arguments does not have a valid lexical value for a Date");
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
            if (x == null || y == null) throw new RdfQueryException("Cannot evaluate date equality when one or both arguments are Null");
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
                            throw new RdfQueryException(
                                "Dates are incomparable, one specifies time zone information while the other does not");
                        // One/Both unspecified so just compare
                        return (c.Year == d.Year && c.Month == d.Month && c.Day == d.Day);
                    case DateTimeKind.Local:
                        // This case should be impossible since AsValuedNode() normalizes DateTime to UTC but cover it for programmatic use
                        if (d.Kind == DateTimeKind.Unspecified && strictEquals)
                            throw new RdfQueryException(
                                "Dates are incomparable, one specifies time zone information while the other does not");
                        // Adjust to UTC and compare
                        c = c.ToUniversalTime();
                        if (d.Kind == DateTimeKind.Local) d = d.ToUniversalTime();
                        goto default;
                    default:
                        // Covers UTC based comparison
                        if (d.Kind == DateTimeKind.Unspecified && strictEquals)
                            throw new RdfQueryException(
                                "Dates are incomparable, one specifies time zone information while the other does not");
                        // Adjust to UTC and compare
                        if (d.Kind == DateTimeKind.Local) d = d.ToUniversalTime();

                        return (c.Year == d.Year && c.Month == d.Month && c.Day == d.Day);
                }
            }
            catch (FormatException)
            {
                throw new RdfQueryException("Cannot evaluate date equality since one of the arguments does not have a valid lexical value for a Date");
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
            if (x == null || y == null) throw new RdfQueryException("Cannot evaluate time span equality when one or both arguments are Null");
            try
            {
                ILiteralNode a = (ILiteralNode)x;
                ILiteralNode b = (ILiteralNode)y;

                TimeSpan c = ToTimeSpan(a);
                TimeSpan d = ToTimeSpan(b);

                return c.Equals(d);
            }
            catch (FormatException)
            {
                throw new RdfQueryException("Cannot evaluate time span equality since one of the arguments does not have a valid lexical value for a Time Span");
            }
        }

        /// <summary>
        /// Converts a Literal Node to a Decimal
        /// </summary>
        /// <param name="n">Literal Node</param>
        /// <returns></returns>
        [Obsolete("Use AsValuedNode().AsDecimal() instead")]
        public static Decimal ToDecimal(ILiteralNode n)
        {
            if (n.DataType == null) throw new RdfQueryException("Cannot convert an untyped Literal to a Decimal");
            return Decimal.Parse(n.Value);
        }

        /// <summary>
        /// Converts a Literal Node to a Double
        /// </summary>
        /// <param name="n">Literal Node</param>
        /// <returns></returns>
        [Obsolete("Use AsValuedNode().AsDouble() instead")]
        public static Double ToDouble(ILiteralNode n)
        {
            if (n.DataType == null) throw new RdfQueryException("Cannot convert an untyped Literal to a Double");
            return Double.Parse(n.Value);
        }

        /// <summary>
        /// Converts a Literal Node to a Float
        /// </summary>
        /// <param name="n">Literal Node</param>
        /// <returns></returns>
        [Obsolete("Use AsValuedNode().AsFloat() instead")]
        public static Single ToFloat(ILiteralNode n)
        {
            if (n.DataType == null) throw new RdfQueryException("Cannot convert an untyped Literal to a Float");
            return Single.Parse(n.Value);
        }

        /// <summary>
        /// Converts a Literal Node to an Integer
        /// </summary>
        /// <param name="n">Literal Node</param>
        /// <returns></returns>
        [Obsolete("Use AsValuedNode().AsInteger() instead")]
        public static Int64 ToInteger(ILiteralNode n)
        {
            if (n.DataType == null) throw new RdfQueryException("Cannot convert an untyped Literal to an Integer");
            return Int64.Parse(n.Value);
        }

        /// <summary>
        /// Converts a Literal Node to a Date Time
        /// </summary>
        /// <param name="n">Literal Node</param>
        /// <returns></returns>
        [Obsolete("Use AsValuedNode().AsDateTime() instead")]
        public static DateTime ToDateTime(ILiteralNode n)
        {
            if (n.DataType == null) throw new RdfQueryException("Cannot convert an untyped Literal to a Date Time");
            return DateTime.Parse(n.Value, null, System.Globalization.DateTimeStyles.AssumeUniversal);
        }

        /// <summary>
        /// Converts a Literal Node to a Date Time Offset
        /// </summary>
        /// <param name="n">Literal Node</param>
        /// <returns></returns>
        [Obsolete("Use AsValuedNode().AsDateTimeOffset() instead")]
        public static DateTimeOffset ToDateTimeOffset(ILiteralNode n)
        {
            if (n.DataType == null) throw new RdfQueryException("Cannot convert an untyped Literal to a Date Time");
            return DateTimeOffset.Parse(n.Value, null, System.Globalization.DateTimeStyles.AssumeUniversal);
        }

        /// <summary>
        /// Converts a Literal Node to a Time Span
        /// </summary>
        /// <param name="n">Literal Node</param>
        /// <returns></returns>
        [Obsolete("Use AsValuedNode().AsTimeSpan() instead")]
        public static TimeSpan ToTimeSpan(ILiteralNode n)
        {
            if (n.DataType == null) throw new RdfQueryException("Cannot convert an untyped Literal to a Time Span");
            return TimeSpan.Parse(n.Value);
        }

        #endregion
    }
}
