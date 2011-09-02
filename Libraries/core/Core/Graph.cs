/*

Copyright Robert Vesse 2009-10
rvesse@vdesign-studios.com

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using VDS.RDF.Query;

namespace VDS.RDF
{
    /// <summary>
    /// Class for representing RDF Graphs
    /// </summary>
    /// <threadsafety instance="false">Safe for multi-threaded read-only access but unsafe if one/more threads may modify the Graph by using the <see cref="Graph.Assert">Assert</see>, <see cref="Graph.Retract">Retract</see> or <see cref="BaseGraph.Merge">Merge</see> methods</threadsafety>
#if !SILVERLIGHT
    [Serializable,XmlRoot(ElementName="graph")]
#endif
    public class Graph 
        : BaseGraph
    {
        #region Constructor

        /// <summary>
        /// Creates a new instance of a Graph
        /// </summary>
        public Graph() 
            : base() { }

        /// <summary>
        /// Creates a new instance of a Graph with an optionally empty Namespace Map
        /// </summary>
        /// <param name="emptyNamespaceMap">Whether the Namespace Map should be empty</param>
        public Graph(bool emptyNamespaceMap)
            : this()
        {
            if (emptyNamespaceMap) this._nsmapper.Clear();
        }

        /// <summary>
        /// Creates a new instance of a Graph using the given Triple Collection
        /// </summary>
        /// <param name="tripleCollection">Triple Collection</param>
        public Graph(BaseTripleCollection tripleCollection)
            : base(tripleCollection) { }

        /// <summary>
        /// Creates a new instance of a Graph using the given Triple and Node Collections
        /// </summary>
        /// <param name="tripleCollection">Triple Collection</param>
        /// <param name="nodeCollection">Node Collection</param>
        public Graph(BaseTripleCollection tripleCollection, BaseNodeCollection nodeCollection)
            : base(tripleCollection, nodeCollection) { }

        /// <summary>
        /// Creates a new instance of a Graph using the given Triple Collection and an optionally empty Namespace Map
        /// </summary>
        /// <param name="tripleCollection">Triple Collection</param>
        /// <param name="emptyNamespaceMap">Whether the Namespace Map should be empty</param>
        public Graph(BaseTripleCollection tripleCollection, bool emptyNamespaceMap)
            : base(tripleCollection)
        {
            if (emptyNamespaceMap) this._nsmapper.Clear();
        }

        /// <summary>
        /// Creates a new instance of a Graph using the given Triple and Node Collections
        /// </summary>
        /// <param name="tripleCollection">Triple Collection</param>
        /// <param name="nodeCollection">Node Collection</param>
        /// <param name="emptyNamespaceMap">Whether the Namespace Map should be empty</param>
        public Graph(BaseTripleCollection tripleCollection, BaseNodeCollection nodeCollection, bool emptyNamespaceMap)
            : base(tripleCollection, nodeCollection) 
        {
            if (emptyNamespaceMap) this._nsmapper.Clear();
        }

#if !SILVERLIGHT
        /// <summary>
        /// Deserialization Constructor
        /// </summary>
        /// <param name="info">Serialization Information</param>
        /// <param name="context">Streaming Context</param>
        protected Graph(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
#endif

        #endregion

        #region Triple Assertion & Retraction

        /// <summary>
        /// Asserts a Triple in the Graph
        /// </summary>
        /// <param name="t">The Triple to add to the Graph</param>
        public override void Assert(Triple t)
        {
            //Check if we've already asserted this Triple
            if (this._triples.Contains(t))
            {
                //Already asserted so exit
                return;
            }

            //Retrieve the Nodes from the Triple
            INode subj, pred, obj;
            subj = t.Subject;
            pred = t.Predicate;
            obj = t.Object;

            //Add to Node Collection if required
            if (!this._nodes.Contains(subj))
            {
                this._nodes.Add(subj);
            }

            //Don't add Predicates to Nodes collection as they are only considered Nodes if they occur
            //elsewhere as the Subject or Object or a Triple

            if (!this._nodes.Contains(obj))
            {
                this._nodes.Add(obj);
            }

            //Add to Triples Collection
            this._triples.Add(t);
            this.RaiseTripleAsserted(t);
        }

        /// <summary>
        /// Asserts multiple Triples in the Graph
        /// </summary>
        /// <param name="ts">Array of Triples to add</param>
        public override void Assert(Triple[] ts)
        {
            foreach(Triple t in ts)
            {
                this.Assert(t);
            }
        }

        /// <summary>
        /// Asserts a List of Triples in the graph
        /// </summary>
        /// <param name="ts">List of Triples to add to the Graph</param>
        public override void Assert(List<Triple> ts)
        {
            foreach (Triple t in ts)
            {
                this.Assert(t);
            }
        }

        /// <summary>
        /// Asserts a List of Triples in the graph
        /// </summary>
        /// <param name="ts">List of Triples in the form of an IEnumerable</param>
        public override void Assert(IEnumerable<Triple> ts)
        {
            foreach (Triple t in ts)
            {
                this.Assert(t);
            }
        }

        /// <summary>
        /// Retracts a Triple from the Graph
        /// </summary>
        /// <param name="t">Triple to Retract</param>
        /// <remarks>Current implementation may have some defunct Nodes left in the Graph as only the Triple is retracted</remarks>
        public override void Retract(Triple t)
        {
            if (this._triples.Contains(t))
            {
                this._triples.Delete(t);
                this.RaiseTripleRetracted(t);
            }
        }

        /// <summary>
        /// Retracts multiple Triples from the Graph
        /// </summary>
        /// <param name="ts">Array of Triples to retract</param>
        public override void Retract(Triple[] ts)
        {
            foreach (Triple t in ts)
            {
                this.Retract(t);
            }
        }

        /// <summary>
        /// Retracts a List of Triples from the graph
        /// </summary>
        /// <param name="ts">List of Triples to retract from the Graph</param>
        public override void Retract(List<Triple> ts)
        {
            foreach (Triple t in ts)
            {
                this.Retract(t);
            }
        }

        /// <summary>
        /// Retracts a enumeration of Triples from the graph
        /// </summary>
        /// <param name="ts">Enumeration of Triples to retract</param>
        public override void Retract(IEnumerable<Triple> ts)
        {
            this.Retract(ts.ToList());
        }

        #endregion

        #region Node Selection

        /// <summary>
        /// Returns the UriNode with the given Uri if it exists
        /// </summary>
        /// <param name="uri">The Uri of the Node to select</param>
        /// <returns>Either the UriNode Or null if no Node with the given Uri exists</returns>
        public override IUriNode GetUriNode(Uri uri)
        {
            IUriNode test = new UriNode(this, uri);
            IEnumerable<IUriNode> us = from u in this._nodes.UriNodes
                                          where u.Equals(test)
                                          select u;
            return us.FirstOrDefault();
        }

        /// <summary>
        /// Returns the UriNode with the given QName if it exists
        /// </summary>
        /// <param name="qname">The QName of the Node to select</param>
        /// <returns></returns>
        public override IUriNode GetUriNode(String qname)
        {
            IUriNode test = new UriNode(this, qname);
            IEnumerable<IUriNode> us = from u in this._nodes.UriNodes
                                      where u.Equals(test)
                                      select u;
            return us.FirstOrDefault();
        }

        /// <summary>
        /// Returns the LiteralNode with the given Value if it exists
        /// </summary>
        /// <param name="literal">The literal value of the Node to select</param>
        /// <returns>Either the LiteralNode Or null if no Node with the given Value exists</returns>
        /// <remarks>The LiteralNode in the Graph must have no Language or DataType set</remarks>
        public override ILiteralNode GetLiteralNode(String literal)
        {
            ILiteralNode test = new LiteralNode(this, literal);
            IEnumerable<ILiteralNode> ls = from l in this._nodes.LiteralNodes
                                          where l.Equals(test)
                                          select l;
            return ls.FirstOrDefault();
        }

        /// <summary>
        /// Returns the LiteralNode with the given Value in the given Language if it exists
        /// </summary>
        /// <param name="literal">The literal value of the Node to select</param>
        /// <param name="langspec">The Language Specifier for the Node to select</param>
        /// <returns>Either the LiteralNode Or null if no Node with the given Value and Language Specifier exists</returns>
        public override ILiteralNode GetLiteralNode(String literal, String langspec)
        {
            ILiteralNode test = new LiteralNode(this, literal, langspec);
            IEnumerable<ILiteralNode> ls = from l in this._nodes.LiteralNodes
                                          where l.Equals(test)
                                          select l;
            return ls.FirstOrDefault();
        }

        /// <summary>
        /// Returns the LiteralNode with the given Value and given Data Type if it exists
        /// </summary>
        /// <param name="literal">The literal value of the Node to select</param>
        /// <param name="datatype">The Uri for the Data Type of the Literal to select</param>
        /// <returns>Either the LiteralNode Or null if no Node with the given Value and Data Type exists</returns>
        public override ILiteralNode GetLiteralNode(String literal, Uri datatype)
        {
            ILiteralNode test = new LiteralNode(this, literal, datatype);
            IEnumerable<ILiteralNode> ls = from l in this._nodes.LiteralNodes
                                          where l.Equals(test)
                                          select l;
            return ls.FirstOrDefault();
        }

        /// <summary>
        /// Returns the Blank Node with the given Identifier
        /// </summary>
        /// <param name="nodeId">The Identifier of the Blank Node to select</param>
        /// <returns>Either the Blank Node or null if no Node with the given Identifier exists</returns>
        public override IBlankNode GetBlankNode(String nodeId)
        {
            IEnumerable<IBlankNode> bs = from b in this._nodes.BlankNodes
                                        where b.InternalID.Equals(nodeId)
                                        select b;

            return bs.FirstOrDefault();
        }

        /// <summary>
        /// Gets all the Nodes according to some arbitrary criteria as embodied in a Selector
        /// </summary>
        /// <param name="selector">Selector class which performs the Selection</param>
        /// <returns>Zero/More Nodes</returns>
        public override IEnumerable<INode> GetNodes(ISelector<INode> selector)
        {
            IEnumerable<INode> ns = from n in this._nodes
                                    where selector.Accepts(n)
                                    select n;

            return ns;
        }

        #endregion

        #region Triple Selection

        /// <summary>
        /// Gets all the Triples involving the given Node
        /// </summary>
        /// <param name="n">The Node to find Triples involving</param>
        /// <returns>Zero/More Triples</returns>
        public override IEnumerable<Triple> GetTriples(INode n)
        {
            return this.GetTriplesWithSubject(n).Concat(this.GetTriplesWithPredicate(n)).Concat(this.GetTriplesWithObject(n));
        }

        /// <summary>
        /// Gets all the Triples involving the given Uri
        /// </summary>
        /// <param name="uri">The Uri to find Triples involving</param>
        /// <returns>Zero/More Triples</returns>
        public override IEnumerable<Triple> GetTriples(Uri uri)
        {
            IEnumerable<Triple> ts = from t in this._triples
                                     where t.Involves(uri)
                                     select t;

            return ts;
        }

        /// <summary>
        /// Gets all the Triples which meet some arbitrary criteria as embodied in a Selector
        /// </summary>
        /// <param name="selector">Selector class which performs the Selection</param>
        /// <returns>Zero/More Triple</returns>
        public override IEnumerable<Triple> GetTriples(ISelector<Triple> selector)
        {
            IEnumerable<Triple> ts = from t in this._triples
                                     where selector.Accepts(t)
                                     select t;

            return ts;
        }

        /// <summary>
        /// Gets all the Triples with the given Node as the Subject
        /// </summary>
        /// <param name="n">The Node to find Triples with it as the Subject</param>
        /// <returns>Zero/More Triples</returns>
        public override IEnumerable<Triple> GetTriplesWithSubject(INode n)
        {
            return this._triples.WithSubject(n);
        }

        /// <summary>
        /// Gets all the Triples with the given Uri as the Subject
        /// </summary>
        /// <param name="u">The Uri to find Triples with it as the Subject</param>
        /// <returns>Zero/More Triples</returns>
        public override IEnumerable<Triple> GetTriplesWithSubject(Uri u)
        {
            return this._triples.WithSubject(this.CreateUriNode(u));
        }

        /// <summary>
        /// Gets all the Triples with a Subject matching some arbitrary criteria as embodied in a Selector
        /// </summary>
        /// <param name="selector">Selector class which performs the Selection</param>
        /// <returns>Zero/More Triples</returns>
        public override IEnumerable<Triple> GetTriplesWithSubject(ISelector<INode> selector)
        {
            IEnumerable<Triple> ts = from t in this._triples
                                     where selector.Accepts(t.Subject)
                                     select t;

            return ts;
        }

        /// <summary>
        /// Gets all the Triples with the given Node as the Predicate
        /// </summary>
        /// <param name="n">The Node to find Triples with it as the Predicate</param>
        /// <returns></returns>
        public override IEnumerable<Triple> GetTriplesWithPredicate(INode n)
        {
            return this._triples.WithPredicate(n);
        }

        /// <summary>
        /// Gets all the Triples with the given Uri as the Predicate
        /// </summary>
        /// <param name="u">The Uri to find Triples with it as the Predicate</param>
        /// <returns>Zero/More Triples</returns>
        public override IEnumerable<Triple> GetTriplesWithPredicate(Uri u)
        {
            return this._triples.WithPredicate(this.CreateUriNode(u));
        }

        /// <summary>
        /// Gets all the Triples with a Predicate matching some arbitrary criteria as embodied in a Selector
        /// </summary>
        /// <param name="selector">Selector class which performs the Selection</param>
        /// <returns>Zero/More Triples</returns>
        public override IEnumerable<Triple> GetTriplesWithPredicate(ISelector<INode> selector)
        {
            IEnumerable<Triple> ts = from t in this._triples
                                     where selector.Accepts(t.Predicate)
                                     select t;

            return ts;
        }

        /// <summary>
        /// Gets all the Triples with the given Node as the Object
        /// </summary>
        /// <param name="n">The Node to find Triples with it as the Object</param>
        /// <returns></returns>
        public override IEnumerable<Triple> GetTriplesWithObject(INode n)
        {
            return this._triples.WithObject(n);
        }

        /// <summary>
        /// Gets all the Triples with the given Uri as the Object
        /// </summary>
        /// <param name="u">The Uri to find Triples with it as the Object</param>
        /// <returns>Zero/More Triples</returns>
        public override IEnumerable<Triple> GetTriplesWithObject(Uri u)
        {
            return this._triples.WithObject(this.CreateUriNode(u));
        }

        /// <summary>
        /// Gets all the Triples with an Object matching some arbitrary criteria as embodied in a Selector
        /// </summary>
        /// <param name="selector">Selector class which performs the Selection</param>
        /// <returns>Zero/More Triples</returns>
        public override IEnumerable<Triple> GetTriplesWithObject(ISelector<INode> selector)
        {
            IEnumerable<Triple> ts = from t in this._triples
                                     where selector.Accepts(t.Object)
                                     select t;

            return ts;
        }

        /// <summary>
        /// Gets all Triples which are selected by all the Selectors in the Chain (with the Selectors applied in order to the result set of the previous Selector)
        /// </summary>
        /// <param name="selectorChain">Chain of Selector Classes to perform the Selection</param>
        /// <returns>Zero/More Triples</returns>
        /// <remarks>This method is used to apply a series of Selectors where each filters the results of the previous.  Each application of a Selector potentially reduces the results set, anything eliminated in a given step cannot possibly be selected by a later Selector in the Chain.</remarks>
        public override IEnumerable<Triple> GetTriples(List<ISelector<Triple>> selectorChain)
        {
            if (selectorChain.Count == 0)
            {
                //Use the None Selector to give an empty enumeration
                return this.GetTriples(new NoneSelector<Triple>());
            }
            else if (selectorChain.Count == 1)
            {
                //Only 1 Selector
                return this.GetTriples(selectorChain[0]);
            }
            else
            {
                //Multiple Selectors

                //Use 1st to get an initial enumeration
                IEnumerable<Triple> ts = this.GetTriples(selectorChain[0]);

                //Chain the subsequent Selectors
                for (int i = 1; i < selectorChain.Count(); i++)
                {
                    ts = this.GetTriples(ts, selectorChain[i]);
                }

                //Return the end results
                return ts;
            }
        }

        /// <summary>
        /// Gets all Triples which are selected by the final Selector in the Chain (where the results of each Selector are used to initialise the next Selector in the chain and selection applied to the whole Graph each time)
        /// </summary>
        /// <param name="firstSelector">Selector Class which does the initial Selection</param>
        /// <param name="selectorChain">Chain of Dependent Selectors to perform the Selection</param>
        /// <returns>Zero/More Triples</returns>
        /// <remarks>This method is used to apply a series of Selectors where each filter is applied to the entire Graph but is initialised with the results of the previous Selector in the chain.  This means that something eliminated in a given step can potentially be selected by a later Selector in the Chain.</remarks>
        public override IEnumerable<Triple> GetTriples(ISelector<Triple> firstSelector, List<IDependentSelector<Triple>> selectorChain)
        {
            if (selectorChain.Count == 0)
            {
                //Just return the Result of using the first Selector
                return this.GetTriples(firstSelector);
            }
            else
            {
                //Chain the Dependant Selectors together
                IEnumerable<Triple> ts = this.GetTriples(firstSelector);

                for (int i = 0; i < selectorChain.Count(); i++)
                {
                    //Initialise the Next Selector
                    selectorChain[i].Initialise(ts);
                    //Run the Next Selector
                    ts = this.GetTriples(selectorChain[i]);
                }

                //Return the end results
                return ts;
            }
        }

        /// <summary>
        /// Internal Helper method for applying a Selector to a subset of the Triples in the Graph
        /// </summary>
        /// <param name="triples">Subset of Triples</param>
        /// <param name="selector">Selector Class to perform the Selection</param>
        /// <returns></returns>
        private IEnumerable<Triple> GetTriples(IEnumerable<Triple> triples, ISelector<Triple> selector)
        {
            IEnumerable<Triple> ts = from t in triples
                                     where selector.Accepts(t)
                                     select t;

            return ts;
        }

        /// <summary>
        /// Selects all Triples with the given Subject and Predicate
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <param name="pred">Predicate</param>
        /// <returns></returns>
        public override IEnumerable<Triple> GetTriplesWithSubjectPredicate(INode subj, INode pred)
        {
            return this._triples.WithSubjectPredicate(subj, pred);
        }

        /// <summary>
        /// Selects all Triples with the given Subject and Object
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        public override IEnumerable<Triple> GetTriplesWithSubjectObject(INode subj, INode obj)
        {
            return this._triples.WithSubjectObject(subj, obj);
        }

        /// <summary>
        /// Selects all Triples with the given Predicate and Object
        /// </summary>
        /// <param name="pred">Predicate</param>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        public override IEnumerable<Triple> GetTriplesWithPredicateObject(INode pred, INode obj)
        {
            return this._triples.WithPredicateObject(pred, obj);
        }

        #endregion

        #region Triple Existence

        /// <summary>
        /// Checks whether any Triples Exist which match a given Selector
        /// </summary>
        /// <param name="selector">Selector Class which performs the Selection</param>
        /// <returns></returns>
        public override bool TriplesExist(ISelector<Triple> selector)
        {
            IEnumerable<Triple> ts = from t in this._triples
                                     where selector.Accepts(t)
                                     select t;
            return ts.Any();
        }

        #endregion
    }

    /// <summary>
    /// Class for representing RDF Graphs when you don't want Indexing
    /// </summary>
    /// <remarks>
    /// Gives better load performance but poorer lookup performance
    /// </remarks>
    public class NonIndexedGraph : Graph
    {
        /// <summary>
        /// Creates a new Graph which is not indexed
        /// </summary>
        public NonIndexedGraph()
            : base(new TripleCollection()) { }
    }
}
