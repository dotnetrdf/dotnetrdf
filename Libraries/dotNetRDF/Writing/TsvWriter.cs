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
using System.Text;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Writing
{
    /// <summary>
    /// Class for generating TSV files from RDF Graphs
    /// </summary>
    public class TsvWriter 
        : BaseRdfWriter, IFormatterBasedWriter
    {
        private TsvFormatter _formatter = new TsvFormatter();

        /// <summary>
        /// Gets the type of the Triple Formatter used by this writer
        /// </summary>
        public Type TripleFormatterType
        {
            get
            {
                return _formatter.GetType();
            }
        }

        /// <summary>
        /// Saves a Graph to TSV format
        /// </summary>
        /// <param name="g">Graph</param>
        /// <param name="filename">File to save to</param>
        public override void Save(IGraph g, string filename)
        {
            using (var stream = File.Open(filename, FileMode.Create))
            {
                Save(g, new StreamWriter(stream, new UTF8Encoding(Options.UseBomForUtf8)));
            }
        }

        /// <summary>
        /// Saves a Graph to TSV format
        /// </summary>
        /// <param name="g">Graph</param>
        /// <param name="output">Writer to save to</param>
        protected override void SaveInternal(IGraph g, TextWriter output)
        {
            foreach (Triple t in g.Triples)
            {
                GenerateNodeOutput(output, t.Subject, TripleSegment.Subject);
                output.Write('\t');
                GenerateNodeOutput(output, t.Predicate, TripleSegment.Predicate);
                output.Write('\t');
                GenerateNodeOutput(output, t.Object, TripleSegment.Object);
                output.Write('\n');
            }
        }

        private void GenerateNodeOutput(TextWriter output, INode n, TripleSegment segment)
        {
            switch (n.NodeType)
            {
                case NodeType.GraphLiteral:
                    throw new RdfOutputException(WriterErrorMessages.GraphLiteralsUnserializable("TSV"));
                case NodeType.Blank:
                case NodeType.Literal:
                case NodeType.Uri:
                    output.Write(_formatter.Format(n));
                    break;
                default:
                    throw new RdfOutputException(WriterErrorMessages.UnknownNodeTypeUnserializable("TSV"));
            }
        }

        /// <summary>
        /// Event which is raised if the Writer detects a non-fatal error with the RDF being output
        /// </summary>
        public override event RdfWriterWarning Warning;

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
