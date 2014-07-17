using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Graphs;
using VDS.RDF.Nodes;

namespace VDS.RDF.Query.Engine.Medusa
{
    /// <summary>
    /// A BGP executor over a single <see cref="IGraph"/> instance
    /// </summary>
    /// <remarks>
    /// When used only BGPs that target the default graph identified by the special node <see cref="Quad.DefaultGraphNode"/> will match, any other graph will return empty results
    /// </remarks>
    public class GraphBgpExecutor
        : BaseBgpExecutor
    {
        public GraphBgpExecutor(IGraph g)
        {
            if (g == null) throw new ArgumentNullException("g");
            this.Graph = g;
        }

        public IGraph Graph { get; private set; }

        protected override IEnumerable<Quad> Find(INode g, INode s, INode p, INode o)
        {
            return Quad.DefaultGraphNode.Equals(g) ? this.Graph.Find(s, p, o).AsQuads(g) : Enumerable.Empty<Quad>();
        }
    }
}