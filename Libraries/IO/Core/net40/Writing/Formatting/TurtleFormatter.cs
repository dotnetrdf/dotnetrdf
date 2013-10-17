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
using System.Text.RegularExpressions;
using VDS.RDF.Nodes;
using VDS.RDF.Parsing;
using VDS.RDF.Specifications;

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
            : base("Turtle (Uncompressed)") { }

        /// <summary>
        /// Creates a new Uncompressed Formatter
        /// </summary>
        /// <param name="formatName">Format Name</param>
        protected UncompressedTurtleFormatter(String formatName)
            : base(formatName) { }

        /// <summary>
        /// Formats characters
        /// </summary>
        /// <param name="c">Character</param>
        /// <returns></returns>
        [Obsolete("This form of the FormatChar() method is considered obsolete as it is inefficient", false)]
        public override string FormatChar(char c)
        {
            return c.ToString();
        }

        /// <summary>
        /// Formats a sequence of characters as a String
        /// </summary>
        /// <param name="cs">Characters</param>
        /// <returns>String</returns>
        public override string FormatChar(char[] cs)
        {
            return new string(cs);
        }
    }

    /// <summary>
    /// Formatter which formats Turtle with QName compression
    /// </summary>
    public class TurtleFormatter 
        : QNameFormatter, IBaseUriFormatter
    {
        private BlankNodeOutputMapper _bnodeMapper = new BlankNodeOutputMapper();
        /// <summary>
        /// Set of characters that must be escaped for Long Literals
        /// </summary>
        protected List<String[]> _longLitMustEscape = new List<String[]>
        { 
            new String[] { @"\", @"\\" }, 
            new String[] { "\"", "\\\"" }
        };

        /// <summary>
        /// Set of characters that must be escaped for Literals
        /// </summary>
        protected List<String[]> _litMustEscape = new List<String[]>
        { 
            new String[] { @"\", @"\\" }, 
            new String[] { "\"", "\\\"" },
            new String[] { "\n", @"\n" },
            new String[] { "\r", @"\r" },
            new String[] { "\t", @"\t" }
        };

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
            : base("Turtle", new QNameOutputMapper(g.Namespaces)) { }

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
            : base(formatName, new QNameOutputMapper(g.Namespaces)) { }

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
        protected override string FormatLiteralNode(INode l, TripleSegment? segment)
        {
            StringBuilder output = new StringBuilder();
            String value, qname;
            bool longlit = false, plainlit = false;

            longlit = TurtleSpecsHelper.IsLongLiteral(l.Value);
            plainlit = TurtleSpecsHelper.IsValidPlainLiteral(l.Value, l.DataType, TurtleSyntax.Original);

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
                value = longlit ? this.Escape(value, this._longLitMustEscape) : this.Escape(value, this._litMustEscape);

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
                    if (this._qnameMapper.ReduceToQName(l.DataType.AbsoluteUri, out qname))
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
        protected override string FormatBlankNode(INode b, TripleSegment? segment)
        {
            return "_:" + this._bnodeMapper.GetOutputID(b.AnonID);
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

    /// <summary>
    /// Formatter which formats Turtle with QName compression using the newer W3C syntax which permits a wider range of valid QNames
    /// </summary>
    public class TurtleW3CFormatter
        : TurtleFormatter
    {
        /// <summary>
        /// Creates a new Turtle Formatter
        /// </summary>
        public TurtleW3CFormatter() 
            : base("Turtle (W3C)", new QNameOutputMapper()) { }

        /// <summary>
        /// Creates a new Turtle Formatter that uses the given QName mapper
        /// </summary>
        /// <param name="qnameMapper">QName Mapper</param>
        public TurtleW3CFormatter(QNameOutputMapper qnameMapper)
            : base("Turtle (W3C)", qnameMapper) { }

        /// <summary>
        /// Creates a new Turtle Formatter for the given Graph
        /// </summary>
        /// <param name="g">Graph</param>
        public TurtleW3CFormatter(IGraph g)
            : base("Turtle (W3C)", new QNameOutputMapper(g.Namespaces)) { }

        /// <summary>
        /// Creates a new Turtle Formatter for the given Namespace Map
        /// </summary>
        /// <param name="nsmap">Namespace Map</param>
        public TurtleW3CFormatter(INamespaceMapper nsmap)
            : base("Turtle (W3C)", new QNameOutputMapper(nsmap)) { }

        /// <summary>
        /// Creates a new Turtle Formatter
        /// </summary>
        /// <param name="formatName">Format Name</param>
        protected TurtleW3CFormatter(String formatName)
            : base(formatName, new QNameOutputMapper()) { }

        /// <summary>
        /// Creates a new Turtle Formatter
        /// </summary>
        /// <param name="formatName">Format Name</param>
        /// <param name="g">Graph</param>
        protected TurtleW3CFormatter(String formatName, IGraph g)
            : base(formatName, new QNameOutputMapper(g.Namespaces)) { }

        /// <summary>
        /// Creates a new Turtle Formatter
        /// </summary>
        /// <param name="formatName">Format Name</param>
        /// <param name="nsmap">Namespace Map</param>
        protected TurtleW3CFormatter(String formatName, INamespaceMapper nsmap)
            : base(formatName, new QNameOutputMapper(nsmap)) { }

        /// <summary>
        /// Creates a new Turtle Formatter
        /// </summary>
        /// <param name="formatName">Format Name</param>
        /// <param name="qnameMapper">QName Map</param>
        protected TurtleW3CFormatter(String formatName, QNameOutputMapper qnameMapper)
            : base(formatName, qnameMapper) { }

        /// <summary>
        /// Gets whether a QName is valid in Turtle as specified by the W3C
        /// </summary>
        /// <param name="value">QName</param>
        /// <returns></returns>
        protected override bool IsValidQName(string value)
        {
            return TurtleSpecsHelper.IsValidQName(value, TurtleSyntax.W3C);
        }
    }
}
