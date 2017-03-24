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
    /// Interface for defining Token classes to be used in Parsing RDF
    /// </summary>
    public interface IToken
    {
        /// <summary>
        /// Gives some Integer representing the Token Type as understood by a specific Parser implementation
        /// </summary>
        int TokenType
        {
            get;
        }

        /// <summary>
        /// Gives the Value of the Token
        /// </summary>
        String Value
        {
            get;
        }

        /// <summary>
        /// Gives the Line at which the Token starts
        /// </summary>
        int StartLine
        {
            get;
        }

        /// <summary>
        /// Gives the Line at which the Token ends
        /// </summary>
        int EndLine
        {
            get;
        }

        /// <summary>
        /// Gives the Position within the Start Line that the Token starts
        /// </summary>
        int StartPosition
        {
            get;
        }

        /// <summary>
        /// Gives the Position within the End Line that the Token ends
        /// </summary>
        int EndPosition
        {
            get;
        }

        /// <summary>
        /// Gives the Length of the Token
        /// </summary>
        int Length
        {
            get;
        }
    }
}
