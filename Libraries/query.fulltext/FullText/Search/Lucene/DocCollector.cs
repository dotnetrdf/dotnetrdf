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
        private List<KeyValuePair<int,double>> _docs = new List<KeyValuePair<int,double>>();
        private double _scoreThreshold = Double.NaN;

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
        /// Returns that documents are accepted out of order
        /// </summary>
        /// <returns></returns>
        public override bool AcceptsDocsOutOfOrder()
        {
            return true;
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
