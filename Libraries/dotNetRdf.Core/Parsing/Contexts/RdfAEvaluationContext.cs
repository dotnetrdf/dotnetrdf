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
using System.Collections.Generic;

namespace VDS.RDF.Parsing.Contexts;

/// <summary>
/// Evaluation Context for RDFa Parsers.
/// </summary>
public class RdfAEvaluationContext
{
    /// <summary>
    /// Creates a new RDFa Evaluation Context.
    /// </summary>
    /// <param name="baseUri">Base URI.</param>
    public RdfAEvaluationContext(Uri baseUri)
    {
        BaseUri = baseUri;
        NamespaceMap = new NamespaceMapper(true);
        NamespaceMap.AddNamespace(string.Empty, UriFactory.Root.Create(RdfAParser.XHtmlVocabNamespace));
    }

    /// <summary>
    /// Creates a new RDFa Evaluation Context.
    /// </summary>
    /// <param name="baseUri">Base URI.</param>
    /// <param name="nsmap">Namespace Map.</param>
    public RdfAEvaluationContext(Uri baseUri, NamespaceMapper nsmap)
    {
        BaseUri = baseUri;
        NamespaceMap = nsmap;
    }

    /// <summary>
    /// Gets/Sets the Base URI.
    /// </summary>
    public Uri BaseUri
    {
        get; set;
    }

    /// <summary>
    /// Gets/Sets the Parent Subject.
    /// </summary>
    public INode ParentSubject { get; set; }

    /// <summary>
    /// Gets/Sets the Parent Object.
    /// </summary>
    public INode ParentObject { get; set; }

    /// <summary>
    /// Gets the Namespace Map.
    /// </summary>
    public NamespaceMapper NamespaceMap { get; }

    /// <summary>
    /// Gets/Sets the Language.
    /// </summary>
    public string Language { get; set; } = string.Empty;

    /// <summary>
    /// Gets the list of incomplete Triples.
    /// </summary>
    public List<IncompleteTriple> IncompleteTriples { get; } = new List<IncompleteTriple>();

    /// <summary>
    /// Gets/Sets the Local Vocabulary.
    /// </summary>
    public IRdfAContext LocalContext { get; set; }

    /// <summary>
    /// Get the list mapping.
    /// </summary>
    public Dictionary<INode, List<INode>> ListMapping { get; set; } = new();
}
