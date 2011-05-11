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
    /// <summary>
    /// Formatter for formatting as NTriples
    /// </summary>
    public class NTriplesFormatter : BaseFormatter
    {
        private BlankNodeOutputMapper _bnodeMapper = new BlankNodeOutputMapper(WriterHelper.IsValidBlankNodeID);

        /// <summary>
        /// Set of characters that may follow a backslash and are legal escapes
        /// </summary>
        protected char[] _validEscapes = new char[] { '\\', '"', 'n', 'r', 't' };
        /// <summary>
        /// Set of characters which must be escaped in Literals
        /// </summary>
        protected char[] _litEscapes = new char[] { '"', '\n', '\r', '\t' };

        /// <summary>
        /// Creates a new NTriples Formatter
        /// </summary>
        public NTriplesFormatter()
            : base("NTriples") { }

        /// <summary>
        /// Creates a new NTriples Formatter
        /// </summary>
        /// <param name="formatName">Format Name</param>
        protected NTriplesFormatter(String formatName)
            : base(formatName) { }

        /// <summary>
        /// Formats a URI Node
        /// </summary>
        /// <param name="u">URI Node</param>
        /// <param name="segment">Triple Segment</param>
        /// <returns></returns>
        protected override string FormatUriNode(IUriNode u, TripleSegment? segment)
        {
            StringBuilder output = new StringBuilder();
            output.Append('<');
            output.Append(this.FormatUri(u.Uri));
            output.Append('>');
            return output.ToString();
        }

        /// <summary>
        /// Formats a Literal Node
        /// </summary>
        /// <param name="l">Literal Node</param>
        /// <param name="segment">Triple Segment</param>
        /// <returns></returns>
        protected override string FormatLiteralNode(ILiteralNode l, TripleSegment? segment)
        {
            StringBuilder output = new StringBuilder();
            String value;

            output.Append('"');
            value = l.Value;

            if (!value.IsFullyEscaped(this._validEscapes, this._litEscapes))
            {
                //This first replace escapes all back slashes for good measure
                value = value.EscapeBackslashes(this._validEscapes);

                //Then these escape characters that can't occur in a NTriples literal
                value = value.Replace("\n", "\\n");
                value = value.Replace("\r", "\\r");
                value = value.Replace("\t", "\\t");
                value = value.Escape('"');

                //Then remove null character since it doesn't change the meaning of the Literal
                value = value.Replace("\0", "");
            }

            foreach (char c in value.ToCharArray())
            {
                output.Append(this.FormatChar(c));
            }
            output.Append('"');

            if (!l.Language.Equals(String.Empty))
            {
                output.Append('@');
                output.Append(l.Language.ToLower());
            }
            else if (l.DataType != null)
            {
                output.Append("^^<");
                foreach (char c in this.FormatUri(l.DataType))
                {
                    output.Append(this.FormatChar(c));
                }
                output.Append('>');
            }

            return output.ToString();
        }

        /// <summary>
        /// Formats a Character
        /// </summary>
        /// <param name="c">Character</param>
        /// <returns></returns>
        public override string FormatChar(char c)
        {
            if (c <= 127)
            {
                //ASCII
                return c.ToString();
            }
            else
            {
                if (c <= 65535)
                {
                    //Small Unicode Escape required
                    return "\\u" + ((int)c).ToString("X4");
                }
                else
                {
                    //Big Unicode Escape required
                    return "\\U" + ((int)c).ToString("X8");
                }
            }
        }

        /// <summary>
        /// Formats a Blank Node
        /// </summary>
        /// <param name="b">Blank Node</param>
        /// <param name="segment">Triple Segment</param>
        /// <returns></returns>
        protected override string FormatBlankNode(IBlankNode b, TripleSegment? segment)
        {
            return "_:" + this._bnodeMapper.GetOutputID(b.InternalID);
        }

        /// <summary>
        /// Formats a URI
        /// </summary>
        /// <param name="u">URI</param>
        /// <returns></returns>
        public override string FormatUri(string u)
        {
            String temp = base.FormatUri(u);
            StringBuilder output = new StringBuilder();
            foreach (char c in temp.ToCharArray())
            {
                output.Append(this.FormatChar(c));
            }
            return output.ToString();
        }
    }
}
