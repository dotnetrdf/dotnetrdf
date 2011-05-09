/*

Copyright Robert Vesse 2009-11
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
using VDS.RDF.Query.Filters;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Optimisation
{
    /// <summary>
    /// A basic abstract implementation of a Query Optimiser
    /// </summary>
    /// <remarks>
    /// <para>
    /// Derived implementations may use override the virtual properties to control what forms of optimisation are used.  Derived implementations must override the <see cref="BaseQueryOptimiser.GetRankingComparer">GetRankingComparer()</see> method, optimisers which do not wish to change the order of Triple Patterns should return the <see cref="NoReorderComparer">NoReorderCompaper</see> in their implementation as a basic sort of Triple Patterns is done even if <see cref="BaseQueryOptimiser.ShouldReorder">ShouldReorder</see> is overridden to return false
    /// </para>
    /// </remarks>
    public abstract class BaseQueryOptimiser : IQueryOptimiser
    {
        /// <summary>
        /// Causes the Graph Pattern to be optimised if it isn't already
        /// </summary>
        /// <param name="gp">Graph Pattern</param>
        /// <param name="variables">Variables that have occurred prior to this Pattern</param>
        public void Optimise(GraphPattern gp, IEnumerable<String> variables)
        {
            //Our Variables is initially only those in our Triple Patterns since
            //anything else is considered to be out of scope
            List<String> ourVariables = (from tp in gp.TriplePatterns
                                         from v in tp.Variables
                                         select v).Distinct().ToList();

            //Start by sorting the Triple Patterns in the list according to the ranking function
            gp.TriplePatterns.Sort(this.GetRankingComparer());

            //Apply reordering unless an optimiser has chosen to disable it
            if (this.ShouldReorder)
            {
                if (gp.TriplePatterns.Count > 0)
                {
                    //After we sort which gives us a rough optimisation we then may want to reorder
                    //based on the Variables that occurred previous to us OR if we're the Root Graph Pattern

                    if (!variables.Any())
                    {
                        //Optimise this Graph Pattern
                        //No previously occurring variables so must be the first Graph Pattern
                        if (gp.TriplePatterns.Count > 1)
                        {
                            HashSet<String> currVariables = new HashSet<String>();
                            gp.TriplePatterns[0].Variables.ForEach(v => currVariables.Add(v));
                            for (int i = 1; i < gp.TriplePatterns.Count - 1; i++)
                            {
                                if (currVariables.Count == 0)
                                {
                                    gp.TriplePatterns[i].Variables.ForEach(v => currVariables.Add(v));
                                    continue;
                                }
                                else if (currVariables.IsDisjoint(gp.TriplePatterns[i].Variables))
                                {
                                    this.TryReorderPatterns(gp, currVariables.ToList(), i + 1, i);
                                    gp.TriplePatterns[i].Variables.ForEach(v => currVariables.Add(v));
                                }
                                else
                                {
                                    gp.TriplePatterns[i].Variables.ForEach(v => currVariables.Add(v));
                                }
                            }
                        }
                    }
                    else
                    {
                        //Optimise this Graph Pattern based on previously occurring variables
                        if (gp.TriplePatterns.Count > 1 && !gp.TriplePatterns[0].Variables.Any(v => variables.Contains(v)) && variables.Intersect(ourVariables).Any())
                        {
                            this.TryReorderPatterns(gp, variables.ToList(), 1, 0);
                        }
                        else if (gp.TriplePatterns.Count > 2)
                        {
                            //In the case where there are more than 2 patterns then we can try and reorder these
                            //in order to further optimise the pattern
                            this.TryReorderPatterns(gp, gp.TriplePatterns[0].Variables, 2, 1);
                        }
                    }
                }
            }

            if (this.ShouldPlaceAssignments)
            {
                //First we need to place Assignments (LETs) in appropriate places within the Pattern
                //This happens before Filter placement since Filters may use variables assigned to in LETs
                if (gp.UnplacedAssignments.Any())
                {
                    //Need to ensure that we sort Assignments
                    //This way those that use fewer variables get placed first
                    List<IAssignmentPattern> ps = gp.UnplacedAssignments.OrderBy(x => x).ToList();

                    //This next bit goes in a do loop as we want to keep attempting to place assignments while
                    //we are able to do so.  If the count of unplaced assignments has decreased but is not
                    //zero it may be that we were unable to place some patterns as they relied on variables
                    //assigned in other LETs which weren't placed when we attempted to place them
                    //When we reach the point where no further placements have occurred or all assignments
                    //are placed we stop trying to place assignments
                    int c;
                    do
                    {
                        c = ps.Count;

                        int i = 0;
                        while (i < ps.Count)
                        {
                            if (this.TryPlaceAssignment(gp, ps[i]))
                            {
                                //Remove from Unplaced Assignments since it's been successfully placed in the Triple Patterns
                                //Don't increment the counter since the next Assignment is now at the index we're already at
                                ps.RemoveAt(i);
                            }
                            else
                            {
                                //Unable to place so increment counter
                                i++;
                            }
                        }
                    } while (c > ps.Count && ps.Count > 0);
                }
            }

            //Regardless of what we've placed already we now place all remaining assignments
            foreach (IAssignmentPattern assignment in gp.UnplacedAssignments.ToList())
            {
                gp.InsertAssignment(assignment, gp.TriplePatterns.Count);
            }

            if (this.ShouldPlaceFilters)
            {
                //Then we need to place the Filters in appropriate places within the Pattern
                if (gp.UnplacedFilters.Any())
                {
                    if (gp.TriplePatterns.Count == 0)
                    {
                        //Where there are no Triple Patterns the Graph Pattern just contains this Filter and possibly some
                        //child Graph Patterns.  In such a case then we shouldn't place the Filters
                    }
                    else
                    {
                        foreach (ISparqlFilter f in gp.UnplacedFilters.ToList())
                        {
                            this.TryPlaceFilter(gp, f);
                        }
                    }
                }
            }

            //Finally optimise the Child Graph Patterns
            foreach (GraphPattern cgp in gp.ChildGraphPatterns)
            {
                //At each point the variables that have occurred are those in the Triple Patterns and
                //those in previous Graph Patterns
                cgp.Optimise(this, ourVariables);
                ourVariables.AddRange(cgp.Variables);
            }

            //Note: Any remaining Unplaced Filters/Assignments are OK since the ToAlgebra() method of a GraphPattern
            //will take care of placing these appropriately
        }

        /// <summary>
        /// Gets a comparer on Triple Patterns that is used to rank Triple Patterns
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// By overriding this in derived classes you can change how the Optimiser weights different patterns and thus the resultant ordering of Triple Patterns
        /// </remarks>
        protected abstract IComparer<ITriplePattern> GetRankingComparer();

        /// <summary>
        /// Controls whether the Optimiser will attempt to reorder Triple Patterns
        /// </summary>
        /// <remarks>
        /// It is recommended that derived classes do not change this setting as this may hurt performance.  If you want to control the optimisation process in detail we suggest you implement <see cref="IQueryOptimiser">IQueryOptimiser</see> directly in your own class and not derive from this implementation.
        /// </remarks>
        protected virtual bool ShouldReorder
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Controls whether the Optimiser will place Filters
        /// </summary>
        /// <remarks>
        /// It is recommended that derived classes do not change this setting as this may hurt performance.  If you want to control the optimisation process in detail we suggest you implement <see cref="IQueryOptimiser">IQueryOptimiser</see> directly in your own class and not derive from this implementation.
        /// </remarks>
        protected virtual bool ShouldPlaceFilters
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Controls whether the Optimiser will place Assignments
        /// </summary>
        /// <remarks>
        /// It is recommended that derived classes do not change this setting as this may hurt performance.  If you want to control the optimisation process in detail we suggest you implement <see cref="IQueryOptimiser">IQueryOptimiser</see> directly in your own class and not derive from this implementation.
        /// </remarks>
        protected virtual bool ShouldPlaceAssignments
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Tries to reorder patterns when the initial ordering is considered poor
        /// </summary>
        /// <param name="gp">Graph Pattern</param>
        /// <param name="desiredVariables">Variables that are desired</param>
        /// <param name="start">Point at which to start looking for better matches</param>
        /// <param name="end">Point at which to move the better match to</param>
        private void TryReorderPatterns(GraphPattern gp, List<String> desiredVariables, int start, int end)
        {
            if (end > start) return;

            //Find the first pattern which does contain a pre-existing variable
            for (int i = start; i < gp.TriplePatterns.Count; i++)
            {
                if (gp.TriplePatterns[i].Variables.Any(v => desiredVariables.Contains(v)))
                {
                    int newEnd = i;
                    desiredVariables.AddRange(gp.TriplePatterns[i].Variables.Where(v => desiredVariables.Contains(v)));
                    while (i > end)
                    {
                        //Swap Patterns around
                        gp.SwapTriplePatterns(i - 1, i);
                        i--;
                    }
                    end = newEnd;
                }
            }
        }

        /// <summary>
        /// Tries to place filters at the earliest point possible i.e. the first point after which all required variables have occurred
        /// </summary>
        /// <param name="gp">Graph Pattern</param>
        /// <param name="filter">Filter to place</param>
        /// <returns></returns>
        private bool TryPlaceFilter(GraphPattern gp, ISparqlFilter filter)
        {
            //Firstly we need to find out what variables are needed in the Filter
            List<String> variablesNeeded = filter.Variables.Distinct().ToList();

            //Then we need to move through the Triple Patterns and find the first place at which all the
            //Variables used in the Filter have been used in ordinary Triple Patterns
            List<String> variablesUsed = new List<string>();
            for (int p = 0; p < gp.TriplePatterns.Count; p++)
            {
                if (gp.TriplePatterns[p] is TriplePattern || gp.TriplePatterns[p] is IAssignmentPattern)
                {
                    foreach (String var in gp.TriplePatterns[p].Variables)
                    {
                        if (!variablesUsed.Contains(var)) variablesUsed.Add(var);
                    }

                    //Have all the Variables we need now been used in a Pattern?
                    if (variablesNeeded.All(v => variablesUsed.Contains(v)))
                    {
                        //We can place this Filter after the Pattern we were just looking at
                        gp.InsertFilter(filter, p + 1);
                        return true;
                    }
                }
            }

            //If we reach here then this means that all the Variables used in the Filter did not occur
            //in the Triple Patterns which means they likely occur in child graph patterns (or the query
            //is malformed).  In this case we cannot place the Filter and it has to be applied post-commit
            //rather than during Triple Pattern execution
            return false;
        }

        /// <summary>
        /// Tries to place assignments at the earliest point possible i.e. the first point after which all required variables have occurred
        /// </summary>
        /// <param name="gp">Graph Pattern</param>
        /// <param name="assignment">Assignment (LET/BIND)</param>
        /// <returns></returns>
        private bool TryPlaceAssignment(GraphPattern gp, IAssignmentPattern assignment)
        {
            //Firstly we need to find out what variables are needed in the Assignment
            //The Variables property will include the variable that the Assignment assigns to so we can safely remove this
            List<String> variablesNeeded = assignment.Variables.Distinct().ToList();
            variablesNeeded.Remove(assignment.VariableName);

            //If there are no Variables Needed we can just place the assignment at the start
            //This implies that the assignment sets something to a fixed value
            if (variablesNeeded.Count == 0)
            {
                gp.InsertAssignment(assignment, 0);
                return true;
            }

            //Then we need to move through the Triple Patterns and find the first place at which all the
            //Variables used in the Assignment have been used in ordinary Triple Patterns
            List<String> variablesUsed = new List<string>();
            for (int p = 0; p < gp.TriplePatterns.Count; p++)
            {
                if (gp.TriplePatterns[p] is TriplePattern || gp.TriplePatterns[p] is IAssignmentPattern)
                {
                    foreach (String var in gp.TriplePatterns[p].Variables)
                    {
                        if (!variablesUsed.Contains(var)) variablesUsed.Add(var);
                    }

                    //Have all the Variables we need now been used in a Pattern?
                    if (variablesNeeded.All(v => variablesUsed.Contains(v)))
                    {
                        //We can place this Assignment after the Pattern we were just looking at
                        gp.InsertAssignment(assignment, p + 1);
                        return true;
                    }
                }
            }

            //If we reach here then this means that all the Variables used in the Assignment did not occur
            //in the Triple Patterns which means they likely occur in child graph patterns (or the query
            //is malformed).  In this case we cannot place the Assignment and it has to be applied post-commit
            //rather than during Triple Pattern execution
            return false;
        }
    }
}
