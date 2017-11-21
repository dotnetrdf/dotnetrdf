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
    /// Tokeniser for NTriples RDF Syntax
    /// </summary>
    public class NTriplesTokeniser
        : BaseTokeniser
    {
        private readonly ParsingTextReader _in;

        /// <summary>
        /// Creates a new NTriples Tokeniser which reads Tokens from the given Stream
        /// </summary>
        /// <param name="input">Stream to read Tokens from</param>
        /// <param name="syntax">NTriples syntax to tokenise</param>
        public NTriplesTokeniser(ParsingTextReader input, NTriplesSyntax syntax)
            : base(input)
        {
            NQuadsMode = false;
            _in = input;
            Format = "NTriples";
            Syntax = syntax;
        }

        /// <summary>
        /// Creates a new NTriples Tokeniser which reads Tokens from the given Stream
        /// </summary>
        /// <param name="input">Stream to read Tokens from</param>
        public NTriplesTokeniser(ParsingTextReader input)
            : this(input, NTriplesSyntax.Rdf11) { }

        /// <summary>
        /// Creates a new NTriples Tokeniser which reads Tokens from the given Stream
        /// </summary>
        /// <param name="input">Stream to read Tokens from</param>
        public NTriplesTokeniser(StreamReader input) 
            : this(ParsingTextReader.Create(input)) { }

        /// <summary>
        /// Creates a new NTriples Tokeniser which reads Tokens from the given Input
        /// </summary>
        /// <param name="input">Input to read Tokens from</param>
        public NTriplesTokeniser(TextReader input)
            : this(ParsingTextReader.Create(input)) { }

        /// <summary>
        /// Creates a new NTriples Tokeniser which reads Tokens from the given Stream
        /// </summary>
        /// <param name="input">Stream to read Tokens from</param>
        /// <param name="syntax">NTriples syntax to tokenise</param>
        public NTriplesTokeniser(StreamReader input, NTriplesSyntax syntax)
            : this(ParsingTextReader.Create(input), syntax) { }

        /// <summary>
        /// Creates a new NTriples Tokeniser which reads Tokens from the given Input
        /// </summary>
        /// <param name="input">Input to read Tokens from</param>
        /// <param name="syntax">NTriples syntax to tokenise</param>
        public NTriplesTokeniser(TextReader input, NTriplesSyntax syntax)
            : this(ParsingTextReader.Create(input), syntax) { }

        /// <summary>
        /// Gets/Sets the NTriples syntax that should be supported
        /// </summary>
        public NTriplesSyntax Syntax { get; set; }

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
        public bool NQuadsMode { get; set; }

        /// <summary>
        /// Gets the next available Token from the Input Stream
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
            try
            {
                do
                {
                    // Reading has started
                    StartNewToken();

                    // Check for EOF
                    if (_in.EndOfStream && !HasBacktracked)
                    {
                        if (Length == 0)
                        {
                            // We're at the End of the Stream and not part-way through reading a Token
                            return new EOFToken(CurrentLine, CurrentPosition);
                        }
                        // We're at the End of the Stream and part-way through reading a Token
                        // Raise an error
                        throw UnexpectedEndOfInput("Token");
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
                        // We're at the End of the Stream and part-way through reading a Token
                        // Raise an error
                        throw UnexpectedEndOfInput("Token");
                    }

                    if (Char.IsWhiteSpace(next))
                    {
                        // Discard white space between Tokens
                        DiscardWhiteSpace();
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
                                return TryGetLangSpec();

                            case '<':
                                // Start of a Uri
                                return TryGetUri();

                            case '_':
                                // Start of a  Blank Node ID
                                return TryGetBlankNode();

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
                                throw UnexpectedCharacter(next, "the second ^ as part of a ^^ Data Type Specifier");

                            case '.':
                                // Dot Terminator
                                ConsumeCharacter();
                                LastTokenType = Token.DOT;
                                return new DotToken(CurrentLine, StartPosition);

                            default:
                                // Unexpected Character
                                if (Syntax == NTriplesSyntax.Original && next > 127) throw Error("Non-ASCII characters are not permitted in Original NTriples, please set the Syntax to Rdf11 to support characters beyond the ASCII range");
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
                // Some other Error so throw
                throw;
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
                if (Syntax == NTriplesSyntax.Original && next > 127) throw Error("Non-ASCII characters are not permitted in Original NTriples, please set the Syntax to Rdf11 to support characters beyond the ASCII range");
                if (ConsumeCharacter(true)) break;
                next = Peek();
            }

            // Create the Token, discard the new line and return
            LastTokenType = Token.COMMENT;
            CommentToken comment = new CommentToken(Value, CurrentLine, StartPosition, EndPosition);
            ConsumeNewLine(false, true);
            return comment;
        }

        private IToken TryGetLangSpec()
        {
            // Skip the first Character which must have been an @
            SkipCharacter();

            // Consume characters which can be in the keyword or Language Specifier
            char next = Peek();
            while (Char.IsLetterOrDigit(next) || next == '-')
            {
                if (Syntax == NTriplesSyntax.Original && next > 127) throw Error("Non-ASCII characters are not permitted in Original NTriples, please set the Syntax to Rdf11 to support characters beyond the ASCII range");
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
            throw Error("Unexpected Content '" + output + "' encountered, expected a valid Language Specifier");
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

                if (Syntax == NTriplesSyntax.Original && next > 127) throw Error("Non-ASCII characters are not permitted in Original NTriples, please set the Syntax to Rdf11 to support characters beyond the ASCII range");

                // Watch out for escapes
                if (next == '\\')
                {
                    HandleEscapes(Syntax == NTriplesSyntax.Original ? TokeniserEscapeMode.PermissiveUri : TokeniserEscapeMode.Uri);
                }
                else if (Syntax == NTriplesSyntax.Rdf11 && next == ' ')
                {
                    throw Error("Spaces are not valid in URIs");
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

        private IToken TryGetBlankNode()
        {
            bool colonoccurred = false;

            // Consume the opening underscore
            ConsumeCharacter();

            // Then expect a :
            char next = Peek();
            if (next != ':') throw Error("Expected a colon after a _ to start a Blank Node ID but got a " + next + " (code " + (int) next + ")");
            ConsumeCharacter();
            next = Peek();

            // Consume remainder of the Node ID
            switch (Syntax)
            {
                case NTriplesSyntax.Original:
                    // Original NTriples only allows very simple Node IDs
                    while (Char.IsLetterOrDigit(next))
                    {
                        if (next > 127) throw Error("Non-ASCII characters are not permitted in Original NTriples, please set the Syntax to Rdf11 to support characters beyond the ASCII range");
                        ConsumeCharacter();
                        next = Peek();
                    }
                    break;
                default:
                    // RDF 1.1 Triples allows much more complex Node IDs more similar to Turtle
                    while (Char.IsLetterOrDigit(next) || next == '-' || next == '_' || next == '.')
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
                    // We may consume the trailing dot which does not form part of the name
                    if (Value.EndsWith(".")) Backtrack();
                    break;
            }

            // Validate the QName
            if (Value.StartsWith("_:"))
            {
                // Blank Node ID
                LastTokenType = Token.BLANKNODEWITHID;
                return new BlankNodeWithIDToken(Value, CurrentLine, StartPosition, EndPosition);
            }
            throw Error("The input '" + Value + "' is not a valid Blank Node Name in {0}");
        }

        private IToken TryGetLiteral()
        {
            // Consume first character which must have been a "
            ConsumeCharacter();

            // Check if this is an empty literal
            char next = Peek();
            if (next == '"')
            {
                ConsumeCharacter();
                // Empty Literal
                LastTokenType = Token.LITERAL;
                return new LiteralToken(Value, CurrentLine, StartPosition, EndPosition);
            }

            // Otherwise grab the contents of the literal
            while (true)
            {
                // Handle Escapes
                if (next == '\\') 
                {
                    HandleEscapes(Syntax == NTriplesSyntax.Original ? TokeniserEscapeMode.QuotedLiterals : TokeniserEscapeMode.QuotedLiteralsBoth);
                    next = Peek();
                    continue;
                }

                if (Syntax == NTriplesSyntax.Original && next > 127) throw Error("Non-ASCII characters are not permitted in Original NTriples, please set the Syntax to Rdf11 to support characters beyond the ASCII range");

                // Add character to output buffer
                ConsumeCharacter();

                // Check for end of Literal
                if (next == '"')
                {
                    // End of Literal
                    LastTokenType = Token.LITERAL;
                    return new LiteralToken(Value, CurrentLine, StartPosition, EndPosition);
                }

                // Continue Reading
                next = Peek();
            }
        }

        private IToken TryGetDataType()
        {
            char next = Peek();
            if (next == '<')
            {
                // Uri for Data Type
                IToken temp = TryGetUri();
                return NQuadsMode ? new DataTypeToken("<" + temp.Value + ">", temp.StartLine, temp.StartPosition - 3, temp.EndPosition + 1) : temp;
            }
            throw UnexpectedCharacter(next, "expected a < to start a URI to specify a Data Type for a Typed Literal");
        }
    }
}
