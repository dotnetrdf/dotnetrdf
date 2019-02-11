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
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents a strongly typed read/write dynamic collection of objects by subject and predicate.
    /// </summary>
    /// <typeparam name="T">The type of statement objects.</typeparam>
    public class DynamicObjectCollection<T> : DynamicObjectCollection, ICollection<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicObjectCollection{T}"/> class.
        /// </summary>
        /// <param name="subject">The subject to use.</param>
        /// <param name="predicate">The predicate to use.</param>
        public DynamicObjectCollection(DynamicNode subject, string predicate)
            : base(
                subject,
                predicate.AsUriNode(subject.Graph, subject.BaseUri))
        {
        }

        /// <summary>
        /// Asserts statements equivalent to given subject and predicate and <paramref name="object"/>.
        /// </summary>
        /// <param name="object">The object to assert.</param>
        public void Add(T @object)
        {
            base.Add(@object);
        }

        /// <summary>
        /// Checks whether a statement exists equivalent to given subject and predicate and <paramref name="object"/>.
        /// </summary>
        /// <param name="object">The object to assert.</param>
        /// <returns>Whether a statement exists equivalent to given subject and predicate and <paramref name="object"/>.</returns>
        public bool Contains(T @object)
        {
            return base.Contains(@object);
        }

        /// <summary>
        /// Copies objects of statements with given subject and predicate <paramref name="array"/> starting at <paramref name="index"/>.
        /// </summary>
        /// <param name="array">The destination of subjects copied.</param>
        /// <param name="index">The index at which copying begins.</param>
        /// <remarks>Known literal nodes are converted to native primitives, URI and blank nodes are wrapped in <see cref="DynamicNode"/>.</remarks>
        public void CopyTo(T[] array, int index)
        {
            this.Objects.Select(Convert).ToArray().CopyTo(array, index);
        }

        /// <summary>
        /// Retracts statements equivalent to given subject and predicate and <paramref name="object"/>.
        /// </summary>
        /// <param name="object">The object to retract.</param>
        /// <returns>Whether any statements were retracted.</returns>
        public bool Remove(T @object)
        {
            return base.Remove(@object);
        }

        /// <inheritdoc/>
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return this.Objects.Select(Convert).GetEnumerator();
        }

        private T Convert(object value)
        {
            var type = typeof(T);

            if (type.IsSubclassOf(typeof(DynamicNode)))
            {
                // TODO: Exception handling
                var ctor = type.GetConstructor(new[] { typeof(INode) });
                value = ctor.Invoke(new[] { value });
            }

            return (T)value;
        }
    }
}
