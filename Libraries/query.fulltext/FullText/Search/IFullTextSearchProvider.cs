using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Query.FullText.Search
{
    /// <summary>
    /// Interface for classes that provide full text search capability
    /// </summary>
    /// <remarks>
    /// The <strong>Match()</strong> methods may allow for provider specific query syntaxes depending on the the underlying provider
    /// </remarks>
    public interface IFullTextSearchProvider
        : IDisposable
    {
        /// <summary>
        /// Searches for matches for specific text
        /// </summary>
        /// <param name="text"></param>
        /// <param name="scoreThreshold"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        IEnumerable<IFullTextSearchResult> Match(String text, double scoreThreshold, int limit);

        /// <summary>
        /// Searches for matches for specific text
        /// </summary>
        IEnumerable<IFullTextSearchResult> Match(String text, double scoreThreshold);

        /// <summary>
        /// Searches for matches for specific text
        /// </summary>
        IEnumerable<IFullTextSearchResult> Match(String text, int limit);

        /// <summary>
        /// Searches for matches for specific text
        /// </summary>
        IEnumerable<IFullTextSearchResult> Match(String text);
    }
}
