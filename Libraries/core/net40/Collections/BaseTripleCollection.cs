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
using VDS.RDF.Graphs;
using VDS.RDF.Nodes;

namespace VDS.RDF.Collections
{
    /// <summary>
    /// Abstract Base Class for Triple Collections
    /// </summary>
    public abstract class BaseTripleCollection
        : ITripleCollection
    {
        /// <summary>
        /// Adds a Triple to the Collection
        /// </summary>
        /// <param name="t">Triple to add</param>
        /// <remarks>Adding a Triple that already exists should be permitted though it is not necessary to persist the duplicate to underlying storage</remarks>
        public abstract bool Add(Triple t);

        /// <summary>
        /// Adds a range of triples to the collection
        /// </summary>
        /// <param name="ts">Triples to add</param>
        /// <returns>True if any triples are added</returns>
        public virtual bool AddRange(IEnumerable<Triple> ts)
        {
            this.StartBatch(NotifyCollectionChangedAction.Add);
            bool added = false;
            foreach (Triple t in ts)
            {
                added = this.Add(t) || added;
            }
            this.EndBatch();
            return added;
        }

        /// <summary>
        /// Determines whether a given Triple is in the collection
        /// </summary>
        /// <param name="t">The Triple to test</param>
        /// <returns>True if the Triple is contained in the collection</returns>
        public abstract bool Contains(Triple t);

        /// <summary>
        /// Gets the Number of Triples in the Triple Collection
        /// </summary>
        public abstract long Count 
        { 
            get; 
        }

        /// <summary>
        /// Removes a Triple from the Collection
        /// </summary>
        /// <param name="t">Triple to remove</param>
        public abstract bool Remove(Triple t);

        /// <summary>
        /// Removes a range of Triples from the Collection
        /// </summary>
        /// <param name="ts">Triples to remove</param>
        /// <returns>True if any triples are removed, false otherwise</returns>
        public virtual bool RemoveRange(IEnumerable<Triple> ts)
        {
            this.StartBatch(NotifyCollectionChangedAction.Remove);
            bool removed = false;
            foreach (Triple t in ts)
            {
                removed = this.Remove(t) || removed;
            }
            this.EndBatch();
            return removed;
        }

        /// <summary>
        /// Clears the collection
        /// </summary>
        public abstract void Clear();

        /// <summary>
        /// Gets all the Nodes which are Objects of Triples in the Triple Collection
        /// </summary>
        public abstract IEnumerable<INode> ObjectNodes 
        { 
            get; 
        }

        /// <summary>
        /// Gets all the Nodes which are Predicates of Triples in the Triple Collection
        /// </summary>
        public abstract IEnumerable<INode> PredicateNodes 
        {
            get; 
        }

        /// <summary>
        /// Gets all the Nodes which are Subjects of Triples in the Triple Collection
        /// </summary>
        public abstract IEnumerable<INode> SubjectNodes 
        { 
            get;
        }

        /// <summary>
        /// Gets all the Triples with the given Subject
        /// </summary>
        /// <param name="subj">ubject to lookup</param>
        /// <returns></returns>
        public virtual IEnumerable<Triple> WithSubject(INode subj)
        {
            return (from t in this
                    where t.Subject.Equals(subj)
                    select t);
        }

        /// <summary>
        /// Gets all the Triples with the given Predicate
        /// </summary>
        /// <param name="pred">Predicate to lookup</param>
        /// <returns></returns>
        public virtual IEnumerable<Triple> WithPredicate(INode pred)
        {
            return (from t in this
                    where t.Predicate.Equals(pred)
                    select t);
        }

        /// <summary>
        /// Gets all the Triples with the given Object
        /// </summary>
        /// <param name="obj">Object to lookup</param>
        /// <returns></returns>
        public virtual IEnumerable<Triple> WithObject(INode obj)
        {
            return (from t in this
                    where t.Object.Equals(obj)
                    select t);
        }

        /// <summary>
        /// Gets all the Triples with the given Subject Predicate pair
        /// </summary>
        /// <param name="subj">Subject to lookup</param>
        /// <param name="pred">Predicate to lookup</param>
        /// <returns></returns>
        public virtual IEnumerable<Triple> WithSubjectPredicate(INode subj, INode pred)
        {
            return (from t in this.WithSubject(subj)
                    where t.Predicate.Equals(pred)
                    select t);
        }

        /// <summary>
        /// Gets all the Triples with the given Predicate Object pair
        /// </summary>
        /// <param name="pred">Predicate to lookup</param>
        /// <param name="obj">Object to lookup</param>
        /// <returns></returns>
        public virtual IEnumerable<Triple> WithPredicateObject(INode pred, INode obj)
        {
            return (from t in this.WithPredicate(pred)
                    where t.Object.Equals(obj)
                    select t);
        }

        /// <summary>
        /// Gets all the Triples with the given Subject Object pair
        /// </summary>
        /// <param name="subj">Subject to lookup</param>
        /// <param name="obj">Object to lookup</param>
        /// <returns></returns>
        public virtual IEnumerable<Triple> WithSubjectObject(INode subj, INode obj)
        {
            return (from t in this.WithSubject(subj)
                    where t.Object.Equals(obj)
                    select t);
        }

        /// <summary>
        /// Diposes of a Triple Collection
        /// </summary>
        public abstract void Dispose();

        /// <summary>
        /// Gets the typed Enumerator for the Triple Collection
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerator<Triple> GetEnumerator();

        /// <summary>
        /// Gets the non-generic Enumerator for the Triple Collection
        /// </summary>
        /// <returns></returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        private bool InBatchOperation { get; set; }

        private List<Triple> CurrentBatch { get; set; }

        private NotifyCollectionChangedAction BatchAction { get; set; }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>
        /// Signals the start of a batch add/remove operation.  Single triple events will be suppressed and queued up until <see cref="EndBatch()" /> is called
        /// </summary>
        /// <param name="action">Action, must be add/remove</param>
        protected void StartBatch(NotifyCollectionChangedAction action)
        {
            if (this.InBatchOperation) throw new InvalidOperationException("A batch operation is already ongoing");
            if (action != NotifyCollectionChangedAction.Add && action != NotifyCollectionChangedAction.Remove) throw new InvalidOperationException("Batch operations can only be used for adds/removes");

            this.InBatchOperation = true;
            this.CurrentBatch = new List<Triple>();
            this.BatchAction = action;
        }

        /// <summary>
        /// Signals the end of a batch add/remove operation, queued events will now fire as a single event
        /// </summary>
        protected void EndBatch()
        {
            if (!this.InBatchOperation) throw new InvalidOperationException("No batch operation was started");

            this.RaiseCollectionChanged(this.BatchAction);
            this.InBatchOperation = false;
            this.CurrentBatch = null;
        }

        protected void RaiseCollectionChanged(Object sender, NotifyCollectionChangedEventArgs args)
        {
            NotifyCollectionChangedEventHandler d = this.CollectionChanged;
            if (d != null)
            {
                d(sender, args);
            }
        }

        private void RaiseCollectionChanged(NotifyCollectionChangedAction action, Triple t)
        {
            NotifyCollectionChangedEventHandler d = this.CollectionChanged;
            if (d != null)
            {
                d(this, new NotifyCollectionChangedEventArgs(action, t));
            }
        }

        protected void RaiseCollectionChanged(NotifyCollectionChangedAction action)
        {
            NotifyCollectionChangedEventHandler d = this.CollectionChanged;
            if (d != null)
            {
                if (this.InBatchOperation)
                {
                    d(this, new NotifyCollectionChangedEventArgs(action, this.CurrentBatch));
                }
                else
                {
                    d(this, new NotifyCollectionChangedEventArgs(action));
                }
            }
        }

        protected void RaiseTripleAdded(Triple t)
        {
            if (this.InBatchOperation)
            {
                this.CurrentBatch.Add(t);
            }
            else
            {
                this.RaiseCollectionChanged(NotifyCollectionChangedAction.Add, t);
            }
        }

        protected void RaiseTripleRemoved(Triple t)
        {
            if (this.InBatchOperation)
            {
                this.CurrentBatch.Add(t);
            }
            else
            {
                this.RaiseCollectionChanged(NotifyCollectionChangedAction.Remove, t);
            }
        }
    }
}
