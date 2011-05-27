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
        private List<ITriplePattern> _triplePatterns = new List<ITriplePattern>();

        /// <summary>
        /// Creates a Streamed BGP containing a single Triple Pattern
        /// </summary>
        /// <param name="p">Triple Pattern</param>
        public AskBgp(ITriplePattern p)
        {
            if (!IsAskEvaluablePattern(p)) throw new ArgumentException("Triple Pattern instance must be a Triple Pattern or a FILTER Pattern", "p");
            this._triplePatterns.Add(p);
        }

        /// <summary>
        /// Creates a Streamed BGP containing a set of Triple Patterns
        /// </summary>
        /// <param name="ps">Triple Patterns</param>
        public AskBgp(IEnumerable<ITriplePattern> ps)
        {
            if (!ps.All(p => IsAskEvaluablePattern(p))) throw new ArgumentException("Triple Pattern instances must all be Triple Patterns or FILTER Patterns", "ps");
            this._triplePatterns.AddRange(ps);
        }

        /// <summary>
        /// Determines whether a Triple Pattern can be evaluated using a Lazy ASK approach
        /// </summary>
        /// <param name="p">Triple Pattern</param>
        /// <returns></returns>
        private bool IsAskEvaluablePattern(ITriplePattern p)
        {
            return (p is TriplePattern || p is FilterPattern);
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
            BaseMultiset results = this.StreamingEvaluate(context, 0, out halt);
            if (results is Multiset && results.IsEmpty) results = new NullMultiset();

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

            BaseMultiset initialInput, localOutput, results;

            //Set up the Input and Output Multiset appropriately
            switch (pattern)
            {
                case 0:
                    //Input is as given and Output is new empty multiset
                    initialInput = context.InputMultiset;
                    localOutput = new Multiset();
                    break;

                case 1:
                    //Input becomes current Output and Output is new empty multiset
                    initialInput = context.OutputMultiset;
                    localOutput = new Multiset();
                    break;

                default:
                    //Input is join of previous input and ouput and Output is new empty multiset
                    if (context.InputMultiset.IsDisjointWith(context.OutputMultiset))
                    {
                        //Disjoint so do a Product
                        initialInput = context.InputMultiset.Product(context.OutputMultiset);
                    }
                    else
                    {
                        //Normal Join
                        initialInput = context.InputMultiset.Join(context.OutputMultiset);
                    }
                    localOutput = new Multiset();
                    break;
            }
            context.InputMultiset = initialInput;
            context.OutputMultiset = localOutput;

            //Get the Triple Pattern we're evaluating
            ITriplePattern temp = this._triplePatterns[pattern];
            int resultsFound = 0;

            if (temp is TriplePattern)
            {
                //Find the first Triple which matches the Pattern
                TriplePattern tp = (TriplePattern)temp;
                foreach (Triple t in tp.GetTriples(context))
                {
                    //Remember to check for Timeout during lazy evaluation
                    context.CheckTimeout();

                    if (tp.Accepts(context, t))
                    {
                        resultsFound++;
                        context.OutputMultiset.Add(tp.CreateResult(t));

                        //Recurse unless we're the last pattern
                        if (pattern < this._triplePatterns.Count - 1)
                        {
                            results = this.StreamingEvaluate(context, pattern + 1, out halt);

                            //If recursion leads to a halt then we halt and return immediately
                            if (halt) return results;

                            //Otherwise we need to keep going here
                            //So must reset our input and outputs before continuing
                            context.InputMultiset = initialInput;
                            context.OutputMultiset = new Multiset();
                            resultsFound--;
                        }
                        else
                        {
                            //If we're at the last pattern and we've found a match then we can halt
                            halt = true;

                            //Generate the final output and return it
                            if (context.InputMultiset.IsDisjointWith(context.OutputMultiset))
                            {
                                //Disjoint so do a Product
                                context.OutputMultiset = context.InputMultiset.ProductWithTimeout(context.OutputMultiset, context.QueryTimeout - context.QueryTime);
                            }
                            else
                            {
                                //Normal Join
                                context.OutputMultiset = context.InputMultiset.Join(context.OutputMultiset);
                            }
                            return context.OutputMultiset;
                        }
                    }
                }
            }
            else if (temp is FilterPattern)
            {
                FilterPattern fp = (FilterPattern)temp;
                ISparqlFilter filter = fp.Filter;
                ISparqlExpression expr = filter.Expression;

                //Find the first result of those we've got so far that matches
                if (context.InputMultiset is IdentityMultiset || context.InputMultiset.IsEmpty)
                {
                    try
                    {
                        //If the Input is the Identity Multiset then the Output is either
                        //the Identity/Null Multiset depending on whether the Expression evaluates to true
                        if (expr.EffectiveBooleanValue(context, 0))
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
                        //If Expression fails to evaluate then result is NullMultiset
                        context.OutputMultiset = new NullMultiset();
                    }
                } 
                else
                {
                    foreach (int id in context.InputMultiset.SetIDs)
                    {
                        //Remember to check for Timeout during lazy evaluation
                        context.CheckTimeout();

                        try
                        {
                            if (expr.EffectiveBooleanValue(context, id))
                            {
                                resultsFound++;
                                context.OutputMultiset.Add(context.InputMultiset[id]);

                                //Recurse unless we're the last pattern
                                if (pattern < this._triplePatterns.Count - 1)
                                {
                                    results = this.StreamingEvaluate(context, pattern + 1, out halt);

                                    //If recursion leads to a halt then we halt and return immediately
                                    if (halt) return results;

                                    //Otherwise we need to keep going here
                                    //So must reset our input and outputs before continuing
                                    context.InputMultiset = initialInput;
                                    context.OutputMultiset = new Multiset();
                                    resultsFound--;
                                }
                                else
                                {
                                    //If we're at the last pattern and we've found a match then we can halt
                                    halt = true;

                                    //Generate the final output and return it
                                    if (context.InputMultiset.IsDisjointWith(context.OutputMultiset))
                                    {
                                        //Disjoint so do a Product
                                        context.OutputMultiset = context.InputMultiset.ProductWithTimeout(context.OutputMultiset, context.RemainingTimeout);
                                    }
                                    else
                                    {
                                        //Normal Join
                                        context.OutputMultiset = context.InputMultiset.Join(context.OutputMultiset);
                                    }
                                    return context.OutputMultiset;
                                }
                            }
                        }
                        catch
                        {
                            //Ignore expression evaluation errors
                        }
                    }
                }
            }

            //If we found no possibles we return the null multiset
            if (resultsFound == 0) return new NullMultiset();

            //We should never reach here so throw an error to that effect
            //The reason we'll never reach here is that this method should always return earlier
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
            q.RootGraphPattern = this.ToGraphPattern();
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
            this._lhs = lhs;
            this._rhs = rhs;
        }

        /// <summary>
        /// Evaluates the Ask Union
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <returns></returns>
        public BaseMultiset Evaluate(SparqlEvaluationContext context)
        {
            BaseMultiset initialInput = context.InputMultiset;
            BaseMultiset lhsResult = context.Evaluate(this._lhs);//this._lhs.Evaluate(context);
            context.CheckTimeout();

            if (lhsResult.IsEmpty)
            {
                //Only evaluate the RHS if the LHS was empty
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
            return "AskUnion(" + this._lhs.ToString() + ", " + this._rhs.ToString() + ")";
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
            return new AskUnion(optimiser.Optimise(this._lhs), optimiser.Optimise(this._rhs));
        }

        /// <summary>
        /// Transforms the LHS of the Join using the given Optimiser
        /// </summary>
        /// <param name="optimiser">Optimser</param>
        /// <returns></returns>
        public ISparqlAlgebra TransformLhs(IAlgebraOptimiser optimiser)
        {
            return new AskUnion(optimiser.Optimise(this._lhs), this._rhs);
        }

        /// <summary>
        /// Transforms the RHS of the Join using the given Optimiser
        /// </summary>
        /// <param name="optimiser">Optimser</param>
        /// <returns></returns>
        public ISparqlAlgebra TransformRhs(IAlgebraOptimiser optimiser)
        {
            return new AskUnion(this._lhs, optimiser.Optimise(this._rhs));
        }
    }
}
