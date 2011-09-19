using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query.FullText.Search;
using VDS.RDF.Query.Optimisation;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Algebra
{
    public abstract class BaseFullTextOperator
        : IUnaryOperator
    {
        private IFullTextSearchProvider _provider;
        private int _limit = -1;
        private double _scoreThreshold = Double.NaN;
        private PatternItem _matchVar, _scoreVar, _searchTerm;

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

        public BaseFullTextOperator(IFullTextSearchProvider provider, ISparqlAlgebra algebra, PatternItem matchVar, PatternItem scoreVar, PatternItem searchTerm, int limit)
            : this(provider, algebra, matchVar, scoreVar, searchTerm, limit, Double.NaN) { }

        public BaseFullTextOperator(IFullTextSearchProvider provider, ISparqlAlgebra algebra, PatternItem matchVar, PatternItem searchTerm, int limit)
            : this(provider, algebra, matchVar, null, searchTerm, limit, Double.NaN) { }

        public BaseFullTextOperator(IFullTextSearchProvider provider, ISparqlAlgebra algebra, PatternItem matchVar, PatternItem scoreVar, PatternItem searchTerm, double scoreThreshold)
            : this(provider, algebra, matchVar, scoreVar, searchTerm, -1, scoreThreshold) { }

        public BaseFullTextOperator(IFullTextSearchProvider provider, ISparqlAlgebra algebra, PatternItem matchVar, PatternItem searchTerm, double scoreThreshold)
            : this(provider, algebra, matchVar, null, searchTerm, -1, scoreThreshold) { }

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

        public abstract ISparqlAlgebra Transform(IAlgebraOptimiser optimiser);

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
                if (this._scoreVar.VariableName == null) throw new RdfQueryException("Queries using full text search that wish to return result scores must provide a variable");
                if (this._scoreVar.VariableName != null && context.InputMultiset.ContainsVariable(this._scoreVar.VariableName)) throw new RdfQueryException("Queries using full text search that wish to return result scores must use an unbound variable to do so");
            }

            //Next ensure that the search text is a node and not a variable
            if (this._searchTerm.VariableName != null) throw new RdfQueryException("Queries using full text search must provide a constant value for the search term");
            INode searchNode = ((NodeMatchPattern)this._searchTerm).Node;
            if (searchNode.NodeType != NodeType.Literal) throw new RdfQueryException("Queries using full text search must use a literal value for the search term");
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

        protected abstract IEnumerable<IFullTextSearchResult> GetResults(String search);

        protected abstract IEnumerable<IFullTextSearchResult> GetResults(String search, int limit);

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

        public SparqlQuery ToQuery()
        {
            throw new NotImplementedException();
        }

        public GraphPattern ToGraphPattern()
        {
            throw new NotImplementedException();
        }

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
