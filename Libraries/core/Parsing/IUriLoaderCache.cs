/*

Copyright Robert Vesse 2009-10
rvesse@vdesign-studios.com

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

#if !NO_URICACHE

using System;
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
        /// Calling code <strong>MUST</strong> always use the <see cref="IUriLoaderCache.HasETag()">HasETag()</see> method prior to using this method so it should be safe to throw the <see cref="KeyNotFoundException">KeyNotFoundException</see> if there is no ETag for the given URI
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

        /// <summary>
        /// Caches a Graph in the Cache
        /// </summary>
        /// <param name="requestUri">URI from which the Graph was requested</param>
        /// <param name="responseUri">The actual URI which responded to the request</param>
        /// <param name="g">Graph</param>
        /// <param name="etag">ETag</param>
        [Obsolete("This form of the ToCache() method is obsolete and should not be used", true)]
        void ToCache(Uri requestUri, Uri responseUri, IGraph g, String etag);
    }
}

#endif