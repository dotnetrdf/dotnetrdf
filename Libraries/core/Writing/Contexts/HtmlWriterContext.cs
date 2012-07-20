/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
#if !NO_WEB
using System.Web.UI;
#endif
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
            this._writer = new HtmlTextWriter(writer);
            //Have to remove the Empty Prefix since this is reserved in (X)HTML+RDFa for the (X)HTML namespace
            this._qnameMapper.RemoveNamespace(String.Empty);

            this._uriFormatter = new HtmlFormatter();
        }

        /// <summary>
        /// HTML Writer to use
        /// </summary>
        public HtmlTextWriter HtmlWriter
        {
            get
            {
                return this._writer;
            }
        }
    }
}
