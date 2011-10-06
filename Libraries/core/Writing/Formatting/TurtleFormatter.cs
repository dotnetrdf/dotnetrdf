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
using System.Text.RegularExpressions;
using VDS.RDF.Parsing;

namespace VDS.RDF.Writing.Formatting
{
    /// <summary>
    /// Formatter which formats Turtle without any compression
    /// </summary>
    public class UncompressedTurtleFormatter
        : NTriplesFormatter
    {
        /// <summary>
        /// Creates a new Uncompressed Turtle Formatter
        /// </summary>
        public UncompressedTurtleFormatter()
            : base("Turtle (Uncompressed)") 
        {
            this._validEscapes = new char[] { '"', 'n', 't', 'u', 'U', 'r', '\\', '0' };
        }

        /// <summary>
        /// Creates a new Uncompressed Formatter
        /// </summary>
        /// <param name="formatName">Format Name</param>
        protected UncompressedTurtleFormatter(String formatName)
            : base(formatName) 
        {
            this._validEscapes = new char[] { '"', 'n', 't', 'u', 'U', 'r', '\\', '0' };
        }

        /// <summary>
        /// Formats characters
        /// </summary>
        /// <param name="c">Character</param>
        /// <returns></returns>
        public override string FormatChar(char c)
        {
            return c.ToString();
        }
    }

    /// <summary>
    /// Formatter which formats Turtle with compression
    /// </summary>
    public class TurtleFormatter : QNameFormatter, IBaseUriFormatter
    {
        private BlankNodeOutputMapper _bnodeMapper = new BlankNodeOutputMapper(WriterHelper.IsValidBlankNodeID);
        /// <summary>
        /// Set of characters which are valid as escapes when preceded by a backslash
        /// </summary>
        protected char[] _validEscapes = new char[] { '"', 'n', 't', 'u', 'U', 'r', '\\', '0' };
        /// <summary>
        /// Set of characters that must be escaped for Long Literals
        /// </summary>
        protected char[] _longLitMustEscape = new char[] { '"' };
        /// <summary>
        /// Set of characters that must be escaped for Literals
        /// </summary>
        protected char[] _litMustEscape = new char[] { '"', '\n', '\r', '\t' };

        /// <summary>
        /// Creates a new Turtle Formatter
        /// </summary>
        public TurtleFormatter() 
            : base("Turtle", new QNameOutputMapper()) { }

        /// <summary>
        /// Creates a new Turtle Formatter that uses the given QName mapper
        /// </summary>
        /// <param name="qnameMapper">QName Mapper</param>
        public TurtleFormatter(QNameOutputMapper qnameMapper)
            : base("Turtle", qnameMapper) { }

        /// <summary>
        /// Creates a new Turtle Formatter for the given Graph
        /// </summary>
        /// <param name="g">Graph</param>
        public TurtleFormatter(IGraph g)
            : base("Turtle", new QNameOutputMapper(g.NamespaceMap)) { }

        /// <summary>
        /// Creates a new Turtle Formatter for the given Namespace Map
        /// </summary>
        /// <param name="nsmap">Namespace Map</param>
        public TurtleFormatter(INamespaceMapper nsmap)
            : base("Turtle", new QNameOutputMapper(nsmap)) { }

        /// <summary>
        /// Creates a new Turtle Formatter
        /// </summary>
        /// <param name="formatName">Format Name</param>
        protected TurtleFormatter(String formatName)
            : base(formatName, new QNameOutputMapper()) { }

        /// <summary>
        /// Creates a new Turtle Formatter
        /// </summary>
        /// <param name="formatName">Format Name</param>
        /// <param name="g">Graph</param>
        protected TurtleFormatter(String formatName, IGraph g)
            : base(formatName, new QNameOutputMapper(g.NamespaceMap)) { }

        /// <summary>
        /// Creates a new Turtle Formatter
        /// </summary>
        /// <param name="formatName">Format Name</param>
        /// <param name="nsmap">Namespace Map</param>
        protected TurtleFormatter(String formatName, INamespaceMapper nsmap)
            : base(formatName, new QNameOutputMapper(nsmap)) { }

        /// <summary>
        /// Creates a new Turtle Formatter
        /// </summary>
        /// <param name="formatName">Format Name</param>
        /// <param name="qnameMapper">QName Map</param>
        protected TurtleFormatter(String formatName, QNameOutputMapper qnameMapper)
            : base(formatName, qnameMapper) { }

