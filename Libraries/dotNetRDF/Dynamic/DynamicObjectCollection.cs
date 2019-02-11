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

namespace VDS.RDF.Dynamic
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using System.Linq.Expressions;

    /// <summary>
    /// Represents a read/write dynamic collection of objects by subject and predicate.
    /// </summary>
    public class DynamicObjectCollection : ICollection<object>, IDynamicMetaObjectProvider
    {
        private readonly DynamicNode subject;
        private readonly INode predicate;

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicObjectCollection"/> class.
        /// </summary>
        /// <param name="subject">The subject to use.</param>
        /// <param name="predicate">The predicate to use.</param>
        /// <exception cref="ArgumentNullException">When <paramref name="subject"/> or <paramref name="predicate"/> are null.</exception>
        public DynamicObjectCollection(DynamicNode subject, INode predicate)
        {
            this.subject = subject ?? throw new ArgumentNullException(nameof(subject));
            this.predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
        }

        /// <summary>
        /// Gets the number of statements with given subject and predicate.
        /// </summary>
        public int Count
        {
            get
            {
                return Objects.Count();
            }
        }

        /// <summary>
        /// Gets a value indicating whether this collection is read only (always false).
        /// </summary>
        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets objects of statements with given subject and predicate.
        /// </summary>
        /// <remarks>Known literal nodes are converted to native primitives, URI and blank nodes are wrapped in <see cref="DynamicNode"/>.</remarks>
        protected IEnumerable<object> Objects
        {
            get
            {
                return
                    from triple
                    in subject.Graph.GetTriplesWithSubjectPredicate(subject, predicate)
                    select triple.Object.AsObject(subject.BaseUri);
            }
        }

        /// <summary>
        /// Asserts statements equivalent to given subject and predicate and <paramref name="object"/>.
        /// </summary>
        /// <param name="object">The object to assert.</param>
        public void Add(object @object)
        {
            subject.Add(predicate, @object);
        }

        /// <summary>
        /// Retracts statements with given subject and predicate.
        /// </summary>
        public void Clear()
        {
            subject.Remove(predicate);
        }

        /// <summary>
        /// Checks whether a statement exists equivalent to given subject and predicate and <paramref name="object"/>.
        /// </summary>
        /// <param name="object">The object to assert.</param>
        /// <returns>Whether a statement exists equivalent to given subject and predicate and <paramref name="object"/>.</returns>
        public bool Contains(object @object)
        {
            return Objects.Contains(@object);
        }

        /// <summary>
        /// Copies objects of statements with given subject and predicate <paramref name="array"/> starting at <paramref name="index"/>.
        /// </summary>
        /// <param name="array">The destination of subjects copied.</param>
        /// <param name="index">The index at which copying begins.</param>
        /// <remarks>Known literal nodes are converted to native primitives, URI and blank nodes are wrapped in <see cref="DynamicNode"/>.</remarks>
        public void CopyTo(object[] array, int index)
        {
            Objects.ToArray().CopyTo(array, index);
        }

        /// <summary>
        /// Returns an enumerator that iterates through objects of statements with given subject and predicate.
        /// </summary>
        /// <returns>An enumerator that iterates through objects of statements with given subject and predicate.</returns>
        /// <remarks>Known literal nodes are converted to native primitives, URI and blank nodes are wrapped in <see cref="DynamicNode"/>.</remarks>
        public IEnumerator<object> GetEnumerator()
        {
            return Objects.GetEnumerator();
        }

        /// <summary>
        /// Retracts statements equivalent to given subject and predicate and <paramref name="object"/>.
        /// </summary>
        /// <param name="object">The object to retract.</param>
        /// <returns>Whether any statements were retracted.</returns>
        public bool Remove(object @object)
        {
            return subject.Remove(predicate, @object);
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <inheritdoc/>
        DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter)
        {
            return new EnumerableMetaObject(parameter, this);
        }
    }
}
