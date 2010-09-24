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
using VDS.RDF.Query.Filters;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Algebra
{
    /// <summary>
    /// Represents a LeftJoin predicated on the existence/non-existence of joinable sets on the RHS for each item on the LHS
    /// </summary>
    public class ExistsJoin : ISparqlAlgebra
    {
        private ISparqlAlgebra _lhs, _rhs;
        private bool _mustExist;

        /// <summary>
        /// Creates a new Exists Join
        /// </summary>
        /// <param name="lhs">LHS Pattern</param>
        /// <param name="rhs">RHS Pattern</param>
        /// <param name="mustExist">Whether a joinable set must exist on the RHS for the LHS set to be preserved</param>
        public ExistsJoin(ISparqlAlgebra lhs, ISparqlAlgebra rhs, bool mustExist)
        {
            this._lhs = lhs;
            this._rhs = rhs;
            this._mustExist = mustExist;
        }

        /// <summary>
        /// Evaluates an ExistsJoin
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <returns></returns>
        public BaseMultiset Evaluate(SparqlEvaluationContext context)
        {
            BaseMultiset initialInput = context.InputMultiset;
            BaseMultiset lhsResult = this._lhs.Evaluate(context);
            context.CheckTimeout();

            if (lhsResult is NullMultiset)
            {
                context.OutputMultiset = lhsResult;
            }
            else if (lhsResult.IsEmpty)
            {
                context.OutputMultiset = new NullMultiset();
            }
            else
            {
                //Only execute the RHS if the LHS had results
                context.InputMultiset = lhsResult;
                BaseMultiset rhsResult = this._rhs.Evaluate(context);
                context.CheckTimeout();

                context.OutputMultiset = lhsResult.ExistsJoin(rhsResult, this._mustExist);
                context.CheckTimeout();
            }

            context.InputMultiset = context.OutputMultiset;
            return context.OutputMultiset;
        }

        /// <summary>
        /// Gets the Variables used in the Algebra
        /// </summary>
        public IEnumerable<String> Variables
        {
            get
            {
                return (this._lhs.Variables.Concat(this._rhs.Variables)).Distinct();
            }
        }

        /// <summary>
        /// Gets the String representation of the Algebra
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "ExistsJoin(" + this._lhs.ToString() + ", " + this._rhs.ToString() + ", " + this._mustExist + ")";
        }
    }

    /// <summary>
    /// Represents a LeftJoin predicated on an arbitrary filter expression
    /// </summary>
    public class LeftJoin : ISparqlAlgebra
    {
        private ISparqlAlgebra _lhs, _rhs;
        private ISparqlFilter _filter = new UnaryExpressionFilter(new BooleanExpressionTerm(true));

        /// <summary>
        /// Creates a new LeftJoin where there is no Filter over the join
        /// </summary>
        /// <param name="lhs">LHS Pattern</param>
        /// <param name="rhs">RHS Pattern</param>
        public LeftJoin(ISparqlAlgebra lhs, ISparqlAlgebra rhs)
        {
            this._lhs = lhs;
            this._rhs = rhs;
        }

        /// <summary>
        /// Creates a new LeftJoin where there is a Filter over the join
        /// </summary>
        /// <param name="lhs">LHS Pattern</param>
        /// <param name="rhs">RHS Pattern</param>
        /// <param name="filter">Filter to decide which RHS solutions are valid</param>
        public LeftJoin(ISparqlAlgebra lhs, ISparqlAlgebra rhs, ISparqlFilter filter)
        {
            this._lhs = lhs;
            this._rhs = rhs;
            this._filter = filter;
        }

        /// <summary>
        /// Evaluates the LeftJoin
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <returns></returns>
        public BaseMultiset Evaluate(SparqlEvaluationContext context)
        {
            BaseMultiset initialInput = context.InputMultiset;
            BaseMultiset lhsResult = this._lhs.Evaluate(context);
            context.CheckTimeout();

            if (lhsResult is NullMultiset)
            {
                context.OutputMultiset = lhsResult;
            }
            else if (lhsResult.IsEmpty)
            {
                context.OutputMultiset = new NullMultiset();
            }
            else
            {
                //Only execute the RHS if the LHS had some results
                context.InputMultiset = lhsResult;
                BaseMultiset rhsResult = this._rhs.Evaluate(context);
                context.CheckTimeout();

                context.OutputMultiset = lhsResult.LeftJoin(rhsResult, this._filter.Expression);
                context.CheckTimeout();
            }

            context.InputMultiset = context.OutputMultiset;
            return context.OutputMultiset;
        }

        /// <summary>
        /// Gets the Variables used in the Algebra
        /// </summary>
        public IEnumerable<String> Variables
        {
            get
            {
                return (this._lhs.Variables.Concat(this._rhs.Variables)).Distinct();
            }
        }

        /// <summary>
        /// Gets the String representation of the Algebra
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            String filter = this._filter.ToString();
            filter = filter.Substring(7, filter.Length - 8);
            return "LeftJoin(" + this._lhs.ToString() + ", " + this._rhs.ToString() + ", " + filter + ")";
        }
    }

    /// <summary>
    /// Represents a Join
    /// </summary>
    public class Join : ISparqlAlgebra
    {
        private ISparqlAlgebra _lhs, _rhs;

        /// <summary>
        /// Creates a new Join
        /// </summary>
        /// <param name="lhs">Left Hand Side</param>
        /// <param name="rhs">Right Hand Side</param>
        public Join(ISparqlAlgebra lhs, ISparqlAlgebra rhs)
        {
            this._lhs = lhs;
            this._rhs = rhs;
        }

        /// <summary>
        /// Creates either a Join or returns just one of the sides of the Join if one side is the empty BGP
        /// </summary>
        /// <param name="lhs">Left Hand Side</param>
        /// <param name="rhs">Right Hand Side</param>
        /// <returns></returns>
        public static ISparqlAlgebra CreateJoin(ISparqlAlgebra lhs, ISparqlAlgebra rhs)
        {
            if (lhs is Bgp)
            {
                if (((Bgp)lhs).IsEmpty)
                {
                    return rhs;
                }
                else if (rhs is Bgp)
                {
                    if (((Bgp)rhs).IsEmpty)
                    {
                        return lhs;
                    }
                    else
                    {
                        return new Join(lhs, rhs);
                    }
                }
                else
                {
                    return new Join(lhs, rhs);
                }
            }
            else if (rhs is Bgp)
            {
                if (((Bgp)rhs).IsEmpty)
                {
                    return lhs;
                }
                else
                {
                    return new Join(lhs, rhs);
                }
            }
            else
            {
                return new Join(lhs, rhs);
            }
        }

        /// <summary>
        /// Evalutes a Join
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <returns></returns>
        public BaseMultiset Evaluate(SparqlEvaluationContext context)
        {
            BaseMultiset initialInput = context.InputMultiset;
            BaseMultiset lhsResult = this._lhs.Evaluate(context);
            context.CheckTimeout();

            if (lhsResult is NullMultiset)
            {
                context.OutputMultiset = lhsResult;
            }
            else if (lhsResult.IsEmpty)
            {
                context.OutputMultiset = new NullMultiset();
            }
            else
            {
                //Only Execute the RHS if the LHS has some results
                context.InputMultiset = lhsResult;
                BaseMultiset rhsResult = this._rhs.Evaluate(context);
                context.CheckTimeout();

                context.OutputMultiset = lhsResult.Join(rhsResult);
                context.CheckTimeout();
            }

            context.InputMultiset = context.OutputMultiset;
            return context.OutputMultiset;
        }

        /// <summary>
        /// Gets the Variables used in the Algebra
        /// </summary>
        public IEnumerable<String> Variables
        {
            get
            {
                return (this._lhs.Variables.Concat(this._rhs.Variables)).Distinct();
            }
        }

        /// <summary>
        /// Gets the String representation of the Join
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "Join(" + this._lhs.ToString() + ", " + this._rhs.ToString() + ")";
        }
    }

    /// <summary>
    /// Represents a Union
    /// </summary>
    public class Union : ISparqlAlgebra
    {
        private ISparqlAlgebra _lhs, _rhs;

        /// <summary>
        /// Creates a new Union
        /// </summary>
        /// <param name="lhs">LHS Pattern</param>
        /// <param name="rhs">RHS Pattern</param>
        public Union(ISparqlAlgebra lhs, ISparqlAlgebra rhs)
        {
            this._lhs = lhs;
            this._rhs = rhs;
        }

        /// <summary>
        /// Evaluates the Union
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public BaseMultiset Evaluate(SparqlEvaluationContext context)
        {
            BaseMultiset initialInput = context.InputMultiset;
            BaseMultiset lhsResult = this._lhs.Evaluate(context);
            context.CheckTimeout();

            context.InputMultiset = initialInput;
            BaseMultiset rhsResult = this._rhs.Evaluate(context);
            context.CheckTimeout();

            context.OutputMultiset = lhsResult.Union(rhsResult);
            context.CheckTimeout();

            context.InputMultiset = context.OutputMultiset;
            return context.OutputMultiset;
        }

        /// <summary>
        /// Gets the Variables used in the Algebra
        /// </summary>
        public IEnumerable<String> Variables
        {
            get
            {
                return (this._lhs.Variables.Concat(this._rhs.Variables)).Distinct();
            }
        }

        /// <summary>
        /// Gets the String representation of the Algebra
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "Union(" + this._lhs.ToString() + ", " + this._rhs.ToString() + ")";
        }
    }
}