        /// <summary>
        /// Formats a Literal Node as a String
        /// </summary>
        /// <param name="l">Literal Node</param>
        /// <param name="segment">Triple Segment</param>
        /// <returns></returns>
        protected override string FormatLiteralNode(ILiteralNode l, TripleSegment? segment)
        {
            StringBuilder output = new StringBuilder();
            String value, qname;
            bool longlit = false, plainlit = false;

            longlit = TurtleSpecsHelper.IsLongLiteral(l.Value);
            plainlit = TurtleSpecsHelper.IsValidPlainLiteral(l.Value, l.DataType);

            if (plainlit)
            {
                if (TurtleSpecsHelper.IsValidDecimal(l.Value) && l.Value.EndsWith("."))
                {
                    //Ensure we strip the trailing dot of any xsd:decimal and add a datatype definition
                    output.Append('"');
                    output.Append(l.Value.Substring(0, l.Value.Length - 1));
                    output.Append("\"^^<");
                    output.Append(this.FormatUri(XmlSpecsHelper.XmlSchemaDataTypeDecimal));
                    output.Append('>');
                }
                else
                {
                    //Otherwise just write out the value
                    output.Append(l.Value);
                }
                //For integers ensure we insert a space after the literal to ensure it can't ever be confused with a decimal
                if (TurtleSpecsHelper.IsValidInteger(l.Value))
                {
                    output.Append(' ');
                }
            }
            else
            {
                output.Append('"');
                if (longlit) output.Append("\"\"");

                value = l.Value;

                bool fullyEscaped = (longlit) ? value.IsFullyEscaped(this._validEscapes, this._longLitMustEscape) : value.IsFullyEscaped(this._validEscapes, this._litMustEscape);

                if (!fullyEscaped)
                {
                    //This first replace escapes all back slashes for good measure
                    value = value.EscapeBackslashes(this._validEscapes);

                    //Then remove null character since it doesn't change the meaning of the Literal
                    value = value.Replace("\0", "");

                    //Don't need all the other escapes for long literals as the characters that would be escaped are permitted in long literals
                    //Need to escape " still
                    value = value.Escape('"');

                    if (!longlit)
                    {
                        //Then if we're not a long literal we'll escape tabs
                        value = value.Replace("\t", "\\t");
                    }
                }
                output.Append(value);
                output.Append('"');
                if (longlit) output.Append("\"\"");

                if (!l.Language.Equals(String.Empty))
                {
                    output.Append('@');
                    output.Append(l.Language.ToLower());
                }
                else if (l.DataType != null)
                {
                    output.Append("^^");
                    if (this._qnameMapper.ReduceToQName(l.DataType.ToString(), out qname))
                    {
                        if (TurtleSpecsHelper.IsValidQName(qname))
                        {
                            output.Append(qname);
                        }
                        else
                        {
                            output.Append('<');
                            output.Append(this.FormatUri(l.DataType));
                            output.Append('>');
                        }
                    }
                    else
                    {
                        output.Append('<');
                        output.Append(this.FormatUri(l.DataType));
                        output.Append('>');
                    }
                }
            }

            return output.ToString();
        }

        /// <summary>
        /// Formats a Blank Node as a String
        /// </summary>
        /// <param name="b">Blank Node</param>
        /// <param name="segment">Triple Segment</param>
        /// <returns></returns>
        protected override string FormatBlankNode(IBlankNode b, TripleSegment? segment)
        {
            return "_:" + this._bnodeMapper.GetOutputID(b.InternalID);
        }

        /// <summary>
        /// Formats a Namespace Decalaration as a @prefix declaration
        /// </summary>
        /// <param name="prefix">Namespace Prefix</param>
        /// <param name="namespaceUri">Namespace URI</param>
        /// <returns></returns>
        public override string FormatNamespace(string prefix, Uri namespaceUri)
        {
            return "@prefix " + prefix + ": <" + this.FormatUri(namespaceUri) + "> .";
        }

        /// <summary>
        /// Formats a Base URI declaration as a @base declaration
        /// </summary>
        /// <param name="u">Base URI</param>
        /// <returns></returns>
        public virtual string FormatBaseUri(Uri u)
        {
            return "@base <" + this.FormatUri(u) + "> .";
        }
    }
}
