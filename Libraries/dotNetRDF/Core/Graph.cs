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
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace VDS.RDF
{
    /// <summary>
    /// Class for representing RDF Graphs
    /// </summary>
    /// <threadsafety instance="false">Safe for multi-threaded read-only access but unsafe if one/more threads may modify the Graph by using the <see cref="Graph.Assert(Triple)">Assert</see>, <see cref="Graph.Retract(Triple)">Retract</see> or <see cref="BaseGraph.Merge(IGraph)">Merge</see> methods</threadsafety>
#if !NETCORE
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
            if (emptyNamespaceMap) _nsmapper.Clear();
        }

        /// <summary>
        /// Creates a new instance of a Graph using the given Triple Collection
        /// </summary>
        /// <param name="tripleCollection">Triple Collection</param>
        public Graph(BaseTripleCollection tripleCollection)
            : base(tripleCollection) { }

        /// <summary>
        /// Creates a new instance of a Graph using the given Triple Collection and an optionally empty Namespace Map
        /// </summary>
        /// <param name="tripleCollection">Triple Collection</param>
        /// <param name="emptyNamespaceMap">Whether the Namespace Map should be empty</param>
        public Graph(BaseTripleCollection tripleCollection, bool emptyNamespaceMap)
            : base(tripleCollection)
        {
            if (emptyNamespaceMap) _nsmapper.Clear();
        }

