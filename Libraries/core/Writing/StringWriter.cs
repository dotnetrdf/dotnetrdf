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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using VDS.RDF.Query;
using VDS.RDF.Storage.Params;

namespace VDS.RDF.Writing
{
    /// <summary>
    /// Static Helper class for the writing of RDF Graphs and SPARQL Result Sets to Strings rather than Streams/Files
    /// </summary>
    public static class StringWriter
    {
        /// <summary>
        /// Writes the Graph to a String and returns the output in your chosen concrete RDF Syntax
        /// </summary>
        /// <param name="g">Graph to save</param>
        /// <param name="writer">Writer to use to generate the concrete RDF Syntax</param>
        /// <returns></returns>
        /// <remarks>
        /// Since the API allows for any <see cref="TextWriter">TextWriter</see> to be passed to the <see cref="IRdfWriter.Save">Save()</see> method of a <see cref="IRdfWriter">IRdfWriter</see> you can just pass in a <see cref="StringWriter">StringWriter</see> to the Save() method to get the output as a String.  This method simply provides a wrapper to doing just that.
        /// </remarks>
        public static String Write(IGraph g, IRdfWriter writer)
        {
            System.IO.StringWriter sw = new System.IO.StringWriter();
            writer.Save(g, sw);

            return sw.ToString();
        }

        /// <summary>
        /// Writes the given Triple Store to a String and returns the output in your chosen concrete RDF dataset syntax
        /// </summary>
        /// <param name="store">Triple Store</param>
        /// <param name="writer">Writer to use to generate conrete RDF Syntax</param>
        /// <returns></returns>
        public static String Write(ITripleStore store, IStoreWriter writer)
        {
            System.IO.StringWriter sw = new System.IO.StringWriter();
            writer.Save(store, new TextWriterParams(sw));

            return sw.ToString();
        }

        /// <summary>
        /// Writes the SPARQL Result Set to a String and returns the Output in your chosen format
        /// </summary>
        /// <param name="results">SPARQL Result Set</param>
        /// <param name="writer">Writer to use to generate the SPARQL Results output</param>
        /// <returns></returns>
        public static String Write(SparqlResultSet results, ISparqlResultsWriter writer)
        {
            System.IO.StringWriter sw = new System.IO.StringWriter();
            writer.Save(results, sw);

            return sw.ToString();
        }
    }
}
