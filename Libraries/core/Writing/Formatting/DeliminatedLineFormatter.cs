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
using VDS.RDF.Parsing;

namespace VDS.RDF.Writing.Formatting
{
    /// <summary>
    /// Abstract Base Class for formatters where things are formatted as lines of plain text deliminated by specific characters
    /// </summary>
    public abstract class DeliminatedLineFormatter 
        : BaseFormatter
    {
        private Nullable<char> _uriStartChar, _uriEndChar, _literalWrapperChar, _longLiteralWrapperChar, _lineEndChar;
        private char _deliminatorChar = ' ';
        private char _escapeChar = '\\';
        private bool _fullLiteralOutput = true;

        //REQ: Improve support for escape characters

        /// <summary>
        /// Creates a new Deliminated Line Formatter
        /// </summary>
        /// <param name="formatName">Format Name</param>
        /// <param name="deliminator">Item Deliminator Character</param>
        /// <param name="escape">Escape Character</param>
        /// <param name="uriStartChar">Character to start URIs (may be null)</param>
        /// <param name="uriEndChar">Character to end URIs (may be null)</param>
        /// <param name="literalWrapperChar">Character to wrap Literals in (may be null)</param>
        /// <param name="longLiteralWrapperChar">Character to wrap Long Literals in (may be null)</param>
        /// <param name="lineEndChar">Character to add at end of line (may be null)</param>
        /// <param name="fullLiteralOutput">Whether Literals are output with Language/Datatype information</param>
        public DeliminatedLineFormatter(String formatName, char deliminator, char escape, Nullable<char> uriStartChar, Nullable<char> uriEndChar, Nullable<char> literalWrapperChar, Nullable<char> longLiteralWrapperChar, Nullable<char> lineEndChar, bool fullLiteralOutput)
            : base(formatName)
        {
            this._deliminatorChar = deliminator;
            this._escapeChar = escape;
            this._uriStartChar = uriStartChar;
            this._uriEndChar = uriEndChar;
            this._literalWrapperChar = literalWrapperChar;
            this._longLiteralWrapperChar = longLiteralWrapperChar;
            this._lineEndChar = lineEndChar;
            this._fullLiteralOutput = fullLiteralOutput;
        }

        /// <summary>
        /// Formats a Triple
        /// </summary>
        /// <param name="t">Triple</param>
        /// <returns></returns>
        public override string Format(Triple t)
        {
            StringBuilder output = new StringBuilder();
            output.Append(this.Format(t.Subject));
            output.Append(this._deliminatorChar);
            output.Append(this.Format(t.Predicate));
            output.Append(this._deliminatorChar);
            output.Append(this.Format(t.Object));
            if (this._lineEndChar != null)
            {
                output.Append(this._lineEndChar);
            }
            return output.ToString();
        }

        /// <summary>
        /// Formats a URI Node
        /// </summary>
        /// <param name="u">URI Node</param>
        /// <param name="segment">Triple Segment</param>
        /// <returns></returns>
        protected override string FormatUriNode(IUriNode u, TripleSegment? segment)
        {
            StringBuilder output = new StringBuilder();
            if (this._uriStartChar != null) output.Append(this._uriStartChar);
            if (this._uriEndChar != null)
            {
                output.Append(this.FormatUri(u.Uri));
                output.Append(this._uriEndChar);
            }
            else
            {
                output.Append(this.FormatUri(u.Uri));
            }
            return output.ToString();
        }

        /// <summary>
        /// Formats a Literal Node
        /// </summary>
        /// <param name="lit">Literal Node</param>
        /// <param name="segment">Triple Segment</param>
        /// <returns></returns>
        protected override string FormatLiteralNode(ILiteralNode lit, TripleSegment? segment)
        {
            StringBuilder output = new StringBuilder();
            if (TurtleSpecsHelper.IsValidPlainLiteral(lit.Value, lit.DataType))
            {
                output.Append(lit.Value);
            }
            else
            {
                String value = lit.Value;

                if (TurtleSpecsHelper.IsLongLiteral(value))
                {
                    value = value.Replace("\n", "\\n");
                    value = value.Replace("\r", "\\r");
                    value = value.Escape('"');
                    value = value.Replace("\t", "\\t");

                    //If there are no wrapper characters then we must escape the deliminator
                    if (value.Contains(this._deliminatorChar))
                    {
                        if (this._literalWrapperChar == null && this._longLiteralWrapperChar == null)
                        {
                            //Replace the deliminator
                            value = value.Replace(new String(new char[] { this._deliminatorChar }), new String(new char[] { this._escapeChar, this._deliminatorChar }));
                        }
                    }

                    //Apply appropriate wrapper characters
                    if (this._longLiteralWrapperChar != null)
                    {
                        output.Append(this._longLiteralWrapperChar + value + this._longLiteralWrapperChar);
                    }
                    else if (this._literalWrapperChar != null)
                    {
                        output.Append(this._literalWrapperChar + value + this._literalWrapperChar);
                    }
                    else
                    {
                        output.Append(value);
                    }
                }
                else
                {
                    //Replace the deliminator
                    value = value.Replace(new String(new char[] { this._deliminatorChar }), new String(new char[] { this._escapeChar, this._deliminatorChar }));

                    //Apply appropriate wrapper characters
                    if (this._literalWrapperChar != null)
                    {
                        output.Append(this._literalWrapperChar + value + this._literalWrapperChar);
                    }
                    else
                    {
                        output.Append(value);
                    }
                }

                if (this._fullLiteralOutput)
                {
                    if (!lit.Language.Equals(String.Empty))
                    {
                        output.Append("@" + lit.Language.ToLower());
                    }
                    else if (lit.DataType != null)
                    {
                        output.Append("^^");
                        if (this._uriStartChar != null) output.Append(this._uriStartChar);
                        if (this._uriEndChar != null)
                        {
                            output.Append(this.FormatUri(lit.DataType));
                            output.Append(this._uriEndChar);
                        }
                        else
                        {
                            output.Append(this.FormatUri(lit.DataType));
                        }
                    }
                }
            }
            return output.ToString();
        }

        /// <summary>
        /// Formats URIs
        /// </summary>
        /// <param name="u"></param>
        /// <returns></returns>
        public override string FormatUri(String u)
        {
            if (this._uriEndChar != null)
            {
                return u.Replace(new String(new char[] { (char)this._uriEndChar }), new String(new char[] { this._escapeChar, (char)this._uriEndChar }));
            }
            else
            {
                return u;
            }
        }
    }
}
