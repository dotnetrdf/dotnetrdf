using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Filters;
using VDS.RDF.Query.Optimisation;

namespace VDS.RDF.Query.Algebra
{
    public class FilteredProduct
        : IAbstractJoin
    {
        private ISparqlAlgebra _lhs, _rhs;
        private ISparqlExpression _expr;

        public FilteredProduct(ISparqlAlgebra lhs, ISparqlAlgebra rhs, ISparqlExpression expr)
        {
            this._lhs = lhs;
            this._rhs = rhs;
            this._expr = expr;
        }

        public ISparqlAlgebra Lhs
        {
            get
            {
                return this._lhs;
            }
        }

        public ISparqlAlgebra Rhs
        {
            get 
            {
                return this._rhs;
            }
        }

        public ISparqlAlgebra Transform(IAlgebraOptimiser optimiser)
        {
            if (optimiser is IExpressionTransformer)
            {
                return new FilteredProduct(optimiser.Optimise(this._lhs), optimiser.Optimise(this._rhs), ((IExpressionTransformer)optimiser).Transform(this._expr));
            }
            else
            {
                return new FilteredProduct(optimiser.Optimise(this._lhs), optimiser.Optimise(this._rhs), this._expr);
            }
        }

        public ISparqlAlgebra TransformLhs(IAlgebraOptimiser optimiser)
        {
            return new FilteredProduct(optimiser.Optimise(this._lhs), this._rhs, this._expr);
        }

        public ISparqlAlgebra TransformRhs(Optimisation.IAlgebraOptimiser optimiser)
        {
            return new FilteredProduct(this._lhs, optimiser.Optimise(this._rhs), this._expr);
        }

        public BaseMultiset Evaluate(SparqlEvaluationContext context)
        {
            BaseMultiset initialInput = context.InputMultiset;
            BaseMultiset lhsResults = context.Evaluate(this._lhs);

            if (lhsResults is NullMultiset || lhsResults.IsEmpty)
            {
                //If LHS Results are Null/Empty then end result will always be null so short circuit
                context.OutputMultiset = new NullMultiset();
            }
            else
            {

                context.InputMultiset = initialInput;
                BaseMultiset rhsResults = context.Evaluate(this._rhs);
                if (rhsResults is NullMultiset || rhsResults.IsEmpty)
                {
                    //If RHS Results are Null/Empty then end results will always be null so short circuit
                    context.OutputMultiset = new NullMultiset();
                }
                else if (rhsResults is IdentityMultiset)
                {
                    //Apply Filter over LHS Results only - defer evaluation to filter implementation
                    context.InputMultiset = lhsResults;
                    UnaryExpressionFilter filter = new UnaryExpressionFilter(this._expr);
                    filter.Evaluate(context);
                    context.OutputMultiset = lhsResults;
                }
                else
                {
                    //Calculate the product applying the filter as we go
#if NET40 && !SILVERLIGHT
                    //if (Options.UsePLinqEvaluation)
                    //{
                        
                    //}
                    //else
                    //{
#endif
                        BaseMultiset productSet = new Multiset();
                        foreach (ISet x in lhsResults.Sets)
                        {
                            foreach (ISet y in rhsResults.Sets)
                            {
                                ISet z = x.Join(y);
                                productSet.Add(z);
                                try
                                {
                                    if (!this._expr.Evaluate(context, z.ID).AsSafeBoolean())
                                    {
                                        //Means the expression evaluates to false so we discard the solution
                                        productSet.Remove(z.ID);
                                    }
                                }
                                catch
                                {
                                    //Means this solution does not meet the FILTER and can be discarded
                                    productSet.Remove(z.ID);
                                }
                            }
#if NET40 && !SILVERLIGHT
                        //}
#endif
                        context.OutputMultiset = productSet;
                    }
                }
            }
            return context.OutputMultiset;
        }

        //private void EvalFilteredProduct(SparqlEvaluationContext context, 

        public IEnumerable<string> Variables
        {
            get
            {
                return this._lhs.Variables.Concat(this._rhs.Variables).Distinct();
            }
        }

        public SparqlQuery ToQuery()
        {
            ISparqlAlgebra algebra = new Filter(new Join(this._lhs, this._rhs), new UnaryExpressionFilter(this._expr));
            return algebra.ToQuery();
        }

        public Patterns.GraphPattern ToGraphPattern()
        {
            ISparqlAlgebra algebra = new Filter(new Join(this._lhs, this._rhs), new UnaryExpressionFilter(this._expr));
            return algebra.ToGraphPattern();
        }

        public override string ToString()
        {
            return "FilteredProduct(" + this._lhs.ToString() + ", " + this._rhs.ToString() + ", " + this._expr.ToString() + ")";
        }
    }
}
