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
#if !NO_DATA
using System.Data;
#endif
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using VDS.RDF.Parsing;
#if !SILVERLIGHT
using VDS.RDF.Writing.Serialization;
#endif

namespace VDS.RDF
{
    /// <summary>
    /// Abstract Base Implementation of the <see cref="IGraph">IGraph</see> interface
    /// </summary>
#if !SILVERLIGHT
    [Serializable,XmlRoot(ElementName="graph")]
#endif
    public abstract class BaseGraph 
        : IGraph
#if !SILVERLIGHT
        ,ISerializable
#endif
    {
        #region Variables

        /// <summary>
        /// Collection of Triples in the Graph
        /// </summary>
        protected BaseTripleCollection _triples;
        /// <summary>
        /// Collection of Subject &amp; Object Nodes in the Graph
        /// </summary>
        protected BaseNodeCollection _nodes;
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
#if !SILVERLIGHT
        private GraphDeserializationInfo _dsInfo;
#endif

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new Base Graph using the given Triple Collection
        /// </summary>
        /// <param name="tripleCollection">Triple Collection to use</param>
        /// <param name="nodeCollection">Node Collection to use</param>
        protected BaseGraph(BaseTripleCollection tripleCollection, BaseNodeCollection nodeCollection)
        {
            this._triples = tripleCollection;
            this._nodes = new NodeCollection();
            this._bnodemapper = new BlankNodeMapper();
            this._nsmapper = new NamespaceMapper();

            //Create Event Handlers and attach to the Triple Collection
            this.TripleAddedHandler = new TripleEventHandler(this.OnTripleAsserted);
            this.TripleRemovedHandler = new TripleEventHandler(this.OnTripleRetracted);
            this.AttachEventHandlers(this._triples);
        }

        /// <summary>
        /// Creates a new Base Graph which uses the given Triple Collection and the default <see cref="NodeCollection">NodeCollection</see> as the Node Collection
        /// </summary>
        /// <param name="tripleCollection">Triple Collection to use</param>
        protected BaseGraph(BaseTripleCollection tripleCollection)
            : this(tripleCollection, new NodeCollection()) { }

        /// <summary>
        /// Creates a new Base Graph which uses the default <see cref="IndexedTripleCollection">IndexedTripleCollection</see> as the Triple Collection and the default <see cref="NodeCollection">NodeCollection</see> as the Node Collection
        /// </summary>
        protected BaseGraph()
            : this(new IndexedTripleCollection()) { }

#if !SILVERLIGHT
        /// <summary>
        /// Creates a Graph from the given Serialization Information
        /// </summary>
        /// <param name="info">Serialization Information</param>
        /// <param name="context">Streaming Context</param>
        protected BaseGraph(SerializationInfo info, StreamingContext context)
            : this()
        {
            this._dsInfo = new GraphDeserializationInfo(info, context);   
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            if (this._dsInfo != null) this._dsInfo.Apply(this);
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
                return this._triples;
            }
        }

        /// <summary>
        /// Gets the set of Nodes which make up this Graph
        /// </summary>
        public virtual BaseNodeCollection Nodes
        {
            get
            {
                return this._nodes;
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
                return this._nsmapper;
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
                return this._baseuri;
            }
            set
            {
                this._baseuri = value;
            }
        }

        /// <summary>
        /// Gets whether a Graph is Empty ie. Contains No Triples or Nodes
        /// </summary>
        public virtual bool IsEmpty
        {
            get
            {
                return (this._triples.Count == 0);
            }
        }

        #endregion

        #region Triple Assertion & Retraction

        /// <summary>
        /// Asserts a List of Triples in the graph
        /// </summary>
        /// <param name="ts">List of Triples to add to the Graph</param>
        public abstract void Assert(List<Triple> ts);

        /// <summary>
        /// Asserts multiple Triples in the Graph
        /// </summary>
        /// <param name="ts">Array of Triples to add</param>
        public abstract void Assert(Triple[] ts);

        /// <summary>
        /// Asserts a Triple in the Graph
        /// </summary>
        /// <param name="t">The Triple to add to the Graph</param>
        public abstract void Assert(Triple t);

        /// <summary>
        /// Asserts a List of Triples in the graph
        /// </summary>
        /// <param name="ts">List of Triples in the form of an IEnumerable</param>
        public abstract void Assert(IEnumerable<Triple> ts);

