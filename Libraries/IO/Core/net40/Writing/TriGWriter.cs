/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

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
using System.Threading;
using VDS.RDF.Graphs;
using VDS.RDF.Namespaces;
using VDS.RDF.Nodes;
using VDS.RDF.Specifications;
using VDS.RDF.Writing.Contexts;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Writing
{
    /// <summary>
    /// Class for writing a graph store or graph to TriG syntax (Turtle with named graphs)
    /// </summary>
    public class TriGWriter 
        : BaseGraphStoreWriter, IHighSpeedWriter, IPrettyPrintingWriter, ICompressingWriter
    {
        private bool _allowHiSpeed = true;
        private bool _prettyprint = true;
        private bool _n3Compat = false;
        private int _compressionLevel = WriterCompressionLevel.Default;

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
                return this._n3Compat;
            }
            set
            {
                this._n3Compat = value;
            }
        }

        /// <summary>
        /// Saves a graph store in TriG (Turtle with Named Graphs) format
        /// </summary>
        /// <param name="store">Store to save</param>
        /// <param name="writer">Writer to save to</param>
        public override void Save(IGraphStore store, TextWriter writer)
        {
            if (store == null) throw new RdfOutputException("Cannot output a null Triple Store");
            if (writer == null) throw new RdfOutputException("Cannot output to a null writer");

            TriGWriterContext context = new TriGWriterContext(store, writer, this._prettyprint, this._allowHiSpeed, this._compressionLevel, this._n3Compat);

            //Write the Header of the File
            foreach (IGraph g in context.GraphStore.Graphs)
            {
                context.Namespaces.Import(g.Namespaces);
            }
            if (context.CompressionLevel > WriterCompressionLevel.None)
            {
                //Only add @prefix declarations if compression is enabled
                context.QNameMapper = new ThreadSafeQNameOutputMapper(context.Namespaces);
                foreach (String prefix in context.Namespaces.Prefixes)
                {
                    if (TurtleSpecsHelper.IsValidQName(prefix + ":"))
                    {
                        context.Output.WriteLine("@prefix " + prefix + ": <" + context.FormatUri(context.Namespaces.GetNamespaceUri(prefix)) + ">.");
                    }
                }
                context.Output.WriteLine();
            }
            else
            {
                context.QNameMapper = new ThreadSafeQNameOutputMapper(new NamespaceMapper(true));
            }
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

        /// <summary>
        /// Generates the Output for a Graph as a String in TriG syntax
        /// </summary>
        /// <param name="globalContext">Context for writing the Store</param>
        /// <param name="context">Context for writing the Graph</param>
        /// <returns></returns>
        private String GenerateGraphOutput(TriGWriterContext globalContext, TurtleWriterContext context)
        {
            if (context.Graph.BaseUri != null)
            {
                //Named Graph
                String gname;
                String sep = (globalContext.N3CompatabilityMode) ? " = " : " ";
                if (globalContext.CompressionLevel > WriterCompressionLevel.None && globalContext.QNameMapper.ReduceToQName(context.Graph.BaseUri.AbsoluteUri, out gname))
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
                    context.Output.Write(this.GenerateNodeOutput(globalContext, context, t.Subject, QuadSegment.Subject));
                    context.Output.Write(' ');
                    context.Output.Write(this.GenerateNodeOutput(globalContext, context, t.Predicate, QuadSegment.Predicate));
                    context.Output.Write(' ');
                    context.Output.Write(this.GenerateNodeOutput(globalContext, context, t.Object, QuadSegment.Object));
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
                        temp = this.GenerateNodeOutput(globalContext, context, t.Subject, QuadSegment.Subject);
                        context.Output.Write(temp);
                        context.Output.Write(" ");
                        subjIndent = baseIndent + temp.Length + 1;
                        lastSubj = t.Subject;

                        //Write the first Predicate
                        temp = this.GenerateNodeOutput(globalContext, context, t.Predicate, QuadSegment.Predicate);
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
                        temp = this.GenerateNodeOutput(globalContext, context, t.Predicate, QuadSegment.Predicate);
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
                    context.Output.Write(this.GenerateNodeOutput(globalContext, context, t.Object, QuadSegment.Object));
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
        private String GenerateNodeOutput(TriGWriterContext globalContext, TurtleWriterContext context, INode n, QuadSegment segment)
        {
            switch (n.NodeType)
            {
                case NodeType.Blank:
                    if (segment == QuadSegment.Predicate) throw new RdfOutputException(WriterErrorMessages.BlankPredicatesUnserializable("TriG"));
                    break;

                case NodeType.GraphLiteral:
                    throw new RdfOutputException(WriterErrorMessages.GraphLiteralsUnserializable("TriG"));

                case NodeType.Literal:
                    if (segment == QuadSegment.Subject) throw new RdfOutputException(WriterErrorMessages.LiteralSubjectsUnserializable("TriG"));
                    if (segment == QuadSegment.Predicate) throw new RdfOutputException(WriterErrorMessages.LiteralPredicatesUnserializable("TriG"));
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
                Uri u = null;
                while (globalContext.TryGetNextUri(out u))
                {
                    //Get the Graph from the Store
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
                }
            }
#if !PORTABLE  // PCL has no Thread.Abort() method or ThreadAbortException
            catch (ThreadAbortException)
            {
                //We've been terminated, don't do anything
#if !SILVERLIGHT
                Thread.ResetAbort();
#endif
            }
#endif
            catch (Exception ex)
            {
                throw new RdfStorageException("Error in Threaded Writer in Thread ID " + Thread.CurrentThread.ManagedThreadId, ex);
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
