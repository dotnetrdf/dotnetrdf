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
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Algebra
{
    /// <summary>
    /// Represents a BGP which is a set of Triple Patterns
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Note: </strong> This is experimental code currently designed only for optimisation of certain forms of queries
    /// </para>
    /// <para>
    /// An Ask BGP differs from a BGP in that rather than evaluating each Triple Pattern in turn it evaluates across all Triple Patterns.  This is used for ASK queries where we are only concerned with whether a BGP matches and not in the specific solutions
    /// </para>
    /// <para>
    /// An Ask BGP can only contain concrete Triple Patterns and not any of the specialised Triple Pattern classes
    /// </para>
    /// </remarks>
    public class LazyBgp : IBgp
    {
        private List<ITriplePattern> _triplePatterns = new List<ITriplePattern>();
        private int _requiredResults = 1;

        /// <summary>
        /// Creates a Streamed BGP containing a single Triple Pattern
        /// </summary>
        /// <param name="p">Triple Pattern</param>
        /// <param name="requiredResults">The number of Results the BGP should attempt to return</param>
        public LazyBgp(ITriplePattern p, int requiredResults)
        {
            if (!(p is TriplePattern || p is FilterPattern)) throw new ArgumentException("Triple Pattern instance must be a Triple Pattern or a FILTER Pattern", "p");
            if (this._requiredResults < 1) throw new ArgumentException("Required Results must be >= 1", "requiredResults");
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
            if (!ps.All(p => p is TriplePattern || p is FilterPattern)) throw new ArgumentException("Triple Pattern instances must all be Triple Patterns or FILTER Patterns", "ps");
            if (this._requiredResults < 1) throw new ArgumentException("Required Results must be >= 1", "requiredResults");
            this._requiredResults = requiredResults;
            this._triplePatterns.AddRange(ps);
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
            int prevResults = -1;

            if (temp is TriplePattern)
            {
                //Find the first Triple which matches the Pattern
                TriplePattern tp = (TriplePattern)temp;
                foreach (Triple t in tp.GetTriples(context))
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

                        //Recurse unless we're the last pattern
                        if (pattern < this._triplePatterns.Count - 1)
                        {
                            results = this.StreamingEvaluate(context, pattern + 1, out halt);

                            //If recursion leads to a halt then we halt and return immediately
                            if (halt && results.Count >= this._requiredResults)
                            {
                                return results;
                            }
                            else if (halt)
                            {
                                if (prevResults > -1)
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
                                results = context.InputMultiset.Product(context.OutputMultiset);
                            }
                            else
                            {
                                //Normal Join
                                results = context.InputMultiset.Join(context.OutputMultiset);
                            }

                            //If not reached required number of results continue
                            if (results.Count >= this._requiredResults)
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
                FilterPattern fp = (FilterPattern)temp;
                ISparqlFilter filter = fp.Filter;
                ISparqlExpression expr = filter.Expression;

                //Find the first result of those we've got so f
                foreach (int id in context.InputMultiset.SetIDs)
                {
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
                                if (halt && results.Count >= this._requiredResults)
                                {
                                    return results;
                                }
                                else if (halt)
                                {
                                    if (prevResults > -1)
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
                                    results = context.InputMultiset.Product(context.OutputMultiset);
                                }
                                else
                                {
                                    //Normal Join
                                    results = context.InputMultiset.Join(context.OutputMultiset);
                                }

                                //If not reached required number of results continue
                                if (results.Count >= this._requiredResults)
                                {
                                    context.OutputMultiset = results;
                                    return context.OutputMultiset;
                                }
                            }
                        }
                    }
                    catch
                    {
                        //Ignore expression evaluation errors
                    }
                }
            }

            //If we found no possibles we return the null multiset
            if (resultsFound == 0)
            {
                return new NullMultiset();
            }
            else
            {
                //Generate the final output and return it
                if (context.InputMultiset.IsDisjointWith(context.OutputMultiset))
                {
                    //Disjoint so do a Product
                    results = context.InputMultiset.Product(context.OutputMultiset);
                }
                else
                {
                    //Normal Join
                    results = context.InputMultiset.Join(context.OutputMultiset);
                }
                context.OutputMultiset = results;
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
    }
}
