using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dotSesame = org.openrdf.model;
using java.util;

namespace VDS.RDF.Interop.Sesame
{
    /// <summary>
    /// Provides a dotNetRDF Graph to Sesame as a Sesame Graph
    /// </summary>
    public class DotNetRdfGraph : dotSesame.Graph
    {
        private IGraph _g;
        private SesameMapping _mapping;
        private DotNetRdfValueFactory _factory;

        public DotNetRdfGraph(IGraph g)
        {
            this._g = g;
            this._mapping = new SesameMapping(this._g, this);
            this._factory = new DotNetRdfValueFactory(this._g);
        }

        #region Graph Members

        public bool add(dotSesame.Resource r, dotSesame.URI uri, dotSesame.Value v, params dotSesame.Resource[] rarr)
        {
            Triple t = new Triple(SesameConverter.FromSesameResource(r, this._mapping), SesameConverter.FromSesameUri(uri, this._mapping), SesameConverter.FromSesameValue(v, this._mapping));
            if (this._g.ContainsTriple(t))
            {
                return false;
            }
            else
            {
                this._g.Assert(t);
                return true;
            }
        }

        public dotSesame.ValueFactory getValueFactory()
        {
            return this._factory;
        }

        public java.util.Iterator match(dotSesame.Resource r, dotSesame.URI uri, dotSesame.Value v, params dotSesame.Resource[] rarr)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Collection Members

        public bool add(object obj)
        {
            if (obj is dotSesame.Statement)
            {
                dotSesame.Statement stmt = (dotSesame.Statement)obj;
                Triple t = SesameConverter.FromSesame(stmt, this._mapping);
                if (this._g.ContainsTriple(t))
                {
                    return false;
                }
                else
                {
                    this._g.Assert(t);
                    return true;
                }
            }
            else if (obj is Triple)
            {
                Triple t = (Triple)obj;
                if (this._g.ContainsTriple(t))
                {
                    return false;
                }
                else
                {
                    this._g.Assert(t);
                    return true;
                }
            }
            else
            {
                return false;
            }
        }

        public bool addAll(java.util.Collection c)
        {
            JavaIteratorWrapper<dotSesame.Statement> stmtIter = new JavaIteratorWrapper<dotSesame.Statement>(c.iterator());
            bool added = false;
            foreach (dotSesame.Statement stmt in stmtIter)
            {
                Triple t = SesameConverter.FromSesame(stmt, this._mapping);
                if (!this._g.ContainsTriple(t))
                {
                    this._g.Assert(t);
                    added = added || true;
                }
            }
            return added;
        }

        public void clear()
        {
            this._g.Clear();
        }

        public bool contains(object obj)
        {
            if (obj is dotSesame.Statement)
            {
                Triple t = SesameConverter.FromSesame((dotSesame.Statement)obj, this._mapping);
                return this._g.ContainsTriple(t);
            }
            else
            {
                return false;
            }
        }

        public bool containsAll(java.util.Collection c)
        {
            JavaIteratorWrapper<dotSesame.Statement> stmtIter = new JavaIteratorWrapper<org.openrdf.model.Statement>(c.iterator());
            bool contains = true;
            foreach (dotSesame.Statement stmt in stmtIter)
            {
                Triple t = SesameConverter.FromSesame(stmt, this._mapping);
                if (!this._g.ContainsTriple(t))
                {
                    contains = false;
                    break;
                }
            }

            return contains;
        }

        public bool equals(object obj)
        {
            if (obj is dotSesame.Graph)
            {
                Graph h = new Graph();
                SesameConverter.FromSesame((dotSesame.Graph)obj, h);

                return this._g.Equals(h);
            }
            else
            {
                return this._g.Equals(obj);
            }
        }

        public int hashCode()
        {
            return this._g.GetHashCode();
        }

        public bool isEmpty()
        {
            return this._g.IsEmpty;
        }

        public java.util.Iterator iterator()
        {
            return new DotNetEnumerableWrapper(this._g.Triples);
        }

        public bool remove(object obj)
        {
            if (obj is dotSesame.Statement)
            {
                Triple t = SesameConverter.FromSesame((dotSesame.Statement)obj, this._mapping);
                if (this._g.ContainsTriple(t))
                {
                    this._g.Retract(t);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public bool removeAll(java.util.Collection c)
        {
            JavaIteratorWrapper<dotSesame.Statement> stmtIter = new JavaIteratorWrapper<org.openrdf.model.Statement>(c.iterator());
            bool removed = false;
            foreach (dotSesame.Statement stmt in stmtIter)
            {
                Triple t = SesameConverter.FromSesame(stmt, this._mapping);
                if (this._g.ContainsTriple(t))
                {
                    this._g.Retract(t);
                    removed = removed || true;
                }
            }

            return removed;
        }

        public bool retainAll(java.util.Collection c)
        {
            throw new NotImplementedException();
        }

        public int size()
        {
            return this._g.Triples.Count;
        }

        public object[] toArray(object[] objarr)
        {
            throw new NotImplementedException();
        }

        public object[] toArray()
        {
            return this._g.Triples.Select(t => SesameConverter.ToSesame(t, this._mapping)).ToArray();
        }

        #endregion
    }
}
