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

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace VDS.RDF.Parsing;

/// <summary>
/// Provides methods for working with BCP 47 language tags.
/// </summary>
public class LanguageTag
{
    /// <summary>
    /// Tags that do not match the general langtag production and which would not otherwise be considered well-formed,
    /// but which are valid although deprecated in most cases.
    /// </summary>
    public static readonly string[] IrregularGrandfatheredTags = new[]
    {
        "en-GB-oed",
        "i-ami",
        "i-bnn",
        "i-default",
        "i-enochian",
        "i-hak",
        "i-klingon",
        "i-lux",
        "i-mingo",
        "i-navajo",
        "i-pwn",
        "i-tao",
        "i-tay",
        "i-tsu",
        "sgn-BE-FR",
        "sgn-BE-NL",
        "sgn-CH-DE",
    };

    /// <summary>
    /// Tags that do match the general langtag production but their subtags are not semantically valid. These are
    /// also considered valid tags although all are deprecated.
    /// </summary>
    public static readonly string[] RegularGrandfatheredTags = new[]
    {

        "art-lojban",
        "cel-gaulish",
        "no-bok",
        "no-nyn",
        "zh-guoyu",
        "zh-hakka",
        "zh-min",
        "zh-min-nan",
        "zh-xiang",
    };

    private static readonly Dictionary<string, bool> GrandfatheredTagsLookup =
        IrregularGrandfatheredTags.Concat(RegularGrandfatheredTags)
            .ToDictionary(k => k.ToLowerInvariant(), v => true);
    
    private static readonly Regex TurtleTagRegex = new("^[a-zA-Z]+(-[a-zA-Z0-9]+)*$");
    private static readonly Regex WellFormedTagRegex = new(
        "^(?<language>([a-zA-Z]{2,3}(-(?<extlang>[a-zA-Z]{3}(-[a-zA-Z]{3}){0,2}))?)|([a-zA-Z]{4,8}))" +
        "(-(?<script>[a-zA-Z]{4}))?" +
        "(-(?<region>[a-zA-Z]{2}|[0-9]{3}))?" +
        "(-(?<variant>([a-zA-Z]{5,8})|([0-9][0-9a-zA-Z]{3})))?" +
        "(-(?<extension>[0-9A-WY-Za-wy-z]-[0-9A-Za-z]{2,8}))?" +
        "(-(?<privateuse>x-[0-9A-Za-z]{1,8}))?$");

    /// <summary>
    /// Determine if a string represents a well-formed language tag.
    /// </summary>
    /// <param name="languageTag">The string to check.</param>
    /// <returns>True if <paramref name="languageTag"/> is a well-formed language or a grandfathered tag that is considered valid.</returns>
    public static bool IsWellFormed(string languageTag)
    {
        return GrandfatheredTagsLookup.ContainsKey(languageTag.ToLowerInvariant()) ||
               WellFormedTagRegex.IsMatch(languageTag);
    }

    /// <summary>
    /// Determine if a string is valid against the LANGTAG production in the Turtle 1.1 specification.
    /// </summary>
    /// <param name="languageTag">The string to check.</param>
    /// <returns>True if <paramref name="languageTag"/> matches the Turtle 1.1 definition of /^[a-zA-Z]+(-[a-zA-Z0-9]+)*$/ .</returns>
    public static bool IsValidTurtle(string languageTag)
    {
        return TurtleTagRegex.IsMatch(languageTag);
    }
}
