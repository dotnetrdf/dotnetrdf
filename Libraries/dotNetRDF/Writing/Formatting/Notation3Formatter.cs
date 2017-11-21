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

using System.Text;

namespace VDS.RDF.Writing.Formatting
{
    /// <summary>
    /// Formatter for formatting as Notation 3 without any compression
    /// </summary>
    public class UncompressedNotation3Formatter
        : UncompressedTurtleFormatter
    {
        /// <summary>
        /// Creates a new Uncompressed Notation 3 Formatter
        /// </summary>
        public UncompressedNotation3Formatter()
            : base("Notation 3 (Uncompressed)") { }

        /// <summary>
        /// Formats a Variable Node for Notation 3
        /// </summary>
        /// <param name="v">Variable</param>
        /// <param name="segment">Triple Segment</param>
        /// <returns></returns>
        protected override string FormatVariableNode(IVariableNode v, TripleSegment? segment)
        {
            return v.ToString();
        }

        /// <summary>
        /// Formats a Graph Literal Node for Notation 3
        /// </summary>
        /// <param name="glit">Graph Literal</param>
        /// <param name="segment">Triple Segment</param>
        /// <returns></returns>
        protected override string FormatGraphLiteralNode(IGraphLiteralNode glit, TripleSegment? segment)
        {
            if (segment == TripleSegment.Predicate) throw new RdfOutputException(WriterErrorMessages.GraphLiteralPredicatesUnserializable(FormatName));

            StringBuilder output = new StringBuilder();
            output.Append("{");
            foreach (Triple t in glit.SubGraph.Triples)
            {
                output.Append(Format(t));
            }
            output.Append("}");
            return output.ToString();
        }
    }

    /// <summary>
    /// Formatter for formatting as Notation 3
    /// </summary>
    public class Notation3Formatter 
        : TurtleFormatter
    {
        /// <summary>
        /// Creates a new Notation 3 Formatter
        /// </summary>
        public Notation3Formatter()
            : base("Notation 3", new QNameOutputMapper()) { }

        /// <summary>
        /// Creates a new Notation 3 Formatter using the given Graph
        /// </summary>
        /// <param name="g">Graph</param>
        public Notation3Formatter(IGraph g)
            : base("Notation 3", new QNameOutputMapper(g.NamespaceMap)) { }

        /// <summary>
        /// Creates a new Notation 3 Formatter using the given Namespace Map
        /// </summary>
        /// <param name="nsmap">Namespace Map</param>
        public Notation3Formatter(INamespaceMapper nsmap)
            : base("Notation 3", new QNameOutputMapper(nsmap)) { }

        /// <summary>
        /// Formats a Variable Node for Notation 3
        /// </summary>
        /// <param name="v">Variable</param>
        /// <param name="segment">Triple Segment</param>
        /// <returns></returns>
        protected override string FormatVariableNode(IVariableNode v, TripleSegment? segment)
        {
            return v.ToString();
        }

        /// <summary>
        /// Formats a Graph Literal Node for Notation 3
        /// </summary>
        /// <param name="glit">Graph Literal</param>
        /// <param name="segment">Triple Segment</param>
        /// <returns></returns>
        protected override string FormatGraphLiteralNode(IGraphLiteralNode glit, TripleSegment? segment)
        {
            if (segment == TripleSegment.Predicate) throw new RdfOutputException(WriterErrorMessages.GraphLiteralPredicatesUnserializable(FormatName));

            StringBuilder output = new StringBuilder();
            output.Append("{");
            foreach (Triple t in glit.SubGraph.Triples)
            {
                output.Append(Format(t));
            }
            output.Append("}");
            return output.ToString();
        }
    }
}
