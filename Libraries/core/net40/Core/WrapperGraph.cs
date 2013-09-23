/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
#if !NO_DATA
using System.Data;
#endif
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
    /// Abstract decorator for Graphs to make it easier to layer functionality on top of existing implementations
    /// </summary>
#if !SILVERLIGHT
    [Serializable, XmlRoot(ElementName="graph")]
#endif
    public abstract class WrapperGraph 
        : IGraph
#if !SILVERLIGHT
        , ISerializable
#endif
    {
        /// <summary>
        /// Underlying Graph this is a wrapper around
        /// </summary>
        protected readonly IGraph _g;
        private TripleEventHandler TripleAssertedHandler, TripleRetractedHandler;
        private GraphEventHandler GraphChangedHandler, GraphClearedHandler, GraphMergedHandler;
        private CancellableGraphEventHandler GraphClearRequestedHandler, GraphMergeRequestedHandler;

        /// <summary>
        /// Creates a wrapper around the default Graph implementation, primarily required only for deserialization and requires that the caller call <see cref="WrapperGraph.AttachEventHandlers"/> to properly wire up event handling
        /// </summary>
        protected WrapperGraph()
        {
            this._g = new Graph();

            //Create Event Handlers and attach to relevant events so the wrapper propogates events upwards
            this.TripleAssertedHandler = new TripleEventHandler(this.OnTripleAsserted);
            this.TripleRetractedHandler = new TripleEventHandler(this.OnTripleRetracted);
            this.GraphChangedHandler = new GraphEventHandler(this.OnChanged);
            this.GraphClearedHandler = new GraphEventHandler(this.OnCleared);
            this.GraphMergedHandler = new GraphEventHandler(this.OnMerged);
            this.GraphClearRequestedHandler = new CancellableGraphEventHandler(this.OnClearRequested);
            this.GraphMergeRequestedHandler = new CancellableGraphEventHandler(this.OnMergeRequested);
        }

        /// <summary>
        /// Creates a new wrapper around the given Graph
        /// </summary>
        /// <param name="g">Graph</param>
        public WrapperGraph(IGraph g)
            : this()
        {
            if (g == null) throw new ArgumentNullException("graph", "Wrapped Graph cannot be null");
            this._g = g;
            this.AttachEventHandlers();
        }      

#if !SILVERLIGHT

        /// <summary>
        /// Deserialization Constructor
        /// </summary>
        /// <param name="info">Serialization Information</param>
        /// <param name="context">Streaming Context</param>
        protected WrapperGraph(SerializationInfo info, StreamingContext context)
            : this() 
        {
            String graphType = info.GetString("graphType");
            Type t = Type.GetType(graphType);
            if (t == null) throw new ArgumentException("Invalid serialization information, graph type '" + graphType + "' is not available in your environment");
            this._g = (IGraph)info.GetValue("innerGraph", t);
            this.AttachEventHandlers();
        }

#endif

        #region Wrappers around all the standard IGraph stuff

        /// <summary>
        /// Gets the number of triples in the graph
        /// </summary>
        public virtual long Count
        {
            get
            {
                return this._g.Count;
            }
        }

        /// <summary>
        /// Gets whether the Graph is empty
        /// </summary>
        public virtual bool IsEmpty
        {
            get 
            { 
                return this._g.IsEmpty; 
            }
        }

        /// <summary>
        /// Gets the Namespace Map for the Graph
        /// </summary>
        public virtual INamespaceMapper Namespaces
        {
            get
            { 
                return this._g.Namespaces; 
            }
        }

        /// <summary>
        /// Gets the nodes that are used as vertices in the graph i.e. those which occur in the subject or object position of a triple
        /// </summary>
        public virtual IEnumerable<INode> Vertices
        {
            get 
            { 
                return this._g.Vertices; 
            }
        }

        /// <summary>
        /// Gets the nodes that are used as edges in the graph i.e. those which occur in the predicate position of a triple
        /// </summary>
        public virtual IEnumerable<INode> Edges
        {
            get { return this._g.Edges; }
        }

        /// <summary>
        /// Gets the triples in the graph
        /// </summary>
        public virtual IEnumerable<Triple> Triples
        {
            get 
            {
                return this._g.Triples; 
            }
        }

        /// <summary>
        /// Gets the quads in the graph
        /// </summary>
        public virtual IEnumerable<Quad> Quads
        {
            get
            {
                return this._g.Quads;
            }
        }

        /// <summary>
        /// Asserts a Triple in the Graph
        /// </summary>
        /// <param name="t">Triple</param>
        public virtual bool Assert(Triple t)
        {
            return this._g.Assert(t);
        }

        /// <summary>
        /// Asserts Triples in the Graph
        /// </summary>
        /// <param name="ts">Triples</param>
        public virtual bool Assert(IEnumerable<Triple> ts)
        {
            return this._g.Assert(ts);
        }

        /// <summary>
        /// Retracts a Triple from the Graph
        /// </summary>
        /// <param name="t">Triple</param>
        public virtual bool Retract(Triple t)
        {
            return this._g.Retract(t);
        }

        /// <summary>
        /// Retracts Triples from the Graph
        /// </summary>
        /// <param name="ts">Triples</param>
        public virtual bool Retract(IEnumerable<Triple> ts)
        {
            return this._g.Retract(ts);
        }

        /// <summary>
        /// Clears the Graph
        /// </summary>
        public virtual void Clear()
        {
            this._g.Clear();
        }

        /// <summary>
        /// Creates a new Blank Node with the given Node ID
        /// </summary>
        /// <param name="nodeId">Node ID</param>
        /// <returns></returns>
        public virtual IBlankNode CreateBlankNode(string nodeId)
        {
            return this._g.CreateBlankNode(nodeId);
        }

        /// <summary>
        /// Creates a new Blank Node
        /// </summary>
        /// <returns></returns>
        public virtual IBlankNode CreateBlankNode()
        {
            return this._g.CreateBlankNode();
        }

        /// <summary>
        /// Gets the next available Blank Node ID
        /// </summary>
        /// <returns></returns>
        [Obsolete("Obsolete, no longer used", true)]
        public string GetNextBlankNodeID()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Creates a new Graph Literal Node with the given sub-graph
        /// </summary>
        /// <param name="subgraph">Sub-graph</param>
        /// <returns></returns>
        public virtual IGraphLiteralNode CreateGraphLiteralNode(IGraph subgraph)
        {
            return this._g.CreateGraphLiteralNode(subgraph);
        }

        /// <summary>
        /// Creates a new Graph Literal Node
        /// </summary>
        /// <returns></returns>
        public virtual IGraphLiteralNode CreateGraphLiteralNode()
        {
            return this._g.CreateGraphLiteralNode();
        }

        /// <summary>
        /// Creates a new Literal Node
        /// </summary>
        /// <param name="literal">Value</param>
        /// <returns></returns>
        public virtual ILiteralNode CreateLiteralNode(string literal)
        {
            return this._g.CreateLiteralNode(literal);
        }

        /// <summary>
        /// Creates a new Literal Node with the given Datatype
        /// </summary>
        /// <param name="literal">Value</param>
        /// <param name="datatype">Datatype URI</param>
        /// <returns></returns>
        public virtual ILiteralNode CreateLiteralNode(string literal, Uri datatype)
        {
            return this._g.CreateLiteralNode(literal, datatype);
        }

        /// <summary>
        /// Creates a new Literal Node with the given Language
        /// </summary>
        /// <param name="literal">Value</param>
        /// <param name="langspec">Language</param>
        /// <returns></returns>
        public virtual ILiteralNode CreateLiteralNode(string literal, string langspec)
        {
            return this._g.CreateLiteralNode(literal, langspec);
        }

        /// <summary>
        /// Creates a new URI Node from a QName
        /// </summary>
        /// <param name="qname">QName</param>
        /// <returns></returns>
        public virtual IUriNode CreateUriNode(string qname)
        {
            return this._g.CreateUriNode(qname);
        }

        /// <summary>
        /// Creates a new URI Node
        /// </summary>
        /// <param name="uri">URI</param>
        /// <returns></returns>
        public virtual IUriNode CreateUriNode(Uri uri)
        {
            return this._g.CreateUriNode(uri);
        }

        /// <summary>
        /// Creates a new Variable Node
        /// </summary>
        /// <param name="varname">Variable Name</param>
        /// <returns></returns>
        public virtual IVariableNode CreateVariableNode(String varname)
        {
            return this._g.CreateVariableNode(varname);
        }

        public virtual IEnumerable<Triple> Find(INode s, INode p, INode o)
        {
            return this._g.Find(s, p, o);
        } 

        /// <summary>
        /// Gets whether a given Triple exists in this Graph
        /// </summary>
        /// <param name="t">Triple to test</param>
        /// <returns></returns>
        public virtual bool ContainsTriple(Triple t)
        {
            return this._g.ContainsTriple(t);
        }

        /// <summary>
        /// Merges another Graph into the current Graph
        /// </summary>
        /// <param name="g">Graph to Merge into this Graph</param>
        /// <remarks>The Graph on which you invoke this method will preserve its Blank Node IDs while the Blank Nodes from the Graph being merged in will be given new IDs as required in the scope of this Graph.</remarks>
        public virtual void Merge(IGraph g)
        {
            this._g.Merge(g);
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
                return this.Equals((IGraph)obj, out temp);
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
        public virtual bool Equals(IGraph g, out Dictionary<INode, INode> mapping)
        {
            return this._g.Equals(g, out mapping);
        }

        /// <summary>
        /// Checks whether this Graph is a sub-graph of the given Graph
        /// </summary>
        /// <param name="g">Graph</param>
        /// <returns></returns>
        public virtual bool IsSubGraphOf(IGraph g)
        {
            return this._g.IsSubGraphOf(g);
        }

        /// <summary>
        /// Checks whether this Graph is a sub-graph of the given Graph
        /// </summary>
        /// <param name="g">Graph</param>
        /// <param name="mapping">Mapping of Blank Nodes</param>
        /// <returns></returns>
        public virtual bool IsSubGraphOf(IGraph g, out Dictionary<INode, INode> mapping)
        {
            return this._g.IsSubGraphOf(g, out mapping);
        }

        /// <summary>
        /// Checks whether this Graph has the given Graph as a sub-graph
        /// </summary>
        /// <param name="g">Graph</param>
        /// <returns></returns>
        public virtual bool HasSubGraph(IGraph g)
        {
            return this._g.HasSubGraph(g);
        }

        /// <summary>
        /// Checks whether this Graph has the given Graph as a sub-graph
        /// </summary>
        /// <param name="g">Graph</param>
        /// <param name="mapping">Mapping of Blank Nodes</param>
        /// <returns></returns>
        public virtual bool HasSubGraph(IGraph g, out Dictionary<INode, INode> mapping)
        {
            return this._g.HasSubGraph(g, out mapping);
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
        public virtual GraphDiffReport Difference(IGraph g)
        {
            return this._g.Difference(g);
        }

        /// <summary>
        /// Helper function for Resolving QNames to URIs
        /// </summary>
        /// <param name="qname">QName to resolve to a Uri</param>
        /// <returns></returns>
        public virtual Uri ResolveQName(string qname)
        {
            return this._g.ResolveQName(qname);
        }

#if !NO_DATA

        /// <summary>
        /// Converts the wrapped graph into a DataTable
        /// </summary>
        /// <returns></returns>
        public virtual DataTable ToDataTable()
        {
            return this._g.ToDataTable();
        }

#endif

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
            this.RaiseTripleAsserted(args);
        }

        /// <summary>
        /// Helper method for raising the <see cref="TripleAsserted">Triple Asserted</see> event manually
        /// </summary>
        /// <param name="args">Triple Event Arguments</param>
        protected void RaiseTripleAsserted(TripleEventArgs args)
        {
            TripleEventHandler d = this.TripleAsserted;
            args.Graph = this;
            if (d != null)
            {
                d(this, args);
            }
            this.RaiseGraphChanged(args);
        }

        /// <summary>
        /// Helper method for raising the <see cref="TripleAsserted">Triple Asserted</see> event manually
        /// </summary>
        /// <param name="t">Triple</param>
        protected void RaiseTripleAsserted(Triple t)
        {
            TripleEventHandler d = this.TripleAsserted;
            GraphEventHandler e = this.Changed;
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
            this.RaiseTripleRetracted(args);
        }

        /// <summary>
        /// Helper method for raising the <see cref="TripleRetracted">Triple Retracted</see> event manually
        /// </summary>
        /// <param name="args"></param>
        protected void RaiseTripleRetracted(TripleEventArgs args)
        {
            TripleEventHandler d = this.TripleRetracted;
            args.Graph = this;
            if (d != null)
            {
                d(this, args);
            }
            this.RaiseGraphChanged(args);
        }

        /// <summary>
        /// Helper method for raising the <see cref="TripleRetracted">Triple Retracted</see> event manually
        /// </summary>
        /// <param name="t">Triple</param>
        protected void RaiseTripleRetracted(Triple t)
        {
            TripleEventHandler d = this.TripleRetracted;
            GraphEventHandler e = this.Changed;
            if (d != null || e != null)
            {
                TripleEventArgs args = new TripleEventArgs(t, this, false);
                if (d != null) d(this, args);
                if (e != null) e(this, new GraphEventArgs(this, args));
            }
        }

        /// <summary>
        /// Event handler to help propogate Graph events from the underlying graph
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="args">Arguments</param>
        protected virtual void OnChanged(Object sender, GraphEventArgs args)
        {
            this.RaiseGraphChanged(args.TripleEvent);
        }

        /// <summary>
        /// Helper method for raising the <see cref="Changed">Changed</see> event
        /// </summary>
        /// <param name="args">Triple Event Arguments</param>
        protected void RaiseGraphChanged(TripleEventArgs args)
        {
            GraphEventHandler d = this.Changed;
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
            GraphEventHandler d = this.Changed;
            if (d != null)
            {
                d(this, new GraphEventArgs(this));
            }
        }

        /// <summary>
        /// Event handler to help propogate Graph events from the underlying graph
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="args">Arguments</param>
        protected virtual void OnClearRequested(Object sender, CancellableGraphEventArgs args)
        {
            this.RaiseClearRequested(args);
        }

        /// <summary>
        /// Helper method for raising the <see cref="ClearRequested">Clear Requested</see> event and returning whether any of the Event Handlers cancelled the operation
        /// </summary>
        /// <returns>True if the operation can continue, false if it should be aborted</returns>
        protected void RaiseClearRequested(CancellableGraphEventArgs args)
        {
            CancellableGraphEventHandler d = this.ClearRequested;
            if (d != null)
            {
                d(this, args);
            }
        }

        /// <summary>
        /// Event handler to help propogate Graph events from the underlying graph
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="args">Arguments</param>
        protected virtual void OnCleared(Object sender, GraphEventArgs args)
        {
            this.RaiseCleared();
        }

        /// <summary>
        /// Helper method for raising the <see cref="Cleared">Cleared</see> event
        /// </summary>
        protected void RaiseCleared()
        {
            GraphEventHandler d = this.Cleared;
            if (d != null)
            {
                d(this, new GraphEventArgs(this));
            }
        }

        /// <summary>
        /// Event handler to help propogate Graph events from the underlying graph
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="args">Arguments</param>
        protected virtual void OnMergeRequested(Object sender, CancellableGraphEventArgs args)
        {
            this.RaiseMergeRequested(args);
        }

        /// <summary>
        /// Helper method for raising the <see cref="MergeRequested">Merge Requested</see> event and returning whether any of the Event Handlers cancelled the operation
        /// </summary>
        /// <returns>True if the operation can continue, false if it should be aborted</returns>
        protected void RaiseMergeRequested(CancellableGraphEventArgs args)
        {
            CancellableGraphEventHandler d = this.MergeRequested;
            if (d != null)
            {
                d(this, args);
            }
        }

        /// <summary>
        /// Event handler to help propogate Graph events from the underlying graph
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="args">Arguments</param>
        protected virtual void OnMerged(Object sender, GraphEventArgs args)
        {
            this.RaiseMerged();
        }

        /// <summary>
        /// Helper method for raising the <see cref="Merged">Merged</see> event
        /// </summary>
        protected void RaiseMerged()
        {
            GraphEventHandler d = this.Merged;
            if (d != null)
            {
                d(this, new GraphEventArgs(this));
            }
        }

        /// <summary>
        /// Helper method for attaching the necessary event handlers to the underlying graph
        /// </summary>
        protected void AttachEventHandlers()
        {
            //Wire up handlers for all the Graph level events
            this._g.Cleared += this.GraphClearedHandler;
            this._g.Changed += this.GraphChangedHandler;
            this._g.Merged += this.GraphMergedHandler;
            this._g.TripleAsserted += this.TripleAssertedHandler;
            this._g.TripleRetracted += this.TripleRetractedHandler;
        }

        #endregion

        /// <summary>
        /// Disposes of the wrapper and in doing so disposes of the underlying graph
        /// </summary>
        public virtual void Dispose()
        {
            this._g.Dispose();
        }

#if !SILVERLIGHT

        #region ISerializable Members

        /// <summary>
        /// Gets the Serialization Information
        /// </summary>
        /// <param name="info">Serialization Information</param>
        /// <param name="context">Streaming Context</param>
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("graphType", this._g.GetType().AssemblyQualifiedName);
            info.AddValue("innerGraph", this._g, typeof(IGraph));
        }

        #endregion

        #region IXmlSerializable Members

        /// <summary>
        /// Gets the Schema for XML serialization
        /// </summary>
        /// <returns></returns>
        public virtual XmlSchema GetSchema()
        {
            return null;
        }

        /// <summary>
        /// Reads the data for XML deserialization
        /// </summary>
        /// <param name="reader">XML Reader</param>
        public virtual void ReadXml(XmlReader reader)
        {
            if (reader.MoveToAttribute("graphType"))
            {
                String graphType = reader.Value;
                Type t = Type.GetType(graphType);
                if (t == null) throw new RdfException("Invalid graphType attribute, the type '" + graphType + "' is not available in your environment");
                reader.MoveToElement();

                XmlSerializer graphDeserializer = new XmlSerializer(t);
                reader.Read();
                if (reader.Name.Equals("innerGraph"))
                {
                    reader.Read();
                    Object temp = graphDeserializer.Deserialize(reader);
                    this._g.Merge((IGraph)temp);
                    this.AttachEventHandlers();
                    reader.Read();
                }
                else
                {
                    throw new RdfException("Expected a <graph> element inside a <graph> element");
                }
            }
            else
            {
                throw new RdfException("Missing graphType attribute on the <graph> element");
            }
        }

        /// <summary>
        /// Writes the data for XML serialization
        /// </summary>
        /// <param name="writer">XML Writer</param>
        public virtual void WriteXml(XmlWriter writer)
        {
            XmlSerializer graphSerializer = new XmlSerializer(this._g.GetType());
            writer.WriteAttributeString("graphType", this._g.GetType().AssemblyQualifiedName);
            writer.WriteStartElement("innerGraph");
            graphSerializer.Serialize(writer, this._g);
            writer.WriteEndElement();
        }

        #endregion

#endif
    }
}

