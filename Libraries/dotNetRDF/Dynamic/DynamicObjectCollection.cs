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

    public class DynamicObjectCollection : ICollection<object>, IDynamicMetaObjectProvider
    {
        private readonly DynamicNode subject;
        private readonly INode predicate;

        public DynamicObjectCollection(DynamicNode subject, INode predicate)
        {
            if (subject is null)
            {
                throw new ArgumentNullException(nameof(subject));
            }

            if (predicate is null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            this.subject = subject;
            this.predicate = predicate;
        }

        public int Count
        {
            get
            {
                return Objects.Count();
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

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

        public void Add(object @object)
        {
            subject.Add(predicate, @object);
        }

        public void Clear()
        {
            subject.Remove(predicate);
        }

        public bool Contains(object @object)
        {
            return Objects.Contains(@object);
        }

        public void CopyTo(object[] array, int index)
        {
            Objects.ToArray().CopyTo(array, index);
        }

        public IEnumerator<object> GetEnumerator()
        {
            return Objects.GetEnumerator();
        }

        public bool Remove(object @object)
        {
            return subject.Remove(predicate, @object);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter)
        {
            return new EnumerableMetaObject(parameter, this);
        }
    }
}
