/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2023 dotNetRDF Project (http://dotnetrdf.org/)
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

using System.Collections.Generic;

namespace VDS.RDF.Writing.Formatting
{
    /// <summary>
    /// Interface for formatters designed to format entire SPARQL Result Sets.
    /// </summary>
    public interface IResultSetFormatter : IResultFormatter
    {
        /// <summary>
        /// Generates a header section using the given variables.
        /// </summary>
        /// <param name="variables">Variables.</param>
        /// <returns></returns>
        string FormatResultSetHeader(IEnumerable<string> variables);

        /// <summary>
        /// Generates a header section assuming no variables.
        /// </summary>
        /// <returns></returns>
        string FormatResultSetHeader();

        /// <summary>
        /// Generates a footer section.
        /// </summary>
        /// <returns></returns>
        string FormatResultSetFooter();
    }
}
