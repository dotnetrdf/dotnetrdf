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

namespace VDS.RDF.Parsing.Tokens
{
    /// <summary>
    /// Base Implementation of IToken used by all derived tokens for ease of implementation
    /// </summary>
    public abstract class BaseToken : IToken
    {

        /// <summary>
        /// Variables for representing the Type and Position of the Token
        /// </summary>
        protected int _tokentype, _startline, _endline, _startpos, _endpos;
        /// <summary>
        /// Variable containg the value of the Token
        /// </summary>
        protected String _value;

        /// <summary>
        /// Creates a Token and fills in its Values
        /// </summary>
        /// <param name="tokenType">Integer denoting the Tokens Type</param>
        /// <param name="value">String value that the Token represents (if any)</param>
        /// <param name="startLine">Line at which the Token starts</param>
        /// <param name="endLine">Line at which the Token ends</param>
        /// <param name="startPos">Column at which the Token starts</param>
        /// <param name="endPos">Column at which the Token ends</param>
        /// <remarks>All the derived classes use this Constructor to fill in the basic values of a Token</remarks>
        protected internal BaseToken(int tokenType, String value, int startLine, int endLine, int startPos, int endPos)
        {
            _tokentype = tokenType;
            _value = value;
            _startline = startLine;
            _endline = endLine;
            _startpos = startPos;
            _endpos = endPos;
        }

        /// <summary>
        /// Gets an arbitrary integer which indicates the Type of the Token
        /// </summary>
        public int TokenType
        {
            get 
            {
                return _tokentype;
            }
        }

        /// <summary>
        /// Gets the String Value which this Token represents (if any)
        /// </summary>
        public string Value
        {
            get
            {
                return _value;
            }
        }

        /// <summary>
        /// Gets the Line at which this Token Starts
        /// </summary>
        public int StartLine
        {
            get 
            {
                return _startline;
            }
        }

        /// <summary>
        /// Gets the Line at which this Token Ends
        /// </summary>
        public int EndLine
        {
            get 
            {
                return _endline;
            }
        }

        /// <summary>
        /// Gets the Column at which this Token Starts
        /// </summary>
        public int StartPosition
        {
            get 
            {
                return _startpos;
            }
        }

        /// <summary>
        /// Gets the Column at which this Token Ends
        /// </summary>
        public int EndPosition
        {
            get 
            {
                return _endpos; 
            }
        }

        /// <summary>
        /// Gets the Length of the Tokens Value
        /// </summary>
        public int Length
        {
            get
            {
                return _value.Length;
            }
        }

        /// <summary>
        /// Gets a String representation of the Token Type and Value
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return GetType().ToString() + " " + _value;
        }

        /// <summary>
        /// Gets a Hash Code for a Token
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
    }
}
