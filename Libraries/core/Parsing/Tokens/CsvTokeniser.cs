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
    /// Tokeniser for tokenising CSV inputs
    /// </summary>
    public class CsvTokeniser
        : BaseTokeniser
    {
        private BlockingTextReader _in;

        /// <summary>
        /// Creates a new CSV Tokeniser
        /// </summary>
        /// <param name="reader">Text Reader</param>
        public CsvTokeniser(BlockingTextReader reader)
            : base(reader)
        {
            this._in = reader;
        }

        /// <summary>
        /// Creates a new CSV Tokeniser
        /// </summary>
        /// <param name="reader">Stream Reader</param>
        public CsvTokeniser(StreamReader reader)
            : this(BlockingTextReader.Create(reader)) { }
        
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

                    switch (next)
                    {
                        case ',':
                            //Comma
                            this.ConsumeCharacter();
                            this.LastTokenType = Token.COMMA;
                            return new CommaToken(this.StartLine, this.StartPosition);

                        case '\r':
                        case '\n':
                            //New Line
                            this.ConsumeNewLine(true);
                            this.LastTokenType = Token.EOL;
                            return new EOLToken(this.StartLine, this.StartPosition);

                        case '"':
                            //Start of a Quoted Field
                            return this.TryGetQuotedField();

                        default:
                            //Start of an Unquoted Field
                            return this.TryGetUnquotedField();
                    }

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

        private IToken TryGetUnquotedField()
        {
            char next = this.Peek();
            while (next != ',' && next != '\n' && next != '\r')
            {
                this.ConsumeCharacter();
                next = this.Peek();
            }

            if (this.Value.StartsWith("_:"))
            {
                return new BlankNodeWithIDToken(this.Value, this.StartLine, this.StartPosition, this.EndPosition);
            }
            else
            {
                return new PlainLiteralToken(this.Value, this.StartLine, this.StartPosition, this.EndPosition);
            }
        }

        private IToken TryGetQuotedField()
        {
            this.ConsumeCharacter();
            char next = this.Peek();
            do
            {
                if (next == '"')
                {
                    //May be end of quoted field unless followed immediately by another quote
                    this.ConsumeCharacter();
                    next = this.Peek();
                    if (next == '"')
                    {
                        //Just a "" to escape a quote, skip the 2nd quote and continue
                        this.SkipCharacter();
                    }
                    else if (next == ',' || next == '\n' || next == '\r' || this._in.EndOfStream)
                    {
                        //Otherwise if a comma/new line/EOF it's the end of the field
                        break;
                    }
                    else
                    {
                        //Anything else is invalid end of field
                        throw Error("Unexpected end of quoted field");
                    }
                }
                else if (next == '\r' || next == '\n')
                {
                    //New lines are permitted inside quoted fields, use consume method to consume correctly
                    this.ConsumeNewLine(true, false);
                }
                else if (this._in.EndOfStream)
                {
                    throw UnexpectedEndOfInput("quoted field");
                }
                else
                {
                    //Any other character has literal meaning and is consumed as-is
                    this.ConsumeCharacter();
                }

                if (this._in.EndOfStream) throw UnexpectedEndOfInput("quoted field");

                next = this.Peek();
            } while (true);

            return new LiteralToken(this.Value, this.StartLine, this.EndLine, this.StartPosition, this.EndPosition);
        }
    }
}
