/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2015 dotNetRDF Project (dotnetrdf-develop@lists.sf.net)

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
        public TriGWriter()
        {
            N3CompatabilityMode = false;
            CompressionLevel = WriterCompressionLevel.Default;
            PrettyPrintMode = true;
            HighSpeedModePermitted = true;
        }

        /// <summary>
        /// Gets/Sets whether High Speed Write Mode is permitted
        /// </summary>
        public bool HighSpeedModePermitted { get; set; }

        /// <summary>
        /// Gets/Sets whether Pretty Printing is used
        /// </summary>
        public bool PrettyPrintMode { get; set; }

        /// <summary>
        /// Gets/Sets the Compression Level for the writer
        /// </summary>
        public int CompressionLevel { get; set; }

        /// <summary>
        /// Gets/Sets whether N3 Compatability Mode is used, in this mode an = is written after Graph Names so an N3 parser can read the TriG file correctly
        /// </summary>
        /// <remarks>
        /// Defaults to <strong>false</strong> from the 0.4.1 release onwards
        /// </remarks>
        public bool N3CompatabilityMode { get; set; }

        /// <summary>
        /// Saves a graph store in TriG (Turtle with Named Graphs) format
        /// </summary>
        /// <param name="store">Store to save</param>
        /// <param name="writer">Writer to save to</param>
        public override void Save(IGraphStore store, TextWriter writer)
        {
            if (store == null) throw new RdfOutputException("Cannot output a null Triple Store");
            if (writer == null) throw new RdfOutputException("Cannot output to a null writer");

            INamespaceMapper namespaces = WriterHelper.ExtractNamespaces(store);
            TriGWriterContext context = new TriGWriterContext(store, namespaces, writer, this.PrettyPrintMode, this.HighSpeedModePermitted, this.CompressionLevel, this.N3CompatabilityMode);

            //Write the Header of the File
            if (context.CompressionLevel > WriterCompressionLevel.None)
            {
                //Only add @prefix declarations if compression is enabled
                foreach (String prefix in namespaces.Prefixes)
                {
                    if (TurtleSpecsHelper.IsValidQName(prefix + ":"))
                    {
                        context.Output.WriteLine("@prefix " + prefix + ": <" + context.UriFormatter.FormatUri(namespaces.GetNamespaceUri(prefix)) + ">.");
                    }
                }
                context.Output.WriteLine();
            }
            if (context.CompressionLevel > WriterCompressionLevel.None)
            {
                context.NodeFormatter = new TurtleFormatter(context.QNameMapper);
            }
            else
            {
                context.NodeFormatter = new UncompressedTurtleFormatter();
            }

            try
            {
                foreach (INode graphName in context.GraphStore.GraphNames)
                {
                    context.CurrentGraphName = graphName;
                    context.CurrentGraph = context.GraphStore[graphName];
                    this.GenerateGraphOutput(context);
                }

                //Make sure to close the output
                context.Output.Close();
            }
            finally
            {
                context.Output.CloseQuietly();
            }
        }

        /// <summary>
        /// Generates the Output for a Graph as a String in TriG syntax
        /// </summary>
        /// <param name="globalContext">Context for writing the Store</param>
        /// <param name="context">Context for writing the Graph</param>
        /// <returns></returns>
        private String GenerateGraphOutput(TriGWriterContext context)
        {
            if (!ReferenceEquals(context.CurrentGraph, null))
            {
                //Named Graph
                String gname;
                String sep = (context.N3CompatabilityMode) ? " = " : " ";
                // TODO Support writing non-URI named graphs
                if (context.CompressionLevel > WriterCompressionLevel.None && context.QNameMapper.ReduceToPrefixedName(context.CurrentGraphName.Uri.AbsoluteUri, out gname))
                {
                    if (TurtleSpecsHelper.IsValidQName(gname))
                    {
                        context.Output.WriteLine(gname + sep + "{");
                    }
                    else
                    {
                        context.Output.WriteLine("<" + context.UriFormatter.FormatUri(context.CurrentGraphName.Uri) + ">" + sep + "{");
                    }
                }
                else
                {
                    context.Output.WriteLine("<" + context.UriFormatter.FormatUri(context.CurrentGraphName.Uri) + ">" + sep + "{");
                }
            }
            else
            {
                context.Output.WriteLine("{");
            }

            //Generate Triples
            this.GenerateTripleOutput(context);

            //Close the Graph
            context.Output.WriteLine("}");

            return context.Output.ToString();
        }

        /// <summary>
        /// Generates the Output for a Triple as a String in Turtle syntax
        /// </summary>
        /// <param name="globalContext">Context for writing the Store</param>
        /// <param name="context">Context for writing the Graph</param>
        private void GenerateTripleOutput(TriGWriterContext context)
        {
            //Decide which write mode to use
            bool hiSpeed = false;
            double subjNodes = context.CurrentGraph.Triples.Select(t => t.Subject).Distinct().Count();
            double triples = context.CurrentGraph.Count;
            if ((subjNodes / triples) > 0.75) hiSpeed = true;

            if (context.CompressionLevel == WriterCompressionLevel.None || hiSpeed && context.HighSpeedModePermitted)
            {
                //Use High Speed Write Mode
                String indentation = new String(' ', 4);
                context.Output.Write(indentation);
                if (context.CompressionLevel > WriterCompressionLevel.None) context.Output.WriteLine("# Written using High Speed Mode");
                foreach (Triple t in context.CurrentGraph.Triples)
                {
                    context.Output.Write(indentation);
                    context.Output.Write(context.NodeFormatter.Format(t.Subject, QuadSegment.Subject));
                    context.Output.Write(' ');
                    context.Output.Write(context.NodeFormatter.Format(t.Predicate, QuadSegment.Predicate));
                    context.Output.Write(' ');
                    context.Output.Write(context.NodeFormatter.Format(t.Object, QuadSegment.Object));
                    context.Output.WriteLine(".");
                }
            }
            else
            {
                //Get the Triples as a Sorted List
                List<Triple> ts = context.CurrentGraph.Triples.ToList();
                ts.Sort();

                //Variables we need to track our writing
                INode lastPred;
                INode lastSubj = lastPred = null;
                int subjIndent = 0, predIndent = 0;
                const int baseIndent = 4;

                foreach (Triple t in ts)
                {
                    String temp;
                    if (lastSubj == null || !t.Subject.Equals(lastSubj))
                    {
                        //Terminate previous Triples
                        if (lastSubj != null) context.Output.WriteLine(".");

                        if (context.PrettyPrint) context.Output.Write(new String(' ', baseIndent));

                        //Start a new set of Triples
                        temp = context.NodeFormatter.Format(t.Subject, QuadSegment.Subject);
                        context.Output.Write(temp);
                        context.Output.Write(" ");
                        subjIndent = baseIndent + temp.Length + 1;
                        lastSubj = t.Subject;

                        //Write the first Predicate
                        temp = context.NodeFormatter.Format(t.Predicate, QuadSegment.Predicate);
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
                        temp = context.NodeFormatter.Format(t.Predicate, QuadSegment.Predicate);
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
                    context.Output.Write(context.NodeFormatter.Format(t.Object, QuadSegment.Object));
                }

                //Terminate Triples
                if (ts.Count > 0) context.Output.WriteLine(".");               
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
