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
using System.Collections.Generic;
using Lucene.Net.Index;
using Lucene.Net.Search;

namespace VDS.RDF.Query.FullText.Search.Lucene
{
    /// <summary>
    /// Collector Implementation used as part of our Lucene.Net integration
    /// </summary>
    class DocCollector
        : Collector
    {
        private Scorer _scorer;
        private int _currBase = 0;
        private readonly List<KeyValuePair<int,double>> _docs = new List<KeyValuePair<int,double>>();
        private readonly double _scoreThreshold = Double.NaN;

        /// <summary>
        /// Creates a new Collector
        /// </summary>
        public DocCollector()
        {

        }

        /// <summary>
        /// Creates a new Collector with a given score threshold
        /// </summary>
        /// <param name="scoreThreshold">Score Threshold</param>
        public DocCollector(double scoreThreshold)
            : this()
        {
            this._scoreThreshold = scoreThreshold;
        }

        /// <summary>
        /// Gets the Documents that have been collected
        /// </summary>
        public IEnumerable<KeyValuePair<int,double>> Documents
        {
            get
            {
                return this._docs;
            }
        }

        /// <summary>
        /// Gets the number of collected documents
        /// </summary>
        public int Count
        {
            get
            {
                return this._docs.Count;
            }
        }

        /// <summary>
        /// Returns that documents are accepted out of order
        /// </summary>
        /// <returns></returns>
        public override bool AcceptsDocsOutOfOrder
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Collects a document if it meets the score threshold (if any)
        /// </summary>
        /// <param name="doc">Document ID</param>
        public override void Collect(int doc)
        {
            double score = this._scorer.Score();
            if (!Double.IsNaN(this._scoreThreshold))
            {
                if (score >= this._scoreThreshold)
                {
                    this._docs.Add(new KeyValuePair<int, double>(doc + this._currBase, score));
                }
            }
            else
            {
                this._docs.Add(new KeyValuePair<int, double>(doc + this._currBase, score));
            }
        }

        /// <summary>
        /// Sets the Next Reader
        /// </summary>
        /// <param name="reader">Index Reader</param>
        /// <param name="docBase">Document Base</param>
        public override void SetNextReader(IndexReader reader, int docBase)
        {
            this._currBase = docBase;
        }

        /// <summary>
        /// Sets the Scorer
        /// </summary>
        /// <param name="scorer">Scorer</param>
        public override void SetScorer(Scorer scorer)
        {
            this._scorer = scorer;
        }
    }
}
