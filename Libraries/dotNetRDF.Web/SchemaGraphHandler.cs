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
using VDS.RDF.Writing;

namespace VDS.RDF.Web
{
    /// <summary>
    /// HTTP Handler for serving Graphs in ASP.Net applications
    /// </summary>
    /// <remarks>
    /// <para>
    /// Configured exactly in the same way as <see cref="GraphHandler">GraphHandler</see> - only difference in functionality is that if the requested Content Type (based on the Accept: header) is HTML then the <see cref="HtmlSchemaWriter">HtmlSchemaWriter</see> will be used to provide a human readable schema document rather than the standard <see cref="HtmlWriter">HtmlWriter</see> which justs creates a table of Triples.  Remember though that this means that the HTML output will not contain embedded RDFa as the <see cref="HtmlSchemaWriter">HtmlSchemaWriter</see> does not embed any as opposed to the standard <see cref="HtmlWriter">HtmlWriter</see> which does.
    /// </para>
    /// </remarks>
    public class SchemaGraphHandler 
        : GraphHandler
    {
        private Type _htmlWriter = typeof(HtmlWriter);

        /// <summary>
        /// Overrides writer Selection to use the <see cref="HtmlSchemaWriter">HtmlSchemaWriter</see> whenever the <see cref="HtmlWriter">HtmlWriter</see> would normally have been used
        /// </summary>
        /// <param name="definition">MIME Type Definition selected based on the Requests Accept header</param>
        /// <returns></returns>
        protected override IRdfWriter SelectWriter(MimeTypeDefinition definition)
        {
            if (_htmlWriter.Equals(definition.RdfWriterType))
            {
                return new HtmlSchemaWriter();
            }
            else
            {
                return base.SelectWriter(definition);
            }
        }
    }
}
