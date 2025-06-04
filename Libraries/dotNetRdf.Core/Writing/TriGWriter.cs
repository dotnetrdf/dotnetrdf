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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VDS.RDF.Parsing;
using VDS.RDF.Storage;
using VDS.RDF.Writing.Contexts;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Writing;

/// <summary>
/// Class for writing a Triple Store in named Graph TriG syntax to a file/stream.
/// </summary>
/// <remarks>
/// <para>
/// For efficiency the TriG Writer splits it's writing over several threads (currently 4), these threads share a reference to a Context object which gives Global writing context eg. the target <see cref="TextWriter">TextWriter</see> being written to.  Each thread generates temporary local writing contexts as it goes along, each of these is scoped to writing a specific Graph.  Graphs are written to a <see cref="StringWriter">StringWriter</see> so the output for each Graph is built completely and then written in one go to the <see cref="TextWriter">TextWriter</see> specified as the target of the writing in the global context.
/// </para>
/// </remarks>
/// <threadsafety instance="true">Designed to be Thread Safe - should be able to call <see cref="TriGWriter.Save(ITripleStore,TextWriter, bool)">Save()</see> from several threads with no issue.  See Remarks for potential performance impact of this.</threadsafety>
public class TriGWriter 
    : BaseStoreWriter, IHighSpeedWriter, IPrettyPrintingWriter, ICompressingWriter, IMultiThreadedWriter
{
    private int _threads = 4;
    private const int PollInterval = 50;

    /// <summary>
    /// Gets/Sets whether High Speed Write Mode is permitted.
    /// </summary>
    public bool HighSpeedModePermitted { get; set; } = true;

    /// <summary>
    /// Gets/Sets whether Pretty Printing is used.
    /// </summary>
    public bool PrettyPrintMode { get; set; } = true;

    /// <summary>
    /// Gets/Sets the Compression Level for the writer.
    /// </summary>
#pragma warning disable CS0618 // Type or member is obsolete
    public int CompressionLevel { get; set; } = Options.DefaultCompressionLevel; //= WriterCompressionLevel.Default;
#pragma warning restore CS0618 // Type or member is obsolete

    /// <summary>
    /// Gets/Sets whether N3 Compatibility Mode is used, in this mode an = is written after Graph Names so an N3 parser can read the TriG file correctly.
    /// </summary>
    /// <remarks>
    /// Defaults to <strong>false</strong> from the 0.4.1 release onwards.
    /// </remarks>
    public bool N3CompatibilityMode { get; set; } = false;

    /// <summary>
    /// Gets/Sets whether multi-threaded writing will be used to generate output faster.
    /// </summary>
#pragma warning disable CS0618 // Type or member is obsolete
    public bool UseMultiThreadedWriting { get; set; } = Options.AllowMultiThreadedWriting; // = false;
#pragma warning restore CS0618 // Type or member is obsolete

    /// <summary>
    /// Get/Sets the syntax mode for the writer.
    /// </summary>
    public TriGSyntax Syntax { get; set; } = TriGSyntax.Rdf11Star;

    /// <summary>
    /// Saves a Store in TriG (Turtle with Named Graphs) format.
    /// </summary>
    /// <param name="store">Store to save.</param>
    /// <param name="writer">Writer to save to.</param>
    /// <param name="leaveOpen">Boolean flag indicating if <paramref name="writer"/> should be left open after the store is saved.</param>
    public override void Save(ITripleStore store, TextWriter writer, bool leaveOpen)
    {
        if (store == null) throw new RdfOutputException("Cannot output a null Triple Store");
        if (writer == null) throw new RdfOutputException("Cannot output to a null writer");

        var context = new TriGWriterContext(store, writer, PrettyPrintMode, HighSpeedModePermitted, CompressionLevel, N3CompatibilityMode);

        // Check there's something to do
        if (context.Store.Graphs.Count == 0)
        {
            if (!leaveOpen)
            {
                context.Output.Close();
            }
            return;
        }

        // Write the Header of the File
        foreach (IGraph g in context.Store.Graphs)
        {
            context.NamespaceMap.Import(g.NamespaceMap);
        }
        if (context.CompressionLevel > WriterCompressionLevel.None)
        {
            // Only add @prefix declarations if compression is enabled
            context.QNameMapper = new ThreadSafeQNameOutputMapper(context.NamespaceMap);
            foreach (var prefix in context.NamespaceMap.Prefixes)
            {
                if (TurtleSpecsHelper.IsValidQName(prefix + ":"))
                {
                    context.Output.WriteLine("@prefix " + prefix + ": <" + context.FormatUri(context.NamespaceMap.GetNamespaceUri(prefix)) + ">.");
                }
            }
            context.Output.WriteLine();
        }
        else
        {
            context.QNameMapper = new ThreadSafeQNameOutputMapper(new NamespaceMapper(true));
        }

        if (UseMultiThreadedWriting)
        {
            // Standard Multi-Threaded Writing

            // Queue the Graphs to be written
            foreach (IGraph g in context.Store.Graphs)
            {
                context.Add(g.Name);
            }

            // Start making the async calls
            var workers = new Task[_threads];
            for (var i = 0; i < _threads; i++)
            {
                workers[i] = Task.Factory.StartNew(() => SaveGraphs(context));
            }

            try
            {
                Task.WaitAll(workers);
            }
            catch (AggregateException ex)
            {
                var outputException =
                    new RdfThreadedOutputException(WriterErrorMessages.ThreadedOutputFailure("TriG"));
                foreach (Exception innerException in ex.InnerExceptions)
                {
                    outputException.AddException(innerException);
                }
            }
            finally
            {
                // Make sure to close the output
                if (!leaveOpen)
                {
                    context.Output.Close();
                }
            }
        }
        else
        {
            try
            {
                // Optional Single Threaded Writing
                foreach (IGraph g in store.Graphs)
                {
                    var graphContext = new CompressingTurtleWriterContext(g, new System.IO.StringWriter(),
                        context.PrettyPrint, context.HighSpeedModePermitted,
                        Syntax == TriGSyntax.Rdf11Star ? TurtleSyntax.Rdf11Star : TurtleSyntax.W3C)
                    {
                        NodeFormatter = GetNodeFormatter(context),
                    };
                    context.Output.WriteLine(GenerateGraphOutput(context, graphContext));
                }

                // Make sure to close the output
                if (!leaveOpen)
                {
                    context.Output.Close();
                }
            }
            catch
            {
                try
                {
                    // Close the output
                    if (!leaveOpen) context.Output.Close();
                }
                catch
                {
                    // No catch actions, just cleaning up the output stream
                }
                throw;
            }
        }
    }

    /// <summary>
    /// Generates the Output for a Graph as a String in TriG syntax.
    /// </summary>
    /// <param name="globalContext">Context for writing the Store.</param>
    /// <param name="context">Context for writing the Graph.</param>
    /// <returns></returns>
    private string GenerateGraphOutput(TriGWriterContext globalContext, CompressingTurtleWriterContext context)
    {
        if (context.Graph.Name != null)
        {
            // Named Graph
            var sep = (globalContext.N3CompatabilityMode) ? " = " : " ";
            context.Output.WriteLine(context.NodeFormatter.Format(context.Graph.Name) + sep + "{");
        }
        else
        {
            context.Output.WriteLine("{");
        }

        // Generate Triples
        GenerateTripleOutput(globalContext, context);

        // Close the Graph
        context.Output.WriteLine("}");

        return context.Output.ToString();
    }

    /// <summary>
    /// Generates the Output for a Triple as a String in Turtle syntax.
    /// </summary>
    /// <param name="globalContext">Context for writing the Store.</param>
    /// <param name="context">Context for writing the Graph.</param>
    private void GenerateTripleOutput(TriGWriterContext globalContext, CompressingTurtleWriterContext context)
    {
        // Decide which write mode to use
        var hiSpeed = false;
        double subjNodes = context.Graph.Triples.SubjectNodes.Count();
        double triples = context.Graph.Triples.Count;
        if ((subjNodes / triples) > 0.75) hiSpeed = true;

        if (globalContext.CompressionLevel == WriterCompressionLevel.None || hiSpeed && context.HighSpeedModePermitted)
        {
            // Use High Speed Write Mode
            var indentation = new string(' ', 4);
            context.Output.Write(indentation);
            if (globalContext.CompressionLevel > WriterCompressionLevel.None) context.Output.WriteLine("# Written using High Speed Mode");
            foreach (Triple t in context.Graph.Triples)
            {
                context.Output.Write(indentation);
                context.Output.Write(GenerateNodeOutput(context, t.Subject, TripleSegment.Subject));
                context.Output.Write(' ');
                context.Output.Write(GenerateNodeOutput(context, t.Predicate, TripleSegment.Predicate));
                context.Output.Write(' ');
                context.Output.Write(GenerateNodeOutput(context, t.Object, TripleSegment.Object));
                context.Output.WriteLine(".");
            }
        }
        else
        {
            if (context.CompressionLevel >= WriterCompressionLevel.More && Syntax == TriGSyntax.Rdf11Star)
            {
                WriterHelper.FindAnnotations(context);
            }

            // Get the Triples as a Sorted List
            var ts = context.Graph.Triples.Where(t=>!context.TriplesDone.Contains(t)).ToList();
            ts.Sort();

            // Variables we need to track our writing
            INode lastSubj, lastPred;
            lastSubj = lastPred = null;
            int subjIndent = 0, predIndent = 0;
            var baseIndent = 4;
            string temp;

            for (var i = 0; i < ts.Count; i++)
            {
                Triple t = ts[i];
                if (lastSubj == null || !t.Subject.Equals(lastSubj))
                {
                    // Terminate previous Triples
                    if (lastSubj != null) context.Output.WriteLine(".");

                    if (context.PrettyPrint) context.Output.Write(new string(' ', baseIndent));

                    // Start a new set of Triples
                    temp = GenerateNodeOutput(context, t.Subject, TripleSegment.Subject);
                    context.Output.Write(temp);
                    context.Output.Write(" ");
                    subjIndent = baseIndent + temp.Length + 1;
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

                    if (context.PrettyPrint) context.Output.Write(new string(' ', subjIndent));

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

                    if (context.PrettyPrint) context.Output.Write(new string(' ', subjIndent + predIndent));
                }

                // Write the Object
                temp = GenerateNodeOutput( context, t.Object, TripleSegment.Object);
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

                temp = GenerateNodeOutput(context, t.Predicate, TripleSegment.Predicate);
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
            temp = GenerateNodeOutput(context, t.Object, TripleSegment.Object);
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
    /// Generates Output for Nodes in Turtle syntax.
    /// </summary>
    /// <param name="context">Context for writing the Graph.</param>
    /// <param name="n">Node to generate output for.</param>
    /// <param name="segment">Segment of the Triple being written.</param>
    /// <returns></returns>
    private string GenerateNodeOutput(TurtleWriterContext context, INode n, TripleSegment segment)
    {
        switch (n.NodeType)
        {
            case NodeType.Blank:
                if (segment == TripleSegment.Predicate) throw new RdfOutputException(WriterErrorMessages.BlankPredicatesUnserializable("TriG"));
                break;

            case NodeType.GraphLiteral:
                throw new RdfOutputException(WriterErrorMessages.GraphLiteralsUnserializable("TriG"));

            case NodeType.Literal:
                if (segment == TripleSegment.Subject) throw new RdfOutputException(WriterErrorMessages.LiteralSubjectsUnserializable("TriG"));
                if (segment == TripleSegment.Predicate) throw new RdfOutputException(WriterErrorMessages.LiteralPredicatesUnserializable("TriG"));
                break;

            case NodeType.Triple:
                if (Syntax != TriGSyntax.Rdf11Star)
                {
                    throw new RdfOutputException(WriterErrorMessages.TripleNodesUnserializable(ToString()));
                }

                if (segment == TripleSegment.Predicate)
                {
                    throw new RdfOutputException(WriterErrorMessages.TripleNodePredicateUnserializable(ToString()));
                }
                break;

            case NodeType.Uri:
                break;

            default:
                throw new RdfOutputException(WriterErrorMessages.UnknownNodeTypeUnserializable("TriG"));
        }

        return context.NodeFormatter.Format(n, segment);
    }

    /// <summary>
    /// Thread Worker method which writes Graphs to the output.
    /// </summary>
    /// <param name="globalContext">Context for writing the Store.</param>
    private void SaveGraphs(TriGWriterContext globalContext)
    {
        try
        {
            while (globalContext.TryGetNextGraphName(out IRefNode u))
            {
                // Get the Graph from the Store
                IGraph g = globalContext.Store.Graphs[u];

                // Generate the Graph Output and add to Stream
                var context = new CompressingTurtleWriterContext(g, new System.IO.StringWriter(),
                    globalContext.PrettyPrint, globalContext.HighSpeedModePermitted,
                    Syntax == TriGSyntax.Rdf11Star ? TurtleSyntax.Rdf11Star : TurtleSyntax.W3C);
                context.NodeFormatter = GetNodeFormatter(globalContext);
                var graphContent = GenerateGraphOutput(globalContext, context);
                try
                {
                    Monitor.Enter(globalContext.Output);
                    globalContext.Output.WriteLine(graphContent);
                    globalContext.Output.Flush();
                }
                finally
                {
                    Monitor.Exit(globalContext.Output);
                }
            }
        }
        catch (ThreadAbortException)
        {
            // We've been terminated, don't do anything
            Thread.ResetAbort();
        }
        catch (Exception ex)
        {
            throw new RdfStorageException("Error in Threaded Writer in Thread ID " + Thread.CurrentThread.ManagedThreadId, ex);
        }
    }

    private INodeFormatter GetNodeFormatter(TriGWriterContext globalContext)
    {
        if (globalContext.CompressionLevel > WriterCompressionLevel.None)
        {
            return Syntax is TriGSyntax.Rdf11 or TriGSyntax.Rdf11Star ? new TurtleW3CFormatter(globalContext.QNameMapper) : new TurtleFormatter(globalContext.QNameMapper);
        }

        return Syntax == TriGSyntax.Rdf11Star ? new UncompressedTurtleStarFormatter() : new UncompressedTurtleFormatter();

    }

    /// <inheritdoc />
    public override event StoreWriterWarning Warning;

    /// <summary>
    /// Internal Helper method which raises the Warning event only if there is an Event Handler registered.
    /// </summary>
    /// <param name="message">Warning Message.</param>
    private void RaiseWarning(string message) 
    {
        Warning?.Invoke(message);
    }

    /// <summary>
    /// Gets the String representation of the writer which is a description of the syntax it produces.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return "TriG" + Syntax switch
        {
            TriGSyntax.Original => " (Original)",
            TriGSyntax.MemberSubmission => " (Submission)",
            TriGSyntax.Rdf11 => " (RDF 1.1)",
            TriGSyntax.Rdf11Star => "(RDF-Star)",
            _ => ""
        };
    }
}
