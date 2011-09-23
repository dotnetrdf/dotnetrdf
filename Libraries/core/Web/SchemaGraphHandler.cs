/*

Copyright Robert Vesse 2009-10
rvesse@vdesign-studios.com

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

#if !NO_ASP && !NO_WEB

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

#endif
