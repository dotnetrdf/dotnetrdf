/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#if !NO_FULLTEXT

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


namespace VDS.RDF.Query.FullText
{
    public static class LuceneTestHarness
    {
        private static bool _init = false;
        private static Directory _indexDir;
        private static IFullTextIndexSchema _schema;
        private static Analyzer _analyzer;

        public readonly static Lucene.Net.Util.Version LuceneVersion = Lucene.Net.Util.Version.LUCENE_30;

        private static void Init()
        {
            _indexDir = new RAMDirectory();
            _schema = new DefaultIndexSchema();
            _analyzer = new StandardAnalyzer(LuceneVersion);
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
#endif