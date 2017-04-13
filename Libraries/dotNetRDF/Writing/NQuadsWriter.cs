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
using System.Threading;
using VDS.RDF.Parsing;
using VDS.RDF.Storage;
using VDS.RDF.Writing.Contexts;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Writing
{
    /// <summary>
    /// Class for serializing a Triple Store in the NQuads (NTriples plus context) syntax
    /// </summary>
    public class NQuadsWriter 
        : IStoreWriter, IPrettyPrintingWriter, IFormatterBasedWriter, IMultiThreadedWriter
    {
        private int _threads = 4;

        /// <summary>
        /// Creates a new writer
        /// </summary>
        public NQuadsWriter()
            : this(NQuadsSyntax.Original) { }

        /// <summary>
        /// Creates a new writer
        /// </summary>
        /// <param name="syntax">NQuads Syntax mode to use</param>
        public NQuadsWriter(NQuadsSyntax syntax)
        {
            PrettyPrintMode = true;
            UseMultiThreadedWriting = Options.AllowMultiThreadedWriting;
            this.Syntax = syntax;
        }

        /// <summary>
        /// Controls whether Pretty Printing is used
        /// </summary>
        /// <remarks>
        /// For NQuads this simply means that Graphs in the output are separated with Whitespace and comments used before each Graph
        /// </remarks>
        public bool PrettyPrintMode { get; set; }

        /// <summary>
        /// Gets/Sets whether Multi-Threaded Writing
        /// </summary>
        public bool UseMultiThreadedWriting { get; set; }

        /// <summary>
        /// Gets the type of the Triple Formatter used by this writer
        /// </summary>
        public Type TripleFormatterType
        {
            get
            {
                return typeof(NQuadsFormatter);
            }
        }

        /// <summary>
        /// Gets/Sets the NQuads syntax mode
        /// </summary>
        public NQuadsSyntax Syntax { get; set; }

        /// <summary>
        /// Saves a Store in NQuads format
        /// </summary>
        /// <param name="store">Store to save</param>
        /// <param name="filename">File to save to</param>
        public void Save(ITripleStore store, String filename)
        {
            if (filename == null) throw new RdfOutputException("Cannot output to a null file");
            using (var writer = new StreamWriter(File.Open(filename, FileMode.Create), Encoding.ASCII))
            {
                this.Save(store, writer);
            }
        }

        /// <summary>
        /// Saves a Store in NQuads format
        /// </summary>
        /// <param name="store">Store to save</param>
        /// <param name="writer">Writer to save to</param>
        public void Save(ITripleStore store, TextWriter writer)
        {
            if (store == null) throw new RdfOutputException("Cannot output a null Triple Store");
            if (writer == null) throw new RdfOutputException("Cannot output to a null writer");

            ThreadedStoreWriterContext context = new ThreadedStoreWriterContext(store, writer, this.PrettyPrintMode, false);
            // Check there's something to do
            if (context.Store.Graphs.Count == 0)
            {
                context.Output.Close();
                return;
            }

            try
            {
                if (this.UseMultiThreadedWriting)
                {
                    // Queue the Graphs to be written
                    foreach (IGraph g in context.Store.Graphs)
                    {
                        context.Add(g.BaseUri);
                    }

                    // Start making the async calls
                    List<IAsyncResult> results = new List<IAsyncResult>();
                    SaveGraphsDelegate d = new SaveGraphsDelegate(this.SaveGraphs);
                    for (int i = 0; i < this._threads; i++)
                    {
                        results.Add(d.BeginInvoke(context, null, null));
                    }

                    // Wait for all the async calls to complete
                    WaitHandle.WaitAll(results.Select(r => r.AsyncWaitHandle).ToArray());
                    RdfThreadedOutputException outputEx = new RdfThreadedOutputException(WriterErrorMessages.ThreadedOutputFailure("TSV"));
                    foreach (IAsyncResult result in results)
                    {
                        try
                        {
                            d.EndInvoke(result);
                        }
                        catch (Exception ex)
                        {
                            outputEx.AddException(ex);
                        }
                    }
                    context.Output.Close();

                    // If there were any errors we'll throw an RdfThreadedOutputException now
                    if (outputEx.InnerExceptions.Any()) throw outputEx;
                }
                else
                {
                    foreach (IGraph g in context.Store.Graphs)
                    {
                        NTriplesWriterContext graphContext = new NTriplesWriterContext(g, context.Output, NQuadsParser.AsNTriplesSyntax(this.Syntax));
                        foreach (Triple t in g.Triples)
                        {
                            context.Output.WriteLine(this.TripleToNQuads(graphContext, t, g.BaseUri));
                        }
                    }
                    context.Output.Close();
                }
            }
            catch
            {
                try
                {
                    context.Output.Close();
                }
                catch
                {
                    // Just cleaning up
                }
                throw;
            }
        }

        private String GraphToNQuads(ThreadedStoreWriterContext globalContext, NTriplesWriterContext context)
        {
            if (context.Graph.IsEmpty) return String.Empty;
            if (context.PrettyPrint && context.Graph.BaseUri != null)
            {
                context.Output.WriteLine("# Graph: " + context.Graph.BaseUri.AbsoluteUri);
            }
            foreach (Triple t in context.Graph.Triples)
            {
                context.Output.WriteLine(this.TripleToNQuads(context, t, context.Graph.BaseUri));
            }
            context.Output.WriteLine();

            return context.Output.ToString();
        }

        /// <summary>
        /// Converts a Triple into relevant NQuads Syntax
        /// </summary>
        /// <param name="context">Writer Context</param>
        /// <param name="t">Triple to convert</param>
        /// <param name="graphUri">Graph URI</param>
        /// <returns></returns>
        private String TripleToNQuads(NTriplesWriterContext context, Triple t, Uri graphUri)
        {
            StringBuilder output = new StringBuilder();
            output.Append(this.NodeToNTriples(context, t.Subject, TripleSegment.Subject));
            output.Append(" ");
            output.Append(this.NodeToNTriples(context, t.Predicate, TripleSegment.Predicate));
            output.Append(" ");
            output.Append(this.NodeToNTriples(context, t.Object, TripleSegment.Object));
            if (t.GraphUri != null || graphUri != null)
            {
                output.Append(" <");
                // Favour graph name rather than per-triple graph name because we may have a triple with a different graph name than the containing graph
                output.Append(context.UriFormatter.FormatUri(graphUri ?? t.GraphUri));
                output.Append(">");
            }
            output.Append(" .");

            return output.ToString();
        }

        /// <summary>
        /// Converts a Node into relevant NTriples Syntax
        /// </summary>
        /// <param name="n">Node to convert</param>
        /// <param name="context">Writer Context</param>
        /// <param name="segment">Triple Segment being written</param>
        /// <returns></returns>
        private String NodeToNTriples(NTriplesWriterContext context, INode n, TripleSegment segment)
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
                case NodeType.GraphLiteral:
                    throw new RdfOutputException(WriterErrorMessages.GraphLiteralsUnserializable("NQuads"));
                default:
                    throw new RdfOutputException(WriterErrorMessages.UnknownNodeTypeUnserializable("NQuads"));
            }

            return context.NodeFormatter.Format(n);
        }

        /// <summary>
        /// Delegate for the SaveGraphs method
        /// </summary>
        /// <param name="globalContext">Context for writing the Store</param>
        private delegate void SaveGraphsDelegate(ThreadedStoreWriterContext globalContext);

        /// <summary>
        /// Thread Worker method which writes Graphs to the output
        /// </summary>
        /// <param name="globalContext">Context for writing the Store</param>
        private void SaveGraphs(ThreadedStoreWriterContext globalContext)
        {
            try
            {
                Uri u = null;
                while (globalContext.TryGetNextUri(out u))
                {
                    // Get the Graph from the Store
                    IGraph g = globalContext.Store.Graphs[u];

                    // Generate the Graph Output and add to Stream
                    NTriplesWriterContext context = new NTriplesWriterContext(g, new System.IO.StringWriter(), NQuadsParser.AsNTriplesSyntax(this.Syntax), globalContext.PrettyPrint, globalContext.HighSpeedModePermitted);
                    String graphContent = this.GraphToNQuads(globalContext, context);
                    if (!graphContent.Equals(String.Empty))
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
#if !NETCORE // .NET Core doesn't provide Thread.Abort() or ThreadAbortException
            catch (ThreadAbortException)
            {
                // We've been terminated, don't do anything
                Thread.ResetAbort();
            }
#endif
            catch (Exception ex)
            {
                throw new RdfStorageException("Error in Threaded Writer in Thread ID " + Thread.CurrentThread.ManagedThreadId, ex);
            }
        }

        /// <summary>
        /// Event which is raised when there is an issue with the Graphs being serialized that doesn't prevent serialization but the user should be aware of
        /// </summary>
        public event StoreWriterWarning Warning;

        /// <summary>
        /// Internal Helper method which raises the Warning event only if there is an Event Handler registered
        /// </summary>
        /// <param name="message">Warning Message</param>
        private void RaiseWarning(String message)
        {
            StoreWriterWarning d = this.Warning;
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
