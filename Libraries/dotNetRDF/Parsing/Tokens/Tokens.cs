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

namespace VDS.RDF.Parsing.Tokens
{

// Definitions for lots and lots of different Tokens
// Not all of these are necessary used but it's useful to have them defined

#region Punctuation Tokens

    /// <summary>
    /// Token which represents the Start of the Input
    /// </summary>
    public class BOFToken : BaseToken
    {
        /// <summary>
        /// Creates a new Beginning of File Token
        /// </summary>
        public BOFToken() : base(Token.BOF, String.Empty, 1, 1, 1, 1) { }
    }

    /// <summary>
    /// Token which represents the End of the Input
    /// </summary>
    public class EOFToken : BaseToken
    {
        /// <summary>
        /// Creates a new End of File Token
        /// </summary>
        /// <param name="line">Line at which the File Ends</param>
        /// <param name="pos">Column as which the File Ends</param>
        public EOFToken(int line, int pos) : base(Token.EOF, String.Empty, line, line, pos, pos) { }
    }

    /// <summary>
    /// Token which represents the End of a Line
    /// </summary>
    public class EOLToken : BaseToken
    {
        /// <summary>
        /// Creates a new End of Line Token
        /// </summary>
        /// <param name="line">Line</param>
        /// <param name="pos">Column at which the line ends</param>
        public EOLToken(int line, int pos) : base(Token.EOL, String.Empty, line, line, pos, pos+1) { }
    }

    /// <summary>
    /// Token which represents the @ Character
    /// </summary>
    public class ATToken : BaseToken
    {
        /// <summary>
        /// Creates a new @ Token
        /// </summary>
        /// <param name="line">Line at which the @ occurs</param>
        /// <param name="pos">Column at which the @ occurs</param>
        public ATToken(int line, int pos) : base(Token.AT, "@", line, line, pos, pos+1) { }
    }

    /// <summary>
    /// Token which represents the . Character
    /// </summary>
    public class DotToken : BaseToken
    {
        /// <summary>
        /// Creates a new . Token
        /// </summary>
        /// <param name="line">Line at which the . occurs</param>
        /// <param name="pos">Column at which the . occurs</param>
        public DotToken(int line, int pos) : base(Token.DOT, ".", line, line, pos, pos+1) { }
    }

    /// <summary>
    /// Token which represents the ; Character
    /// </summary>
    public class SemicolonToken : BaseToken
    {
        /// <summary>
        /// Creates a new ; Token
        /// </summary>
        /// <param name="line">Line at which the ; occurs</param>
        /// <param name="pos">Column at which the ; occurs</param>
        public SemicolonToken(int line, int pos) : base(Token.SEMICOLON, ";", line, line, pos, pos+1) { }
    }

    /// <summary>
    /// Token which represents the , Character
    /// </summary>
    public class CommaToken : BaseToken
    {
        /// <summary>
        /// Creates a new , Token
        /// </summary>
        /// <param name="line">Line at which the , occurs</param>
        /// <param name="pos">Column at which the , occurs</param>
        public CommaToken(int line, int pos) : base(Token.COMMA, ",", line, line, pos, pos+1) { }
    }

    /// <summary>
    /// Tokens which represents the tab character
    /// </summary>
    public class TabToken : BaseToken
    {
        /// <summary>
        /// Creates a new Tab Token
        /// </summary>
        /// <param name="line">Line at which the tab occurs</param>
        /// <param name="pos">Column at which the tab occurs</param>
        public TabToken(int line, int pos) : base(Token.TAB, "\t", line, line, pos, pos + 1) { }
    }

    /// <summary>
    /// Token which represents the # Character
    /// </summary>
    public class HashToken : BaseToken
    {
        /// <summary>
        /// Creates a new # Token
        /// </summary>
        /// <param name="line">Line at which the # occurs</param>
        /// <param name="pos">Column at which the # occurs</param>
        public HashToken(int line, int pos) : base(Token.HASH, "#", line, line, pos, pos+1) { }
    }

