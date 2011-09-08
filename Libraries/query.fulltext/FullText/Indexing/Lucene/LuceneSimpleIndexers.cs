using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;
using LucSearch = Lucene.Net.Search;
using VDS.RDF.Query.FullText.Schema;

namespace VDS.RDF.Query.FullText.Indexing.Lucene
{
    public class LucenePredicatesIndexer
        : BaseSimpleLuceneIndexer
    {
        public LucenePredicatesIndexer(Directory indexDir, Analyzer analyzer, IFullTextIndexSchema schema)
            : base(indexDir, analyzer, schema, IndexingMode.Predicates) { }
    }

    public class LuceneObjectsIndexer
        : BaseSimpleLuceneIndexer
    {
        public LuceneObjectsIndexer(Directory indexDir, Analyzer analyzer, IFullTextIndexSchema schema)
            : base(indexDir, analyzer, schema, IndexingMode.Objects) { }
    }

    public class LuceneSubjectsIndexer
        : BaseSimpleLuceneIndexer
    {
        public LuceneSubjectsIndexer(Directory indexDir, Analyzer analyzer, IFullTextIndexSchema schema)
            : base(indexDir, analyzer, schema, IndexingMode.Subjects) { }
    }
}
