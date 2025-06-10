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
using System.Threading;
using System.Threading.Tasks;
using VDS.RDF.Storage;
using VDS.RDF.Writing.Contexts;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Writing;

/// <summary>
/// Class for generating TSV output from RDF datasets.
/// </summary>
public class TsvStoreWriter 
    : BaseStoreWriter, IFormatterBasedWriter
{
    private int _threads = 4;
    private readonly TsvFormatter _formatter = new TsvFormatter();

    /// <summary>
    /// Gets the type of the Triple Formatter used by this writer.
    /// </summary>
    public Type TripleFormatterType => _formatter.GetType();

    /// <summary>
    /// Saves a Triple Store to TSV format.
    /// </summary>
    /// <param name="store">Triple Store to save.</param>
    /// <param name="writer">Writer to save to.</param>
    /// <param name="leaveOpen">Boolean flag indicating if <paramref name="writer"/> should be left open after the store is saved.</param>
    public override void Save(ITripleStore store, TextWriter writer, bool leaveOpen)
    {
        if (store == null) throw new RdfOutputException("Cannot output a null Triple Store");
        if (writer == null) throw new RdfOutputException("Cannot output to a null writer");

        var context = new ThreadedStoreWriterContext(store, writer);

        // Check there's something to do
        if (context.Store.Graphs.Count == 0)
        {
            if (!leaveOpen)
            {
                context.Output.Close();
            }
            return;
        }

        // Queue the Graphs to be written
        foreach (IGraph g in context.Store.Graphs)
        {
            context.Add(g.Name);
        }

        // Start making the async calls
        var tasks = new List<Task>();
        for (var i = 0; i < _threads; i++)
        {
            tasks.Add(Task.Factory.StartNew(()=>SaveGraphs(context)));
        }

        // Wait for all the async calls to complete
        var outputEx = new RdfThreadedOutputException(WriterErrorMessages.ThreadedOutputFailure("TSV"));
        try
        {
            Task.WaitAll(tasks.ToArray());
        }
        catch (AggregateException ex)
        {
            foreach (Exception innerEx in ex.InnerExceptions)
            {
                outputEx.AddException(innerEx);
            }
        }

        if (!leaveOpen)
        {
            context.Output.Close();
        }

        // If there were any errors we'll throw an RdfThreadedOutputException now
        if (outputEx.InnerExceptions.Any()) throw outputEx;
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
                var context = new BaseWriterContext(g, new System.IO.StringWriter());
                var graphContent = GenerateGraphOutput(context);
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

    /// <summary>
    /// Generates the Output for a Graph as a String in TSV syntax.
    /// </summary>
    /// <param name="context">Context for writing the Graph.</param>
    /// <returns></returns>
    private string GenerateGraphOutput(IWriterContext context)
    {
        if (context.Graph.BaseUri != null)
        {
            // Named Graphs have a fourth context field added
            foreach (Triple t in context.Graph.Triples)
            {
                GenerateNodeOutput(context, t.Subject);
                context.Output.Write('\t');
                GenerateNodeOutput(context, t.Predicate);
                context.Output.Write('\t');
                GenerateNodeOutput(context, t.Object);
                context.Output.Write('\t');
                context.Output.Write('<');
                context.Output.Write(_formatter.FormatUri(context.Graph.BaseUri));
                context.Output.Write('>');
                context.Output.Write('\n');
            }
        }
        else
        {
            // Default Graph has an empty field added
            foreach (Triple t in context.Graph.Triples)
            {
                GenerateNodeOutput(context, t.Subject);
                context.Output.Write('\t');
                GenerateNodeOutput(context, t.Predicate);
                context.Output.Write('\t');
                GenerateNodeOutput(context, t.Object);
                context.Output.Write('\t');
                context.Output.Write('\n');
            }
        }

        return context.Output.ToString();
    }

    /// <summary>
    /// Generates Output for the given Node.
    /// </summary>
    /// <param name="context">Writer Context.</param>
    /// <param name="n">Node.</param>
    private void GenerateNodeOutput(IWriterContext context, INode n)
    {
        switch (n.NodeType)
        {
            case NodeType.GraphLiteral:
                throw new RdfOutputException(WriterErrorMessages.GraphLiteralsUnserializable("TSV"));
            case NodeType.Blank:
            case NodeType.Literal:
            case NodeType.Uri:
                context.Output.Write(_formatter.Format(n));
                break;
            default:
                throw new RdfOutputException(WriterErrorMessages.UnknownNodeTypeUnserializable("TSV"));
        }
    }

    /// <summary>
    /// Event which is raised if the Writer detects a non-fatal error with the RDF being output
    /// </summary>
    /// <remarks>This class does not raise this event.</remarks>
#pragma warning disable CS0067
    public override event StoreWriterWarning Warning;
#pragma warning restore CS0067

    /// <summary>
    /// Gets the String representation of the writer which is a description of the syntax it produces.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return "TSV";
    }
}