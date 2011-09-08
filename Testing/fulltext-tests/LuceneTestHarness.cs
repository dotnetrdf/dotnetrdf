using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Store;
using VDS.RDF.Query;
using VDS.RDF.Query.FullText;
using VDS.RDF.Query.FullText.Schema;


namespace VDS.RDF.Test.Query.FullText
{
    public static class LuceneTestHarness
    {
        private static bool _init = false;
        private static Directory _indexDir;
        private static IFullTextIndexSchema _schema;
        private static Analyzer _analyzer;

        public readonly static Lucene.Net.Util.Version LuceneVersion = Lucene.Net.Util.Version.LUCENE_29;

        private static void Init()
        {
            _indexDir = new RAMDirectory();
            _schema = new DefaultIndexSchema();
            _analyzer = new StandardAnalyzer();
            _init = true;
        }

        public static Directory Index
        {
            get
            {
                if (!_init) Init();
                if (!_indexDir.isOpen_ForNUnit) Init();
                return _indexDir;
            }
        }

        public static IFullTextIndexSchema Schema
        {
            get
            {
                if (!_init) Init();
                return _schema;
            }
        }

        public static Analyzer Analyzer
        {
            get
            {
                if (!_init) Init();
                return _analyzer;
            }
        }
    }
}
