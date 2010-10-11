using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF;

namespace VDS.Alexandria.Utilities
{
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
