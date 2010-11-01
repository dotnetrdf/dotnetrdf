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


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SemWeb;

namespace VDS.RDF.Interop.SemWeb
{
    /// <summary>
    /// An Enumeration Sink is a StatementSink used for the purposes of returning results as Triples
    /// </summary>
    class EnumerationSink : StatementSink, IEnumerable<Triple>
    {
        private SemWebMapping _mapping;
        private List<Triple> _triples = new List<Triple>();

        /// <summary>
        /// Creates a new Enumeration Sink
        /// </summary>
        /// <param name="mapping">Mapping of Blank Nodes</param>
        public EnumerationSink(SemWebMapping mapping)
        {
            this._mapping = mapping;
        }

        /// <summary>
        /// Adds a Statement to the Sink
        /// </summary>
        /// <param name="statement">Statement</param>
        /// <returns></returns>
        public bool Add(Statement statement)
        {
            Triple t = SemWebConverter.FromSemWeb(statement, this._mapping);
            this._triples.Add(t);
            return true;
        }

        /// <summary>
        /// Gets the enumeration of Triples
        /// </summary>
        /// <returns></returns>
        public IEnumerator<Triple> GetEnumerator()
        {
            return this._triples.GetEnumerator();
        }

        /// <summary>
        /// Gets the enumeration of Triples
        /// </summary>
        /// <returns></returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this._triples.GetEnumerator();
        }
    }
}
