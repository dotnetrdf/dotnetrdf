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
using System.Diagnostics;
using VDS.RDF.Graphs;
using VDS.RDF.Nodes;
using VDS.RDF.Writing.Contexts;

namespace VDS.RDF.Writing
{
    /// <summary>
    /// A Writer which generates GraphViz DOT Format files from an RDF Graph
    /// </summary>
    public class GraphVizWriter 
        : BaseGraphWriter
    {
        /// <summary>
        /// Saves a Graph into GraphViz DOT Format
        /// </summary>
        /// <param name="g">Graph to save</param>
        /// <param name="output">Stream to save to</param>
        public override void Save(IGraph g, TextWriter output)
        {
            //Start the Graph
            output.WriteLine("digraph G {");

            BaseWriterContext context = new BaseWriterContext(g.Namespaces, output);

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
                return this.UriNodeToDot(n, context);
            }
            else if (n.NodeType == NodeType.Literal)
            {
                return this.LiteralNodeToDot(n);
            }
            else if (n.NodeType == NodeType.Blank)
            {
                return this.BlankNodeToDot(n);
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
        private String UriNodeToDot(INode u, BaseWriterContext context)
        {
            StringBuilder output = new StringBuilder();
            output.Append("\"");

            //Try QName reduction
            String qname;
            if (context.QNameMapper.ReduceToPrefixedName(u.Uri.AbsoluteUri, out qname))
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
        private String BlankNodeToDot(INode b)
        {
            //Generate a _: QName
            StringBuilder output = new StringBuilder();
            output.Append("\"_:");
            output.Append(b.AnonID);
            output.Append("\"");

            return output.ToString();
        }

        /// <summary>
        /// Internal Helper Method for converting Literal Nodes to DOT notation
        /// </summary>
        /// <param name="l">Literal Node to convert</param>
        /// <returns></returns>
        private String LiteralNodeToDot(INode l)
        {
            StringBuilder output = new StringBuilder();
            output.Append("\"");

            //Escape any quotes and newlines in the value
            String value = l.Value.Replace("\"", "\\\"");
            value = value.Replace("\n", "\\n");

            output.Append(value);

            if (l.HasLanguage)
            {
                output.Append("@");
                output.Append(l.Language);
            }
            else if (l.HasDataType)
            {
                output.Append("^^");
                output.Append(l.DataType.AbsoluteUri);
            }

            output.Append("\"");

            return output.ToString();
        }

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
