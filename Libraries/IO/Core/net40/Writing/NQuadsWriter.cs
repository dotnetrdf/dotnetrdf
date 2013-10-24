/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

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
using System.IO;
using System.Text;
using VDS.RDF.Graphs;
using VDS.RDF.Nodes;
using VDS.RDF.Writing.Contexts;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Writing
{
    /// <summary>
    /// Class for serializing a Triple Store in the NQuads (NTriples plus context) syntax
    /// </summary>
    public class NQuadsWriter 
        : IRdfWriter, IPrettyPrintingWriter, IFormatterBasedWriter
    {

        /// <summary>
        /// Controls whether Pretty Printing is used
        /// </summary>
        /// <remarks>
        /// For NQuads this simply means that Graphs in the output are separated with Whitespace and comments used before each Graph
        /// </remarks>
        public bool PrettyPrintMode { get; set; }

        /// <summary>
        /// Gets the type of the Triple Formatter used by this writer
        /// </summary>
        public Type TripleFormatterType
        {
            get
            {
                return typeof(NQuadsFormatter);
            }
        }

        /// <summary>
        /// Saves a Store in NQuads format
        /// </summary>
        /// <param name="store">Store to save</param>
        /// <param name="writer">Writer to save to</param>
        public void Save(IGraphStore store, TextWriter writer)
        {
            if (store == null) throw new RdfOutputException("Cannot output a null Graph Store");
            if (writer == null) throw new RdfOutputException("Cannot output to a null writer");

            try
            {
                    foreach (Quad q in store.Quads)
                    {
                        NTriplesWriterContext graphContext = new NTriplesWriterContext(g, context.Output);
                        foreach (Triple t in g.Triples)
                        {
                            writer.WriteLine(this.TripleToNQuads(graphContext, t, g.BaseUri));
                        }
                    }
                    writer.Close();
            }
            catch
            {
                try
                {
                    writer.Close();
                }
                catch
                {
                    //Just cleaning up
                }
                throw;
            }
        }

        /// <summary>
        /// Converts a Triple into relevant NQuads Syntax
        /// </summary>
        /// <param name="context">Writer Context</param>
        /// <param name="t">Triple to convert</param>
        /// <returns></returns>
        private String TripleToNQuads(NTriplesWriterContext context, Triple t, Uri graphUri)
        {
            StringBuilder output = new StringBuilder();
            output.Append(this.NodeToNTriples(context, t.Subject, QuadSegment.Subject));
            output.Append(" ");
            output.Append(this.NodeToNTriples(context, t.Predicate, QuadSegment.Predicate));
            output.Append(" ");
            output.Append(this.NodeToNTriples(context, t.Object, QuadSegment.Object));
            if (graphUri != null)
            {
                output.Append(" <");
                output.Append(context.UriFormatter.FormatUri(graphUri));
                output.Append(">");
            }
            output.Append(" .");

            return output.ToString();
        }

        /// <summary>
        /// Converts a Node into relevant NTriples Syntax
        /// </summary>
        /// <param name="n">Node to convert</param>
        /// <param name="context">Writer Context</param>
        /// <param name="segment">Triple Segment being written</param>
        /// <returns></returns>
        private String NodeToNTriples(NTriplesWriterContext context, INode n, QuadSegment segment)
        {
            switch (n.NodeType)
            {
                case NodeType.Blank:
                    if (segment == QuadSegment.Predicate) throw new RdfOutputException(WriterErrorMessages.BlankPredicatesUnserializable("NQuads"));
                    break;
                case NodeType.Literal:
                    if (segment == QuadSegment.Subject) throw new RdfOutputException(WriterErrorMessages.LiteralSubjectsUnserializable("NQuads"));
                    if (segment == QuadSegment.Predicate) throw new RdfOutputException(WriterErrorMessages.LiteralPredicatesUnserializable("NQuads"));
                    break;
                case NodeType.Uri:
                    break;
                case NodeType.GraphLiteral:
                    throw new RdfOutputException(WriterErrorMessages.GraphLiteralsUnserializable("NQuads"));
                default:
                    throw new RdfOutputException(WriterErrorMessages.UnknownNodeTypeUnserializable("NQuads"));
            }

            return context.NodeFormatter.Format(n);
        }

        /// <summary>
        /// Event which is raised when there is an issue with the Graphs being serialized that doesn't prevent serialization but the user should be aware of
        /// </summary>
        public event RdfWriterWarning Warning;

        /// <summary>
        /// Internal Helper method which raises the Warning event only if there is an Event Handler registered
        /// </summary>
        /// <param name="message">Warning Message</param>
        private void RaiseWarning(String message)
        {
            StoreWriterWarning d = this.Warning;
            if (d != null)
            {
                d(message);
            }
        }

        /// <summary>
        /// Gets the String representation of the writer which is a description of the syntax it produces
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "NQuads";
        }
    }
}
