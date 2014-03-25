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
using VDS.RDF.Graphs;
using VDS.RDF.Parsing;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Writing
{
    /// <summary>
    /// Class for serializing a Triple Store in the NQuads (NTriples plus context) syntax
    /// </summary>
    public class NQuadsWriter
        : IRdfWriter, IFormatterBasedWriter
    {
        /// <summary>
        /// Creates a new writer
        /// </summary>
        public NQuadsWriter()
            : this(NQuadsSyntax.Original)
        {
        }

        /// <summary>
        /// Creates a new writer
        /// </summary>
        /// <param name="syntax">NQuads Syntax mode to use</param>
        public NQuadsWriter(NQuadsSyntax syntax)
        {
            PrettyPrintMode = true;
            this.Syntax = syntax;
        }

        /// <summary>
        /// Gets/Sets whether pretty print mode is enabled
        /// </summary>
        public bool PrettyPrintMode { get; set; }

        /// <summary>
        /// Gets the type of the Triple Formatter used by this writer
        /// </summary>
        public Type TripleFormatterType
        {
            get { return typeof (NQuadsFormatter); }
        }

        /// <summary>
        /// Gets/Sets the NQuads syntax mode
        /// </summary>
        public NQuadsSyntax Syntax { get; set; }

        /// <summary>
        /// Saves a graph in NQuads format
        /// </summary>
        /// <param name="g">Graph to save</param>
        /// <param name="writer">Writer to write to</param>
        public void Save(IGraph g, TextWriter writer)
        {
            if (g == null) throw new RdfOutputException("Cannot output a null Graph");
            if (writer == null) throw new RdfOutputException("Cannot output to a null writer");

            try
            {
                NQuadsFormatter formatter = new NQuadsFormatter();
                foreach (Triple t in g.Triples)
                {
                    writer.WriteLine(formatter.Format(t));
                }
                writer.Close();
            }
            finally
            {
                writer.CloseQuietly();
            }
        }

        /// <summary>
        /// Saves a graph store in NQuads format
        /// </summary>
        /// <param name="store">Graph store to save</param>
        /// <param name="writer">Writer to write to</param>
        public void Save(IGraphStore store, TextWriter writer)
        {
            if (store == null) throw new RdfOutputException("Cannot output a null Graph Store");
            if (writer == null) throw new RdfOutputException("Cannot output to a null writer");

            try
            {
                NQuadsFormatter formatter = new NQuadsFormatter();
                foreach (Quad q in store.Quads)
                {
                    writer.WriteLine(formatter.Format(q));
                }
                writer.Close();
            }
            finally
            {
                writer.CloseQuietly();
            }
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
            return "NQuads";
        }
    }
}