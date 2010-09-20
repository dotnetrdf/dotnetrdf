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

namespace VDS.RDF.Parsing.Tokens
{
    #region Query Keyword Tokens

    /// <summary>
    /// Token which represents the Sparql SELECT Keyword
    /// </summary>
    public class SelectKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new SELECT Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public SelectKeywordToken(int line, int pos) : base(Token.SELECT, "SELECT", line, line, pos, pos + 6) { }
    }

    /// <summary>
    /// Token which represents the Sparql ASK Keyword
    /// </summary>
    public class AskKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new ASK Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public AskKeywordToken(int line, int pos) : base(Token.ASK, "ASK", line, line, pos, pos + 3) { }
    }

    /// <summary>
    /// Token which represents the Sparql DESCRIBE Keyword
    /// </summary>
    public class DescribeKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new DESCRIBE Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public DescribeKeywordToken(int line, int pos) : base(Token.DESCRIBE, "DESCRIBE", line, line, pos, pos + 8) { }
    }

    /// <summary>
    /// Token which represents the Sparql CONSTRUCT Keyword
    /// </summary>
    public class ConstructKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new CONSTRUCT Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public ConstructKeywordToken(int line, int pos) : base(Token.CONSTRUCT, "CONSTRUCT", line, line, pos, pos + 9) { }
    }

    #endregion

    /// <summary>
    /// Token which represents the use of the * character to mean All
    /// </summary>
    public class AllToken : BaseToken
    {
        /// <summary>
        /// Creates a new All Token
        /// </summary>
        /// <param name="line">Line the * occurs on</param>
        /// <param name="pos">Position the * occurs at</param>
        public AllToken(int line, int pos) : base(Token.ALL, "*", line, line, pos, pos + 1) { }
    }

    #region Non-Query Keyword Tokens

    /// <summary>
    /// Token which represents the Sparql AS Keyword
    /// </summary>
    public class AsKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new AS Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public AsKeywordToken(int line, int pos) : base(Token.AS, "AS", line, line, pos, pos + 2) { }
    }

    /// <summary>
    /// Token which represents the Sparql ASC Keyword
    /// </summary>
    public class AscKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new ASC Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public AscKeywordToken(int line, int pos) : base(Token.ASC, "ASC", line, line, pos, pos + 3) { }
    }

    /// <summary>
    /// Token which represents the Sparql AVG Keyword
    /// </summary>
    public class AvgKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new AVG Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public AvgKeywordToken(int line, int pos) : base(Token.AVG, "AVG", line, line, pos, pos + 3) { }
    }

    /// <summary>
    /// Token which represents the Sparql BINDINGS Keyword
    /// </summary>
    public class BindingsKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new BINDINGS Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public BindingsKeywordToken(int line, int pos) : base(Token.BINDINGS, "BINDINGS", line, line, pos, pos + 8) { }
    }

    /// <summary>
    /// Token which represents the Sparql BOUND Keyword
    /// </summary>
    public class BoundKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new BOUND Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public BoundKeywordToken(int line, int pos) : base(Token.BOUND, "BOUND", line, line, pos, pos + 5) { }
    }

    /// <summary>
    /// Token which represents the Sparql COALESCE Keyword
    /// </summary>
    public class CoalesceKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new COALESCE Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public CoalesceKeywordToken(int line, int pos) : base(Token.COALESCE, "COALESCE", line, line, pos, pos + 8) { }
    }

    /// <summary>
    /// Token which represents the Sparql COUNT Keyword
    /// </summary>
    public class CountKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new COUNT Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public CountKeywordToken(int line, int pos) : base(Token.COUNT, "COUNT", line, line, pos, pos + 5) { }
    }

    /// <summary>
    /// Token which represents the Sparql DATATYPE Keyword
    /// </summary>
    public class DataTypeKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new DATATYPE Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public DataTypeKeywordToken(int line, int pos) : base(Token.DATATYPEFUNC, "DATATYPE", line, line, pos, pos + 8) { }
    }

    /// <summary>
    /// Token which represents the Sparql DESC Keyword
    /// </summary>
    public class DescKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new DESC Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public DescKeywordToken(int line, int pos) : base(Token.DESC, "DESC", line, line, pos, pos + 4) { }
    }

    /// <summary>
    /// Token which represents the Sparql DISTINCT Keyword
    /// </summary>
    public class DistinctKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new DISTINCT Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public DistinctKeywordToken(int line, int pos) : base(Token.DISTINCT, "DISTINCT", line, line, pos, pos + 8) { }
    }

    /// <summary>
    /// Token which represents the Sparql EXISTS Keyword
    /// </summary>
    public class ExistsKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new EXISTS Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public ExistsKeywordToken(int line, int pos) : base(Token.EXISTS, "EXISTS", line, line, pos, pos + 6) { }
    }

    /// <summary>
    /// Token which represents the Sparql FILTER Keyword
    /// </summary>
    public class FilterKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new FILTER Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public FilterKeywordToken(int line, int pos) : base(Token.FILTER, "FILTER", line, line, pos, pos + 6) { }
    }

    /// <summary>
    /// Token which represents the Sparql FROM Keyword
    /// </summary>
    public class FromKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new FROM Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public FromKeywordToken(int line, int pos) : base(Token.FROM, "FROM", line, line, pos, pos + 4) { }
    }

    /// <summary>
    /// Token which represents the Sparql FROM NAMED Keyword combination
    /// </summary>
    public class FromNamedKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new FROM NAMED Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public FromNamedKeywordToken(int line, int pos) : base(Token.FROMNAMED, "FROM NAMED", line, line, pos, pos + 10) { }
    }

    /// <summary>
    /// Token which represents the Sparql GRAPH Keyword
    /// </summary>
    public class GraphKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new GRAPH Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public GraphKeywordToken(int line, int pos) : base(Token.GRAPH, "GRAPH", line, line, pos, pos + 5) { }
    }

    /// <summary>
    /// Token which represents the Sparql GROUP BY Keyword
    /// </summary>
    public class GroupByKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new GROUP BY Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public GroupByKeywordToken(int line, int pos) : base(Token.GROUPBY, "GROUP BY", line, line, pos, pos + 8) { }
    }

    /// <summary>
    /// Token which represents the Sparql GROUP_CONCAT Keyword
    /// </summary>
    public class GroupConcatKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new GROUP_CONCAT Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public GroupConcatKeywordToken(int line, int pos) : base(Token.GROUPCONCAT, "GROUP_CONCAT", line, line, pos, pos + 12) { }
    }

    /// <summary>
    /// Token which represents the Sparql HAVING Keyword
    /// </summary>
    public class HavingKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new HAVING Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public HavingKeywordToken(int line, int pos) : base(Token.HAVING, "HAVING", line, line, pos, pos + 8) { }
    }

    /// <summary>
    /// Token which represents the Sparql IF Keyword
    /// </summary>
    public class IfKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new IF Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public IfKeywordToken(int line, int pos) : base(Token.IF, "IF", line, line, pos, pos + 2) { }
    }

    /// <summary>
    /// Token which represents the Sparql IN Keyword
    /// </summary>
    public class InKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new IN Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public InKeywordToken(int line, int pos) : base(Token.IN, "IN", line, line, pos, pos + 2) { }
    }

    /// <summary>
    /// Token which represents the Sparql IRI Keyword
    /// </summary>
    public class IriKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new IRI Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public IriKeywordToken(int line, int pos) : base(Token.IRI, "IRI", line, line, pos, pos + 3) { }
    }

    /// <summary>
    /// Token which represents the Sparql ISBLANK Keyword
    /// </summary>
    public class IsBlankKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new ISBLANK Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public IsBlankKeywordToken(int line, int pos) : base(Token.ISBLANK, "isBlank", line, line, pos, pos + 7) { }
    }

    /// <summary>
    /// Token which represents the Sparql ISIRI Keyword
    /// </summary>
    public class IsIriKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new ISIRI Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public IsIriKeywordToken(int line, int pos) : base(Token.ISIRI, "isIRI", line, line, pos, pos + 5) { }
    }

    /// <summary>
    /// Token which represents the Sparql ISLITERAL Keyword
    /// </summary>
    public class IsLiteralKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new ISLITERAL Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public IsLiteralKeywordToken(int line, int pos) : base(Token.ISLITERAL, "isLiteral", line, line, pos, pos + 8) { }
    }

    /// <summary>
    /// Token which represents the Sparql ISURI Keyword
    /// </summary>
    public class IsUriKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new ISURI Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public IsUriKeywordToken(int line, int pos) : base(Token.ISURI, "isURI", line, line, pos, pos + 5) { }
    }

    /// <summary>
    /// Token which represents the Sparql LANG Keyword
    /// </summary>
    public class LangKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new LANG Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public LangKeywordToken(int line, int pos) : base(Token.LANG, "LANG", line, line, pos, pos + 4) { }
    }

    /// <summary>
    /// Token which represents the Sparql LANGMATCHES Keyword
    /// </summary>
    public class LangMatchesKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new LANGMATCHES Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public LangMatchesKeywordToken(int line, int pos) : base(Token.LANGMATCHES, "LANGMATCHES", line, line, pos, pos + 11) { }
    }

    /// <summary>
    /// Token which represents the Sparql LENGTH Keyword
    /// </summary>
    public class LengthKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new LENGTH Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public LengthKeywordToken(int line, int pos) : base(Token.LENGTH, "LENGTH", line, line, pos, pos + 6) { }
    }

    /// <summary>
    /// Token which represents the Sparql LET Keyword
    /// </summary>
    public class LetKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new LET Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public LetKeywordToken(int line, int pos) : base(Token.LET, "LET", line, line, pos, pos + 3) { }
    }

    /// <summary>
    /// Token which represents the Sparql LIMIT Keyword
    /// </summary>
    public class LimitKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new LIMIT Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public LimitKeywordToken(int line, int pos) : base(Token.LIMIT, "LIMIT", line, line, pos, pos + 5) { }
    }

    /// <summary>
    /// Token which represents the Sparql MAX Keyword
    /// </summary>
    public class MaxKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new MAX Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public MaxKeywordToken(int line, int pos) : base(Token.MAX, "MAX", line, line, pos, pos + 3) { }
    }

    /// <summary>
    /// Token which represents the Sparql MEDIAN Keyword
    /// </summary>
    public class MedianKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new MEDIAN Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public MedianKeywordToken(int line, int pos) : base(Token.MEDIAN, "MEDIAN", line, line, pos, pos + 6) { }
    }

    /// <summary>
    /// Token which represents the Sparql MIN Keyword
    /// </summary>
    public class MinKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new MIN Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public MinKeywordToken(int line, int pos) : base(Token.MIN, "MIN", line, line, pos, pos + 3) { }
    }

    /// <summary>
    /// Token which represents the Sparql MINUS Keyword
    /// </summary>
    public class MinusKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new MINUS Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public MinusKeywordToken(int line, int pos) : base(Token.MINUS_P, "MINUS", line, line, pos, pos + 5) { }
    }

    /// <summary>
    /// Token which represents the Sparql MODE Keyword
    /// </summary>
    public class ModeKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new MODE Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public ModeKeywordToken(int line, int pos) : base(Token.MODE, "MODE", line, line, pos, pos + 4) { }
    }

    /// <summary>
    /// Token which represents the Sparql NAMED Keyword
    /// </summary>
    public class NamedKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new NAMED Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public NamedKeywordToken(int line, int pos) : base(Token.NAMED, "NAMED", line, line, pos, pos + 5) { }
    }

    /// <summary>
    /// Token which represents the Sparql NOT IN Keyword
    /// </summary>
    public class NotInKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new NOT IN Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public NotInKeywordToken(int line, int pos) : base(Token.NOTIN, "NOT IN", line, line, pos, pos + 6) { }
    }

    /// <summary>
    /// Token which represents the Sparql NMAX Keyword
    /// </summary>
    public class NumericMaxKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new NMAX Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public NumericMaxKeywordToken(int line, int pos) : base(Token.NMAX, "NMAX", line, line, pos, pos + 4) { }
    }

    /// <summary>
    /// Token which represents the Sparql NMIN Keyword
    /// </summary>
    public class NumericMinKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new NMIN Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public NumericMinKeywordToken(int line, int pos) : base(Token.NMIN, "NMIN", line, line, pos, pos + 4) { }
    }

    /// <summary>
    /// Token which represents the Sparql NOT EXISTS Keyword
    /// </summary>
    public class NotExistsKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new NOT EXISTS Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public NotExistsKeywordToken(int line, int pos) : base(Token.NOTEXISTS, "NOT EXISTS", line, line, pos, pos + 10) { }
    }

    /// <summary>
    /// Token which represents the Sparql OFFSET Keyword
    /// </summary>
    public class OffsetKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new OFFSET Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public OffsetKeywordToken(int line, int pos) : base(Token.OFFSET, "OFFSET", line, line, pos, pos + 6) { }
    }

    /// <summary>
    /// Token which represents the Sparql OPTIONAL Keyword
    /// </summary>
    public class OptionalKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new OPTIONAL Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public OptionalKeywordToken(int line, int pos) : base(Token.OPTIONAL, "OPTIONAL", line, line, pos, pos + 8) { }
    }

    /// <summary>
    /// Token which represents the Sparql ORDER BY Keyword combination
    /// </summary>
    public class OrderByKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new ORDER BY Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public OrderByKeywordToken(int line, int pos) : base(Token.ORDERBY, "ORDER BY", line, line, pos, pos + 8) { }
    }

    /// <summary>
    /// Token which represents the Sparql REDUCED Keyword
    /// </summary>
    public class ReducedKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new REDUCED Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public ReducedKeywordToken(int line, int pos) : base(Token.REDUCED, "REDUCED", line, line, pos, pos + 7) { }
    }

    /// <summary>
    /// Token which represents the Sparql REGEX Keyword
    /// </summary>
    public class RegexKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new REGEX Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public RegexKeywordToken(int line, int pos) : base(Token.REGEX, "REGEX", line, line, pos, pos + 5) { }
    }

    /// <summary>
    /// Token which represents the Sparql SAMETERM Keyword
    /// </summary>
    public class SameTermKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new SAMETERM Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public SameTermKeywordToken(int line, int pos) : base(Token.SAMETERM, "sameTerm", line, line, pos, pos + 8) { }
    }

    /// <summary>
    /// Token which represents the Sparql SAMPLE Keyword
    /// </summary>
    public class SampleKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new SAMPLE Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public SampleKeywordToken(int line, int pos) : base(Token.SAMPLE, "SAMPLE", line, line, pos, pos + 6) { }
    }

    /// <summary>
    /// Token which represents the Sparql SEPARATOR Keyword
    /// </summary>
    public class SeparatorKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new SEPARATOR Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public SeparatorKeywordToken(int line, int pos) : base(Token.SEPARATOR, "SEPARATOR", line, line, pos, pos + 9) { }
    }

    /// <summary>
    /// Token which represents the Sparql SERVICE Keyword
    /// </summary>
    public class ServiceKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new SERVICE Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public ServiceKeywordToken(int line, int pos) : base(Token.SERVICE, "SERVICE", line, line, pos, pos + 7) { }
    }

    /// <summary>
    /// Token which represents the Sparql STR Keyword
    /// </summary>
    public class StrKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new STR Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public StrKeywordToken(int line, int pos) : base(Token.STR, "STR", line, line, pos, pos + 3) { }
    }

    /// <summary>
    /// Token which represents the Sparql STRDT Keyword
    /// </summary>
    public class StrDtKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new STRDT Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public StrDtKeywordToken(int line, int pos) : base(Token.STRDT, "STRDT", line, line, pos, pos + 5) { }
    }

    /// <summary>
    /// Token which represents the Sparql STRLANG Keyword
    /// </summary>
    public class StrLangKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new STRLANG Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public StrLangKeywordToken(int line, int pos) : base(Token.STRLANG, "STRLANG", line, line, pos, pos + 7) { }
    }

    /// <summary>
    /// Token which represents the Sparql SUM Keyword
    /// </summary>
    public class SumKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new SUM Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public SumKeywordToken(int line, int pos) : base(Token.SUM, "SUM", line, line, pos, pos + 3) { }
    }

    /// <summary>
    /// Token which represents the Sparql UNDEF Keyword
    /// </summary>
    public class UndefKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new UNDEF Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public UndefKeywordToken(int line, int pos) : base(Token.UNDEF, "UNDEF", line, line, pos, pos + 5) { }
    }

    /// <summary>
    /// Token which represents the Sparql UNION Keyword
    /// </summary>
    public class UnionKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new UNION Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public UnionKeywordToken(int line, int pos) : base(Token.UNION, "UNION", line, line, pos, pos + 5) { }
    }

    /// <summary>
    /// Token which represents the Sparql UNSAID Keyword
    /// </summary>
    public class UnsaidKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new UNSAID Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public UnsaidKeywordToken(int line, int pos) : base(Token.UNSAID, "UNSAID", line, line, pos, pos + 6) { }
    }

    /// <summary>
    /// Token which represents the Sparql URI Keyword
    /// </summary>
    public class UriKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new URI Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public UriKeywordToken(int line, int pos) : base(Token.URI, "URI", line, line, pos, pos + 3) { }
    }

    /// <summary>
    /// Token which represents the Sparql WHERE Keyword
    /// </summary>
    public class WhereKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new WHERE Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public WhereKeywordToken(int line, int pos) : base(Token.WHERE, "WHERE", line, line, pos, pos + 5) { }
    }

    #endregion

    #region Math and Logic Tokens

    /// <summary>
    /// Token which represents Mathematical Plus
    /// </summary>
    public class PlusToken : BaseToken
    {
        /// <summary>
        /// Creates a new Mathematical Plus Token
        /// </summary>
        /// <param name="line">Line the Token occurs on</param>
        /// <param name="pos">Position the Token occurs at</param>
        public PlusToken(int line, int pos) : base(Token.PLUS, "+", line, line, pos, pos + 1) { }
    }

    /// <summary>
    /// Token which represents Mathematical Minus
    /// </summary>
    public class MinusToken : BaseToken
    {
        /// <summary>
        /// Creates a new Mathematical Minus Token
        /// </summary>
        /// <param name="line">Line the Token occurs on</param>
        /// <param name="pos">Position the Token occurs at</param>
        public MinusToken(int line, int pos) : base(Token.MINUS, "-", line, line, pos, pos + 1) { }
    }

    /// <summary>
    /// Token which represents Mathematical Multiply
    /// </summary>
    public class MultiplyToken : BaseToken
    {
        /// <summary>
        /// Creates a new Mathematical Multiply Token
        /// </summary>
        /// <param name="line">Line the Token occurs on</param>
        /// <param name="pos">Position the Token occurs at</param>
        public MultiplyToken(int line, int pos) : base(Token.MULTIPLY, "*", line, line, pos, pos + 1) { }
    }

    /// <summary>
    /// Token which represents Mathematical Divide
    /// </summary>
    public class DivideToken : BaseToken
    {
        /// <summary>
        /// Creates a new Mathematical Divide Token
        /// </summary>
        /// <param name="line">Line the Token occurs on</param>
        /// <param name="pos">Position the Token occurs at</param>
        public DivideToken(int line, int pos) : base(Token.DIVIDE, "/", line, line, pos, pos + 1) { }
    }

    /// <summary>
    /// Token which represents Logical Not Equals
    /// </summary>
    public class NotEqualsToken : BaseToken
    {
        /// <summary>
        /// Creates a new Logical Not Equals Token
        /// </summary>
        /// <param name="line">Line the Token occurs on</param>
        /// <param name="pos">Position the Token occurs at</param>
        public NotEqualsToken(int line, int pos) : base(Token.NOTEQUALS, "!=", line, line, pos, pos + 2) { }
    }

    /// <summary>
    /// Token which represents Logical Negation
    /// </summary>
    public class NegationToken : BaseToken
    {
        /// <summary>
        /// Creates a new Logical Negation Token
        /// </summary>
        /// <param name="line">Line the Token occurs on</param>
        /// <param name="pos">Position the Token occurs at</param>
        public NegationToken(int line, int pos) : base(Token.NEGATION, "!", line, line, pos, pos + 1) { }
    }

    /// <summary>
    /// Token which represents Logical And
    /// </summary>
    public class AndToken : BaseToken
    {
        /// <summary>
        /// Creates a new Logical And Token
        /// </summary>
        /// <param name="line">Line the Token occurs on</param>
        /// <param name="pos">Position the Token occurs at</param>
        public AndToken(int line, int pos) : base(Token.AND, "&&", line, line, pos, pos + 2) { }
    }

    /// <summary>
    /// Token which represents Logical Or
    /// </summary>
    public class OrToken : BaseToken
    {
        /// <summary>
        /// Creates a new Logical Or Token
        /// </summary>
        /// <param name="line">Line the Token occurs on</param>
        /// <param name="pos">Position the Token occurs at</param>
        public OrToken(int line, int pos) : base(Token.OR, "||", line, line, pos, pos + 2) { }
    }

    /// <summary>
    /// Token which represents Relational Less Than
    /// </summary>
    public class LessThanToken : BaseToken
    {
        /// <summary>
        /// Creates a new Relation Less Than Token
        /// </summary>
        /// <param name="line">Line the Token occurs on</param>
        /// <param name="pos">Position the Token occurs at</param>
        public LessThanToken(int line, int pos) : base(Token.LESSTHAN, "<", line, line, pos, pos + 1) { }
    }

    /// <summary>
    /// Token which represents Relational Less Than or Equal To
    /// </summary>
    public class LessThanOrEqualToToken : BaseToken
    {
        /// <summary>
        /// Creates a new Relation Less Than or Equal To Token
        /// </summary>
        /// <param name="line">Line the Token occurs on</param>
        /// <param name="pos">Position the Token occurs at</param>
        public LessThanOrEqualToToken(int line, int pos) : base(Token.LESSTHANOREQUALTO, "<=", line, line, pos, pos + 2) { }
    }

    /// <summary>
    /// Token which represents Relational Greater Than
    /// </summary>
    public class GreaterThanToken : BaseToken
    {
        /// <summary>
        /// Creates a new Relation Greater Than Token
        /// </summary>
        /// <param name="line">Line the Token occurs on</param>
        /// <param name="pos">Position the Token occurs at</param>
        public GreaterThanToken(int line, int pos) : base(Token.GREATERTHAN, ">", line, line, pos, pos + 1) { }
    }

    /// <summary>
    /// Token which represents Greater Than or Equal To
    /// </summary>
    public class GreaterThanOrEqualToToken : BaseToken
    {
        /// <summary>
        /// Creates a new Relation Greater Than or Equal To Token
        /// </summary>
        /// <param name="line">Line the Token occurs on</param>
        /// <param name="pos">Position the Token occurs at</param>
        public GreaterThanOrEqualToToken(int line, int pos) : base(Token.GREATERTHANOREQUALTO, ">=", line, line, pos, pos + 2) { }
    }

    #endregion
}