    /// <summary>
    /// Token which repreents the _ Character
    /// </summary>
    public class UnderscoreToken : BaseToken
    {
        /// <summary>
        /// Creates a new _ Token
        /// </summary>
        /// <param name="line">Line at which the _ occurs</param>
        /// <param name="pos">Column at which the _ occurs</param>
        public UnderscoreToken(int line, int pos) : base(Token.UNDERSCORE, "_", line, line, pos, pos+1) { }
    }

    /// <summary>
    /// Token which represents the ^^ sequence used for Data Type specification in some RDF Syntaxes
    /// </summary>
    public class HatHatToken : BaseToken
    {
        /// <summary>
        /// Creates a new ^^Token
        /// </summary>
        /// <param name="line">Line at which the ^^ occurs</param>
        /// <param name="pos">Column at which the ^^ occurs</param>
        public HatHatToken(int line, int pos) : base(Token.HATHAT, "^^", line, line, pos, pos + 2) { }
    }

    /// <summary>
    /// Token which represents the ^ Character used for Reverse Path Traversal in somme RDF Syntaxes
    /// </summary>
    public class HatToken : BaseToken
    {
        /// <summary>
        /// Creates a new ^ Token
        /// </summary>
        /// <param name="line">Line at which the ^ occurs</param>
        /// <param name="pos">Column at which the ^ occurs</param>
        public HatToken(int line, int pos) : base(Token.HAT, "^", line, line, pos, pos+1) { }
    }

    /// <summary>
    /// Token which represents the ! Character used for Forward Path Traversal in some RDF Syntaxes
    /// </summary>
    public class ExclamationToken : BaseToken
    {
        /// <summary>
        /// Creates a new ! Token
        /// </summary>
        /// <param name="line">Line at which the ! occurs</param>
        /// <param name="pos">Column at which the ! occurs</param>
        public ExclamationToken(int line, int pos) : base(Token.EXCLAMATION, "!", line, line, pos, pos+1) { }
    }

    /// <summary>
    /// Token which represents Comments
    /// </summary>
    public class CommentToken : BaseToken
    {
        /// <summary>
        /// Creates a new Comment Token
        /// </summary>
        /// <param name="value">The Comment</param>
        /// <param name="line">Line on which the Comment occurs</param>
        /// <param name="start">Column at which the Comment starts</param>
        /// <param name="end">Column at which the Comment ends</param>
        public CommentToken(String value, int line, int start, int end) : base(Token.COMMENT, value, line, line, start, end) { }
    }

    /// <summary>
    /// Token which represents the [ Character
    /// </summary>
    public class LeftSquareBracketToken : BaseToken
    {
        /// <summary>
        /// Creates a new [ Token
        /// </summary>
        /// <param name="line">Line at which the [ occurs</param>
        /// <param name="pos">Column at which the [ occurs</param>
        public LeftSquareBracketToken(int line, int pos) : base(Token.LEFTSQBRACKET, "[", line, line, pos, pos+1) { }
    }

    /// <summary>
    /// Token which represents the ] Character
    /// </summary>
    public class RightSquareBracketToken : BaseToken
    {
        /// <summary>
        /// Creates a new ] Token
        /// </summary>
        /// <param name="line">Line at which the ] occurs</param>
        /// <param name="pos">Column at which the ] occurs</param>
        public RightSquareBracketToken(int line, int pos) : base(Token.RIGHTSQBRACKET, "]", line, line, pos, pos+1) { }
    }

    /// <summary>
    /// Token which represents the ( Character
    /// </summary>
    public class LeftBracketToken : BaseToken
    {
        /// <summary>
        /// Creates a new ( Token
        /// </summary>
        /// <param name="line">Line at which the ( occurs</param>
        /// <param name="pos">Column at which the ( occurs</param>
        public LeftBracketToken(int line, int pos) : base(Token.LEFTBRACKET, "(", line, line, pos, pos+1) { }
    }

