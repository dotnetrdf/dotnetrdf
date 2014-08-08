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
        private DateTimeOffset? _now;

        /// <summary>
        /// Creates a new execution context
        /// </summary>
        public QueryExecutionContext()
            : this(Quad.DefaultGraphNode, Quad.DefaultGraphNode.AsEnumerable(), null) {}

        /// <summary>
        /// Creates a neq execution context
        /// </summary>
        /// <param name="activeGraph">Active Graph</param>
        /// <param name="defaultGraphs">Default Graphs</param>
        /// <param name="namedGraphs">Named Graphs</param>
        public QueryExecutionContext(INode activeGraph, IEnumerable<INode> defaultGraphs, IEnumerable<INode> namedGraphs)
        {
            this.ActiveGraph = activeGraph;
            this.DefaultGraphs = defaultGraphs != null ? new MaterializedImmutableView<INode>(defaultGraphs) : new MaterializedImmutableView<INode>();
            this.NamedGraphs = namedGraphs != null ? new MaterializedImmutableView<INode>(namedGraphs) : new MaterializedImmutableView<INode>();
        }

        /// <summary>
        /// Creates a new execution context for the given query
        /// </summary>
        /// <param name="query">Query</param>
        /// <param name="datasetDefaultGraphs">Dataset Default Graphs, used only if the query does not specify a dataset</param>
        /// <param name="datasetNamedGraphs">Dataset Named Graphs, used only if the query does not specify a dataset</param>
        public QueryExecutionContext(IQuery query, IEnumerable<INode> datasetDefaultGraphs, IEnumerable<INode> datasetNamedGraphs)
        {
            if (query == null) throw new ArgumentNullException("query");

            this.ActiveGraph = Quad.DefaultGraphNode;
            if (query.DefaultGraphs.Count > 0 || query.NamedGraphs.Count > 0)
            {
                this.DefaultGraphs = new MaterializedImmutableView<INode>(query.DefaultGraphs);
                this.NamedGraphs = new MaterializedImmutableView<INode>(query.NamedGraphs);
            }
            else
            {
                if (datasetDefaultGraphs == null) throw new ArgumentNullException("datasetDefaultGraphs");
                if (datasetNamedGraphs == null) throw new ArgumentNullException("datasetNamedGraphs");
                this.DefaultGraphs = new MaterializedImmutableView<INode>(datasetDefaultGraphs);
                this.NamedGraphs = new MaterializedImmutableView<INode>(datasetNamedGraphs);
            }
        }

        public INode ActiveGraph { get; private set; }

        public ICollection<INode> DefaultGraphs { get; private set; }

        public ICollection<INode> NamedGraphs { get; private set; }

        public IExecutionContext PushActiveGraph(INode graphName)
        {
            return new QueryExecutionContext(graphName, this.DefaultGraphs, this.NamedGraphs);
        }

        public virtual IExpressionContext CreateExpressionContext()
        {
            return new ExpressionContext(this);
        }

        public DateTimeOffset EffectiveNow
        {
            get
            {
                if (!this._now.HasValue) this._now = DateTimeOffset.Now;
                return this._now.Value;
            }
        }
    }
}