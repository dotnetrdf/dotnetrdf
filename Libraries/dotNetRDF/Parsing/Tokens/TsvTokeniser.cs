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
using VDS.RDF.Query;

namespace VDS.RDF.Parsing.Tokens
{
    /// <summary>
    /// Tokeniser for tokenising TSV inputs
    /// </summary>
    public class TsvTokeniser
        : BaseTokeniser
    {
        private ParsingTextReader _in;

        /// <summary>
        /// Creates a new TSV Tokeniser
        /// </summary>
        /// <param name="reader">Text Reader</param>
        public TsvTokeniser(ParsingTextReader reader)
            : base(reader)
        {
            _in = reader;
        }

        /// <summary>
        /// Creates a new TSV Tokeniser
        /// </summary>
        /// <param name="reader">Stream Reader</param>
        public TsvTokeniser(StreamReader reader)
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

                    if (Char.IsDigit(next) || next == '+' || next == '-')
                    {
                        return TryGetNumericLiteral();
                    }

                    switch (next)
                    {
                        case '\t':
                            // Tab
                            ConsumeCharacter();
                            LastTokenType = Token.TAB;
                            return new TabToken(StartLine, StartPosition);

                        case '\r':
                        case '\n':
                            // New Line
                            ConsumeNewLine(true);
                            LastTokenType = Token.EOL;
                            return new EOLToken(StartLine, StartPosition);

                        case '.':
                            // Dot
                            ConsumeCharacter();
                            return TryGetNumericLiteral();

                        case '@':
                            // Start of a Keyword or Language Specifier
                            return TryGetLangSpec();

                        case '?':
                            // Start of a Variable
                            return TryGetVariable();

                        case '<':
                            // Start of a Uri
                            return TryGetUri();

                        case '_':
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
                                // Try and get the Datatype
                                StartNewToken();
                                return TryGetDataType();
                            }
                            else
                            {
                                throw UnexpectedCharacter(next, "the second ^ as part of a ^^ Data Type Specifier");
                            }

                        case 't':
                        case 'f':
                            // Plain Literal?
                            return TryGetPlainLiteral();

                        default:
                            // Unexpected Character
                            throw UnexpectedCharacter(next, String.Empty);
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

        private IToken TryGetVariable()
        {
            // Consume first Character which must be a ?/$
            ConsumeCharacter();

            // Consume other valid Characters
            char next = Peek();
            while (Char.IsLetterOrDigit(next) || UnicodeSpecsHelper.IsLetterOrDigit(next) || next == '-' || next == '_' || next == '\\')
            {
                if (next == '\\')
                {
                    // Check its a valid Escape
                    HandleEscapes(TokeniserEscapeMode.QName);
                }
                else
                {
                    ConsumeCharacter();
                }
                next = Peek();
            }

            // Validate
            String value = Value;

            if (value.EndsWith("."))
            {
                Backtrack();
                value = value.Substring(0, value.Length - 1);
            }

            if (SparqlSpecsHelper.IsValidVarName(value))
            {
                return new VariableToken(value, CurrentLine, StartPosition, EndPosition);
            }
            else
            {
                throw Error("The value '" + value + "' is not valid a Variable Name");
            }

        }

        private IToken TryGetLangSpec()
        {
            // Skip the first Character which must have been an @
            SkipCharacter();

            // Consume characters which can be in the keyword or Language Specifier
            char next = Peek();
            while (Char.IsLetterOrDigit(next) || next == '-')
            {
                ConsumeCharacter();
                next = Peek();
            }

            // Check the output to see if it's valid
            String output = Value;
            if (RdfSpecsHelper.IsValidLangSpecifier(output))
            {
                LastTokenType = Token.LANGSPEC;
                return new LanguageSpecifierToken(output, CurrentLine, StartPosition, EndPosition);
            }
            else
            {
                throw Error("Unexpected Content '" + output + "' encountered, expected a valid Language Specifier");
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
                    HandleEscapes(TokeniserEscapeMode.Uri);
                }
                else
                {
                    ConsumeCharacter();
                }

            } while (next != '>');

            // Return the Token
            LastTokenType = Token.URI;
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

            // Validate the QName
            if (Value.StartsWith("_:"))
            {
                // Blank Node ID
                LastTokenType = Token.BLANKNODEWITHID;
                return new BlankNodeWithIDToken(Value, CurrentLine, StartPosition, EndPosition);
            }
            else
            {
                throw Error("The input '" + Value + "' is not a valid Blank Node Name in {0}");
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
                }
                else
                {
                    // Empty Literal
                    LastTokenType = Token.LITERAL;
                    return new LiteralToken(Value, CurrentLine, StartPosition, EndPosition);
                }
            }

