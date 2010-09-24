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
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Tokens;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Filters;

namespace VDS.RDF.Query.Patterns
{
    /// <summary>
    /// Class for representing Graph Patterns in Sparql Queries
    /// </summary>
    public class GraphPattern
    {
        private bool _isFiltered = false;
        private bool _isOptional = false;
        private bool _isUnion = false;
        private bool _isGraph = false;
        private bool _isOptimised = false;
        private bool _isExists = false;
        private bool _isNotExists = false;
        private bool _isMinus = false;
        private bool _isService = false;
        private IToken _graphSpecifier = null;
        private List<GraphPattern> _graphPatterns = new List<GraphPattern>();
        private List<ITriplePattern> _triplePatterns = new List<ITriplePattern>();
        private List<ISparqlFilter> _unplacedFilters = new List<ISparqlFilter>();
        private List<LetPattern> _unplacedAssignments = new List<LetPattern>();
        private ISparqlFilter _filter;
        private bool _break = false, _broken = false;

        /// <summary>
        /// Creates a new Graph Pattern
        /// </summary>
        protected internal GraphPattern()
        {

        }

        /// <summary>
        /// Adds a Triple Pattern to the Graph Pattern
        /// </summary>
        /// <param name="p">Triple Pattern</param>
        protected internal void AddTriplePattern(ITriplePattern p)
        {
            if (this._break)
            {
                if (this._broken)
                {
                    this._graphPatterns.Last().AddTriplePattern(p);
                }
                else
                {
                    GraphPattern breakPattern = new GraphPattern();
                    breakPattern.AddTriplePattern(p);
                    this._graphPatterns.Add(breakPattern);
                }
            }
            else
            {
                this._triplePatterns.Add(p);
            }
        }

        /// <summary>
        /// Adds a child Graph Pattern to the Graph Pattern
        /// </summary>
        /// <param name="p"></param>
        protected internal void AddGraphPattern(GraphPattern p)
        {
            if (this._break)
            {
                if (this._broken)
                {
                    this._graphPatterns.Last().AddGraphPattern(p);
                }
                else
                {
                    this._graphPatterns.Add(p);
                }
            }
            else
            {
                this._graphPatterns.Add(p);
                if (!this._isUnion && !p.IsSubQuery) this.BreakBGP();
            }
        }

        /// <summary>
        /// Tells the Graph Pattern that any subsequent Graph/Triple Patterns added go in a new BGP
        /// </summary>
        protected internal void BreakBGP()
        {
            if (this._break)
            {
                if (this._broken)
                {
                    this._graphPatterns.Last().BreakBGP();
                }
            }
            else
            {
                this._break = true;
            }
        }

        #region Properties

        /// <summary>
        /// Gets/Sets whether the Graph Pattern is Optional
        /// </summary>
        public bool IsOptional
        {
            get
            {
                return this._isOptional;
            }
            internal set
            {
                this._isOptional = value;
            }
        }

        /// <summary>
        /// Gets/Sets whether the Graph Pattern is Filtered
        /// </summary>
        public bool IsFiltered
        {
            get
            {
                return this._isFiltered;
            }
            internal set
            {
                this._isFiltered = value;
            }
        }

        /// <summary>
        /// Gets/Sets whether the Graph Pattern is a Union of its Child Graph Patterns
        /// </summary>
        public bool IsUnion
        {
            get
            {
                return this._isUnion;
            }
            internal set
            {
                this._isUnion = value;
            }
        }

        /// <summary>
        /// Gets/Sets whether the Graph Pattern operates on a specific Graph
        /// </summary>
        public bool IsGraph
        {
            get
            {
                return this._isGraph;
            }
            internal set
            {
                this._isGraph = value;
            }
        }