    /// <summary>
    /// Token which represents the ) Character
    /// </summary>
    public class RightBracketToken : BaseToken
    {
        /// <summary>
        /// Creates a new ) Token
        /// </summary>
        /// <param name="line">Line at which the ) occurs</param>
        /// <param name="pos">Column at which the ) occurs</param>
        public RightBracketToken(int line, int pos) : base(Token.RIGHTBRACKET, ")", line, line, pos, pos+1) { }
    }

    /// <summary>
    /// Token which represents the { Character
    /// </summary>
    public class LeftCurlyBracketToken : BaseToken
    {
        /// <summary>
        /// Creates a new { Token
        /// </summary>
        /// <param name="line">Line at which the { occurs</param>
        /// <param name="pos">Column at which the { occurs</param>
        public LeftCurlyBracketToken(int line, int pos) : base(Token.LEFTCURLYBRACKET, "{", line, line, pos, pos+1) { }
    }

    /// <summary>
    /// Token which represents the } Character
    /// </summary>
    public class RightCurlyBracketToken : BaseToken
    {
        /// <summary>
        /// Creates a new } Token
        /// </summary>
        /// <param name="line">Line at which the } occurs</param>
        /// <param name="pos">Column at which the } occurs</param>
        public RightCurlyBracketToken(int line, int pos) : base(Token.RIGHTCURLYBRACKET, "}", line, line, pos, pos+1) { }
    }

    /// <summary>
    /// Token which reprsents the := Assignment Operator
    /// </summary>
    public class AssignmentToken : BaseToken
    {
        /// <summary>
        /// Creates a new := Token
        /// </summary>
        /// <param name="line">Line on which the := occurs</param>
        /// <param name="post">Position at which the := occurs</param>
        public AssignmentToken(int line, int post) : base(Token.ASSIGNMENT, ":=", line, line, post, post + 2) { }
    }

    /// <summary>
    /// Token which represents the ? Character
    /// </summary>
    public class QuestionToken : BaseToken
    {
        /// <summary>
        /// Creates a new ? Token
        /// </summary>
        /// <param name="line">Line at which the ? occurs</param>
        /// <param name="pos">Column at which the ? occurs</param>
        public QuestionToken(int line, int pos) : base(Token.QUESTION, "?", line, line, pos, pos + 1) { }
    }

    /// <summary>
    /// Token which represents the | Character
    /// </summary>
    public class BitwiseOrToken : BaseToken
    {
        /// <summary>
        /// Creates a new | Token
        /// </summary>
        /// <param name="line">Line at which the | occurs</param>
        /// <param name="pos">Column at which the | occurs</param>
        public BitwiseOrToken(int line, int pos) : base(Token.BITWISEOR, "|", line, line, pos, pos + 1) { }
    }

#endregion

#region Directive Tokens

    /// <summary>
    /// Token which represents a Prefix Directive
    /// </summary>
    public class PrefixDirectiveToken : BaseToken
    {
        /// <summary>
        /// Creates a new Prefix Direction Token
        /// </summary>
        /// <param name="line">Line at which the Prefix Directive occurs</param>
        /// <param name="pos">Column at which the Prefix Directive occurs</param>
        public PrefixDirectiveToken(int line, int pos) : base(Token.PREFIXDIRECTIVE, "prefix", line, line, pos, pos + 6) { }
    }

    /// <summary>
    /// Token which represents the Prefix specified after a Prefix Directive
    /// </summary>
    public class PrefixToken : BaseToken
    {
        /// <summary>
        /// Creates a new Prefix Token
        /// </summary>
        /// <param name="value">Prefix</param>
        /// <param name="line">Line at which the Prefix occurs</param>
        /// <param name="start">Column at which the Prefix starts</param>
        /// <param name="end">Column at which the Prefix ends</param>
        public PrefixToken(String value, int line, int start, int end) : base(Token.PREFIX, value, line, line, start, end) { }
    }

