/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/

using System;

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

        /// <summary>
        /// Gets the Graph URI for this result
        /// </summary>
        Uri GraphUri
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
        /// <param name="graphUri">Graph URI</param>
        /// <param name="n">Node</param>
        /// <param name="score">Score</param>
        public FullTextSearchResult(Uri graphUri, INode n, double score)
        {
            this.GraphUri = graphUri;
            this.Node = n;
            this.Score = score;
        }

        /// <summary>
        /// Creates a new Full Text Search Result
        /// </summary>
        /// <param name="n">Node</param>
        /// <param name="score">Score</param>
        public FullTextSearchResult(INode n, double score)
            : this(null, n, score) { }

        /// <summary>
        /// Gets the Node
        /// </summary>
        public INode Node
        {
            get; 
            private set;
        }

        /// <summary>
        /// Gets the Score
        /// </summary>
        public double Score
        {
            get; 
            private set;
        }

        /// <summary>
        /// Gets the Graph URI of the result
        /// </summary>
        public Uri GraphUri
        {
            get;
            private set;
        }
    }
}
