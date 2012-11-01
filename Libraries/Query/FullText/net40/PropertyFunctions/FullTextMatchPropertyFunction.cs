/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

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
using VDS.RDF.Nodes;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.FullText;
using VDS.RDF.Query.FullText.Indexing;
using VDS.RDF.Query.FullText.Search;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.PropertyFunctions
{
    /// <summary>
    /// Property Function which does full text matching
    /// </summary>
    public class FullTextMatchPropertyFunction
        : ISparqlPropertyFunction
    {
        private PatternItem _matchVar, _scoreVar, _searchVar;
        private List<String> _vars = new List<String>();
        private int? _limit;
        private double? _threshold;

        /// <summary>
        /// Constructs a Full Text Match property function
        /// </summary>
        /// <param name="info">Property Function information</param>
        public FullTextMatchPropertyFunction(PropertyFunctionInfo info)
        {
            if (info == null) throw new ArgumentNullException("info");
            if (!EqualityHelper.AreUrisEqual(info.FunctionUri, this.FunctionUri)) throw new ArgumentException("Property Function information is not valid for this function");

            //Get basic arguments
            this._matchVar = info.SubjectArgs[0];
            if (this._matchVar.VariableName != null) this._vars.Add(this._matchVar.VariableName);
            if (info.SubjectArgs.Count == 2)
            {
                this._scoreVar = info.SubjectArgs[1];
                if (this._scoreVar.VariableName != null) this._vars.Add(this._scoreVar.VariableName);
            }

            //Check extended arguments
            this._searchVar = info.ObjectArgs[0];
            if (this._searchVar.VariableName != null) this._vars.Add(this._searchVar.VariableName);
            switch (info.ObjectArgs.Count)
            {
                case 1:
                    break;
                case 2:
                    PatternItem arg = info.ObjectArgs[1];
                    if (arg.VariableName != null) throw new RdfQueryException("Cannot use a variable as the limit/score threshold for full text queries, must use a numeric constant");
                    IValuedNode n = ((NodeMatchPattern)arg).Node.AsValuedNode();
                    switch (n.NumericType)
                    {
                        case SparqlNumericType.Integer:
                            this._limit = (int)n.AsInteger();
                            break;
                        case SparqlNumericType.Decimal:
                        case SparqlNumericType.Double:
                        case SparqlNumericType.Float:
                            this._threshold = n.AsDouble();
                            break;
                        default:
                            throw new RdfQueryException("Cannot use a non-numeric constant as the limit/score threshold for full text queries, must use a numeric constant");
                    }
                    break;
                case 3:
                default:
                    PatternItem arg1 = info.ObjectArgs[1];
                    PatternItem arg2 = info.ObjectArgs[2];
                    if (arg1.VariableName != null || arg2.VariableName != null) throw new RdfQueryException("Cannot use a variable as the limit/score threshold for full text queries, must use a numeric constant");
                    IValuedNode n1 = ((NodeMatchPattern)arg1).Node.AsValuedNode();
                    switch (n1.NumericType)
                    {
                        case SparqlNumericType.NaN:
                            throw new RdfQueryException("Cannot use a non-numeric constant as the score threshold for full text queries, must use a numeric constant");
                        default:
                            this._threshold = n1.AsDouble();
                            break;
                    }
                    IValuedNode n2 = ((NodeMatchPattern)arg2).Node.AsValuedNode();
                    switch (n2.NumericType)
                    {
                        case SparqlNumericType.NaN:
                            throw new RdfQueryException("Cannot use a non-numeric constant as the limit for full text queries, must use a numeric constant");
                        default:
                            this._limit = (int)n2.AsInteger();
                            break;
                    }
                    break;
            }
        }

        /// <summary>
        /// Gets the Function URI for the property function
        /// </summary>
        public Uri FunctionUri
        {
            get 
            {
                return UriFactory.Create(FullTextHelper.FullTextMatchPredicateUri); 
            }
        }

        /// <summary>
        /// Gets the Variables used in the property function
        /// </summary>
        public IEnumerable<String> Variables
        {
            get
            {
                return this._vars;
            }
        }

        /// <summary>
        /// Evaluates the property function
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <returns></returns>
        public BaseMultiset Evaluate(SparqlEvaluationContext context)
        {
            //The very first thing we must do is check the incoming input
            if (context.InputMultiset is NullMultiset) return context.InputMultiset; //Can abort evaluation if input is null
            if (context.InputMultiset.IsEmpty) return context.InputMultiset; //Can abort evaluation if input is null

            //Then we need to retrieve the full text search provider
            IFullTextSearchProvider provider = context[FullTextHelper.ContextKey] as IFullTextSearchProvider;
            if (provider == null) throw new FullTextQueryException("No Full Text Search Provider is available, please ensure you attach a FullTextQueryOptimiser to your query");

            //First determine whether we can apply the limit when talking to the provider
            //Essentially as long as the Match Variable (the one we'll bind results to) is not already
            //bound AND we are actually using a limit
            bool applyLimitDirect = this._limit.HasValue && this._limit.Value > -1 && this._matchVar.VariableName != null && !context.InputMultiset.ContainsVariable(this._matchVar.VariableName);

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
            if (this._searchVar.VariableName != null) throw new FullTextQueryException("Queries using full text search must provide a constant value for the search term");
            INode searchNode = ((NodeMatchPattern)this._searchVar).Node;
            if (searchNode.NodeType != NodeType.Literal) throw new FullTextQueryException("Queries using full text search must use a literal value for the search term");
            String search = ((ILiteralNode)searchNode).Value;

            //Determine which graphs we are operating over
            IEnumerable<Uri> graphUris = context.Data.ActiveGraphUris;

            //Now we can use the full text search provider to start getting results
            context.OutputMultiset = new Multiset();
            IEnumerable<IFullTextSearchResult> results = applyLimitDirect ? this.GetResults(graphUris, provider, search, this._limit.Value) : this.GetResults(graphUris, provider, search);
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
        /// <param name="graphUris">Graph URIs</param>
        /// <param name="provider">Search Provider</param>
        /// <param name="search">Search Query</param>
        /// <returns></returns>
        protected IEnumerable<IFullTextSearchResult> GetResults(IEnumerable<Uri> graphUris, IFullTextSearchProvider provider, string search)
        {
            if (this._threshold.HasValue)
            {
                //Use a Score Threshold
                return provider.Match(graphUris, search, this._threshold.Value);
            }
            else
            {
                return provider.Match(graphUris, search);
            }
        }

        /// <summary>
        /// Gets the Full Text Results for a specific search query
        /// </summary>
        /// <param name="graphUris">Graph URIs</param>
        /// <param name="provider">Search Provider</param>
        /// <param name="search">Search Query</param>
        /// <param name="limit">Result Limit</param>
        /// <returns></returns>
        protected virtual IEnumerable<IFullTextSearchResult> GetResults(IEnumerable<Uri> graphUris, IFullTextSearchProvider provider, string search, int limit)
        {
            if (this._threshold.HasValue)
            {
                //Use a Score Threshold
                return provider.Match(graphUris, search, this._threshold.Value, limit);
            }
            else
            {
                return provider.Match(graphUris, search, limit);
            }
        }
    }
}
