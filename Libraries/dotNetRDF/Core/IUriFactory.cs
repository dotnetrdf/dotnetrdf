// unset

using System;

namespace VDS.RDF
{
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
        /// Clears all interned URIs.
        /// </summary>
        public void Clear();
    }
}