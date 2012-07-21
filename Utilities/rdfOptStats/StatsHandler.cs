/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query.Optimisation;

namespace VDS.RDF.Utilities.OptimiserStats
{
    abstract class BaseStatsHandler : BaseRdfHandler
    {
        private bool _literals = false;
        private INamespaceMapper _nsmap = new NamespaceMapper();

        public BaseStatsHandler()
        { }

        public BaseStatsHandler(bool literals)
        {
            this._literals = literals;
        }

        public void GetStats(IGraph g)
        {
            g.NamespaceMap.AddNamespace("opt", new Uri(SparqlOptimiser.OptimiserStatsNamespace));
            INode subjectCount = g.CreateUriNode("opt:subjectCount");
            INode predicateCount = g.CreateUriNode("opt:predicateCount");
            INode objectCount = g.CreateUriNode("opt:objectCount");
            INode count = g.CreateUriNode("opt:count");

            this.AddStats(g, this.NodeCounts, count);
            this.AddStats(g, this.ObjectCounts, objectCount);
            this.AddStats(g, this.PredicateCounts, predicateCount);
            this.AddStats(g, this.SubjectCounts, subjectCount);
        }
        
        private void AddStats(IGraph g, IEnumerable<KeyValuePair<INode, long>> stats, INode statsProperty)
        {
            foreach (KeyValuePair<INode, long> kvp in stats)
            {
                g.Assert(kvp.Key.CopyNode(g), statsProperty, kvp.Value.ToLiteral(g));
            }
        }

        protected abstract override bool HandleTripleInternal(Triple t);

        protected sealed override bool HandleNamespaceInternal(string prefix, Uri namespaceUri)
        {
            this._nsmap.AddNamespace(prefix, namespaceUri);
            return true;
        }

        public override bool AcceptsAll
        {
	        get 
            { 
                return true; 
            }
        }

        protected bool IsCountableNode(INode n)
        {
            switch (n.NodeType)
            {
                case NodeType.Literal:
                    return this._literals;
                case NodeType.Uri:
                    return true;
                default:
                    return false;
            }
        }

        public INamespaceMapper Namespaces
        {
            get
            {
                return this._nsmap;
            }
        }

        protected virtual IEnumerable<KeyValuePair<INode, long>> SubjectCounts
        {
            get
            {
                return Enumerable.Empty<KeyValuePair<INode, long>>();
            }
        }

        protected virtual IEnumerable<KeyValuePair<INode, long>> PredicateCounts
        {
            get
            {
                return Enumerable.Empty<KeyValuePair<INode, long>>();
            }
        }

        protected virtual IEnumerable<KeyValuePair<INode, long>> ObjectCounts
        {
            get
            {
                return Enumerable.Empty<KeyValuePair<INode, long>>();
            }
        }

        protected virtual IEnumerable<KeyValuePair<INode, long>> NodeCounts
        {
            get
            {
                return Enumerable.Empty<KeyValuePair<INode, long>>();
            }
        }
    }

    /// <summary>
    /// Stats Handler that gathers statistics on Subject, Predicate and Object counts
    /// </summary>
    class SPOStatsHandler : BaseStatsHandler
    {
        private Dictionary<INode, long> _subjectCount = new Dictionary<INode, long>();
        private Dictionary<INode, long> _predicateCount = new Dictionary<INode, long>();
        private Dictionary<INode, long> _objectCount = new Dictionary<INode, long>();

        public SPOStatsHandler()
        { }

        public SPOStatsHandler(bool literals)
            : base(literals) { }

        protected override bool HandleTripleInternal(Triple t)
        {
            if (this.IsCountableNode(t.Subject))
            {
                if (!this._subjectCount.ContainsKey(t.Subject))
                {
                    this._subjectCount.Add(t.Subject, 0);
                }
                this._subjectCount[t.Subject]++;
            }

            if (this.IsCountableNode(t.Predicate))
            {
                if (!this._predicateCount.ContainsKey(t.Predicate))
                {
                    this._predicateCount.Add(t.Predicate, 0);
                }
                this._predicateCount[t.Predicate]++;
            }

            if (this.IsCountableNode(t.Object))
            {
                if (!this._objectCount.ContainsKey(t.Object))
                {
                    this._objectCount.Add(t.Object, 0);
                }
                this._objectCount[t.Object]++;
            }

            return true;
        }

        public override bool AcceptsAll
        {
            get 
            {
                return true; 
            }
        }

        protected override IEnumerable<KeyValuePair<INode, long>> ObjectCounts
        {
            get
            {
                return this._objectCount;
            }
        }

        protected override IEnumerable<KeyValuePair<INode, long>> PredicateCounts
        {
            get
            {
                return this._predicateCount;
            }
        }

        protected override IEnumerable<KeyValuePair<INode, long>> SubjectCounts
        {
            get
            {
                return this._subjectCount;
            }
        }
    }

