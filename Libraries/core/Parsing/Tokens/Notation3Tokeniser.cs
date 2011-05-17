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
using System.Text.RegularExpressions;
using System.IO;

namespace VDS.RDF.Parsing.Tokens
{
    /// <summary>
    /// A Class for Reading an Input Stream and generating Notation 3 Tokens from it
    /// </summary>
    public class Notation3Tokeniser : BaseTokeniser
    {
        //OPT: Extract these constants into a Notation3SpecsHelper class

        /// <summary>
        /// Pattern for Valid QNames that use only the Latin Alphabet
        /// </summary>
        public const String ValidQNamesPattern = "^(([_A-Za-z])|([_A-Za-z][\\w\\-]*))?:?[_A-Za-z][\\w\\-]*$";
        /// <summary>
        /// Patter for Valid Variable Names
        /// </summary>
        public const String ValidVarNamesPattern = "^\\?[_A-Za-z][\\w\\-]*$";

        private BlockingTextReader _in;
        private List<String> _keywords = new List<string>();
        private bool _keywordsmode = false;
        private Regex _isValidQName = new Regex(ValidQNamesPattern);
        private Regex _isValidVarName = new Regex(ValidVarNamesPattern);

        /// <summary>
        /// Creates a new Instance of the Tokeniser
        /// </summary>
        /// <param name="input">The Input Stream to generate Tokens from</param>
        public Notation3Tokeniser(StreamReader input)
            : this(new BlockingTextReader(input)) { }

        /// <summary>
        /// Creates a new Instance of the Tokeniser
        /// </summary>
        /// <param name="input">The Input Stream to generate Tokens from</param>
        public Notation3Tokeniser(BlockingTextReader input)
            : base(input)
        {
            this._in = input;
            this.Format = "Notation 3";
        }

        /// <summary>
        /// Creates a new Instance of the Tokeniser
        /// </summary>
        /// <param name="input">The Input to generate Tokens from</param>
        public Notation3Tokeniser(TextReader input)
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
                this.StartNewToken();

