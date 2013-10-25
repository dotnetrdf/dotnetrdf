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
using VDS.RDF.Graphs;
using VDS.RDF.Nodes;
using VDS.RDF.Query;

namespace VDS.RDF.Writing.Formatting
{
    /// <summary>
    /// Abstract Base Class for Formatters
    /// </summary>
    public abstract class BaseFormatter
        : INodeFormatter, ITripleFormatter, IUriFormatter, ICharFormatter
    {
        private String _format;

        /// <summary>
        /// Creates a new Formatter
        /// </summary>
        /// <param name="formatName">Format Name</param>
        public BaseFormatter(String formatName)
        {
            this._format = formatName;
        }

        /// <summary>
        /// Gets the Format Name
        /// </summary>
        public String FormatName
        {
            get
            {
                return this._format;
            }
        }

        /// <summary>
        /// Formats a Node as a String
        /// </summary>
        /// <param name="n">Node</param>
        /// <param name="segment">Triple Segment</param>
        /// <returns></returns>
        public virtual String Format(INode n, QuadSegment? segment)
        {
            switch (n.NodeType)
            {
                case NodeType.Blank:
                    return this.FormatBlankNode(n, segment);
                case NodeType.GraphLiteral:
                    return this.FormatGraphLiteralNode(n, segment);
                case NodeType.Literal:
                    return this.FormatLiteralNode(n, segment);
                case NodeType.Uri:
                    return this.FormatUriNode(n, segment);
                case NodeType.Variable:
                    return this.FormatVariableNode(n, segment);
                default:
                    throw new RdfOutputException(WriterErrorMessages.UnknownNodeTypeUnserializable(this._format));
            }
        }

        /// <summary>
        /// Formats a Node as a String
        /// </summary>
        /// <param name="n">Node</param>
        /// <returns></returns>
        public virtual String Format(INode n)
        {
            return this.Format(n, null);
        }

        /// <summary>
        /// Formats a Triple as a String
        /// </summary>
        /// <param name="t">Triple</param>
        /// <returns></returns>
        public virtual String Format(Triple t)
        {
            return this.Format(t.Subject, QuadSegment.Subject) + " " + this.Format(t.Predicate, QuadSegment.Predicate) + " " + this.Format(t.Object, QuadSegment.Object) + " .";
        }

        /// <summary>
        /// Formats a URI Node as a String for the given Format
        /// </summary>
        /// <param name="u">URI Node</param>
        /// <param name="segment">Triple Segment</param>
        /// <returns></returns>
        protected abstract string FormatUriNode(INode u, QuadSegment? segment);

        /// <summary>
        /// Formats a URI as a String for full Output
        /// </summary>
        /// <param name="u">URI</param>
        /// <returns></returns>
        public virtual String FormatUri(String u)
        {
            //String uri = Uri.EscapeUriString(u);
            u = u.Replace(">", "\\>");
            return u;
        }

        /// <summary>
        /// Formats a URI as a String for full Output
        /// </summary>
        /// <param name="u">URI</param>
        /// <returns></returns>
        public virtual String FormatUri(Uri u)
        {
            return this.FormatUri(u.AbsoluteUri);
        }

        /// <summary>
        /// Formats a Literal Node as a String for the given Format
        /// </summary>
        /// <param name="l">Literal Node</param>
        /// <param name="segment">Triple Segment</param>
        /// <returns></returns>
        protected abstract string FormatLiteralNode(INode l, QuadSegment? segment);

        /// <summary>
        /// Formats a Blank Node as a String for the given Format
        /// </summary>
        /// <param name="b">Blank Node</param>
        /// <param name="segment">Triple Segment</param>
        /// <returns></returns>
        protected virtual string FormatBlankNode(INode b, QuadSegment? segment)
        {
            if (segment == QuadSegment.Predicate) throw new RdfOutputException(WriterErrorMessages.BlankPredicatesUnserializable(this._format));
            return b.ToString();
        }

        /// <summary>
        /// Formats a Variable Node as a String for the given Format
        /// </summary>
        /// <param name="v">Variable Name</param>
        /// <param name="segment">Triple Segment</param>
        /// <returns></returns>
        protected virtual string FormatVariableNode(INode v, QuadSegment? segment)
        {
            throw new RdfOutputException(WriterErrorMessages.VariableNodesUnserializable(this._format));
        }

        /// <summary>
        /// Formats a Graph Literal Node as a String for the given Format
        /// </summary>
        /// <param name="glit">Graph Literal</param>
        /// <param name="segment">Triple Segment</param>
        /// <returns></returns>
        protected virtual string FormatGraphLiteralNode(INode glit, QuadSegment? segment)
        {
            throw new RdfOutputException(WriterErrorMessages.GraphLiteralsUnserializable(this._format));
        }

        /// <summary>
        /// Formats a sequence of characters as a String
        /// </summary>
        /// <param name="cs">Characters</param>
        /// <returns>String</returns>
        public virtual String FormatChar(char[] cs)
        {
            return new String(cs);
        }

        /// <summary>
        /// Applies escapes to the given value
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="escapes">Escapes</param>
        /// <returns>Escaped string</returns>
        protected String Escape(String value, List<String[]> escapes)
        {
            foreach (String[] escape in escapes)
            {
                if (escape.Length != 2) continue;
                value = value.Replace(escape[0], escape[1]);
            }
            return value;
        }

        /// <summary>
        /// Gets the Name of the Format this Format uses
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.FormatName;
        }
    }
}
