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
    using System.Linq;
    using System.Reflection;

    public partial class DynamicGraph : IDictionary<INode, object>
    {
        /// <inheritdoc/>
        ICollection<INode> IDictionary<INode, object>.Keys
        {
            get
            {
                return UriNodes.Cast<INode>().ToList();
            }
        }

        private IEnumerable<IUriNode> UriNodes
        {
            get
            {
                return Nodes.UriNodes();
            }
        }

        private IDictionary<INode, object> NodePairs
        {
            get
            {
                return UriNodes.ToDictionary(
                    node => (INode)node,
                    node => this[node]);
            }
        }

        /// <summary>
        /// Gets nodes equal to <paramref name="node"/> or sets statements with subject equal to <paramref name="node"/> and predicate and objects equivalent to <paramref name="value"/>.
        /// </summary>
        /// <param name="node">The node to wrap dynamically.</param>
        /// <returns>A <see cref="DynamicNode"/> wrapped around the <paramref name="node"/>.</returns>
        /// <exception cref="ArgumentNullException">When <paramref name="node"/> is null.</exception>
        public object this[INode node]
        {
            get
            {
                if (node is null)
                {
                    throw new ArgumentNullException(nameof(node));
                }

                return new DynamicNode(node, PredicateBaseUri);
            }

            set
            {
                if (node is null)
                {
                    throw new ArgumentNullException(nameof(node));
                }

                Remove(node);

                if (value != null)
                {
                    Add(node, value);
                }
            }
        }

        /// <summary>
        /// Asserts statements equivalent to the parameters.
        /// </summary>
        /// <param name="subject">The subject to assert.</param>
        /// <param name="predicateAndObjects">An object with public properties or a dictionary representing predicates and objects to assert.</param>
        /// <exception cref="ArgumentNullException">When <paramref name="subject"/> or <paramref name="predicateAndObjects"/> is null.</exception>
        /// <exception cref="InvalidOperationException">When <paramref name="predicateAndObjects"/> is a dictionary with keys other than <see cref="INode"/>, <see cref="Uri"/> or <see cref="string"/>.</exception>
        public void Add(INode subject, object predicateAndObjects)
        {
            if (subject is null)
            {
                throw new ArgumentNullException(nameof(subject));
            }

            if (predicateAndObjects is null)
            {
                throw new ArgumentNullException(nameof(predicateAndObjects));
            }

            // Make a copy of the key node local to this graph
            // so dynamic references are resolved correctly
            // (they depend on node's graph)
            // TODO: Which graph exactly are we copying into?
            var targetNode = new DynamicNode(subject.CopyNode(this._g), PredicateBaseUri);

            foreach (DictionaryEntry entry in ConvertToDictionary(predicateAndObjects))
            {
                switch (entry.Key)
                {
                    case string stringKey:
                        targetNode.Add(stringKey, entry.Value);
                        break;

                    case Uri uriKey:
                        targetNode.Add(uriKey, entry.Value);
                        break;

                    case INode nodeKey:
                        targetNode.Add(nodeKey, entry.Value);
                        break;

                    default:
                        throw new InvalidOperationException("Only INode, Uri and string keys are allowed.");
                }
            }
        }

        /// <inheritdoc/>
        void ICollection<KeyValuePair<INode, object>>.Add(KeyValuePair<INode, object> item)
        {
            Add(item.Key, item.Value);
        }

        /// <summary>
        /// Checks whether statements exist equivalent to the parameters.
        /// </summary>
        /// <param name="subject">The subject to check.</param>
        /// <param name="predicateAndObjects">An object with public properties or a dictionary representing predicates and objects to check.</param>
        /// <returns>Whether statements exist equivalent to the parameters.</returns>
        /// <exception cref="InvalidOperationException">When <paramref name="predicateAndObjects"/> is a dictionary with keys other than <see cref="INode"/>, <see cref="Uri"/> or <see cref="string"/>.</exception>
        public bool Contains(INode subject, object predicateAndObjects)
        {
            if (subject is null)
            {
                return false;
            }

            if (predicateAndObjects is null)
            {
                return false;
            }

            if (!TryGetValue(subject, out var value))
            {
                return false;
            }

            var node = (DynamicNode)value;

            foreach (DictionaryEntry entry in ConvertToDictionary(predicateAndObjects))
            {
                var found = true;

                switch (entry.Key)
                {
                    case string stringKey:
                        found = node.Contains(stringKey, entry.Value);
                        break;

                    case Uri uriKey:
                        found = node.Contains(uriKey, entry.Value);
                        break;

                    case INode nodeKey:
                        found = node.Contains(nodeKey, entry.Value);
                        break;

                    default:
                        throw new InvalidOperationException("Only INode, Uri and string keys are allowed.");
                }

                if (!found)
                {
                    return false;
                }
            }

            return true;
        }

        /// <inheritdoc/>
        bool ICollection<KeyValuePair<INode, object>>.Contains(KeyValuePair<INode, object> item)
        {
            return Contains(item.Key, item.Value);
        }

        /// <summary>
        /// Checks whether a URI node equal to <paramref name="key"/> exists.
        /// </summary>
        /// <param name="key">The node to check.</param>
        /// <returns>Whether a URI node equal to <paramref name="key"/> exists.</returns>
        public bool ContainsKey(INode key)
        {
            if (key is null)
            {
                return false;
            }

            return UriNodes.Contains(key);
        }

        /// <inheritdoc/>
        void ICollection<KeyValuePair<INode, object>>.CopyTo(KeyValuePair<INode, object>[] array, int arrayIndex)
        {
            NodePairs.ToArray().CopyTo(array, arrayIndex);
        }

        /// <inheritdoc/>
        IEnumerator<KeyValuePair<INode, object>> IEnumerable<KeyValuePair<INode, object>>.GetEnumerator()
        {
            return NodePairs.GetEnumerator();
        }

        /// <summary>
        /// Retracts statements with <paramref name="subject"/>.
        /// </summary>
        /// <param name="subject">The subject to retract.</param>
        /// <returns>Whether any statements were retracted.</returns>
        public bool Remove(INode subject)
        {
            if (subject is null)
            {
                return false;
            }

            return Retract(GetTriplesWithSubject(subject).ToList());
        }

        /// <summary>
        /// Retracts statements equivalent to the parameters.
        /// </summary>
        /// <param name="subject">The subject to retract.</param>
        /// <param name="predicateAndObjects">An object with public properties or a dictionary representing predicates and objects to retract.</param>
        /// <returns>Whether any statements were retracted.</returns>
        public bool Remove(INode subject, object predicateAndObjects)
        {
            if (subject is null)
            {
                return false;
            }

            if (predicateAndObjects is null)
            {
                return false;
            }

            if (!Contains(subject, predicateAndObjects))
            {
                return false;
            }

            var node = (DynamicNode)this[subject];

            foreach (DictionaryEntry entry in ConvertToDictionary(predicateAndObjects))
            {
                switch (entry.Key)
                {
                    case string stringKey:
                        node.Remove(stringKey, entry.Value);
                        break;

                    case Uri uriKey:
                        node.Remove(uriKey, entry.Value);
                        break;

                    case INode nodeKey:
                        node.Remove(nodeKey, entry.Value);
                        break;
                }
            }

            return true;
        }

        /// <inheritdoc/>
        bool ICollection<KeyValuePair<INode, object>>.Remove(KeyValuePair<INode, object> item)
        {
            return Remove(item.Key, item.Value);
        }

        /// <summary>
        /// Tries to get a node from the graph.
        /// </summary>
        /// <param name="node">The node to try.</param>
        /// <param name="value">A <see cref="DynamicNode"/> wrapped around the <paramref name="node"/>.</param>
        /// <returns>A value representing whether a <paramref name="value"/> was set or not.</returns>
        public bool TryGetValue(INode node, out object value)
        {
            value = null;

            if (node is null)
            {
                return false;
            }

            if (!ContainsKey(node))
            {
                return false;
            }

            value = this[node];
            return true;
        }

        private static IDictionary ConvertToDictionary(object value)
        {
            if (value is IDictionary valueDictionary)
            {
                return valueDictionary;
            }

            return GetProperties(value)
                .ToDictionary(
                    p => p.Name,
                    p => p.GetValue(value, null));
        }

        private static IEnumerable<PropertyInfo> GetProperties(object value)
        {
            return value
                .GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => !p.GetIndexParameters().Any());
        }
    }
}
