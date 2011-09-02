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
using System.Linq;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Functions;
using VDS.RDF.Update;

namespace VDS.RDF.Query.Optimisation
{
    /// <summary>
    /// An Algebra Optimiser which implements the Identity Filter optimisation
    /// </summary>
    public class IdentityFilterOptimiser
        : IAlgebraOptimiser
    {
        private Type _exprType = typeof(NodeExpressionTerm);

        /// <summary>
        /// Optimises the Algebra to use Identity Filters where applicable
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
                    String var;
                    INode term;
                    bool equals = false;
                    if (this.IsIdentityExpression(f.SparqlFilter.Expression, out var, out term, out equals))
                    {
                        if (equals)
                        {
                            return new IdentityFilter(this.Optimise(f.InnerAlgebra), var, new NodeExpressionTerm(term));
                        }
                        else
                        {
                            return new SameTermFilter(this.Optimise(f.InnerAlgebra), var, new NodeExpressionTerm(term));
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
        /// Determines whether an expression is an Identity Expression
        /// </summary>
        /// <param name="expr">Expression</param>
        /// <param name="var">Variable</param>
        /// <param name="term">Term</param>
        /// <param name="equals">Whether it is an equals expression (true) or a same term expression (false)</param>
        /// <returns></returns>
        private bool IsIdentityExpression(ISparqlExpression expr, out String var, out INode term, out bool equals)
        {
            var = null;
            term = null;
            equals = false;
            ISparqlExpression lhs, rhs;
            if (expr is EqualsExpression)
            {
                equals = true;
                EqualsExpression eq = (EqualsExpression)expr;
                lhs = eq.Arguments.First();
                rhs = eq.Arguments.Last();
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

            if (lhs is VariableExpressionTerm)
            {
                if (rhs.GetType().Equals(this._exprType))
                {
                    var = lhs.Variables.First();
                    term = rhs.Value(null, 0);
                    if (term.NodeType == NodeType.Uri)
                    {
                        return true;
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
            else if (lhs.GetType().Equals(this._exprType))
            {
                if (rhs is VariableExpressionTerm)
                {
                    var = rhs.Variables.First();
                    term = lhs.Value(null, 0);
                    if (term.NodeType == NodeType.Uri)
                    {
                        return true;
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
