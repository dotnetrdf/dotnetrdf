using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Query.Optimisation
{
    public class IdentityFilterOptimiser : IAlgebraOptimiser
    {
        private Type _exprType = typeof(NodeExpressionTerm);

        public ISparqlAlgebra Optimise(ISparqlAlgebra algebra)
        {
            try
            {
                if (algebra is Filter)
                {
                    Filter f = (Filter)algebra;
                    String var;
                    INode term;
                    if (this.IsIdentityExpression(f.SparqlFilter.Expression, out var, out term))
                    {
                        return new IdentityFilter(this.Optimise(f.InnerAlgebra), var, new NodeExpressionTerm(term));
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

        private bool IsIdentityExpression(ISparqlExpression expr, out String var, out INode term)
        {
            var = null;
            term = null;
            if (expr is EqualsExpression)
            {
                EqualsExpression eq = (EqualsExpression)expr;
                ISparqlExpression lhs = eq.Arguments.First();
                ISparqlExpression rhs = eq.Arguments.Last();

                if (lhs is VariableExpressionTerm)
                {
                    if (rhs.GetType().Equals(this._exprType))
                    {
                        var = lhs.Variables.First();
                        term = rhs.Value(null, 0);
                        return true;
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

        public bool IsApplicable(SparqlQuery q)
        {
            return true;
        }
    }
}
