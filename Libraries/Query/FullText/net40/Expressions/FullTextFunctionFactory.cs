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

namespace VDS.RDF.Query.Expressions
{
    /// <summary>
    /// A SPARQL Expression Factory reserved as a future extension point but not currently compiled or used
    /// </summary>
    public class FullTextFunctionFactory
        : ISparqlCustomExpressionFactory
    {

        /// <summary>
        /// Tries to create an expression
        /// </summary>
        /// <param name="u">Function URI</param>
        /// <param name="args">Arguments</param>
        /// <param name="scalarArguments">Scalar Arguments</param>
        /// <param name="expr">Resulting SPARQL Expression</param>
        /// <returns>True if a SPARQL Expression could be created, False otherwise</returns>
        public bool TryCreateExpression(Uri u, List<ISparqlExpression> args, Dictionary<string, ISparqlExpression> scalarArguments, out ISparqlExpression expr)
        {
            //TODO: Add support for FullTextMatchFunction and FullTextSearchFunction

            //switch (u.ToString())
            //{

            //}

            expr = null;
            return false;
        }

        /// <summary>
        /// Gets the URIs of available extension functions
        /// </summary>
        public IEnumerable<Uri> AvailableExtensionFunctions
        {
            get 
            {
                return Enumerable.Empty<Uri>(); 
            }
        }

        /// <summary>
        /// Gets the URIs of available extension aggregates
        /// </summary>
        public IEnumerable<Uri> AvailableExtensionAggregates
        {
            get 
            {
                return Enumerable.Empty<Uri>();
            }
        }
    }
}
