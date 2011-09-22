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

namespace VDS.RDF.Query.FullText.Search
{
    /// <summary>
    /// Interface for representing Full Text Search Results
    /// </summary>
    public interface IFullTextSearchResult
    {
        /// <summary>
        /// Gets the Node that was returned for this result
        /// </summary>
        INode Node
        {
            get;
        }

        /// <summary>
        /// Gets the Score for this result
        /// </summary>
        double Score
        {
            get;
        }
    }

    /// <summary>
    /// Basic Implementation of a Full Text Search Result
    /// </summary>
    public sealed class FullTextSearchResult
        : IFullTextSearchResult
    {
        /// <summary>
        /// Creates a new Full Text Search Result
        /// </summary>
        /// <param name="n">Node</param>
        /// <param name="score">Score</param>
        public FullTextSearchResult(INode n, double score)
        {
            this.Node = n;
            this.Score = score;
        }

        /// <summary>
        /// Gets the Node
        /// </summary>
        public INode Node
        {
            get; private set;
        }

        /// <summary>
        /// Gets the Score
        /// </summary>
        public double Score
        {
            get; private set;
        }
    }
}
