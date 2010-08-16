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
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using VDS.RDF.Parsing;

namespace VDS.RDF.Writing.Contexts
{
    /// <summary>
    /// Interface for Writer Contexts
    /// </summary>
    public interface IWriterContext
    {
        /// <summary>
        /// Gets the Graph being written
        /// </summary>
        IGraph Graph
        {
            get;
        }

        /// <summary>
        /// Gets the TextWriter being written to
        /// </summary>
        TextWriter Output
        {
            get;
        }

        /// <summary>
        /// Gets/Sets the Pretty Printing Mode used
        /// </summary>
        bool PrettyPrint
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets the High Speed Mode used
        /// </summary>
        bool HighSpeedModePermitted
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets the Compression Level used
        /// </summary>
        int CompressionLevel
        {
            get;
            set;
        }

        /// <summary>
        /// Formats a Node as a String for the given Format
        /// </summary>
        /// <param name="n">Node</param>
        /// <param name="format">Format</param>
        /// <returns></returns>
        String FormatNode(INode n, NodeFormat format);

        /// <summary>
        /// Formats a URI as a String for full Output
        /// </summary>
        /// <param name="u">URI</param>
        /// <returns></returns>
        String FormatUri(String u);

        /// <summary>
        /// Formats a URI as a String for full Output
        /// </summary>
        /// <param name="u">URI</param>
        /// <returns></returns>
        String FormatUri(Uri u);

        /// <summary>
        /// Formats a Character for the given Format
        /// </summary>
        /// <param name="c">Character</param>
        /// <param name="format">Format</param>
        /// <returns></returns>
        String FormatChar(char c, NodeFormat format);
    }

    /// <summary>
    /// Interface for Writer Contexts which store collection compression data
    /// </summary>
    public interface ICollectionCompressingWriterContext : IWriterContext
    {
        /// <summary>
        /// Gets the mapping from Blank Nodes to Collections
        /// </summary>
        Dictionary<INode, OutputRDFCollection> Collections
        {
            get;
        }

        /// <summary>
        /// Gets the Triples that should be excluded from standard output as they are part of collections
        /// </summary>
        BaseTripleCollection TriplesDone
        {
            get;
        }
    }

    /// <summary>
    /// Base Class for Writer Context Objects
    /// </summary>
    /// <remarks>
    /// This is not an abstract class since some writers will require only this information or possibly less
    /// </remarks>
    public class BaseWriterContext
    {
        /// <summary>
        /// Compression Level to be used
        /// </summary>
        protected int _compressionLevel = WriterCompressionLevel.Default;
        /// <summary>
        /// Pretty Printing Mode setting
        /// </summary>
        protected bool _prettyPrint = true;
        /// <summary>
        /// High Speed Mode setting
        /// </summary>
        protected bool _hiSpeedAllowed = true;
        /// <summary>
        /// Graph being written
        /// </summary>
        private IGraph _g;
        /// <summary>
        /// TextWriter being written to
        /// </summary>
        private TextWriter _output;
        /// <summary>
        /// QName Output Mapper
        /// </summary>
        protected QNameOutputMapper _qnameMapper;

        /// <summary>
        /// Creates a new Base Writer Context with default settings
        /// </summary>
        /// <param name="g">Graph being written</param>
        /// <param name="output">TextWriter being written to</param>
        public BaseWriterContext(IGraph g, TextWriter output)
        {
            this._g = g;
            this._output = output;
            this._qnameMapper = new QNameOutputMapper(this._g.NamespaceMap);
        }

        /// <summary>
        /// Creates a new Base Writer Context with custom settings
        /// </summary>
        /// <param name="g">Graph being written</param>
        /// <param name="output">TextWriter being written to</param>
        /// <param name="compressionLevel">Compression Level</param>
        public BaseWriterContext(IGraph g, TextWriter output, int compressionLevel)
            : this(g, output)
        {
            this._compressionLevel = compressionLevel;
        }

