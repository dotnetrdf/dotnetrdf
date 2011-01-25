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

If this license is not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Aggregates;
using VDS.RDF.Query.Expressions.Functions;

namespace VDS.RDF.Query.Expressions
{
    /// <summary>
    /// Expression Factory which generates XPath Function expressions
    /// </summary>
    public class XPathFunctionFactory : ISparqlCustomExpressionFactory
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

        private String[] AggregateUris = {
                                             StringJoin
                                         };

        /// <summary>
        /// Argument Type Validator for validating that a Literal either has no datatype or is a String
        /// </summary>
        public static Func<Uri, bool> AcceptStringArguments = (u => u == null || u.ToString().Equals(XmlSpecsHelper.XmlSchemaDataTypeString));
        /// <summary>
        /// Argument Type Validator for validating that a Literal has an Integer datatype
        /// </summary>
        public static Func<Uri, bool> AcceptIntegerArguments = (u => u != null && SparqlSpecsHelper.IntegerDataTypes.Contains(u.ToString()));
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
            //If any Scalar Arguments are present then can't possibly be an XPath Function
            if (scalarArgs.Count > 0)
            {
                expr = null;
                return false;
            }

            String func = u.ToString();
            if (func.StartsWith(XPathFunctionFactory.XPathFunctionsNamespace))
            {
                func = func.Substring(XPathFunctionFactory.XPathFunctionsNamespace.Length);
                ISparqlExpression xpathFunc = null;

                switch (func)
                {
                    case XPathFunctionFactory.Absolute:
                        if (args.Count == 1)
                        {
                            xpathFunc = new XPathAbsoluteFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the XPath abs() function");
                        }
                        break;
                    case XPathFunctionFactory.AdjustDateTimeToTimezone:
                        throw new NotSupportedException("XPath adjust-dateTime-to-timezone() function is not supported");
                    case XPathFunctionFactory.Boolean:
                        if (args.Count == 1)
                        {
                            xpathFunc = new XPathBooleanFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the XPath boolean() function");
                        }
                        throw new NotSupportedException("XPath boolean() function is not supported");
                    case XPathFunctionFactory.Ceiling:
                        if (args.Count == 1)
                        {
                            xpathFunc = new XPathCeilingFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the XPath ceiling() function");
                        }
                        break;
                    case XPathFunctionFactory.Compare:
                        if (args.Count == 2)
                        {
                            xpathFunc = new XPathCompareFunction(args.First(), args.Last());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the XPath compare() function");
                        }
                        break;
                    case XPathFunctionFactory.Concat:
                        if (args.Count == 2)
                        {
                            xpathFunc = new XPathConcatFunction(args.First(), args.Last());
                        }
                        else if (args.Count > 2)
                        {
                            xpathFunc = new XPathConcatFunction(args);
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the XPath concat() function");
                        }
                        break;
                    case XPathFunctionFactory.Contains:
                        if (args.Count == 2)
                        {
                            xpathFunc = new XPathContainsFunction(args.First(), args.Last());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the XPath contains() function");
                        }
                        break;
                    case XPathFunctionFactory.DayFromDateTime:
                        if (args.Count == 1)
                        {
                            xpathFunc = new XPathDayFromDateTimeFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the XPath day-from-dateTime() function");
                        }
                        break;
                    case XPathFunctionFactory.EncodeForURI:
                        if (args.Count == 1)
                        {
                            xpathFunc = new XPathEncodeForUriFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the XPath encode-for-uri() function");
                        }
                        break;
                    case XPathFunctionFactory.EndsWith:
                        if (args.Count == 2)
                        {
                            xpathFunc = new XPathEndsWithFunction(args.First(), args.Last());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the XPath ends-with() function");
                        }
                        break;
#if !NO_WEB
                    case XPathFunctionFactory.EscapeHtmlURI:
                        if (args.Count == 1)
                        {
                            xpathFunc = new XPathEscapeHtmlUriFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the XPath escape-html-uri() function");
                        }
                        break;
#endif
                    case XPathFunctionFactory.False:
                        if (args.Count == 0)
                        {
                            xpathFunc = new BooleanExpressionTerm(false);
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the XPath false() function");
                        }
                        break;
                    case XPathFunctionFactory.Floor:
                        if (args.Count == 1)
                        {
                            xpathFunc = new XPathFloorFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the XPath floor() function");
                        }
                        break;
                    case XPathFunctionFactory.HoursFromDateTime:
                        if (args.Count == 1)
                        {
                            xpathFunc = new XPathHoursFromDateTimeFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the XPath hours-from-dateTime() function");
                        }
                        break;
                    case XPathFunctionFactory.LowerCase:
                        if (args.Count == 1)
                        {
                            xpathFunc = new XPathLowerCaseFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the XPath lower-case() function");
                        }
                        break;
                    case XPathFunctionFactory.Matches:
                        if (args.Count == 2)
                        {
                            xpathFunc = new RegexFunction(args.First(), args.Last());
                        }
                        else if (args.Count == 3)
                        {
                            xpathFunc = new RegexFunction(args.First(), args[1], args.Last());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the XPath matches() function");
                        }
                        break;
                    case XPathFunctionFactory.MinutesFromDateTime:
                        if (args.Count == 1)
                        {
                            xpathFunc = new XPathMinutesFromDateTimeFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the XPath minutes-from-dateTime() function");
                        }
                        break;
                    case XPathFunctionFactory.MonthFromDateTime:
                        if (args.Count == 1)
                        {
                            xpathFunc = new XPathMonthFromDateTimeFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the XPath month-from-dateTime() function");
                        }
                        break;
                    case XPathFunctionFactory.NormalizeSpace:
                        if (args.Count == 1)
                        {
                            xpathFunc = new XPathNormalizeSpaceFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the XPath normalize-space() function");
                        }
                        break;
#if !NO_NORM
                    case XPathFunctionFactory.NormalizeUnicode:
                        if (args.Count == 1)
                        {
                            xpathFunc = new XPathNormalizeUnicodeFunction(args.First());
                        }
                        else if (args.Count == 2)
                        {
                            xpathFunc = new XPathNormalizeUnicodeFunction(args.First(), args.Last());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the XPath normalize-space() function");
                        } 
                        break;
#endif
                    case XPathFunctionFactory.Not:
                        if (args.Count == 1)
                        {
                            xpathFunc = new NegationExpression(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the XPath not() function");
                        }
                        break;
                    case XPathFunctionFactory.Replace:
                        if (args.Count == 3)
                        {
                            xpathFunc = new XPathReplaceFunction(args.First(), args[1], args.Last());
                        }
                        else if (args.Count == 4)
                        {
                            xpathFunc = new XPathReplaceFunction(args.First(), args[1], args[2], args.Last());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the XPath replace() function");
                        }
                        break;
                    case XPathFunctionFactory.Round:
                        if (args.Count == 1)
                        {
                            xpathFunc = new XPathRoundFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the XPath round() function");
                        }
                        break;
#if !SILVERLIGHT
                    case XPathFunctionFactory.RoundHalfToEven:
                        if (args.Count == 1)
                        {
                            xpathFunc = new XPathRoundHalfToEvenFunction(args.First());
                        }
                        else if (args.Count == 2)
                        {
                            xpathFunc = new XPathRoundHalfToEvenFunction(args.First(), args.Last());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the XPath round-half-to-even() function");
                        }
                        break;
#endif
                    case XPathFunctionFactory.SecondsFromDateTime:
                        if (args.Count == 1)
                        {
                            xpathFunc = new XPathSecondsFromDateTimeFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the XPath seconds-from-dateTime() function");
                        }
                        break;
                    case XPathFunctionFactory.StartsWith:
                        if (args.Count == 2)
                        {
                            xpathFunc = new XPathStartsWithFunction(args.First(), args.Last());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the XPath starts-with() function");
                        }
                        break;
                    case XPathFunctionFactory.StringJoin:
                        if (args.Count == 1)
                        {
                            xpathFunc = new NonNumericAggregateExpressionTerm(new XPathStringJoinFunction(args.First()));
                        }
                        else if (args.Count == 2)
                        {
                            xpathFunc = new NonNumericAggregateExpressionTerm(new XPathStringJoinFunction(args.First(), args.Last()));
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the XPath string-join() function");
                        }
                        break;
                    case XPathFunctionFactory.StringLength:
                        if (args.Count == 1)
                        {
                            xpathFunc = new XPathStringLengthFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the XPath string-length() function");
                        }
                        break;
                    case XPathFunctionFactory.Substring:
                        if (args.Count == 2)
                        {
                            xpathFunc = new XPathSubstringFunction(args.First(), args.Last());
                        }
                        else if (args.Count == 3)
                        {
                            xpathFunc = new XPathSubstringFunction(args.First(), args[1], args.Last());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the XPath substring() function");
                        }
                        break;
                    case XPathFunctionFactory.SubstringAfter:
                        if (args.Count == 2)
                        {
                            xpathFunc = new XPathSubstringAfterFunction(args.First(), args.Last());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the XPath substring-after() function");
                        }
                        break;
                    case XPathFunctionFactory.SubstringBefore:
                        if (args.Count == 2)
                        {
                            xpathFunc = new XPathSubstringBeforeFunction(args.First(), args.Last());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the XPath substring-before() function");
                        }
                        break;
                    case XPathFunctionFactory.TimezoneFromDateTime:
                        if (args.Count == 1)
                        {
                            xpathFunc = new XPathTimezoneFromDateTimeFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the XPath timezone-from-dateTime() function");
                        }
                        break;
                    case XPathFunctionFactory.Translate:
                        throw new NotSupportedException("XPath translate() function is not supported");
                    case XPathFunctionFactory.True:
                        if (args.Count == 0)
                        {
                            xpathFunc = new BooleanExpressionTerm(true);
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the XPath true() function");
                        }
                        break;
                    case XPathFunctionFactory.UpperCase:
                        if (args.Count == 1)
                        {
                            xpathFunc = new XPathUpperCaseFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the XPath upper-case() function");
                        }
                        break;
                    case XPathFunctionFactory.YearFromDateTime:
                        if (args.Count == 1)
                        {
                            xpathFunc = new XPathYearFromDateTimeFunction(args.First());
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
                        select new Uri(XPathFunctionsNamespace + u));
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
                        select new Uri(XPathFunctionsNamespace + u));
            }
        }
    }
}
