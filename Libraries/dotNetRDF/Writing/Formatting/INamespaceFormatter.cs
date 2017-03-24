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

namespace VDS.RDF.Writing.Formatting
{
    /// <summary>
    /// Interface for Formatters which can format Namespace Information
    /// </summary>
    public interface INamespaceFormatter
    {
        /// <summary>
        /// Formats Namespace Information as a String
        /// </summary>
        /// <param name="prefix">Namespae Prefix</param>
        /// <param name="namespaceUri">Namespace URI</param>
        /// <returns></returns>
        String FormatNamespace(String prefix, Uri namespaceUri);
    }

    /// <summary>
    /// Interface for Formatters which can format Base URI Information
    /// </summary>
    public interface IBaseUriFormatter
    {
        /// <summary>
        /// Formats Base URI Information as a String
        /// </summary>
        /// <param name="u">Base URI</param>
        /// <returns></returns>
        String FormatBaseUri(Uri u);
    }
}
