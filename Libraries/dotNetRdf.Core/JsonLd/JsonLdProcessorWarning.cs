/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2025 dotNetRDF Project (http://dotnetrdf.org/)
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

namespace VDS.RDF.JsonLd;

/// <summary>
/// Represents a warning message raised during JSON-LD processing.
/// </summary>
public class JsonLdProcessorWarning
{
    /// <summary>
    /// The error code associated with the warning.
    /// </summary>
    public JsonLdErrorCode ErrorCode { get; }

    /// <summary>
    /// The detailed warning message.
    /// </summary>
    public string Message { get;}

    /// <summary>
    /// Create a new warning message with the specified code and message.
    /// </summary>
    /// <param name="errorCode"></param>
    /// <param name="message"></param>
    public JsonLdProcessorWarning(JsonLdErrorCode errorCode, string message)
    {
        ErrorCode = errorCode;
        Message = message;
    }

    /// <summary>
    /// Get a string representation of the warning.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"{ErrorCode}: {Message}";
    }
}