        /// <summary>
        /// Creates a new Base Writer Context with custom settings
        /// </summary>
        /// <param name="g">Graph being written</param>
        /// <param name="output">TextWriter being written to</param>
        /// <param name="compressionLevel">Compression Level</param>
        /// <param name="prettyPrint">Pretty Print Mode</param>
        /// <param name="hiSpeedAllowed">High Speed Mode</param>
        public BaseWriterContext(IGraph g, TextWriter output, int compressionLevel, bool prettyPrint, bool hiSpeedAllowed)
            : this(g, output)
        {
            this._compressionLevel = compressionLevel;
            this._prettyPrint = prettyPrint;
            this._hiSpeedAllowed = hiSpeedAllowed;
        }

        /// <summary>
        /// Gets the Graph being written
        /// </summary>
        public IGraph Graph
        {
            get
            {
                return this._g;
            }
        }

        /// <summary>
        /// Gets the TextWriter being written to
        /// </summary>
        public TextWriter Output
        {
            get
            {
                return this._output;
            }
        }

        /// <summary>
        /// Gets the QName Output Mapper in use
        /// </summary>
        public QNameOutputMapper QNameMapper
        {
            get
            {
                return this._qnameMapper;
            }
        }

        /// <summary>
        /// Gets/Sets the Compression Level used
        /// </summary>
        public virtual int CompressionLevel
        {
            get
            {
                return this._compressionLevel;
            }
            set
            {
                this._compressionLevel = value;
            }
        }

        /// <summary>
        /// Gets/Sets the Pretty Printing Mode used
        /// </summary>
        public bool PrettyPrint
        {
            get
            {
                return this._prettyPrint;
            }
            set
            {
                this._prettyPrint = value;
            }
        }

        /// <summary>
        /// Gets/Sets the High Speed Mode used
        /// </summary>
        public bool HighSpeedModePermitted
        {
            get
            {
                return this._hiSpeedAllowed;
            }
            set
            {
                this._hiSpeedAllowed = value;
            }
        }

        /// <summary>
        /// Sets the QName Mapper used
        /// </summary>
        /// <param name="mapper">QName Mapper</param>
        protected internal void SetQNameOutputerMapper(QNameOutputMapper mapper) 
        {
            this._qnameMapper = mapper;
        }

        /// <summary>
        /// Formats a Node as a String for the given Format
        /// </summary>
        /// <param name="n">Node</param>
        /// <param name="format">Format</param>
        /// <returns></returns>
        public virtual String FormatNode(INode n, NodeFormat format)
        {
            switch (n.NodeType)
            {
                case NodeType.Blank:
                    return this.FormatBlankNode((BlankNode)n, format);
                case NodeType.GraphLiteral:
                    throw new NotSupportedException("Graph Literal Nodes cannot be formatted with this function");
                case NodeType.Literal:
                    return this.FormatLiteralNode((LiteralNode)n, format);
                case NodeType.Uri:
                    return this.FormatUriNode((UriNode)n, format);
                default:
                    throw new RdfOutputException(WriterErrorMessages.UnknownNodeTypeUnserializable(format.ToString()));
            }
        }

        /// <summary>
        /// Formats a URI Node as a String for the given Format
        /// </summary>
        /// <param name="u">URI Node</param>
        /// <param name="format">Format</param>
        /// <returns></returns>
        protected virtual String FormatUriNode(UriNode u, NodeFormat format)
        {
            StringBuilder output = new StringBuilder();
            String qname;
            switch (format)
            {
                case NodeFormat.NTriples:
                case NodeFormat.UncompressedNotation3:
                case NodeFormat.UncompressedTurtle:
                default:
                    output.Append('<');
                    foreach (char c in this.FormatUri(u.StringUri).ToCharArray())
                    {
                        output.Append(this.FormatChar(c, format));
                    }
                    output.Append('>');
                    break;

                case NodeFormat.Notation3:
                case NodeFormat.Turtle:
                    if (this._qnameMapper.ReduceToQName(u.StringUri, out qname))
                    {
                        if (TurtleSpecsHelper.IsValidQName(qname))
                        {
                            output.Append(qname);
                        }
                        else
                        {
                            output.Append('<');
                            output.Append(this.FormatUri(u.StringUri));
                            output.Append('>');
                        }
                    }
                    else if (u.StringUri.Equals(RdfSpecsHelper.RdfType))
                    {
                        output.Append('a');
                    }
                    else
                    {
                        output.Append('<');
                        output.Append(this.FormatUri(u.StringUri));
                        output.Append('>');
                    }
                    break;
            }

            return output.ToString();
        }