                try
                {
                    if (this.LastTokenType == Token.BOF && this._in.EndOfStream)
                    {
                        //Empty File
                        return new EOFToken(0,0);
                    }
                    else if (this.LastTokenType == Token.KEYWORDDIRECTIVE || this.LastTokenType == Token.KEYWORDDEF)
                    {
                        //Get Keyword Definitions
                        return this.TryGetKeywordDefinition();
                    }
                    else if (this.LastTokenType == Token.PREFIXDIRECTIVE)
                    {
                        //Get Prefix
                        return this.TryGetPrefix();
                    }
                    else if (this.LastTokenType == Token.HATHAT)
                    {
                        //Get DataType
                        return this.TryGetDataType();
                    }

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

                        if (Char.IsWhiteSpace(next))
                        {
                            //Discard White Space when not in a Token
                            this.DiscardWhiteSpace();
                        }
                        else if (Char.IsDigit(next) || next == '-' || next == '+')
                        {
                            //Start of a Numeric Plain Literal
                            return this.TryGetNumericLiteral();
                        }
                        else if (Char.IsLetter(next))
                        {
                            //Start of a Plain Literal
                            return this.TryGetPlainLiteralOrQName();
                        }
                        else
                        {
                            switch (next)
                            {
                              
                                case '#':
                                    //Start of a Comment
                                    return this.TryGetCommentToken();

                                case '@':
                                    //Start of a Keyword or Language Specifier
                                    return this.TryGetKeywordOrLangSpec();

                                #region Line Terminators

                                case '.':
                                    //Dot Terminator
                                    this.ConsumeCharacter();
                                    if (!this._in.EndOfStream && Char.IsDigit(this.Peek()))
                                    {
                                        return this.TryGetNumericLiteral();
                                    }
                                    else
                                    {
                                        this.LastTokenType = Token.DOT;
                                        return new DotToken(this.CurrentLine, this.StartPosition);
                                    }
                                case ';':
                                    //Semicolon Terminator
                                    this.ConsumeCharacter();
                                    this.LastTokenType = Token.SEMICOLON;
                                    return new SemicolonToken(this.CurrentLine, this.StartPosition);
                                case ',':
                                    //Comma Terminator
                                    this.ConsumeCharacter();
                                    this.LastTokenType = Token.COMMA;
                                    return new CommaToken(this.CurrentLine, this.StartPosition);

                                #endregion

                                #region URIs and QNames

                                case '<':
                                    //Start of a Uri
                                    return this.TryGetUri();
                                case '_':
                                case ':':
                                    //Start of a  QName
                                    return this.TryGetQName();
                                case '?':
                                    //Start of a Universally Quantified Variable
                                    return this.TryGetVariable();

                                #endregion

                                #region Literals

                                case '"':
                                    //Start of a Literal
                                    return this.TryGetLiteral();
                                case '^':
                                    //Start of a DataType/Path
                                    return this.TryGetDataTypeOrPath();

                                #endregion

                                case '!':
                                    //Forward Path Traversal
                                    this.ConsumeCharacter();
                                    this.LastTokenType = Token.EXCLAMATION;
                                    return new ExclamationToken(this.CurrentLine, this.StartPosition);

                                case '=':
                                    //Equality or Implies
                                    return this.TryGetEqualityOrImplies();

                                #region Collections and Formula

                                case '[':
                                    //Blank Node Collection
                                    this.ConsumeCharacter();
                                    this.LastTokenType = Token.LEFTSQBRACKET;
                                    return new LeftSquareBracketToken(this.CurrentLine, this.StartPosition);
                                case ']':
                                    //Blank Node Collection
                                    this.ConsumeCharacter();
                                    this.LastTokenType = Token.RIGHTSQBRACKET;
                                    return new RightSquareBracketToken(this.CurrentLine, this.StartPosition);
                                case '{':
                                    //Formula
                                    //return this.TryGetFormula();
                                    this.ConsumeCharacter();
                                    this.LastTokenType = Token.LEFTCURLYBRACKET;
                                    return new LeftCurlyBracketToken(this.CurrentLine, this.StartPosition);
                                case '}':
                                    //Formula
                                    this.ConsumeCharacter();
                                    this.LastTokenType = Token.RIGHTCURLYBRACKET;
                                    return new RightCurlyBracketToken(this.CurrentLine, this.StartPosition);
                                case '(':
                                    //Collection
                                    this.ConsumeCharacter();
                                    this.LastTokenType = Token.LEFTBRACKET;
                                    return new LeftBracketToken(this.CurrentLine, this.StartPosition);
                                case ')':
                                    //Collection
                                    this.ConsumeCharacter();
                                    this.LastTokenType = Token.RIGHTBRACKET;
                                    return new RightBracketToken(this.CurrentLine, this.StartPosition);

                                #endregion

                                default:
                                    //Unexpected Character
                                    throw UnexpectedCharacter(next, String.Empty);
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

        private IToken TryGetNumericLiteral()
        {
            bool dotoccurred = false;
            bool expoccurred = false;
            bool signoccurred = false;

            if (this.Length == 1) dotoccurred = true;

            char next = this.Peek();

            while (Char.IsDigit(next) || next == '-' || next == '+' || next == 'e' || next == 'E' || (next == '.' && !dotoccurred))
            {
                //Consume the Character
                this.ConsumeCharacter();

                if (next == '-' || next == '+')
                {
                    if (signoccurred || expoccurred) 
                    {
                        char last = this.Value[this.Value.Length-2];
                        if (expoccurred)
                        {
                            if (last != 'e' && last != 'E')
                            {
                                //Can't occur here as isn't directly after the exponent
                                throw UnexpectedCharacter(next, "The +/- Sign can only occur at the start of a Numeric Literal or at the start of the Exponent");
                            }
                        }
                        else
                        {
                            //Negative sign already seen
                            throw UnexpectedCharacter(next, "The +/- Sign can only occur at the start of a Numeric Literal or at the start of the Exponent");
                        }
                    } 
                    else
                    {
                        signoccurred = true;

                        //Check this is at the start of the string
                        if (this.Length > 1)
                        {
                            throw UnexpectedCharacter(next, "The +/- Sign can only occur at the start of a Numeric Literal or at the start of the Exponent");
                        }
                    }
                }
                else if (next == 'e' || next == 'E')
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
                            throw UnexpectedCharacter(next, "The Exponent specifier cannot occur at the start of a Numeric Literal");
                        }
                    }
                }
                else if (next == '.')
                {
                    dotoccurred = true;
                }

                next = this.Peek();
            }

            //Validate the final result
            if (this.Value.EndsWith(".")) this.Backtrack();
            if (!TurtleSpecsHelper.IsValidPlainLiteral(this.Value)) 
            {
                throw Error("The format of the Numeric Literal '" + this.Value + "' is not valid!");
            }

            //Return the Token
            this.LastTokenType = Token.PLAINLITERAL;
            return new PlainLiteralToken(this.Value, this.CurrentLine, this.StartPosition, this.EndPosition);
            
        }

