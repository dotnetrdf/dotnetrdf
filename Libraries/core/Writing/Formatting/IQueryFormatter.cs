/*

Copyright Robert Vesse 2009-11
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
using VDS.RDF.Query;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Writing.Formatting
{

    /// <summary>
    /// Interface for classes which can format SPARQL Queries into Strings
    /// </summary>
    public interface IQueryFormatter : INodeFormatter, IUriFormatter
    {
        /// <summary>
        /// Formats a SPARQL Query into a String
        /// </summary>
        /// <param name="query">SPARQL Query</param>
        /// <returns></returns>
        String Format(SparqlQuery query);

        /// <summary>
        /// Formats a Graph Pattern into a String
        /// </summary>
        /// <param name="gp">Graph Pattern</param>
        /// <returns></returns>
        String Format(GraphPattern gp);

        /// <summary>
        /// Formats a Triple Pattern into a String
        /// </summary>
        /// <param name="tp">Triple Pattern</param>
        /// <returns></returns>
        String Format(ITriplePattern tp);

        /// <summary>
        /// Formats a Triple Pattern item into a String
        /// </summary>
        /// <param name="item">Pattern Item</param>
        /// <param name="segment">Segment of the Triple Pattern in which the Item appears</param>
        /// <returns></returns>
        String Format(PatternItem item, TripleSegment? segment);
    }
}
