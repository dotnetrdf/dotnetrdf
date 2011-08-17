using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Index;
using Lucene.Net.Search;

namespace VDS.RDF.Query.FullText.Search.Lucene
{
    public class DocCollector
        : Collector
    {
        private Scorer _scorer;
        private int _currBase = 0;
        private List<KeyValuePair<int,double>> _docs = new List<KeyValuePair<int,double>>();
        private double _scoreThreshold = Double.NaN;

        public DocCollector()
        {

        }

        public DocCollector(double scoreThreshold)
            : this()
        {
            this._scoreThreshold = scoreThreshold;
        }

        public IEnumerable<KeyValuePair<int,double>> Documents
        {
            get
            {
                return this._docs;
            }
        }

        public override bool AcceptsDocsOutOfOrder()
        {
            return true;
        }

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

        public override void SetNextReader(IndexReader reader, int docBase)
        {
            this._currBase = docBase;
        }

        public override void SetScorer(Scorer scorer)
        {
            this._scorer = scorer;
        }
    }
}
