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
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Aggregates.Sparql;
using VDS.RDF.Query.Expressions.Functions.Sparql;
using VDS.RDF.Query.Expressions.Functions.Sparql.Boolean;
using VDS.RDF.Query.Expressions.Functions.Sparql.Constructor;
using VDS.RDF.Query.Expressions.Functions.Sparql.DateTime;
using VDS.RDF.Query.Expressions.Functions.Sparql.Hash;
using VDS.RDF.Query.Expressions.Functions.Sparql.Numeric;
using VDS.RDF.Query.Expressions.Functions.Sparql.String;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query.Expressions
{
    /// <summary>
    /// Expression Factory which generates SPARQL Function expressions
    /// </summary>
    /// <remarks>
    /// This supports the requirement of SPARQL 1.1 that all functions can be accessed via URI as well as by keyword.  This also means that SPARQL 1.1 functions can be used in SPARQL 1.0 mode by using their URIs instead of their keywords and they are then treated simply as extension functions
    /// </remarks>
    public class SparqlBuiltInFunctionFactory 
        : ISparqlCustomExpressionFactory
    {
        /// <summary>
        /// Namespace Uri for SPARQL Built In Functions Namespace
        /// </summary>
        public const String SparqlFunctionsNamespace = "http://www.w3.org/ns/sparql#";

        /// <summary>
        /// Tries to create a SPARQL Function expression if the function Uri correseponds to a supported SPARQL Function
        /// </summary>
        /// <param name="u">Function Uri</param>
        /// <param name="args">Function Arguments</param>
        /// <param name="scalarArguments">Scalar Arguments</param>
        /// <param name="expr">Generated Expression</param>
        /// <returns>Whether an expression was successfully generated</returns>
        public bool TryCreateExpression(Uri u, List<ISparqlExpression> args, Dictionary<string, ISparqlExpression> scalarArguments, out ISparqlExpression expr)
        {
            String func = u.ToString();
            if (func.StartsWith(SparqlFunctionsNamespace))
            {
                func = func.Substring(SparqlFunctionsNamespace.Length);
                func = func.ToUpper();

                // If any Scalar Arguments are present then can't be a SPARQL Function UNLESS it is
                // a GROUP_CONCAT function and it has the SEPARATOR argument
                if (scalarArguments.Count > 0)
                {
                    if (func.Equals(SparqlSpecsHelper.SparqlKeywordGroupConcat) && scalarArguments.Count == 1 && scalarArguments.ContainsKey(SparqlSpecsHelper.SparqlKeywordSeparator))
                    {
                        // OK
                    }
                    else
                    {
                        expr = null;
                        return false;
                    }
                }

                // Q: Will there be special URIs for the DISTINCT modified forms of aggregates?

                ISparqlExpression sparqlFunc = null;
                switch (func)
                {
                    case SparqlSpecsHelper.SparqlKeywordAbs:
                        if (args.Count == 1)
                        {
                            sparqlFunc = new AbsFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the SPARQL ABS() function");
                        }
                        break;
                    case SparqlSpecsHelper.SparqlKeywordAvg:
                        if (args.Count == 1)
                        {
                            sparqlFunc = new AggregateTerm(new AverageAggregate(args.First()));
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the SPARQL AVG() aggregate");
                        }
                        break;
                    case SparqlSpecsHelper.SparqlKeywordBound:
                        if (args.Count == 1)  
                        {
                            if (args[0] is VariableTerm)
                            {
                                sparqlFunc = new BoundFunction((VariableTerm)args[0]);
                            }
                            else
                            {
                                throw new RdfParseException("The SPARQL BOUND() function only operates over Variables");
                            }
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the SPARQL BOUND() function");
                        }
                        break;
                    case SparqlSpecsHelper.SparqlKeywordCeil:
                        if (args.Count == 1)
                        {
                            sparqlFunc = new CeilFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the SPARQL CEIL() function");
                        }
                        break;
                    case SparqlSpecsHelper.SparqlKeywordCoalesce:
                        if (args.Count >= 1)
                        {
                            sparqlFunc = new CoalesceFunction(args);
                        }
                        else
                        {
                            throw new RdfParseException("The SPARQL COALESCE() function requires at least 1 argument");
                        }
                        break;
                    case SparqlSpecsHelper.SparqlKeywordConcat:
                        if (args.Count >= 1)
                        {
                            sparqlFunc = new ConcatFunction(args);
                        }
                        else
                        {
                            throw new RdfParseException("The SPARQL CONCAT() function requires at least 1 argument");
                        }
                        break;
                    case SparqlSpecsHelper.SparqlKeywordContains:
                        if (args.Count == 2)
                        {
                            sparqlFunc = new ContainsFunction(args[0], args[1]);
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the SPARQL CONTAINS() function");
                        }
                        break;
                    case SparqlSpecsHelper.SparqlKeywordCount:
                        // Q: What will the URIs be for the special forms of COUNT?
                        if (args.Count == 1)
                        {
                            sparqlFunc = new AggregateTerm(new CountAggregate(args.First()));
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the SPARQL COUNT() aggregate");
                        }
                        break;
                    case SparqlSpecsHelper.SparqlKeywordDataType:
                        if (args.Count == 1)
                        {
                            sparqlFunc = new DataTypeFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the SPARQL DATATYPE() function");
                        }
                        break;
                    case SparqlSpecsHelper.SparqlKeywordDay:
                        if (args.Count == 1)
                        {
                            sparqlFunc = new DayFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the SPARQL DAY() function");
                        }
                        break;
                    case SparqlSpecsHelper.SparqlKeywordEncodeForUri:
                        if (args.Count == 1)
                        {
                            sparqlFunc = new EncodeForUriFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the SPARQL ENCODE_FOR_URI() function");
                        }
                        break;
                    case SparqlSpecsHelper.SparqlKeywordFloor:
                        if (args.Count == 1)
                        {
                            sparqlFunc = new FloorFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the SPARQL FLOOR() function");
                        }
                        break;
                    case SparqlSpecsHelper.SparqlKeywordGroupConcat:
                        if (args.Count == 1)
                        {
                            if (scalarArguments.ContainsKey(SparqlSpecsHelper.SparqlKeywordSeparator))
                            {
                                sparqlFunc = new AggregateTerm(new GroupConcatAggregate(args.First(), scalarArguments[SparqlSpecsHelper.SparqlKeywordSeparator]));
                            }
                            else
                            {
                                sparqlFunc = new AggregateTerm(new GroupConcatAggregate(args.First()));
                            }
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the SPARQL GROUP_CONCAT() aggregate");
                        }
                        break;
                    case SparqlSpecsHelper.SparqlKeywordHours:
                        if (args.Count == 1)
                        {
                            sparqlFunc = new HoursFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the SPARQL HOURS() function");
                        }
                        break;
                    case SparqlSpecsHelper.SparqlKeywordIf:
                        if (args.Count == 3)
                        {
                            sparqlFunc = new IfElseFunction(args[0], args[1], args[2]);
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the SPARQL IF() function");
                        }
                        break;
                    case SparqlSpecsHelper.SparqlKeywordIri:
                        if (args.Count == 1)
                        {
                            sparqlFunc = new IriFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the SPARQL IRI() function");
                        }
                        break;
                    case SparqlSpecsHelper.SparqlKeywordIsBlank:
                        if (args.Count == 1)
                        {
                            sparqlFunc = new IsBlankFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the SPARQL ISBLANK() function");
                        }
                        break;
                    case SparqlSpecsHelper.SparqlKeywordIsIri:
                        if (args.Count == 1)
                        {
                            sparqlFunc = new IsIriFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the SPARQL ISIRI() function");
                        }
                        break;
                    case SparqlSpecsHelper.SparqlKeywordIsLiteral:
                        if (args.Count == 1)
                        {
                            sparqlFunc = new IsLiteralFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the SPARQL ISLITERAL() function");
                        }
                        break;
                    case SparqlSpecsHelper.SparqlKeywordIsNumeric:
                        if (args.Count == 1)
                        {
                            sparqlFunc = new IsNumericFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the SPARQL ISNUMERIC() function");
                        }
                        break;
                    case SparqlSpecsHelper.SparqlKeywordIsUri:
                        if (args.Count == 1)
                        {
                            sparqlFunc = new IsUriFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the SPARQL ISURI() function");
                        }
                        break;
                    case SparqlSpecsHelper.SparqlKeywordLang:
                        if (args.Count == 1)
                        {
                            sparqlFunc = new LangFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the SPARQL LANG() function");
                        }
                        break;
                    case SparqlSpecsHelper.SparqlKeywordLangMatches:
                        if (args.Count == 2)
                        {
                            sparqlFunc = new LangMatchesFunction(args[0], args[1]);
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the SPARQL LANGMATCHES() function");
                        }
                        break;
                    case SparqlSpecsHelper.SparqlKeywordLCase:
                        if (args.Count == 1)
                        {
                            sparqlFunc = new LCaseFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the SPARQL LCASE() function");
                        }
                        break;
                    case SparqlSpecsHelper.SparqlKeywordMax:
                        if (args.Count == 1)
                        {
                            sparqlFunc = new AggregateTerm(new MaxAggregate(args.First()));
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the SPARQL MAX() aggregate");
                        }
                        break;
                    case SparqlSpecsHelper.SparqlKeywordMD5:
                        if (args.Count == 1)
                        {
                            sparqlFunc = new MD5HashFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the SPARQL MD5() function");
                        }
                        break;
                    case SparqlSpecsHelper.SparqlKeywordMin:
                        if (args.Count == 1)
                        {
                            sparqlFunc = new AggregateTerm(new MinAggregate(args.First()));
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the SPARQL MIN() aggregate");
                        }
                        break;
                    case SparqlSpecsHelper.SparqlKeywordMinutes:
                        if (args.Count == 1)
                        {
                            sparqlFunc = new MinutesFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the SPARQL MINUTES() function");
                        }
                        break;
                    case SparqlSpecsHelper.SparqlKeywordMonth:
                        if (args.Count == 1)
                        {
                            sparqlFunc = new MonthFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the SPARQL MONTH() function");
                        }
                        break;
                    case SparqlSpecsHelper.SparqlKeywordNow:
                        if (args.Count == 0)
                        {
                            sparqlFunc = new NowFunction();
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the SPARQL ABS() function");
                        }
                        break;
                    case SparqlSpecsHelper.SparqlKeywordRegex:
                        if (args.Count == 2)
                        {
                            sparqlFunc = new RegexFunction(args[0], args[1]);
                        }
                        else if (args.Count == 3)
                        {
                            sparqlFunc = new RegexFunction(args[0], args[1], args[2]);
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the SPARQL REGEX() function");
                        }
                        break;
                    case SparqlSpecsHelper.SparqlKeywordRound:
                        if (args.Count == 1)
                        {
                            sparqlFunc = new RoundFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the SPARQL ROUND() function");
                        }
                        break;
                    case SparqlSpecsHelper.SparqlKeywordSameTerm:
                        if (args.Count == 2)
                        {
                            sparqlFunc = new SameTermFunction(args[0], args[1]);
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the SPARQL SAMETERM() function");
                        }
                        break;
                    case SparqlSpecsHelper.SparqlKeywordSample:
                        if (args.Count == 1)
                        {
                            sparqlFunc = new AggregateTerm(new SampleAggregate(args.First()));
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the SPARQL AVG() aggregate");
                        }
                        break;
                    case SparqlSpecsHelper.SparqlKeywordSeconds:
                        if (args.Count == 1)
                        {
                            sparqlFunc = new SecondsFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the SPARQL SECONDS() function");
                        }
                        break;
                    case SparqlSpecsHelper.SparqlKeywordSha1:
                        if (args.Count == 1)
                        {
                            sparqlFunc = new Sha1HashFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the SPARQL SHA1() function");
                        }
                        break;
                    case SparqlSpecsHelper.SparqlKeywordSha256:
                        if (args.Count == 1)
                        {
                            sparqlFunc = new Sha256HashFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the SPARQL SHA256() function");
                        }
                        break;
                    case SparqlSpecsHelper.SparqlKeywordSha384:
                        if (args.Count == 1)
                        {
                            sparqlFunc = new Sha384HashFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the SPARQL SHA384() function");
                        }
                        break;
                    case SparqlSpecsHelper.SparqlKeywordSha512:
                        if (args.Count == 1)
                        {
                            sparqlFunc = new Sha512HashFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the SPARQL SHA512() function");
                        }
                        break;
                    case SparqlSpecsHelper.SparqlKeywordStr:
                        if (args.Count == 1)
                        {
                            sparqlFunc = new StrFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the SPARQL STR() function");
                        }
                        break;
                    case SparqlSpecsHelper.SparqlKeywordStrDt:
                        if (args.Count == 2)
                        {
                            sparqlFunc = new StrDtFunction(args[0], args[1]);
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the SPARQL STRDT() function");
                        }
                        break;
                    case SparqlSpecsHelper.SparqlKeywordStrEnds:
                        if (args.Count == 2)
                        {
                            sparqlFunc = new StrEndsFunction(args[0], args[1]);
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the SPARQL STRENDS() function");
                        }
                        break;
                    case SparqlSpecsHelper.SparqlKeywordStrLang:
                        if (args.Count == 2)
                        {
                            sparqlFunc = new StrLangFunction(args[0], args[1]);
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the SPARQL STRLANG() function");
                        }
                        break;
                    case SparqlSpecsHelper.SparqlKeywordStrLen:
                        if (args.Count == 1)
                        {
                            sparqlFunc = new StrLenFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the SPARQL STRKEN() function");
                        }
                        break;
                    case SparqlSpecsHelper.SparqlKeywordStrStarts:
                        if (args.Count == 2)
                        {
                            sparqlFunc = new StrStartsFunction(args[0], args[1]);
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the SPARQL STRSTARTS() function");
                        }
                        break;
                    case SparqlSpecsHelper.SparqlKeywordSubStr:
                        if (args.Count == 2)
                        {
                            sparqlFunc = new SubStrFunction(args[0], args[1]);
                        }
                        else if (args.Count == 3)
                        {
                            sparqlFunc = new SubStrFunction(args[0], args[1], args[2]);
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the SPARQL SUBSTR() function");
                        }
                        break;
                    case SparqlSpecsHelper.SparqlKeywordSum:
                        if (args.Count == 1)
                        {
                            sparqlFunc = new AggregateTerm(new SumAggregate(args.First()));
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the SPARQL SUM() aggregate");
                        }
                        break;
                    case SparqlSpecsHelper.SparqlKeywordTimezone:
                        if (args.Count == 1)
                        {
                            sparqlFunc = new TimezoneFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the SPARQL TIMEZONE() function");
                        }
                        break;
                    case SparqlSpecsHelper.SparqlKeywordTz:
                        if (args.Count == 1)
                        {
                            sparqlFunc = new TZFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the SPARQL TZ() function");
                        }
                        break;
                    case SparqlSpecsHelper.SparqlKeywordUCase:
                        if (args.Count == 1)
                        {
                            sparqlFunc = new UCaseFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the SPARQL UCASE() function");
                        }
                        break;
                    case SparqlSpecsHelper.SparqlKeywordUri:
                        if (args.Count == 1)
                        {
                            sparqlFunc = new IriFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the SPARQL URI() function");
                        }
                        break;
                    case SparqlSpecsHelper.SparqlKeywordYear:
                        if (args.Count == 1)
                        {
                            sparqlFunc = new YearFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the SPARQL YEAR() function");
                        }
                        break;
                }

                if (sparqlFunc != null)
                {
                    expr = sparqlFunc;
                    return true;
                }
            }
            expr = null;
            return false;
        }

        /// <summary>
        /// Gets the URIs of available SPARQL Functions
        /// </summary>
        public IEnumerable<Uri> AvailableExtensionFunctions
        {
            get 
            {
                return (from u in SparqlSpecsHelper.FunctionKeywords
                        select UriFactory.Create(SparqlFunctionsNamespace + u.ToLower()));
            }
        }

        /// <summary>
        /// Gets the URIs of available SPARQL Aggregates
        /// </summary>
        public IEnumerable<Uri> AvailableExtensionAggregates
        {
            get 
            {
                return (from u in SparqlSpecsHelper.AggregateFunctionKeywords
                        select UriFactory.Create(SparqlFunctionsNamespace + u.ToLower()));
            }
        }
    }
}
