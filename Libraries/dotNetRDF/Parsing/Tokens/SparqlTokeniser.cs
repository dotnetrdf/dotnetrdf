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
    /// A Class for Reading an Input Stream and generating SPARQL Tokens
    /// </summary>
    public class SparqlTokeniser 
        : BaseTokeniser
    {
        private ParsingTextReader _in;
        private bool _queryKeywordSeen = false;
        private bool _baseDeclared = false;
        private SparqlQuerySyntax _syntax = Options.QueryDefaultSyntax;

        /// <summary>
        /// Creates a new Instance of the Tokeniser
        /// </summary>
        /// <param name="input">The Input Stream to generate Tokens from</param>
        /// <param name="syntax">Syntax Mode to use when parsing</param>
        public SparqlTokeniser(StreamReader input, SparqlQuerySyntax syntax)
            : this(ParsingTextReader.Create(input), syntax) { }

        /// <summary>
        /// Creates a new Instance of the Tokeniser
        /// </summary>
        /// <param name="input">The Input Stream to generate Tokens from</param>
        /// <param name="syntax">Syntax Mode to use when parsing</param>
        public SparqlTokeniser(ParsingTextReader input, SparqlQuerySyntax syntax)
            : base(input)
        {
            _in = input;
            Format = "SPARQL";
            _syntax = syntax;
        }

        /// <summary>
        /// Creates a new Instance of the Tokeniser
        /// </summary>
        /// <param name="input">The Input to generate Tokens from</param>
        /// <param name="syntax">Syntax Mode to use when parsing</param>
        public SparqlTokeniser(TextReader input, SparqlQuerySyntax syntax)
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

                StartNewToken();

                try
                {
                    if (LastTokenType == Token.BOF && _in.EndOfStream)
                    {
                        // Empty File
                        return new EOFToken(0, 0);
                    }
                    else if (!_queryKeywordSeen)
                    {
                        // Expecting the Prologue/Keyword of the Query
                        return TryGetPrologueOrQueryKeyword();
                    }
                    else if (LastTokenType == Token.HATHAT)
                    {
                        // Should get a DataType
                        return TryGetDataType();
                    }

                    do
                    {
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
                                throw Error("Unexpected End of File encountered while attempting to Parse Token from content\n" + Value);

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
                        else if (Char.IsLetter(next))
                        {
                            // Could be a Keyword or a QName
                            return TryGetQNameOrKeyword();
                        }
                        else if (Char.IsDigit(next))
                        {
                            // Must be a Numeric Literal
                            return TryGetNumericLiteral();
                        }
                        else
                        {
                            switch (next)
                            {
                                #region Punctuation

                                case '*':
                                    // All/Multiply Token
                                    ConsumeCharacter();
                                    if (LastTokenType == Token.SELECT || LastTokenType == Token.DISTINCT || LastTokenType == Token.REDUCED)
                                    {
                                        LastTokenType = Token.ALL;
                                        return new AllToken(CurrentLine, StartPosition);
                                    }
                                    else
                                    {
                                        LastTokenType = Token.MULTIPLY;
                                        return new MultiplyToken(CurrentLine, StartPosition);
                                    }

                                case '/':
                                    // Divide Token
                                    ConsumeCharacter();
                                    LastTokenType = Token.DIVIDE;
                                    return new DivideToken(CurrentLine, StartPosition);

                                case '=':
                                    // Equals Token
                                    ConsumeCharacter();
                                    LastTokenType = Token.EQUALS;
                                    return new EqualityToken(CurrentLine, StartPosition);

                                case '#':
                                    // Comment Token
                                    return TryGetComment();

                                case '.':
                                    // Statement Terminator
                                    ConsumeCharacter();

                                    // Watch our for plain literals
                                    if (!_in.EndOfStream && Char.IsDigit(Peek()))
                                    {
                                        IToken temp = TryGetNumericLiteral();
                                        if (temp is PlainLiteralToken)
                                        {
                                            LastTokenType = Token.PLAINLITERAL;
                                            return temp;
                                        }
                                        else
                                        {
                                            throw UnexpectedToken("Plain Literal", temp);
                                        }
                                    }
                                    else
                                    {
                                        // This should be the end of a directive
                                        LastTokenType = Token.DOT;
                                        return new DotToken(CurrentLine, StartPosition);
                                    }

                                case ';':
                                    // Predicate Object List deliminator
                                    ConsumeCharacter();
                                    LastTokenType = Token.SEMICOLON;
                                    return new SemicolonToken(CurrentLine, StartPosition);

                                case ',':
                                    // Object List deleminator
                                    ConsumeCharacter();
                                    LastTokenType = Token.COMMA;
                                    return new CommaToken(CurrentLine, StartPosition);

                                case '<':
                                    // Start of a Uri or a Less Than
                                    return TryGetUri();

                                case '>':
                                    // Greater Than
                                    ConsumeCharacter();
                                    next = Peek();
                                    if (next == '=')
                                    {
                                        // Greater Than or Equal To
                                        ConsumeCharacter();
                                        LastTokenType = Token.GREATERTHANOREQUALTO;
                                        return new GreaterThanOrEqualToToken(CurrentLine, StartPosition);
                                    }
                                    else
                                    {
                                        // Greater Than
                                        LastTokenType = Token.GREATERTHAN;
                                        return new GreaterThanToken(CurrentLine, StartPosition);
                                    }

                                case '"':
                                    // Start of a Literal
                                    return TryGetLiteral('"');

                                case '\'':
                                    // Start of a Literal
                                    return TryGetLiteral('\'');

                                case '^':
                                    // DataType Specifier/Path Inverse
                                    ConsumeCharacter();
                                    next = Peek();
                                    if (next == '^')
                                    {
                                        // DataType specifier
                                        ConsumeCharacter();
                                        LastTokenType = Token.HATHAT;
                                        return new HatHatToken(CurrentLine, StartPosition);
                                    }
                                    else
                                    {
                                        // Path inverse
                                        LastTokenType = Token.HAT;
                                        return new HatToken(CurrentLine, StartPosition);
                                        // throw Error("Unexpected Character (Code " + (int)next + ") " + (char)next + " was encountered after a ^ character, expected a ^^ for a Data Type specifier");
                                    }

                                case '+':
                                case '-':
                                    // Start of a Numeric Literal
                                    return TryGetNumericLiteral();

                                case '!':
                                    // Logical Negation/Not Equals
                                    ConsumeCharacter();
                                    next = Peek();
                                    if (next == '=')
                                    {
                                        // Not Equals
                                        ConsumeCharacter();
                                        LastTokenType = Token.NOTEQUALS;
                                        return new NotEqualsToken(CurrentLine, StartPosition);
                                    }
                                    else
                                    {
                                        // Logical Negation
                                        LastTokenType = Token.NEGATION;
                                        return new NegationToken(CurrentLine, StartPosition);
                                    }

                                case '&':
                                    // Logical and
                                    ConsumeCharacter();
                                    next = Peek();
                                    if (next == '&')
                                    {
                                        ConsumeCharacter();
                                        LastTokenType = Token.AND;
                                        return new AndToken(CurrentLine, StartPosition);
                                    }
                                    else
                                    {
                                        throw Error("Unexpected Character (Code " + (int)next + ") " + (char)next + " was encountered while trying to parse a Logical And operator");
                                    }

                                case '|':
                                    // Logical or/Path Alternative
                                    ConsumeCharacter();
                                    next = Peek();
                                    if (next == '|')
                                    {
                                        // Logical Or
                                        ConsumeCharacter();
                                        LastTokenType = Token.OR;
                                        return new OrToken(CurrentLine, StartPosition);
                                    }
                                    else
                                    {
                                        // Path Alternative
                                        LastTokenType = Token.BITWISEOR;
                                        return new BitwiseOrToken(CurrentLine, StartPosition);
                                        // throw Error("Unexpected Character (Code " + (int)next + ") " + (char)next + " was encountered while trying to parse a Logical Or operator");
                                    }

                                case '@':
                                    // Lang Specifier
                                    return TryGetLangSpec();

                                #endregion

                                #region Variables and QNames

                                case '$':
                                case '?':
                                    // Start of a Variable
                                    return TryGetVariable();

                                case ':':
                                case '_':
                                    // A QName
                                    return TryGetQName();

                                #endregion

                                #region Brackets

                                case '{':
                                    // Left Bracket
                                    ConsumeCharacter();
                                    LastTokenType = Token.LEFTCURLYBRACKET;
                                    return new LeftCurlyBracketToken(CurrentLine, StartPosition);

                                case '}':
                                    // Right Bracket
                                    ConsumeCharacter();
                                    LastTokenType = Token.RIGHTCURLYBRACKET;
                                    return new RightCurlyBracketToken(CurrentLine, StartPosition);

                                case '(':
                                    // Left Bracket
                                    ConsumeCharacter();
                                    LastTokenType = Token.LEFTBRACKET;
                                    return new LeftBracketToken(CurrentLine, StartPosition);

                                case ')':
                                    // Right Bracket
                                    ConsumeCharacter();
                                    LastTokenType = Token.RIGHTBRACKET;
                                    return new RightBracketToken(CurrentLine, StartPosition);

                                case '[':
                                    // Left Bracket
                                    ConsumeCharacter();
                                    LastTokenType = Token.LEFTSQBRACKET;
                                    return new LeftSquareBracketToken(CurrentLine, StartPosition);

                                case ']':
                                    // Right Bracket
                                    ConsumeCharacter();
                                    LastTokenType = Token.RIGHTSQBRACKET;
                                    return new RightSquareBracketToken(CurrentLine, StartPosition);

                                #endregion

                                default:
                                    // Unexpected Character
                                    throw Error("Unexpected Character (Code " + (int)next + ") " + (char)next + " was encountered!");

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

        private IToken TryGetComment()
        {
            // Consume the first character which must have been a #
            ConsumeCharacter();

            // Consume everything up till we hit the new line
            char next = Peek();
            while (next != '\n' && next != '\r')
            {
                if (ConsumeCharacter(true)) break;
                next = Peek();
            }

            // Create the Token, discard the new line and return
            LastTokenType = Token.COMMENT;
            CommentToken comment = new CommentToken(Value, CurrentLine, StartPosition, EndPosition);
            ConsumeNewLine(false, true);
            return comment;
        }

        private IToken TryGetPrologueOrQueryKeyword()
        {
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

            // Discard leading White Space
            if (Char.IsWhiteSpace(next))
            {
                DiscardWhiteSpace();
            }

            // Get Characters that form the Token
            bool ok = true;
            do
            {
                next = Peek();

                if (Char.IsWhiteSpace(next)) break;

                switch (next)
                {
                    case '#':
                        return TryGetComment();
                    case '<':
                        if (Length == 0)
                        {
                            return TryGetUri();
                        }
                        ok = false;
                        break;
                    case '{':
                        ok = false;
                        break;
                    default:
                        ConsumeCharacter();
                        break;
                }

                if (_in.EndOfStream)
                {
                    // throw Error("Unexpected end of query while trying to Parse Query Prologue");
                    break;
                }
            } while (ok);

            // May need to do backtracking if there was no whitespace between query keyword and {
            if (!_queryKeywordSeen && Value.EndsWith("{"))
            {
                Backtrack();
            }

            // Work out what type of Token we've got
            String value = Value;
            switch (LastTokenType)
            {
                case Token.PREFIXDIRECTIVE:
                    // Expect to see a Prefix
                    return TryGetPrefix();

                case Token.URI:
                case Token.DOT:
                case Token.BOF:
                case Token.COMMENT:
                    // Expect a new BASE, PREFIX or Keyword after a Start of File/Previous Declaration
                    if (value.Equals(SparqlSpecsHelper.SparqlKeywordBase, StringComparison.OrdinalIgnoreCase))
                    {
                        if (_baseDeclared)
                        {
                            // Only one Base Declaration permitted
                            throw Error("Unexpected Base Declaration encountered, the Query Prologue may only contain one Base Declaration");
                        }
                        else
                        {
                            // Got a Base Declaration
                            _baseDeclared = true;
                            LastTokenType = Token.BASEDIRECTIVE;
                            return new BaseDirectiveToken(CurrentLine, StartPosition);
                        }
                    }
                    else if (value.Equals(SparqlSpecsHelper.SparqlKeywordPrefix, StringComparison.OrdinalIgnoreCase))
                    {
                        // Got a Prefix Declaration
                        LastTokenType = Token.PREFIXDIRECTIVE;
                        return new PrefixDirectiveToken(CurrentLine, StartPosition);
                    }
                    else if (SparqlSpecsHelper.IsQueryKeyword(value))
                    {
                        // Which keyword did we get
                        _queryKeywordSeen = true;
                        if (value.Equals(SparqlSpecsHelper.SparqlKeywordAsk, StringComparison.OrdinalIgnoreCase))
                        {
                            // Ask Keyword
                            LastTokenType = Token.ASK;
                            return new AskKeywordToken(CurrentLine, StartPosition);
                        }
                        else if (value.Equals(SparqlSpecsHelper.SparqlKeywordConstruct, StringComparison.OrdinalIgnoreCase))
                        {
                            // Construct Keyword
                            LastTokenType = Token.CONSTRUCT;
                            return new ConstructKeywordToken(CurrentLine, StartPosition);
                        }
                        else if (value.Equals(SparqlSpecsHelper.SparqlKeywordDescribe, StringComparison.OrdinalIgnoreCase))
                        {
                            // Describe Keyword
                            LastTokenType = Token.DESCRIBE;
                            return new DescribeKeywordToken(CurrentLine, StartPosition);
                        }
                        else if (value.Equals(SparqlSpecsHelper.SparqlKeywordSelect, StringComparison.OrdinalIgnoreCase))
                        {
                            // Select Keyword
                            LastTokenType = Token.SELECT;
                            return new SelectKeywordToken(CurrentLine, StartPosition);
                        }
                        else
                        {
                            throw Error("Unexpected String '" + value + "' encountered when a SPARQL Query Keyword was expected");
                        }
                    }
                    else if (SparqlSpecsHelper.IsUpdateKeyword(value))
                    {
                        _queryKeywordSeen = true;
                        value = value.ToUpper();
                        switch (value)
                        {
                            case SparqlSpecsHelper.SparqlKeywordAdd:
                                // Add Keyword
                                LastTokenType = Token.ADD;
                                return new AddKeywordToken(CurrentLine, StartPosition);
                            case SparqlSpecsHelper.SparqlKeywordClear:
                                // Clear Keyword
                                LastTokenType = Token.CLEAR;
                                return new ClearKeywordToken(CurrentLine, StartPosition);
                            case SparqlSpecsHelper.SparqlKeywordCopy:
                                // Copy Keyword
                                LastTokenType = Token.COPY;
                                return new CopyKeywordToken(CurrentLine, StartPosition);
                            case SparqlSpecsHelper.SparqlKeywordCreate:
                                // Create Keyword
                                LastTokenType = Token.CREATE;
                                return new CreateKeywordToken(CurrentLine, StartPosition);
                            case SparqlSpecsHelper.SparqlKeywordDelete:
                                // Delete Keyword
                                LastTokenType = Token.DELETE;
                                return new DeleteKeywordToken(CurrentLine, StartPosition);
                            case SparqlSpecsHelper.SparqlKeywordDrop:
                                // Drop Keyword
                                LastTokenType = Token.DROP;
                                return new DropKeywordToken(CurrentLine, StartPosition);
                            case SparqlSpecsHelper.SparqlKeywordInsert:
                                // Insert Keyword
                                LastTokenType = Token.INSERT;
                                return new InsertKeywordToken(CurrentLine, StartPosition);
                            case SparqlSpecsHelper.SparqlKeywordLoad:
                                // Load Keyword
                                LastTokenType = Token.LOAD;
                                return new LoadKeywordToken(CurrentLine, StartPosition);
                            case SparqlSpecsHelper.SparqlKeywordMove:
                                // Move Keyword
                                LastTokenType = Token.MOVE;
                                return new MoveKeywordToken(CurrentLine, StartPosition);
                            case SparqlSpecsHelper.SparqlKeywordWith:
                                // With Keyword
                                LastTokenType = Token.WITH;
                                return new WithKeywordToken(CurrentLine, StartPosition);
                            default:
                                throw Error("Unexpected Update Keyword '" + value + "' encountered while trying to parse the Query Prologue, expected an Update Keyword which can start an Update Command");
                        }
                    }
                    else
                    {
                        throw Error("Unexpected String '" + value + "' encountered while trying to parse the Query Prologue, expected a Base Declaration, Prefix Declaration or a Query/Update Keyword");
                    }

                default:
                    // Shouldn't get anything but the above as the Previous Token
                    throw Error("Unexpected String '" + value + "' encountered while trying to parse the Query Prologue, expected a Base Declaration, Prefix Declaration or a Query/Update Keyword");
            }
        }

        private IToken TryGetPrefix()
        {
            // Drop leading white space
            // this.DiscardWhiteSpace();

            // Get the Prefix Characters (unless we've already got them)
            if (Length == 0)
            {
                char next = Peek();
                while (!Char.IsWhiteSpace(next) && next != '<')
                {
                    ConsumeCharacter();
                    if (next == ':') break;
                    next = Peek();
                }
            }
            if (!Value.EndsWith(":"))
            {
                throw new RdfParseException("Didn't find expected : Character while attempting to parse Prefix at content:\n" + Value + "\nPrefixes must end in a Colon Character", StartLine, CurrentLine, StartPosition, CurrentPosition);
            }
            if (!SparqlSpecsHelper.IsValidPrefix(Value))
            {
                throw new RdfParseException("The value '" + Value + "' is not a valid Prefix in SPARQL", new PositionInfo(StartLine, CurrentLine, StartPosition, EndPosition));
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
                // Might be a less than or equal to
                ConsumeCharacter();
                next = Peek();
                if (Char.IsWhiteSpace(next))
                {
                    // A Less Than or Equal To
                    LastTokenType = Token.LESSTHANOREQUALTO;
                    return new LessThanOrEqualToToken(CurrentLine, StartPosition);
                }
                else
                {
                    // Ambigious
                    ConsumeCharacter();
                    throw Error("Ambigious syntax in string '" + Value + "', the Tokeniser is unable to determine whether a Less Than or a URI was intended");
                }
            }
            else if (Char.IsWhiteSpace(next))
            {
                // Appears to be a Less Than
                LastTokenType = Token.LESSTHAN;
                return new LessThanToken(CurrentLine, StartPosition);
            }
            else
            {

                while (next != '>')
                {
                    if (Char.IsWhiteSpace(next))
                    {
                        // White space is illegal in URIs
                        throw Error("Illegal white space in URI '" + Value + "'");
                    }
                    else if (next == '\\')
                    {
                        // Might be an escape for a >
                        HandleEscapes(TokeniserEscapeMode.Uri);
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

            while (UnicodeSpecsHelper.IsLetterOrDigit(next) || UnicodeSpecsHelper.IsLetterModifier(next) || next == '_' || next == '-' || next == ':' || next == '.'  || next == '\\' || next == '%')
            {
                if (next == '\\' || next == '%')
                {
                    // Handle Escapes
                    if (!colonoccurred || _syntax == SparqlQuerySyntax.Sparql_1_0)
                    {
                        HandleEscapes(TokeniserEscapeMode.QName);
                    }
                    else
                    {
                        HandleComplexLocalNameEscapes();
                    }
                    next = Peek();
                    continue;
                }
                else
                {
                    // Consume
                    ConsumeCharacter();
                }

                if (next == ':')
                {
                    if (colonoccurred)
                    {
                        // Can't contain more than 1 Colon in SPARQL 1.0
                        if (_syntax == SparqlQuerySyntax.Sparql_1_0) throw Error("Unexpected Character (Code " + (int)next + " :\nThe Colon Character can only occur once in a QName");
                    }
                    else
                    {
                        colonoccurred = true;

                        // Check it's not an assignment operator
                        if (Value.Length == 1)
                        {
                            next = Peek();
                            if (next == '=')
                            {
                                ConsumeCharacter();
                                LastTokenType = Token.ASSIGNMENT;
                                return new AssignmentToken(CurrentLine, StartPosition);
                            }
                        }
                    }
                }

                next = Peek();
            }

            String value = Value;

            // Backtrack if necessary
            if (value.EndsWith(".") && (_syntax == SparqlQuerySyntax.Sparql_1_0 || value[value.Length-2] != '\\'))
            {
                Backtrack();
                value = value.Substring(0, value.Length - 1);
            }

            if (value.Equals("a"))
            {
                // Keyword 'a'
                return new KeywordAToken(CurrentLine, StartPosition);
            }
            else if (value.Equals("true") || value.Equals("false"))
            {
                // Boolean Literal
                return new PlainLiteralToken(value, CurrentLine, StartPosition, EndPosition);
            }
            else if (value.StartsWith("_:"))
            {
                if (!SparqlSpecsHelper.IsValidBNode(value)) throw Error("The value '" + value + "' is not valid as a Blank Node identifier");

                // A Blank Node QName
                LastTokenType = Token.BLANKNODEWITHID;
                return new BlankNodeWithIDToken(value, CurrentLine, StartPosition, EndPosition);
            }
            else if (!SparqlSpecsHelper.IsValidQName(value, _syntax))
            {
                // Not a valid QName
                throw Error("The value '" + value + "' is not valid as a QName");
            }
            else
            {
                // Return the QName
                LastTokenType = Token.QNAME;
                return new QNameToken(SparqlSpecsHelper.UnescapeQName(value), CurrentLine, StartPosition, EndPosition);
            }
        }

        private IToken TryGetQNameOrKeyword()
        {
            char next = Peek();
            bool colonoccurred = false;
            String value;

            while (UnicodeSpecsHelper.IsLetterOrDigit(next) || UnicodeSpecsHelper.IsLetterModifier(next) || next == '_' || next == '-' || next == ':' || next == '.' || next == '\\' || next == '%')
            {
                if (next == '\\' || next == '%')
                {
                    // Handle Escapes
                    if (!colonoccurred || _syntax == SparqlQuerySyntax.Sparql_1_0)
                    {
                        HandleEscapes(TokeniserEscapeMode.QName);
                    }
                    else
                    {
                        HandleComplexLocalNameEscapes();
                    }
                    next = Peek();
                    continue;
                }
                else
                {
                    // Consume
                    ConsumeCharacter();
                }

                if (next == ':')
                {
                    if (colonoccurred)
                    {
                        // Can't contain more than 1 Colon in SPARQL 1.0
                        if (_syntax == SparqlQuerySyntax.Sparql_1_0) throw Error("Unexpected Character (Code " + (int)next + " :\nThe Colon Character can only occur once in a QName");
                    }
                    else
                    {
                        colonoccurred = true;
                    }
                }

                next = Peek();
            }

            value = Value;

            // Backtrack if necessary
            if (value.EndsWith("."))
            {
                Backtrack();
                value = value.Substring(0, value.Length - 1);
            }

            if (!colonoccurred && (SparqlSpecsHelper.IsNonQueryKeyword(value) || SparqlSpecsHelper.IsFunctionKeyword(value) || SparqlSpecsHelper.IsAggregateKeyword(value) || SparqlSpecsHelper.IsUpdateKeyword(value)))
            {
                value = value.ToUpper();
                switch (value)
                {
                    case SparqlSpecsHelper.SparqlKeywordAbs:
                        // Abs Function Keyword
                        LastTokenType = Token.ABS;
                        return new AbsKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordAdd:
                        // Add Update Keyword
                        LastTokenType = Token.ADD;
                        return new AddKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordAll:
                        // All Keyword
                        LastTokenType = Token.ALLWORD;
                        return new AllKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordAs:
                        // As Alias Keyword
                        LastTokenType = Token.AS;
                        return new AsKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordAvg:
                        // Average Aggregate Keyword
                        LastTokenType = Token.AVG;
                        return new AvgKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordBind:
                        // Bind Keyword
                        LastTokenType = Token.BIND;
                        return new BindKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordBindings:
                        // Bindings Keyword
                        LastTokenType = Token.BINDINGS;
                        return new BindingsKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordBNode:
                        // BNode Keyword
                        LastTokenType = Token.BNODE;
                        return new BNodeKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordBound:
                        // Bound Function Keyword
                        LastTokenType = Token.BOUND;
                        return new BoundKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordCall:
                        // Call Function Keyword
                        LastTokenType = Token.CALL;
                        return new CallKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordCeil:
                        // Ceil Function Keyword
                        LastTokenType = Token.CEIL;
                        return new CeilKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordClear:
                        // Clear Keyword
                        LastTokenType = Token.CLEAR;
                        return new ClearKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordCopy:
                        // Copy Update Keyword
                        LastTokenType = Token.COPY;
                        return new CopyKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordCoalesce:
                        // COALESCE Function Keyword
                        LastTokenType = Token.COALESCE;
                        return new CoalesceKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordConcat:
                        // Concat Function Keyword
                        LastTokenType = Token.CONCAT;
                        return new ConcatKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordCount:
                        // Count Aggregate Keyword
                        LastTokenType = Token.COUNT;
                        return new CountKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordCreate:
                        // Create Keyword
                        LastTokenType = Token.CREATE;
                        return new CreateKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordData:
                        // Data Keyword
                        LastTokenType = Token.DATA;
                        return new DataKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordDataType:
                        // Datatype Function Keyword
                        LastTokenType = Token.DATATYPEFUNC;
                        return new DataTypeKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordDay:
                        // Day Function Keyword
                        LastTokenType = Token.DAY;
                        return new DayKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordDefault:
                        // Default Keyword
                        LastTokenType = Token.DEFAULT;
                        return new DefaultKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordDelete:
                        // Delete Keyword
                        LastTokenType = Token.DELETE;
                        return new DeleteKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordDistinct:
                        // Distinct Keyword
                        LastTokenType = Token.DISTINCT;
                        return new DistinctKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordDrop:
                        // Drop Keyword
                        LastTokenType = Token.DROP;
                        return new DropKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordEncodeForUri:
                        // EncodeForUri Function Keyword
                        LastTokenType = Token.ENCODEFORURI;
                        return new EncodeForUriKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordExists:
                        // Exists Keyword
                        LastTokenType = Token.EXISTS;
                        return new ExistsKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordFilter:
                        // Filter Keyword
                        LastTokenType = Token.FILTER;
                        return new FilterKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordFloor:
                        // Floor Function Keyword
                        LastTokenType = Token.FLOOR;
                        return new FloorKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordFrom:
                        // From Keyword
                        LastTokenType = Token.FROM;
                        return new FromKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordGraph:
                        // Graph Keyword
                        LastTokenType = Token.GRAPH;
                        return new GraphKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordGroup:
                        // GROUP Keyword, must be followed by a BY Keyword to form a GROUP BY keyword
                        if (GetExpectedKeyword(SparqlSpecsHelper.SparqlKeywordGroupBy))
                        {
                            LastTokenType = Token.GROUPBY;
                            return new GroupByKeywordToken(CurrentLine, StartPosition);
                        }
                        else
                        {
                            throw Error("Unexpected content while attempting to parse an GROUP BY keyword from Content:\n" + Value);
                        }
                    case SparqlSpecsHelper.SparqlKeywordGroupConcat:
                        // GROUP_CONCAT Keyword
                        LastTokenType = Token.GROUPCONCAT;
                        return new GroupConcatKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordHaving:
                        // HAVING Keyword
                        LastTokenType = Token.HAVING;
                        return new HavingKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordHours:
                        // Hours Function Keyword
                        LastTokenType = Token.HOURS;
                        return new HoursKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordIf:
                        // IF Keyword
                        LastTokenType = Token.IF;
                        return new IfKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordIn:
                        // IN Keyword
                        LastTokenType = Token.IN;
                        return new InKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordInsert:
                        // Insert Keyword
                        LastTokenType = Token.INSERT;
                        return new InsertKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordInto:
                        // Into Keyword
                        LastTokenType = Token.INTO;
                        return new IntoKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordIri:
                        // IRI Function Keyword
                        LastTokenType = Token.IRI;
                        return new IriKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordIsBlank:
                        // isBlank Function Keyword
                        LastTokenType = Token.ISBLANK;
                        return new IsBlankKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordIsIri:
                        // isIRI Function Keyword
                        LastTokenType = Token.ISIRI;
                        return new IsIriKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordIsLiteral:
                        // isLiteral Keyword
                        LastTokenType = Token.ISLITERAL;
                        return new IsLiteralKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordIsNumeric:
                        // IsNumeric Function Keyword
                        LastTokenType = Token.ISNUMERIC;
                        return new IsNumericKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordIsUri:
                        // isURI Keyword
                        LastTokenType = Token.ISURI;
                        return new IsUriKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordLang:
                        // Lang Keyword
                        LastTokenType = Token.LANG;
                        return new LangKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordLangMatches:
                        // Lang Matches Keyword
                        LastTokenType = Token.LANGMATCHES;
                        return new LangMatchesKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordLCase:
                        // LCase Function Keyword
                        LastTokenType = Token.LCASE;
                        return new LCaseKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordLength:
                        // Length Keyword
                        if (_syntax != SparqlQuerySyntax.Extended) throw Error("The LENGTH keyword is only supported when syntax is set to Extended");
                        LastTokenType = Token.LENGTH;
                        return new LengthKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordLet:
                        // Let Keyword
                        LastTokenType = Token.LET;
                        return new LetKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordLimit:
                        // Limit Keyword
                        LastTokenType = Token.LIMIT;
                        return new LimitKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordLoad:
                        // Load Keyword
                        LastTokenType = Token.LOAD;
                        return new LoadKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordMax:
                        // Max Aggregate Keyword
                        LastTokenType = Token.MAX;
                        return new MaxKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordMD5:
                        // MD5 Function Keyword
                        LastTokenType = Token.MD5;
                        return new MD5KeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordMedian:
                        // Median Aggregate Keyword
                        LastTokenType = Token.MEDIAN;
                        return new MedianKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordMin:
                        // Min Aggregate Keyword
                        LastTokenType = Token.MIN;
                        return new MinKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordMinus:
                        // Minus Keyword
                        LastTokenType = Token.MINUS_P;
                        return new MinusKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordMinutes:
                        // Minutes Function Keyword
                        LastTokenType = Token.MINUTES;
                        return new MinutesKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordMode:
                        // Mode Aggregate Keyword
                        LastTokenType = Token.MODE;
                        return new ModeKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordMonth:
                        // Month Function Keyword
                        LastTokenType = Token.MONTH;
                        return new MonthKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordMove:
                        // Move Update Keyword
                        LastTokenType = Token.MOVE;
                        return new MoveKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordNamed:
                        // Named Keyword
                        LastTokenType = Token.NAMED;
                        return new NamedKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordNMax:
                        // Numeric Max Keyword
                        LastTokenType = Token.NMAX;
                        return new NumericMaxKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordNMin:
                        // Numeric Min Keyword
                        LastTokenType = Token.NMIN;
                        return new NumericMinKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordNot:
                        // Not Keyword
                        // Must be followed by a EXISTS or an IN Keyword
                        if (GetExpectedKeyword(SparqlSpecsHelper.SparqlKeywordNotExists))
                        {
                            // Not Exists Keyword
                            LastTokenType = Token.NOTEXISTS;
                            return new NotExistsKeywordToken(CurrentLine, StartPosition);
                        }
                        else if (Value.Equals(SparqlSpecsHelper.SparqlKeywordNotIn, StringComparison.OrdinalIgnoreCase))
                        {
                            // Not In Keyword
                            LastTokenType = Token.NOTIN;
                            return new NotInKeywordToken(CurrentLine, StartPosition);
                        }
                        else
                        {
                            throw Error("Unexpected content while attempting to parse an NOT EXISTS/NOT IN keyword from Content:\n" + Value);
                        }
                    case SparqlSpecsHelper.SparqlKeywordNow:
                        // Now Function Keyword
                        LastTokenType = Token.NOW;
                        return new NowKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordOffset:
                        // Offset Keyword
                        LastTokenType = Token.OFFSET;
                        return new OffsetKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordOptional:
                        // Optional Keyword
                        LastTokenType = Token.OPTIONAL;
                        return new OptionalKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordOrder:
                        // Order Keyword
                        // Must be followed by a BY Keyword
                        if (GetExpectedKeyword(SparqlSpecsHelper.SparqlKeywordOrderBy))
                        {
                            // Order By Keyword
                            LastTokenType = Token.ORDERBY;
                            return new OrderByKeywordToken(CurrentLine, StartPosition);
                        }
                        else
                        {
                            throw Error("Unexpected content while attempting to parse an ORDER BY keyword from Content:\n" + Value);
                        }
                    case SparqlSpecsHelper.SparqlKeywordRand:
                        // Rand Keyword
                        LastTokenType = Token.RAND;
                        return new RandKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordReduced:
                        // Reduced Keyword
                        LastTokenType = Token.REDUCED;
                        return new ReducedKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordRegex:
                        // Regex Keyword
                        LastTokenType = Token.REGEX;
                        return new RegexKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordReplace:
                        // Replace Keyword
                        LastTokenType = Token.REPLACE;
                        return new ReplaceKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordRound:
                        // Round Function Keyword
                        LastTokenType = Token.ROUND;
                        return new RoundKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordSameTerm:
                        // sameTerm Keyword
                        LastTokenType = Token.SAMETERM;
                        return new SameTermKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordSample:
                        // Sample Keyword
                        LastTokenType = Token.SAMPLE;
                        return new SampleKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordSeconds:
                        // Seconds Keywords
                        LastTokenType = Token.SECONDS;
                        return new SecondsKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordSeparator:
                        // Separator Keyword
                        LastTokenType = Token.SEPARATOR;
                        return new SeparatorKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordService:
                        // Service Keyword
                        LastTokenType = Token.SERVICE;
                        return new ServiceKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordSha1:
                        // Sha1 Function Keyword
                        LastTokenType = Token.SHA1;
                        return new Sha1KeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordSha256:
                        // Sha256 Function Keyword
                        LastTokenType = Token.SHA256;
                        return new Sha256KeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordSha384:
                        // Sha384 Function Keyword
                        LastTokenType = Token.SHA384;
                        return new Sha384KeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordSha512:
                        // Sha1 Function Keyword
                        LastTokenType = Token.SHA512;
                        return new Sha512KeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordSilent:
                        // Silent Keyword
                        LastTokenType = Token.SILENT;
                        return new SilentKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordStr:
                        // Str Keyword
                        LastTokenType = Token.STR;
                        return new StrKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordStrAfter:
                        // StrAfter Keyword
                        LastTokenType = Token.STRAFTER;
                        return new StrAfterKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordStrBefore:
                        // StrBefore Keyword
                        LastTokenType = Token.STRBEFORE;
                        return new StrBeforeKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordContains:
                        // StrContains Function Keyword
                        LastTokenType = Token.CONTAINS;
                        return new StrContainsKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordStrDt:
                        // StrDt Keyword
                        LastTokenType = Token.STRDT;
                        return new StrDtKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordStrLang:
                        // StrLang Keyword
                        LastTokenType = Token.STRLANG;
                        return new StrLangKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordStrLen:
                        // StrLen Function Keyword
                        LastTokenType = Token.STRLEN;
                        return new StrLenKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordStrEnds:
                        // StrEnds Function Keyword
                        LastTokenType = Token.STRENDS;
                        return new StrEndsKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordStrStarts:
                        // StrStarts Function Keyword
                        LastTokenType = Token.STRSTARTS;
                        return new StrStartsKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordStrUUID:
                        // StrUUID Function Keyword
                        LastTokenType = Token.STRUUID;
                        return new StrUUIDKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordSubStr:
                        // SubStr Function Keyword
                        LastTokenType = Token.SUBSTR;
                        return new SubStrKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordSum:
                        // Sum Keyword
                        LastTokenType = Token.SUM;
                        return new SumKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordTimezone:
                        // Timezone Function Keyword
                        LastTokenType = Token.TIMEZONE;
                        return new TimezoneKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordTo:
                        // To Keyword
                        LastTokenType = Token.TO;
                        return new ToKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordTz:
                        // TZ Function Keyword
                        LastTokenType = Token.TZ;
                        return new TZKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordUCase:
                        // UCase Function Keyword
                        LastTokenType = Token.UCASE;
                        return new UCaseKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordUndef:
                        // Undef Keyword
                        LastTokenType = Token.UNDEF;
                        return new UndefKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordUnion:
                        // Union Keyword
                        LastTokenType = Token.UNION;
                        return new UnionKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordUnsaid:
                        // Unsaid Keyword
                        LastTokenType = Token.UNSAID;
                        return new UnsaidKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordUri:
                        // Uri Keyword
                        LastTokenType = Token.URIFUNC;
                        return new UriKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordUUID:
                        // UUID Function Keyword
                        LastTokenType = Token.UUID;
                        return new UUIDKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordUsing:
                        // Using Keyword
                        LastTokenType = Token.USING;
                        return new UsingKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordValues:
                        // Values Kewyord
                        LastTokenType = Token.VALUES;
                        return new ValuesKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordWhere:
                        // Where Keyword
                        LastTokenType = Token.WHERE;
                        return new WhereKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordWith:
                        // With Keyword
                        LastTokenType = Token.WITH;
                        return new WithKeywordToken(CurrentLine, StartPosition);
                    case SparqlSpecsHelper.SparqlKeywordYear:
                        // Year Function Keyword
                        LastTokenType = Token.YEAR;
                        return new YearKeywordToken(CurrentLine, StartPosition);
                    default:
                        throw Error("Unexpected String '" + value + "' encountered while trying to parse a SPARQL Keyword");
                }
            }
            else if (!colonoccurred && _queryKeywordSeen && SparqlSpecsHelper.IsQueryKeyword(value))
            {
                if (_syntax == SparqlQuerySyntax.Sparql_1_0) throw Error("Unexpected String '" + value + "' encountered while trying to parse a SPARQL Keyword.  This appears to be an attempt to use a sub-query but sub-queries are not supported in SPARQL 1.0");

                if (value.Equals(SparqlSpecsHelper.SparqlKeywordSelect, StringComparison.OrdinalIgnoreCase))
                {
                    // Select Keyword
                    LastTokenType = Token.SELECT;
                    return new SelectKeywordToken(CurrentLine, StartPosition);
                }
                else
                {
                    // Other Query Keyword
                    throw Error("Unexpected String '" + value + "' encountered while trying to parse a SPARQL Keyword.  This appears to be an attempt to use an ASK/CONSTRUCT/DESCRIBE as a sub-query which is not supported");
                }
            }
            else if (value.Equals(SparqlSpecsHelper.SparqlKeywordAsc, StringComparison.OrdinalIgnoreCase))
            {
                LastTokenType = Token.ASC;
                return new AscKeywordToken(CurrentLine, StartPosition);
            }
            else if (value.Equals(SparqlSpecsHelper.SparqlKeywordDesc, StringComparison.OrdinalIgnoreCase))
            {
                LastTokenType = Token.DESC;
                return new DescKeywordToken(CurrentLine, StartPosition);
            }
            else if (value.Equals("a"))
            {
                // Keyword 'a'
                LastTokenType = Token.KEYWORDA;
                return new KeywordAToken(CurrentLine, StartPosition);
            }
            else if (value.Equals("true") || value.Equals("false"))
            {
                // Boolean Literal
                LastTokenType = Token.PLAINLITERAL;
                return new PlainLiteralToken(value, CurrentLine, StartPosition, EndPosition);
            }
            else if (!SparqlSpecsHelper.IsValidQName(value, _syntax))
            {
                // Not a valid QName
                throw Error("The value '" + value + "' is not valid as a QName");
            }
            else
            {
                // Return the QName
                LastTokenType = Token.QNAME;
                return new QNameToken(SparqlSpecsHelper.UnescapeQName(value), CurrentLine, StartPosition, EndPosition);
            }
        }

        private bool GetExpectedKeyword(String expectedKeyword)
        {
            char next = Peek();

            // Consume white space
            while (Char.IsWhiteSpace(next))
            {
                ConsumeCharacter();
                next = Peek();
            }

            try 
            {
                // Grab as many characters as we can which are letters

                while (Char.IsLetter(next))
                {
                    ConsumeCharacter();
                    next = Peek();
                }

                // Check this string is the expected keyword
                String keyword = Value;
                return expectedKeyword.Equals(keyword, StringComparison.OrdinalIgnoreCase);
            } 
            catch 
            {
                // If we error then we definitely can't get the expected keyword
                return false;
            }
        }

        private IToken TryGetLiteral(char quotechar)
        {
            // Consume first character which must be a " or '
            ConsumeCharacter();
            char next = Peek();

            if (next == quotechar)
            {
                // Might be a Long Literal or an Empty String
                ConsumeCharacter();
                next = Peek();

                if (next == quotechar)
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
                            if (quotechar == '"')
                            {
                                HandleEscapes(TokeniserEscapeMode.QuotedLiterals);
                            }
                            else
                            {
                                HandleEscapes(TokeniserEscapeMode.QuotedLiteralsAlternate);
                            }
                        }
                        else if (next == quotechar)
                        {
                            // Check to see whether we get three in a row
                            ConsumeCharacter();
                            next = Peek();
                            if (next == quotechar)
                            {
                                // Got two in a row so far
                                ConsumeCharacter();
                                next = Peek();
                                if (next == quotechar)
                                {
                                    // Got three in a row
                                    ConsumeCharacter();
                                    LastTokenType = Token.LONGLITERAL;

                                    // If there are any additional quotes immediatedly following this then
                                    // we want to consume them also
                                    next = Peek();
                                    while (next == quotechar)
                                    {
                                        ConsumeCharacter();
                                        next = Peek();
                                    }

                                    return new LongLiteralToken(Value, StartLine, EndLine, StartPosition, EndPosition);
                                }
                            }
                        }
                        else if (next == '\n' || next == '\r')
                        {
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
                while (next != quotechar)
                {
                    if (next == '\\')
                    {
                        // Do Escape Processing
                        if (quotechar == '"')
                        {
                            HandleEscapes(TokeniserEscapeMode.QuotedLiterals);
                        }
                        else
                        {
                            HandleEscapes(TokeniserEscapeMode.QuotedLiteralsAlternate);
                        }
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
                    } else {
                        ConsumeCharacter();
                    }
                    next = Peek();
                }
                // Consume the last character which must be a " or '
                ConsumeCharacter();

                // Return the Literal
                LastTokenType = Token.LITERAL;
                return new LiteralToken(Value, CurrentLine, StartPosition, EndPosition);

                #endregion
            }
        }

        private IToken TryGetNumericLiteral()
        {
            bool dotoccurred = false;
            bool expoccurred = false;
            bool negoccurred = false;

            if (Length == 1) dotoccurred = true;

            char next = Peek();

            while (Char.IsDigit(next) || next == '-' || next == '+' || next == 'e' || next == 'E' || (next == '.' && !dotoccurred))
            {
                if (next == '+')
                {
                    // Can only be first character in the numeric literal or come immediately after the 'e'
                    if (Length > 0 && !Value.ToLower().EndsWith("e"))
                    {
                        // throw Error("Unexpected Character (Code " + (int)next + ") +\nThe plus sign can only occur once at the Start of a Numeric Literal and once immediately after the exponent specifier, if this was intended as an additive operator please insert space to disambiguate this");
                        break;
                    }
                }
                if (next == '-')
                {
                    if (negoccurred && !Value.ToLower().EndsWith("e"))
                    {
                        // Negative sign already seen
                        // throw Error("Unexpected Character (Code " + (int)next + ") -\nThe minus sign can only occur once at the Start of a Numeric Literal, if this was intended as a subtractive operator please insert space to disambiguate this");
                        break;
                    }
                    else
                    {
                        negoccurred = true;

                        // Check this is at the start of the string or immediately after the 'e'
                        if (Length > 0 && !Value.ToLower().EndsWith("e"))
                        {
                            throw Error("Unexpected Character (Code " + (int)next + ") -\nThe minus sign can only occur at the Start of a Numeric Literal and once immediately after the exponent specifier, if this was intended as a subtractive operator please insert space to disambiguate this");
                        }
                    }
                }
                else if (next == 'e' || next == 'E')
                {
                    if (expoccurred)
                    {
                        // Exponent already seen
                        throw Error("Unexpected Character (Code " + (int)next + " " + next + "\nThe Exponent specifier can only occur once in a Numeric Literal");
                    }
                    else
                    {
                        expoccurred = true;

                        // Check that it isn't the start of the string
                        if (Length == 0)
                        {
                            throw Error("Unexpected Character (Code " + (int)next + " " + next + "\nThe Exponent specifier cannot occur at the start of a Numeric Literal");
                        }
                    }
                }
                else if (next == '.')
                {
                    dotoccurred = true;
                }

                // Consume the Character
                ConsumeCharacter();

                next = Peek();
            }

            // If not SPARQL 1.0 then decimals can no longer end with a .
            if (_syntax != SparqlQuerySyntax.Sparql_1_0)
            {
                // Decimals can't end with a . so backtrack
                if (Value.EndsWith(".")) Backtrack();
            }

            String value = Value;
            if (value.Equals("+"))
            {
                LastTokenType = Token.PLUS;
                return new PlusToken(CurrentLine, StartPosition);
            }
            else if (value.Equals("-"))
            {
                LastTokenType = Token.MINUS;
                return new MinusToken(CurrentLine, StartPosition);
            }
            else if (!SparqlSpecsHelper.IsValidNumericLiteral(value))
            {
                // Invalid Numeric Literal
                throw Error("The format of the Numeric Literal '" + Value + "' is not valid!");
            }

            // Return the Token
            LastTokenType = Token.PLAINLITERAL;
            return new PlainLiteralToken(Value, CurrentLine, StartPosition, EndPosition);
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
            else if (Char.IsLetter(next) || UnicodeSpecsHelper.IsLetter(next) || next == '_' || next == ':')
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

        private IToken TryGetLangSpec()
        {
            if (LastTokenType == Token.LITERAL || LastTokenType == Token.LONGLITERAL)
            {
                // Discard first character which will be the @
                SkipCharacter();
                StartNewToken();

                char next = Peek();
                while (Char.IsLetterOrDigit(next) || next == '-') 
                {
                    ConsumeCharacter();
                    next = Peek();
                }

                // Return the Language Specifier after validation
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
            else
            {
                throw Error("Unexpected Character (Code " + (int)'@' + ") @ encountered, the @ character can only be used as part of a Language Specifier after a Literal/Long Literal");
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
            else if (value.Equals("?"))
            {
                // Path Cardinality Modifier
                return new QuestionToken(CurrentLine, StartPosition);
            }
            else
            {
                throw Error("The value '" + value + "' is not valid as a Variable Name");
            }

        }


    }
}
