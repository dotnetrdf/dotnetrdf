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

namespace VDS.RDF.Parsing.Tokens
{
    /// <summary>
    /// Token Queue Mode Constants
    /// </summary>
    public enum TokenQueueMode : int
    {
        /// <summary>
        /// No Buffering used
        /// </summary>
        QueueAllBeforeParsing = 0,
        /// <summary>
        /// Synchronous Buffering used
        /// </summary>
        SynchronousBufferDuringParsing = 1,
        /// <summary>
        /// Asynchronous Buffering used
        /// </summary>
        AsynchronousBufferDuringParsing = 2
    }

    /// <summary>
    /// Interface for Tokenisers
    /// </summary>
    /// <remarks>
    /// A Tokeniser is a class that takes an input stream and produces textual tokens from it for use in token based parsers
    /// </remarks>
    public interface ITokeniser
    {
        /// <summary>
        /// Causes the Tokeniser to attempt to retrieve the next Token
        /// </summary>
        /// <returns></returns>
        /// <exception cref="RdfParseException">Thrown if a valid Token cannot be parsed</exception>
        /// <exception cref="System.IO.IOException">Thrown if there is a problem reading the Input Stream</exception>
        IToken GetNextToken();
    }
}
