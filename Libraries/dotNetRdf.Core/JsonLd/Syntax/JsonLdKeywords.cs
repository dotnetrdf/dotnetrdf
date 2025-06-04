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

namespace VDS.RDF.JsonLd.Syntax;

/// <summary>
/// Defines the keywords and categories of keywords that are valid in JSON-LD 1.1.
/// </summary>
public static class JsonLdKeywords
{
    /// <summary>
    /// The list of JSON-LD keywords defined by the API and Processing specification.
    /// </summary>
    public static readonly string[] CoreKeywords =
    {
        "@base",
        "@container",
        "@context",
        "@direction",
        "@graph",
        "@id",
        "@import",
        "@included",
        "@index",
        "@json",
        "@language",
        "@list",
        "@nest",
        "@none",
        "@prefix",
        "@propagate",
        "@protected",
        "@reverse",
        "@value",
        "@set",
        "@type",
        "@value",
        "@version",
        "@vocab",
    };

    /// <summary>
    /// The list of JSON-LD keywords added by the Framing specification.
    /// </summary>
    public static readonly string[] FramingKeywords =
    {
        "@default",
        "@embed",
        "@explicit",
        "@omitDefault",
        "@requireAll",
    };

    /// <summary>
    /// Keywords that are valid in a term definition.
    /// </summary>
    public static readonly string[] TermDefinitionKeys =
    {
        "@id",
        "@reverse",
        "@container",
        "@context",
        "@direction",
        "@index",
        "@language",
        "@nest",
        "@prefix",
        "@protected",
        "@type",
    };

    /// <summary>
    /// Keywords that are valid properties of a value object.
    /// </summary>
    public static readonly string[] ValueObjectKeys =
    {
        "@direction",
        "@value",
        "@language",
        "@type",
        "@index",
    };

    /// <summary>
    /// Keywords that are valid properties of a graph object.
    /// </summary>
    public static readonly string[] GraphObjectKeys =
    {
        "@graph",
        "@id",
        "@index",
    };

    /// <summary>
    /// Keywords that are valid top-level properties of a context object.
    /// </summary>
    public static readonly string[] JsonLdContextKeywords =
    {
        "@base",
        "@direction",
        "@import",
        "@language",
        "@propagate",
        "@protected",
        "@version",
        "@vocab",
    };
}
