using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Query.FullText.Search
{
    /// <summary>
    /// Interface for classes that provide full text search capability
    /// </summary>
    public interface IFullTextSearchProvider
    {
        /// <summary>
        /// Searches for matches for specific text
        /// </summary>
        /// <param name="text"></param>
        /// <param name="scoreThreshold"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        IEnumerable<IFullTextSearchResult> Search(String text, double scoreThreshold, int limit);

        /// <summary>
        /// Searches for matches for specific text
        /// </summary>
        IEnumerable<IFullTextSearchResult> Search(String text, double scoreThreshold);

        /// <summary>
        /// Searches for matches for specific text
        /// </summary>
        IEnumerable<IFullTextSearchResult> Search(String text, int limit);

        /// <summary>
        /// Searches for matches for specific text
        /// </summary>
        IEnumerable<IFullTextSearchResult> Search(String text);

        /// <summary>
        /// Searches for matches using the underlying full text search systems own query syntax
        /// </summary>
        IEnumerable<IFullTextSearchResult> Query(String query, double scoreThreshold, int limit);

        /// <summary>
        /// Searches for matches using the underlying full text search systems own query syntax
        /// </summary>
        IEnumerable<IFullTextSearchResult> Query(String query, double scoreThreshold);

        /// <summary>
        /// Searches for matches using the underlying full text search systems own query syntax
        /// </summary>
        IEnumerable<IFullTextSearchResult> Query(String query, int limit);

        /// <summary>
        /// Searches for matches using the underlying full text search systems own query syntax
        /// </summary>
        IEnumerable<IFullTextSearchResult> Query(String query);
    }
}