    /// <summary>
    /// Token which represents a Base Directive
    /// </summary>
    public class BaseDirectiveToken : BaseToken
    {
        /// <summary>
        /// Creates a new Base Directive Token
        /// </summary>
        /// <param name="line">Line at which the Base Directive occurs</param>
        /// <param name="pos">Column at which the Base Directive occurs</param>
        public BaseDirectiveToken(int line, int pos) : base(Token.BASEDIRECTIVE, "base", line, line, pos, pos + 4) { }
    }

    /// <summary>
    /// Token which represents a Keyword Directive
    /// </summary>
    public class KeywordDirectiveToken : BaseToken
    {
        /// <summary>
        /// Creates a new Keyword Directive Token
        /// </summary>
        /// <param name="line">Line at which the Keyword Directive occurs</param>
        /// <param name="pos">Column at which the Keyword Directive occurs</param>
        public KeywordDirectiveToken(int line, int pos) : base(Token.KEYWORDDIRECTIVE, "keywords", line, line, pos, pos + 8) { }
    }

    /// <summary>
    /// Token which represents a For All Quantifier
    /// </summary>
    public class ForAllQuantifierToken : BaseToken
    {
        /// <summary>
        /// Creates a new For All Quantifier Token
        /// </summary>
        /// <param name="line">Line at which the For All Quantifier occurs</param>
        /// <param name="pos">Column at which the For All Quantifier occurs</param>
        public ForAllQuantifierToken(int line, int pos) : base(Token.FORALL, "forall", line, line, pos, pos + 6) { }
    }

    /// <summary>
    /// Token which represents a For Some Quantifier
    /// </summary>
    public class ForSomeQuantifierToken : BaseToken
    {
        /// <summary>
        /// Creates a new For Some Quantifier Token
        /// </summary>
        /// <param name="line">Line at which the For Some Quantifier occurs</param>
        /// <param name="pos">Column at which the For Some Quantifier occurs</param>
        public ForSomeQuantifierToken(int line, int pos) : base(Token.FORSOME, "forsome", line, line, pos, pos + 7) { }
    }

#endregion

#region Uri Tokens

    /// <summary>
    /// Token which represents URIs
    /// </summary>
    public class UriToken : BaseToken
    {
        /// <summary>
        /// Creates a new Uri Token
        /// </summary>
        /// <param name="value">Value of the Uri including the &lt; &gt; deliminators</param>
        /// <param name="line">Line the Uri occurs on</param>
        /// <param name="start">Column the Uri starts at</param>
        /// <param name="end">Column the Uri ends at</param>
        public UriToken(String value, int line, int start, int end) : base(Token.URI, value.Substring(1, value.Length - 2), line, line, start + 1, end - 1) { }
    }

    /// <summary>
    /// Token which represents QNames
    /// </summary>
    public class QNameToken : BaseToken
    {
        /// <summary>
        /// Creates a new QName Token
        /// </summary>
        /// <param name="value">QName</param>
        /// <param name="line">Line the QName occurs on</param>
        /// <param name="start">Column the QName starts at</param>
        /// <param name="end">Column the QName ends at</param>
        public QNameToken(String value, int line, int start, int end) : base(Token.QNAME, value, line, line, start, end) { }
    }

#endregion

#region Literal Tokens

    /// <summary>
    /// Token which represents Plain (Unquoted) Literals
    /// </summary>
    public class PlainLiteralToken : BaseToken
    {
        /// <summary>
        /// Creates a new Plain Literal Token
        /// </summary>
        /// <param name="value">Literal Value</param>
        /// <param name="line">Line the Literal occurs on</param>
        /// <param name="start">Column the Literal starts at</param>
        /// <param name="end">Column the Literal ends at</param>
        public PlainLiteralToken(String value, int line, int start, int end) : base(Token.PLAINLITERAL, value, line, line, start, end) { }
    }

