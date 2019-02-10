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

    public partial class DynamicNode : IDictionary<INode, object>
    {
        ICollection<INode> IDictionary<INode, object>.Keys
        {
            get
            {
                return PredicateNodes.Cast<INode>().ToList();
            }
        }

        private IEnumerable<IUriNode> PredicateNodes
        {
            get
            {
                var predicates =
                    from t in Graph.GetTriplesWithSubject(this)
                    select t.Predicate as IUriNode;

                return predicates.Distinct();
            }
        }

        private IDictionary<INode, object> NodePairs
        {
            get
            {
                return PredicateNodes
                    .ToDictionary(
                        predicate => predicate as INode,
                        predicate => this[predicate]);
            }
        }

        public object this[INode predicate]
        {
            get
            {
                if (predicate is null)
                {
                    throw new ArgumentNullException(nameof(predicate));
                }

                return new DynamicObjectCollection(this, predicate);
            }

            set
            {
                if (predicate is null)
                {
                    throw new ArgumentNullException(nameof(predicate));
                }

                Remove(predicate);

                if (value != null)
                {
                    Add(predicate, value);
                }
            }
        }

        public void Add(INode predicate, object objects)
        {
            if (predicate is null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            if (objects is null)
            {
                throw new ArgumentNullException(nameof(objects));
            }

            Graph.Assert(ConvertToTriples(predicate, objects));
        }

        void ICollection<KeyValuePair<INode, object>>.Add(KeyValuePair<INode, object> item)
        {
            Add(item.Key, item.Value);
        }

        public bool Contains(INode predicate, object objects)
        {
            if (predicate is null)
            {
                return false;
            }

            if (objects is null)
            {
                return false;
            }

            var g = new Graph();
            g.Assert(ConvertToTriples(predicate, objects));
            return Graph.HasSubGraph(g);
        }

        bool ICollection<KeyValuePair<INode, object>>.Contains(KeyValuePair<INode, object> item)
        {
            return Contains(item.Key, item.Value);
        }

        public bool ContainsKey(INode predicate)
        {
            if (predicate is null)
            {
                return false;
            }

            return PredicateNodes.Contains(predicate);
        }

        public void CopyTo(KeyValuePair<INode, object>[] array, int arrayIndex)
        {
            NodePairs.CopyTo(array, arrayIndex);
        }

        IEnumerator<KeyValuePair<INode, object>> IEnumerable<KeyValuePair<INode, object>>.GetEnumerator()
        {
            return NodePairs.GetEnumerator();
        }

        public bool Remove(INode predicate)
        {
            if (predicate is null)
            {
                return false;
            }

            return Graph.Retract(Graph.GetTriplesWithSubjectPredicate(this, predicate).ToList());
        }

        public bool Remove(INode predicate, object objects)
        {
            if (predicate is null)
            {
                return false;
            }

            if (objects is null)
            {
                return false;
            }

            return Graph.Retract(ConvertToTriples(predicate, objects));
        }

        bool ICollection<KeyValuePair<INode, object>>.Remove(KeyValuePair<INode, object> item)
        {
            return Remove(item.Key, item.Value);
        }

        public bool TryGetValue(INode predicate, out object objects)
        {
            objects = null;

            if (predicate is null || !ContainsKey(predicate))
            {
                return false;
            }

            objects = this[predicate];
            return true;
        }

        private IEnumerable<Triple> ConvertToTriples(INode predicate, object value)
        {
            // Strings are enumerable but not for our case
            if (value is string || value is DynamicNode || !(value is IEnumerable enumerable))
            {
                enumerable = value.AsEnumerable(); // When they're not enumerable, wrap them in an enumerable of one
            }

            foreach (var @object in enumerable)
            {
                // TODO: Maybe this should throw on null
                if (@object != null)
                {
                    // TODO: This is a mess
                    yield return new Triple(
                        this.CopyNode(predicate.Graph),
                        predicate,
                        @object.AsNode(predicate.Graph));
                }
            }
        }
    }
}
