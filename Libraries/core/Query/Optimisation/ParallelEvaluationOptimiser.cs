using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query.Algebra;

namespace VDS.RDF.Query.Optimisation
{
    public class ParallelEvaluationOptimiser : IAlgebraOptimiser
    {

        public ISparqlAlgebra Optimise(ISparqlAlgebra algebra)
        {
            if (algebra is IAbstractJoin)
            {
                if (algebra is Join)
                {
                    Join join = (Join)algebra;
                    if (join.Lhs.Variables.IsDisjoint(join.Rhs.Variables))
                    {
                        return new ParallelJoin(this.Optimise(join.Lhs), this.Optimise(join.Rhs));
                    }
                    else
                    {
                        return join.Transform(this);
                    }
                }
                else if (algebra is Union)
                {
                    Union u = (Union)algebra;
                    return new ParallelUnion(this.Optimise(u.Lhs), this.Optimise(u.Rhs));
                }
                else
                {
                    return ((IAbstractJoin)algebra).Transform(this);
                }
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

        public bool IsApplicable(SparqlQuery q)
        {
            return true;
        }
    }
}
