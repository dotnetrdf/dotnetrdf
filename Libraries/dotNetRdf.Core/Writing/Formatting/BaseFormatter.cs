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
using System.Collections.Generic;
using VDS.RDF.Query;

namespace VDS.RDF.Writing.Formatting;

/// <summary>
/// Abstract Base Class for Formatters.
/// </summary>
public abstract class BaseFormatter
    : INodeFormatter, ITripleFormatter, IUriFormatter, ICharFormatter, IResultFormatter
{
    private string _format;

    /// <summary>
    /// Creates a new Formatter.
    /// </summary>
    /// <param name="formatName">Format Name.</param>
    public BaseFormatter(string formatName)
    {
        _format = formatName;
    }

    /// <summary>
    /// Gets the Format Name.
    /// </summary>
    public string FormatName
    {
        get
        {
            return _format;
        }
    }

    /// <summary>
    /// Formats a Node as a String.
    /// </summary>
    /// <param name="n">Node.</param>
    /// <param name="segment">Triple Segment.</param>
    /// <returns></returns>
    public virtual string Format(INode n, TripleSegment? segment)
    {
        switch (n.NodeType)
        {
            case NodeType.Blank:
                return FormatBlankNode((IBlankNode)n, segment);
            case NodeType.GraphLiteral:
                return FormatGraphLiteralNode((IGraphLiteralNode)n, segment);
            case NodeType.Literal:
                return FormatLiteralNode((ILiteralNode)n, segment);
            case NodeType.Uri:
                return FormatUriNode((IUriNode)n, segment);
            case NodeType.Variable:
                return FormatVariableNode((IVariableNode)n, segment);
            case NodeType.Triple:
                return FormatTripleNode((ITripleNode)n, segment);
            default:
                throw new RdfOutputException(WriterErrorMessages.UnknownNodeTypeUnserializable(_format));
        }
    }

    /// <summary>
    /// Formats a Node as a String.
    /// </summary>
    /// <param name="n">Node.</param>
    /// <returns></returns>
    public virtual string Format(INode n)
    {
        return Format(n, null);
    }

    /// <summary>
    /// Formats a Triple as a String.
    /// </summary>
    /// <param name="t">Triple.</param>
    /// <returns></returns>
    public virtual string Format(Triple t)
    {
        return Format(t.Subject, TripleSegment.Subject) + " " + Format(t.Predicate, TripleSegment.Predicate) + " " + Format(t.Object, TripleSegment.Object) + " .";
    }

    /// <summary>
    /// Formats a triple in the context of a graph as a string.
    /// </summary>
    /// <param name="t">The triple.</param>
    /// <param name="graph">The graph containing the triple.</param>
    /// <returns></returns>
    public virtual string Format(Triple t, IRefNode graph)
    {
        if (graph == null) return this.Format(t);
        throw new RdfOutputException(WriterErrorMessages.QuadsUnserializable(_format));
    }

    /// <summary>
    /// Formats a URI Node as a String for the given Format.
    /// </summary>
    /// <param name="u">URI Node.</param>
    /// <param name="segment">Triple Segment.</param>
    /// <returns></returns>
    protected abstract string FormatUriNode(IUriNode u, TripleSegment? segment);

    /// <summary>
    /// Formats a URI as a String for full Output.
    /// </summary>
    /// <param name="u">URI.</param>
    /// <returns></returns>
    public virtual string FormatUri(string u)
    {
        return Rfc3987Formatter.EscapeUriString(u);
    }

    /// <summary>
    /// Formats a URI as a String for full Output.
    /// </summary>
    /// <param name="u">URI.</param>
    /// <returns></returns>
    public virtual string FormatUri(Uri u)
    {
        return FormatUri(u.ToString());
    }

    /// <summary>
    /// Formats a Literal Node as a String for the given Format.
    /// </summary>
    /// <param name="l">Literal Node.</param>
    /// <param name="segment">Triple Segment.</param>
    /// <returns></returns>
    protected abstract string FormatLiteralNode(ILiteralNode l, TripleSegment? segment);

    /// <summary>
    /// Formats a Blank Node as a String for the given Format.
    /// </summary>
    /// <param name="b">Blank Node.</param>
    /// <param name="segment">Triple Segment.</param>
    /// <returns></returns>
    protected virtual string FormatBlankNode(IBlankNode b, TripleSegment? segment)
    {
        if (segment == TripleSegment.Predicate) throw new RdfOutputException(WriterErrorMessages.BlankPredicatesUnserializable(_format));
        return b.ToString();
    }

    /// <summary>
    /// Formats a triple node a a string for the given format.
    /// </summary>
    /// <param name="t">Triple node.</param>
    /// <param name="segment">Triple segment being written.</param>
    /// <returns></returns>
    /// <remarks>This method should be overridden in formatters that support serializing <see cref="ITripleNode"/>s.</remarks>
    protected virtual string FormatTripleNode(ITripleNode t, TripleSegment? segment)
    {
        throw new RdfOutputException(WriterErrorMessages.TripleNodesUnserializable(_format));
    }

    /// <summary>
    /// Formats a Variable Node as a String for the given Format.
    /// </summary>
    /// <param name="v">Variable Name.</param>
    /// <param name="segment">Triple Segment.</param>
    /// <returns></returns>
    protected virtual string FormatVariableNode(IVariableNode v, TripleSegment? segment)
    {
        throw new RdfOutputException(WriterErrorMessages.VariableNodesUnserializable(_format));
    }

    /// <summary>
    /// Formats a Graph Literal Node as a String for the given Format.
    /// </summary>
    /// <param name="glit">Graph Literal.</param>
    /// <param name="segment">Triple Segment.</param>
    /// <returns></returns>
    protected virtual string FormatGraphLiteralNode(IGraphLiteralNode glit, TripleSegment? segment)
    {
        throw new RdfOutputException(WriterErrorMessages.GraphLiteralsUnserializable(_format));
    }

    /// <summary>
    /// Formats a Character for the given Format.
    /// </summary>
    /// <param name="c">Character.</param>
    /// <returns></returns>
    [Obsolete("This form of the FormatChar() method is considered obsolete as it is inefficient", true)]
    public virtual string FormatChar(char c)
    {
        return c.ToString();
    }

    /// <summary>
    /// Formats a sequence of characters as a String.
    /// </summary>
    /// <param name="cs">Characters.</param>
    /// <returns>String.</returns>
    public virtual string FormatChar(char[] cs)
    {
        return new string(cs);
    }

    /// <summary>
    /// Formats a SPARQL Result for the given format.
    /// </summary>
    /// <param name="result">SPARQL Result.</param>
    /// <returns></returns>
    public virtual string Format(ISparqlResult result)
    {
        return result.ToString(this);
    }

    /// <summary>
    /// Formats a SPARQL Boolean Result for the given format.
    /// </summary>
    /// <param name="result">Boolean Result.</param>
    /// <returns></returns>
    public virtual string FormatBooleanResult(bool result)
    {
        return result.ToString().ToLower();
    }

    /// <summary>
    /// Applies escapes to the given value.
    /// </summary>
    /// <param name="value">Value.</param>
    /// <param name="escapes">Escapes.</param>
    /// <returns>Escaped string.</returns>
    protected string Escape(string value, List<string[]> escapes)
    {
        foreach (var escape in escapes)
        {
            if (escape.Length != 2) continue;
            value = value.Replace(escape[0], escape[1]);
        }
        return value;
    }

    /// <summary>
    /// Gets the Name of the Format this Format uses.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return FormatName;
    }
}
