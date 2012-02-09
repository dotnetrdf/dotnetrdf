/*

Copyright Robert Vesse 2009-12
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
        /// <param name="text">Search Query</param>
        /// <param name="scoreThreshold">Score Threshold</param>
        /// <param name="limit">Result Limit</param>
        /// <returns></returns>
        IEnumerable<IFullTextSearchResult> Match(String text, double scoreThreshold, int limit);

        /// <summary>
        /// Searches for matches for specific text
        /// </summary>
        /// <param name="text">Search Query</param>
        /// <param name="scoreThreshold">Score Threshold</param>
        IEnumerable<IFullTextSearchResult> Match(String text, double scoreThreshold);

        /// <summary>
        /// Searches for matches for specific text
        /// </summary>
        /// <param name="text">Search Query</param>
        /// <param name="limit">Result Limit</param>
        IEnumerable<IFullTextSearchResult> Match(String text, int limit);

        /// <summary>
        /// Searches for matches for specific text
        /// </summary>
        /// <param name="text">Search Query</param>
        IEnumerable<IFullTextSearchResult> Match(String text);

        /// <summary>
        /// Gets whether the search provider is automatically synced with the index i.e. whether queries will always return results based on the latest state of the index
        /// </summary>
        /// <remarks>
        /// Some implementations may allow this behaviour to be configured while for others this feature may always be on/off
        /// </remarks>
        bool IsAutoSynced
        {
            get;
        }
    }
}
