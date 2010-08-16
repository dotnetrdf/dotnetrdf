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
using VDS.RDF.Query.Patterns;
using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Query
{
    /// <summary>
    /// Represents an Group of Bindings which is used when executing Queries with GROUP BY clauses
    /// </summary>
    public class BindingGroup : IEnumerable<int>
    {
        private List<int> _bindingIDs = new List<int>();

        /// <summary>
        /// Adds a Binding ID to the Group
        /// </summary>
        /// <param name="id"></param>
        public void Add(int id)
        {
            this._bindingIDs.Add(id);
        }

        /// <summary>
        /// Gets the Enumerator for the Binding IDs in the Group
        /// </summary>
        /// <returns></returns>
        public IEnumerator<int> GetEnumerator()
        {
            return this._bindingIDs.GetEnumerator();
        }

        /// <summary>
        /// Gets the Enumerator for the Binding IDs in the Group
        /// </summary>
        /// <returns></returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this._bindingIDs.GetEnumerator();
        }

        /// <summary>
        /// Gets the Binding IDs in the Group
        /// </summary>
        public IEnumerable<int> BindingIDs
        {
            get
            {
                return (from id in this._bindingIDs
                        select id);
            }
        }
    }
}