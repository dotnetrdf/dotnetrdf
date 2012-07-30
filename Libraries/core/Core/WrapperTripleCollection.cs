/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

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
    /// Abstract decorator for Triple Collections to make it easier to add additional functionality to existing collections
    /// </summary>
    public abstract class WrapperTripleCollection
        : BaseTripleCollection
    {
        /// <summary>
        /// Underlying Triple Collection
        /// </summary>
        protected readonly BaseTripleCollection _triples;

        /// <summary>
        /// Creates a new decorator over the default <see cref="TreeIndexedTripleCollection"/>
        /// </summary>
        public WrapperTripleCollection()
            : this(new TreeIndexedTripleCollection()) { }

        /// <summary>
        /// Creates a new decorator around the given triple collection
        /// </summary>
        /// <param name="tripleCollection">Triple Collection</param>
        public WrapperTripleCollection(BaseTripleCollection tripleCollection)
        {
            if (tripleCollection == null) throw new ArgumentNullException("tripleCollection");
            this._triples = tripleCollection;
            this._triples.TripleAdded += this.HandleTripleAdded;
            this._triples.TripleRemoved += this.HandleTripleRemoved;
        }

        private void HandleTripleAdded(Object sender, TripleEventArgs args)
        {
            this.RaiseTripleAdded(args.Triple);
        }

        private void HandleTripleRemoved(Object sender, TripleEventArgs args)
        {
            this.RaiseTripleRemoved(args.Triple);
        }

        /// <summary>
        /// Adds a triple to the collection
        /// </summary>
        /// <param name="t">Triple</param>
        /// <returns></returns>
        protected internal override bool Add(Triple t)
        {
            return this._triples.Add(t);
        }

        /// <summary>
        /// Gets whether the collection contains the given Triple
        /// </summary>
        /// <param name="t">Triple</param>
        /// <returns></returns>
        public override bool Contains(Triple t)
        {
            return this._triples.Contains(t);
        }

        /// <summary>
        /// Counts the triples in the collection
        /// </summary>
        public override int Count
        {
            get
            {
                return this._triples.Count; 
            }
        }

        /// <summary>
        /// Deletes a triple from the collection
        /// </summary>
        /// <param name="t">Triple</param>
        /// <returns></returns>
        protected internal override bool Delete(Triple t)
        {
            return this._triples.Delete(t);
        }

        /// <summary>
        /// Gets the specific instance of a Triple from the collection
        /// </summary>
        /// <param name="t">Triple</param>
        /// <returns></returns>
        public override Triple this[Triple t]
        {
            get
            {
                return this._triples[t];
            }
        }

        /// <summary>
        /// Gets the object nodes
        /// </summary>
        public override IEnumerable<INode> ObjectNodes
        {
            get 
            {
                return this._triples.ObjectNodes;
            }
        }

        /// <summary>
        /// Gets the predicate nodes
        /// </summary>
        public override IEnumerable<INode> PredicateNodes
        {
            get 
            {
                return this._triples.PredicateNodes; 
            }
        }

        /// <summary>
        /// Gets the subject nodes
        /// </summary>
        public override IEnumerable<INode> SubjectNodes
        {
            get 
            {
                return this._triples.SubjectNodes;
            }
        }

        /// <summary>
        /// Disposes of the collection
        /// </summary>
        public override void Dispose()
        {
            this._triples.Dispose();
        }

        /// <summary>
        /// Gets the enumerator for the collection
        /// </summary>
        /// <returns></returns>
        public override IEnumerator<Triple> GetEnumerator()
        {
            return this._triples.GetEnumerator();
        }

        /// <summary>
        /// Gets all the triples with the given object
        /// </summary>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        public override IEnumerable<Triple> WithObject(INode obj)
        {
            return this._triples.WithObject(obj);
        }

        /// <summary>
        /// Gets all the triples with the given predicate
        /// </summary>
        /// <param name="pred">Predicate</param>
        /// <returns></returns>
        public override IEnumerable<Triple> WithPredicate(INode pred)
        {
            return this._triples.WithPredicate(pred);
        }

        /// <summary>
        /// Gets all the triples with the given predicate and object
        /// </summary>
        /// <param name="pred">Predicate</param>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        public override IEnumerable<Triple> WithPredicateObject(INode pred, INode obj)
        {
            return this._triples.WithPredicateObject(pred, obj);
        }

        /// <summary>
        /// Gets all the triples with the given subject
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <returns></returns>
        public override IEnumerable<Triple> WithSubject(INode subj)
        {
            return this._triples.WithSubject(subj);
        }

        /// <summary>
        /// Gets all the triples with the given subject and object
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        public override IEnumerable<Triple> WithSubjectObject(INode subj, INode obj)
        {
            return this._triples.WithSubjectObject(subj, obj);
        }

        /// <summary>
        /// Gets all the triples with the given subject and predicate
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <param name="pred">Predicate</param>
        /// <returns></returns>
        public override IEnumerable<Triple> WithSubjectPredicate(INode subj, INode pred)
        {
            return this._triples.WithSubjectPredicate(subj, pred);
        }
    }
}
