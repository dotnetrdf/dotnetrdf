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
using VDS.RDF.Storage;
using VDS.RDF.Storage.Params;
using VDS.RDF.Writing.Contexts;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Writing
{
    /// <summary>
    /// Class for generating TSV files from RDF Graphs
    /// </summary>
    public class TsvWriter 
        : IRdfWriter, IFormatterBasedWriter
    {
        private TsvFormatter _formatter = new TsvFormatter();

        /// <summary>
        /// Gets the type of the Triple Formatter used by this writer
        /// </summary>
        public Type TripleFormatterType
        {
            get
            {
                return this._formatter.GetType();
            }
        }

        /// <summary>
        /// Saves a Graph to TSV format
        /// </summary>
        /// <param name="g">Graph</param>
        /// <param name="filename">File to save to</param>
        public void Save(IGraph g, string filename)
        {
            this.Save(g, new StreamWriter(filename, false, new UTF8Encoding(Options.UseBomForUtf8)));
        }

        /// <summary>
        /// Saves a Graph to TSV format
        /// </summary>
        /// <param name="g">Graph</param>
        /// <param name="output">Writer to save to</param>
        public void Save(IGraph g, TextWriter output)
        {
            try
            {
                foreach (Triple t in g.Triples)
                {
                    this.GenerateNodeOutput(output, t.Subject, TripleSegment.Subject);
                    output.Write('\t');
                    this.GenerateNodeOutput(output, t.Predicate, TripleSegment.Predicate);
                    output.Write('\t');
                    this.GenerateNodeOutput(output, t.Object, TripleSegment.Object);
                    output.Write('\n');
                }

                output.Close();
            }
            catch
            {
                try
                {
                    output.Close();
                }
                catch
                {
                    //No error handling, just trying to clean up
                }
                throw;
            }
        }

        private void GenerateNodeOutput(TextWriter output, INode n, TripleSegment segment)
        {
            switch (n.NodeType)
            {
                case NodeType.GraphLiteral:
                    throw new RdfOutputException(WriterErrorMessages.GraphLiteralsUnserializable("TSV"));
                case NodeType.Blank:
                case NodeType.Literal:
                case NodeType.Uri:
                    output.Write(this._formatter.Format(n));
                    break;
                default:
                    throw new RdfOutputException(WriterErrorMessages.UnknownNodeTypeUnserializable("TSV"));
            }
        }

        /// <summary>
        /// Event which is raised if the Writer detects a non-fatal error with the RDF being output
        /// </summary>
        public event RdfWriterWarning Warning;

        /// <summary>
        /// Gets the String representation of the writer which is a description of the syntax it produces
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "TSV";
        }
    }

    /// <summary>
    /// Class for generating TSV output from RDF Datasets
    /// </summary>
    public class TsvStoreWriter 
        : IStoreWriter, IFormatterBasedWriter
    {
        private int _threads = 4;
        private TsvFormatter _formatter = new TsvFormatter();

        /// <summary>
        /// Gets the type of the Triple Formatter used by this writer
        /// </summary>
        public Type TripleFormatterType
        {
            get
            {
                return this._formatter.GetType();
            }
        }

        /// <summary>
        /// Saves a Triple Store to TSV format
        /// </summary>
        /// <param name="store">Triple Store to save</param>
        /// <param name="parameters">A set of <see cref="StreamParams">StreamParams</see></param>
        public void Save(ITripleStore store, IStoreParams parameters)
        {
            ThreadedStoreWriterContext context = null;
            if (parameters is StreamParams)
            {
                //Create a new Writer Context
                context = new ThreadedStoreWriterContext(store, ((StreamParams)parameters).StreamWriter);
            } 
            else if (parameters is TextWriterParams)
            {
                context = new ThreadedStoreWriterContext(store, ((TextWriterParams)parameters).TextWriter);
            }

            if (context != null)
            {
                //Check there's something to do
                if (context.Store.Graphs.Count == 0)
                {
                    context.Output.Close();
                    return;
                }

                //Queue the Graphs to be written
                foreach (IGraph g in context.Store.Graphs)
                {
                    context.Add(g.BaseUri);
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

                //If there were any errors we'll throw an RdfThreadedOutputException now
                if (outputEx.InnerExceptions.Any()) throw outputEx;
            }
            else
            {
                throw new RdfStorageException("Parameters for the TsvStoreWriter must be of the type StreamParams/TextWriterParams");
            }
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
                Uri u = globalContext.GetNextUri();
                while (u != null)
                {
                    //Get the Graph from the Store
                    IGraph g = globalContext.Store.Graphs[u];

                    //Generate the Graph Output and add to Stream
                    BaseWriterContext context = new BaseWriterContext(g, new System.IO.StringWriter());
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
        /// Generates the Output for a Graph as a String in TSV syntax
        /// </summary>
        /// <param name="globalContext">Context for writing the Store</param>
        /// <param name="context">Context for writing the Graph</param>
        /// <returns></returns>
        private String GenerateGraphOutput(ThreadedStoreWriterContext globalContext, BaseWriterContext context)
        {
            if (!WriterHelper.IsDefaultGraph(context.Graph.BaseUri))
            {
                //Named Graphs have a fourth context field added
                foreach (Triple t in context.Graph.Triples)
                {
                    this.GenerateNodeOutput(context, t.Subject, TripleSegment.Subject);
                    context.Output.Write('\t');
                    this.GenerateNodeOutput(context, t.Predicate, TripleSegment.Predicate);
                    context.Output.Write('\t');
                    this.GenerateNodeOutput(context, t.Object, TripleSegment.Object);
                    context.Output.Write('\t');
                    context.Output.Write('<');
                    context.Output.Write(this._formatter.FormatUri(context.Graph.BaseUri));
                    context.Output.Write('>');
                    context.Output.Write('\n');
                }
            }
            else
            {
                //Default Graph has an empty field added
                foreach (Triple t in context.Graph.Triples)
                {
                    this.GenerateNodeOutput(context, t.Subject, TripleSegment.Subject);
                    context.Output.Write('\t');
                    this.GenerateNodeOutput(context, t.Predicate, TripleSegment.Predicate);
                    context.Output.Write('\t');
                    this.GenerateNodeOutput(context, t.Object, TripleSegment.Object);
                    context.Output.Write('\t');
                    context.Output.Write('\n');
                }
            }

            return context.Output.ToString();
        }

        /// <summary>
        /// Generates Output for the given Node
        /// </summary>
        /// <param name="context">Writer Context</param>
        /// <param name="n">Node</param>
        /// <param name="segment">Triple Context</param>
        private void GenerateNodeOutput(BaseWriterContext context, INode n, TripleSegment segment)
        {
            switch (n.NodeType)
            {
                case NodeType.GraphLiteral:
                    throw new RdfOutputException(WriterErrorMessages.GraphLiteralsUnserializable("TSV"));
                case NodeType.Blank:
                case NodeType.Literal:
                case NodeType.Uri:
                    context.Output.Write(this._formatter.Format(n));
                    break;
                default:
                    throw new RdfOutputException(WriterErrorMessages.UnknownNodeTypeUnserializable("TSV"));
            }
        }

        /// <summary>
        /// Event which is raised if the Writer detects a non-fatal error with the RDF being output
        /// </summary>
        public event StoreWriterWarning Warning;

        /// <summary>
        /// Gets the String representation of the writer which is a description of the syntax it produces
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "TSV";
        }
    }

}
