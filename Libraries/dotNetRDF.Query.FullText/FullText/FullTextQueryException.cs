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
using System.Linq;
using System.Text;

namespace VDS.RDF.Query.FullText
{
    /// <summary>
    /// Exception Type for exceptions that may occur during Full Text Query
    /// </summary>
    public class FullTextQueryException
        : RdfQueryException
    {
        /// <summary>
        /// Creates a new Full Text Query Exception
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="cause">Inner Exception</param>
        public FullTextQueryException(String message, Exception cause)
            : base(message, cause) { }

        /// <summary>
        /// Creates a new Full Text Query Exception
        /// </summary>
        /// <param name="message">Message</param>
        public FullTextQueryException(String message)
            : base(message) { }
    }

    /// <summary>
    /// Exception Type for exceptions that may occur during Full Text Indexing
    /// </summary>
    public class FullTextIndexException
        : RdfQueryException
    {
        /// <summary>
        /// Creates a new Full Text Index Exception
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="cause">Inner Exception</param>
        public FullTextIndexException(String message, Exception cause)
            : base(message, cause) { }

        /// <summary>
        /// Creates a new Full Text Index Exception
        /// </summary>
        /// <param name="message">Message</param>
        public FullTextIndexException(String message)
            : base(message) { }
    }
}
