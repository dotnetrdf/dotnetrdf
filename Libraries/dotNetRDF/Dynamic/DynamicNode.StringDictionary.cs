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

        public void Add(string predicate, object objects)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            Add(Convert(predicate), objects);
        }

        public void Add(KeyValuePair<string, object> item)
        {
            Add(item.Key, item.Value);
        }

        public bool Contains(string predicate, object objects)
        {
            if (predicate == null)
            {
                return false;
            }

            return Contains(Convert(predicate), objects);
        }

        bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item)
        {
            return Contains(item.Key, item.Value);
        }

        public bool ContainsKey(string predicate)
        {
            if (predicate == null)
            {
                return false;
            }

            return ContainsKey(Convert(predicate));
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            StringPairs.CopyTo(array, arrayIndex);
        }

        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
        {
            return StringPairs.GetEnumerator();
        }

        public bool Remove(string predicate)
        {
            if (predicate == null)
            {
                return false;
            }

            return Remove(Convert(predicate));
        }

        public bool Remove(string predicate, object objects)
        {
            if (predicate == null)
            {
                return false;
            }

            return Remove(Convert(predicate), objects);
        }

        bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item)
        {
            return Remove(item.Key, item.Value);
        }

        public bool TryGetValue(string predicate, out object objects)
        {
            objects = null;

            if (predicate is null)
            {
                return false;
            }

            return TryGetValue(Convert(predicate), out objects);
        }

        private INode Convert(string predicate)
        {
            return predicate.AsUriNode(this.Graph, BaseUri);
        }
    }
}
