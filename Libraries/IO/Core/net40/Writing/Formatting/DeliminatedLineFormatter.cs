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
using VDS.RDF.Graphs;
using VDS.RDF.Nodes;
using VDS.RDF.Parsing;
using VDS.RDF.Specifications;

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

        private List<String[]> _delimEscapes;

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

            this._delimEscapes = new List<string[]>();
            this._delimEscapes.Add(new String[] { new String(new char[] { this._deliminatorChar }), new String(new char[] { this._escapeChar, this._deliminatorChar }) });
            this._delimEscapes.Add(new String[] { new String(new char[] { '\n' }), new String(new char[] { this._escapeChar, 'n' }) });
            this._delimEscapes.Add(new String[] { new String(new char[] { '\r' }), new String(new char[] { this._escapeChar, 'r' }) });
            this._delimEscapes.Add(new String[] { new String(new char[] { '\t' }), new String(new char[] { this._escapeChar, 't' }) });

            //TODO: Need to handle difference between standard and long literals better
            if (this._literalWrapperChar.HasValue)
            {
                this._delimEscapes.Add(new String[] { new String(new char[] { this._literalWrapperChar.Value }), new String(new char[] { this._escapeChar, this._literalWrapperChar.Value }) });
            }
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
        protected override string FormatUriNode(INode u, QuadSegment? segment)
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
        protected override string FormatLiteralNode(INode lit, QuadSegment? segment)
        {
            StringBuilder output = new StringBuilder();
            if (TurtleSpecsHelper.IsValidPlainLiteral(lit.Value, lit.DataType, TurtleSyntax.Original))
            {
                output.Append(lit.Value);
            }
            else
            {
                String value = lit.Value;

                if (TurtleSpecsHelper.IsLongLiteral(value))
                {
                    value = this.Escape(value, this._delimEscapes);

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
                    value = this.Escape(value, this._delimEscapes);

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
