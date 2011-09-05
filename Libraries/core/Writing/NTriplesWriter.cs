/*

Copyright Robert Vesse 2009-10
rvesse@vdesign-studios.com

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

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
                if (this._sort) ts.Sort();

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
