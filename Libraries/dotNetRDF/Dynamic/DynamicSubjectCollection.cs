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
    /// Represents a read/write dynamic collection of subjects by predicate and object.
    /// </summary>
    public class DynamicSubjectCollection : ICollection<INode>, IDynamicMetaObjectProvider
    {
        private readonly DynamicNode @object;
        private readonly INode predicate;

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicSubjectCollection"/> class.
        /// </summary>
        /// <param name="predicate">The predicate to use.</param>
        /// <param name="object">The object to use.</param>
        /// <exception cref="ArgumentNullException">When <paramref name="predicate"/> or <paramref name="object"/> are null.</exception>
        public DynamicSubjectCollection(INode predicate, DynamicNode @object)
        {
            if (predicate is null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            if (@object is null)
            {
                throw new ArgumentNullException(nameof(@object));
            }

            this.@object = @object;
            this.predicate = predicate;
        }

        /// <summary>
        /// Gets the number of statements with given predicate and object.
        /// </summary>
        public int Count
        {
            get
            {
                return Subjects.Count();
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
        /// Gets subjects of statements with given predicate and object.
        /// </summary>
        /// <remarks>Nodes are wrapped in a <see cref="DynamicNode"/>.</remarks>
        protected IEnumerable<INode> Subjects
        {
            get
            {
                return
                    from triple
                    in @object.Graph.GetTriplesWithPredicateObject(predicate, @object)
                    select triple.Subject.AsObject(@object.BaseUri) as INode;
            }
        }

        /// <summary>
        /// Asserts a statement with <paramref name="subject"/> and given predicate and object.
        /// </summary>
        /// <param name="subject">The subject to assert.</param>
        public void Add(INode subject)
        {
            @object.Graph.Assert(subject.AsNode(@object.Graph), predicate, @object);
        }

        /// <summary>
        /// Retracts statements with given predicate and object.
        /// </summary>
        public void Clear()
        {
            @object.Graph.Retract(@object.Graph.GetTriplesWithPredicateObject(predicate, @object).ToList());
        }

        /// <summary>
        /// Checks whether a statement exists with <paramref name="subject"/> and given predicate and object.
        /// </summary>
        /// <param name="subject">The subject to check.</param>
        /// <returns>Whether a statement exists with <paramref name="subject"/> and given predicate and object.</returns>
        public bool Contains(INode subject)
        {
            return Subjects.Contains(subject);
        }

        /// <summary>
        /// Copies subjects of statements with given predicate and object to <paramref name="array"/> starting at <paramref name="index"/>.
        /// </summary>
        /// <param name="array">The destination of subjects copied.</param>
        /// <param name="index">The index at which copying begins.</param>
        /// <remarks>Nodes are wrapped in a <see cref="DynamicNode"/>.</remarks>
        public void CopyTo(INode[] array, int index)
        {
            Subjects.ToArray().CopyTo(array, index);
        }

        /// <summary>
        /// Returns an enumerator that iterates through subjects of statements with given predicate and object.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through subjects of statements with given predicate and object.</returns>
        /// <remarks>Nodes are wrapped in a <see cref="DynamicNode"/>.</remarks>
        public IEnumerator<INode> GetEnumerator()
        {
            return Subjects.GetEnumerator();
        }

        /// <summary>
        /// Retracts statements with <paramref name="subject"/> and given predicate and object.
        /// </summary>
        /// <param name="subject">The subject to retract.</param>
        /// <returns>Whether any statements were retracted.</returns>
        public bool Remove(INode subject)
        {
            return @object.Graph.Retract(@object.Graph.GetTriplesWithPredicateObject(predicate, @object).WithSubject(subject.AsNode(@object.Graph)).ToList());
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
