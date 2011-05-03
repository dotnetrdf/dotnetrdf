using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Patterns;

namespace sparqlDown
{
    public static class SparqlDownHelper
    {
        public static bool IsSparql10(this ISparqlAlgebra algebra)
        {
            if (algebra is IAbstractJoin)
            {
                if (algebra is IMinus || algebra is IExistsJoin)
                {
                    return false;
                }
                else
                {
                    IAbstractJoin join = (IAbstractJoin)algebra;
                    return join.Lhs.IsSparql10() && join.Rhs.IsSparql10();
                }
            }
            else if (algebra is IBgp)
            {
                IBgp bgp = (IBgp)algebra;
                return bgp.TriplePatterns.All(p => p.IsSparql10());
            }
            else if (algebra is IUnaryOperator)
            {
                if (algebra is Bindings || algebra is GroupBy || algebra is Having)
                {
                    return false;
                }
                else if (algebra is Project)
                {
                    Project p = (Project)algebra;
                    return p.SparqlVariables.All(v => !v.IsAggregate && !v.IsProjection) && p.InnerAlgebra.IsSparql10();
                }
                else if (algebra is Filter)
                {
                    Filter f = (Filter)algebra;
                    return f.SparqlFilter.Expression.IsSparql10() && f.InnerAlgebra.IsSparql10();
                }
                else
                {
                    IUnaryOperator op = (IUnaryOperator)algebra;
                    return op.InnerAlgebra.IsSparql10();
                }
            }
            else
            {
                return false;
            }
        }

        public static bool IsSparql10(this ITriplePattern tp)
        {
            if (tp is LetPattern || tp is BindPattern || tp is PropertyPathPattern || tp is SubQueryPattern)
            {
                return false;
            }
            else if (tp is FilterPattern)
            {
                FilterPattern fp = (FilterPattern)tp;
                return fp.Filter.Expression.IsSparql10();
            }
            else
            {
                return true;
            }
        }

        public static bool IsSparql10(this ISparqlExpression expr)
        {
            switch (expr.Type)
            {
                case SparqlExpressionType.Aggregate:
                case SparqlExpressionType.GraphOperator:
                case SparqlExpressionType.SetOperator:
                    return false;
                case SparqlExpressionType.BinaryOperator:
                case SparqlExpressionType.UnaryOperator:
                    return true;
                case SparqlExpressionType.Primary:
                    if (expr is GraphPatternExpressionTerm)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                case SparqlExpressionType.Function:
                    String functor = expr.Functor;
                    return SparqlSpecsHelper.IsFunctionKeyword(functor) && !SparqlSpecsHelper.IsFunctionKeyword11(functor);
                default:
                    return false;
            }
        }
    }
}
