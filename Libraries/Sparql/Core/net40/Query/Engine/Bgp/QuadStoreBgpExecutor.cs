using System;
using System.Collections.Generic;
using VDS.RDF.Graphs;
using VDS.RDF.Nodes;

namespace VDS.RDF.Query.Engine.Medusa
{
    /// <summary>
    /// A BGP executor over a <see cref="IQuadStore"/>
    /// </summary>
    public class QuadStoreBgpExecutor
        : BaseBgpExecutor
    {
        public QuadStoreBgpExecutor(IQuadStore quadStore)
        {
            if (quadStore == null) throw new ArgumentNullException("quadStore");
            this.QuadStore = quadStore;
        }

        public IQuadStore QuadStore { get; private set; }

        protected override IEnumerable<Quad> Find(INode g, INode s, INode p, INode o)
        {
            return this.QuadStore.Find(g, s, p, o);
        }
    }
}