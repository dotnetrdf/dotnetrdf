/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2013 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Collections;
using VDS.RDF.Nodes;

namespace VDS.RDF.Graphs
{
    /// <summary>
    /// In-memory implementation of a Graph store that uses a <see cref="IGraphCollection"/> behind the scenes
    /// </summary>
    public class GraphStore
        : IGraphStore
    {
        /// <summary>
        /// The graph collection being used
        /// </summary>
        protected readonly IGraphCollection _graphs;

        /// <summary>
        /// Creates a new graph store using the default graph collection implementation
        /// </summary>
        public GraphStore()
            : this(new GraphCollection()) { }

        /// <summary>
        /// Creates a new graph store using the given graph collection
        /// </summary>
        /// <param name="collection">Graph Collection</param>
        public GraphStore(IGraphCollection collection)
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

        public void Add(IGraph g)
        {
            this.Add(Quad.DefaultGraphNode, g);
        }

        public void Add(INode graphName, IGraph g)
        {
            if (ReferenceEquals(graphName, null)) graphName = Quad.DefaultGraphNode;
            this._graphs.Add(graphName, g);
        }

        public void Add(INode graphName, Triple t)
        {
            if (ReferenceEquals(graphName, null)) graphName = Quad.DefaultGraphNode;
            this.Add(t.AsQuad(graphName));
        }

        public void Add(Quad q)
        {
            if (this._graphs.ContainsKey(q.Graph))
            {
                IGraph g = this[q.Graph];
                g.Assert(q.AsTriple());
            }
            else
            {
                IGraph g = new Graph();
                g.Assert(q.AsTriple());
                this.Add(q.Graph, g);
            }
        }

        public void Copy(INode srcName, INode destName, bool overwrite)
        {
            if (EqualityHelper.AreNodesEqual(srcName, destName)) return;

            //Get the source graph if available
            IGraph src;
            if (ReferenceEquals(srcName, null)) srcName = Quad.DefaultGraphNode;
            if (this.HasGraph(srcName))
            {
                src = this[srcName];
            }
            else
            {
                return;
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
        }

        public void Move(INode srcName, INode destName, bool overwrite)
        {
            if (EqualityHelper.AreNodesEqual(srcName, destName)) return;

            //Get the source graph if available
            IGraph src;
            if (ReferenceEquals(srcName, null)) srcName = Quad.DefaultGraphNode;
            if (this.HasGraph(srcName))
            {
                src = this[srcName];
            }
            else
            {
                return;
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
        }

        public void Clear(INode graphName)
        {
            if (ReferenceEquals(graphName, null)) graphName = Quad.DefaultGraphNode;
            if (this.HasGraph(graphName))
            {
                IGraph g = this[graphName];
                g.Clear();
            }
        }

        public void Remove(IGraph g)
        {
            this.Remove(Quad.DefaultGraphNode, g);
        }

        public void Remove(INode graphName, IGraph g)
        {
            if (ReferenceEquals(graphName, null)) graphName = Quad.DefaultGraphNode;
            if (this.HasGraph(graphName))
            {
                IGraph dest = this[graphName];
                dest.Retract(g.Triples);
            }
        }

        public void Remove(INode graphName)
        {
            if (ReferenceEquals(graphName, null)) graphName = Quad.DefaultGraphNode;
            this._graphs.Remove(graphName);
        }

        public void Remove(INode graphName, Triple t)
        {
            if (ReferenceEquals(graphName, null)) graphName = Quad.DefaultGraphNode;
            if (this.HasGraph(graphName))
            {
                IGraph g = this[graphName];
                g.Retract(t);
            }
        }

        public void Remove(Quad q)
        {
            this.Remove(q.Graph, q.AsTriple());
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
