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
using VDS.RDF.Query;
using VDS.RDF.Writing.Contexts;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Writing
{

    /// <summary>
    /// Class for generating Turtle Concrete RDF Syntax which provides varying levels of Syntax Compression
    /// </summary>
    /// <remarks>
    /// Similar in speed to the standard <see cref="TurtleWriter">TurtleWriter</see> but capable of using more syntax compressions depending on the Compression level set
    /// </remarks>
    /// <threadsafety instance="true">Designed to be Thread Safe - should be able to call the Save() method from multiple threads on different Graphs without issue</threadsafety>
    public class CompressingTurtleWriter 
        : IRdfWriter, IPrettyPrintingWriter, IHighSpeedWriter, ICompressingWriter, INamespaceWriter, IFormatterBasedWriter
    {
        private bool _prettyprint = true;
        private bool _allowHiSpeed = true;
        private int _compressionLevel = WriterCompressionLevel.Default;
        private INamespaceMapper _defaultNamespaces = new NamespaceMapper();

        /// <summary>
        /// Creates a new Compressing Turtle Writer which uses the Default Compression Level
        /// </summary>
        public CompressingTurtleWriter()
        {

        }

        /// <summary>
        /// Creates a new Compressing Turtle Writer which uses the given Compression Level
        /// </summary>
        /// <param name="compressionLevel">Desired Compression Level</param>
        /// <remarks>See Remarks for this classes <see cref="CompressingTurtleWriter.CompressionLevel">CompressionLevel</see> property to see what effect different compression levels have</remarks>
        public CompressingTurtleWriter(int compressionLevel)
        {
            this._compressionLevel = compressionLevel;
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
        /// Gets/Sets whether High Speed Write Mode should be allowed
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
        /// Gets/Sets the Compression Level to be used
        /// </summary>
        /// <remarks>
        /// <para>
        /// If the Compression Level is set to <see cref="WriterCompressionLevels.None">None</see> then High Speed mode will always be used regardless of the input Graph and the <see cref="CompressingTurtleWriter.HighSpeedModePermitted">HighSpeedMorePermitted</see> property.
        /// </para>
        /// <para>
        /// If the Compression Level is set to <see cref="WriterCompressionLevels.Minimal">Minimal</see> or above then full Predicate Object lists will be used for Triples.
        /// </para>
        /// <para>
        /// If the Compression Level is set to <see cref="WriterCompressionLevels.More">More</see> or above then Blank Node Collections and Collection syntax will be used if the Graph contains Triples that can be compressed in that way.</para>
        /// </remarks>
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
        /// Gets/Sets the Default Namespaces that are always available
        /// </summary>
        public INamespaceMapper DefaultNamespaces
        {
            get
            {
                return this._defaultNamespaces;
            }
            set
            {
                this._defaultNamespaces = value;
            }
        }

        /// <summary>
        /// Gets the type of the Triple Formatter used by the writer
        /// </summary>
        public Type TripleFormatterType
        {
            get
            {
                return typeof(TurtleFormatter);
            }
        }

        /// <summary>
        /// Saves a Graph to a file using Turtle Syntax
        /// </summary>
        /// <param name="g">Graph to save</param>
        /// <param name="filename">File to save to</param>
        public void Save(IGraph g, string filename)
        {
            this.Save(g, new StreamWriter(filename, false, new UTF8Encoding(Options.UseBomForUtf8)));
        }

        /// <summary>
        /// Saves a Graph to the given Stream using Turtle Syntax
        /// </summary>
        /// <param name="g">Graph to save</param>
        /// <param name="output">Stream to save to</param>
        public void Save(IGraph g, TextWriter output)
        {
            try
            {
                //Create the Writing Context
                g.NamespaceMap.Import(this._defaultNamespaces);
                CompressingTurtleWriterContext context = new CompressingTurtleWriterContext(g, output, this._compressionLevel, this._prettyprint, this._allowHiSpeed);
                this.GenerateOutput(context);
            }
            finally
            {
                try
                {
                    output.Close();
                }
                catch
                {
                    //No Catch actions - just trying to clean up
                }
            }
        }

        /// <summary>
        /// Generates the Turtle Syntax for the Graph
        /// </summary>
        private void GenerateOutput(CompressingTurtleWriterContext context)
        {
            //Create the Header
            //Base Directive
            if (context.Graph.BaseUri != null)
            {
                context.Output.WriteLine("@base <" + context.UriFormatter.FormatUri(context.Graph.BaseUri) + ">.");
                context.Output.WriteLine();
            }
            //Prefix Directives
            foreach (String prefix in context.Graph.NamespaceMap.Prefixes)
            {
                if (TurtleSpecsHelper.IsValidQName(prefix + ":"))
                {
                    if (!prefix.Equals(String.Empty))
                    {
                        context.Output.WriteLine("@prefix " + prefix + ": <" + context.UriFormatter.FormatUri(context.Graph.NamespaceMap.GetNamespaceUri(prefix)) + ">.");
                    }
                    else
                    {
                        context.Output.WriteLine("@prefix : <" + context.UriFormatter.FormatUri(context.Graph.NamespaceMap.GetNamespaceUri(String.Empty)) + ">.");
                    }
                }
            }
            context.Output.WriteLine();

            //Decide on the Write Mode to use
            bool hiSpeed = false;
            double subjNodes = context.Graph.Triples.SubjectNodes.Count();
            double triples = context.Graph.Triples.Count;
            if ((subjNodes / triples) > 0.75) hiSpeed = true;

            if (context.CompressionLevel == WriterCompressionLevel.None || (hiSpeed && context.HighSpeedModePermitted))
            {
                this.RaiseWarning("High Speed Write Mode in use - minimal syntax compression will be used");
                context.CompressionLevel = WriterCompressionLevel.Minimal;
                context.NodeFormatter = new UncompressedTurtleFormatter();

                foreach (Triple t in context.Graph.Triples)
                {
                    context.Output.WriteLine(this.GenerateTripleOutput(context, t));
                }
            }
            else
            {
                if (context.CompressionLevel >= WriterCompressionLevel.More)
                {
                    WriterHelper.FindCollections(context);
                }

                //Get the Triples as a Sorted List
                List<Triple> ts = context.Graph.Triples.Where(t => !context.TriplesDone.Contains(t)).ToList();
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
                        temp = this.GenerateNodeOutput(context, t.Subject, TripleSegment.Subject, 0);
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

                        //Write the first Predicate
                        temp = this.GenerateNodeOutput(context, t.Predicate, TripleSegment.Predicate, subjIndent);
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
                        temp = this.GenerateNodeOutput(context, t.Predicate, TripleSegment.Predicate, subjIndent);
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
                    context.Output.Write(this.GenerateNodeOutput(context, t.Object, TripleSegment.Object, subjIndent + predIndent));
                }

                //Terminate Triples
                if (ts.Count > 0) context.Output.WriteLine(".");

                return;
            }
            
        }

        /// <summary>
        /// Generates Output for Triples as a single "s p o." Triple
        /// </summary>
        /// <param name="context">Writer Context</param>
        /// <param name="t">Triple to output</param>
        /// <returns></returns>
        /// <remarks>Used only in High Speed Write Mode</remarks>
        private String GenerateTripleOutput(CompressingTurtleWriterContext context, Triple t)
        {
            StringBuilder temp = new StringBuilder();
            temp.Append(this.GenerateNodeOutput(context, t.Subject, TripleSegment.Subject, 0));
            temp.Append(' ');
            temp.Append(this.GenerateNodeOutput(context, t.Predicate, TripleSegment.Predicate, 0));
            temp.Append(' ');
            temp.Append(this.GenerateNodeOutput(context, t.Object, TripleSegment.Object, 0));
            temp.Append('.');

            return temp.ToString();
        }

        /// <summary>
        /// Generates Output for Nodes in Turtle syntax
        /// </summary>
        /// <param name="context">Writer Context</param>
        /// <param name="n">Node to generate output for</param>
        /// <param name="segment">Segment of the Triple being written</param>
        /// <param name="indent">Indentation</param>
        /// <returns></returns>
        private String GenerateNodeOutput(CompressingTurtleWriterContext context, INode n, TripleSegment segment, int indent)
        {
            StringBuilder output = new StringBuilder();

            switch (n.NodeType)
            {
                case NodeType.Blank:
                    if (segment == TripleSegment.Predicate) throw new RdfOutputException(WriterErrorMessages.BlankPredicatesUnserializable("Turtle"));

                    if (context.Collections.ContainsKey(n))
                    {
                        output.Append(this.GenerateCollectionOutput(context, context.Collections[n], indent));
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

                default:
                    throw new RdfOutputException(WriterErrorMessages.UnknownNodeTypeUnserializable("Turtle"));
            }

            return output.ToString();
        }

        /// <summary>
        /// Internal Helper method which converts a Collection into Turtle Syntax
        /// </summary>
        /// <param name="context">Writer Context</param>
        /// <param name="c">Collection to convert</param>
        /// <param name="indent">Indentation</param>
        /// <returns></returns>
        private String GenerateCollectionOutput(CompressingTurtleWriterContext context, OutputRdfCollection c, int indent)
        {
            StringBuilder output = new StringBuilder();
            bool first = true;

            if (!c.IsExplicit)
            {
                output.Append('(');

                while (c.Triples.Count > 0)
                {
                    if (context.PrettyPrint && !first) output.Append(new String(' ', indent));
                    first = false;
                    output.Append(this.GenerateNodeOutput(context, c.Triples.First().Object, TripleSegment.Object, indent));
                    c.Triples.RemoveAt(0);
                    if (c.Triples.Count > 0)
                    {
                        output.Append(' ');
                    }
                }

                output.Append(')');
            }
            else
            {
                if (c.Triples.Count == 0)
                {
                    //Empty Collection
                    //Can represent as a single Blank Node []
                    output.Append("[]");
                }
                else
                {
                    output.Append('[');

                    while (c.Triples.Count > 0)
                    {
                        if (context.PrettyPrint && !first) output.Append(new String(' ', indent));
                        first = false;
                        String temp = this.GenerateNodeOutput(context, c.Triples.First().Predicate, TripleSegment.Predicate, indent);
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
                        output.Append(this.GenerateNodeOutput(context, c.Triples.First().Object, TripleSegment.Object, indent + 2 + addIndent));
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

        /// <summary>
        /// Helper method for generating Parser Warning Events
        /// </summary>
        /// <param name="message">Warning Message</param>
        private void RaiseWarning(String message)
        {
            if (this.Warning != null)
            {
                this.Warning(message);
            }
        }

        /// <summary>
        /// Event which is raised when there is a non-fatal issue with the Graph being written
        /// </summary>
        public event RdfWriterWarning Warning;

        /// <summary>
        /// Gets the String representation of the writer which is a description of the syntax it produces
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "Turtle (Compressing Writer)";
        }
    }
}
