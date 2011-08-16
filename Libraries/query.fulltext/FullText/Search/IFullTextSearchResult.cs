using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Query.FullText.Search
{
    public interface IFullTextSearchResult
    {
        INode Node
        {
            get;
        }

        double Score
        {
            get;
        }
    }

    public class FullTextSearchResult
        : IFullTextSearchResult
    {
        public FullTextSearchResult(INode n, double score)
        {
            this.Node = n;
            this.Score = score;
        }

        public INode Node
        {
            get; private set;
        }

        public double Score
        {
            get; private set;
        }
    }
}
