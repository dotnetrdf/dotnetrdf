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

using System.IO;
using System.Linq;
using System.Xml;

namespace VDS.RDF.Writing
{
    /// <summary>
    /// Class for serializing a <see cref="IGraph">graph</see> in GraphML format
    /// </summary>
    public class GraphMLWriter : IStoreWriter
    {
        /// <summary>
        /// Event raised when there is ambiguity in the syntax being producing
        /// </summary>
        /// <remarks>This class doesn't raise this event</remarks>
        public event StoreWriterWarning Warning;

        /// <summary>
        /// Saves a triple store to a file in GraphML format
        /// </summary>
        /// <param name="store">The source triple store</param>
        /// <param name="filename">The name of the target file</param>
        public void Save(ITripleStore store, string filename)
        {
            using (var writer = new StreamWriter(File.OpenWrite(filename)))
            {
                this.Save(store, writer);
            }
        }

        /// <summary>
        /// Saves a triple store to a text writer in GraphML format
        /// </summary>
        /// <param name="store">The source triple store</param>
        /// <param name="output">The target text writer</param>
        public void Save(ITripleStore store, TextWriter output)
        {
            this.Save(store, output, false);
        }

        /// <summary>
        /// Saves a triple store to a text writer in GraphML format
        /// </summary>
        /// <param name="store">The source triple store</param>
        /// <param name="output">The target text writer</param>
        /// <param name="leaveOpen">Boolean flag indicating if the output writer should be left open by the writer when it completes</param>
        public void Save(ITripleStore store, TextWriter output, bool leaveOpen)
        {
            using (var writer = XmlWriter.Create(output, new XmlWriterSettings { CloseOutput = !leaveOpen, OmitXmlDeclaration = true }))
            {
                this.Save(store, writer);
            }
        }

        /// <summary>
        /// Saves a triple store to an XML writer in GraphML format
        /// </summary>
        /// <param name="store">The source triple store</param>
        /// <param name="output">The target XML writer</param>
        public void Save(ITripleStore store, XmlWriter output)
        {
            GraphMLWriter.WriteGraphML(output, store);
        }

        private static void WriteGraphML(XmlWriter writer, ITripleStore store)
        {
            writer.WriteStartElement(GraphMLHelper.GraphML, GraphMLHelper.NS);

            GraphMLWriter.WriteKey(writer, GraphMLHelper.EdgeLabel, GraphMLHelper.Edge);
            GraphMLWriter.WriteKey(writer, GraphMLHelper.NodeLabel, GraphMLHelper.Node);

            foreach (var graph in store.Graphs)
            {
                GraphMLWriter.WriteGraph(writer, graph);
            }

            writer.WriteEndElement();
        }

        private static void WriteGraph(XmlWriter writer, IGraph graph)
        {
            writer.WriteStartElement(GraphMLHelper.Graph);

            // Named graphs
            if (graph.BaseUri != null)
            {
                writer.WriteAttributeString(GraphMLHelper.Id, graph.BaseUri.AbsoluteUri);
            }

            // RDF is always a directed graph
            writer.WriteAttributeString(GraphMLHelper.Edgedefault, GraphMLHelper.Directed);

            // The next two iterations are disjoint
            foreach (var literalNode in graph.Nodes.Where(n => n.NodeType != NodeType.Literal))
            {
                GraphMLWriter.WriteNode(writer, literalNode.GetHashCode().ToString(), literalNode.ToString());
            }

            // Literal node identifiers are the whole statement so they become distinct
            foreach (var triple in graph.Triples.Where(t => t.Object.NodeType == NodeType.Literal))
            {
                GraphMLWriter.WriteNode(writer, triple.GetHashCode().ToString(), triple.Object.ToString());
            }

            foreach (var triple in graph.Triples)
            {
                GraphMLWriter.WriteEdge(writer, triple);
            }

            writer.WriteEndElement();
        }

