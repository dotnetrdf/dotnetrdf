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

using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using VDS.RDF.Nodes;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Query;

/// <summary>
/// Class containing Helper information and methods pertaining to the Sparql Query Language for RDF.
/// </summary>
public static class SparqlSpecsHelper
{
    #region Keywords and Constants

    /// <summary>
    /// Namespace Uri for SPARQL Namespace.
    /// </summary>
    public const string SparqlNamespace = "http://www.w3.org/2005/sparql-results#";
    /// <summary>
    /// Namespace Uri for the RDF serialization of a SPARQL Result Set.
    /// </summary>
    public const string SparqlRdfResultsNamespace = "http://www.w3.org/2001/sw/DataAccess/tests/result-set#";

    /// <summary>
    /// Keywords in Sparql.
    /// </summary>
    public const string SparqlKeywordBase = "BASE",
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
                  SparqlKeywordCall = "CALL",
                  SparqlStarKeywordIsTriple = "ISTRIPLE",
                  SparqlStarKeywordObject = "OBJECT",
                  SparqlStarKeywordPredicate = "PREDICATE",
                  SparqlStarKeywordSubject = "SUBJECT",
                  SparqlStarKeywordTriple = "TRIPLE"
                  ;

    /// <summary>
    /// Set of SPARQL Keywords that are Non-Query Keywords.
    /// </summary>
    public static string[] NonQueryKeywords = {   
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
    /// Set of SPARQL Keywords that are Function Keywords.
    /// </summary>
    public static string[] FunctionKeywords = {   
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
                                                  SparqlStarKeywordIsTriple,
                                                  SparqlKeywordIsUri, 
                                                  SparqlKeywordLang,
                                                  SparqlKeywordLangMatches, 
                                                  SparqlKeywordLCase,
                                                  SparqlKeywordMD5,
                                                  SparqlKeywordMinutes,
                                                  SparqlKeywordMonth,
                                                  SparqlKeywordNotIn,
                                                  SparqlKeywordNow,
                                                  SparqlStarKeywordObject,
                                                  SparqlStarKeywordPredicate,
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
                                                  SparqlStarKeywordSubject,
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
                                                  SparqlStarKeywordTriple,
                                                  SparqlKeywordTz,
                                                  SparqlKeywordUCase,
                                                  SparqlKeywordUri,
                                                  SparqlKeywordUUID,
                                                  SparqlKeywordYear,
                                              };

    /// <summary>
    /// Set of SPARQL Keywords that are Aggregate Keywords.
    /// </summary>
    /// <remarks>
    /// Unlike <see cref="SparqlSpecsHelper.AggregateFunctionKeywords">AggregateFunctionKeywords[]</see> this includes keywords related to aggregates (like DISTINCT) and those for Leviathan extension aggregates which are not standard SPARQL 1.1 syntax.
    /// </remarks>
    public static string[] AggregateKeywords = {   
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
                                                   SparqlKeywordSeparator,
                                               };

    /// <summary>
    /// Set of SPARQL Keywords that are built in SPARQL Aggregate Functions.
    /// </summary>
    public static string[] AggregateFunctionKeywords = {
                                                            SparqlKeywordAvg,
                                                            SparqlKeywordCount,
                                                            SparqlKeywordGroupConcat,
                                                            SparqlKeywordMax,
                                                            SparqlKeywordMin,
                                                            SparqlKeywordSum,
                                                            SparqlKeywordSample,
                                                       };


    /// <summary>
    /// Set of IRIs for supported Cast Functions.
    /// </summary>
    public static string[] SupportedCastFunctions = {   
                                                        XmlSpecsHelper.XmlSchemaDataTypeBoolean, 
                                                        XmlSpecsHelper.XmlSchemaDataTypeDateTime, 
                                                        XmlSpecsHelper.XmlSchemaDataTypeDecimal, 
                                                        XmlSpecsHelper.XmlSchemaDataTypeDouble, 
                                                        XmlSpecsHelper.XmlSchemaDataTypeFloat, 
                                                        XmlSpecsHelper.XmlSchemaDataTypeInteger, 
                                                        XmlSpecsHelper.XmlSchemaDataTypeString,
                                                    };

    /// <summary>
    /// Set of Keywords for SPARQL Query 1.0.
    /// </summary>
    public static string[] SparqlQuery10Keywords = {
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
    /// Set of additional Keywords for SPARQL Query 1.1.
    /// </summary>
    public static string[] SparqlQuery11Keywords = {
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
                                                    SparqlKeywordYear,
                                                   };

    /// <summary>
    /// Set of SPARQL Keywords that are Update Keywords.
    /// </summary>
    public static string[] UpdateKeywords = {   
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
                                                SparqlKeywordWith,
                                            };

    /// <summary>
    /// Set of Keywords for SPARQL Update 1.1.
    /// </summary>
    public static string[] SparqlUpdate11Keywords = {
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
                                                      SparqlKeywordWith,                                                   
                                                   };
                                                

    /// <summary>
    /// Regular Expression Pattern for Valid Integers in Sparql.
    /// </summary>
    public static Regex SparqlInteger = new Regex(TurtleSpecsHelper.ValidIntegerPattern);
    /// <summary>
    /// Regular Expression Pattern for Valid Decimals in Sparql.
    /// </summary>
    public static Regex SparqlDecimal = new Regex(TurtleSpecsHelper.ValidDecimalPattern);
    /// <summary>
    /// Regular Expression Pattern for Valid Doubles in Sparql.
    /// </summary>
    public static Regex SparqlDouble = new Regex(TurtleSpecsHelper.ValidDoublePattern);

    #endregion

    #region Keyword Test Functions

    /// <summary>
    /// Checks whether a given Keyword is a SPARQL Query Keyword.
    /// </summary>
    /// <param name="keyword">Keyword to check.</param>
    /// <returns></returns>
    public static bool IsQueryKeyword(string keyword)
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
    /// Checks whether a given Keyword is a SPARQL Non-Query Keyword.
    /// </summary>
    /// <param name="keyword">Keyword to check.</param>
    /// <returns></returns>
    public static bool IsNonQueryKeyword(string keyword)
    {
        return NonQueryKeywords.Contains(keyword, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Checks whether a given Keyword is a SPARQL Function Verb.
    /// </summary>
    /// <param name="keyword">Keyword to check.</param>
    /// <returns></returns>
    public static bool IsFunctionKeyword(string keyword)
    {
        return FunctionKeywords.Contains(keyword, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Checks whether a given Keyword is a SPARQL 1.1 Function Verb.
    /// </summary>
    /// <param name="keyword">Keyword to check.</param>
    /// <returns></returns>
    public static bool IsFunctionKeyword11(string keyword)
    {
        return IsFunctionKeyword(keyword) && SparqlQuery11Keywords.Contains(keyword, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Checks whether a given Keyword is a SPARQL Aggregate Keyword (includes keywords related to aggregates like DISTINCT, AS and Leviathan extension aggregate keywords).
    /// </summary>
    /// <param name="keyword">Keyword to check.</param>
    /// <returns></returns>
    public static bool IsAggregateKeyword(string keyword)
    {
        return AggregateKeywords.Contains(keyword, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Checks whether a given Keyword is a SPARQL Aggregate Function Keyword (only keywords for the SPARQL built-in aggregate functions).
    /// </summary>
    /// <param name="keyword">Keyword to check.</param>
    /// <returns></returns>
    public static bool IsAggregateFunctionKeyword(string keyword)
    {
        return AggregateFunctionKeywords.Contains(keyword, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Checks whether a given Keyword is a SPARQL Update Keyword.
    /// </summary>
    /// <param name="keyword"></param>
    /// <returns></returns>
    public static bool IsUpdateKeyword(string keyword)
    {
        return UpdateKeywords.Contains(keyword, StringComparer.OrdinalIgnoreCase);
    }

    #endregion

    #region QName and Variable Name Validation

    /// <summary>
    /// Checks whether a given QName is valid in Sparql.
    /// </summary>
    /// <param name="value">QName to check.</param>
    /// <param name="syntax">SPARQL Syntax.</param>
    /// <returns></returns>
    public static bool IsValidQName(string value, SparqlQuerySyntax syntax)
    {
        if (!value.Contains(':')) 
        {
            // Must have a Colon in a QName
            return false;
        } 
        else
        {
            // Split into Prefix and Local Name
            var parts = value.Split(':');

            // If SPARQL 1.0 then can only have two sections
            if (syntax == SparqlQuerySyntax.Sparql_1_0 && parts.Length > 2) return false;

            // All sections ending in a colon (i.e. all but the last) must match PN_PREFIX production
            for (var i = 0; i < parts.Length - 1; i++)
            {
                if (!IsPNPrefix(parts[i].ToCharArray())) return false;
            }
            // Final section must match PN_LOCAL
            return IsPNLocal(parts[parts.Length - 1].ToCharArray(), syntax);
        }
    }

    /// <summary>
    /// Checks whether a given Variable Name is valid in Sparql.
    /// </summary>
    /// <param name="value">Variable Name to check.</param>
    /// <returns></returns>
    public static bool IsValidVarName(string value)
    {
        var cs = value.ToCharArray(1,value.Length-1);

        // Variable Names can't be empty
        if (cs.Length == 0)
        {
            return false;
        }

        // First Character must be from PN_CHARS_U or a digit
        var first = cs[0];
        if (char.IsDigit(first) || IsPNCharsU(first))
        {
            if (cs.Length > 1)
            {
                for (var i = 1; i < cs.Length; i++)
                {
                    if (i < cs.Length - 1)
                    {
                        // Subsequent Chars must be from PN_CHARS (except -) or a '.'
                        if (cs[i] == '.' || cs[i] == '-') return false;
                        if (!IsPNChars(cs[i])) return false;
                    }
                }
                return true;
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
    /// Gets whether a given prefix declaration is valid in SPARQL.
    /// </summary>
    /// <param name="value">Prefix declaration.</param>
    /// <returns></returns>
    public static bool IsValidPrefix(string value)
    {
        // Empty string is not a valid prefix
        if (value.Length == 0) return false;
        // Prefix must end with a colon
        if (!value.EndsWith(":")) return false;
        // Empty prefix is valid
        if (value.Length == 1) return true;
        // Otherwise must match IsPNPrefix() production
        // Remember to remove the terminating : which we have already validated
        return IsPNPrefix(value.Substring(0, value.Length - 1).ToCharArray());
    }

    /// <summary>
    /// Gets whether a given BNode ID is valid.
    /// </summary>
    /// <param name="value">Value.</param>
    /// <returns></returns>
    public static bool IsValidBNode(string value)
    {
        // Must be at least 3 characters
        if (value.Length < 3) return false;
        // Must start with _:
        if (!value.StartsWith("_:")) return false;

        var cs = value.Substring(2).ToCharArray();

        // First character must be PN_CHARS_U or digit
        if (!char.IsDigit(cs[0]) && !IsPNCharsU(cs[0])) return false;

        // If only one character it's a valid identifier since we've validated the first character
        if (cs.Length == 1) return true;

        // Otherwise we need to validate the rest of the identifier
        for (var i = 1; i < cs.Length; i++)
        {
            if (i < cs.Length - 1)
            {
                // Middle characters may be PN_CHARS or a .
                if (cs[i] != '.' && !IsPNChars(cs[i])) return false;
            }
            else
            {
                // Final character must be in PN_CHARS
                return IsPNChars(cs[i]);
            }
        }
        // Should be impossible to get here but must keep the compiler happy
        return false;            
    }

    /// <summary>
    /// Checks whether a given Character matches the PN_CHARS_BASE rule from the Sparql Specification.
    /// </summary>
    /// <param name="c">Character to test.</param>
    /// <returns></returns>
    public static bool IsPNCharsBase(char c)
    {
        if (c >= 'A' && c <= 'Z')
        {
            return true;
        }
        else if (c >= 'a' && c <= 'z')
        {
            return true;
        }
        else if ((c >= 0x00c0 && c <= 0x00d6) ||
                 (c >= 0x00d8 && c <= 0x00f6) ||
                 (c >= 0x00f8 && c <= 0x02ff) ||
                 (c >= 0x0370 && c <= 0x037d) ||
                 (c >= 0x037f && c <= 0x1fff) ||
                 (c >= 0x200c && c <= 0x200d) ||
                 (c >= 0x2070 && c <= 0x218f) ||
                 (c >= 0x2c00 && c <= 0x2fef) ||
                 (c >= 0x3001 && c <= 0xd7ff) ||
                 (c >= 0xf900 && c <= 0xfdcf) ||
                 (c >= 0xfdf0 && c <= 0xfffd) /*||
                 (c >= 0x10000 && c <= 0xeffff)*/)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Checks whether a given Character matches the PN_CHARS_U rule from the SPARQL Specification.
    /// </summary>
    /// <param name="c">Character to test.</param>
    /// <returns></returns>
    public static bool IsPNCharsU(char c)
    {
        return (c == '_' || IsPNCharsBase(c));
    }

    /// <summary>
    /// Checks whether a given Character matches the PN_CHARS rule from the SPARQL Specification.
    /// </summary>
    /// <param name="c">Character to test.</param>
    /// <returns></returns>
    public static bool IsPNChars(char c)
    {
        if (c == '-' || char.IsDigit(c))
        {
            return true;
        }
        else if (c == 0x00b7)
        {
            return true;
        }
        else if (IsPNCharsU(c))
        {
            return true;
        }
        else if ((c >= 0x0300 && c <= 0x036f) ||
                 (c >= 0x204f && c <= 0x2040))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Checks whether a given String matches the PN_LOCAL rule from the Sparql Specification.
    /// </summary>
    /// <param name="cs">String as character array.</param>
    /// <param name="syntax">SPARQL Syntax.</param>
    /// <returns></returns>
    public static bool IsPNLocal(char[] cs, SparqlQuerySyntax syntax)
    {
        if (cs.Length == 0)
        {
            // Empty Local Names are valid
            return true;
        }

        // First character must be a digit or from PN_CHARS_U
        var first = cs[0];
        var start = 0;
        if (char.IsDigit(first) || IsPNCharsU(first) ||
            (syntax != SparqlQuerySyntax.Sparql_1_0 && IsPLX(cs, 0, out start)))
        {
            if (start > 0)
            {
                // Means the first thing was a PLX
                // If the only thing in the local name was a PLX this is valid
                if (start == cs.Length - 1) return true;
                // If there are further characters we'll start 
            }
            else
            {
                // Otherwise we need to check the rest of the characters
                start = 1;
            }

            // Check the rest of the characters
            if (cs.Length > start)
            {
                for (var i = start; i < cs.Length; i++)
                {
                    if (i < cs.Length - 1)
                    {
                        // Middle characters may be from PN_CHARS or '.'
                        var j = i;
                        if (!(cs[i] == '.' || IsPNChars(cs[i]) || 
                              (syntax != SparqlQuerySyntax.Sparql_1_0 && IsPLX(cs, i, out j))
                            ))
                        {
                            return false;
                        }
                        if (i != j)
                        {
                            // This means we just saw a PLX
                            // Last thing being a PLX is valid
                            if (j == cs.Length - 1) return true;
                            // Otherwise adjust the index appropriately and continue checking further characters
                            i = j;
                        }
                    }
                    else
                    {
                        // Last Character must be from PN_CHARS if it wasn't a PLX which is handled elsewhere
                        return IsPNChars(cs[i]);
                    }
                }

                // Should never get here but have to add this to keep compiler happy
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
    /// Checks whether a given String matches the PN_PREFIX rule from the SPARQL Specification.
    /// </summary>
    /// <param name="cs">String as character array.</param>
    /// <returns></returns>
    public static bool IsPNPrefix(char[] cs)
    {
        // Empty Prefixes are valid
        if (cs.Length == 0) return true;

        // First character must be from PN_CHARS_BASE
        var first = cs[0];
        if (IsPNCharsBase(first))
        {
            if (cs.Length > 1)
            {
                for (var i = 1; i < cs.Length; i++)
                {
                    if (i < cs.Length - 1)
                    {
                        // Middle characters may be from PN_CHARS or '.'
                        if (!(cs[i] == '.' || IsPNChars(cs[i])))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        // Last Character must be from PN_CHARS
                        return IsPNChars(cs[i]);
                    }
                }

                // Should never get here but have to add this to keep compiler happy
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

    /// <summary>
    /// Checks whether a given String matches the PLX rule from the SPARQL Specification.
    /// </summary>
    /// <param name="cs">String as character array.</param>
    /// <param name="startIndex">Start Index.</param>
    /// <param name="endIndex">Resulting End Index.</param>
    /// <returns></returns>
    public static bool IsPLX(char[] cs, int startIndex, out int endIndex)
    {
        endIndex = startIndex;
        if (cs[startIndex] == '%')
        {
            if (startIndex >= cs.Length - 2)
            {
                // If we saw a base % but there are not two subsequent characters not a valid PLX escape
                return false;
            }
            else
            {
                var a = cs[startIndex + 1];
                var b = cs[startIndex + 2];
                if (IsHex(a) && IsHex(b))
                {
                    // Valid % encoding
                    endIndex = startIndex + 2;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        else if (cs[startIndex] == '\\')
        {
            if (startIndex >= cs.Length - 1)
            {
                // If we saw a backslash but no subsequent character not a valid PLX escape
                return false;
            }
            else
            {
                var c = cs[startIndex + 1];
                switch (c)
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
                        // Valid Escape
                        endIndex = startIndex + 1;
                        return true;
                    default:
                        return false;
                }
            }
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Gets whether a character is a Hex character.
    /// </summary>
    /// <param name="c">Character.</param>
    /// <returns></returns>
    public static bool IsHex(char c)
    {
        if (char.IsDigit(c))
        {
            return true;
        }
        else
        {
            switch (c)
            {
                case 'A':
                case 'a':
                case 'B':
                case 'b':
                case 'C':
                case 'c':
                case 'D':
                case 'd':
                case 'E':
                case 'f':
                case 'F':
                    return true;
                default:
                    return false;
            }
        }
    }

    /// <summary>
    /// Unescapes local name escapes from QNames.
    /// </summary>
    /// <param name="value">Value to unescape.</param>
    /// <returns></returns>
    public static string UnescapeQName(string value)
    {
        if (value.Contains('\\') || value.Contains('%'))
        {
            var output = new StringBuilder();
            output.Append(value.Substring(0, value.IndexOf(':')));
            var cs = value.ToCharArray();
            for (var i = output.Length; i < cs.Length; i++)
            {
                if (cs[i] == '\\')
                {
                    if (i == cs.Length - 1) throw new RdfParseException("Invalid backslash to start an escape at the end of the Local Name, expecting a single character after the backslash");
                    var esc = cs[i + 1];
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
                    // Remember that we are supposed to preserve precent encoded characters as-is
                    // Simply need to validate that they are valid encoding
                    if (i > cs.Length - 2)
                    {
                        throw new RdfParseException("Invalid % to start a percent encoded character in a Local Name, two hex digits are required after a %, use \\% to denote a percent character directly");
                    }
                    else
                    {
                        if (!IsHex(cs[i + 1]) || !IsHex(cs[i + 2]))
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

    #endregion

    #region Plan Literal Validation

    /// <summary>
    /// Checks whether the given value is a valid Numeric Literal in Sparql.
    /// </summary>
    /// <param name="value">Value to test.</param>
    /// <returns></returns>
    public static bool IsValidNumericLiteral(string value)
    {
        return (SparqlInteger.IsMatch(value) || SparqlDecimal.IsMatch(value) || SparqlDouble.IsMatch(value));
    }

    /// <summary>
    /// Checks whether the given value is a valid Integer Literal in Sparql.
    /// </summary>
    /// <param name="value">Value to test.</param>
    /// <returns></returns>
    public static bool IsInteger(string value)
    {
        return SparqlInteger.IsMatch(value);
    }

    /// <summary>
    /// Checks whether the given value is a valid Decimal Literal in Sparql.
    /// </summary>
    /// <param name="value">Value to test.</param>
    /// <returns></returns>
    public static bool IsDecimal(string value)
    {
        return SparqlDecimal.IsMatch(value) || SparqlInteger.IsMatch(value);
    }

    /// <summary>
    /// Checks whether the given value is a valid Float Literal in Sparql.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool IsFloat(string value)
    {
        return IsDouble(value);
    }

    /// <summary>
    /// Checks whether the given value is a valid Double Literal in Sparql.
    /// </summary>
    /// <param name="value">Value to test.</param>
    /// <returns></returns>
    public static bool IsDouble(string value)
    {
        return SparqlDouble.IsMatch(value) || SparqlDecimal.IsMatch(value) || SparqlInteger.IsMatch(value);
    }

    #endregion

    /// <summary>
    /// Calculates the Effective Boolean Value of a given Node according to the Sparql specification.
    /// </summary>
    /// <param name="n">Node to computer EBV for.</param>
    /// <returns></returns>
    [Obsolete("Replaced by the INode.EffectiveBooleanValue extension method.")]
    public static bool EffectiveBooleanValue(INode n)
    {
        return n.EffectiveBooleanValue();
    }

    /// <summary>
    /// Checks whether the Query is a SELECT Query.
    /// </summary>
    /// <param name="type">Query Type.</param>
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
    /// Implements Node Equality with SPARQL Semantics.
    /// </summary>
    /// <param name="x">Node.</param>
    /// <param name="y">Node.</param>
    /// <returns></returns>
    public static bool Equality(INode x, INode y)
    {
        if (x == null || y == null)
        {
            // Nulls can't be equal to each other
            throw new RdfQueryException("Cannot evaluate equality when one/both arguments are null");
        }

        if (x.NodeType != y.NodeType)
        {
            // Different Type Nodes are never equal to each other
            return false;
        }

        if (x.NodeType == NodeType.Literal)
        {
            // Do they have supported Data Types?
            string xtype, ytype;
            try 
            {
                xtype = XmlSpecsHelper.GetSupportedDataType(x);
                ytype = XmlSpecsHelper.GetSupportedDataType(y);
            } 
            catch (RdfException) 
            {
                // Can't determine a Data Type for one/both of the Nodes so use RDF Term equality instead
                return x.Equals(y);
            }

            if (xtype.Equals(string.Empty) || ytype.Equals(string.Empty))
            {
                // One/both has an unknown type
                if (x.Equals(y))
                {
                    // If RDF Term equality returns true then we return true;
                    return true;
                }
                else
                {
                    // If RDF Term equality returns false then we error
                    throw new RdfQueryException("Unable to determine equality since one/both arguments has an Unknown Type");
                }
            }
            else
            {
                // Both have known types
                var numtype = (SparqlNumericType)Math.Max((int)NumericTypesHelper.GetNumericTypeFromDataTypeUri(xtype), (int)NumericTypesHelper.GetNumericTypeFromDataTypeUri(ytype));
                if (numtype != SparqlNumericType.NaN)
                {
                    // Both are Numeric so use Numeric equality
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
                        // If this errors try RDF Term equality since 
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
                            // Both Strings so use Lexical string equality
                            return ((ILiteralNode)x).Value.Equals(((ILiteralNode)y).Value);
                        default:
                            // Use value equality
                            return (x.CompareTo(y) == 0);
                    }
                }
                else
                {
                    var commontype = XmlSpecsHelper.GetCompatibleSupportedDataType(xtype, ytype);
                    if (commontype.Equals(string.Empty))
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

        if (x.NodeType == NodeType.Triple)
        {
            // Use the equality helper recursively on subject, predicate and object
            Triple xt = (x as ITripleNode)?.Triple;
            Triple yt = (y as ITripleNode)?.Triple;
            if (xt == null || yt == null) return false;

            return Equality(xt.Subject, yt.Subject) &&
                   Equality(xt.Predicate, yt.Predicate) &&
                   Equality(xt.Object, yt.Object);
        }

        // For any other Node types equality is RDF Term equality
        return x.Equals(y);
    }

    /// <summary>
    /// Implements Node Inequality with SPARQL Semantics.
    /// </summary>
    /// <param name="x">Node.</param>
    /// <param name="y">Node.</param>
    /// <returns></returns>
    public static bool Inequality(INode x, INode y)
    {
        if (x == null || y == null)
        {
            // Nulls can't be equal to each other
            throw new RdfQueryException("Cannot evaluate inequality when one/both arguments are null");
        }
        else if (x.NodeType != y.NodeType)
        {
            // Different Type Nodes are never equal to each other
            return true;
        }
        else if (x.NodeType == NodeType.Literal)
        {
            // Do they have supported Data Types?
            string xtype, ytype;
            try
            {
                xtype = XmlSpecsHelper.GetSupportedDataType(x);
                ytype = XmlSpecsHelper.GetSupportedDataType(y);
            }
            catch (RdfException)
            {
                // Can't determine a Data Type for one/both of the Nodes so use RDF Term equality instead
                return !x.Equals(y);
            }

            if (xtype == RdfSpecsHelper.RdfLangString || ytype == RdfSpecsHelper.RdfLangString)
            {
                if (xtype == ytype)
                {
                    // use direct literal comparison for language tagged literals
                    return !x.Equals(y);
                }
                // otherwise a langString cannot equal a non-langString literal
                return true;
            }
            if (xtype.Equals(string.Empty) || ytype.Equals(string.Empty))
            {
                // One/both has an unknown type
                return false;
            }
            else
            {
                // Both have known types
                SparqlNumericType xnumtype = NumericTypesHelper.GetNumericTypeFromDataTypeUri(xtype);
                SparqlNumericType ynumtype = NumericTypesHelper.GetNumericTypeFromDataTypeUri(ytype);
                var numtype = (SparqlNumericType)Math.Max((int)xnumtype, (int)ynumtype);
                if (numtype != SparqlNumericType.NaN)
                {
                    if ((xnumtype != SparqlNumericType.NaN &&
                         x.AsValuedNode().NumericType == SparqlNumericType.NaN) ||
                        (ynumtype != SparqlNumericType.NaN &&
                         y.AsValuedNode().NumericType == SparqlNumericType.NaN))
                    {
                        // A node with a numeric datatype has a value that cannot be parsed as such. So comparison must fail.
                        return false;
                    }

                    if (xnumtype == SparqlNumericType.NaN || ynumtype == SparqlNumericType.NaN)
                    {
                        // If one is non-numeric then we SHOULD assume non-equality
                        return true;
                    }

                    // Both are Numeric so use Numeric equality
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
                        // If this errors try RDF Term equality since 
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
                            // Both Strings so use Lexical string equality
                            return !((ILiteralNode)x).Value.Equals(((ILiteralNode)y).Value);
                        default:
                            // Use value equality
                            return (x.CompareTo(y) != 0);
                    }
                }
                else
                {
                    var commontype = XmlSpecsHelper.GetCompatibleSupportedDataType(xtype, ytype);
                    if (commontype.Equals(string.Empty))
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
            // For any other Node types equality is RDF Term equality
            return !x.Equals(y);
        }
    }

    /// <summary>
    /// Implements Numeric Equality with SPARQL Semantics.
    /// </summary>
    /// <param name="x">Node.</param>
    /// <param name="y">Node.</param>
    /// <param name="type">SPARQL Numeric Tyoe.</param>
    /// <returns></returns>
    public static bool NumericEquality(INode x, INode y, SparqlNumericType type)
    {
        if (x == null || y == null) throw new RdfQueryException("Cannot evaluate numeric equality when one or both arguments are Null");
        if (type == SparqlNumericType.NaN) throw new RdfQueryException("Cannot evaluate numeric equality when the Numeric Type is NaN");

        var a = (ILiteralNode)x;
        var b = (ILiteralNode)y;

        switch (type)
        {
            case SparqlNumericType.Decimal:
                return a.AsValuedNode().AsDecimal().Equals(b.AsValuedNode().AsDecimal());
            case SparqlNumericType.Double:
                return a.AsValuedNode().AsDouble().Equals(b.AsValuedNode().AsDouble());
            case SparqlNumericType.Float:
                return a.AsValuedNode().AsFloat().Equals(b.AsValuedNode().AsFloat());
            case SparqlNumericType.Integer:
                return a.AsValuedNode().AsInteger().Equals(b.AsValuedNode().AsInteger());
            default:
                throw new RdfQueryException("Cannot evaluate numeric equality since of the arguments is not numeric");
        }
    }

    /// <summary>
    /// Implements Date Time Equality with SPARQL Semantics.
    /// </summary>
    /// <param name="x">Node.</param>
    /// <param name="y">Node.</param>
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
    /// Implements Date Equality with SPARQL Semantics.
    /// </summary>
    /// <param name="x">Node.</param>
    /// <param name="y">Node.</param>
    /// <returns></returns>
    public static bool DateEquality(INode x, INode y)
    {
        if (x == null || y == null) throw new RdfQueryException("Cannot evaluate date equality when one or both arguments are Null");
        try
        {
            IValuedNode a = x.AsValuedNode();
            IValuedNode b = y.AsValuedNode();

            var strictEquals = (a.EffectiveType != b.EffectiveType);

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
    /// Implements Time Span Equality with SPARQL Semantics.
    /// </summary>
    /// <param name="x">Node.</param>
    /// <param name="y">Node.</param>
    /// <returns></returns>
    public static bool TimeSpanEquality(INode x, INode y)
    {
        if (x == null || y == null) throw new RdfQueryException("Cannot evaluate time span equality when one or both arguments are Null");
        try
        {
            var a = (ILiteralNode)x;
            var b = (ILiteralNode)y;

            TimeSpan c = a.AsValuedNode().AsTimeSpan();
            TimeSpan d = b.AsValuedNode().AsTimeSpan();

            return c.Equals(d);
        }
        catch (FormatException)
        {
            throw new RdfQueryException("Cannot evaluate time span equality since one of the arguments does not have a valid lexical value for a Time Span");
        }
    }


    #endregion

    /// <summary>
    /// Determine if the specified datatype IRI is one of the recognized IRIs for string literal data types.
    /// </summary>
    /// <param name="datatype">The datatype IRI.</param>
    /// <returns>True if <paramref name="datatype"/> is either of the XML Schema string datatype IRI, or the RDF langString datatype IRI.</returns>
    public static bool IsStringDatatype(string datatype)
    {
        return RdfSpecsHelper.RdfLangString.Equals(datatype) ||
               XmlSpecsHelper.XmlSchemaDataTypeString.Equals(datatype);
    }
}