        /// <summary>
        /// Retracts multiple Triples from the Graph
        /// </summary>
        /// <param name="ts">Array of Triples to retract</param>
        public abstract void Retract(Triple[] ts);

        /// <summary>
        /// Retracts a Triple from the Graph
        /// </summary>
        /// <param name="t">Triple to Retract</param>
        /// <remarks>Current implementation may have some defunct Nodes left in the Graph as only the Triple is retracted</remarks>
        public abstract void Retract(Triple t);

        /// <summary>
        /// Retracts a List of Triples from the graph
        /// </summary>
        /// <param name="ts">List of Triples to retract from the Graph</param>
        public abstract void Retract(List<Triple> ts);

        /// <summary>
        /// Retracts a enumeration of Triples from the graph
        /// </summary>
        /// <param name="ts">Enumeration of Triples to retract</param>
        public abstract void Retract(IEnumerable<Triple> ts);

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
            if (!this.RaiseClearRequested()) return;

            this.Retract(this.Triples);

            this.RaiseCleared();
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
            //try
            //{
            //    Monitor.Enter(this._bnodemapper);
                this._bnodemapper.CheckID(ref nodeId);
                return new BlankNode(this, nodeId);
            //}
            //finally
            //{
            //    Monitor.Exit(this._bnodemapper);
            //}
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
            return new UriNode(this, new Uri(Tools.ResolveUri(String.Empty, this._baseuri.ToSafeString())));
        }

        /// <summary>
        /// Creates a new URI Node with the given URI
        /// </summary>
        /// <param name="uri">URI for the Node</param>
        /// <returns></returns>
        public virtual IUriNode CreateUriNode(Uri uri)
        {
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
        /// Gets all the Nodes according to some arbitrary criteria as embodied in a Selector
        /// </summary>
        /// <param name="selector">Selector class which performs the Selection</param>
        /// <returns>Zero/More Nodes</returns>
        public abstract IEnumerable<INode> GetNodes(ISelector<INode> selector);

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
        /// Gets all Triples which are selected by the final Selector in the Chain (where the results of each Selector are used to initialise the next Selector in the chain and selection applied to the whole Graph each time)
        /// </summary>
        /// <param name="firstSelector">Selector Class which does the initial Selection</param>
        /// <param name="selectorChain">Chain of Dependent Selectors to perform the Selection</param>
        /// <returns>Zero/More Triples</returns>
        /// <remarks>This method is used to apply a series of Selectors where each filter is applied to the entire Graph but is initialised with the results of the previous Selector in the chain.  This means that something eliminated in a given step can potentially be selected by a later Selector in the Chain.</remarks>
        public abstract IEnumerable<Triple> GetTriples(ISelector<Triple> firstSelector, List<IDependentSelector<Triple>> selectorChain);

        /// <summary>
        /// Gets all Triples which are selected by all the Selectors in the Chain (with the Selectors applied in order to the result set of the previous Selector)
        /// </summary>
        /// <param name="selectorChain">Chain of Selector Classes to perform the Selection</param>
        /// <returns>Zero/More Triples</returns>
        /// <remarks>This method is used to apply a series of Selectors where each filters the results of the previous.  Each application of a Selector potentially reduces the results set, anything eliminated in a given step cannot possibly be selected by a later Selector in the Chain.</remarks>
        public abstract IEnumerable<Triple> GetTriples(List<ISelector<Triple>> selectorChain);

        /// <summary>
        /// Gets all the Triples involving the given Uri
        /// </summary>
        /// <param name="uri">The Uri to find Triples involving</param>
        /// <returns>Zero/More Triples</returns>
        public abstract IEnumerable<Triple> GetTriples(Uri uri);

        /// <summary>
        /// Gets all the Triples which meet some arbitrary criteria as embodied in a Selector
        /// </summary>
        /// <param name="selector">Selector class which performs the Selection</param>
        /// <returns>Zero/More Triple</returns>
        public abstract IEnumerable<Triple> GetTriples(ISelector<Triple> selector);

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
        /// Gets all the Triples with an Object matching some arbitrary criteria as embodied in a Selector
        /// </summary>
        /// <param name="selector">Selector class which performs the Selection</param>
        /// <returns>Zero/More Triples</returns>
        public abstract IEnumerable<Triple> GetTriplesWithObject(ISelector<INode> selector);

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
        /// Gets all the Triples with a Predicate matching some arbitrary criteria as embodied in a Selector
        /// </summary>
        /// <param name="selector">Selector class which performs the Selection</param>
        /// <returns>Zero/More Triples</returns>
        public abstract IEnumerable<Triple> GetTriplesWithPredicate(ISelector<INode> selector);

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
        /// Gets all the Triples with a Subject matching some arbitrary criteria as embodied in a Selector
        /// </summary>
        /// <param name="selector">Selector class which performs the Selection</param>
        /// <returns>Zero/More Triples</returns>
        public abstract IEnumerable<Triple> GetTriplesWithSubject(ISelector<INode> selector);

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
        /// Checks whether any Triples Exist which match a given Selector
        /// </summary>
        /// <param name="selector">Selector Class which performs the Selection</param>
        /// <returns></returns>
        public abstract bool TriplesExist(ISelector<Triple> selector);

        /// <summary>
        /// Gets whether a given Triple exists in this Graph
        /// </summary>
        /// <param name="t">Triple to test</param>
        /// <returns></returns>
        public virtual bool ContainsTriple(Triple t)
        {
            return this._triples.Contains(t);
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
            this.Merge(g, false);
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

            //Check that the merge can go ahead
            if (!this.RaiseMergeRequested()) return;

            //First copy and Prefixes across which aren't defined in this Graph
            this._nsmapper.Import(g.NamespaceMap);

            if (this.IsEmpty)
            {
                //Empty Graph so do a quick copy
                foreach (Triple t in g.Triples)
                {
                    this.Assert(new Triple(Tools.CopyNode(t.Subject, this, keepOriginalGraphUri), Tools.CopyNode(t.Predicate, this, keepOriginalGraphUri), Tools.CopyNode(t.Object, this, keepOriginalGraphUri), t.Context));
                }
            }
            else
            {   
                //Prepare a mapping of Blank Nodes to Blank Nodes
                Dictionary<INode, IBlankNode> mapping = new Dictionary<INode, IBlankNode>();

                foreach (Triple t in g.Triples)
                {
                    INode s, p, o;
                    if (t.Subject.NodeType == NodeType.Blank)
                    {
                        if (!mapping.ContainsKey(t.Subject))
                        {
                            IBlankNode temp = this.CreateBlankNode();
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
                            IBlankNode temp = this.CreateBlankNode();
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
                            IBlankNode temp = this.CreateBlankNode();
                            if (keepOriginalGraphUri) temp.GraphUri = t.Object.GraphUri;
                            mapping.Add(t.Object, temp);
                        }
                        o = mapping[t.Object];
                    }
                    else
                    {
                        o = Tools.CopyNode(t.Object, this, keepOriginalGraphUri);
                    }

                    this.Assert(new Triple(s, p, o, t.Context));
                }
            }

            this.RaiseMerged();
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
            //Graphs can't be equal to null
            if (obj == null) return false;

            if (obj is IGraph)
            {
                IGraph g = (IGraph)obj;

                Dictionary<INode, INode> temp;
                return this.Equals(g, out temp);
            }
            else
            {
                //Graphs can only be equal to other Graphs
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
            //Set the mapping to be null
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
            Dictionary<INode, INode> temp;
            return this.IsSubGraphOf(g, out temp);
        }

        /// <summary>
        /// Checks whether this Graph is a sub-graph of the given Graph
        /// </summary>
        /// <param name="g">Graph</param>
        /// <param name="mapping">Mapping of Blank Nodes</param>
        /// <returns></returns>
        public bool IsSubGraphOf(IGraph g, out Dictionary<INode, INode> mapping)
        {
            //Set the mapping to be null
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
            return new Uri(Tools.ResolveQName(qname, this._nsmapper, this._baseuri));
        }

        /// <summary>
        /// Creates a new unused Blank Node ID and returns it
        /// </summary>
        /// <returns></returns>
        public virtual String GetNextBlankNodeID()
        {
            //try
            //{
            //    Monitor.Enter(this._bnodemapper);
                return this._bnodemapper.GetNextID();
            //}
            //finally
            //{
            //    Monitor.Exit(this._bnodemapper);
            //}
        }

        #endregion

        #region Operators

#if !NO_DATA

        /// <summary>
        /// Converts a Graph into a DataTable using the explicit cast operator defined by this class
        /// </summary>
        /// <returns>
        /// A DataTable containing three Columns (Subject, Predicate and Object) all typed as <see cref="INode">INode</see> with a Row per Triple
        /// </returns>
        /// <remarks>
        /// <strong>Warning:</strong> Not available under builds which remove the Data Storage layer from dotNetRDF e.g. Silverlight
        /// </remarks>
        public virtual DataTable ToDataTable()
        {
            return (DataTable)this;
        }

        /// <summary>
        /// Casts a Graph to a DataTable with all Columns typed as <see cref="INode">INode</see> (Column Names are Subject, Predicate and Object
        /// </summary>
        /// <param name="g">Graph to convert</param>
        /// <returns>
        /// A DataTable containing three Columns (Subject, Predicate and Object) all typed as <see cref="INode">INode</see> with a Row per Triple
        /// </returns>
        /// <remarks>
        /// <strong>Warning:</strong> Not available under builds which remove the Data Storage layer from dotNetRDF e.g. Silverlight
        /// </remarks>
        public static explicit operator DataTable(BaseGraph g)
        {
            DataTable table = new DataTable();
            table.Columns.Add(new DataColumn("Subject", typeof(INode)));
            table.Columns.Add(new DataColumn("Predicate", typeof(INode)));
            table.Columns.Add(new DataColumn("Object", typeof(INode)));

            foreach (Triple t in g.Triples)
            {
                DataRow row = table.NewRow();
                row["Subject"] = t.Subject;
                row["Predicate"] = t.Predicate;
                row["Object"] = t.Object;
                table.Rows.Add(row);
            }

            return table;
        }

#endif

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
        /// Helper method for raising the <see cref="ClearRequested">Clear Requested</see> event and returning whether any of the Event Handlers cancelled the operation
        /// </summary>
        /// <returns>True if the operation can continue, false if it should be aborted</returns>
        protected bool RaiseClearRequested()
        {
            CancellableGraphEventHandler d = this.ClearRequested;
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
            GraphEventHandler d = this.Cleared;
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
            CancellableGraphEventHandler d = this.MergeRequested;
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
            GraphEventHandler d = this.Merged;
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
            tripleCollection.TripleAdded += this.TripleAddedHandler;
            tripleCollection.TripleRemoved += this.TripleRemovedHandler;
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
            tripleCollection.TripleAdded -= this.TripleAddedHandler;
            tripleCollection.TripleRemoved -= this.TripleRemovedHandler;
        }

        #endregion

        /// <summary>
        /// Disposes of a Graph
        /// </summary>
        public virtual void Dispose()
        {
            this.DetachEventHandlers(this._triples);
        }

#if !SILVERLIGHT

        #region ISerializable Members

        /// <summary>
        /// Gets the Serialization Information for serializing a Graph
        /// </summary>
        /// <param name="info">Serialization Information</param>
        /// <param name="context">Streaming Context</param>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("base", this.BaseUri.ToSafeString());
            info.AddValue("triples", this.Triples.ToList(), typeof(List<Triple>));
            IEnumerable<KeyValuePair<String,String>> ns = from p in this.NamespaceMap.Prefixes
                                                          select new KeyValuePair<String,String>(p, this.NamespaceMap.GetNamespaceUri(p).ToString());
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
                                Uri u = new Uri(reader.Value);
                                this.NamespaceMap.AddNamespace(prefix, u);
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
                            this.Assert((Triple)temp);
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

            //Serialize Base Uri
            if (this.BaseUri != null)
            {
                writer.WriteAttributeString("base", this.BaseUri.ToString());
            }

            //Serialize Namespace Map
            writer.WriteStartElement("namespaces");
            foreach (String prefix in this.NamespaceMap.Prefixes)
            {
                writer.WriteStartElement("namespace");
                writer.WriteAttributeString("prefix", prefix);
                writer.WriteAttributeString("uri", this.NamespaceMap.GetNamespaceUri(prefix).ToString());
                writer.WriteEndElement();
            }
            writer.WriteEndElement();

            //Serialize Triples
            writer.WriteStartElement("triples");
            foreach (Triple t in this.Triples)
            {
                tripleSerializer.Serialize(writer, t);
            }
            writer.WriteEndElement();
        }

        #endregion

#endif
    }
}
