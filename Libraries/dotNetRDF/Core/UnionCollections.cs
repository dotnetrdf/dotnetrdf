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

using System;
using System.Collections.Generic;
using System.Linq;

namespace VDS.RDF
{
    /// <summary>
    /// Represents a union of multiple Triple Collections
    /// </summary>
    /// <remarks>
    /// <para>
    /// The union consists of a <em>Base</em> collection which is the collection that Triples can actually be added to and deleted from and any number of additional collections which are read-only as far as the union is concerned (this does not mean they cannot be altered elsewhere by other code)
    /// </para>
    /// </remarks>
    public class UnionTripleCollection 
        : BaseTripleCollection
    {
        private List<BaseTripleCollection> _collections = new List<BaseTripleCollection>();
        private BaseTripleCollection _baseCollection;

        /// <summary>
        /// Creates a new Union Triple Collection which is a union of two collections
        /// </summary>
        /// <param name="baseTriples">Base Triple Collection</param>
        /// <param name="additionalTriples">Additional Triple Collection</param>
        public UnionTripleCollection(BaseTripleCollection baseTriples, BaseTripleCollection additionalTriples)
        {
            if (baseTriples == null) throw new ArgumentNullException("baseTriple");
            if (additionalTriples == null) throw new ArgumentNullException("additionalTriples");
            _collections.Add(baseTriples);
            _collections.Add(additionalTriples);
            _baseCollection = baseTriples;
        }

        /// <summary>
        /// Creates a new Union Triple Collection which is a union of any number of collections
        /// </summary>
        /// <param name="baseTriples">Base Triple Collection</param>
        /// <param name="additionalTriples">Additional Triple Collection(s)</param>
        public UnionTripleCollection(BaseTripleCollection baseTriples, IEnumerable<BaseTripleCollection> additionalTriples)
        {
            if (baseTriples == null) throw new ArgumentNullException("baseTriple");
            _collections.Add(baseTriples);
            _collections.AddRange(additionalTriples);
            _baseCollection = baseTriples;
        }

        /// <summary>
        /// Adds a Triple to the base collection
        /// </summary>
        /// <param name="t">Triple to add</param>
        protected internal override bool Add(Triple t)
        {
            return _baseCollection.Add(t);
        }

        /// <summary>
        /// Checks whether the union contains this Triple in any of the collections it comprises
        /// </summary>
        /// <param name="t">Triple to test</param>
        /// <returns></returns>
        public override bool Contains(Triple t)
        {
            return _collections.Any(c => c.Contains(t));
        }

        /// <summary>
        /// Gets the count of Triples in this union
        /// </summary>
        /// <remarks>
        /// The Count is the total number of Triples, this may be different from the number of distinct triples
        /// </remarks>
        public override int Count
        {
            get 
            {
                return _collections.Sum(c => c.Count);
            }
        }

        /// <summary>
        /// Deletes a Triple from the base collection
        /// </summary>
        /// <param name="t">Triple to delete</param>
        protected internal override bool Delete(Triple t)
        {
            return _baseCollection.Delete(t);
        }

        /// <summary>
        /// Retrieves a Triple from the union
        /// </summary>
        /// <param name="t">Triple to retrieve</param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException">Thrown if the Triple is not contained in any of the collections this union comprises</exception>
        public override Triple this[Triple t]
        {
            get 
            {
                foreach (BaseTripleCollection c in _collections)
                {
                    if (c.Contains(t))
                    {
                        return c[t];
                    }
                }
                throw new KeyNotFoundException("The given Triple does not exist in this Triple Collection");
            }
        }

        /// <summary>
        /// Gets the enumeration of distinct objects of Triples
        /// </summary>
        public override IEnumerable<INode> ObjectNodes
        {
            get 
            {
                return (from c in _collections
                        from o in c.ObjectNodes
                        select o);
            }
        }

        /// <summary>
        /// Gets the enumeration of distinct predicates of Triples
        /// </summary>
        public override IEnumerable<INode> PredicateNodes
        {
            get 
            {
                return (from c in _collections
                        from p in c.PredicateNodes
                        select p); 
            }
        }

        /// <summary>
        /// Gets the enumeration of distinct subjects of Triples
        /// </summary>
        public override IEnumerable<INode> SubjectNodes
        {
            get 
            {
                return (from c in _collections
                        from s in c.SubjectNodes
                        select s); 
            }
        }

        /// <summary>
        /// Disposes of the collection
        /// </summary>
        /// <remarks>
        /// This does nothing since we don't know where and how the collections we are the union of are being used and therefore to dispose of them could have unwanted/unexpected results
        /// </remarks>
        public override void Dispose()
        {
            // Do Nothing
        }

        /// <summary>
        /// Gets the enumeration of Triples in the union
        /// </summary>
        /// <returns></returns>
        public override IEnumerator<Triple> GetEnumerator()
        {
            return _collections.SelectMany(c => c).GetEnumerator();
        }
    }
}
