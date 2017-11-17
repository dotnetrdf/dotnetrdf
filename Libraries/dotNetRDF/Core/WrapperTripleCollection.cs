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
            _triples = tripleCollection;
            _triples.TripleAdded += HandleTripleAdded;
            _triples.TripleRemoved += HandleTripleRemoved;
        }

        private void HandleTripleAdded(Object sender, TripleEventArgs args)
        {
            RaiseTripleAdded(args.Triple);
        }

        private void HandleTripleRemoved(Object sender, TripleEventArgs args)
        {
            RaiseTripleRemoved(args.Triple);
        }

        /// <summary>
        /// Adds a triple to the collection
        /// </summary>
        /// <param name="t">Triple</param>
        /// <returns></returns>
        protected internal override bool Add(Triple t)
        {
            return _triples.Add(t);
        }

        /// <summary>
        /// Gets whether the collection contains the given Triple
        /// </summary>
        /// <param name="t">Triple</param>
        /// <returns></returns>
        public override bool Contains(Triple t)
        {
            return _triples.Contains(t);
        }

        /// <summary>
        /// Counts the triples in the collection
        /// </summary>
        public override int Count
        {
            get
            {
                return _triples.Count; 
            }
        }

        /// <summary>
        /// Deletes a triple from the collection
        /// </summary>
        /// <param name="t">Triple</param>
        /// <returns></returns>
        protected internal override bool Delete(Triple t)
        {
            return _triples.Delete(t);
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
                return _triples[t];
            }
        }

        /// <summary>
        /// Gets the object nodes
        /// </summary>
        public override IEnumerable<INode> ObjectNodes
        {
            get 
            {
                return _triples.ObjectNodes;
            }
        }

        /// <summary>
        /// Gets the predicate nodes
        /// </summary>
        public override IEnumerable<INode> PredicateNodes
        {
            get 
            {
                return _triples.PredicateNodes; 
            }
        }

        /// <summary>
        /// Gets the subject nodes
        /// </summary>
        public override IEnumerable<INode> SubjectNodes
        {
            get 
            {
                return _triples.SubjectNodes;
            }
        }

        /// <summary>
        /// Disposes of the collection
        /// </summary>
        public override void Dispose()
        {
            _triples.Dispose();
        }

        /// <summary>
        /// Gets the enumerator for the collection
        /// </summary>
        /// <returns></returns>
        public override IEnumerator<Triple> GetEnumerator()
        {
            return _triples.GetEnumerator();
        }

        /// <summary>
        /// Gets all the triples with the given object
        /// </summary>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        public override IEnumerable<Triple> WithObject(INode obj)
        {
            return _triples.WithObject(obj);
        }

        /// <summary>
        /// Gets all the triples with the given predicate
        /// </summary>
        /// <param name="pred">Predicate</param>
        /// <returns></returns>
        public override IEnumerable<Triple> WithPredicate(INode pred)
        {
            return _triples.WithPredicate(pred);
        }

        /// <summary>
        /// Gets all the triples with the given predicate and object
        /// </summary>
        /// <param name="pred">Predicate</param>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        public override IEnumerable<Triple> WithPredicateObject(INode pred, INode obj)
        {
            return _triples.WithPredicateObject(pred, obj);
        }

        /// <summary>
        /// Gets all the triples with the given subject
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <returns></returns>
        public override IEnumerable<Triple> WithSubject(INode subj)
        {
            return _triples.WithSubject(subj);
        }

        /// <summary>
        /// Gets all the triples with the given subject and object
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        public override IEnumerable<Triple> WithSubjectObject(INode subj, INode obj)
        {
            return _triples.WithSubjectObject(subj, obj);
        }

        /// <summary>
        /// Gets all the triples with the given subject and predicate
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <param name="pred">Predicate</param>
        /// <returns></returns>
        public override IEnumerable<Triple> WithSubjectPredicate(INode subj, INode pred)
        {
            return _triples.WithSubjectPredicate(subj, pred);
        }
    }
}
