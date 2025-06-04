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

namespace VDS.RDF.Query.FullText.Search;

/// <summary>
/// Interface for classes that provide full text search capability.
/// </summary>
/// <remarks>
/// The <strong>Match()</strong> methods may allow for provider specific query syntaxes depending on the the underlying provider.
/// </remarks>
public interface IFullTextSearchProvider
    : IDisposable
{
    /// <summary>
    /// Searches for matches for specific text.
    /// </summary>
    /// <param name="text">Search Query.</param>
    /// <param name="scoreThreshold">Score Threshold.</param>
    /// <param name="limit">Result Limit.</param>
    /// <returns></returns>
    IEnumerable<IFullTextSearchResult> Match(String text, double scoreThreshold, int limit);

    /// <summary>
    /// Searches for matches for specific text.
    /// </summary>
    /// <param name="text">Search Query.</param>
    /// <param name="scoreThreshold">Score Threshold.</param>
    IEnumerable<IFullTextSearchResult> Match(String text, double scoreThreshold);

    /// <summary>
    /// Searches for matches for specific text.
    /// </summary>
    /// <param name="text">Search Query.</param>
    /// <param name="limit">Result Limit.</param>
    IEnumerable<IFullTextSearchResult> Match(String text, int limit);

    /// <summary>
    /// Searches for matches for specific text.
    /// </summary>
    /// <param name="text">Search Query.</param>
    IEnumerable<IFullTextSearchResult> Match(String text);

    /// <summary>
    /// Searches for matches for specific text.
    /// </summary>
    /// <param name="graphUris">Graph URIs.</param>
    /// <param name="text">Search Query.</param>
    /// <param name="scoreThreshold">Score Threshold.</param>
    /// <param name="limit">Result Limit.</param>
    /// <returns></returns>
    [Obsolete("Replaced by Match(IEnumerable<IRefNode>, string, double, int)")]
    IEnumerable<IFullTextSearchResult> Match(IEnumerable<Uri> graphUris, String text, double scoreThreshold, int limit);

    /// <summary>
    /// Searches for matches for specific text.
    /// </summary>
    /// <param name="graphUris">Graph URIs.</param>
    /// <param name="text">Search Query.</param>
    /// <param name="scoreThreshold">Score Threshold.</param>
    [Obsolete("Replaced by Match(IEnumerable<IRefNode>, string, double)")]
    IEnumerable<IFullTextSearchResult> Match(IEnumerable<Uri> graphUris, String text, double scoreThreshold);

    /// <summary>
    /// Searches for matches for specific text.
    /// </summary>
    /// <param name="graphUris">Graph URIs.</param>
    /// <param name="text">Search Query.</param>
    /// <param name="limit">Result Limit.</param>
    [Obsolete("Replaced by Match(IEnumerable<IRefNode>, string, int)")]
    IEnumerable<IFullTextSearchResult> Match(IEnumerable<Uri> graphUris, String text, int limit);

    /// <summary>
    /// Searches for matches for specific text.
    /// </summary>
    /// <param name="graphUris">Graph URIs.</param>
    /// <param name="text">Search Query.</param>
    [Obsolete("Replaced by Match(IEnumerable<IRefNode>, string)")]
    IEnumerable<IFullTextSearchResult> Match(IEnumerable<Uri> graphUris, String text);

    /// <summary>
    /// Searches for matches for specific text.
    /// </summary>
    /// <param name="graphUris">Graph URIs.</param>
    /// <param name="text">Search Query.</param>
    /// <param name="scoreThreshold">Score Threshold.</param>
    /// <param name="limit">Result Limit.</param>
    /// <returns></returns>
    IEnumerable<IFullTextSearchResult> Match(IEnumerable<IRefNode> graphUris, String text, double scoreThreshold, int limit);

    /// <summary>
    /// Searches for matches for specific text.
    /// </summary>
    /// <param name="graphUris">Graph URIs.</param>
    /// <param name="text">Search Query.</param>
    /// <param name="scoreThreshold">Score Threshold.</param>
    IEnumerable<IFullTextSearchResult> Match(IEnumerable<IRefNode> graphUris, String text, double scoreThreshold);

    /// <summary>
    /// Searches for matches for specific text.
    /// </summary>
    /// <param name="graphUris">Graph URIs.</param>
    /// <param name="text">Search Query.</param>
    /// <param name="limit">Result Limit.</param>
    IEnumerable<IFullTextSearchResult> Match(IEnumerable<IRefNode> graphUris, String text, int limit);

    /// <summary>
    /// Searches for matches for specific text.
    /// </summary>
    /// <param name="graphUris">Graph URIs.</param>
    /// <param name="text">Search Query.</param>
    IEnumerable<IFullTextSearchResult> Match(IEnumerable<IRefNode> graphUris, String text);

    /// <summary>
    /// Gets whether the search provider is automatically synced with the index i.e. whether queries will always return results based on the latest state of the index.
    /// </summary>
    /// <remarks>
    /// Some implementations may allow this behaviour to be configured while for others this feature may always be on/off.
    /// </remarks>
    bool IsAutoSynced
    {
        get;
    }
}
