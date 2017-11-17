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
    /// A Class for Reading an Input Stream and generating Turtle Tokens from it
    /// </summary>
    public class TurtleTokeniser 
        : BaseTokeniser
    {
        private ParsingTextReader _in;
        private TurtleSyntax _syntax = TurtleSyntax.W3C;

        /// <summary>
        /// Creates a new Turtle Tokeniser
        /// </summary>
        /// <param name="input">The Input Stream to generate Tokens from</param>
        public TurtleTokeniser(StreamReader input)
            : this(ParsingTextReader.Create(input)) { }

        /// <summary>
        /// Creates a new Turtle Tokeniser
        /// </summary>
        /// <param name="input">The Input Stream to generate Tokens from</param>
        public TurtleTokeniser(ParsingTextReader input)
            : this(input, TurtleSyntax.W3C) { }

        /// <summary>
        /// Creates a new Turtle Tokeniser
        /// </summary>
        /// <param name="input">Input to read from</param>
        public TurtleTokeniser(TextReader input)
            : this(ParsingTextReader.Create(input)) { }

        /// <summary>
        /// Creates a new Turtle Tokeniser
        /// </summary>
        /// <param name="input">The Input Stream to generate Tokens from</param>
        /// <param name="syntax">Turtle Syntax</param>
        public TurtleTokeniser(StreamReader input, TurtleSyntax syntax)
            : this(ParsingTextReader.Create(input), syntax) { }

        /// <summary>
        /// Creates a new Turtle Tokeniser
        /// </summary>
        /// <param name="input">The Input Stream to generate Tokens from</param>
        /// <param name="syntax">Turtle Syntax</param>
        public TurtleTokeniser(ParsingTextReader input, TurtleSyntax syntax)
            : base(input)
        {
            _in = input;
            Format = "Turtle";
            _syntax = syntax;
        }

        /// <summary>
        /// Creates a new Turtle Tokeniser
        /// </summary>
        /// <param name="input">Input to read from</param>
        /// <param name="syntax">Turtle Syntax</param>
        public TurtleTokeniser(TextReader input, TurtleSyntax syntax)
            : this(ParsingTextReader.Create(input), syntax) { }

        /// <summary>
        /// Gets the next parseable Token from the Input or raises an Error
        /// </summary>
        /// <returns></returns>
        /// <exception cref="RdfParseException">Occurs when a Token cannot be parsed</exception>
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
                // Reading has started

                // Reset Start and End Position Counters
                StartNewToken();

                #region Use Specific Tokenisers Section

                // Decide whether to use a specific Tokeniser function base on last Token
                // Need to use a Try Catch here as the specific functions will throw errors if the required Token can't be found
                try
                {
                    switch (LastTokenType)
                    {
                        case Token.AT:
                            return TryGetDirectiveToken();
                        case Token.BOF:
                            if (_in.EndOfStream)
                            {
                                // Empty File
                                return new EOFToken(0, 0);
                            }
                            break;

                        case Token.PREFIXDIRECTIVE:
                            return TryGetPrefixToken();

                        default:
                            break;
                    }
                }
                catch (IOException ioEx)
                {
                    // Error reading Input
                    throw new RdfParseException("Unable to Read Input successfully due to an IOException", ioEx);
                }

                #endregion

                // Local Buffer and Tokenising options
                bool newlineallowed = false;
                bool anycharallowed = false;
                bool whitespaceallowed = false;
                bool whitespaceignored = true;
                bool rightangleallowed = true;
                bool quotemarksallowed = true;
                bool altquotemarksallowed = true;
                bool longliteral = false;
                bool altlongliteral = false;

                try
                {
                    do
                    {
                        // Check for EOF
                        if (_in.EndOfStream && !HasBacktracked)
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

                        // Get the Next Character
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

                        if (Char.IsLetter(next) || UnicodeSpecsHelper.IsLetter(next) || UnicodeSpecsHelper.IsLetterModifier(next) || TurtleSpecsHelper.IsPNCharsBase(next) || UnicodeSpecsHelper.IsHighSurrogate(next) || UnicodeSpecsHelper.IsLowSurrogate(next))
                        {
                            // Alphanumeric Character Handling
                            if (anycharallowed || !quotemarksallowed)
                            {
                                // We're either reading part of a String Literal/Uri so proceed
                            }
                            else if (!UnicodeSpecsHelper.IsLetterModifier(next))
                            {
                                // Have to assume start of a QName
                                return TryGetQNameToken();                                
                            }
                        }
                        else if (Char.IsDigit(next) && !anycharallowed)
                        {
                            // Must be a plain literal
                            return TryGetNumericLiteralToken();
                        }
                        else
                        {
                            // Non Alphanumeric Character Handling
                            switch (next)
                            {

                                #region Punctuation Handling

                                #region @ Handling
                                case '@':
                                    if (!anycharallowed)
                                    {
                                        if (LastTokenType == Token.LITERAL || LastTokenType == Token.LONGLITERAL)
                                        {
                                            // Should be a Language Specifier
                                            SkipCharacter();
                                            StartNewToken();
                                            return TryGetLanguageSpecToken();
                                        }
                                        else if (LastTokenType == Token.PLAINLITERAL)
                                        {
                                            // Can't specify Language on a Plain Literal
                                            throw Error("Unexpected @ Character after a Plain Literal, Language cannot be specified for Plain Literals!");
                                        }
                                        else
                                        {
                                            // This should be the start of a directive
                                            LastTokenType = Token.AT;
                                            ConsumeCharacter();
                                            return new ATToken(CurrentLine, StartPosition);
                                        }
                                    }
                                    break;

                                #endregion

                                #region ^ Handling
                                case '^':
                                    if (!anycharallowed)
                                    {
                                        if (LastTokenType == Token.LITERAL || LastTokenType == Token.LONGLITERAL)
                                        {
                                            // Discard this and look at the next Character
                                            SkipCharacter();
                                            next = Peek();

                                            // Next character must be a ^ or we'll error
                                            if (next == '^')
                                            {
                                                // Discard this as well
                                                SkipCharacter();
                                                StartNewToken();
                                                return TryGetDataTypeToken();
                                            }
                                            else
                                            {
                                                throw UnexpectedCharacter(next, "Data Type");
                                            }

                                        }
                                        else if (LastTokenType == Token.PLAINLITERAL)
                                        {
                                            // Can't specify datatype on a Plain Literal
                                            throw Error("Unexpected ^ Character after a Plain Literal, Data Type cannot be specified for Plain Literals!");
                                        }
                                    }
                                    break;

                                #endregion

                                #region # Handling

                                case '#':
                                    if (!anycharallowed)
                                    {
                                        // Start of a Comment
                                        return TryGetCommentToken();
                                    }
                                    break;

                                #endregion

                                #region Line Terminators Handling

                                case '.':
                                    if (!anycharallowed)
                                    {
                                        ConsumeCharacter();

                                        // Watch out for plain literals
                                        if (!_in.EndOfStream && Char.IsDigit(Peek()))
                                        {
                                            return TryGetNumericLiteralToken();
                                        }
                                        else
                                        {
                                            // This should be the end of a directive
                                            LastTokenType = Token.DOT;
                                            return new DotToken(CurrentLine, StartPosition);
                                        }
                                    }
                                    break;

                                case ';':
                                    if (!anycharallowed)
                                    {
                                        // This should be the terminator of a Triple where a new Predicate and Object will be given on the subsequent line
                                        LastTokenType = Token.SEMICOLON;
                                        ConsumeCharacter();
                                        return new SemicolonToken(CurrentLine, StartPosition);
                                    }
                                    break;

                                case ',':
                                    if (!anycharallowed)
                                    {
                                        // This should be the terminator of a Triple where a new Object will be given on the subsequent line
                                        LastTokenType = Token.COMMA;
                                        ConsumeCharacter();
                                        return new CommaToken(CurrentLine, StartPosition);
                                    }
                                    break;

                                #endregion

                                #region Quotation Mark Handling (Literals)

                                case '"':
                                    if (!anycharallowed)
                                    {
                                        // Start of a String Literal
                                        StartNewToken();
                                        anycharallowed = true;
                                        whitespaceignored = false;
                                        whitespaceallowed = true;
                                        quotemarksallowed = false;
                                    }
                                    else if (quotemarksallowed && longliteral)
                                    {
                                        // Could be the end of a Long Literal

                                        ConsumeCharacter();
                                        next = Peek();

                                        if (next != '"')
                                        {
                                            // Just a quote in a long literal
                                            continue;
                                        }
                                        else
                                        {
                                            // Got Two Quote Marks in a row
                                            ConsumeCharacter();
                                            next = Peek();

                                            // Did we get the Third?
                                            if (next == '"')
                                            {
                                                // End of Long Literal
                                                ConsumeCharacter();
                                                LastTokenType = Token.LONGLITERAL;

                                                // If there are any additional quotes immediatedly following this then
                                                // we want to consume them also
                                                next = Peek();
                                                if (next == '"')
                                                {
                                                    throw Error("Too many \" characters encountered at the end of a long literal - ensure that you have escaped quotes in a long literal to avoid this error");
                                                }

                                                return new LongLiteralToken(Value, StartLine, EndLine, StartPosition, EndPosition);
                                            }
                                            else
                                            {
                                                // Just two quotes in a long literal
                                                continue;
                                            }
                                        }
                                    }
                                    else if (!quotemarksallowed)
                                    {
                                        // See if this is a Triple Quote for Long Literals
                                        // OR if it's the Empty String
                                        if (Length == 1 && Value.StartsWith("\""))
                                        {
                                            ConsumeCharacter();
                                            next = Peek();

                                            if (next == '"')
                                            {
                                                // Turn on Support for Long Literal reading
                                                newlineallowed = true;
                                                quotemarksallowed = true;
                                                longliteral = true;
                                            }
                                            else if (Char.IsWhiteSpace(next) || next == '.' || next == ';' || next == ',' || next == '^' || next == '@')
                                            {
                                                // Empty String
                                                LastTokenType = Token.LITERAL;
                                                return new LiteralToken(Value, CurrentLine, StartPosition, EndPosition);
                                            }
                                        }
                                        else
                                        {
                                            // Assume End of String Literal
                                            ConsumeCharacter();
                                            LastTokenType = Token.LITERAL;

                                            return new LiteralToken(Value, CurrentLine, StartPosition, EndPosition);
                                        }
                                    }
                                    break;

                                case '\'':
                                    if (_syntax == TurtleSyntax.W3C)
                                    {
                                        if (!anycharallowed)
                                        {
                                            // Start of a String Literal
                                            StartNewToken();
                                            anycharallowed = true;
                                            whitespaceignored = false;
                                            whitespaceallowed = true;
                                            altquotemarksallowed = false;
                                        }
                                        else if (altquotemarksallowed && altlongliteral)
                                        {
                                            // Could be the end of a Long Literal

                                            ConsumeCharacter();
                                            next = Peek();

                                            if (next != '\'')
                                            {
                                                // Just a quote in a long literal
                                                continue;
                                            }
                                            else
                                            {
                                                // Got Two Quote Marks in a row
                                                ConsumeCharacter();
                                                next = Peek();

                                                // Did we get the Third?
                                                if (next == '\'')
                                                {
                                                    // End of Long Literal
                                                    ConsumeCharacter();
                                                    LastTokenType = Token.LONGLITERAL;

                                                    // If there are any additional quotes immediatedly following this then
                                                    // we want to consume them also
                                                    next = Peek();
                                                    if (next == '\'')
                                                    {
                                                        throw Error("Too many \' characters encountered at the end of a long literal - ensure that you have escaped quotes in a long literal to avoid this error");
                                                    }

                                                    return new LongLiteralToken(Value, StartLine, EndLine, StartPosition, EndPosition);
                                                }
                                                else
                                                {
                                                    // Just two quotes in a long literal
                                                    continue;
                                                }
                                            }
                                        }
                                        else if (!altquotemarksallowed)
                                        {
                                            // See if this is a Triple Quote for Long Literals
                                            // OR if it's the Empty String
                                            if (Length == 1 && Value.StartsWith("'"))
                                            {
                                                ConsumeCharacter();
                                                next = Peek();

                                                if (next == '\'')
                                                {
                                                    // Turn on Support for Long Literal reading
                                                    newlineallowed = true;
                                                    altquotemarksallowed = true;
                                                    altlongliteral = true;
                                                }
                                                else if (Char.IsWhiteSpace(next) || next == '.' || next == ';' || next == ',' || next == '^' || next == '@')
                                                {
                                                    // Empty String
                                                    LastTokenType = Token.LITERAL;
                                                    return new LiteralToken(Value, CurrentLine, StartPosition, EndPosition);
                                                }
                                            }
                                            else
                                            {
                                                // Assume End of String Literal
                                                ConsumeCharacter();
                                                LastTokenType = Token.LITERAL;

                                                return new LiteralToken(Value, CurrentLine, StartPosition, EndPosition);
                                            }
                                        }
                                        break;
                                    }
                                    else
                                    {
                                        // Fallback to default behaviour if not W3C Turtle
                                        goto default;
                                    }

                                #endregion

                                #region Bracket Handling

                                case '(':
                                    if (!anycharallowed)
                                    {
                                        // This should be the start of a collection
                                        LastTokenType = Token.LEFTBRACKET;
                                        ConsumeCharacter();
                                        return new LeftBracketToken(CurrentLine, StartPosition);
                                    }
                                    break;

                                case ')':
                                    if (!anycharallowed)
                                    {
                                        // This should be the end of a directive
                                        LastTokenType = Token.RIGHTBRACKET;
                                        ConsumeCharacter();
                                        return new RightBracketToken(CurrentLine, StartPosition);
                                    }
                                    break;

                                case '[':
                                    if (!anycharallowed)
                                    {
                                        // This should be the start of a Blank Node
                                        LastTokenType = Token.LEFTSQBRACKET;
                                        ConsumeCharacter();
                                        return new LeftSquareBracketToken(CurrentLine, StartPosition);
                                    }
                                    break;

                                case ']':
                                    if (!anycharallowed)
                                    {
                                        // This should be the start of a Blank Node
                                        LastTokenType = Token.RIGHTSQBRACKET;
                                        ConsumeCharacter();
                                        return new RightSquareBracketToken(CurrentLine, StartPosition);
                                    }
                                    break;

                                case '{':
                                case '}':
                                    if (!anycharallowed)
                                    {
                                        // This is invalid syntax
                                        throw Error("Unexpected Character (Code " + (int)next + "): " + next + "\nThis appears to be an attempt to use a Graph Literal which is not valid in Turtle");
                                    }
                                    break;

                                #endregion

                                #region Underscore Handling

                                case '_':
                                    if (!anycharallowed)
                                    {
                                        // Start of a Blank Node QName
                                        IToken temp = TryGetQNameToken();
                                        if (temp is BlankNodeToken || temp is BlankNodeWithIDToken)
                                        {
                                            return temp;
                                        }
                                        else
                                        {
                                            throw UnexpectedToken(" when a QName for a Blank Node was expected", temp);
                                        }
                                    }
                                    break;

                                #endregion

                                #region Colon Handling
                                case ':':
                                    if (!anycharallowed)
                                    {
                                        // Start of a Default Namespace QName
                                        IToken temp = TryGetQNameToken();
                                        if (temp is QNameToken)
                                        {
                                            return temp;
                                        }
                                        else
                                        {
                                            throw UnexpectedToken(" when a QName in the Default Empty Namespace was expected", temp);
                                        }
                                    }
                                    break;

                                #endregion

                                #region Equals Sign Handling

                                case '=':
                                    if (!anycharallowed)
                                    {
                                        // This is invalid syntax of some kind
                                        // Want to work out what kind though

                                        SkipCharacter();
                                        next = Peek();

                                        if (Char.IsWhiteSpace(next))
                                        {
                                            // Equality
                                            throw Error("Unexpected = Character, this appears to be an attempt to use Equality which is not valid in Turtle");
                                        }
                                        else if (next == '>')
                                        {
                                            // Implies
                                            throw Error("Unexpected =>, this appears to be an attempt to use Implies which is not valid in Turtle");
                                        }
                                        else
                                        {
                                            // Unknown?
                                            throw Error("Unexpected = Character, = can only occur in a URI or String Literal");
                                        }
                                    }

                                    break;
                                #endregion

                                #region Minus and Plus Sign Handling

                                case '-':
                                    if (!anycharallowed)
                                    {
                                        ConsumeCharacter();
                                        return TryGetNumericLiteralToken();
                                    }
                                    break;
                                case '+':
                                    if (!anycharallowed)
                                    {
                                        ConsumeCharacter();
                                        return TryGetNumericLiteralToken();
                                    }
                                    break;

                                #endregion

                                #region Backslash Escape Handling

                                case '\\':
                                    if (anycharallowed)
                                    {
                                        // May be used as an Escape in this Context

                                        if (rightangleallowed)
                                        {
                                            HandleEscapes(_syntax == TurtleSyntax.Original ? TokeniserEscapeMode.QuotedLiterals : TokeniserEscapeMode.QuotedLiteralsBoth);
                                        }
                                        else
                                        {
                                            switch (_syntax)
                                            {
                                                case TurtleSyntax.Original:
                                                    HandleEscapes(TokeniserEscapeMode.PermissiveUri);
                                                    break;
                                                default:
                                                    HandleEscapes(TokeniserEscapeMode.Uri);
                                                    break;
                                            }
                                        }
                                        continue;
                                    }
                                    else
                                    {
                                        // Shouldn't occur outside a String Literal/Uri
                                        throw Error("Unexpected Character \\, the \\ character can only occur inside String Literals and URIs");
                                    }

                                #endregion

                                #endregion

                                #region New Line Handling

                                case '\n':
                                    // New Line
                                    if (newlineallowed)
                                    {
                                        ConsumeNewLine(true);
                                    }
                                    else if (whitespaceignored)
                                    {
                                        ConsumeNewLine(false);
                                    }
                                    else
                                    {
                                        // Raise an Error
                                        throw UnexpectedNewLine("Token");
                                    }
                                    continue;

                                case '\r':
                                    // New Line
                                    if (newlineallowed)
                                    {
                                        ConsumeNewLine(true);
                                    }
                                    else if (whitespaceignored)
                                    {
                                        ConsumeNewLine(false);
                                    }
                                    else
                                    {
                                        // Raise an Error
                                        throw UnexpectedNewLine("Token");
                                    }
                                    continue;
                                #endregion

                                #region White Space Handling

                                case ' ':
                                case '\t':
                                    if (_syntax != TurtleSyntax.Original && next == ' ' && !rightangleallowed)
                                    {
                                        throw Error("Illegal white space in URI");
                                    }
                                    if (anycharallowed || whitespaceallowed)
                                    {
                                        // We're allowing anything/whitespace so continue
                                    }
                                    else if (whitespaceignored)
                                    {
                                        // Discard the White Space
                                        SkipCharacter();
                                        continue;
                                    }
                                    else
                                    {
                                        // Got some White Space when we didn't expect it so raise error
                                        throw Error("Unexpected White Space");
                                    }
                                    break;

                                #endregion

                                #region Explicit Uri Handling

                                case '<':
                                    // Start of a Uri Token
                                    if (!anycharallowed)
                                    {
                                        StartNewToken();
                                        anycharallowed = true;
                                        rightangleallowed = false;

                                        ConsumeCharacter();
                                        next = Peek();

                                        // Check if we get a = or a > next
                                        // Want to ensure we don't get an invalid use of <= or an Empty Uri
                                        if (next == '=')
                                        {
                                            // This means we have an attempt to use <=
                                            ConsumeCharacter();
                                            next = Peek();

                                            // Need to confirm that white space follows the <= so it's a distinct token
                                            if (Char.IsWhiteSpace(next))
                                            {
                                                // Definitely a use of <=
                                                throw Error("Unexpected <=, this appears to be an attempt to use Implied By which is not valid in Turtle");
                                            }
                                        }
                                        else if (next == '>')
                                        {
                                            // Have an Empty Uri
                                            ConsumeCharacter();
                                            return new UriToken(Value, CurrentLine, StartPosition, EndPosition);
                                        }
                                    }
                                    break;

                                case '>':
                                    // End of a Uri Token
                                    if (!rightangleallowed)
                                    {
                                        // Should be end of a Uri
                                        LastTokenType = Token.URI;
                                        ConsumeCharacter();

                                        // Produce the Token
                                        if (Options.ValidateIris && _syntax == TurtleSyntax.W3C && !IriSpecsHelper.IsIri(Value.Substring(1, Length - 2))) throw Error("Illegal IRI " + Value + " encountered");
                                        return new UriToken(Value, CurrentLine, StartPosition, EndPosition);
                                    }
                                    else if (!anycharallowed)
                                    {
                                        // Raise an Error
                                        throw UnexpectedCharacter(next, String.Empty);
                                    }
                                    break;

                                #endregion

                                #region SPARQL style BASE and PREFIX Handling

                                case 'b':
                                case 'B':
                                    if (!anycharallowed)
                                    {
                                        return TryGetDirectiveToken();
                                    }
                                    break;
                                case 'p':
                                case 'P':
                                    if (!anycharallowed)
                                    {
                                        return TryGetDirectiveToken();
                                    }
                                    break;

                                #endregion

                                #region Default

                                default:
                                    if (anycharallowed)
                                    {
                                        // We're allowing anything so continue
                                    }
                                    else
                                    {
                                        // Raise an Error
                                        throw UnexpectedCharacter(next, String.Empty);
                                    }
                                    break;

                                #endregion
                            }
                        }

                        // Read in the Character to the Buffer and Increment Position Counters
                        ConsumeCharacter();

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

        /// <summary>
        /// Internal Helper method which attempts to get a Directive Token
        /// </summary>
        /// <returns></returns>
        private IToken TryGetDirectiveToken()
        {
            // Buffers
            char next = Peek();

            // Which directive do we expect?
            // 1 = Prefix, 2 = Base
            int directiveExpected = -1;

            do {
                switch (directiveExpected)
                {
                    case -1:
                        // Not sure which directive we might see yet
                        if (next == 'b')
                        {
                            directiveExpected = 2;
                        }
                        else if (next == 'p')
                        {
                            directiveExpected = 1;
                        }
                        else if (next == 'k' || next == 'K')
                        {
                            throw Error("Unexpected Character (Code " + (int)next + "): " + next + "\nThis appears to be the start of a Keywords Directive which is not valid in Turtle");
                        }
                        else if (next == 'f' || next == 'F')
                        {
                            throw Error("Unexpected Character (Code " + (int)next + "): " + next + "\nThis appears to be the start of a forAll or forSome Quantification which is not valid in Turtle");
                        }
                        else
                        {
                            throw Error("Unexpected Character (Code " + (int)next + "): " + next + "\nExpected the start of a Base/Prefix directive");
                        }
                        break;
                    case 1:
                        // Expecting a Prefix Directive
                        while (Length < 6)
                        {
                            ConsumeCharacter();
                        }
#if NETCORE
                        if (this.Value.Equals("prefix", StringComparison.OrdinalIgnoreCase))
#else
                        if (Value.Equals("prefix", StringComparison.InvariantCultureIgnoreCase))
#endif
                        {
                            // Got a Prefix Directive
                            LastTokenType = Token.PREFIXDIRECTIVE;
                            return new PrefixDirectiveToken(CurrentLine, StartPosition);
                        }
                        else
                        {
                            // Not what we expected so Error
                            throw Error("Expected a Prefix Directive and got '" + Value + "'");
                        }
                    case 2:
                        // Expecting a Base Directive
                        while (Length < 4)
                        {
                            ConsumeCharacter();
                        }
#if NETCORE
                        if (this.Value.Equals("base", StringComparison.OrdinalIgnoreCase))
#else                        
                        if (Value.Equals("base", StringComparison.InvariantCultureIgnoreCase))
#endif
                        {
                            // Got a Base Directive
                            LastTokenType = Token.BASEDIRECTIVE;
                            return new BaseDirectiveToken(CurrentLine, StartPosition);
                        }
                        else
                        {
                            throw Error("Expected a Base Directive and got '" + Value + "'");
                        }
                    default:
                        throw Error("Unknown Parsing Error in TurtleTokeniser.TryGetDirectiveToken()");
                }

                // Should only hit this once when we do the first case to decide which directive we'll get
             } while (true);
        }

        /// <summary>
        /// Internal Helper method which attempts to get a Prefix Token
        /// </summary>
        /// <returns></returns>
        private IToken TryGetPrefixToken()
        {
            char next = Peek();

            // Drop leading white space
            while (Char.IsWhiteSpace(next))
            {
                // If we hit a New Line then Error
                if (next == '\n' || next == '\r') 
                {
                    throw UnexpectedNewLine("Prefix");
                }

                // Discard unecessary White Space
                SkipCharacter();
                next = Peek();
            }

            StartNewToken();

            // Get the Prefix Characters
            while (!Char.IsWhiteSpace(next) && next != '<')
            {
                ConsumeCharacter();
                if (next == ':') break;
                next = Peek();
            }
            if (!Value.EndsWith(":"))
            {
                throw new RdfParseException("Didn't find expected : Character while attempting to parse Prefix at content:\n" + Value + "\nPrefixes must end in a Colon Character", StartLine, CurrentLine, StartPosition, CurrentPosition);
            }
            if (!TurtleSpecsHelper.IsValidPrefix(Value, _syntax))
            {
                throw new RdfParseException("The value '" + Value + "' is not a valid Prefix in Turtle", new PositionInfo(StartLine, CurrentLine, StartPosition, EndPosition));
            }

            // Produce a PrefixToken
            LastTokenType = Token.PREFIX;
            return new PrefixToken(Value, CurrentLine, StartPosition, EndPosition);
        }

        /// <summary>
        /// Internal Helper method which attempts to get a QName Token
        /// </summary>
        /// <returns></returns>
        /// <remarks>In fact this function may return a number of Tokens depending on the characters it finds.  It may find a QName, Plain Literal, Blank Node QName (with ID) or Keyword.  QName &amp; Keyword Validation is carried out by this function</remarks>
        private IToken TryGetQNameToken()
        {
            char next = Peek();
            bool colonoccurred = false;
            StringComparison comparison = (_syntax == TurtleSyntax.Original ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase);

            StartNewToken();

            // Grab all the Characters in the QName
            while (!Char.IsWhiteSpace(next) && next != ';' && next != ',' && next != '(' && next != ')' && next != '[' && next != ']' && next != '#' && (next != '.' || _syntax == TurtleSyntax.W3C))
            {
                // Can't have more than one Colon in a QName unless we're using the W3C syntax
                if (next == ':' && !colonoccurred)
                {
                    colonoccurred = true;
                }
                else if (next == ':' && _syntax == TurtleSyntax.Original) 
                {
                    throw Error("Unexpected additional Colon Character while trying to parse a QName from content:\n" + Value + "\nQNames can only contain 1 Colon character");
                }

                // A Backslash allow for unicode escapes in QNames or reserved name escapes in local names
                if (next == '\\')
                {
                    if (colonoccurred)
                    {
                        // If we are in local name portion complex escapes apply
                        HandleComplexLocalNameEscapes();
                    }
                    else
                    {
                        // Prior to first colon only standard escapes apply
                        HandleEscapes(TokeniserEscapeMode.QName);
                    }
                    // If escape is handled characters have already been consumed so must continue to avoid double consumption
                    next = Peek();
                    continue;
                }

                ConsumeCharacter();
                next = Peek();
                if (_in.EndOfStream) break;
            }

            // If it ends in a trailing . then we need to backtrack
            if (Value.EndsWith(".")) Backtrack();

            if (colonoccurred)
            {
                // A QName must contain a Colon at some point
                String qname = Value;

                // Was this a Blank Node
                if (qname.StartsWith("_:"))
                {
                    // Blank Node with an ID
                    if (qname.Length == 2)
                    {
                        // No ID
                        return new BlankNodeToken(CurrentLine, StartPosition);
                    }
                    else
                    {
                        // User specified ID
                        return new BlankNodeWithIDToken(qname, CurrentLine, StartPosition, EndPosition);
                    }
                }
                else if (qname.StartsWith("-"))
                {
                    // Illegal use of - to start a QName
                    throw Error("The - Character cannot be used at the start of a QName");
                }
                else if (qname.StartsWith("."))
                {
                    // Illegal use of . to start a QName
                    throw Error("The . Character cannot be used at the start of a QName");
                }
                else
                {
                    if (!TurtleSpecsHelper.IsValidQName(qname, _syntax))
                    {
                        throw Error("The QName " + qname + " is not valid in Turtle");
                    }

                    // Normal QName
                    LastTokenType = Token.QNAME;
                    return new QNameToken(qname, CurrentLine, StartPosition, EndPosition);
                }
            }
            else
            {
                // If we don't see a Colon then have to assume a Plain Literal
                // BUT we also need to check it's not a keyword
                String value = Value;

                if (value.Equals("a"))
                {
                    // The 'a' Keyword
                    LastTokenType = Token.KEYWORDA;
                    return new KeywordAToken(CurrentLine, StartPosition);
                }
                else if (value.Equals("is"))
                {
                    // This 'is' Keyword
                    throw Error("The 'is' Keyword is not Valid in Turtle");
                }
                else if (value.Equals("of"))
                {
                    // The 'of' Keyword
                    throw Error("The 'of' Keyword is not Valid in Turtle");
                }
                else if (value.Equals("base", StringComparison.OrdinalIgnoreCase))
                {
                    LastTokenType = Token.BASEDIRECTIVE;
                    return new BaseDirectiveToken(CurrentLine, StartPosition);
                }
                else if (value.Equals("prefix", StringComparison.OrdinalIgnoreCase))
                {
                    LastTokenType = Token.PREFIXDIRECTIVE;
                    return new PrefixDirectiveToken(CurrentLine, StartPosition);
                }
                else
                {
                    // Must be a Plain Literal
                    if (!TurtleSpecsHelper.IsValidPlainLiteral(value, _syntax))
                    {
                        throw Error("The value of the Plain Literal '" + value + "' is not valid in Turtle.  Turtle supports Boolean, Integer, Decimal and Double Plain Literals");
                    }
                    LastTokenType = Token.PLAINLITERAL;
                    return new PlainLiteralToken(value, CurrentLine, StartPosition, EndPosition);
                }
            }
        }

        private IToken TryGetNumericLiteralToken()
        {
            bool dotoccurred = false;
            bool signoccurred = false;
            bool expoccurred = false;

            if (Length == 1)
            {
                switch (Value[0])
                {
                    case '.':
                        dotoccurred = true;
                        break;
                    case '+':
                        signoccurred = true;
                        break;
                    case '-':
                        signoccurred = true;
                        break;
                    default:
                        throw Error("Unexpected state while trying to parse a plain literal");
                }
            }
            else
            {
                StartNewToken();
            }

            // Find acceptable characters
            char next = Peek();
            bool exit = false;
            while (Char.IsDigit(next) || next == '.' || next == '+' || next == '-' || next == 'e' || next == 'E')
            {
                switch (next)
                {
                    case '.':
                        // If we've already seen a dot and we see another assume that it is the subsequent triple terminator and not part of the literal
                        if (dotoccurred) exit = true;
                        dotoccurred = true;
                        break;
                    case '+':
                    case '-':
                        // Seeing another sign is illegal if one has already been seen
                        if (signoccurred) throw Error("Unexpected additional sign (" + next + ") character while trying to parse a signed numeric literal");
                        if (Length != 0 && !expoccurred) throw Error("Unexpected sign (" + next + ") character while trying to parse a numeric literal, sign may only occur at the start of the literal or immediately after an exponent");
                        if (expoccurred && Value[Length - 1] != 'e' && Value[Length - 1] != 'E') throw Error("Unexpected sign (" + next + ") character, sign may only occur at the start of the literal or immediatedly after an exponent");
                        signoccurred = true;
                        break;
                    case 'e':
                    case 'E':
                        // Seeing another sign is illegal if one has already been seen
                        if (expoccurred) throw Error("Unexpected additional exponent character while trying to parse a numeric literal");
                        expoccurred = true;
                        signoccurred = false; //We need to allow sign characters after an exponent
                        break;
                }

                if (exit) break;

                ConsumeCharacter();
                next = Peek();
                if (_in.EndOfStream) break;
            }

            // Backtrack if we get a trailing .
            if (Value.EndsWith(".")) Backtrack();
            
            if (TurtleSpecsHelper.IsValidPlainLiteral(Value, _syntax))
            {
                LastTokenType = Token.PLAINLITERAL;
                return new PlainLiteralToken(Value, CurrentLine, StartPosition, EndPosition);
            }
            else
            {
                throw Error("The numeric literal " + Value + " is not valid in Turtle");
            }
        }

        /// <summary>
        /// Internal Helper method which attempts to get a Language Specifier Token
        /// </summary>
        /// <returns></returns>
        private IToken TryGetLanguageSpecToken()
        {
            char next = Peek();

            // Consume Letter Characters
            while (Char.IsLetterOrDigit(next) || next == '-')
            {
                ConsumeCharacter();
                next = Peek();
            }

            if (Length == 0)
            {
                // Empty output so no Language Specifier
                throw UnexpectedCharacter(next, "Language Specifier for preceding Literal Token");
            }
            else
            {
                if (RdfSpecsHelper.IsValidLangSpecifier(Value))
                {
                    LastTokenType = Token.LANGSPEC;
                    return new LanguageSpecifierToken(Value, CurrentLine, StartPosition, EndPosition);
                }
                else
                {
                    throw Error("Unexpected Content '" + Value + "' encountered, expected a valid Language Specifier");
                }
            }
        }

        /// <summary>
        /// Internal Helper method which attempts to get a Date Type Token
        /// </summary>
        /// <returns></returns>
        private IToken TryGetDataTypeToken()
        {
            char next = Peek();

            if (next == '<')
            {
                // DataType is specified by a Uri
                ConsumeCharacter();
                next = Peek();

                // Get Characters while they're valid
                while (next != '>' && next != '\n' && next != '\r')
                {
                    // Append to output
                    ConsumeCharacter();
                    next = Peek();
                }

                // Check we didn't hit an illegal character
                if (next == '\n' || next == '\r')
                {
                    throw UnexpectedNewLine("DataType URI");
                }
                else
                {
                    // Get the final >
                    ConsumeCharacter();
                }

                return new DataTypeToken(Value, CurrentLine, StartPosition, EndPosition);
            }
            else
            {
                // DataType is specified by a QName
                IToken temp = TryGetQNameToken();
                if (temp is QNameToken)
                {
                    // Turn into a DataTypeToken
                    return new DataTypeToken(temp.Value, temp.StartLine, temp.StartPosition, temp.EndPosition);
                }
                else
                {
                    // If we got a PlainLiteralToken or anything else something went wrong
                    throw Error("Parsed a '" + temp.GetType().ToString() + "' Token while attempting to get a QNameToken for a DataType");
                }
            }

        }

        /// <summary>
        /// Internal Helper method which attempts to get a Comment Token
        /// </summary>
        /// <returns></returns>
        private IToken TryGetCommentToken()
        {
            char next = Peek();

            // Grab characters until we hit the new line
            while (next != '\n' && next != '\r')
            {
                if (ConsumeCharacter(true)) break;
                next = Peek();
            }

            // Discard New line and reset position
            CommentToken comment = new CommentToken(Value, CurrentLine, StartPosition, EndPosition);
            ConsumeNewLine(false, true);
            return comment;
        }
    }
}
