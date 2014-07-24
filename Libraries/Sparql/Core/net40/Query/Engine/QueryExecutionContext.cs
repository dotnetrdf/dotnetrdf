using System;
using System.Collections.Generic;
using VDS.Common.Collections;
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
            this.DefaultGraphs = defaultGraphs != null ? new MaterializedImmutableView<INode>(defaultGraphs) : new MaterializedImmutableView<INode>();
            this.NamedGraphs = namedGraphs != null ? new MaterializedImmutableView<INode>(namedGraphs) : new MaterializedImmutableView<INode>();
        }

        public QueryExecutionContext(IQuery query)
            : this(Quad.DefaultGraphNode, query.DefaultGraphs, query.NamedGraphs) { }

        public INode ActiveGraph { get; private set; }

        public ICollection<INode> DefaultGraphs { get; private set; } 

        public ICollection<INode> NamedGraphs { get; private set; } 

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