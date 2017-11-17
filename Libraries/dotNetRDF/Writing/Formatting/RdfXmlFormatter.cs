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
using System.Linq;
using System.Text;
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
            _mapper = new QNameOutputMapper(g.NamespaceMap);
            StringBuilder output = new StringBuilder();
            output.Append(GetGraphHeaderBase());
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
            _mapper = new QNameOutputMapper(namespaces);
            StringBuilder output = new StringBuilder();
            output.Append(GetGraphHeaderBase());
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
            return GetGraphHeaderBase() + ">";
        }

        /// <summary>
        /// Formats a Graph Footer by closing the <strong>&lt;rdf:RDF&gt;</strong> element
        /// </summary>
        /// <returns></returns>
        public string FormatGraphFooter()
        {
            _mapper = null;
            return "</rdf:RDF>";
        }

        private void GetQName(Uri u, out String qname, out String ns)
        {
            if (_mapper != null && _mapper.ReduceToQName(u.AbsoluteUri, out qname) && RdfXmlSpecsHelper.IsValidQName(qname))
            {
                // Succesfully reduced to a QName using the known namespaces
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
                qname = u.Segments.LastOrDefault();
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
                    GetQName(u, out qname, out ns);
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
