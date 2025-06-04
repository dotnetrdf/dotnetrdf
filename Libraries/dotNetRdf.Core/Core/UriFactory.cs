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

namespace VDS.RDF;

/// <summary>
/// A static helper class for interning URIs to reduce memory usage.
/// </summary>
public static class UriFactory
{
    /// <summary>
    /// Get or set the root URI factory for this app domain.
    /// </summary>
    /// <remarks>Internally, the root URI factory is used as the default factory when no other factory is specified in constructors/method parameters.</remarks>
    public static IUriFactory Root = new CachingUriFactory();

    /// <summary>
    /// Get / set the flag that controls the caching of Uri instances constructed by this factory.
    /// </summary>
    /// <remarks>When <see cref="InternUris"/> is set to true, the factory will cache each constructed URI against the original string value used for construction and return a cached Uri where available in preference to calling the Uri constructor.</remarks>
    public static bool InternUris
    {
        get => Root.InternUris;
        set => Root.InternUris = value;
    }

    /// <summary>
    /// Creates a URI interning it if interning is enabled via the <see cref="InternUris"/> property.
    /// </summary>
    /// <param name="uri">String URI.</param>
    /// <returns></returns>
    /// <remarks>
    /// When URI interning is disabled this is equivalent to just invoking the constructor of the <see cref="Uri">Uri</see> class.
    /// </remarks>
    public static Uri Create(string uri)
    {
        return Root.Create(uri);
    }

    /// <summary>
    /// Clears all interned URIs.
    /// </summary>
    public static void Clear()
    {
        Root.Clear();
    }
}
