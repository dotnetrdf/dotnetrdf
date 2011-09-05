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
using VDS.RDF.Query;
using VDS.RDF.Query.Algebra;

namespace VDS.RDF.Parsing.Handlers
{
    /// <summary>
    /// A SPARQL Results Handler which loads directly into a <see cref="Multiset">Multiset</see>
    /// </summary>
    /// <remarks>
    /// Primarily intended for internal usage for future optimisation of some SPARQL evaluation
    /// </remarks>
    public class MultisetHandler 
        : BaseResultsHandler
    {
        private Multiset _mset;

        /// <summary>
        /// Creates a new Multiset Handler
        /// </summary>
        /// <param name="mset">Multiset</param>
        public MultisetHandler(Multiset mset)
        {
            if (mset == null) throw new ArgumentNullException("mset", "Multiset to load into cannot be null");
            this._mset = mset;
        }

        /// <summary>
        /// Handles a Boolean Result by doing nothing
        /// </summary>
        /// <param name="result">Boolean Result</param>
        protected override void HandleBooleanResultInternal(bool result)
        {
            //Does Nothing
        }

        /// <summary>
        /// Handles a Variable by adding it to the Multiset
        /// </summary>
        /// <param name="var">Variable</param>
        /// <returns></returns>
        protected override bool HandleVariableInternal(string var)
        {
            this._mset.AddVariable(var);
            return true;
        }

        /// <summary>
        /// Handles a Result by adding it to the Multiset
        /// </summary>
        /// <param name="result">Result</param>
        /// <returns></returns>
        protected override bool HandleResultInternal(SparqlResult result)
        {
            this._mset.Add(new Set(result));
            return true;
        }
    }
}
