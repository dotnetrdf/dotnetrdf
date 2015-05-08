/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

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
using System.Collections.Specialized;
using System.Linq;
using VDS.Common.Collections;
using VDS.RDF.Graphs;
using VDS.RDF.Nodes;

namespace VDS.RDF.Collections
{
    /// <summary>
    /// Basic Triple Collection which is not indexed
    /// </summary>
    public class TripleCollection 
        : BaseTripleCollection, IEnumerable<Triple>
    {
        /// <summary>
        /// Underlying Storage of the Triple Collection
        /// </summary>
        protected readonly MultiDictionary<Triple, Object> _triples = new MultiDictionary<Triple, object>(new FullTripleComparer(new FastNodeComparer()));

        public override bool CanModifyDuringIteration
        {
            get { return true; }
        }

        public override bool HasIndexes
        {
            get { return false; }
        }

        /// <summary>
        /// Determines whether a given Triple is in the Triple Collection
        /// </summary>
        /// <param name="t">The Triple to test</param>
        /// <returns>True if the Triple already exists in the Triple Collection</returns>
        public override bool Contains(Triple t)
        {
            return this._triples.ContainsKey(t);
        }

        /// <summary>
        /// Adds a Triple to the Collection
        /// </summary>
        /// <param name="t">Triple to add</param>
        public override bool Add(Triple t)
        {
            if (this.Contains(t)) return false;

            this._triples.Add(t, null);
            this.RaiseTripleAdded(t);
            return true;
        }

        /// <summary>
        /// Deletes a Triple from the Colleciton
        /// </summary>
        /// <param name="t">Triple to remove</param>
        /// <remarks>Deleting something that doesn't exist has no effect and gives no error</remarks>
        public override bool Remove(Triple t)
        {
            if (!this._triples.Remove(t)) return false;

            this.RaiseTripleRemoved(t);
            return true;
        }

        public override void Clear()
        {
            this._triples.Clear();
            this.RaiseCollectionChanged(NotifyCollectionChangedAction.Reset);
        }

        /// <summary>
        /// Gets the Number of Triples in the Triple Collection
        /// </summary>
        public override long Count
        {
            get
            {
                return this._triples.Count;
            }
        }

        /// <summary>
        /// Gets all the Nodes which are Subjects of Triples in the Triple Collection
        /// </summary>
        public override IEnumerable<INode> SubjectNodes
        {
            get
            {
                IEnumerable<INode> ns = from t in this
                                        select t.Subject;

                return ns.Distinct();
            }
        }

        /// <summary>
        /// Gets all the Nodes which are Predicates of Triples in the Triple Collection
        /// </summary>
        public override IEnumerable<INode> PredicateNodes
        {
            get
            {
                IEnumerable<INode> ns = from t in this
                                        select t.Predicate;

                return ns.Distinct();
            }
        }

        /// <summary>
        /// Gets all the Nodes which are Objects of Triples in the Triple Collectio
        /// </summary>
        public override IEnumerable<INode> ObjectNodes
        {
            get
            {
                IEnumerable<INode> ns = from t in this
                                        select t.Object;

                return ns.Distinct();
            }
        }

        #region IEnumerable<Triple> Members

        /// <summary>
        /// Gets the Enumerator for the Collection
        /// </summary>
        /// <returns></returns>
        public override IEnumerator<Triple> GetEnumerator()
        {
            return this._triples.Keys.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Gets the Enumerator for the Collection
        /// </summary>
        /// <returns></returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Disposes of a Triple Collection
        /// </summary>
        public override void Dispose()
        {
            //No unmanaged resources to dispose of
        }

        #endregion
    }
}
