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

namespace VDS.RDF.Writing.Formatting
{
    /// <summary>
    /// Formatter for formatting as NTriples
    /// </summary>
    public class NTriplesFormatter
        : BaseFormatter
    {
        private BlankNodeOutputMapper _bnodeMapper = new BlankNodeOutputMapper(WriterHelper.IsValidStrictBlankNodeID);

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
            value = this.Escape(value, this._litEscapes);

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
