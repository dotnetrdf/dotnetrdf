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

namespace VDS.RDF.Writing.Formatting
{
    public abstract class BaseFormatter : INodeFormatter, ITripleFormatter
    {
        private String _format;

        public BaseFormatter(String formatName)
        {
            this._format = formatName;
        }

        /// <summary>
        /// Formats a Node as a String
        /// </summary>
        /// <param name="n">Node</param>
        /// <returns></returns>
        public virtual String Format(INode n)
        {
            switch (n.NodeType)
            {
                case NodeType.Blank:
                    return this.FormatBlankNode((BlankNode)n);
                case NodeType.GraphLiteral:
                    throw new NotSupportedException("Graph Literal Nodes cannot be formatted with this function");
                case NodeType.Literal:
                    return this.FormatLiteralNode((LiteralNode)n);
                case NodeType.Uri:
                    return this.FormatUriNode((UriNode)n);
                default:
                    throw new RdfOutputException(WriterErrorMessages.UnknownNodeTypeUnserializable(this._format));
            }
        }

        /// <summary>
        /// Formats a Triple as a String
        /// </summary>
        /// <param name="t">Triple</param>
        /// <returns></returns>
        public virtual String Format(Triple t)
        {
            return this.Format(t.Subject) + " " + this.Format(t.Predicate) + " " + this.Format(t.Object) + " .";
        }

        /// <summary>
        /// Formats a URI Node as a String for the given Format
        /// </summary>
        /// <param name="u">URI Node</param>
        /// <returns></returns>
        protected abstract String FormatUriNode(UriNode u);

        /// <summary>
        /// Formats a URI as a String for full Output
        /// </summary>
        /// <param name="u">URI</param>
        /// <returns></returns>
        protected virtual String FormatUri(String u)
        {
            String uri = Uri.EscapeUriString(u);
            uri = uri.Replace(">", "\\>");
            return uri;
        }

        /// <summary>
        /// Formats a URI as a String for full Output
        /// </summary>
        /// <param name="u">URI</param>
        /// <returns></returns>
        protected virtual String FormatUri(Uri u)
        {
            return this.FormatUri(u.ToString());
        }

        /// <summary>
        /// Formats a Literal Node as a String for the given Format
        /// </summary>
        /// <param name="l">Literal Node</param>
        /// <returns></returns>
        protected abstract String FormatLiteralNode(LiteralNode l);

        /// <summary>
        /// Formats a Blank Node as a String for the given Format
        /// </summary>
        /// <param name="b">Blank Node</param>
        /// <returns></returns>
        protected virtual String FormatBlankNode(BlankNode b)
        {
            return b.ToString();
        }

        /// <summary>
        /// Formats a Character for the given Format
        /// </summary>
        /// <param name="c">Character</param>
        /// <returns></returns>
        public virtual String FormatChar(char c)
        {
            return c.ToString();
        }
    }
}
