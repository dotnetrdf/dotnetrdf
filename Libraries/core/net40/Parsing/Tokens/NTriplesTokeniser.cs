/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
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
            this._in = input;
            this.Format = "NTriples";
            this.Syntax = syntax;
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
            //Have we read anything yet?
            if (this.LastTokenType == -1)
            {
                //Nothing read yet so produce a BOF Token
                this.LastTokenType = Token.BOF;
                return new BOFToken();
            }
            try
            {
                do
                {
                    //Reading has started
                    this.StartNewToken();

                    //Check for EOF
                    if (this._in.EndOfStream && !this.HasBacktracked)
                    {
                        if (this.Length == 0)
                        {
                            //We're at the End of the Stream and not part-way through reading a Token
                            return new EOFToken(this.CurrentLine, this.CurrentPosition);
                        }
                        //We're at the End of the Stream and part-way through reading a Token
                        //Raise an error
                        throw UnexpectedEndOfInput("Token");
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
                        //We're at the End of the Stream and part-way through reading a Token
                        //Raise an error
                        throw UnexpectedEndOfInput("Token");
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

                            case '<':
                                //Start of a Uri
                                return this.TryGetUri();

                            case '_':
                                //Start of a  Blank Node ID
                                return this.TryGetBlankNode();

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
                                throw UnexpectedCharacter(next, "the second ^ as part of a ^^ Data Type Specifier");

                            case '.':
                                //Dot Terminator
                                this.ConsumeCharacter();
                                this.LastTokenType = Token.DOT;
                                return new DotToken(this.CurrentLine, this.StartPosition);

                            default:
                                //Unexpected Character
                                if (this.Syntax == NTriplesSyntax.Original && next > 127) throw Error("Non-ASCII characters are not permitted in Original NTriples, please set the Syntax to Rdf11 to support characters beyond the ASCII range");
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
                //Some other Error so throw
                throw;
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
                if (this.Syntax == NTriplesSyntax.Original && next > 127) throw Error("Non-ASCII characters are not permitted in Original NTriples, please set the Syntax to Rdf11 to support characters beyond the ASCII range");
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
            while (Char.IsLetterOrDigit(next) || next == '-')
            {
                if (this.Syntax == NTriplesSyntax.Original && next > 127) throw Error("Non-ASCII characters are not permitted in Original NTriples, please set the Syntax to Rdf11 to support characters beyond the ASCII range");
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
            throw Error("Unexpected Content '" + output + "' encountered, expected a valid Language Specifier");
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

                if (this.Syntax == NTriplesSyntax.Original && next > 127) throw Error("Non-ASCII characters are not permitted in Original NTriples, please set the Syntax to Rdf11 to support characters beyond the ASCII range");

                //Watch out for escapes
                if (next == '\\')
                {
                    this.HandleEscapes(this.Syntax == NTriplesSyntax.Original ? TokeniserEscapeMode.PermissiveUri : TokeniserEscapeMode.Uri);
                }
                else if (this.Syntax == NTriplesSyntax.Rdf11 && next == ' ')
                {
                    throw Error("Spaces are not valid in URIs");
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

        private IToken TryGetBlankNode()
        {
            bool colonoccurred = false;

            // Consume the opening underscore
            this.ConsumeCharacter();

            // Then expect a :
            char next = this.Peek();
            if (next != ':') throw Error("Expected a colon after a _ to start a Blank Node ID but got a " + next + " (code " + (int) next + ")");
            this.ConsumeCharacter();
            next = this.Peek();

            // Consume remainder of the Node ID
            switch (this.Syntax)
            {
                case NTriplesSyntax.Original:
                    // Original NTriples only allows very simple Node IDs
                    while (Char.IsLetterOrDigit(next))
                    {
                        if (next > 127) throw Error("Non-ASCII characters are not permitted in Original NTriples, please set the Syntax to Rdf11 to support characters beyond the ASCII range");
                        this.ConsumeCharacter();
                        next = this.Peek();
                    }
                    break;
                default:
                    // RDF 1.1 Triples allows much more complex Node IDs more similar to Turtle
                    while (Char.IsLetterOrDigit(next) || next == '-' || next == '_' || next == '.')
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
                    // We may consume the trailing dot which does not form part of the name
                    if (this.Value.EndsWith(".")) this.Backtrack();
                    break;
            }

            //Validate the QName
            if (this.Value.StartsWith("_:"))
            {
                //Blank Node ID
                this.LastTokenType = Token.BLANKNODEWITHID;
                return new BlankNodeWithIDToken(this.Value, this.CurrentLine, this.StartPosition, this.EndPosition);
            }
            throw Error("The input '" + this.Value + "' is not a valid Blank Node Name in {0}");
        }

        private IToken TryGetLiteral()
        {
            //Consume first character which must have been a "
            this.ConsumeCharacter();

            //Check if this is an empty literal
            char next = this.Peek();
            if (next == '"')
            {
                this.ConsumeCharacter();
                // Empty Literal
                this.LastTokenType = Token.LITERAL;
                return new LiteralToken(this.Value, this.CurrentLine, this.StartPosition, this.EndPosition);
            }

            // Otherwise grab the contents of the literal
            while (true)
            {
                //Handle Escapes
                if (next == '\\') 
                {
                    this.HandleEscapes(this.Syntax == NTriplesSyntax.Original ? TokeniserEscapeMode.QuotedLiterals : TokeniserEscapeMode.QuotedLiteralsBoth);
                    next = this.Peek();
                    continue;
                }

                if (this.Syntax == NTriplesSyntax.Original && next > 127) throw Error("Non-ASCII characters are not permitted in Original NTriples, please set the Syntax to Rdf11 to support characters beyond the ASCII range");

                //Add character to output buffer
                this.ConsumeCharacter();

                //Check for end of Literal
                if (next == '"')
                {
                    //End of Literal
                    this.LastTokenType = Token.LITERAL;
                    return new LiteralToken(this.Value, this.CurrentLine, this.StartPosition, this.EndPosition);
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
                return this.NQuadsMode ? new DataTypeToken("<" + temp.Value + ">", temp.StartLine, temp.StartPosition - 3, temp.EndPosition + 1) : temp;
            }
            throw UnexpectedCharacter(next, "expected a < to start a URI to specify a Data Type for a Typed Literal");
        }
    }
}
