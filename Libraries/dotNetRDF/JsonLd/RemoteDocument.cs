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
    /// Class used to return information about a remote document or context
    /// </summary>
    public class RemoteDocument
    {
        /// <summary>
        /// If available, the value of the HTTP Link Header [RFC5988] using the http://www.w3.org/ns/json-ld#context link relation in the response. 
        /// </summary>
        /// <remarks>If the response's content type is application/ld+json, the HTTP Link Header is ignored. 
        /// If multiple HTTP Link Headers using the http://www.w3.org/ns/json-ld#context link relation are found, 
        /// the Promise of the LoadDocumentCallback is rejected with a JsonLdError whose code is set to multiple context link headers.</remarks>
        public Uri ContextUrl { get; set; }

        /// <summary>
        /// The final URL of the loaded document. This is important to handle HTTP redirects properly.
        /// </summary>
        public Uri DocumentUrl { get; set; }

        /// <summary>
        /// The retrieved document. This can either be the raw payload or the already parsed document.
        /// </summary>
        /// <remarks>This property may be a JToken or a string. If it is a string, the string is parsed to a JToken</remarks>
        public object Document { get; set; }
    }
}
