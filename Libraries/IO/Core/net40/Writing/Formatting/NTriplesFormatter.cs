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
using VDS.RDF.Nodes;

namespace VDS.RDF.Writing.Formatting
{
    /// <summary>
    /// Formatter for formatting as NTriples
    /// </summary>
    public class NTriplesFormatter
        : BaseFormatter
    {
        private BlankNodeOutputMapper _bnodeMapper = new BlankNodeOutputMapper();

        /// <summary>
        /// Set of characters which must be escaped in Literals
        /// </summary>
        protected List<String[]> _litEscapes = new List<String[]>
        { 
            new String[] { @"\", @"\\" }, 
            new String[] { "\"", "\\\"" },
            new String[] { "\n", @"\n" },
            new String[] { "\r", @"\r" },
            new String[] { "\t", @"\t" }
        };

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
        protected override string FormatUriNode(INode u, QuadSegment? segment)
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
        protected override string FormatLiteralNode(INode l, QuadSegment? segment)
        {
            StringBuilder output = new StringBuilder();
            String value;

            output.Append('"');
            value = l.Value;
            value = this.Escape(value, this._litEscapes);
            output.Append(this.FormatChar(value.ToCharArray()));
            output.Append('"');

            if (!l.Language.Equals(String.Empty))
            {
                output.Append('@');
                output.Append(l.Language.ToLower());
            }
            else if (l.DataType != null)
            {
                output.Append("^^<");
                output.Append(this.FormatUri(l.DataType));
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
            if (c <= 127)
            {
                //ASCII
                return c.ToString();
            }
            else
            {
                //Small Unicode Escape required
                return "\\u" + ((int)c).ToString("X4");
            }
        }

        /// <summary>
        /// Formats a sequence of characters as a String
        /// </summary>
        /// <param name="cs">Characters</param>
        /// <returns>String</returns>
        public override string FormatChar(char[] cs)
        {
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
            else
            {
                if (length > 0) builder.Append(cs, start, length);
                return builder.ToString();
            }
        }

        /// <summary>
        /// Formats a Blank Node
        /// </summary>
        /// <param name="b">Blank Node</param>
        /// <param name="segment">Triple Segment</param>
        /// <returns></returns>
        protected override string FormatBlankNode(INode b, QuadSegment? segment)
        {
            return "_:" + this._bnodeMapper.GetOutputID(b.AnonID);
        }

        /// <summary>
        /// Formats a URI
        /// </summary>
        /// <param name="u">URI</param>
        /// <returns></returns>
        public override string FormatUri(string u)
        {
            String temp = base.FormatUri(u);
            return this.FormatChar(temp.ToCharArray());
        }
    }
}
