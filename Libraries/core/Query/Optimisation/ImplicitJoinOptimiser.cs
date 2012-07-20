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
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Functions;
using VDS.RDF.Query.Expressions.Functions.Sparql.Boolean;
using VDS.RDF.Query.Expressions.Comparison;
using VDS.RDF.Query.Expressions.Primary;
using VDS.RDF.Query.Patterns;
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
    /// <para>
    /// Under normal circumstances this optimisation is only used when the implict join is denoted by a SAMETERM expression or the optimiser is sure the variables don't represent literals (they never occur in the Object position) since when value equality is involved substituing one variable for another changes the semantics of the query and may lead to unexpected results.  Since this optimisation may offer big performance benefits for some queries (at the cost of potentially incorrect results) this form of the optimisation is allowed when you set <see cref="Options.UnsafeOptimisation"/> to true.
    /// </para>
    /// <para>
    /// This optimiser is also capable of generating special algebra to deal with the case where there is an implicit join but the substitution based optimisation does not apply because variables cannot be substituted into the inner algebra, in this case a <see cref="FilteredProduct"/> is generated instead.
    /// </para>
    /// </remarks>
    public class ImplicitJoinOptimiser
        : IAlgebraOptimiser
    {
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
                            try
                            {
                                //Try to use the extend style optimization
                                VariableSubstitutionTransformer transformer = new VariableSubstitutionTransformer(rhsVar, lhsVar);
                                if (!equals || Options.UnsafeOptimisation) transformer.CanReplaceObjects = true;
                                ISparqlAlgebra extAlgebra = transformer.Optimise(f.InnerAlgebra);
                                return new Extend(extAlgebra, new VariableTerm(lhsVar), rhsVar);
                            }
                            catch
                            {
                                //See if the Filtered Product style optimization applies instead
                                int splitPoint = -1;
                                if (this.IsDisjointOperation(f.InnerAlgebra, lhsVar, rhsVar, out splitPoint))
                                {
                                    if (splitPoint > -1)
                                    {
                                        //Means the inner algebra is a BGP we can split into two parts
                                        IBgp bgp = (IBgp)f.InnerAlgebra;
                                        return new FilteredProduct(new Bgp(bgp.TriplePatterns.Take(splitPoint)), new Bgp(bgp.TriplePatterns.Skip(splitPoint)), f.SparqlFilter.Expression);
                                    }
                                    else
                                    {
                                        //Means that the inner algebra is a Join where the sides are disjoint
                                        IJoin join = (IJoin)f.InnerAlgebra;
                                        return new FilteredProduct(join.Lhs, join.Rhs, f.SparqlFilter.Expression);
                                    }
                                }
                                else
                                {
                                    return f.Transform(this);
                                }
                            }
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

        private bool IsDisjointOperation(ISparqlAlgebra algebra, String lhsVar, String rhsVar, out int splitPoint)
        {
            splitPoint = -1;
            if (algebra is IBgp)
            {
                //Get Triple Patterns, can't split into a product if there are blank variables present
                List<ITriplePattern> ps = ((IBgp)algebra).TriplePatterns.ToList();
                if (ps.Any(p => !p.HasNoBlankVariables)) return false;

                //Iterate over the Triple Patterns to see if we can split into a Product
                List<String> vars = new List<String>();
                for (int i = 0; i < ps.Count; i++)
                {
                    //Not a product if we've seen both variables already
                    if (vars.Contains(lhsVar) && vars.Contains(rhsVar)) return false;

                    ITriplePattern p = ps[i];
                    if (p is TriplePattern || p is SubQueryPattern)
                    {
                        if (vars.Count > 0 && vars.IsDisjoint(p.Variables))
                        {
                            //May be a filterable product if we've seen only one variable so far and have hit a point where a product occurs
                            if (vars.Contains(lhsVar) && !vars.Contains(rhsVar))
                            {
                                Bgp rhs = new Bgp(ps.Skip(i));
                                if (rhs.Variables.Contains(rhsVar))
                                {
                                    splitPoint = i;
                                    return true;
                                }
                            }
                            else if (vars.Contains(rhsVar) && !vars.Contains(lhsVar))
                            {
                                Bgp rhs = new Bgp(ps.Skip(i));
                                if (rhs.Variables.Contains(lhsVar))
                                {
                                    splitPoint = i;
                                    return true;
                                }
                            }
                        }
                        vars.AddRange(p.Variables);
                    }
                    else if (p is IAssignmentPattern)
                    {
                        vars.Add(((IAssignmentPattern)p).VariableName);
                    }
                    else if (p is FilterPattern)
                    {
                        continue;
                    }
                    else
                    {
                        return false;
                    }
                }
                //If we get all the way here then not a product
                return false;
            }
            else if (algebra is IJoin)
            {
                IJoin join = (IJoin)algebra;
                if (join.Lhs.Variables.IsDisjoint(join.Rhs.Variables))
                {
                    //There a product between the two sides of the join but are the two variables on different sides of that join
                    return !(join.Lhs.Variables.Contains(lhsVar) && join.Lhs.Variables.Contains(rhsVar)) && !(join.Rhs.Variables.Contains(lhsVar) && join.Rhs.Variables.Contains(rhsVar));
                }
                else
                {
                    return false;
                }
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
