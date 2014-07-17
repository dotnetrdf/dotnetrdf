using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Graphs;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Query.Engine
{
    public class QueryExecutionContext
        : IExecutionContext
    {
        public QueryExecutionContext()
            : this(Quad.DefaultGraphNode, Quad.DefaultGraphNode.AsEnumerable(), null) {}

        public QueryExecutionContext(INode activeGraph, IEnumerable<INode> defaultGraphs, IEnumerable<INode> namedGraphs)
        {
            this.ActiveGraph = activeGraph;
            this.DefaultGraphs = defaultGraphs != null ? new List<INode>(defaultGraphs) : Enumerable.Empty<INode>();
            this.NamedGraphs = namedGraphs != null ? new List<INode>(namedGraphs) : Enumerable.Empty<INode>();
        }

        public INode ActiveGraph { get; private set; }

        public IEnumerable<INode> DefaultGraphs { get; private set; } 

        public IEnumerable<INode> NamedGraphs { get; private set; } 

        public IExecutionContext PushActiveGraph(INode graphName)
        {
            return new QueryExecutionContext(graphName, this.DefaultGraphs, this.NamedGraphs);
        }

        public virtual IExpressionContext CreateExpressionContext()
        {
            throw new NotImplementedException();
        }
    }
}