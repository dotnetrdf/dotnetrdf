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
using VDS.RDF.Nodes;

namespace VDS.RDF.Query.Expressions.Functions.Sparql.String
{
    /// <summary>
    /// Represents the SPARQL UUID Function
    /// </summary>
    public class UUIDFunction
        : BaseUUIDFunction
    {
        /// <summary>
        /// Evaluates the function by generating the URN UUID form based on the given UUID
        /// </summary>
        /// <param name="uuid">UUID</param>
        /// <returns></returns>
        protected override IValuedNode EvaluateInternal(Guid uuid)
        {
            return new UriNode(null, new Uri("urn:uuid:" + uuid.ToString()));
        }

        /// <summary>
        /// Gets the functor for the expression
        /// </summary>
        public override string Functor
        {
            get 
            {
                return SparqlSpecsHelper.SparqlKeywordUUID;
            }
        }
    }

    /// <summary>
    /// Represents the SPARQL STRUUID Function
    /// </summary>
    public class StrUUIDFunction
        : BaseUUIDFunction
    {
        /// <summary>
        /// Evaluates the function by returning the string form of the given UUID
        /// </summary>
        /// <param name="uuid">UUID</param>
        /// <returns></returns>
        protected override IValuedNode EvaluateInternal(Guid uuid)
        {
            return new StringNode(null, uuid.ToString());
        }

        /// <summary>
        /// Gets the functor for the expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordStrUUID;
            }
        }
    }
}
