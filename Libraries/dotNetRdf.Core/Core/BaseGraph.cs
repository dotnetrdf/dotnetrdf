/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2025 dotNetRDF Project (http://dotnetrdf.org/)
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

namespace VDS.RDF;

/// <summary>
/// Abstract Base Implementation of the <see cref="IGraph">IGraph</see> interface.
/// </summary>
public abstract class BaseGraph : IGraph
{
    #region Variables

    /// <summary>
    /// Collection of Triples in the Graph.
    /// </summary>
    protected BaseTripleCollection _triples;

    /// <summary>
    /// The factory to use when creating nodes in this graph.
    /// </summary>
    protected readonly INodeFactory NodeFactory;

    /// <summary>
    /// The factory to use when creating URIs in this graph.
    /// </summary>
    /// <remarks>This property delegates to the <see cref="INodeFactory.UriFactory"/> property of <see cref="NodeFactory"/>.</remarks>
    public IUriFactory UriFactory
    {
        get => NodeFactory.UriFactory; 
        set => NodeFactory.UriFactory = value;
    }

    /// <summary>
    /// The name of the graph.
    /// </summary>
    protected readonly IRefNode _name;

    /// <summary>
    /// Blank Node ID Mapper.
    /// </summary>
    protected BlankNodeMapper _bnodemapper;

    private readonly TripleEventHandler TripleAddedHandler;
    private readonly TripleEventHandler TripleRemovedHandler;

    private bool _disposeTriples = false;
    private bool _disposed = false;

    #endregion

    #region Constructor

    /// <summary>
    /// Creates a new Base Graph using the given Triple Collection.
    /// </summary>
    /// <param name="tripleCollection">Triple Collection to use. If null, a new <see cref="TreeIndexedTripleCollection"/> will be used.</param>
    /// <param name="graphName">The name to assign to the graph.</param>
    /// <param name="nodeFactory">The factory to use when creating nodes in this graph.</param>
    /// <param name="uriFactory">The factory to use when creating URIs in this graph. If not specified or null, defaults to the <see cref="INodeFactory.UriFactory"/> property of <paramref name="nodeFactory"/> or else <see cref="VDS.RDF.UriFactory.Root">root UriFactory</see> if <paramref name="nodeFactory"/> is null.</param>
    protected BaseGraph(BaseTripleCollection tripleCollection, IRefNode graphName = null, INodeFactory nodeFactory = null, IUriFactory uriFactory = null)
    {
        _triples = tripleCollection ?? new TreeIndexedTripleCollection();
        _disposeTriples = tripleCollection == null;
        _bnodemapper = new BlankNodeMapper();
        NodeFactory = nodeFactory ?? new NodeFactory(new NodeFactoryOptions(), uriFactory:uriFactory);
        UriFactory = uriFactory ?? NodeFactory.UriFactory;
        _name = graphName;

        // Create Event Handlers and attach to the Triple Collection
        TripleAddedHandler = OnTripleAsserted;
        TripleRemovedHandler = OnTripleRetracted;
        AttachEventHandlers(_triples);
    }

    /// <summary>
    /// Creates a new Base Graph which uses the default <see cref="TreeIndexedTripleCollection" /> as the Triple Collection.
    /// </summary>
    /// <param name="graphName">The name to assign to the new graph.</param>
    protected BaseGraph(IRefNode graphName = null)
        : this(null, graphName)
    {
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets the set of Triples described in this Graph.
    /// </summary>
    public virtual BaseTripleCollection Triples => _triples;

    /// <summary>
    /// Gets the quoted triples in the graph.
    /// </summary>
    public virtual IEnumerable<Triple> QuotedTriples => _triples.SubjectNodes.OfType<ITripleNode>()
        .Union(_triples.ObjectNodes.OfType<ITripleNode>()).Select(tn => tn.Triple);

    /// <inheritdoc />
    public virtual IEnumerable<INode> Nodes => _triples.SubjectNodes.Union(_triples.ObjectNodes);

    /// <inheritdoc />
    public virtual IEnumerable<INode> AllNodes
    {
        get { return _triples.Asserted.SelectMany(t => t.Nodes).Distinct(); }
    }

    /// <inheritdoc />
    public virtual IEnumerable<INode> QuotedNodes => _triples.QuotedSubjectNodes.Union(_triples.QuotedObjectNodes);

    /// <inheritdoc />
    public virtual IEnumerable<INode> AllQuotedNodes => _triples.QuotedSubjectNodes
        .Union(_triples.QuotedPredicateNodes).Union(_triples.QuotedObjectNodes);

    /// <summary>
    /// Gets the Namespace Mapper for this Graph which contains all in use Namespace Prefixes and their URIs.
    /// </summary>
    /// <returns></returns>
    public INamespaceMapper NamespaceMap => NodeFactory.NamespaceMap;


    /// <summary>
    /// Gets the current Base Uri for the Graph.
    /// </summary>
    /// <remarks>
    /// This value may be changed during Graph population depending on whether the Concrete syntax allows the Base Uri to be changed and how the Parser handles this.
    /// </remarks>
    public Uri BaseUri
    {
        get => NodeFactory.BaseUri;
        set => NodeFactory.BaseUri = value;
    }

    /// <summary>
    /// Get or set the name of the graph.
    /// </summary>
    public virtual IRefNode Name
    {
        get => _name;
    }

    /// <summary>
    /// Gets whether a Graph is Empty ie. Contains No Triples or Nodes.
    /// </summary>
    public virtual bool IsEmpty => _triples.Count == 0;

    /// <summary>
    /// Get or set whether to normalize the value strings of literal nodes on creation.
    /// </summary>
#pragma warning disable CS0618 // Type or member is obsolete
    public virtual bool NormalizeLiteralValues { get; set; } = Options.LiteralValueNormalization;
#pragma warning restore CS0618 // Type or member is obsolete

    /// <inheritdoc />
    public LanguageTagValidationMode LanguageTagValidation
    {
        get => NodeFactory.LanguageTagValidation;
        set => NodeFactory.LanguageTagValidation = value;
    }
    #endregion

    #region Triple Assertion & Retraction

    /// <summary>
    /// Asserts a Triple in the Graph.
    /// </summary>
    /// <param name="t">The Triple to add to the Graph.</param>
    public abstract bool Assert(Triple t);

    /// <summary>
    /// Asserts a List of Triples in the graph.
    /// </summary>
    /// <param name="ts">List of Triples in the form of an IEnumerable.</param>
    public abstract bool Assert(IEnumerable<Triple> ts);

    /// <summary>
    /// Retracts a Triple from the Graph.
    /// </summary>
    /// <param name="t">Triple to Retract.</param>
    /// <remarks>Current implementation may have some defunct Nodes left in the Graph as only the Triple is retracted.</remarks>
    public abstract bool Retract(Triple t);

    /// <summary>
    /// Retracts a enumeration of Triples from the graph.
    /// </summary>
    /// <param name="ts">Enumeration of Triples to retract.</param>
    public abstract bool Retract(IEnumerable<Triple> ts);

    /// <summary>
    /// Clears all Triples from the Graph.
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

    #region NodeFactory methods

    /// <summary>
    /// Creates a Blank Node with a new automatically generated ID.
    /// </summary>
    /// <returns></returns>
    public virtual IBlankNode CreateBlankNode()
    {
        return NodeFactory.CreateBlankNode();
    }

    /// <summary>
    /// Creates a Blank Node with the given Node ID.
    /// </summary>
    /// <param name="nodeId">Node ID.</param>
    /// <returns></returns>
    public virtual IBlankNode CreateBlankNode(string nodeId)
    {
        return NodeFactory.CreateBlankNode(nodeId);
    }

    /// <summary>
    /// Creates a Graph Literal Node which represents the empty Subgraph.
    /// </summary>
    /// <returns></returns>
    public virtual IGraphLiteralNode CreateGraphLiteralNode()
    {
        return NodeFactory.CreateGraphLiteralNode();
    }

    /// <summary>
    /// Creates a Graph Literal Node which represents the given Subgraph.
    /// </summary>
    /// <param name="subgraph">Subgraph.</param>
    /// <returns></returns>
    public virtual IGraphLiteralNode CreateGraphLiteralNode(IGraph subgraph)
    {
        return NodeFactory.CreateGraphLiteralNode(subgraph);
    }

    /// <summary>
    /// Creates a Literal Node with the given Value and Data Type.
    /// </summary>
    /// <param name="literal">Value of the Literal.</param>
    /// <param name="datatype">Data Type URI of the Literal.</param>
    /// <returns></returns>
    public virtual ILiteralNode CreateLiteralNode(string literal, Uri datatype)
    {
        return NodeFactory.CreateLiteralNode(literal, datatype);
    }

    /// <summary>
    /// Creates a Literal Node with the given Value.
    /// </summary>
    /// <param name="literal">Value of the Literal.</param>
    /// <returns></returns>
    public virtual ILiteralNode CreateLiteralNode(string literal)
    {
        return NodeFactory.CreateLiteralNode(literal);
    }

    /// <summary>
    /// Creates a Literal Node with the given Value and Language.
    /// </summary>
    /// <param name="literal">Value of the Literal.</param>
    /// <param name="langSpec">Language Specifier for the Literal.</param>
    /// <returns></returns>
    public virtual ILiteralNode CreateLiteralNode(string literal, string langSpec)
    {
        return NodeFactory.CreateLiteralNode(literal, langSpec);
    }

    /// <summary>
    /// Creates a URI Node for the given URI.
    /// </summary>
    /// <param name="uri">URI.</param>
    /// <returns></returns>
    public virtual IUriNode CreateUriNode(Uri uri)
    {
        return NodeFactory.CreateUriNode(uri);
    }

    /// <summary>
    /// Creates a URI Node for the given QName using the Graphs NamespaceMap to resolve the QName.
    /// </summary>
    /// <param name="qName">QName.</param>
    /// <returns></returns>
    public virtual IUriNode CreateUriNode(string qName)
    {
        return NodeFactory.CreateUriNode(qName);
    }

    /// <summary>
    /// Creates a URI Node that corresponds to the current Base URI of the node factory.
    /// </summary>
    /// <returns></returns>
    public virtual IUriNode CreateUriNode()
    {
        return NodeFactory.CreateUriNode();
    }

    /// <summary>
    /// Creates a Variable Node for the given Variable Name.
    /// </summary>
    /// <param name="varName"></param>
    /// <returns></returns>
    public virtual IVariableNode CreateVariableNode(string varName)
    {
        return NodeFactory.CreateVariableNode(varName);
    }

    /// <summary>
    /// Creates a node that quotes the given triple.
    /// </summary>
    /// <param name="triple">The triple to be the quoted value of the created node.</param>
    /// <returns></returns>
    public virtual ITripleNode CreateTripleNode(Triple triple)
    {
        return NodeFactory.CreateTripleNode(triple);
    }
    #endregion


    #region Node Selection

    /// <summary>
    /// Returns the Blank Node with the given Identifier.
    /// </summary>
    /// <param name="nodeId">The Identifier of the Blank Node to select.</param>
    /// <returns>Either the Blank Node or null if no Node with the given Identifier exists.</returns>
    public abstract IBlankNode GetBlankNode(string nodeId);

    /// <summary>
    /// Returns the LiteralNode with the given Value in the given Language if it exists.
    /// </summary>
    /// <param name="literal">The literal value of the Node to select.</param>
    /// <param name="langspec">The Language Specifier for the Node to select.</param>
    /// <returns>Either the LiteralNode Or null if no Node with the given Value and Language Specifier exists.</returns>
    public abstract ILiteralNode GetLiteralNode(string literal, string langspec);

    /// <summary>
    /// Returns the LiteralNode with the given Value if it exists.
    /// </summary>
    /// <param name="literal">The literal value of the Node to select.</param>
    /// <returns>Either the LiteralNode Or null if no Node with the given Value exists.</returns>
    /// <remarks>The LiteralNode in the Graph must have no Language or DataType set.</remarks>
    public abstract ILiteralNode GetLiteralNode(string literal);

    /// <summary>
    /// Returns the LiteralNode with the given Value and given Data Type if it exists.
    /// </summary>
    /// <param name="literal">The literal value of the Node to select.</param>
    /// <param name="datatype">The Uri for the Data Type of the Literal to select.</param>
    /// <returns>Either the LiteralNode Or null if no Node with the given Value and Data Type exists.</returns>
    public abstract ILiteralNode GetLiteralNode(string literal, Uri datatype);

    /// <summary>
    /// Returns the UriNode with the given QName if it exists.
    /// </summary>
    /// <param name="qname">The QName of the Node to select.</param>
    /// <returns></returns>
    public abstract IUriNode GetUriNode(string qname);

    /// <summary>
    /// Returns the UriNode with the given Uri if it exists.
    /// </summary>
    /// <param name="uri">The Uri of the Node to select.</param>
    /// <returns>Either the UriNode Or null if no Node with the given Uri exists.</returns>
    public abstract IUriNode GetUriNode(Uri uri);

    /// <summary>
    /// Selects the Triple Node with the given Triple value if it exists in the graph.
    /// </summary>
    /// <param name="triple">Triple.</param>
    /// <returns>The triple node if it exists in the graph or else null.</returns>
    public abstract ITripleNode GetTripleNode(Triple triple);

    #endregion

    #region Triple Selection

    /// <summary>
    /// Gets all the Triples involving the given Uri.
    /// </summary>
    /// <param name="uri">The Uri to find Triples involving.</param>
    /// <returns>Zero/More Triples.</returns>
    public abstract IEnumerable<Triple> GetTriples(Uri uri);

    /// <summary>
    /// Gets all the Triples involving the given Node.
    /// </summary>
    /// <param name="n">The Node to find Triples involving.</param>
    /// <returns>Zero/More Triples.</returns>
    public abstract IEnumerable<Triple> GetTriples(INode n);

    /// <summary>
    /// Gets all the Triples with the given Uri as the Object.
    /// </summary>
    /// <param name="u">The Uri to find Triples with it as the Object.</param>
    /// <returns>Zero/More Triples.</returns>
    public abstract IEnumerable<Triple> GetTriplesWithObject(Uri u);

    /// <summary>
    /// Gets all the Triples with the given Node as the Object.
    /// </summary>
    /// <param name="n">The Node to find Triples with it as the Object.</param>
    /// <returns></returns>
    public abstract IEnumerable<Triple> GetTriplesWithObject(INode n);

    /// <summary>
    /// Gets all the Triples with the given Node as the Predicate.
    /// </summary>
    /// <param name="n">The Node to find Triples with it as the Predicate.</param>
    /// <returns></returns>
    public abstract IEnumerable<Triple> GetTriplesWithPredicate(INode n);

    /// <summary>
    /// Gets all the Triples with the given Uri as the Predicate.
    /// </summary>
    /// <param name="u">The Uri to find Triples with it as the Predicate.</param>
    /// <returns>Zero/More Triples.</returns>
    public abstract IEnumerable<Triple> GetTriplesWithPredicate(Uri u);

    /// <summary>
    /// Gets all the Triples with the given Node as the Subject.
    /// </summary>
    /// <param name="n">The Node to find Triples with it as the Subject.</param>
    /// <returns>Zero/More Triples.</returns>
    public abstract IEnumerable<Triple> GetTriplesWithSubject(INode n);

    /// <summary>
    /// Gets all the Triples with the given Uri as the Subject.
    /// </summary>
    /// <param name="u">The Uri to find Triples with it as the Subject.</param>
    /// <returns>Zero/More Triples.</returns>
    public abstract IEnumerable<Triple> GetTriplesWithSubject(Uri u);

    /// <summary>
    /// Selects all Triples with the given Subject and Predicate.
    /// </summary>
    /// <param name="subj">Subject.</param>
    /// <param name="pred">Predicate.</param>
    /// <returns></returns>
    public abstract IEnumerable<Triple> GetTriplesWithSubjectPredicate(INode subj, INode pred);

    /// <summary>
    /// Selects all Triples with the given Subject and Object.
    /// </summary>
    /// <param name="subj">Subject.</param>
    /// <param name="obj">Object.</param>
    /// <returns></returns>
    public abstract IEnumerable<Triple> GetTriplesWithSubjectObject(INode subj, INode obj);

    /// <summary>
    /// Selects all Triples with the given Predicate and Object.
    /// </summary>
    /// <param name="pred">Predicate.</param>
    /// <param name="obj">Object.</param>
    /// <returns></returns>
    public abstract IEnumerable<Triple> GetTriplesWithPredicateObject(INode pred, INode obj);

    /// <inheritdoc/>
    public virtual bool ContainsTriple(Triple t)
    {
        return _triples.Contains(t);
    }

    /// <summary>
    /// Gets whether a given triple is quoted in this graph.
    /// </summary>
    /// <param name="t">Triple to test.</param>
    /// <returns>True if the triple is quoted in this graph, false otherwise.</returns>
    public virtual bool ContainsQuotedTriple(Triple t)
    {
        return _triples.ContainsQuoted(t);
    }

    /// <inheritdoc/>
    public virtual IEnumerable<Triple> GetQuoted(Uri uri) => GetQuoted(new UriNode(uri));
    
    /// <inheritdoc/>
    public abstract IEnumerable<Triple> GetQuoted(INode n);

    /// <inheritdoc/>
    public virtual IEnumerable<Triple> GetQuotedWithObject(Uri u) => GetQuotedWithObject(new UriNode(u));
    
    /// <inheritdoc/>
    public abstract IEnumerable<Triple> GetQuotedWithObject(INode n);
    
    /// <inheritdoc/>
    public abstract IEnumerable<Triple> GetQuotedWithPredicate(INode n);

    /// <inheritdoc/>
    public virtual IEnumerable<Triple> GetQuotedWithPredicate(Uri u) => GetQuotedWithPredicate(new UriNode(u));
    
    /// <inheritdoc/>
    public abstract IEnumerable<Triple> GetQuotedWithSubject(INode n);

    /// <inheritdoc/>
    public virtual IEnumerable<Triple> GetQuotedWithSubject(Uri u) => GetQuotedWithSubject(new UriNode(u));
    
    /// <inheritdoc/>
    public abstract IEnumerable<Triple> GetQuotedWithSubjectPredicate(INode subj, INode pred);
    
    /// <inheritdoc/>
    public abstract IEnumerable<Triple> GetQuotedWithSubjectObject(INode subj, INode obj);
    
    /// <inheritdoc/>
    public abstract IEnumerable<Triple> GetQuotedWithPredicateObject(INode pred, INode obj);

    #endregion

    #region Graph Merging

    /// <summary>
    /// Merges another Graph into the current Graph.
    /// </summary>
    /// <param name="g">Graph to Merge into this Graph.</param>
    /// <remarks>The Graph on which you invoke this method will preserve its Blank Node IDs while the Blank Nodes from the Graph being merged in will be given new IDs as required in the scope of this Graph.</remarks>
    public virtual void Merge(IGraph g)
    {
        Merge(g, false);
    }

    /// <summary>
    /// Merges another Graph into the current Graph.
    /// </summary>
    /// <param name="g">Graph to Merge into this Graph.</param>
    /// <param name="keepOriginalGraphUri">Indicates that the Merge should preserve the Graph URIs of Nodes so they refer to the Graph they originated in.</param>
    /// <remarks>
    /// <para>
    /// The Graph on which you invoke this method will preserve its Blank Node IDs while the Blank Nodes from the Graph being merged in will be given new IDs as required in the scope of this Graph.
    /// </para>
    /// <para>
    /// The Graph will raise the <see cref="MergeRequested">MergeRequested</see> event before the Merge operation which gives any event handlers the oppurtunity to cancel this event.  When the Merge operation is completed the <see cref="Merged">Merged</see> event is raised.
    /// </para>
    /// </remarks>
    public virtual void Merge(IGraph g, bool keepOriginalGraphUri)
    {
        if (ReferenceEquals(this, g)) throw new RdfException("You cannot Merge an RDF Graph with itself");

        // Check that the merge can go ahead
        if (!RaiseMergeRequested()) return;

        // First copy and Prefixes across which aren't defined in this Graph
        NodeFactory.NamespaceMap.Import(g.NamespaceMap);

        if (IsEmpty)
        {
            // Empty Graph so do a quick copy
            Assert(g.Triples);
        }
        else
        {
            // Prepare a mapping of Blank Nodes to Blank Nodes
            var mapping = new Dictionary<INode, IBlankNode>();

            foreach (Triple t in g.Triples)
            {
                INode s = MapBlankNode(t.Subject, mapping);
                INode p = MapBlankNode(t.Predicate, mapping);
                INode o = MapBlankNode(t.Object, mapping);
                Assert(new Triple(s, p, o, t.Context));
            }
        }

        RaiseMerged();
    }

    private INode MapBlankNode(INode node, IDictionary<INode, IBlankNode> mapping)
    {
        switch (node.NodeType)
        {
            case NodeType.Triple:
                var tn = node as ITripleNode;
                return tn == null || tn.Triple.IsGroundTriple
                    ? tn
                    : new TripleNode(new Triple(
                        MapBlankNode(tn.Triple.Subject, mapping),
                        MapBlankNode(tn.Triple.Predicate, mapping),
                        MapBlankNode(tn.Triple.Object, mapping)));

            case NodeType.Blank:
                if (mapping.TryGetValue(node, out IBlankNode mapped)) return mapped;
                IBlankNode tmp = CreateBlankNode();
                mapping.Add(node, tmp);
                return tmp;
         
            default:
                return node;
        }
    }


    /// <inheritdoc />
    public void Unstar()
    {
        RdfStarHelper.Unstar(this);
    }

    

    #endregion

    #region Graph Equality

    /// <summary>
    /// Determines whether a Graph is equal to another Object.
    /// </summary>
    /// <param name="other">Other graph to compare to.</param>
    /// <returns></returns>
    /// <remarks>
    /// <para>
    /// Graph Equality is determined by a somewhat complex algorithm which is explained in the remarks of the other overload for Equals.
    /// </para>
    /// </remarks>
    public bool Equals(IGraph other)
    {
        return Equals(other, out _);
    }

    /// <summary>
    /// Determines whether this Graph is equal to the given Graph.
    /// </summary>
    /// <param name="g">Graph to test for equality.</param>
    /// <param name="mapping">Mapping of Blank Nodes iff the Graphs are equal and contain some Blank Nodes.</param>
    /// <returns></returns>
    /// <remarks>
    /// See <see cref="GraphMatcher"/> for documentation of the equality algorithm used.
    /// </remarks>
    public virtual bool Equals(IGraph g, out Dictionary<INode, INode> mapping)
    {
        // Set the mapping to be null
        mapping = null;

        var matcher = new GraphMatcher();
        if (matcher.Equals(this, g))
        {
            mapping = matcher.Mapping;
            return true;
        }

        return false;
    }

    #endregion

    #region Sub-Graph Matching

    /// <summary>
    /// Checks whether this Graph is a sub-graph of the given Graph.
    /// </summary>
    /// <param name="g">Graph.</param>
    /// <returns></returns>
    public bool IsSubGraphOf(IGraph g)
    {
        return IsSubGraphOf(g, out Dictionary<INode, INode> _);
    }

    /// <summary>
    /// Checks whether this Graph is a sub-graph of the given Graph.
    /// </summary>
    /// <param name="g">Graph.</param>
    /// <param name="mapping">Mapping of Blank Nodes.</param>
    /// <returns></returns>
    public bool IsSubGraphOf(IGraph g, out Dictionary<INode, INode> mapping)
    {
        // Set the mapping to be null
        mapping = null;

        var matcher = new SubGraphMatcher();
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
    /// Checks whether this Graph has the given Graph as a sub-graph.
    /// </summary>
    /// <param name="g">Graph.</param>
    /// <returns></returns>
    public bool HasSubGraph(IGraph g)
    {
        return g.IsSubGraphOf(this);
    }

    /// <summary>
    /// Checks whether this Graph has the given Graph as a sub-graph.
    /// </summary>
    /// <param name="g">Graph.</param>
    /// <param name="mapping">Mapping of Blank Nodes.</param>
    /// <returns></returns>
    public bool HasSubGraph(IGraph g, out Dictionary<INode, INode> mapping)
    {
        return g.IsSubGraphOf(this, out mapping);
    }

    #endregion

    #region Graph Difference

    /// <summary>
    /// Computes the Difference between this Graph the given Graph.
    /// </summary>
    /// <param name="g">Graph.</param>
    /// <returns></returns>
    /// <remarks>
    /// <para>
    /// Produces a report which shows the changes that must be made to this Graph to produce the given Graph.
    /// </para>
    /// </remarks>
    public GraphDiffReport Difference(IGraph g)
    {
        var differ = new GraphDiff();
        return differ.Difference(this, g);
    }

    #endregion

    #region Helper Functions

    /// <summary>
    /// Helper function for Resolving QNames to URIs.
    /// </summary>
    /// <param name="qname">QName to resolve to a Uri.</param>
    /// <returns></returns>
    public virtual Uri ResolveQName(string qname)
    {
        return NodeFactory.ResolveQName(qname);
    }

    /// <summary>
    /// Creates a new unused Blank Node ID and returns it.
    /// </summary>
    /// <returns></returns>
    public virtual string GetNextBlankNodeID()
    {
        return _bnodemapper.GetNextID();
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
    /// Event Handler which handles the <see cref="BaseTripleCollection.TripleAdded">Triple Added</see> event from the underlying Triple Collection by raising the Graph's <see cref="TripleAsserted">TripleAsserted</see> event.
    /// </summary>
    /// <param name="sender">Sender.</param>
    /// <param name="args">Triple Event Arguments.</param>
    protected virtual void OnTripleAsserted(object sender, TripleEventArgs args)
    {
        RaiseTripleAsserted(args);
    }

    /// <summary>
    /// Helper method for raising the <see cref="TripleAsserted">Triple Asserted</see> event manually.
    /// </summary>
    /// <param name="args">Triple Event Arguments.</param>
    protected void RaiseTripleAsserted(TripleEventArgs args)
    {
        args.Graph = this;
        TripleAsserted?.Invoke(this, args);
        RaiseGraphChanged(args);
    }

    /// <summary>
    /// Helper method for raising the <see cref="TripleAsserted">Triple Asserted</see> event manually.
    /// </summary>
    /// <param name="t">Triple.</param>
    protected void RaiseTripleAsserted(Triple t)
    {
        TripleEventHandler d = TripleAsserted;
        GraphEventHandler e = Changed;
        if (d != null || e != null)
        {
            var args = new TripleEventArgs(t, this);
            d?.Invoke(this, args);
            e?.Invoke(this, new GraphEventArgs(this, args));
        }
    }

    /// <summary>
    /// Event Handler which handles the <see cref="BaseTripleCollection.TripleRemoved">Triple Removed</see> event from the underlying Triple Collection by raising the Graph's <see cref="TripleRetracted">Triple Retracted</see> event.
    /// </summary>
    /// <param name="sender">Sender.</param>
    /// <param name="args">Triple Event Arguments.</param>
    protected virtual void OnTripleRetracted(object sender, TripleEventArgs args)
    {
        RaiseTripleRetracted(args);
    }

    /// <summary>
    /// Helper method for raising the <see cref="TripleRetracted">Triple Retracted</see> event manually.
    /// </summary>
    /// <param name="args"></param>
    protected void RaiseTripleRetracted(TripleEventArgs args)
    {
        TripleEventHandler d = TripleRetracted;
        args.Graph = this;
        d?.Invoke(this, args);
        RaiseGraphChanged(args);
    }

    /// <summary>
    /// Helper method for raising the <see cref="TripleRetracted">Triple Retracted</see> event manually.
    /// </summary>
    /// <param name="t">Triple.</param>
    protected void RaiseTripleRetracted(Triple t)
    {
        TripleEventHandler d = TripleRetracted;
        GraphEventHandler e = Changed;
        if (d != null || e != null)
        {
            var args = new TripleEventArgs(t, this, false);
            d?.Invoke(this, args);
            e?.Invoke(this, new GraphEventArgs(this, args));
        }
    }

    /// <summary>
    /// Helper method for raising the <see cref="Changed">Changed</see> event.
    /// </summary>
    /// <param name="args">Triple Event Arguments.</param>
    protected void RaiseGraphChanged(TripleEventArgs args)
    {
        Changed?.Invoke(this, new GraphEventArgs(this, args));
    }

    /// <summary>
    /// Helper method for raising the <see cref="Changed">Changed</see> event.
    /// </summary>
    protected void RaiseGraphChanged()
    {
        Changed?.Invoke(this, new GraphEventArgs(this));
    }

    /// <summary>
    /// Helper method for raising the <see cref="ClearRequested">Clear Requested</see> event and returning whether any of the Event Handlers cancelled the operation.
    /// </summary>
    /// <returns>True if the operation can continue, false if it should be aborted.</returns>
    protected bool RaiseClearRequested()
    {
        CancellableGraphEventHandler d = ClearRequested;
        if (d != null)
        {
            var args = new CancellableGraphEventArgs(this);
            d(this, args);
            return !args.Cancel;
        }
        else
        {
            return true;
        }
    }

    /// <summary>
    /// Helper method for raising the <see cref="Cleared">Cleared</see> event.
    /// </summary>
    protected void RaiseCleared()
    {
        Cleared?.Invoke(this, new GraphEventArgs(this));
    }

    /// <summary>
    /// Helper method for raising the <see cref="MergeRequested">Merge Requested</see> event and returning whether any of the Event Handlers cancelled the operation.
    /// </summary>
    /// <returns>True if the operation can continue, false if it should be aborted.</returns>
    protected bool RaiseMergeRequested()
    {
        CancellableGraphEventHandler d = MergeRequested;
        if (d != null)
        {
            var args = new CancellableGraphEventArgs(this);
            d(this, args);
            return !args.Cancel;
        }
        else
        {
            return true;
        }
    }

    /// <summary>
    /// Helper method for raising the <see cref="Merged">Merged</see> event.
    /// </summary>
    protected void RaiseMerged()
    {
        Merged?.Invoke(this, new GraphEventArgs(this));
    }

    /// <summary>
    /// Helper method for attaching the necessary event Handlers to a Triple Collection.
    /// </summary>
    /// <param name="tripleCollection">Triple Collection.</param>
    /// <remarks>
    /// May be useful if you replace the Triple Collection after instantiation e.g. as done in <see cref="Query.SparqlView">SparqlView</see>'s.
    /// </remarks>
    protected void AttachEventHandlers(BaseTripleCollection tripleCollection)
    {
        tripleCollection.TripleAdded += TripleAddedHandler;
        tripleCollection.TripleRemoved += TripleRemovedHandler;
    }

    /// <summary>
    /// Helper method for detaching the necessary event Handlers from a Triple Collection.
    /// </summary>
    /// <param name="tripleCollection">Triple Collection.</param>
    /// <remarks>
    /// May be useful if you replace the Triple Collection after instantiation e.g. as done in <see cref="Query.SparqlView">SparqlView</see>'s.
    /// </remarks>
    protected void DetachEventHandlers(BaseTripleCollection tripleCollection)
    {
        tripleCollection.TripleAdded -= TripleAddedHandler;
        tripleCollection.TripleRemoved -= TripleRemovedHandler;
    }

    #endregion

    /// <summary>
    /// Disposes of a Graph.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes of the graph and any resources it owns.
    /// </summary>
    /// <param name="disposing"></param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            _disposed = true;
            if (disposing)
            {
                DetachEventHandlers(_triples);
                if (_disposeTriples)
                {
                    _triples.Dispose();
                }
            }
        }
    }
}
