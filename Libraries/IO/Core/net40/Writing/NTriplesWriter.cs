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
using VDS.RDF.Graphs;
using VDS.RDF.Nodes;
using VDS.RDF.Parsing;
using VDS.RDF.Writing.Contexts;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Writing
{
    /// <summary>
    /// Class for generating RDF in NTriples Concrete Syntax
    /// </summary>
    /// <threadsafety instance="true">Designed to be Thread Safe - should be able to call the Save() method from multiple threads on different Graphs without issue</threadsafety>
    public class NTriplesWriter 
        : BaseGraphWriter, IFormatterBasedWriter
    {
        private bool _sort = false;

        /// <summary>
        /// Creates a new writer
        /// </summary>
        /// <param name="syntax">NTriples Syntax Mode</param>
        public NTriplesWriter(NTriplesSyntax syntax)
        {
            this.Syntax = syntax;
        }

        /// <summary>
        /// Creates a new writer
        /// </summary>
        public NTriplesWriter()
            : this(NTriplesSyntax.Original) { }

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
        /// Gets/Sets the NTriples syntax mode
        /// </summary>
        public NTriplesSyntax Syntax { get; set; }

        /// <summary>
        /// Saves the Graph in NTriples Syntax to the given stream
        /// </summary>
        /// <param name="g">Graph to save</param>
        /// <param name="output">Stream to save to</param>
        public override void Save(IGraph g, TextWriter output)
        {
            try
            {
                NTriplesWriterContext context = new NTriplesWriterContext(g, output, this.Syntax);
                List<Triple> ts = g.Triples.ToList();
                if (this._sort) ts.Sort(new FullTripleComparer(new FastNodeComparer()));

                foreach (Triple t in ts)
                {
                    output.WriteLine(this.TripleToNTriples(context, t));
                }

                output.Close();
            }
            finally
            {
                output.CloseQuietly();
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
            output.Append(context.NodeFormatter.Format(t.Subject, QuadSegment.Subject));
            output.Append(" ");
            output.Append(context.NodeFormatter.Format(t.Predicate, QuadSegment.Predicate));
            output.Append(" ");
            output.Append(context.NodeFormatter.Format(t.Object, QuadSegment.Object));
            output.Append(" .");

            return output.ToString();
        }

        /// <summary>
        /// Gets the String representation of the writer which is a description of the syntax it produces
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.Syntax == NTriplesSyntax.Original ? "NTriples" : "NTriples (RDF 1.1)";
        }
    }
}