        /// <summary>
        /// Gets whether this is an empty Graph Pattern
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return (this._triplePatterns.Count == 0 && this._graphPatterns.Count == 0 && !this._isFiltered && !this._isOptional && !this._isUnion);
            }
        }

        /// <summary>
        /// Gets/Sets whether the Graph Pattern is an EXISTS clause
        /// </summary>
        public bool IsExists
        {
            get
            {
                return this._isExists;
            }
            internal set
            {
                if (value && this._isNotExists) throw new RdfQueryException("A Graph Pattern cannot be both an EXISTS and a NOT EXISTS");
                this._isExists = value;
                this._isOptional = value;
            }
        }

        /// <summary>
        /// Gets/Sets whether the Graph Pattern is a NOT EXISTS clause
        /// </summary>
        public bool IsNotExists
        {
            get
            {
                return this._isNotExists;
            }
            internal set
            {
                if (value && this._isExists) throw new RdfQueryException("A Graph Pattern cannot be both an EXISTS and a NOT EXISTS");
                this._isNotExists = value;
                this._isOptional = value;
            }
        }

        /// <summary>
        /// Gets/Sets whether the Graph Pattern is a MINUS clause
        /// </summary>
        public bool IsMinus
        {
            get
            {
                return this._isMinus;
            }
            internal set
            {
                this._isMinus = value;
            }
        }

        /// <summary>
        /// Gets/Sets whether the Graph Pattern is a SERVICE clause
        /// </summary>
        public bool IsService
        {
            get
            {
                return this._isService;
            }
            internal set
            {
                this._isService = value;
            }
        }

        /// <summary>
        /// Gets whether Optimisation has been applied to this query
        /// </summary>
        /// <remarks>
        /// Optimisation involves the reordering of Triple Patterns and placement of FILTERs in an attempt to improve performance
        /// </remarks>
        public bool IsOptimised
        {
            get
            {
                return this._isOptimised;
            }
        }

        /// <summary>
        /// Gets/Sets the FILTER that applies to this Graph Pattern
        /// </summary>
        public ISparqlFilter Filter
        {
            get
            {
                return this._filter;
            }
            internal set
            {
                if (this._filter == null)
                {
                    //Set the Filter
                    this._filter = value;
                }
                else if (this._filter is ChainFilter)
                {
                    //Add to the Filter Chain
                    ((ChainFilter)this._filter).Add(value);
                }
                else
                {
                    //Create a Filter Chain
                    this._filter = new ChainFilter(this._filter, value);
                }
            }
        }

        /// <summary>
        /// Gets/Sets the Graph Specifier that applies to this Graph Pattern
        /// </summary>
        /// <remarks>
        /// This property is also used internally for SERVICE specifiers to save adding an additional property unnecessarily
        /// </remarks>
        public IToken GraphSpecifier
        {
            get
            {
                return this._graphSpecifier;
            }
            internal set
            {
                this._graphSpecifier = value;
            }
        }

        /// <summary>
        /// Checks whether this Pattern has any Child Graph Patterns
        /// </summary>
        public bool HasChildGraphPatterns
        {
            get
            {
                return (this._graphPatterns.Count > 0);
            }
        }

        /// <summary>
        /// Gets the Last Child Graph Pattern of this Pattern and removes it from this Pattern
        /// </summary>
        protected internal GraphPattern LastChildPattern()
        {
            GraphPattern p = this._graphPatterns[this._graphPatterns.Count - 1];
            this._graphPatterns.RemoveAt(this._graphPatterns.Count - 1);
            return p;
        }

        /// <summary>
        /// Gets the Child Graph Patterns of this Pattern
        /// </summary>
        public List<GraphPattern> ChildGraphPatterns
        {
            get
            {
                return this._graphPatterns;
            }
        }

        /// <summary>
        /// Gets the Triple Patterns in this Pattern
        /// </summary>
        public List<ITriplePattern> TriplePatterns
        {
            get
            {
                return this._triplePatterns;
            }
        }

        /// <summary>
        /// Gets whether this Pattern can be simplified
        /// </summary>
        protected internal bool IsSimplifiable
        {
            get
            {
                return (this._graphPatterns.Count == 1 && this._triplePatterns.Count == 0 && !this._isFiltered && !this._isGraph && !this._isOptional && !this._isUnion);
            }
        }

        /// <summary>
        /// Gets whether this Graph Pattern is a Sub-query which can be simplified
        /// </summary>
        public bool IsSubQuery
        {
            get
            {
                return (this._graphPatterns.Count == 0 && this._triplePatterns.Count == 1 && !this._isFiltered && !this._isGraph && !this._isOptional && !this._isUnion && this._triplePatterns[0] is SubQueryPattern);
            }
        }

        /// <summary>
        /// Gets the list of Filters that apply to this Graph Pattern which will be placed appropriately later
        /// </summary>
        protected internal List<ISparqlFilter> UnplacedFilters
        {
            get
            {
                return this._unplacedFilters;
            }
        }

        /// <summary>
        /// Gets the list of LET assignments that are in this Graph Pattern which will be placed appropriately later
        /// </summary>
        protected internal List<LetPattern> UnplacedAssignments
        {
            get
            {
                return this._unplacedAssignments;
            }
        }

        /// <summary>
        /// Gets the Variab
        /// </summary>
        public IEnumerable<String> Variables
        {
            get
            {
                return (from tp in this._triplePatterns
                        from v in tp.Variables
                        select v).Concat(from gp in this._graphPatterns
                                         from v in gp.Variables
                                         select v).Distinct();
            }
        }

        #endregion

        #region Pattern Optimisation

        /// <summary>
        /// Causes the Query to be optimised if it isn't already
        /// </summary>
        /// <remarks>
        /// Variables that have occurred prior to this Pattern
        /// </remarks>
        protected internal void Optimise(IEnumerable<String> variables)
        {
            if (this._isOptimised) return;

            //Our Variables is initially only those in our Triple Patterns since
            //anything else is considered to be out of scope
            List<String> ourVariables = (from tp in this._triplePatterns
                                         from v in tp.Variables
                                         select v).Distinct().ToList();
            foreach (GraphPattern gp in this._graphPatterns)
            {
                //At each point the variables that have occurred are those in the Triple Patterns and
                //those in previous Graph Patterns
                gp.Optimise(ourVariables);
                ourVariables.AddRange(gp.Variables);
            }

            if (Options.QueryOptimisation)
            {
                //We only apply Optimisation if the global setting is enabled

                //Just call sort on the Triple Patterns list
                //Triple Patterns have a CompareTo defined that orders them based on what is considered to be 
                //an optimal order
                //This order is only an approximation and may not be effective depending on the underlying dataset
                this._triplePatterns.Sort();

                if (this._triplePatterns.Count > 0) {
                    //After we sort which gives us a rough optimisation we then may want to reorder
                    //based on the Variables that occurred previous to us
                    //We only need to do this if the first pattern does not use a previously referenced variable
                    if (this._triplePatterns.Count > 1 && !this._triplePatterns[0].Variables.Any(v => variables.Contains(v)) && variables.Intersect(ourVariables).Any())
                    {
                        this.TryReorderPatterns(variables.ToList(), 1, 0);
                    }
                    else if (this._triplePatterns.Count > 2)
                    {
                        //In the case where there are more than 2 patterns then we can try and reorder these
                        //in order to further optimise the pattern
                        this.TryReorderPatterns(this._triplePatterns[0].Variables, 2, 1);
                    }
                }

                //First we need to place Assignments (LETs) in appropriate places within the Pattern
                //This happens before Filter placement since Filters may use variables assigned to in LETs
                if (this._unplacedAssignments.Count > 0)
                {
                    //Need to ensure that we sort Assignments
                    //This way those that use fewer variables get placed first
                    this._unplacedAssignments.Sort();

                    //This next bit goes in a do loop as we want to keep attempting to place assignments while
                    //we are able to do so.  If the count of unplaced assignments has decreased but is not
                    //zero it may be that we were unable to place some patterns as they relied on variables
                    //assigned in other LETs which weren't placed when we attempted to place them
                    //When we reach the point where no further placements have occurred or all assignments
                    //are placed we stop trying to place assignments
                    int c;
                    do
                    {
                        c = this._unplacedAssignments.Count;

                        int i = 0;
                        while (i < this._unplacedAssignments.Count)
                        {
                            if (this.TryPlaceAssignment(this._unplacedAssignments[i]))
                            {
                                //Remove from Unplaced Assignments since it's been successfully placed in the Triple Patterns
                                //Don't increment the counter since the next Assignment is now at the index we're already at
                                this._unplacedAssignments.RemoveAt(i);
                            }
                            else
                            {
                                //Unable to place so increment counter
                                i++;
                            }
                        }
                    } while (c > this._unplacedAssignments.Count && this._unplacedAssignments.Count > 0);
                }

                //Then we need to place the Filters in appropriate places within the Pattern
                if (this._unplacedFilters.Count > 0)
                {
                    if (this._triplePatterns.Count == 0)
                    {
                        //Where there are no Triple Patterns the Graph Pattern just contains this Filter and possibly some
                        //child Graph Patterns.  In such a case all Patterns shold be applied post-commit
                        //We do nothing here and the next bit of code automatically sets the Filters to the 
                        //Graph Pattern
                    }
                    else
                    {
                        int i = 0;
                        while (i < this._unplacedFilters.Count)
                        {
                            if (this.TryPlaceFilter(this._unplacedFilters[i]))
                            {
                                //Remove from Unplaced filters since it's been successfully placed in the Triple Patterns
                                //Don't increment the counter since the next filter is at the index we're already at
                                this._unplacedFilters.RemoveAt(i);
                            }
                            else
                            {
                                //Unable to place so leave unplaced
                                //Increment the counter so we can try and place the next Filter
                                i++;
                            }
                        }
                    }
                }
            }

            //Any unplaced Filters now get turned into Filters on the whole Graph Pattern
            //Filters might be unplaced if they use variables not directly in the Triple Patterns but in child
            //Graph Patterns or if optimisation has been disabled
            foreach (ISparqlFilter f in this._unplacedFilters) 
            {
                //We use the Filter property here since the set method will automatically
                //generate Chain Filters as necessary
                this.Filter = f;
            }

            //Any Unplaced assignments (i.e. LETs) are left in the list
            //These get processed in the Graph Patterns Execute method

            this._isOptimised = true;
        }

        /// <summary>
        /// Tries to reorder patterns when the initial ordering is considered poor
        /// </summary>
        /// <param name="desiredVariables">Variables that are desired</param>
        /// <param name="start">Point at which to start looking for better matches</param>
        /// <param name="end">Point at which to move the better match to</param>
        private void TryReorderPatterns(List<String> desiredVariables, int start, int end)
        {
            if (end > start) return;

            //Find the first pattern which does contain a pre-existing variable
            for (int i = start; i < this._triplePatterns.Count; i++)
            {
                if (this._triplePatterns[i].Variables.Any(v => desiredVariables.Contains(v)))
                {
                    int newEnd = i;
                    desiredVariables.AddRange(this._triplePatterns[i].Variables.Where(v => desiredVariables.Contains(v)));
                    while (i > end)
                    {
                        ITriplePattern temp = this._triplePatterns[i - 1];
                        this._triplePatterns[i - 1] = this._triplePatterns[i];
                        this._triplePatterns[i] = temp;
                        i--;
                    }
                    //this.TryReorderPatterns(this._triplePatterns[end].Variables, Math.Min(start, end + 1), end + 1);
                    //return;
                    end = newEnd;
                }
            }
        }

        /// <summary>
        /// Tries to place filters at the earliest point possible i.e. the first point after which all required variables have occurred
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        private bool TryPlaceFilter(ISparqlFilter filter)
        {
            //Firstly we need to find out what variables are needed in the Filter
            List<String> variablesNeeded = filter.Variables.Distinct().ToList();
            
            //Then we need to move through the Triple Patterns and find the first place at which all the
            //Variables used in the Filter have been used in ordinary Triple Patterns
            List<String> variablesUsed = new List<string>();
            for (int p = 0; p < this._triplePatterns.Count; p++)
            {
                if (this._triplePatterns[p] is TriplePattern || this._triplePatterns[p] is LetPattern)
                {
                    foreach (String var in this._triplePatterns[p].Variables)
                    {
                        if (!variablesUsed.Contains(var)) variablesUsed.Add(var);
                    }

                    //Have all the Variables we need now been used in a Pattern?
                    if (variablesNeeded.All(v => variablesUsed.Contains(v)))
                    {
                        //We can place this Filter after the Pattern we were just looking at
                        FilterPattern filterPattern = new FilterPattern(filter);
                        this._triplePatterns.Insert(p + 1, filterPattern);
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
        /// <param name="let">LET Assignment</param>
        /// <returns></returns>
        private bool TryPlaceAssignment(LetPattern let)
        {
            //Firstly we need to find out what variables are needed in the Assignment
            //The Variables property will include the variable that the LET assigns to so we can safely remove this
            List<String> variablesNeeded = let.Variables.Distinct().ToList();
            variablesNeeded.Remove(let.VariableName);

            //If there are no Variables Needed we can just place the assignment at the start
            //This implies that the assignment sets something to a fixed value
            if (variablesNeeded.Count == 0)
            {
                this._triplePatterns.Insert(0, let);
                return true;
            }

            //Then we need to move through the Triple Patterns and find the first place at which all the
            //Variables used in the Assignment have been used in ordinary Triple Patterns
            List<String> variablesUsed = new List<string>();
            for (int p = 0; p < this._triplePatterns.Count; p++)
            {
                if (this._triplePatterns[p] is TriplePattern || this._triplePatterns[p] is LetPattern)
                {
                    foreach (String var in this._triplePatterns[p].Variables)
                    {
                        if (!variablesUsed.Contains(var)) variablesUsed.Add(var);
                    }

                    //Have all the Variables we need now been used in a Pattern?
                    if (variablesNeeded.All(v => variablesUsed.Contains(v)))
                    {
                        //We can place this Assignment after the Pattern we were just looking at
                        this._triplePatterns.Insert(p + 1, let);
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

        #endregion

        #region String and Algebra Representations

        /// <summary>
        /// Gets the String representation of the Graph Pattern
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            String indent = String.Empty;

            if (this._isUnion)
            {
                indent = new String(' ', 2);
                for (int i = 0; i < this._graphPatterns.Count; i++)
                {
                    if (i > 0) output.Append(indent);
                    String temp = this._graphPatterns[i].ToString();
                    if (!temp.Contains('\n'))
                    {
                        output.Append(temp + " ");
                    }
                    else
                    {
                        String[] lines = temp.Split('\n');
                        for (int j = 0; j < lines.Length; j++)
                        {
                            if (j > 0) output.Append(indent);
                            output.Append(lines[j]);
                            if (j < lines.Length - 1) output.AppendLine();
                        }
                    }
                    output.AppendLine();
                    if (i < this._graphPatterns.Count - 1)
                    {
                        output.Append(indent);
                        output.AppendLine(" UNION ");
                    }
                }
                return output.ToString();
            }
            else if (this._isGraph)
            {
                output.Append("GRAPH ");
                switch (this._graphSpecifier.TokenType) {
                    case Token.QNAME:
                        output.Append(this._graphSpecifier.Value);
                        break;
                    case Token.URI:
                        output.Append('<');
                        output.Append(this._graphSpecifier.Value);
                        output.Append('>');
                        break;
                    case Token.VARIABLE:
                    default:
                        output.Append(this._graphSpecifier.Value);
                        break;
                }
                output.Append(" ");
            }
            else if (this._isOptional)
            {
                if (this._isExists)
                {
                    output.Append("EXISTS ");
                }
                else if (this._isNotExists)
                {
                    output.Append("NOT EXISTS ");
                }
                else
                {
                    output.Append("OPTIONAL ");
                }
            }

            output.Append("{ ");
            bool linebreaks = ((this._triplePatterns.Count + this._graphPatterns.Count + this._unplacedAssignments.Count) > 1) || this._isFiltered;
            if (linebreaks)
            {
                output.AppendLine();
                indent = new String(' ', 2);
            }
            //Triple Patterns
            foreach (ITriplePattern tp in this._triplePatterns)
            {
                String temp = tp.ToString();
                output.Append(indent);
                if (temp.Contains('\n'))
                {
                    String[] lines = temp.Split('\n');
                    for (int i = 0; i < lines.Length; i++)
                    {
                        if (i > 0) output.Append(indent);
                        if (i > 0 && i < lines.Length - 1) output.Append(' ');
                        output.Append(lines[i]);
                        if (i < lines.Length - 1) output.AppendLine();
                    }
                    output.Append(" . ");
                    if (linebreaks) output.AppendLine();
                }
                else
                {
                    output.Append(temp + " . ");
                    if (linebreaks) output.AppendLine();
                }
            }
            //Graph Patterns
            foreach (GraphPattern gp in this._graphPatterns)
            {
                output.Append(indent);
                String temp = gp.ToString();
                if (!temp.Contains('\n'))
                {
                    output.Append(temp + " ");
                }
                else
                {
                    String[] lines = temp.Split('\n');
                    for (int i = 0; i < lines.Length; i++)
                    {
                        if (i > 0) output.Append(indent);
                        output.Append(lines[i]);
                        if (i < lines.Length - 1) output.AppendLine();
                    }
                }
                if (linebreaks) output.AppendLine();
            }
            //Unplaced Assignments
            foreach (LetPattern lp in this._unplacedAssignments)
            {
                output.Append(lp.ToString());
                if (linebreaks) output.AppendLine();
            }
            //Filters
            if (this._filter != null)
            {
                output.Append(indent);
                output.Append(this._filter.ToString());
                if (linebreaks) output.AppendLine();
            }
            output.Append("}");

            return output.ToString();
        }

        /// <summary>
        /// Gets the Algebra representation of the Graph Pattern
        /// </summary>
        /// <returns></returns>
        public ISparqlAlgebra ToAlgebra()
        {
            if (this._isUnion)
            {
                //If this Graph Pattern represents a UNION of Graph Patterns turn into a series of UNIONs
                ISparqlAlgebra union = new Union(this._graphPatterns[0].ToAlgebra(), this._graphPatterns[1].ToAlgebra());
                if (this._graphPatterns.Count > 2)
                {
                    for (int i = 2; i < this._graphPatterns.Count; i++)
                    {
                        union = new Union(union, this._graphPatterns[i].ToAlgebra());
                    }
                }
                //If there's a FILTER apply it over the Union
                if (this._isFiltered && this._filter != null)
                {
                    return new Filter(union, this._filter);
                }
                else
                {
                    return union;
                }
            }
            else if (this._graphPatterns.Count == 0)
            {
                //If there are no Child Graph Patterns then this is a BGP
                ISparqlAlgebra bgp = new Bgp(this._triplePatterns);
                if (this._unplacedAssignments.Count > 0)
                {
                    //If we have any unplaced LETs these get Joined onto the BGP
                    bgp = Join.CreateJoin(bgp, new Bgp(this._unplacedAssignments));
                }
                if (this._isFiltered && this._filter != null)
                {
                    if (this._isOptional && !(this._isExists || this._isNotExists))
                    {
                        //If we contain an unplaced FILTER and we're an OPTIONAL then the FILTER
                        //applies over the LEFT JOIN and will have been added elsewhere in the Algebra transform
                        return bgp;
                    }
                    else
                    {
                        //If we contain an unplaced FILTER and we're not an OPTIONAL the FILTER
                        //applies here
                        return new Filter(bgp, this._filter);
                    }
                }
                else
                {
                    //We're not filtered (or all FILTERs were placed in the BGP) so we're just a BGP
                    return bgp;
                }
            }
            else
            {
                //Create a basic BGP to start with
                ISparqlAlgebra complex = new Bgp();
                if (this._triplePatterns.Count > 0)
                {
                    complex = new Bgp(this._triplePatterns);
                }

                //Then Join each of the Graph Patterns as appropriate
                foreach (GraphPattern gp in this._graphPatterns)
                {
                    if (gp.IsGraph)
                    {
                        //A GRAPH clause means a Join of the current pattern to a Graph clause
                        complex = Join.CreateJoin(complex, new Algebra.Graph(gp.ToAlgebra(), gp.GraphSpecifier));
                    }
                    else if (gp.IsOptional)
                    {
                        if (gp.IsExists || gp.IsNotExists)
                        {
                            //An EXISTS/NOT EXISTS means an Exists Join of the current pattern to the EXISTS/NOT EXISTS clause
                            complex = new ExistsJoin(complex, gp.ToAlgebra(), gp.IsExists);
                        }
                        else
                        {
                            //An OPTIONAL means a Left Join of the current pattern to the OPTIONAL clause
                            //with a possible FILTER applied over the LeftJoin
                            if (gp.IsFiltered && gp.Filter != null)
                            {
                                //If the OPTIONAL clause has an unplaced FILTER it applies over the Left Join
                                complex = new LeftJoin(complex, gp.ToAlgebra(), gp.Filter);
                            }
                            else
                            {
                                complex = new LeftJoin(complex, gp.ToAlgebra());
                            }
                        }
                    }
                    else if (gp.IsMinus)
                    {
                        //Always introduce a Minus here even if the Minus is disjoint since during evaluation we'll choose
                        //not to execute it if it's disjoint
                        complex = new Minus(complex, gp.ToAlgebra());
                    }
                    else if (gp.IsService)
                    {
                        complex = Join.CreateJoin(complex, new Service(gp.GraphSpecifier, gp));
                    }
                    else
                    {
                        //Otherwise we just join the pattern to the existing pattern
                        complex = Join.CreateJoin(complex, gp.ToAlgebra());
                    }
                }
                if (this._unplacedAssignments.Count > 0)
                {
                    //Unplaced assignments get Joined as a BGP here
                    complex = Join.CreateJoin(complex, new Bgp(this._unplacedAssignments));
                }
                if (this._isFiltered && this._filter != null)
                {
                    if (this._isOptional && !(this._isExists || this._isNotExists))
                    {
                        //If there's an unplaced FILTER and we're an OPTIONAL then the FILTER will
                        //apply over the LeftJoin and is applied elsewhere in the Algebra transform
                        return complex;
                    }
                    else
                    {
                        //If there's an unplaced FILTER and we're not an OPTIONAL pattern we apply
                        //the FILTER here
                        return new Filter(complex, this._filter);
                    }
                }
                else
                {
                    //If no FILTER just return the transform
                    return complex;
                }
            }
        }

        #endregion
    }
}