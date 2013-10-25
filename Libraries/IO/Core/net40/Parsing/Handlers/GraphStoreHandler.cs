using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Graphs;

namespace VDS.RDF.Parsing.Handlers
{
    public class GraphStoreHandler
        : BaseRdfHandler
    {
        private readonly IGraphStore _graphStore;

        public GraphStoreHandler(IGraphStore graphStore)
        {
            if (ReferenceEquals(graphStore, null)) throw new ArgumentNullException("graphStore");
            this._graphStore = graphStore;
        }

        protected override bool HandleTripleInternal(Triple t)
        {
            return this.HandleQuadInternal(t.AsQuad(Quad.DefaultGraphNode));
        }

        protected override bool HandleQuadInternal(Quad q)
        {
            this._graphStore.Add(q);
            return true;
        }

        public override bool AcceptsAll
        {
            get { return true; }
        }
    }
}
