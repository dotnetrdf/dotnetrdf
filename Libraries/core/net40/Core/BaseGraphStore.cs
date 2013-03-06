using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Collections;

namespace VDS.RDF.Core
{
    public abstract class BaseGraphStore
        : IGraphStore
    {
        protected readonly IGraphCollection _graphs;

        public BaseGraphStore()
            : this(new GraphCollection()) { }

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
            throw new NotImplementedException();
        }

        public bool Remove(IGraph g)
        {
            return this.Remove(g.BaseUri, g);
        }

        public bool Remove(Uri graphUri, IGraph g)
        {
            if (this.HasGraph(graphUri))
            {
                //foreach (Triple t in this.GetTr
                throw new NotImplementedException();
            }
            else
            {
                return false;
            }
        }

        public bool Remove(Uri graphUri)
        {
            throw new NotImplementedException();
        }

        public bool Remove(Uri graphUri, Triple t)
        {
            throw new NotImplementedException();
        }

        public bool Remove(Quad q)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Triple> Triples
        {
            get { throw new NotImplementedException(); }
        }

        public IEnumerable<Triple> GetTriplesWithSubject(IEnumerable<Uri> graphUris, INode subj)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Triple> GetTriplesWithSubjectPredicate(IEnumerable<Uri> graphUris, INode subj, INode pred)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Triple> GetTriplesWithSubjectObject(IEnumerable<Uri> graphUris, INode subj, INode obj)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Triple> GetTriplesWithPredicate(IEnumerable<Uri> graphUris, INode pred)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Triple> GetTriplesWithPredicateObject(IEnumerable<Uri> graphUris, INode pred, INode obj)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Triple> GetTriplesWithObject(IEnumerable<Uri> graphUris, INode obj)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Quad> Quads
        {
            get { throw new NotImplementedException(); }
        }

        public IEnumerable<Quad> GetQuadsWithSubject(INode subj)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Quad> GetQuadsWithSubjectPredicate(INode subj, INode pred)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Quad> GetQuadsWithSubjectObject(INode subj)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Quad> GetQuadsWithPredicate(INode subj, INode obj)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Quad> GetQuadsWithPredicateObject(INode pred, INode obj)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Quad> GetQuadsWithObject(INode obj)
        {
            throw new NotImplementedException();
        }

        public bool Contains(Triple t)
        {
            throw new NotImplementedException();
        }

        public bool Contains(IEnumerable<Uri> graphUris, Triple t)
        {
            throw new NotImplementedException();
        }

        public bool Contains(Quad q)
        {
            throw new NotImplementedException();
        }
    }
}
