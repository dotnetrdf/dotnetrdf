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
using System.IO;
using VDS.RDF.Query;

namespace VDS.RDF.Parsing.Tokens
{
    /// <summary>
    /// A Class for Reading an Input Stream and generating SPARQL Tokens
    /// </summary>
    public class SparqlTokeniser : BaseTokeniser
    {
        private BlockingTextReader _in;
        private bool _queryKeywordSeen = false;
        private bool _baseDeclared = false;
        private SparqlQuerySyntax _syntax = Options.QueryDefaultSyntax;

        /// <summary>
        /// Creates a new Instance of the Tokeniser
        /// </summary>
        /// <param name="input">The Input Stream to generate Tokens from</param>
        /// <param name="syntax">Syntax Mode to use when parsing</param>
        public SparqlTokeniser(StreamReader input, SparqlQuerySyntax syntax)
            : this(new BlockingTextReader(input), syntax) { }

        /// <summary>
        /// Creates a new Instance of the Tokeniser
        /// </summary>
        /// <param name="input">The Input Stream to generate Tokens from</param>
        /// <param name="syntax">Syntax Mode to use when parsing</param>
        public SparqlTokeniser(BlockingTextReader input, SparqlQuerySyntax syntax)
            : base(input)
        {
            this._in = input;
            this.Format = "SPARQL";
            this._syntax = syntax;
        }

        /// <summary>
        /// Creates a new Instance of the Tokeniser
        /// </summary>
        /// <param name="input">The Input to generate Tokens from</param>
        /// <param name="syntax">Syntax Mode to use when parsing</param>
        public SparqlTokeniser(TextReader input, SparqlQuerySyntax syntax)
            : this(new BlockingTextReader(input), syntax) { }

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

                this.StartNewToken();

