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

using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace VDS.RDF.Writing;

/// <summary>
/// Class for serializing a <see cref="IGraph">graph</see> in GraphML format.
/// </summary>
public class GraphMLWriter : BaseStoreWriter, ICollapseLiteralsWriter
{
    /// <summary>
    /// Controls whether to collapse distinct literals.
    /// </summary>
    public bool CollapseLiterals { get; set; } = true;

    /// <summary>
    /// Event raised when there is ambiguity in the syntax being producing
    /// </summary>
    /// <remarks>This class doesn't raise this event</remarks>
#pragma warning disable CS0067
    public override event StoreWriterWarning Warning;
#pragma warning restore CS0067

    /// <summary>
    /// Saves a triple store to a text writer in GraphML format.
    /// </summary>
    /// <param name="store">The source triple store.</param>
    /// <param name="output">The target text writer.</param>
    /// <param name="leaveOpen">Boolean flag indicating if the output writer should be left open by the writer when it completes.</param>
    public override void Save(ITripleStore store, TextWriter output, bool leaveOpen)
    {
        using var writer = XmlWriter.Create(output, new XmlWriterSettings { CloseOutput = !leaveOpen, OmitXmlDeclaration = true });
        Save(store, writer);
    }

    /// <summary>
    /// Saves a triple store to an XML writer in GraphML format.
    /// </summary>
    /// <param name="store">The source triple store.</param>
    /// <param name="output">The target XML writer.</param>
    public void Save(ITripleStore store, XmlWriter output)
    {
        WriteGraphML(output, store, CollapseLiterals);
    }

    private static void WriteGraphML(XmlWriter writer, ITripleStore store, bool collapseLiterals)
    {
        writer.WriteStartElement(GraphMLSpecsHelper.GraphML, GraphMLSpecsHelper.NS);

        WriteKey(writer, GraphMLSpecsHelper.EdgeLabel, GraphMLSpecsHelper.Edge);
        WriteKey(writer, GraphMLSpecsHelper.NodeLabel, GraphMLSpecsHelper.Node);
        WriteKey(writer, GraphMLSpecsHelper.GraphLabel, GraphMLSpecsHelper.Graph);

        foreach (IGraph graph in store.Graphs)
        {
            WriteGraph(writer, graph, collapseLiterals);
        }

        writer.WriteEndElement();
    }

    private static void WriteGraph(XmlWriter writer, IGraph graph, bool collapseLiterals)
    {
        writer.WriteStartElement(GraphMLSpecsHelper.Graph);


        // RDF is always a directed graph
        writer.WriteAttributeString(GraphMLSpecsHelper.Edgedefault, GraphMLSpecsHelper.Directed);

        // Named graphs
        if (graph.Name != null)
        {
            //writer.WriteAttributeString(GraphMLSpecsHelper.Id, graph.BaseUri.AbsoluteUri);
            WriteData(writer, GraphMLSpecsHelper.GraphLabel, graph.Name.ToSafeString());
        }

        WriteTriples(writer, graph, collapseLiterals);

        writer.WriteEndElement();
    }

    private static void WriteTriples(XmlWriter writer, IGraph graph, bool collapseLiterals)
    {
        var nodesAlreadyWritten = new HashSet<object>();

        foreach (Triple triple in graph.Triples)
        {
            foreach (INode node in new[] { triple.Subject, triple.Object })
            {
                object id = CalculateNodeId(node, triple, collapseLiterals);

                // Skip if already written
                if (nodesAlreadyWritten.Add(id))
                {
                    WriteNode(writer, id.GetHashCode().ToString(), node.ToString());
                }
            }

            WriteEdge(writer, triple, collapseLiterals);
        }
    }

    private static void WriteEdge(XmlWriter writer, Triple triple, bool collapseLiterals)
    {
        writer.WriteStartElement(GraphMLSpecsHelper.Edge);
        writer.WriteAttributeString(GraphMLSpecsHelper.Source, triple.Subject.GetHashCode().ToString());

        writer.WriteStartAttribute(GraphMLSpecsHelper.Target);

        var id = CalculateNodeId(triple.Object, triple, collapseLiterals);
        writer.WriteString(id.GetHashCode().ToString());

        WriteData(writer, GraphMLSpecsHelper.EdgeLabel, triple.Predicate.ToString());

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

        WriteData(writer, GraphMLSpecsHelper.NodeLabel, label);

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