    /// <summary>
    /// Token which represents Literals
    /// </summary>
    public class LiteralToken : BaseToken
    {
        /// <summary>
        /// Creates a new Literal Token
        /// </summary>
        /// <param name="value">Literal Value including the Quote deliminators</param>
        /// <param name="line">Line the Literal occurs on</param>
        /// <param name="start">Column the Literal starts at</param>
        /// <param name="end">Column the Literal ends at</param>
        public LiteralToken(String value, int line, int start, int end) : base(Token.LITERAL, value.Substring(1, value.Length - 2), line, line, start, end) { }

        /// <summary>
        /// Creates a new Literal Token
        /// </summary>
        /// <param name="value">Literal Value including the Quote deliminators</param>
        /// <param name="startLine">Line the Literal starts on</param>
        /// <param name="endLine">Line the Literal ends on</param>
        /// <param name="start">Column the Literal starts at</param>
        /// <param name="end">Column the Literal ends at</param>
        /// <remarks>
        /// Most syntaxes use different deliminators for multiline literals and will usually use a <see cref="LongLiteralToken">LongLiteralToken</see> instead but some formats like CSV only use quotes for multiline literals and use no delimitors for single line literals
        /// </remarks>
        public LiteralToken(String value, int startLine, int endLine, int start, int end) : base(Token.LITERAL, value.Substring(1, value.Length - 2), startLine, endLine, start, end) { }
    }

    /// <summary>
    /// Token which represents Long Literals (allows multi-line values)
    /// </summary>
    public class LongLiteralToken : BaseToken
    {
        /// <summary>
        /// Creates a new Long Literal Token
        /// </summary>
        /// <param name="value">Literal Value including the Triple Quote deliminators</param>
        /// <param name="startLine">Line the Long Literal starts on</param>
        /// <param name="endLine">Line the Long Literal ends on</param>
        /// <param name="start">Column the Literal starts at</param>
        /// <param name="end">Column the Literal ends at</param>
        public LongLiteralToken(String value, int startLine, int endLine, int start, int end) : base(Token.LONGLITERAL, value.Substring(3, value.Length-6), startLine, endLine, start, end) { }
}

    /// <summary>
    /// Token which represents the Language Specifier for a Literal
    /// </summary>
    public class LanguageSpecifierToken : BaseToken
    {
        /// <summary>
        /// Creates a new Language Specifier Token
        /// </summary>
        /// <param name="value">Language Specifier</param>
        /// <param name="line">Line the Literal occurs on</param>
        /// <param name="start">Column the Literal starts at</param>
        /// <param name="end">Column the Literal ends at</param>
        public LanguageSpecifierToken(String value, int line, int start, int end) : base(Token.LANGSPEC, value, line, line, start, end) { }
    }

    /// <summary>
    /// Token which represents the Data Type for a Literal
    /// </summary>
    public class DataTypeToken : BaseToken
    {
        /// <summary>
        /// Creates a new DataType Token
        /// </summary>
        /// <param name="value">DataType Uri including the &lt; &gt; deliminators or a QName</param>
        /// <param name="line">Line the DataType occurs on</param>
        /// <param name="start">Column the DataType starts at</param>
        /// <param name="end">Column the DataType ends at</param>
        public DataTypeToken(String value, int line, int start, int end) : base(Token.DATATYPE, value, line, line, start, end) { }
    }

    /// <summary>
    /// Token which represents Literals with Language Specifiers
    /// </summary>
    public class LiteralWithLanguageSpecifierToken : BaseToken
    {
        private String _lang;

        /// <summary>
        /// Creates a new Literal with Language Specifier Token
        /// </summary>
        /// <param name="lit">Literal Token</param>
        /// <param name="lang">Language Specifier Token</param>
        public LiteralWithLanguageSpecifierToken(IToken lit, LanguageSpecifierToken lang)
            : base(Token.LITERALWITHLANG, lit.Value, lit.StartLine, lit.EndLine, lit.StartPosition, lit.EndPosition)
        {
            _lang = lang.Value;
        }

