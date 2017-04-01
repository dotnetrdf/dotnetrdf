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

namespace VDS.RDF.Parsing.Tokens
{
    #region Query Keyword Tokens

    /// <summary>
    /// Token which represents the SPARQL SELECT Keyword
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
    /// Token which represents the SPARQL ASK Keyword
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
    /// Token which represents the SPARQL DESCRIBE Keyword
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
    /// Token which represents the SPARQL CONSTRUCT Keyword
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
    /// Token which represents the SPARQL ABS Keyword
    /// </summary>
    public class AbsKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new ABS Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public AbsKeywordToken(int line, int pos) : base(Token.ABS, "ABS", line, line, pos, pos + 3) { }
    }

    /// <summary>
    /// Token which represents the SPARQL ALL Keyword
    /// </summary>
    public class AllKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new ALL Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public AllKeywordToken(int line, int pos) : base(Token.ALLWORD, "ALL", line, line, pos, pos + 3) { }
    }

    /// <summary>
    /// Token which represents the SPARQL AS Keyword
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
    /// Token which represents the SPARQL ASC Keyword
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
    /// Token which represents the SPARQL AVG Keyword
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
    /// Token which represents the SPARQL BIND Keyword
    /// </summary>
    public class BindKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new BIND Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public BindKeywordToken(int line, int pos) : base(Token.BIND, "BIND", line, line, pos, pos + 4) { }
    }

    /// <summary>
    /// Token which represents the SPARQL BINDINGS Keyword
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
    /// Token which represents the SPARQL BNODE Keyword
    /// </summary>
    public class BNodeKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new BNODE Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public BNodeKeywordToken(int line, int pos) : base(Token.BNODE, "BNODE", line, line, pos, pos + 4) { }
    }

    /// <summary>
    /// Token which represents the SPARQL BOUND Keyword
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
    /// Token which represents the SPARQL CALL Keyword
    /// </summary>
    public class CallKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new CALL Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public CallKeywordToken(int line, int pos) : base(Token.CALL, "CALL", line, line, pos, pos + 4) { }
    }

    /// <summary>
    /// Token which represents the SPARQL CEIL Keyword
    /// </summary>
    public class CeilKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new CEIL Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public CeilKeywordToken(int line, int pos) : base(Token.CEIL, "CEIL", line, line, pos, pos + 4) { }
    }

    /// <summary>
    /// Token which represents the SPARQL COALESCE Keyword
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
    /// Token which represents the SPARQL CONCAT Keyword
    /// </summary>
    public class ConcatKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new CONCAT Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public ConcatKeywordToken(int line, int pos) : base(Token.CONCAT, "CONCAT", line, line, pos, pos + 6) { }
    }

    /// <summary>
    /// Token which represents the SPARQL COUNT Keyword
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
    /// Token which represents the SPARQL DATATYPE Keyword
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
    /// Token which represents the SPARQL DAY Keyword
    /// </summary>
    public class DayKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new DAY Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public DayKeywordToken(int line, int pos) : base(Token.DAY, "DAY", line, line, pos, pos + 3) { }
    }

    /// <summary>
    /// Token which represents the SPARQL DESC Keyword
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
    /// Token which represents the SPARQL DISTINCT Keyword
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
    /// Token which represents the SPARQL ENCODE_FOR_URI Keyword
    /// </summary>
    public class EncodeForUriKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new ENCODE_FOR_URI Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public EncodeForUriKeywordToken(int line, int pos) : base(Token.ENCODEFORURI, "ENCODE_FOR_URI", line, line, pos, pos + 14) { }
    }

    /// <summary>
    /// Token which represents the SPARQL EXISTS Keyword
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
    /// Token which represents the SPARQL FILTER Keyword
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
    /// Token which represents the SPARQL FLOOR Keyword
    /// </summary>
    public class FloorKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new FLOOR Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public FloorKeywordToken(int line, int pos) : base(Token.FLOOR, "FLOOR", line, line, pos, pos + 5) { }
    }

    /// <summary>
    /// Token which represents the SPARQL FROM Keyword
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
    /// Token which represents the SPARQL FROM NAMED Keyword combination
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
    /// Token which represents the SPARQL GRAPH Keyword
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
    /// Token which represents the SPARQL GROUP BY Keyword
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
    /// Token which represents the SPARQL GROUP_CONCAT Keyword
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
    /// Token which represents the SPARQL HAVING Keyword
    /// </summary>
    public class HavingKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new HAVING Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public HavingKeywordToken(int line, int pos) : base(Token.HAVING, "HAVING", line, line, pos, pos + 6) { }
    }

    /// <summary>
    /// Token which represents the SPARQL HOURS Keyword
    /// </summary>
    public class HoursKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new HOURS Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public HoursKeywordToken(int line, int pos) : base(Token.HOURS, "HOURS", line, line, pos, pos + 5) { }
    }

    /// <summary>
    /// Token which represents the SPARQL IF Keyword
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
    /// Token which represents the SPARQL IN Keyword
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
    /// Token which represents the SPARQL IRI Keyword
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
    /// Token which represents the SPARQL ISBLANK Keyword
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
    /// Token which represents the SPARQL ISIRI Keyword
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
    /// Token which represents the SPARQL ISLITERAL Keyword
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
    /// Token which represents the SPARQL ISNUMERIC Keyword
    /// </summary>
    public class IsNumericKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new ISNUMERIC Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public IsNumericKeywordToken(int line, int pos) : base(Token.ISNUMERIC, "ISNUMERIC", line, line, pos, pos + 9) { }
    }

    /// <summary>
    /// Token which represents the SPARQL ISURI Keyword
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
    /// Token which represents the SPARQL LANG Keyword
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
    /// Token which represents the SPARQL LANGMATCHES Keyword
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
    /// Token which represents the SPARQL LCASE Keyword
    /// </summary>
    public class LCaseKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new LCASE Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public LCaseKeywordToken(int line, int pos) : base(Token.LCASE, "LCASE", line, line, pos, pos + 5) { }
    }

    /// <summary>
    /// Token which represents the SPARQL LENGTH Keyword
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
    /// Token which represents the SPARQL LET Keyword
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
    /// Token which represents the SPARQL LIMIT Keyword
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
    /// Token which represents the SPARQL MAX Keyword
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
    /// Token which represents the SPARQL MD5 Keyword
    /// </summary>
    public class MD5KeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new MD5 Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public MD5KeywordToken(int line, int pos) : base(Token.MD5, "MD5", line, line, pos, pos + 3) { }
    }

    /// <summary>
    /// Token which represents the SPARQL MEDIAN Keyword
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
    /// Token which represents the SPARQL MIN Keyword
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
    /// Token which represents the SPARQL MINUTES Keyword
    /// </summary>
    public class MinutesKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new MINUTES Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public MinutesKeywordToken(int line, int pos) : base(Token.MINUTES, "MINUTES", line, line, pos, pos + 7) { }
    }

    /// <summary>
    /// Token which represents the SPARQL MINUS Keyword
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
    /// Token which represents the SPARQL MODE Keyword
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
    /// Token which represents the SPARQL MONTH Keyword
    /// </summary>
    public class MonthKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new MONTH Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public MonthKeywordToken(int line, int pos) : base(Token.MONTH, "MONTH", line, line, pos, pos + 5) { }
    }

    /// <summary>
    /// Token which represents the SPARQL NAMED Keyword
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
    /// Token which represents the SPARQL NOT IN Keyword
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
    /// Token which represents the SPARQL NMAX Keyword
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
    /// Token which represents the SPARQL NMIN Keyword
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
    /// Token which represents the SPARQL NOT EXISTS Keyword
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
    /// Token which represents the SPARQL NOW Keyword
    /// </summary>
    public class NowKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new NOW Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public NowKeywordToken(int line, int pos) : base(Token.NOW, "NOW", line, line, pos, pos + 3) { }
    }

    /// <summary>
    /// Token which represents the SPARQL OFFSET Keyword
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
    /// Token which represents the SPARQL OPTIONAL Keyword
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
    /// Token which represents the SPARQL ORDER BY Keyword combination
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
    /// Token which represents the SPARQL RAND Keyword
    /// </summary>
    public class RandKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new RAND Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public RandKeywordToken(int line, int pos) : base(Token.RAND, "RAND", line, line, pos, pos + 4) { }
    }

    /// <summary>
    /// Token which represents the SPARQL REDUCED Keyword
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
    /// Token which represents the SPARQL REGEX Keyword
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
    /// Token which represents the SPARQL REPLACE Keyword
    /// </summary>
    public class ReplaceKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new REPLACE Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public ReplaceKeywordToken(int line, int pos) : base(Token.REPLACE, "REPLACE", line, line, pos, pos + 7) { }
    }

    /// <summary>
    /// Token which represents the SPARQL ROUND Keyword
    /// </summary>
    public class RoundKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new ROUND Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public RoundKeywordToken(int line, int pos) : base(Token.ROUND, "ROUND", line, line, pos, pos + 5) { }
    }

    /// <summary>
    /// Token which represents the SPARQL SAMETERM Keyword
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
    /// Token which represents the SPARQL SAMPLE Keyword
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
    /// Token which represents the SPARQL SECONDS Keyword
    /// </summary>
    public class SecondsKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new SECONDS Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public SecondsKeywordToken(int line, int pos) : base(Token.SECONDS, "SECONDS", line, line, pos, pos + 7) { }
    }

    /// <summary>
    /// Token which represents the SPARQL SEPARATOR Keyword
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
    /// Token which represents the SPARQL SERVICE Keyword
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
    /// Token which represents the SPARQL SHA1 Keyword
    /// </summary>
    public class Sha1KeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new SHA1 Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public Sha1KeywordToken(int line, int pos) : base(Token.SHA1, "SHA1", line, line, pos, pos + 4) { }
    }

    /// <summary>
    /// Token which represents the SPARQL SHA224 Keyword
    /// </summary>
    public class Sha224KeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new SHA224 Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public Sha224KeywordToken(int line, int pos) : base(Token.SHA224, "SHA224", line, line, pos, pos + 6) { }
    }

    /// <summary>
    /// Token which represents the SPARQL SHA256 Keyword
    /// </summary>
    public class Sha256KeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new SHA256 Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public Sha256KeywordToken(int line, int pos) : base(Token.SHA256, "SHA256", line, line, pos, pos + 6) { }
    }

    /// <summary>
    /// Token which represents the SPARQL SHA384 Keyword
    /// </summary>
    public class Sha384KeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new SHA384 Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public Sha384KeywordToken(int line, int pos) : base(Token.SHA384, "SHA384", line, line, pos, pos + 6) { }
    }

    /// <summary>
    /// Token which represents the SPARQL SHA512 Keyword
    /// </summary>
    public class Sha512KeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new SHA512 Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public Sha512KeywordToken(int line, int pos) : base(Token.SHA512, "SHA512", line, line, pos, pos + 6) { }
    }

    /// <summary>
    /// Token which represents the SPARQL STR Keyword
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
    /// Token which represents the SPARQL STRAFTER Keyword
    /// </summary>
    public class StrAfterKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new STRAFTER Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public StrAfterKeywordToken(int line, int pos) : base(Token.STRAFTER, "STRAFTER", line, line, pos, pos + 8) { }
    }

    /// <summary>
    /// Token which represents the SPARQL STRBEFORE Keyword
    /// </summary>
    public class StrBeforeKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new STRBEFORE Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public StrBeforeKeywordToken(int line, int pos) : base(Token.STRBEFORE, "STRBEFORE", line, line, pos, pos + 9) { }
    }

    /// <summary>
    /// Token which represents the SPARQL CONTAINS Keyword
    /// </summary>
    public class StrContainsKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new CONTAINS Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public StrContainsKeywordToken(int line, int pos) : base(Token.CONTAINS, "CONTAINS", line, line, pos, pos + 11) { }
    }

    /// <summary>
    /// Token which represents the SPARQL STRDT Keyword
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
    /// Token which represents the SPARQL STRENDS Keyword
    /// </summary>
    public class StrEndsKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new STRENDS Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public StrEndsKeywordToken(int line, int pos) : base(Token.STRENDS, "STRENDS", line, line, pos, pos + 7) { }
    }

    /// <summary>
    /// Token which represents the SPARQL STRLANG Keyword
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
    /// Token which represents the SPARQL STRLEN Keyword
    /// </summary>
    public class StrLenKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new STRLEN Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public StrLenKeywordToken(int line, int pos) : base(Token.STRLEN, "STRLEN", line, line, pos, pos + 6) { }
    }

    /// <summary>
    /// Token which represents the SPARQL STRSTARTS Keyword
    /// </summary>
    public class StrStartsKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new STRSTARTS Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public StrStartsKeywordToken(int line, int pos) : base(Token.STRSTARTS, "STRSTARTS", line, line, pos, pos + 9) { }
    }

    /// <summary>
    /// Token which represents the SPARQL STRUUID Keyword
    /// </summary>
    public class StrUUIDKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new STRUUID Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public StrUUIDKeywordToken(int line, int pos) : base(Token.STRUUID, "STRUUID", line, line, pos, pos + 7) { }
    }

    /// <summary>
    /// Token which represents the SPARQL SUBSTR Keyword
    /// </summary>
    public class SubStrKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new SUBSTR Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public SubStrKeywordToken(int line, int pos) : base(Token.SUBSTR, "SUBSTR", line, line, pos, pos + 6) { }
    }

    /// <summary>
    /// Token which represents the SPARQL SUM Keyword
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
    /// Token which represents the SPARQL TIMEZONE Keyword
    /// </summary>
    public class TimezoneKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new TIMEZONE Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public TimezoneKeywordToken(int line, int pos) : base(Token.TIMEZONE, "TIMEZONE", line, line, pos, pos + 8) { }
    }

    /// <summary>
    /// Token which represents the SPARQL TZ Keyword
    /// </summary>
    public class TZKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new TZ Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public TZKeywordToken(int line, int pos) : base(Token.TZ, "TZ", line, line, pos, pos + 2) { }
    }

    /// <summary>
    /// Token which represents the SPARQL UCASE Keyword
    /// </summary>
    public class UCaseKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new UCASE Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public UCaseKeywordToken(int line, int pos) : base(Token.UCASE, "UCASE", line, line, pos, pos + 5) { }
    }

    /// <summary>
    /// Token which represents the SPARQL UNDEF Keyword
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
    /// Token which represents the SPARQL UNION Keyword
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
    /// Token which represents the SPARQL UNSAID Keyword
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
    /// Token which represents the SPARQL URI Keyword
    /// </summary>
    public class UriKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new URI Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public UriKeywordToken(int line, int pos) : base(Token.URIFUNC, "URI", line, line, pos, pos + 3) { }
    }

    /// <summary>
    /// Token which represents the SPARQL UUID Keyword
    /// </summary>
    public class UUIDKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new UUID Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public UUIDKeywordToken(int line, int pos) : base(Token.UUID, "UUID", line, line, pos, pos + 4) { }
    }

    /// <summary>
    /// Token which represents the SPARQL VALUES Keyword
    /// </summary>
    public class ValuesKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new VALUES Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="post">Position the Keyword occurs at</param>
        public ValuesKeywordToken(int line, int post) : base(Token.VALUES, "VALUES", line, line, post, post + 6) { }
    }

    /// <summary>
    /// Token which represents the SPARQL WHERE Keyword
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

    /// <summary>
    /// Token which represents the SPARQL YEAR Keyword
    /// </summary>
    public class YearKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new YEAR Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public YearKeywordToken(int line, int pos) : base(Token.YEAR, "YEAR", line, line, pos, pos + 4) { }
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