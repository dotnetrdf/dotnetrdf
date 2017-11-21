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
using System.Linq;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Comparison;
using VDS.RDF.Query.Expressions.Functions.Sparql.Boolean;
using VDS.RDF.Query.Expressions.Primary;
using VDS.RDF.Update;

namespace VDS.RDF.Query.Optimisation
{
    /// <summary>
    /// An Algebra Optimiser which implements the Identity Filter optimisation
    /// </summary>
    public class IdentityFilterOptimiser
        : IAlgebraOptimiser
    {
        private Type _exprType = typeof(ConstantTerm);

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
                    if (IsIdentityExpression(f.SparqlFilter.Expression, out var, out term, out equals))
                    {
                        try
                        {
                            // Try to use the extend style optimization
                            VariableSubstitutionTransformer transformer = new VariableSubstitutionTransformer(var, term);
                            ISparqlAlgebra extAlgebra = transformer.Optimise(f.InnerAlgebra);
                            return new Extend(extAlgebra, new ConstantTerm(term), var);
                        }
                        catch
                        {
                            // Fall back to simpler Identity Filter
                            if (equals)
                            {
                                return new IdentityFilter(Optimise(f.InnerAlgebra), var, new ConstantTerm(term));
                            }
                            else
                            {
                                return new SameTermFilter(Optimise(f.InnerAlgebra), var, new ConstantTerm(term));
                            }
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

            if (lhs is VariableTerm)
            {
                if (rhs.GetType().Equals(_exprType))
                {
                    var = lhs.Variables.First();
                    term = rhs.Evaluate(null, 0);
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
            else if (lhs.GetType().Equals(_exprType))
            {
                if (rhs is VariableTerm)
                {
                    var = rhs.Variables.First();
                    term = lhs.Evaluate(null, 0);
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
