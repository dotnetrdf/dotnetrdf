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
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace VDS.RDF.Parsing.Tokens
{
    /// <summary>
    /// A Class for Reading an Input Stream and generating Notation 3 Tokens from it
    /// </summary>
    public class Notation3Tokeniser 
        : BaseTokeniser
    {
        // OPT: Extract these constants into a Notation3SpecsHelper class

        /// <summary>
        /// Pattern for Valid QNames that use only the Latin Alphabet
        /// </summary>
        public const String ValidQNamesPattern = "^(([_A-Za-z])|([_A-Za-z][\\w\\-]*))?:?[_A-Za-z][\\w\\-]*$";
        /// <summary>
        /// Patter for Valid Variable Names
        /// </summary>
        public const String ValidVarNamesPattern = "^\\?[_A-Za-z][\\w\\-]*$";

        private ParsingTextReader _in;
        private List<String> _keywords = new List<string>();
        private bool _keywordsmode = false;
        private Regex _isValidQName = new Regex(ValidQNamesPattern);
        private Regex _isValidVarName = new Regex(ValidVarNamesPattern);

        /// <summary>
        /// Creates a new Instance of the Tokeniser
        /// </summary>
        /// <param name="input">The Input Stream to generate Tokens from</param>
        public Notation3Tokeniser(StreamReader input)
            : this(ParsingTextReader.Create(input)) { }

        /// <summary>
        /// Creates a new Instance of the Tokeniser
        /// </summary>
        /// <param name="input">The Input Stream to generate Tokens from</param>
        public Notation3Tokeniser(ParsingTextReader input)
            : base(input)
        {
            _in = input;
            Format = "Notation 3";
        }

        /// <summary>
        /// Creates a new Instance of the Tokeniser
        /// </summary>
        /// <param name="input">The Input to generate Tokens from</param>
        public Notation3Tokeniser(TextReader input)
            : this(ParsingTextReader.Create(input)) { }

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
                StartNewToken();

                try
                {
                    if (LastTokenType == Token.BOF && _in.EndOfStream)
                    {
                        // Empty File
                        return new EOFToken(0,0);
                    }
                    else if (LastTokenType == Token.KEYWORDDIRECTIVE || LastTokenType == Token.KEYWORDDEF)
                    {
                        // Get Keyword Definitions
                        return TryGetKeywordDefinition();
                    }
                    else if (LastTokenType == Token.PREFIXDIRECTIVE)
                    {
                        // Get Prefix
                        return TryGetPrefix();
                    }
                    else if (LastTokenType == Token.HATHAT)
                    {
                        // Get DataType
                        return TryGetDataType();
                    }

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

                        if (Char.IsWhiteSpace(next))
                        {
                            // Discard White Space when not in a Token
                            DiscardWhiteSpace();
                        }
                        else if (Char.IsDigit(next) || next == '-' || next == '+')
                        {
                            // Start of a Numeric Plain Literal
                            return TryGetNumericLiteral();
                        }
                        else if (Char.IsLetter(next))
                        {
                            // Start of a Plain Literal
                            return TryGetPlainLiteralOrQName();
                        }
                        else
                        {
                            switch (next)
                            {
                              
                                case '#':
                                    // Start of a Comment
                                    return TryGetCommentToken();

                                case '@':
                                    // Start of a Keyword or Language Specifier
                                    return TryGetKeywordOrLangSpec();

                                #region Line Terminators

                                case '.':
                                    // Dot Terminator
                                    ConsumeCharacter();
                                    if (!_in.EndOfStream && Char.IsDigit(Peek()))
                                    {
                                        return TryGetNumericLiteral();
                                    }
                                    else
                                    {
                                        LastTokenType = Token.DOT;
                                        return new DotToken(CurrentLine, StartPosition);
                                    }
                                case ';':
                                    // Semicolon Terminator
                                    ConsumeCharacter();
                                    LastTokenType = Token.SEMICOLON;
                                    return new SemicolonToken(CurrentLine, StartPosition);
                                case ',':
                                    // Comma Terminator
                                    ConsumeCharacter();
                                    LastTokenType = Token.COMMA;
                                    return new CommaToken(CurrentLine, StartPosition);

                                #endregion

                                #region URIs and QNames

                                case '<':
                                    // Start of a Uri
                                    return TryGetUri();
                                case '_':
                                case ':':
                                    // Start of a  QName
                                    return TryGetQName();
                                case '?':
                                    // Start of a Universally Quantified Variable
                                    return TryGetVariable();

                                #endregion

                                #region Literals

                                case '"':
                                    // Start of a Literal
                                    return TryGetLiteral();
                                case '^':
                                    // Start of a DataType/Path
                                    return TryGetDataTypeOrPath();

                                #endregion

                                case '!':
                                    // Forward Path Traversal
                                    ConsumeCharacter();
                                    LastTokenType = Token.EXCLAMATION;
                                    return new ExclamationToken(CurrentLine, StartPosition);

                                case '=':
                                    // Equality or Implies
                                    return TryGetEqualityOrImplies();

                                #region Collections and Formula

                                case '[':
                                    // Blank Node Collection
                                    ConsumeCharacter();
                                    LastTokenType = Token.LEFTSQBRACKET;
                                    return new LeftSquareBracketToken(CurrentLine, StartPosition);
                                case ']':
                                    // Blank Node Collection
                                    ConsumeCharacter();
                                    LastTokenType = Token.RIGHTSQBRACKET;
                                    return new RightSquareBracketToken(CurrentLine, StartPosition);
                                case '{':
                                    // Formula
                                    // return this.TryGetFormula();
                                    ConsumeCharacter();
                                    LastTokenType = Token.LEFTCURLYBRACKET;
                                    return new LeftCurlyBracketToken(CurrentLine, StartPosition);
                                case '}':
                                    // Formula
                                    ConsumeCharacter();
                                    LastTokenType = Token.RIGHTCURLYBRACKET;
                                    return new RightCurlyBracketToken(CurrentLine, StartPosition);
                                case '(':
                                    // Collection
                                    ConsumeCharacter();
                                    LastTokenType = Token.LEFTBRACKET;
                                    return new LeftBracketToken(CurrentLine, StartPosition);
                                case ')':
                                    // Collection
                                    ConsumeCharacter();
                                    LastTokenType = Token.RIGHTBRACKET;
                                    return new RightBracketToken(CurrentLine, StartPosition);

                                #endregion

                                default:
                                    // Unexpected Character
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
                    else
                    {
                        // Some other Error so throw
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

        private IToken TryGetNumericLiteral()
        {
            bool dotoccurred = false;
            bool expoccurred = false;
            bool signoccurred = false;

            if (Length == 1) dotoccurred = true;

            char next = Peek();

            while (Char.IsDigit(next) || next == '-' || next == '+' || next == 'e' || next == 'E' || (next == '.' && !dotoccurred))
            {
                // Consume the Character
                ConsumeCharacter();

                if (next == '-' || next == '+')
                {
                    if (signoccurred || expoccurred) 
                    {
                        char last = Value[Value.Length-2];
                        if (expoccurred)
                        {
                            if (last != 'e' && last != 'E')
                            {
                                // Can't occur here as isn't directly after the exponent
                                throw UnexpectedCharacter(next, "The +/- Sign can only occur at the start of a Numeric Literal or at the start of the Exponent");
                            }
                        }
                        else
                        {
                            // Negative sign already seen
                            throw UnexpectedCharacter(next, "The +/- Sign can only occur at the start of a Numeric Literal or at the start of the Exponent");
                        }
                    } 
                    else
                    {
                        signoccurred = true;

                        // Check this is at the start of the string
                        if (Length > 1)
                        {
                            throw UnexpectedCharacter(next, "The +/- Sign can only occur at the start of a Numeric Literal or at the start of the Exponent");
                        }
                    }
                }
                else if (next == 'e' || next == 'E')
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
                            throw UnexpectedCharacter(next, "The Exponent specifier cannot occur at the start of a Numeric Literal");
                        }
                    }
                }
                else if (next == '.')
                {
                    dotoccurred = true;
                }

                next = Peek();
            }

            // Validate the final result
            if (Value.EndsWith(".")) Backtrack();
            if (!TurtleSpecsHelper.IsValidPlainLiteral(Value, TurtleSyntax.Original)) 
            {
                throw Error("The format of the Numeric Literal '" + Value + "' is not valid!");
            }

            // Return the Token
            LastTokenType = Token.PLAINLITERAL;
            return new PlainLiteralToken(Value, CurrentLine, StartPosition, EndPosition);
            
        }

        private IToken TryGetPlainLiteralOrQName()
        {
            char next = Peek();

            if (!_keywordsmode)
            {
                #region Non-Keywords Mode

                // Not in Keywords Mode
                bool colonoccurred = false;
                while (Char.IsLetterOrDigit(next) || next == ':' || next == '-' || next == '_')
                {
                    // Consume Character
                    ConsumeCharacter();

                    if (next == ':')
                    {
                        if (colonoccurred)
                        {
                            // Can't contain more than 1 Colon
                            throw Error("Unexpected Character (Code " + (int)next + " :\nThe Colon Character can only occur once in a QName");
                        }
                        else
                        {
                            colonoccurred = true;
                        }
                    }

                    next = Peek();
                }

                // Validate
                String value = Value;

                // If it ends in a trailing . then we need to backtrack
                if (value.EndsWith("."))
                {
                    Backtrack();
                    value = value.Substring(0, value.Length - 1);
                }

                if (value.Equals("a"))
                {
                    // Keyword 'a'
                    LastTokenType = Token.KEYWORDA;
                    return new KeywordAToken(CurrentLine, StartPosition);
                }
                else if (value.Equals("is"))
                {
                    // Keyword 'is'
                    LastTokenType = Token.KEYWORDIS;
                    return new KeywordIsToken(CurrentLine, StartPosition);
                }
                else if (value.Equals("of"))
                {
                    // Keyword 'of'
                    LastTokenType = Token.KEYWORDOF;
                    return new KeywordOfToken(CurrentLine, StartPosition);
                }
                else if (TurtleSpecsHelper.IsValidPlainLiteral(value, TurtleSyntax.Original))
                {
                    // Other Valid Plain Literal
                    LastTokenType = Token.PLAINLITERAL;
                    return new PlainLiteralToken(value, CurrentLine, StartPosition, EndPosition);
                }
                else if (IsValidQName(value) && value.Contains(":"))
                {
                    // Valid QName
                    // Note that in the above condition we require a : since without Keywords mode
                    // all QNames must be in a Namespace
                    if (value.StartsWith("_:"))
                    {
                        // A Blank Node QName
                        LastTokenType = Token.BLANKNODEWITHID;
                        return new BlankNodeWithIDToken(value, CurrentLine, StartPosition, EndPosition);
                    }
                    else
                    {
                        // Normal QName
                        LastTokenType = Token.QNAME;
                        return new QNameToken(value, CurrentLine, StartPosition, EndPosition);
                    }
                }
                else
                {
                    // Not Valid
                    throw Error("The value '" + value + "' is not valid as a Plain Literal or QName");
                }

                #endregion
            }
            else
            {
                #region Keywords Mode

                // Since we're in Keywords Mode this is actually a QName
                // UNLESS it's in the Keywords list

                bool colonoccurred = false;

                while (Char.IsLetterOrDigit(next) || next == '_' || next == '-' || next == ':')
                {
                    // Consume
                    ConsumeCharacter();

                    if (next == ':')
                    {
                        if (colonoccurred)
                        {
                            // Can't contain more than 1 Colon
                            throw Error("Unexpected Character (Code " + (int)next + " :\nThe Colon Character can only occur once in a QName");
                        }
                        else
                        {
                            colonoccurred = true;
                        }
                    }

                    next = Peek();
                }

                // Validate
                String value = Value;

                // If it ends in a trailing . then we need to backtrack
                if (value.EndsWith("."))
                {
                    Backtrack();
                    value = value.Substring(0, value.Length - 1);
                }

                if (_keywords.Contains(value))
                {
                    // A Custom Keyword
                    LastTokenType = Token.KEYWORDCUSTOM;
                    return new CustomKeywordToken(value, CurrentLine, StartPosition, EndPosition);
                }
                else if (!IsValidQName(value))
                {
                    // Not a valid QName
                    throw Error("The value '" + value + "' is not valid as a QName");
                }
                else if (value.StartsWith("_:"))
                {
                    // A Blank Node QName
                    LastTokenType = Token.BLANKNODEWITHID;
                    return new BlankNodeWithIDToken(value, CurrentLine, StartPosition, EndPosition);
                }
                else 
                {
                    // Return the QName
                    LastTokenType = Token.QNAME;

                    // If no Colon need to append it to the front to make a QName in the Default namespace
                    if (!colonoccurred) {
                        value = ":" + value;
                    }

                    return new QNameToken(value, CurrentLine, StartPosition, EndPosition);
                }

                #endregion
            }
        }

        private IToken TryGetKeywordOrLangSpec()
        {
            if (LastTokenType == Token.LITERAL || LastTokenType == Token.LONGLITERAL)
            {
                // Must be a Language Specifier

                // Discard the @
                SkipCharacter();

                // Get the Specifier
                char next = Peek();
                while (Char.IsLetterOrDigit(next) || next == '-')
                {
                    ConsumeCharacter();
                    next = Peek();
                }

                // Return the Language Specifier
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
            else if (LastTokenType == Token.PLAINLITERAL)
            {
                // Can't specify Language on a Plain Literal
                throw Error("Unexpected Character (Code " + (int)'@' + " @\nThe @ sign cannot be used to specify a Language on a Plain Literal");
            }
            else
            {
                // Must be some Keyword

                // Discard the @
                SkipCharacter();

                // Consume until we hit White Space
                char next = Peek();
                while (!Char.IsWhiteSpace(next) && next != '.')
                {
                    ConsumeCharacter();
                    next = Peek();
                }

                // Now check we get something that's an actual Keyword/Directive
                String value = Value;
                if (value.Equals("keywords", StringComparison.OrdinalIgnoreCase))
                {
                    // Keywords Directive

                    // Remember to enable Keywords Mode
                    _keywordsmode = true;

                    LastTokenType = Token.KEYWORDDIRECTIVE;
                    return new KeywordDirectiveToken(CurrentLine, StartPosition);
                }
                else if (value.Equals("base", StringComparison.OrdinalIgnoreCase))
                {
                    // Base Directive
                    LastTokenType = Token.BASEDIRECTIVE;
                    return new BaseDirectiveToken(CurrentLine, StartPosition);
                }
                else if (value.Equals("prefix", StringComparison.OrdinalIgnoreCase))
                {
                    // Prefix Directive
                    LastTokenType = Token.PREFIXDIRECTIVE;
                    return new PrefixDirectiveToken(CurrentLine, StartPosition);
                }
                else if (value.Equals("forall", StringComparison.OrdinalIgnoreCase))
                {
                    // ForAll Quantifier
                    LastTokenType = Token.FORALL;
                    return new ForAllQuantifierToken(CurrentLine, StartPosition);
                }
                else if (value.Equals("forsome", StringComparison.OrdinalIgnoreCase))
                {
                    // ForSome Quantifier
                    LastTokenType = Token.FORSOME;
                    return new ForSomeQuantifierToken(CurrentLine, StartPosition);
                }
                else if (value.Equals("a"))
                {
                    // 'a' Keyword
                    LastTokenType = Token.KEYWORDA;
                    return new KeywordAToken(CurrentLine, StartPosition);
                }
                else if (value.Equals("is"))
                {
                    // 'is' Keyword
                    LastTokenType = Token.KEYWORDIS;
                    return new KeywordIsToken(CurrentLine, StartPosition);
                }
                else if (value.Equals("of"))
                {
                    // 'of' Keyword
                    LastTokenType = Token.KEYWORDOF;
                    return new KeywordOfToken(CurrentLine, StartPosition);
                }
                else if (value.Equals("false") || value.Equals("true"))
                {
                    // Plain Literal Boolean
                    LastTokenType = Token.PLAINLITERAL;
                    return new PlainLiteralToken(value, CurrentLine, StartPosition, EndPosition);
                }
                else if (_keywords.Contains(value))
                {
                    // A Custom Keyword which has been defined
                    LastTokenType = Token.KEYWORDCUSTOM;
                    return new CustomKeywordToken(value, CurrentLine, StartPosition, EndPosition);
                }
                else
                {
                    // Some other unknown and undefined Keyword
                    throw Error("The Keyword '" + value + "' has not been defined and is not a valid Notation 3 Keyword");
                }
            }
        }

        private IToken TryGetKeywordDefinition()
        {
            char next = Peek();

            while (Char.IsWhiteSpace(next))
            {
                // Discard white space we don't want
                if (next == '\n' || next == '\r')
                {
                    // Newlines are forbidden
                    throw Error("Unexpected New Line encountered in a Keywords Directive.  A Keywords Directive must be terminated by a . Line Terminator character");
                }
                else
                {
                    // Discard
                    SkipCharacter();
                }

                next = Peek();
            }
            StartNewToken();

            if (next == '.')
            {
                // Directive is Terminated
                ConsumeCharacter();
                LastTokenType = Token.DOT;
                return new DotToken(CurrentLine, StartPosition);
            }
            else
            {
                while (!Char.IsWhiteSpace(next) && next != '.')
                {
                    // Consume
                    ConsumeCharacter();
                    next = Peek();
                }

                // Add to Keywords List
                if (!_keywords.Contains(Value))
                {
                    _keywords.Add(Value);
                }

                // Return the Keyword Definition
                LastTokenType = Token.KEYWORDDEF;
                return new CustomKeywordDefinitionToken(Value, CurrentLine, StartPosition, EndPosition);
            }
        }

        private IToken TryGetPrefix()
        {
            char next = Peek();

            // Drop leading white space
            while (Char.IsWhiteSpace(next))
            {
                // If we hit a New Line then Error
                if (next == '\n' || next == '\r')
                {
                    throw Error("Unexpected New Line character encountered while attempting to parse Prefix at content:\n" + Value);
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

            // Produce a PrefixToken
            LastTokenType = Token.PREFIX;
            return new PrefixToken(Value, CurrentLine, StartPosition, CurrentPosition);
        }

        private IToken TryGetUri()
        {
            // Consume first thing which must be a <
            ConsumeCharacter();

            char next = Peek();
            if (next == '=')
            {
                // Might be a reverse implies
                ConsumeCharacter();
                next = Peek();
                if (Char.IsWhiteSpace(next))
                {
                    // A Reverse Implies
                    LastTokenType = Token.IMPLIEDBY;
                    return new ImpliedByToken(CurrentLine, StartPosition);
                }
                else
                {
                    // Ambigious
                    ConsumeCharacter();
                    throw Error("Ambigious syntax in string '" + Value + "', the Tokeniser is unable to determine whether an Implied By or a URI was intended");
                }
            }
            else
            {

                while (next != '>')
                {
                    if (Char.IsWhiteSpace(next))
                    {
                        // Discard White Space inside URIs
                        DiscardWhiteSpace();
                    }
                    else if (next == '\\')
                    {
                        // Might be an escape for a >

                        HandleEscapes(TokeniserEscapeMode.PermissiveUri);
                    }
                    else
                    {
                        ConsumeCharacter();
                    }

                    next = Peek();
                }
                // Consume the concluding >
                ConsumeCharacter();

                LastTokenType = Token.URI;
                return new UriToken(Value, CurrentLine, StartPosition, EndPosition);
            }

        }

        private IToken TryGetQName()
        {
            char next = Peek();
            bool colonoccurred = false;

            while (Char.IsLetterOrDigit(next) || next == '_' || next == '-' || next == ':')
            {
                // Consume
                ConsumeCharacter();

                if (next == ':')
                {
                    if (colonoccurred)
                    {
                        // Can't contain more than 1 Colon
                        throw Error("Unexpected Character (Code " + (int)next + " :\nThe Colon Character can only occur once in a QName");
                    }
                    else
                    {
                        colonoccurred = true;
                    }
                }

                next = Peek();
            }

            String value = Value;

            // If it ends in a trailing . then we need to backtrack
            if (value.EndsWith("."))
            {
                Backtrack();
                value = value.Substring(0, value.Length - 1);
            }

            if (!IsValidQName(value))
            {
                // Not a valid QName
                throw Error("The value '" + value + "' is not valid as a QName");
            }
            else if (!colonoccurred && !_keywordsmode)
            {
                // Not a valid QName
                // No : and not in Keywords mode so the : is required
                throw Error("The value '" + value + "' is not valid as a QName since it doesn't contain a Colon Character and the Namespace is thus not determinable");
            }
            else if (value.StartsWith("_:"))
            {
                // A Blank Node QName
                LastTokenType = Token.BLANKNODEWITHID;
                return new BlankNodeWithIDToken(value, CurrentLine, StartPosition, EndPosition);
            }
            else if (_keywordsmode && _keywords.Contains(value))
            {
                // A Custom Keyword
                LastTokenType = Token.KEYWORDCUSTOM;
                return new CustomKeywordToken(value, CurrentLine, StartPosition, EndPosition);
            }
            else
            {
                // Return the QName
                LastTokenType = Token.QNAME;

                // If no Colon need to append it to the front to make a QName in the Default namespace
                // Only apply this if we're in Keywords Mode
                if (!colonoccurred && _keywordsmode)
                {
                    value = ":" + value;
                }

                return new QNameToken(value, CurrentLine, StartPosition, EndPosition);
            }
        }

        private IToken TryGetLiteral()
        {
            // Consume first character which must be a "
            ConsumeCharacter();
            char next = Peek();

            if (next == '"')
            {
                // Might be a Long Literal or an Empty String
                ConsumeCharacter();
                next = Peek();

                if (next == '"')
                {
                    #region Long Literals

                    // Long Literal
                    ConsumeCharacter();

                    next = Peek();
                    while (true)
                    {
                        if (next == '\\')
                        {
                            // Do Escape Processing
                            HandleEscapes(TokeniserEscapeMode.QuotedLiterals);
                        }
                        else if (next == '"')
                        {
                            // Check to see whether we get three in a row
                            ConsumeCharacter();
                            next = Peek();
                            if (next == '"')
                            {
                                // Got two in a row so far
                                ConsumeCharacter();
                                next = Peek();
                                if (next == '"')
                                {
                                    // Got three in a row
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
                            }
                        }
                        else if (next == '\n' || next == '\r')
                        {
                            // Consume the New Line
                            ConsumeNewLine(true);
                        }
                        else if (_in.EndOfStream)
                        {
                            // Hit End of Stream unexpectedly
                            throw Error("Unexpected End of File while trying to Parse a Long Literal from content:\n" + Value);
                        }
                        else
                        {
                            ConsumeCharacter();
                        }
                        next = Peek();
                    }

                    #endregion
                } 
                else 
                {
                    // Empty String
                    LastTokenType = Token.LITERAL;
                    return new LiteralToken(Value, CurrentLine, StartPosition, EndPosition);
                }
            }
            else
            {
                #region Literals

                // Simple quoted literal
                while (next != '"')
                {
                    if (next == '\\')
                    {
                        // Do Escape Processing
                        HandleEscapes(TokeniserEscapeMode.QuotedLiterals);
                    }
                    else if (next == '\n' || next == '\r')
                    {
                        // Illegal New Line
                        throw Error("Unexpected New Line while trying to Parse a Quoted Literal from content:\n" + Value + "\nTo use New Lines you must use the Triple Quote Long Literal syntax");
                    }
                    else if (_in.EndOfStream)
                    {
                        // Hit End of Stream unexpectedly
                        throw Error("Unexpected End of File while trying to Parse a Quoted Literal from content:\n" + Value);
                    } 
                    else 
                    {
                        ConsumeCharacter();
                    }
                    next = Peek();
                }
                // Consume the last character which must be a "
                ConsumeCharacter();

                // Return the Literal
                LastTokenType = Token.LITERAL;
                return new LiteralToken(Value, CurrentLine, StartPosition, EndPosition);

                #endregion
            }
        }

        private IToken TryGetDataTypeOrPath()
        {
            // Consume first character which must be a ^
            ConsumeCharacter();

            // Take a look at the next Character to determine if this is a DataType specifier or a Path specifier
            char next = Peek();
            if (next == '^')
            {
                ConsumeCharacter();
                // Must occur after a Literal
                if (LastTokenType == Token.LITERAL || LastTokenType == Token.LONGLITERAL)
                {
                    // Return the specifier
                    LastTokenType = Token.HATHAT;
                    return new HatHatToken(CurrentLine, StartPosition);
                }
                else if (LastTokenType == Token.PLAINLITERAL)
                {
                    // Can't specify Type on a Plain Literal
                    throw Error("Unexpected ^^ sequence for specifying a DataType was encountered, the DataType cannot be specified for Plain Literals");
                }
                else
                {
                    // Can't use this except after a Literal
                    throw Error("Unexpected ^^ sequence for specifying a DataType was encountered, the DataType specifier can only be used after a Quoted Literal");
                }
            }
            else
            {
                // Path Specifier
                LastTokenType = Token.HAT;
                return new HatToken(CurrentLine, StartPosition);
            }
        }

        private IToken TryGetDataType()
        {
            // Expect to either see a start of a Uri or QName
            char next = Peek();

            if (next == '<')
            {
                // Uri specified DataType
                IToken temp = TryGetUri();
                LastTokenType = Token.DATATYPE;
                return new DataTypeToken("<" + temp.Value + ">", temp.StartLine, temp.StartPosition, temp.EndPosition);
            }
            else if (Char.IsLetter(next) || UnicodeSpecsHelper.IsLetter(next) || next == '_')
            {
                // QName specified Data Type
                IToken temp = TryGetQName();
                if (temp.TokenType == Token.QNAME)
                {
                    LastTokenType = Token.DATATYPE;
                    return new DataTypeToken(temp.Value, temp.StartLine, temp.StartPosition, temp.EndPosition);
                }
                else
                {
                    throw Error("Unexpected Token '" + temp.GetType().ToString() + "' was produced when a QName for a Data Type was expected!");
                }
            } 
            else
            {
                // Invalid Start Character
                throw Error("Unexpected Character (Code " + (int)next + " " + next + "\nExpected a < to start a URI or a valid start character for a QName to specify Data Type");
            }
        }

        private IToken TryGetEqualityOrImplies()
        {
            // Consume first thing
            ConsumeCharacter();

            // See what the next character is
            char next = Peek();

            if (next != '>')
            {
                // Just an Equality Sign
                LastTokenType = Token.EQUALS;
                return new EqualityToken(CurrentLine, StartPosition);
            }
            else
            {
                // An implies
                ConsumeCharacter();
                next = Peek();

                LastTokenType = Token.IMPLIES;
                return new ImpliesToken(CurrentLine, StartPosition);
            }
        }

        private IToken TryGetFormula()
        {
            int openBrackets = 0;
            int closeBrackets = 0;
            char next;

            // Continue consuming characters accounting for nesting as appropriate
            do
            {
                if (_in.EndOfStream && openBrackets > 0)
                {
                    throw Error("Unexpected End of File while trying to Parse a Formula from Content:\n" + Value);
                }

                next = Peek();

                switch (next)
                {
                    case '{':
                        openBrackets++;
                        break;
                    case '}':
                        // openBrackets--;
                        closeBrackets++;
                        break;
                    case '\n':
                    case '\r':
                        // Discard the White Space
                        ConsumeNewLine(false);
                        continue;
                }

                // Consume
                ConsumeCharacter();
            } while (openBrackets > closeBrackets);

            // Return the GraphLiteral
            LastTokenType = Token.GRAPHLITERAL;
            return new GraphLiteralToken(Value, StartLine, EndLine, StartPosition, EndPosition);
        }

        private IToken TryGetVariable()
        {
            // Consume first Character which must be a ?
            ConsumeCharacter();

            // Consume other valid Characters
            char next = Peek();
            while (Char.IsLetterOrDigit(next) || UnicodeSpecsHelper.IsLetterOrDigit(next) || next == '-' || next == '_')
            {
                ConsumeCharacter();
                next = Peek();
            }

            // Validate
            String value = Value;
            if (IsValidVarName(value))
            {
                return new VariableToken(value, CurrentLine, StartPosition, EndPosition);
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
            if (_isValidQName.IsMatch(value))
            {
                return true;
            }
            else
            {
                // Have to validate Character by Character
                char[] cs = value.ToCharArray();
                char first = cs[0];

                // First character must be an underscore, colon or letter
                if (first == '_' || first == ':' || Char.IsLetter(first) || UnicodeSpecsHelper.IsLetter(first))
                {
                    // Remaining Characters must be underscores, colons, letters, numbers or hyphens
                    for (int i = 1; i < cs.Length; i++)
                    {
                        char c = cs[i];
                        if (c == '_' || c == ':' || c == '-' || Char.IsLetterOrDigit(c) || UnicodeSpecsHelper.IsLetterOrDigit(c))
                        {
                            // OK
                        }
                        else
                        {
                            // Invalid Character
                            return false;
                        }
                    }

                    // If we get here it's all fine
                    return true;
                }
                else
                {
                    // Invalid Start Character
                    return false;
                }
            }
        }

        private bool IsValidVarName(String value)
        {
            if (_isValidVarName.IsMatch(value))
            {
                return true;
            }
            else
            {
                // Have to validate Character by Character
                char[] cs = value.ToCharArray();
                char first = cs[0];

                // First character must be an underscore or letter
                if (first == '_' || Char.IsLetter(first) || UnicodeSpecsHelper.IsLetter(first))
                {
                    // Remaining Characters must be underscores, letters, numbers or hyphens
                    for (int i = 1; i < cs.Length; i++)
                    {
                        char c = cs[i];
                        if (c == '_' || c == '-' || Char.IsLetterOrDigit(c) || UnicodeSpecsHelper.IsLetterOrDigit(c))
                        {
                            // OK
                        }
                        else
                        {
                            // Invalid Character
                            return false;
                        }
                    }

                    // If we get here it's all fine
                    return true;
                }
                else
                {
                    // Invalid Start Character
                    return false;
                }
            }
        }

    }
}
