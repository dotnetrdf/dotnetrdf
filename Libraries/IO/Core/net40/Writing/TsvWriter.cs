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
using VDS.RDF.Storage;
using VDS.RDF.Writing.Contexts;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Writing
{
    /// <summary>
    /// Class for generating TSV files from RDF Graphs
    /// </summary>
    public class TsvWriter 
        : IRdfWriter, IFormatterBasedWriter
    {
        private TsvFormatter _formatter = new TsvFormatter();

        /// <summary>
        /// Gets the type of the Triple Formatter used by this writer
        /// </summary>
        public Type TripleFormatterType
        {
            get
            {
                return this._formatter.GetType();
            }
        }

        /// <summary>
        /// Saves a Graph to TSV format
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
                    output.Write('\t');
                    this.GenerateNodeOutput(output, t.Predicate, QuadSegment.Predicate);
                    output.Write('\t');
                    this.GenerateNodeOutput(output, t.Object, QuadSegment.Object);
                    output.Write('\n');
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
            try
            {
                foreach (INode graphName in graphStore.GraphNames)
                {
                    IGraph g = graphStore[graphName];

                    // Write out quads for the graph
                    foreach (Quad q in g.Triples.AsQuads(graphName))
                    {
                        this.GenerateNodeOutput(output, q.Subject, QuadSegment.Subject);
                        output.Write('\t');
                        this.GenerateNodeOutput(output, q.Predicate, QuadSegment.Predicate);
                        output.Write('\t');
                        this.GenerateNodeOutput(output, q.Object, QuadSegment.Object);
                        if (!q.InDefaultGraph)
                        {
                            output.Write('\t');
                            this.GenerateNodeOutput(output, q.Graph, QuadSegment.Graph);
                        }
                        output.WriteLine();
                    }
                }
                output.Close();
            }
            finally
            {
                output.CloseQuietly();
            }
        }

        private void GenerateNodeOutput(TextWriter output, INode n, QuadSegment segment)
        {
            switch (n.NodeType)
            {
                case NodeType.GraphLiteral:
                    throw new RdfOutputException(WriterErrorMessages.GraphLiteralsUnserializable("TSV"));
                case NodeType.Blank:
                case NodeType.Literal:
                case NodeType.Uri:
                    output.Write(this._formatter.Format(n, segment));
                    break;
                case NodeType.Variable:
                    throw new RdfOutputException(WriterErrorMessages.VariableNodesUnserializable("TSV"));
                default:
                    throw new RdfOutputException(WriterErrorMessages.UnknownNodeTypeUnserializable("TSV"));
            }
        }

        /// <summary>
        /// Event which is raised if the Writer detects a non-fatal error with the RDF being output
        /// </summary>
        public event RdfWriterWarning Warning;

        /// <summary>
        /// Gets the String representation of the writer which is a description of the syntax it produces
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "TSV";
        }
    }

}
