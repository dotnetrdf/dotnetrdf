/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2021 dotNetRDF Project (http://dotnetrdf.org/)
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
using System.Security.Cryptography.X509Certificates;
using VDS.Common.Collections;

namespace VDS.RDF
{
    /// <summary>
    /// Basic Triple Collection which is not indexed.
    /// </summary>
    public class TripleCollection 
        : BaseTripleCollection, IEnumerable<Triple>
    {
        /// <summary>
        /// Struct used for reference counting the quotation of triples in an RDF-star graph.
        /// </summary>
        protected class TripleRefs
        {
            /// <summary>
            /// Flag indicating if the triple is asserted in the graph.
            /// </summary>
            public bool Asserted;
            /// <summary>
            /// Count of the number of times the triple is quoted in the graph.
            /// </summary>
            public uint QuoteCount;
        }

        /// <summary>
        /// Underlying Storage of the Triple Collection.
        /// </summary>
        protected readonly MultiDictionary<Triple, TripleRefs> Triples = new MultiDictionary<Triple, TripleRefs>(new FullTripleComparer(new FastVirtualNodeComparer()));

        /// <summary>
        /// Creates a new Triple Collection.
        /// </summary>
        public TripleCollection() { }

        /// <summary>
        /// Determines whether a given Triple is in the Triple Collection.
        /// </summary>
        /// <param name="t">The Triple to test.</param>
        /// <returns>True if the Triple already exists in the Triple Collection.</returns>
        public override bool Contains(Triple t)
        {
            return Triples.ContainsKey(t);
        }

        /// <inheritdoc />
        public override bool ContainsAsserted(Triple t)
        {
            return Triples.TryGetValue(t, out TripleRefs refs) && refs.Asserted;
        }

        /// <inheritdoc/>
        public override bool ContainsQuoted(Triple t)
        {
            return Triples.TryGetValue(t, out TripleRefs refs) && refs.QuoteCount > 0;
        }

        /// <summary>
        /// Adds a Triple to the Collection.
        /// </summary>
        /// <param name="t">Triple to add.</param>
        protected internal override bool Add(Triple t)
        {
            if (Triples.TryGetValue(t, out TripleRefs refs) && refs.Asserted)
            {
                return false;
            }

            if (refs == null)
            {
                // Triple has not been quoted or asserted
                refs = new TripleRefs { Asserted = true };
            }
            else
            {
                // Triple has been quoted before but hasn't been asserted
                refs.Asserted = true;
            }

            Triples.Add(t, refs);
            RaiseTripleAdded(t);
            if (t.Subject is ITripleNode stn) AddQuoted(stn);
            if (t.Object is ITripleNode otn) AddQuoted(otn);
            return true;
        }

        /// <summary>
        /// Adds a quotation of a triple to the collection.
        /// </summary>
        /// <param name="tripleNode">The triple node that quotes the triple to be added to the collection.</param>
        protected internal void AddQuoted(ITripleNode tripleNode)
        {
            if (Triples.TryGetValue(tripleNode.Triple, out TripleRefs refs))
            {
                refs.QuoteCount++;
            }
            else
            {
                Triples.Add(tripleNode.Triple, new TripleRefs{Asserted = false, QuoteCount = 1});
            }
            // Recursively process any nested quotations
            if (tripleNode.Triple.Subject is ITripleNode stn) AddQuoted(stn);
            if (tripleNode.Triple.Object is ITripleNode otn) AddQuoted(otn);
        }

        /// <summary>
        /// Deletes a Triple from the Collection.
        /// </summary>
        /// <param name="t">Triple to remove.</param>
        /// <remarks>Deleting something that doesn't exist has no effect and gives no error.</remarks>
        protected internal override bool Delete(Triple t)
        {
            if (Triples.Remove(t))
            {
                RaiseTripleRemoved(t);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the Number of Triples in the Triple Collection.
        /// </summary>
        public override int Count
        {
            get
            {
                return Triples.Count;
            }
        }

        /// <summary>
        /// Gets the given Triple.
        /// </summary>
        /// <param name="t">Triple to retrieve.</param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException">Thrown if the given Triple does not exist in the Triple Collection.</exception>
        public override Triple this[Triple t]
        {
            get 
            {
                Triple actual;
                if (Triples.TryGetKey(t, out actual))
                {
                    return actual;
                }
                else
                {
                    throw new KeyNotFoundException("The given Triple does not exist in the Triple Collection");
                }
            }
        }

        /// <summary>
        /// Gets all the Nodes which are Subjects of Triples in the Triple Collection.
        /// </summary>
        public override IEnumerable<INode> SubjectNodes
        {
            get
            {
                IEnumerable<INode> ns = from t in this
                                        select t.Subject;

                return ns.Distinct();
            }
        }

        /// <summary>
        /// Gets all the Nodes which are Predicates of Triples in the Triple Collection.
        /// </summary>
        public override IEnumerable<INode> PredicateNodes
        {
            get
            {
                IEnumerable<INode> ns = from t in this
                                        select t.Predicate;

                return ns.Distinct();
            }
        }

        /// <summary>
        /// Gets all the Nodes which are Objects of Triples in the Triple Collection.
        /// </summary>
        public override IEnumerable<INode> ObjectNodes
        {
            get
            {
                IEnumerable<INode> ns = from t in this
                                        select t.Object;

                return ns.Distinct();
            }
        }

        #region IEnumerable<Triple> Members

        /// <inheritdoc />
        public override IEnumerable<Triple> Quoted
        {
            get
            {
                foreach (KeyValuePair<Triple, TripleRefs> x in Triples)
                {
                    if (x.Value.QuoteCount > 0) yield return x.Key;
                }
            }
        }

        /// <summary>
        /// Gets the Enumerator for the Collection.
        /// </summary>
        /// <returns></returns>
        public override IEnumerator<Triple> GetEnumerator()
        {
            return Triples.Keys.GetEnumerator();
        }

        /// <inheritdoc />
        public override IEnumerable<Triple> Asserted {
            get
            {
                foreach (KeyValuePair<Triple, TripleRefs> x in Triples)
                {
                    if (x.Value.Asserted) yield return x.Key;
                }
            }
        }


        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Gets the Enumerator for the Collection.
        /// </summary>
        /// <returns></returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Disposes of a Triple Collection.
        /// </summary>
        public override void Dispose()
        {
            Triples.Clear();
        }

        #endregion
    }
}
