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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace VDS.RDF.Writing
{
    /// <summary>
    /// Class for serializing a <see cref="IGraph">graph</see> in GraphML format
    /// </summary>
    public class GraphMLWriter : IStoreWriter, ICollapseLiteralsWriter
    {
        /// <summary>
        /// Controls whether to collapse distinct literals
        /// </summary>
        public bool CollapseLiterals { get; set; } = true;

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
            GraphMLWriter.WriteGraphML(output, store, this.CollapseLiterals);
        }

        private static void WriteGraphML(XmlWriter writer, ITripleStore store, bool collapseLiterals)
        {
            writer.WriteStartElement(GraphMLSpecsHelper.GraphML, GraphMLSpecsHelper.NS);

            GraphMLWriter.WriteKey(writer, GraphMLSpecsHelper.EdgeLabel, GraphMLSpecsHelper.Edge);
            GraphMLWriter.WriteKey(writer, GraphMLSpecsHelper.NodeLabel, GraphMLSpecsHelper.Node);

            foreach (var graph in store.Graphs)
            {
                GraphMLWriter.WriteGraph(writer, graph, collapseLiterals);
            }

            writer.WriteEndElement();
        }

        private static void WriteGraph(XmlWriter writer, IGraph graph, bool collapseLiterals)
        {
            writer.WriteStartElement(GraphMLSpecsHelper.Graph);

            // Named graphs
            if (graph.BaseUri != null)
            {
                writer.WriteAttributeString(GraphMLSpecsHelper.Id, graph.BaseUri.AbsoluteUri);
            }

            // RDF is always a directed graph
            writer.WriteAttributeString(GraphMLSpecsHelper.Edgedefault, GraphMLSpecsHelper.Directed);

            GraphMLWriter.WriteTriples(writer, graph, collapseLiterals);

            writer.WriteEndElement();
        }

        private static void WriteTriples(XmlWriter writer, IGraph graph, bool collapseLiterals)
        {
            var nodesAlreadyWritten = new HashSet<object>();

            foreach (var triple in graph.Triples)
            {
                foreach (var node in new[] { triple.Subject, triple.Object })
                {
                    var id = GraphMLWriter.CalculateNodeId(node, triple, collapseLiterals);

                    // Skip if already written
                    if (nodesAlreadyWritten.Add(id))
                    {
                        GraphMLWriter.WriteNode(writer, id.GetHashCode().ToString(), node.ToString());
                    }
                }

                GraphMLWriter.WriteEdge(writer, triple, collapseLiterals);
            }
        }

        private static void WriteEdge(XmlWriter writer, Triple triple, bool collapseLiterals)
        {
            writer.WriteStartElement(GraphMLSpecsHelper.Edge);
            writer.WriteAttributeString(GraphMLSpecsHelper.Source, triple.Subject.GetHashCode().ToString());

            writer.WriteStartAttribute(GraphMLSpecsHelper.Target);

            var id = GraphMLWriter.CalculateNodeId(triple.Object, triple, collapseLiterals);
            writer.WriteString(id.GetHashCode().ToString());

            GraphMLWriter.WriteData(writer, GraphMLSpecsHelper.EdgeLabel, triple.Predicate.ToString());

            writer.WriteEndElement();
        }

        private static void WriteKey(XmlWriter writer, string id, string domain)
        {
            writer.WriteStartElement(GraphMLSpecsHelper.Key);
            writer.WriteAttributeString(GraphMLSpecsHelper.Id, id);
            writer.WriteAttributeString(GraphMLSpecsHelper.Domain, domain);
            writer.WriteAttributeString(GraphMLSpecsHelper.AttributeTyte, GraphMLSpecsHelper.String);
            writer.WriteEndElement();
        }

        private static void WriteNode(XmlWriter writer, string id, string label)
        {
            writer.WriteStartElement(GraphMLSpecsHelper.Node);
            writer.WriteAttributeString(GraphMLSpecsHelper.Id, id);

            GraphMLWriter.WriteData(writer, GraphMLSpecsHelper.NodeLabel, label);

            writer.WriteEndElement();
        }

        private static void WriteData(XmlWriter writer, string key, string value)
        {
            writer.WriteStartElement(GraphMLSpecsHelper.Data);
            writer.WriteAttributeString(GraphMLSpecsHelper.Key, key);
            writer.WriteString(value);
            writer.WriteEndElement();
        }

        private static object CalculateNodeId(INode node, Triple triple, bool collapseLiterals)
        {
            // Literal nodes are identified either by their value or by their containing triple.
            // When identified by value, there will be a single node representing all literals with the same value.
            // When identified by triple, there will be a separate node representing each triple that has an object with that value.
            var idObject = node as object;

            if (!collapseLiterals && node is ILiteralNode)
            {
                idObject = triple;
            }

            return idObject;
        }
    }
}
