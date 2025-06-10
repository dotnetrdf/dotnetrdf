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

namespace VDS.RDF;

/// <summary>
/// Abstract decorator for Graphs to make it easier to layer functionality on top of existing implementations.
/// </summary>
public abstract class WrapperGraph 
    : IGraph
{
    /// <summary>
    /// Underlying Graph this is a wrapper around.
    /// </summary>
    protected readonly IGraph _g;
    private TripleEventHandler TripleAssertedHandler, TripleRetractedHandler;
    private GraphEventHandler GraphChangedHandler, GraphClearedHandler, GraphMergedHandler;
    private CancellableGraphEventHandler GraphClearRequestedHandler, GraphMergeRequestedHandler;

    /// <summary>
    /// Creates a wrapper around the default Graph implementation, primarily required only for deserialization and requires that the caller call <see cref="WrapperGraph.AttachEventHandlers"/> to properly wire up event handling.
    /// </summary>
    /// <param name="graphName">The name to assign to the new graph.</param>
    protected WrapperGraph(IRefNode graphName = null)
    {
        _g = new Graph(graphName);

        // Create Event Handlers and attach to relevant events so the wrapper propogates events upwards
        TripleAssertedHandler = new TripleEventHandler(OnTripleAsserted);
        TripleRetractedHandler = new TripleEventHandler(OnTripleRetracted);
        GraphChangedHandler = new GraphEventHandler(OnChanged);
        GraphClearedHandler = new GraphEventHandler(OnCleared);
        GraphMergedHandler = new GraphEventHandler(OnMerged);
        GraphClearRequestedHandler = new CancellableGraphEventHandler(OnClearRequested);
        GraphMergeRequestedHandler = new CancellableGraphEventHandler(OnMergeRequested);
    }

    /// <summary>
    /// Creates a new wrapper around the given Graph.
    /// </summary>
    /// <param name="g">Graph.</param>
    protected WrapperGraph(IGraph g)
        : this()
    {
        _g = g ?? throw new ArgumentNullException("graph", "Wrapped Graph cannot be null");
        AttachEventHandlers();
    }

    #region Wrappers around all the standard IGraph stuff

    /// <summary>
    /// Gets/Sets the Base URI of the Graph.
    /// </summary>
    public virtual Uri BaseUri
    {
        get => _g.BaseUri;
        set => _g.BaseUri = value;
    }

    /// <summary>
    /// Gets the name of the graph.
    /// </summary>
    public virtual IRefNode Name
    {
        get
        {
            return _g.Name;
        }
    }

    /// <summary>
    /// Gets whether the Graph is empty.
    /// </summary>
    public virtual bool IsEmpty => _g.IsEmpty;

    /// <summary>
    /// Gets the Namespace Map for the Graph.
    /// </summary>
    public virtual INamespaceMapper NamespaceMap => _g.NamespaceMap;

    /// <summary>
    /// Get or set the factory to use when creating URIs.
    /// </summary>
    public IUriFactory UriFactory
    {
        get => _g.UriFactory;
        set => _g.UriFactory = value;
    }

    /// <inheritdoc />
    public LanguageTagValidationMode LanguageTagValidation
    {
        get => _g.LanguageTagValidation;
        set => _g.LanguageTagValidation = value;
    }


    /// <inheritdoc />
    public virtual IEnumerable<INode> Nodes => _g.Nodes;


    /// <inheritdoc />
    public virtual IEnumerable<INode> AllNodes => _g.AllNodes;

    /// <summary>
    /// Gets the Triple Collection for the Graph.
    /// </summary>
    public virtual BaseTripleCollection Triples => _g.Triples;

    /// <inheritdoc />
    public virtual IEnumerable<Triple> QuotedTriples => _g.QuotedTriples;

    /// <inheritdoc />
    public IEnumerable<INode> QuotedNodes => _g.QuotedNodes;

    /// <inheritdoc />
    public IEnumerable<INode> AllQuotedNodes => _g.AllQuotedNodes;

    /// <summary>
    /// Asserts a Triple in the Graph.
    /// </summary>
    /// <param name="t">Triple.</param>
    public virtual bool Assert(Triple t)
    {
        return _g.Assert(t);
    }

    /// <summary>
    /// Asserts Triples in the Graph.
    /// </summary>
    /// <param name="ts">Triples.</param>
    public virtual bool Assert(IEnumerable<Triple> ts)
    {
        return _g.Assert(ts);
    }

    /// <summary>
    /// Retracts a Triple from the Graph.
    /// </summary>
    /// <param name="t">Triple.</param>
    public virtual bool Retract(Triple t)
    {
        return _g.Retract(t);
    }

    /// <summary>
    /// Retracts Triples from the Graph.
    /// </summary>
    /// <param name="ts">Triples.</param>
    public virtual bool Retract(IEnumerable<Triple> ts)
    {
        return _g.Retract(ts);
    }

    /// <summary>
    /// Clears the Graph.
    /// </summary>
    public virtual void Clear()
    {
        _g.Clear();
    }

    /// <summary>
    /// Get or set whether to normalize literal values on creation.
    /// </summary>
    public virtual bool NormalizeLiteralValues
    {
        get => _g.NormalizeLiteralValues;
        set => _g.NormalizeLiteralValues = value;
    }
    /// <summary>
    /// Creates a new Blank Node with the given Node ID.
    /// </summary>
    /// <param name="nodeId">Node ID.</param>
    /// <returns></returns>
    public virtual IBlankNode CreateBlankNode(string nodeId)
    {
        return _g.CreateBlankNode(nodeId);
    }

    /// <summary>
    /// Creates a new Blank Node.
    /// </summary>
    /// <returns></returns>
    public virtual IBlankNode CreateBlankNode()
    {
        return _g.CreateBlankNode();
    }

    /// <summary>
    /// Gets the next available Blank Node ID.
    /// </summary>
    /// <returns></returns>
    public virtual string GetNextBlankNodeID()
    {
        return _g.GetNextBlankNodeID();
    }

    /// <summary>
    /// Creates a new Graph Literal Node with the given sub-graph.
    /// </summary>
    /// <param name="subgraph">Sub-graph.</param>
    /// <returns></returns>
    public virtual IGraphLiteralNode CreateGraphLiteralNode(IGraph subgraph)
    {
        return _g.CreateGraphLiteralNode(subgraph);
    }

    /// <summary>
    /// Creates a new Graph Literal Node.
    /// </summary>
    /// <returns></returns>
    public virtual IGraphLiteralNode CreateGraphLiteralNode()
    {
        return _g.CreateGraphLiteralNode();
    }

    /// <summary>
    /// Creates a new Literal Node.
    /// </summary>
    /// <param name="literal">Value.</param>
    /// <returns></returns>
    public virtual ILiteralNode CreateLiteralNode(string literal)
    {
        return _g.CreateLiteralNode(literal);
    }

    /// <summary>
    /// Creates a new Literal Node with the given Datatype.
    /// </summary>
    /// <param name="literal">Value.</param>
    /// <param name="datatype">Datatype URI.</param>
    /// <returns></returns>
    public virtual ILiteralNode CreateLiteralNode(string literal, Uri datatype)
    {
        return _g.CreateLiteralNode(literal, datatype);
    }

    /// <summary>
    /// Creates a new Literal Node with the given Language.
    /// </summary>
    /// <param name="literal">Value.</param>
    /// <param name="langSpec">Language.</param>
    /// <returns></returns>
    public virtual ILiteralNode CreateLiteralNode(string literal, string langSpec)
    {
        return _g.CreateLiteralNode(literal, langSpec);
    }

    /// <summary>
    /// Creates a new URI Node that references the Graphs Base URI.
    /// </summary>
    /// <returns></returns>
    public virtual IUriNode CreateUriNode()
    {
        return _g.CreateUriNode();
    }

    /// <summary>
    /// Creates a new URI Node from a QName.
    /// </summary>
    /// <param name="qName">QName.</param>
    /// <returns></returns>
    public virtual IUriNode CreateUriNode(string qName)
    {
        return _g.CreateUriNode(qName);
    }

    /// <summary>
    /// Creates a new URI Node.
    /// </summary>
    /// <param name="uri">URI.</param>
    /// <returns></returns>
    public virtual IUriNode CreateUriNode(Uri uri)
    {
        return _g.CreateUriNode(uri);
    }

    /// <summary>
    /// Creates a new Variable Node.
    /// </summary>
    /// <param name="varName">Variable Name.</param>
    /// <returns></returns>
    public virtual IVariableNode CreateVariableNode(string varName)
    {
        return _g.CreateVariableNode(varName);
    }

    /// <inheritdoc />
    public virtual ITripleNode CreateTripleNode(Triple triple)
    {
        return _g.CreateTripleNode(triple);
    }

    /// <summary>
    /// Attempts to get the Blank Node with the given ID.
    /// </summary>
    /// <param name="nodeId">Node ID.</param>
    /// <returns>The Node if it exists or null.</returns>
    public virtual IBlankNode GetBlankNode(string nodeId)
    {
        return _g.GetBlankNode(nodeId);
    }

    /// <summary>
    /// Attempts to get the Literal Node with the given Value and Language.
    /// </summary>
    /// <param name="literal">Value.</param>
    /// <param name="langspec">Language.</param>
    /// <returns>The Node if it exists or null.</returns>
    public virtual ILiteralNode GetLiteralNode(string literal, string langspec)
    {
        return _g.GetLiteralNode(literal, langspec);
    }

    /// <summary>
    /// Attempts to get the Literal Node with the given Value.
    /// </summary>
    /// <param name="literal">Value.</param>
    /// <returns>The Node if it exists or null.</returns>
    public virtual ILiteralNode GetLiteralNode(string literal)
    {
        return _g.GetLiteralNode(literal);
    }

    /// <summary>
    /// Attempts to get the Literal Node with the given Value and Datatype.
    /// </summary>
    /// <param name="literal">Value.</param>
    /// <param name="datatype">Datatype URI.</param>
    /// <returns>The Node if it exists or null otherwise.</returns>
    public virtual ILiteralNode GetLiteralNode(string literal, Uri datatype)
    {
        return _g.GetLiteralNode(literal, datatype);
    }


    /// <summary>
    /// Selects the Triple Node with the given Triple value if it exists in the graph.
    /// </summary>
    /// <param name="triple">Triple.</param>
    /// <returns>The triple node if it exists in the graph or else null.</returns>
    public virtual ITripleNode GetTripleNode(Triple triple)
    {
        return _g.GetTripleNode(triple);
    }

    /// <summary>
    /// Gets all the Triples involving the given URI.
    /// </summary>
    /// <param name="uri">The URI to find Triples involving.</param>
    /// <returns>Zero/More Triples.</returns>
    public virtual IEnumerable<Triple> GetTriples(Uri uri)
    {
        return _g.GetTriples(uri);
    }

    /// <summary>
    /// Gets all the Triples involving the given Node.
    /// </summary>
    /// <param name="n">The Node to find Triples involving.</param>
    /// <returns>Zero/More Triples.</returns>
    public virtual IEnumerable<Triple> GetTriples(INode n)
    {
        return _g.GetTriples(n);
    }

    /// <summary>
    /// Gets all the Triples with the given URI as the Object.
    /// </summary>
    /// <param name="u">The URI to find Triples with it as the Object.</param>
    /// <returns>Zero/More Triples.</returns>
    public virtual IEnumerable<Triple> GetTriplesWithObject(Uri u)
    {
        return _g.GetTriplesWithObject(u);
    }

    /// <summary>
    /// Gets all the Triples with the given Node as the Object.
    /// </summary>
    /// <param name="n">The Node to find Triples with it as the Object.</param>
    /// <returns></returns>
    public virtual IEnumerable<Triple> GetTriplesWithObject(INode n)
    {
        return _g.GetTriplesWithObject(n);
    }

    /// <summary>
    /// Gets all the Triples with the given Node as the Predicate.
    /// </summary>
    /// <param name="n">The Node to find Triples with it as the Predicate.</param>
    /// <returns></returns>
    public virtual IEnumerable<Triple> GetTriplesWithPredicate(INode n)
    {
        return _g.GetTriplesWithPredicate(n);
    }

    /// <summary>
    /// Gets all the Triples with the given Uri as the Predicate.
    /// </summary>
    /// <param name="u">The Uri to find Triples with it as the Predicate.</param>
    /// <returns>Zero/More Triples.</returns>
    public virtual IEnumerable<Triple> GetTriplesWithPredicate(Uri u)
    {
        return _g.GetTriplesWithPredicate(u);
    }

    /// <summary>
    /// Gets all the Triples with the given Node as the Subject.
    /// </summary>
    /// <param name="n">The Node to find Triples with it as the Subject.</param>
    /// <returns>Zero/More Triples.</returns>
    public virtual IEnumerable<Triple> GetTriplesWithSubject(INode n)
    {
        return _g.GetTriplesWithSubject(n);
    }

    /// <summary>
    /// Gets all the Triples with the given Uri as the Subject.
    /// </summary>
    /// <param name="u">The Uri to find Triples with it as the Subject.</param>
    /// <returns>Zero/More Triples.</returns>
    public virtual IEnumerable<Triple> GetTriplesWithSubject(Uri u)
    {
        return _g.GetTriplesWithSubject(u);
    }

    /// <summary>
    /// Selects all Triples with the given Subject and Predicate.
    /// </summary>
    /// <param name="subj">Subject.</param>
    /// <param name="pred">Predicate.</param>
    /// <returns></returns>
    public virtual IEnumerable<Triple> GetTriplesWithSubjectPredicate(INode subj, INode pred)
    {
        return _g.GetTriplesWithSubjectPredicate(subj, pred);
    }

    /// <summary>
    /// Selects all Triples with the given Subject and Object.
    /// </summary>
    /// <param name="subj">Subject.</param>
    /// <param name="obj">Object.</param>
    /// <returns></returns>
    public virtual IEnumerable<Triple> GetTriplesWithSubjectObject(INode subj, INode obj)
    {
        return _g.GetTriplesWithSubjectObject(subj, obj);
    }

    /// <summary>
    /// Selects all Triples with the given Predicate and Object.
    /// </summary>
    /// <param name="pred">Predicate.</param>
    /// <param name="obj">Object.</param>
    /// <returns></returns>
    public virtual IEnumerable<Triple> GetTriplesWithPredicateObject(INode pred, INode obj)
    {
        return _g.GetTriplesWithPredicateObject(pred, obj);
    }

    /// <inheritdoc/>
    public IEnumerable<Triple> GetQuoted(Uri uri)
    {
        return _g.GetQuoted(uri);
    }

    /// <inheritdoc/>
    public IEnumerable<Triple> GetQuoted(INode n)
    {
        return _g.GetQuoted(n);
    }

    /// <inheritdoc/>
    public IEnumerable<Triple> GetQuotedWithObject(Uri u)
    {
        return _g.GetQuotedWithObject(u);
    }

    /// <inheritdoc/>
    public IEnumerable<Triple> GetQuotedWithObject(INode n)
    {
        return _g.GetQuotedWithObject(n);
    }

    /// <inheritdoc/>
    public IEnumerable<Triple> GetQuotedWithPredicate(INode n)
    {
        return _g.GetQuotedWithPredicate(n);
    }

    /// <inheritdoc/>
    public IEnumerable<Triple> GetQuotedWithPredicate(Uri u)
    {
        return _g.GetQuotedWithPredicate(u);
    }

    /// <inheritdoc/>
    public IEnumerable<Triple> GetQuotedWithSubject(INode n)
    {
        return _g.GetQuotedWithSubject(n);
    }

    /// <inheritdoc/>
    public IEnumerable<Triple> GetQuotedWithSubject(Uri u)
    {
        return _g.GetQuotedWithSubject(u);
    }

    /// <inheritdoc/>
    public IEnumerable<Triple> GetQuotedWithSubjectPredicate(INode subj, INode pred)
    {
        return _g.GetQuotedWithSubjectPredicate(subj, pred);
    }

    /// <inheritdoc/>
    public IEnumerable<Triple> GetQuotedWithSubjectObject(INode subj, INode obj)
    {
        return _g.GetQuotedWithSubjectObject(subj, obj);
    }

    /// <inheritdoc/>
    public IEnumerable<Triple> GetQuotedWithPredicateObject(INode pred, INode obj)
    {
        return _g.GetQuotedWithPredicateObject(pred, obj);
    }

    /// <summary>
    /// Returns the UriNode with the given QName if it exists.
    /// </summary>
    /// <param name="qname">The QName of the Node to select.</param>
    /// <returns></returns>
    public virtual IUriNode GetUriNode(string qname)
    {
        return _g.GetUriNode(qname);
    }

    /// <summary>
    /// Returns the UriNode with the given Uri if it exists.
    /// </summary>
    /// <param name="uri">The Uri of the Node to select.</param>
    /// <returns>Either the UriNode Or null if no Node with the given Uri exists.</returns>
    public virtual IUriNode GetUriNode(Uri uri)
    {
        return _g.GetUriNode(uri);
    }

    /// <summary>
    /// Gets whether a given Triple is either quoted or asserted in this Graph.
    /// </summary>
    /// <param name="t">Triple to test.</param>
    /// <returns></returns>
    public virtual bool ContainsTriple(Triple t)
    {
        return _g.ContainsTriple(t);
    }

    /// <summary>
    /// Gets whether a given triple is quoted in this graph.
    /// </summary>
    /// <param name="t">Triple to test.</param>
    /// <returns>True if the triple is quoted in this graph, false otherwise.</returns>
    public virtual bool ContainsQuotedTriple(Triple t)
    {
        return _g.ContainsQuotedTriple(t);
    }

    /// <summary>
    /// Merges another Graph into the current Graph.
    /// </summary>
    /// <param name="g">Graph to Merge into this Graph.</param>
    /// <remarks>The Graph on which you invoke this method will preserve its Blank Node IDs while the Blank Nodes from the Graph being merged in will be given new IDs as required in the scope of this Graph.</remarks>
    public virtual void Merge(IGraph g)
    {
        _g.Merge(g);
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
        _g.Merge(g, keepOriginalGraphUri);
    }

    /// <summary>
    /// Determines whether this graph is equal to another graph.
    /// </summary>
    /// <param name="other">Graph to test.</param>
    /// <returns></returns>
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
    /// <para>
    /// The algorithm used to determine Graph equality is based in part on a Iterative Vertex Classification Algorithm described in a Technical Report from HP by Jeremy J Carroll -. <a href="http://www.hpl.hp.com/techreports/2001/HPL-2001-293.html">Matching RDF Graphs</a>
    /// </para>
    /// <para>
    /// Graph Equality is determined according to the following algorithm:.
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
    public virtual bool Equals(IGraph g, out Dictionary<INode, INode> mapping)
    {
        return _g.Equals(g, out mapping);
    }

    /// <summary>
    /// Checks whether this Graph is a sub-graph of the given Graph.
    /// </summary>
    /// <param name="g">Graph.</param>
    /// <returns></returns>
    public virtual bool IsSubGraphOf(IGraph g)
    {
        return _g.IsSubGraphOf(g);
    }

    /// <summary>
    /// Checks whether this Graph is a sub-graph of the given Graph.
    /// </summary>
    /// <param name="g">Graph.</param>
    /// <param name="mapping">Mapping of Blank Nodes.</param>
    /// <returns></returns>
    public virtual bool IsSubGraphOf(IGraph g, out Dictionary<INode, INode> mapping)
    {
        return _g.IsSubGraphOf(g, out mapping);
    }

    /// <summary>
    /// Checks whether this Graph has the given Graph as a sub-graph.
    /// </summary>
    /// <param name="g">Graph.</param>
    /// <returns></returns>
    public virtual bool HasSubGraph(IGraph g)
    {
        return _g.HasSubGraph(g);
    }

    /// <summary>
    /// Checks whether this Graph has the given Graph as a sub-graph.
    /// </summary>
    /// <param name="g">Graph.</param>
    /// <param name="mapping">Mapping of Blank Nodes.</param>
    /// <returns></returns>
    public virtual bool HasSubGraph(IGraph g, out Dictionary<INode, INode> mapping)
    {
        return _g.HasSubGraph(g, out mapping);
    }

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
    public virtual GraphDiffReport Difference(IGraph g)
    {
        return _g.Difference(g);
    }

    /// <inheritdoc />
    public void Unstar()
    {
        _g.Unstar();
    }

    /// <summary>
    /// Helper function for Resolving QNames to URIs.
    /// </summary>
    /// <param name="qname">QName to resolve to a Uri.</param>
    /// <returns></returns>
    public virtual Uri ResolveQName(string qname)
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
        TripleEventHandler d = TripleAsserted;
        args.Graph = this;
        if (d != null)
        {
            d(this, args);
        }
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
            if (d != null) d(this, args);
            if (e != null) e(this, new GraphEventArgs(this, args));
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
        if (d != null)
        {
            d(this, args);
        }
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
            if (d != null) d(this, args);
            if (e != null) e(this, new GraphEventArgs(this, args));
        }
    }

    /// <summary>
    /// Event handler to help propogate Graph events from the underlying graph.
    /// </summary>
    /// <param name="sender">Sender.</param>
    /// <param name="args">Arguments.</param>
    protected virtual void OnChanged(object sender, GraphEventArgs args)
    {
        RaiseGraphChanged(args.TripleEvent);
    }

    /// <summary>
    /// Helper method for raising the <see cref="Changed">Changed</see> event.
    /// </summary>
    /// <param name="args">Triple Event Arguments.</param>
    protected void RaiseGraphChanged(TripleEventArgs args)
    {
        GraphEventHandler d = Changed;
        if (d != null)
        {
            d(this, new GraphEventArgs(this, args));
        }
    }

    /// <summary>
    /// Helper method for raising the <see cref="Changed">Changed</see> event.
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
    /// Event handler to help propogate Graph events from the underlying graph.
    /// </summary>
    /// <param name="sender">Sender.</param>
    /// <param name="args">Arguments.</param>
    protected virtual void OnClearRequested(object sender, CancellableGraphEventArgs args)
    {
        RaiseClearRequested(args);
    }

    /// <summary>
    /// Helper method for raising the <see cref="ClearRequested">Clear Requested</see> event and returning whether any of the Event Handlers cancelled the operation.
    /// </summary>
    /// <returns>True if the operation can continue, false if it should be aborted.</returns>
    protected void RaiseClearRequested(CancellableGraphEventArgs args)
    {
        CancellableGraphEventHandler d = ClearRequested;
        if (d != null)
        {
            d(this, args);
        }
    }

    /// <summary>
    /// Event handler to help propogate Graph events from the underlying graph.
    /// </summary>
    /// <param name="sender">Sender.</param>
    /// <param name="args">Arguments.</param>
    protected virtual void OnCleared(object sender, GraphEventArgs args)
    {
        RaiseCleared();
    }

    /// <summary>
    /// Helper method for raising the <see cref="Cleared">Cleared</see> event.
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
    /// Event handler to help propogate Graph events from the underlying graph.
    /// </summary>
    /// <param name="sender">Sender.</param>
    /// <param name="args">Arguments.</param>
    protected virtual void OnMergeRequested(object sender, CancellableGraphEventArgs args)
    {
        RaiseMergeRequested(args);
    }

    /// <summary>
    /// Helper method for raising the <see cref="MergeRequested">Merge Requested</see> event and returning whether any of the Event Handlers cancelled the operation.
    /// </summary>
    /// <returns>True if the operation can continue, false if it should be aborted.</returns>
    protected void RaiseMergeRequested(CancellableGraphEventArgs args)
    {
        CancellableGraphEventHandler d = MergeRequested;
        if (d != null)
        {
            d(this, args);
        }
    }

    /// <summary>
    /// Event handler to help propogate Graph events from the underlying graph.
    /// </summary>
    /// <param name="sender">Sender.</param>
    /// <param name="args">Arguments.</param>
    protected virtual void OnMerged(object sender, GraphEventArgs args)
    {
        RaiseMerged();
    }

    /// <summary>
    /// Helper method for raising the <see cref="Merged">Merged</see> event.
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
    /// Helper method for attaching the necessary event handlers to the underlying graph.
    /// </summary>
    protected void AttachEventHandlers()
    {
        // Wire up handlers for all the Graph level events
        _g.Cleared += GraphClearedHandler;
        _g.Changed += GraphChangedHandler;
        _g.Merged += GraphMergedHandler;
        _g.TripleAsserted += TripleAssertedHandler;
        _g.TripleRetracted += TripleRetractedHandler;
    }

    #endregion

    /// <summary>
    /// Disposes of the wrapper and in doing so disposes of the underlying graph.
    /// </summary>
    public virtual void Dispose()
    {
        _g.Dispose();
    }

}

