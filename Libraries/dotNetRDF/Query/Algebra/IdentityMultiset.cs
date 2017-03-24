/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Query.Algebra
{
    /// <summary>
    /// Represents the Identity Multiset
    /// </summary>
    public class IdentityMultiset 
        : BaseMultiset
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
            // If Other is Null/Empty then the Join still results in Identity
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
            // If Other is Null/Empty then the Join still results in Identity
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
            // Identity is always disjoint with Minus so return Identity
            return this;
        }

        /// <summary>
        /// Generates the Product of this Set and another Multiset
        /// </summary>
        /// <param name="other">Other Multiset</param>
        /// <returns>The other Multiset</returns>
        public override BaseMultiset Product(BaseMultiset other)
        {
            // If Other is Null/Empty then the Join still results in Identity
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
            // If Other is Null/Empty then the Join still results in Identity
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
        /// Sets the variable ordering for the multiset
        /// </summary>
        /// <param name="variables">Variable Ordering</param>
        public override void SetVariableOrder(IEnumerable<string> variables)
        {
            if (variables.Any()) throw new RdfQueryException("Cannot set variable order for the Identify Multiset");
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
