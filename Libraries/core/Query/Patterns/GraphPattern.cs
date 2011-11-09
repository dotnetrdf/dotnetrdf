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
using VDS.RDF.Query.Optimisation;

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
        private bool _isSilent = false;
        private IToken _graphSpecifier = null;
        private List<GraphPattern> _graphPatterns = new List<GraphPattern>();
        private List<ITriplePattern> _triplePatterns = new List<ITriplePattern>();
        private List<ISparqlFilter> _unplacedFilters = new List<ISparqlFilter>();
        private List<IAssignmentPattern> _unplacedAssignments = new List<IAssignmentPattern>();
        private ISparqlFilter _filter;
        private bool _break = false, _broken = false;

        /// <summary>
        /// Creates a new Graph Pattern
        /// </summary>
        internal GraphPattern()
        {

        }

        internal GraphPattern(GraphPattern gp)
        {
            this._break = gp._break;
            this._broken = gp._broken;
            this._filter = gp._filter;
            this._graphPatterns.AddRange(gp._graphPatterns);
            this._graphSpecifier = gp._graphSpecifier;
            this._isExists = gp._isExists;
            this._isFiltered = gp._isFiltered;
            this._isGraph = gp._isGraph;
            this._isMinus = gp._isMinus;
            this._isNotExists = gp._isExists;
            this._isOptimised = gp._isOptimised;
            this._isOptional = gp._isOptional;
            this._isService = gp._isService;
            this._isSilent = gp._isSilent;
            this._isUnion = gp._isUnion;
            this._triplePatterns.AddRange(gp._triplePatterns);
            this._unplacedAssignments.AddRange(gp._unplacedAssignments);
            this._unplacedFilters.AddRange(gp._unplacedFilters);
        }

        /// <summary>
        /// Adds a Triple Pattern to the Graph Pattern respecting any BGP breaks
        /// </summary>
        /// <param name="p">Triple Pattern</param>
        internal void AddTriplePattern(ITriplePattern p)
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
        /// Adds an Assignment to the Graph Pattern respecting any BGP breaks
        /// </summary>
        /// <param name="p">Assignment Pattern</param>
        internal void AddAssignment(IAssignmentPattern p)
        {
            if (this._break)
            {
                if (this._broken)
                {
                    this._graphPatterns.Last().AddAssignment(p);
                }
                else
                {
                    GraphPattern breakPattern = new GraphPattern();
                    breakPattern.AddAssignment(p);
                    this._graphPatterns.Add(breakPattern);
                }
            }
            else
            {
                this._unplacedAssignments.Add(p);
                this.BreakBGP();
            }
        }

        /// <summary>
        /// Adds a Filter to the Graph Pattern respecting any BGP breaks
        /// </summary>
        /// <param name="filter">Filter</param>
        internal void AddFilter(ISparqlFilter filter)
        {
            this._unplacedFilters.Add(filter);
        }

        /// <summary>
        /// Adds a child Graph Pattern to the Graph Pattern respecting any BGP breaks
        /// </summary>
        /// <param name="p">Graph Pattern</param>
        internal void AddGraphPattern(GraphPattern p)
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
        internal void BreakBGP()
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

        /// <summary>
        /// Swaps the position of the two given Triple Patterns
        /// </summary>
        /// <param name="i">First Position</param>
        /// <param name="j">Second Position</param>
        /// <remarks>
        /// Intended for use by Query Optimisers
        /// </remarks>
        public void SwapTriplePatterns(int i, int j)
        {
            ITriplePattern temp = this._triplePatterns[i];
            this._triplePatterns[i] = this._triplePatterns[j];
            this._triplePatterns[j] = temp;
        }

        /// <summary>
        /// Inserts a Filter at a given position
        /// </summary>
        /// <param name="filter">Filter</param>
        /// <param name="i">Position to insert at</param>
        /// <remarks>
        /// Intended for use by Query Optimisers
        /// </remarks>
        public void InsertFilter(ISparqlFilter filter, int i)
        {
            if (!this._unplacedFilters.Contains(filter)) throw new RdfQueryException("Cannot Insert a Filter that is not currentlyy an unplaced Filter in this Graph Pattern");
            this._unplacedFilters.Remove(filter);
            FilterPattern p = new FilterPattern(filter);
            this._triplePatterns.Insert(i, p);
        }

        /// <summary>
        /// Inserts an Assignment at a given position
        /// </summary>
        /// <param name="assignment">Assignment</param>
        /// <param name="i">Position to insert at</param>
        /// <remarks>
        /// Intended for use by Query Optimisers
        /// </remarks>
        public void InsertAssignment(IAssignmentPattern assignment, int i)
        {
            if (!this._unplacedAssignments.Contains(assignment)) throw new RdfQueryException("Cannot Insert an Assignment that is not currently an unplaced Assignment in this Graph Pattern");
            this._unplacedAssignments.Remove(assignment);
            this._triplePatterns.Insert(i, assignment);
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
                return (this._triplePatterns.Count == 0 && this._graphPatterns.Count == 0 && !this._isFiltered && !this._isOptional && !this._isUnion && this._unplacedFilters.Count == 0 && this._unplacedAssignments.Count == 0);
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
        /// This only indicates that an Optimiser has been applied to the Pattern.  You can always reoptimise by calling the <see cref="SparqlQuert.Optimise">Optimise()</see> method with an optimiser of your choice on the query to which this Pattern belongs
        /// </remarks>
        public bool IsOptimised
        {
            get
            {
                return this._isOptimised;
            }
        }

        /// <summary>
        /// Gets whether Evaluation Errors in this Graph Pattern are suppressed (currently only valid with SERVICE)
        /// </summary>
        public bool IsSilent
        {
            get
            {
                return this._isSilent;
            }
            internal set
            {
                this._isSilent = value;
            }
        }

        /// <summary>
        /// Determines whether the Graph Pattern has any kind of Modifier (GRAPH, MINUS, OPTIONAL etc) applied
        /// </summary>
        public bool HasModifier
        {
            get
            {
                return (this.IsExists || this.IsGraph || this.IsMinus || this.IsNotExists || this.IsOptional || this.IsService || this.IsSubQuery);
            }
        }

        /// <summary>
        /// Gets/Sets the FILTER that applies to this Graph Pattern
        /// </summary>
        public ISparqlFilter Filter
        {
            get
            {
                if (this._unplacedFilters.Count > 0)
                {
                    if (this._filter == null)
                    {
                        return new ChainFilter(this._unplacedFilters);
                    }
                    else
                    {
                        return new ChainFilter(this._filter, this._unplacedFilters);
                    }
                }
                else
                {
                    return this._filter;
                }
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
        internal GraphPattern LastChildPattern()
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
        internal bool IsSimplifiable
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
        /// Gets whether the Graph Pattern uses the Default Dataset
        /// </summary>
        /// <remarks>
        /// Graph Patterns generally use the Default Dataset unless they are a GRAPH pattern or they contain a Triple Pattern, child Graph Pattern or a FILTER/BIND which does not use the default dataset
        /// </remarks>
        public bool UsesDefaultDataset
        {
            get
            {
                //SERVICE patterns are irrelevant as their dataset is irrelevant to whether the query uses the default dataset
                if (this._isService) return true;
                //Otherwise a pattern must not be a GRAPH pattern, all its triple patterns and child graph patterns must use the default dataset and any filters/assignments must use the default dataset
                return !this._isGraph && this._triplePatterns.All(tp => tp.UsesDefaultDataset) && this._graphPatterns.All(gp => gp.UsesDefaultDataset) && (this._filter == null || this._filter.Expression.UsesDefaultDataset()) && this._unplacedAssignments.All(ap => ap.AssignExpression.UsesDefaultDataset()) && this._unplacedFilters.All(f => f.Expression.UsesDefaultDataset());
            }
        }

        /// <summary>
        /// Gets the enumeration of Filters that apply to this Graph Pattern which will have yet to be placed within the Graph Pattern
        /// </summary>
        public IEnumerable<ISparqlFilter> UnplacedFilters
        {
            get
            {
                return this._unplacedFilters;
            }
        }

        /// <summary>
        /// Gets the enumeration of LET assignments that are in this Graph Pattern which will be placed appropriately later
        /// </summary>
        public IEnumerable<IAssignmentPattern> UnplacedAssignments
        {
            get
            {
                return this._unplacedAssignments;
            }
        }

        /// <summary>
        /// Gets the Variables used in the Pattern
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
        /// Causes the Graph Pattern to be optimised if it isn't already
        /// </summary>
        /// <param name="variables">Variables that have occurred prior to this Pattern</param>
        [Obsolete("This method represents the old fixed optimiser and is no longer used", true)]
        internal void Optimise(IEnumerable<String> variables)
        {
            throw new NotSupportedException("The old fixed optimiser is no longer supported");
        }

        /// <summary>
        /// Optimises the Graph Pattern using the current global optimiser
        /// </summary>
        public void Optimise()
        {
            this.Optimise(SparqlOptimiser.QueryOptimiser);
        }

        /// <summary>
        /// Optimises the Graph Pattern using the given optimiser
        /// </summary>
        /// <param name="optimiser">Query Optimiser</param>
        public void Optimise(IQueryOptimiser optimiser)
        {
            optimiser.Optimise(this, Enumerable.Empty<String>());
        }

        /// <summary>
        /// Optimises the Graph Pattern using the given optimiser and with the given variables
        /// </summary>
        /// <param name="optimiser">Query Optimiser</param>
        /// <param name="vars">Variables</param>
        /// <remarks>
        /// The <paramref name="vars">vars</paramref> parameter contains Variables mentioned in the parent Graph Pattern (if any) that can be used to guide optimisation of child graph patterns
        /// </remarks>
        public void Optimise(IQueryOptimiser optimiser, IEnumerable<String> vars)
        {
            optimiser.Optimise(this, vars);
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
            else if (this._isGraph || this._isService)
            {
                if (this._isGraph)
                {
                    output.Append("GRAPH ");
                }
                else
                {
                    output.Append("SERVICE ");
                    if (this._isSilent) output.Append("SILENT ");
                }
                switch (this._graphSpecifier.TokenType)
                {
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
            else if (this._isMinus)
            {
                output.Append("MINUS ");
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
            //Unplaced Assignments
            foreach (IAssignmentPattern ap in this._unplacedAssignments)
            {
                output.Append(ap.ToString());
                if (linebreaks) output.AppendLine();
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
            //Filters
            if (this._filter != null)
            {
                output.Append(indent);
                output.Append(this._filter.ToString());
                if (linebreaks) output.AppendLine();
            }
            foreach (ISparqlFilter filter in this._unplacedFilters)
            {
                output.Append(indent);
                output.Append(filter.ToString());
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
                if (this._isFiltered && (this._filter != null || this._unplacedFilters.Count > 0))
                {
                    return new Filter(union, this.Filter);
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
                if (this._isFiltered && (this._filter != null || this._unplacedFilters.Count > 0))
                {
                    if (this._isOptional && !(this._isExists || this._isNotExists))
                    {
                        //If we contain an unplaced FILTER and we're an OPTIONAL then the FILTER
                        //applies over the LEFT JOIN and will have been added elsewhere in the Algebra transform
                        return bgp;
                    }
                    else
                    {
                        ISparqlAlgebra complex = bgp;

                        //If we contain an unplaced FILTER and we're not an OPTIONAL the FILTER
                        //applies here
                        return new Filter(bgp, this.Filter);
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
                        complex = Join.CreateJoin(complex, new Service(gp.GraphSpecifier, gp, gp.IsSilent));
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
                if (this._isFiltered && (this._filter != null || this._unplacedFilters.Count > 0))
                {
                    if (this._isOptional && !(this._isExists || this._isNotExists))
                    {
                        //If there's an unplaced FILTER and we're an OPTIONAL then the FILTER will
                        //apply over the LeftJoin and is applied elsewhere in the Algebra transform
                        return complex;
                    }
                    else
                    {
                        if (this._filter != null || this._unplacedFilters.Count > 0)
                        {
                            //If there's an unplaced FILTER and we're not an OPTIONAL pattern we apply
                            //the FILTER here
                            return new Filter(complex, this.Filter);
                        }
                        else
                        {
                            return complex;
                        }
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