                try
                {
                    if (this.LastTokenType == Token.BOF && this._in.EndOfStream)
                    {
                        //Empty File
                        return new EOFToken(0, 0);
                    }
                    else if (!this._queryKeywordSeen)
                    {
                        //Expecting the Prologue/Keyword of the Query
                        return this.TryGetPrologueOrQueryKeyword();
                    }
                    else if (this.LastTokenType == Token.HATHAT)
                    {
                        //Should get a DataType
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

                        if (Char.IsWhiteSpace(next))
                        {
                            //Discard White Space when not in a Token
                            this.DiscardWhiteSpace();
                        }
                        else if (Char.IsLetter(next))
                        {
                            //Could be a Keyword or a QName
                            return this.TryGetQNameOrKeyword();
                        }
                        else if (Char.IsDigit(next))
                        {
                            //Could be a Numeric Literal or a QName
                            return this.TryGetQNameOrNumericLiteral();
                        }
                        else
                        {
                            switch (next)
                            {
                                #region Punctuation

                                case '*':
                                    //All/Multiply Token
                                    this.ConsumeCharacter();
                                    if (this.LastTokenType == Token.SELECT || this.LastTokenType == Token.DISTINCT || this.LastTokenType == Token.REDUCED)
                                    {
                                        this.LastTokenType = Token.ALL;
                                        return new AllToken(this.CurrentLine, this.StartPosition);
                                    }
                                    else
                                    {
                                        this.LastTokenType = Token.MULTIPLY;
                                        return new MultiplyToken(this.CurrentLine, this.StartPosition);
                                    }

                                case '/':
                                    //Divide Token
                                    this.ConsumeCharacter();
                                    this.LastTokenType = Token.DIVIDE;
                                    return new DivideToken(this.CurrentLine, this.StartPosition);

                                case '=':
                                    //Equals Token
                                    this.ConsumeCharacter();
                                    this.LastTokenType = Token.EQUALS;
                                    return new EqualityToken(this.CurrentLine, this.StartPosition);

                                case '#':
                                    //Comment Token
                                    return this.TryGetComment();

                                case '.':
                                    //Statement Terminator
                                    this.ConsumeCharacter();

                                    //Watch our for plain literals
                                    if (!this._in.EndOfStream && Char.IsDigit(this.Peek()))
                                    {
                                        IToken temp = this.TryGetNumericLiteral();
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

                                case ';':
                                    //Predicate Object List deliminator
                                    this.ConsumeCharacter();
                                    this.LastTokenType = Token.SEMICOLON;
                                    return new SemicolonToken(this.CurrentLine, this.StartPosition);

                                case ',':
                                    //Object List deleminator
                                    this.ConsumeCharacter();
                                    this.LastTokenType = Token.COMMA;
                                    return new CommaToken(this.CurrentLine, this.StartPosition);

                                case '<':
                                    //Start of a Uri or a Less Than
                                    return this.TryGetUri();

                                case '>':
                                    //Greater Than
                                    this.ConsumeCharacter();
                                    next = this.Peek();
                                    if (next == '=')
                                    {
                                        //Greater Than or Equal To
                                        this.ConsumeCharacter();
                                        this.LastTokenType = Token.GREATERTHANOREQUALTO;
                                        return new GreaterThanOrEqualToToken(this.CurrentLine, this.StartPosition);
                                    }
                                    else
                                    {
                                        //Greater Than
                                        this.LastTokenType = Token.GREATERTHAN;
                                        return new GreaterThanToken(this.CurrentLine, this.StartPosition);
                                    }

                                case '"':
                                    //Start of a Literal
                                    return this.TryGetLiteral('"');

                                case '\'':
                                    //Start of a Literal
                                    return this.TryGetLiteral('\'');

                                case '^':
                                    //DataType Specifier/Path Inverse
                                    this.ConsumeCharacter();
                                    next = this.Peek();
                                    if (next == '^')
                                    {
                                        //DataType specifier
                                        this.ConsumeCharacter();
                                        this.LastTokenType = Token.HATHAT;
                                        return new HatHatToken(this.CurrentLine, this.StartPosition);
                                    }
                                    else
                                    {
                                        //Path inverse
                                        this.LastTokenType = Token.HAT;
                                        return new HatToken(this.CurrentLine, this.StartPosition);
                                        //throw Error("Unexpected Character (Code " + (int)next + ") " + (char)next + " was encountered after a ^ character, expected a ^^ for a Data Type specifier");
                                    }

                                case '+':
                                case '-':
                                    //Start of a Numeric Literal
                                    return this.TryGetNumericLiteral();

                                case '!':
                                    //Logical Negation/Not Equals
                                    this.ConsumeCharacter();
                                    next = this.Peek();
                                    if (next == '=')
                                    {
                                        //Not Equals
                                        this.ConsumeCharacter();
                                        this.LastTokenType = Token.NOTEQUALS;
                                        return new NotEqualsToken(this.CurrentLine, this.StartPosition);
                                    }
                                    else
                                    {
                                        //Logical Negation
                                        this.LastTokenType = Token.NEGATION;
                                        return new NegationToken(this.CurrentLine, this.StartPosition);
                                    }

                                case '&':
                                    //Logical and
                                    this.ConsumeCharacter();
                                    next = this.Peek();
                                    if (next == '&')
                                    {
                                        this.ConsumeCharacter();
                                        this.LastTokenType = Token.AND;
                                        return new AndToken(this.CurrentLine, this.StartPosition);
                                    }
                                    else
                                    {
                                        throw Error("Unexpected Character (Code " + (int)next + ") " + (char)next + " was encountered while trying to parse a Logical And operator");
                                    }

                                case '|':
                                    //Logical or/Path Alternative
                                    this.ConsumeCharacter();
                                    next = this.Peek();
                                    if (next == '|')
                                    {
                                        //Logical Or
                                        this.ConsumeCharacter();
                                        this.LastTokenType = Token.OR;
                                        return new OrToken(this.CurrentLine, this.StartPosition);
                                    }
                                    else
                                    {
                                        //Path Alternative
                                        this.LastTokenType = Token.BITWISEOR;
                                        return new BitwiseOrToken(this.CurrentLine, this.StartPosition);
                                        //throw Error("Unexpected Character (Code " + (int)next + ") " + (char)next + " was encountered while trying to parse a Logical Or operator");
                                    }

                                case '@':
                                    //Lang Specifier
                                    return this.TryGetLangSpec();

                                #endregion

                                #region Variables and QNames

                                case '$':
                                case '?':
                                    //Start of a Variable
                                    return this.TryGetVariable();

                                case ':':
                                case '_':
                                    //A QName
                                    return this.TryGetQName();

                                #endregion

                                #region Brackets

                                case '{':
                                    //Left Bracket
                                    this.ConsumeCharacter();
                                    this.LastTokenType = Token.LEFTCURLYBRACKET;
                                    return new LeftCurlyBracketToken(this.CurrentLine, this.StartPosition);

                                case '}':
                                    //Right Bracket
                                    this.ConsumeCharacter();
                                    this.LastTokenType = Token.RIGHTCURLYBRACKET;
                                    return new RightCurlyBracketToken(this.CurrentLine, this.StartPosition);

                                case '(':
                                    //Left Bracket
                                    this.ConsumeCharacter();
                                    this.LastTokenType = Token.LEFTBRACKET;
                                    return new LeftBracketToken(this.CurrentLine, this.StartPosition);

                                case ')':
                                    //Right Bracket
                                    this.ConsumeCharacter();
                                    this.LastTokenType = Token.RIGHTBRACKET;
                                    return new RightBracketToken(this.CurrentLine, this.StartPosition);

                                case '[':
                                    //Left Bracket
                                    this.ConsumeCharacter();
                                    this.LastTokenType = Token.LEFTSQBRACKET;
                                    return new LeftSquareBracketToken(this.CurrentLine, this.StartPosition);

                                case ']':
                                    //Right Bracket
                                    this.ConsumeCharacter();
                                    this.LastTokenType = Token.RIGHTSQBRACKET;
                                    return new RightSquareBracketToken(this.CurrentLine, this.StartPosition);

                                #endregion

                                default:
                                    //Unexpected Character
                                    throw Error("Unexpected Character (Code " + (int)next + ") " + (char)next + " was encountered!");

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

        private IToken TryGetPrologueOrQueryKeyword()
        {
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

            //Discard leading White Space
            if (Char.IsWhiteSpace(next))
            {
                this.DiscardWhiteSpace();
            }

            //Get Characters that form the Token
            bool ok = true;
            do
            {
                next = this.Peek();

                if (Char.IsWhiteSpace(next)) break;

                switch (next)
                {
                    case '#':
                        return this.TryGetComment();
                    case '<':
                        if (this.Length == 0)
                        {
                            return this.TryGetUri();
                        }
                        ok = false;
                        break;
                    default:
                        this.ConsumeCharacter();
                        break;
                }

                if (this._in.EndOfStream)
                {
                    //throw Error("Unexpected end of query while trying to Parse Query Prologue");
                    break;
                }
            } while (ok);

            //May need to do backtracking if there was no whitespace between query keyword and {
            if (!this._queryKeywordSeen && this.Value.EndsWith("{"))
            {
                this.Backtrack();
            }

            //Work out what type of Token we've got
            String value = this.Value;
            switch (this.LastTokenType)
            {
                case Token.PREFIXDIRECTIVE:
                    //Expect to see a Prefix
                    return this.TryGetPrefix();

                case Token.URI:
                case Token.DOT:
                case Token.BOF:
                case Token.COMMENT:
                    //Expect a new BASE, PREFIX or Keyword after a Start of File/Previous Declaration
                    if (value.Equals(SparqlSpecsHelper.SparqlKeywordBase, StringComparison.OrdinalIgnoreCase))
                    {
                        if (this._baseDeclared)
                        {
                            //Only one Base Declaration permitted
                            throw Error("Unexpected Base Declaration encountered, the Query Prologue may only contain one Base Declaration");
                        }
                        else
                        {
                            //Got a Base Declaration
                            this._baseDeclared = true;
                            this.LastTokenType = Token.BASEDIRECTIVE;
                            return new BaseDirectiveToken(this.CurrentLine, this.StartPosition);
                        }
                    }
                    else if (value.Equals(SparqlSpecsHelper.SparqlKeywordPrefix, StringComparison.OrdinalIgnoreCase))
                    {
                        //Got a Prefix Declaration
                        this.LastTokenType = Token.PREFIXDIRECTIVE;
                        return new PrefixDirectiveToken(this.CurrentLine, this.StartPosition);
                    }
                    else if (SparqlSpecsHelper.IsQueryKeyword(value))
                    {
                        //Which keyword did we get
                        this._queryKeywordSeen = true;
                        if (value.Equals(SparqlSpecsHelper.SparqlKeywordAsk, StringComparison.OrdinalIgnoreCase))
                        {
                            //Ask Keyword
                            this.LastTokenType = Token.ASK;
                            return new AskKeywordToken(this.CurrentLine, this.StartPosition);
                        }
                        else if (value.Equals(SparqlSpecsHelper.SparqlKeywordConstruct, StringComparison.OrdinalIgnoreCase))
                        {
                            //Construct Keyword
                            this.LastTokenType = Token.CONSTRUCT;
                            return new ConstructKeywordToken(this.CurrentLine, this.StartPosition);
                        }
                        else if (value.Equals(SparqlSpecsHelper.SparqlKeywordDescribe, StringComparison.OrdinalIgnoreCase))
                        {
                            //Describe Keyword
                            this.LastTokenType = Token.DESCRIBE;
                            return new DescribeKeywordToken(this.CurrentLine, this.StartPosition);
                        }
                        else if (value.Equals(SparqlSpecsHelper.SparqlKeywordSelect, StringComparison.OrdinalIgnoreCase))
                        {
                            //Select Keyword
                            this.LastTokenType = Token.SELECT;
                            return new SelectKeywordToken(this.CurrentLine, this.StartPosition);
                        }
                        else
                        {
                            throw Error("Unexpected String '" + value + "' encountered when a SPARQL Query Keyword was expected");
                        }
                    }
                    else if (SparqlSpecsHelper.IsUpdateKeyword(value))
                    {
                        this._queryKeywordSeen = true;
                        value = value.ToUpper();
                        switch (value)
                        {
                            case SparqlSpecsHelper.SparqlKeywordAdd:
                                //Add Keyword
                                this.LastTokenType = Token.ADD;
                                return new AddKeywordToken(this.CurrentLine, this.StartPosition);
                            case SparqlSpecsHelper.SparqlKeywordClear:
                                //Clear Keyword
                                this.LastTokenType = Token.CLEAR;
                                return new ClearKeywordToken(this.CurrentLine, this.StartPosition);
                            case SparqlSpecsHelper.SparqlKeywordCopy:
                                //Copy Keyword
                                this.LastTokenType = Token.COPY;
                                return new CopyKeywordToken(this.CurrentLine, this.StartPosition);
                            case SparqlSpecsHelper.SparqlKeywordCreate:
                                //Create Keyword
                                this.LastTokenType = Token.CREATE;
                                return new CreateKeywordToken(this.CurrentLine, this.StartPosition);
                            case SparqlSpecsHelper.SparqlKeywordDelete:
                                //Delete Keyword
                                this.LastTokenType = Token.DELETE;
                                return new DeleteKeywordToken(this.CurrentLine, this.StartPosition);
                            case SparqlSpecsHelper.SparqlKeywordDrop:
                                //Drop Keyword
                                this.LastTokenType = Token.DROP;
                                return new DropKeywordToken(this.CurrentLine, this.StartPosition);
                            case SparqlSpecsHelper.SparqlKeywordInsert:
                                //Insert Keyword
                                this.LastTokenType = Token.INSERT;
                                return new InsertKeywordToken(this.CurrentLine, this.StartPosition);
                            case SparqlSpecsHelper.SparqlKeywordLoad:
                                //Load Keyword
                                this.LastTokenType = Token.LOAD;
                                return new LoadKeywordToken(this.CurrentLine, this.StartPosition);
                            case SparqlSpecsHelper.SparqlKeywordMove:
                                //Move Keyword
                                this.LastTokenType = Token.MOVE;
                                return new MoveKeywordToken(this.CurrentLine, this.StartPosition);
                            case SparqlSpecsHelper.SparqlKeywordWith:
                                //With Keyword
                                this.LastTokenType = Token.WITH;
                                return new WithKeywordToken(this.CurrentLine, this.StartPosition);
                            default:
                                throw Error("Unexpected Update Keyword '" + value + "' encountered while trying to parse the Query Prologue, expected an Update Keyword which can start an Update Command");
                        }
                    }
                    else
                    {
                        throw Error("Unexpected String '" + value + "' encountered while trying to parse the Query Prologue, expected a Base Declaration, Prefix Declaration or a Query/Update Keyword");
                    }

                default:
                    //Shouldn't get anything but the above as the Previous Token
                    throw Error("Unexpected String '" + value + "' encountered while trying to parse the Query Prologue, expected a Base Declaration, Prefix Declaration or a Query/Update Keyword");
            }
        }

        private IToken TryGetPrefix()
        {
            //Drop leading white space
            //this.DiscardWhiteSpace();

            //Get the Prefix Characters (unless we've already got them)
            if (this.Length == 0)
            {
                char next = this.Peek();
                while (!Char.IsWhiteSpace(next) && next != '<')
                {
                    this.ConsumeCharacter();
                    next = this.Peek();
                }
            }
            if (!this.Value.EndsWith(":"))
            {
                throw new RdfParseException("Didn't find expected : Character while attempting to parse Prefix at content:\n" + this.Value + "\nPrefixes must end in a Colon Character", this.StartLine, this.CurrentLine, this.StartPosition, this.CurrentPosition);
            }
            if (!SparqlSpecsHelper.IsValidQName(this.Value))
            {
                throw new RdfParseException("The value '" + this.Value + "' is not a valid Prefix in SPARQL", new PositionInfo(this.StartLine, this.CurrentLine, this.StartPosition, this.EndPosition));
            }

            //Produce a PrefixToken
            this.LastTokenType = Token.PREFIX;
            return new PrefixToken(this.Value, this.CurrentLine, this.StartPosition, this.CurrentPosition);
        }

        private IToken TryGetUri()
        {
            //Consume first thing which must be a <
            this.ConsumeCharacter();

            char next = this.Peek();
            if (next == '=')
            {
                //Might be a less than or equal to
                this.ConsumeCharacter();
                next = this.Peek();
                if (Char.IsWhiteSpace(next))
                {
                    //A Less Than or Equal To
                    this.LastTokenType = Token.LESSTHANOREQUALTO;
                    return new LessThanOrEqualToToken(this.CurrentLine, this.StartPosition);
                }
                else
                {
                    //Ambigious
                    this.ConsumeCharacter();
                    throw Error("Ambigious syntax in string '" + this.Value + "', the Tokeniser is unable to determine whether a Less Than or a URI was intended");
                }
            }
            else if (Char.IsWhiteSpace(next))
            {
                //Appears to be a Less Than
                this.LastTokenType = Token.LESSTHAN;
                return new LessThanToken(this.CurrentLine, this.StartPosition);
            }
            else
            {

                while (next != '>')
                {
                    if (Char.IsWhiteSpace(next))
                    {
                        //Ignore White Space inside URIs
                        this.SkipCharacter();
                    }
                    else if (next == '\\')
                    {
                        //Might be an escape for a >
                        this.HandleEscapes(TokeniserEscapeMode.Uri);
                    }
                    else
                    {
                        this.ConsumeCharacter();
                    }

                    next = this.Peek();
                }
                //Consume the concluding >
                this.ConsumeCharacter();

                this.LastTokenType = Token.URI;
                return new UriToken(this.Value, this.CurrentLine, this.StartPosition, this.EndPosition);
            }

        }

        private IToken TryGetQName()
        {
            char next = this.Peek();
            bool colonoccurred = false;

            while (UnicodeSpecsHelper.IsLetterOrDigit(next) || UnicodeSpecsHelper.IsLetterModifier(next) || next == '_' || next == '-' || next == ':' || next == '.'  || next == '\\')
            {
                if (next == '\\')
                {
                    //Handle Escapes
                    this.HandleEscapes(TokeniserEscapeMode.QName);
                    next = this.Peek();
                    continue;
                }
                else
                {
                    //Consume
                    this.ConsumeCharacter();
                }

                if (next == ':')
                {
                    if (colonoccurred)
                    {
                        //Can't contain more than 1 Colon
                        throw Error("Unexpected Character (Code " + (int)next + " :\nThe Colon Character can only occur once in a QName");
                    }
                    else
                    {
                        colonoccurred = true;

                        //Check it's not an assignment operator
                        if (this.Value.Length == 1)
                        {
                            next = this.Peek();
                            if (next == '=')
                            {
                                this.ConsumeCharacter();
                                this.LastTokenType = Token.ASSIGNMENT;
                                return new AssignmentToken(this.CurrentLine, this.StartPosition);
                            }
                        }
                    }
                }

                next = this.Peek();
            }

//#if !NO_NORM
//            String value = this.Value.Normalize();
//#else
            String value = this.Value;
//#endif
            //Backtrack if necessary
            if (value.EndsWith("."))
            {
                this.Backtrack();
                value = value.Substring(0, value.Length - 1);
            }

            if (value.Equals("a"))
            {
                //Keyword 'a'
                return new KeywordAToken(this.CurrentLine, this.StartPosition);
            }
            else if (value.Equals("true") || value.Equals("false"))
            {
                //Boolean Literal
                return new PlainLiteralToken(value, this.CurrentLine, this.StartPosition, this.EndPosition);
            }
            else if (value.StartsWith("_:"))
            {
                //A Blank Node QName
                this.LastTokenType = Token.BLANKNODEWITHID;
                return new BlankNodeWithIDToken(value, this.CurrentLine, this.StartPosition, this.EndPosition);
            }
            else if (!SparqlSpecsHelper.IsValidQName(value))
            {
                //Not a valid QName
                throw Error("The value '" + value + "' is not valid as a QName");
            }
            else
            {
                //Return the QName
                this.LastTokenType = Token.QNAME;
                return new QNameToken(value, this.CurrentLine, this.StartPosition, this.EndPosition);
            }
        }

        private IToken TryGetQNameOrKeyword()
        {
            char next = this.Peek();
            bool colonoccurred = false;
            String value;

            while (UnicodeSpecsHelper.IsLetterOrDigit(next) || UnicodeSpecsHelper.IsLetterModifier(next) || next == '_' || next == '-' || next == ':' || next == '.' || next == '\\')
            {
                if (next == '\\')
                {
                    //Handle Escapes
                    this.HandleEscapes(TokeniserEscapeMode.QName);
                    next = this.Peek();
                    continue;
                }
                else
                {
                    //Consume
                    this.ConsumeCharacter();
                }

                if (next == ':')
                {
                    if (colonoccurred)
                    {
                        //Can't contain more than 1 Colon
                        throw Error("Unexpected Character (Code " + (int)next + " :\nThe Colon Character can only occur once in a QName");
                    }
                    else
                    {
                        colonoccurred = true;
                    }
                }

                next = this.Peek();
            }

//#if !NO_NORM
//            value = this.Value.Normalize();
//#else
            value = this.Value;
//#endif

            //Backtrack if necessary
            if (value.EndsWith("."))
            {
                this.Backtrack();
                value = value.Substring(0, value.Length - 1);
            }

            if (!colonoccurred && (SparqlSpecsHelper.IsNonQueryKeyword(value) || SparqlSpecsHelper.IsFunctionKeyword(value) || SparqlSpecsHelper.IsAggregateKeyword(value) || SparqlSpecsHelper.IsUpdateKeyword(value)))
            {
                value = value.ToUpper();
                switch (value)
                {
                    case SparqlSpecsHelper.SparqlKeywordAbs:
                        //Abs Function Keyword
                        this.LastTokenType = Token.ABS;
                        return new AbsKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordAdd:
                        //Add Update Keyword
                        this.LastTokenType = Token.ADD;
                        return new AddKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordAll:
                        //All Keyword
                        this.LastTokenType = Token.ALLWORD;
                        return new AllKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordAs:
                        //As Alias Keyword
                        this.LastTokenType = Token.AS;
                        return new AsKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordAvg:
                        //Average Aggregate Keyword
                        this.LastTokenType = Token.AVG;
                        return new AvgKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordBind:
                        //Bind Keyword
                        this.LastTokenType = Token.BIND;
                        return new BindKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordBindings:
                        //Bindings Keyword
                        this.LastTokenType = Token.BINDINGS;
                        return new BindingsKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordBNode:
                        //BNode Keyword
                        this.LastTokenType = Token.BNODE;
                        return new BNodeKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordBound:
                        //Bound Function Keyword
                        this.LastTokenType = Token.BOUND;
                        return new BoundKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordCeil:
                        //Ceil Function Keyword
                        this.LastTokenType = Token.CEIL;
                        return new CeilKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordClear:
                        //Clear Keyword
                        this.LastTokenType = Token.CLEAR;
                        return new ClearKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordCopy:
                        //Copy Update Keyword
                        this.LastTokenType = Token.COPY;
                        return new CopyKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordCoalesce:
                        //COALESCE Function Keyword
                        this.LastTokenType = Token.COALESCE;
                        return new CoalesceKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordConcat:
                        //Concat Function Keyword
                        this.LastTokenType = Token.CONCAT;
                        return new ConcatKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordCount:
                        //Count Aggregate Keyword
                        this.LastTokenType = Token.COUNT;
                        return new CountKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordCreate:
                        //Create Keyword
                        this.LastTokenType = Token.CREATE;
                        return new CreateKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordData:
                        //Data Keyword
                        this.LastTokenType = Token.DATA;
                        return new DataKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordDataType:
                        //Datatype Function Keyword
                        this.LastTokenType = Token.DATATYPEFUNC;
                        return new DataTypeKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordDay:
                        //Day Function Keyword
                        this.LastTokenType = Token.DAY;
                        return new DayKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordDefault:
                        //Default Keyword
                        this.LastTokenType = Token.DEFAULT;
                        return new DefaultKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordDelete:
                        //Delete Keyword
                        this.LastTokenType = Token.DELETE;
                        return new DeleteKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordDistinct:
                        //Distinct Keyword
                        this.LastTokenType = Token.DISTINCT;
                        return new DistinctKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordDrop:
                        //Drop Keyword
                        this.LastTokenType = Token.DROP;
                        return new DropKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordEncodeForUri:
                        //EncodeForUri Function Keyword
                        this.LastTokenType = Token.ENCODEFORURI;
                        return new EncodeForUriKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordExists:
                        //Exists Keyword
                        this.LastTokenType = Token.EXISTS;
                        return new ExistsKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordFilter:
                        //Filter Keyword
                        this.LastTokenType = Token.FILTER;
                        return new FilterKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordFloor:
                        //Floor Function Keyword
                        this.LastTokenType = Token.FLOOR;
                        return new FloorKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordFrom:
                        //From Keyword
                        this.LastTokenType = Token.FROM;
                        return new FromKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordGraph:
                        //Graph Keyword
                        this.LastTokenType = Token.GRAPH;
                        return new GraphKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordGroup:
                        //GROUP Keyword, must be followed by a BY Keyword to form a GROUP BY keyword
                        if (this.GetExpectedKeyword(SparqlSpecsHelper.SparqlKeywordGroupBy))
                        {
                            this.LastTokenType = Token.GROUPBY;
                            return new GroupByKeywordToken(this.CurrentLine, this.StartPosition);
                        }
                        else
                        {
                            throw Error("Unexpected content while attempting to parse an GROUP BY keyword from Content:\n" + this.Value);
                        }
                    case SparqlSpecsHelper.SparqlKeywordGroupConcat:
                        //GROUP_CONCAT Keyword
                        this.LastTokenType = Token.GROUPCONCAT;
                        return new GroupConcatKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordHaving:
                        //HAVING Keyword
                        this.LastTokenType = Token.HAVING;
                        return new HavingKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordHours:
                        //Hours Function Keyword
                        this.LastTokenType = Token.HOURS;
                        return new HoursKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordIf:
                        //IF Keyword
                        this.LastTokenType = Token.IF;
                        return new IfKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordIn:
                        //IN Keyword
                        this.LastTokenType = Token.IN;
                        return new InKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordInsert:
                        //Insert Keyword
                        this.LastTokenType = Token.INSERT;
                        return new InsertKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordInto:
                        //Into Keyword
                        this.LastTokenType = Token.INTO;
                        return new IntoKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordIri:
                        //IRI Function Keyword
                        this.LastTokenType = Token.IRI;
                        return new IriKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordIsBlank:
                        //isBlank Function Keyword
                        this.LastTokenType = Token.ISBLANK;
                        return new IsBlankKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordIsIri:
                        //isIRI Function Keyword
                        this.LastTokenType = Token.ISIRI;
                        return new IsIriKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordIsLiteral:
                        //isLiteral Keyword
                        this.LastTokenType = Token.ISLITERAL;
                        return new IsLiteralKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordIsNumeric:
                        //IsNumeric Function Keyword
                        this.LastTokenType = Token.ISNUMERIC;
                        return new IsNumericKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordIsUri:
                        //isURI Keyword
                        this.LastTokenType = Token.ISURI;
                        return new IsUriKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordLang:
                        //Lang Keyword
                        this.LastTokenType = Token.LANG;
                        return new LangKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordLangMatches:
                        //Lang Matches Keyword
                        this.LastTokenType = Token.LANGMATCHES;
                        return new LangMatchesKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordLCase:
                        //LCase Function Keyword
                        this.LastTokenType = Token.LCASE;
                        return new LCaseKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordLength:
                        //Length Keyword
                        if (this._syntax != SparqlQuerySyntax.Extended) throw Error("The LENGTH keyword is only supported when syntax is set to Extended");
                        this.LastTokenType = Token.LENGTH;
                        return new LengthKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordLet:
                        //Let Keyword
                        this.LastTokenType = Token.LET;
                        return new LetKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordLimit:
                        //Limit Keyword
                        this.LastTokenType = Token.LIMIT;
                        return new LimitKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordLoad:
                        //Load Keyword
                        this.LastTokenType = Token.LOAD;
                        return new LoadKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordMax:
                        //Max Aggregate Keyword
                        this.LastTokenType = Token.MAX;
                        return new MaxKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordMD5:
                        //MD5 Function Keyword
                        this.LastTokenType = Token.MD5;
                        return new MD5KeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordMedian:
                        //Median Aggregate Keyword
                        this.LastTokenType = Token.MEDIAN;
                        return new MedianKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordMin:
                        //Min Aggregate Keyword
                        this.LastTokenType = Token.MIN;
                        return new MinKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordMinus:
                        //Minus Keyword
                        this.LastTokenType = Token.MINUS_P;
                        return new MinusKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordMinutes:
                        //Minutes Function Keyword
                        this.LastTokenType = Token.MINUTES;
                        return new MinutesKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordMode:
                        //Mode Aggregate Keyword
                        this.LastTokenType = Token.MODE;
                        return new ModeKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordMonth:
                        //Month Function Keyword
                        this.LastTokenType = Token.MONTH;
                        return new MonthKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordMove:
                        //Move Update Keyword
                        this.LastTokenType = Token.MOVE;
                        return new MoveKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordNamed:
                        //Named Keyword
                        this.LastTokenType = Token.NAMED;
                        return new NamedKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordNMax:
                        //Numeric Max Keyword
                        this.LastTokenType = Token.NMAX;
                        return new NumericMaxKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordNMin:
                        //Numeric Min Keyword
                        this.LastTokenType = Token.NMIN;
                        return new NumericMinKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordNot:
                        //Not Keyword
                        //Must be followed by a EXISTS or an IN Keyword
                        if (this.GetExpectedKeyword(SparqlSpecsHelper.SparqlKeywordNotExists))
                        {
                            //Not Exists Keyword
                            this.LastTokenType = Token.NOTEXISTS;
                            return new NotExistsKeywordToken(this.CurrentLine, this.StartPosition);
                        }
                        else if (this.Value.Equals(SparqlSpecsHelper.SparqlKeywordNotIn, StringComparison.OrdinalIgnoreCase))
                        {
                            //Not In Keyword
                            this.LastTokenType = Token.NOTIN;
                            return new NotInKeywordToken(this.CurrentLine, this.StartPosition);
                        }
                        else
                        {
                            throw Error("Unexpected content while attempting to parse an NOT EXISTS/NOT IN keyword from Content:\n" + this.Value);
                        }
                    case SparqlSpecsHelper.SparqlKeywordNow:
                        //Now Function Keyword
                        this.LastTokenType = Token.NOW;
                        return new NowKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordOffset:
                        //Offset Keyword
                        this.LastTokenType = Token.OFFSET;
                        return new OffsetKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordOptional:
                        //Optional Keyword
                        this.LastTokenType = Token.OPTIONAL;
                        return new OptionalKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordOrder:
                        //Order Keyword
                        //Must be followed by a BY Keyword
                        if (this.GetExpectedKeyword(SparqlSpecsHelper.SparqlKeywordOrderBy))
                        {
                            //Order By Keyword
                            this.LastTokenType = Token.ORDERBY;
                            return new OrderByKeywordToken(this.CurrentLine, this.StartPosition);
                        }
                        else
                        {
                            throw Error("Unexpected content while attempting to parse an ORDER BY keyword from Content:\n" + this.Value);
                        }
                    case SparqlSpecsHelper.SparqlKeywordRand:
                        //Rand Keyword
                        this.LastTokenType = Token.RAND;
                        return new RandKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordReduced:
                        //Reduced Keyword
                        this.LastTokenType = Token.REDUCED;
                        return new ReducedKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordRegex:
                        //Regex Keyword
                        this.LastTokenType = Token.REGEX;
                        return new RegexKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordReplace:
                        //Replace Keyword
                        this.LastTokenType = Token.REPLACE;
                        return new ReplaceKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordRound:
                        //Round Function Keyword
                        this.LastTokenType = Token.ROUND;
                        return new RoundKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordSameTerm:
                        //sameTerm Keyword
                        this.LastTokenType = Token.SAMETERM;
                        return new SameTermKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordSample:
                        //Sample Keyword
                        this.LastTokenType = Token.SAMPLE;
                        return new SampleKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordSeconds:
                        //Seconds Keywords
                        this.LastTokenType = Token.SECONDS;
                        return new SecondsKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordSeparator:
                        //Separator Keyword
                        this.LastTokenType = Token.SEPARATOR;
                        return new SeparatorKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordService:
                        //Service Keyword
                        this.LastTokenType = Token.SERVICE;
                        return new ServiceKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordSha1:
                        //Sha1 Function Keyword
                        this.LastTokenType = Token.SHA1;
                        return new Sha1KeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordSha224:
                        //Sha224 Function Keyword
                        this.LastTokenType = Token.SHA224;
                        return new Sha224KeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordSha256:
                        //Sha256 Function Keyword
                        this.LastTokenType = Token.SHA256;
                        return new Sha256KeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordSha384:
                        //Sha384 Function Keyword
                        this.LastTokenType = Token.SHA384;
                        return new Sha384KeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordSha512:
                        //Sha1 Function Keyword
                        this.LastTokenType = Token.SHA512;
                        return new Sha512KeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordSilent:
                        //Silent Keyword
                        this.LastTokenType = Token.SILENT;
                        return new SilentKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordStr:
                        //Str Keyword
                        this.LastTokenType = Token.STR;
                        return new StrKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordStrAfter:
                        //StrAfter Keyword
                        this.LastTokenType = Token.STRAFTER;
                        return new StrAfterKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordStrBefore:
                        //StrBefore Keyword
                        this.LastTokenType = Token.STRBEFORE;
                        return new StrBeforeKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordContains:
                        //StrContains Function Keyword
                        this.LastTokenType = Token.CONTAINS;
                        return new StrContainsKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordStrDt:
                        //StrDt Keyword
                        this.LastTokenType = Token.STRDT;
                        return new StrDtKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordStrLang:
                        //StrLang Keyword
                        this.LastTokenType = Token.STRLANG;
                        return new StrLangKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordStrLen:
                        //StrLen Function Keyword
                        this.LastTokenType = Token.STRLEN;
                        return new StrLenKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordStrEnds:
                        //StrEnds Function Keyword
                        this.LastTokenType = Token.STRENDS;
                        return new StrEndsKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordStrStarts:
                        //StrStarts Function Keyword
                        this.LastTokenType = Token.STRSTARTS;
                        return new StrStartsKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordSubStr:
                        //SubStr Function Keyword
                        this.LastTokenType = Token.SUBSTR;
                        return new SubStrKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordSum:
                        //Sum Keyword
                        this.LastTokenType = Token.SUM;
                        return new SumKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordTimezone:
                        //Timezone Function Keyword
                        this.LastTokenType = Token.TIMEZONE;
                        return new TimezoneKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordTo:
                        //To Keyword
                        this.LastTokenType = Token.TO;
                        return new ToKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordTz:
                        //TZ Function Keyword
                        this.LastTokenType = Token.TZ;
                        return new TZKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordUCase:
                        //UCase Function Keyword
                        this.LastTokenType = Token.UCASE;
                        return new UCaseKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordUndef:
                        //Undef Keyword
                        this.LastTokenType = Token.UNDEF;
                        return new UndefKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordUnion:
                        //Union Keyword
                        this.LastTokenType = Token.UNION;
                        return new UnionKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordUnsaid:
                        //Unsaid Keyword
                        this.LastTokenType = Token.UNSAID;
                        return new UnsaidKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordUri:
                        //Uri Keyword
                        this.LastTokenType = Token.URIFUNC;
                        return new UriKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordUsing:
                        //Using Keyword
                        this.LastTokenType = Token.USING;
                        return new UsingKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordWhere:
                        //Where Keyword
                        this.LastTokenType = Token.WHERE;
                        return new WhereKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordWith:
                        //With Keyword
                        this.LastTokenType = Token.WITH;
                        return new WithKeywordToken(this.CurrentLine, this.StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordYear:
                        //Year Function Keyword
                        this.LastTokenType = Token.YEAR;
                        return new YearKeywordToken(this.CurrentLine, this.StartPosition);
                    default:
                        throw Error("Unexpected String '" + value + "' encountered while trying to parse a SPARQL Keyword");
                }
            }
            else if (!colonoccurred && this._queryKeywordSeen && SparqlSpecsHelper.IsQueryKeyword(value))
            {
                if (this._syntax == SparqlQuerySyntax.Sparql_1_0) throw Error("Unexpected String '" + value + "' encountered while trying to parse a SPARQL Keyword.  This appears to be an attempt to use a sub-query but sub-queries are not supported in SPARQL 1.0");

                if (value.Equals(SparqlSpecsHelper.SparqlKeywordSelect, StringComparison.OrdinalIgnoreCase))
                {
                    //Select Keyword
                    this.LastTokenType = Token.SELECT;
                    return new SelectKeywordToken(this.CurrentLine, this.StartPosition);
                }
                else
                {
                    //Other Query Keyword
                    throw Error("Unexpected String '" + value + "' encountered while trying to parse a SPARQL Keyword.  This appears to be an attempt to use an ASK/CONSTRUCT/DESCRIBE as a sub-query which is not supported");
                }
            }
            //else if (!colonoccurred && (this.LastTokenType == Token.ORDERBY || (/*this._orderByKeywordSeen && */this.LastTokenType == Token.RIGHTBRACKET)))
            //{
                //Should be an ASC/DESC Keyword
                else if (value.Equals(SparqlSpecsHelper.SparqlKeywordAsc, StringComparison.OrdinalIgnoreCase))
                {
                    this.LastTokenType = Token.ASC;
                    return new AscKeywordToken(this.CurrentLine, this.StartPosition);
                }
                else if (value.Equals(SparqlSpecsHelper.SparqlKeywordDesc, StringComparison.OrdinalIgnoreCase))
                {
                    this.LastTokenType = Token.DESC;
                    return new DescKeywordToken(this.CurrentLine, this.StartPosition);
                //}
                //else
                //{
                //    throw Error("Unexpected String '" + value + "' encountered after an Order By which is not a valid QName/Keyword");
                //}
            }
            else if (value.Equals("a"))
            {
                //Keyword 'a'
                this.LastTokenType = Token.KEYWORDA;
                return new KeywordAToken(this.CurrentLine, this.StartPosition);
            }
            else if (value.Equals("true") || value.Equals("false"))
            {
                //Boolean Literal
                this.LastTokenType = Token.PLAINLITERAL;
                return new PlainLiteralToken(value, this.CurrentLine, this.StartPosition, this.EndPosition);
            }
            else if (!SparqlSpecsHelper.IsValidQName(value))
            {
                //Not a valid QName
                throw Error("The value '" + value + "' is not valid as a QName");
            }
            else
            {
                //Return the QName
                this.LastTokenType = Token.QNAME;
                return new QNameToken(value, this.CurrentLine, this.StartPosition, this.EndPosition);
            }
        }

        private IToken TryGetQNameOrNumericLiteral()
        {
            char next = this.Peek();
            bool colonoccurred = false;
            bool dotoccurred = false;

            while (UnicodeSpecsHelper.IsLetterOrDigit(next) || UnicodeSpecsHelper.IsLetterModifier(next) || next == '_' || next == '-' || next == ':' || next == '\\' || (next == '.' && !dotoccurred && !colonoccurred) || next == '+' || next == '-')
            {
                if (next == '\\')
                {
                    //Handle Escapes
                    this.HandleEscapes(TokeniserEscapeMode.QName);
                    next = this.Peek();
                    continue;
                }

                if (next == ':')
                {
                    if (colonoccurred)
                    {
                        //Can't contain more than 1 Colon
                        throw Error("Unexpected Character (Code " + (int)next + " :\nThe Colon Character can only occur once in a QName");
                    }
                    else
                    {
                        colonoccurred = true;
                    }
                }
                else if (next == '.')
                {
                    dotoccurred = true;
                }

                //Consume
                this.ConsumeCharacter();

                next = this.Peek();
            }

//#if !NO_NORM
//            String value = this.Value.Normalize();
//#else
            String value = this.Value;
//#endif

            //Backtrack if necessary
            if (value.EndsWith("."))
            {
                if (this._syntax == SparqlQuerySyntax.Sparql_1_0)
                {
                    //For SPARQL 1.0 only QNames can end with an ignorable .
                    if (!Char.IsDigit(value[0]) && value[0] != '+' && value[0] != '-')
                    {
                        //Backtrack only for QNames ending in a .
                        this.Backtrack();
                        value = value.Substring(0, value.Length - 1);
                    }
                }
                else
                {
                    //For SPARQL 1.1 backtrack regardless
                    this.Backtrack();
                    value = value.Substring(0, value.Length - 1);
                }
            }

            if (colonoccurred)
            {
                //Must be a QName
                if (!SparqlSpecsHelper.IsValidQName(value))
                {
                    //Not a valid QName
                    throw Error("The value '" + value + "' is not valid as a QName");
                }
                else
                {
                    this.LastTokenType = Token.QNAME;
                    return new QNameToken(value, this.CurrentLine, this.StartPosition, this.EndPosition);
                }
            }
            else
            {
                //Should be a Numeric Literal
                if (!SparqlSpecsHelper.IsValidNumericLiteral(value))
                {
                    //Not a Valid Numeric Literal
                    throw Error("The value '" + value + "' is not valid as a Numeric Literal");
                }
                else
                {
                    this.LastTokenType = Token.QNAME;
                    return new PlainLiteralToken(value, this.CurrentLine, this.StartPosition, this.EndPosition);
                }
            }
        }

        private bool GetExpectedKeyword(String expectedKeyword)
        {
            char next = this.Peek();

            //Consume white space
            while (Char.IsWhiteSpace(next))
            {
                this.ConsumeCharacter();
                next = this.Peek();
            }

            try 
            {
                //Grab as many characters as we can which are letters

                while (Char.IsLetter(next))
                {
                    this.ConsumeCharacter();
                    next = this.Peek();
                }

                //Check this string is the expected keyword
                String keyword = this.Value;
                return expectedKeyword.Equals(keyword, StringComparison.OrdinalIgnoreCase);
            } 
            catch 
            {
                //If we error then we definitely can't get the expected keyword
                return false;
            }
        }

        private IToken TryGetLiteral(char quotechar)
        {
            //Consume first character which must be a " or '
            this.ConsumeCharacter();
            char next = this.Peek();

            if (next == quotechar)
            {
                //Might be a Long Literal or an Empty String
                this.ConsumeCharacter();
                next = this.Peek();

                if (next == quotechar)
                {
                    #region Long Literals

                    //Long Literal
                    this.ConsumeCharacter();

                    next = this.Peek();
                    while (true)
                    {
                        if (next == '\\')
                        {
                            //Do Escape Processing
                            if (quotechar == '"')
                            {
                                this.HandleEscapes(TokeniserEscapeMode.QuotedLiterals);
                            }
                            else
                            {
                                this.HandleEscapes(TokeniserEscapeMode.QuotedLiteralsAlternate);
                            }
                        }
                        else if (next == quotechar)
                        {
                            //Check to see whether we get three in a row
                            this.ConsumeCharacter();
                            next = this.Peek();
                            if (next == quotechar)
                            {
                                //Got two in a row so far
                                this.ConsumeCharacter();
                                next = this.Peek();
                                if (next == quotechar)
                                {
                                    //Got three in a row
                                    this.ConsumeCharacter();
                                    this.LastTokenType = Token.LONGLITERAL;

                                    //If there are any additional quotes immediatedly following this then
                                    //we want to consume them also
                                    next = this.Peek();
                                    while (next == quotechar)
                                    {
                                        this.ConsumeCharacter();
                                        next = this.Peek();
                                    }

                                    return new LongLiteralToken(this.Value, this.StartLine, this.EndLine, this.StartPosition, this.EndPosition);
                                }
                            }
                        }
                        else if (next == '\n' || next == '\r')
                        {
                            this.ConsumeNewLine(true);
                        }
                        else if (this._in.EndOfStream)
                        {
                            //Hit End of Stream unexpectedly
                            throw Error("Unexpected End of File while trying to Parse a Long Literal from content:\n" + this.Value);
                        }
                        else
                        {
                            this.ConsumeCharacter();
                        }
                        next = this.Peek();
                    }

                    #endregion
                } 
                else 
                {
                    //Empty String
                    this.LastTokenType = Token.LITERAL;
                    return new LiteralToken(this.Value, this.CurrentLine, this.StartPosition, this.EndPosition);
                }
            }
            else
            {
                #region Literals

                //Simple quoted literal
                while (next != quotechar)
                {
                    if (next == '\\')
                    {
                        //Do Escape Processing
                        if (quotechar == '"')
                        {
                            this.HandleEscapes(TokeniserEscapeMode.QuotedLiterals);
                        }
                        else
                        {
                            this.HandleEscapes(TokeniserEscapeMode.QuotedLiteralsAlternate);
                        }
                    }
                    else if (next == '\n' || next == '\r')
                    {
                        //Illegal New Line
                        throw Error("Unexpected New Line while trying to Parse a Quoted Literal from content:\n" + this.Value + "\nTo use New Lines you must use the Triple Quote Long Literal syntax");
                    }
                    else if (this._in.EndOfStream)
                    {
                        //Hit End of Stream unexpectedly
                        throw Error("Unexpected End of File while trying to Parse a Quoted Literal from content:\n" + this.Value);
                    } else {
                        this.ConsumeCharacter();
                    }
                    next = this.Peek();
                }
                //Consume the last character which must be a " or '
                this.ConsumeCharacter();

                //Return the Literal
                this.LastTokenType = Token.LITERAL;
                return new LiteralToken(this.Value, this.CurrentLine, this.StartPosition, this.EndPosition);

                #endregion
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

            //If not SPARQL 1.0 then decimals can no longer end with a .
            if (this._syntax != SparqlQuerySyntax.Sparql_1_0)
            {
                //Decimals can't end with a . so backtrack
                if (this.Value.EndsWith(".")) this.Backtrack();
            }

            String value = this.Value;
            if (value.Equals("+"))
            {
                this.LastTokenType = Token.PLUS;
                return new PlusToken(this.CurrentLine, this.StartPosition);
            }
            else if (value.Equals("-"))
            {
                this.LastTokenType = Token.MINUS;
                return new MinusToken(this.CurrentLine, this.StartPosition);
            }
            else if (!SparqlSpecsHelper.IsValidNumericLiteral(value))
            {
                //Invalid Numeric Literal
                throw Error("The format of the Numeric Literal '" + this.Value + "' is not valid!");
            }

            //Return the Token
            this.LastTokenType = Token.PLAINLITERAL;
            return new PlainLiteralToken(this.Value, this.CurrentLine, this.StartPosition, this.EndPosition);
        }

        private IToken TryGetDataType()
        {
            //Expect to either see a start of a Uri or QName
            char next = this.Peek();

            if (next == '<')
            {
                //Uri specified DataType
                IToken temp = this.TryGetUri();
                this.LastTokenType = Token.DATATYPE;
                return new DataTypeToken("<" + temp.Value + ">", temp.StartLine, temp.StartPosition, temp.EndPosition);
            }
            else if (Char.IsLetter(next) || UnicodeSpecsHelper.IsLetter(next) || next == '_' || next == ':')
            {
                //QName specified Data Type
                IToken temp = this.TryGetQName();
                if (temp.TokenType == Token.QNAME)
                {
                    this.LastTokenType = Token.DATATYPE;
                    return new DataTypeToken(temp.Value, temp.StartLine, temp.StartPosition, temp.EndPosition);
                }
                else
                {
                    throw Error("Unexpected Token '" + temp.GetType().ToString() + "' was produced when a QName for a Data Type was expected!");
                }
            } 
            else
            {
                //Invalid Start Character
                throw Error("Unexpected Character (Code " + (int)next + " " + next + "\nExpected a < to start a URI or a valid start character for a QName to specify Data Type");
            }
        }

        private IToken TryGetLangSpec()
        {
            if (this.LastTokenType == Token.LITERAL || this.LastTokenType == Token.LONGLITERAL)
            {
                //Discard first character which will be the @
                this.SkipCharacter();
                this.StartNewToken();

                char next = this.Peek();
                bool dashoccurred = false;
                while (Char.IsLetter(next) || (next == '-' && !dashoccurred)) 
                {
                    this.ConsumeCharacter();

                    if (next == '-') dashoccurred = true;
                    next = this.Peek();
                }

                //Return the Language Specifier after validation
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
            else
            {
                throw Error("Unexpected Character (Code " + (int)'@' + ") @ encountered, the @ character can only be used as part of a Language Specifier after a Literal/Long Literal");
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
            else if (value.Equals("?"))
            {
                //Path Cardinality Modifier
                return new QuestionToken(this.CurrentLine, this.StartPosition);
            }
            else
            {
                throw Error("The value '" + value + "' is not valid a Variable Name");
            }

        }
    }
}
