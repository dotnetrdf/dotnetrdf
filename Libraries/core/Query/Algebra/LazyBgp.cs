/*

Copyright Robert Vesse 2009-10
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Filters;
using VDS.RDF.Query.Optimisation;
using VDS.RDF.Query.Ordering;
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
    public class LazyBgp : IBgp
    {
        private List<ITriplePattern> _triplePatterns = new List<ITriplePattern>();
        private int _requiredResults = -1;

        /// <summary>
        /// Creates a Streamed BGP containing a single Triple Pattern
        /// </summary>
        /// <param name="p">Triple Pattern</param>
        public LazyBgp(ITriplePattern p)
        {
            if (!IsLazilyEvaluablePattern(p)) throw new ArgumentException("Triple Pattern instance must be a Triple Pattern or a Subquery, BIND or FILTER Pattern", "p");
            this._triplePatterns.Add(p);
        }

        /// <summary>
        /// Creates a Streamed BGP containing a set of Triple Patterns
        /// </summary>
        /// <param name="ps">Triple Patterns</param>
        public LazyBgp(IEnumerable<ITriplePattern> ps)
        {
            if (!ps.All(p => IsLazilyEvaluablePattern(p))) throw new ArgumentException("Triple Pattern instances must all be Triple Patterns or Subquery, BIND, FILTER Patterns", "ps");
            this._triplePatterns.AddRange(ps);
        }

        /// <summary>
        /// Creates a Streamed BGP containing a single Triple Pattern
        /// </summary>
        /// <param name="p">Triple Pattern</param>
        /// <param name="requiredResults">The number of Results the BGP should attempt to return</param>
        public LazyBgp(ITriplePattern p, int requiredResults)
        {
            if (!IsLazilyEvaluablePattern(p)) throw new ArgumentException("Triple Pattern instance must be a Triple Pattern, BIND or FILTER Pattern", "p");
            this._requiredResults = requiredResults;
            this._triplePatterns.Add(p);
        }

        /// <summary>
        /// Creates a Streamed BGP containing a set of Triple Patterns
        /// </summary>
        /// <param name="ps">Triple Patterns</param>
        /// <param name="requiredResults">The number of Results the BGP should attempt to return</param>
        public LazyBgp(IEnumerable<ITriplePattern> ps, int requiredResults)
        {
            if (!ps.All(p => IsLazilyEvaluablePattern(p))) throw new ArgumentException("Triple Pattern instances must all be Triple Patterns, BIND or FILTER Patterns", "ps");
            this._requiredResults = requiredResults;
            this._triplePatterns.AddRange(ps);
        }

        private bool IsLazilyEvaluablePattern(ITriplePattern p)
        {
            return (p is TriplePattern || p is FilterPattern || p is BindPattern);
        }

        /// <summary>
        /// Gets the number of Triple Patterns in the BGP
        /// </summary>
        public int PatternCount
        {
            get
            {
                return this._triplePatterns.Count;
            }
        }

        /// <summary>
        /// Gets the Triple Patterns in the BGP
        /// </summary>
        public IEnumerable<ITriplePattern> TriplePatterns
        {
            get
            {
                return this._triplePatterns;
            }
        }

        /// <summary>
        /// Gets the Variables used in the Algebra
        /// </summary>
        public IEnumerable<String> Variables
        {
            get
            {
                return (from tp in this._triplePatterns
                        from v in tp.Variables
                        select v).Distinct();
            }
        }

        /// <summary>
        /// Gets whether the BGP is the emtpy BGP
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return (this._triplePatterns.Count == 0);
            }
        }

        /// <summary>
        /// Evaluates the BGP against the Evaluation Context
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <returns></returns>
        public BaseMultiset Evaluate(SparqlEvaluationContext context)
        {
            bool halt;
            BaseMultiset results;
            int origRequired = this._requiredResults;

            //May need to detect the actual
            if (this._requiredResults < 0)
            {
                if (context.Query != null)
                {
                    if (context.Query.HasDistinctModifier || (context.Query.OrderBy != null && !context.Query.IsOptimisableOrderBy) || context.Query.GroupBy != null || context.Query.Having != null || context.Query.Bindings != null)
                    {
                        //If there's an DISTINCT/ORDER BY/GROUP BY/HAVING/BINDINGS present then can't do Lazy evaluation
                        this._requiredResults = -1;
                    }
                    else
                    {
                        int limit = context.Query.Limit;
                        int offset = context.Query.Offset;
                        if (limit >= 0 && offset >= 0)
                        {
                            //If there is a Limit and Offset specified then the required results is the LIMIT+OFFSET
                            this._requiredResults = limit + offset;
                        }
                        else if (limit >= 0)
                        {
                            //If there is just a Limit specified then the required results is the LIMIT
                            this._requiredResults = limit;
                        }
                        else
                        {
                            //In any other case required results is everything i.e. -1
                            this._requiredResults = -1;
                        }
                    }
                }
            }

            if (this._requiredResults != 0)
            {
                results = this.StreamingEvaluate(context, 0, out halt);
                if (results is Multiset && results.IsEmpty) results = new NullMultiset();
            }
            else
            {
                results = new NullMultiset();
            }
            this._requiredResults = origRequired;

            context.OutputMultiset = results;
            context.OutputMultiset.Trim();
            return context.OutputMultiset;
        }

        private BaseMultiset StreamingEvaluate(SparqlEvaluationContext context, int pattern, out bool halt)
        {
            halt = false;

            //Handle Empty BGPs
            if (pattern == 0 && this._triplePatterns.Count == 0)
            {
                context.OutputMultiset = new IdentityMultiset();
                return context.OutputMultiset;
            }

            BaseMultiset initialInput, localOutput, results = null;

            //Determine whether the Pattern modifies the existing Input rather than joining to it
            bool modifies = (this._triplePatterns[pattern] is FilterPattern);
            bool extended = (pattern > 0 && this._triplePatterns[pattern-1] is BindPattern);
            bool modified = (pattern > 0 && this._triplePatterns[pattern-1] is FilterPattern);

            //Set up the Input and Output Multiset appropriately
            switch (pattern)
            {
                case 0:
                    //Input is as given and Output is new empty multiset
                    if (!modifies)
                    {
                        initialInput = context.InputMultiset;
                    }
                    else
                    {
                        //If the Pattern will modify the Input and is the first thing in the BGP then it actually modifies a new empty input
                        //This takes care of FILTERs being out of scope
                        initialInput = new Multiset();
                    }
                    localOutput = new Multiset();
                    break;

                case 1:
                    //Input becomes current Output and Output is new empty multiset
                    initialInput = context.OutputMultiset;
                    localOutput = new Multiset();
                    break;

                default:
                    if (!extended && !modified)
                    {
                        //Input is join of previous input and output and Output is new empty multiset
                        if (context.InputMultiset.IsDisjointWith(context.OutputMultiset))
                        {
                            //Disjoint so do a Product
                            initialInput = context.InputMultiset.ProductWithTimeout(context.OutputMultiset, context.RemainingTimeout);
                        }
                        else
                        {
                            //Normal Join
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

            //Get the Triple Pattern we're evaluating
            ITriplePattern temp = this._triplePatterns[pattern];
            int resultsFound = 0;
            int prevResults = -1;

            if (temp is TriplePattern)
            {
                //Find the first Triple which matches the Pattern
                TriplePattern tp = (TriplePattern)temp;
                IEnumerable<Triple> ts = tp.GetTriples(context);

                //In the case that we're lazily evaluating an optimisable ORDER BY then
                //we need to apply OrderBy()'s to our enumeration
                //This only applies to the 1st pattern
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
                                //Can't get a comparer so can't optimise
                                this._requiredResults = -1;
                            }
                        }
                    }
                }

                foreach (Triple t in ts)
                {
                    //Remember to check for Timeouts during Lazy Evaluation
                    context.CheckTimeout();

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

                        //Recurse unless we're the last pattern
                        if (pattern < this._triplePatterns.Count - 1)
                        {
                            results = this.StreamingEvaluate(context, pattern + 1, out halt);

                            //If recursion leads to a halt then we halt and return immediately
                            if (halt && results.Count >= this._requiredResults && this._requiredResults != -1)
                            {
                                return results;
                            }
                            else if (halt)
                            {
                                if (results.Count == 0)
                                {
                                    //If recursing leads to no results then eliminate all outputs
                                    //Also reset to prevResults to -1
                                    resultsFound = 0;
                                    localOutput = new Multiset();
                                    prevResults = -1;
                                }
                                else if (prevResults > -1)
                                {
                                    if (results.Count == prevResults)
                                    {
                                        //If the amount of results found hasn't increased then this match does not
                                        //generate any further solutions further down the recursion so we can eliminate
                                        //this from the results
                                        localOutput.Remove(localOutput.SetIDs.Max());
                                    }
                                }
                                prevResults = results.Count;

                                //If we're supposed to halt but not reached the number of required results then continue
                                context.InputMultiset = initialInput;
                                context.OutputMultiset = localOutput;
                            }
                            else
                            {
                                //Otherwise we need to keep going here
                                //So must reset our input and outputs before continuing
                                context.InputMultiset = initialInput;
                                context.OutputMultiset = new Multiset();
                                resultsFound--;
                            }
                        }
                        else
                        {
                            //If we're at the last pattern and we've found a match then we can halt
                            halt = true;

                            //Generate the final output and return it
                            if (context.InputMultiset.IsDisjointWith(context.OutputMultiset))
                            {
                                //Disjoint so do a Product
                                results = context.InputMultiset.ProductWithTimeout(context.OutputMultiset, context.RemainingTimeout);
                            }
                            else
                            {
                                //Normal Join
                                results = context.InputMultiset.Join(context.OutputMultiset);
                            }

                            //If not reached required number of results continue
                            if (results.Count >= this._requiredResults && this._requiredResults != -1)
                            {
                                context.OutputMultiset = results;
                                return context.OutputMultiset;
                            }
                        }
                    }
                }
            }
            else if (temp is FilterPattern)
            {
                FilterPattern filter = (FilterPattern)temp;
                ISparqlExpression filterExpr = filter.Filter.Expression;

                if (filter.Variables.IsDisjoint(context.InputMultiset.Variables))
                {
                    //Remember to check for Timeouts during Lazy Evaluation
                    context.CheckTimeout();

                    //Filter is Disjoint so determine whether it has any affect or not
                    if (filter.Variables.Any())
                    {
                        //Has Variables but disjoint from input => not in scope so gets ignored

                        //Do we recurse or not?
                        if (pattern < this._triplePatterns.Count - 1)
                        {
                            //Recurse and return
                            results = this.StreamingEvaluate(context, pattern + 1, out halt);
                            return results;
                        }
                        else
                        {
                            //We don't affect the input in any way so just return it
                            return context.InputMultiset;
                        }
                    }
                    else
                    {
                        //No Variables so have to evaluate it to see if it gives true otherwise
                        try
                        {
                            if (filterExpr.EffectiveBooleanValue(context, 0))
                            {
                                if (pattern < this._triplePatterns.Count - 1)
                                {
                                    //Recurse and return
                                    results = this.StreamingEvaluate(context, pattern + 1, out halt);
                                    return results;
                                }
                                else
                                {
                                    //Last Pattern and we evaluate to true so can return the input as-is
                                    halt = true;
                                    return context.InputMultiset;
                                }
                            }
                        }
                        catch (RdfQueryException)
                        {
                            //Evaluates to false so eliminates all solutions (use an empty Multiset)
                            return new Multiset();
                        }
                    }
                } 
                else
                {
                    //Remember to check for Timeouts during Lazy Evaluation
                    context.CheckTimeout();

                    //Test each solution found so far against the Filter and eliminate those that evalute to false/error
                    foreach (int id in context.InputMultiset.SetIDs.ToList())
                    {
                        try
                        {
                            if (filterExpr.EffectiveBooleanValue(context, id))
                            {
                                //If evaluates to true then add to output
                                context.OutputMultiset.Add(context.InputMultiset[id]);
                            }
                        } 
                        catch (RdfQueryException)
                        {
                            //Error means we ignore the solution
                        }
                    }

                    //Remember to check for Timeouts during Lazy Evaluation
                    context.CheckTimeout();
                    
                    //Decide whether to recurse or not
                    resultsFound = context.OutputMultiset.Count;
                    if (pattern < this._triplePatterns.Count - 1)
                    {
                        //Recurse then return
                        //We can never decide whether to recurse again at this point as we are not capable of deciding
                        //which solutions should be dumped (that is the job of an earlier pattern in the BGP)
                        results = this.StreamingEvaluate(context, pattern + 1, out halt);

                        return results;
                    }
                    else
                    {
                        halt = true;

                        //However many results we need we'll halt - previous patterns can call us again if they find more potential solutions
                        //for us to filter
                        return context.OutputMultiset;
                    }
                }
            }
            else if (temp is BindPattern)
            {
                BindPattern bind = (BindPattern)temp;
                ISparqlExpression bindExpr = bind.AssignExpression;
                String bindVar = bind.VariableName;

                if (context.InputMultiset.ContainsVariable(bindVar))
                {
                    throw new RdfQueryException("Cannot use a BIND assigment to BIND to a variable that has previously been used in the Query");
                }
                else
                {
                    //Remember to check for Timeouts during Lazy Evaluation
                    context.CheckTimeout();

                    //Compute the Binding for every value
                    context.OutputMultiset.AddVariable(bindVar);
                    foreach (ISet s in context.InputMultiset.Sets)
                    {
                        ISet x = s.Copy();
                        try
                        {
                            INode val = bindExpr.Value(context, s.ID);
                            x.Add(bindVar, val);
                        }
                        catch (RdfQueryException)
                        {
                            //Equivalent to no assignment but the solution is preserved
                        }
                        context.OutputMultiset.Add(x);
                    }

                    //Remember to check for Timeouts during Lazy Evaluation
                    context.CheckTimeout();

                    //Decide whether to recurse or not
                    resultsFound = context.OutputMultiset.Count;
                    if (pattern < this._triplePatterns.Count - 1)
                    {
                        //Recurse then return
                        results = this.StreamingEvaluate(context, pattern + 1, out halt);
                        return results;
                    }
                    else
                    {
                        halt = true;

                        //However many results we need we'll halt - previous patterns can call us again if they find more potential solutions
                        //for us to extend
                        return context.OutputMultiset;
                    }
                }
            }
            else
            {
                throw new RdfQueryException("Encountered a " + temp.GetType().FullName + " which is not a lazily evaluable Pattern");
            }

            //If we found no possibles we return the null multiset
            if (resultsFound == 0)
            {
                return new NullMultiset();
            }
            else
            {
                //Generate the final output and return it
                if (!modifies)
                {
                    if (context.InputMultiset.IsDisjointWith(context.OutputMultiset))
                    {
                        //Disjoint so do a Product
                        results = context.InputMultiset.ProductWithTimeout(context.OutputMultiset, context.RemainingTimeout);
                    }
                    else
                    {
                        //Normal Join
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
            return "LazyBgp()";
        }

        /// <summary>
        /// Converts the Algebra back to a SPARQL Query
        /// </summary>
        /// <returns></returns>
        public SparqlQuery ToQuery()
        {
            SparqlQuery q = new SparqlQuery();
            q.RootGraphPattern = this.ToGraphPattern();
            q.Optimise();
            return q;
        }

        /// <summary>
        /// Converts the BGP to a Graph Pattern
        /// </summary>
        /// <returns></returns>
        public GraphPattern ToGraphPattern()
        {
            GraphPattern p = new GraphPattern();
            foreach (TriplePattern tp in this._triplePatterns)
            {
                p.AddTriplePattern(tp);
            }
            return p;
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
        private ISparqlAlgebra _lhs, _rhs;
        private int _requiredResults = -1;

        /// <summary>
        /// Creates a new Lazy Union
        /// </summary>
        /// <param name="lhs">LHS Pattern</param>
        /// <param name="rhs">RHS Pattern</param>
        public LazyUnion(ISparqlAlgebra lhs, ISparqlAlgebra rhs)
        {
            this._lhs = lhs;
            this._rhs = rhs;
        }

        /// <summary>
        /// Creates a new Lazy Union
        /// </summary>
        /// <param name="lhs">LHS Pattern</param>
        /// <param name="rhs">RHS Pattern</param>
        /// <param name="requiredResults">The number of results that the Union should attempt to return</param>
        public LazyUnion(ISparqlAlgebra lhs, ISparqlAlgebra rhs, int requiredResults)
        {
            this._lhs = lhs;
            this._rhs = rhs;
        }

        /// <summary>
        /// Evaluates the Lazy Union
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <returns></returns>
        public BaseMultiset Evaluate(SparqlEvaluationContext context)
        {
            BaseMultiset initialInput = context.InputMultiset;
            BaseMultiset lhsResult = context.Evaluate(this._lhs);//this._lhs.Evaluate(context);
            context.CheckTimeout();

            if (lhsResult.Count >= this._requiredResults || this._requiredResults == -1)
            {
                //Only evaluate the RHS if the LHS didn't yield sufficient results
                context.InputMultiset = initialInput;
                BaseMultiset rhsResult = context.Evaluate(this._rhs);//this._rhs.Evaluate(context);
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
            get
            {
                return (this._lhs.Variables.Concat(this._rhs.Variables)).Distinct();
            }
        }

        /// <summary>
        /// Gets the LHS of the Join
        /// </summary>
        public ISparqlAlgebra Lhs
        {
            get
            {
                return this._lhs;
            }
        }

        /// <summary>
        /// Gets the RHS of the Join
        /// </summary>
        public ISparqlAlgebra Rhs
        {
            get
            {
                return this._rhs;
            }
        }

        /// <summary>
        /// Gets the String representation of the Algebra
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "LazyUnion(" + this._lhs.ToString() + ", " + this._rhs.ToString() + ")";
        }

        /// <summary>
        /// Converts the Algebra back to a SPARQL Query
        /// </summary>
        /// <returns></returns>
        public SparqlQuery ToQuery()
        {
            SparqlQuery q = new SparqlQuery();
            q.RootGraphPattern = this.ToGraphPattern();
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
            p.AddGraphPattern(this._lhs.ToGraphPattern());
            p.AddGraphPattern(this._rhs.ToGraphPattern());
            return p;
        }

        /// <summary>
        /// Transforms both sides of the Join using the given Optimiser
        /// </summary>
        /// <param name="optimiser">Optimser</param>
        /// <returns></returns>
        public ISparqlAlgebra Transform(IAlgebraOptimiser optimiser)
        {
            return new LazyUnion(optimiser.Optimise(this._lhs), optimiser.Optimise(this._rhs));
        }

        /// <summary>
        /// Transforms the LHS of the Join using the given Optimiser
        /// </summary>
        /// <param name="optimiser">Optimser</param>
        /// <returns></returns>
        public ISparqlAlgebra TransformLhs(IAlgebraOptimiser optimiser)
        {
            return new LazyUnion(optimiser.Optimise(this._lhs), this._rhs);
        }

        /// <summary>
        /// Transforms the RHS of the Join using the given Optimiser
        /// </summary>
        /// <param name="optimiser">Optimser</param>
        /// <returns></returns>
        public ISparqlAlgebra TransformRhs(IAlgebraOptimiser optimiser)
        {
            return new LazyUnion(this._lhs, optimiser.Optimise(this._rhs));
        }
    }
}
