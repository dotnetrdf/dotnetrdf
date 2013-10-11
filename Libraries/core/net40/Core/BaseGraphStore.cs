using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Collections;

namespace VDS.RDF.Core
{
    /// <summary>
    /// Abstract base implementation of a Graph store that uses a <see cref="IGraphCollection"/> behind the scenes
    /// </summary>
    public abstract class BaseGraphStore
        : IGraphStore
    {
        /// <summary>
        /// The graph collection being used
        /// </summary>
        protected readonly IGraphCollection _graphs;

        /// <summary>
        /// Creates a new graph store using the default graph collection implementation
        /// </summary>
        protected BaseGraphStore()
            : this(new GraphCollection()) { }

        /// <summary>
        /// Creates a new graph store using the given graph collection
        /// </summary>
        /// <param name="collection">Graph Collection</param>
        protected BaseGraphStore(IGraphCollection collection)
        {
            if (collection == null) throw new ArgumentNullException("collection", "Graph Collection cannot be null");
            this._graphs = collection;
        }

        public IEnumerable<INode> GraphNames
        {
            get
            {
                return this._graphs.Keys; 
            }
        }

        public IEnumerable<IGraph> Graphs
        {
            get 
            {
                return this._graphs.Values;
            }
        }

        public IGraph this[INode graphName]
        {
            get
            {
                if (ReferenceEquals(graphName, null)) graphName = Quad.DefaultGraphNode;
                return this._graphs[graphName];
            }
        }

        public bool HasGraph(INode graphName)
        {
            if (ReferenceEquals(graphName, null)) graphName = Quad.DefaultGraphNode;
            return this._graphs.ContainsKey(graphName);
        }

        public bool Add(IGraph g)
        {
            return this.Add(Quad.DefaultGraphNode, g);
        }

        public bool Add(INode graphName, IGraph g)
        {
            if (ReferenceEquals(graphName, null)) graphName = Quad.DefaultGraphNode;
            this._graphs.Add(graphName, g);
            return true;
        }

        public bool Add(INode graphName, Triple t)
        {
            if (ReferenceEquals(graphName, null)) graphName = Quad.DefaultGraphNode;
            return this.Add(t.AsQuad(graphName));
        }

        public bool Add(Quad q)
        {
            if (this._graphs.ContainsKey(q.Graph))
            {
                IGraph g = this[q.Graph];
                return g.Assert(q.AsTriple());
            }
            else
            {
                IGraph g = new Graph();
                g.Assert(q.AsTriple());
                this.Add(q.Graph, g);
                return true;
            }
        }

        public bool Copy(INode srcName, INode destName, bool overwrite)
        {
            if (EqualityHelper.AreNodesEqual(srcName, destName)) return false;

            //Get the source graph if available
            IGraph src;
            if (ReferenceEquals(srcName, null)) srcName = Quad.DefaultGraphNode;
            if (this.HasGraph(srcName))
            {
                src = this[srcName];
            }
            else
            {
                return false;
            }
            //Get the destination graph
            IGraph dest;
            if (ReferenceEquals(destName, null)) destName = Quad.DefaultGraphNode;
            if (this.HasGraph(destName))
            {
                dest = this[destName];
                if (overwrite) dest.Clear();
            }
            else
            {
                dest = new Graph();
                this.Add(destName, dest);
            }
            
            //Copy triples
            dest.Assert(src.Triples);
            return true;
        }

        public bool Move(INode srcName, INode destName, bool overwrite)
        {
            if (EqualityHelper.AreNodesEqual(srcName, destName)) return false;

            //Get the source graph if available
            IGraph src;
            if (ReferenceEquals(srcName, null)) srcName = Quad.DefaultGraphNode;
            if (this.HasGraph(srcName))
            {
                src = this[srcName];
            }
            else
            {
                return false;
            }
            //Get the destination graph
            IGraph dest;
            if (ReferenceEquals(destName, null)) destName = Quad.DefaultGraphNode;
            if (this.HasGraph(destName))
            {
                dest = this[destName];
                if (overwrite) dest.Clear();
            }
            else
            {
                dest = new Graph();
                this.Add(destName, dest);
            }

            //Copy triples
            dest.Assert(src.Triples);

            //Remove from source
            src.Clear();
            return true;
        }

        public bool Clear(INode graphName)
        {
            if (ReferenceEquals(graphName, null)) graphName = Quad.DefaultGraphNode;
            if (this.HasGraph(graphName))
            {
                IGraph g = this[graphName];
                g.Clear();
                return true;
            }
            return false;
        }

        public bool Remove(IGraph g)
        {
            return this.Remove(Quad.DefaultGraphNode, g);
        }

        public bool Remove(INode graphName, IGraph g)
        {
            if (ReferenceEquals(graphName, null)) graphName = Quad.DefaultGraphNode;
            if (this.HasGraph(graphName))
            {
                IGraph dest = this[graphName];
                return dest.Retract(g.Triples);
            }
            return false;
        }

        public bool Remove(INode graphName)
        {
            if (ReferenceEquals(graphName, null)) graphName = Quad.DefaultGraphNode;
            return this._graphs.Remove(graphName);
        }

        public bool Remove(INode graphName, Triple t)
        {
            if (ReferenceEquals(graphName, null)) graphName = Quad.DefaultGraphNode;
            if (this.HasGraph(graphName))
            {
                IGraph g = this[graphName];
                return g.Retract(t);
            }
            return false;
        }

        public bool Remove(Quad q)
        {
            return this.Remove(q.Graph, q.AsTriple());
        }

        public IEnumerable<Triple> Triples
        {
            get 
            {
                return (from g in this._graphs.Values
                        from t in g.Triples
                        select t);
            }
        }

        public IEnumerable<Quad> Quads
        {
            get 
            {
                return (from kvp in this._graphs
                        from t in kvp.Value.Triples
                        select t.AsQuad(kvp.Key));

            }
        }

        public virtual IEnumerable<Triple> FindTriples(INode s, INode p, INode o)
        {
            return (from IGraph g in this._graphs.Values
                    from t in g.Find(s, p, o)
                    select t);
        }

        public virtual IEnumerable<Quad> FindQuads(INode s, INode p, INode o)
        {
            return (from KeyValuePair<INode, IGraph> kvp in this._graphs
                    from t in kvp.Value.Find(s, p, o)
                    select t.AsQuad(kvp.Key));
        }

        public virtual IEnumerable<Quad> FindQuads(INode g, INode s, INode p, INode o)
        {
            if (ReferenceEquals(g, null))
            {
                // Null acts as a wildcard
                return this.FindQuads(s, p, o);
            }
            else if (this.HasGraph(g))
            {
                // Otherwise search in a specific named graph
                return this[g].Find(s, p, o).AsQuads(g);
            }
            else
            {
                return Enumerable.Empty<Quad>();
            }
        }

        public bool Contains(Triple t)
        {
            return this._graphs.Values.Any(g => g.ContainsTriple(t));
        }

        public bool Contains(Quad q)
        {
            if (this.HasGraph(q.Graph))
            {
                return this[q.Graph].ContainsTriple(q.AsTriple());
            }
            return false;
        }
    }
}
