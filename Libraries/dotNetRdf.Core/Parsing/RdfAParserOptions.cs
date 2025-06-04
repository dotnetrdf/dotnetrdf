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

namespace VDS.RDF.Parsing;

/// <summary>
/// A collection of options for configuring the RDFa Parser.
/// </summary>
public class RdfAParserOptions
{
    /// <summary>
    /// The base IRI to use when processing the document.
    /// </summary>
    /// <remarks>If set, this overrides the input document's IRI, but does not override any @base attribute in the document itself.</remarks>
    public Uri Base { get; set; }

    /// <summary>
    /// The RDFa syntax to process.
    /// </summary>
    public RdfASyntax Syntax { get; set; }

    /// <summary>
    /// Get/set the default RDFa context to use for the resolution of terms and prefixes.
    /// </summary>
    public RdfAContext DefaultContext { get; set; } = StaticRdfAContexts.XhtmlRdfAContext;

    /// <summary>
    /// Get/set the flag that indicates if the parser should perform RDFa property copying.
    /// </summary>
    public bool PropertyCopyEnabled { get; set; } = true;
}
