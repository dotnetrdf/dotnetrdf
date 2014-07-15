using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Graphs;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Algebra;

namespace VDS.RDF.Query.Engine.Medusa
{
    public abstract class MedusaAlgebraExecutor
        : IAlgebraExecutor
    {
        private Stack<INode> _activeGraphNames = new Stack<INode>();

        protected MedusaAlgebraExecutor(IBgpExecutor executor)
        {
            if (executor == null) throw new ArgumentNullException("executor");
            this.BgpExecutor = executor;
            this._activeGraphNames.Push(Quad.DefaultGraphNode);
        }

        public IBgpExecutor BgpExecutor { get; private set; }

        public INode ActiveGraph { get { return this._activeGraphNames.Peek(); } }

        public void PushActiveGraph(INode graphName)
        {
            this._activeGraphNames.Push(graphName);
        }

        public void PopActiveGraph()
        {
            if (this._activeGraphNames.Count <= 1) throw new RdfQueryException("Cannot pop the active graph when there is only 1 active graph in the stack");
            this._activeGraphNames.Pop();
        }

        public virtual IEnumerable<ISet> Execute(IAlgebra algebra)
        {
            throw new NotImplementedException();
        }

        public virtual IEnumerable<ISet> Execute(Bgp bgp)
        {
            List<Triple> patterns = bgp.TriplePatterns.ToList();
            if (patterns.Count == 0) return new Set().AsEnumerable();

            IEnumerable<ISet> results = Enumerable.Empty<ISet>();
            for (int i = 0; i < patterns.Count; i++)
            {
                if (i == 0)
                {
                    results = this.BgpExecutor.Match(this.ActiveGraph, patterns[i]);
                }
                else
                {
                    int i1 = i;
                    results = results.SelectMany(s => this.BgpExecutor.Match(this.ActiveGraph, patterns[i1], s));
                }
            }
            return results;
        }

        public virtual IEnumerable<ISet> Execute(Slice slice)
        {
            if (slice.Limit == 0) return Enumerable.Empty<ISet>();

            IEnumerable<ISet> innerResult = this.Execute(slice.InnerAlgebra);
            if (slice.Limit > 0)
            {
                return slice.Offset > 0 ? innerResult.Skip(slice.Offset).Take(slice.Limit) : innerResult.Take(slice.Limit);
            }
            return slice.Offset > 0 ? innerResult.Skip(slice.Offset) : innerResult;
        }
    }
}
