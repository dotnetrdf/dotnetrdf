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

namespace VDS.RDF.Parsing;

/// <summary>
/// Interface for RDFa Vocabularies.
/// </summary>
public interface IRdfAContext
{
    /// <summary>
    /// Gets whether a Vocabulary contains a Term.
    /// </summary>
    /// <param name="term">Term.</param>
    /// <returns></returns>
    bool HasTerm(string term);

    /// <summary>
    /// Resolves a Term in the Vocabulary.
    /// </summary>
    /// <param name="term">Term.</param>
    /// <returns></returns>
    string ResolveTerm(string term);

    /// <summary>
    /// Adds a Term to the Vocabulary.
    /// </summary>
    /// <param name="term">Term.</param>
    /// <param name="uri">URI.</param>
    void AddTerm(string term, string uri);

    /// <summary>
    /// Adds a Namespace to the Vocabulary.
    /// </summary>
    /// <param name="prefix">Prefix.</param>
    /// <param name="nsUri">Namespace URI.</param>
    void AddNamespace(string prefix, string nsUri);

    /// <summary>
    /// Merges another Vocabulary into this one.
    /// </summary>
    /// <param name="vocab">Vocabulary.</param>
    void Merge(IRdfAContext vocab);

    /// <summary>
    /// Gets/Sets the Default Vocabulary URI.
    /// </summary>
    /// <remarks>May be NULL if the context does not define a default vocabulary URI.</remarks>
    string VocabularyUri
    {
        get;
        set;
    }

    /// <summary>
    /// Gets the Term Mappings.
    /// </summary>
    IEnumerable<KeyValuePair<string, string>> Mappings
    {
        get;
    }

    /// <summary>
    /// Gets the Namespace Mappings.
    /// </summary>
    [Obsolete("Use the NamespaceMapper property to access the namespace map")]
    IEnumerable<KeyValuePair<string, string>> Namespaces
    {
        get;
    }

    /// <summary>
    /// Gets the namespace mappings.
    /// </summary>
    INamespaceMapper NamespaceMap { get; }

    /// <summary>
    /// Resolve a CURIE using the namespaces defined in this vocabulary.
    /// </summary>
    /// <param name="curie">The CURIE string to resolve.</param>
    /// <param name="baseUri"></param>
    /// <returns></returns>
    string ResolveCurie(string curie, Uri baseUri);

}
