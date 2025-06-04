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
using VDS.RDF.Parsing;

namespace VDS.RDF.Writing.Formatting;

/// <summary>
/// Formatter for formatting as NTriples.
/// </summary>
public class NTriplesFormatter
    : BaseFormatter, ICommentFormatter
{
    private readonly BlankNodeOutputMapper _bnodeMapper;

    /// <summary>
    /// Creates a new NTriples formatter.
    /// </summary>
    /// <param name="syntax">NTriples syntax to output.</param>
    /// <param name="formatName">Format Name.</param>
    public NTriplesFormatter(NTriplesSyntax syntax, string formatName)
        : base(formatName)
    {
        Syntax = syntax;
        switch (Syntax)
        {
            case NTriplesSyntax.Original:
                _bnodeMapper = new BlankNodeOutputMapper(WriterHelper.IsValidStrictBlankNodeID);
                break;
            default:
                _bnodeMapper = new BlankNodeOutputMapper(WriterHelper.IsValidBlankNodeID);
                break;
        }
    }

            /// <summary>
    /// Creates a new NTriples Formatter.
    /// </summary>
    public NTriplesFormatter(NTriplesSyntax syntax)
        : this(syntax, GetName(syntax)) { }

    /// <summary>
    /// Creates a new NTriples Formatter.
    /// </summary>
    public NTriplesFormatter()
        : this(NTriplesSyntax.Original, GetName()) { }

    /// <summary>
    /// Creates a new NTriples Formatter.
    /// </summary>
    /// <param name="formatName">Format Name.</param>
    protected NTriplesFormatter(string formatName)
        : this(NTriplesSyntax.Original, formatName) { }

    private static string GetName(NTriplesSyntax syntax = NTriplesSyntax.Original)
    {
        switch (syntax)
        {
            case NTriplesSyntax.Original:
                return "NTriples";
            case NTriplesSyntax.Rdf11Star:
                return "NTriples (RDF-Star)";
            default:
                return "NTriples (RDF 1.1)";
        }
    }

    /// <summary>
    /// Gets the NTriples syntax being used.
    /// </summary>
    public NTriplesSyntax Syntax { get; private set; }

    /// <inheritdoc />
    public override string Format(Triple t, IRefNode graph)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Formats a URI Node.
    /// </summary>
    /// <param name="u">URI Node.</param>
    /// <param name="segment">Triple Segment.</param>
    /// <returns></returns>
    protected override string FormatUriNode(IUriNode u, TripleSegment? segment)
    {
        var output = new StringBuilder();
        output.Append('<');
        output.Append(FormatUri(u.Uri));
        output.Append('>');
        return output.ToString();
    }


    /// <summary>
    /// Formats a Literal Node.
    /// </summary>
    /// <param name="l">Literal Node.</param>
    /// <param name="segment">Triple Segment.</param>
    /// <returns></returns>
    protected override string FormatLiteralNode(ILiteralNode l, TripleSegment? segment)
    {
        var output = new StringBuilder();

        output.Append('"');
        output.Append(EscapeString(l.Value));
        output.Append('"');

        if (!l.Language.Equals(string.Empty))
        {
            output.Append('@');
            output.Append(l.Language.ToLower());
        }
        else if (l.DataType != null)
        {
            output.Append(FormatDatatype(l.DataType));
        }

        return output.ToString();
    }

    private string EscapeString(string s)
    {
        var builder = new StringBuilder();
        foreach (var c in s)
        {
            builder.Append(c switch
            {
                // https://www.w3.org/TR/n-triples/#grammar-production-ECHAR
                '\t' => @"\t", '\b' => @"\b", '\n' => @"\n", '\r' => @"\r", '\f' => @"\f",
                '"' => "\\\"", '\\' => @"\\",

                // Escape other non-printable characters as \uXXXX.
                _ when c < ' ' || c == 0x7F => @"\u" + ((int)c).ToString("X4"),

                // For Original NTriples syntax characters outside the ASCII range must be escaped
                _ when Syntax == NTriplesSyntax.Original && c > 127 => @"\u" + ((int)c).ToString("X4"),

                // Everything else gets passed through verbatim.
                _ => c,
            });
        }
        return builder.ToString();
    }

    /// <summary>
    /// Format the datatype specification for a literal value.
    /// </summary>
    /// <param name="datatypeUri">The datatype URI.</param>
    /// <returns></returns>
    protected virtual string FormatDatatype(Uri datatypeUri)
    {
        return $"^^<{FormatUri(datatypeUri)}>";
    }

    /// <summary>
    /// Formats a triple node a a string for the given format.
    /// </summary>
    /// <param name="t">Triple node.</param>
    /// <param name="segment">Triple segment being written.</param>
    /// <returns></returns>
    protected override string FormatTripleNode(ITripleNode t, TripleSegment? segment)
    {
        if (Syntax != NTriplesSyntax.Rdf11Star)
        {
            throw new RdfOutputException(
                WriterErrorMessages.TripleNodesUnserializable(this.FormatName + "/" + Syntax));
        }
        var output = new StringBuilder();
        output.Append("<< ");
        output.Append(Format(t.Triple.Subject, TripleSegment.Subject));
        output.Append(" ");
        output.Append(Format(t.Triple.Predicate, TripleSegment.Predicate));
        output.Append(" ");
        output.Append(Format(t.Triple.Object, TripleSegment.Object));
        output.Append(" >>");
        return output.ToString();
    }

    /// <summary>
    /// Formats a Character.
    /// </summary>
    /// <param name="c">Character.</param>
    /// <returns></returns>
    [Obsolete("This form of the FormatChar() method is considered obsolete as it is inefficient", true)]
    public override string FormatChar(char c)
    {
        if (Syntax != NTriplesSyntax.Original) return base.FormatChar(c);
        if (c <= 127)
        {
            // ASCII
            return c.ToString();
        }
        // Small Unicode Escape required
        return "\\u" + ((int)c).ToString("X4");
    }

    /// <summary>
    /// Formats a sequence of characters as a String.
    /// </summary>
    /// <param name="cs">Characters.</param>
    /// <returns>String.</returns>
    public override string FormatChar(char[] cs)
    {
        if (Syntax != NTriplesSyntax.Original) return base.FormatChar(cs);

        var builder = new StringBuilder();
        int start = 0, length = 0;
        for (var i = 0; i < cs.Length; i++)
        {
            var c = cs[i];
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
        if (length > 0) builder.Append(cs, start, length);
        return builder.ToString();
    }

    /// <summary>
    /// Formats a Blank Node.
    /// </summary>
    /// <param name="b">Blank Node.</param>
    /// <param name="segment">Triple Segment.</param>
    /// <returns></returns>
    protected override string FormatBlankNode(IBlankNode b, TripleSegment? segment)
    {
        return "_:" + _bnodeMapper.GetOutputID(b.InternalID);
    }

    /// <inheritdoc/>
    public override string FormatUri(Uri u)
    {
        if (!u.IsAbsoluteUri)
        {
            throw new ArgumentException("IRIs to be formatted by the NTriplesFormatter must be absolute IRIs");
        }

        //if (u.IsWellFormedOriginalString())
        //{
        //    return FormatUri(u.ToString());
        //}

        var uriString = u.ToString();
        if (uriString.EndsWith("/") && !u.OriginalString.EndsWith("/"))
            uriString = uriString.Substring(0, uriString.Length - 1);
        else if (!uriString.EndsWith("/") && u.OriginalString.EndsWith("/"))
            uriString += '/';
        else if (uriString.Contains("/#") && !u.OriginalString.Contains("/#"))
            uriString = uriString.Replace("/#", "#");
        else if (!uriString.Contains("/#") && u.OriginalString.Contains("/#"))
            uriString = uriString.Replace("#", "/#");

        return FormatUri(Rfc3987Formatter.EscapeUriString(uriString));
    }

    /// <inheritdoc />
    public override string FormatUri(string u)
    {
        return FormatChar(u.ToCharArray());
    }

    /// <summary>
    /// Matches the beginning of all lines.
    /// </summary>
    static readonly Regex commentLineRegex = new Regex(@"^", RegexOptions.Compiled | RegexOptions.Multiline);

    /// <inheritdoc />
    public virtual string FormatComment(string text)
    {
        return commentLineRegex.Replace(text, "#");
    }
}

/// <summary>
/// Formatter for formatting as NTriples according to the RDF 1.1 specification.
/// </summary>
/// <remarks>The primary difference between this formatter and <see cref="NTriplesFormatter"/> is that this formatter will drop the xsd:string datatype IRI from literals as this is
/// the default datatype assigned to string literals by the RDF 1.1 specification.</remarks>
public class NTriples11Formatter
    : NTriplesFormatter
{
    /// <summary>
    /// Creates a new formatter.
    /// </summary>
    public NTriples11Formatter()
        : base(NTriplesSyntax.Rdf11) { }

    /// <summary>
    /// Return the datatype specification for a literal value.
    /// </summary>
    /// <param name="datatypeUri">The datatype URI.</param>
    /// <returns>The formatted datatype specification unless <paramref name="datatypeUri"/> matches the XML Schema String datatype URI, in which case an empty string is returned.</returns>
    protected override string FormatDatatype(Uri datatypeUri)
    {
        return datatypeUri.AbsoluteUri.Equals(XmlSpecsHelper.XmlSchemaDataTypeString) ? string.Empty : base.FormatDatatype(datatypeUri);
    }
}
