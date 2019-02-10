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

    public class DynamicSubjectCollection : ICollection<INode>, IDynamicMetaObjectProvider
    {
        private readonly DynamicNode @object;
        private readonly INode predicate;

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

        public int Count => Subjects.Count();

        public bool IsReadOnly => false;

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

        public void Add(INode item)
        {
            @object.Graph.Assert(item.AsNode(@object.Graph), predicate, @object);
        }

        public void Clear()
        {
            @object.Graph.Retract(@object.Graph.GetTriplesWithPredicateObject(predicate, @object).ToList());
        }

        public bool Contains(INode item)
        {
            return Subjects.Contains(item);
        }

        public void CopyTo(INode[] array, int index) => Subjects.ToArray().CopyTo(array, index);

        public IEnumerator<INode> GetEnumerator() => Subjects.GetEnumerator();

        public bool Remove(INode item)
        {
            return @object.Graph.Retract(@object.Graph.GetTriplesWithPredicateObject(predicate, @object).WithSubject(item.AsNode(@object.Graph)).ToList());
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter)
        {
            return new EnumerableMetaObject(parameter, this);
        }
    }
}