        private static void WriteEdge(XmlWriter writer, Triple triple)
        {
            writer.WriteStartElement(GraphMLHelper.Edge);
            writer.WriteAttributeString(GraphMLHelper.Source, triple.Subject.GetHashCode().ToString());

            writer.WriteStartAttribute(GraphMLHelper.Target);
            if (triple.Object.NodeType == NodeType.Literal)
            {
                writer.WriteString(triple.GetHashCode().ToString());
            }
            else
            {
                writer.WriteString(triple.Object.GetHashCode().ToString());
            }

            GraphMLWriter.WriteData(writer, GraphMLHelper.EdgeLabel, triple.Predicate.ToString());

            writer.WriteEndElement();
        }

        private static void WriteKey(XmlWriter writer, string id, string domain)
        {
            writer.WriteStartElement(GraphMLHelper.Key);
            writer.WriteAttributeString(GraphMLHelper.Id, id);
            writer.WriteAttributeString(GraphMLHelper.Domain, domain);
            writer.WriteAttributeString(GraphMLHelper.AttributeTyte, GraphMLHelper.String);
            writer.WriteEndElement();
        }

        private static void WriteNode(XmlWriter writer, string id, string label)
        {
            writer.WriteStartElement(GraphMLHelper.Node);
            writer.WriteAttributeString(GraphMLHelper.Id, id);

            GraphMLWriter.WriteData(writer, GraphMLHelper.NodeLabel, label);

            writer.WriteEndElement();
        }

        private static void WriteData(XmlWriter writer, string key, string value)
        {
            writer.WriteStartElement(GraphMLHelper.Data);
            writer.WriteAttributeString(GraphMLHelper.Key, key);
            writer.WriteString(value);
            writer.WriteEndElement();
        }
    }

    /// <summary>
    /// A helper class containing GraphML name and URI constants
    /// </summary>
    public static class GraphMLHelper
    {
        /// <summary>
        /// The namespace URI for GraphML XML elements
        /// </summary>
        public const string NS = "http://graphml.graphdrawing.org/xmlns";

        /// <summary>
        /// The URL of the GraphML XML schema
        /// </summary>
        public const string XsdUri = "http://graphml.graphdrawing.org/xmlns/1.1/graphml.xsd";

        /// <summary>
        /// The name of the GraphML XML root element
        /// </summary>
        public const string GraphML = "graphml";

        /// <summary>
        /// The name of the GraphML XML element representing a graph
        /// </summary>
        public const string Graph = "graph";

        /// <summary>
        /// The name of the GraphML XML attribute representing the default directedness of and edge
        /// </summary>
        public const string Edgedefault = "edgedefault";

        /// <summary>
        /// The value representing a directed edge
        /// </summary>
        public const string Directed = "directed";

        /// <summary>
        /// The name of the GraphML XML element representing an edge
        /// </summary>
        public const string Edge = "edge";

        /// <summary>
        /// The name of the GraphML attribute representing the source of an edge
        /// </summary>
        public const string Source = "source";

        /// <summary>
        /// The name of the GraphML attribute representing the target of an edge
        /// </summary>
        public const string Target = "target";

        /// <summary>
        /// The name of the GraphML element representing the source of a node
        /// </summary>
        public const string Node = "node";

        /// <summary>
        /// The name of the GraphML element representing custom attributes for nodes and edges
        /// </summary>
        public const string Data = "data";

        /// <summary>
        /// The name of the GraphML attribute representing the domain of a key
        /// </summary>
        public const string Domain = "for";

        /// <summary>
        /// The name of the GraphML attribute representing the type of an attribute
        /// </summary>
        public const string AttributeTyte = "attr.type";

        /// <summary>
        /// The value representing the string type
        /// </summary>
        public const string String = "string";

        /// <summary>
        /// The value representing a node label attribute id
        /// </summary>
        public const string NodeLabel = "label";

        /// <summary>
        /// The value representing an edge label attribute id
        /// </summary>
        public const string EdgeLabel = "edgelabel";

        /// <summary>
        /// The name of the GraphML attribute representing the id of a node or edge
        /// </summary>
        public const string Id = "id";

        /// <summary>
        /// The name of the GraphML element representing a key
        /// </summary>
        public const string Key = "key";
    }
}
