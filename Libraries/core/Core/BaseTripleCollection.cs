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

namespace VDS.RDF
{
    /// <summary>
    /// Abstract Base Class for Triple Collections
    /// </summary>
    /// <remarks>
    /// Designed to allow the underlying storage of a Triple Collection to be changed at a later date without affecting classes that use it.
    /// </remarks>
    public abstract class BaseTripleCollection : IEnumerable<Triple>, IDisposable
    {
        /// <summary>
        /// Adds a Triple to the Collection
        /// </summary>
        /// <param name="t">Triple to add</param>
        /// <remarks>Adding a Triple that already exists should be permitted though it is not necessary to persist the duplicate to underlying storage</remarks>
        protected abstract internal void Add(Triple t);

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
        protected abstract internal void Delete(Triple t);

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
            TripleEventHandler d = this.TripleAdded;
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
            TripleEventHandler d = this.TripleRemoved;
            if (d != null)
            {
                d(this, new TripleEventArgs(t, null, false));
            }
        }
    }
}
