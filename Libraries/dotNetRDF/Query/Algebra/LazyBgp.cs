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
using System.Diagnostics;
using System.Linq;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Optimisation;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Algebra
{
    /// <summary>
    /// Represents a BGP which is a set of Triple Patterns
    /// </summary>
    /// <remarks>
    /// <para>
    /// A Lazy BGP differs from a BGP in that rather than evaluating each Triple Pattern in turn it evaluates across all Triple Patterns.  This is used for queries where we are only want to retrieve a limited number of solutions
    /// </para>
    /// <para>
    /// A Lazy BGP can only contain concrete Triple Patterns and/or FILTERs and not any of other the specialised Triple Pattern classes
    /// </para>
    /// </remarks>
    public class LazyBgp
        : Bgp
    {
        private int _requiredResults = -1;

        /// <summary>
        /// Creates a Streamed BGP containing a single Triple Pattern
        /// </summary>
        /// <param name="p">Triple Pattern</param>
        public LazyBgp(ITriplePattern p)
        {
            if (!IsLazilyEvaluablePattern(p)) throw new ArgumentException("Triple Pattern instance must be a Triple Pattern or a Subquery, BIND or FILTER Pattern", "p");
            _triplePatterns.Add(p);
        }

        /// <summary>
        /// Creates a Streamed BGP containing a set of Triple Patterns
        /// </summary>
        /// <param name="ps">Triple Patterns</param>
        public LazyBgp(IEnumerable<ITriplePattern> ps)
        {
            if (!ps.All(p => IsLazilyEvaluablePattern(p))) throw new ArgumentException("Triple Pattern instances must all be Triple Patterns or Subquery, BIND, FILTER Patterns", "ps");
            _triplePatterns.AddRange(ps);
        }

        /// <summary>
        /// Creates a Streamed BGP containing a single Triple Pattern
        /// </summary>
        /// <param name="p">Triple Pattern</param>
        /// <param name="requiredResults">The number of Results the BGP should attempt to return</param>
        public LazyBgp(ITriplePattern p, int requiredResults)
        {
            if (!IsLazilyEvaluablePattern(p)) throw new ArgumentException("Triple Pattern instance must be a Triple Pattern, BIND or FILTER Pattern", "p");
            _requiredResults = requiredResults;
            _triplePatterns.Add(p);
        }

        /// <summary>
        /// Creates a Streamed BGP containing a set of Triple Patterns
        /// </summary>
        /// <param name="ps">Triple Patterns</param>
        /// <param name="requiredResults">The number of Results the BGP should attempt to return</param>
        public LazyBgp(IEnumerable<ITriplePattern> ps, int requiredResults)
        {
            if (!ps.All(p => IsLazilyEvaluablePattern(p))) throw new ArgumentException("Triple Pattern instances must all be Triple Patterns, BIND or FILTER Patterns", "ps");
            _requiredResults = requiredResults;
            _triplePatterns.AddRange(ps);
        }

        private bool IsLazilyEvaluablePattern(ITriplePattern p)
        {
            return (p.PatternType == TriplePatternType.Match || p.PatternType == TriplePatternType.Filter || p.PatternType == TriplePatternType.BindAssignment);
        }

        /// <summary>
        /// Evaluates the BGP against the Evaluation Context
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <returns></returns>
        public override BaseMultiset Evaluate(SparqlEvaluationContext context)
        {
            bool halt;
            BaseMultiset results = null;
            int origRequired = _requiredResults;

            // May need to detect the actual amount of required results if not specified at instantation
            if (_requiredResults < 0)
            {
                if (context.Query != null)
                {
                    if (context.Query.HasDistinctModifier || (context.Query.OrderBy != null && !context.Query.IsOptimisableOrderBy) || context.Query.GroupBy != null || context.Query.Having != null || context.Query.Bindings != null)
                    {
                        // If there's an DISTINCT/ORDER BY/GROUP BY/HAVING/BINDINGS present then can't do Lazy evaluation
                        _requiredResults = -1;
                    }
                    else
                    {
                        int limit = context.Query.Limit;
                        int offset = context.Query.Offset;
                        if (limit >= 0 && offset >= 0)
                        {
                            // If there is a Limit and Offset specified then the required results is the LIMIT+OFFSET
                            _requiredResults = limit + offset;
                        }
                        else if (limit >= 0)
                        {
                            // If there is just a Limit specified then the required results is the LIMIT
                            _requiredResults = limit;
                        }
                        else
                        {
                            // In any other case required results is everything i.e. -1
                            _requiredResults = -1;
                        }
                    }
                    Debug.WriteLine("Lazy Evaluation - Number of required results is " + _requiredResults);
                }
            }

            switch (_requiredResults)
            {
                case -1:
                    // If required results is everything just use normal evaluation as otherwise 
                    // lazy evaluation will significantly impact performance and lead to an apparent infinite loop
                    return base.Evaluate(context);
                case 0:
                    // Don't need any results
                    results = new NullMultiset();
                    break;
                default:
                    // Do streaming evaluation
                    if (_requiredResults != 0)
                    {
                        results = StreamingEvaluate(context, 0, out halt);
                        if (results is Multiset && results.IsEmpty) results = new NullMultiset();
                    }
                    break;
            }
            _requiredResults = origRequired;

            context.OutputMultiset = results;
            context.OutputMultiset.Trim();
            return context.OutputMultiset;
        }

        private BaseMultiset StreamingEvaluate(SparqlEvaluationContext context, int pattern, out bool halt)
        {
            // Remember to check for Timeouts during Lazy Evaluation
            context.CheckTimeout();

            halt = false;

            // Handle Empty BGPs
            if (pattern == 0 && _triplePatterns.Count == 0)
            {
                context.OutputMultiset = new IdentityMultiset();
                return context.OutputMultiset;
            }

            BaseMultiset initialInput, localOutput, results = null;

            // Determine whether the Pattern modifies the existing Input rather than joining to it
            bool modifies = (_triplePatterns[pattern].PatternType == TriplePatternType.Filter);
            bool extended = (pattern > 0 && _triplePatterns[pattern - 1].PatternType == TriplePatternType.BindAssignment);
            bool modified = (pattern > 0 && _triplePatterns[pattern - 1].PatternType == TriplePatternType.Filter);

            // Set up the Input and Output Multiset appropriately
            switch (pattern)
            {
                case 0:
                    // Input is as given and Output is new empty multiset
                    if (!modifies)
                    {
                        initialInput = context.InputMultiset;
                    }
                    else
                    {
                        // If the Pattern will modify the Input and is the first thing in the BGP then it actually modifies a new empty input
                        // This takes care of FILTERs being out of scope
                        initialInput = new Multiset();
                    }
                    localOutput = new Multiset();
                    break;

                case 1:
                    // Input becomes current Output and Output is new empty multiset
                    initialInput = context.OutputMultiset;
                    localOutput = new Multiset();
                    break;

                default:
                    if (!extended && !modified)
                    {
                        // Input is join of previous input and output and Output is new empty multiset
                        if (context.InputMultiset.IsDisjointWith(context.OutputMultiset))
                        {
                            // Disjoint so do a Product
                            initialInput = context.InputMultiset.ProductWithTimeout(context.OutputMultiset, context.RemainingTimeout);
                        }
                        else
                        {
                            // Normal Join
                            initialInput = context.InputMultiset.Join(context.OutputMultiset);
                        }
                    }
                    else
                    {
                        initialInput = context.OutputMultiset;
                    }
                    localOutput = new Multiset();
                    break;
            }
            context.InputMultiset = initialInput;
            context.OutputMultiset = localOutput;

            // Get the Triple Pattern we're evaluating
            ITriplePattern temp = _triplePatterns[pattern];
            int resultsFound = 0;
            int prevResults = -1;

            if (temp.PatternType == TriplePatternType.Match)
            {
                // Find the first Triple which matches the Pattern
                IMatchTriplePattern tp = (IMatchTriplePattern) temp;
                IEnumerable<Triple> ts = tp.GetTriples(context);

                // In the case that we're lazily evaluating an optimisable ORDER BY then
                // we need to apply OrderBy()'s to our enumeration
                // This only applies to the 1st pattern
                if (pattern == 0)
                {
                    if (context.Query != null)
                    {
                        if (context.Query.OrderBy != null && context.Query.IsOptimisableOrderBy)
                        {
                            IComparer<Triple> comparer = context.Query.OrderBy.GetComparer(tp);
                            if (comparer != null)
                            {
                                ts = ts.OrderBy(t => t, comparer);
                            }
                            else
                            {
                                // Can't get a comparer so can't optimise
                                // Thus required results is everything so just use normal evaluation as otherwise 
                                // lazy evaluation will significantly impact performance and lead to an apparent infinite loop
                                return base.Evaluate(context);
                            }
                        }
                    }
                }

                foreach (Triple t in ts)
                {
                    if (tp.Accepts(context, t))
                    {
                        resultsFound++;
                        if (tp.IndexType == TripleIndexType.NoVariables)
                        {
                            localOutput = new IdentityMultiset();
                            context.OutputMultiset = localOutput;
                        }
                        else
                        {
                            context.OutputMultiset.Add(tp.CreateResult(t));
                        }
                    }
                }
                // Recurse unless we're the last pattern
                if (pattern < _triplePatterns.Count - 1)
                {
                    results = StreamingEvaluate(context, pattern + 1, out halt);

                    // If recursion leads to a halt then we halt and return immediately
                    if (halt && results.Count >= _requiredResults && _requiredResults != -1)
                    {
                        return results;
                    }
                    else if (halt)
                    {
                        if (results.Count == 0)
                        {
                            // If recursing leads to no results then eliminate all outputs
                            // Also reset to prevResults to -1
                            resultsFound = 0;
                            localOutput = new Multiset();
                            prevResults = -1;
                        }
                        else if (prevResults > -1)
                        {
                            if (results.Count == prevResults)
                            {
                                // If the amount of results found hasn't increased then this match does not
                                // generate any further solutions further down the recursion so we can eliminate
                                // this from the results
                                localOutput.Remove(localOutput.SetIDs.Max());
                            }
                        }
                        prevResults = results.Count;

                        // If we're supposed to halt but not reached the number of required results then continue
                        context.InputMultiset = initialInput;
                        context.OutputMultiset = localOutput;
                    }
                    else
                    {
                        // Otherwise we need to keep going here
                        // So must reset our input and outputs before continuing
                        context.InputMultiset = initialInput;
                        context.OutputMultiset = new Multiset();
                        resultsFound--;
                    }
                }
                else
                {
                    // If we're at the last pattern and we've found a match then we can halt
                    halt = true;

                    // Generate the final output and return it
                    if (context.InputMultiset.IsDisjointWith(context.OutputMultiset))
                    {
                        // Disjoint so do a Product
                        results = context.InputMultiset.ProductWithTimeout(context.OutputMultiset,
                            context.RemainingTimeout);
                    }
                    else
                    {
                        // Normal Join
                        results = context.InputMultiset.Join(context.OutputMultiset);
                    }

                    // If not reached required number of results continue
                    if (results.Count >= _requiredResults && _requiredResults != -1)
                    {
                        context.OutputMultiset = results;
                        return context.OutputMultiset;
                    }
                }
                context.InputMultiset = results;
            }
            else if (temp.PatternType == TriplePatternType.Filter)
            {
                IFilterPattern filter = (IFilterPattern) temp;
                ISparqlExpression filterExpr = filter.Filter.Expression;

                if (filter.Variables.IsDisjoint(context.InputMultiset.Variables))
                {
                    // Filter is Disjoint so determine whether it has any affect or not
                    if (filter.Variables.Any())
                    {
                        // Has Variables but disjoint from input => not in scope so gets ignored

                        // Do we recurse or not?
                        if (pattern < _triplePatterns.Count - 1)
                        {
                            // Recurse and return
                            results = StreamingEvaluate(context, pattern + 1, out halt);
                            return results;
                        }
                        else
                        {
                            // We don't affect the input in any way so just return it
                            return context.InputMultiset;
                        }
                    }
                    else
                    {
                        // No Variables so have to evaluate it to see if it gives true otherwise
                        try
                        {
                            if (filterExpr.Evaluate(context, 0).AsSafeBoolean())
                            {
                                if (pattern < _triplePatterns.Count - 1)
                                {
                                    // Recurse and return
                                    results = StreamingEvaluate(context, pattern + 1, out halt);
                                    return results;
                                }
                                else
                                {
                                    // Last Pattern and we evaluate to true so can return the input as-is
                                    halt = true;
                                    return context.InputMultiset;
                                }
                            }
                        }
                        catch (RdfQueryException)
                        {
                            // Evaluates to false so eliminates all solutions (use an empty Multiset)
                            return new Multiset();
                        }
                    }
                }
                else
                {
                    // Test each solution found so far against the Filter and eliminate those that evalute to false/error
                    foreach (int id in context.InputMultiset.SetIDs.ToList())
                    {
                        try
                        {
                            if (filterExpr.Evaluate(context, id).AsSafeBoolean())
                            {
                                // If evaluates to true then add to output
                                context.OutputMultiset.Add(context.InputMultiset[id].Copy());
                            }
                        }
                        catch (RdfQueryException)
                        {
                            // Error means we ignore the solution
                        }
                    }

                    // Remember to check for Timeouts during Lazy Evaluation
                    context.CheckTimeout();

                    // Decide whether to recurse or not
                    resultsFound = context.OutputMultiset.Count;
                    if (pattern < _triplePatterns.Count - 1)
                    {
                        // Recurse then return
                        // We can never decide whether to recurse again at this point as we are not capable of deciding
                        // which solutions should be dumped (that is the job of an earlier pattern in the BGP)
                        results = StreamingEvaluate(context, pattern + 1, out halt);

                        return results;
                    }
                    else
                    {
                        halt = true;

                        // However many results we need we'll halt - previous patterns can call us again if they find more potential solutions
                        // for us to filter
                        return context.OutputMultiset;
                    }
                }
            }
            else if (temp is BindPattern)
            {
                BindPattern bind = (BindPattern) temp;
                ISparqlExpression bindExpr = bind.AssignExpression;
                String bindVar = bind.VariableName;

                if (context.InputMultiset.ContainsVariable(bindVar))
                {
                    throw new RdfQueryException(
                        "Cannot use a BIND assigment to BIND to a variable that has previously been used in the Query");
                }
                else
                {
                    // Compute the Binding for every value
                    context.OutputMultiset.AddVariable(bindVar);
                    foreach (ISet s in context.InputMultiset.Sets)
                    {
                        ISet x = s.Copy();
                        try
                        {
                            INode val = bindExpr.Evaluate(context, s.ID);
                            x.Add(bindVar, val);
                        }
                        catch (RdfQueryException)
                        {
                            // Equivalent to no assignment but the solution is preserved
                        }
                        context.OutputMultiset.Add(x.Copy());
                    }

                    // Remember to check for Timeouts during Lazy Evaluation
                    context.CheckTimeout();

                    // Decide whether to recurse or not
                    resultsFound = context.OutputMultiset.Count;
                    if (pattern < _triplePatterns.Count - 1)
                    {
                        // Recurse then return
                        results = StreamingEvaluate(context, pattern + 1, out halt);
                        return results;
                    }
                    else
                    {
                        halt = true;

                        // However many results we need we'll halt - previous patterns can call us again if they find more potential solutions
                        // for us to extend
                        return context.OutputMultiset;
                    }
                }
            }
            else
            {
                throw new RdfQueryException("Encountered a " + temp.GetType().FullName +
                                            " which is not a lazily evaluable Pattern");
            }

            // If we found no possibles we return the null multiset
            if (resultsFound == 0)
            {
                return new NullMultiset();
            }
            else
            {
                // Remember to check for Timeouts during Lazy Evaluation
                context.CheckTimeout();

                // Generate the final output and return it
                if (!modifies)
                {
                    if (context.InputMultiset.IsDisjointWith(context.OutputMultiset))
                    {
                        // Disjoint so do a Product
                        results = context.InputMultiset.ProductWithTimeout(context.OutputMultiset, context.RemainingTimeout);
                    }
                    else
                    {
                        // Normal Join
                        results = context.InputMultiset.Join(context.OutputMultiset);
                    }
                    context.OutputMultiset = results;
                }
                return context.OutputMultiset;
            }
        }

        /// <summary>
        /// Gets the String representation of the Algebra
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "LazyBgp(" + _requiredResults + ")";
        }
    }

    /// <summary>
    /// Represents a Union
    /// </summary>
    /// <remarks>
    /// <para>
    /// A Lazy Union differs from a standard Union in that if it finds sufficient solutions on the LHS it has no need to evaluate the RHS
    /// </para>
    /// </remarks>
    public class LazyUnion : IUnion
    {
        private readonly ISparqlAlgebra _lhs, _rhs;
        private int _requiredResults = -1;

        /// <summary>
        /// Creates a new Lazy Union
        /// </summary>
        /// <param name="lhs">LHS Pattern</param>
        /// <param name="rhs">RHS Pattern</param>
        public LazyUnion(ISparqlAlgebra lhs, ISparqlAlgebra rhs)
        {
            _lhs = lhs;
            _rhs = rhs;
        }

        /// <summary>
        /// Creates a new Lazy Union
        /// </summary>
        /// <param name="lhs">LHS Pattern</param>
        /// <param name="rhs">RHS Pattern</param>
        /// <param name="requiredResults">The number of results that the Union should attempt to return</param>
        public LazyUnion(ISparqlAlgebra lhs, ISparqlAlgebra rhs, int requiredResults)
        {
            _lhs = lhs;
            _rhs = rhs;
        }

        /// <summary>
        /// Evaluates the Lazy Union
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <returns></returns>
        public BaseMultiset Evaluate(SparqlEvaluationContext context)
        {
            BaseMultiset initialInput = context.InputMultiset;
            if (_lhs is Extend || _rhs is Extend) initialInput = new IdentityMultiset();

            context.InputMultiset = initialInput;
            BaseMultiset lhsResult = context.Evaluate(_lhs); //this._lhs.Evaluate(context);
            context.CheckTimeout();

            if (lhsResult.Count >= _requiredResults || _requiredResults == -1)
            {
                // Only evaluate the RHS if the LHS didn't yield sufficient results
                context.InputMultiset = initialInput;
                BaseMultiset rhsResult = context.Evaluate(_rhs); //this._rhs.Evaluate(context);
                context.CheckTimeout();

                context.OutputMultiset = lhsResult.Union(rhsResult);
                context.CheckTimeout();

                context.InputMultiset = context.OutputMultiset;
            }
            else
            {
                context.OutputMultiset = lhsResult;
            }
            return context.OutputMultiset;
        }

        /// <summary>
        /// Gets the Variables used in the Algebra
        /// </summary>
        public IEnumerable<String> Variables
        {
            get { return (_lhs.Variables.Concat(_rhs.Variables)).Distinct(); }
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
            get { return _lhs; }
        }

        /// <summary>
        /// Gets the RHS of the Join
        /// </summary>
        public ISparqlAlgebra Rhs
        {
            get { return _rhs; }
        }

        /// <summary>
        /// Gets the String representation of the Algebra
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "LazyUnion(" + _lhs.ToString() + ", " + _rhs.ToString() + ")";
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
        /// Converts the Union back to Graph Patterns
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
            return new LazyUnion(optimiser.Optimise(_lhs), optimiser.Optimise(_rhs));
        }

        /// <summary>
        /// Transforms the LHS of the Join using the given Optimiser
        /// </summary>
        /// <param name="optimiser">Optimser</param>
        /// <returns></returns>
        public ISparqlAlgebra TransformLhs(IAlgebraOptimiser optimiser)
        {
            return new LazyUnion(optimiser.Optimise(_lhs), _rhs);
        }

        /// <summary>
        /// Transforms the RHS of the Join using the given Optimiser
        /// </summary>
        /// <param name="optimiser">Optimser</param>
        /// <returns></returns>
        public ISparqlAlgebra TransformRhs(IAlgebraOptimiser optimiser)
        {
            return new LazyUnion(_lhs, optimiser.Optimise(_rhs));
        }
    }
}