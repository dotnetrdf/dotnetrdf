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
using System.Diagnostics;
using VDS.RDF.Writing.Contexts;

namespace VDS.RDF.Writing
{
    /// <summary>
    /// A Writer which generates GraphViz DOT Format files from an RDF Graph
    /// </summary>
    public class GraphVizWriter : IRdfWriter
    {
        /// <summary>
        /// Saves a Graph into GraphViz DOT Format
        /// </summary>
        /// <param name="g">Graph to save</param>
        /// <param name="filename">File to save to</param>
        public void Save(IGraph g, string filename)
        {
            //Open the Stream for the File
            StreamWriter output = new StreamWriter(filename);

            //Call the other version of Save to do the actual work
            this.Save(g, output);
        }

        /// <summary>
        /// Saves a Graph into GraphViz DOT Format
        /// </summary>
        /// <param name="g">Graph to save</param>
        /// <param name="output">Stream to save to</param>
        public void Save(IGraph g, TextWriter output)
        {
            //Start the Graph
            output.WriteLine("digraph G {");

            BaseWriterContext context = new BaseWriterContext(g, output);

            //Write all the Triples to the Graph
            foreach (Triple t in g.Triples)
            {
                output.WriteLine(this.TripleToDot(t, context));
            }

            //End the Graph
            output.WriteLine("}");

            output.Close();
        }

        /// <summary>
        /// Internal Helper Method for converting a Triple into DOT notation
        /// </summary>
        /// <param name="t">Triple to convert</param>
        /// <param name="context">Writer Context</param>
        /// <returns></returns>
        private String TripleToDot(Triple t, BaseWriterContext context)
        {
            StringBuilder output = new StringBuilder();

            //Output Node lines for Literal Node so we show them as Boxes
            //This is in keeping with Standard Graph representation of RDF
            //Literals are shown in Boxes, Uri Nodes in ellipses (GraphViz's default shape)
            if (t.Subject.NodeType == NodeType.Literal)
            {
                output.Append(this.NodeToDot(t.Subject, context));
                output.Append(" [shape=box];\n");
            }
            if (t.Object.NodeType == NodeType.Literal)
            {
                output.Append(this.NodeToDot(t.Object, context));
                output.Append(" [shape=box];\n");
            }

            //Output the actual lines that state the relationship between the Nodes
            //We use the Predicate as the Label on the relationship
            output.Append(this.NodeToDot(t.Subject, context));
            output.Append(" -> ");
            output.Append(this.NodeToDot(t.Object, context));
            output.Append(" [label=");
            output.Append(this.NodeToDot(t.Predicate, context));
            output.Append("];");

            return output.ToString();
        }

        /// <summary>
        /// Internal Helper method for converting a Node into DOT notation
        /// </summary>
        /// <param name="n">Node to Convert</param>
        /// <param name="context">Writer Context</param>
        /// <returns></returns>
        /// <remarks>Currently Graphs containing Graph Literal Nodes cannot be converted</remarks>
        private String NodeToDot(INode n, BaseWriterContext context)
        {
            if (n.NodeType == NodeType.Uri)
            {
                return this.UriNodeToDot((IUriNode)n, context);
            }
            else if (n.NodeType == NodeType.Literal)
            {
                return this.LiteralNodeToDot((ILiteralNode)n);
            }
            else if (n.NodeType == NodeType.Blank)
            {
                return this.BlankNodeToDot((IBlankNode)n);
            }
            else if (n.NodeType == NodeType.GraphLiteral)
            {
                throw new RdfOutputException("Graphs containing Graph Literal Nodes cannot be converted into GraphViz DOT Format");
            }
            else
            {
                throw new RdfOutputException("Unknown Node Type cannot be converted into GraphViz DOT Format");
            }
        }

        /// <summary>
        /// Internal Helper method for converting Uri Nodes to DOT Notation
        /// </summary>
        /// <param name="u">Uri Node to convert</param>
        /// <param name="context">Writer Context</param>
        /// <returns></returns>
        private String UriNodeToDot(IUriNode u, BaseWriterContext context)
        {
            StringBuilder output = new StringBuilder();
            output.Append("\"");

            //Try QName reduction
            String qname;
            if (context.QNameMapper.ReduceToQName(u.Uri.ToString(), out qname))
            {
                //Use the QName
                output.Append(qname);
            }
            else
            {
                //Use the full Uri
                output.Append(u.Uri);
            }

            output.Append("\"");

            return output.ToString();
        }

        /// <summary>
        /// Internal Helper Method for converting Blank Nodes to DOT notation
        /// </summary>
        /// <param name="b">Blank Node to Convert</param>
        /// <returns></returns>
        private String BlankNodeToDot(IBlankNode b)
        {
            //Generate a _: QName
            StringBuilder output = new StringBuilder();
            output.Append("\"_:");
            output.Append(b.InternalID);
            output.Append("\"");

            return output.ToString();
        }

        /// <summary>
        /// Internal Helper Method for converting Literal Nodes to DOT notation
        /// </summary>
        /// <param name="l">Literal Node to convert</param>
        /// <returns></returns>
        private String LiteralNodeToDot(ILiteralNode l)
        {
            StringBuilder output = new StringBuilder();
            output.Append("\"");

            //Escape any quotes and newlines in the value
            String value = l.Value.Replace("\"", "\\\"");
            value = value.Replace("\n", "\\n");

            output.Append(value);

            if (!l.Language.Equals(String.Empty))
            {
                output.Append("@");
                output.Append(l.Language);
            }
            else if (!(l.DataType == null))
            {
                output.Append("^^");
                output.Append(l.DataType.ToString());
            }


            output.Append("\"");

            return output.ToString();
        }

        /// <summary>
        /// Internal Helper method which handles raising the Warning event if an Event Handler is registered to it
        /// </summary>
        /// <param name="message">Warning Message</param>
        private void OnWarning(String message)
        {
            RdfWriterWarning d = this.Warning;
            if (d != null)
            {
                d(message);
            }
        }

        /// <summary>
        /// Event that is raised if there is a potential problem with the RDF being output
        /// </summary>
        /// <remarks>Not used by this Writer</remarks>
        public event RdfWriterWarning Warning;

        /// <summary>
        /// Gets the String representation of the writer which is a description of the syntax it produces
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "GraphViz DOT";
        }
    }
}
