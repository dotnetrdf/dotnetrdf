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

using Newtonsoft.Json.Linq;
using System;

namespace VDS.RDF.JsonLd
{
    /// <summary>
    /// Base clss for exceptions raised when a validation error is found in a value object
    /// </summary>
    public class InvalidValueObjectException : JsonLdProcessorException
    {
        /// <summary>
        /// The value object that the validation error applies to
        /// </summary>
        public JToken InvalidValueObject { get; private set; }

        /// <summary>
        /// Create a new validation exception for a value object
        /// </summary>
        /// <param name="message">The exception message</param>
        /// <param name="valueObject">The value object found to be invalid</param>
        public InvalidValueObjectException(string message, JToken valueObject) : base(message)
        {
            InvalidValueObject = valueObject;
        }
    }
}