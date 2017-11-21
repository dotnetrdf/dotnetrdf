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
    /// Tokeniser for tokenising CSV inputs
    /// </summary>
    public class CsvTokeniser
        : BaseTokeniser
    {
        private ParsingTextReader _in;

        /// <summary>
        /// Creates a new CSV Tokeniser
        /// </summary>
        /// <param name="reader">Text Reader</param>
        public CsvTokeniser(ParsingTextReader reader)
            : base(reader)
        {
            _in = reader;
        }

        /// <summary>
        /// Creates a new CSV Tokeniser
        /// </summary>
        /// <param name="reader">Stream Reader</param>
        public CsvTokeniser(StreamReader reader)
            : this(ParsingTextReader.Create(reader)) { }
        
        /// <summary>
        /// Gets the next available token from the input
        /// </summary>
        /// <returns></returns>
        public override IToken GetNextToken()
        {
            // Have we read anything yet?
            if (LastTokenType == -1)
            {
                // Nothing read yet so produce a BOF Token
                LastTokenType = Token.BOF;
                return new BOFToken();
            }
            else
            {
                try
                {
                    // Reading has started
                    StartNewToken();

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
                            throw UnexpectedEndOfInput("Token");
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

                    switch (next)
                    {
                        case ',':
                            // Comma
                            ConsumeCharacter();
                            LastTokenType = Token.COMMA;
                            return new CommaToken(StartLine, StartPosition);

                        case '\r':
                        case '\n':
                            // New Line
                            ConsumeNewLine(true);
                            LastTokenType = Token.EOL;
                            return new EOLToken(StartLine, StartPosition);

                        case '"':
                            // Start of a Quoted Field
                            return TryGetQuotedField();

                        default:
                            // Start of an Unquoted Field
                            return TryGetUnquotedField();
                    }

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

        private IToken TryGetUnquotedField()
        {
            char next = Peek();
            while (next != ',' && next != '\n' && next != '\r')
            {
                ConsumeCharacter();
                next = Peek();
            }

            if (Value.StartsWith("_:"))
            {
                return new BlankNodeWithIDToken(Value, StartLine, StartPosition, EndPosition);
            }
            else
            {
                return new PlainLiteralToken(Value, StartLine, StartPosition, EndPosition);
            }
        }

        private IToken TryGetQuotedField()
        {
            ConsumeCharacter();
            char next = Peek();
            do
            {
                if (next == '"')
                {
                    // May be end of quoted field unless followed immediately by another quote
                    ConsumeCharacter();
                    next = Peek();
                    if (next == '"')
                    {
                        // Just a "" to escape a quote, skip the 2nd quote and continue
                        SkipCharacter();
                    }
                    else if (next == ',' || next == '\n' || next == '\r' || _in.EndOfStream)
                    {
                        // Otherwise if a comma/new line/EOF it's the end of the field
                        break;
                    }
                    else
                    {
                        // Anything else is invalid end of field
                        throw Error("Unexpected end of quoted field");
                    }
                }
                else if (next == '\r' || next == '\n')
                {
                    // New lines are permitted inside quoted fields, use consume method to consume correctly
                    ConsumeNewLine(true, false);
                }
                else if (_in.EndOfStream)
                {
                    throw UnexpectedEndOfInput("quoted field");
                }
                else
                {
                    // Any other character has literal meaning and is consumed as-is
                    ConsumeCharacter();
                }

                if (_in.EndOfStream) throw UnexpectedEndOfInput("quoted field");

                next = Peek();
            } while (true);

            return new LiteralToken(Value, StartLine, EndLine, StartPosition, EndPosition);
        }
    }
}
