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
using System.Threading;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Storage;
using VDS.RDF.Storage.Params;
using VDS.RDF.Writing.Contexts;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Writing
{
    /// <summary>
    /// Class for writing a Triple Store in named Graph TriG syntax to a file/stream
    /// </summary>
    /// <remarks>
    /// <para>
    /// For efficiency the TriG Writer splits it's writing over several threads (currently 4), these threads share a reference to a Context object which gives Global writing context eg. the target <see cref="TextWriter">TextWriter</see> being written to.  Each thread generates temporary local writing contexts as it goes along, each of these is scoped to writing a specific Graph.  Graphs are written to a <see cref="StringWriter">StringWriter</see> so the output for each Graph is built completely and then written in one go to the <see cref="TextWriter">TextWriter</see> specified as the target of the writing in the global context.
    /// </para>
    /// </remarks>
    /// <threadsafety instance="true">Designed to be Thread Safe - should be able to call <see cref="TriGWriter.Save">Save()</see> from several threads with no issue.  See Remarks for potential performance impact of this.</threadsafety>
    public class TriGWriter : IStoreWriter, IHighSpeedWriter, IPrettyPrintingWriter, ICompressingWriter, IMultiThreadedWriter
    {
        private int _threads = 4;
        private const int PollInterval = 50;
        private bool _allowHiSpeed = true;
        private bool _prettyprint = true;
        private bool _n3compat = false;
        private int _compressionLevel = WriterCompressionLevel.Default;
        private bool _useMultiThreading = Options.AllowMultiThreadedWriting;

        /// <summary>
        /// Gets/Sets whether High Speed Write Mode is permitted
        /// </summary>
        public bool HighSpeedModePermitted
        {
            get
            {
                return this._allowHiSpeed;
            }
            set
            {
                this._allowHiSpeed = value;
            }
        }

        /// <summary>
        /// Gets/Sets whether Pretty Printing is used
        /// </summary>
        public bool PrettyPrintMode 
        {
            get 
            {
                return this._prettyprint;
            }
            set 
            {
                this._prettyprint = value;
            }
        }

        /// <summary>
        /// Gets/Sets the Compression Level for the writer
        /// </summary>
        public int CompressionLevel
        {
            get
            {
                return this._compressionLevel;
            }
            set
            {
                this._compressionLevel = value;
            }
        }

        /// <summary>
        /// Gets/Sets whether N3 Compatability Mode is used, in this mode an = is written after Graph Names so an N3 parser can read the TriG file correctly
        /// </summary>
        /// <remarks>
        /// Defaults to <strong>false</strong> from the 0.4.1 release onwards
        /// </remarks>
        public bool N3CompatabilityMode
        {
            get
            {
                return this._n3compat;
            }
            set
            {
                this._n3compat = value;
            }
        }

        /// <summary>
        /// Gets/Sets whether multi-threaded writing will be used to generate output faster
        /// </summary>
        public bool UseMultiThreadedWriting
        {
            get
            {
                return this._useMultiThreading;
            }
            set
            {
                this._useMultiThreading = value;
            }
        }

        /// <summary>
        /// Saves a Store in TriG (Turtle with Named Graphs) format
        /// </summary>
        /// <param name="store">Store to save</param>
        /// <param name="parameters">Parameters indicating a Stream to write to</param>
        public void Save(ITripleStore store, IStoreParams parameters)
        {
            //Try and determine the TextWriter to output to
            TriGWriterContext context = null;
            if (parameters is StreamParams)
            {
                //Create a new Writer Context
                ((StreamParams)parameters).Encoding = new UTF8Encoding(Options.UseBomForUtf8);
                context = new TriGWriterContext(store, ((StreamParams)parameters).StreamWriter, this._prettyprint, this._allowHiSpeed, this._compressionLevel, this._n3compat);
            } 
            else if (parameters is TextWriterParams)
            {
                context = new TriGWriterContext(store, ((TextWriterParams)parameters).TextWriter, this._prettyprint, this._allowHiSpeed, this._compressionLevel, this._n3compat);
            }

            if (context != null)
            {
                //Check there's something to do
                if (context.Store.Graphs.Count == 0) 
                {
                    context.Output.Close();
                    return;
                }

                //Write the Header of the File
                foreach (IGraph g in context.Store.Graphs)
                {
                    context.NamespaceMap.Import(g.NamespaceMap);
                }
                if (context.CompressionLevel > WriterCompressionLevel.None)
                {
                    //Only add @prefix declarations if compression is enabled
                    context.QNameMapper = new ThreadSafeQNameOutputMapper(context.NamespaceMap);
                    foreach (String prefix in context.NamespaceMap.Prefixes)
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

                if (this._useMultiThreading)
                {
                    //Standard Multi-Threaded Writing

                    //Queue the Graphs to be written
                    foreach (IGraph g in context.Store.Graphs)
                    {
                        if (g.BaseUri == null)
                        {
                            context.Add(new Uri(GraphCollection.DefaultGraphUri));
                        }
                        else
                        {
                            context.Add(g.BaseUri);
                        }
                    }

                    //Start making the async calls
                    List<IAsyncResult> results = new List<IAsyncResult>();
                    SaveGraphsDelegate d = new SaveGraphsDelegate(this.SaveGraphs);
                    for (int i = 0; i < this._threads; i++)
                    {
                        results.Add(d.BeginInvoke(context, null, null));
                    }

                    //Wait for all the async calls to complete
                    WaitHandle.WaitAll(results.Select(r => r.AsyncWaitHandle).ToArray());
                    RdfThreadedOutputException outputEx = new RdfThreadedOutputException(WriterErrorMessages.ThreadedOutputFailure("TriG"));
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
                    //Make sure to close the output
                    context.Output.Close();

                    //If there were any errors we'll throw an RdfThreadedOutputException now
                    if (outputEx.InnerExceptions.Any()) throw outputEx;
                }
                else
                {
                    try
                    {
                        //Optional Single Threaded Writing
                        foreach (IGraph g in store.Graphs)
                        {
                            TurtleWriterContext graphContext = new TurtleWriterContext(g, new System.IO.StringWriter(), context.PrettyPrint, context.HighSpeedModePermitted);
                            if (context.CompressionLevel > WriterCompressionLevel.None)
                            {
                                graphContext.NodeFormatter = new TurtleFormatter(context.QNameMapper);
                            }
                            else
                            {
                                graphContext.NodeFormatter = new UncompressedTurtleFormatter();
                            }
                            context.Output.WriteLine(this.GenerateGraphOutput(context, graphContext));
                        }
                        
                        //Make sure to close the output
                        context.Output.Close();
                    }
                    catch
                    {
                        try
                        {
                            //Close the output
                            context.Output.Close();
                        }
                        catch
                        {
                            //No catch actions, just cleaning up the output stream
                        }
                        throw;
                    }
                }
            }
            else
            {
                throw new RdfStorageException("Parameters for the TriGWriter must be of the type StreamParams/TextWriterParams");
            }
        }

        /// <summary>
        /// Generates the Output for a Graph as a String in TriG syntax
        /// </summary>
        /// <param name="globalContext">Context for writing the Store</param>
        /// <param name="context">Context for writing the Graph</param>
        /// <returns></returns>
        private String GenerateGraphOutput(TriGWriterContext globalContext, TurtleWriterContext context)
        {
            if (!WriterHelper.IsDefaultGraph(context.Graph.BaseUri))
            {
                //Named Graph
                String gname;
                String sep = (globalContext.N3CompatabilityMode) ? " = " : " ";
                if (globalContext.CompressionLevel > WriterCompressionLevel.None && globalContext.QNameMapper.ReduceToQName(context.Graph.BaseUri.ToString(), out gname))
                {
                    if (TurtleSpecsHelper.IsValidQName(gname))
                    {
                        context.Output.WriteLine(gname + sep + "{");
                    }
                    else
                    {
                        context.Output.WriteLine("<" + context.UriFormatter.FormatUri(context.Graph.BaseUri) + ">" + sep + "{");
                    }
                }
                else
                {
                    context.Output.WriteLine("<" + context.UriFormatter.FormatUri(context.Graph.BaseUri) + ">" + sep + "{");
                }
            }
            else
            {
                context.Output.WriteLine("{");
            }

            //Generate Triples
            this.GenerateTripleOutput(globalContext, context);

            //Close the Graph
            context.Output.WriteLine("}");

            return context.Output.ToString();
        }

        /// <summary>
        /// Generates the Output for a Triple as a String in Turtle syntax
        /// </summary>
        /// <param name="globalContext">Context for writing the Store</param>
        /// <param name="context">Context for writing the Graph</param>
        private void GenerateTripleOutput(TriGWriterContext globalContext, TurtleWriterContext context)
        {
            //Decide which write mode to use
            bool hiSpeed = false;
            double subjNodes = context.Graph.Triples.SubjectNodes.Count();
            double triples = context.Graph.Triples.Count;
            if ((subjNodes / triples) > 0.75) hiSpeed = true;

            if (globalContext.CompressionLevel == WriterCompressionLevel.None || hiSpeed && context.HighSpeedModePermitted)
            {
                //Use High Speed Write Mode
                String indentation = new String(' ', 4);
                context.Output.Write(indentation);
                if (globalContext.CompressionLevel > WriterCompressionLevel.None) context.Output.WriteLine("# Written using High Speed Mode");
                foreach (Triple t in context.Graph.Triples)
                {
                    context.Output.Write(indentation);
                    context.Output.Write(this.GenerateNodeOutput(globalContext, context, t.Subject, TripleSegment.Subject));
                    context.Output.Write(' ');
                    context.Output.Write(this.GenerateNodeOutput(globalContext, context, t.Predicate, TripleSegment.Predicate));
                    context.Output.Write(' ');
                    context.Output.Write(this.GenerateNodeOutput(globalContext, context, t.Object, TripleSegment.Object));
                    context.Output.WriteLine(".");
                }
            }
            else
            {
                //Get the Triples as a Sorted List
                List<Triple> ts = context.Graph.Triples.ToList();
                ts.Sort();

                //Variables we need to track our writing
                INode lastSubj, lastPred;
                lastSubj = lastPred = null;
                int subjIndent = 0, predIndent = 0;
                int baseIndent = 4;
                String temp;

                for (int i = 0; i < ts.Count; i++)
                {
                    Triple t = ts[i];
                    if (lastSubj == null || !t.Subject.Equals(lastSubj))
                    {
                        //Terminate previous Triples
                        if (lastSubj != null) context.Output.WriteLine(".");

                        if (context.PrettyPrint) context.Output.Write(new String(' ', baseIndent));

                        //Start a new set of Triples
                        temp = this.GenerateNodeOutput(globalContext, context, t.Subject, TripleSegment.Subject);
                        context.Output.Write(temp);
                        context.Output.Write(" ");
                        subjIndent = baseIndent + temp.Length + 1;
                        lastSubj = t.Subject;

                        //Write the first Predicate
                        temp = this.GenerateNodeOutput(globalContext, context, t.Predicate, TripleSegment.Predicate);
                        context.Output.Write(temp);
                        context.Output.Write(" ");
                        predIndent = temp.Length + 1;
                        lastPred = t.Predicate;
                    }
                    else if (lastPred == null || !t.Predicate.Equals(lastPred))
                    {
                        //Terminate previous Predicate Object list
                        context.Output.WriteLine(";");

                        if (context.PrettyPrint) context.Output.Write(new String(' ', subjIndent));

                        //Write the next Predicate
                        temp = this.GenerateNodeOutput(globalContext, context, t.Predicate, TripleSegment.Predicate);
                        context.Output.Write(temp);
                        context.Output.Write(" ");
                        predIndent = temp.Length + 1;
                        lastPred = t.Predicate;
                    }
                    else
                    {
                        //Continue Object List
                        context.Output.WriteLine(",");

                        if (context.PrettyPrint) context.Output.Write(new String(' ', subjIndent + predIndent));
                    }

                    //Write the Object
                    context.Output.Write(this.GenerateNodeOutput(globalContext, context, t.Object, TripleSegment.Object));
                }

                //Terminate Triples
                if (ts.Count > 0) context.Output.WriteLine(".");               
            }
        }

        /// <summary>
        /// Generates Output for Nodes in Turtle syntax
        /// </summary>
        /// <param name="globalContext">Context for writing the Store</param>
        /// <param name="context">Context for writing the Graph</param>
        /// <param name="n">Node to generate output for</param>
        /// <param name="segment">Segment of the Triple being written</param>
        /// <returns></returns>
        private String GenerateNodeOutput(TriGWriterContext globalContext, TurtleWriterContext context, INode n, TripleSegment segment)
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

                case NodeType.Uri:
                    break;

                default:
                    throw new RdfOutputException(WriterErrorMessages.UnknownNodeTypeUnserializable("TriG"));
            }

            return context.NodeFormatter.Format(n, segment);
        }

        /// <summary>
        /// Delegate for the SaveGraphs method
        /// </summary>
        /// <param name="globalContext">Context for writing the Store</param>
        private delegate void SaveGraphsDelegate(TriGWriterContext globalContext);

        /// <summary>
        /// Thread Worker method which writes Graphs to the output
        /// </summary>
        /// <param name="globalContext">Context for writing the Store</param>
        private void SaveGraphs(TriGWriterContext globalContext)
        {
            try
            {
                Uri u = globalContext.GetNextUri();
                while (u != null)
                {
                    //Get the Graph from the Store
                    if (WriterHelper.IsDefaultGraph(u) && !globalContext.Store.HasGraph(u)) u = null;
                    IGraph g = globalContext.Store.Graphs[u];

                    //Generate the Graph Output and add to Stream
                    TurtleWriterContext context = new TurtleWriterContext(g, new System.IO.StringWriter(), globalContext.PrettyPrint, globalContext.HighSpeedModePermitted);
                    if (globalContext.CompressionLevel > WriterCompressionLevel.None)
                    {
                        context.NodeFormatter = new TurtleFormatter(globalContext.QNameMapper);
                    }
                    else
                    {
                        context.NodeFormatter = new UncompressedTurtleFormatter();
                    }
                    String graphContent = this.GenerateGraphOutput(globalContext, context);
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

                    //Get the Next Uri
                    u = globalContext.GetNextUri();
                }
            }
            catch (ThreadAbortException)
            {
                //We've been terminated, don't do anything
#if !SILVERLIGHT
                Thread.ResetAbort();
#endif
            }
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
            return "TriG";
        }
    }
}
