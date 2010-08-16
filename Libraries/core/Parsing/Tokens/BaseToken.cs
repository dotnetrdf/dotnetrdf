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
            this._tokentype = tokenType;
            this._value = value;
            this._startline = startLine;
            this._endline = endLine;
            this._startpos = startPos;
            this._endpos = endPos;
        }

        /// <summary>
        /// Gets an arbitrary integer which indicates the Type of the Token
        /// </summary>
        public int TokenType
        {
            get 
            {
                return this._tokentype;
            }
        }

        /// <summary>
        /// Gets the String Value which this Token represents (if any)
        /// </summary>
        public string Value
        {
            get
            {
                return this._value;
            }
        }

        /// <summary>
        /// Gets the Line at which this Token Starts
        /// </summary>
        public int StartLine
        {
            get 
            {
                return this._startline;
            }
        }

        /// <summary>
        /// Gets the Line at which this Token Ends
        /// </summary>
        public int EndLine
        {
            get 
            {
                return this._endline;
            }
        }

        /// <summary>
        /// Gets the Column at which this Token Starts
        /// </summary>
        public int StartPosition
        {
            get 
            {
                return this._startpos;
            }
        }

        /// <summary>
        /// Gets the Column at which this Token Ends
        /// </summary>
        public int EndPosition
        {
            get 
            {
                return this._endpos; 
            }
        }

        /// <summary>
        /// Gets the Length of the Tokens Value
        /// </summary>
        public int Length
        {
            get
            {
                return this._value.Length;
            }
        }

        /// <summary>
        /// Gets a String representation of the Token Type and Value
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.GetType().ToString() + " " + this._value;
        }

        /// <summary>
        /// Gets a Hash Code for a Token
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }
    }
}
