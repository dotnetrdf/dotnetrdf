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
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using VDS.RDF.Graphs;
using VDS.RDF.Nodes;
using VDS.RDF.Parsing;
using VDS.RDF.Storage;
using VDS.RDF.Writing.Contexts;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Writing
{
    /// <summary>
    /// Class for generating CSV output from RDF Graphs
    /// </summary>
    public class CsvWriter 
        : IRdfWriter, IFormatterBasedWriter
    {
        private CsvFormatter _formatter = new CsvFormatter();

        /// <summary>
        /// Gets the type of the Triple Formatter used by the writer
        /// </summary>
        public Type TripleFormatterType
        {
            get
            {
                return _formatter.GetType();
            }
        }

#if !NO_FILE
        /// <summary>
        /// Saves a Graph to CSV format
        /// </summary>
        /// <param name="g">Graph</param>
        /// <param name="filename">File to save to</param>
        public void Save(IGraph g, string filename)
        {
            this.Save(g, new StreamWriter(filename, false, Encoding.UTF8));
        }
#endif

        /// <summary>
        /// Saves a Graph to CSV format
        /// </summary>
        /// <param name="g">Graph</param>
        /// <param name="output">Writer to save to</param>
        public void Save(IGraph g, TextWriter output)
        {
            try
            {
                foreach (Triple t in g.Triples)
                {
                    this.GenerateNodeOutput(output, t.Subject, QuadSegment.Subject);
                    output.Write(',');
                    this.GenerateNodeOutput(output, t.Predicate, QuadSegment.Predicate);
                    output.Write(',');
                    this.GenerateNodeOutput(output, t.Object, QuadSegment.Object);
                    output.Write("\r\n");
                }

                output.Close();
            }
            finally
            {
                output.CloseQuietly();
            }
        }

        public void Save(IGraphStore graphStore, TextWriter output)
        {
            if (graphStore == null) throw new RdfOutputException("Cannot output a null Graph Store");
            if (output == null) throw new RdfOutputException("Cannot output to a null writer");

            try
            {
                foreach (Quad quad in graphStore.Quads)
                {
                    this.GenerateNodeOutput(output, quad.Subject, QuadSegment.Subject);
                    output.Write(',');
                    this.GenerateNodeOutput(output, quad.Predicate, QuadSegment.Predicate);
                    output.Write(',');
                    this.GenerateNodeOutput(output, quad.Object, QuadSegment.Object);
                    output.Write(',');
                    this.GenerateNodeOutput(output, quad.Graph, QuadSegment.Graph);
                    output.Write("\r\n");
                }

                output.Close();
            }
            finally
            {
                output.CloseQuietly();
            }
        }

        /// <summary>
        /// Generates Node Output for the given Node
        /// </summary>
        /// <param name="output">Text Writer</param>
        /// <param name="n">Node</param>
        /// <param name="segment">Triple Segment</param>
        private void GenerateNodeOutput(TextWriter output, INode n, QuadSegment segment)
        {
            switch (n.NodeType)
            {
                case NodeType.Blank:
                    if (segment == QuadSegment.Predicate) throw new RdfOutputException(WriterErrorMessages.BlankPredicatesUnserializable("CSV"));

                    output.Write(this._formatter.Format(n));
                    break;

                case NodeType.GraphLiteral:
                    throw new RdfOutputException(WriterErrorMessages.GraphLiteralsUnserializable("CSV"));

                case NodeType.Literal:
                    if (segment == QuadSegment.Subject) throw new RdfOutputException(WriterErrorMessages.LiteralSubjectsUnserializable("CSV"));
                    if (segment == QuadSegment.Predicate) throw new RdfOutputException(WriterErrorMessages.LiteralPredicatesUnserializable("CSV"));

                    output.Write(this._formatter.Format(n));
                    break;

                case NodeType.Uri:
                    output.Write(this._formatter.Format(n));
                    break;

                case NodeType.Variable:
                    throw new RdfOutputException(WriterErrorMessages.VariableNodesUnserializable("CSV"));

                default:
                    throw new RdfOutputException(WriterErrorMessages.UnknownNodeTypeUnserializable("CSV"));
            }
        }

        /// <summary>
        /// Event which is raised if the Writer detects a non-fatal error while outputting CSV
        /// </summary>
        public event RdfWriterWarning Warning;

        /// <summary>
        /// Gets the String representation of the writer which is a description of the syntax it produces
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "CSV";
        }
    }
}
