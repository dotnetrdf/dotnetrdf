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
    /// Represents the Identity Multiset
    /// </summary>
    public class IdentityMultiset : BaseMultiset
    {
        /// <summary>
        /// Joins the Multiset to another Multiset
        /// </summary>
        /// <param name="other">Other Multiset</param>
        /// <returns>
        /// The other Multiset
        /// </returns>
        public override BaseMultiset Join(BaseMultiset other)
        {
            //If Other is Null/Empty then the Join still results in Identity
            if (other is NullMultiset) return this;
            if (other.IsEmpty) return this;
            return other;
        }

        /// <summary>
        /// Left Joins the Multiset to another Multiset
        /// </summary>
        /// <param name="other">Other Multiset</param>
        /// <param name="expr">Expression which the Join is predicated on</param>
        /// <returns>The other Multiset</returns>
        public override BaseMultiset LeftJoin(BaseMultiset other, ISparqlExpression expr)
        {
            //If Other is Null/Empty then the Join still results in Identity
            if (other is NullMultiset) return this;
            if (other.IsEmpty) return this;
            return other;
        }

        /// <summary>
        /// Exists Joins the Multiset to another Multiset
        /// </summary>
        /// <param name="other">Other Multiset</param>
        /// <param name="mustExist">Whether solutions must exist in the Other Multiset for the Join to suceed</param>
        /// <returns></returns>
        public override BaseMultiset ExistsJoin(BaseMultiset other, bool mustExist)
        {
            if (mustExist)
            {
                if (other is NullMultiset) return other;
                return this;
            }
            else
            {
                if (other is NullMultiset) return this;
                if (other is IdentityMultiset) return new NullMultiset();
                return this;
            }
        }

        /// <summary>
        /// Minus Joins this Multiset to another Multiset
        /// </summary>
        /// <param name="other">Other Multiset</param>
        /// <returns></returns>
        public override BaseMultiset MinusJoin(BaseMultiset other)
        {
            //Identity is always disjoint with Minus so return Identity
            return this;
        }

        /// <summary>
        /// Generates the Product of this Set and another Multiset
        /// </summary>
        /// <param name="other">Other Multiset</param>
        /// <returns>The other Multiset</returns>
        public override BaseMultiset Product(BaseMultiset other)
        {
            //If Other is Null/Empty then the Join still results in Identity
            if (other is NullMultiset) return this;
            if (other.IsEmpty) return this;
            return other;
        }

        /// <summary>
        /// Generates the Union of this Set and another Multiset
        /// </summary>
        /// <param name="other">Other Multiset</param>
        /// <returns>The other Multiset</returns>
        public override BaseMultiset Union(BaseMultiset other)
        {
            //If Other is Null/Empty then the Join still results in Identity
            if (other is NullMultiset) return this;
            if (other.IsEmpty) return this;
            return other;
        }

        /// <summary>
        /// Returns True since the Identity Multiset is considered to contain all values
        /// </summary>
        /// <param name="var">Variable</param>
        /// <param name="n">Value</param>
        /// <returns></returns>
        public override bool ContainsValue(String var, INode n)
        {
            return true;
        }

        /// <summary>
        /// Returns False since the Identity Multiset contains no Variables
        /// </summary>
        /// <param name="var">Variable</param>
        /// <returns></returns>
        public override bool ContainsVariable(string var)
        {
            return false;
        }

        /// <summary>
        /// Returns False since the Identity Multiset is not disjoint with anything
        /// </summary>
        /// <param name="other">Other Multiset</param>
        /// <returns></returns>
        public override bool IsDisjointWith(BaseMultiset other)
        {
            return false;
        }

        /// <summary>
        /// Adds a Set to the Multiset
        /// </summary>
        /// <param name="s">Set</param>
        /// <exception cref="RdfQueryException">Thrown since this operation is invalid on an Identity Multiset</exception>
        public override void Add(ISet s)
        {
            throw new RdfQueryException("Cannot add a Set to the Identity Multiset");
        }

        /// <summary>
        /// Adds a Variable to the Multiset
        /// </summary>
        /// <param name="variable">Variable</param>
        /// <exception cref="RdfQueryException">Thrown since this operation is invalid on an Identity Multiset</exception>
        public override void AddVariable(string variable)
        {
            throw new RdfQueryException("Cannot add a Variable to the Identity Multiset");
        }

        /// <summary>
        /// Removes a Set to the Multiset
        /// </summary>
        /// <param name="id">Set ID</param>
        /// <exception cref="RdfQueryException">Thrown since this operation is invalid on an Identity Multiset</exception>
        public override void Remove(int id)
        {
            throw new RdfQueryException("Cannot remove a Set from the Identity Mutliset");
        }

        /// <summary>
        /// Returns false as the Identity Multiset is not considered empty
        /// </summary>
        public override bool IsEmpty
        {
            get 
            {
                return false;
            }
        }

        /// <summary>
        /// Returns an empty enumerable as the Identity Multiset contains no Variables
        /// </summary>
        public override IEnumerable<string> Variables
        {
            get 
            {
                return Enumerable.Empty<String>();
            }
        }

        /// <summary>
        /// Returns an empty enumerable as the Identity Multiset contains no Sets
        /// </summary>
        public override IEnumerable<ISet> Sets
        {
            get 
            {
                return Enumerable.Empty<ISet>(); 
            }
        }

        /// <summary>
        /// Returns an empty enumerable as the Identity Multiset contains no Sets
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
        /// <exception cref="RdfQueryException">Thrown since the Identity Multiset contains no Sets</exception>
        public override ISet this[int index]
        {
            get 
            {
                throw new RdfQueryException("Cannot retrieve a Set from the Identity Multiset");
            }
        }
    }
}
