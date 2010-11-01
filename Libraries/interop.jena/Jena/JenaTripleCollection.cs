using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.hp.hpl.jena.rdf.model;

namespace VDS.RDF.Interop.Jena
{
    class JenaTripleCollection : BaseTripleCollection
    {
        private Model _m;
        private JenaMapping _mapping;

        public JenaTripleCollection(IGraph g, Model m)
        {
            this._m = m;
            this._mapping = new JenaMapping(g, this._m);
        }

        protected override void Add(Triple t)
        {
            this._m.add(JenaConverter.ToJena(t, this._mapping));
        }

        public override bool Contains(Triple t)
        {
            return this._m.contains(JenaConverter.ToJena(t, this._mapping));
        }

        public override int Count
        {
            get 
            {
                StmtIterator iter = this._m.listStatements();
                int i = 0;
                while (iter.hasNext())
                {
                    i++;
                    iter.next();
                }
                return i;
            }
        }

        protected override void Delete(Triple t)
        {
            this._m.remove(JenaConverter.ToJena(t, this._mapping));
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
                    throw new KeyNotFoundException("The given Triple is not present in this Triple Collection");
                }
            }
        }

        public override IEnumerable<INode> ObjectNodes
        {
            get 
            {
                throw new NotImplementedException();
            }
        }

        public override IEnumerable<INode> PredicateNodes
        {
            get { throw new NotImplementedException(); }
        }

        public override IEnumerable<INode> SubjectNodes
        {
            get { throw new NotImplementedException(); }
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }

        public override IEnumerator<Triple> GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
