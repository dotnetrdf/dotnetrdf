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
using System.Text;
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
            : base("Turtle (Uncompressed)") { }

        /// <summary>
        /// Creates a new Uncompressed Formatter
        /// </summary>
        /// <param name="formatName">Format Name</param>
        protected UncompressedTurtleFormatter(string formatName)
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
        private readonly BlankNodeOutputMapper _bnodeMapper = new BlankNodeOutputMapper(WriterHelper.IsValidBlankNodeID);
        /// <summary>
        /// Set of characters that must be escaped for Long Literals
        /// </summary>
        protected List<string[]> _longLitMustEscape = new List<string[]>
        { 
            new string[] { @"\", @"\\" }, 
            new string[] { "\"", "\\\"" },
        };

        /// <summary>
        /// Set of characters that must be escaped for Literals
        /// </summary>
        protected List<string[]> _litMustEscape = new List<string[]>
        { 
            new string[] { @"\", @"\\" }, 
            new string[] { "\"", "\\\"" },
            new string[] { "\n", @"\n" },
            new string[] { "\r", @"\r" },
            new string[] { "\t", @"\t" },
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
        protected TurtleFormatter(string formatName)
            : base(formatName, new QNameOutputMapper()) { }

        /// <summary>
        /// Creates a new Turtle Formatter
        /// </summary>
        /// <param name="formatName">Format Name</param>
        /// <param name="g">Graph</param>
        protected TurtleFormatter(string formatName, IGraph g)
            : base(formatName, new QNameOutputMapper(g.NamespaceMap)) { }

        /// <summary>
        /// Creates a new Turtle Formatter
        /// </summary>
        /// <param name="formatName">Format Name</param>
        /// <param name="nsmap">Namespace Map</param>
        protected TurtleFormatter(string formatName, INamespaceMapper nsmap)
            : base(formatName, new QNameOutputMapper(nsmap)) { }

        /// <summary>
        /// Creates a new Turtle Formatter
        /// </summary>
        /// <param name="formatName">Format Name</param>
        /// <param name="qnameMapper">QName Map</param>
        protected TurtleFormatter(string formatName, QNameOutputMapper qnameMapper)
            : base(formatName, qnameMapper) { }

        /// <summary>
        /// Formats a Literal Node as a String
        /// </summary>
        /// <param name="l">Literal Node</param>
        /// <param name="segment">Triple Segment</param>
        /// <returns></returns>
        protected override string FormatLiteralNode(ILiteralNode l, TripleSegment? segment)
        {
            var output = new StringBuilder();
            bool longlit, plainlit;

            longlit = TurtleSpecsHelper.IsLongLiteral(l.Value);
            plainlit = TurtleSpecsHelper.IsValidPlainLiteral(l.Value, l.DataType, TurtleSyntax.Original);

            if (plainlit)
            {
                if (TurtleSpecsHelper.IsValidDecimal(l.Value) && l.Value.EndsWith("."))
                {
                    // Ensure we strip the trailing dot of any xsd:decimal and add a datatype definition
                    output.Append('"');
                    output.Append(l.Value.Substring(0, l.Value.Length - 1));
                    output.Append("\"^^<");
                    output.Append(FormatUri(XmlSpecsHelper.XmlSchemaDataTypeDecimal));
                    output.Append('>');
                }
                else
                {
                    // Otherwise just write out the value
                    output.Append(l.Value);
                }
                // For integers ensure we insert a space after the literal to ensure it can't ever be confused with a decimal
                if (TurtleSpecsHelper.IsValidInteger(l.Value))
                {
                    output.Append(' ');
                }
            }
            else
            {
                output.Append('"');
                if (longlit) output.Append("\"\"");

                var value = l.Value;
                value = longlit ? Escape(value, _longLitMustEscape) : Escape(value, _litMustEscape);

                output.Append(value);
                output.Append('"');
                if (longlit) output.Append("\"\"");

                if (!l.Language.Equals(string.Empty))
                {
                    output.Append('@');
                    output.Append(l.Language.ToLower());
                }
                else if (l.DataType != null)
                {
                    output.Append("^^");
                    if (_qnameMapper.ReduceToQName(l.DataType.AbsoluteUri, out var qname))
                    {
                        if (TurtleSpecsHelper.IsValidQName(qname))
                        {
                            output.Append(qname);
                        }
                        else
                        {
                            output.Append('<');
                            output.Append(FormatUri(l.DataType));
                            output.Append('>');
                        }
                    }
                    else
                    {
                        output.Append('<');
                        output.Append(FormatUri(l.DataType));
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
            return "_:" + _bnodeMapper.GetOutputID(b.InternalID);
        }

        /// <summary>
        /// Formats a Namespace Decalaration as a @prefix declaration
        /// </summary>
        /// <param name="prefix">Namespace Prefix</param>
        /// <param name="namespaceUri">Namespace URI</param>
        /// <returns></returns>
        public override string FormatNamespace(string prefix, Uri namespaceUri)
        {
            return "@prefix " + prefix + ": <" + FormatUri(namespaceUri) + "> .";
        }

        /// <summary>
        /// Formats a Base URI declaration as a @base declaration
        /// </summary>
        /// <param name="u">Base URI</param>
        /// <returns></returns>
        public virtual string FormatBaseUri(Uri u)
        {
            return "@base <" + FormatUri(u) + "> .";
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
            : base("Turtle (W3C)", new QNameOutputMapper(g.NamespaceMap)) { }

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
        protected TurtleW3CFormatter(string formatName)
            : base(formatName, new QNameOutputMapper()) { }

        /// <summary>
        /// Creates a new Turtle Formatter
        /// </summary>
        /// <param name="formatName">Format Name</param>
        /// <param name="g">Graph</param>
        protected TurtleW3CFormatter(string formatName, IGraph g)
            : base(formatName, new QNameOutputMapper(g.NamespaceMap)) { }

        /// <summary>
        /// Creates a new Turtle Formatter
        /// </summary>
        /// <param name="formatName">Format Name</param>
        /// <param name="nsmap">Namespace Map</param>
        protected TurtleW3CFormatter(string formatName, INamespaceMapper nsmap)
            : base(formatName, new QNameOutputMapper(nsmap)) { }

        /// <summary>
        /// Creates a new Turtle Formatter
        /// </summary>
        /// <param name="formatName">Format Name</param>
        /// <param name="qnameMapper">QName Map</param>
        protected TurtleW3CFormatter(string formatName, QNameOutputMapper qnameMapper)
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