        /// <summary>
        /// The Language Specifier for this Literal
        /// </summary>
        public String Language
        {
            get
            {
                return _lang;
            }
        }

    }

    /// <summary>
    /// Token which represents Literals with Data Types
    /// </summary>
    public class LiteralWithDataTypeToken : BaseToken
    {
        private String _datatype;

        /// <summary>
        /// Creates a new Literal with DataType Token
        /// </summary>
        /// <param name="lit">Literal Token</param>
        /// <param name="dt">DataType Token</param>
        public LiteralWithDataTypeToken(IToken lit, DataTypeToken dt) : base(Token.LITERALWITHDT, lit.Value, lit.StartLine, lit.EndLine, lit.StartPosition, lit.EndPosition)
        {
            _datatype = dt.Value;
        }

        /// <summary>
        /// The Data Type Uri/QName for this Literal
        /// </summary>
        public String DataType
        {
            get
            {
                return _datatype;
            }
        }
    }

    /// <summary>
    /// Token which represents Graph Literals
    /// </summary>
    public class GraphLiteralToken : BaseToken
    {
        private Graph _g = new Graph();

        /// <summary>
        /// Creates a new Graph Literal Token
        /// </summary>
        /// <param name="value">Value of the Graph Literal</param>
        /// <param name="startLine">Line the Graph Literal starts on</param>
        /// <param name="endLine">Line the Graph Literal ends on</param>
        /// <param name="startPos">Column the Graph Literal starts at</param>
        /// <param name="endPos">Column the Graph Literal ends at</param>
        public GraphLiteralToken(String value, int startLine, int endLine, int startPos, int endPos)
            : base(Token.GRAPHLITERAL, value, startLine, endLine, startPos, endPos)
        {
        }

    }

#endregion

#region Blank Node Tokens

    /// <summary>
    /// Token which represents anonymous Blank Nodes
    /// </summary>
    public class BlankNodeToken : BaseToken
    {
        /// <summary>
        /// Creates a new Anonymous Blank Node Token
        /// </summary>
        /// <param name="line">Line the Blank Node occurs on</param>
        /// <param name="pos">Column the Blank Node occurs at</param>
        public BlankNodeToken(int line, int pos) : base(Token.BLANKNODE, "[]", line, line, pos, pos + 1) { }
    }

    /// <summary>
    /// Token which represents named Blank Nodes
    /// </summary>
    public class BlankNodeWithIDToken : BaseToken
    {
        /// <summary>
        /// Creates a new Blank Node Token
        /// </summary>
        /// <param name="value">ID of the Blank Node</param>
        /// <param name="line">Line the Blank Node occurs on</param>
        /// <param name="start">Column the Blank Node starts at</param>
        /// <param name="end">Column the Blank Node ends at</param>
        public BlankNodeWithIDToken(String value, int line, int start, int end) : base(Token.BLANKNODEWITHID, value, line, line, start, end) { }
     }

    /// <summary>
    /// Token which represents Blank Node Collections
    /// </summary>
    public class BlankNodeCollectionToken : BaseToken
    {
        private Stack<IToken> _contents;

        /// <summary>
        /// Creates a new Blank Node Collection Token
        /// </summary>
        /// <param name="contents">Contents of the Blank Node Collection</param>
        /// <param name="startline">Line the Collection starts on</param>
        /// <param name="endline">Line the Collection ends on</param>
        /// <param name="start">Column the Collection starts at</param>
        /// <param name="end">Column the Collection ends at</param>
        public BlankNodeCollectionToken(Stack<IToken> contents, int startline, int endline, int start, int end)
            : base(Token.BLANKNODECOLLECTION, "", startline, endline, start, end)
        {
            _contents = contents;
        }

        /// <summary>
        /// The Tokens contained in the Blank Node Collection
        /// </summary>
        public Stack<IToken> Collection
        {
            get
            {
                return _contents;
            }
        }
    }

#endregion

#region Keyword Tokens

