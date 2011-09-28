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
    /// Tokeniser for NTriples RDF Syntax
    /// </summary>
    public class NTriplesTokeniser
        : BaseTokeniser
    {
        private BlockingTextReader _in;
        private bool _nquadsMode = false;

        /// <summary>
        /// Creates a new NTriples Tokeniser which reads Tokens from the given Stream
        /// </summary>
        /// <param name="input">Stream to read Tokens from</param>
        public NTriplesTokeniser(StreamReader input) 
            : this(new BlockingTextReader(input)) { }

        /// <summary>
        /// Creates a new NTriples Tokeniser which reads Tokens from the given Stream
        /// </summary>
        /// <param name="input">Stream to read Tokens from</param>
        public NTriplesTokeniser(BlockingTextReader input)
            : base(input)
        {
            this._in = input;
            this.Format = "NTriples";
        }

        /// <summary>
        /// Creates a new NTriples Tokeniser which reads Tokens from the given Input
        /// </summary>
        /// <param name="input">Input to read Tokens from</param>
        public NTriplesTokeniser(TextReader input)
            : this(new BlockingTextReader(input)) { }

        /// <summary>
        /// Gets/Sets whether the output should be altered slightly to support NQuads parsing
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is used internally to alter how DataTypes get tokenised, normally these are just returned as a <see cref="UriToken">UriToken</see> since a Literal can only occur as the Object in NTriples and so if we see a Uri after a Literal it must be it's datatype and not part of another Triple.
        /// </para>
        /// <para>
        /// In the case of NQuads a <see cref="UriToken">UriToken</see> may follow a Literal as the Context of that Triple and not its datatype so it's important to distinguish by using a <see cref="DataTypeToken">DataTypeToken</see> instead
        /// </para>
        /// </remarks>
        public bool NQuadsMode
        {
            get
            {
                return this._nquadsMode;
            }
            set
            {
                this._nquadsMode = value;
            }
        }

        /// <summary>
        /// Gets the next available Token from the Input Stream
        /// </summary>
        /// <returns></returns>
        public override IToken GetNextToken()
        {
            //Have we read anything yet?
            if (this.LastTokenType == -1)
            {
                //Nothing read yet so produce a BOF Token
                this.LastTokenType = Token.BOF;
                return new BOFToken();
            }
            else
            {
                try
                {
                    do
                    {
                        //Reading has started
                        this.StartNewToken();

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
                                throw UnexpectedEndOfInput("Token");
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
                        else
                        {
                            switch (next)
                            {
                                case '#':
                                    //Comment
                                    return this.TryGetComment();

                                case '@':
                                    //Start of a Keyword or Language Specifier
                                    return this.TryGetLangSpec();

                                #region URIs, QNames and Literals

                                case '<':
                                    //Start of a Uri
                                    return this.TryGetUri();

                                case '_':
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
                                        //Try and get the Datatype
                                        this.StartNewToken();
                                        return this.TryGetDataType();
                                    }
                                    else
                                    {
                                        throw UnexpectedCharacter(next, "the second ^ as part of a ^^ Data Type Specifier");
                                    }

                                #endregion

                                #region Line Terminators

                                case '.':
                                    //Dot Terminator
                                    this.ConsumeCharacter();
                                    this.LastTokenType = Token.DOT;
                                    return new DotToken(this.CurrentLine, this.StartPosition);

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
            this.LastTokenType = Token.COMMENT;
            CommentToken comment = new CommentToken(this.Value, this.CurrentLine, this.StartPosition, this.EndPosition);
            this.ConsumeNewLine(false, true);
            return comment;
        }

        private IToken TryGetLangSpec()
        {
            //Skip the first Character which must have been an @
            this.SkipCharacter();

            //Consume characters which can be in the keyword or Language Specifier
            char next = this.Peek();
            while (Char.IsLetter(next) || next == '-')
            {
                this.ConsumeCharacter();
                next = this.Peek();
            }

            //Check the output to see if it's valid
            String output = this.Value;
            if (RdfSpecsHelper.IsValidLangSpecifier(output))
            {
                this.LastTokenType = Token.LANGSPEC;
                return new LanguageSpecifierToken(output, this.CurrentLine, this.StartPosition, this.EndPosition);
            }
            else
            {
                throw Error("Unexpected Content '" + output + "' encountered, expected a valid Language Specifier");
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
            this.LastTokenType = Token.URI;
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

            //Validate the QName
            if (this.Value.StartsWith("_:"))
            {
                //Blank Node ID
                this.LastTokenType = Token.BLANKNODEWITHID;
                return new BlankNodeWithIDToken(this.Value, this.CurrentLine, this.StartPosition, this.EndPosition);
            }
            else
            {
                throw Error("The input '" + this.Value + "' is not a valid Blank Node Name in {0}");
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
                }
                else
                {
                    //Empty Literal
                    this.LastTokenType = Token.LITERAL;
                    return new LiteralToken(this.Value, this.CurrentLine, this.StartPosition, this.EndPosition);
                }
            }

            while (true)
            {
                //Handle Escapes
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
                                this.LastTokenType = Token.LONGLITERAL;
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
                        this.LastTokenType = Token.LITERAL;
                        return new LiteralToken(this.Value, this.CurrentLine, this.StartPosition, this.EndPosition);
                    }
                }

                //Continue Reading
                next = this.Peek();
            }
        }

        private IToken TryGetDataType()
        {
            char next = this.Peek();
            if (next == '<')
            {
                //Uri for Data Type
                IToken temp = this.TryGetUri();
                if (this._nquadsMode)
                {
                    //Wrap in a DataType token
                    return new DataTypeToken("<" + temp.Value + ">", temp.StartLine, temp.StartPosition - 3, temp.EndPosition + 1);
                }
                else
                {
                    return temp;
                }
            }
            else
            {
                throw UnexpectedCharacter(next, "expected a < to start a URI to specify a Data Type for a Typed Literal");
            }
        }
    }
}
