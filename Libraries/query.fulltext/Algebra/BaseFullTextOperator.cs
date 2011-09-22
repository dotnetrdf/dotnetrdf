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
using VDS.RDF.Query.FullText;
using VDS.RDF.Query.FullText.Search;
using VDS.RDF.Query.Optimisation;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Algebra
{
    /// <summary>
    /// Abstract Base Class for providing Full Text Query as an algebra operator
    /// </summary>
    public abstract class BaseFullTextOperator
        : IUnaryOperator
    {
        private IFullTextSearchProvider _provider;
        private int _limit = -1;
        private double _scoreThreshold = Double.NaN;
        private PatternItem _matchVar, _scoreVar, _searchTerm;

        /// <summary>
        /// Creates a new Full Text Operator
        /// </summary>
        /// <param name="provider">Search Provider</param>
        /// <param name="algebra">Inner Algebra</param>
        /// <param name="matchVar">Match Variable</param>
        /// <param name="scoreVar">Score Variable</param>
        /// <param name="searchTerm">Search Term</param>
        /// <param name="limit">Limit</param>
        /// <param name="scoreThreshold">Score Threshold</param>
        public BaseFullTextOperator(IFullTextSearchProvider provider, ISparqlAlgebra algebra, PatternItem matchVar, PatternItem scoreVar, PatternItem searchTerm, int limit, double scoreThreshold)
        {
            this._provider = provider;
            this.InnerAlgebra = algebra;
            this._matchVar = matchVar;
            this._scoreVar = scoreVar;
            this._searchTerm = searchTerm;
            this._limit = limit;
            this._scoreThreshold = scoreThreshold;
        }

        /// <summary>
        /// Creates a new Full Text Operator
        /// </summary>
        /// <param name="provider">Search Provider</param>
        /// <param name="algebra">Inner Algebra</param>
        /// <param name="matchVar">Match Variable</param>
        /// <param name="scoreVar">Score Variable</param>
        /// <param name="searchTerm">Search Term</param>
        /// <param name="limit">Limit</param>
        public BaseFullTextOperator(IFullTextSearchProvider provider, ISparqlAlgebra algebra, PatternItem matchVar, PatternItem scoreVar, PatternItem searchTerm, int limit)
            : this(provider, algebra, matchVar, scoreVar, searchTerm, limit, Double.NaN) { }

        /// <summary>
        /// Creates a new Full Text Operator
        /// </summary>
        /// <param name="provider">Search Provider</param>
        /// <param name="algebra">Inner Algebra</param>
        /// <param name="matchVar">Match Variable</param>
        /// <param name="searchTerm">Search Term</param>
        /// <param name="limit">Limit</param>
        public BaseFullTextOperator(IFullTextSearchProvider provider, ISparqlAlgebra algebra, PatternItem matchVar, PatternItem searchTerm, int limit)
            : this(provider, algebra, matchVar, null, searchTerm, limit, Double.NaN) { }

        /// <summary>
        /// Creates a new Full Text Operator
        /// </summary>
        /// <param name="provider">Search Provider</param>
        /// <param name="algebra">Inner Algebra</param>
        /// <param name="matchVar">Match Variable</param>
        /// <param name="scoreVar">Score Variable</param>
        /// <param name="searchTerm">Search Term</param>
        /// <param name="scoreThreshold">Score Threshold</param>
        public BaseFullTextOperator(IFullTextSearchProvider provider, ISparqlAlgebra algebra, PatternItem matchVar, PatternItem scoreVar, PatternItem searchTerm, double scoreThreshold)
            : this(provider, algebra, matchVar, scoreVar, searchTerm, -1, scoreThreshold) { }

        /// <summary>
        /// Creates a new Full Text Operator
        /// </summary>
        /// <param name="provider">Search Provider</param>
        /// <param name="algebra">Inner Algebra</param>
        /// <param name="matchVar">Match Variable</param>
        /// <param name="searchTerm">Search Term</param>
        /// <param name="scoreThreshold">Score Threshold</param>
        public BaseFullTextOperator(IFullTextSearchProvider provider, ISparqlAlgebra algebra, PatternItem matchVar, PatternItem searchTerm, double scoreThreshold)
            : this(provider, algebra, matchVar, null, searchTerm, -1, scoreThreshold) { }

        /// <summary>
        /// Gets the Inner Algebra
        /// </summary>
        public ISparqlAlgebra InnerAlgebra
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the Full Text Search provider to use
        /// </summary>
        public IFullTextSearchProvider SearchProvider
        {
            get
            {
                return this._provider;
            }
        }

        /// <summary>
        /// Gets the Node/Variable that results must match or are bound to as appropriate
        /// </summary>
        public PatternItem MatchItem
        {
            get
            {
                return this._matchVar;
            }
        }

        /// <summary>
        /// Gets the Variable (if any) that result scores are bound to
        /// </summary>
        public PatternItem ScoreItem
        {
            get
            {
                return this._scoreVar;
            }
        }

        /// <summary>
        /// Gets the Search Term/Query to use
        /// </summary>
        public PatternItem SearchTerm
        {
            get
            {
                return this._searchTerm;
            }
        }

        /// <summary>
        /// Gets the Score Threshold (returns NaN if no threshold)
        /// </summary>
        public double ScoreThreshold
        {
            get
            {
                return this._scoreThreshold;
            }
        }

        /// <summary>
        /// Gets the Result Limit (returns -1 if no limit)
        /// </summary>
        public int Limit
        {
            get
            {
                return this._limit;
            }
        }

        /// <summary>
        /// Transforms the Inner Algebra using the given Optimiser
        /// </summary>
        /// <param name="optimiser">Optimiser</param>
        /// <returns></returns>
        public abstract ISparqlAlgebra Transform(IAlgebraOptimiser optimiser);

        /// <summary>
        /// Evaluates the Algebra in the given Context
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <returns></returns>
        public BaseMultiset Evaluate(SparqlEvaluationContext context)
        {
            //The very first thing we must do is evaluate the inner algebra
            BaseMultiset innerResult = context.Evaluate(this.InnerAlgebra);
            if (innerResult is NullMultiset) return innerResult; //Can abort evaluation if inner evaluation gives null
            context.InputMultiset = innerResult;

            //First determine whether we can apply the limit when talking to the provider
            //Essentially as long as the Match Variable (the one we'll bind results to) is not already
            //bound AND we are actually using a limit
            bool applyLimitDirect = this._limit > -1 && this._matchVar.VariableName != null && !context.InputMultiset.ContainsVariable(this._matchVar.VariableName);

            //Is there a constant for the Match Item?  If so extract it now
            //Otherwise are we needing to check against existing bindings
            INode matchConstant = null;
            bool checkExisting = false;
            HashSet<INode> existing = null;
            if (this._matchVar.VariableName == null)
            {
                matchConstant = ((NodeMatchPattern)this._matchVar).Node;
            }
            else if (this._matchVar.VariableName != null && context.InputMultiset.ContainsVariable(this._matchVar.VariableName))
            {
                checkExisting = true;
                existing = new HashSet<INode>();
                foreach (INode n in context.InputMultiset.Sets.Select(s => s[this._matchVar.VariableName]).Where(s => s != null))
                {
                    existing.Add(n);
                }
            }

            //Then check that the score variable is not already bound, if so error
            //If a Score Variable is provided and it is OK then we'll bind scores at a later stage
            if (this._scoreVar != null)
            {
                if (this._scoreVar.VariableName == null) throw new FullTextQueryException("Queries using full text search that wish to return result scores must provide a variable");
                if (this._scoreVar.VariableName != null && context.InputMultiset.ContainsVariable(this._scoreVar.VariableName)) throw new FullTextQueryException("Queries using full text search that wish to return result scores must use an unbound variable to do so");
            }

            //Next ensure that the search text is a node and not a variable
            if (this._searchTerm.VariableName != null) throw new FullTextQueryException("Queries using full text search must provide a constant value for the search term");
            INode searchNode = ((NodeMatchPattern)this._searchTerm).Node;
            if (searchNode.NodeType != NodeType.Literal) throw new FullTextQueryException("Queries using full text search must use a literal value for the search term");
            String search = ((ILiteralNode)searchNode).Value;

            //Now we can use the full text search provider to start getting results
            context.OutputMultiset = new Multiset();
            IEnumerable<IFullTextSearchResult> results = applyLimitDirect ? this.GetResults(search, this._limit) : this.GetResults(search);
            int r = 0;
            String matchVar = this._matchVar.VariableName;
            String scoreVar = this._scoreVar != null ? this._scoreVar.VariableName : null;
            foreach (IFullTextSearchResult result in results)
            {
                if (matchConstant != null)
                {
                    //Check against constant if present
                    if (result.Node.Equals(matchConstant))
                    {
                        r++;
                        context.OutputMultiset.Add(result.ToSet(matchVar, scoreVar));
                    }
                }
                else if (checkExisting)
                {
                    //Check against existing bindings if present
                    if (existing.Contains(result.Node))
                    {
                        r++;
                        context.OutputMultiset.Add(result.ToSet(matchVar, scoreVar));
                    }
                }
                else
                {
                    //Otherwise all results are acceptable
                    r++;
                    context.OutputMultiset.Add(result.ToSet(matchVar, scoreVar));
                }

                //Apply the limit locally if necessary
                if (!applyLimitDirect && this._limit > -1 && r >= this._limit) break;
            }

            return context.OutputMultiset;
        }

        /// <summary>
        /// Gets the Full Text Results for a specific search query
        /// </summary>
        /// <param name="search">Search Query</param>
        /// <returns></returns>
        protected abstract IEnumerable<IFullTextSearchResult> GetResults(String search);

        /// <summary>
        /// Gets the Full Text Results for a specific search query
        /// </summary>
        /// <param name="search">Search Query</param>
        /// <param name="limit">Result Limit</param>
        /// <returns></returns>
        protected abstract IEnumerable<IFullTextSearchResult> GetResults(String search, int limit);

        /// <summary>
        /// Gets the Variables used in the algebra
        /// </summary>
        public IEnumerable<string> Variables
        {
            get 
            {
                List<String> vars = new List<string>();
                if (this._matchVar.VariableName != null) vars.Add(this._matchVar.VariableName);
                if (this._scoreVar.VariableName != null) vars.Add(this._scoreVar.VariableName);
                if (this._searchTerm.VariableName != null) vars.Add(this._searchTerm.VariableName);
                vars.AddRange(this.InnerAlgebra.Variables);

                return vars.Distinct();
            }
        }

        /// <summary>
        /// Converts the Algebra back to a SPARQL Query
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotSupportedException">Thrown as this operator cannot be converted to a SPARQL Query</exception>
        public SparqlQuery ToQuery()
        {
            throw new NotSupportedException("Full Text Operators cannot be converted back into Queries");
        }

        /// <summary>
        /// Converts the Algebra back to a Graph Pattern
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotSupportedException">Thrown as this operator cannot be converted to a Graph Pattern</exception>
        public GraphPattern ToGraphPattern()
        {
            throw new NotSupportedException("Full Text Operators cannot be converted back into Graph Patterns");
        }

        /// <summary>
        /// Gets the String representation of the Algebra
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (this._scoreVar != null)
            {
                return "FullTextMatch(" + this.MatchItem.ToString() + ", " + this.SearchTerm.ToString() + ", " + this.ScoreItem.ToString() + ", " + this.InnerAlgebra.ToString() + ")";
            }
            else
            {
                return "FullTextMatch(" + this.MatchItem.ToString() + ", " + this.SearchTerm.ToString() + ", " + this.InnerAlgebra.ToString() + ")";
            }
        }
    }
}