            while (true)
            {
                // Handle Escapes
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
                                LastTokenType = Token.LONGLITERAL;
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
                        LastTokenType = Token.LITERAL;
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
            bool negoccurred = false;

            if (Length == 1) dotoccurred = true;

            char next = Peek();

            while (Char.IsDigit(next) || next == '-' || next == '+' || next == 'e' || (next == '.' && !dotoccurred))
            {
                // Consume the Character
                ConsumeCharacter();

                if (next == '+')
                {
                    // Can only be first character in the numeric literal or come immediately after the 'e'
                    if (Length > 1 && !Value.EndsWith("e+"))
                    {
                        throw Error("Unexpected Character (Code " + (int)next + ") +\nThe plus sign can only occur once at the Start of a Numeric Literal and once immediately after the 'e' exponent specifier, if this was intended as an additive operator please insert space to disambiguate this");
                    }
                }
                if (next == '-')
                {
                    if (negoccurred && !Value.EndsWith("e-"))
                    {
                        // Negative sign already seen
                        throw Error("Unexpected Character (Code " + (int)next + ") -\nThe minus sign can only occur once at the Start of a Numeric Literal, if this was intended as a subtractive operator please insert space to disambiguate this");
                    }
                    else
                    {
                        negoccurred = true;

                        // Check this is at the start of the string or immediately after the 'e'
                        if (Length > 1 && !Value.EndsWith("e-"))
                        {
                            throw Error("Unexpected Character (Code " + (int)next + ") -\nThe minus sign can only occur at the Start of a Numeric Literal and once immediately after the 'e' exponent specifier, if this was intended as a subtractive operator please insert space to disambiguate this");
                        }
                    }
                }
                else if (next == 'e')
                {
                    if (expoccurred)
                    {
                        // Exponent already seen
                        throw Error("Unexpected Character (Code " + (int)next + " e\nThe Exponent specifier can only occur once in a Numeric Literal");
                    }
                    else
                    {
                        expoccurred = true;

                        // Check that it isn't the start of the string
                        if (Length == 1)
                        {
                            throw Error("Unexpected Character (Code " + (int)next + " e\nThe Exponent specifier cannot occur at the start of a Numeric Literal");
                        }
                    }
                }
                else if (next == '.')
                {
                    dotoccurred = true;
                }

                next = Peek();
            }

            // Decimals can't end with a . so backtrack
            if (Value.EndsWith(".")) Backtrack();

            String value = Value;
            if (!SparqlSpecsHelper.IsValidNumericLiteral(value))
            {
                // Invalid Numeric Literal
                throw Error("The format of the Numeric Literal '" + Value + "' is not valid!");
            }

            // Return the Token
            LastTokenType = Token.PLAINLITERAL;
            return new PlainLiteralToken(Value, CurrentLine, StartPosition, EndPosition);
        }

        private IToken TryGetPlainLiteral()
        {
            ConsumeCharacter();
            char next = Peek();
            while (Char.IsLetter(next))
            {
                ConsumeCharacter();
                next = Peek();
            }

            if (TurtleSpecsHelper.IsValidPlainLiteral(Value, TurtleSyntax.Original))
            {
                LastTokenType = Token.PLAINLITERAL;
                return new PlainLiteralToken(Value, StartLine, StartPosition, EndPosition);
            }
            else
            {
                throw new RdfParseException("'" + Value + "' is not a valid Plain Literal!");
            }
        }

        private IToken TryGetDataType()
        {
            char next = Peek();
            if (next == '<')
            {
                // Uri for Data Type
                IToken temp = TryGetUri();
                return new DataTypeToken("<" + temp.Value + ">", temp.StartLine, temp.StartPosition - 3, temp.EndPosition + 1);
            }
            else
            {
                throw UnexpectedCharacter(next, "expected a < to start a URI to specify a Data Type for a Typed Literal");
            }
        }
    }
}
