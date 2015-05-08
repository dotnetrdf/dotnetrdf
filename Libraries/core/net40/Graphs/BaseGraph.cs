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
using System.Collections.Specialized;
using VDS.RDF.Graphs.Utilities;
#if !NO_DATA
using System.Data;
#endif
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using VDS.Common.Collections;
using VDS.RDF.Collections;
using VDS.RDF.Namespaces;
using VDS.RDF.Nodes;
#if !SILVERLIGHT
using VDS.RDF.Writing.Serialization;
#endif

namespace VDS.RDF.Graphs
{
    /// <summary>
    /// Abstract Base Implementation of the <see cref="IGraph">IGraph</see> interface
    /// </summary>
#if !SILVERLIGHT
    [Serializable,XmlRoot(ElementName="graph")]
#endif
    public abstract class BaseGraph 
        : NodeFactory, IEventedGraph
#if !SILVERLIGHT
        ,ISerializable
#endif
    {
        #region Member Variables

        /// <summary>
        /// Collection of Triples in the Graph
        /// </summary>
        protected ITripleCollection _triples;
        /// <summary>
        /// Namespace Mapper
        /// </summary>
        protected readonly NamespaceMapper _nsmapper;
        private readonly NotifyCollectionChangedEventHandler _changedHandler;

#if !SILVERLIGHT
        private GraphDeserializationInfo _dsInfo;
#endif

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new Base Graph using the given Triple Collection
        /// </summary>
        /// <param name="tripleCollection">Triple Collection to use</param>
        protected BaseGraph(ITripleCollection tripleCollection)
        {
            this._triples = tripleCollection;
            this._nsmapper = new NamespaceMapper();
            this._changedHandler = this.HandleTripleCollectionChanged;

            // Attach event handlers
            this.AttachEventHandlers();
        }

        /// <summary>
        /// Creates a new Base Graph which uses the default <see cref="TreeIndexedTripleCollection" /> as the Triple Collection
        /// </summary>
        protected BaseGraph()
            : this(new TreeIndexedTripleCollection()) { }

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
        /// Gets the set of Triples in this Graph
        /// </summary>
        public virtual IEnumerable<Triple> Triples
        {
            get
            {
                return this._triples;
            }
        }

        /// <summary>
        /// Gets the set of Quads in the graph
        /// </summary>
        /// <remarks>
        /// Since a graph has no name directly associated with it the resulting quads will have the null name assigned to them and so will appears as if in the default unnamed graph
        /// </remarks>
        public virtual IEnumerable<Quad> Quads
        {
            get
            {
                return this._triples.Select(t => t.AsQuad(null));
            }
        }

        /// <summary>
        /// Gets the nodes that are used as vertices in the graph i.e. those which occur in the subject or object position of a triple
        /// </summary>
        public virtual IEnumerable<INode> Vertices
        {
            get
            {
                return (from t in this._triples
                        select t.Subject).Concat(from t in this._triples
                                                 select t.Object).Distinct();
            }
        }

        /// <summary>
        /// Gets the nodes that are used as edges in the graph i.e. those which occur in the predicate position of a triple
        /// </summary>
        public virtual IEnumerable<INode> Edges
        {
            get
            {
                return (from t in this._triples
                        select t.Predicate).Distinct();
            }
        }

        /// <summary>
        /// Gets the Namespace Mapper for this Graph which contains all in use Namespace Prefixes and their URIs
        /// </summary>
        /// <returns></returns>
        public virtual INamespaceMapper Namespaces
        {
            get
            {
                return this._nsmapper;
            }
        }

        /// <summary>
        /// Gets the number of triples in the graph
        /// </summary>
        public virtual long Count
        {
            get
            {
                return this._triples.Count;
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

        public virtual IGraphCapabilities Capabilities
        {
            get
            {
                return new TripleCollectionCapabilities(this._triples, this._triples.IsReadOnly ? GraphAccessMode.Read : GraphAccessMode.ReadWrite);
            }
        }

        #endregion

        #region Triple Assertion & Retraction

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
        /// Retracts a Triple from the Graph
        /// </summary>
        /// <param name="t">Triple to Retract</param>
        /// <remarks>Current implementation may have some defunct Nodes left in the Graph as only the Triple is retracted</remarks>
        public abstract void Retract(Triple t);

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
            this._triples.Clear();
        }

        #endregion

        #region Node Creation

        /// <summary>
        /// Creates a new URI Node with the given prefixed name
        /// </summary>
        /// <param name="prefixedName">Prefixed name for the Node</param>
        /// <returns>URI Node</returns>
        /// <remarks>Internally the Graph will resolve the prefixed name to a full URI, this throws an exception when this is not possible</remarks>
        public virtual INode CreateUriNode(String prefixedName)
        {
            return new UriNode(UriFactory.ResolvePrefixedName(prefixedName, this._nsmapper, null));
        }

        #endregion

        #region Triple Selection

        /// <summary>
        /// Finds triples matching the given search criteria i.e. those where the given nodes occur in the appropriate position(s).  Null values are considered wildcards for a position.
        /// </summary>
        /// <param name="s">Subject</param>
        /// <param name="p">Predicate</param>
        /// <param name="o">Object</param>
        /// <returns>Triples</returns>
        public virtual IEnumerable<Triple> Find(INode s, INode p, INode o)
        {
            if (ReferenceEquals(s, null))
            {
                // Wildcard Subject
                if (ReferenceEquals(p, null))
                {
                    // Wildcard Subject and Predicate
                    if (ReferenceEquals(o, null))
                    {
                        // Wildcard Subject, Predicate and Object
                        return this.Triples;
                    }
                    // Wildcard Subject and Predicate with Fixed Object
                    return this._triples.WithObject(o);
                }
                // Fixed Predicate with Wildcard Subject
                if (ReferenceEquals(o, null))
                {
                    // Fixed Predicate with Wildcard Subject and Object
                    return this._triples.WithPredicate(p);
                }
                // Fixed Predicate and Object with Wildcard Subject
                return this._triples.WithPredicateObject(p, o);
            }

            // Fixed Subject
            if (ReferenceEquals(p, null))
            {
                // Wildcard Predicate with Fixed Subject
                if (ReferenceEquals(o, null))
                {
                    // Wildcard Predicate and Object with Fixed Subject
                    return this._triples.WithSubject(s);
                }
                // Wildcard Predicate with Fixed Subject and Object
                return this._triples.WithSubjectObject(s, o);
            }

            // Fixed Subject and Predicate
            if (ReferenceEquals(o, null))
            {
                // Fixed Subject and Predicate with Wildcard Object
                return this._triples.WithSubjectPredicate(s, p);
            }

            // Fixed Subject, Predicate and Object
            Triple t = new Triple(s, p, o);
            return this._triples.Contains(t) ? t.AsEnumerable() : Enumerable.Empty<Triple>();
        }

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

            // Graphs can only be equal to other Graphs
            if (!(obj is IGraph)) return false;

            IGraph g = (IGraph)obj;

            Dictionary<INode, INode> temp;
            return this.Equals(g, out temp);
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
            //Set the mapping to be null
            mapping = null;

            GraphMatcher matcher = new GraphMatcher();
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
            return false;
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
        /// Creates a new unused Blank Node ID and returns it
        /// </summary>
        /// <returns></returns>
        [Obsolete("Obsolete, no longer used", true)]
        public virtual String GetNextBlankNodeID()
        {
            throw new NotSupportedException();
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

        public virtual bool HasEvents { get { return true; } }

        protected void AttachEventHandlers()
        {
            this._triples.CollectionChanged += this._changedHandler;
        }

        protected void HandleTripleCollectionChanged(Object sender, NotifyCollectionChangedEventArgs args)
        {
            this.RaiseCollectionChanged(args);
        }

        /// <summary>
        /// Raises the collection changed event
        /// </summary>
        /// <param name="args">Collection changed arguments</param>
        protected void RaiseCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            NotifyCollectionChangedEventHandler d = this.CollectionChanged;
            if (d != null)
            {
                d(this, args);
            }
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        #endregion

        /// <summary>
        /// Disposes of a Graph
        /// </summary>
        public virtual void Dispose()
        {
            // Nothing to do by default
            // We don't do anything to the triple collection because it could be bound to multiple graphs
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
            info.AddValue("triples", this.Triples.ToList(), typeof(List<Triple>));
            IEnumerable<KeyValuePair<String,String>> ns = from p in this.Namespaces.Prefixes
                                                          select new KeyValuePair<String,String>(p, this.Namespaces.GetNamespaceUri(p).AbsoluteUri);
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
                                this.Namespaces.AddNamespace(prefix, u);
                                reader.Read();
                            }
                            else
                            {
                                throw new RdfException("Expected a uri attribute on a <namespace> element");
                            }
                        }
                        else
                        {
                            throw new RdfException("Expected a prefix attribute on a <namespace> element");
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
                        Object temp = tripleDeserializer.Deserialize(reader);
                        this.Assert((Triple)temp);
                        reader.Read();
                    }
                }
                // Attach events after deserialization completes
                this.AttachEventHandlers();
            }
            else
            {
                throw new RdfException("Expected a <triples> element inside a <graph> element but got a <" + reader.Name + "> element instead");
            }
        }

        /// <summary>
        /// Writes the data for XML serialization
        /// </summary>
        /// <param name="writer">XML Writer</param>
        public void WriteXml(XmlWriter writer)
        {
            XmlSerializer tripleSerializer = new XmlSerializer(typeof(Triple));

            //Serialize Namespace Map
            writer.WriteStartElement("namespaces");
            foreach (String prefix in this.Namespaces.Prefixes)
            {
                writer.WriteStartElement("namespace");
                writer.WriteAttributeString("prefix", prefix);
                writer.WriteAttributeString("uri", this.Namespaces.GetNamespaceUri(prefix).AbsoluteUri);
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
