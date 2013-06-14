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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using VDS.RDF.Writing.Contexts;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Writing
{
    /// <summary>
    /// Class for generating RDF in NTriples Concrete Syntax
    /// </summary>
    /// <threadsafety instance="true">Designed to be Thread Safe - should be able to call the Save() method from multiple threads on different Graphs without issue</threadsafety>
    public class NTriplesWriter 
        : IRdfWriter, IFormatterBasedWriter
    {
        private bool _sort = false;

        /// <summary>
        /// Gets/Sets whether Triples are sorted before being Output
        /// </summary>
        public bool SortTriples
        {
            get 
            {
                return this._sort;
            }
            set
            {
                this._sort = value;
            }
        }

        /// <summary>
        /// Gets the type of the Triple Formatter used by this writer
        /// </summary>
        public Type TripleFormatterType
        {
            get
            {
                return typeof(NTriplesFormatter);
            }
        }

#if !NO_FILE
        /// <summary>
        /// Saves the Graph in NTriples Syntax to the given stream
        /// </summary>
        /// <param name="g">Graph to save</param>
        /// <param name="filename">File to save to</param>
        public void Save(IGraph g, string filename)
        {
#if SILVERLIGHT
            StreamWriter output = new StreamWriter(filename);
#else
            StreamWriter output = new StreamWriter(filename, false, Encoding.ASCII);
#endif

            this.Save(g, output);
        }
#endif

        /// <summary>
        /// Saves the Graph in NTriples Syntax to the given stream
        /// </summary>
        /// <param name="g">Graph to save</param>
        /// <param name="output">Stream to save to</param>
        public void Save(IGraph g, TextWriter output)
        {
            try
            {
                NTriplesWriterContext context = new NTriplesWriterContext(g, output);
                List<Triple> ts = g.Triples.ToList();
                if (this._sort) ts.Sort(new FullTripleComparer(new FastNodeComparer()));

                foreach (Triple t in ts)
                {
                    output.WriteLine(this.TripleToNTriples(context, t));
                }

                output.Close();
            }
            catch
            {
                //Try and ensure the Stream gets closed
                try
                {
                    output.Close();
                }
                catch
                {
                    //No Catch actions
                }
                throw;
            }
        }

        /// <summary>
        /// Converts a Triple into relevant NTriples Syntax
        /// </summary>
        /// <param name="context">Writer Context</param>
        /// <param name="t">Triple to convert</param>
        /// <returns></returns>
        private String TripleToNTriples(NTriplesWriterContext context, Triple t)
        {
            StringBuilder output = new StringBuilder();
            output.Append(this.NodeToNTriples(context, t.Subject, TripleSegment.Subject));
            output.Append(" ");
            output.Append(this.NodeToNTriples(context, t.Predicate, TripleSegment.Predicate));
            output.Append(" ");
            output.Append(this.NodeToNTriples(context, t.Object, TripleSegment.Object));
            output.Append(" .");

            return output.ToString();
        }

        /// <summary>
        /// Converts a Node into relevant NTriples Syntax
        /// </summary>
        /// <param name="context">Writer Context</param>
        /// <param name="n">Node to convert</param>
        /// <param name="segment">Segment of the Triple being written</param>
        /// <returns></returns>
        private String NodeToNTriples(NTriplesWriterContext context, INode n, TripleSegment segment)
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
        public event RdfWriterWarning Warning;

        /// <summary>
        /// Internal Helper method which raises the Warning event only if there is an Event Handler registered
        /// </summary>
        /// <param name="message">Warning Message</param>
        private void RaiseWarning(String message)
        {
            RdfWriterWarning d = this.Warning;
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
            return "NTriples";
        }
    }
}
