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
    /// A Class for Reading an Input Stream and generating Turtle Tokens from it
    /// </summary>
    public class TurtleTokeniser : BaseTokeniser
    {
        private BlockingTextReader _in;

        /// <summary>
        /// Creates a new Turtle Tokeniser
        /// </summary>
        /// <param name="input">The Input Stream to generate Tokens from</param>
        public TurtleTokeniser(StreamReader input)
            : this(new BlockingTextReader(input)) { }

        /// <summary>
        /// Creates a new Turtle Tokeniser
        /// </summary>
        /// <param name="input">The Input Stream to generate Tokens from</param>
        public TurtleTokeniser(BlockingTextReader input)
            : base(input)
        {
            this._in = input;
            this.Format = "Turtle";
        }

        /// <summary>
        /// Creates a new Turtle Tokeniser
        /// </summary>
        /// <param name="input">Input to read from</param>
        public TurtleTokeniser(TextReader input)
            : this(new BlockingTextReader(input)) { }

        /// <summary>
        /// Gets the next parseable Token from the Input or raises an Error
        /// </summary>
        /// <returns></returns>
        /// <exception cref="RdfParseException">Occurs when a Token cannot be parsed</exception>
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
                //Reading has started

                //Reset Start and End Position Counters
                this.StartNewToken();

                #region Use Specific Tokenisers Section

                //Decide whether to use a specific Tokeniser function base on last Token
                //Need to use a Try Catch here as the specific functions will throw errors if the required Token can't be found
                try
                {
                    switch (this.LastTokenType)
                    {
                        case Token.AT:
                            return this.TryGetDirectiveToken();
                        case Token.BOF:
                            if (this._in.EndOfStream)
                            {
                                //Empty File
                                return new EOFToken(0, 0);
                            }
                            break;

                        case Token.PREFIXDIRECTIVE:
                            return this.TryGetPrefixToken();

                        default:
                            break;
                    }
                }
                catch (IOException ioEx)
                {
                    //Error reading Input
                    throw new RdfParseException("Unable to Read Input successfully due to an IOException", ioEx);
                }

                #endregion

                //Local Buffer and Tokenising options
                bool newlineallowed = false;
                bool anycharallowed = false;
                bool whitespaceallowed = false;
                bool whitespaceignored = true;
                bool rightangleallowed = true;
                bool quotemarksallowed = true;
                bool longliteral = false;

                try
                {
                    do
                    {
                        //Check for EOF
                        if (this._in.EndOfStream && !this.HasBacktracked)
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

                        //Get the Next Character
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

                        if (Char.IsLetterOrDigit(next) || UnicodeSpecsHelper.IsLetterOrDigit(next) || UnicodeSpecsHelper.IsLetterModifier(next))
                        {
                            //Alphanumeric Character Handling
                            if (anycharallowed || !quotemarksallowed)
                            {
                                //We're either reading part of a String Literal/Uri so proceed
                            }
                            else if (!UnicodeSpecsHelper.IsLetterModifier(next))
                            {
                                //Have to assume start of a QName
                                return this.TryGetQNameToken();                                
                            }
                        }
                        else
                        {
                            //Non Alphanumeric Character Handling
                            switch (next)
                            {

                                #region Punctuation Handling

                                #region @ Handling
                                case '@':
                                    if (!anycharallowed)
                                    {
                                        if (this.LastTokenType == Token.LITERAL || this.LastTokenType == Token.LONGLITERAL)
                                        {
                                            //Should be a Language Specifier
                                            this.SkipCharacter();
                                            this.StartNewToken();
                                            return this.TryGetLanguageSpecToken();
                                        }
                                        else if (this.LastTokenType == Token.PLAINLITERAL)
                                        {
                                            //Can't specify Language on a Plain Literal
                                            throw Error("Unexpected @ Character after a Plain Literal, Language cannot be specified for Plain Literals!");
                                        }
                                        else
                                        {
                                            //This should be the start of a directive
                                            this.LastTokenType = Token.AT;
                                            this.ConsumeCharacter();
                                            return new ATToken(this.CurrentLine, this.StartPosition);
                                        }
                                    }
                                    break;

                                #endregion

                                #region ^ Handling
                                case '^':
                                    if (!anycharallowed)
                                    {
                                        if (this.LastTokenType == Token.LITERAL || this.LastTokenType == Token.LONGLITERAL)
                                        {
                                            //Discard this and look at the next Character
                                            this.SkipCharacter();
                                            next = this.Peek();

                                            //Next character must be a ^ or we'll error
                                            if (next == '^')
                                            {
                                                //Discard this as well
                                                this.SkipCharacter();
                                                this.StartNewToken();
                                                return this.TryGetDataTypeToken();
                                            }
                                            else
                                            {
                                                throw UnexpectedCharacter(next, "Data Type");
                                            }

                                        }
                                        else if (this.LastTokenType == Token.PLAINLITERAL)
                                        {
                                            //Can't specify datatype on a Plain Literal
                                            throw Error("Unexpected ^ Character after a Plain Literal, Data Type cannot be specified for Plain Literals!");
                                        }
                                    }
                                    break;

                                #endregion

                                #region # Handling

                                case '#':
                                    if (!anycharallowed)
                                    {
                                        //Start of a Comment
                                        return this.TryGetCommentToken();
                                    }
                                    break;

                                #endregion

                                #region Line Terminators Handling

                                case '.':
                                    if (!anycharallowed)
                                    {
                                        this.ConsumeCharacter();

                                        //Watch our for plain literals
                                        if (!this._in.EndOfStream && Char.IsDigit(this.Peek()))
                                        {
                                            IToken temp = this.TryGetQNameToken();
                                            if (temp is PlainLiteralToken)
                                            {
                                                this.LastTokenType = Token.PLAINLITERAL;
                                                return temp;
                                            }
                                            else
                                            {
                                                throw UnexpectedToken("Plain Literal", temp);
                                            }
                                        } 
                                        else 
                                        {
                                            //This should be the end of a directive
                                            this.LastTokenType = Token.DOT;
                                            return new DotToken(this.CurrentLine, this.StartPosition);
                                        }
                                    }
                                    break;

                                case ';':
                                    if (!anycharallowed)
                                    {
                                        //This should be the terminator of a Triple where a new Predicate and Object will be given on the subsequent line
                                        this.LastTokenType = Token.SEMICOLON;
                                        this.ConsumeCharacter();
                                        return new SemicolonToken(this.CurrentLine, this.StartPosition);
                                    }
                                    break;

                                case ',':
                                    if (!anycharallowed)
                                    {
                                        //This should be the terminator of a Triple where a new Object will be given on the subsequent line
                                        this.LastTokenType = Token.COMMA;
                                        this.ConsumeCharacter();
                                        return new CommaToken(this.CurrentLine, this.StartPosition);
                                    }
                                    break;

                                #endregion

                                #region Quotation Mark Handling

                                case '"':
                                    if (!anycharallowed)
                                    {
                                        //Start of a String Literal
                                        this.StartNewToken();
                                        anycharallowed = true;
                                        whitespaceignored = false;
                                        whitespaceallowed = true;
                                        quotemarksallowed = false;
                                    }
                                    else if (quotemarksallowed && longliteral)
                                    {
                                        //Could be the end of a Long Literal

                                        this.ConsumeCharacter();
                                        next = this.Peek();

                                        if (next != '"')
                                        {
                                            //Just a quote in a long literal
                                            continue;
                                        }
                                        else
                                        {
                                            //Got Two Quote Marks in a row
                                            this.ConsumeCharacter();
                                            next = this.Peek();

                                            //Did we get the Third?
                                            if (next == '"')
                                            {
                                                //End of Long Literal
                                                this.ConsumeCharacter();
                                                this.LastTokenType = Token.LONGLITERAL;

                                                //If there are any additional quotes immediatedly following this then
                                                //we want to consume them also
                                                next = this.Peek();
                                                if (next == '"')
                                                {
                                                    throw Error("Too many \" characters encountered at the end of a long literal - ensure that you have escaped quotes in a long literal to avoid this error");
                                                }

                                                return new LongLiteralToken(this.Value, this.StartLine, this.EndLine, this.StartPosition, this.EndPosition);
                                            }
                                            else
                                            {
                                                //Just two quotes in a long literal
                                                continue;
                                            }
                                        }
                                    }
                                    else if (!quotemarksallowed)
                                    {
                                        //See if this is a Triple Quote for Long Literals
                                        //OR if it's the Empty String
                                        if (this.Length == 1 && this.Value.StartsWith("\""))
                                        {
                                            this.ConsumeCharacter();
                                            next = this.Peek();

                                            if (next == '"')
                                            {
                                                //Turn on Support for Long Literal reading
                                                newlineallowed = true;
                                                quotemarksallowed = true;
                                                longliteral = true;
                                            }
                                            else if (Char.IsWhiteSpace(next) || next == '.' || next == ';' || next == ',' || next == '^' || next == '@')
                                            {
                                                //Empty String
                                                this.LastTokenType = Token.LITERAL;
                                                return new LiteralToken(this.Value, this.CurrentLine, this.StartPosition, this.EndPosition);
                                            }
                                        }
                                        else
                                        {
                                            //Assume End of String Literal
                                            this.ConsumeCharacter();
                                            this.LastTokenType = Token.LITERAL;

                                            return new LiteralToken(this.Value, this.CurrentLine, this.StartPosition, this.EndPosition);
                                        }
                                    }
                                    break;

                                #endregion

                                #region Bracket Handling

                                case '(':
                                    if (!anycharallowed)
                                    {
                                        //This should be the start of a collection
                                        this.LastTokenType = Token.LEFTBRACKET;
                                        this.ConsumeCharacter();
                                        return new LeftBracketToken(this.CurrentLine, this.StartPosition);
                                    }
                                    break;

                                case ')':
                                    if (!anycharallowed)
                                    {
                                        //This should be the end of a directive
                                        this.LastTokenType = Token.RIGHTBRACKET;
                                        this.ConsumeCharacter();
                                        return new RightBracketToken(this.CurrentLine, this.StartPosition);
                                    }
                                    break;

                                case '[':
                                    if (!anycharallowed)
                                    {
                                        //This should be the start of a Blank Node
                                        this.LastTokenType = Token.LEFTSQBRACKET;
                                        this.ConsumeCharacter();
                                        return new LeftSquareBracketToken(this.CurrentLine, this.StartPosition);
                                    }
                                    break;

                                case ']':
                                    if (!anycharallowed)
                                    {
                                        //This should be the start of a Blank Node
                                        this.LastTokenType = Token.RIGHTSQBRACKET;
                                        this.ConsumeCharacter();
                                        return new RightSquareBracketToken(this.CurrentLine, this.StartPosition);
                                    }
                                    break;

                                case '{':
                                case '}':
                                    if (!anycharallowed)
                                    {
                                        //This is invalid syntax
                                        throw Error("Unexpected Character (Code " + (int)next + "): " + next + "\nThis appears to be an attempt to use a Graph Literal which is not valid in Turtle");
                                    }
                                    break;

                                #endregion

                                #region Underscore Handling

                                case '_':
                                    if (!anycharallowed)
                                    {
                                        //Start of a Blank Node QName
                                        IToken temp = this.TryGetQNameToken();
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
                                        //Start of a Default Namespace QName
                                        IToken temp = this.TryGetQNameToken();
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
                                        //This is invalid syntax of some kind
                                        //Want to work out what kind though

                                        this.SkipCharacter();
                                        next = this.Peek();

                                        if (Char.IsWhiteSpace(next))
                                        {
                                            //Equality
                                            throw Error("Unexpected = Character, this appears to be an attempt to use Equality which is not valid in Turtle");
                                        }
                                        else if (next == '>')
                                        {
                                            //Implies
                                            throw Error("Unexpected =>, this appears to be an attempt to use Implies which is not valid in Turtle");
                                        }
                                        else
                                        {
                                            //Unknown?
                                            throw Error("Unexpected = Character, = can only occur in a URI or String Literal");
                                        }
                                    }

                                    break;
                                #endregion

                                #region Minus and Plus Sign Handling

                                case '-':
                                    if (!anycharallowed)
                                    {
                                        //Should only occur at Start of a Negative Plain Literal
                                        IToken temp = this.TryGetQNameToken();
                                        if (temp is PlainLiteralToken)
                                        {
                                            return temp;
                                        }
                                        else
                                        {
                                            throw UnexpectedToken(" when a Plain Literal of a Negative Number was expected", temp);
                                        }
                                    }
                                    break;
                                case '+':
                                    if (!anycharallowed)
                                    {
                                        //Should only occur at Start of a Positive Plain Literal
                                        IToken temp = this.TryGetQNameToken();
                                        if (temp is PlainLiteralToken)
                                        {
                                            return temp;
                                        }
                                        else
                                        {
                                            throw UnexpectedToken(" when a Plain Literal of a Positive Number was expected", temp);
                                        }
                                    }
                                    break;

                                #endregion

                                #region Backslash Escape Handling

                                case '\\':
                                    if (anycharallowed)
                                    {
                                        //May be used as an Escape in this Context

                                        if (rightangleallowed)
                                        {
                                            this.HandleEscapes(TokeniserEscapeMode.QuotedLiterals);
                                        }
                                        else
                                        {
                                            this.HandleEscapes(TokeniserEscapeMode.Uri);
                                        }
                                        continue;
                                    }
                                    else
                                    {
                                        //Shouldn't occur outside a String Literal/Uri
                                        throw Error("Unexpected Character \\, the \\ character can only occur inside String Literals and URIs");
                                    }

                                #endregion

                                #endregion

                                #region New Line Handling

                                case '\n':
                                    //New Line
                                    if (newlineallowed)
                                    {
                                        this.ConsumeNewLine(true);
                                    }
                                    else if (whitespaceignored)
                                    {
                                        this.ConsumeNewLine(false);
                                    }
                                    else
                                    {
                                        //Raise an Error
                                        throw UnexpectedNewLine("Token");
                                    }
                                    continue;

                                case '\r':
                                    //New Line
                                    if (newlineallowed)
                                    {
                                        this.ConsumeNewLine(true);
                                    }
                                    else if (whitespaceignored)
                                    {
                                        this.ConsumeNewLine(false);
                                    }
                                    else
                                    {
                                        //Raise an Error
                                        throw UnexpectedNewLine("Token");
                                    }
                                    continue;
                                #endregion

                                #region White Space Handling

                                case ' ':
                                case '\t':
                                    if (anycharallowed || whitespaceallowed)
                                    {
                                        //We're allowing anything/whitespace so continue
                                    }
                                    else if (whitespaceignored)
                                    {
                                        //Discard the White Space
                                        this.SkipCharacter();
                                        continue;
                                    }
                                    else
                                    {
                                        //Got some White Space when we didn't expect it so raise error
                                        throw Error("Unexpected White Space");
                                    }
                                    break;

                                #endregion

                                #region Explicit Uri Handling

                                case '<':
                                    //Start of a Uri Token
                                    if (!anycharallowed)
                                    {
                                        this.StartNewToken();
                                        anycharallowed = true;
                                        rightangleallowed = false;

                                        this.ConsumeCharacter();
                                        next = this.Peek();

                                        //Check if we get a = or a > next
                                        //Want to ensure we don't get an invalid use of <= or an Empty Uri
                                        if (next == '=')
                                        {
                                            //This means we have an attempt to use <=
                                            this.ConsumeCharacter();
                                            next = this.Peek();

                                            //Need to confirm that white space follows the <= so it's a distinct token
                                            if (Char.IsWhiteSpace(next))
                                            {
                                                //Definitely a use of <=
                                                throw Error("Unexpected <=, this appears to be an attempt to use Implied By which is not valid in Turtle");
                                            }
                                        }
                                        else if (next == '>')
                                        {
                                            //Have an Empty Uri
                                            this.ConsumeCharacter();
                                            return new UriToken(this.Value, this.CurrentLine, this.StartPosition, this.EndPosition);
                                        }
                                    }
                                    break;

                                case '>':
                                    //End of a Uri Token
                                    if (!rightangleallowed)
                                    {
                                        //Should be end of a Uri
                                        this.LastTokenType = Token.URI;
                                        this.ConsumeCharacter();

                                        //Produce the Token
                                        return new UriToken(this.Value, this.CurrentLine, this.StartPosition, this.EndPosition);
                                    }
                                    else if (!anycharallowed)
                                    {
                                        //Raise an Error
                                        throw UnexpectedCharacter(next, String.Empty);
                                    }
                                    break;

                                #endregion

                                #region Default

                                default:
                                    if (anycharallowed)
                                    {
                                        //We're allowing anything so continue
                                    }
                                    else
                                    {
                                        //Raise an Error
                                        throw UnexpectedCharacter(next, String.Empty);
                                    }
                                    break;

                                #endregion
                            }
                        }

                        //Read in the Character to the Buffer and Increment Position Counters
                        this.ConsumeCharacter();

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

        /// <summary>
        /// Internal Helper method which attempts to get a Directive Token
        /// </summary>
        /// <returns></returns>
        private IToken TryGetDirectiveToken()
        {
            //Buffers
            char next = this.Peek();

            //Which directive do we expect?
            //1 = Prefix, 2 = Base
            int directiveExpected = -1;

            do {
                switch (directiveExpected)
                {
                    case -1:
                        //Not sure which directive we might see yet
                        if (next == 'b' || next == 'B')
                        {
                            directiveExpected = 2;
                        }
                        else if (next == 'p' || next == 'P')
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
                        //Expecting a Prefix Directive
                        while (this.Length < 6)
                        {
                            this.ConsumeCharacter();
                        }
                        if (this.Value.Equals("prefix", StringComparison.InvariantCultureIgnoreCase))
                        {
                            //Got a Prefix Directive
                            this.LastTokenType = Token.PREFIXDIRECTIVE;
                            return new PrefixDirectiveToken(this.CurrentLine, this.StartPosition);
                        }
                        else
                        {
                            //Not what we expected so Error
                            throw Error("Expected a Prefix Directive and got '" + this.Value + "'");
                        }
                    case 2:
                        //Expecting a Base Directive
                        while (this.Length < 4)
                        {
                            this.ConsumeCharacter();
                        }
                        if (this.Value.Equals("base", StringComparison.InvariantCultureIgnoreCase))
                        {
                            //Got a Base Directive
                            this.LastTokenType = Token.BASEDIRECTIVE;
                            return new BaseDirectiveToken(this.CurrentLine, this.StartPosition);
                        }
                        else
                        {
                            throw Error("Expected a Base Directive and got '" + this.Value + "'");
                        }
                    default:
                        throw Error("Unknown Parsing Error in TurtleTokeniser.TryGetDirectiveToken()");
                }

                //Should only hit this once when we do the first case to decide which directive we'll get
             } while (true);
        }

        /// <summary>
        /// Internal Helper method which attempts to get a Prefix Token
        /// </summary>
        /// <returns></returns>
        private IToken TryGetPrefixToken()
        {
            char next = this.Peek();

            //Drop leading white space
            while (Char.IsWhiteSpace(next))
            {
                //If we hit a New Line then Error
                if (next == '\n' || next == '\r') 
                {
                    throw UnexpectedNewLine("Prefix");
                }

                //Discard unecessary White Space
                this.SkipCharacter();
                next = this.Peek();
            }

            this.StartNewToken();

            //Get the Prefix Characters
            while (!Char.IsWhiteSpace(next))
            {
                this.ConsumeCharacter();
                next = this.Peek();
            }
            if (!this.Value.EndsWith(":"))
            {
                throw new RdfParseException("Didn't find expected : Character while attempting to parse Prefix at content:\n" + this.Value + "\nPrefixes must end in a Colon Character", this.StartLine, this.CurrentLine, this.StartPosition, this.CurrentPosition);
            }
            if (!TurtleSpecsHelper.IsValidQName(this.Value))
            {
                throw new RdfParseException("The value '" + this.Value + "' is not a valid Prefix in Turtle", new PositionInfo(this.StartLine, this.CurrentLine, this.StartPosition, this.EndPosition));
            }

            //Produce a PrefixToken
            this.LastTokenType = Token.PREFIX;
            return new PrefixToken(this.Value, this.CurrentLine, this.StartPosition, this.EndPosition);
        }

        /// <summary>
        /// Internal Helper method which attempts to get a QName Token
        /// </summary>
        /// <returns></returns>
        /// <remarks>In fact this function may return a number of Tokens depending on the characters it finds.  It may find a QName, Plain Literal, Blank Node QName (with ID) or Keyword.  QName &amp; Keyword Validation is carried out by this function</remarks>
        private IToken TryGetQNameToken()
        {
            char next = this.Peek();
            bool colonoccurred = false;
            bool dotoccurred = false;

            if (this.Length == 1 && this.Value[0] == '.')
            {
                dotoccurred = true;
            }
            else
            {
                this.StartNewToken();
            }

            //Grab all the Characters in the QName
            while (next == ':' || Char.IsLetterOrDigit(next) || next == '_' || next == '-' || next == '+' || (next == '.' && !colonoccurred))
            {
                //Can't have more than one Colon in a QName
                if (next == ':' && !colonoccurred)
                {
                    colonoccurred = true;
                }
                else if (next == ':') 
                {
                    throw Error("Unexpected additional Colon Character while trying to parse a QName from content:\n" + this.Value + "\nQNames can only contain 1 Colon character");
                }

                //Can't have more than one Dot in a QName
                if (next == '.' && !dotoccurred)
                {
                    if (this.Value.Equals("true") || this.Value.Equals("false")) break;
                    dotoccurred = true;
                }
                else if (next == '.')
                {
                    throw Error("Unexpected additional Dot Character while trying to parse a Plain Literal from content:\n" + this.Value + "\nPlain Literals can only contain 1 Dot Character, ensure you use White Space after a Plain Literal which contains a dot to avoid ambiguity");
                }

                this.ConsumeCharacter();
                next = this.Peek();
            }

            //If it ends in a trailing . then we need to backtrack
            if (this.Value.EndsWith(".")) this.Backtrack();

            if (colonoccurred && !dotoccurred)
            {
                //A QName must contain a Colon at some point
                String qname = this.Value;

                //Was this a Blank Node
                if (qname.StartsWith("_:"))
                {
                    //Blank Node with an ID
                    if (qname.Length == 2)
                    {
                        //No ID
                        return new BlankNodeToken(this.CurrentLine, this.StartPosition);
                    }
                    else
                    {
                        //User specified ID
                        return new BlankNodeWithIDToken(qname, this.CurrentLine, this.StartPosition, this.EndPosition);
                    }
                }
                else if (qname.StartsWith("-"))
                {
                    //Illegal use of - to start a QName
                    throw Error("The - Character cannot be used at the start of a QName");
                }
                else
                {
                    if (qname.Length > 1) 
                    {
                        //Check Illegal use of - or a Digit to start a Local Name
                        String[] localname = qname.Split(':');
                        if (localname[1].Length >= 1)
                        {
                            if (localname[1].StartsWith("-"))
                            {
                                throw Error("The - Character cannot be used as the start of a Local Name within a QName");
                            }
                            //Check for Illegal use of a Digit to start a Local Name
                            char[] lnamechar = localname[1].Substring(0, 1).ToCharArray();
                            if (Char.IsDigit(lnamechar[0]))
                            {
                                throw Error("A Local Name within a QName may not start with a Number");
                            }
                        }

                    }

                    //QNames can't start with a Digit
                    char[] firstchar = qname.Substring(0, 1).ToCharArray();
                    if (Char.IsDigit(firstchar[0]))
                    {
                        throw Error("A QName may not start with a Number");
                    }

                    //Normal QName
                    this.LastTokenType = Token.QNAME;
                    return new QNameToken(qname, this.CurrentLine, this.StartPosition, this.EndPosition);
                }
            }
            else
            {
                //If we don't see a Colon then have to assume a Plain Literal
                //BUT we also need to check it's not a keyword
                String value = this.Value;

                if (value.Equals("a"))
                {
                    //The 'a' Keyword
                    this.LastTokenType = Token.KEYWORDA;
                    return new KeywordAToken(this.CurrentLine, this.StartPosition);
                }
                else if (value.Equals("is"))
                {
                    //This 'is' Keyword
                    throw Error("The 'is' Keyword is not Valid in Turtle");
                }
                else if (value.Equals("of"))
                {
                    //The 'of' Keyword
                    throw Error("The 'of' Keyword is not Valid in Turtle");
                }
                else
                {
                    //Must be a Plain Literal
                    if (!TurtleSpecsHelper.IsValidPlainLiteral(value))
                    {
                        throw Error("The value of the Plain Literal '" + value + "' is not valid in Turtle.  Turtle supports Boolean, Integer, Decimal and Double Plain Literals");
                    }
                    this.LastTokenType = Token.PLAINLITERAL;
                    return new PlainLiteralToken(value, this.CurrentLine, this.StartPosition, this.EndPosition);
                }
            }
        }

        /// <summary>
        /// Internal Helper method which attempts to get a Language Specifier Token
        /// </summary>
        /// <returns></returns>
        private IToken TryGetLanguageSpecToken()
        {
            char next = this.Peek();

            //Consume Letter Characters
            while (Char.IsLetter(next) || next == '-')
            {
                this.ConsumeCharacter();
                next = this.Peek();
            }

            if (this.Length == 0)
            {
                //Empty output so no Language Specifier
                throw UnexpectedCharacter(next, "Language Specifier for preceding Literal Token");
            }
            else
            {
                if (RdfSpecsHelper.IsValidLangSpecifier(this.Value))
                {
                    this.LastTokenType = Token.LANGSPEC;
                    return new LanguageSpecifierToken(this.Value, this.CurrentLine, this.StartPosition, this.EndPosition);
                }
                else
                {
                    throw Error("Unexpected Content '" + this.Value + "' encountered, expected a valid Language Specifier");
                }
            }
        }

        /// <summary>
        /// Internal Helper method which attempts to get a Date Type Token
        /// </summary>
        /// <returns></returns>
        private IToken TryGetDataTypeToken()
        {
            char next = this.Peek();

            if (next == '<')
            {
                //DataType is specified by a Uri
                this.ConsumeCharacter();
                next = this.Peek();

                //Get Characters while they're valid
                while (next != '>' && next != '\n' && next != '\r')
                {
                    //Append to output
                    this.ConsumeCharacter();
                    next = this.Peek();
                }

                //Check we didn't hit an illegal character
                if (next == '\n' || next == '\r')
                {
                    throw UnexpectedNewLine("DataType URI");
                }
                else
                {
                    //Get the final >
                    this.ConsumeCharacter();
                }

                return new DataTypeToken(this.Value, this.CurrentLine, this.StartPosition, this.EndPosition);
            }
            else
            {
                //DataType is specified by a QName
                IToken temp = this.TryGetQNameToken();
                if (temp is QNameToken)
                {
                    //Turn into a DataTypeToken
                    return new DataTypeToken(temp.Value, temp.StartLine, temp.StartPosition, temp.EndPosition);
                }
                else
                {
                    //If we got a PlainLiteralToken or anything else something went wrong
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
            char next = this.Peek();

            //Grab characters until we hit the new line
            while (next != '\n' && next != '\r')
            {
                if (this.ConsumeCharacter(true)) break;
                next = this.Peek();
            }

            //Discard New line and reset position
            CommentToken comment = new CommentToken(this.Value, this.CurrentLine, this.StartPosition, this.EndPosition);
            this.ConsumeNewLine(false, true);
            return comment;
        }
    }
}
