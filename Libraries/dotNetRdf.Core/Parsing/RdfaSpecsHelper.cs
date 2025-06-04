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

namespace VDS.RDF.Parsing;

/// <summary>
/// Provides constants for use in processing RDFa documents.
/// </summary>
public static class RdfASpecsHelper
{
    /// <summary>
    /// Namespace URI for RDFa.
    /// </summary>
    public const string RdfANamespace = "http://www.w3.org/ns/rdfa#";

    /// <summary>
    /// URI for the RDFa Pattern resource type.
    /// </summary>
    public const string RdfAPattern = RdfANamespace + "Pattern";
    
    /// <summary>
    /// URI for the RDFa copy property type.
    /// </summary>
    public const string RdfACopy = RdfANamespace + "copy";

    /// <summary>
    /// URI for the RDFa prefix property type.
    /// </summary>
    public const string RdfAPrefix = RdfANamespace + "prefix";

    /// <summary>
    /// URI for the RDFa uri property type.
    /// </summary>
    public const string RdfAUri = RdfANamespace + "uri";

    /// <summary>
    /// URI for the RDFa term property type.
    /// </summary>
    public const string RdfATerm = RdfANamespace + "term";
    
    /// <summary>
    /// URI for the RDFa vocabulary property type.
    /// </summary>
    public const string RdfAVocabulary = RdfANamespace + "vocabulary";
}
