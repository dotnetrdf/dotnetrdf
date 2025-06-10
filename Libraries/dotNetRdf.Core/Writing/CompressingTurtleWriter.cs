/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2025 dotNetRDF Project (http://dotnetrdf.org/)
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

using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing;
using VDS.RDF.Writing.Contexts;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Writing;


/// <summary>
/// Class for generating Turtle Concrete RDF Syntax which provides varying levels of Syntax Compression.
/// </summary>
/// <remarks>
/// Similar in speed to the standard <see cref="TurtleWriter">TurtleWriter</see> but capable of using more syntax compressions depending on the Compression level set.
/// </remarks>
/// <threadsafety instance="true">Designed to be Thread Safe - should be able to call the Save() method from multiple threads on different Graphs without issue.</threadsafety>
public class CompressingTurtleWriter 
    : BaseRdfWriter, IPrettyPrintingWriter, IHighSpeedWriter, ICompressingWriter, INamespaceWriter, IFormatterBasedWriter
{
    private readonly TurtleSyntax _syntax = TurtleSyntax.Original;

    /// <summary>
    /// Creates a new Compressing Turtle Writer which uses the Default Compression Level.
    /// </summary>
    public CompressingTurtleWriter()
    {

    }

    /// <summary>
    /// Creates a new Compressing Turtle Writer which uses the given Compression Level.
    /// </summary>
    /// <param name="compressionLevel">Desired Compression Level.</param>
    /// <remarks>See Remarks for this classes <see cref="CompressingTurtleWriter.CompressionLevel">CompressionLevel</see> property to see what effect different compression levels have.</remarks>
    public CompressingTurtleWriter(int compressionLevel)
    {
        CompressionLevel = compressionLevel;
    }

    /// <summary>
    /// Creates a new compressing Turtle writer using the given syntax level.
    /// </summary>
    /// <param name="syntax">Syntax Level.</param>
    public CompressingTurtleWriter(TurtleSyntax syntax)
    {
        _syntax = syntax;
    }

    /// <summary>
    /// Creates a new Compressing Turtle Writer which uses the given Compression Level and Syntax Level.
    /// </summary>
    /// <param name="compressionLevel">Desired Compression Level.</param>
    /// <param name="syntax">Syntax Level.</param>
    /// <remarks>See Remarks for this classes <see cref="CompressingTurtleWriter.CompressionLevel">CompressionLevel</see> property to see what effect different compression levels have.</remarks>
    public CompressingTurtleWriter(int compressionLevel, TurtleSyntax syntax)
        : this(compressionLevel)
    {
        _syntax = syntax;
    }

    /// <summary>
    /// Gets/Sets whether Pretty Printing is used.
    /// </summary>
    public bool PrettyPrintMode { get; set; } = true;

    /// <summary>
    /// Gets/Sets whether High Speed Write Mode should be allowed.
    /// </summary>
    public bool HighSpeedModePermitted { get; set; } = true;

    /// <summary>
    /// Gets/Sets the Compression Level to be used.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If the Compression Level is set to <see cref="WriterCompressionLevel.None">None</see> then High Speed mode will always be used regardless of the input Graph and the <see cref="CompressingTurtleWriter.HighSpeedModePermitted">HighSpeedMorePermitted</see> property.
    /// </para>
    /// <para>
    /// If the Compression Level is set to <see cref="WriterCompressionLevel.Minimal">Minimal</see> or above then full Predicate Object lists will be used for Triples.
    /// </para>
    /// <para>
    /// If the Compression Level is set to <see cref="WriterCompressionLevel.More">More</see> or above then Blank Node Collections and Collection syntax will be used if the Graph contains Triples that can be compressed in that way;
    /// and if writing <see cref="TurtleSyntax.Rdf11Star"/> syntax, triple annotations syntax will be used if the graph contains asserted triples that are also quoted as the subject of one or more other triples.</para>
    /// </remarks>
#pragma warning disable CS0618 // Type or member is obsolete
    public int CompressionLevel { get; set; } = Options.DefaultCompressionLevel; // = WriterCompressionLevel.More;
#pragma warning restore CS0618 // Type or member is obsolete

    /// <summary>
    /// Gets/Sets the Default Namespaces that are always available.
    /// </summary>
    public INamespaceMapper DefaultNamespaces { get; set; } = new NamespaceMapper();

    /// <summary>
    /// Gets the type of the Triple Formatter used by the writer.
    /// </summary>
    public Type TripleFormatterType => (_syntax == TurtleSyntax.Original ? typeof(TurtleFormatter) : typeof(TurtleW3CFormatter));

    /// <summary>
    /// Saves a Graph to the given Stream using Turtle Syntax.
    /// </summary>
    /// <param name="g">Graph to save.</param>
    /// <param name="output">Stream to save to.</param>
    protected override void SaveInternal(IGraph g, TextWriter output)
    {
        // Create the Writing Context
        g.NamespaceMap.Import(DefaultNamespaces);
        var context = new CompressingTurtleWriterContext(g, output, CompressionLevel, PrettyPrintMode, HighSpeedModePermitted, _syntax);
        GenerateOutput(context);
    }

    /// <summary>
    /// Generates the Turtle Syntax for the Graph.
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
        foreach (var prefix in context.Graph.NamespaceMap.Prefixes)
        {
            if (TurtleSpecsHelper.IsValidQName(prefix + ":"))
            {
                if (!prefix.Equals(string.Empty))
                {
                    context.Output.WriteLine("@prefix " + prefix + ": <" + context.UriFormatter.FormatUri(context.Graph.NamespaceMap.GetNamespaceUri(prefix)) + ">.");
                }
                else
                {
                    context.Output.WriteLine("@prefix : <" + context.UriFormatter.FormatUri(context.Graph.NamespaceMap.GetNamespaceUri(string.Empty)) + ">.");
                }
            }
        }
        context.Output.WriteLine();

        // Decide on the Write Mode to use
        var hiSpeed = false;
        double subjNodes = context.Graph.Triples.SubjectNodes.Count();
        double triples = context.Graph.Triples.Count;
        if ((subjNodes / triples) > 0.75) hiSpeed = true;

        if (context.CompressionLevel == WriterCompressionLevel.None || (hiSpeed && context.HighSpeedModePermitted))
        {
            RaiseWarning("High Speed Write Mode in use - minimal syntax compression will be used");
            context.CompressionLevel = WriterCompressionLevel.Minimal;
            context.NodeFormatter = _syntax == TurtleSyntax.Rdf11Star ? new UncompressedTurtleStarFormatter() :  new UncompressedTurtleFormatter();

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
                if (_syntax == TurtleSyntax.Rdf11Star) WriterHelper.FindAnnotations(context);
            }

            // Get the Triples as a Sorted List
            var ts = context.Graph.Triples.Where(t => !context.TriplesDone.Contains(t)).ToList();
            WriterHelper.SortTriplesBySubjectPredicate(ts);

            // Variables we need to track our writing
            INode lastSubj, lastPred;
            lastSubj = lastPred = null;
            int subjIndent = 0, predIndent = 0;
            string temp;

            foreach (Triple t in ts)
            {
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

                    if (context.PrettyPrint) context.Output.Write(new string(' ', subjIndent));

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

                    if (context.PrettyPrint) context.Output.Write(new string(' ', subjIndent + predIndent));
                }

                // Write the Object
                temp = GenerateNodeOutput(context, t.Object, TripleSegment.Object, subjIndent + predIndent);
                context.Output.Write(temp);

                // Write any annotations on the object
                if (context.Annotations.ContainsKey(t))
                {
                    context.Output.Write(GenerateAnnotationOutput(context, context.Annotations[t], subjIndent + predIndent + temp.Length + 1));
                }
            }

            // Terminate Triples
            if (ts.Count > 0) context.Output.WriteLine(".");
        }
        
    }

    /// <summary>
    /// Generates Output for Triples as a single "s p o." Triple.
    /// </summary>
    /// <param name="context">Writer Context.</param>
    /// <param name="t">Triple to output.</param>
    /// <returns></returns>
    /// <remarks>Used only in High Speed Write Mode.</remarks>
    private string GenerateTripleOutput(CompressingTurtleWriterContext context, Triple t)
    {
        var temp = new StringBuilder();
        temp.Append(GenerateNodeOutput(context, t.Subject, TripleSegment.Subject, 0));
        temp.Append(' ');
        temp.Append(GenerateNodeOutput(context, t.Predicate, TripleSegment.Predicate, 0));
        temp.Append(' ');
        temp.Append(GenerateNodeOutput(context, t.Object, TripleSegment.Object, 0));
        temp.Append('.');

        return temp.ToString();
    }

    /// <summary>
    /// Generates Output for Nodes in Turtle syntax.
    /// </summary>
    /// <param name="context">Writer Context.</param>
    /// <param name="n">Node to generate output for.</param>
    /// <param name="segment">Segment of the Triple being written.</param>
    /// <param name="indent">Indentation.</param>
    /// <returns></returns>
    private string GenerateNodeOutput(CompressingTurtleWriterContext context, INode n, TripleSegment segment, int indent)
    {
        var output = new StringBuilder();

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

            case NodeType.Triple:
                if (_syntax != TurtleSyntax.Rdf11Star)
                {
                    throw new RdfOutputException(
                        WriterErrorMessages.TripleNodesUnserializable($"Turtle/{_syntax}"));
                }
                if (segment == TripleSegment.Predicate)
                {
                    throw new RdfOutputException(WriterErrorMessages.TripleNodePredicateUnserializable("Turtle"));
                }

                return context.NodeFormatter.Format(n, segment);

            default:
                throw new RdfOutputException(WriterErrorMessages.UnknownNodeTypeUnserializable("Turtle"));
        }

        return output.ToString();
    }

    /// <summary>
    /// Internal Helper method which converts a Collection into Turtle Syntax.
    /// </summary>
    /// <param name="context">Writer Context.</param>
    /// <param name="c">Collection to convert.</param>
    /// <param name="indent">Indentation.</param>
    /// <returns></returns>
    private string GenerateCollectionOutput(CompressingTurtleWriterContext context, OutputRdfCollection c, int indent)
    {
        var output = new StringBuilder();
        var first = true;

        if (!c.IsExplicit)
        {
            output.Append('(');

            while (c.Triples.Count > 0)
            {
                if (context.PrettyPrint && !first) output.Append(new string(' ', indent));
                first = false;
                output.Append(GenerateNodeOutput(context, c.Triples.First().Object, TripleSegment.Object, indent));
                c.Triples.RemoveAt(0);
                if (c.Triples.Count > 0)
                {
                    if (context.PrettyPrint) output.AppendLine("");
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
                    if (context.PrettyPrint && !first) output.Append(new string(' ', indent));
                    first = false;
                    var temp = GenerateNodeOutput(context, c.Triples.First().Predicate, TripleSegment.Predicate, indent);
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

    private string GenerateAnnotationOutput(CompressingTurtleWriterContext context, List<Triple> annotationTriples,
        int indent)
    {
        var output = new StringBuilder();
        string temp;
        output.Append(" {| ");
        WriterHelper.SortTriplesBySubjectPredicate(annotationTriples);
        INode lastPred = null;
        indent += 3;
        int predIndent = 0;
        foreach (Triple t in annotationTriples)
        {
            if (lastPred == null || !lastPred.Equals(t.Predicate))
            {
                if (lastPred != null)
                {
                    // New line for the next predicate
                    output.AppendLine(";");
                    if (context.PrettyPrint) output.Append(' ', indent);
                }

                temp = GenerateNodeOutput(context, t.Predicate, TripleSegment.Predicate, indent);
                predIndent = temp.Length + 1;
                lastPred = t.Predicate;
                output.Append(temp);
                output.Append(' ');
            }
            else
            {
                output.AppendLine(",");
                if (context.PrettyPrint) output.Append(' ', indent + predIndent);
            }

            // Write the Object
            temp = GenerateNodeOutput(context, t.Object, TripleSegment.Object, indent + predIndent);
            output.Append(temp);

            // Write any annotations on the object
            if (context.Annotations.ContainsKey(t))
            {
                output.Append(GenerateAnnotationOutput(context, context.Annotations[t],
                    indent + predIndent + temp.Length + 1));
            }
        }

        output.Append(" |}");
        return output.ToString();
    }

    /// <summary>
    /// Helper method for generating Parser Warning Events.
    /// </summary>
    /// <param name="message">Warning Message.</param>
    private void RaiseWarning(string message)
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
    /// Gets the String representation of the writer which is a description of the syntax it produces.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return "Turtle (Compressing Writer)" + _syntax switch
        {
            TurtleSyntax.W3C => " (W3C)", TurtleSyntax.Rdf11Star => " (RDF-Star)", _ => ""
        };
    }
}
