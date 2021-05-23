using System;

namespace VDS.RDF
{
    /// <summary>
    /// A Graph Factory provides access to consistent Graph References so that Nodes and Triples can be instantiated in the correct Graphs.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Primarily designed for internal use in some of our code but may prove useful to other users hence is a public class.  Internally this is just a wrapper around a <see cref="TripleStore">TripleStore</see> instance.
    /// </para>
    /// <para>
    /// The main usage for this class is scenarios where consistent graph references matter such as returning node references from out of memory datasets (like SQL backed ones) particularly with regards to blank nodes since blank node equality is predicated upon Graph reference.
    /// </para>
    /// </remarks>
    [Obsolete("This class is obsolete and will be removed in a future release. There is no replacement for this class.")]
    public class GraphFactory
    {
        private TripleStore _store = new TripleStore();

        /// <summary>
        /// Gets a Graph Reference for the given Graph URI.
        /// </summary>
        /// <param name="graphUri">Graph URI.</param>
        /// <returns></returns>
        public IGraph this[Uri graphUri]
        {
            get
            {
                if (_store.HasGraph(graphUri))
                {
                    return _store[graphUri];
                }
                else
                {
                    var g = new Graph();
                    g.BaseUri = graphUri;
                    _store.Add(g);
                    return g;
                }
            }
        }

        /// <summary>
        /// Gets a Graph Reference for the given Graph URI.
        /// </summary>
        /// <param name="graphUri">Graph URI.</param>
        /// <returns></returns>
        /// <remarks>
        /// Synonym for the index access method i.e. factory[graphUri].
        /// </remarks>
        public IGraph GetGraph(Uri graphUri)
        {
            return this[graphUri];
        }

        /// <summary>
        /// Gets a Graph Reference for the given Graph URI and indicates whether this was a new Graph reference.
        /// </summary>
        /// <param name="graphUri">Graph URI.</param>
        /// <param name="created">Indicates whether the returned reference was newly created.</param>
        /// <returns></returns>
        public IGraph TryGetGraph(Uri graphUri, out bool created)
        {
            if (_store.HasGraph(graphUri))
            {
                created = false;
                return _store[graphUri];
            }
            else
            {
                created = true;
                var g = new Graph();
                g.BaseUri = graphUri;
                _store.Add(g);
                return g;
            }
        }

        /// <summary>
        /// Resets the Factory so any Graphs with contents are emptied.
        /// </summary>
        public void Reset()
        {
            foreach (IGraph g in _store.Graphs)
            {
                g.Clear();
            }
        }
    }
}
