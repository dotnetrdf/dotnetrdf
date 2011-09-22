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
using VDS.RDF.Query.FullText.Search;
using VDS.RDF.Query.Optimisation;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Algebra
{
    /// <summary>
    /// Algebra Operator representing a Full Text Query operation
    /// </summary>
    public class FullTextMatch
        : BaseFullTextOperator
    {
        /// <summary>
        /// Creates a new Full Text Operator
        /// </summary>
        /// <param name="provider">Full Text Search Provider</param>
        /// <param name="algebra">Inner Algebra</param>
        /// <param name="matchVar">Match Variable</param>
        /// <param name="scoreVar">Score Variable</param>
        /// <param name="searchTerm">Search Term</param>
        /// <param name="limit">Result Limit</param>
        /// <param name="scoreThreshold">Score Threshold</param>
        public FullTextMatch(IFullTextSearchProvider provider, ISparqlAlgebra algebra, PatternItem matchVar, PatternItem scoreVar, PatternItem searchTerm, int limit, double scoreThreshold)
            : base(provider, algebra, matchVar, scoreVar, searchTerm, limit, scoreThreshold) { }

        /// <summary>
        /// Creates a new Full Text Operator
        /// </summary>
        /// <param name="provider">Full Text Search Provider</param>
        /// <param name="algebra">Inner Algebra</param>
        /// <param name="matchVar">Match Variable</param>
        /// <param name="scoreVar">Score Variable</param>
        /// <param name="searchTerm">Search Term</param>
        /// <param name="limit">Result Limit</param>
        public FullTextMatch(IFullTextSearchProvider provider, ISparqlAlgebra algebra, PatternItem matchVar, PatternItem scoreVar, PatternItem searchTerm, int limit)
            : this(provider, algebra, matchVar, scoreVar, searchTerm, limit, Double.NaN) { }

        /// <summary>
        /// Creates a new Full Text Operator
        /// </summary>
        /// <param name="provider">Full Text Search Provider</param>
        /// <param name="algebra">Inner Algebra</param>
        /// <param name="matchVar">Match Variable</param>
        /// <param name="searchTerm">Search Term</param>
        /// <param name="limit">Result Limit</param>
        public FullTextMatch(IFullTextSearchProvider provider, ISparqlAlgebra algebra, PatternItem matchVar, PatternItem searchTerm, int limit)
            : this(provider, algebra, matchVar, null, searchTerm, limit, Double.NaN) { }

        /// <summary>
        /// Creates a new Full Text Operator
        /// </summary>
        /// <param name="provider">Full Text Search Provider</param>
        /// <param name="algebra">Inner Algebra</param>
        /// <param name="matchVar">Match Variable</param>
        /// <param name="scoreVar">Score Variable</param>
        /// <param name="searchTerm">Search Term</param>
        public FullTextMatch(IFullTextSearchProvider provider, ISparqlAlgebra algebra, PatternItem matchVar, PatternItem scoreVar, PatternItem searchTerm)
            : this(provider, algebra, matchVar, scoreVar, searchTerm, -1, Double.NaN) { }

        /// <summary>
        /// Creates a new Full Text Operator
        /// </summary>
        /// <param name="provider">Full Text Search Provider</param>
        /// <param name="algebra">Inner Algebra</param>
        /// <param name="matchVar">Match Variable</param>
        /// <param name="scoreVar">Score Variable</param>
        /// <param name="searchTerm">Search Term</param>
        /// <param name="scoreThreshold">Score Threshold</param>
        public FullTextMatch(IFullTextSearchProvider provider, ISparqlAlgebra algebra, PatternItem matchVar, PatternItem scoreVar, PatternItem searchTerm, double scoreThreshold)
            : this(provider, algebra, matchVar, scoreVar, searchTerm, -1, scoreThreshold) { }

        /// <summary>
        /// Creates a new Full Text Operator
        /// </summary>
        /// <param name="provider">Full Text Search Provider</param>
        /// <param name="algebra">Inner Algebra</param>
        /// <param name="matchVar">Match Variable</param>
        /// <param name="searchTerm">Search Term</param>
        /// <param name="scoreThreshold">Score Threshold</param>
        public FullTextMatch(IFullTextSearchProvider provider, ISparqlAlgebra algebra, PatternItem matchVar, PatternItem searchTerm, double scoreThreshold)
            : this(provider, algebra, matchVar, null, searchTerm, -1, scoreThreshold) { }

        /// <summary>
        /// Creates a new Full Text Operator
        /// </summary>
        /// <param name="provider">Full Text Search Provider</param>
        /// <param name="algebra">Inner Algebra</param>
        /// <param name="matchVar">Match Variable</param>
        /// <param name="searchTerm">Search Term</param>
        public FullTextMatch(IFullTextSearchProvider provider, ISparqlAlgebra algebra, PatternItem matchVar, PatternItem searchTerm)
            : this(provider, algebra, matchVar, null, searchTerm, -1, Double.NaN) { }

        /// <summary>
        /// Transforms the Inner Algebra using the given Optimiser
        /// </summary>
        /// <param name="optimiser">Optimiser</param>
        /// <returns></returns>
        public override ISparqlAlgebra Transform(IAlgebraOptimiser optimiser)
        {
            return new FullTextMatch(this.SearchProvider, optimiser.Optimise(this.InnerAlgebra), this.MatchItem, this.ScoreItem, this.SearchTerm, this.Limit, this.ScoreThreshold);
        }

        /// <summary>
        /// Gets the Full Text Results for a specific search query
        /// </summary>
        /// <param name="search">Search Query</param>
        /// <returns></returns>
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

        /// <summary>
        /// Gets the Full Text Results for a specific search query
        /// </summary>
        /// <param name="search">Search Query</param>
        /// <param name="limit">Result Limit</param>
        /// <returns></returns>
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
