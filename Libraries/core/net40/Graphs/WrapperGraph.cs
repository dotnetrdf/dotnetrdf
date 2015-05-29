/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2015 dotNetRDF Project (dotnetrdf-develop@lists.sf.net)

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
#if !NO_DATA
using System.Data;
#endif
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using VDS.RDF.Namespaces;
using VDS.RDF.Nodes;

namespace VDS.RDF.Graphs
{
    /// <summary>
    /// Abstract decorator for Graphs to make it easier to layer functionality on top of existing implementations
    /// </summary>
#if !SILVERLIGHT
    [Serializable, XmlRoot(ElementName="graph")]
#endif
    public abstract class WrapperGraph 
        : IEventedGraph
#if !SILVERLIGHT
        , ISerializable
#endif
    {
        /// <summary>
        /// Underlying Graph this is a wrapper around
        /// </summary>
        protected readonly IGraph _g;
        private readonly NotifyCollectionChangedEventHandler _changedHandler;

        /// <summary>
        /// Creates a wrapper around the default Graph implementation, primarily required only for deserialization and requires that the caller call <see cref="WrapperGraph.AttachEventHandlers"/> to properly wire up event handling
        /// </summary>
        protected WrapperGraph()
            : this(new Graph()) { }

        /// <summary>
        /// Creates a new wrapper around the given Graph
        /// </summary>
        /// <param name="g">Graph</param>
        protected WrapperGraph(IGraph g)
        {
            if (g == null) throw new ArgumentNullException("g", "Wrapped Graph cannot be null");
            this._g = g;

            this._changedHandler = this.HandleCollectionChanged;
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

        public virtual IGraphCapabilities Capabilities
        {
            get { return this._g.Capabilities; }
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
        public virtual void Assert(Triple t)
        {
            this._g.Assert(t);
        }

        /// <summary>
        /// Asserts Triples in the Graph
        /// </summary>
        /// <param name="ts">Triples</param>
        public virtual void Assert(IEnumerable<Triple> ts)
        {
            this._g.Assert(ts);
        }

        /// <summary>
        /// Retracts a Triple from the Graph
        /// </summary>
        /// <param name="t">Triple</param>
        public virtual void Retract(Triple t)
        {
            this._g.Retract(t);
        }

        /// <summary>
        /// Retracts Triples from the Graph
        /// </summary>
        /// <param name="ts">Triples</param>
        public virtual void Retract(IEnumerable<Triple> ts)
        {
            this._g.Retract(ts);
        }

        /// <summary>
        /// Clears the Graph
        /// </summary>
        public virtual void Clear()
        {
            this._g.Clear();
        }

        /// <summary>
        /// Indicates whether this factory produces RDF 1.1 literals
        /// </summary>
        /// <remarks>
        /// If true then calling <see cref="CreateLiteralNode(string)"/> will produce a literal typed as xsd:string and calling <see cref="CreateLiteralNode(string,string)"/> will produce a literal typed as rdf:langString.  If false then literals are created only with the fields provided.
        /// </remarks>
        public virtual bool CreatesImplicitlyTypedLiterals
        {
            get { return this._g.CreatesImplicitlyTypedLiterals; }
        }

        /// <summary>
        /// Creates a new Blank Node
        /// </summary>
        /// <returns></returns>
        public virtual INode CreateBlankNode()
        {
            return this._g.CreateBlankNode();
        }

        /// <summary>
        /// Creates a new blank node with the given ID
        /// </summary>
        /// <param name="id">ID</param>
        /// <returns>Blank node</returns>
        public virtual INode CreateBlankNode(Guid id)
        {
            return this._g.CreateBlankNode(id);
        }

        /// <summary>
        /// Creates a new Graph Literal Node with the given sub-graph
        /// </summary>
        /// <param name="subgraph">Sub-graph</param>
        /// <returns></returns>
        public virtual INode CreateGraphLiteralNode(IGraph subgraph)
        {
            return this._g.CreateGraphLiteralNode(subgraph);
        }

        /// <summary>
        /// Creates a new Graph Literal Node
        /// </summary>
        /// <returns></returns>
        public virtual INode CreateGraphLiteralNode()
        {
            return this._g.CreateGraphLiteralNode();
        }

        /// <summary>
        /// Creates a new Literal Node
        /// </summary>
        /// <param name="literal">Value</param>
        /// <returns></returns>
        public virtual INode CreateLiteralNode(string literal)
        {
            return this._g.CreateLiteralNode(literal);
        }

        /// <summary>
        /// Creates a new Literal Node with the given Datatype
        /// </summary>
        /// <param name="literal">Value</param>
        /// <param name="datatype">Datatype URI</param>
        /// <returns></returns>
        public virtual INode CreateLiteralNode(string literal, Uri datatype)
        {
            return this._g.CreateLiteralNode(literal, datatype);
        }

        /// <summary>
        /// Creates a new Literal Node with the given Language
        /// </summary>
        /// <param name="literal">Value</param>
        /// <param name="langspec">Language</param>
        /// <returns></returns>
        public virtual INode CreateLiteralNode(string literal, string langspec)
        {
            return this._g.CreateLiteralNode(literal, langspec);
        }

        /// <summary>
        /// Creates a new URI Node from a QName
        /// </summary>
        /// <param name="prefixedName">QName</param>
        /// <returns></returns>
        public virtual INode CreateUriNode(string prefixedName)
        {
            return this._g.CreateUriNode(prefixedName);
        }

        /// <summary>
        /// Creates a new URI Node
        /// </summary>
        /// <param name="uri">URI</param>
        /// <returns></returns>
        public virtual INode CreateUriNode(Uri uri)
        {
            return this._g.CreateUriNode(uri);
        }

        /// <summary>
        /// Creates a new Variable Node
        /// </summary>
        /// <param name="varname">Variable Name</param>
        /// <returns></returns>
        public virtual INode CreateVariableNode(String varname)
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
            return ReferenceEquals(this, obj);
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

        /// <summary>
        /// Event which is raised when the graph changes
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>
        /// Gets whether the graph has events
        /// </summary>
        public virtual bool HasEvents
        {
            get
            {
                return this._g is IEventedGraph && ((IEventedGraph)this._g).HasEvents;
            }
        }

        /// <summary>
        /// Helper method uses to handle the <see cref="INotifyCollectionChanged.CollectionChanged"/> from the wrapped graph and propagate it to this graphs <see cref="CollectionChanged"/> event
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="args">Arguments</param>
        private void HandleCollectionChanged(Object sender, NotifyCollectionChangedEventArgs args)
        {
            NotifyCollectionChangedEventHandler d = this.CollectionChanged;
            if (d != null)
            {
                d(this, args);
            }
        }

        /// <summary>
        /// Attaches event handles to the underlying graph
        /// </summary>
        protected void AttachEventHandlers()
        {
            IEventedGraph e = this._g as IEventedGraph;
            if (e == null) return;
            if (e.HasEvents)
            {
                e.CollectionChanged += this._changedHandler;
            }
        }

        /// <summary>
        /// Disposes of the wrapper and in doing so disposes of the underlying graph
        /// </summary>
        public virtual void Dispose()
        {
            this._g.Dispose();
        }

#if !SILVERLIGHT

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
                    this._g.Assert(((IGraph)temp).Triples);
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

#endif
    }
}

