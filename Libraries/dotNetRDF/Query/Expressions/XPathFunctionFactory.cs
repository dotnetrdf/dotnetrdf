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
using VDS.RDF.Nodes;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Aggregates.XPath;
using VDS.RDF.Query.Expressions.Conditional;
using VDS.RDF.Query.Expressions.Functions.XPath;
using VDS.RDF.Query.Expressions.Functions.XPath.DateTime;
using VDS.RDF.Query.Expressions.Functions.XPath.Numeric;
using VDS.RDF.Query.Expressions.Functions.XPath.String;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query.Expressions
{
    /// <summary>
    /// Expression Factory which generates XPath Function expressions
    /// </summary>
    public class XPathFunctionFactory 
        : ISparqlCustomExpressionFactory
    {
        /// <summary>
        /// Namespace Uri for XPath Functions Namespace
        /// </summary>
        public const String XPathFunctionsNamespace = "http://www.w3.org/2005/xpath-functions#";

        /// <summary>
        /// Constant representing the XPath boolean functions
        /// </summary>
        public const String Not = "not",
                            Boolean = "boolean";

        /// <summary>
        /// Constants representing the names of XPath String functions
        /// </summary>
        public const String Matches = "matches",
                            Contains = "contains",
                            StartsWith = "starts-with",
                            EndsWith = "ends-with",
                            StringLength = "string-length",
                            Concat = "concat",
                            Substring = "substring",
                            NormalizeSpace = "normalize-space",
                            NormalizeUnicode = "normalize-unicode",
                            UpperCase = "upper-case",
                            LowerCase = "lower-case",
                            EncodeForURI = "encode-for-uri",
                            SubstringBefore = "substring-before",
                            SubstringAfter = "substring-after",
                            Replace = "replace",
                            Translate = "translate",
                            Compare = "compare",
                            StringJoin = "string-join",
                            EscapeHtmlURI = "escape-html-uri";

        /// <summary>
        /// Constants representing the XPath Boolean constructor functions
        /// </summary>
        public const String True = "true",
                            False = "false";

        /// <summary>
        /// Constants representing the XPath Numeric Functions
        /// </summary>
        public const String Absolute = "abs",
                            Ceiling = "ceiling",
                            Floor = "floor",
                            Round = "round",
                            RoundHalfToEven = "round-half-to-even";

        /// <summary>
        /// Constants representing the XPath Date Time functions
        /// </summary>
        public const String YearFromDateTime = "year-from-dateTime",
                            MonthFromDateTime = "month-from-dateTime",
                            DayFromDateTime = "day-from-dateTime",
                            HoursFromDateTime = "hours-from-dateTime",
                            MinutesFromDateTime = "minutes-from-dateTime",
                            SecondsFromDateTime = "seconds-from-dateTime",
                            TimezoneFromDateTime = "timezone-from-dateTime",
                            AdjustDateTimeToTimezone = "adjust-dateTime-to-timezone";

        /// <summary>
        /// Constants representing the Normalization Form values supported by the XPath normalize-unicode() function
        /// </summary>
        public const String XPathUnicodeNormalizationFormC = "NFC",
                            XPathUnicodeNormalizationFormD = "NFD",
                            XPathUnicodeNormalizationFormKC = "NFKC",
                            XPathUnicodeNormalizationFormKD = "NFKD",
                            XPathUnicodeNormalizationFormFull = "FULLY-NORMALIZED";

        private String[] FunctionUris = {
                                            Not,
                                            Boolean,
                                            True,
                                            False,
                                            Matches,
                                            Contains,
                                            StartsWith,
                                            EndsWith,
                                            StringLength,
                                            Concat,
                                            Substring,
                                            SubstringAfter,
                                            SubstringBefore,
                                            NormalizeSpace,
                                            NormalizeUnicode,
                                            UpperCase,
                                            LowerCase,
                                            EncodeForURI,
                                            Replace,
                                            EscapeHtmlURI,
                                            Absolute,
                                            Ceiling,
                                            Floor,
                                            Round,
                                            RoundHalfToEven,
                                            YearFromDateTime,
                                            MonthFromDateTime,
                                            DayFromDateTime,
                                            HoursFromDateTime,
                                            MinutesFromDateTime,
                                            SecondsFromDateTime,
                                            TimezoneFromDateTime,
                                        };

        private String[] AggregateUris = {StringJoin};

        /// <summary>
        /// Argument Type Validator for validating that a Literal either has no datatype or is a String
        /// </summary>
        public static Func<Uri, bool> AcceptStringArguments = (u => u == null || u.AbsoluteUri.Equals(XmlSpecsHelper.XmlSchemaDataTypeString));
        /// <summary>
        /// Argument Type Validator for validating that a Literal has an Integer datatype
        /// </summary>
        public static Func<Uri, bool> AcceptIntegerArguments = (u => u != null && SparqlSpecsHelper.IntegerDataTypes.Contains(u.AbsoluteUri));
        /// <summary>
        /// Argument Type Validator for validating that a Literal has a Numeric datatype
        /// </summary>
        public static Func<Uri, bool> AcceptNumericArguments = (u => u != null && SparqlSpecsHelper.GetNumericTypeFromDataTypeUri(u) != SparqlNumericType.NaN);

        /// <summary>
        /// Tries to create an XPath Function expression if the function Uri correseponds to a supported XPath Function
        /// </summary>
        /// <param name="u">Function Uri</param>
        /// <param name="args">Function Arguments</param>
        /// <param name="scalarArgs">Scalar Arguments</param>
        /// <param name="expr">Generated Expression</param>
        /// <returns>Whether an expression was successfully generated</returns>
        public bool TryCreateExpression(Uri u, List<ISparqlExpression> args, Dictionary<String,ISparqlExpression> scalarArgs, out ISparqlExpression expr)
        {
            // If any Scalar Arguments are present then can't possibly be an XPath Function
            if (scalarArgs.Count > 0)
            {
                expr = null;
                return false;
            }

            String func = u.AbsoluteUri;
            if (func.StartsWith(XPathFunctionsNamespace))
            {
                func = func.Substring(XPathFunctionsNamespace.Length);
                ISparqlExpression xpathFunc = null;

                switch (func)
                {
                    case Absolute:
                        if (args.Count == 1)
                        {
                            xpathFunc = new AbsFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the XPath abs() function");
                        }
                        break;
                    case AdjustDateTimeToTimezone:
                        throw new NotSupportedException("XPath adjust-dateTime-to-timezone() function is not supported");
                    case Boolean:
                        if (args.Count == 1)
                        {
                            xpathFunc = new BooleanFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the XPath boolean() function");
                        }
                        throw new NotSupportedException("XPath boolean() function is not supported");
                    case Ceiling:
                        if (args.Count == 1)
                        {
                            xpathFunc = new CeilingFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the XPath ceiling() function");
                        }
                        break;
                    case Compare:
                        if (args.Count == 2)
                        {
                            xpathFunc = new CompareFunction(args.First(), args.Last());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the XPath compare() function");
                        }
                        break;
                    case Concat:
                        if (args.Count == 2)
                        {
                            xpathFunc = new ConcatFunction(args.First(), args.Last());
                        }
                        else if (args.Count > 2)
                        {
                            xpathFunc = new ConcatFunction(args);
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the XPath concat() function");
                        }
                        break;
                    case Contains:
                        if (args.Count == 2)
                        {
                            xpathFunc = new ContainsFunction(args.First(), args.Last());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the XPath contains() function");
                        }
                        break;
                    case DayFromDateTime:
                        if (args.Count == 1)
                        {
                            xpathFunc = new DayFromDateTimeFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the XPath day-from-dateTime() function");
                        }
                        break;
                    case EncodeForURI:
                        if (args.Count == 1)
                        {
                            xpathFunc = new EncodeForUriFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the XPath encode-for-uri() function");
                        }
                        break;
                    case EndsWith:
                        if (args.Count == 2)
                        {
                            xpathFunc = new EndsWithFunction(args.First(), args.Last());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the XPath ends-with() function");
                        }
                        break;
                    case EscapeHtmlURI:
                        if (args.Count == 1)
                        {
                            xpathFunc = new EscapeHtmlUriFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the XPath escape-html-uri() function");
                        }
                        break;
                    case False:
                        if (args.Count == 0)
                        {
                            xpathFunc = new ConstantTerm(new BooleanNode(null, false));
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the XPath false() function");
                        }
                        break;
                    case Floor:
                        if (args.Count == 1)
                        {
                            xpathFunc = new FloorFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the XPath floor() function");
                        }
                        break;
                    case HoursFromDateTime:
                        if (args.Count == 1)
                        {
                            xpathFunc = new HoursFromDateTimeFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the XPath hours-from-dateTime() function");
                        }
                        break;
                    case LowerCase:
                        if (args.Count == 1)
                        {
                            xpathFunc = new LowerCaseFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the XPath lower-case() function");
                        }
                        break;
                    case Matches:
                        if (args.Count == 2)
                        {
                            xpathFunc = new Functions.Sparql.Boolean.RegexFunction(args.First(), args.Last());
                        }
                        else if (args.Count == 3)
                        {
                            xpathFunc = new Functions.Sparql.Boolean.RegexFunction(args.First(), args[1], args.Last());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the XPath matches() function");
                        }
                        break;
                    case MinutesFromDateTime:
                        if (args.Count == 1)
                        {
                            xpathFunc = new MinutesFromDateTimeFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the XPath minutes-from-dateTime() function");
                        }
                        break;
                    case MonthFromDateTime:
                        if (args.Count == 1)
                        {
                            xpathFunc = new MonthFromDateTimeFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the XPath month-from-dateTime() function");
                        }
                        break;
                    case NormalizeSpace:
                        if (args.Count == 1)
                        {
                            xpathFunc = new NormalizeSpaceFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the XPath normalize-space() function");
                        }
                        break;
                    case NormalizeUnicode:
                        if (args.Count == 1)
                        {
                            xpathFunc = new NormalizeUnicodeFunction(args.First());
                        }
                        else if (args.Count == 2)
                        {
                            xpathFunc = new NormalizeUnicodeFunction(args.First(), args.Last());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the XPath normalize-space() function");
                        } 
                        break;
                    case Not:
                        if (args.Count == 1)
                        {
                            xpathFunc = new NotExpression(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the XPath not() function");
                        }
                        break;
                    case Replace:
                        if (args.Count == 3)
                        {
                            xpathFunc = new ReplaceFunction(args.First(), args[1], args.Last());
                        }
                        else if (args.Count == 4)
                        {
                            xpathFunc = new ReplaceFunction(args.First(), args[1], args[2], args.Last());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the XPath replace() function");
                        }
                        break;
                    case Round:
                        if (args.Count == 1)
                        {
                            xpathFunc = new RoundFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the XPath round() function");
                        }
                        break;
                    case RoundHalfToEven:
                        if (args.Count == 1)
                        {
                            xpathFunc = new RoundHalfToEvenFunction(args.First());
                        }
                        else if (args.Count == 2)
                        {
                            xpathFunc = new RoundHalfToEvenFunction(args.First(), args.Last());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the XPath round-half-to-even() function");
                        }
                        break;
                    case SecondsFromDateTime:
                        if (args.Count == 1)
                        {
                            xpathFunc = new SecondsFromDateTimeFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the XPath seconds-from-dateTime() function");
                        }
                        break;
                    case StartsWith:
                        if (args.Count == 2)
                        {
                            xpathFunc = new StartsWithFunction(args.First(), args.Last());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the XPath starts-with() function");
                        }
                        break;
                    case StringJoin:
                        if (args.Count == 1)
                        {
                            xpathFunc = new AggregateTerm(new StringJoinAggregate(args.First()));
                        }
                        else if (args.Count == 2)
                        {
                            xpathFunc = new AggregateTerm(new StringJoinAggregate(args.First(), args.Last()));
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the XPath string-join() function");
                        }
                        break;
                    case StringLength:
                        if (args.Count == 1)
                        {
                            xpathFunc = new StringLengthFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the XPath string-length() function");
                        }
                        break;
                    case Substring:
                        if (args.Count == 2)
                        {
                            xpathFunc = new SubstringFunction(args.First(), args.Last());
                        }
                        else if (args.Count == 3)
                        {
                            xpathFunc = new SubstringFunction(args.First(), args[1], args.Last());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the XPath substring() function");
                        }
                        break;
                    case SubstringAfter:
                        if (args.Count == 2)
                        {
                            xpathFunc = new SubstringAfterFunction(args.First(), args.Last());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the XPath substring-after() function");
                        }
                        break;
                    case SubstringBefore:
                        if (args.Count == 2)
                        {
                            xpathFunc = new SubstringBeforeFunction(args.First(), args.Last());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the XPath substring-before() function");
                        }
                        break;
                    case TimezoneFromDateTime:
                        if (args.Count == 1)
                        {
                            xpathFunc = new TimezoneFromDateTimeFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the XPath timezone-from-dateTime() function");
                        }
                        break;
                    case Translate:
                        throw new NotSupportedException("XPath translate() function is not supported");
                    case True:
                        if (args.Count == 0)
                        {
                            xpathFunc = new ConstantTerm(new BooleanNode(null, true));
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the XPath true() function");
                        }
                        break;
                    case UpperCase:
                        if (args.Count == 1)
                        {
                            xpathFunc = new UpperCaseFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the XPath upper-case() function");
                        }
                        break;
                    case YearFromDateTime:
                        if (args.Count == 1)
                        {
                            xpathFunc = new YearFromDateTimeFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the XPath year-from-dateTime() function");
                        }
                        break;
                }

                if (xpathFunc != null)
                {
                    expr = xpathFunc;
                    return true;
                }
            }
            expr = null;
            return false;        
        }

        /// <summary>
        /// Gets the Extension Function URIs supported by this Factory
        /// </summary>
        public IEnumerable<Uri> AvailableExtensionFunctions
        {
            get
            {
                return (from u in FunctionUris
                        select UriFactory.Create(XPathFunctionsNamespace + u));
            }
        }

        /// <summary>
        /// Gets the Extension Aggregate URIs supported by this Factory
        /// </summary>
        public IEnumerable<Uri> AvailableExtensionAggregates
        {
            get
            {
                return (from u in AggregateUris
                        select UriFactory.Create(XPathFunctionsNamespace + u));
            }
        }
    }
}
