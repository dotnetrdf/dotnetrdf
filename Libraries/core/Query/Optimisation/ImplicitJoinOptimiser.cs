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
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Functions;
using VDS.RDF.Query.Expressions.Functions.Sparql.Boolean;
using VDS.RDF.Query.Expressions.Comparison;
using VDS.RDF.Query.Expressions.Primary;
using VDS.RDF.Update;

namespace VDS.RDF.Query.Optimisation
{
    /// <summary>
    /// An Algebra Optimiser which implements the Implicit Join optimisation
    /// </summary>
    /// <remarks>
    /// <para>
    /// An implict join is implied by a query like the following:
    /// </para>
    /// <pre>
    /// SELECT *
    /// WHERE
    /// {
    ///   ?x a ?type .
    ///   ?y a ?type .
    ///   FILTER (?x = ?y) .
    /// }
    /// </pre>
    /// <para>
    /// Such queries can be very expensive to calculate, the implict join optimisation attempts to substitute one variable for the other and use a BIND to ensure both variables are visible outside of the graph pattern affected i.e. the resulting query looks like the following:
    /// </para>
    /// <pre>
    /// SELECT *
    /// WHERE
    /// {
    ///   ?x a ?type .
    ///   ?x a ?type .
    ///   BIND (?x AS ?y)
    /// }
    /// </pre>
    /// </remarks>
    public class ImplicitJoinOptimiser
        : IAlgebraOptimiser
    {
        private Type _exprType = typeof(ConstantTerm);

        /// <summary>
        /// Optimises the Algebra to use implict joins where applicable
        /// </summary>
        /// <param name="algebra">Algebra</param>
        /// <returns></returns>
        public ISparqlAlgebra Optimise(ISparqlAlgebra algebra)
        {
            try
            {
                if (algebra is Filter)
                {
                    Filter f = (Filter)algebra;
                    String lhsVar, rhsVar;
                    bool equals;
                    if (this.IsImplicitJoinExpression(f.SparqlFilter.Expression, out lhsVar, out rhsVar, out equals))
                    {
                        //We must ensure that both variables are in scope
                        List<String> vars = f.InnerAlgebra.Variables.ToList();
                        if (vars.Contains(lhsVar) && vars.Contains(rhsVar))
                        {
                            //Try to use the extend style optimization
                            VariableSubstitutionTransformer transformer = new VariableSubstitutionTransformer(rhsVar, lhsVar);
                            if (!equals) transformer.CanReplaceObjects = true;
                            ISparqlAlgebra extAlgebra = transformer.Optimise(f.InnerAlgebra);
                            return new Extend(extAlgebra, new VariableTerm(lhsVar), rhsVar);
                        }
                        else
                        {
                            return f.Transform(this);
                        }
                    }
                    else
                    {
                        return f.Transform(this);
                    }
                }
                else if (algebra is IAbstractJoin)
                {
                    return ((IAbstractJoin)algebra).Transform(this);
                }
                else if (algebra is IUnaryOperator)
                {
                    return ((IUnaryOperator)algebra).Transform(this);
                }
                else
                {
                    return algebra;
                }
            }
            catch
            {
                return algebra;
            }
        }

        /// <summary>
        /// Determines whether an expression is an Implicit Join Expression
        /// </summary>
        /// <param name="expr">Expression</param>
        /// <param name="lhsVar">LHS Variable</param>
        /// <param name="rhsVar">RHS Variable</param>
        /// <param name="equals">Whether the expression is an equals (true) or a same term (false)</param>
        /// <returns></returns>
        private bool IsImplicitJoinExpression(ISparqlExpression expr, out String lhsVar, out String rhsVar, out bool equals)
        {
            lhsVar = null;
            rhsVar = null;
            equals = false;
            ISparqlExpression lhs, rhs;
            if (expr is EqualsExpression)
            {
                EqualsExpression eq = (EqualsExpression)expr;
                lhs = eq.Arguments.First();
                rhs = eq.Arguments.Last();
                equals = true;
            } 
            else if (expr is SameTermFunction)
            {
                SameTermFunction st = (SameTermFunction)expr;
                lhs = st.Arguments.First();
                rhs = st.Arguments.Last();
            }
            else 
            {
                return false;
            }

            if (lhs is VariableTerm && rhs is VariableTerm)
            {
                lhsVar = lhs.Variables.First();
                rhsVar = rhs.Variables.First();
                return !lhsVar.Equals(rhsVar);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Returns that this optimiser is applicable to all queries
        /// </summary>
        /// <param name="q">Query</param>
        /// <returns></returns>
        public bool IsApplicable(SparqlQuery q)
        {
            return true;
        }

        /// <summary>
        /// Returns that this optimiser is applicable to all updates
        /// </summary>
        /// <param name="cmds">Updates</param>
        /// <returns></returns>
        public bool IsApplicable(SparqlUpdateCommandSet cmds)
        {
            return true;
        }
    }
}
