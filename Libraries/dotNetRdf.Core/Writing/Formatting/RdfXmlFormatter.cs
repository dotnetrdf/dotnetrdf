/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2025 dotNetRDF Project (http://dotnetrdf.org/)
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
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using VDS.RDF.Parsing;

namespace VDS.RDF.Writing.Formatting;

/// <summary>
/// A formatter which formats triples for RDF/XML output.
/// </summary>
public class RdfXmlFormatter 
    : IGraphFormatter, ICommentFormatter
{
    private QNameOutputMapper _mapper;

    private string GetGraphHeaderBase()
    {
        return @"<?xml version=""1.0"" encoding=""utf-8""?>
<rdf:RDF xmlns:rdf=""" + NamespaceMapper.RDF + "\"";
    }

    /// <summary>
    /// Formats a Graph Header by creating an <strong>&lt;rdf:RDF&gt;</strong> element and adding namespace definitions.
    /// </summary>
    /// <param name="g">Graph.</param>
    /// <returns></returns>
    public string FormatGraphHeader(IGraph g)
    {
        _mapper = new QNameOutputMapper(g.NamespaceMap, g.UriFactory);
        var output = new StringBuilder();
        output.Append(GetGraphHeaderBase());
        foreach (var prefix in g.NamespaceMap.Prefixes)
        {
            if (!prefix.Equals("rdf"))
            {
                if (prefix.Equals(string.Empty))
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
    /// Formats a Graph Header by creating an <strong>&lt;rdf:RDF&gt;</strong> element and adding namespace definitions.
    /// </summary>
    /// <param name="namespaces">Namespaces.</param>
    /// <param name="uriFactory">The factory to use when creating new Uri instances.</param>
    /// <returns></returns>
    public string FormatGraphHeader(INamespaceMapper namespaces, IUriFactory uriFactory = null)
    {
        _mapper = new QNameOutputMapper(namespaces, uriFactory);
        var output = new StringBuilder();
        output.Append(GetGraphHeaderBase());
        foreach (var prefix in namespaces.Prefixes)
        {
            if (!prefix.Equals("rdf"))
            {
                if (prefix.Equals(string.Empty))
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
    /// Formats a Graph Header by creating an <strong>&lt;rdf:RDF&gt;</strong> element.
    /// </summary>
    public string FormatGraphHeader()
    {
        return GetGraphHeaderBase() + ">";
    }

    /// <summary>
    /// Formats a Graph Footer by closing the <strong>&lt;rdf:RDF&gt;</strong> element.
    /// </summary>
    /// <returns></returns>
    public string FormatGraphFooter()
    {
        _mapper = null;
        return "</rdf:RDF>";
    }

    /// <summary>
    /// Attempt to split the string representation of the specified URI into a valid QName and namespace pair.
    /// </summary>
    /// <param name="u">The URI to be split.</param>
    /// <param name="qName">Receives the QName portion of the split URI if splitting was possible, null otherwise.</param>
    /// <param name="ns">Receives the namespace portion of the split URI if splitting was possible, null otherwise.</param>
    /// <returns>True if the specified URI could be successfully split into a Namespace/QName pair, false otherwise.</returns>
    public static bool TryReduceUriToQName(Uri u, out string qName, out string ns)
    {
        var uri = u.AbsoluteUri;
        if (String.IsNullOrEmpty(uri))
        {
            qName = ns = null; 
            return false;
        }
        var lastStart = -1;
        for (var start = uri.Length - 1; start >= 0; start--)
        {
            var c = uri[start];
            if (!XmlConvert.IsNCNameChar(c))
            {
                // No valid local name by this point
                break;
            }
            if (XmlConvert.IsStartNCNameChar(c))
            {
                // The longest local name may start here
                lastStart = start;
            }
        }
        if (lastStart != -1)
        {
            // The local name can be extracted
            qName = uri.Substring(lastStart);
            ns = uri.Substring(0, lastStart);
            return true;
        }
        qName = ns = null;
        return false;
    }

    private void GetQName(Uri u, out string qName, out string ns)
    {
        if (_mapper != null && _mapper.ReduceToQName(u.AbsoluteUri, out qName) && RdfXmlSpecsHelper.IsValidQName(qName))
        {
            // Successfully reduced to a QName using the known namespaces
            ns = string.Empty;
            return;
        }

        if (TryReduceUriToQName(u, out qName, out ns) && RdfXmlSpecsHelper.IsValidQName(qName))
        {
            return;
        }
        throw new RdfOutputException(WriterErrorMessages.UnreducablePropertyURIUnserializable);
    }

    /// <summary>
    /// Formats a Triple as a <strong>&lt;rdf:Description&gt;</strong> element.
    /// </summary>
    /// <param name="t">Triple.</param>
    /// <returns></returns>
    public string Format(Triple t)
    {
        var output = new StringBuilder();
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

        string qName;
        switch (t.Predicate.NodeType)
        {
            case NodeType.Uri:
                Uri u = ((IUriNode)t.Predicate).Uri;
                GetQName(u, out qName, out var ns);
                output.Append('\t');
                if (ns.Equals(string.Empty))
                {
                    output.Append("<" + qName);
                }
                else
                {
                    output.Append("<" + qName + " xmlns=\"" + WriterHelper.EncodeForXml(ns) + "\"");
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
                var lit = (ILiteralNode)t.Object;
                if (lit.DataType.AbsoluteUri == RdfSpecsHelper.RdfLangString && !lit.Language.Equals(string.Empty))
                {
                    output.AppendLine(" xml:lang=\"" + lit.Language + "\">" + WriterHelper.EncodeForXml(lit.Value) + "</" + qName + ">");
                }
                else if (lit.DataType.AbsoluteUri.Equals(XmlSpecsHelper.XmlSchemaDataTypeString))
                {
                    output.AppendLine(">" + WriterHelper.EncodeForXml(lit.Value) + "</" + qName + ">");
                }
                else if (lit.DataType != null)
                {
                    if (lit.DataType.ToString().Equals(RdfSpecsHelper.RdfXmlLiteral))
                    {
                        output.AppendLine(" rdf:parseType=\"Literal\">" + lit.Value + "</" + qName + ">");
                    }
                    else
                    {
                        output.AppendLine(" rdf:datatype=\"" + WriterHelper.EncodeForXml(lit.DataType.AbsoluteUri) + "\">" + WriterHelper.EncodeForXml(lit.Value) + "</" + qName + ">");
                    }
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
    /// Matches any hyphen that is followed by another hyphen or the end of the string.
    /// </summary>
    static readonly Regex commentHyphenRegex = new Regex(@"-(?=-|$)", RegexOptions.Compiled);

    /// <inheritdoc />
    public virtual string FormatComment(string text)
    {
        text = WriterHelper.RemoveInvalidXmlChars(text);
        return "<!--" + commentHyphenRegex.Replace(text, "-\u200B") + "-->";
    }

    /// <summary>
    /// Gets the String description of this formatter.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return "RDF/XML";
    }
}
