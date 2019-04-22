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
    /// Class for generating Turtle Concrete RDF Syntax which provides varying levels of Syntax Compression
    /// </summary>
    /// <remarks>
    /// Similar in speed to the standard <see cref="TurtleWriter">TurtleWriter</see> but capable of using more syntax compressions depending on the Compression level set
    /// </remarks>
    /// <threadsafety instance="true">Designed to be Thread Safe - should be able to call the Save() method from multiple threads on different Graphs without issue</threadsafety>
    public class CompressingTurtleWriter 
        : BaseRdfWriter, IPrettyPrintingWriter, IHighSpeedWriter, ICompressingWriter, INamespaceWriter, IFormatterBasedWriter
    {
        private bool _prettyprint = true;
        private bool _allowHiSpeed = true;
        private int _compressionLevel = Options.DefaultCompressionLevel;
        private INamespaceMapper _defaultNamespaces = new NamespaceMapper();
        private TurtleSyntax _syntax = TurtleSyntax.Original;

        /// <summary>
        /// Creates a new Compressing Turtle Writer which uses the Default Compression Level
        /// </summary>
        public CompressingTurtleWriter()
        {

        }

        /// <summary>
        /// Creates a new Compressing Turtle Writer which uses the given Compression Level
        /// </summary>
        /// <param name="compressionLevel">Desired Compression Level</param>
        /// <remarks>See Remarks for this classes <see cref="CompressingTurtleWriter.CompressionLevel">CompressionLevel</see> property to see what effect different compression levels have</remarks>
        public CompressingTurtleWriter(int compressionLevel)
        {
            _compressionLevel = compressionLevel;
        }

        /// <summary>
        /// Creates a new compressing Turtle writer using the given syntax level
        /// </summary>
        /// <param name="syntax">Syntax Level</param>
        public CompressingTurtleWriter(TurtleSyntax syntax)
        {
            _syntax = syntax;
        }

        /// <summary>
        /// Creates a new Compressing Turtle Writer which uses the given Compression Level and Syntax Level
        /// </summary>
        /// <param name="compressionLevel">Desired Compression Level</param>
        /// <param name="syntax">Syntax Level</param>
        /// <remarks>See Remarks for this classes <see cref="CompressingTurtleWriter.CompressionLevel">CompressionLevel</see> property to see what effect different compression levels have</remarks>
        public CompressingTurtleWriter(int compressionLevel, TurtleSyntax syntax)
            : this(compressionLevel)
        {
            _syntax = syntax;
        }

        /// <summary>
        /// Gets/Sets whether Pretty Printing is used
        /// </summary>
        public bool PrettyPrintMode
        {
            get
            {
                return _prettyprint;
            }
            set
            {
                _prettyprint = value;
            }
        }

        /// <summary>
        /// Gets/Sets whether High Speed Write Mode should be allowed
        /// </summary>
        public bool HighSpeedModePermitted
        {
            get
            {
                return _allowHiSpeed;
            }
            set
            {
                _allowHiSpeed = value;
            }
        }

        /// <summary>
        /// Gets/Sets the Compression Level to be used
        /// </summary>
        /// <remarks>
        /// <para>
        /// If the Compression Level is set to <see cref="WriterCompressionLevel.None">None</see> then High Speed mode will always be used regardless of the input Graph and the <see cref="CompressingTurtleWriter.HighSpeedModePermitted">HighSpeedMorePermitted</see> property.
        /// </para>
        /// <para>
        /// If the Compression Level is set to <see cref="WriterCompressionLevel.Minimal">Minimal</see> or above then full Predicate Object lists will be used for Triples.
        /// </para>
        /// <para>
        /// If the Compression Level is set to <see cref="WriterCompressionLevel.More">More</see> or above then Blank Node Collections and Collection syntax will be used if the Graph contains Triples that can be compressed in that way.</para>
        /// </remarks>
        public int CompressionLevel
        {
            get
            {
                return _compressionLevel;
            }
            set
            {
                _compressionLevel = value;
            }
        }

        /// <summary>
        /// Gets/Sets the Default Namespaces that are always available
        /// </summary>
        public INamespaceMapper DefaultNamespaces
        {
            get
            {
                return _defaultNamespaces;
            }
            set
            {
                _defaultNamespaces = value;
            }
        }

        /// <summary>
        /// Gets the type of the Triple Formatter used by the writer
        /// </summary>
        public Type TripleFormatterType
        {
            get
            {
                return (_syntax == TurtleSyntax.Original ? typeof(TurtleFormatter) : typeof(TurtleW3CFormatter));
            }
        }

        /// <summary>
        /// Saves a Graph to a file using Turtle Syntax
        /// </summary>
        /// <param name="g">Graph to save</param>
        /// <param name="filename">File to save to</param>
        public override void Save(IGraph g, string filename)
        {
            using (var stream = File.Open(filename, FileMode.Create))
            {
                Save(g, new StreamWriter(stream, new UTF8Encoding(Options.UseBomForUtf8)));
            }
        }

        /// <summary>
        /// Saves a Graph to the given Stream using Turtle Syntax
        /// </summary>
        /// <param name="g">Graph to save</param>
        /// <param name="output">Stream to save to</param>
        protected override void SaveInternal(IGraph g, TextWriter output)
        {
            // Create the Writing Context
            g.NamespaceMap.Import(_defaultNamespaces);
            CompressingTurtleWriterContext context = new CompressingTurtleWriterContext(g, output, _compressionLevel, _prettyprint, _allowHiSpeed, _syntax);
            GenerateOutput(context);
        }

        /// <summary>
        /// Generates the Turtle Syntax for the Graph
        /// </summary>
        private void GenerateOutput(CompressingTurtleWriterContext context)
        {
            // Create the Header
            // Base Directive
            if (context.Graph.BaseUri != null)
            {
                context.Output.WriteLine("@base <" + context.UriFormatter.FormatUri(context.Graph.BaseUri) + ">.");
                context.Output.WriteLine();
            }
            // Prefix Directives
            foreach (String prefix in context.Graph.NamespaceMap.Prefixes)
            {
                if (TurtleSpecsHelper.IsValidQName(prefix + ":"))
                {
                    if (!prefix.Equals(String.Empty))
                    {
                        context.Output.WriteLine("@prefix " + prefix + ": <" + context.UriFormatter.FormatUri(context.Graph.NamespaceMap.GetNamespaceUri(prefix)) + ">.");
                    }
                    else
                    {
                        context.Output.WriteLine("@prefix : <" + context.UriFormatter.FormatUri(context.Graph.NamespaceMap.GetNamespaceUri(String.Empty)) + ">.");
                    }
                }
            }
            context.Output.WriteLine();

            // Decide on the Write Mode to use
            bool hiSpeed = false;
            double subjNodes = context.Graph.Triples.SubjectNodes.Count();
            double triples = context.Graph.Triples.Count;
            if ((subjNodes / triples) > 0.75) hiSpeed = true;

            if (context.CompressionLevel == WriterCompressionLevel.None || (hiSpeed && context.HighSpeedModePermitted))
            {
                RaiseWarning("High Speed Write Mode in use - minimal syntax compression will be used");
                context.CompressionLevel = WriterCompressionLevel.Minimal;
                context.NodeFormatter = new UncompressedTurtleFormatter();

                foreach (Triple t in context.Graph.Triples)
                {
                    context.Output.WriteLine(GenerateTripleOutput(context, t));
                }
            }
            else
            {
                if (context.CompressionLevel >= WriterCompressionLevel.More)
                {
                    WriterHelper.FindCollections(context);
                }

                // Get the Triples as a Sorted List
                var ts = context.Graph.Triples.Where(t => !context.TriplesDone.Contains(t)).ToList();
                WriterHelper.SortTriplesBySubjectPredicate(ts);

                // Variables we need to track our writing
                INode lastSubj, lastPred;
                lastSubj = lastPred = null;
                int subjIndent = 0, predIndent = 0;
                String temp;

                for (int i = 0; i < ts.Count; i++)
                {
                    Triple t = ts[i];

                    if (lastSubj == null || !t.Subject.Equals(lastSubj))
                    {
                        // Terminate previous Triples
                        if (lastSubj != null) context.Output.WriteLine(".");

                        // Start a new set of Triples
                        temp = GenerateNodeOutput(context, t.Subject, TripleSegment.Subject, 0);
                        context.Output.Write(temp);
                        context.Output.Write(" ");
                        if (temp.Contains('\n'))
                        {
                            subjIndent = temp.Split('\n').Last().Length + 1;
                        }
                        else
                        {
                            subjIndent = temp.Length + 1;
                        }
                        lastSubj = t.Subject;

                        // Write the first Predicate
                        temp = GenerateNodeOutput(context, t.Predicate, TripleSegment.Predicate, subjIndent);
                        context.Output.Write(temp);
                        context.Output.Write(" ");
                        predIndent = temp.Length + 1;
                        lastPred = t.Predicate;
                    }
                    else if (lastPred == null || !t.Predicate.Equals(lastPred))
                    {
                        // Terminate previous Predicate Object list
                        context.Output.WriteLine(";");

                        if (context.PrettyPrint) context.Output.Write(new String(' ', subjIndent));

                        // Write the next Predicate
                        temp = GenerateNodeOutput(context, t.Predicate, TripleSegment.Predicate, subjIndent);
                        context.Output.Write(temp);
                        context.Output.Write(" ");
                        predIndent = temp.Length + 1;
                        lastPred = t.Predicate;
                    }
                    else
                    {
                        // Continue Object List
                        context.Output.WriteLine(",");

                        if (context.PrettyPrint) context.Output.Write(new String(' ', subjIndent + predIndent));
                    }

                    // Write the Object
                    context.Output.Write(GenerateNodeOutput(context, t.Object, TripleSegment.Object, subjIndent + predIndent));
                }

                // Terminate Triples
                if (ts.Count > 0) context.Output.WriteLine(".");

                return;
            }
            
        }

        /// <summary>
        /// Generates Output for Triples as a single "s p o." Triple
        /// </summary>
        /// <param name="context">Writer Context</param>
        /// <param name="t">Triple to output</param>
        /// <returns></returns>
        /// <remarks>Used only in High Speed Write Mode</remarks>
        private String GenerateTripleOutput(CompressingTurtleWriterContext context, Triple t)
        {
            StringBuilder temp = new StringBuilder();
            temp.Append(GenerateNodeOutput(context, t.Subject, TripleSegment.Subject, 0));
            temp.Append(' ');
            temp.Append(GenerateNodeOutput(context, t.Predicate, TripleSegment.Predicate, 0));
            temp.Append(' ');
            temp.Append(GenerateNodeOutput(context, t.Object, TripleSegment.Object, 0));
            temp.Append('.');

            return temp.ToString();
        }

        /// <summary>
        /// Generates Output for Nodes in Turtle syntax
        /// </summary>
        /// <param name="context">Writer Context</param>
        /// <param name="n">Node to generate output for</param>
        /// <param name="segment">Segment of the Triple being written</param>
        /// <param name="indent">Indentation</param>
        /// <returns></returns>
        private String GenerateNodeOutput(CompressingTurtleWriterContext context, INode n, TripleSegment segment, int indent)
        {
            StringBuilder output = new StringBuilder();

            switch (n.NodeType)
            {
                case NodeType.Blank:
                    if (segment == TripleSegment.Predicate) throw new RdfOutputException(WriterErrorMessages.BlankPredicatesUnserializable("Turtle"));

                    if (context.Collections.ContainsKey(n))
                    {
                        output.Append(GenerateCollectionOutput(context, context.Collections[n], indent));
                    }
                    else
                    {
                        return context.NodeFormatter.Format(n, segment);
                    }
                    break;

                case NodeType.GraphLiteral:
                    throw new RdfOutputException(WriterErrorMessages.GraphLiteralsUnserializable("Turtle"));

                case NodeType.Literal:
                    if (segment == TripleSegment.Subject) throw new RdfOutputException(WriterErrorMessages.LiteralSubjectsUnserializable("Turtle"));
                    if (segment == TripleSegment.Predicate) throw new RdfOutputException(WriterErrorMessages.LiteralPredicatesUnserializable("Turtle"));
                    return context.NodeFormatter.Format(n, segment);

                case NodeType.Uri:
                    return context.NodeFormatter.Format(n, segment);

                default:
                    throw new RdfOutputException(WriterErrorMessages.UnknownNodeTypeUnserializable("Turtle"));
            }

            return output.ToString();
        }

        /// <summary>
        /// Internal Helper method which converts a Collection into Turtle Syntax
        /// </summary>
        /// <param name="context">Writer Context</param>
        /// <param name="c">Collection to convert</param>
        /// <param name="indent">Indentation</param>
        /// <returns></returns>
        private String GenerateCollectionOutput(CompressingTurtleWriterContext context, OutputRdfCollection c, int indent)
        {
            StringBuilder output = new StringBuilder();
            bool first = true;

            if (!c.IsExplicit)
            {
                output.Append('(');

                while (c.Triples.Count > 0)
                {
                    if (context.PrettyPrint && !first) output.Append(new String(' ', indent));
                    first = false;
                    output.Append(GenerateNodeOutput(context, c.Triples.First().Object, TripleSegment.Object, indent));
                    c.Triples.RemoveAt(0);
                    if (c.Triples.Count > 0)
                    {
                        output.Append(' ');
                    }
                }

                output.Append(')');
            }
            else
            {
                if (c.Triples.Count == 0)
                {
                    // Empty Collection
                    // Can represent as a single Blank Node []
                    output.Append("[]");
                }
                else
                {
                    output.Append('[');

                    while (c.Triples.Count > 0)
                    {
                        if (context.PrettyPrint && !first) output.Append(new String(' ', indent));
                        first = false;
                        String temp = GenerateNodeOutput(context, c.Triples.First().Predicate, TripleSegment.Predicate, indent);
                        output.Append(temp);
                        output.Append(' ');
                        int addIndent;
                        if (temp.Contains('\n'))
                        {
                            addIndent = temp.Split('\n').Last().Length;
                        }
                        else
                        {
                            addIndent = temp.Length;
                        }
                        output.Append(GenerateNodeOutput(context, c.Triples.First().Object, TripleSegment.Object, indent + 2 + addIndent));
                        c.Triples.RemoveAt(0);

                        if (c.Triples.Count > 0)
                        {
                            output.AppendLine(" ; ");
                            output.Append(' ');
                        }
                    }

                    output.Append(']');
                }
            }
            return output.ToString();
        }

        /// <summary>
        /// Helper method for generating Parser Warning Events
        /// </summary>
        /// <param name="message">Warning Message</param>
        private void RaiseWarning(String message)
        {
            if (Warning != null)
            {
                Warning(message);
            }
        }

        /// <summary>
        /// Event which is raised when there is a non-fatal issue with the Graph being written
        /// </summary>
        public override event RdfWriterWarning Warning;

        /// <summary>
        /// Gets the String representation of the writer which is a description of the syntax it produces
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "Turtle (Compressing Writer)" + (_syntax == TurtleSyntax.Original ? "" : " (W3C)");
        }
    }
}
