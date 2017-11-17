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
using System.IO;
using System.Web.UI;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Writing.Contexts
{
    /// <summary>
    /// Writer Context for XHTML+RDFa Writers
    /// </summary>
    public class HtmlWriterContext : BaseWriterContext
    {
        private HtmlTextWriter _writer;

        /// <summary>
        /// Creates a new HTML Writer Context
        /// </summary>
        /// <param name="g">Graph</param>
        /// <param name="writer">Text Writer</param>
        public HtmlWriterContext(IGraph g, TextWriter writer)
            : base(g, writer) 
        {
            _writer = new HtmlTextWriter(writer);
            // Have to remove the Empty Prefix since this is reserved in (X)HTML+RDFa for the (X)HTML namespace
            _qnameMapper.RemoveNamespace(String.Empty);

            _uriFormatter = new HtmlFormatter();
        }

        /// <summary>
        /// HTML Writer to use
        /// </summary>
        public HtmlTextWriter HtmlWriter
        {
            get
            {
                return _writer;
            }
        }
    }
}
