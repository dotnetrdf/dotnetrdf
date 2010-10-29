/*

Copyright Robert Vesse 2009-10
rvesse@vdesign-studios.com

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

namespace VDS.RDF.Query.Datasets
{
    public class InMemoryDataset : BaseDataset
    {
        private IInMemoryQueryableStore _store;

        public InMemoryDataset(IInMemoryQueryableStore store)
        {
            this._store = store;
        }

        #region Graph Existence and Retrieval

        public override void AddGraph(IGraph g)
        {
            this._store.Add(g, true);
        }

        public override void RemoveGraph(Uri graphUri)
        {
            this._store.Remove(graphUri);
        }

        public override bool HasGraph(Uri graphUri)
        {
            return this._store.HasGraph(graphUri);
        }

        public override IEnumerable<IGraph> Graphs
        {
            get 
            {
                return this._store.Graphs; 
            }
        }

        public override IEnumerable<Uri> GraphUris
        {
            get 
            {
                return (from g in this._store.Graphs
                        select g.BaseUri);
            }
        }

        public override IGraph this[Uri graphUri]
        {
            get 
            {
                return this._store.Graph(graphUri); 
            }
        }

        #endregion

        #region Triple Existence and Retrieval

        public override bool ContainsTriple(Triple t)
        {
            return this._store.Contains(t);
        }

        protected override IEnumerable<Triple> GetAllTriples()
        {
            return this._store.Triples;
        }

        public override IEnumerable<Triple> GetTriplesWithSubject(INode subj)
        {
            if (this._activeGraph == null)
            {
                if (this._defaultGraph == null)
                {
                    return (from g in this.Graphs
                            from t in g.GetTriplesWithSubject(subj)
                            select t);
                }
                else
                {
                    return this._defaultGraph.GetTriplesWithSubject(subj);
                }
            }
            else
            {
                return this._activeGraph.GetTriplesWithSubject(subj);
            }
        }

        public override IEnumerable<Triple> GetTriplesWithPredicate(INode pred)
        {
            if (this._activeGraph == null)
            {
                if (this._defaultGraph == null)
                {
                    return (from g in this.Graphs
                            from t in g.GetTriplesWithPredicate(pred)
                            select t);
                }
                else
                {
                    return this._defaultGraph.GetTriplesWithPredicate(pred);
                }
            }
            else
            {
                return this._activeGraph.GetTriplesWithPredicate(pred);
            }
        }

        public override IEnumerable<Triple> GetTriplesWithObject(INode obj)
        {
            if (this._activeGraph == null)
            {
                if (this._defaultGraph == null)
                {
                    return (from g in this.Graphs
                            from t in g.GetTriplesWithObject(obj)
                            select t);
                }
                else
                {
                    return this._defaultGraph.GetTriplesWithObject(obj);
                }
            }
            else
            {
                return this._activeGraph.GetTriplesWithObject(obj);
            }
        }

        public override IEnumerable<Triple> GetTriplesWithSubjectPredicate(INode subj, INode pred)
        {
            if (this._activeGraph == null)
            {
                if (this._defaultGraph == null)
                {
                    return (from g in this.Graphs
                            from t in g.GetTriplesWithSubjectPredicate(subj, pred)
                            select t);
                }
                else
                {
                    return this._defaultGraph.GetTriplesWithSubjectPredicate(subj, pred);
                }
            }
            else
            {
                return this._activeGraph.GetTriplesWithSubjectPredicate(subj, pred);
            }
        }

        public override IEnumerable<Triple> GetTriplesWithSubjectObject(INode subj, INode obj)
        {
            if (this._activeGraph == null)
            {
                if (this._defaultGraph == null)
                {
                    return (from g in this.Graphs
                            from t in g.GetTriplesWithSubjectObject(subj, obj)
                            select t);
                }
                else
                {
                    return this._defaultGraph.GetTriplesWithSubjectObject(subj, obj);
                }
            }
            else
            {
                return this._activeGraph.GetTriplesWithSubjectObject(subj, obj);
            }
        }

        public override IEnumerable<Triple> GetTriplesWithPredicateObject(INode pred, INode obj)
        {
            if (this._activeGraph == null)
            {
                if (this._defaultGraph == null)
                {
                    return (from g in this.Graphs
                            from t in g.GetTriplesWithPredicateObject(pred, obj)
                            select t);
                }
                else
                {
                    return this._defaultGraph.GetTriplesWithPredicateObject(pred, obj);
                }
            }
            else
            {
                return this._activeGraph.GetTriplesWithPredicateObject(pred, obj);
            }
        }

        #endregion

        public override void Flush()
        {
            if (this._store is IFlushableStore)
            {
                ((IFlushableStore)this._store).Flush();
            }
        }
    }
}
