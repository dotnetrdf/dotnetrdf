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

    public partial class DynamicGraph : IDictionary<Uri, object>
    {
        ICollection<Uri> IDictionary<Uri, object>.Keys
        {
            get
            {
                return UriPairs.Keys;
            }
        }

        private IDictionary<Uri, object> UriPairs
        {
            get
            {
                return UriNodes
                    .ToDictionary(
                        subject => subject.Uri,
                        subject => this[subject]);
            }
        }

        public object this[Uri predicate]
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

        public void Add(Uri subject, object predicateAndObjects)
        {
            if (subject is null)
            {
                throw new ArgumentNullException(nameof(subject));
            }

            Add(Convert(subject), predicateAndObjects);
        }

        void ICollection<KeyValuePair<Uri, object>>.Add(KeyValuePair<Uri, object> item)
        {
            Add(item.Key, item.Value);
        }

        public bool Contains(Uri subject, object predicateAndObjects)
        {
            if (subject is null)
            {
                return false;
            }

            return Contains(Convert(subject), predicateAndObjects);
        }

        bool ICollection<KeyValuePair<Uri, object>>.Contains(KeyValuePair<Uri, object> item)
        {
            return Contains(item.Key, item.Value);
        }

        public bool ContainsKey(Uri subject)
        {
            if (subject is null)
            {
                return false;
            }

            return ContainsKey(Convert(subject));
        }

        public void CopyTo(KeyValuePair<Uri, object>[] array, int arrayIndex)
        {
            UriPairs.ToArray().CopyTo(array, arrayIndex);
        }

        IEnumerator<KeyValuePair<Uri, object>> IEnumerable<KeyValuePair<Uri, object>>.GetEnumerator()
        {
            return UriPairs.GetEnumerator();
        }

        public bool Remove(Uri subject)
        {
            if (subject is null)
            {
                throw new ArgumentNullException(nameof(subject));
            }

            return Remove(Convert(subject));
        }

        public bool Remove(Uri subject, object predicateAndObjects)
        {
            if (subject is null)
            {
                return false;
            }

            return Remove(Convert(subject), predicateAndObjects);
        }

        bool ICollection<KeyValuePair<Uri, object>>.Remove(KeyValuePair<Uri, object> item)
        {
            return Remove(item.Key, item.Value);
        }

        public bool TryGetValue(Uri subject, out object predicateAndObjects)
        {
            predicateAndObjects = null;

            if (subject is null)
            {
                return false;
            }

            return TryGetValue(Convert(subject), out predicateAndObjects);
        }

        private INode Convert(Uri subject)
        {
            return subject.AsUriNode(this, this.SubjectBaseUri);
        }
    }
}
