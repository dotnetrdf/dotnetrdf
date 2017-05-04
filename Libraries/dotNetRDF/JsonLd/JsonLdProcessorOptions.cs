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
using System.Collections.Generic;
using System.Text;

namespace VDS.RDF.JsonLd
{
    /// <summary>
    /// A collection of options for setting up the JSON-LD processor
    /// </summary>
    public class JsonLdProcessorOptions
    {
        /// <summary>
        /// Overrides the base IRI of the document being processed
        /// </summary>
        public Uri Base { get; set; }

        /// <summary>
        /// Get or set the function to use to resolve an IRI reference to a document
        /// into parsed JSON.
        /// </summary>
        /// <remarks>If the function returns null or throws an exception, it will be assumed that dereferencing the IRI has failed</remarks>
        public Func<Uri, JToken> Loader { get; set; }

        /// <summary>
        /// Get or set the syntax version that the processor will use
        /// </summary>
        public JsonLdSyntax Syntax = JsonLdSyntax.JsonLd11;

    }
}
