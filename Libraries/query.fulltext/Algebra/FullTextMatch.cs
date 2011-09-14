using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query.FullText.Search;
using VDS.RDF.Query.Optimisation;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Algebra
{
    public class FullTextMatch
        : BaseFullTextOperator
    {

        public FullTextMatch(IFullTextSearchProvider provider, ISparqlAlgebra algebra, PatternItem matchVar, PatternItem scoreVar, PatternItem searchTerm, int limit, double scoreThreshold)
            : base(provider, algebra, matchVar, scoreVar, searchTerm, limit, scoreThreshold) { }

        public FullTextMatch(IFullTextSearchProvider provider, ISparqlAlgebra algebra, PatternItem matchVar, PatternItem scoreVar, PatternItem searchTerm, int limit)
            : this(provider, algebra, matchVar, scoreVar, searchTerm, limit, Double.NaN) { }

        public FullTextMatch(IFullTextSearchProvider provider, ISparqlAlgebra algebra, PatternItem matchVar, PatternItem searchTerm, int limit)
            : this(provider, algebra, matchVar, null, searchTerm, limit, Double.NaN) { }

        public FullTextMatch(IFullTextSearchProvider provider, ISparqlAlgebra algebra, PatternItem matchVar, PatternItem scoreVar, PatternItem searchTerm)
            : this(provider, algebra, matchVar, scoreVar, searchTerm, -1, Double.NaN) { }

        public FullTextMatch(IFullTextSearchProvider provider, ISparqlAlgebra algebra, PatternItem matchVar, PatternItem scoreVar, PatternItem searchTerm, double scoreThreshold)
            : this(provider, algebra, matchVar, scoreVar, searchTerm, -1, scoreThreshold) { }

        public FullTextMatch(IFullTextSearchProvider provider, ISparqlAlgebra algebra, PatternItem matchVar, PatternItem searchTerm, double scoreThreshold)
            : this(provider, algebra, matchVar, null, searchTerm, -1, scoreThreshold) { }

        public FullTextMatch(IFullTextSearchProvider provider, ISparqlAlgebra algebra, PatternItem matchVar, PatternItem searchTerm)
            : this(provider, algebra, matchVar, null, searchTerm, -1, Double.NaN) { }

        public override ISparqlAlgebra Transform(IAlgebraOptimiser optimiser)
        {
            throw new NotImplementedException();
        }

        protected override IEnumerable<IFullTextSearchResult> GetResults(string search)
        {
            if (!Double.IsNaN(this.ScoreThreshold))
            {
                //Use a Score Threshold
                return this.SearchProvider.Match(search, this.ScoreThreshold);
            }
            else
            {
                return this.SearchProvider.Match(search);
            }
        }

        protected override IEnumerable<IFullTextSearchResult> GetResults(string search, int limit)
        {
            if (!Double.IsNaN(this.ScoreThreshold))
            {
                //Use a Score Threshold
                return this.SearchProvider.Match(search, this.ScoreThreshold, limit);
            }
            else
            {
                return this.SearchProvider.Match(search, limit);
            }
        }
    }
}
