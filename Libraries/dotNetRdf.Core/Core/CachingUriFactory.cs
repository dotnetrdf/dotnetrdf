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
using VDS.Common.Tries;

namespace VDS.RDF;

/// <summary>
/// The default implementation of <see cref="IUriFactory"/> which caches the URI instances it creates when <see cref="InternUris"/> is set to true.
/// </summary>
public class CachingUriFactory : IUriFactory
{
    private readonly ITrie<string, char, Uri> _uris = new SparseStringTrie<Uri>();
    private readonly IUriFactory _parent;

    /// <summary>
    /// Creates a new factory instance as a child of the root UriFactory as specified by <see cref="UriFactory.Root"/>.
    /// </summary>
    public CachingUriFactory() : this(UriFactory.Root) { }

    /// <summary>
    /// Creates a new factory instances as a child of the specified parent factory.
    /// </summary>
    /// <param name="parent">The parent factory instance. May be null.</param>
    public CachingUriFactory(IUriFactory parent)
    {
        _parent = parent;
    }

    /// <summary>
    /// Get / set the flag that controls the caching of Uri instances constructed by this factory.
    /// </summary>
    /// <remarks>When <see cref="InternUris"/> is set to true, the factory will cache each constructed URI against the original string value used for construction and return a cached Uri where available in preference to calling the Uri constructor.</remarks>
    public bool InternUris { get; set; } = true;

    /// <summary>
    /// Creates a URI interning it if interning is enabled via the <see cref="InternUris"/> property.
    /// </summary>
    /// <param name="uri">String URI.</param>
    /// <returns></returns>
    /// <remarks>
    /// When URI interning is disabled this is equivalent to just invoking the constructor of the <see cref="Uri">Uri</see> class.
    /// </remarks>
    public Uri Create(string uri)
    {
        if (!InternUris)
        {
            return new Uri(uri);
        }

        // First see if the value is in our cache
        ITrieNode<char, Uri> node = _uris.MoveToNode(uri);
        if (node.HasValue)
        {
            return node.Value;
        }

        // If we have a parent factory see if that has the value cached.
        if (_parent != null && _parent.TryGetUri(uri, out Uri value))
        {
            return value;
        }

        // IF the value is not found in our cache or our parent's then add it to our cache
        var u = new Uri(uri);
        node.Value = u;
        return node.Value;
    }

    /// <summary>
    /// Create a <see cref="Uri"/> instance, interning it if <see cref="IUriFactory.InternUris"/> is set to true.
    /// </summary>
    /// <param name="baseUri">The base URI to resolve <paramref name="relativeUri"/> against.</param>
    /// <param name="relativeUri">String URI.</param>
    /// <returns></returns>
    /// <remarks>
    /// When URI interning is disabled this is equivalent to just invoking the constructor of the <see cref="Uri">Uri</see> class.
    /// </remarks>
    public Uri Create(Uri baseUri, string relativeUri)
    {
        if (baseUri == null) return Create(relativeUri);
        if (!InternUris)
        {
            return new Uri(baseUri, relativeUri);
        }
        // We have to create a temporary Uri instance to do the URI resolution
        var u = new Uri(baseUri, relativeUri);
        return Create(u.ToString());
    }

    /// <inheritdoc/>
    public bool TryGetUri(string uri, out Uri value)
    {
        if (!InternUris)
        {
            // When interning is disabled, ignore the cache and always return false.
            value = null;
            return false;
        }

        ITrieNode<char, Uri> node = _uris.MoveToNode(uri);
        if (node.HasValue)
        {
            value = node.Value;
            return true;
        }

        if (_parent != null)
        {
            return _parent.TryGetUri(uri, out value);
        }

        // If we get here there is no parent and we don't have the value cached.
        value = null;
        return false;
    }

    /// <summary>
    /// Clears all interned URIs.
    /// </summary>
    public void Clear()
    {
        _uris.Clear();
    }
}