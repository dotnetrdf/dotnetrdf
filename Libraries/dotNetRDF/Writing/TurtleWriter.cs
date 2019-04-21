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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing;
using VDS.RDF.Writing.Contexts;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Writing
{
    /// <summary>
    /// Class for generating RDF in Turtle Syntax
    /// </summary>
    /// <remarks>
    /// Similar in speed to the <see cref="CompressingTurtleWriter">CompressingTurtleWriter</see> but doesn't use the full Blank Node and Collection syntax compressions
    /// </remarks>
    /// <threadsafety instance="true">Designed to be Thread Safe - should be able to call the Save() method from multiple threads on different Graphs without issue</threadsafety>
    [Obsolete("Deprecated in favour of the CompressingTurtleWriter which uses a much fuller range of syntax compressions", false)]
    public class TurtleWriter
        : BaseRdfWriter, IPrettyPrintingWriter, IHighSpeedWriter, IFormatterBasedWriter
    {
        private bool _prettyprint = true;
        private bool _allowhispeed = false;
        private TurtleSyntax _syntax = TurtleSyntax.Original;

        /// <summary>
        /// Creates a new Turtle Writer
        /// </summary>
        public TurtleWriter() { }

        /// <summary>
        /// Creates a new Turtle Writer
        /// </summary>
        /// <param name="syntax">Turtle Syntax</param>
        public TurtleWriter(TurtleSyntax syntax)
        {
            _syntax = syntax;
        }

        /// <summary>
        /// Gets/Sets whether Pretty Printing is used
        /// </summary>
        public bool PrettyPrintMode
        {
            get => _prettyprint;
            set => _prettyprint = value;
        }

        /// <summary>
        /// Gets/Sets whether the Writer is allowed to use High Speed write mode
        /// </summary>
        /// <remarks>High Speed Write Mode is engaged when the Writer determines that the contents of the Graph are not well suited to Turtle syntax compressions.  Usually the writer compresses triples into groups by Subject using Predicate-Object lists to output the Triples relating to each Subject.  If the number of distinct Subjects is greater than 75% of the Triples in the Graph then High Speed write mode will be used, in High Speed mode all Triples are written fully and no grouping of any sort is done.</remarks>
        public bool HighSpeedModePermitted
        {
            get => _allowhispeed;
            set => _allowhispeed = value;
        }

        /// <summary>
        /// Gets the type of the Triple Formatter used by this writer
        /// </summary>
        public Type TripleFormatterType => (_syntax == TurtleSyntax.Original ? typeof(TurtleFormatter) : typeof(TurtleW3CFormatter));

        /// <summary>
        /// Saves a Graph to a File
        /// </summary>
        /// <param name="g">Graph to save</param>
        /// <param name="filename">Filename to save to</param>
        public override void Save(IGraph g, string filename)
        {
            using (var stream = File.Open(filename, FileMode.Create))
            {
                Save(g, new StreamWriter(stream, new UTF8Encoding(Options.UseBomForUtf8)));
            }
        }

        /// <summary>
        /// Saves a Graph using an arbitrary <see cref="TextWriter">TextWriter</see>
        /// </summary>
        /// <param name="g">Graph to save</param>
        /// <param name="output">Writer to save using</param>
        protected override void SaveInternal(IGraph g, TextWriter output)
        {
            TurtleWriterContext context = new TurtleWriterContext(g, output, _prettyprint, _allowhispeed, _syntax);
            GenerateOutput(context);
        }

        /// <summary>
        /// Generates the Output for a Graph
        /// </summary>
        /// <param name="context">Context for writing the Graph</param>
        private void GenerateOutput(TurtleWriterContext context)
        {
            // Write Base Uri
            if (context.Graph.BaseUri != null)
            {
                context.Output.WriteLine("@base <" + context.UriFormatter.FormatUri(context.Graph.BaseUri) + ">.");
                context.Output.WriteLine();
            }

            // Write Prefixes
            foreach (string prefix in context.Graph.NamespaceMap.Prefixes)
            {
                if (TurtleSpecsHelper.IsValidQName(prefix + ":", _syntax))
                {
                    context.Output.Write("@prefix " + prefix + ": <");
                    string nsUri = context.UriFormatter.FormatUri(context.Graph.NamespaceMap.GetNamespaceUri(prefix));
                    context.Output.WriteLine(nsUri + ">.");
                }
            }
            context.Output.WriteLine();

            // Decide which write mode to use
            bool hiSpeed = false;
            double subjNodes = context.Graph.Triples.SubjectNodes.Count();
            double triples = context.Graph.Triples.Count;
            if ((subjNodes / triples) > 0.75)
            {
                hiSpeed = true;
            }

            if (hiSpeed && context.HighSpeedModePermitted)
            {
                // High Speed Writing Mode
                // Writes everything as individual Triples
                RaiseWarning("High Speed Write Mode in use - minimal syntax compressions will be used");
                context.NodeFormatter = new UncompressedTurtleFormatter();
                foreach (Triple t in context.Graph.Triples)
                {
                    context.Output.Write(GenerateNodeOutput(context, t.Subject, TripleSegment.Subject));
                    context.Output.Write(" ");
                    context.Output.Write(GenerateNodeOutput(context, t.Predicate, TripleSegment.Predicate));
                    context.Output.Write(" ");
                    context.Output.Write(GenerateNodeOutput(context, t.Object, TripleSegment.Object));
                    context.Output.WriteLine(".");
                }
            }
            else
            {

                // Get the Triples as a Sorted List
                var ts = WriterHelper.GetTriplesSortedBySubjectPredicate(context.Graph);

                // Variables we need to track our writing
                INode lastSubj, lastPred;
                lastSubj = lastPred = null;
                int subjIndent = 0, predIndent = 0;
                string temp;

                for (int i = 0; i < ts.Count; i++)
                {
                    Triple t = ts[i];
                    if (lastSubj == null || !t.Subject.Equals(lastSubj))
                    {
                        // Terminate previous Triples
                        if (lastSubj != null)
                        {
                            context.Output.WriteLine(".");
                        }

                        // Start a new set of Triples
                        temp = GenerateNodeOutput(context, t.Subject, TripleSegment.Subject);
                        context.Output.Write(temp);
                        context.Output.Write(" ");
                        subjIndent = temp.Length + 1;
                        lastSubj = t.Subject;

                        // Write the first Predicate
                        temp = GenerateNodeOutput(context, t.Predicate, TripleSegment.Predicate);
                        context.Output.Write(temp);
                        context.Output.Write(" ");
                        predIndent = temp.Length + 1;
                        lastPred = t.Predicate;
                    }
                    else if (lastPred == null || !t.Predicate.Equals(lastPred))
                    {
                        // Terminate previous Predicate Object list
                        context.Output.WriteLine(";");

                        if (context.PrettyPrint)
                        {
                            context.Output.Write(new string(' ', subjIndent));
                        }

                        // Write the next Predicate
                        temp = GenerateNodeOutput(context, t.Predicate, TripleSegment.Predicate);
                        context.Output.Write(temp);
                        context.Output.Write(" ");
                        predIndent = temp.Length + 1;
                        lastPred = t.Predicate;
                    }
                    else
                    {
                        // Continue Object List
                        context.Output.WriteLine(",");

                        if (context.PrettyPrint)
                        {
                            context.Output.Write(new string(' ', subjIndent + predIndent));
                        }
                    }

                    // Write the Object
                    context.Output.Write(GenerateNodeOutput(context, t.Object, TripleSegment.Object));
                }

                // Terminate Triples
                if (ts.Count > 0)
                {
                    context.Output.WriteLine(".");
                }
            }
        }

        /// <summary>
        /// Generates the Output for a Node in Turtle Syntax
        /// </summary>
        /// <param name="context">Context for writing the Graph</param>
        /// <param name="n">Node to generate Output for</param>
        /// <param name="segment">Segment of the Triple being written</param>
        /// <returns></returns>
        private string GenerateNodeOutput(TurtleWriterContext context, INode n, TripleSegment segment)
        {
            switch (n.NodeType)
            {
                case NodeType.Blank:
                    if (segment == TripleSegment.Predicate)
                    {
                        throw new RdfOutputException(WriterErrorMessages.BlankPredicatesUnserializable("Turtle"));
                    }

                    return context.NodeFormatter.Format(n, segment);

                case NodeType.GraphLiteral:
                    throw new RdfOutputException(WriterErrorMessages.GraphLiteralsUnserializable("Turtle"));

                case NodeType.Literal:
                    if (segment == TripleSegment.Subject)
                    {
                        throw new RdfOutputException(WriterErrorMessages.LiteralSubjectsUnserializable("Turtle"));
                    }

                    if (segment == TripleSegment.Predicate)
                    {
                        throw new RdfOutputException(WriterErrorMessages.LiteralPredicatesUnserializable("Turtle"));
                    }

                    return context.NodeFormatter.Format(n, segment);

                case NodeType.Uri:
                    return context.NodeFormatter.Format(n, segment);

                default:
                    throw new RdfOutputException(WriterErrorMessages.UnknownNodeTypeUnserializable("Turtle"));
            }
        }

        /// <summary>
        /// Helper method for raising the <see cref="Warning">Warning</see> event
        /// </summary>
        /// <param name="message">Warning Message</param>
        private void RaiseWarning(string message)
        {
            RdfWriterWarning d = Warning;
            if (d != null)
            {
                d(message);
            }
        }

        /// <summary>
        /// Event which is raised when a non-fatal issue with the Graph being serialized is encountered
        /// </summary>
        public override event RdfWriterWarning Warning;

        /// <summary>
        /// Gets the String representation of the writer which is a description of the syntax it produces
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "Turtle" + (_syntax == TurtleSyntax.Original ? "" : " (W3C)");
        }
    }
}
