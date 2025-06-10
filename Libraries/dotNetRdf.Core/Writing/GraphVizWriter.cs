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
using System.IO;
using System.Text;
using VDS.RDF.Writing.Contexts;

namespace VDS.RDF.Writing;

/// <summary>
/// A Writer which generates GraphViz DOT Format files from an RDF Graph.
/// </summary>
public class GraphVizWriter : BaseRdfWriter, IPrettyPrintingWriter, ICollapseLiteralsWriter
{
    /// <summary>
    /// Gets/Sets Pretty Print Mode for the Writer.
    /// </summary>
    public bool PrettyPrintMode { get; set; } = true;

    /// <summary>
    /// Gets/Sets whether to collapse distinct literal nodes.
    /// </summary>
    public bool CollapseLiterals { get; set; } = true;

    /// <summary>
    /// Saves a Graph into GraphViz DOT Format.
    /// </summary>
    /// <param name="g">Graph to save.</param>
    /// <param name="filename">File to save to.</param>
    public override void Save(IGraph g, string filename)
    {
        // Open the Stream for the File
        using var output = new StreamWriter(File.OpenWrite(filename), Encoding.UTF8);

        // Call the other version of Save to do the actual work
        Save(g, output);
    }

    /// <summary>
    /// Saves a Graph into GraphViz DOT Format.
    /// </summary>
    /// <param name="g">Graph to save.</param>
    /// <param name="output">Stream to save to.</param>
    protected override void SaveInternal(IGraph g, TextWriter output)
    {
        var context = new BaseWriterContext(g, output) { PrettyPrint = PrettyPrintMode };

        WriteGraph(context, CollapseLiterals);
    }

    private static void WriteGraph(BaseWriterContext context, bool collapseLiterals)
    {
        context.Output.Write(Dot.Digraph);

        if (context.Graph.BaseUri != null)
        {
            var graphId = ReduceToQName(context.Graph.BaseUri, context);

            Prettify(Dot.Space, context);
            WriteQuoted(graphId, context);
        }

        Prettify(Dot.Space, context);
        context.Output.Write(Dot.OpenCurly);
        Prettify(Dot.NewLine, context);

        foreach (Triple t in context.Graph.Triples)
        {
            WriteTriple(t, context, collapseLiterals);
        }

        context.Output.Write(Dot.CloseCurly);
    }

    private static void WriteTriple(Triple triple, BaseWriterContext context, bool collapseLiterals)
    {
        if (triple.Predicate is IUriNode predicateNode)
        {
            // Output Node lines for Literal Node so we show them as Boxes
            // This is in keeping with Standard Graph representation of RDF
            // Literals are shown in Boxes, Uri Nodes in ellipses (GraphViz's default shape)
            var subjectId = ProcessNode(triple, TripleSegment.Subject, context, collapseLiterals);
            var objectId = ProcessNode(triple, TripleSegment.Object, context, collapseLiterals);

            // Output the actual lines that state the relationship between the Nodes
            // We use the Predicate as the Label on the relationship
            var predicateLabel = ReduceToQName(predicateNode.Uri, context);

            Prettify(Dot.Tab, context);
            WriteQuoted(subjectId, context);
            Prettify(Dot.Space, context);
            context.Output.Write(Dot.Arrow);
            Prettify(Dot.Space, context);
            WriteQuoted(objectId, context);
            Prettify(Dot.Space, context);
            context.Output.Write(Dot.OpenSquare);
            context.Output.Write(Dot.Label);
            Prettify(Dot.Space, context);
            context.Output.Write(Dot.Equal);
            Prettify(Dot.Space, context);
            WriteQuoted(predicateLabel, context);
            context.Output.Write(Dot.CloseSquare);
            context.Output.Write(Dot.Semicolon);
            Prettify(Dot.NewLine, context);
        }
    }

    private static string ProcessNode(Triple t, TripleSegment segment, BaseWriterContext context, bool collapseLiterals)
    {
        INode node = GetNode(t, segment);

        switch (node)
        {
            case ILiteralNode literalNode:
                return WriteLiteralNode(literalNode, t, context, collapseLiterals);

            case IUriNode uriNode:
                return ReduceToQName(uriNode.Uri, context);

            case IBlankNode blankNode:
                return blankNode.ToString();

            default:
                throw new RdfOutputException("Only Uri nodes, literal nodes and blank nodes can be converted to GraphViz DOT Format.");
        }
    }

