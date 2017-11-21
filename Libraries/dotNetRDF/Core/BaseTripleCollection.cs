/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/

using System;
using System.Collections.Generic;
using System.Linq;

namespace VDS.RDF
{
    /// <summary>
    /// Abstract Base Class for Triple Collections
    /// </summary>
    /// <remarks>
    /// Designed to allow the underlying storage of a Triple Collection to be changed at a later date without affecting classes that use it.
    /// </remarks>
    public abstract class BaseTripleCollection
        : IEnumerable<Triple>, IDisposable
    {
        /// <summary>
        /// Adds a Triple to the Collection
        /// </summary>
        /// <param name="t">Triple to add</param>
        /// <remarks>Adding a Triple that already exists should be permitted though it is not necessary to persist the duplicate to underlying storage</remarks>
        protected abstract internal bool Add(Triple t);

        /// <summary>
        /// Determines whether a given Triple is in the Triple Collection
        /// </summary>
        /// <param name="t">The Triple to test</param>
        /// <returns>True if the Triple already exists in the Triple Collection</returns>
        public abstract bool Contains(Triple t);

        /// <summary>
        /// Gets the Number of Triples in the Triple Collection
        /// </summary>
        public abstract int Count 
        { 
            get; 
        }

        /// <summary>
        /// Deletes a Triple from the Collection
        /// </summary>
        /// <param name="t">Triple to remove</param>
        /// <remarks>Deleting something that doesn't exist should have no effect and give no error</remarks>
        protected abstract internal bool Delete(Triple t);

        /// <summary>
        /// Gets the given Triple
        /// </summary>
        /// <param name="t">Triple to retrieve</param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException">Thrown if the given Triple doesn't exist</exception>
        public abstract Triple this[Triple t]
        {
            get;
        }

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
            return (from t in WithSubject(subj)
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
            return (from t in WithPredicate(pred)
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
            return (from t in WithSubject(subj)
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
            return Enumerable.Empty<Triple>().GetEnumerator();
        }

        /// <summary>
        /// Event which occurs when a Triple is added to the Collection
        /// </summary>
        public event TripleEventHandler TripleAdded;

        /// <summary>
        /// Event which occurs when a Triple is removed from the Collection
        /// </summary>
        public event TripleEventHandler TripleRemoved;

        /// <summary>
        /// Helper method for raising the <see cref="TripleAdded">Triple Added</see> event
        /// </summary>
        /// <param name="t">Triple</param>
        protected void RaiseTripleAdded(Triple t)
        {
            TripleEventHandler d = TripleAdded;
            if (d != null)
            {
                d(this, new TripleEventArgs(t, null));
            }
        }

        /// <summary>
        /// Helper method for raising the <see cref="TripleRemoved">Triple Removed</see> event
        /// </summary>
        /// <param name="t">Triple</param>
        protected void RaiseTripleRemoved(Triple t)
        {
            TripleEventHandler d = TripleRemoved;
            if (d != null)
            {
                d(this, new TripleEventArgs(t, null, false));
            }
        }
    }
}
