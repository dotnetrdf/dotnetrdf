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
        public BaseGraphStore()
            : this(new GraphCollection()) { }

        /// <summary>
        /// Creates a new graph store using the given graph collection
        /// </summary>
        /// <param name="collection">Graph Collection</param>
        public BaseGraphStore(IGraphCollection collection)
        {
            if (collection == null) throw new ArgumentNullException("collection", "Graph Collection cannot be null");
            this._graphs = collection;
        }

        public IEnumerable<Uri> GraphUris
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

        public IGraph this[Uri u]
        {
            get 
            {
                return this._graphs[u];
            }
        }

        public bool HasGraph(Uri u)
        {
            return this._graphs.ContainsKey(u);
        }

        public bool Add(IGraph g)
        {
            return this.Add(g.BaseUri, g);
        }

        public bool Add(Uri graphUri, IGraph g)
        {
            this._graphs.Add(graphUri, g);
            return true;
        }

        public bool Add(Uri graphUri, Triple t)
        {
            return this.Add(t.AsQuad(graphUri));
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
                g.BaseUri = q.Graph;
                g.Assert(q.AsTriple());
                this.Add(g);
                return true;
            }
        }

        public bool Copy(Uri srcUri, Uri destUri, bool overwrite)
        {
            if (EqualityHelper.AreUrisEqual(srcUri, destUri)) return false;

            //Get the source graph if available
            IGraph src;
            if (this.HasGraph(srcUri))
            {
                src = this[srcUri];
            }
            else
            {
                return false;
            }
            //Get the destination graph
            IGraph dest;
            if (this.HasGraph(destUri))
            {
                dest = this[destUri];
                if (overwrite) dest.Clear();
            }
            else
            {
                dest = new Graph();
                dest.BaseUri = destUri;
                this.Add(dest);
            }
            
            //Copy triples
            dest.Assert(src.Triples);
            return true;
        }

        public bool Move(Uri srcUri, Uri destUri, bool overwrite)
        {
            if (EqualityHelper.AreUrisEqual(srcUri, destUri)) return false;

            //Get the source graph if available
            IGraph src;
            if (this.HasGraph(srcUri))
            {
                src = this[srcUri];
            }
            else
            {
                return false;
            }
            //Get the destination graph
            IGraph dest;
            if (this.HasGraph(destUri))
            {
                dest = this[destUri];
                if (overwrite) dest.Clear();
            }
            else
            {
                dest = new Graph();
                dest.BaseUri = destUri;
                this.Add(dest);
            }

            //Copy triples
            dest.Assert(src.Triples);

            //Remove from source
            src.Clear();
            return true;
        }

        public bool Clear(Uri graphUri)
        {
            if (this.HasGraph(graphUri))
            {
                IGraph g = this[graphUri];
                g.Clear();
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool Remove(IGraph g)
        {
            return this.Remove(g.BaseUri, g);
        }

        public bool Remove(Uri graphUri, IGraph g)
        {
            if (this.HasGraph(graphUri))
            {
                IGraph dest = this[graphUri];
                return dest.Retract(g.Triples);
            }
            else
            {
                return false;
            }
        }

        public bool Remove(Uri graphUri)
        {
            return this._graphs.Remove(graphUri);
        }

        public bool Remove(Uri graphUri, Triple t)
        {
            if (this.HasGraph(graphUri))
            {
                IGraph g = this[graphUri];
                return g.Retract(t);
            }
            else
            {
                return false;
            }
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

        public IEnumerable<Triple> GetTriplesWithSubject(IEnumerable<Uri> graphUris, INode subj)
        {
            return (from g in graphUris.Select(u => this[u])
                    from t in g.GetTriplesWithSubject(subj)
                    select t);
        }

        public IEnumerable<Triple> GetTriplesWithSubjectPredicate(IEnumerable<Uri> graphUris, INode subj, INode pred)
        {
            return (from g in graphUris.Select(u => this[u])
                    from t in g.GetTriplesWithSubjectPredicate(subj, pred)
                    select t);
        }

        public IEnumerable<Triple> GetTriplesWithSubjectObject(IEnumerable<Uri> graphUris, INode subj, INode obj)
        {
            return (from g in graphUris.Select(u => this[u])
                    from t in g.GetTriplesWithSubjectObject(subj, obj)
                    select t);
        }

        public IEnumerable<Triple> GetTriplesWithPredicate(IEnumerable<Uri> graphUris, INode pred)
        {
            return (from g in graphUris.Select(u => this[u])
                    from t in g.GetTriplesWithPredicate(pred)
                    select t);
        }

        public IEnumerable<Triple> GetTriplesWithPredicateObject(IEnumerable<Uri> graphUris, INode pred, INode obj)
        {
            return (from g in graphUris.Select(u => this[u])
                    from t in g.GetTriplesWithPredicateObject(pred, obj)
                    select t);
        }

        public IEnumerable<Triple> GetTriplesWithObject(IEnumerable<Uri> graphUris, INode obj)
        {
            return (from g in graphUris.Select(u => this[u])
                    from t in g.GetTriplesWithObject(obj)
                    select t);
        }

        public IEnumerable<Quad> Quads
        {
            get 
            {
                return (from g in this._graphs.Values
                        from q in g.Quads
                        select q);

            }
        }

        public IEnumerable<Quad> GetQuadsWithSubject(INode subj)
        {
            return (from kvp in this._graphs
                    from t in kvp.Value.GetTriplesWithSubject(subj)
                    select t.AsQuad(kvp.Key));
        }

        public IEnumerable<Quad> GetQuadsWithSubjectPredicate(INode subj, INode pred)
        {
            return (from kvp in this._graphs
                    from t in kvp.Value.GetTriplesWithSubjectPredicate(subj, pred)
                    select t.AsQuad(kvp.Key));
        }

        public IEnumerable<Quad> GetQuadsWithSubjectObject(INode subj, INode obj)
        {
            return (from kvp in this._graphs
                    from t in kvp.Value.GetTriplesWithSubjectObject(subj, obj)
                    select t.AsQuad(kvp.Key));
        }

        public IEnumerable<Quad> GetQuadsWithPredicate(INode pred)
        {
            return (from kvp in this._graphs
                    from t in kvp.Value.GetTriplesWithPredicate(pred)
                    select t.AsQuad(kvp.Key));
        }

        public IEnumerable<Quad> GetQuadsWithPredicateObject(INode pred, INode obj)
        {
            return (from kvp in this._graphs
                    from t in kvp.Value.GetTriplesWithPredicateObject(pred, obj)
                    select t.AsQuad(kvp.Key));
        }

        public IEnumerable<Quad> GetQuadsWithObject(INode obj)
        {
            return (from kvp in this._graphs
                    from t in kvp.Value.GetTriplesWithObject(obj)
                    select t.AsQuad(kvp.Key));
        }

        public bool Contains(Triple t)
        {
            return this._graphs.Values.Any(g => g.ContainsTriple(t));
        }

        public bool Contains(IEnumerable<Uri> graphUris, Triple t)
        {
            return (from u in graphUris
                    select this[u].ContainsTriple(t)).Any();
        }

        public bool Contains(Quad q)
        {
            if (this.HasGraph(q.Graph))
            {
                return this[q.Graph].ContainsTriple(q.AsTriple());
            }
            else
            {
                return false;
            }
        }
    }
}
