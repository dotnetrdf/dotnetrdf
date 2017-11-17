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
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Algebra
{
    /// <summary>
    /// Represents a BGP which is a set of Triple Patterns
    /// </summary>
    /// <remarks>
    /// <para>
    /// An Ask BGP differs from a BGP in that rather than evaluating each Triple Pattern in turn it evaluates across all Triple Patterns.  This is used for ASK queries where we are only concerned with whether a BGP matches and not in the specific solutions
    /// </para>
    /// <para>
    /// An Ask BGP can only contain concrete Triple Patterns and/or FILTERs and not any of the other specialised Triple Pattern classes
    /// </para>
    /// </remarks>
    public class AskBgp : IBgp
    {
        private readonly List<ITriplePattern> _triplePatterns = new List<ITriplePattern>();

        /// <summary>
        /// Creates a Streamed BGP containing a single Triple Pattern
        /// </summary>
        /// <param name="p">Triple Pattern</param>
        public AskBgp(ITriplePattern p)
        {
            if (!IsAskEvaluablePattern(p)) throw new ArgumentException("Triple Pattern instance must be a Triple Pattern or a FILTER Pattern", "p");
            _triplePatterns.Add(p);
        }

        /// <summary>
        /// Creates a Streamed BGP containing a set of Triple Patterns
        /// </summary>
        /// <param name="ps">Triple Patterns</param>
        public AskBgp(IEnumerable<ITriplePattern> ps)
        {
            if (!ps.All(p => IsAskEvaluablePattern(p))) throw new ArgumentException("Triple Pattern instances must all be Triple Patterns or FILTER Patterns", "ps");
            _triplePatterns.AddRange(ps);
        }

        /// <summary>
        /// Determines whether a Triple Pattern can be evaluated using a Lazy ASK approach
        /// </summary>
        /// <param name="p">Triple Pattern</param>
        /// <returns></returns>
        private bool IsAskEvaluablePattern(ITriplePattern p)
        {
            return (p.PatternType == TriplePatternType.Match || p.PatternType == TriplePatternType.Filter);
        }

        /// <summary>
        /// Gets the number of Triple Patterns in the BGP
        /// </summary>
        public int PatternCount
        {
            get
            {
                return _triplePatterns.Count;
            }
        }

        /// <summary>
        /// Gets the Triple Patterns in the BGP
        /// </summary>
        public IEnumerable<ITriplePattern> TriplePatterns
        {
            get
            {
                return _triplePatterns;
            }
        }

        /// <summary>
        /// Gets the Variables used in the Algebra
        /// </summary>
        public IEnumerable<String> Variables
        {
            get
            {
                return (from tp in _triplePatterns
                        from v in tp.Variables
                        select v).Distinct();
            }
        }

        /// <summary>
        /// Gets the enumeration of fixed variables in the algebra i.e. variables that are guaranteed to have a bound value
        /// </summary>
        public IEnumerable<string> FixedVariables
        {
            get
            {
                return (from tp in _triplePatterns
                        from v in tp.FixedVariables
                        select v).Distinct();
            }
        }

        /// <summary>
        /// Gets the enumeration of floating variables in the algebra i.e. variables that are not guaranteed to have a bound value
        /// </summary>
        public IEnumerable<string> FloatingVariables
        {
            get
            {
                // Floating variables are those declared as floating by triple patterns minus those that are declared as fixed by the triple patterns
                IEnumerable<String> floating = from tp in _triplePatterns
                                               from v in tp.FloatingVariables
                                               select v;
                HashSet<String> fixedVars = new HashSet<string>(FixedVariables);
                return floating.Where(v => !fixedVars.Contains(v)).Distinct();
            }
        }

        /// <summary>
        /// Gets whether the BGP is the emtpy BGP
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return (_triplePatterns.Count == 0);
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
            context.CheckTimeout();
            BaseMultiset results = StreamingEvaluate(context, 0, out halt);
            if (results is Multiset && results.IsEmpty) results = new NullMultiset();
            context.CheckTimeout();

            context.OutputMultiset = results;
            context.OutputMultiset.Trim();
            return context.OutputMultiset;
        }

        private BaseMultiset StreamingEvaluate(SparqlEvaluationContext context, int pattern, out bool halt)
        {
            halt = false;

            // Handle Empty BGPs
            if (pattern == 0 && _triplePatterns.Count == 0)
            {
                context.OutputMultiset = new IdentityMultiset();
                return context.OutputMultiset;
            }

            BaseMultiset initialInput, localOutput, results;

            // Set up the Input and Output Multiset appropriately
            switch (pattern)
            {
                case 0:
                    // Input is as given and Output is new empty multiset
                    initialInput = context.InputMultiset;
                    localOutput = new Multiset();
                    break;

                case 1:
                    // Input becomes current Output and Output is new empty multiset
                    initialInput = context.OutputMultiset;
                    localOutput = new Multiset();
                    break;

                default:
                    // Input is join of previous input and ouput and Output is new empty multiset
                    if (context.InputMultiset.IsDisjointWith(context.OutputMultiset))
                    {
                        // Disjoint so do a Product
                        initialInput = context.InputMultiset.Product(context.OutputMultiset);
                    }
                    else
                    {
                        // Normal Join
                        initialInput = context.InputMultiset.Join(context.OutputMultiset);
                    }
                    localOutput = new Multiset();
                    break;
            }
            context.InputMultiset = initialInput;
            context.OutputMultiset = localOutput;

            // Get the Triple Pattern we're evaluating
            ITriplePattern temp = _triplePatterns[pattern];
            int resultsFound = 0;

            if (temp.PatternType == TriplePatternType.Match)
            {
                // Find the first Triple which matches the Pattern
                IMatchTriplePattern tp = (IMatchTriplePattern)temp;
                foreach (Triple t in tp.GetTriples(context))
                {
                    // Remember to check for Timeout during lazy evaluation
                    context.CheckTimeout();

                    if (tp.Accepts(context, t))
                    {
                        resultsFound++;
                        context.OutputMultiset.Add(tp.CreateResult(t));

                        // Recurse unless we're the last pattern
                        if (pattern < _triplePatterns.Count - 1)
                        {
                            results = StreamingEvaluate(context, pattern + 1, out halt);

                            // If recursion leads to a halt then we halt and return immediately
                            if (halt) return results;

                            // Otherwise we need to keep going here
                            // So must reset our input and outputs before continuing
                            context.InputMultiset = initialInput;
                            context.OutputMultiset = new Multiset();
                            resultsFound--;
                        }
                        else
                        {
                            // If we're at the last pattern and we've found a match then we can halt
                            halt = true;

                            // Generate the final output and return it
                            if (context.InputMultiset.IsDisjointWith(context.OutputMultiset))
                            {
                                // Disjoint so do a Product
                                context.OutputMultiset = context.InputMultiset.ProductWithTimeout(context.OutputMultiset, context.QueryTimeout - context.QueryTime);
                            }
                            else
                            {
                                // Normal Join
                                context.OutputMultiset = context.InputMultiset.Join(context.OutputMultiset);
                            }
                            return context.OutputMultiset;
                        }
                    }
                }
            }
            else if (temp.PatternType == TriplePatternType.Filter)
            {
                IFilterPattern fp = (IFilterPattern)temp;
                ISparqlFilter filter = fp.Filter;
                ISparqlExpression expr = filter.Expression;

                // Find the first result of those we've got so far that matches
                if (context.InputMultiset is IdentityMultiset || context.InputMultiset.IsEmpty)
                {
                    try
                    {
                        // If the Input is the Identity Multiset then the Output is either
                        // the Identity/Null Multiset depending on whether the Expression evaluates to true
                        if (expr.Evaluate(context, 0).AsSafeBoolean())
                        {
                            context.OutputMultiset = new IdentityMultiset();
                        }
                        else
                        {
                            context.OutputMultiset = new NullMultiset();
                        }
                    }
                    catch
                    {
                        // If Expression fails to evaluate then result is NullMultiset
                        context.OutputMultiset = new NullMultiset();
                    }
                } 
                else
                {
                    foreach (int id in context.InputMultiset.SetIDs)
                    {
                        // Remember to check for Timeout during lazy evaluation
                        context.CheckTimeout();

                        try
                        {
                            if (expr.Evaluate(context, id).AsSafeBoolean())
                            {
                                resultsFound++;
                                context.OutputMultiset.Add(context.InputMultiset[id].Copy());

                                // Recurse unless we're the last pattern
                                if (pattern < _triplePatterns.Count - 1)
                                {
                                    results = StreamingEvaluate(context, pattern + 1, out halt);

                                    // If recursion leads to a halt then we halt and return immediately
                                    if (halt) return results;

                                    // Otherwise we need to keep going here
                                    // So must reset our input and outputs before continuing
                                    context.InputMultiset = initialInput;
                                    context.OutputMultiset = new Multiset();
                                    resultsFound--;
                                }
                                else
                                {
                                    // If we're at the last pattern and we've found a match then we can halt
                                    halt = true;

                                    // Generate the final output and return it
                                    if (context.InputMultiset.IsDisjointWith(context.OutputMultiset))
                                    {
                                        // Disjoint so do a Product
                                        context.OutputMultiset = context.InputMultiset.ProductWithTimeout(context.OutputMultiset, context.RemainingTimeout);
                                    }
                                    else
                                    {
                                        // Normal Join
                                        context.OutputMultiset = context.InputMultiset.Join(context.OutputMultiset);
                                    }
                                    return context.OutputMultiset;
                                }
                            }
                        }
                        catch
                        {
                            // Ignore expression evaluation errors
                        }
                    }
                }
            }

            // If we found no possibles we return the null multiset
            if (resultsFound == 0) return new NullMultiset();

            // We should never reach here so throw an error to that effect
            // The reason we'll never reach here is that this method should always return earlier
            throw new RdfQueryException("Unexpected control flow in evaluating a Streamed BGP for an ASK query");
        }

        /// <summary>
        /// Gets the String representation of the Algebra
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "AskBgp()";
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
        /// Converts the BGP back to a Graph Pattern
        /// </summary>
        /// <returns></returns>
        public GraphPattern ToGraphPattern()
        {
            GraphPattern p = new GraphPattern();
            foreach (ITriplePattern tp in _triplePatterns)
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
    /// An Ask Union differs from a standard Union in that if it finds a solution on the LHS it has no need to evaluate the RHS
    /// </para>
    /// </remarks>
    public class AskUnion : IUnion
    {
        private ISparqlAlgebra _lhs, _rhs;

        /// <summary>
        /// Creates a new Ask Union
        /// </summary>
        /// <param name="lhs">LHS Pattern</param>
        /// <param name="rhs">RHS Pattern</param>
        public AskUnion(ISparqlAlgebra lhs, ISparqlAlgebra rhs)
        {
            _lhs = lhs;
            _rhs = rhs;
        }

        /// <summary>
        /// Evaluates the Ask Union
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <returns></returns>
        public BaseMultiset Evaluate(SparqlEvaluationContext context)
        {
            BaseMultiset initialInput = context.InputMultiset;
            if (_lhs is Extend || _rhs is Extend) initialInput = new IdentityMultiset();

            context.InputMultiset = initialInput;
            BaseMultiset lhsResult = context.Evaluate(_lhs);//this._lhs.Evaluate(context);
            context.CheckTimeout();

            if (lhsResult.IsEmpty)
            {
                // Only evaluate the RHS if the LHS was empty
                context.InputMultiset = initialInput;
                BaseMultiset rhsResult = context.Evaluate(_rhs);//this._rhs.Evaluate(context);
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
            return "AskUnion(" + _lhs.ToString() + ", " + _rhs.ToString() + ")";
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
            return new AskUnion(optimiser.Optimise(_lhs), optimiser.Optimise(_rhs));
        }

        /// <summary>
        /// Transforms the LHS of the Join using the given Optimiser
        /// </summary>
        /// <param name="optimiser">Optimser</param>
        /// <returns></returns>
        public ISparqlAlgebra TransformLhs(IAlgebraOptimiser optimiser)
        {
            return new AskUnion(optimiser.Optimise(_lhs), _rhs);
        }

        /// <summary>
        /// Transforms the RHS of the Join using the given Optimiser
        /// </summary>
        /// <param name="optimiser">Optimser</param>
        /// <returns></returns>
        public ISparqlAlgebra TransformRhs(IAlgebraOptimiser optimiser)
        {
            return new AskUnion(_lhs, optimiser.Optimise(_rhs));
        }
    }
}
