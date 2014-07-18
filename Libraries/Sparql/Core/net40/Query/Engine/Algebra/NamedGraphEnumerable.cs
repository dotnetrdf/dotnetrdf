using System;
using System.Collections;
using System.Collections.Generic;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Algebra;

namespace VDS.RDF.Query.Engine.Algebra
{
    public class NamedGraphEnumerable
        : IEnumerable<ISet>
    {
        public NamedGraphEnumerable(NamedGraph namedGraph, IAlgebraExecutor executor, IExecutionContext context)
        {
            if (namedGraph == null) throw new ArgumentNullException("namedGraph");
            if (executor == null) throw new ArgumentNullException("executor");
            if (context == null) throw new ArgumentNullException("context");
            this.NamedGraph = namedGraph;
            this.Executor = executor;
            this.Context = context;
        }

        private NamedGraph NamedGraph { get; set; }

        private IAlgebraExecutor Executor { get; set; }

        private IExecutionContext Context { get; set; }

        public IEnumerator<ISet> GetEnumerator()
        {
            return new NamedGraphEnumerator(this.NamedGraph, this.Executor, this.Context);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class NamedGraphEnumerator
        : IEnumerator<ISet>
    {
        private ISet _current;
        private IEnumerator<INode> _graphNames;
        private IEnumerator<ISet> _enumerator;

        public NamedGraphEnumerator(NamedGraph namedGraph, IAlgebraExecutor executor, IExecutionContext context)
        {
            if (namedGraph == null) throw new ArgumentNullException("namedGraph");
            if (executor == null) throw new ArgumentNullException("executor");
            if (context == null) throw new ArgumentNullException("context");
            this.NamedGraph = namedGraph;
            this.Executor = executor;
            this.Context = context;
            this.GraphVariable = this.NamedGraph.Graph.VariableName;
        }

        private NamedGraph NamedGraph { get; set; }

        private IAlgebraExecutor Executor { get; set; }

        private IExecutionContext Context { get; set; }

        private String GraphVariable { get; set; }

        private bool Started { get; set; }

        private bool Finished { get; set; }

        public void Dispose()
        {
            if (this._graphNames != null)
                this._graphNames.Dispose();
            if (this._enumerator != null)
                this._enumerator.Dispose();
        }

        public bool MoveNext()
        {
            if (!this.Started)
            {
                this.Started = true;
                this._graphNames = this.Context.NamedGraphs.GetEnumerator();
            }
            if (this.Finished) return false;

            while (true)
            {
                if (this._enumerator == null)
                {
                    // Move to next graph
                    if (!this._graphNames.MoveNext())
                    {
                        this._graphNames.Dispose();
                        this.Finished = true;
                        return false;
                    }
                    // Execute the inner algebra against the next graph
                    IExecutionContext nextContext = this.Context.PushActiveGraph(this._graphNames.Current);
                    this._enumerator = this.NamedGraph.InnerAlgebra.Execute(this.Executor, nextContext).GetEnumerator();
                }

                // If no further results for current graph try the next graph
                if (!this._enumerator.MoveNext())
                {
                    this._enumerator.Dispose();
                    this._enumerator = null;
                    continue;
                }

                // We have a next set from the inner algebra execution
                // Assign/Check Graph Variable as appropriate
                ISet set = this._enumerator.Current;
                if (set[this.GraphVariable] == null)
                {
                    // Not yet assigned so assign now
                    this._current = new Set(set);
                    this._current.Add(this.GraphVariable, this._graphNames.Current);
                    return true;
                }
                // If assigned to non-matching value then ignore
                if (!set[this.GraphVariable].Equals(this._graphNames.Current)) continue;

                // Already assigned to correct value so leave as-is
                this._current = set;
                return true;
            }
        }

        public void Reset()
        {
            this.Started = true;
            this.Finished = true;
            this._graphNames = null;
            this._enumerator = null;
        }

        public ISet Current
        {
            get
            {
                if (!this.Started) throw new InvalidOperationException("Currently before the start of the enumerator, call MoveNext() before accessing Current");
                if (this.Finished) throw new InvalidOperationException("Currently after end of the enumerator");
                return this._current;
            }
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }
    }
}