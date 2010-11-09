using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dotSesame = org.openrdf.model;

namespace VDS.RDF.Interop.Sesame
{
    public class SesameTripleCollection : BaseTripleCollection
    {
        private dotSesame.Graph _g;
        private SesameMapping _mapping;

        public SesameTripleCollection(dotSesame.Graph g, SesameMapping mapping)
        {
            this._g = g;
            this._mapping = mapping;
        }

        protected override void Add(Triple t)
        {
            this._g.add(SesameConverter.ToSesame(t, this._mapping));
        }

        public override bool Contains(Triple t)
        {
            return this._g.contains(SesameConverter.ToSesame(t, this._mapping));
        }

        public override int Count
        {
            get 
            {
                return this._g.size();
            }
        }

        protected override void Delete(Triple t)
        {
            this._g.remove(SesameConverter.ToSesame(t, this._mapping));
        }

        public override Triple this[Triple t]
        {
            get 
            {
                if (this.Contains(t))
                {
                    return t;
                }
                else
                {
                    throw new KeyNotFoundException("The given Triple does not exist in this Triple Collection");
                }
            }
        }

        public override IEnumerable<INode> ObjectNodes
        {
            get 
            {
                JavaIteratorWrapper<dotSesame.Statement> stmtIter = new JavaIteratorWrapper<org.openrdf.model.Statement>(this._g.iterator());
                return (from s in stmtIter
                        select SesameConverter.FromSesameValue(s.getObject(), this._mapping)).Distinct();
            }
        }

        public override IEnumerable<INode> PredicateNodes
        {
            get 
            {
                JavaIteratorWrapper<dotSesame.Statement> stmtIter = new JavaIteratorWrapper<org.openrdf.model.Statement>(this._g.iterator());
                return (from s in stmtIter
                        select SesameConverter.FromSesameUri(s.getPredicate(), this._mapping)).Distinct();
            }
        }

        public override IEnumerable<INode> SubjectNodes
        {
            get 
            {
                JavaIteratorWrapper<dotSesame.Statement> stmtIter = new JavaIteratorWrapper<org.openrdf.model.Statement>(this._g.iterator());
                return (from s in stmtIter
                        select SesameConverter.FromSesameResource(s.getSubject(), this._mapping)).Distinct(); 
            }
        }

        public override void Dispose()
        {
            //Do Nothing
        }

        public override IEnumerator<Triple> GetEnumerator()
        {
            JavaIteratorWrapper<dotSesame.Statement> stmtIter = new JavaIteratorWrapper<org.openrdf.model.Statement>(this._g.iterator());
            return stmtIter.Select(s => SesameConverter.FromSesame(s, this._mapping)).GetEnumerator();
        }

        public override IEnumerable<Triple> WithObject(INode obj)
        {
            dotSesame.Value v = SesameConverter.ToSesameValue(obj, this._mapping);
            JavaIteratorWrapper<dotSesame.Statement> stmtIter = new JavaIteratorWrapper<org.openrdf.model.Statement>(this._g.match(null, null, v, null));
            return stmtIter.Select(s => SesameConverter.FromSesame(s, this._mapping));
        }

        public override IEnumerable<Triple> WithPredicate(INode pred)
        {
            dotSesame.URI u = SesameConverter.ToSesameUri(pred, this._mapping);
            JavaIteratorWrapper<dotSesame.Statement> stmtIter = new JavaIteratorWrapper<org.openrdf.model.Statement>(this._g.match(null, u, null, null));
            return stmtIter.Select(s => SesameConverter.FromSesame(s, this._mapping));
        }

        public override IEnumerable<Triple> WithPredicateObject(INode pred, INode obj)
        {
            dotSesame.URI u = SesameConverter.ToSesameUri(pred, this._mapping);
            dotSesame.Value v = SesameConverter.ToSesameValue(obj, this._mapping);
            JavaIteratorWrapper<dotSesame.Statement> stmtIter = new JavaIteratorWrapper<org.openrdf.model.Statement>(this._g.match(null, u, v, null));
            return stmtIter.Select(s => SesameConverter.FromSesame(s, this._mapping));
        }

        public override IEnumerable<Triple> WithSubject(INode subj)
        {
            dotSesame.Resource r = SesameConverter.ToSesameResource(subj, this._mapping);
            JavaIteratorWrapper<dotSesame.Statement> stmtIter = new JavaIteratorWrapper<org.openrdf.model.Statement>(this._g.match(r, null, null, null));
            return stmtIter.Select(s => SesameConverter.FromSesame(s, this._mapping));
        }

        public override IEnumerable<Triple> WithSubjectObject(INode subj, INode obj)
        {
            dotSesame.Resource r = SesameConverter.ToSesameResource(subj, this._mapping);
            dotSesame.Value v = SesameConverter.ToSesameValue(obj, this._mapping);
            JavaIteratorWrapper<dotSesame.Statement> stmtIter = new JavaIteratorWrapper<org.openrdf.model.Statement>(this._g.match(r, null, v, null));
            return stmtIter.Select(s => SesameConverter.FromSesame(s, this._mapping));
        }

        public override IEnumerable<Triple> WithSubjectPredicate(INode subj, INode pred)
        {
            dotSesame.Resource r = SesameConverter.ToSesameResource(subj, this._mapping);
            dotSesame.URI u = SesameConverter.ToSesameUri(pred, this._mapping);
            JavaIteratorWrapper<dotSesame.Statement> stmtIter = new JavaIteratorWrapper<org.openrdf.model.Statement>(this._g.match(r, u, null, null));
            return stmtIter.Select(s => SesameConverter.FromSesame(s, this._mapping));
        }
    }
}
