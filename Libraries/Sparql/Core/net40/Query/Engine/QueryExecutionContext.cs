using System;
using System.Collections.Generic;
using VDS.RDF.Graphs;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Query.Engine
{
    public class QueryExecutionContext
        : IExecutionContext
    {
        public QueryExecutionContext()
            : this(Quad.DefaultGraphNode) {}

        public QueryExecutionContext(INode activeGraph)
        {
            this.ActiveGraph = activeGraph;
        }

        public INode ActiveGraph { get; private set; }

        public IExecutionContext PushActiveGraph(INode graphName)
        {
            return new QueryExecutionContext(graphName);
        }

        public virtual IExpressionContext CreateExpressionContext()
        {
            throw new NotImplementedException();
        }
    }
}