using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Store;
using LucUtil = Lucene.Net.Util;
using VDS.RDF.Query.FullText.Schema;

namespace VDS.RDF.Query.FullText.Search.Lucene
{
    public class LuceneSearchProvider
        : BaseLuceneSearchProvider
    {
        public LuceneSearchProvider(LucUtil.Version ver, Directory indexDir, Analyzer analyzer, IFullTextIndexSchema schema)
            : base(ver, indexDir, analyzer, schema) { }

        public LuceneSearchProvider(LucUtil.Version ver, Directory indexDir, Analyzer analyzer)
            : this(ver, indexDir, analyzer, new DefaultIndexSchema()) { }

        public LuceneSearchProvider(LucUtil.Version ver, Directory indexDir, IFullTextIndexSchema schema)
            : this(ver, indexDir, new StandardAnalyzer(), schema) { }

        public LuceneSearchProvider(LucUtil.Version ver, Directory indexDir)
            : this(ver, indexDir, new StandardAnalyzer(), new DefaultIndexSchema()) { }
    }
}