        private IToken TryGetPlainLiteralOrQName()
        {
            char next = this.Peek();

            if (!this._keywordsmode)
            {
                #region Non-Keywords Mode

                //Not in Keywords Mode
                bool colonoccurred = false;
                while (Char.IsLetterOrDigit(next) || next == ':' || next == '-' || next == '_')
                {
                    //Consume Character
                    this.ConsumeCharacter();

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

                //Validate
                String value = this.Value;

                //If it ends in a trailing . then we need to backtrack
                if (value.EndsWith("."))
                {
                    this.Backtrack();
                    value = value.Substring(0, value.Length - 1);
                }

                if (value.Equals("a"))
                {
                    //Keyword 'a'
                    this.LastTokenType = Token.KEYWORDA;
                    return new KeywordAToken(this.CurrentLine, this.StartPosition);
                }
                else if (value.Equals("is"))
                {
                    //Keyword 'is'
                    this.LastTokenType = Token.KEYWORDIS;
                    return new KeywordIsToken(this.CurrentLine, this.StartPosition);
                }
                else if (value.Equals("of"))
                {
                    //Keyword 'of'
                    this.LastTokenType = Token.KEYWORDOF;
                    return new KeywordOfToken(this.CurrentLine, this.StartPosition);
                }
                else if (TurtleSpecsHelper.IsValidPlainLiteral(value))
                {
                    //Other Valid Plain Literal
                    this.LastTokenType = Token.PLAINLITERAL;
                    return new PlainLiteralToken(value, this.CurrentLine, this.StartPosition, this.EndPosition);
                }
                else if (this.IsValidQName(value) && value.Contains(":"))
                {
                    //Valid QName
                    //Note that in the above condition we require a : since without Keywords mode
                    //all QNames must be in a Namespace
                    if (value.StartsWith("_:"))
                    {
                        //A Blank Node QName
                        this.LastTokenType = Token.BLANKNODEWITHID;
                        return new BlankNodeWithIDToken(value.Substring(2), this.CurrentLine, this.StartPosition, this.EndPosition);
                    }
                    else
                    {
                        //Normal QName
                        this.LastTokenType = Token.QNAME;
                        return new QNameToken(value, this.CurrentLine, this.StartPosition, this.EndPosition);
                    }
                }
                else
                {
                    //Not Valid
                    throw Error("The value '" + value + "' is not valid as a Plain Literal or QName");
                }

                #endregion
            }
            else
            {
                #region Keywords Mode

                //Since we're in Keywords Mode this is actually a QName
                //UNLESS it's in the Keywords list

                bool colonoccurred = false;

                while (Char.IsLetterOrDigit(next) || next == '_' || next == '-' || next == ':')
                {
                    //Consume
                    this.ConsumeCharacter();

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

                //Validate
                String value = this.Value;

                //If it ends in a trailing . then we need to backtrack
                if (value.EndsWith("."))
                {
                    this.Backtrack();
                    value = value.Substring(0, value.Length - 1);
                }

                if (this._keywords.Contains(value))
                {
                    //A Custom Keyword
                    this.LastTokenType = Token.KEYWORDCUSTOM;
                    return new CustomKeywordToken(value, this.CurrentLine, this.StartPosition, this.EndPosition);
                }
                else if (!this.IsValidQName(value))
                {
                    //Not a valid QName
                    throw Error("The value '" + value + "' is not valid as a QName");
                }
                else if (value.StartsWith("_:"))
                {
                    //A Blank Node QName
                    this.LastTokenType = Token.BLANKNODEWITHID;
                    return new BlankNodeWithIDToken(value.Substring(2), this.CurrentLine, this.StartPosition, this.EndPosition);
                }
                else 
                {
                    //Return the QName
                    this.LastTokenType = Token.QNAME;

                    //If no Colon need to append it to the front to make a QName in the Default namespace
                    if (!colonoccurred) {
                        value = ":" + value;
                    }

                    return new QNameToken(value, this.CurrentLine, this.StartPosition, this.EndPosition);
                }

                #endregion
            }
        }

        private IToken TryGetKeywordOrLangSpec()
        {
            if (this.LastTokenType == Token.LITERAL || this.LastTokenType == Token.LONGLITERAL)
            {
                //Must be a Language Specifier

                //Discard the @
                this.SkipCharacter();

                //Get the Specifier
                char next = this.Peek();
                bool negoccurred = false;
                while (Char.IsLetterOrDigit(next) || next == '-')
                {
                    this.ConsumeCharacter();
                    if (next == '-')
                    {
                        if (negoccurred)
                        {
                            throw Error("Unexpected Character (Code " + (int)next + " -\nThe hyphen character can only be used once in a Language Specifier");
                        }
                        else
                        {
                            negoccurred = true;
                        }
                    }

                    next = this.Peek();
                }

                //Return the Language Specifier
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
            else if (this.LastTokenType == Token.PLAINLITERAL)
            {
                //Can't specify Language on a Plain Literal
                throw Error("Unexpected Character (Code " + (int)'@' + " @\nThe @ sign cannot be used to specify a Language on a Plain Literal");
            }
            else
            {
                //Must be some Keyword

                //Discard the @
                this.SkipCharacter();

                //Consume until we hit White Space
                char next = this.Peek();
                while (!Char.IsWhiteSpace(next) && next != '.')
                {
                    this.ConsumeCharacter();
                    next = this.Peek();
                }

                //Now check we get something that's an actual Keyword/Directive
                String value = this.Value;
                if (value.Equals("keywords", StringComparison.OrdinalIgnoreCase))
                {
                    //Keywords Directive

                    //Remember to enable Keywords Mode
                    this._keywordsmode = true;

                    this.LastTokenType = Token.KEYWORDDIRECTIVE;
                    return new KeywordDirectiveToken(this.CurrentLine, this.StartPosition);
                }
                else if (value.Equals("base", StringComparison.OrdinalIgnoreCase))
                {
                    //Base Directive
                    this.LastTokenType = Token.BASEDIRECTIVE;
                    return new BaseDirectiveToken(this.CurrentLine, this.StartPosition);
                }
                else if (value.Equals("prefix", StringComparison.OrdinalIgnoreCase))
                {
                    //Prefix Directive
                    this.LastTokenType = Token.PREFIXDIRECTIVE;
                    return new PrefixDirectiveToken(this.CurrentLine, this.StartPosition);
                }
                else if (value.Equals("forall", StringComparison.OrdinalIgnoreCase))
                {
                    //ForAll Quantifier
                    this.LastTokenType = Token.FORALL;
                    return new ForAllQuantifierToken(this.CurrentLine, this.StartPosition);
                }
                else if (value.Equals("forsome", StringComparison.OrdinalIgnoreCase))
                {
                    //ForSome Quantifier
                    this.LastTokenType = Token.FORSOME;
                    return new ForSomeQuantifierToken(this.CurrentLine, this.StartPosition);
                }
                else if (value.Equals("a"))
                {
                    //'a' Keyword
                    this.LastTokenType = Token.KEYWORDA;
                    return new KeywordAToken(this.CurrentLine, this.StartPosition);
                }
                else if (value.Equals("is"))
                {
                    //'is' Keyword
                    this.LastTokenType = Token.KEYWORDIS;
                    return new KeywordIsToken(this.CurrentLine, this.StartPosition);
                }
                else if (value.Equals("of"))
                {
                    //'of' Keyword
                    this.LastTokenType = Token.KEYWORDOF;
                    return new KeywordOfToken(this.CurrentLine, this.StartPosition);
                }
                else if (value.Equals("false") || value.Equals("true"))
                {
                    //Plain Literal Boolean
                    this.LastTokenType = Token.PLAINLITERAL;
                    return new PlainLiteralToken(value, this.CurrentLine, this.StartPosition, this.EndPosition);
                }
                else if (this._keywords.Contains(value))
                {
                    //A Custom Keyword which has been defined
                    this.LastTokenType = Token.KEYWORDCUSTOM;
                    return new CustomKeywordToken(value, this.CurrentLine, this.StartPosition, this.EndPosition);
                }
                else
                {
                    //Some other unknown and undefined Keyword
                    throw Error("The Keyword '" + value + "' has not been defined and is not a valid Notation 3 Keyword");
                }
            }
        }

        private IToken TryGetKeywordDefinition()
        {
            char next = this.Peek();

            while (Char.IsWhiteSpace(next))
            {
                //Discard white space we don't want
                if (next == '\n' || next == '\r')
                {
                    //Newlines are forbidden
                    throw Error("Unexpected New Line encountered in a Keywords Directive.  A Keywords Directive must be terminated by a . Line Terminator character");
                }
                else
                {
                    //Discard
                    this.SkipCharacter();
                }

                next = this.Peek();
            }
            this.StartNewToken();

            if (next == '.')
            {
                //Directive is Terminated
                this.ConsumeCharacter();
                this.LastTokenType = Token.DOT;
                return new DotToken(this.CurrentLine, this.StartPosition);
            }
            else
            {
                while (!Char.IsWhiteSpace(next) && next != '.')
                {
                    //Consume
                    this.ConsumeCharacter();
                    next = this.Peek();
                }

                //Add to Keywords List
                if (!this._keywords.Contains(this.Value))
                {
                    this._keywords.Add(this.Value);
                }

                //Return the Keyword Definition
                this.LastTokenType = Token.KEYWORDDEF;
                return new CustomKeywordDefinitionToken(this.Value, this.CurrentLine, this.StartPosition, this.EndPosition);
            }
        }

        private IToken TryGetPrefix()
        {
            char next = this.Peek();

            //Drop leading white space
            while (Char.IsWhiteSpace(next))
            {
                //If we hit a New Line then Error
                if (next == '\n' || next == '\r')
                {
                    throw Error("Unexpected New Line character encountered while attempting to parse Prefix at content:\n" + this.Value);
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
                //Might be a reverse implies
                this.ConsumeCharacter();
                next = this.Peek();
                if (Char.IsWhiteSpace(next))
                {
                    //A Reverse Implies
                    this.LastTokenType = Token.IMPLIEDBY;
                    return new ImpliedByToken(this.CurrentLine, this.StartPosition);
                }
                else
                {
                    //Ambigious
                    this.ConsumeCharacter();
                    throw Error("Ambigious syntax in string '" + this.Value + "', the Tokeniser is unable to determine whether an Implied By or a URI was intended");
                }
            }
            else
            {

                while (next != '>')
                {
                    if (Char.IsWhiteSpace(next))
                    {
                        //Discard White Space inside URIs
                        this.DiscardWhiteSpace();
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

            while (Char.IsLetterOrDigit(next) || next == '_' || next == '-' || next == ':')
            {
                //Consume
                this.ConsumeCharacter();

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

            String value = this.Value;

            //If it ends in a trailing . then we need to backtrack
            if (value.EndsWith("."))
            {
                this.Backtrack();
                value = value.Substring(0, value.Length - 1);
            }

            if (!this.IsValidQName(value))
            {
                //Not a valid QName
                throw Error("The value '" + value + "' is not valid as a QName");
            }
            else if (!colonoccurred && !this._keywordsmode)
            {
                //Not a valid QName
                //No : and not in Keywords mode so the : is required
                throw Error("The value '" + value + "' is not valid as a QName since it doesn't contain a Colon Character and the Namespace is thus not determinable");
            }
            else if (value.StartsWith("_:"))
            {
                //A Blank Node QName
                this.LastTokenType = Token.BLANKNODEWITHID;
                return new BlankNodeWithIDToken(value, this.CurrentLine, this.StartPosition, this.EndPosition);
            }
            else if (this._keywordsmode && this._keywords.Contains(value))
            {
                //A Custom Keyword
                this.LastTokenType = Token.KEYWORDCUSTOM;
                return new CustomKeywordToken(value, this.CurrentLine, this.StartPosition, this.EndPosition);
            }
            else
            {
                //Return the QName
                this.LastTokenType = Token.QNAME;

                //If no Colon need to append it to the front to make a QName in the Default namespace
                //Only apply this if we're in Keywords Mode
                if (!colonoccurred && this._keywordsmode)
                {
                    value = ":" + value;
                }

                return new QNameToken(value, this.CurrentLine, this.StartPosition, this.EndPosition);
            }
        }

        private IToken TryGetLiteral()
        {
            //Consume first character which must be a "
            this.ConsumeCharacter();
            char next = this.Peek();

            if (next == '"')
            {
                //Might be a Long Literal or an Empty String
                this.ConsumeCharacter();
                next = this.Peek();

                if (next == '"')
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
                            this.HandleEscapes(TokeniserEscapeMode.QuotedLiterals);
                        }
                        else if (next == '"')
                        {
                            //Check to see whether we get three in a row
                            this.ConsumeCharacter();
                            next = this.Peek();
                            if (next == '"')
                            {
                                //Got two in a row so far
                                this.ConsumeCharacter();
                                next = this.Peek();
                                if (next == '"')
                                {
                                    //Got three in a row
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
                            }
                        }
                        else if (next == '\n' || next == '\r')
                        {
                            //Consume the New Line
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
                while (next != '"')
                {
                    if (next == '\\')
                    {
                        //Do Escape Processing
                        this.HandleEscapes(TokeniserEscapeMode.QuotedLiterals);
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
                    } 
                    else 
                    {
                        this.ConsumeCharacter();
                    }
                    next = this.Peek();
                }
                //Consume the last character which must be a "
                this.ConsumeCharacter();

                //Return the Literal
                this.LastTokenType = Token.LITERAL;
                return new LiteralToken(this.Value, this.CurrentLine, this.StartPosition, this.EndPosition);

                #endregion
            }
        }

        private IToken TryGetDataTypeOrPath()
        {
            //Consume first character which must be a ^
            this.ConsumeCharacter();

            //Take a look at the next Character to determine if this is a DataType specifier or a Path specifier
            char next = this.Peek();
            if (next == '^')
            {
                this.ConsumeCharacter();
                //Must occur after a Literal
                if (this.LastTokenType == Token.LITERAL || this.LastTokenType == Token.LONGLITERAL)
                {
                    //Return the specifier
                    this.LastTokenType = Token.HATHAT;
                    return new HatHatToken(this.CurrentLine, this.StartPosition);
                }
                else if (this.LastTokenType == Token.PLAINLITERAL)
                {
                    //Can't specify Type on a Plain Literal
                    throw Error("Unexpected ^^ sequence for specifying a DataType was encountered, the DataType cannot be specified for Plain Literals");
                }
                else
                {
                    //Can't use this except after a Literal
                    throw Error("Unexpected ^^ sequence for specifying a DataType was encountered, the DataType specifier can only be used after a Quoted Literal");
                }
            }
            else
            {
                //Path Specifier
                this.LastTokenType = Token.HAT;
                return new HatToken(this.CurrentLine, this.StartPosition);
            }
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
            else if (Char.IsLetter(next) || UnicodeSpecsHelper.IsLetter(next) || next == '_')
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

        private IToken TryGetEqualityOrImplies()
        {
            //Consume first thing
            this.ConsumeCharacter();

            //See what the next character is
            char next = this.Peek();

            if (next != '>')
            {
                //Just an Equality Sign
                this.LastTokenType = Token.EQUALS;
                return new EqualityToken(this.CurrentLine, this.StartPosition);
            }
            else
            {
                //An implies
                this.ConsumeCharacter();
                next = this.Peek();

                this.LastTokenType = Token.IMPLIES;
                return new ImpliesToken(this.CurrentLine, this.StartPosition);
            }
        }

        private IToken TryGetFormula()
        {
            int openBrackets = 0;
            int closeBrackets = 0;
            char next;

            //Continue consuming characters accounting for nesting as appropriate
            do
            {
                if (this._in.EndOfStream && openBrackets > 0)
                {
                    throw Error("Unexpected End of File while trying to Parse a Formula from Content:\n" + this.Value);
                }

                next = this.Peek();

                switch (next)
                {
                    case '{':
                        openBrackets++;
                        break;
                    case '}':
                        //openBrackets--;
                        closeBrackets++;
                        break;
                    case '\n':
                    case '\r':
                        //Discard the White Space
                        this.ConsumeNewLine(false);
                        continue;
                }

                //Consume
                this.ConsumeCharacter();
            } while (openBrackets > closeBrackets);

            //Return the GraphLiteral
            this.LastTokenType = Token.GRAPHLITERAL;
            return new GraphLiteralToken(this.Value, this.StartLine, this.EndLine, this.StartPosition, this.EndPosition);
        }

        private IToken TryGetVariable()
        {
            //Consume first Character which must be a ?
            this.ConsumeCharacter();

            //Consume other valid Characters
            char next = this.Peek();
            while (Char.IsLetterOrDigit(next) || UnicodeSpecsHelper.IsLetterOrDigit(next) || next == '-' || next == '_')
            {
                this.ConsumeCharacter();
                next = this.Peek();
            }

            //Validate
            String value = this.Value;
            if (this.IsValidVarName(value))
            {
                return new VariableToken(value, this.CurrentLine, this.StartPosition, this.EndPosition);
            }
            else
            {
                throw Error("The value '" + value + "' is not valid a Variable Name");
            }

        }

        /// <summary>
        /// Determines whether a given Token represents an RDF Term or part thereof
        /// </summary>
        /// <param name="tokentype">Token Type to test</param>
        /// <returns></returns>
        private bool IsRDFTermToken(int tokentype)
        {
            switch (tokentype)
            {
                case Token.BLANKNODE:
                case Token.BLANKNODEWITHID:
                case Token.DATATYPE:
                case Token.GRAPHLITERAL:
                case Token.IMPLIEDBY:
                case Token.IMPLIES:
                case Token.KEYWORDA:
                case Token.KEYWORDIS:
                case Token.KEYWORDOF:
                case Token.LANGSPEC:
                case Token.LITERAL:
                case Token.LONGLITERAL:
                case Token.PLAINLITERAL:
                case Token.QNAME:
                case Token.URI:
                    return true;
                default:
                    return false;
            }
        }

        private bool IsValidQName(String value)
        {
            if (this._isValidQName.IsMatch(value))
            {
                return true;
            }
            else
            {
                //Have to validate Character by Character
                char[] cs = value.ToCharArray();
                char first = cs[0];

                //First character must be an underscore, colon or letter
                if (first == '_' || first == ':' || Char.IsLetter(first) || UnicodeSpecsHelper.IsLetter(first))
                {
                    //Remaining Characters must be underscores, colons, letters, numbers or hyphens
                    for (int i = 1; i < cs.Length; i++)
                    {
                        char c = cs[i];
                        if (c == '_' || c == ':' || c == '-' || Char.IsLetterOrDigit(c) || UnicodeSpecsHelper.IsLetterOrDigit(c))
                        {
                            //OK
                        }
                        else
                        {
                            //Invalid Character
                            return false;
                        }
                    }

                    //If we get here it's all fine
                    return true;
                }
                else
                {
                    //Invalid Start Character
                    return false;
                }
            }
        }

        private bool IsValidVarName(String value)
        {
            if (this._isValidVarName.IsMatch(value))
            {
                return true;
            }
            else
            {
                //Have to validate Character by Character
                char[] cs = value.ToCharArray();
                char first = cs[0];

                //First character must be an underscore or letter
                if (first == '_' || Char.IsLetter(first) || UnicodeSpecsHelper.IsLetter(first))
                {
                    //Remaining Characters must be underscores, letters, numbers or hyphens
                    for (int i = 1; i < cs.Length; i++)
                    {
                        char c = cs[i];
                        if (c == '_' || c == '-' || Char.IsLetterOrDigit(c) || UnicodeSpecsHelper.IsLetterOrDigit(c))
                        {
                            //OK
                        }
                        else
                        {
                            //Invalid Character
                            return false;
                        }
                    }

                    //If we get here it's all fine
                    return true;
                }
                else
                {
                    //Invalid Start Character
                    return false;
                }
            }
        }

    }
}
