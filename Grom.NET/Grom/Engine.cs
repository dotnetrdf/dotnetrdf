namespace Grom
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using VDS.RDF;

    // TODO: Implement dictionary?
    public partial class Engine
    {
        private readonly IGraph graph;
        internal Uri prefix;

        // TODO: Treat insance uri and schema uri separately?
        public Engine(IGraph graph, Uri prefix) : this(graph)
        {
            this.prefix = prefix;
        }

        public Engine(IGraph graph)
        {
            this.graph = graph;
        }

        private IEnumerable<INode> GetNodes()
        {
            return
                from node in this.graph.Nodes
                where node.NodeType == NodeType.Uri || node.NodeType == NodeType.Blank
                select node;
        }
    }
}
