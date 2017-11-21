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
using VDS.RDF.Query.Expressions.Primary;
using VDS.RDF.Query.Filters;
using VDS.RDF.Query.Optimisation;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Algebra
{
    /// <summary>
    /// Represents a LeftJoin predicated on the existence/non-existence of joinable sets on the RHS for each item on the LHS
    /// </summary>
    public class ExistsJoin 
        : IExistsJoin
    {
        private readonly ISparqlAlgebra _lhs, _rhs;
        private readonly bool _mustExist;

        /// <summary>
        /// Creates a new Exists Join
        /// </summary>
        /// <param name="lhs">LHS Pattern</param>
        /// <param name="rhs">RHS Pattern</param>
        /// <param name="mustExist">Whether a joinable set must exist on the RHS for the LHS set to be preserved</param>
        public ExistsJoin(ISparqlAlgebra lhs, ISparqlAlgebra rhs, bool mustExist)
        {
            _lhs = lhs;
            _rhs = rhs;
            _mustExist = mustExist;
        }

        /// <summary>
        /// Evaluates an ExistsJoin
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <returns></returns>
        public BaseMultiset Evaluate(SparqlEvaluationContext context)
        {
            BaseMultiset initialInput = context.InputMultiset;
            BaseMultiset lhsResult = context.Evaluate(_lhs);//this._lhs.Evaluate(context);
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
                // Only execute the RHS if the LHS had results
                context.InputMultiset = lhsResult;
                BaseMultiset rhsResult = context.Evaluate(_rhs);//this._rhs.Evaluate(context);
                context.CheckTimeout();

                context.OutputMultiset = lhsResult.ExistsJoin(rhsResult, _mustExist);
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
                return (_lhs.Variables.Concat(_rhs.Variables)).Distinct();
            }
        }

        /// <summary>
        /// Gets the enumeration of floating variables in the algebra i.e. variables that are not guaranteed to have a bound value
        /// </summary>
        public IEnumerable<String> FloatingVariables
        {
            get { return _lhs.FloatingVariables; }
        }

        /// <summary>
        /// Gets the enumeration of fixed variables in the algebra i.e. variables that are guaranteed to have a bound value
        /// </summary>
        public IEnumerable<String> FixedVariables
        {
            get { return _lhs.FixedVariables; }
        }

        /// <summary>
        /// Gets the LHS of the Join
        /// </summary>
        public ISparqlAlgebra Lhs
        {
            get
            {
                return _lhs;
            }
        }

        /// <summary>
        /// Gets the RHS of the Join
        /// </summary>
        public ISparqlAlgebra Rhs
        {
            get
            {
                return _rhs;
            }
        }

        /// <summary>
        /// Gets whether this is an EXISTS join
        /// </summary>
        public bool MustExist
        {
            get
            {
                return _mustExist;
            }
        }

        /// <summary>
        /// Gets the String representation of the Algebra
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "ExistsJoin(" + _lhs.ToString() + ", " + _rhs.ToString() + ", " + _mustExist + ")";
        }

        /// <summary>
        /// Converts the Algebra back to a SPARQL Query
        /// </summary>
        /// <returns></returns>
        public SparqlQuery ToQuery()
        {
            SparqlQuery q = new SparqlQuery();
            q.RootGraphPattern = ToGraphPattern();
            q.Optimise();
            return q;
        }

        /// <summary>
        /// Converts the Algebra back to a Graph Pattern
        /// </summary>
        /// <returns></returns>
        public GraphPattern ToGraphPattern()
        {
            GraphPattern p = _lhs.ToGraphPattern();
            GraphPattern opt = _rhs.ToGraphPattern();
            if (_mustExist)
            {
                opt.IsExists = true;
            }
            else
            {
                opt.IsNotExists = true;
            }
            p.AddGraphPattern(opt);
            return p;
        }

        /// <summary>
        /// Transforms both sides of the Join using the given Optimiser
        /// </summary>
        /// <param name="optimiser">Optimser</param>
        /// <returns></returns>
        public ISparqlAlgebra Transform(IAlgebraOptimiser optimiser)
        {
            return new ExistsJoin(optimiser.Optimise(_lhs), optimiser.Optimise(_rhs), _mustExist);
        }

        /// <summary>
        /// Transforms the LHS of the Join using the given Optimiser
        /// </summary>
        /// <param name="optimiser">Optimser</param>
        /// <returns></returns>
        public ISparqlAlgebra TransformLhs(IAlgebraOptimiser optimiser)
        {
            return new ExistsJoin(optimiser.Optimise(_lhs), _rhs, _mustExist);
        }

        /// <summary>
        /// Transforms the RHS of the Join using the given Optimiser
        /// </summary>
        /// <param name="optimiser">Optimser</param>
        /// <returns></returns>
        public ISparqlAlgebra TransformRhs(IAlgebraOptimiser optimiser)
        {
            return new ExistsJoin(_lhs, optimiser.Optimise(_rhs), _mustExist);
        }
    }

    /// <summary>
    /// Represents a LeftJoin predicated on an arbitrary filter expression
    /// </summary>
    public class LeftJoin 
        : ILeftJoin
    {
        private readonly ISparqlAlgebra _lhs, _rhs;
        private readonly ISparqlFilter _filter = new UnaryExpressionFilter(new ConstantTerm(new BooleanNode(null, true)));

        /// <summary>
        /// Creates a new LeftJoin where there is no Filter over the join
        /// </summary>
        /// <param name="lhs">LHS Pattern</param>
        /// <param name="rhs">RHS Pattern</param>
        public LeftJoin(ISparqlAlgebra lhs, ISparqlAlgebra rhs)
        {
            _lhs = lhs;
            _rhs = rhs;
        }

        /// <summary>
        /// Creates a new LeftJoin where there is a Filter over the join
        /// </summary>
        /// <param name="lhs">LHS Pattern</param>
        /// <param name="rhs">RHS Pattern</param>
        /// <param name="filter">Filter to decide which RHS solutions are valid</param>
        public LeftJoin(ISparqlAlgebra lhs, ISparqlAlgebra rhs, ISparqlFilter filter)
            : this(lhs, rhs)
        {
            _filter = filter;
        }

        /// <summary>
        /// Evaluates the LeftJoin
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <returns></returns>
        public BaseMultiset Evaluate(SparqlEvaluationContext context)
        {
            // Need to be careful about whether we linearize (CORE-406)
            if (!CanLinearizeLhs(context))
            {
                context.InputMultiset = new IdentityMultiset();
            }
            BaseMultiset lhsResult = context.Evaluate(_lhs);
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
                // Only execute the RHS if the LHS had some results
                // Need to be careful about whether we linearize (CORE-406)
                context.InputMultiset = CanFlowResultsToRhs(context) && !IsCrossProduct ? lhsResult : new IdentityMultiset();
                BaseMultiset rhsResult = context.Evaluate(_rhs);
                context.CheckTimeout();

                context.OutputMultiset = lhsResult.LeftJoin(rhsResult, _filter.Expression);
                context.CheckTimeout();
            }

            context.InputMultiset = context.OutputMultiset;
            return context.OutputMultiset;
        }

        private bool CanLinearizeLhs(SparqlEvaluationContext context)
        {
            // Must be no floating variables already present in the results to be flowed
            return _lhs.FloatingVariables.All(v => !context.InputMultiset.ContainsVariable(v));
        }

        private bool CanFlowResultsToRhs(SparqlEvaluationContext context)
        {
            // Can't have any conflicting variables
            HashSet<String> lhsFixed = new HashSet<string>(_lhs.FixedVariables);
            HashSet<String> lhsFloating = new HashSet<string>(_lhs.FloatingVariables);
            HashSet<String> rhsFloating = new HashSet<string>(_rhs.FloatingVariables);
            HashSet<String> rhsFixed = new HashSet<string>(_rhs.FixedVariables);

            // RHS Floating can't be floating/fixed on LHS
            if (rhsFloating.Any(v => lhsFloating.Contains(v) || lhsFixed.Contains(v))) return false;
            // RHS Fixed can't be floating on LHS
            if (rhsFixed.Any(v => lhsFloating.Contains(v))) return false;

            // Otherwise OK
            return true;
        }

        private bool IsCrossProduct
        {
            get { return !_lhs.Variables.Any(v => _rhs.Variables.Contains(v)); }
        }

        /// <summary>
        /// Gets the Variables used in the Algebra
        /// </summary>
        public IEnumerable<String> Variables
        {
            get
            {
                return (_lhs.Variables.Concat(_rhs.Variables)).Distinct();
            }
        }

        /// <summary>
        /// Gets the enumeration of floating variables in the algebra i.e. variables that are not guaranteed to have a bound value
        /// </summary>
        public IEnumerable<String> FloatingVariables
        {
            get
            {
                // Floating variables are those fixed on RHS or floating on either side and not fixed on LHS
                IEnumerable<String> floating = _lhs.FloatingVariables.Concat(_rhs.FloatingVariables).Concat(_rhs.FixedVariables).Distinct();
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
                // Fixed variables are those fixed on LHS
                return _lhs.FixedVariables;
            }
        }

        /// <summary>
        /// Gets the Filter that applies across the Join
        /// </summary>
        public ISparqlFilter Filter
        {
            get
            {
                return _filter;
            }
        }

        /// <summary>
        /// Gets the LHS of the Join
        /// </summary>
        public ISparqlAlgebra Lhs
        {
            get
            {
                return _lhs;
            }
        }

        /// <summary>
        /// Gets the RHS of the Join
        /// </summary>
        public ISparqlAlgebra Rhs
        {
            get
            {
                return _rhs;
            }
        }

        /// <summary>
        /// Gets the String representation of the Algebra
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            String filter = _filter.ToString();
            filter = filter.Substring(7, filter.Length - 8);
            return "LeftJoin(" + _lhs.ToString() + ", " + _rhs.ToString() + ", " + filter + ")";
        }

        /// <summary>
        /// Converts the Algebra back to a SPARQL Query
        /// </summary>
        /// <returns></returns>
        public SparqlQuery ToQuery()
        {
            SparqlQuery q = new SparqlQuery();
            q.RootGraphPattern = ToGraphPattern();
            q.Optimise();
            return q;
        }

        /// <summary>
        /// Converts the Algebra back to a SPARQL Query
        /// </summary>
        /// <returns></returns>
        public GraphPattern ToGraphPattern()
        {
            GraphPattern p = _lhs.ToGraphPattern();
            GraphPattern opt = _rhs.ToGraphPattern();
            opt.IsOptional = true;
            if (_filter.Expression is ConstantTerm)
            {
                try
                {
                    if (!_filter.Expression.Evaluate(null, 0).AsSafeBoolean())
                    {
                        opt.Filter = _filter;
                    }
                }
                catch
                {
                    opt.Filter = _filter;
                }
            }
            else
            {
                opt.Filter = _filter;
            }
            p.AddGraphPattern(opt);
            return p;
        }

        /// <summary>
        /// Transforms both sides of the Join using the given Optimiser
        /// </summary>
        /// <param name="optimiser">Optimser</param>
        /// <returns></returns>
        public ISparqlAlgebra Transform(IAlgebraOptimiser optimiser)
        {
            if (optimiser is IExpressionTransformer)
            {
                return new LeftJoin(optimiser.Optimise(_lhs), optimiser.Optimise(_rhs), new UnaryExpressionFilter(((IExpressionTransformer)optimiser).Transform(_filter.Expression)));
            }
            else
            {
                return new LeftJoin(optimiser.Optimise(_lhs), optimiser.Optimise(_rhs), _filter);
            }
        }

        /// <summary>
        /// Transforms the LHS of the Join using the given Optimiser
        /// </summary>
        /// <param name="optimiser">Optimser</param>
        /// <returns></returns>
        public ISparqlAlgebra TransformLhs(IAlgebraOptimiser optimiser)
        {
            if (optimiser is IExpressionTransformer)
            {
                return new LeftJoin(optimiser.Optimise(_lhs), _rhs, new UnaryExpressionFilter(((IExpressionTransformer)optimiser).Transform(_filter.Expression)));
            }
            else
            {
                return new LeftJoin(optimiser.Optimise(_lhs), _rhs, _filter);
            }
        }

        /// <summary>
        /// Transforms the RHS of the Join using the given Optimiser
        /// </summary>
        /// <param name="optimiser">Optimser</param>
        /// <returns></returns>
        public ISparqlAlgebra TransformRhs(IAlgebraOptimiser optimiser)
        {
            if (optimiser is IExpressionTransformer)
            {
                return new LeftJoin(_lhs, optimiser.Optimise(_rhs), new UnaryExpressionFilter(((IExpressionTransformer)optimiser).Transform(_filter.Expression)));
            }
            else
            {
                return new LeftJoin(_lhs, optimiser.Optimise(_rhs), _filter);
            }
        }
    }

    /// <summary>
    /// Represents a Join
    /// </summary>
    public class Join 
        : IJoin
    {
        private readonly ISparqlAlgebra _lhs, _rhs;

        /// <summary>
        /// Creates a new Join
        /// </summary>
        /// <param name="lhs">Left Hand Side</param>
        /// <param name="rhs">Right Hand Side</param>
        public Join(ISparqlAlgebra lhs, ISparqlAlgebra rhs)
        {
            _lhs = lhs;
            _rhs = rhs;
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
            BaseMultiset lhsResult = context.Evaluate(_lhs);
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
                // Only Execute the RHS if the LHS has some results
                context.InputMultiset = lhsResult;
                BaseMultiset rhsResult = context.Evaluate(_rhs);
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
                return (_lhs.Variables.Concat(_rhs.Variables)).Distinct();
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
        /// Gets the LHS of the Join
        /// </summary>
        public ISparqlAlgebra Lhs
        {
            get
            {
                return _lhs;
            }
        }

        /// <summary>
        /// Gets the RHS of the Join
        /// </summary>
        public ISparqlAlgebra Rhs
        {
            get
            {
                return _rhs;
            }
        }

        /// <summary>
        /// Gets the String representation of the Join
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "Join(" + _lhs.ToString() + ", " + _rhs.ToString() + ")";
        }

        /// <summary>
        /// Converts the Algebra back to a SPARQL Query
        /// </summary>
        /// <returns></returns>
        public SparqlQuery ToQuery()
        {
            SparqlQuery q = new SparqlQuery();
            q.RootGraphPattern = ToGraphPattern();
            q.Optimise();
            return q;
        }

        /// <summary>
        /// Converts the Algebra back to a SPARQL Query
        /// </summary>
        /// <returns></returns>
        public GraphPattern ToGraphPattern()
        {
            GraphPattern p = _lhs.ToGraphPattern();
            p.AddGraphPattern(_rhs.ToGraphPattern());
            return p;
        }

        /// <summary>
        /// Transforms both sides of the Join using the given Optimiser
        /// </summary>
        /// <param name="optimiser">Optimser</param>
        /// <returns></returns>
        public ISparqlAlgebra Transform(IAlgebraOptimiser optimiser)
        {
            return new Join(optimiser.Optimise(_lhs), optimiser.Optimise(_rhs));
        }

        /// <summary>
        /// Transforms the LHS of the Join using the given Optimiser
        /// </summary>
        /// <param name="optimiser">Optimser</param>
        /// <returns></returns>
        public ISparqlAlgebra TransformLhs(IAlgebraOptimiser optimiser)
        {
            return new Join(optimiser.Optimise(_lhs), _rhs);
        }

        /// <summary>
        /// Transforms the RHS of the Join using the given Optimiser
        /// </summary>
        /// <param name="optimiser">Optimser</param>
        /// <returns></returns>
        public ISparqlAlgebra TransformRhs(IAlgebraOptimiser optimiser)
        {
            return new Join(_lhs, optimiser.Optimise(_rhs));
        }
    }

    /// <summary>
    /// Represents a Union
    /// </summary>
    public class Union 
        : IUnion
    {
        private readonly ISparqlAlgebra _lhs, _rhs;

        /// <summary>
        /// Creates a new Union
        /// </summary>
        /// <param name="lhs">LHS Pattern</param>
        /// <param name="rhs">RHS Pattern</param>
        public Union(ISparqlAlgebra lhs, ISparqlAlgebra rhs)
        {
            _lhs = lhs;
            _rhs = rhs;
        }

        /// <summary>
        /// Evaluates the Union
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public BaseMultiset Evaluate(SparqlEvaluationContext context)
        {
            BaseMultiset initialInput = context.InputMultiset;
            if (_lhs is Extend || _rhs is Extend) initialInput = new IdentityMultiset();

            context.InputMultiset = initialInput;
            BaseMultiset lhsResult = context.Evaluate(_lhs);
            context.CheckTimeout();

            context.InputMultiset = initialInput;
            BaseMultiset rhsResult = context.Evaluate(_rhs);
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
                return (_lhs.Variables.Concat(_rhs.Variables)).Distinct();
            }
        }

        /// <summary>
        /// Gets the enumeration of floating variables in the algebra i.e. variables that are not guaranteed to have a bound value
        /// </summary>
        public IEnumerable<String> FloatingVariables
        {
            get
            {
                // Floating variables are those not fixed
                HashSet<String> fixedVars = new HashSet<string>(FixedVariables);
                return Variables.Where(v => !fixedVars.Contains(v));
            }
        }

        /// <summary>
        /// Gets the enumeration of fixed variables in the algebra i.e. variables that are guaranteed to have a bound value
        /// </summary>
        public IEnumerable<String> FixedVariables
        {
            get
            {
                // Fixed variables are those fixed on both sides
                return _lhs.FixedVariables.Intersect(_rhs.FixedVariables);
            }
        }

        /// <summary>
        /// Gets the LHS of the Join
        /// </summary>
        public ISparqlAlgebra Lhs
        {
            get
            {
                return _lhs;
            }
        }

        /// <summary>
        /// Gets the RHS of the Join
        /// </summary>
        public ISparqlAlgebra Rhs
        {
            get
            {
                return _rhs;
            }
        }

        /// <summary>
        /// Gets the String representation of the Algebra
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "Union(" + _lhs.ToString() + ", " + _rhs.ToString() + ")";
        }

        /// <summary>
        /// Converts the Algebra back to a SPARQL Query
        /// </summary>
        /// <returns></returns>
        public SparqlQuery ToQuery()
        {
            SparqlQuery q = new SparqlQuery();
            q.RootGraphPattern = ToGraphPattern();
            q.Optimise();
            return q;
        }

        /// <summary>
        /// Converts the Algebra back to a SPARQL Query
        /// </summary>
        /// <returns></returns>
        public GraphPattern ToGraphPattern()
        {
            GraphPattern p = new GraphPattern();
            p.IsUnion = true;
            p.AddGraphPattern(_lhs.ToGraphPattern());
            p.AddGraphPattern(_rhs.ToGraphPattern());
            return p;
        }

        /// <summary>
        /// Transforms both sides of the Join using the given Optimiser
        /// </summary>
        /// <param name="optimiser">Optimser</param>
        /// <returns></returns>
        public ISparqlAlgebra Transform(IAlgebraOptimiser optimiser)
        {
            return new Union(optimiser.Optimise(_lhs), optimiser.Optimise(_rhs));
        }

        /// <summary>
        /// Transforms the LHS of the Join using the given Optimiser
        /// </summary>
        /// <param name="optimiser">Optimser</param>
        /// <returns></returns>
        public ISparqlAlgebra TransformLhs(IAlgebraOptimiser optimiser)
        {
            return new Union(optimiser.Optimise(_lhs), _rhs);
        }

        /// <summary>
        /// Transforms the RHS of the Join using the given Optimiser
        /// </summary>
        /// <param name="optimiser">Optimser</param>
        /// <returns></returns>
        public ISparqlAlgebra TransformRhs(IAlgebraOptimiser optimiser)
        {
            return new Union(_lhs, optimiser.Optimise(_rhs));
        }
    }
}
