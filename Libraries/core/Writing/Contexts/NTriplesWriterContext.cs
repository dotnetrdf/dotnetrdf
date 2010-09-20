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

using System.IO;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Writing.Contexts
{
    /// <summary>
    /// Writer Context for NTriples Writers
    /// </summary>
    public class NTriplesWriterContext : BaseWriterContext
    {
        /// <summary>
        /// Creates a new NTriples Writer Context with default settings
        /// </summary>
        /// <param name="g">Graph to write</param>
        /// <param name="output">TextWriter to write to</param>
        public NTriplesWriterContext(IGraph g, TextWriter output)
            : base(g, output) 
        {
            this._formatter = new NTriplesFormatter();
            this._uriFormatter = (IUriFormatter)this._formatter;
        }

        /// <summary>
        /// Creates a new NTriples Writer Context with custom settings
        /// </summary>
        /// <param name="g">Graph to write</param>
        /// <param name="output">TextWriter to write to</param>
        /// <param name="prettyPrint">Pretty Print Mode</param>
        /// <param name="hiSpeed">High Speed Mode</param>
        public NTriplesWriterContext(IGraph g, TextWriter output, bool prettyPrint, bool hiSpeed)
            : base(g, output, WriterCompressionLevel.Default, prettyPrint, hiSpeed) 
        {
            this._formatter = new NTriplesFormatter();
            this._uriFormatter = (IUriFormatter)this._formatter;
        }
    }
}
