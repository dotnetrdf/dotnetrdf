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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace VDS.RDF.Parsing.Tokens
{
    /// <summary>
    /// Tokeniser for TriG (Turtle with Named Graphs) RDF Syntax
    /// </summary>
    public class TriGTokeniser : BaseTokeniser
    {
        private BlockingTextReader _in;
        private int _lasttokentype = -1;

        /// <summary>
        /// Creates a new TriG Tokeniser which reads Tokens from the given Stream
        /// </summary>
        /// <param name="input">Stream to read Tokens from</param>
        public TriGTokeniser(StreamReader input)
            : this(new BlockingTextReader(input)) { }

        /// <summary>
        /// Creates a new TriG Tokeniser which reads Tokens from the given Stream
        /// </summary>
        /// <param name="input">Stream to read Tokens from</param>
        public TriGTokeniser(BlockingTextReader input)
            : base(input)
        {
            this._in = input;
            this.Format = "TriG";
        }

        /// <summary>
        /// Creates a new TriG Tokeniser which reads Tokens from the given Input
        /// </summary>
        /// <param name="input">Input to read Tokens from</param>
        public TriGTokeniser(TextReader input)
            : this(new BlockingTextReader(input)) { }

        /// <summary>
        /// Gets the next available Token from the Input Stream
        /// </summary>
        /// <returns></returns>
        public override IToken GetNextToken()
        {
            //Have we read anything yet?
            if (this._lasttokentype == -1)
            {
                //Nothing read yet so produce a BOF Token
                this._lasttokentype = Token.BOF;
                return new BOFToken();
            }
            else
            {
                //Reading has started
                this.StartNewToken();

                try
                {
                    //Certain Last Tokens restrict what we expect next
                    if (this.LastTokenType == Token.BOF && this._in.EndOfStream)
                    {
                        //Empty File
                        return new EOFToken(0, 0);
                    }
                    else if (this._lasttokentype == Token.PREFIXDIRECTIVE)
                    {
                        //Discard any white space first
                        this.DiscardWhiteSpace();

                        //Get Prefix
                        return this.TryGetPrefix();
                    }
                    else if (this._lasttokentype == Token.HATHAT)
                    {
                        //Get DataType
                        return this.TryGetDataType();
                    }

                    do
                    {
                        //Check for EOF
                        if (this._in.EndOfStream)
                        {
                            if (this.Length == 0)
                            {
                                //We're at the End of the Stream and not part-way through reading a Token
                                return new EOFToken(this.CurrentLine, this.CurrentPosition);
                            }
                            else
                            {
                                //We're at the End of the Stream and part-way through reading a Token
                                //Raise an error
                                throw Error("Unexpected End of File encountered while attempting to Parse Token from content\n" + this.Value);
                            }
                        }

                        char next = this.Peek();

                        //Always need to do a check for End of Stream after Peeking to handle empty files OK
                        if (next == Char.MaxValue && this._in.EndOfStream)
                        {
                            if (this.Length == 0)
                            {
                                //We're at the End of the Stream and not part-way through reading a Token
                                return new EOFToken(this.CurrentLine, this.CurrentPosition);
                            }
                            else
                            {
                                //We're at the End of the Stream and part-way through reading a Token
                                //Raise an error
                                throw UnexpectedEndOfInput("Token");
                            }
                        }

                        if (Char.IsWhiteSpace(next))
                        {
                            //Discard white space between Tokens
                            this.DiscardWhiteSpace();
                        }
                        else if (Char.IsDigit(next) || next == '-' || next == '+')
                        {
                            //Start of a Numeric Plain Literal
                            return this.TryGetNumericLiteral();
                        }
                        else if (Char.IsLetter(next))
                        {
                            //Start of a Plain Literal
                            return this.TryGetPlainLiteralOrQName();
                        }
                        else
                        {
                            switch (next)
                            {
                                case '#':
                                    //Comment
                                    return this.TryGetComment();

                                case '@':
                                    //Start of a Keyword or Language Specifier
                                    return this.TryGetKeywordOrLangSpec();

                                case '=':
                                    //Equality
                                    this.ConsumeCharacter();
                                    this._lasttokentype = Token.EQUALS;
                                    return new EqualityToken(this.CurrentLine, this.StartPosition);

                                #region URIs, QNames and Literals

                                case '<':
                                    //Start of a Uri
                                    return this.TryGetUri();

                                case '_':
                                case ':':
                                    //Start of a  QName
                                    return this.TryGetQName();

                                case '"':
                                    //Start of a Literal
                                    return this.TryGetLiteral();

                                case '^':
                                    //Data Type Specifier
                                    this.ConsumeCharacter();
                                    next = this.Peek();
                                    if (next == '^')
                                    {
                                        this.ConsumeCharacter();
                                        this._lasttokentype = Token.HATHAT;
                                        return new HatHatToken(this.CurrentLine, this.StartPosition);
                                    }
                                    else
                                    {
                                        throw Error("Unexpected Character (Code " + (int)next + ") " + (char)next + " was encountered, expected the second ^ as part of a ^^ Data Type Specifier");
                                    }

                                #endregion

                                #region Line Terminators

                                case '.':
                                    //Dot Terminator
                                    this.ConsumeCharacter();
                                    if (!this._in.EndOfStream && Char.IsDigit(this.Peek()))
                                    {
                                        return this.TryGetNumericLiteral();
                                    }
                                    else
                                    {
                                        this._lasttokentype = Token.DOT;
                                        return new DotToken(this.CurrentLine, this.StartPosition);
                                    }
                                case ';':
                                    //Semicolon Terminator
                                    this.ConsumeCharacter();
                                    this._lasttokentype = Token.SEMICOLON;
                                    return new SemicolonToken(this.CurrentLine, this.StartPosition);
                                case ',':
                                    //Comma Terminator
                                    this.ConsumeCharacter();
                                    this._lasttokentype = Token.COMMA;
                                    return new CommaToken(this.CurrentLine, this.StartPosition);

                                #endregion

                                #region Collections and Graphs

                                case '[':
                                    //Blank Node Collection
                                    this.ConsumeCharacter();
                                    this._lasttokentype = Token.LEFTSQBRACKET;
                                    return new LeftSquareBracketToken(this.CurrentLine, this.StartPosition);
                                case ']':
                                    //Blank Node Collection
                                    this.ConsumeCharacter();
                                    this._lasttokentype = Token.RIGHTSQBRACKET;
                                    return new RightSquareBracketToken(this.CurrentLine, this.StartPosition);
                                case '{':
                                    //Graph
                                    this.ConsumeCharacter();
                                    this._lasttokentype = Token.LEFTCURLYBRACKET;
                                    return new LeftCurlyBracketToken(this.CurrentLine, this.StartPosition);
                                case '}':
                                    //Graph
                                    this.ConsumeCharacter();
                                    this._lasttokentype = Token.RIGHTCURLYBRACKET;
                                    return new RightCurlyBracketToken(this.CurrentLine, this.StartPosition);
                                case '(':
                                    //Collection
                                    this.ConsumeCharacter();
                                    this._lasttokentype = Token.LEFTBRACKET;
                                    return new LeftBracketToken(this.CurrentLine, this.StartPosition);
                                case ')':
                                    //Collection
                                    this.ConsumeCharacter();
                                    this._lasttokentype = Token.RIGHTBRACKET;
                                    return new RightBracketToken(this.CurrentLine, this.StartPosition);

                                #endregion

                                default:
                                    //Unexpected Character
                                    throw this.UnexpectedCharacter(next, String.Empty);
                            }
                        }
                    } while (true);

                }
                catch (IOException)
                {
                    //End Of Stream Check
                    if (this._in.EndOfStream)
                    {
                        //At End of Stream so produce the EOFToken
                        return new EOFToken(this.CurrentLine, this.CurrentPosition);
                    }
                    else
                    {
                        //Some other Error so throw
                        throw;
                    }
                }
            }
        }

        private IToken TryGetComment()
        {
            //Consume the first character which must have been a #
            this.ConsumeCharacter();

            //Consume everything up till we hit the new line
            char next = this.Peek();
            while (next != '\n' && next != '\r')
            {
                if (this.ConsumeCharacter(true)) break;
                next = this.Peek();
            }

            //Create the Token, discard the new line and return
            this._lasttokentype = Token.COMMENT;
            CommentToken comment = new CommentToken(this.Value, this.CurrentLine, this.StartPosition, this.EndPosition);
            this.ConsumeNewLine(false, true);
            return comment;
        }

        private IToken TryGetPrefix()
        {
            //Get the prefix
            char next = this.Peek();
            while (!Char.IsWhiteSpace(next))
            {
                this.ConsumeCharacter();
                next = this.Peek();
            }

            //Last character must be a :
            if (!this.Value.EndsWith(":"))
            {
                throw this.UnexpectedCharacter(next, "expected a : to end a Prefix specification");
            }
            if (!TurtleSpecsHelper.IsValidQName(this.Value))
            {
                throw new RdfParseException("The value '" + this.Value + "' is not a valid Prefix in TriG", new PositionInfo(this.StartLine, this.CurrentLine, this.StartPosition, this.EndPosition));
            }

            this._lasttokentype = Token.PREFIX;
            return new PrefixToken(this.Value, this.CurrentLine, this.StartPosition, this.EndPosition);
        }

        private IToken TryGetKeywordOrLangSpec()
        {
            //Consume the first Character which must have been an @
            this.ConsumeCharacter();

            //Consume characters which can be in the keyword or Language Specifier
            char next = this.Peek();
            while (Char.IsLetter(next) || next == '-')
            {
                this.ConsumeCharacter();
                next = this.Peek();
            }

            //Check the output to see if it's valid
            String output = this.Value;
            if (output.Equals("@prefix"))
            {
                this._lasttokentype = Token.PREFIXDIRECTIVE;
                return new PrefixDirectiveToken(this.CurrentLine, this.StartPosition);
            }
            else if (output.Equals("@base"))
            {
                this._lasttokentype = Token.BASEDIRECTIVE;
                return new BaseDirectiveToken(this.CurrentLine, this.StartPosition);
            }
            else if (RdfSpecsHelper.IsValidLangSpecifier(output))
            {
                this._lasttokentype = Token.LANGSPEC;
                return new LanguageSpecifierToken(output.Substring(1), this.CurrentLine, this.StartPosition, this.EndPosition);
            }
            else
            {
                throw Error("Unexpected Content '" + output + "' encountered, expected an @prefix/@base keyword or a Language Specifier");
            }
        }

        private IToken TryGetUri()
        {
            //Consume the first Character which must have been a <
            this.ConsumeCharacter();

            //Consume subsequent characters
            char next;
            do
            {
                next = this.Peek();

                //Watch out for escapes
                if (next == '\\')
                {
                    this.HandleEscapes(TokeniserEscapeMode.Uri);
                }
                else
                {
                    this.ConsumeCharacter();
                }

            } while (next != '>');

            //Return the Token
            this._lasttokentype = Token.URI;
            return new UriToken(this.Value, this.CurrentLine, this.StartPosition, this.EndPosition);
        }

        private IToken TryGetQName()
        {
            bool colonoccurred = false;

            char next = this.Peek();
            while (Char.IsLetterOrDigit(next) || next == '-' || next == '_' || next == ':')
            {
                this.ConsumeCharacter();
                if (next == ':')
                {
                    if (colonoccurred)
                    {
                        throw Error("Unexpected Colon encountered in input '" + this.Value + "', a Colon may only occur once in a QName");
                    }
                    colonoccurred = true;
                }

                next = this.Peek();
            }

            if (this.Value.StartsWith(".")) this.Backtrack();

            //Validate the QName
            if (this.Value.StartsWith("_:"))
            {
                //Blank Node ID
                this._lasttokentype = Token.BLANKNODEWITHID;
                return new BlankNodeWithIDToken(this.Value, this.CurrentLine, this.StartPosition, this.EndPosition);
            } 
            else if (TurtleSpecsHelper.IsValidQName(this.Value))
            {
                //QName
                this._lasttokentype = Token.QNAME;
                return new QNameToken(this.Value, this.CurrentLine, this.StartPosition, this.EndPosition);
            }
            else
            {
                throw Error("The input '" + this.Value + "' is not a valid QName in {0}");
            }

        }

        private IToken TryGetLiteral()
        {
            bool longliteral = false;

            //Consume first character which must have been a "
            this.ConsumeCharacter();

            //Check if this is a long literal
            char next = this.Peek();
            if (next == '"')
            {
                this.ConsumeCharacter();
                next = this.Peek();

                if (next == '"')
                {
                    //Long Literal
                    longliteral = true;
                    this.ConsumeCharacter();
                    next = this.Peek();
                }
                else
                {
                    //Empty Literal
                    this._lasttokentype = Token.LITERAL;
                    return new LiteralToken(this.Value, this.CurrentLine, this.StartPosition, this.EndPosition);
                }
            }

            while (true)
            {
                if (next == '\\')
                {
                    this.HandleEscapes(TokeniserEscapeMode.QuotedLiterals);
                    next = this.Peek();
                    continue;
                }

                //Add character to output buffer
                this.ConsumeCharacter();

                //Check for end of Literal
                if (next == '"')
                {
                    if (longliteral)
                    {
                        next = this.Peek();
                        if (next == '"')
                        {
                            //Got two quotes so far
                            this.ConsumeCharacter();
                            next = this.Peek();
                            if (next == '"')
                            {
                                //Triple quote - end of literal
                                this.ConsumeCharacter();
                                this._lasttokentype = Token.LONGLITERAL;

                                //If there are any additional quotes immediatedly following this then
                                //we want to consume them also
                                next = this.Peek();
                                while (next == '"')
                                {
                                    this.ConsumeCharacter();
                                    next = this.Peek();
                                }

                                return new LongLiteralToken(this.Value, this.StartLine, this.EndLine, this.StartPosition, this.EndPosition);
                            }
                            else
                            {
                                //Not a triple quote so continue
                                continue;
                            }
                        }
                        else
                        {
                            //Not a Triple quote so continue
                            continue;
                        }
                    }
                    else
                    {
                        //End of Literal
                        this._lasttokentype = Token.LITERAL;
                        return new LiteralToken(this.Value, this.CurrentLine, this.StartPosition, this.EndPosition);
                    }
                }

                //Continue Reading
                next = this.Peek();
            }
        }

        private IToken TryGetNumericLiteral()
        {
            bool dotoccurred = false;
            bool expoccurred = false;
            bool signoccurred = false;

            if (this.Length == 1) dotoccurred = true;

            char next = this.Peek();

            //Read the Characters of the Numeric Literal
            while (Char.IsDigit(next) || next == '-' || next == '+' || (next == '.' && !dotoccurred) || next == 'e' || next == 'E')
            {
                if (next == '-' || next == '+')
                {
                    //Sign can occur at start and immediatedly after an exponent
                    if ((signoccurred || expoccurred) && !(this.Value.EndsWith("e") || this.Value.EndsWith("E")))
                    {
                        //+/- can only occur once at start and once after exponent
                        throw Error("Unexpected Character " + next + " encountered while parsing a Numeric Literal from input '" + this.Value + "', a +/- to specify sign has already occurred in this Numeric Literal");
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
                        throw Error("Unexpected Character " + next + " encountered while parsing a Numeric Literal from input '" + this.Value + "', a e/E to specify exponent has already occurred in this Numeric Literal");
                    }
                    expoccurred = true;
                }

                this.ConsumeCharacter();
                next = this.Peek();
            }

            if (this.Value.EndsWith(".")) this.Backtrack();

            //Validate
            if (TurtleSpecsHelper.IsValidPlainLiteral(this.Value))
            {
                this._lasttokentype = Token.PLAINLITERAL;
                return new PlainLiteralToken(this.Value, this.CurrentLine, this.StartPosition, this.EndPosition);
            }
            else
            {
                throw Error("The input '" + this.Value + "' is not a valid Plain Literal in {0}");
            }
        }

        private IToken TryGetPlainLiteralOrQName()
        {
            //Read Valid Plain Literal and QName Chars
            char next = this.Peek();

            bool colonoccurred = false;
            while (Char.IsLetterOrDigit(next) || next == '_' || next == '-' || next == ':')
            {
                this.ConsumeCharacter();

                if (next == ':') {
                    if (colonoccurred)
                    {
                        throw Error("Unexpected Colon encountered in input '" + this.Value + "', a Colon may only occur once in a QName");
                    }
                    colonoccurred = true;
                }

                next = this.Peek();
            }

            //Validate
            String value = this.Value;

            //If it ends in a trailing . then we need to backtrack
            if (value.EndsWith("."))
            {
                this.Backtrack();
                value = value.Substring(0, value.Length - 1);
            }

            if (value.Equals("a"))
            {
                //Keyword 'a'
                this._lasttokentype = Token.KEYWORDA;
                return new KeywordAToken(this.CurrentLine, this.StartPosition);
            }
            else if (value.Equals("true") || value.Equals("false"))
            {
                //Boolean Plain Literal
                this._lasttokentype = Token.PLAINLITERAL;
                return new PlainLiteralToken(this.Value, this.CurrentLine, this.StartPosition, this.EndPosition);
            }
            else if (TurtleSpecsHelper.IsValidQName(value))
            {
                //QName
                this._lasttokentype = Token.QNAME;
                return new QNameToken(this.Value, this.CurrentLine, this.StartPosition, this.EndPosition);
            }
            else
            {
                //Error
                throw Error("Unexpected input '" + value + "', expected a QName, the 'a' Keyword or a Plain Literal");
            }
        }

        private IToken TryGetDataType()
        {
            char next = this.Peek();
            if (next == '<')
            {
                //Uri for Data Type
                return this.TryGetUri();
            }
            else
            {
                //Should be a QName
                IToken qname = this.TryGetQName();
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
