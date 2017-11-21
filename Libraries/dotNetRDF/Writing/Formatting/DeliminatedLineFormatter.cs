/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
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
            _deliminatorChar = deliminator;
            _escapeChar = escape;
            _uriStartChar = uriStartChar;
            _uriEndChar = uriEndChar;
            _literalWrapperChar = literalWrapperChar;
            _longLiteralWrapperChar = longLiteralWrapperChar;
            _lineEndChar = lineEndChar;
            _fullLiteralOutput = fullLiteralOutput;

            _delimEscapes = new List<string[]>();
            _delimEscapes.Add(new String[] { new String(new char[] { _deliminatorChar }), new String(new char[] { _escapeChar, _deliminatorChar }) });
            _delimEscapes.Add(new String[] { new String(new char[] { '\n' }), new String(new char[] { _escapeChar, 'n' }) });
            _delimEscapes.Add(new String[] { new String(new char[] { '\r' }), new String(new char[] { _escapeChar, 'r' }) });
            _delimEscapes.Add(new String[] { new String(new char[] { '\t' }), new String(new char[] { _escapeChar, 't' }) });

            // TODO: Need to handle difference between standard and long literals better
            if (_literalWrapperChar.HasValue)
            {
                _delimEscapes.Add(new String[] { new String(new char[] { _literalWrapperChar.Value }), new String(new char[] { _escapeChar, _literalWrapperChar.Value }) });
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
            output.Append(Format(t.Subject));
            output.Append(_deliminatorChar);
            output.Append(Format(t.Predicate));
            output.Append(_deliminatorChar);
            output.Append(Format(t.Object));
            if (_lineEndChar != null)
            {
                output.Append(_lineEndChar);
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
            if (_uriStartChar != null) output.Append(_uriStartChar);
            if (_uriEndChar != null)
            {
                output.Append(FormatUri(u.Uri));
                output.Append(_uriEndChar);
            }
            else
            {
                output.Append(FormatUri(u.Uri));
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
            if (TurtleSpecsHelper.IsValidPlainLiteral(lit.Value, lit.DataType, TurtleSyntax.Original))
            {
                output.Append(lit.Value);
            }
            else
            {
                String value = lit.Value;

                if (TurtleSpecsHelper.IsLongLiteral(value))
                {
                    value = Escape(value, _delimEscapes);

                    // If there are no wrapper characters then we must escape the deliminator
                    if (value.Contains(_deliminatorChar))
                    {
                        if (_literalWrapperChar == null && _longLiteralWrapperChar == null)
                        {
                            // Replace the deliminator
                            value = value.Replace(new String(new char[] { _deliminatorChar }), new String(new char[] { _escapeChar, _deliminatorChar }));
                        }
                    }

                    // Apply appropriate wrapper characters
                    if (_longLiteralWrapperChar != null)
                    {
                        output.Append(_longLiteralWrapperChar + value + _longLiteralWrapperChar);
                    }
                    else if (_literalWrapperChar != null)
                    {
                        output.Append(_literalWrapperChar + value + _literalWrapperChar);
                    }
                    else
                    {
                        output.Append(value);
                    }
                }
                else
                {
                    // Replace the deliminator
                    value = Escape(value, _delimEscapes);

                    // Apply appropriate wrapper characters
                    if (_literalWrapperChar != null)
                    {
                        output.Append(_literalWrapperChar + value + _literalWrapperChar);
                    }
                    else
                    {
                        output.Append(value);
                    }
                }

                if (_fullLiteralOutput)
                {
                    if (!lit.Language.Equals(String.Empty))
                    {
                        output.Append("@" + lit.Language.ToLower());
                    }
                    else if (lit.DataType != null)
                    {
                        output.Append("^^");
                        if (_uriStartChar != null) output.Append(_uriStartChar);
                        if (_uriEndChar != null)
                        {
                            output.Append(FormatUri(lit.DataType));
                            output.Append(_uriEndChar);
                        }
                        else
                        {
                            output.Append(FormatUri(lit.DataType));
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
            if (_uriEndChar != null)
            {
                return u.Replace(new String(new char[] { (char)_uriEndChar }), new String(new char[] { _escapeChar, (char)_uriEndChar }));
            }
            else
            {
                return u;
            }
        }
    }
}
