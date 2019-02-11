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
    using System.Collections.Generic;
    using System.Linq;

    public partial class DynamicNode : IDictionary<string, object>
    {
        /// <summary>
        /// Gets an <see cref="ICollection{String}"/> containing outgoing predicate node names shortened as much as possible.
        /// </summary>
        public ICollection<string> Keys
        {
            get
            {
                return StringPairs.Keys;
            }
        }

        private IDictionary<string, object> StringPairs
        {
            get
            {
                return PredicateNodes
                    .ToDictionary(
                        predicate => predicate.AsName(BaseUri),
                        predicate => this[predicate]);
            }
        }

        /// <summary>
        /// Gets statement objects with this subject and predicate equivalent to <paramref name="predicate"/> or sets staements with this subject, predicate equivalent to <paramref name="predicate"/> and objects equivalent to <paramref name="value"/>.
        /// </summary>
        /// <param name="predicate">The predicate to use.</param>
        /// <returns>A <see cref="DynamicObjectCollection"/> with this subject and <paramref name="predicate"/>.</returns>
        /// <exception cref="ArgumentNullException">When <paramref name="predicate"/> is null.</exception>
        public object this[string predicate]
        {
            get
            {
                if (predicate is null)
                {
                    throw new ArgumentNullException(nameof(predicate));
                }

                return this[Convert(predicate)];
            }

            set
            {
                if (predicate is null)
                {
                    throw new ArgumentNullException(nameof(predicate));
                }

                this[Convert(predicate)] = value;
            }
        }

        /// <summary>
        /// Asserts statements with this subject and predicate and objects equivalent to parameters.
        /// </summary>
        /// <param name="predicate">The predicate to assert.</param>
        /// <param name="objects">An object or enumerable representing objects to assert.</param>
        /// <exception cref="ArgumentNullException">When <paramref name="predicate"/> is null.</exception>
        public void Add(string predicate, object objects)
        {
            if (predicate is null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            Add(Convert(predicate), objects);
        }

        void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item)
        {
            Add(item.Key, item.Value);
        }

        /// <summary>
        /// Checks whether statements exist with this subject, predicate equivalent to <paramref name="predicate"/> and objects equivalent to <paramref name="objects"/>.
        /// </summary>
        /// <param name="predicate">The predicate to assert.</param>
        /// <param name="objects">An object or enumerable representing objects to assert.</param>
        /// <returns>Whether statements exist with this subject, predicate equivalent to <paramref name="predicate"/> and objects equivalent to <paramref name="objects"/>.</returns>
        public bool Contains(string predicate, object objects)
        {
            if (predicate is null)
            {
                return false;
            }

            return Contains(Convert(predicate), objects);
        }

        /// <inheritdoc/>
        bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item)
        {
            return Contains(item.Key, item.Value);
        }

        /// <summary>
        /// Checks whether this node has an outgoing predicate equivalent to <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The node to check.</param>
        /// <returns>Whether this node has an outgoing predicate equivalent to <paramref name="key"/>.</returns>
        public bool ContainsKey(string key)
        {
            if (key is null)
            {
                return false;
            }

            return ContainsKey(Convert(key));
        }

        /// <inheritdoc/>
        void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            StringPairs.CopyTo(array, arrayIndex);
        }

        /// <inheritdoc/>
        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
        {
            return StringPairs.GetEnumerator();
        }

        /// <summary>
        /// Retracts statements with this subject and equivalent to <paramref name="predicate"/>.
        /// </summary>
        /// <param name="predicate">The predicate to retract.</param>
        /// <returns>Whether any statements were retracted.</returns>
        public bool Remove(string predicate)
        {
            if (predicate is null)
            {
                return false;
            }

            return Remove(Convert(predicate));
        }

        /// <summary>
        /// Retracts statements with this subject, predicate equivalent to <paramref name="predicate"/> and objects equivalent to <paramref name="objects"/>.
        /// </summary>
        /// <param name="predicate">The predicate to retract.</param>
        /// <param name="objects">An object with public properties or a dictionary representing predicates and objects to retract.</param>
        /// <returns>Whether any statements were retracted.</returns>
        public bool Remove(string predicate, object objects)
        {
            if (predicate is null)
            {
                return false;
            }

            return Remove(Convert(predicate), objects);
        }

        /// <inheritdoc/>
        bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item)
        {
            return Remove(item.Key, item.Value);
        }

        /// <summary>
        /// Tries to get an object collection.
        /// </summary>
        /// <param name="predicate">The predicate to try.</param>
        /// <param name="value">A <see cref="DynamicObjectCollection"/>.</param>
        /// <returns>A value representing whether a <paramref name="value"/> was set or not.</returns>
        public bool TryGetValue(string predicate, out object value)
        {
            value = null;

            if (predicate is null)
            {
                return false;
            }

            return TryGetValue(Convert(predicate), out value);
        }

        private INode Convert(string predicate)
        {
            return predicate.AsUriNode(this.Graph, BaseUri);
        }
    }
}
