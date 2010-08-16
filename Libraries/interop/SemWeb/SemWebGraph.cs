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

If this license is not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/


using SemWeb;

namespace VDS.RDF.Interop.SemWeb
{
    /// <summary>
    /// Provides a SemWeb StatementSource and StatementSink as a Graph to dotNetRDF
    /// </summary>
    public class SemWebGraph : Graph
    {
        /// <summary>
        /// Creates a new SemWeb Graph which is a dotNetRDF wrapper around a SemWeb StatementSource and StatementSink
        /// </summary>
        /// <param name="sink">Sink</param>
        /// <param name="source">Source</param>
        public SemWebGraph(StatementSink sink, StatementSource source)
        {
            this._triples = new SemWebTripleCollection(this, sink, source);
        }
    }
}
