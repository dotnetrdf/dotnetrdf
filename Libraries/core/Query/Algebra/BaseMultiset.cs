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
    /// Abstract Base Class for representing Multisets
    /// </summary>
    public abstract class BaseMultiset
    {
        /// <summary>
        /// Join combines two multisets that are not disjoint
        /// </summary>
        /// <param name="other">Multiset to join with</param>
        /// <returns></returns>
        public abstract BaseMultiset Join(BaseMultiset other);

        /// <summary>
        /// Left Join combines two multisets where the join is predicated on an arbitrary expression
        /// </summary>
        /// <param name="other">Multiset to join with</param>
        /// <param name="expr">Expression on which the Join is predicated</param>
        /// <returns></returns>
        /// <remarks>
        /// Used for doing OPTIONALs
        /// </remarks>
        public abstract BaseMultiset LeftJoin(BaseMultiset other, ISparqlExpression expr);

        /// <summary>
        /// Exists Join is the equivalent of Left Join where the Join is predicated on the existence/non-existince of an appropriate join candidate in the other multiset
        /// </summary>
        /// <param name="other">Multiset to join with</param>
        /// <param name="mustExist">Whether a valid join candidate must exist in the other multiset for sets from this multiset to be kept</param>
        /// <returns></returns>
        public abstract BaseMultiset ExistsJoin(BaseMultiset other, bool mustExist);

        /// <summary>
        /// Minus Join is a special type of Join which only preserves sets from this Multiset which cannot be joined to the other Multiset
        /// </summary>
        /// <param name="other">Multiset to join with</param>
        /// <returns></returns>
        public abstract BaseMultiset MinusJoin(BaseMultiset other);

        /// <summary>
        /// Product combines two multisets that are disjoint
        /// </summary>
        /// <param name="other">Multiset to join with</param>
        /// <returns></returns>
        public abstract BaseMultiset Product(BaseMultiset other);

        /// <summary>
        /// Union combines two concatenates two mutlisets
        /// </summary>
        /// <param name="other">Multiset to concatenate with</param>
        /// <returns></returns>
        public abstract BaseMultiset Union(BaseMultiset other);

        /// <summary>
        /// Determines whether the Multiset contains the given Value for the given Variable
        /// </summary>
        /// <param name="var">Variable</param>
        /// <param name="n">Value</param>
        /// <returns></returns>
        public abstract bool ContainsValue(String var, INode n);

        /// <summary>
        /// Determines whether the Multiset contains the given Variable
        /// </summary>
        /// <param name="var">Variable</param>
        /// <returns></returns>
        public abstract bool ContainsVariable(String var);

        /// <summary>
        /// Determines whether the Mutliset is disjoint with the given Multiset
        /// </summary>
        /// <param name="other">Multiset</param>
        /// <returns></returns>
        public abstract bool IsDisjointWith(BaseMultiset other);

        /// <summary>
        /// Adds a Set to the Mutliset
        /// </summary>
        /// <param name="s">Set to add</param>
        public abstract void Add(ISet s);

        /// <summary>
        /// Adds a Variable to the Multiset
        /// </summary>
        /// <param name="variable">Variable</param>
        public abstract void AddVariable(String variable);

        /// <summary>
        /// Removes a Set (by ID) from the Multiset
        /// </summary>
        /// <param name="id">ID</param>
        public abstract void Remove(int id);

        /// <summary>
        /// Sorts the Multiset
        /// </summary>
        /// <param name="comparer"></param>
        public virtual void Sort(IComparer<ISet> comparer)
        {
            //Sorting does nothing by default
        }

        /// <summary>
        /// Returns whether the Multiset is Empty
        /// </summary>
        public abstract bool IsEmpty
        {
            get;
        }

        /// <summary>
        /// Gets the Count of Sets in the Multiset
        /// </summary>
        public virtual int Count
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        /// Trims the Multiset of Temporary Variables
        /// </summary>
        public virtual void Trim()
        {
            //Does nothing by default
        }

        /// <summary>
        /// Trims the Multiset by removing all Values for the given Variable
        /// </summary>
        /// <param name="variable">Variable</param>
        public virtual void Trim(String variable)
        {

        }

        /// <summary>
        /// Gets the Variables in the Multiset
        /// </summary>
        public abstract IEnumerable<String> Variables
        {
            get;
        }

        /// <summary>
        /// Gets the Sets in the Multiset
        /// </summary>
        public abstract IEnumerable<ISet> Sets
        {
            get;
        }

        /// <summary>
        /// Gets the IDs of Sets in the Multiset
        /// </summary>
        public abstract IEnumerable<int> SetIDs
        {
            get;
        }

        /// <summary>
        /// Retrieves the Set with the given ID
        /// </summary>
        /// <param name="id">ID</param>
        /// <returns></returns>
        public abstract ISet this[int id]
        {
            get;
        }
    }
}
