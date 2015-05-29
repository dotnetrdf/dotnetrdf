/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2015 dotNetRDF Project (dotnetrdf-develop@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Concurrent;
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

            // Make sure to use a concurrent dictionary because in principal things could choose to share state while executing in parallel
            this.SharedObjects = new ConcurrentDictionary<string, object>();
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

        public IDictionary<string, object> SharedObjects { get; private set; }
    }
}