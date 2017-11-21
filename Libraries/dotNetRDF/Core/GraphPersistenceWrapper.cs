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
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using VDS.RDF.Parsing;
using VDS.RDF.Storage;

namespace VDS.RDF
{
    /// <summary>
    /// The Graph Persistence Wrapper is a wrapper around another Graph that can be used to batch persistence actions with the ability to Flush/Discard changes as desired.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When disposed any outstanding changes are always flushed so if you make changes which you don't want to persist be sure to call the <see cref="GraphPersistenceWrapper.Discard">Discard()</see> method before disposing of the Graph
    /// </para>
    /// <para>
    /// Implementors who wish to make persistent graphs should extend this class and override the <see cref="GraphPersistenceWrapper.SupportsTriplePersistence">SupportsTriplePersistence</see> property and the <see cref="GraphPersistenceWrapper.PersistInsertedTriples">PersistInsertedTriples()</see>, <see cref="GraphPersistenceWrapper.PersistDeletedTriples">PersistDeletedTriples()</see> and <see cref="GraphPersistenceWrapper.PersistGraph">PersistGraph()</see> methods.  If you return true for the property then the <see cref="GraphPersistenceWrapper.PersistInsertedTriples">PersistInsertedTriples()</see> and <see cref="GraphPersistenceWrapper.PersistDeletedTriples">PersistDeletedTriples()</see> methods will be invoked to do persistence on batches of Triples.  If your persistence mechanism requires persisting the entire graph at once return false for the property and override the <see cref="GraphPersistenceWrapper.PersistGraph">PersistGraph()</see> method appropriately.
    /// </para>
    /// <h3>Warning</h3>
    /// <para>
    /// Note that the wrapper does not automatically dispose of the wrapped graph when the wrapper is Dispose, this is by design since disposing of the wrapped Graph can have unintended consequences
    /// </para>
    /// </remarks>
#if !NETCORE
    [Serializable,XmlRoot(ElementName="graph")]
#endif
    public class GraphPersistenceWrapper 
        : IGraph, ITransactionalGraph
    {
        /// <summary>
        /// Underlying Graph this is a wrapper around
        /// </summary>
        protected readonly IGraph _g;
        private readonly List<TriplePersistenceAction> _actions = new List<TriplePersistenceAction>();
        private bool _alwaysQueueActions = false;
        private readonly TripleEventHandler _tripleAddedHandler;
        private readonly TripleEventHandler _tripleRemovedHandler;

        /// <summary>
        /// Creates a new Graph Persistence Wrapper around a new Graph
        /// </summary>
        public GraphPersistenceWrapper()
            : this(new Graph()) { }

        /// <summary>
        /// Creates a new Graph Persistence Wrapper around a new Graph with the given always queue setting
        /// </summary>
        /// <param name="alwaysQueueActions">Whether to always queue actions</param>
        /// <remarks>
        /// The <paramref name="alwaysQueueActions">alwaysQueueActions</paramref> setting when enabled will cause the wrapper to queue Asserts and Retracts for persistence regardless of whether the relevant Triples already exist (i.e. normally if a Triple exists is cannot be asserted again and if it doesn't exist it cannot be retracted).  This is useful for creating derived wrappers which operate in write-only mode i.e. append mode for an existing graph that may be too large to reasonably load into memory
        /// </remarks>
        public GraphPersistenceWrapper(bool alwaysQueueActions)
            : this(new Graph(), alwaysQueueActions) { }

        /// <summary>
        /// Creates a new Graph Persistence Wrapper around the given Graph
        /// </summary>
        /// <param name="g">Graph</param>
        public GraphPersistenceWrapper(IGraph g)
        {
            if (g == null) throw new ArgumentNullException("graph", "Wrapped Graph cannot be null");
            _g = g;

            // Create Event Handlers and attach to the Triple Collection
            _tripleAddedHandler = new TripleEventHandler(OnTripleAsserted);
            _tripleRemovedHandler = new TripleEventHandler(OnTripleRetracted);
            AttachEventHandlers(_g.Triples);
        }

        /// <summary>
        /// Creates a new Graph Persistence Wrapper around the given Graph with the given always queue setting
        /// </summary>
        /// <param name="g">Graph</param>
        /// <param name="alwaysQueueActions">Whether to always queue actions</param>
        /// <remarks>
        /// The <paramref name="alwaysQueueActions">alwaysQueueActions</paramref> setting when enabled will cause the wrapper to queue Asserts and Retracts for persistence regardless of whether the relevant Triples already exist (i.e. normally if a Triple exists is cannot be asserted again and if it doesn't exist it cannot be retracted).  This is useful for creating derived wrappers which operate in write-only mode i.e. append mode for an existing graph that may be too large to reasonably load into memory
        /// </remarks>
        public GraphPersistenceWrapper(IGraph g, bool alwaysQueueActions)
            : this(g)
        {
            _alwaysQueueActions = alwaysQueueActions;
        }

#if !NETCORE
        private List<Triple> _temp;

        /// <summary>
        /// Deserialization Constructor
        /// </summary>
        /// <param name="info">Serialization Information</param>
        /// <param name="context">Streaming Context</param>
        protected GraphPersistenceWrapper(SerializationInfo info, StreamingContext context)
            : this()
        {
            _temp = (List<Triple>)info.GetValue("triples", typeof(List<Triple>));   
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            if (_temp != null)
            {
                Assert(_temp);
                _temp = null;
            }
        }

#endif

        /// <summary>
        /// Destructor for the wrapper to ensure that <see cref="GraphPersistenceWrapper.Dispose()">Dispose()</see> is called and thus that persistence happens
        /// </summary>
        ~GraphPersistenceWrapper()
        {
            Dispose(false);
        }

        #region Wrappers around all the standard IGraph stuff

        /// <summary>
        /// Gets/Sets the Base URI of the Graph
        /// </summary>
        public Uri BaseUri
        {
            get
            {
                return _g.BaseUri;
            }
            set
            {
                _g.BaseUri = value;
            }
        }

        /// <summary>
        /// Gets whether the Graph is empty
        /// </summary>
        public bool IsEmpty
        {
            get 
            { 
                return _g.IsEmpty; 
            }
        }

        /// <summary>
        /// Gets the Namespace Map for the Graph
        /// </summary>
        public INamespaceMapper NamespaceMap
        {
            get
            { 
                return _g.NamespaceMap; 
            }
        }

        /// <summary>
        /// Gets the Nodes of the Graph
        /// </summary>
        public IEnumerable<INode> Nodes
        {
            get 
            { 
                return _g.Nodes; 
            }
        }

        /// <summary>
        /// Gets the Triple Collection for the Graph
        /// </summary>
        public BaseTripleCollection Triples
        {
            get 
            {
                return _g.Triples; 
            }
        }

        /// <summary>
        /// Asserts a Triple in the Graph
        /// </summary>
        /// <param name="t">Triple</param>
        public virtual bool Assert(Triple t)
        {
            if (_alwaysQueueActions || !_g.Triples.Contains(t))
            {
                t = t.CopyTriple(_g);
                _g.Assert(t);
                _actions.Add(new TriplePersistenceAction(t));
                return true;
            }
            return false;
        }

        /// <summary>
        /// Asserts Triples in the Graph
        /// </summary>
        /// <param name="ts">Triples</param>
        public bool Assert(IEnumerable<Triple> ts)
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
        /// <param name="t">Triple</param>
        public virtual bool Retract(Triple t)
        {
            if (_alwaysQueueActions || _g.Triples.Contains(t))
            {
                _g.Retract(t);
                _actions.Add(new TriplePersistenceAction(t, true));
                return true;
            }
            return false;
        }

        /// <summary>
        /// Retracts Triples from the Graph
        /// </summary>
        /// <param name="ts">Triples</param>
        public bool Retract(IEnumerable<Triple> ts)
        {
            bool retracted = false;
            foreach (Triple t in ts)
            {
                retracted = Retract(t) || retracted;
            }
            return retracted;
        }

        /// <summary>
        /// Clears the Graph
        /// </summary>
        public void Clear()
        {
            foreach (Triple t in _g.Triples)
            {
                _actions.Add(new TriplePersistenceAction(t, true));
            }
            _g.Clear();
        }

        /// <summary>
        /// Creates a new Blank Node with the given Node ID
        /// </summary>
        /// <param name="nodeId">Node ID</param>
        /// <returns></returns>
        public IBlankNode CreateBlankNode(string nodeId)
        {
            return _g.CreateBlankNode(nodeId);
        }

        /// <summary>
        /// Creates a new Blank Node
        /// </summary>
        /// <returns></returns>
        public IBlankNode CreateBlankNode()
        {
            return _g.CreateBlankNode();
        }

        /// <summary>
        /// Gets the next available Blank Node ID
        /// </summary>
        /// <returns></returns>
        public string GetNextBlankNodeID()
        {
            return _g.GetNextBlankNodeID();
        }

        /// <summary>
        /// Creates a new Graph Literal Node with the given sub-graph
        /// </summary>
        /// <param name="subgraph">Sub-graph</param>
        /// <returns></returns>
        public IGraphLiteralNode CreateGraphLiteralNode(IGraph subgraph)
        {
            return _g.CreateGraphLiteralNode(subgraph);
        }

        /// <summary>
        /// Creates a new Graph Literal Node
        /// </summary>
        /// <returns></returns>
        public IGraphLiteralNode CreateGraphLiteralNode()
        {
            return _g.CreateGraphLiteralNode();
        }

        /// <summary>
        /// Creates a new Literal Node
        /// </summary>
        /// <param name="literal">Value</param>
        /// <returns></returns>
        public ILiteralNode CreateLiteralNode(string literal)
        {
            return _g.CreateLiteralNode(literal);
        }

        /// <summary>
        /// Creates a new Literal Node with the given Datatype
        /// </summary>
        /// <param name="literal">Value</param>
        /// <param name="datatype">Datatype URI</param>
        /// <returns></returns>
        public ILiteralNode CreateLiteralNode(string literal, Uri datatype)
        {
            return _g.CreateLiteralNode(literal, datatype);
        }

        /// <summary>
        /// Creates a new Literal Node with the given Language
        /// </summary>
        /// <param name="literal">Value</param>
        /// <param name="langspec">Language</param>
        /// <returns></returns>
        public ILiteralNode CreateLiteralNode(string literal, string langspec)
        {
            return _g.CreateLiteralNode(literal, langspec);
        }

        /// <summary>
        /// Creates a new URI Node that references the Graphs Base URI
        /// </summary>
        /// <returns></returns>
        public IUriNode CreateUriNode()
        {
            return _g.CreateUriNode();
        }

        /// <summary>
        /// Creates a new URI Node from a QName
        /// </summary>
        /// <param name="qname">QName</param>
        /// <returns></returns>
        public IUriNode CreateUriNode(string qname)
        {
            return _g.CreateUriNode(qname);
        }

        /// <summary>
        /// Creates a new URI Node
        /// </summary>
        /// <param name="uri">URI</param>
        /// <returns></returns>
        public IUriNode CreateUriNode(Uri uri)
        {
            return _g.CreateUriNode(uri);
        }

        /// <summary>
        /// Creates a new Variable Node
        /// </summary>
        /// <param name="varname">Variable Name</param>
        /// <returns></returns>
        public IVariableNode CreateVariableNode(String varname)
        {
            return _g.CreateVariableNode(varname);
        }

        /// <summary>
        /// Attempts to get the Blank Node with the given ID
        /// </summary>
        /// <param name="nodeId">Node ID</param>
        /// <returns>The Node if it exists or null</returns>
        public IBlankNode GetBlankNode(string nodeId)
        {
            return _g.GetBlankNode(nodeId);
        }

        /// <summary>
        /// Attempts to get the Literal Node with the given Value and Language
        /// </summary>
        /// <param name="literal">Value</param>
        /// <param name="langspec">Language</param>
        /// <returns>The Node if it exists or null</returns>
        public ILiteralNode GetLiteralNode(string literal, string langspec)
        {
            return _g.GetLiteralNode(literal, langspec);
        }

        /// <summary>
        /// Attempts to get the Literal Node with the given Value
        /// </summary>
        /// <param name="literal">Value</param>
        /// <returns>The Node if it exists or null</returns>
        public ILiteralNode GetLiteralNode(string literal)
        {
            return _g.GetLiteralNode(literal);
        }

        /// <summary>
        /// Attempts to get the Literal Node with the given Value and Datatype
        /// </summary>
        /// <param name="literal">Value</param>
        /// <param name="datatype">Datatype URI</param>
        /// <returns>The Node if it exists or null otherwise</returns>
        public ILiteralNode GetLiteralNode(string literal, Uri datatype)
        {
            return _g.GetLiteralNode(literal, datatype);
        }

        /// <summary>
        /// Gets all the Triples involving the given URI
        /// </summary>
        /// <param name="uri">The URI to find Triples involving</param>
        /// <returns>Zero/More Triples</returns>
        public IEnumerable<Triple> GetTriples(Uri uri)
        {
            return _g.GetTriples(uri);
        }

        /// <summary>
        /// Gets all the Triples involving the given Node
        /// </summary>
        /// <param name="n">The Node to find Triples involving</param>
        /// <returns>Zero/More Triples</returns>
        public IEnumerable<Triple> GetTriples(INode n)
        {
            return _g.GetTriples(n);
        }

        /// <summary>
        /// Gets all the Triples with the given URI as the Object
        /// </summary>
        /// <param name="u">The URI to find Triples with it as the Object</param>
        /// <returns>Zero/More Triples</returns>
        public IEnumerable<Triple> GetTriplesWithObject(Uri u)
        {
            return _g.GetTriplesWithObject(u);
        }

        /// <summary>
        /// Gets all the Triples with the given Node as the Object
        /// </summary>
        /// <param name="n">The Node to find Triples with it as the Object</param>
        /// <returns></returns>
        public IEnumerable<Triple> GetTriplesWithObject(INode n)
        {
            return _g.GetTriplesWithObject(n);
        }

        /// <summary>
        /// Gets all the Triples with the given Node as the Predicate
        /// </summary>
        /// <param name="n">The Node to find Triples with it as the Predicate</param>
        /// <returns></returns>
        public IEnumerable<Triple> GetTriplesWithPredicate(INode n)
        {
            return _g.GetTriplesWithPredicate(n);
        }

        /// <summary>
        /// Gets all the Triples with the given Uri as the Predicate
        /// </summary>
        /// <param name="u">The Uri to find Triples with it as the Predicate</param>
        /// <returns>Zero/More Triples</returns>
        public IEnumerable<Triple> GetTriplesWithPredicate(Uri u)
        {
            return _g.GetTriplesWithPredicate(u);
        }

        /// <summary>
        /// Gets all the Triples with the given Node as the Subject
        /// </summary>
        /// <param name="n">The Node to find Triples with it as the Subject</param>
        /// <returns>Zero/More Triples</returns>
        public IEnumerable<Triple> GetTriplesWithSubject(INode n)
        {
            return _g.GetTriplesWithSubject(n);
        }

        /// <summary>
        /// Gets all the Triples with the given Uri as the Subject
        /// </summary>
        /// <param name="u">The Uri to find Triples with it as the Subject</param>
        /// <returns>Zero/More Triples</returns>
        public IEnumerable<Triple> GetTriplesWithSubject(Uri u)
        {
            return _g.GetTriplesWithSubject(u);
        }

        /// <summary>
        /// Selects all Triples with the given Subject and Predicate
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <param name="pred">Predicate</param>
        /// <returns></returns>
        public IEnumerable<Triple> GetTriplesWithSubjectPredicate(INode subj, INode pred)
        {
            return _g.GetTriplesWithSubjectPredicate(subj, pred);
        }

        /// <summary>
        /// Selects all Triples with the given Subject and Object
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        public IEnumerable<Triple> GetTriplesWithSubjectObject(INode subj, INode obj)
        {
            return _g.GetTriplesWithSubjectObject(subj, obj);
        }

        /// <summary>
        /// Selects all Triples with the given Predicate and Object
        /// </summary>
        /// <param name="pred">Predicate</param>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        public IEnumerable<Triple> GetTriplesWithPredicateObject(INode pred, INode obj)
        {
            return _g.GetTriplesWithPredicateObject(pred, obj);
        }

        /// <summary>
        /// Returns the UriNode with the given QName if it exists
        /// </summary>
        /// <param name="qname">The QName of the Node to select</param>
        /// <returns></returns>
        public IUriNode GetUriNode(string qname)
        {
            return _g.GetUriNode(qname);
        }

        /// <summary>
        /// Returns the UriNode with the given Uri if it exists
        /// </summary>
        /// <param name="uri">The Uri of the Node to select</param>
        /// <returns>Either the UriNode Or null if no Node with the given Uri exists</returns>
        public IUriNode GetUriNode(Uri uri)
        {
            return _g.GetUriNode(uri);
        }

        /// <summary>
        /// Gets whether a given Triple exists in this Graph
        /// </summary>
        /// <param name="t">Triple to test</param>
        /// <returns></returns>
        public virtual bool ContainsTriple(Triple t)
        {
            return _g.ContainsTriple(t);
        }

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
            // First copy and Prefixes across which aren't defined in this Graph
            _g.NamespaceMap.Import(g.NamespaceMap);

            if (IsEmpty)
            {
                // Empty Graph so do a quick copy
                foreach (Triple t in g.Triples)
                {
                    Assert(new Triple(Tools.CopyNode(t.Subject, _g, keepOriginalGraphUri), Tools.CopyNode(t.Predicate, _g, keepOriginalGraphUri), Tools.CopyNode(t.Object, _g, keepOriginalGraphUri)));
                }
            }
            else
            {   //Prepare a mapping of Blank Nodes to Blank Nodes
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
                        s = Tools.CopyNode(t.Subject, _g, keepOriginalGraphUri);
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
                        p = Tools.CopyNode(t.Predicate, _g, keepOriginalGraphUri);
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
                        o = Tools.CopyNode(t.Object, _g, keepOriginalGraphUri);
                    }

                    Assert(new Triple(s, p, o));
                }
            }
        }

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
            if (obj is IGraph)
            {
                Dictionary<INode, INode> temp;
                return Equals((IGraph)obj, out temp);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Determines whether this Graph is equal to the given Graph
        /// </summary>
        /// <param name="g">Graph to test for equality</param>
        /// <param name="mapping">Mapping of Blank Nodes iff the Graphs are equal and contain some Blank Nodes</param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// The algorithm used to determine Graph equality is based in part on a Iterative Vertex Classification Algorithm described in a Technical Report from HP by Jeremy J Carroll - <a href="http://www.hpl.hp.com/techreports/2001/HPL-2001-293.html">Matching RDF Graphs</a>
        /// </para>
        /// <para>
        /// Graph Equality is determined according to the following algorithm:
        /// </para>
        /// <ol>
        /// <li>If the given Graph is null Graphs are not equal</li>
        /// <li>If the given Graph is this Graph (as determined by Reference Equality) then Graphs are equal</li>
        /// <li>If the Graphs have a different number of Triples they are not equal</li>
        /// <li>Declare a list of Triples which are the Triples of the given Graph called <em>OtherTriples</em></li>
        /// <li>Declare two dictionaries of Nodes to Integers which are called <em>LocalClassification</em> and <em>OtherClassification</em></li>
        /// <li>For Each Triple in this Graph
        ///     <ol>
        ///     <li>If it is a Ground Triple and cannot be found and removed from <em>OtherTriples</em> then Graphs are not equal since the Triple does not exist in both Graphs</li>
        ///     <li>If it contains Blank Nodes track the number of usages of this Blank Node in <em>LocalClassification</em></li>
        ///     </ol>
        /// </li> 
        /// <li>If there are any Triples remaining in <em>OtherTriples</em> which are Ground Triples then Graphs are not equal since this Graph does not contain them</li>
        /// <li>If all the Triples from both Graphs were Ground Triples and there were no Blank Nodes then the Graphs are equal</li>
        /// <li>Iterate over the remaining Triples in <em>OtherTriples</em> and populate the <em>OtherClassification</em></li>
        /// <li>If the count of the two classifications is different the Graphs are not equal since there are differing numbers of Blank Nodes in the Graph</li>
        /// <li>Now build two additional dictionaries of Integers to Integers which are called <em>LocalDegreeClassification</em> and <em>OtherDegreeClassification</em>.  Iterate over <em>LocalClassification</em> and <em>OtherClassification</em> such that the corresponding degree classifications contain a mapping of the number of Blank Nodes with a given degree</li>
        /// <li>If the count of the two degree classifications is different the Graphs are not equal since there are not the same range of Blank Node degrees in both Graphs</li>
        /// <li>For All classifications in <em>LocalDegreeClassification</em> there must be a matching classification in <em>OtherDegreeClassification</em> else the Graphs are not equal</li>
        /// <li>Then build a possible mapping using the following rules:
        ///     <ol>
        ///     <li>Any Blank Node used only once should be mapped to an equivalent Blank Node in the other Graph.  If this is not possible then the Graphs are not equal</li>
        ///     <li>Any Blank Node with a unique degree should be mapped to an equivalent Blank Node in the other Graph.  If this is not possible then the Graphs are not equal</li>
        ///     <li>Keep a copy of the mapping up to this point as a Base Mapping for use as a fallback in later steps</li>
        ///     <li>Build up lists of dependent pairs of Blank Nodes for both Graphs</li>
        ///     <li>Use these lists to determine if there are any independent nodes not yet mapped.  These should be mapped to equivalent Blank Nodes in the other Graph, if this is not possible the Graphs are not equal</li>
        ///     <li>Use the Dependencies and existing mappings to generate a possible mapping</li>
        ///     <li>If a Complete Possible Mapping (there is a Mapping for each Blank Node from this Graph to the Other Graph) then test this mapping.  If it succeeds then the Graphs are equal</li>
        ///     <li>Otherwise we now fallback to the Base Mapping and use it as a basis for Brute Forcing the possible solution space and testing every possibility until either a mapping works or we find the Graphs to be non-equal</li>
        ///     </ol>
        /// </li>
        /// </ol>
        /// </remarks>
        public bool Equals(IGraph g, out Dictionary<INode, INode> mapping)
        {
            return _g.Equals(g, out mapping);
        }

        /// <summary>
        /// Checks whether this Graph is a sub-graph of the given Graph
        /// </summary>
        /// <param name="g">Graph</param>
        /// <returns></returns>
        public bool IsSubGraphOf(IGraph g)
        {
            return _g.IsSubGraphOf(g);
        }

        /// <summary>
        /// Checks whether this Graph is a sub-graph of the given Graph
        /// </summary>
        /// <param name="g">Graph</param>
        /// <param name="mapping">Mapping of Blank Nodes</param>
        /// <returns></returns>
        public bool IsSubGraphOf(IGraph g, out Dictionary<INode, INode> mapping)
        {
            return _g.IsSubGraphOf(g, out mapping);
        }

        /// <summary>
        /// Checks whether this Graph has the given Graph as a sub-graph
        /// </summary>
        /// <param name="g">Graph</param>
        /// <returns></returns>
        public bool HasSubGraph(IGraph g)
        {
            return _g.HasSubGraph(g);
        }

        /// <summary>
        /// Checks whether this Graph has the given Graph as a sub-graph
        /// </summary>
        /// <param name="g">Graph</param>
        /// <param name="mapping">Mapping of Blank Nodes</param>
        /// <returns></returns>
        public bool HasSubGraph(IGraph g, out Dictionary<INode, INode> mapping)
        {
            return _g.HasSubGraph(g, out mapping);
        }

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
            return _g.Difference(g);
        }

        /// <summary>
        /// Helper function for Resolving QNames to URIs
        /// </summary>
        /// <param name="qname">QName to resolve to a Uri</param>
        /// <returns></returns>
        public Uri ResolveQName(string qname)
        {
            return _g.ResolveQName(qname);
        }

        #endregion

        #region Event Handling

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
            TripleEventHandler d = TripleAsserted;
            args.Graph = this;
            if (d != null)
            {
                d(this, args);
            }
            RaiseGraphChanged(args);
        }

        /// <summary>
        /// Helper method for raising the <see cref="TripleAsserted">Triple Asserted</see> event manually
        /// </summary>
        /// <param name="t">Triple</param>
        protected void RaiseTripleAsserted(Triple t)
        {
            TripleEventHandler d = TripleAsserted;
            GraphEventHandler e = Changed;
            if (d != null || e != null)
            {
                TripleEventArgs args = new TripleEventArgs(t, this);
                if (d != null) d(this, args);
                if (e != null) e(this, new GraphEventArgs(this, args));
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
            TripleEventHandler d = TripleRetracted;
            args.Graph = this;
            if (d != null)
            {
                d(this, args);
            }
            RaiseGraphChanged(args);
        }

        /// <summary>
        /// Helper method for raising the <see cref="TripleRetracted">Triple Retracted</see> event manually
        /// </summary>
        /// <param name="t">Triple</param>
        protected void RaiseTripleRetracted(Triple t)
        {
            TripleEventHandler d = TripleRetracted;
            GraphEventHandler e = Changed;
            if (d != null || e != null)
            {
                TripleEventArgs args = new TripleEventArgs(t, this, false);
                if (d != null) d(this, args);
                if (e != null) e(this, new GraphEventArgs(this, args));
            }
        }

        /// <summary>
        /// Helper method for raising the <see cref="Changed">Changed</see> event
        /// </summary>
        /// <param name="args">Triple Event Arguments</param>
        protected void RaiseGraphChanged(TripleEventArgs args)
        {
            GraphEventHandler d = Changed;
            if (d != null)
            {
                d(this, new GraphEventArgs(this, args));
            }
        }

        /// <summary>
        /// Helper method for raising the <see cref="Changed">Changed</see> event
        /// </summary>
        protected void RaiseGraphChanged()
        {
            GraphEventHandler d = Changed;
            if (d != null)
            {
                d(this, new GraphEventArgs(this));
            }
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
            GraphEventHandler d = Cleared;
            if (d != null)
            {
                d(this, new GraphEventArgs(this));
            }
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
            GraphEventHandler d = Merged;
            if (d != null)
            {
                d(this, new GraphEventArgs(this));
            }
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
            tripleCollection.TripleAdded += _tripleAddedHandler;
            tripleCollection.TripleRemoved += _tripleRemovedHandler;
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
            tripleCollection.TripleAdded -= _tripleAddedHandler;
            tripleCollection.TripleRemoved -= _tripleRemovedHandler;
        }

        #endregion

        #region Persistence Implementation

        /// <summary>
        /// Flushes all changes which have yet to be persisted to the underlying storage
        /// </summary>
        public void Flush()
        {
            if (_actions.Count > 0)
            {
                if (SupportsTriplePersistence)
                {
                    TriplePersistenceAction action = _actions[0];
                    bool isDelete = action.IsDelete;
                    List<Triple> ts = new List<Triple>();
                    ts.Add(action.Triple);

                    int i = 1;
                    while (i < _actions.Count)
                    {
                        action = _actions[i];
                        if (action.IsDelete != isDelete)
                        {
                            // Action switches to/from delete so process the current batch then continue
                            if (isDelete)
                            {
                                PersistDeletedTriples(ts);
                            }
                            else
                            {
                                PersistInsertedTriples(ts);
                            }
                            isDelete = action.IsDelete;
                            ts.Clear();
                        }
                        ts.Add(action.Triple);
                        i++;
                    }

                    // Most likely will be left with a batch to process at the end
                    if (ts.Count > 0)
                    {
                        if (isDelete)
                        {
                            PersistDeletedTriples(ts);
                        }
                        else
                        {
                            PersistInsertedTriples(ts);
                        }
                    }
                }
                else
                {
                    PersistGraph();
                }
                _actions.Clear();
            }
        }

        /// <summary>
        /// Discards all changes which have yet to be persisted so that they are not persisted to the underlying storage
        /// </summary>
        public void Discard()
        {
            int total = _actions.Count;
            int i = _actions.Count - 1;
            while (i >= 0)
            {
                TriplePersistenceAction action = _actions[i];
                if (action.IsDelete)
                {
                    _g.Assert(action.Triple);
                }
                else
                {
                    _g.Retract(action.Triple);
                }
                i--;
            }

            if (total == _actions.Count)
            {
                _actions.Clear();
            }
            else
            {
                _actions.RemoveRange(0, total);
            }
        }

        /// <summary>
        /// Used to indicate whether the persistence mechansim can persist batches of Triples
        /// </summary>
        /// <remarks>
        /// <para>
        /// If <strong>true</strong> then the <see cref="GraphPersistenceWrapper.PersistInsertedTriples">PersistInsertedTriples()</see> and <see cref="GraphPersistenceWrapper.PersistDeletedTriples">PersistDeletedTriples()</see> methods are used to persist changes when the <see cref="GraphPersistenceWrapper.Flush">Flush()</see> method is called.  If <strong>false</strong> then the <see cref="GraphPersistenceWrapper.PersistGraph">PersistGraph()</see> method will be invoked instead.
        /// </para>
        /// </remarks>
        protected virtual bool SupportsTriplePersistence
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Persists inserted Triples to the underlying Storage
        /// </summary>
        /// <param name="ts">Triples</param>
        protected virtual void PersistInsertedTriples(IEnumerable<Triple> ts)
        {
            // Does Nothing
        }

        /// <summary>
        /// Persists deleted Triples to the underlying Storage
        /// </summary>
        /// <param name="ts"></param>
        protected virtual void PersistDeletedTriples(IEnumerable<Triple> ts)
        {
            // Does Nothing
        }

        /// <summary>
        /// Persists the entire Graph to the underlying Storage
        /// </summary>
        protected virtual void PersistGraph()
        {
            // Does Nothing
        }

        #endregion

        /// <summary>
        /// Disposes of the persistence wrapper and in doing so persists any changes to the underlying storage
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Disposes of the persistence wrapper and in doing so persists any changes to the underlying storage
        /// </summary>
        /// <param name="disposing">Whether the method was called from Dispose() or the destructor</param>
        protected void Dispose(bool disposing)
        {
            if (disposing) GC.SuppressFinalize(this);
            Flush();
        }

#if !NETCORE

        #region ISerializable Members

        /// <summary>
        /// Gets the Serialization Information
        /// </summary>
        /// <param name="info">Serialization Information</param>
        /// <param name="context">Streaming Context</param>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("triples", Triples.ToList(), typeof(List<Triple>));
        }

        #endregion

        #region IXmlSerializable Members

        /// <summary>
        /// Gets the Schema for XML serialization
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
                throw new RdfParseException("Expected a <triples> element inside a <graph> element");
            }
        }

        /// <summary>
        /// Writes the data for XML serialization
        /// </summary>
        /// <param name="writer">XML Writer</param>
        public void WriteXml(XmlWriter writer)
        {
            XmlSerializer tripleSerializer = new XmlSerializer(typeof(Triple));
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

    /// <summary>
    /// The Store Graph Persistence Wrapper is a wrapper around another Graph that will be persisted to an underlying store via a provided <see cref="IStorageProvider">IStorageProvider</see> implementation
    /// </summary>
    public class StoreGraphPersistenceWrapper
        : GraphPersistenceWrapper
    {
        private IStorageProvider _manager;

        /// <summary>
        /// Creates a new Store Graph Persistence Wrapper
        /// </summary>
        /// <param name="manager">Generic IO Manager</param>
        /// <param name="g">Graph to wrap</param>
        /// <param name="graphUri">Graph URI (the URI the Graph will be persisted as)</param>
        /// <param name="writeOnly">Whether to operate in write-only mode</param>
        /// <remarks>
        /// <para>
        /// <strong>Note:</strong> In order to operate in write-only mode the <see cref="IStorageProvider">IStorageProvider</see> must support triple level updates indicated by it returning true to its <see cref="IStorageCapabilities.UpdateSupported">UpdateSupported</see> property and the Graph to be wrapped must be an empty Graph
        /// </para>
        /// </remarks>
        public StoreGraphPersistenceWrapper(IStorageProvider manager, IGraph g, Uri graphUri, bool writeOnly)
            : base(g, writeOnly)
        {
            if (manager == null) throw new ArgumentNullException("manager","Cannot persist to a null Generic IO Manager");
            if (manager.IsReadOnly) throw new ArgumentException("Cannot persist to a read-only Generic IO Manager", "manager");
            if (writeOnly && !manager.UpdateSupported) throw new ArgumentException("If writeOnly is set to true then the Generic IO Manager must support triple level updates", "writeOnly");
            if (writeOnly && !g.IsEmpty) throw new ArgumentException("If writeOnly is set to true then the input graph must be empty", "writeOnly");

            _manager = manager;
            BaseUri = graphUri;
        }

        /// <summary>
        /// Creates a new Store Graph Persistence Wrapper
        /// </summary>
        /// <param name="manager">Generic IO Manager</param>
        /// <param name="g">Graph to wrap</param>
        /// <param name="writeOnly">Whether to operate in write-only mode</param>
        /// <remarks>
        /// <para>
        /// <strong>Note:</strong> In order to operate in write-only mode the <see cref="IStorageProvider">IStorageProvider</see> must support triple level updates indicated by it returning true to its <see cref="IStorageCapabilities.UpdateSupported">UpdateSupported</see> property and the Graph to be wrapped must be an empty Graph
        /// </para>
        /// </remarks>
        public StoreGraphPersistenceWrapper(IStorageProvider manager, IGraph g, bool writeOnly)
            : this(manager, g, g.BaseUri, writeOnly) { }

        /// <summary>
        /// Creates a new Store Graph Persistence Wrapper
        /// </summary>
        /// <param name="manager">Generic IO Manager</param>
        /// <param name="g">Graph to wrap</param>
        public StoreGraphPersistenceWrapper(IStorageProvider manager, IGraph g)
            : this(manager, g, g.BaseUri, false) { }

        /// <summary>
        /// Creates a new Store Graph Persistence Wrapper around a new empty Graph
        /// </summary>
        /// <param name="manager">Generic IO Manager</param>
        /// <param name="graphUri">Graph URI (the URI the Graph will be persisted as)</param>
        /// <param name="writeOnly">Whether to operate in write-only mode</param>
        /// <remarks>
        /// <para>
        /// <strong>Note:</strong> In order to operate in write-only mode the <see cref="IStorageProvider">IStorageProvider</see> must support triple level updates indicated by it returning true to its <see cref="IStorageCapabilities.UpdateSupported">UpdateSupported</see> property
        /// </para>
        /// <para>
        /// When not operating in write-only mode the existing Graph will be loaded from the underlying store
        /// </para>
        /// </remarks>
        public StoreGraphPersistenceWrapper(IStorageProvider manager, Uri graphUri, bool writeOnly)
            : base(writeOnly)
        {
            if (manager == null) throw new ArgumentNullException("manager", "Cannot persist to a null Generic IO Manager");
            if (manager.IsReadOnly) throw new ArgumentException("Cannot persist to a read-only Generic IO Manager", "manager");
            if (writeOnly && !manager.UpdateSupported) throw new ArgumentException("If writeOnly is set to true then the Generic IO Manager must support triple level updates", "writeOnly");

            _manager = manager;
            BaseUri = graphUri;

            if (!writeOnly)
            {
                // Load in the existing data
                _manager.LoadGraph(_g, graphUri);
            }
        }

        /// <summary>
        /// Creates a new Store Graph Persistence Wrapper around a new empty Graph
        /// </summary>
        /// <param name="manager">Generic IO Manager</param>
        /// <param name="graphUri">Graph URI (the URI the Graph will be persisted as)</param>
        public StoreGraphPersistenceWrapper(IStorageProvider manager, Uri graphUri)
            : this(manager, graphUri, false) { }

        /// <summary>
        /// Gets whether the in-use <see cref="IStorageProvider">IStorageProvider</see> supports triple level updates
        /// </summary>
        protected override bool SupportsTriplePersistence
        {
            get
            {
                return _manager.UpdateSupported;
            }
        }

        /// <summary>
        /// Persists the deleted Triples to the in-use <see cref="IStorageProvider">IStorageProvider</see>
        /// </summary>
        /// <param name="ts">Triples</param>
        protected override void PersistDeletedTriples(IEnumerable<Triple> ts)
        {
            if (_manager.UpdateSupported)
            {
                _manager.UpdateGraph(BaseUri, null, ts);
            }
            else
            {
                throw new NotSupportedException("The underlying Generic IO Manager does not support Triple Level persistence");
            }
        }

        /// <summary>
        /// Persists the inserted Triples to the in-use <see cref="IStorageProvider">IStorageProvider</see>
        /// </summary>
        /// <param name="ts">Triples</param>
        protected override void PersistInsertedTriples(IEnumerable<Triple> ts)
        {
            if (_manager.UpdateSupported)
            {
                _manager.UpdateGraph(BaseUri, ts, null);
            }
            else
            {
                throw new NotSupportedException("The underlying Generic IO Manager does not support Triple Level persistence");
            }
        }

        /// <summary>
        /// Persists the entire Graph to the in-use <see cref="IStorageProvider">IStorageProvider</see>
        /// </summary>
        protected override void PersistGraph()
        {
            _manager.SaveGraph(this);
        }
    }

    /// <summary>
    /// The File Graph Persistence Wrapper is a wrapper around antoher Graph that will be persisted to a file
    /// </summary>
    public class FileGraphPersistenceWrapper 
        : GraphPersistenceWrapper
    {
        private String _filename;

        /// <summary>
        /// Creates a new File Graph Persistence Wrapper around the given Graph
        /// </summary>
        /// <param name="g">Graph</param>
        /// <param name="filename">File to persist to</param>
        public FileGraphPersistenceWrapper(IGraph g, String filename)
            : base(g)
        {
            if (filename == null) throw new ArgumentException("Cannot persist to a null Filename", "filename");
            _filename = filename;
        }

        /// <summary>
        /// Creates a new File Graph Persistence Wrapper around a new emtpy Graph
        /// </summary>
        /// <param name="filename">File to persist to</param>
        /// <remarks>
        /// If the given file already exists then the Graph will be loaded from that file
        /// </remarks>
        public FileGraphPersistenceWrapper(String filename)
            : base(new Graph())
        {
            if (filename == null) throw new ArgumentException("Cannot persist to a null Filename", "filename");

            if (File.Exists(filename))
            {
                _g.LoadFromFile(filename);
            }
        }

        /// <summary>
        /// Returns that Triple persistence is not supported
        /// </summary>
        protected override bool SupportsTriplePersistence
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Persists the entire Graph to a File
        /// </summary>
        protected override void PersistGraph()
        {
            this.SaveToFile(_filename);
        }
    }
}
