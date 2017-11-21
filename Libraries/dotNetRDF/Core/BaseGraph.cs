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
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using VDS.RDF.Parsing;
using VDS.RDF.Writing.Serialization;

namespace VDS.RDF
{
    /// <summary>
    /// Abstract Base Implementation of the <see cref="IGraph">IGraph</see> interface
    /// </summary>
#if !NETCORE
    [Serializable,XmlRoot(ElementName="graph")]
#endif
    public abstract class BaseGraph 
        : IGraph
#if !NETCORE
        ,ISerializable
#endif
    {
        #region Variables

        /// <summary>
        /// Collection of Triples in the Graph
        /// </summary>
        protected BaseTripleCollection _triples;
        /// <summary>
        /// Namespace Mapper
        /// </summary>
        protected NamespaceMapper _nsmapper;
        /// <summary>
        /// Base Uri of the Graph
        /// </summary>
        protected Uri _baseuri = null;
        /// <summary>
        /// Blank Node ID Mapper
        /// </summary>
        protected BlankNodeMapper _bnodemapper;

        private TripleEventHandler TripleAddedHandler, TripleRemovedHandler;
#if !NETCORE
        private GraphDeserializationInfo _dsInfo;
#endif

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new Base Graph using the given Triple Collection
        /// </summary>
        /// <param name="tripleCollection">Triple Collection to use</param>
        protected BaseGraph(BaseTripleCollection tripleCollection)
        {
            _triples = tripleCollection;
            _bnodemapper = new BlankNodeMapper();
            _nsmapper = new NamespaceMapper();

            // Create Event Handlers and attach to the Triple Collection
            TripleAddedHandler = new TripleEventHandler(OnTripleAsserted);
            TripleRemovedHandler = new TripleEventHandler(OnTripleRetracted);
            AttachEventHandlers(_triples);
        }

        /// <summary>
        /// Creates a new Base Graph which uses the default <see cref="TreeIndexedTripleCollection" /> as the Triple Collection
        /// </summary>
        protected BaseGraph()
            : this(new TreeIndexedTripleCollection()) { }

#if !NETCORE
        /// <summary>
        /// Creates a Graph from the given Serialization Information
        /// </summary>
        /// <param name="info">Serialization Information</param>
        /// <param name="context">Streaming Context</param>
        protected BaseGraph(SerializationInfo info, StreamingContext context)
            : this()
        {
            _dsInfo = new GraphDeserializationInfo(info, context);   
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            if (_dsInfo != null) _dsInfo.Apply(this);
        }
#endif

        #endregion

        #region Properties

        /// <summary>
        /// Gets the set of Triples described in this Graph
        /// </summary>
        public virtual BaseTripleCollection Triples
        {
            get
            {
                return _triples;
            }
        }

        /// <summary>
        /// Gets the set of Nodes which make up this Graph
        /// </summary>
        public virtual IEnumerable<INode> Nodes
        {
            get
            {
                return (from t in _triples
                        select t.Subject).Concat(from t in _triples
                                                 select t.Object).Distinct();
            }
        }

        /// <summary>
        /// Gets the Namespace Mapper for this Graph which contains all in use Namespace Prefixes and their URIs
        /// </summary>
        /// <returns></returns>
        public virtual INamespaceMapper NamespaceMap
        {
            get
            {
                return _nsmapper;
            }
        }

        /// <summary>
        /// Gets the current Base Uri for the Graph
        /// </summary>
        /// <remarks>
        /// This value may be changed during Graph population depending on whether the Concrete syntax allows the Base Uri to be changed and how the Parser handles this
        /// </remarks>
        public virtual Uri BaseUri
        {
            get
            {
                return _baseuri;
            }
            set
            {
                _baseuri = value;
            }
        }

        /// <summary>
        /// Gets whether a Graph is Empty ie. Contains No Triples or Nodes
        /// </summary>
        public virtual bool IsEmpty
        {
            get
            {
                return (_triples.Count == 0);
            }
        }

        #endregion

        #region Triple Assertion & Retraction

        /// <summary>
        /// Asserts a Triple in the Graph
        /// </summary>
        /// <param name="t">The Triple to add to the Graph</param>
        public abstract bool Assert(Triple t);

        /// <summary>
        /// Asserts a List of Triples in the graph
        /// </summary>
        /// <param name="ts">List of Triples in the form of an IEnumerable</param>
        public abstract bool Assert(IEnumerable<Triple> ts);

        /// <summary>
        /// Retracts a Triple from the Graph
        /// </summary>
        /// <param name="t">Triple to Retract</param>
        /// <remarks>Current implementation may have some defunct Nodes left in the Graph as only the Triple is retracted</remarks>
        public abstract bool Retract(Triple t);

        /// <summary>
        /// Retracts a enumeration of Triples from the graph
        /// </summary>
        /// <param name="ts">Enumeration of Triples to retract</param>
        public abstract bool Retract(IEnumerable<Triple> ts);

        /// <summary>
        /// Clears all Triples from the Graph
        /// </summary>
        /// <remarks>
        /// <para>
        /// The Graph will raise the <see cref="ClearRequested">ClearRequested</see> event at the start of the Clear operation which allows for aborting the operation if the operation is cancelled by an event handler.  On completing the Clear the <see cref="Cleared">Cleared</see> event will be raised.
        /// </para>
        /// </remarks>
        public virtual void Clear()
        {
            if (!RaiseClearRequested()) return;

            Retract(Triples.ToList());

            RaiseCleared();
        }

        #endregion

        #region Node Creation

        /// <summary>
        /// Creates a New Blank Node with an auto-generated Blank Node ID
        /// </summary>
        /// <returns></returns>
        public virtual IBlankNode CreateBlankNode()
        {
            return new BlankNode(this);
        }

        /// <summary>
        /// Creates a New Blank Node with a user-defined Blank Node ID
        /// </summary>
        /// <param name="nodeId">Node ID to use</param>
        /// <returns></returns>
        public virtual IBlankNode CreateBlankNode(String nodeId)
        {
            // try
            // {
            //    Monitor.Enter(this._bnodemapper);
                _bnodemapper.CheckID(ref nodeId);
                return new BlankNode(this, nodeId);
            // }
            // finally
            // {
            //    Monitor.Exit(this._bnodemapper);
            // }
        }

        /// <summary>
        /// Creates a New Literal Node with the given Value
        /// </summary>
        /// <param name="literal">String value of the Literal</param>
        /// <returns></returns>
        public virtual ILiteralNode CreateLiteralNode(String literal)
        {
            return new LiteralNode(this, literal);
        }

        /// <summary>
        /// Creates a New Literal Node with the given Value and Language Specifier
        /// </summary>
        /// <param name="literal">String value of the Literal</param>
        /// <param name="langspec">Language Specifier of the Literal</param>
        /// <returns></returns>
        public virtual ILiteralNode CreateLiteralNode(String literal, String langspec)
        {
            return new LiteralNode(this, literal, langspec);
        }

        /// <summary>
        /// Creates a new Literal Node with the given Value and Data Type
        /// </summary>
        /// <param name="literal">String value of the Literal</param>
        /// <param name="datatype">URI of the Data Type</param>
        /// <returns></returns>
        public virtual ILiteralNode CreateLiteralNode(String literal, Uri datatype)
        {
            return new LiteralNode(this, literal, datatype);
        }

        /// <summary>
        /// Creates a new URI Node that refers to the Base Uri of the Graph
        /// </summary>
        /// <returns></returns>
        public virtual IUriNode CreateUriNode()
        {
            return new UriNode(this, UriFactory.Create(Tools.ResolveUri(String.Empty, _baseuri.ToSafeString())));
        }

        /// <summary>
        /// Creates a new URI Node with the given URI
        /// </summary>
        /// <param name="uri">URI for the Node</param>
        /// <returns></returns>
        /// <remarks>
        /// Generally we expect to be passed an absolute URI, while relative URIs are permitted the behaviour is less well defined.  If there is a Base URI defined for the Graph then relative URIs will be automatically resolved against that Base, if the Base URI is not defined then relative URIs will be left as is.  In this case issues may occur when trying to serialize the data or when accurate round tripping is required.
        /// </remarks>
        public virtual IUriNode CreateUriNode(Uri uri)
        {
            if (!uri.IsAbsoluteUri && _baseuri != null) uri = Tools.ResolveUri(uri, _baseuri);
            return new UriNode(this, uri);
        }

        /// <summary>
        /// Creates a new URI Node with the given QName
        /// </summary>
        /// <param name="qname">QName for the Node</param>
        /// <returns></returns>
        /// <remarks>Internally the Graph will resolve the QName to a full URI, throws an RDF Exception when this is not possible</remarks>
        public virtual IUriNode CreateUriNode(String qname)
        {
            return new UriNode(this, qname);
        }

        /// <summary>
        /// Creates a new Variable Node
        /// </summary>
        /// <param name="varname">Variable Name</param>
        /// <returns></returns>
        public virtual IVariableNode CreateVariableNode(String varname)
        {
            return new VariableNode(this, varname);
        }

        /// <summary>
        /// Creates a new Graph Literal Node with its value being an Empty Subgraph
        /// </summary>
        /// <returns></returns>
        public virtual IGraphLiteralNode CreateGraphLiteralNode()
        {
            return new GraphLiteralNode(this);
        }

        /// <summary>
        /// Creates a new Graph Literal Node with its value being the given Subgraph
        /// </summary>
        /// <param name="subgraph">Subgraph this Node represents</param>
        /// <returns></returns>
        public virtual IGraphLiteralNode CreateGraphLiteralNode(IGraph subgraph)
        {
            return new GraphLiteralNode(this, subgraph);
        }

        #endregion

        #region Node Selection

        /// <summary>
        /// Returns the Blank Node with the given Identifier
        /// </summary>
        /// <param name="nodeId">The Identifier of the Blank Node to select</param>
        /// <returns>Either the Blank Node or null if no Node with the given Identifier exists</returns>
        public abstract IBlankNode GetBlankNode(string nodeId);

        /// <summary>
        /// Returns the LiteralNode with the given Value in the given Language if it exists
        /// </summary>
        /// <param name="literal">The literal value of the Node to select</param>
        /// <param name="langspec">The Language Specifier for the Node to select</param>
        /// <returns>Either the LiteralNode Or null if no Node with the given Value and Language Specifier exists</returns>
        public abstract ILiteralNode GetLiteralNode(string literal, string langspec);

        /// <summary>
        /// Returns the LiteralNode with the given Value if it exists
        /// </summary>
        /// <param name="literal">The literal value of the Node to select</param>
        /// <returns>Either the LiteralNode Or null if no Node with the given Value exists</returns>
        /// <remarks>The LiteralNode in the Graph must have no Language or DataType set</remarks>
        public abstract ILiteralNode GetLiteralNode(string literal);

        /// <summary>
        /// Returns the LiteralNode with the given Value and given Data Type if it exists
        /// </summary>
        /// <param name="literal">The literal value of the Node to select</param>
        /// <param name="datatype">The Uri for the Data Type of the Literal to select</param>
        /// <returns>Either the LiteralNode Or null if no Node with the given Value and Data Type exists</returns>
        public abstract ILiteralNode GetLiteralNode(string literal, Uri datatype);

        /// <summary>
        /// Returns the UriNode with the given QName if it exists
        /// </summary>
        /// <param name="qname">The QName of the Node to select</param>
        /// <returns></returns>
        public abstract IUriNode GetUriNode(string qname);

        /// <summary>
        /// Returns the UriNode with the given Uri if it exists
        /// </summary>
        /// <param name="uri">The Uri of the Node to select</param>
        /// <returns>Either the UriNode Or null if no Node with the given Uri exists</returns>
        public abstract IUriNode GetUriNode(Uri uri);

        #endregion

        #region Triple Selection

        /// <summary>
        /// Gets all the Triples involving the given Uri
        /// </summary>
        /// <param name="uri">The Uri to find Triples involving</param>
        /// <returns>Zero/More Triples</returns>
        public abstract IEnumerable<Triple> GetTriples(Uri uri);

        /// <summary>
        /// Gets all the Triples involving the given Node
        /// </summary>
        /// <param name="n">The Node to find Triples involving</param>
        /// <returns>Zero/More Triples</returns>
        public abstract IEnumerable<Triple> GetTriples(INode n);

        /// <summary>
        /// Gets all the Triples with the given Uri as the Object
        /// </summary>
        /// <param name="u">The Uri to find Triples with it as the Object</param>
        /// <returns>Zero/More Triples</returns>
        public abstract IEnumerable<Triple> GetTriplesWithObject(Uri u);

        /// <summary>
        /// Gets all the Triples with the given Node as the Object
        /// </summary>
        /// <param name="n">The Node to find Triples with it as the Object</param>
        /// <returns></returns>
        public abstract IEnumerable<Triple> GetTriplesWithObject(INode n);

        /// <summary>
        /// Gets all the Triples with the given Node as the Predicate
        /// </summary>
        /// <param name="n">The Node to find Triples with it as the Predicate</param>
        /// <returns></returns>
        public abstract IEnumerable<Triple> GetTriplesWithPredicate(INode n);

        /// <summary>
        /// Gets all the Triples with the given Uri as the Predicate
        /// </summary>
        /// <param name="u">The Uri to find Triples with it as the Predicate</param>
        /// <returns>Zero/More Triples</returns>
        public abstract IEnumerable<Triple> GetTriplesWithPredicate(Uri u);

        /// <summary>
        /// Gets all the Triples with the given Node as the Subject
        /// </summary>
        /// <param name="n">The Node to find Triples with it as the Subject</param>
        /// <returns>Zero/More Triples</returns>
        public abstract IEnumerable<Triple> GetTriplesWithSubject(INode n);

        /// <summary>
        /// Gets all the Triples with the given Uri as the Subject
        /// </summary>
        /// <param name="u">The Uri to find Triples with it as the Subject</param>
        /// <returns>Zero/More Triples</returns>
        public abstract IEnumerable<Triple> GetTriplesWithSubject(Uri u);

        /// <summary>
        /// Selects all Triples with the given Subject and Predicate
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <param name="pred">Predicate</param>
        /// <returns></returns>
        public abstract IEnumerable<Triple> GetTriplesWithSubjectPredicate(INode subj, INode pred);

        /// <summary>
        /// Selects all Triples with the given Subject and Object
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        public abstract IEnumerable<Triple> GetTriplesWithSubjectObject(INode subj, INode obj);

        /// <summary>
        /// Selects all Triples with the given Predicate and Object
        /// </summary>
        /// <param name="pred">Predicate</param>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        public abstract IEnumerable<Triple> GetTriplesWithPredicateObject(INode pred, INode obj);

        /// <summary>
        /// Gets whether a given Triple exists in this Graph
        /// </summary>
        /// <param name="t">Triple to test</param>
        /// <returns></returns>
        public virtual bool ContainsTriple(Triple t)
        {
            return _triples.Contains(t);
        }

        #endregion

        #region Graph Merging

        /// <summary>
        /// Merges another Graph into the current Graph
        /// </summary>
        /// <param name="g">Graph to Merge into this Graph</param>
        /// <remarks>The Graph on which you invoke this method will preserve its Blank Node IDs while the Blank Nodes from the Graph being merged in will be given new IDs as required in the scope of this Graph.</remarks>
        public virtual void Merge(IGraph g)
        {
            Merge(g, false);
        }

        /// <summary>
        /// Merges another Graph into the current Graph
        /// </summary>
        /// <param name="g">Graph to Merge into this Graph</param>
        /// <param name="keepOriginalGraphUri">Indicates that the Merge should preserve the Graph URIs of Nodes so they refer to the Graph they originated in</param>
        /// <remarks>
        /// <para>
        /// The Graph on which you invoke this method will preserve its Blank Node IDs while the Blank Nodes from the Graph being merged in will be given new IDs as required in the scope of this Graph.
        /// </para>
        /// <para>
        /// The Graph will raise the <see cref="MergeRequested">MergeRequested</see> event before the Merge operation which gives any event handlers the oppurtunity to cancel this event.  When the Merge operation is completed the <see cref="Merged">Merged</see> event is raised
        /// </para>
        /// </remarks>
        public virtual void Merge(IGraph g, bool keepOriginalGraphUri)
        {
            if (ReferenceEquals(this, g)) throw new RdfException("You cannot Merge an RDF Graph with itself");

            // Check that the merge can go ahead
            if (!RaiseMergeRequested()) return;

            // First copy and Prefixes across which aren't defined in this Graph
            _nsmapper.Import(g.NamespaceMap);

            if (IsEmpty)
            {
                // Empty Graph so do a quick copy
                foreach (Triple t in g.Triples)
                {
                    Assert(new Triple(Tools.CopyNode(t.Subject, this, keepOriginalGraphUri), Tools.CopyNode(t.Predicate, this, keepOriginalGraphUri), Tools.CopyNode(t.Object, this, keepOriginalGraphUri), t.Context));
                }
            }
            else
            {   
                // Prepare a mapping of Blank Nodes to Blank Nodes
                Dictionary<INode, IBlankNode> mapping = new Dictionary<INode, IBlankNode>();

                foreach (Triple t in g.Triples)
                {
                    INode s, p, o;
                    if (t.Subject.NodeType == NodeType.Blank)
                    {
                        if (!mapping.ContainsKey(t.Subject))
                        {
                            IBlankNode temp = CreateBlankNode();
                            if (keepOriginalGraphUri) temp.GraphUri = t.Subject.GraphUri;
                            mapping.Add(t.Subject, temp);
                        }
                        s = mapping[t.Subject];
                    }
                    else
                    {
                        s = Tools.CopyNode(t.Subject, this, keepOriginalGraphUri);
                    }

                    if (t.Predicate.NodeType == NodeType.Blank)
                    {
                        if (!mapping.ContainsKey(t.Predicate))
                        {
                            IBlankNode temp = CreateBlankNode();
                            if (keepOriginalGraphUri) temp.GraphUri = t.Predicate.GraphUri;
                            mapping.Add(t.Predicate, temp);
                        }
                        p = mapping[t.Predicate];
                    }
                    else
                    {
                        p = Tools.CopyNode(t.Predicate, this, keepOriginalGraphUri);
                    }

                    if (t.Object.NodeType == NodeType.Blank)
                    {
                        if (!mapping.ContainsKey(t.Object))
                        {
                            IBlankNode temp = CreateBlankNode();
                            if (keepOriginalGraphUri) temp.GraphUri = t.Object.GraphUri;
                            mapping.Add(t.Object, temp);
                        }
                        o = mapping[t.Object];
                    }
                    else
                    {
                        o = Tools.CopyNode(t.Object, this, keepOriginalGraphUri);
                    }

                    Assert(new Triple(s, p, o, t.Context));
                }
            }

            RaiseMerged();
        }

        #endregion

        #region Graph Equality

        /// <summary>
        /// Determines whether a Graph is equal to another Object
        /// </summary>
        /// <param name="obj">Object to test</param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// A Graph can only be equal to another Object which is an <see cref="IGraph">IGraph</see>
        /// </para>
        /// <para>
        /// Graph Equality is determined by a somewhat complex algorithm which is explained in the remarks of the other overload for Equals
        /// </para>
        /// </remarks>
        public override bool Equals(object obj)
        {
            // Graphs can't be equal to null
            if (obj == null) return false;

            if (obj is IGraph g)
            {
                return Equals(g, out var temp);
            }
            // Graphs can only be equal to other Graphs
            return false;
        }

        /// <summary>
        /// Determines whether this Graph is equal to the given Graph
        /// </summary>
        /// <param name="g">Graph to test for equality</param>
        /// <param name="mapping">Mapping of Blank Nodes iff the Graphs are equal and contain some Blank Nodes</param>
        /// <returns></returns>
        /// <remarks>
        /// See <see cref="GraphMatcher"/> for documentation of the equality algorithm used.
        /// </remarks>
        public virtual bool Equals(IGraph g, out Dictionary<INode, INode> mapping)
        {
            // Set the mapping to be null
            mapping = null;

            GraphMatcher matcher = new GraphMatcher();
            if (matcher.Equals(this, g))
            {
                mapping = matcher.Mapping;
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region Sub-Graph Matching

        /// <summary>
        /// Checks whether this Graph is a sub-graph of the given Graph
        /// </summary>
        /// <param name="g">Graph</param>
        /// <returns></returns>
        public bool IsSubGraphOf(IGraph g)
        {
            return IsSubGraphOf(g, out var temp);
        }

        /// <summary>
        /// Checks whether this Graph is a sub-graph of the given Graph
        /// </summary>
        /// <param name="g">Graph</param>
        /// <param name="mapping">Mapping of Blank Nodes</param>
        /// <returns></returns>
        public bool IsSubGraphOf(IGraph g, out Dictionary<INode, INode> mapping)
        {
            // Set the mapping to be null
            mapping = null;

            SubGraphMatcher matcher = new SubGraphMatcher();
            if (matcher.IsSubGraph(this, g))
            {
                mapping = matcher.Mapping;
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Checks whether this Graph has the given Graph as a sub-graph
        /// </summary>
        /// <param name="g">Graph</param>
        /// <returns></returns>
        public bool HasSubGraph(IGraph g)
        {
            return g.IsSubGraphOf(this);
        }

        /// <summary>
        /// Checks whether this Graph has the given Graph as a sub-graph
        /// </summary>
        /// <param name="g">Graph</param>
        /// <param name="mapping">Mapping of Blank Nodes</param>
        /// <returns></returns>
        public bool HasSubGraph(IGraph g, out Dictionary<INode, INode> mapping)
        {
            return g.IsSubGraphOf(this, out mapping);
        }

        #endregion

        #region Graph Difference

        /// <summary>
        /// Computes the Difference between this Graph the given Graph
        /// </summary>
        /// <param name="g">Graph</param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// Produces a report which shows the changes that must be made to this Graph to produce the given Graph
        /// </para>
        /// </remarks>
        public GraphDiffReport Difference(IGraph g)
        {
            GraphDiff differ = new GraphDiff();
            return differ.Difference(this, g);
        }

        #endregion

        #region Helper Functions

        /// <summary>
        /// Helper function for Resolving QNames to URIs
        /// </summary>
        /// <param name="qname">QName to resolve to a Uri</param>
        /// <returns></returns>
        public virtual Uri ResolveQName(String qname)
        {
            return UriFactory.Create(Tools.ResolveQName(qname, _nsmapper, _baseuri));
        }

        /// <summary>
        /// Creates a new unused Blank Node ID and returns it
        /// </summary>
        /// <returns></returns>
        public virtual String GetNextBlankNodeID()
        {
            // try
            // {
            //    Monitor.Enter(this._bnodemapper);
                return _bnodemapper.GetNextID();
            // }
            // finally
            // {
            //    Monitor.Exit(this._bnodemapper);
            // }
        }

        #endregion

        #region Events

        /// <summary>
        /// Event which is raised when a Triple is asserted in the Graph
        /// </summary>
        public event TripleEventHandler TripleAsserted;

        /// <summary>
        /// Event which is raised when a Triple is retracted from the Graph
        /// </summary>
        public event TripleEventHandler TripleRetracted;

        /// <summary>
        /// Event which is raised when the Graph contents change
        /// </summary>
        public event GraphEventHandler Changed;

        /// <summary>
        /// Event which is raised just before the Graph is cleared of its contents
        /// </summary>
        public event CancellableGraphEventHandler ClearRequested;

        /// <summary>
        /// Event which is raised after the Graph is cleared of its contents
        /// </summary>
        public event GraphEventHandler Cleared;

        /// <summary>
        /// Event which is raised when a Merge operation is requested on the Graph
        /// </summary>
        public event CancellableGraphEventHandler MergeRequested;

        /// <summary>
        /// Event which is raised when a Merge operation is completed on the Graph
        /// </summary>
        public event GraphEventHandler Merged;

        /// <summary>
        /// Event Handler which handles the <see cref="BaseTripleCollection.TripleAdded">Triple Added</see> event from the underlying Triple Collection by raising the Graph's <see cref="TripleAsserted">TripleAsserted</see> event
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="args">Triple Event Arguments</param>
        protected virtual void OnTripleAsserted(Object sender, TripleEventArgs args)
        {
            RaiseTripleAsserted(args);
        }

        /// <summary>
        /// Helper method for raising the <see cref="TripleAsserted">Triple Asserted</see> event manually
        /// </summary>
        /// <param name="args">Triple Event Arguments</param>
        protected void RaiseTripleAsserted(TripleEventArgs args)
        {
            args.Graph = this;
            TripleAsserted?.Invoke(this, args);
            RaiseGraphChanged(args);
        }

        /// <summary>
        /// Helper method for raising the <see cref="TripleAsserted">Triple Asserted</see> event manually
        /// </summary>
        /// <param name="t">Triple</param>
        protected void RaiseTripleAsserted(Triple t)
        {
            var d = TripleAsserted;
            var e = Changed;
            if (d != null || e != null)
            {
                var args = new TripleEventArgs(t, this);
                d?.Invoke(this, args);
                e?.Invoke(this, new GraphEventArgs(this, args));
            }
        }

        /// <summary>
        /// Event Handler which handles the <see cref="BaseTripleCollection.TripleRemoved">Triple Removed</see> event from the underlying Triple Collection by raising the Graph's <see cref="TripleRetracted">Triple Retracted</see> event
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="args">Triple Event Arguments</param>
        protected virtual void OnTripleRetracted(Object sender, TripleEventArgs args)
        {
            RaiseTripleRetracted(args);
        }

        /// <summary>
        /// Helper method for raising the <see cref="TripleRetracted">Triple Retracted</see> event manually
        /// </summary>
        /// <param name="args"></param>
        protected void RaiseTripleRetracted(TripleEventArgs args)
        {
            var d = TripleRetracted;
            args.Graph = this;
            d?.Invoke(this, args);
            RaiseGraphChanged(args);
        }

        /// <summary>
        /// Helper method for raising the <see cref="TripleRetracted">Triple Retracted</see> event manually
        /// </summary>
        /// <param name="t">Triple</param>
        protected void RaiseTripleRetracted(Triple t)
        {
            var d = TripleRetracted;
            var e = Changed;
            if (d != null || e != null)
            {
                var args = new TripleEventArgs(t, this, false);
                d?.Invoke(this, args);
                e?.Invoke(this, new GraphEventArgs(this, args));
            }
        }

        /// <summary>
        /// Helper method for raising the <see cref="Changed">Changed</see> event
        /// </summary>
        /// <param name="args">Triple Event Arguments</param>
        protected void RaiseGraphChanged(TripleEventArgs args)
        {
            Changed?.Invoke(this, new GraphEventArgs(this, args));
        }

        /// <summary>
        /// Helper method for raising the <see cref="Changed">Changed</see> event
        /// </summary>
        protected void RaiseGraphChanged()
        {
            Changed?.Invoke(this, new GraphEventArgs(this));
        }

        /// <summary>
        /// Helper method for raising the <see cref="ClearRequested">Clear Requested</see> event and returning whether any of the Event Handlers cancelled the operation
        /// </summary>
        /// <returns>True if the operation can continue, false if it should be aborted</returns>
        protected bool RaiseClearRequested()
        {
            CancellableGraphEventHandler d = ClearRequested;
            if (d != null)
            {
                CancellableGraphEventArgs args = new CancellableGraphEventArgs(this);
                d(this, args);
                return !args.Cancel;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Helper method for raising the <see cref="Cleared">Cleared</see> event
        /// </summary>
        protected void RaiseCleared()
        {
            Cleared?.Invoke(this, new GraphEventArgs(this));
        }

        /// <summary>
        /// Helper method for raising the <see cref="MergeRequested">Merge Requested</see> event and returning whether any of the Event Handlers cancelled the operation
        /// </summary>
        /// <returns>True if the operation can continue, false if it should be aborted</returns>
        protected bool RaiseMergeRequested()
        {
            CancellableGraphEventHandler d = MergeRequested;
            if (d != null)
            {
                CancellableGraphEventArgs args = new CancellableGraphEventArgs(this);
                d(this, args);
                return !args.Cancel;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Helper method for raising the <see cref="Merged">Merged</see> event
        /// </summary>
        protected void RaiseMerged()
        {
            Merged?.Invoke(this, new GraphEventArgs(this));
        }

        /// <summary>
        /// Helper method for attaching the necessary event Handlers to a Triple Collection
        /// </summary>
        /// <param name="tripleCollection">Triple Collection</param>
        /// <remarks>
        /// May be useful if you replace the Triple Collection after instantiation e.g. as done in <see cref="Query.SparqlView">SparqlView</see>'s
        /// </remarks>
        protected void AttachEventHandlers(BaseTripleCollection tripleCollection)
        {
            tripleCollection.TripleAdded += TripleAddedHandler;
            tripleCollection.TripleRemoved += TripleRemovedHandler;
        }

        /// <summary>
        /// Helper method for detaching the necessary event Handlers from a Triple Collection
        /// </summary>
        /// <param name="tripleCollection">Triple Collection</param>
        /// <remarks>
        /// May be useful if you replace the Triple Collection after instantiation e.g. as done in <see cref="Query.SparqlView">SparqlView</see>'s
        /// </remarks>
        protected void DetachEventHandlers(BaseTripleCollection tripleCollection)
        {
            tripleCollection.TripleAdded -= TripleAddedHandler;
            tripleCollection.TripleRemoved -= TripleRemovedHandler;
        }

        #endregion

        /// <summary>
        /// Disposes of a Graph
        /// </summary>
        public virtual void Dispose()
        {
            DetachEventHandlers(_triples);
        }

#if !NETCORE

        #region ISerializable Members

        /// <summary>
        /// Gets the Serialization Information for serializing a Graph
        /// </summary>
        /// <param name="info">Serialization Information</param>
        /// <param name="context">Streaming Context</param>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("base", BaseUri.ToSafeString());
            info.AddValue("triples", Triples.ToList(), typeof(List<Triple>));
            IEnumerable<KeyValuePair<String,String>> ns = from p in NamespaceMap.Prefixes
                                                          select new KeyValuePair<String,String>(p, NamespaceMap.GetNamespaceUri(p).AbsoluteUri);
            info.AddValue("namespaces", ns.ToList(), typeof(List<KeyValuePair<String, String>>));
        }

        #endregion

        #region IXmlSerializable Members

        /// <summary>
        /// Gets the Schema for XML Serialization
        /// </summary>
        /// <returns></returns>
        public XmlSchema GetSchema()
        {
            return null;
        }

        /// <summary>
        /// Reads the data for XML deserialization
        /// </summary>
        /// <param name="reader">XML Reader</param>
        public void ReadXml(XmlReader reader)
        {
            XmlSerializer tripleDeserializer = new XmlSerializer(typeof(Triple));
            reader.Read();
            if (reader.Name.Equals("namespaces"))
            {
                if (!reader.IsEmptyElement)
                {
                    reader.Read();
                    while (reader.Name.Equals("namespace"))
                    {
                        if (reader.MoveToAttribute("prefix"))
                        {
                            String prefix = reader.Value;
                            if (reader.MoveToAttribute("uri"))
                            {
                                Uri u = UriFactory.Create(reader.Value);
                                NamespaceMap.AddNamespace(prefix, u);
                                reader.Read();
                            }
                            else
                            {
                                throw new RdfParseException("Expected a uri attribute on a <namespace> element");
                            }
                        }
                        else
                        {
                            throw new RdfParseException("Expected a prefix attribute on a <namespace> element");
                        }
                    }
                }
            }
            reader.Read();
            if (reader.Name.Equals("triples"))
            {
                if (!reader.IsEmptyElement)
                {
                    reader.Read();
                    while (reader.Name.Equals("triple"))
                    {
                        try
                        {
                            Object temp = tripleDeserializer.Deserialize(reader);
                            Assert((Triple)temp);
                            reader.Read();
                        }
                        catch
                        {
                            throw;
                        }
                    }
                }
            }
            else
            {
                throw new RdfParseException("Expected a <triples> element inside a <graph> element but got a <" + reader.Name + "> element instead");
            }
        }

        /// <summary>
        /// Writes the data for XML serialization
        /// </summary>
        /// <param name="writer">XML Writer</param>
        public void WriteXml(XmlWriter writer)
        {
            XmlSerializer tripleSerializer = new XmlSerializer(typeof(Triple));

            // Serialize Base Uri
            if (BaseUri != null)
            {
                writer.WriteAttributeString("base", BaseUri.AbsoluteUri);
            }

            // Serialize Namespace Map
            writer.WriteStartElement("namespaces");
            foreach (String prefix in NamespaceMap.Prefixes)
            {
                writer.WriteStartElement("namespace");
                writer.WriteAttributeString("prefix", prefix);
                writer.WriteAttributeString("uri", NamespaceMap.GetNamespaceUri(prefix).AbsoluteUri);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();

            // Serialize Triples
            writer.WriteStartElement("triples");
            foreach (Triple t in Triples)
            {
                tripleSerializer.Serialize(writer, t);
            }
            writer.WriteEndElement();
        }

        #endregion

#endif
    }
}