    /// <summary>
    /// Stats Handler that gathers statistics on Subject and Predicate counts
    /// </summary>
    class SPStatsHandler : BaseStatsHandler
    {
        private Dictionary<INode, long> _subjectCount = new Dictionary<INode, long>();
        private Dictionary<INode, long> _predicateCount = new Dictionary<INode, long>();

        public SPStatsHandler()
        { }

        public SPStatsHandler(bool literals)
            : base(literals) { }

        protected override bool HandleTripleInternal(Triple t)
        {
            if (this.IsCountableNode(t.Subject))
            {
                if (!this._subjectCount.ContainsKey(t.Subject))
                {
                    this._subjectCount.Add(t.Subject, 0);
                }
                this._subjectCount[t.Subject]++;
            }

            if (this.IsCountableNode(t.Predicate))
            {
                if (!this._predicateCount.ContainsKey(t.Predicate))
                {
                    this._predicateCount.Add(t.Predicate, 0);
                }
                this._predicateCount[t.Predicate]++;
            }

            return true;
        }

        public override bool AcceptsAll
        {
            get 
            {
                return true; 
            }
        }

        protected override IEnumerable<KeyValuePair<INode, long>> PredicateCounts
        {
            get
            {
                return this._predicateCount;
            }
        }

        protected override IEnumerable<KeyValuePair<INode, long>> SubjectCounts
        {
            get
            {
                return this._subjectCount;
            }
        }
    }

    class SubjectStatsHandler : BaseStatsHandler
    {
        private Dictionary<INode, long> _subjectCount = new Dictionary<INode, long>();

        public SubjectStatsHandler()
            : base() { }

        public SubjectStatsHandler(bool literals)
            : base(literals) { }

        protected override bool HandleTripleInternal(Triple t)
        {
            if (this.IsCountableNode(t.Subject))
            {
                if (!this._subjectCount.ContainsKey(t.Subject))
                {
                    this._subjectCount.Add(t.Subject, 0);
                }
                this._subjectCount[t.Subject]++;
            }
            return true;
        }

        protected override IEnumerable<KeyValuePair<INode, long>> SubjectCounts
        {
            get
            {
                return this._subjectCount;
            }
        }
    }

    class PredicateStatsHandler : BaseStatsHandler
    {
        private Dictionary<INode, long> _predicateCount = new Dictionary<INode, long>();

        public PredicateStatsHandler()
            : base() { }

        public PredicateStatsHandler(bool literals)
            : base(literals) { }

        protected override bool HandleTripleInternal(Triple t)
        {
            if (this.IsCountableNode(t.Predicate))
            {
                if (!this._predicateCount.ContainsKey(t.Predicate))
                {
                    this._predicateCount.Add(t.Predicate, 0);
                }
                this._predicateCount[t.Predicate]++;
            }

            return true;
        }

        protected override IEnumerable<KeyValuePair<INode, long>> PredicateCounts
        {
            get
            {
                return this._predicateCount;
            }
        }
    }

    class ObjectStatsHandler : BaseStatsHandler
    {
        private Dictionary<INode, long> _objectCount = new Dictionary<INode, long>();

        public ObjectStatsHandler()
            : base() { }

        public ObjectStatsHandler(bool literals)
            : base(literals) { }

        protected override bool HandleTripleInternal(Triple t)
        {
            if (this.IsCountableNode(t.Object))
            {
                if (!this._objectCount.ContainsKey(t.Object))
                {
                    this._objectCount.Add(t.Object, 0);
                }
                this._objectCount[t.Object]++;
            }

            return true;
        }

        protected override IEnumerable<KeyValuePair<INode, long>> ObjectCounts
        {
            get
            {
                return this._objectCount;
            }
        }
    }

    class NodeStatsHandler : BaseStatsHandler
    {
        private Dictionary<INode, long> _nodeCount = new Dictionary<INode, long>();

        public NodeStatsHandler()
            : base() { }

        public NodeStatsHandler(bool literals)
            : base(literals) { }

        protected override bool HandleTripleInternal(Triple t)
        {
            if (this.IsCountableNode(t.Subject))
            {
                if (!this._nodeCount.ContainsKey(t.Subject))
                {
                    this._nodeCount.Add(t.Subject, 0);
                }
                this._nodeCount[t.Subject]++;
            }

            if (this.IsCountableNode(t.Predicate))
            {
                if (!this._nodeCount.ContainsKey(t.Predicate))
                {
                    this._nodeCount.Add(t.Predicate, 0);
                }
                this._nodeCount[t.Predicate]++;
            }

            if (this.IsCountableNode(t.Object))
            {
                if (!this._nodeCount.ContainsKey(t.Object))
                {
                    this._nodeCount.Add(t.Object, 0);
                }
                this._nodeCount[t.Object]++;
            }

            return true;
        }

        protected override IEnumerable<KeyValuePair<INode, long>> NodeCounts
        {
            get
            {
                return this._nodeCount;
            }
        }
    }
}
