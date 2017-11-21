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
using VDS.RDF.Nodes;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Filters;
using VDS.RDF.Query.Optimisation;

namespace VDS.RDF.Query.Algebra
{
    /// <summary>
    /// Algebra operator which combines a Filter and a Product into a single operation for improved performance and reduced memory usage
    /// </summary>
    public class FilteredProduct
        : IAbstractJoin
    {
        private readonly ISparqlAlgebra _lhs, _rhs;
        private readonly ISparqlExpression _expr;

        /// <summary>
        /// Creates a new Filtered Product
        /// </summary>
        /// <param name="lhs">LHS Algebra</param>
        /// <param name="rhs">RHS Algebra</param>
        /// <param name="expr">Expression to filter with</param>
        public FilteredProduct(ISparqlAlgebra lhs, ISparqlAlgebra rhs, ISparqlExpression expr)
        {
            _lhs = lhs;
            _rhs = rhs;
            _expr = expr;
        }

        /// <summary>
        /// Gets the LHS Algebra
        /// </summary>
        public ISparqlAlgebra Lhs
        {
            get
            {
                return _lhs;
            }
        }

        /// <summary>
        /// Gets the RHS Algebra
        /// </summary>
        public ISparqlAlgebra Rhs
        {
            get 
            {
                return _rhs;
            }
        }

        /// <summary>
        /// Transforms the inner algebra with the given optimiser
        /// </summary>
        /// <param name="optimiser">Algebra Optimiser</param>
        /// <returns></returns>
        public ISparqlAlgebra Transform(IAlgebraOptimiser optimiser)
        {
            if (optimiser is IExpressionTransformer)
            {
                return new FilteredProduct(optimiser.Optimise(_lhs), optimiser.Optimise(_rhs), ((IExpressionTransformer)optimiser).Transform(_expr));
            }
            else
            {
                return new FilteredProduct(optimiser.Optimise(_lhs), optimiser.Optimise(_rhs), _expr);
            }
        }

        /// <summary>
        /// Transforms the LHS algebra only with the given optimiser
        /// </summary>
        /// <param name="optimiser">Algebra Optimiser</param>
        /// <returns></returns>
        public ISparqlAlgebra TransformLhs(IAlgebraOptimiser optimiser)
        {
            return new FilteredProduct(optimiser.Optimise(_lhs), _rhs, _expr);
        }

        /// <summary>
        /// Transforms the RHS algebra only with the given optimiser
        /// </summary>
        /// <param name="optimiser">Algebra Optimiser</param>
        /// <returns></returns>
        public ISparqlAlgebra TransformRhs(IAlgebraOptimiser optimiser)
        {
            return new FilteredProduct(_lhs, optimiser.Optimise(_rhs), _expr);
        }

        /// <summary>
        /// Evaluates the filtered product
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <returns></returns>
        public BaseMultiset Evaluate(SparqlEvaluationContext context)
        {
            BaseMultiset initialInput = context.InputMultiset;
            BaseMultiset lhsResults = context.Evaluate(_lhs);

            if (lhsResults is NullMultiset || lhsResults.IsEmpty)
            {
                // If LHS Results are Null/Empty then end result will always be null so short circuit
                context.OutputMultiset = new NullMultiset();
            }
            else
            {

                context.InputMultiset = initialInput;
                BaseMultiset rhsResults = context.Evaluate(_rhs);
                if (rhsResults is NullMultiset || rhsResults.IsEmpty)
                {
                    // If RHS Results are Null/Empty then end results will always be null so short circuit
                    context.OutputMultiset = new NullMultiset();
                }
                else if (rhsResults is IdentityMultiset)
                {
                    // Apply Filter over LHS Results only - defer evaluation to filter implementation
                    context.InputMultiset = lhsResults;
                    UnaryExpressionFilter filter = new UnaryExpressionFilter(_expr);
                    filter.Evaluate(context);
                    context.OutputMultiset = lhsResults;
                }
                else
                {
                    // Calculate the product applying the filter as we go
#if NET40
                    if (Options.UsePLinqEvaluation && this._expr.CanParallelise)
                    {
                        PartitionedMultiset partitionedSet;
                        SparqlResultBinder binder = context.Binder;
                        if (lhsResults.Count >= rhsResults.Count)
                        {
                            partitionedSet = new PartitionedMultiset(lhsResults.Count, rhsResults.Count);
                            context.Binder = new LeviathanLeftJoinBinder(partitionedSet);
                            lhsResults.Sets.AsParallel().ForAll(x => this.EvalFilteredProduct(context, x, rhsResults, partitionedSet));
                        }
                        else
                        {
                            partitionedSet = new PartitionedMultiset(rhsResults.Count, lhsResults.Count);
                            context.Binder = new LeviathanLeftJoinBinder(partitionedSet);
                            rhsResults.Sets.AsParallel().ForAll(y => this.EvalFilteredProduct(context, y, lhsResults, partitionedSet));
                        }

                        context.Binder = binder;
                        context.OutputMultiset = partitionedSet;
                    }
                    else
                    {
#endif
                        BaseMultiset productSet = new Multiset();
                        SparqlResultBinder binder = context.Binder;
                        context.Binder = new LeviathanLeftJoinBinder(productSet);
                        foreach (ISet x in lhsResults.Sets)
                        {
                            foreach (ISet y in rhsResults.Sets)
                            {
                                ISet z = x.Join(y);
                                productSet.Add(z);
                                try
                                {
                                    if (!_expr.Evaluate(context, z.ID).AsSafeBoolean())
                                    {
                                        // Means the expression evaluates to false so we discard the solution
                                        productSet.Remove(z.ID);
                                    }
                                }
                                catch
                                {
                                    // Means this solution does not meet the FILTER and can be discarded
                                    productSet.Remove(z.ID);
                                }
                            }
                            // Remember to check for timeouts occassionaly
                            context.CheckTimeout();
                        }
                        context.Binder = binder;
                        context.OutputMultiset = productSet;
#if NET40
                    }
#endif
                }
            }
            return context.OutputMultiset;
        }

#if NET40

        private void EvalFilteredProduct(SparqlEvaluationContext context, ISet x, BaseMultiset other, PartitionedMultiset partitionedSet)
        {
            int id = partitionedSet.GetNextBaseID();
            foreach (ISet y in other.Sets)
            {
                id++;
                ISet z = x.Join(y);
                z.ID = id;
                partitionedSet.Add(z);
                try
                {
                    if (!this._expr.Evaluate(context, z.ID).AsSafeBoolean())
                    {
                        // Means the expression evaluates to false so we discard the solution
                        partitionedSet.Remove(z.ID);
                    }
                }
                catch
                {
                    // Means the solution does not meet the FILTER and can be discarded
                    partitionedSet.Remove(z.ID);
                }
            }
            // Remember to check for timeouts occassionally
            context.CheckTimeout();
        }

#endif

        /// <summary>
        /// Gets the Variables used in the Algebra
        /// </summary>
        public IEnumerable<string> Variables
        {
            get
            {
                return _lhs.Variables.Concat(_rhs.Variables).Concat(_expr.Variables).Distinct();
            }
        }

        /// <summary>
        /// Gets the enumeration of floating variables in the algebra i.e. variables that are not guaranteed to have a bound value
        /// </summary>
        public IEnumerable<String> FloatingVariables
        {
            get
            {
                // Floating variables are those floating on either side which are not fixed
                IEnumerable<String> floating = _lhs.FloatingVariables.Concat(_rhs.FloatingVariables).Distinct();
                HashSet<String> fixedVars = new HashSet<string>(FixedVariables);
                return floating.Where(v => !fixedVars.Contains(v));
            }
        }

        /// <summary>
        /// Gets the enumeration of fixed variables in the algebra i.e. variables that are guaranteed to have a bound value
        /// </summary>
        public IEnumerable<String> FixedVariables
        {
            get
            {
                // Fixed variables are those fixed on either side
                return _lhs.FixedVariables.Concat(_rhs.FixedVariables).Distinct();
            }
        }

        /// <summary>
        /// Converts the algebra back into a query
        /// </summary>
        /// <returns></returns>
        public SparqlQuery ToQuery()
        {
            ISparqlAlgebra algebra = new Filter(new Join(_lhs, _rhs), new UnaryExpressionFilter(_expr));
            return algebra.ToQuery();
        }

        /// <summary>
        /// Converts the algebra back into a Graph Pattern
        /// </summary>
        /// <returns></returns>
        public Patterns.GraphPattern ToGraphPattern()
        {
            ISparqlAlgebra algebra = new Filter(new Join(_lhs, _rhs), new UnaryExpressionFilter(_expr));
            return algebra.ToGraphPattern();
        }

        /// <summary>
        /// Gets the string represenation of the algebra
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "FilteredProduct(" + _lhs.ToString() + ", " + _rhs.ToString() + ", " + _expr.ToString() + ")";
        }
    }
}
