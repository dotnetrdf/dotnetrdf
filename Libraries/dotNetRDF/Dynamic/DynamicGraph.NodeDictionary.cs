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
                    node => node as INode,
                    node => this[node]);
            }
        }

        public object this[INode subject]
        {
            get
            {
                if (subject is null)
                {
                    throw new ArgumentNullException(nameof(subject));
                }

                return new DynamicNode(subject, PredicateBaseUri);
            }

            set
            {
                if (subject is null)
                {
                    throw new ArgumentNullException(nameof(subject));
                }

                Remove(subject);

                if (!(value is null))
                {
                    Add(subject, value);
                }
            }
        }

        ICollection<INode> IDictionary<INode, object>.Keys
        {
            get
            {
                return UriNodes.Cast<INode>().ToList();
            }
        }

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
                        // TODO: Make more specific
                        throw new Exception();
                }
            }
        }

        void ICollection<KeyValuePair<INode, object>>.Add(KeyValuePair<INode, object> item)
        {
            Add(item.Key, item.Value);
        }

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
                        // TODO: Make more specific
                        throw new Exception();
                }

                if (!found)
                {
                    return false;
                }
            }

            return true;
        }

        bool ICollection<KeyValuePair<INode, object>>.Contains(KeyValuePair<INode, object> item)
        {
            return Contains(item.Key, item.Value);
        }

        public bool ContainsKey(INode subject)
        {
            if (subject is null)
            {
                return false;
            }

            return UriNodes.Contains(subject);
        }

        public void CopyTo(KeyValuePair<INode, object>[] array, int arrayIndex)
        {
            NodePairs.ToArray().CopyTo(array, arrayIndex);
        }

        IEnumerator<KeyValuePair<INode, object>> IEnumerable<KeyValuePair<INode, object>>.GetEnumerator()
        {
            return NodePairs.GetEnumerator();
        }

        public bool Remove(INode subject)
        {
            if (subject is null)
            {
                return false;
            }

            return Retract(GetTriplesWithSubject(subject).ToList());
        }

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

        bool ICollection<KeyValuePair<INode, object>>.Remove(KeyValuePair<INode, object> item)
        {
            return Remove(item.Key, item.Value);
        }

        public bool TryGetValue(INode subject, out object predicateAndObjects)
        {
            predicateAndObjects = null;

            if (subject is null)
            {
                return false;
            }

            if (!ContainsKey(subject))
            {
                return false;
            }

            predicateAndObjects = this[subject];
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
