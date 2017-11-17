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
    /// Formatter for formatting as NTriples
    /// </summary>
    public class NTriplesFormatter
        : BaseFormatter
    {
        private readonly BlankNodeOutputMapper _bnodeMapper;

        /// <summary>
        /// Set of characters which must be escaped in Literals
        /// </summary>
        private readonly List<string[]> _litEscapes = new List<string[]>
        { 
            new [] { @"\", @"\\" }, 
            new [] { "\"", "\\\"" },
            new [] { "\n", @"\n" },
            new [] { "\r", @"\r" },
            new [] { "\t", @"\t" },
        };

        /// <summary>
        /// Creates a new NTriples formatter
        /// </summary>
        /// <param name="syntax">NTriples syntax to output</param>
        /// <param name="formatName">Format Name</param>
        public NTriplesFormatter(NTriplesSyntax syntax, string formatName)
            : base(formatName)
        {
            Syntax = syntax;
            switch (Syntax)
            {
                case NTriplesSyntax.Original:
                    _bnodeMapper = new BlankNodeOutputMapper(WriterHelper.IsValidStrictBlankNodeID);
                    break;
                default:
                    _bnodeMapper = new BlankNodeOutputMapper(WriterHelper.IsValidBlankNodeID);
                    break;
            }
        }

                /// <summary>
        /// Creates a new NTriples Formatter
        /// </summary>
        public NTriplesFormatter(NTriplesSyntax syntax)
            : this(syntax, GetName(syntax)) { }

        /// <summary>
        /// Creates a new NTriples Formatter
        /// </summary>
        public NTriplesFormatter()
            : this(NTriplesSyntax.Original, GetName()) { }

        /// <summary>
        /// Creates a new NTriples Formatter
        /// </summary>
        /// <param name="formatName">Format Name</param>
        protected NTriplesFormatter(string formatName)
            : this(NTriplesSyntax.Original, formatName) { }

        private static string GetName(NTriplesSyntax syntax = NTriplesSyntax.Original)
        {
            switch (syntax)
            {
                case NTriplesSyntax.Original:
                    return "NTriples";
                default:
                    return "NTriples (RDF 1.1)";
            }
        }

        /// <summary>
        /// Gets the NTriples syntax being used
        /// </summary>
        public NTriplesSyntax Syntax { get; private set; }

        /// <summary>
        /// Formats a URI Node
        /// </summary>
        /// <param name="u">URI Node</param>
        /// <param name="segment">Triple Segment</param>
        /// <returns></returns>
        protected override string FormatUriNode(IUriNode u, TripleSegment? segment)
        {
            var output = new StringBuilder();
            output.Append('<');
            output.Append(FormatUri(u.Uri));
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
            var output = new StringBuilder();

            output.Append('"');
            var value = l.Value;
            value = Escape(value, _litEscapes);
            output.Append(FormatChar(value.ToCharArray()));
            output.Append('"');

            if (!l.Language.Equals(string.Empty))
            {
                output.Append('@');
                output.Append(l.Language.ToLower());
            }
            else if (l.DataType != null)
            {
                output.Append("^^<");
                output.Append(FormatUri(l.DataType));
                output.Append('>');
            }

            return output.ToString();
        }

        /// <summary>
        /// Formats a Character
        /// </summary>
        /// <param name="c">Character</param>
        /// <returns></returns>
        [Obsolete("This form of the FormatChar() method is considered obsolete as it is inefficient", false)]
        public override string FormatChar(char c)
        {
            if (Syntax != NTriplesSyntax.Original) return base.FormatChar(c);
            if (c <= 127)
            {
                // ASCII
                return c.ToString();
            }
            // Small Unicode Escape required
            return "\\u" + ((int)c).ToString("X4");
        }

        /// <summary>
        /// Formats a sequence of characters as a String
        /// </summary>
        /// <param name="cs">Characters</param>
        /// <returns>String</returns>
        public override string FormatChar(char[] cs)
        {
            if (Syntax != NTriplesSyntax.Original) return base.FormatChar(cs);

            StringBuilder builder = new StringBuilder();
            int start = 0, length = 0;
            for (int i = 0; i < cs.Length; i++)
            {
                char c = cs[i];
                if (c <= 127)
                {
                    length++;
                }
                else
                {
                    builder.Append(cs, start, length);
                    start = i + 1;
                    length = 0;
                    builder.Append("\\u");
                    builder.Append(((int) c).ToString("X4"));
                }
            }
            if (length == cs.Length)
            {
                return new string(cs);
            }
            if (length > 0) builder.Append(cs, start, length);
            return builder.ToString();
        }

        /// <summary>
        /// Formats a Blank Node
        /// </summary>
        /// <param name="b">Blank Node</param>
        /// <param name="segment">Triple Segment</param>
        /// <returns></returns>
        protected override string FormatBlankNode(IBlankNode b, TripleSegment? segment)
        {
            return "_:" + _bnodeMapper.GetOutputID(b.InternalID);
        }

        private static readonly char[] IriEscapeChars = { '<', '>', '"', '{', '}', '|', '^', '`', '\\' };

        /// <inheritdoc/>
        public override string FormatUri(Uri u)
        {
            if (!u.IsAbsoluteUri)
            {
                throw new ArgumentException("IRIs to be formatted by the NTriplesFormatter must be absolute IRIs");
            }
            return FormatUri(u.ToString());
        }

        /// <inheritdoc />
        public override string FormatUri(string u)
        {
            var output = new StringBuilder();
            foreach (var c in u)
            {
                if (c <= 0x20 || c < 0x0061 && IriEscapeChars.Contains(c))
                {
                    output.AppendFormat("\\u{0:X4}", (ushort)c);
                }
                else
                {
                    output.Append(c);
                }
            }
            return output.ToString();
        }

    }

    /// <summary>
    /// Formatter for formatting as NTriples according to the RDF 1.1 specification
    /// </summary>
    public class NTriples11Formatter
        : NTriplesFormatter
    {
        /// <summary>
        /// Creaates a new formatter
        /// </summary>
        public NTriples11Formatter()
            : base(NTriplesSyntax.Rdf11) { }
    }
}
