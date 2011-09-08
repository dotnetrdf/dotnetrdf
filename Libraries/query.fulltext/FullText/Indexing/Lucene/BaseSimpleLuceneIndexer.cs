using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Lucene.Net.Util;
using LucSearch = Lucene.Net.Search;
using VDS.RDF.Query.FullText.Schema;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query.FullText.Indexing.Lucene
{
    public abstract class BaseSimpleLuceneIndexer
        : BaseSimpleFullTextIndexer
    {
        private IndexingMode _mode;
        private Directory _indexDir;
        private Analyzer _analyzer;
        private IndexWriter _writer;
        private IFullTextIndexSchema _schema;
        private NTriplesFormatter _formatter = new NTriplesFormatter();
        private LucSearch.IndexSearcher _searcher;

        public BaseSimpleLuceneIndexer(Directory indexDir, Analyzer analyzer, IFullTextIndexSchema schema, IndexingMode mode)
        {
            if (this._mode == IndexingMode.Custom) throw new ArgumentException("Cannot use IndexingMode.Custom with the BaseSimpleLuceneIndexer");
            this._mode = mode;
            this._indexDir = indexDir;
            this._analyzer = analyzer;
            this._schema = schema;
            this._writer = new IndexWriter(indexDir, analyzer);
            this._searcher = new LucSearch.IndexSearcher(this._indexDir);
        }

        public override IndexingMode IndexingMode
        {
            get
            {
                return this._mode;
            }
        }

        protected override void Index(INode n, string text)
        {
            Document doc = this.CreateDocument(n, text);
            this._writer.AddDocument(doc);
        }

        protected override void Unindex(INode n, string text)
        {
            LucSearch.TermQuery query = new LucSearch.TermQuery(new Term(this._schema.HashField, this.GetHash(n, text)));
            this._writer.DeleteDocuments(query);
        }

        protected virtual Document CreateDocument(INode n, String text)
        {
            Document doc = new Document();

            //Create the Fields that represent the Node associated with the Indexed Text
            Field nodeTypeField = new Field(this._schema.NodeTypeField, n.NodeType.ToLuceneFieldValue(), Field.Store.YES, Field.Index.NO, Field.TermVector.NO);
            doc.Add(nodeTypeField);
            Field nodeValueField = new Field(this._schema.NodeValueField, n.ToLuceneFieldValue(), Field.Store.YES, Field.Index.NO, Field.TermVector.NO);
            doc.Add(nodeValueField);
            String meta = n.ToLuceneFieldMeta();
            if (meta != null)
            {
                Field nodeMetaField = new Field(this._schema.NodeMetaField, meta, Field.Store.YES, Field.Index.NO, Field.TermVector.NO);
                doc.Add(nodeMetaField);
            }

            //Add the fields for the Indexed Text
            Field indexField = new Field(this._schema.IndexField, text, Field.Store.NO, Field.Index.ANALYZED, Field.TermVector.NO);
            doc.Add(indexField);

            //Create the hash field which we use for Unindexing things
            Field hashField = new Field(this._schema.HashField, this.GetHash(n, text), Field.Store.NO, Field.Index.NOT_ANALYZED, Field.TermVector.NO);
            doc.Add(hashField);

            return doc;
        }

        protected String GetHash(INode n, String text)
        {
            String hashInput = this._formatter.Format(n);
            hashInput += text;
            return hashInput.GetSha256Hash();
        }

        public override void Flush()
        {
            this._writer.Flush();
        }

        protected override void DisposeInternal()
        {
            this._searcher.Close();
            if (this._indexDir.isOpen_ForNUnit) this._writer.Close();
        }
    }
}
