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
using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Query.Algebra
{
    /// <summary>
    /// Represents a Multiset when there are no possible Solutions
    /// </summary>
    public class NullMultiset : BaseMultiset
    {
        /// <summary>
        /// Joins another Multiset to this Null Mutliset
        /// </summary>
        /// <param name="other">Other Multiset</param>
        /// <returns>
        /// Results in this Null Multiset since Null joined to anything is Null
        /// </returns>
        public override BaseMultiset Join(BaseMultiset other)
        {
            //Left Join results in Null Multiset
            return this;
        }

        /// <summary>
        /// Left Joins another Multiset to this Null Mutliset
        /// </summary>
        /// <param name="other">Other Multiset</param>
        /// <param name="expr">Expression the join is predicate upon</param>
        /// <returns>
        /// Results in this Null Multiset since Null joined to anything is Null
        /// </returns>
        public override BaseMultiset LeftJoin(BaseMultiset other, ISparqlExpression expr)
        {
            //Left Outer Join results in Null Multiset
            return this;
        }

        /// <summary>
        /// Exists Joins another Multiset to this Null Mutliset
        /// </summary>
        /// <param name="other">Other Multiset</param>
        /// <param name="mustExist">Whether joinable solutions must exist in the other Multiset for joins to be made</param>
        /// <returns>
        /// Results in this Null Multiset since Null joined to anything is Null
        /// </returns>
        public override BaseMultiset ExistsJoin(BaseMultiset other, bool mustExist)
        {
            return this;
        }

        /// <summary>
        /// Minus Joins this Multiset to another Multiset
        /// </summary>
        /// <param name="other">Other Multiset</param>
        /// <returns></returns>
        public override BaseMultiset MinusJoin(BaseMultiset other)
        {
            return this;
        }

        /// <summary>
        /// Computes the Product of this Multiset and another Multiset
        /// </summary>
        /// <param name="other">Other Multiset</param>
        /// <returns>
        /// Results in the Other Multiset since for Product we consider this Multiset to contain a single empty Set
        /// </returns>
        public override BaseMultiset Product(BaseMultiset other)
        {
            //Join results in Other Multiset
            return other;
        }

        /// <summary>
        /// Unions this Multiset with another Multiset
        /// </summary>
        /// <param name="other">Other Multiset</param>
        /// <returns>
        /// Results in the Other Multiset as this is an empty Multiset
        /// </returns>
        public override BaseMultiset Union(BaseMultiset other)
        {
            //Union results in Other Multiset
            return other;
        }

        /// <summary>
        /// Returns false since the Null Multiset contains no values
        /// </summary>
        /// <param name="var">Variable</param>
        /// <param name="n">Value</param>
        /// <returns></returns>
        public override bool ContainsValue(String var, INode n)
        {
            return false;
        }

        /// <summary>
        /// Returns false since the Null Multiset contains no variables
        /// </summary>
        /// <param name="var">Variable</param>
        /// <returns></returns>
        public override bool ContainsVariable(string var)
        {
            return false;
        }

        /// <summary>
        /// Returns true since the Null Multiset is disjoint with all Multisets
        /// </summary>
        /// <param name="other">Other Multiset</param>
        /// <returns></returns>
        public override bool IsDisjointWith(BaseMultiset other)
        {
            return true;
        }

        /// <summary>
        /// Adds a Set to this Multiset
        /// </summary>
        /// <param name="s">Set</param>
        /// <exception cref="RdfQueryException">Thrown since the operation is invalid on a Null Multiset</exception>
        public override void Add(ISet s)
        {
            throw new RdfQueryException("Cannot add a Set to the Null Multiset");
        }

        /// <summary>
        /// Adds a Variable to this Multiset
        /// </summary>
        /// <param name="variable">Variable</param>
        /// <exception cref="RdfQueryException">Thrown since the operation is invalid on a Null Multiset</exception>
        public override void AddVariable(string variable)
        {
            throw new RdfQueryException("Cannot add a Variable to the Null Multiset");
        }

        /// <summary>
        /// Removes a Set from a Multiset
        /// </summary>
        /// <param name="id">Set ID</param>
        /// <exception cref="RdfQueryException">Thrown since the operation is invalid on a Null Multiset</exception>
        public override void Remove(int id)
        {
            throw new RdfQueryException("Cannot remove a Set from the Null Multiset");
        }

        /// <summary>
        /// Returns true since the Null Multiset is always empty
        /// </summary>
        public override bool IsEmpty
        {
            get 
            {
                return true;
            }
        }

        /// <summary>
        /// Returns an empty enumerable as the Null Multiset contains no Variables
        /// </summary>
        public override IEnumerable<string> Variables
        {
            get 
            {
                return Enumerable.Empty<String>();
            }
        }

        /// <summary>
        /// Returns an empty enumerable as the Null Multiset contains no Sets
        /// </summary>
        public override IEnumerable<ISet> Sets
        {
            get 
            {
                return Enumerable.Empty<ISet>();
            }
        }

        /// <summary>
        /// Returns an empty enumerable as the Null Multiset contains no Sets
        /// </summary>
        public override IEnumerable<int> SetIDs
        {
            get 
            {
                return Enumerable.Empty<int>();
            }
        }

        /// <summary>
        /// Gets the Set with the given ID
        /// </summary>
        /// <param name="index">Set ID</param>
        /// <returns></returns>
        /// <exception cref="RdfQueryException">Thrown since the Null Multiset contains no Sets</exception>
        public override ISet this[int index]
        {
            get 
            {
                throw new RdfQueryException("Cannot retrieve a Set from the Null Multiset");
            }
        }
    }
}
