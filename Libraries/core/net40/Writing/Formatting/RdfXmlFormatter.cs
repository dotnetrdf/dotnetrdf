/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

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
using System.Xml;
using VDS.RDF.Parsing;

namespace VDS.RDF.Writing.Formatting
{
    /// <summary>
    /// A formatter which formats triples for RDF/XML output
    /// </summary>
    public class RdfXmlFormatter 
        : IGraphFormatter
    {
        private QNameOutputMapper _mapper;

        private String GetGraphHeaderBase()
        {
            return @"<?xml version=""1.0"" encoding=""utf-8""?>
<rdf:RDF xmlns:rdf=""" + NamespaceMapper.RDF + "\"";
        }

        /// <summary>
        /// Formats a Graph Header by creating an <strong>&lt;rdf:RDF&gt;</strong> element and adding namespace definitions
        /// </summary>
        /// <param name="g">Graph</param>
        /// <returns></returns>
        public string FormatGraphHeader(IGraph g)
        {
            this._mapper = new QNameOutputMapper(g.NamespaceMap);
            StringBuilder output = new StringBuilder();
            output.Append(this.GetGraphHeaderBase());
            foreach (String prefix in g.NamespaceMap.Prefixes)
            {
                if (!prefix.Equals("rdf"))
                {
                    if (prefix.Equals(String.Empty))
                    {
                        output.Append(" xmlns=\"" + WriterHelper.EncodeForXml(g.NamespaceMap.GetNamespaceUri(prefix).AbsoluteUri) + "\"");
                    }
                    else
                    {
                        output.Append(" xmlns:" + prefix + "=\"" + WriterHelper.EncodeForXml(g.NamespaceMap.GetNamespaceUri(prefix).AbsoluteUri) + "\"");
                    }
                }
            }
            if (g.BaseUri != null)
            {
                output.Append(" xml:base=\"" + WriterHelper.EncodeForXml(g.BaseUri.AbsoluteUri) + "\"");
            }
            output.Append(">");
            return output.ToString();
        }

        /// <summary>
        /// Formats a Graph Header by creating an <strong>&lt;rdf:RDF&gt;</strong> element and adding namespace definitions
        /// </summary>
        /// <param name="namespaces">Namespaces</param>
        /// <returns></returns>
        public string FormatGraphHeader(INamespaceMapper namespaces)
        {
            this._mapper = new QNameOutputMapper(namespaces);
            StringBuilder output = new StringBuilder();
            output.Append(this.GetGraphHeaderBase());
            foreach (String prefix in namespaces.Prefixes)
            {
                if (!prefix.Equals("rdf"))
                {
                    if (prefix.Equals(String.Empty))
                    {
                        output.Append(" xmlns=\"" + WriterHelper.EncodeForXml(namespaces.GetNamespaceUri(prefix).AbsoluteUri) + "\"");
                    }
                    else
                    {
                        output.Append(" xmlns:" + prefix + "=\"" + WriterHelper.EncodeForXml(namespaces.GetNamespaceUri(prefix).AbsoluteUri) + "\"");
                    }
                }
            }
            output.Append(">");
            return output.ToString();
        }

        /// <summary>
        /// Formats a Graph Header by creating an <strong>&lt;rdf:RDF&gt;</strong> element
        /// </summary>
        public string FormatGraphHeader()
        {
            return this.GetGraphHeaderBase() + ">";
        }

        /// <summary>
        /// Formats a Graph Footer by closing the <strong>&lt;rdf:RDF&gt;</strong> element
        /// </summary>
        /// <returns></returns>
        public string FormatGraphFooter()
        {
            this._mapper = null;
            return "</rdf:RDF>";
        }

        private void GetQName(Uri u, out String qname, out String ns)
        {
            if (this._mapper != null && this._mapper.ReduceToQName(u.AbsoluteUri, out qname) && RdfXmlSpecsHelper.IsValidQName(qname))
            {
                //Succesfully reduced to a QName using the known namespaces
                ns = String.Empty;
                return;
            }
            else if (!u.Fragment.Equals(String.Empty))
            {
                ns = u.AbsoluteUri.Substring(0, u.AbsoluteUri.Length - u.Fragment.Length + 1);
                qname = u.Fragment.Substring(1);
            }
            else
            {
#if !SILVERLIGHT
                qname = u.Segments.LastOrDefault();
#else
                qname = u.Segments().LastOrDefault();
#endif
                if (qname == null || !RdfXmlSpecsHelper.IsValidQName(qname)) throw new RdfOutputException(WriterErrorMessages.UnreducablePropertyURIUnserializable);
                ns = u.AbsoluteUri.Substring(0, u.AbsoluteUri.Length - qname.Length);
            }
        }

        /// <summary>
        /// Formats a Triple as a <strong>&lt;rdf:Description&gt;</strong> element
        /// </summary>
        /// <param name="t">Triple</param>
        /// <returns></returns>
        public string Format(Triple t)
        {
            StringBuilder output = new StringBuilder();
            output.Append("<rdf:Description ");
            switch (t.Subject.NodeType)
            {
                case NodeType.Uri:
                    output.Append("rdf:about=\"" + WriterHelper.EncodeForXml(t.Subject.ToString()) + "\"");
                    break;
                case NodeType.Blank:
                    output.Append("rdf:nodeID=\"" + ((IBlankNode)t.Subject).InternalID + "\"");
                    break;
                case NodeType.Literal:
                    throw new RdfOutputException(WriterErrorMessages.LiteralSubjectsUnserializable("RDF/XML"));
                case NodeType.GraphLiteral:
                    throw new RdfOutputException(WriterErrorMessages.GraphLiteralsUnserializable("RDF/XML"));
                case NodeType.Variable:
                    throw new RdfOutputException(WriterErrorMessages.VariableNodesUnserializable("RDF/XML"));
                default:
                    throw new RdfOutputException(WriterErrorMessages.UnknownNodeTypeUnserializable("RDF/XML"));
            }
            output.AppendLine(">");

            String qname;
            String ns;
            switch (t.Predicate.NodeType)
            {
                case NodeType.Uri:
                    Uri u = ((IUriNode)t.Predicate).Uri;
                    this.GetQName(u, out qname, out ns);
                    output.Append('\t');
                    if (ns.Equals(String.Empty))
                    {
                        output.Append("<" + qname);
                    }
                    else
                    {
                        output.Append("<" + qname + " xmlns=\"" + WriterHelper.EncodeForXml(ns) + "\"");
                    }
                    break;
                case NodeType.Blank:
                    throw new RdfOutputException(WriterErrorMessages.BlankPredicatesUnserializable("RDF/XML"));
                case NodeType.Literal:
                    throw new RdfOutputException(WriterErrorMessages.LiteralPredicatesUnserializable("RDF/XML"));
                case NodeType.GraphLiteral:
                    throw new RdfOutputException(WriterErrorMessages.GraphLiteralsUnserializable("RDF/XML"));
                case NodeType.Variable:
                    throw new RdfOutputException(WriterErrorMessages.VariableNodesUnserializable("RDF/XML"));
                default:
                    throw new RdfOutputException(WriterErrorMessages.UnknownNodeTypeUnserializable("RDF/XML"));

            }

            switch (t.Object.NodeType)
            {
                case NodeType.Blank:
                    output.AppendLine(" rdf:nodeID=\"" + ((IBlankNode)t.Object).InternalID + "\" />");
                    break;
                case NodeType.GraphLiteral:
                    throw new RdfOutputException(WriterErrorMessages.GraphLiteralsUnserializable("RDF/XML"));
                case NodeType.Literal:
                    ILiteralNode lit = (ILiteralNode)t.Object;
                    if (lit.DataType != null)
                    {
                        if (lit.DataType.ToString().Equals(RdfSpecsHelper.RdfXmlLiteral))
                        {
                            output.AppendLine(" rdf:parseType=\"Literal\">" + lit.Value + "</" + qname + ">");
                        }
                        else
                        {
                            output.AppendLine(" rdf:datatype=\"" + WriterHelper.EncodeForXml(lit.DataType.AbsoluteUri) + "\">" + WriterHelper.EncodeForXml(lit.Value) + "</" + qname + ">");
                        }
                    } 
                    else if (!lit.Language.Equals(String.Empty))
                    {
                        output.AppendLine(" xml:lang=\"" + lit.Language + "\">" + WriterHelper.EncodeForXml(lit.Value) + "</" + qname + ">");
                    }
                    else 
                    {
                        output.AppendLine(">" + WriterHelper.EncodeForXml(lit.Value) + "</" + qname + ">");
                    }
                    break;
                case NodeType.Uri:
                    output.AppendLine(" rdf:resource=\"" + WriterHelper.EncodeForXml(t.Object.ToString()) + "\" />");
                    break;
                case NodeType.Variable:
                    throw new RdfOutputException(WriterErrorMessages.VariableNodesUnserializable("RDF/XML"));
                default:
                    throw new RdfOutputException(WriterErrorMessages.UnknownNodeTypeUnserializable("RDF/XML"));
            }

            output.Append("</rdf:Description>");
            return output.ToString();
        }

        /// <summary>
        /// Gets the String description of this formatter
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "RDF/XML";
        }
    }
}