    /// <summary>
    /// Token representing the 'a' Keyword
    /// </summary>
    public class KeywordAToken : BaseToken
    {
        /// <summary>
        /// Creates a new 'a' Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Column the Keyword occurs at</param>
        public KeywordAToken(int line, int pos) : base(Token.KEYWORDA, "a", line, line, pos, pos) { }
    }

    /// <summary>
    /// Token representing the 'is' Keyword
    /// </summary>
    public class KeywordIsToken : BaseToken
    {
        /// <summary>
        /// Creates a new 'is' Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Column the Keyword occurs at</param>
        public KeywordIsToken(int line, int pos) : base(Token.KEYWORDIS, "is", line, line, pos, pos+1) { }
    }

    /// <summary>
    /// Token representing the 'of' Keyword
    /// </summary>
    public class KeywordOfToken : BaseToken
    {
        /// <summary>
        /// Creates a new 'of' Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Column the Keyword occurs at</param>
        public KeywordOfToken(int line, int pos) : base(Token.KEYWORDOF, "of", line, line, pos, pos+1) { }
    }

    /// <summary>
    /// Token representing the '=>' implies Syntax
    /// </summary>
    public class ImpliesToken : BaseToken
    {
        /// <summary>
        /// Creates a new '=>' Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Column the Keyword occurs at</param>
        public ImpliesToken(int line, int pos) : base(Token.IMPLIES, "=>", line, line, pos, pos + 1) { }
    }

    /// <summary>
    /// Token representing the '>=' implied by Syntax
    /// </summary>
    public class ImpliedByToken : BaseToken
    {
        /// <summary>
        /// Creates a new '&lt;=' Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Column the Keyword occurs at</param>
        public ImpliedByToken(int line, int pos) : base(Token.IMPLIEDBY, "<=", line, line, pos, pos + 1) { }
    }

    /// <summary>
    /// Token representing the '=' equality Syntax
    /// </summary>
    public class EqualityToken : BaseToken
    {
        /// <summary>
        /// Creates a new '=' Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Column the Keyword occurs at</param>
        public EqualityToken(int line, int pos) : base(Token.EQUALS, "=", line, line, pos, pos + 1) { }
    }

    /// <summary>
    /// Token representing the use of a Custom Keyword
    /// </summary>
    public class CustomKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new Custom Keyword Token
        /// </summary>
        /// <param name="value">Custom Keyword</param>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="start">Column the Keyword starts at</param>
        /// <param name="end">Column the Keyword ends at</param>
        public CustomKeywordToken(String value, int line, int start, int end) : base(Token.KEYWORDCUSTOM, value, line, line, start, end) { }
    }

    /// <summary>
    /// Token representing the definition of a Custom Keyword
    /// </summary>
    public class CustomKeywordDefinitionToken : BaseToken
    {
        /// <summary>
        /// Creates a new Custom Keyword Definition Token
        /// </summary>
        /// <param name="value">Custom Keyword Definition</param>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="start">Column the Keyword starts at</param>
        /// <param name="end">Column the Keyword ends at</param>
        public CustomKeywordDefinitionToken(String value, int line, int start, int end) : base(Token.KEYWORDDEF, value, line, line, start, end) { }
    }

#endregion

#region Misc Tokens

    /// <summary>
    /// Token representing Variables
    /// </summary>
    public class VariableToken : BaseToken
    {
        /// <summary>
        /// Creates a new Variable Token
        /// </summary>
        /// <param name="value">Variable</param>
        /// <param name="line">Line the Variable occurs on</param>
        /// <param name="start">Column the Variable starts at</param>
        /// <param name="end">Column the Variable ends at</param>
        public VariableToken(String value, int line, int start, int end) : base(Token.VARIABLE, value, line, line, start, end) { }
    }

    class PlaceholderToken : BaseToken
    {
        public PlaceholderToken(String value, int line, int pos) : base(-1, value, line, line, pos, pos + value.Length) { }
    }

#endregion
}