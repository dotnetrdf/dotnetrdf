/*

Copyright Robert Vesse 2009-10
rvesse@vdesign-studios.com

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Patterns;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query
{
    /// <summary>
    /// Class containing Helper information and methods pertaining to the Sparql Query Language for RDF
    /// </summary>
    public static class SparqlSpecsHelper
    {
        private static SparqlFormatter _formatter;

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
                      SparqlKeywordSha224 = "SHA224",
                      SparqlKeywordSha256 = "SHA256",
                      SparqlKeywordSha384 = "SHA384",
                      SparqlKeywordSha512 = "SHA512",
                      SparqlKeywordAny = "ANY",
                      SparqlKeywordNone = "NONE",
                      SparqlKeywordAdd = "ADD",
                      SparqlKeywordCopy = "COPY",
                      SparqlKeywordMove = "MOVE",
                      SparqlKeywordTo = "TO"
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
                                                      SparqlKeywordUndef,
                                                      SparqlKeywordDefault
                                                  };
        /// <summary>
        /// Set of SPARQL Keywords that are Function Keywords
        /// </summary>
        public static String[] FunctionKeywords = {   
                                                      SparqlKeywordAbs,
                                                      SparqlKeywordBNode,
                                                      SparqlKeywordBound,
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
                                                      SparqlKeywordSha224,
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
                                                      SparqlKeywordSubStr,
                                                      SparqlKeywordTimezone,
                                                      SparqlKeywordTz,
                                                      SparqlKeywordUCase,
                                                      SparqlKeywordUri,
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
                                                        SparqlKeywordBindings,
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
                                                        SparqlKeywordSha224,
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
                                                        SparqlKeywordSubStr,
                                                        SparqlKeywordSum,
                                                        SparqlKeywordTimezone,
                                                        SparqlKeywordTz,
                                                        SparqlKeywordUCase,
                                                        SparqlKeywordUndef,
                                                        SparqlKeywordUri,
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

        #region QName and Variable Name Validation

        /// <summary>
        /// Checks whether a given QName is valid in Sparql
        /// </summary>
        /// <param name="value">QName to check</param>
        /// <returns></returns>
        public static bool IsValidQName(String value)
        {
            if (!value.Contains(':')) 
            {
                //Must have a Colon in a QName
                return false;
            } 
            else if (value.StartsWith(":"))
            {
                //No need to validate QName
                //Just validation Local Name
                char[] cs = value.ToCharArray(1, value.Length - 1);

                return IsPNLocal(cs);
            }
            else
            {
                //Split into Prefix and Local Name
                char[] prefix = value.ToCharArray(0, value.IndexOf(':'));
                char[] local = value.ToCharArray(value.IndexOf(':') + 1, value.Length - value.IndexOf(':')-1);

                return (IsPNPrefix(prefix) && IsPNLocal(local));
            }
        }

        /// <summary>
        /// Checks whether a given Variable Name is valid in Sparql
        /// </summary>
        /// <param name="value">Variable Name to check</param>
        /// <returns></returns>
        public static bool IsValidVarName(String value)
        {
            char[] cs = value.ToCharArray(1,value.Length-1);

            //Variable Names can't be empty
            if (cs.Length == 0)
            {
                return false;
            }

            //First Character must be from PN_CHARS_U or a digit
            char first = cs[0];
            if (Char.IsDigit(first) || IsPNCharU(first))
            {
                if (cs.Length > 1)
                {
                    for (int i = 1; i < cs.Length; i++)
                    {
                        if (i < cs.Length - 1)
                        {
                            //Middle Chars must be from PN_CHARS or a '.'
                            if (!(cs[i] == '.' || IsPNChar(cs[i])))
                            {
                                return false;
                            }
                        }
                        else
                        {
                            //Last Character must be from PN_CHARS
                            return IsPNChar(cs[i]);
                        }
                    }

                    //Should never get here but have to add this to keep compiler happy
                    throw new RdfParseException("Variable Name validation error in SparqlSpecsHelper.IsValidVarName(String value)");
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Checks whether a given Character matches the PN_CHARS_BASE rule from the Sparql Specification
        /// </summary>
        /// <param name="c">Character to test</param>
        /// <returns></returns>
        public static bool IsPNCharBase(char c)
        {
            return XmlSpecsHelper.IsNameStartChar(c);
        }

        /// <summary>
        /// Checks whether a given Character matches the PN_CHARS_U rule from the Sparql Specification
        /// </summary>
        /// <param name="c">Character to test</param>
        /// <returns></returns>
        public static bool IsPNCharU(char c)
        {
            return (c == '_' || IsPNCharBase(c));
        }

        /// <summary>
        /// Checks whether a given Character matches the PN_CHARS rule from the Sparql Specification
        /// </summary>
        /// <param name="c">Character to test</param>
        /// <returns></returns>
        public static bool IsPNChar(char c)
        {
            return (c != '.' && XmlSpecsHelper.IsNameChar(c));
        }

        /// <summary>
        /// Checks whether a given String matches the PN_LOCAL rule from the Sparql Specification
        /// </summary>
        /// <param name="cs">String as character array</param>
        /// <returns></returns>
        public static bool IsPNLocal(char[] cs)
        {
            if (cs.Length == 0)
            {
                //Empty Local Names are valid
                return true;
            }

            //First character must be a digit or from PN_CHARS_U
            char first = cs[0];
            if (Char.IsDigit(first) || IsPNCharU(first))
            {
                if (cs.Length > 1)
                {
                    for (int i = 1; i < cs.Length; i++)
                    {
                        if (i < cs.Length - 1)
                        {
                            //Middle characters may be from PN_CHARS or '.'
                            if (!(cs[i] == '.' || IsPNChar(cs[i])))
                            {
                                return false;
                            }
                        }
                        else
                        {
                            //Last Character must be from PN_CHARS
                            return IsPNChar(cs[i]);
                        }
                    }

                    //Should never get here but have to add this to keep compiler happy
                    throw new RdfParseException("Local Name validation error in SparqlSpecsHelper.IsPNLocal(char[] cs)");
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Checks whether a given String matches the PN_PREFIX rule from the Sparql Specification
        /// </summary>
        /// <param name="cs">String as character array</param>
        /// <returns></returns>
        public static bool IsPNPrefix(char[] cs)
        {
            //First character must be from PN_CHARS_BASE
            char first = cs[0];
            if (IsPNCharBase(first))
            {
                if (cs.Length > 1)
                {
                    for (int i = 1; i < cs.Length; i++)
                    {
                        if (i < cs.Length - 1)
                        {
                            //Middle characters may be from PN_CHARS or '.'
                            if (!(cs[i] == '.' || IsPNChar(cs[i])))
                            {
                                return false;
                            }
                        }
                        else
                        {
                            //Last Character must be from PN_CHARS
                            return IsPNChar(cs[i]);
                        }
                    }

                    //Should never get here but have to add this to keep compiler happy
                    throw new RdfParseException("Namespace Prefix validation error in SparqlSpecsHelper.IsPNPrefix(char[] cs)");
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
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
            return GetNumericTypeFromDataTypeUri(dtUri.ToString());
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
                                    if (Decimal.TryParse(lit.Value, out dec))
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
                                    if (Double.TryParse(lit.Value, out dbl))
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

        /// <summary>
        /// Checks whether the Query is a SELECT Query
        /// </summary>
        /// <param name="type">Query Type</param>
        /// <returns></returns>
        public static bool IsSelectQuery(SparqlQueryType type)
        {
            switch (type)
            {
                case SparqlQueryType.Select:
                case SparqlQueryType.SelectAll:
                case SparqlQueryType.SelectAllDistinct:
                case SparqlQueryType.SelectAllReduced:
                case SparqlQueryType.SelectDistinct:
                case SparqlQueryType.SelectReduced:
                    return true;
                default:
                    return false;
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
                                return !DateEquality(x, y);
                            case XmlSpecsHelper.XmlSchemaDataTypeDateTime:
                                return !DateTimeEquality(x, y);
                            case XmlSpecsHelper.XmlSchemaDataTypeDuration:
                                return !TimeSpanEquality(x, y);
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
                                    return !DateEquality(x, y);
                                case XmlSpecsHelper.XmlSchemaDataTypeDateTime:
                                    return !DateTimeEquality(x, y);
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
            if (x == null || y == null) throw new RdfQueryException("Cannot evaluate date time equality when one or both arguments are Null");
            try
            {
                ILiteralNode a = (ILiteralNode)x;
                ILiteralNode b = (ILiteralNode)y;

                return ToDateTime(a).Equals(ToDateTime(b));
            }
            catch (FormatException)
            {
                throw new RdfQueryException("Cannot evaluate date time equality since one of the arguments does not have a valid lexical value for a Date Time");
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
                ILiteralNode a = (ILiteralNode)x;
                ILiteralNode b = (ILiteralNode)y;

                DateTimeOffset c = ToDateTimeOffset(a);
                DateTimeOffset d = ToDateTimeOffset(b);

                if (!c.Offset.Equals(d.Offset)) return false;
                return (c.Year == d.Year && c.Month == d.Month && c.Day == d.Day);
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
        public static TimeSpan ToTimeSpan(ILiteralNode n)
        {
            if (n.DataType == null) throw new RdfQueryException("Cannot convert an untyped Literal to a Time Span");
            return TimeSpan.Parse(n.Value);
        }

        #endregion

        #region Query Formatting

        /// <summary>
        /// Gets a SPARQL Formatter to use in formatting Queries as Strings
        /// </summary>
        internal static SparqlFormatter Formatter
        {
            get
            {
                if (_formatter == null) _formatter = new SparqlFormatter();
                return _formatter;
            }
        }

        #endregion
    }
}
