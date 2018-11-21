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

using System;
using System.IO;
using VDS.RDF.Writing.Contexts;

namespace VDS.RDF.Writing
{
    /// <summary>
    /// A Writer which generates GraphViz DOT Format files from an RDF Graph
    /// </summary>
    public class GraphVizWriter : BaseRdfWriter, IPrettyPrintingWriter, ICollapseLiteralsWriter
    {
        /// <summary>
        /// Gets/Sets Pretty Print Mode for the Writer
        /// </summary>
        public bool PrettyPrintMode { get; set; } = true;

        /// <summary>
        /// Gets/Sets whether to collapse distinct literal nodes
        /// </summary>
        public bool CollapseLiterals { get; set; } = true;

        /// <summary>
        /// Saves a Graph into GraphViz DOT Format
        /// </summary>
        /// <param name="g">Graph to save</param>
        /// <param name="filename">File to save to</param>
        public override void Save(IGraph g, string filename)
        {
            // Open the Stream for the File
            StreamWriter output = new StreamWriter(File.OpenWrite(filename));

            // Call the other version of Save to do the actual work
            Save(g, output);
        }

        /// <summary>
        /// Saves a Graph into GraphViz DOT Format
        /// </summary>
        /// <param name="g">Graph to save</param>
        /// <param name="output">Stream to save to</param>
        protected override void SaveInternal(IGraph g, TextWriter output)
        {
            var context = new BaseWriterContext(g, output) { PrettyPrint = this.PrettyPrintMode };

            GraphVizWriter.WriteGraph(context, this.CollapseLiterals);
        }

        private static void WriteGraph(BaseWriterContext context, bool collapseLiterals)
        {
            context.Output.Write(DOT.Digraph);

            if (context.Graph.BaseUri != null)
            {
                var graphId = GraphVizWriter.ReduceToQName(context.Graph.BaseUri, context);

                GraphVizWriter.Prettify(DOT.Space, context);
                GraphVizWriter.WriteQuoted(graphId, context);
            }

            GraphVizWriter.Prettify(DOT.Space, context);
            context.Output.Write(DOT.OpenCurly);
            GraphVizWriter.Prettify(DOT.NewLine, context);

            foreach (var t in context.Graph.Triples)
            {
                GraphVizWriter.WriteTriple(t, context, collapseLiterals);
            }

            context.Output.Write(DOT.CloseCurly);
        }

        private static void WriteTriple(Triple triple, BaseWriterContext context, bool collapseLiterals)
        {
            // Output Node lines for Literal Node so we show them as Boxes
            // This is in keeping with Standard Graph representation of RDF
            // Literals are shown in Boxes, Uri Nodes in ellipses (GraphViz's default shape)
            var subjectId = GraphVizWriter.ProcessNode(triple, TripleSegment.Subject, context, collapseLiterals);
            var objectId = GraphVizWriter.ProcessNode(triple, TripleSegment.Object, context, collapseLiterals);

            // Output the actual lines that state the relationship between the Nodes
            // We use the Predicate as the Label on the relationship
            var predicateLabel = GraphVizWriter.ReduceToQName((triple.Predicate as IUriNode).Uri, context);

            GraphVizWriter.Prettify(DOT.Tab, context);
            GraphVizWriter.WriteQuoted(subjectId, context);
            GraphVizWriter.Prettify(DOT.Space, context);
            context.Output.Write(DOT.Arrow);
            GraphVizWriter.Prettify(DOT.Space, context);
            GraphVizWriter.WriteQuoted(objectId, context);
            GraphVizWriter.Prettify(DOT.Space, context);
            context.Output.Write(DOT.OpenSquare);
            context.Output.Write(DOT.Label);
            GraphVizWriter.Prettify(DOT.Space, context);
            context.Output.Write(DOT.Equal);
            GraphVizWriter.Prettify(DOT.Space, context);
            GraphVizWriter.WriteQuoted(predicateLabel, context);
            context.Output.Write(DOT.CloseSquare);
            context.Output.Write(DOT.Semicolon);
            GraphVizWriter.Prettify(DOT.NewLine, context);
        }

