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
using VDS.RDF.Parsing;
using VDS.RDF.Writing.Contexts;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Writing
{
    /// <summary>
    /// Class for generating RDF in Turtle Syntax
    /// </summary>
    /// <remarks>
    /// Similar in speed to the <see cref="CompressingTurtleWriter">CompressingTurtleWriter</see> but doesn't use the full Blank Node and Collection syntax compressions
    /// </remarks>
    /// <threadsafety instance="true">Designed to be Thread Safe - should be able to call the Save() method from multiple threads on different Graphs without issue</threadsafety>
    public class TurtleWriter 
        : IRdfWriter, IPrettyPrintingWriter, IHighSpeedWriter, IFormatterBasedWriter
    {
        private bool _prettyprint = true;
        private bool _allowhispeed = false;

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
        /// Gets/Sets whether the Writer is allowed to use High Speed write mode
        /// </summary>
        /// <remarks>High Speed Write Mode is engaged when the Writer determines that the contents of the Graph are not well suited to Turtle syntax compressions.  Usually the writer compresses triples into groups by Subject using Predicate-Object lists to output the Triples relating to each Subject.  If the number of distinct Subjects is greater than 75% of the Triples in the Graph then High Speed write mode will be used, in High Speed mode all Triples are written fully and no grouping of any sort is done.</remarks>
        public bool HighSpeedModePermitted
        {
            get
            {
                return this._allowhispeed;
            }
            set
            {
                this._allowhispeed = value;
            }
        }

        /// <summary>
        /// Gets the type of the Triple Formatter used by this writer
        /// </summary>
        public Type TripleFormatterType
        {
            get
            {
                return typeof(TurtleFormatter);
            }
        }

        /// <summary>
        /// Saves a Graph to a File
        /// </summary>
        /// <param name="g">Graph to save</param>
        /// <param name="filename">Filename to save to</param>
        public void Save(IGraph g, string filename)
        {
            this.Save(g, new StreamWriter(filename, false, new UTF8Encoding(Options.UseBomForUtf8)));
        }

        /// <summary>
        /// Saves a Graph using an arbitrary <see cref="TextWriter">TextWriter</see>
        /// </summary>
        /// <param name="g">Graph to save</param>
        /// <param name="output">Writer to save using</param>
        public void Save(IGraph g, TextWriter output)
        {
            try
            {
                TurtleWriterContext context = new TurtleWriterContext(g, output, this._prettyprint, this._allowhispeed);
                this.GenerateOutput(context);
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
                    //No Catch Actions - just trying to clean up
                }
                throw;
            }
        }

        /// <summary>
        /// Generates the Output for a Graph
        /// </summary>
        /// <param name="context">Context for writing the Graph</param>
        private void GenerateOutput(TurtleWriterContext context)
        {
            //Write Base Uri
            if (context.Graph.BaseUri != null)
            {
                context.Output.WriteLine("@base <" + context.UriFormatter.FormatUri(context.Graph.BaseUri) + ">.");
                context.Output.WriteLine();
            }

            //Write Prefixes
            foreach (String prefix in context.Graph.NamespaceMap.Prefixes)
            {
                if (TurtleSpecsHelper.IsValidQName(prefix + ":"))
                {
                    context.Output.Write("@prefix " + prefix + ": <");
                    String nsUri = context.UriFormatter.FormatUri(context.Graph.NamespaceMap.GetNamespaceUri(prefix));
                    context.Output.WriteLine(nsUri + ">.");
                }
            }
            context.Output.WriteLine();

            //Decide which write mode to use
            bool hiSpeed = false;
            double subjNodes = context.Graph.Triples.SubjectNodes.Count();
            double triples = context.Graph.Triples.Count;
            if ((subjNodes / triples) > 0.75) hiSpeed = true;

            if (hiSpeed && context.HighSpeedModePermitted)
            {
                //High Speed Writing Mode
                //Writes everything as individual Triples
                this.RaiseWarning("High Speed Write Mode in use - minimal syntax compressions will be used");
                context.NodeFormatter = new UncompressedTurtleFormatter();
                foreach (Triple t in context.Graph.Triples)
                {
                    context.Output.Write(this.GenerateNodeOutput(context, t.Subject, TripleSegment.Subject));
                    context.Output.Write(" ");
                    context.Output.Write(this.GenerateNodeOutput(context, t.Predicate, TripleSegment.Predicate));
                    context.Output.Write(" ");
                    context.Output.Write(this.GenerateNodeOutput(context, t.Object, TripleSegment.Object));
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
                String temp;

                for (int i = 0; i < ts.Count; i++)
                {
                    Triple t = ts[i];
                    if (lastSubj == null || !t.Subject.Equals(lastSubj))
                    {
                        //Terminate previous Triples
                        if (lastSubj != null) context.Output.WriteLine(".");

                        //Start a new set of Triples
                        temp = this.GenerateNodeOutput(context, t.Subject, TripleSegment.Subject);
                        context.Output.Write(temp);
                        context.Output.Write(" ");
                        subjIndent = temp.Length + 1;
                        lastSubj = t.Subject;

                        //Write the first Predicate
                        temp = this.GenerateNodeOutput(context, t.Predicate, TripleSegment.Predicate);
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
                        temp = this.GenerateNodeOutput(context, t.Predicate, TripleSegment.Predicate);
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
                    context.Output.Write(this.GenerateNodeOutput(context, t.Object, TripleSegment.Object));
                }

                //Terminate Triples
                if (ts.Count > 0) context.Output.WriteLine(".");
            }
        }

        /// <summary>
        /// Generates the Output for a Node in Turtle Syntax
        /// </summary>
        /// <param name="context">Context for writing the Graph</param>
        /// <param name="n">Node to generate Output for</param>
        /// <param name="segment">Segment of the Triple being written</param>
        /// <returns></returns>
        private String GenerateNodeOutput(TurtleWriterContext context, INode n, TripleSegment segment)
        {
            switch (n.NodeType)
            {
                case NodeType.Blank:
                    if (segment == TripleSegment.Predicate) throw new RdfOutputException(WriterErrorMessages.BlankPredicatesUnserializable("Turtle"));
                    return context.NodeFormatter.Format(n, segment);

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
        }

        /// <summary>
        /// Helper method for raising the <see cref="Warning">Warning</see> event
        /// </summary>
        /// <param name="message">Warning Message</param>
        private void RaiseWarning(String message)
        {
            RdfWriterWarning d = this.Warning;
            if (d != null)
            {
                d(message);
            }
        }

        /// <summary>
        /// Event which is raised when a non-fatal issue with the Graph being serialized is encountered
        /// </summary>
        public event RdfWriterWarning Warning;

        /// <summary>
        /// Gets the String representation of the writer which is a description of the syntax it produces
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "Turtle";
        }
    }
}
