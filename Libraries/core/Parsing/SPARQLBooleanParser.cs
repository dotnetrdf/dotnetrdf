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

namespace VDS.RDF.Parsing
{
    /// <summary>
    /// Parser for SPARQL Boolean results as Plain Text
    /// </summary>
    public class SparqlBooleanParser : ISparqlResultsReader
    {
        /// <summary>
        /// Loads a Result Set from an Input Stream
        /// </summary>
        /// <param name="results">Result Set to load into</param>
        /// <param name="input">Input Stream to read from</param>
        public void Load(SparqlResultSet results, StreamReader input)
        {
            //Ensure Empty Result Set
            if (!results.IsEmpty)
            {
                throw new RdfParseException("Cannot load a Result Set from a Stream into a non-empty Result Set");
            }

            try
            {
                String data = input.ReadToEnd();
                bool result;
                if (Boolean.TryParse(data.Trim(), out result))
                {
                    results.SetResult(result);
                    results.SetEmpty(false);
                }
                else
                {
                    throw new RdfParseException("The input was not a single boolean value as a String");
                }
            }
            catch
            {
                try
                {
                    input.Close();
                }
                catch
                {
                    //No Catch just cleaning up
                }
                throw;
            }
        }

        /// <summary>
        /// Loads a Result Set from an Input Stream
        /// </summary>
        /// <param name="results">Result Set to load into</param>
        /// <param name="filename">File to read from</param>
        public void Load(SparqlResultSet results, string filename)
        {
            this.Load(results, new StreamReader(filename));
        }
    }
}
