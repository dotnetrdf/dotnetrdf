/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

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
