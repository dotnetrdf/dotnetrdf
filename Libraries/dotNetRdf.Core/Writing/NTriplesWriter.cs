/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2023 dotNetRDF Project (http://dotnetrdf.org/)
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
using System.Linq;
using System.Text;
using VDS.RDF.Parsing;
using VDS.RDF.Writing.Contexts;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Writing
{
    /// <summary>
    /// Class for generating RDF in NTriples Concrete Syntax.
    /// </summary>
    /// <threadsafety instance="true">Designed to be Thread Safe - should be able to call the Save() method from multiple threads on different Graphs without issue.</threadsafety>
    public class NTriplesWriter 
        : BaseRdfWriter, IFormatterBasedWriter, IRdfStarCapableWriter
    {
        private bool _sort = false;

        /// <summary>
        /// Creates a new writer.
        /// </summary>
        /// <param name="syntax">NTriples Syntax Mode.</param>
        public NTriplesWriter(NTriplesSyntax syntax)
        {
            Syntax = syntax;
        }

        /// <summary>
        /// Creates a new writer.
        /// </summary>
        public NTriplesWriter()
            : this(NTriplesSyntax.Original) { }

        /// <summary>
        /// Gets/Sets whether Triples are sorted before being Output.
        /// </summary>
        public bool SortTriples
        {
            get 
            {
                return _sort;
            }
            set
            {
                _sort = value;
            }
        }

        /// <summary>
        /// Gets the type of the Triple Formatter used by this writer.
        /// </summary>
        public Type TripleFormatterType
        {
            get
            {
                return typeof(NTriplesFormatter);
            }
        }

        /// <summary>
        /// Gets/Sets the NTriples syntax mode.
        /// </summary>
        public NTriplesSyntax Syntax { get; set; }

        /// <summary>
        /// Gets whether the current syntax mode supports writing RDF-Star triple nodes or not.
        /// </summary>
        public bool CanWriteTripleNodes => Syntax == NTriplesSyntax.Rdf11Star;

        /// <summary>
        /// Saves the Graph in NTriples Syntax to the given stream.
        /// </summary>
        /// <param name="g">Graph to save.</param>
        /// <param name="filename">File to save to.</param>
        public override void Save(IGraph g, string filename)
        {
            using (var writer = new StreamWriter(File.Open(filename, FileMode.Create), Encoding.ASCII))
            {
                Save(g, writer);
            }
        }

        /// <summary>
        /// Saves the Graph in NTriples Syntax to the given stream.
        /// </summary>
        /// <param name="g">Graph to save.</param>
        /// <param name="output">Stream to save to.</param>
        protected override void SaveInternal(IGraph g, TextWriter output)
        {
            var context = new NTriplesWriterContext(g, output, Syntax);
            var ts = g.Triples.ToList();
            if (_sort) ts.Sort(new FullTripleComparer(new FastNodeComparer()));

            foreach (Triple t in ts)
            {
                output.WriteLine(TripleToNTriples(context, t));
            }
        }

        /// <summary>
        /// Converts a Triple into relevant NTriples Syntax.
        /// </summary>
        /// <param name="context">Writer Context.</param>
        /// <param name="t">Triple to convert.</param>
        /// <returns></returns>
        private string TripleToNTriples(NTriplesWriterContext context, Triple t)
        {
            var output = new StringBuilder();
            output.Append(NodeToNTriples(context, t.Subject, TripleSegment.Subject));
            output.Append(" ");
            output.Append(NodeToNTriples(context, t.Predicate, TripleSegment.Predicate));
            output.Append(" ");
            output.Append(NodeToNTriples(context, t.Object, TripleSegment.Object));
            output.Append(" .");

            return output.ToString();
        }

        /// <summary>
        /// Converts a Node into relevant NTriples Syntax.
        /// </summary>
        /// <param name="context">Writer Context.</param>
        /// <param name="n">Node to convert.</param>
        /// <param name="segment">Segment of the Triple being written.</param>
        /// <returns></returns>
        private string NodeToNTriples(NTriplesWriterContext context, INode n, TripleSegment segment)
        {
            switch (n.NodeType)
            {
                case NodeType.Blank:
                    if (segment == TripleSegment.Predicate) throw new RdfOutputException(WriterErrorMessages.BlankPredicatesUnserializable("NTriples"));
                    break;

                case NodeType.Literal:
                    if (segment == TripleSegment.Subject) throw new RdfOutputException(WriterErrorMessages.LiteralSubjectsUnserializable("NTriples"));
                    if (segment == TripleSegment.Predicate) throw new RdfOutputException(WriterErrorMessages.LiteralPredicatesUnserializable("NTriples"));
                    break;

                case NodeType.Uri:
                    break;

                case NodeType.Triple:
                    if (Syntax != NTriplesSyntax.Rdf11Star)
                    {
                        throw new RdfOutputException(WriterErrorMessages.TripleNodesUnserializable(ToString()));
                    }

                    if (segment == TripleSegment.Predicate)
                        throw new RdfOutputException(WriterErrorMessages.TripleNodePredicateUnserializable(ToString()));
                    break;

                case NodeType.GraphLiteral:
                    throw new RdfOutputException(WriterErrorMessages.GraphLiteralsUnserializable("NTriples"));

                default:
                    throw new RdfOutputException(WriterErrorMessages.UnknownNodeTypeUnserializable("NTriples"));
            }

            return context.NodeFormatter.Format(n);
        }

        /// <summary>
        /// Event which is raised when there is an issue with the Graph being serialized that doesn't prevent serialization but the user should be aware of
        /// </summary>
        public override event RdfWriterWarning Warning;

        /// <summary>
        /// Internal Helper method which raises the Warning event only if there is an Event Handler registered.
        /// </summary>
        /// <param name="message">Warning Message.</param>
        private void RaiseWarning(string message)
        {
            RdfWriterWarning d = Warning;
            if (d != null)
            {
                d(message);
            }
        }

        /// <summary>
        /// Gets the String representation of the writer which is a description of the syntax it produces.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Syntax switch
            {
                NTriplesSyntax.Original => "NTriples",
                NTriplesSyntax.Rdf11 => "NTriples (RDF 1.1)",
                NTriplesSyntax.Rdf11Star => "NTriples (RDF-Star)",
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}