    private static string WriteLiteralNode(ILiteralNode literalNode, Triple t, BaseWriterContext context, bool collapseLiterals)
    {
        // Example output:
        //     "h" [label = "v", shape = box];
        // where h is the hash of the triple containing the literal node
        // and v is value of literal node

        // Literal nodes are identified either by their value or by their containing triple.
        // When identified by value, there will be a single node representing all literals with the same value.
        // When identified by triple, there will be a separate node representing each triple that has an object with that value.
        var idObject = collapseLiterals ? literalNode : t as object;
        var nodeId = idObject.GetHashCode().ToString();

        Prettify(Dot.Tab, context);
        WriteQuoted(nodeId, context);
        Prettify(Dot.Space, context);
        context.Output.Write(Dot.OpenSquare);
        context.Output.Write(Dot.Label);
        Prettify(Dot.Space, context);
        context.Output.Write(Dot.Equal);
        Prettify(Dot.Space, context);
        WriteLiteralNodeLabel(literalNode, context);
        context.Output.Write(Dot.Comma);
        Prettify(Dot.Space, context);
        context.Output.Write(Dot.Shape);
        Prettify(Dot.Space, context);
        context.Output.Write(Dot.Equal);
        Prettify(Dot.Space, context);
        context.Output.Write(Dot.Box);
        context.Output.Write(Dot.CloseSquare);
        context.Output.Write(Dot.NewLine);

        return nodeId;
    }

    private static void WriteQuoted(string value, IWriterContext context)
    {
        context.Output.Write(Dot.Quote);

        context.Output.Write(value);

        context.Output.Write(Dot.Quote);
    }

    private static void WriteLiteralNodeLabel(ILiteralNode literalNode, BaseWriterContext context)
    {
        var nodeValue = Escape(literalNode.Value);

        context.Output.Write(Dot.Quote);
        context.Output.Write(nodeValue);

        if (!string.IsNullOrEmpty(literalNode.Language))
        {
            context.Output.Write("@");
            context.Output.Write(literalNode.Language);
        }

        if (literalNode.DataType != null)
        {
            var datatype = ReduceToQName(literalNode.DataType, context);

            context.Output.Write("^^");
            context.Output.Write(datatype);
        }

        context.Output.Write(Dot.Quote);
    }

    private static string ReduceToQName(Uri uri, BaseWriterContext context)
    {
        if (!context.QNameMapper.ReduceToQName(uri.ToString(), out var result))
        {
            result = uri.ToString();
        }

        return result;
    }

    private static void Prettify(string value, BaseWriterContext context)
    {
        if (context.PrettyPrint)
        {
            context.Output.Write(value);
        }
    }

    private static string Escape(string value)
    {
        value = value.Replace("\"", "\\\"");
        value = value.Replace("\n", "\\n");

        return value;
    }

    private static INode GetNode(Triple t, TripleSegment segment)
    {
        switch (segment)
        {
            case TripleSegment.Subject:
                return t.Subject;

            case TripleSegment.Object:
                return t.Object;

            default:
                return null;
        }
    }

    /// <summary>
    /// Event that is raised if there is a potential problem with the RDF being output
    /// </summary>
    /// <remarks>This class does not raise this event.</remarks>
#pragma warning disable CS0067
    public override event RdfWriterWarning Warning;
#pragma warning restore CS0067

    /// <summary>
    /// Gets the String representation of the writer which is a description of the syntax it produces.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return "GraphViz DOT";
    }

    private static class Dot
    {
        internal const string Digraph = "digraph";
        internal const string Label = "label";
        internal const string Shape = "shape";
        internal const string Box = "box";
        internal const string Quote = "\"";
        internal const string Arrow = "->";
        internal const string Equal = "=";
        internal const string OpenSquare = "[";
        internal const string CloseSquare = "]";
        internal const string OpenCurly = "{";
        internal const string CloseCurly = "}";
        internal const string Semicolon = ";";
        internal const string Space = " ";
        internal const string Tab = "    ";
        internal const string Comma = ",";
        internal const string NewLine = "\n";
    }
}