        /// <summary>
        /// Formats a URI as a String for full Output
        /// </summary>
        /// <param name="u">URI</param>
        /// <returns></returns>
        public virtual String FormatUri(String u)
        {
            String uri = Uri.EscapeUriString(u);
            uri = uri.Replace(">", "\\>");
            return uri;
        }

        /// <summary>
        /// Formats a URI as a String for full Output
        /// </summary>
        /// <param name="u">URI</param>
        /// <returns></returns>
        public virtual String FormatUri(Uri u)
        {
            return this.FormatUri(u.ToString());
        }

        /// <summary>
        /// Formats a Literal Node as a String for the given Format
        /// </summary>
        /// <param name="l">Literal Node</param>
        /// <param name="format">Format</param>
        /// <returns></returns>
        protected virtual String FormatLiteralNode(LiteralNode l, NodeFormat format)
        {
            StringBuilder output = new StringBuilder();
            String value, qname;
            bool longlit = false, plainlit = false;

            switch (format)
            {
                case NodeFormat.NTriples:
                case NodeFormat.UncompressedNotation3:
                case NodeFormat.UncompressedTurtle:
                default:
                    output.Append('"');
                    value = l.Value;

                    //This first replace escapes all back slashes for good measure
                    if (value.Contains("\\")) value = value.Replace("\\","\\\\");

                    //Then these escape characters that can't occur in a NTriples literal
                    value = value.Replace("\n", "\\n");
                    value = value.Replace("\r", "\\r");
                    value = value.Replace("\t", "\\t");
                    value = value.Replace("\"", "\\\"");

                    //Then remove null character since it doesn't change the meaning of the Literal
                    value = value.Replace("\0", "");

                    foreach (char c in value.ToCharArray())
                    {
                        output.Append(this.FormatChar(c, format));
                    }
                    output.Append('"');

                    if (!l.Language.Equals(String.Empty))
                    {
                        output.Append('@');
                        output.Append(l.Language);
                    }
                    else if (l.DataType != null)
                    {
                        output.Append("^^<");
                        foreach (char c in this.FormatUri(l.DataType))
                        {
                            output.Append(this.FormatChar(c, format));
                        }
                        output.Append('>');
                    }

                    break;

                case NodeFormat.Notation3:
                case NodeFormat.Turtle:
                    longlit = TurtleSpecsHelper.IsLongLiteral(l.Value);
                    plainlit = TurtleSpecsHelper.IsValidPlainLiteral(l.Value);

                    if (plainlit)
                    {
                        output.Append(l.Value);
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
                        //This first replace escapes all back slashes for good measure
                        if (value.Contains("\\")) value = value.Replace("\\", "\\\\");

                        //Then remove null character since it doesn't change the meaning of the Literal
                        value = value.Replace("\0", "");

                        //Don't need all the other escapes for long literals as the characters that would be escaped are permitted in long literals
                        //Need to escape " still
                        value = value.Replace("\"", "\\\"");

                        if (!longlit)
                        {
                            //Then if we're not a long literal we'll escape tabs
                            value = value.Replace("\t", "\\t");
                        }
                        output.Append(value);
                        output.Append('"');
                        if (longlit) output.Append("\"\"");

                        if (!l.Language.Equals(String.Empty))
                        {
                            output.Append('@');
                            output.Append(l.Language);
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

                    break;
            }

            return output.ToString();
        }

        /// <summary>
        /// Formats a Blank Node as a String for the given Format
        /// </summary>
        /// <param name="b">Blank Node</param>
        /// <param name="format">Format</param>
        /// <returns></returns>
        protected virtual String FormatBlankNode(BlankNode b, NodeFormat format)
        {
            return b.ToString();
        }

        /// <summary>
        /// Formats a Character for the given Format
        /// </summary>
        /// <param name="c">Character</param>
        /// <param name="format">Format</param>
        /// <returns></returns>
        public virtual String FormatChar(char c, NodeFormat format)
        {
            if (format == NodeFormat.NTriples)
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
            else
            {
                return c.ToString();
            }
        }
    }
}