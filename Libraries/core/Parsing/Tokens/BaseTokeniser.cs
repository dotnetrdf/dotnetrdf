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
    /// Possible Escape Handling Modes for the Tokeniser
    /// </summary>
    public enum TokeniserEscapeMode
    {
        /// <summary>
        /// Escaping for URIs (every escape but \" is valid)
        /// </summary>
        Uri,
        /// <summary>
        /// Escaping for Quoted Literals (every escape but \&lt; is valid)
        /// </summary>
        QuotedLiterals,
        /// <summary>
        /// Escaping for single Quoted Literals (every escape but \&lt; is valid)
        /// </summary>
        QuotedLiteralsAlternate,
        /// <summary>
        /// Escaping for QNames (only Unicode espaces are valid)
        /// </summary>
        QName
    }

    /// <summary>
    /// Abstract Base Class for Tokeniser which handles the Position tracking
    /// </summary>
    public abstract class BaseTokeniser : ITokeniser
    {
        private TextReader _reader;
        private StringBuilder _output;
        private int _startline = 1;
        private int _endline = 1;
        private int _currline = 1;
        private int _startpos = 1;
        private int _endpos = 1;
        private int _currpos = 1;
        private String _format = "Unknown";
        private int _lasttokentype = -1;
        private char? _tempChar;

        /// <summary>
        /// Constructor for the BaseTokeniser which takes in a TextReader that the Tokeniser will generate Tokens from
        /// </summary>
        /// <param name="reader">TextReader to generator Tokens from</param>
        public BaseTokeniser(TextReader reader)
        {
            this._reader = reader;
        }

        /// <summary>
        /// Gets/Sets the Format that this Tokeniser is used for
        /// </summary>
        /// <remarks>The value set here will replace any instances of {0} specified in inputs to the <see cref="BaseTokeniser.Error">Error()</see> function allowing messages regarding certain syntaxes not being valid in a given format to be provided</remarks>
        protected String Format
        {
            get
            {
                return this._format;
            }
            set
            {
                this._format = value;
            }
        }

        /// <summary>
        /// Gets the Next available Token from the Input
        /// </summary>
        /// <returns></returns>
        /// <exception cref="RdfParseException">Parser Exception if a valid Token cannot be retrieved</exception>
        public abstract IToken GetNextToken();

        /// <summary>
        /// Informs the Helper that you wish to start reading a new Token
        /// </summary>
        protected void StartNewToken()
        {
            //New Output Buffer
            this._output = new StringBuilder();

            //Reset Start and End Position Counters
            this._startline = this._currline;
            this._endline = this._startline;
            this._startpos = this._currpos;
            this._endpos = this._currpos;
        }

        /// <summary>
        /// Peeks at the next Character
        /// </summary>
        /// <returns></returns>
        protected char Peek()
        {
            if (this._tempChar != null)
            {
                char c = (char)this._tempChar;
                return c;
            }
            else
            {
                return (char)this._reader.Peek();
            }
        }

        /// <summary>
        /// Allows you to Backtrack one character (and no more)
        /// </summary>
        protected void Backtrack()
        {
            if (this._tempChar != null) throw Error("Cannot backtrack more than one character");
            if (this._output.Length == 0) throw Error("Cannot backtrack when no characters have been consumed");

            this._tempChar = this._output[this._output.Length - 1];
            this._output.Remove(this._output.Length - 1, 1);            
        }

        /// <summary>
        /// Gets the value of the Output Buffer
        /// </summary>
        protected String Value
        {
            get
            {
                return this._output.ToString();
            }
        }

        /// <summary>
        /// Gets the current length of the Output Buffer
        /// </summary>
        protected int Length
        {
            get
            {
                return this._output.Length;
            }
        }

        /// <summary>
        /// Gets the Current Line in the Input Stream
        /// </summary>
        protected int CurrentLine
        {
            get
            {
                return this._currline;
            }
        }

        /// <summary>
        /// Gets the Current Position in the Input Stream
        /// </summary>
        protected int CurrentPosition
        {
            get
            {
                return this._currpos;
            }
        }

        /// <summary>
        /// Gets the Start Line in the Input Stream of the current Token
        /// </summary>
        protected int StartLine
        {
            get
            {
                return this._startline;
            }
        }

        /// <summary>
        /// Gets the Start Position in the Input Stream of the current Token
        /// </summary>
        protected int StartPosition
        {
            get
            {
                return this._startpos;
            }
        }

        /// <summary>
        /// Gets the End Line in the Input Stream of the current Token
        /// </summary>
        protected int EndLine
        {
            get
            {
                return this._endline;
            }
        }

        /// <summary>
        /// Gets the End Position in the Input Stream of the current Token
        /// </summary>
        protected int EndPosition
        {
            get
            {
                return this._endpos;
            }
        }

        /// <summary>
        /// Gets/Sets the Last Token Type
        /// </summary>
        protected int LastTokenType
        {
            get
            {
                return this._lasttokentype;
            }
            set
            {
                this._lasttokentype = value;
            }
        }

        /// <summary>
        /// Gets whether the Tokeniser has backtracked
        /// </summary>
        protected bool HasBacktracked
        {
            get
            {
                return this._tempChar != null;
            }
        }

        /// <summary>
        /// Consumes a single Character into the Output Buffer and increments the Position Counters
        /// </summary>
        /// <exception cref="RdfParseException">Thrown if the caller tries to read beyond the end of the Stream</exception>
        protected void ConsumeCharacter()
        {
            if (this._tempChar != null)
            {
                char c = (char)this._tempChar;
                this._tempChar = null;
                this._output.Append(c);
                return;
            }

            int temp = this._reader.Read();
            if (temp > -1)
            {
                this._output.Append((char)temp);
                this._currpos++;
                this._endpos++;
            }
            else
            {
                throw Error("Unexpected End of Stream while trying to tokenise from the following input:\n" + this._output.ToString());
            }
        }

        /// <summary>
        /// Consumes a single Character into the Output Buffer and increments the Position Counters
        /// </summary>
        /// <param name="allowEOF">Whether EOF is allowed</param>
        /// <returns>True if the EOF is reached</returns>
        /// <remarks>
        /// If <paramref name="allowEOF"/> is set to false then the normal behaviour is used and an error will be thrown on end of file
        /// </remarks>
        protected bool ConsumeCharacter(bool allowEOF)
        {
            if (!allowEOF)
            {
                this.ConsumeCharacter();
                return false;
            }
            else
            {
                int temp = this._reader.Read();
                if (temp > -1)
                {
                    this._output.Append((char)temp);
                    this._currpos++;
                    this._endpos++;
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        /// <summary>
        /// Consumes a New Line (which may be a single \n or \r or the two characters following each other)
        /// </summary>
        /// <param name="asOutput">Whether the New Line should be added to the Output Buffer</param>
        protected void ConsumeNewLine(bool asOutput)
        {
            this.ConsumeNewLine(asOutput, false);
        }

        /// <summary>
        /// Consumes a New Line (which may be a single \n or \r or the two characters following each other)
        /// </summary>
        /// <param name="asOutput">Whether the New Line should be added to the Output Buffer</param>
        /// <param name="allowEOF">Whether EOF is permitted instead of a New Line</param>
        protected void ConsumeNewLine(bool asOutput, bool allowEOF)
        {
            int c = this._reader.Peek();
            if (c == -1)
            {
                if (!allowEOF) throw UnexpectedEndOfInput(", expected a New Line");
                return;
            }
            char next = (char)c;

            switch (next)
            {
                case '\n':

                    //Discard the White Space
                    this._reader.Read();
                    this._currpos = 1;
                    this._currline++;
                    this._endpos = this._currpos;
                    this._endline++;

                    //See if there's a \r to discard as well
                    next = this.Peek();
                    if (next == '\r')
                    {
                        this._reader.Read();
                        if (asOutput) this._output.Append("\n\r");
                    }
                    else if (asOutput)
                    {
                        this._output.Append('\n');
                    }

                    break;

                case '\r':

                    //Discard the White Space
                    this._reader.Read();
                    this._currpos = 1;
                    this._currline++;
                    this._endpos = this._currpos;
                    this._endline++;

                    //See if there's a \n to discard as well
                    next = this.Peek();
                    if (next == '\n')
                    {
                        this._reader.Read();
                        if (asOutput) this._output.Append("\r\n");
                    }
                    else if (asOutput)
                    {
                        this._output.Append('\r');
                    }

                    break;

                default:
                    throw Error("Cannot Consume a New Line since there is no New Line character at the current position!");

            }
        }

        /// <summary>
        /// Skips a single Character and increments the Position Counters
        /// </summary>
        /// <remarks>Use when you are reading characters into some local buffer and not the global token buffer, used in String escaping</remarks>
        /// <exception cref="RdfParseException">Thrown if the caller tries to read beyond the end of the Stream</exception>
        protected char SkipCharacter()
        {
            if (this._tempChar != null)
            {
                char c = (char)this._tempChar;
                this._tempChar = null;
                return c;
            }

            int temp = this._reader.Read();
            if (temp > -1)
            {
                this._currpos++;
                this._endpos++;
                return (char)temp;
            }
            else
            {
                throw Error("Unexpected End of Stream while trying to tokenise from the following input:\n" + this._output.ToString());
            }
        }

        /// <summary>
        /// Helper function which discards White Space which the Tokeniser doesn't care about and increments position counters correctly
        /// </summary>
        protected void DiscardWhiteSpace()
        {
            char next = this.Peek();

            while (Char.IsWhiteSpace(next))
            {
                switch (next)
                {
                    case '\n':
                    case '\r':
                        this.ConsumeNewLine(false);
                        break;

                    default:
                        //Discard and Increment Position Counters
                        this.SkipCharacter();
                        this._startpos++;
                        break;
                }

                //Get the Next Character
                next = this.Peek();
            }

            this.StartNewToken();
        }

        /// <summary>
        /// Handles the standard escapes supported in all the  UTF-8 based RDF serializations
        /// </summary>
        protected void HandleEscapes(TokeniserEscapeMode mode)
        {
            //Grab the first character which must be a \
            char next = this.SkipCharacter();

            //Stuff for Unicode escapes
            StringBuilder localOutput;

            next = this.Peek();
            switch (next)
            {
                case '\\':
                    //Backslash escape
                    if (mode != TokeniserEscapeMode.QName)
                    {
                        //Consume this one Backslash
                        this.ConsumeCharacter();

                        //If this was a backslash escape i.e. \\
                        //Then need to check whether the subsequent character could be confused with a valid escape
                        //in the tokenised output and if so insert another backslash into the output
                        next = this.Peek();
                        switch (next)
                        {
                            case 't':
                            case 'n':
                            case 'r':
                            case 'u':
                            case 'U':
                                this._output.Append("\\");
                                break;
                        }

                        return;
                    }
                    else
                    {
                        goto default;
                    }
                case '"':
                    //Quote escape (only valid in Quoted Literals)
                    if (mode == TokeniserEscapeMode.QuotedLiterals || mode == TokeniserEscapeMode.QuotedLiteralsAlternate)
                    {
                        //Consume and return
                        this.ConsumeCharacter();
                        return;
                    }
                    else
                    {
                        goto default;
                    }
                case '\'':
                    //Single Quote Escape (only valid in Alternate Quoted Literals)
                    if (mode == TokeniserEscapeMode.QuotedLiteralsAlternate)
                    {
                        //Consume and return
                        this.ConsumeCharacter();
                        return;
                    }
                    else
                    {
                        goto default;
                    }
                case '>':
                    //End Uri Escape (only valid in URIs)
                    if (mode == TokeniserEscapeMode.Uri)
                    {
                        //Consume and return
                        this.ConsumeCharacter();
                        return;
                    }
                    else
                    {
                        goto default;
                    }

                #region White Space Escapes

                case 'n':
                    //New Line Escape
                    if (mode != TokeniserEscapeMode.QName)
                    {
                        //Discard and append a real New Line to the output
                        this.SkipCharacter();
                        this._output.Append("\n");
                        return;
                    }
                    else
                    {
                        goto default;
                    }
                case 'r':
                    //New Line Escape
                    if (mode != TokeniserEscapeMode.QName)
                    {
                        //Discard and append a real New Line to the output
                        this.SkipCharacter();
                        this._output.Append("\r");
                        return;
                    }
                    else
                    {
                        goto default;
                    }
                case 't':
                    //Tab Escape
                    if (mode != TokeniserEscapeMode.QName)
                    {
                        //Discard and append a real Tab to the output
                        this.SkipCharacter();
                        this._output.Append("\t");
                        return;
                    }
                    else
                    {
                        goto default;
                    }

                #endregion

                #region Unicode Escapes

                case 'u':
                    //Need to consume the u first
                    localOutput = new StringBuilder();
                    this.SkipCharacter();

                    next = this.Peek();

                    //Try to get Four Hex Digits
                    while (localOutput.Length < 4 && this.IsHexDigit(next))
                    {
                        localOutput.Append(next);
                        this.SkipCharacter();
                        next = this.Peek();
                    }

                    //Did we get four Hex Digits
                    if (localOutput.Length != 4)
                    {
                        throw Error("Unexpected Character (Code " + (int)next + "): " + next + " encountered while trying to parse Unicode Escape from Content:\n" + this._output.ToString() + "\nThe \\u Escape must be followed by four Hex Digits");
                    }
                    else if (localOutput.ToString().Equals("0000"))
                    {
                        //Ignore the null escape
                    }
                    else
                    {
                        this._output.Append(UnicodeSpecsHelper.ConvertToChar(localOutput.ToString()));
                    }

                    return;

                case 'U':
                    //Need to consume the U first
                    localOutput = new StringBuilder();
                    this.SkipCharacter();

                    next = this.Peek();

                    //Try to get Eight Hex Digits
                    while (localOutput.Length < 8 && this.IsHexDigit(next))
                    {
                        localOutput.Append(next);
                        this.SkipCharacter();
                        next = this.Peek();
                    }

                    //Did we get eight Hex Digits
                    this._output.Append(localOutput.ToString());
                    if (localOutput.Length != 8)
                    {
                        throw Error("Unexpected Character (Code " + (int)next + "): " + next + " encountered while trying to parse Unicode Escape from Content:\n" + this._output.ToString() + "\nThe \\U Escape must be followed by eight Hex Digits");
                    }
                    else if (localOutput.ToString().Equals("00000000"))
                    {
                        //Ignore the null escape
                    }
                    else
                    {
                        this._output.Append(UnicodeSpecsHelper.ConvertToChar(localOutput.ToString()));
                    }
                    return;

                #endregion

                default:
                    //Not an escape character
                    if (mode != TokeniserEscapeMode.QName)
                    {
                        //Append the \ and then return
                        //Processing continues normally in the caller function
                        this._output.Append("\\");
                        return;
                    }
                    else
                    {
                        throw Error("Unexpected Backslash Character encountered in a QName, the Backslash Character can only be used for Unicode escapes (\\u and \\U) in QNames");
                    }

            }
        }          

        /// <summary>
        /// Determines whether a given Character can be valid as a Hex Digit
        /// </summary>
        /// <param name="c">Character to test</param>
        /// <returns></returns>
        protected bool IsHexDigit(char c)
        {
            if (Char.IsDigit(c))
            {
                return true;
            }
            else
            {
                switch (c)
                {
                    case 'A':
                    case 'a':
                    case 'B':
                    case 'b':
                    case 'C':
                    case 'c':
                    case 'D':
                    case 'd':
                    case 'E':
                    case 'f':
                    case 'F':
                        return true;
                    default:
                        return false;
                }
            }
        }

        /// <summary>
        /// Helper Function for generating Standardised Parser Errors
        /// </summary>
        /// <param name="detail">The Error Message</param>
        /// <returns></returns>
        protected RdfParseException Error(String detail)
        {
            this._reader.Close();
            if (detail.Contains("{0}"))
            {
                return new RdfParseException("[Line " + this._currline + " Column " + this._currpos + "] " + String.Format(detail, this._format), this._currline, this._currpos);
            }
            else
            {
                return new RdfParseException("[Line " + this._currline + " Column " + this._currpos + "] " + detail, this._currline, this._currpos);
            }
        }

        /// <summary>
        /// Helper Function for generating Standardised Parser Errors about unexpected characters
        /// </summary>
        /// <param name="c">Unexpected Character</param>
        /// <param name="expected">Message detailing what was expected (may be empty if no explicit expectation)</param>
        /// <returns></returns>
        protected RdfParseException UnexpectedCharacter(char c, String expected)
        {
            StringBuilder error = new StringBuilder();
            error.Append("Unexpected Character (Code " + (int)c + ") " + (char)c + " was encountered");
            if (!expected.Equals(String.Empty))
            {
                error.Append(", " + expected);
            }
            return Error(error.ToString());
        }

        /// <summary>
        /// Helper Function for generating Standardised Parser Errors about unexpected end of input
        /// </summary>
        /// <param name="expected">Message detailing what was expected (may be empty if no explicit expectation)</param>
        /// <returns></returns>
        protected RdfParseException UnexpectedEndOfInput(String expected)
        {
            StringBuilder error = new StringBuilder();
            error.Append("[Line " + this._startline + " Column " + this._startpos + " to Line " + this._currline + " Column " + this._currpos + "] Unexpected end of input while trying to parse " + expected);
            if (this.Value.Length > 0 && !this.Value.ToCharArray().All(c => Char.IsWhiteSpace(c)))
            {
                error.AppendLine(" from content:");
                error.Append(this.Value);
            }
            return new RdfParseException(error.ToString(), this._startline, this._currline, this._startpos, this._currpos);
        }

        /// <summary>
        /// Helper Function for generating Standardised Parser Errors about unexpected new lines
        /// </summary>
        /// <param name="expected">Message detailing what was expected (may be empty if no explicit expectation)</param>
        /// <returns></returns>
        protected RdfParseException UnexpectedNewLine(String expected)
        {
            StringBuilder error = new StringBuilder();
            error.AppendLine("[Line " + this._startline + " Column " + this._startpos + " to Line " + this._currline + " Column " + this._currpos + "] Unexpected new line while trying to parse " + expected + " from content:");
            if (this.Value.Length > 0 && !this.Value.ToCharArray().All(c => Char.IsWhiteSpace(c)))
            {
                error.AppendLine(" from content:");
                error.Append(this.Value);
            }
            return new RdfParseException(error.ToString(), this._startline, this._currline, this._startpos, this._currpos);
        }

        /// <summary>
        /// Helper Function for generating Standardised Parser Errors about unexpected tokens
        /// </summary>
        /// <param name="expected">Message detailing what was expected (may be empty if no explicity expectation)</param>
        /// <param name="t">Token that was parsed</param>
        /// <returns></returns>
        protected RdfParseException UnexpectedToken(String expected, IToken t)
        {
            StringBuilder error = new StringBuilder();
            error.Append("[Line " + t.StartLine + " Column " + t.StartPosition + " to Line " + t.EndLine + " Column " + t.EndPosition + "] Unexpected Token parsed " + expected);
            return new RdfParseException(error.ToString(), t);
        }
    }
}
