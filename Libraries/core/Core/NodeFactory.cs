using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF
{
    /// <summary>
    /// A Node Factory provides access to consistent Graph References so that Nodes and Triples can be instantiated in the correct Graphs
    /// </summary>
    /// <remarks>
    /// <para>
    /// Primarily designed for internal use in some of our code but may prove useful to other users hence is a public class.  Internally this is just a Triple Store
    /// </para>
    /// </remarks>
    public class NodeFactory
    {
        private TripleStore _store = new TripleStore();

        public IGraph this[Uri graphUri]
        {
            get
            {
                if (this._store.HasGraph(graphUri))
                {
                    return this._store.Graph(graphUri);
                }
                else
                {
                    Graph g = new Graph();
                    g.BaseUri = graphUri;
                    this._store.Add(g);
                    return g;
                }
            }
        }
    }
}
