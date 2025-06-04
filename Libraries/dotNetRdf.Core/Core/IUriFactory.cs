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

// unset

using System;

namespace VDS.RDF;

/// <summary>
/// Defines the interface for a factory class that creates URIs and optionally interns them to reduce memory usage from repeated creation of URIs with the same string value.
/// </summary>
public interface IUriFactory
{
    /// <summary>
    /// Create a <see cref="Uri"/> instance, interning it if <see cref="InternUris"/> is set to true.
    /// </summary>
    /// <param name="uri">String URI.</param>
    /// <returns></returns>
    /// <remarks>
    /// When URI interning is disabled this is equivalent to just invoking the constructor of the <see cref="Uri">Uri</see> class.
    /// </remarks>
    public Uri Create(string uri);

    /// <summary>
    /// Create a <see cref="Uri"/> instance, interning it if <see cref="InternUris"/> is set to true.
    /// </summary>
    /// <param name="baseUri">The base URI to resolve <paramref name="relativeUri"/> against.</param>
    /// <param name="relativeUri">String URI.</param>
    /// <returns></returns>
    /// <remarks>
    /// When URI interning is disabled this is equivalent to just invoking the constructor of the <see cref="Uri">Uri</see> class.
    /// </remarks>
    public Uri Create(Uri baseUri, string relativeUri);

    /// <summary>
    /// Controls whether URI instances are interned by this instance.
    /// </summary>
    /// <remarks>When <see cref="InternUris"/> is set to true, the factory will cache each constructed URI against the original string value used for construction and return a cached Uri where available in preference to calling the Uri constructor.</remarks>
    public bool InternUris { get; set; }

    /// <summary>
    /// Return the interned URI instance if available.
    /// </summary>
    /// <param name="uri">The string URI to return an interned URI instance for.</param>
    /// <param name="value">Receives the interned <see cref="Uri"/> instance if it is available, null otherwise.</param>
    /// <returns>True if an interned <see cref="Uri"/> instance was found in this factory or its parent, false otherwise.</returns>
    public bool TryGetUri(string uri, out Uri value);

    /// <summary>
    /// Clears all interned URIs.
    /// </summary>
    public void Clear();
}