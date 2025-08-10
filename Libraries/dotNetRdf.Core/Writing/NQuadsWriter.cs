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
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VDS.RDF.Parsing;
using VDS.RDF.Storage;
using VDS.RDF.Writing.Contexts;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Writing;

/// <summary>
/// Class for serializing a Triple Store in the NQuads (NTriples plus context) syntax.
/// </summary>
public class NQuadsWriter 
    : BaseStoreWriter, IPrettyPrintingWriter, IFormatterBasedWriter, IMultiThreadedWriter, IRdfStarCapableWriter
{
    private int _threads = 4;

    /// <summary>
    /// Creates a new writer.
    /// </summary>
    public NQuadsWriter()
        : this(NQuadsSyntax.Original) { }

    /// <summary>
    /// Creates a new writer.
    /// </summary>
    /// <param name="syntax">NQuads Syntax mode to use.</param>
    public NQuadsWriter(NQuadsSyntax syntax)
    {
        PrettyPrintMode = true;
        Syntax = syntax;
    }

    /// <summary>
    /// Controls whether Pretty Printing is used.
    /// </summary>
    /// <remarks>
    /// For NQuads this simply means that Graphs in the output are separated with Whitespace and comments used before each Graph.
    /// </remarks>
    public bool PrettyPrintMode { get; set; }

    /// <summary>
    /// Gets/Sets whether Multi-Threaded Writing.
    /// </summary>
#pragma warning disable CS0618 // Type or member is obsolete
    public bool UseMultiThreadedWriting { get; set; } = Options.AllowMultiThreadedWriting; // = false;
#pragma warning restore CS0618 // Type or member is obsolete

    /// <summary>
    /// Gets the type of the Triple Formatter used by this writer.
    /// </summary>
    public Type TripleFormatterType => typeof(NQuadsFormatter);

    /// <summary>
    /// Gets/Sets the NQuads syntax mode.
    /// </summary>
    public NQuadsSyntax Syntax { get; set; }

    /// <summary>
    /// Gets whether the current syntax mode supports writing RDF-Star triple nodes or not.
    /// </summary>
    public bool CanWriteTripleNodes => Syntax == NQuadsSyntax.Rdf11;

    /// <summary>
    /// Saves a Store in NQuads format.
    /// </summary>
    /// <param name="store">Store to save.</param>
    /// <param name="writer">Writer to save to.</param>
    /// <param name="leaveOpen">Boolean flag indicating if <paramref name="writer"/> should be left open after the store is written.</param>
    public override void Save(ITripleStore store, TextWriter writer, bool leaveOpen)
    {
        if (store == null) throw new RdfOutputException("Cannot output a null Triple Store");
        if (writer == null) throw new RdfOutputException("Cannot output to a null writer");

        var context = new ThreadedStoreWriterContext(store, writer, PrettyPrintMode, false);
        // Check there's something to do
        if (context.Store.Graphs.Count == 0)
        {
            if (!leaveOpen)
            {
                context.Output.Close();
            }
            return;
        }

        try
        {
            if (UseMultiThreadedWriting)
            {
                // Queue the Graphs to be written
                foreach (IGraph g in context.Store.Graphs)
                {
                    context.Add(g.Name);
                }

                // Start making the async calls
                //var results = new List<IAsyncResult>();
                var workers = new Task[_threads];
                for (var i = 0; i < _threads; i++)
                {
                    workers[i] = Task.Factory.StartNew(()=>SaveGraphs(context));
                }

                try
                {
                    Task.WaitAll(workers);
                }
                catch (AggregateException ex)
                {
                    var outputException =
                        new RdfThreadedOutputException(WriterErrorMessages.ThreadedOutputFailure("TSV"));
                    foreach (Exception innerException in ex.InnerExceptions)
                    {
                        outputException.AddException(innerException);
                    }
                }
                finally
                {
                    if (!leaveOpen) context.Output.Close();
                }
            }
            else
            {
                foreach (IGraph g in context.Store.Graphs)
                {
                    var graphContext = new NTriplesWriterContext(g, context.Output, NQuadsParser.AsNTriplesSyntax(Syntax));
                    foreach (Triple t in g.Triples)
                    {
                        context.Output.WriteLine(TripleToNQuads(graphContext, t, g.Name));
                    }
                }
                if (!leaveOpen)
                {
                    context.Output.Close();
                }
            }
        }
        catch
        {
            try
            {
                if (!leaveOpen)
                {
                    context.Output.Close();
                }
            }
            catch
            {
                // Just cleaning up
            }
            throw;
        }
    }

    private string GraphToNQuads(ThreadedStoreWriterContext globalContext, NTriplesWriterContext context)
    {
        if (context.Graph.IsEmpty) return string.Empty;
        if (context.PrettyPrint && context.Graph.BaseUri != null)
        {
            context.Output.WriteLine("# Graph: " + context.Graph.BaseUri.AbsoluteUri);
        }
        foreach (Triple t in context.Graph.Triples)
        {
            context.Output.WriteLine(TripleToNQuads(context, t, context.Graph.Name));
        }
        context.Output.WriteLine();

        return context.Output.ToString();
    }

    /// <summary>
    /// Converts a Triple into relevant NQuads Syntax.
    /// </summary>
    /// <param name="context">Writer Context.</param>
    /// <param name="t">Triple to convert.</param>
    /// <param name="graphName">Graph name.</param>
    /// <returns></returns>
    private string TripleToNQuads(NTriplesWriterContext context, Triple t, IRefNode graphName)
    {
        var output = new StringBuilder();
        output.Append(NodeToNTriples(context, t.Subject, TripleSegment.Subject));
        output.Append(" ");
        output.Append(NodeToNTriples(context, t.Predicate, TripleSegment.Predicate));
        output.Append(" ");
        output.Append(NodeToNTriples(context, t.Object, TripleSegment.Object));
        if (graphName != null)
        {
            output.Append(" ");
            output.Append(context.NodeFormatter.Format(graphName));
        }
        output.Append(" .");

        return output.ToString();
    }

    /// <summary>
    /// Converts a Node into relevant NTriples Syntax.
    /// </summary>
    /// <param name="n">Node to convert.</param>
    /// <param name="context">Writer Context.</param>
    /// <param name="segment">Triple Segment being written.</param>
    /// <returns></returns>
    private string NodeToNTriples(NTriplesWriterContext context, INode n, TripleSegment segment)
    {
        switch (n.NodeType)
        {
            case NodeType.Blank:
                if (segment == TripleSegment.Predicate) throw new RdfOutputException(WriterErrorMessages.BlankPredicatesUnserializable("NQuads"));
                break;
            case NodeType.Literal:
                if (segment == TripleSegment.Subject) throw new RdfOutputException(WriterErrorMessages.LiteralSubjectsUnserializable("NQuads"));
                if (segment == TripleSegment.Predicate) throw new RdfOutputException(WriterErrorMessages.LiteralPredicatesUnserializable("NQuads"));
                break;
            case NodeType.Uri:
                break;
            case NodeType.Triple:
                if (Syntax != NQuadsSyntax.Rdf11Star)
                {
                    throw new RdfOutputException(WriterErrorMessages.TripleNodesUnserializable(ToString()));
                }

                if (segment == TripleSegment.Predicate)
                    throw new RdfOutputException(WriterErrorMessages.TripleNodePredicateUnserializable(ToString()));
                break;
            case NodeType.GraphLiteral:
                throw new RdfOutputException(WriterErrorMessages.GraphLiteralsUnserializable("NQuads"));
            default:
                throw new RdfOutputException(WriterErrorMessages.UnknownNodeTypeUnserializable("NQuads"));
        }

        return context.NodeFormatter.Format(n);
    }

    /// <summary>
    /// Thread Worker method which writes Graphs to the output.
    /// </summary>
    /// <param name="globalContext">Context for writing the Store.</param>
    private void SaveGraphs(ThreadedStoreWriterContext globalContext)
    {
        try
        {
            while (globalContext.TryGetNextGraphName(out IRefNode u))
            {
                // Get the Graph from the Store
                IGraph g = globalContext.Store.Graphs[u];

                // Generate the Graph Output and add to Stream
                var context = new NTriplesWriterContext(g, new System.IO.StringWriter(), NQuadsParser.AsNTriplesSyntax(Syntax), globalContext.PrettyPrint, globalContext.HighSpeedModePermitted);
                var graphContent = GraphToNQuads(globalContext, context);
                if (!graphContent.Equals(string.Empty))
                {
                    try
                    {
                        Monitor.Enter(globalContext.Output);
                        globalContext.Output.WriteLine(graphContent);
                        globalContext.Output.Flush();
                    }
                    catch
                    {
                        throw;
                    }
                    finally
                    {
                        Monitor.Exit(globalContext.Output);
                    }
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

    /// <summary>
    /// Event which is raised when there is an issue with the Graphs being serialized that doesn't prevent serialization but the user should be aware of
    /// </summary>
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
        return Syntax switch
        {
            NQuadsSyntax.Rdf11 => "NQuads (RDF 1.1)",
            NQuadsSyntax.Rdf11Star => "NQuads (RDF-Star)",
            _ => "NQuads"
        };
    }
}
