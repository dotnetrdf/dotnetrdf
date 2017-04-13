/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
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
using VDS.RDF.Parsing.Handlers;

namespace VDS.RDF.Parsing
{
    /// <summary>
    /// Interface for Caches that can be used to cache the result of loading Graphs from URIs
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Warning:</strong> Only available in Builds for which caching is supported e.g. not supported under Silverlight
    /// </para>
    /// <para>
    /// Implementors should take care to implement their caches such that any errors in the cache do not bubble up outside of the cache.  If the cache encounters any error when caching data or retrieving data from the cache it should indicate that the cached data is not available
    /// </para>
    /// </remarks>
    public interface IUriLoaderCache
    {
        /// <summary>
        /// Gets/Sets the Cache Directory that is in use
        /// </summary>
        /// <remarks>
        /// <para>
        /// Non-filesystem based caches are free to return String.Empty or null but <strong>MUST NOT</strong> throw any form or error
        /// </para>
        /// </remarks>
        String CacheDirectory { get; set; }

        /// <summary>
        /// Gets/Sets how long results should be cached
        /// </summary>
        /// <remarks>
        /// This only applies to downloaded URIs where an ETag is not available, where ETags are available ETag based caching <strong>SHOULD</strong> be used
        /// </remarks>
        TimeSpan CacheDuration { get; set; }

        /// <summary>
        /// Clears the Cache
        /// </summary>
        void Clear();

        /// <summary>
        /// Gets the ETag for the given URI
        /// </summary>
        /// <param name="u">URI</param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException">Thrown if there is no ETag for the given URI</exception>
        /// <remarks>
        /// <para>
        /// Calling code <strong>MUST</strong> always use the <see cref="HasETag(Uri)">HasETag()</see> method prior to using this method so it should be safe to throw the <see cref="KeyNotFoundException">KeyNotFoundException</see> if there is no ETag for the given URI
        /// </para>
        /// </remarks>
        String GetETag(Uri u);

        /// <summary>
        /// Gets the path to the locally cached copy of the Graph from the given URI
        /// </summary>
        /// <param name="u">URI</param>
        /// <returns></returns>
        String GetLocalCopy(Uri u);

        /// <summary>
        /// Gets whether there is an ETag for the given URI
        /// </summary>
        /// <param name="u">URI</param>
        /// <returns></returns>
        bool HasETag(Uri u);

        /// <summary>
        /// Is there a locally cached copy of the Graph from the given URI which is not expired
        /// </summary>
        /// <param name="u">URI</param>
        /// <param name="requireFreshness">Whether the local copy is required to meet the Cache Freshness (set by the Cache Duration)</param>
        /// <returns></returns>
        bool HasLocalCopy(Uri u, bool requireFreshness);

        /// <summary>
        /// Remove the ETag record for the given URI
        /// </summary>
        /// <param name="u">URI</param>
        void RemoveETag(Uri u);

        /// <summary>
        /// Removes a locally cached copy of a URIs results from the Cache
        /// </summary>
        /// <param name="u">URI</param>
        void RemoveLocalCopy(Uri u);

        /// <summary>
        /// Associates an ETag (if any) with the Request and Response URIs plus returns an IRdfHandler that can be used to write to the cache
        /// </summary>
        /// <param name="requestUri">URI from which the RDF Graph was requested</param>
        /// <param name="responseUri">The actual URI which responded to the request</param>
        /// <param name="etag">ETag of the response (if any)</param>
        /// <returns>Either an instance of an <see cref="IRdfHandler">IRdfHandler</see> that will do the caching or null if no caching is possible</returns>
        IRdfHandler ToCache(Uri requestUri, Uri responseUri, String etag);
    }
}