        private static string ProcessNode(Triple t, TripleSegment segment, BaseWriterContext context, bool collapseLiterals)
        {
            var node = GraphVizWriter.GetNode(t, segment);

            switch (node)
            {
                case ILiteralNode literalnode:
                    return GraphVizWriter.WriteLiteralNode(literalnode, t, context, collapseLiterals);

                case IUriNode uriNode:
                    return GraphVizWriter.ReduceToQName(uriNode.Uri, context);

                case IBlankNode blankNode:
                    return blankNode.ToString();

                default:
                    throw new RdfOutputException("Only Uri nodes, literal nodes and blank nodes can be converted to GraphViz DOT Format.");
            }
        }

        private static string WriteLiteralNode(ILiteralNode literalnode, Triple t, BaseWriterContext context, bool collapseLiterals)
        {
            // Example output:
            //     "h" [label = "v", shape = box];
            // where h is the hash of the triple containing the literal node
            // and v is value of literal node

            // Literal nodes are identified either by their value or by their containing triple.
            // When identified by value, there will be a single node representing all literals with the same value.
            // When identified by triple, there will be a separate node representing each triple that has an object with that value.
            var idObject = collapseLiterals ? literalnode as object : t as object;
            var nodeId = idObject.GetHashCode().ToString();

            GraphVizWriter.Prettify(DOT.Tab, context);
            GraphVizWriter.WriteQuoted(nodeId, context);
            GraphVizWriter.Prettify(DOT.Space, context);
            context.Output.Write(DOT.OpenSquare);
            context.Output.Write(DOT.Label);
            GraphVizWriter.Prettify(DOT.Space, context);
            context.Output.Write(DOT.Equal);
            GraphVizWriter.Prettify(DOT.Space, context);
            GraphVizWriter.WriteLiteralNodeLabel(literalnode, context);
            context.Output.Write(DOT.Comma);
            GraphVizWriter.Prettify(DOT.Space, context);
            context.Output.Write(DOT.Shape);
            GraphVizWriter.Prettify(DOT.Space, context);
            context.Output.Write(DOT.Equal);
            GraphVizWriter.Prettify(DOT.Space, context);
            context.Output.Write(DOT.Box);
            context.Output.Write(DOT.CloseSquare);
            context.Output.Write(DOT.NewLine, context);

            return nodeId;
        }

        private static void WriteQuoted(string value, BaseWriterContext context)
        {
            context.Output.Write(DOT.Quote);

            context.Output.Write(value);

            context.Output.Write(DOT.Quote);
        }

        private static void WriteLiteralNodeLabel(ILiteralNode literalnode, BaseWriterContext context)
        {
            var nodeValue = GraphVizWriter.Escape(literalnode.Value);

            context.Output.Write(DOT.Quote);
            context.Output.Write(nodeValue);

            if (!string.IsNullOrEmpty(literalnode.Language))
            {
                context.Output.Write("@");
                context.Output.Write(literalnode.Language);
            }

            if (literalnode.DataType != null)
            {
                string datatype = GraphVizWriter.ReduceToQName(literalnode.DataType, context);

                context.Output.Write("^^");
                context.Output.Write(datatype);
            }

            context.Output.Write(DOT.Quote);
        }

        private static string ReduceToQName(Uri uri, BaseWriterContext context)
        {
            if (!context.QNameMapper.ReduceToQName(uri.ToString(), out string result))
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
        /// <remarks>Not used by this Writer</remarks>
        public override event RdfWriterWarning Warning;

        /// <summary>
        /// Gets the String representation of the writer which is a description of the syntax it produces
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "GraphViz DOT";
        }

        private static class DOT
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
}
