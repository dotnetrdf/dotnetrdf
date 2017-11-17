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

namespace VDS.RDF.JsonLd
{
    /// <summary>
    /// Exception raised by the JSON-LD processor when a processing error is encountered
    /// </summary>
    public class JsonLdProcessorException : Exception
    {
        /// <summary>
        /// The JSON-LD error code that describes the processing error encountered
        /// </summary>
        public JsonLdErrorCode ErrorCode { get; }

        /// <summary>
        /// Create a new exception
        /// </summary>
        /// <param name="errorCode">The JSON-LD error code that describes the processing error encountered</param>
        /// <param name="message">A message providing the user with further contextual information about the error</param>
        public JsonLdProcessorException(JsonLdErrorCode errorCode, string message) : base(message)
        {
            ErrorCode = errorCode;
        }


        /// <summary>
        /// Create a new exception
        /// </summary>
        /// <param name="errorCode">The JSON-LD error code that describes the processing error encountered</param>
        /// <param name="message">A message proiding the user with further contextual information about the error</param>
        /// <param name="innerException">The inner exception that led to this exception being raised</param>
        public JsonLdProcessorException(JsonLdErrorCode errorCode, string message, Exception innerException) : base(
            message, innerException)
        {
            ErrorCode = errorCode;
        }
    }
}