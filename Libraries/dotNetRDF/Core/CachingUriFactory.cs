// unset

using System;
using VDS.Common.Tries;

namespace VDS.RDF
{
    /// <summary>
    /// The default implementation of <see cref="IUriFactory"/> which caches the URI instances it creates when <see cref="InternUris"/> is set to true.
    /// </summary>
    public class CachingUriFactory : IUriFactory
    {
        private readonly ITrie<string, char, Uri> _uris = new SparseStringTrie<Uri>();

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

            ITrieNode<char, Uri> node = _uris.MoveToNode(uri);
            if (node.HasValue)
            {
                return node.Value;
            }
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
            if (!InternUris) return new Uri(baseUri, relativeUri);
            // We have to create a temporary Uri instance to do the URI resolution
            var u = new Uri(baseUri, relativeUri);
            ITrieNode<char, Uri> node = _uris.MoveToNode(u.ToString());
            if (!node.HasValue) node.Value = u;
            return node.Value;
        }

        /// <summary>
        /// Clears all interned URIs.
        /// </summary>
        public void Clear()
        {
            _uris.Clear();
        }
    }
}