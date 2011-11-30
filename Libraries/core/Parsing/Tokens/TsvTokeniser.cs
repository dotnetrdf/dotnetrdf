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

If this license is not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;
using System.IO;
using VDS.RDF.Query;

namespace VDS.RDF.Parsing.Tokens
{
    /// <summary>
    /// Tokeniser for tokenising TSV inputs
    /// </summary>
    public class TsvTokeniser
        : BaseTokeniser
    {
        private BlockingTextReader _in;

        /// <summary>
        /// Creates a new TSV Tokeniser
        /// </summary>
        /// <param name="reader">Text Reader</param>
        public TsvTokeniser(BlockingTextReader reader)
            : base(reader)
        {
            this._in = reader;
        }

        /// <summary>
        /// Creates a new TSV Tokeniser
        /// </summary>
        /// <param name="reader">Stream Reader</param>
        public TsvTokeniser(StreamReader reader)
            : this(new BlockingTextReader(reader)) { }
        
        /// <summary>
        /// Gets the next available token from the input
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

                        if (Char.IsDigit(next) || next == '+' || next == '-')
                        {
                            return this.TryGetNumericLiteral();
                        }

                        switch (next)
                        {
                            case '\t':
                                //Tab
                                this.ConsumeCharacter();
                                this.LastTokenType = Token.TAB;
                                return new TabToken(this.StartLine, this.StartPosition);

                            case '\r':
                            case '\n':
                                //New Line
                                this.ConsumeNewLine(true);
                                this.LastTokenType = Token.EOL;
                                return new EOLToken(this.StartLine, this.StartPosition);

                            case '.':
                                //Dot
                                this.ConsumeCharacter();
                                return this.TryGetNumericLiteral();

                            case '@':
                                //Start of a Keyword or Language Specifier
                                return this.TryGetLangSpec();

                            case '?':
                                //Start of a Variable
                                return this.TryGetVariable();

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

                            case 't':
                            case 'f':
                                //Plain Literal?
                                return this.TryGetPlainLiteral();

                            default:
                                //Unexpected Character
                                throw this.UnexpectedCharacter(next, String.Empty);
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

        private IToken TryGetVariable()
        {
            //Consume first Character which must be a ?/$
            this.ConsumeCharacter();

            //Consume other valid Characters
            char next = this.Peek();
            while (Char.IsLetterOrDigit(next) || UnicodeSpecsHelper.IsLetterOrDigit(next) || next == '-' || next == '_' || next == '\\')
            {
                if (next == '\\')
                {
                    //Check its a valid Escape
                    this.HandleEscapes(TokeniserEscapeMode.QName);
                }
                else
                {
                    this.ConsumeCharacter();
                }
                next = this.Peek();
            }

            //Validate
            String value = this.Value;

            if (value.EndsWith("."))
            {
                this.Backtrack();
                value = value.Substring(0, value.Length - 1);
            }

            if (SparqlSpecsHelper.IsValidVarName(value))
            {
                return new VariableToken(value, this.CurrentLine, this.StartPosition, this.EndPosition);
            }
            else
            {
                throw Error("The value '" + value + "' is not valid a Variable Name");
            }

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

        private IToken TryGetNumericLiteral()
        {
            bool dotoccurred = false;
            bool expoccurred = false;
            bool negoccurred = false;

            if (this.Length == 1) dotoccurred = true;

            char next = this.Peek();

            while (Char.IsDigit(next) || next == '-' || next == '+' || next == 'e' || (next == '.' && !dotoccurred))
            {
                //Consume the Character
                this.ConsumeCharacter();

                if (next == '+')
                {
                    //Can only be first character in the numeric literal or come immediately after the 'e'
                    if (this.Length > 1 && !this.Value.EndsWith("e+"))
                    {
                        throw Error("Unexpected Character (Code " + (int)next + ") +\nThe plus sign can only occur once at the Start of a Numeric Literal and once immediately after the 'e' exponent specifier, if this was intended as an additive operator please insert space to disambiguate this");
                    }
                }
                if (next == '-')
                {
                    if (negoccurred && !this.Value.EndsWith("e-"))
                    {
                        //Negative sign already seen
                        throw Error("Unexpected Character (Code " + (int)next + ") -\nThe minus sign can only occur once at the Start of a Numeric Literal, if this was intended as a subtractive operator please insert space to disambiguate this");
                    }
                    else
                    {
                        negoccurred = true;

                        //Check this is at the start of the string or immediately after the 'e'
                        if (this.Length > 1 && !this.Value.EndsWith("e-"))
                        {
                            throw Error("Unexpected Character (Code " + (int)next + ") -\nThe minus sign can only occur at the Start of a Numeric Literal and once immediately after the 'e' exponent specifier, if this was intended as a subtractive operator please insert space to disambiguate this");
                        }
                    }
                }
                else if (next == 'e')
                {
                    if (expoccurred)
                    {
                        //Exponent already seen
                        throw Error("Unexpected Character (Code " + (int)next + " e\nThe Exponent specifier can only occur once in a Numeric Literal");
                    }
                    else
                    {
                        expoccurred = true;

                        //Check that it isn't the start of the string
                        if (this.Length == 1)
                        {
                            throw Error("Unexpected Character (Code " + (int)next + " e\nThe Exponent specifier cannot occur at the start of a Numeric Literal");
                        }
                    }
                }
                else if (next == '.')
                {
                    dotoccurred = true;
                }

                next = this.Peek();
            }

            //Decimals can't end with a . so backtrack
            if (this.Value.EndsWith(".")) this.Backtrack();

            String value = this.Value;
            if (!SparqlSpecsHelper.IsValidNumericLiteral(value))
            {
                //Invalid Numeric Literal
                throw Error("The format of the Numeric Literal '" + this.Value + "' is not valid!");
            }

            //Return the Token
            this.LastTokenType = Token.PLAINLITERAL;
            return new PlainLiteralToken(this.Value, this.CurrentLine, this.StartPosition, this.EndPosition);
        }

        private IToken TryGetPlainLiteral()
        {
            this.ConsumeCharacter();
            char next = this.Peek();
            while (Char.IsLetter(next))
            {
                this.ConsumeCharacter();
                next = this.Peek();
            }

            if (TurtleSpecsHelper.IsValidPlainLiteral(this.Value))
            {
                this.LastTokenType = Token.PLAINLITERAL;
                return new PlainLiteralToken(this.Value, this.StartLine, this.StartPosition, this.EndPosition);
            }
            else
            {
                throw new RdfParseException("'" + this.Value + "' is not a valid Plain Literal!");
            }
        }

        private IToken TryGetDataType()
        {
            char next = this.Peek();
            if (next == '<')
            {
                //Uri for Data Type
                IToken temp = this.TryGetUri();
                return new DataTypeToken("<" + temp.Value + ">", temp.StartLine, temp.StartPosition - 3, temp.EndPosition + 1);
            }
            else
            {
                throw UnexpectedCharacter(next, "expected a < to start a URI to specify a Data Type for a Typed Literal");
            }
        }
    }
}
