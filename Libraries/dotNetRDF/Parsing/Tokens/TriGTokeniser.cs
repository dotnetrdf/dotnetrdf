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
using System.IO;

namespace VDS.RDF.Parsing.Tokens
{
    /// <summary>
    /// Tokeniser for TriG (Turtle with Named Graphs) RDF Syntax
    /// </summary>
    public class TriGTokeniser
        : BaseTokeniser
    {
        private ParsingTextReader _in;
        private int _lasttokentype = -1;
        private TriGSyntax _syntax = TriGSyntax.Recommendation;

        /// <summary>
        /// Creates a new TriG Tokeniser which reads Tokens from the given Stream
        /// </summary>
        /// <param name="input">Stream to read Tokens from</param>
        public TriGTokeniser(StreamReader input)
            : this(ParsingTextReader.Create(input)) { }

        /// <summary>
        /// Creates a new TriG Tokeniser which reads Tokens from the given Stream using the specified syntax
        /// </summary>
        /// <param name="input">Stream to read Tokens from</param>
        /// <param name="syntax">Syntax</param>
        public TriGTokeniser(StreamReader input, TriGSyntax syntax)
            : this(ParsingTextReader.Create(input), syntax) { }

        /// <summary>
        /// Creates a new TriG Tokeniser which reads Tokens from the given Stream
        /// </summary>
        /// <param name="input">Stream to read Tokens from</param>
        public TriGTokeniser(ParsingTextReader input)
            : base(input)
        {
            _in = input;
            Format = "TriG";
        }

        /// <summary>
        /// Creates a new TriG Tokeniser which reads Tokens from the given Stream
        /// </summary>
        /// <param name="input">Stream to read Tokens from</param>
        /// <param name="syntax">Syntax</param>
        public TriGTokeniser(ParsingTextReader input, TriGSyntax syntax)
            : this(input) 
        {
            _syntax = syntax;
        }

        /// <summary>
        /// Creates a new TriG Tokeniser which reads Tokens from the given Input
        /// </summary>
        /// <param name="input">Input to read Tokens from</param>
        public TriGTokeniser(TextReader input)
            : this(ParsingTextReader.Create(input)) { }

        /// <summary>
        /// Creates a new TriG Tokeniser which reads Tokens from the given Input
        /// </summary>
        /// <param name="input">Input to read Tokens from</param>
        /// <param name="syntax">Syntax</param>
        public TriGTokeniser(TextReader input, TriGSyntax syntax)
            : this(ParsingTextReader.Create(input), syntax) { }

        /// <summary>
        /// Gets the next available Token from the Input Stream
        /// </summary>
        /// <returns></returns>
        public override IToken GetNextToken()
        {
            // Have we read anything yet?
            if (_lasttokentype == -1)
            {
                // Nothing read yet so produce a BOF Token
                _lasttokentype = Token.BOF;
                return new BOFToken();
            }
            else
            {
                // Reading has started
                StartNewToken();

                try
                {
                    // Certain Last Tokens restrict what we expect next
                    if (LastTokenType == Token.BOF && _in.EndOfStream)
                    {
                        // Empty File
                        return new EOFToken(0, 0);
                    }
                    else if (_lasttokentype == Token.PREFIXDIRECTIVE)
                    {
                        // Discard any white space first
                        DiscardWhiteSpace();

                        // Get Prefix
                        return TryGetPrefix();
                    }
                    else if (_lasttokentype == Token.HATHAT)
                    {
                        // Get DataType
                        return TryGetDataType();
                    }

                    do
                    {
                        // Check for EOF
                        if (_in.EndOfStream)
                        {
                            if (Length == 0)
                            {
                                // We're at the End of the Stream and not part-way through reading a Token
                                return new EOFToken(CurrentLine, CurrentPosition);
                            }
                            else
                            {
                                // We're at the End of the Stream and part-way through reading a Token
                                // Raise an error
                                throw Error("Unexpected End of File encountered while attempting to Parse Token from content\n" + Value);
                            }
                        }

                        char next = Peek();

                        // Always need to do a check for End of Stream after Peeking to handle empty files OK
                        if (next == Char.MaxValue && _in.EndOfStream)
                        {
                            if (Length == 0)
                            {
                                // We're at the End of the Stream and not part-way through reading a Token
                                return new EOFToken(CurrentLine, CurrentPosition);
                            }
                            else
                            {
                                // We're at the End of the Stream and part-way through reading a Token
                                // Raise an error
                                throw UnexpectedEndOfInput("Token");
                            }
                        }

                        if (Char.IsWhiteSpace(next))
                        {
                            // Discard white space between Tokens
                            DiscardWhiteSpace();
                        }
                        else if (Char.IsDigit(next) || next == '-' || next == '+')
                        {
                            // Start of a Numeric Plain Literal
                            return TryGetNumericLiteral();
                        }
                        else if (Char.IsLetter(next))
                        {
                            // Start of a Plain Literal
                            return TryGetPlainLiteralOrQName();
                        }
                        else
                        {
                            switch (next)
                            {
                                case '#':
                                    // Comment
                                    return TryGetComment();

                                case '@':
                                    // Start of a Keyword or Language Specifier
                                    return TryGetKeywordOrLangSpec();

                                case '=':
                                    // Equality
                                    ConsumeCharacter();
                                    _lasttokentype = Token.EQUALS;
                                    return new EqualityToken(CurrentLine, StartPosition);

                                #region URIs, QNames and Literals

                                case '<':
                                    // Start of a Uri
                                    return TryGetUri();

                                case '_':
                                case ':':
                                    // Start of a  QName
                                    return TryGetQName();

                                case '"':
                                    // Start of a Literal
                                    return TryGetLiteral();

                                case '^':
                                    // Data Type Specifier
                                    ConsumeCharacter();
                                    next = Peek();
                                    if (next == '^')
                                    {
                                        ConsumeCharacter();
                                        _lasttokentype = Token.HATHAT;
                                        return new HatHatToken(CurrentLine, StartPosition);
                                    }
                                    else
                                    {
                                        throw Error("Unexpected Character (Code " + (int)next + ") " + (char)next + " was encountered, expected the second ^ as part of a ^^ Data Type Specifier");
                                    }

                                #endregion

                                #region Line Terminators

                                case '.':
                                    // Dot Terminator
                                    ConsumeCharacter();
                                    if (!_in.EndOfStream && Char.IsDigit(Peek()))
                                    {
                                        return TryGetNumericLiteral();
                                    }
                                    else
                                    {
                                        _lasttokentype = Token.DOT;
                                        return new DotToken(CurrentLine, StartPosition);
                                    }
                                case ';':
                                    // Semicolon Terminator
                                    ConsumeCharacter();
                                    _lasttokentype = Token.SEMICOLON;
                                    return new SemicolonToken(CurrentLine, StartPosition);
                                case ',':
                                    // Comma Terminator
                                    ConsumeCharacter();
                                    _lasttokentype = Token.COMMA;
                                    return new CommaToken(CurrentLine, StartPosition);

                                #endregion

                                #region Collections and Graphs

                                case '[':
                                    // Blank Node Collection
                                    ConsumeCharacter();
                                    _lasttokentype = Token.LEFTSQBRACKET;
                                    return new LeftSquareBracketToken(CurrentLine, StartPosition);
                                case ']':
                                    // Blank Node Collection
                                    ConsumeCharacter();
                                    _lasttokentype = Token.RIGHTSQBRACKET;
                                    return new RightSquareBracketToken(CurrentLine, StartPosition);
                                case '{':
                                    // Graph
                                    ConsumeCharacter();
                                    _lasttokentype = Token.LEFTCURLYBRACKET;
                                    return new LeftCurlyBracketToken(CurrentLine, StartPosition);
                                case '}':
                                    // Graph
                                    ConsumeCharacter();
                                    _lasttokentype = Token.RIGHTCURLYBRACKET;
                                    return new RightCurlyBracketToken(CurrentLine, StartPosition);
                                case '(':
                                    // Collection
                                    ConsumeCharacter();
                                    _lasttokentype = Token.LEFTBRACKET;
                                    return new LeftBracketToken(CurrentLine, StartPosition);
                                case ')':
                                    // Collection
                                    ConsumeCharacter();
                                    _lasttokentype = Token.RIGHTBRACKET;
                                    return new RightBracketToken(CurrentLine, StartPosition);

                                #endregion

                                default:
                                    // Unexpected Character
                                    throw UnexpectedCharacter(next, String.Empty);
                            }
                        }
                    } while (true);

                }
                catch (IOException)
                {
                    // End Of Stream Check
                    if (_in.EndOfStream)
                    {
                        // At End of Stream so produce the EOFToken
                        return new EOFToken(CurrentLine, CurrentPosition);
                    }
                    else
                    {
                        // Some other Error so throw
                        throw;
                    }
                }
            }
        }

        private IToken TryGetComment()
        {
            // Consume the first character which must have been a #
            ConsumeCharacter();

            // Consume everything up till we hit the new line
            char next = Peek();
            while (next != '\n' && next != '\r')
            {
                if (ConsumeCharacter(true)) break;
                next = Peek();
            }

            // Create the Token, discard the new line and return
            _lasttokentype = Token.COMMENT;
            CommentToken comment = new CommentToken(Value, CurrentLine, StartPosition, EndPosition);
            ConsumeNewLine(false, true);
            return comment;
        }

        private IToken TryGetPrefix()
        {
            // Get the prefix
            char next = Peek();
            while (!Char.IsWhiteSpace(next))
            {
                ConsumeCharacter();
                next = Peek();
            }

            // Last character must be a :
            if (!Value.EndsWith(":"))
            {
                throw UnexpectedCharacter(next, "expected a : to end a Prefix specification");
            }
            if (!TurtleSpecsHelper.IsValidQName(Value, _syntax == TriGSyntax.Recommendation ? TurtleSyntax.W3C : TurtleSyntax.Original))
            {
                throw new RdfParseException("The value '" + Value + "' is not a valid Prefix in TriG", new PositionInfo(StartLine, CurrentLine, StartPosition, EndPosition));
            }

            _lasttokentype = Token.PREFIX;
            return new PrefixToken(Value, CurrentLine, StartPosition, EndPosition);
        }

        private IToken TryGetKeywordOrLangSpec()
        {
            // Consume the first Character which must have been an @
            ConsumeCharacter();

            // Consume characters which can be in the keyword or Language Specifier
            char next = Peek();
            while (Char.IsLetterOrDigit(next) || next == '-')
            {
                ConsumeCharacter();
                next = Peek();
            }

            // Check the output to see if it's valid
            String output = Value;
            if (output.Equals("@prefix"))
            {
                _lasttokentype = Token.PREFIXDIRECTIVE;
                return new PrefixDirectiveToken(CurrentLine, StartPosition);
            }
            else if (output.Equals("@base"))
            {
                if (_syntax == TriGSyntax.Original) throw new RdfParseException("The @base directive is not permitted in this version of TriG, later versions of TriG support this feature and may be enabled by changing your syntax setting when you create a TriG Parser");
                _lasttokentype = Token.BASEDIRECTIVE;
                return new BaseDirectiveToken(CurrentLine, StartPosition);
            }
            else if (RdfSpecsHelper.IsValidLangSpecifier(output))
            {
                _lasttokentype = Token.LANGSPEC;
                return new LanguageSpecifierToken(output.Substring(1), CurrentLine, StartPosition, EndPosition);
            }
            else
            {
                throw Error("Unexpected Content '" + output + "' encountered, expected an @prefix/@base keyword or a Language Specifier");
            }
        }

        private IToken TryGetUri()
        {
            // Consume the first Character which must have been a <
            ConsumeCharacter();

            // Consume subsequent characters
            char next;
            do
            {
                next = Peek();

                // Watch out for escapes
                if (next == '\\')
                {
                    switch (_syntax)
                    {
                        case TriGSyntax.MemberSubmission:
                        case TriGSyntax.Original:
                            HandleEscapes(TokeniserEscapeMode.PermissiveUri);
                            break;
                        default:
                            HandleEscapes(TokeniserEscapeMode.Uri);
                            break;
                    }
                }
                else
                {
                    ConsumeCharacter();
                }

            } while (next != '>');

            // Return the Token
            _lasttokentype = Token.URI;
            return new UriToken(Value, CurrentLine, StartPosition, EndPosition);
        }

        private IToken TryGetQName()
        {
            bool colonoccurred = false;

            char next = Peek();
            while (Char.IsLetterOrDigit(next) || next == '-' || next == '_' || next == ':')
            {
                ConsumeCharacter();
                if (next == ':')
                {
                    if (colonoccurred)
                    {
                        throw Error("Unexpected Colon encountered in input '" + Value + "', a Colon may only occur once in a QName");
                    }
                    colonoccurred = true;
                }

                next = Peek();
            }

            if (Value.StartsWith(".")) Backtrack();

            // Validate the QName
            if (Value.StartsWith("_:"))
            {
                // Blank Node ID
                _lasttokentype = Token.BLANKNODEWITHID;
                return new BlankNodeWithIDToken(Value, CurrentLine, StartPosition, EndPosition);
            } 
            else if (TurtleSpecsHelper.IsValidQName(Value, _syntax == TriGSyntax.Recommendation ? TurtleSyntax.W3C : TurtleSyntax.Original))
            {
                // QName
                _lasttokentype = Token.QNAME;
                return new QNameToken(Value, CurrentLine, StartPosition, EndPosition);
            }
            else
            {
                throw Error("The input '" + Value + "' is not a valid QName in {0}");
            }

        }

        private IToken TryGetLiteral()
        {
            bool longliteral = false;

            // Consume first character which must have been a "
            ConsumeCharacter();

            // Check if this is a long literal
            char next = Peek();
            if (next == '"')
            {
                ConsumeCharacter();
                next = Peek();

                if (next == '"')
                {
                    // Long Literal
                    longliteral = true;
                    ConsumeCharacter();
                    next = Peek();
                }
                else
                {
                    // Empty Literal
                    _lasttokentype = Token.LITERAL;
                    return new LiteralToken(Value, CurrentLine, StartPosition, EndPosition);
                }
            }

            while (true)
            {
                if (next == '\\')
                {
                    HandleEscapes(TokeniserEscapeMode.QuotedLiterals);
                    next = Peek();
                    continue;
                }

                // Add character to output buffer
                ConsumeCharacter();

                // Check for end of Literal
                if (next == '"')
                {
                    if (longliteral)
                    {
                        next = Peek();
                        if (next == '"')
                        {
                            // Got two quotes so far
                            ConsumeCharacter();
                            next = Peek();
                            if (next == '"')
                            {
                                // Triple quote - end of literal
                                ConsumeCharacter();
                                _lasttokentype = Token.LONGLITERAL;

                                // If there are any additional quotes immediatedly following this then
                                // we want to consume them also
                                next = Peek();
                                while (next == '"')
                                {
                                    ConsumeCharacter();
                                    next = Peek();
                                }

                                return new LongLiteralToken(Value, StartLine, EndLine, StartPosition, EndPosition);
                            }
                            else
                            {
                                // Not a triple quote so continue
                                continue;
                            }
                        }
                        else
                        {
                            // Not a Triple quote so continue
                            continue;
                        }
                    }
                    else
                    {
                        // End of Literal
                        _lasttokentype = Token.LITERAL;
                        return new LiteralToken(Value, CurrentLine, StartPosition, EndPosition);
                    }
                }

                // Continue Reading
                next = Peek();
            }
        }

        private IToken TryGetNumericLiteral()
        {
            bool dotoccurred = false;
            bool expoccurred = false;
            bool signoccurred = false;

            if (Length == 1) dotoccurred = true;

            char next = Peek();

            // Read the Characters of the Numeric Literal
            while (Char.IsDigit(next) || next == '-' || next == '+' || (next == '.' && !dotoccurred) || next == 'e' || next == 'E')
            {
                if (next == '-' || next == '+')
                {
                    // Sign can occur at start and immediatedly after an exponent
                    if ((signoccurred || expoccurred) && !(Value.EndsWith("e") || Value.EndsWith("E")))
                    {
                        // +/- can only occur once at start and once after exponent
                        throw Error("Unexpected Character " + next + " encountered while parsing a Numeric Literal from input '" + Value + "', a +/- to specify sign has already occurred in this Numeric Literal");
                    }
                    signoccurred = true;
                }
                else if (next == '.')
                {
                    dotoccurred = true;
                }
                else if (next == 'e' || next == 'E')
                {
                    if (expoccurred)
                    {
                        throw Error("Unexpected Character " + next + " encountered while parsing a Numeric Literal from input '" + Value + "', a e/E to specify exponent has already occurred in this Numeric Literal");
                    }
                    expoccurred = true;
                }

                ConsumeCharacter();
                next = Peek();
            }

            if (Value.EndsWith(".")) Backtrack();

            // Validate
            if (TurtleSpecsHelper.IsValidPlainLiteral(Value, TurtleSyntax.Original))
            {
                _lasttokentype = Token.PLAINLITERAL;
                return new PlainLiteralToken(Value, CurrentLine, StartPosition, EndPosition);
            }
            else
            {
                throw Error("The input '" + Value + "' is not a valid Plain Literal in {0}");
            }
        }

        private IToken TryGetPlainLiteralOrQName()
        {
            // Read Valid Plain Literal and QName Chars
            char next = Peek();

            bool colonoccurred = false;
            while (Char.IsLetterOrDigit(next) || next == '_' || next == '-' || next == ':')
            {
                ConsumeCharacter();

                if (next == ':') {
                    if (colonoccurred)
                    {
                        throw Error("Unexpected Colon encountered in input '" + Value + "', a Colon may only occur once in a QName");
                    }
                    colonoccurred = true;
                }

                next = Peek();
            }

            // Validate
            String value = Value;

            // If it ends in a trailing . then we need to backtrack
            if (value.EndsWith("."))
            {
                Backtrack();
                value = value.Substring(0, value.Length - 1);
            }

            if (value.Equals("a"))
            {
                // Keyword 'a'
                _lasttokentype = Token.KEYWORDA;
                return new KeywordAToken(CurrentLine, StartPosition);
            }
            else if (value.Equals("true") || value.Equals("false"))
            {
                // Boolean Plain Literal
                _lasttokentype = Token.PLAINLITERAL;
                return new PlainLiteralToken(Value, CurrentLine, StartPosition, EndPosition);
            }
            else if (TurtleSpecsHelper.IsValidQName(value, _syntax == TriGSyntax.Recommendation ? TurtleSyntax.W3C : TurtleSyntax.Original))
            {
                // QName
                _lasttokentype = Token.QNAME;
                return new QNameToken(Value, CurrentLine, StartPosition, EndPosition);
            }
            else
            {
                // Error
                throw Error("Unexpected input '" + value + "', expected a QName, the 'a' Keyword or a Plain Literal");
            }
        }

        private IToken TryGetDataType()
        {
            char next = Peek();
            if (next == '<')
            {
                // Uri for Data Type
                return TryGetUri();
            }
            else
            {
                // Should be a QName
                IToken qname = TryGetQName();
                if (qname.TokenType != Token.QNAME)
                {
                    throw Error("Unexpected Token '" + qname.GetType().ToString() + "' parsed when a QName Token to specify a Data Type was expected");
                }
                else
                {
                    return qname;
                }
            }
        }
    }
}
