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
    /// Represents a strongly type read/write dynamic collection of subjects by predicate and object.
    /// </summary>
    /// <typeparam name="T">The type of subjects.</typeparam>
    public class DynamicSubjectCollection<T> : DynamicSubjectCollection, ICollection<T>
        where T : INode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicSubjectCollection{T}"/> class.
        /// </summary>
        /// <param name="predicate">The predicate to use.</param>
        /// <param name="object">The object to use.</param>
        public DynamicSubjectCollection(string predicate, DynamicNode @object)
            : base(
                predicate.AsUriNode(@object.Graph, @object.BaseUri),
                @object)
        {
        }

        /// <summary>
        /// Asserts a statement with <paramref name="subject"/> and given predicate and object.
        /// </summary>
        /// <param name="subject">The subject to assert.</param>
        public void Add(T subject)
        {
            base.Add(subject);
        }

        /// <summary>
        /// Checks whether a statement exists with <paramref name="subject"/> and given predicate and object.
        /// </summary>
        /// <param name="subject">The subject to check.</param>
        /// <returns>Whether a statement exists with <paramref name="subject"/> and given predicate and object.</returns>
        public bool Contains(T subject)
        {
            return base.Contains(subject);
        }

        /// <summary>
        /// Copies subjects of statements with given predicate and object to <paramref name="array"/> starting at <paramref name="index"/>.
        /// </summary>
        /// <param name="array">The destination of subjects copied.</param>
        /// <param name="index">The index at which copying begins.</param>
        /// <remarks>Nodes are wrapped in a <see cref="DynamicNode"/>.</remarks>
        public void CopyTo(T[] array, int index)
        {
            this.Subjects.Select(Convert).ToArray().CopyTo(array, index);
        }

        /// <summary>
        /// Retracts statements with <paramref name="subject"/> and given predicate and object.
        /// </summary>
        /// <param name="subject">The subject to retract.</param>
        /// <returns>Whether any statements were retracted.</returns>
        public bool Remove(T subject)
        {
            return base.Remove(subject);
        }

        /// <inheritdoc/>
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return this.Subjects.Select(Convert).GetEnumerator();
        }

        private T Convert(INode value)
        {
            var type = typeof(T);

            if (type.IsSubclassOf(typeof(DynamicNode)))
            {
                // TODO: Exception handling
                var ctor = type.GetConstructor(new[] { typeof(INode) });
                value = (DynamicNode)ctor.Invoke(new[] { value });
            }

            return (T)value;
        }
    }
}