#if !NETCORE
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
        public override bool Assert(Triple t)
        {
            // Add to Triples Collection
            if (_triples.Add(t))
            {
                RaiseTripleAsserted(t);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Asserts a List of Triples in the graph
        /// </summary>
        /// <param name="ts">List of Triples in the form of an IEnumerable</param>
        public override bool Assert(IEnumerable<Triple> ts)
        {
            bool asserted = false;
            foreach (Triple t in ts)
            {
                asserted = Assert(t) || asserted;
            }
            return asserted;
        }

        /// <summary>
        /// Retracts a Triple from the Graph
        /// </summary>
        /// <param name="t">Triple to Retract</param>
        /// <remarks>Current implementation may have some defunct Nodes left in the Graph as only the Triple is retracted</remarks>
        public override bool Retract(Triple t)
        {
            if (_triples.Delete(t))
            {
                RaiseTripleRetracted(t);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Retracts a enumeration of Triples from the graph
        /// </summary>
        /// <param name="ts">Enumeration of Triples to retract</param>
        public override bool Retract(IEnumerable<Triple> ts)
        {
            bool retracted = false;
            foreach (Triple t in ts)
            {
                retracted = Retract(t) || retracted;
            }
            return retracted;
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
            return GetNode(new UriNode(this, uri));
        }

        /// <summary>
        /// Returns the UriNode with the given QName if it exists
        /// </summary>
        /// <param name="qname">The QName of the Node to select</param>
        /// <returns></returns>
        public override IUriNode GetUriNode(String qname)
        {
            return GetNode(new UriNode(this, qname));
        }

        /// <summary>
        /// Returns the LiteralNode with the given Value if it exists
        /// </summary>
        /// <param name="literal">The literal value of the Node to select</param>
        /// <returns>Either the LiteralNode Or null if no Node with the given Value exists</returns>
        /// <remarks>The LiteralNode in the Graph must have no Language or DataType set</remarks>
        public override ILiteralNode GetLiteralNode(String literal)
        {
            return GetNode(new LiteralNode(this, literal));
        }

        /// <summary>
        /// Returns the LiteralNode with the given Value in the given Language if it exists
        /// </summary>
        /// <param name="literal">The literal value of the Node to select</param>
        /// <param name="langspec">The Language Specifier for the Node to select</param>
        /// <returns>Either the LiteralNode Or null if no Node with the given Value and Language Specifier exists</returns>
        public override ILiteralNode GetLiteralNode(String literal, String langspec)
        {
            return GetNode(new LiteralNode(this, literal, langspec));
        }

        /// <summary>
        /// Returns the LiteralNode with the given Value and given Data Type if it exists
        /// </summary>
        /// <param name="literal">The literal value of the Node to select</param>
        /// <param name="datatype">The Uri for the Data Type of the Literal to select</param>
        /// <returns>Either the LiteralNode Or null if no Node with the given Value and Data Type exists</returns>
        public override ILiteralNode GetLiteralNode(String literal, Uri datatype)
        {
            return GetNode(new LiteralNode(this, literal, datatype));
        }

        /// <summary>
        /// Returns the Blank Node with the given Identifier
        /// </summary>
        /// <param name="nodeId">The Identifier of the Blank Node to select</param>
        /// <returns>Either the Blank Node or null if no Node with the given Identifier exists</returns>
        public override IBlankNode GetBlankNode(String nodeId)
        {
            return GetNode(new BlankNode(this, nodeId));
        }

        private T GetNode<T>(T node) where T: INode
        {
            var ret = (T) Triples.WithSubject(node).FirstOrDefault()?.Subject;
            if (ret != null) return ret;

            ret = (T) Triples.WithObject(node).FirstOrDefault()?.Object;
            if (ret != null) return ret;

            ret = (T) Triples.WithPredicate(node).FirstOrDefault()?.Predicate;
            return ret;
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
            return GetTriplesWithSubject(n).Union(GetTriplesWithPredicate(n)).Union(GetTriplesWithObject(n));
        }

        /// <summary>
        /// Gets all the Triples involving the given Uri
        /// </summary>
        /// <param name="uri">The Uri to find Triples involving</param>
        /// <returns>Zero/More Triples</returns>
        public override IEnumerable<Triple> GetTriples(Uri uri)
        {
            return GetTriples(new UriNode(null, uri));
        }


        /// <summary>
        /// Gets all the Triples with the given Node as the Subject
        /// </summary>
        /// <param name="n">The Node to find Triples with it as the Subject</param>
        /// <returns>Zero/More Triples</returns>
        public override IEnumerable<Triple> GetTriplesWithSubject(INode n)
        {
            return _triples.WithSubject(n);
        }

        /// <summary>
        /// Gets all the Triples with the given Uri as the Subject
        /// </summary>
        /// <param name="u">The Uri to find Triples with it as the Subject</param>
        /// <returns>Zero/More Triples</returns>
        public override IEnumerable<Triple> GetTriplesWithSubject(Uri u)
        {
            return _triples.WithSubject(CreateUriNode(u));
        }

        /// <summary>
        /// Gets all the Triples with the given Node as the Predicate
        /// </summary>
        /// <param name="n">The Node to find Triples with it as the Predicate</param>
        /// <returns></returns>
        public override IEnumerable<Triple> GetTriplesWithPredicate(INode n)
        {
            return _triples.WithPredicate(n);
        }

        /// <summary>
        /// Gets all the Triples with the given Uri as the Predicate
        /// </summary>
        /// <param name="u">The Uri to find Triples with it as the Predicate</param>
        /// <returns>Zero/More Triples</returns>
        public override IEnumerable<Triple> GetTriplesWithPredicate(Uri u)
        {
            return _triples.WithPredicate(CreateUriNode(u));
        }

        /// <summary>
        /// Gets all the Triples with the given Node as the Object
        /// </summary>
        /// <param name="n">The Node to find Triples with it as the Object</param>
        /// <returns></returns>
        public override IEnumerable<Triple> GetTriplesWithObject(INode n)
        {
            return _triples.WithObject(n);
        }

        /// <summary>
        /// Gets all the Triples with the given Uri as the Object
        /// </summary>
        /// <param name="u">The Uri to find Triples with it as the Object</param>
        /// <returns>Zero/More Triples</returns>
        public override IEnumerable<Triple> GetTriplesWithObject(Uri u)
        {
            return _triples.WithObject(CreateUriNode(u));
        }

        /// <summary>
        /// Selects all Triples with the given Subject and Predicate
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <param name="pred">Predicate</param>
        /// <returns></returns>
        public override IEnumerable<Triple> GetTriplesWithSubjectPredicate(INode subj, INode pred)
        {
            return _triples.WithSubjectPredicate(subj, pred);
        }

        /// <summary>
        /// Selects all Triples with the given Subject and Object
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        public override IEnumerable<Triple> GetTriplesWithSubjectObject(INode subj, INode obj)
        {
            return _triples.WithSubjectObject(subj, obj);
        }

        /// <summary>
        /// Selects all Triples with the given Predicate and Object
        /// </summary>
        /// <param name="pred">Predicate</param>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        public override IEnumerable<Triple> GetTriplesWithPredicateObject(INode pred, INode obj)
        {
            return _triples.WithPredicateObject(pred, obj);
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
