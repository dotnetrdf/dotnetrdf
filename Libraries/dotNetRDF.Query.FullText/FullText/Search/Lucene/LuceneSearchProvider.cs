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

using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Store;
using LucUtil = Lucene.Net.Util;
using VDS.RDF.Query.FullText.Schema;

namespace VDS.RDF.Query.FullText.Search.Lucene
{
    /// <summary>
    /// A Full Text Search provider using Lucene.Net
    /// </summary>
    public class LuceneSearchProvider
        : BaseLuceneSearchProvider
    {
        /// <summary>
        /// Creates a new Lucene Search Provider
        /// </summary>
        /// <param name="ver">Version</param>
        /// <param name="indexDir">Directory</param>
        /// <param name="analyzer">Analyzer</param>
        /// <param name="schema">Index Schema</param>
        /// <param name="autoSync">Whether to keep the search provider in sync with the index</param>
        public LuceneSearchProvider(LucUtil.Version ver, Directory indexDir, Analyzer analyzer, IFullTextIndexSchema schema, bool autoSync)
            : base(ver, indexDir, analyzer, schema, autoSync) { }

        /// <summary>
        /// Creates a new Lucene Search Provider
        /// </summary>
        /// <param name="ver">Version</param>
        /// <param name="indexDir">Directory</param>
        /// <param name="analyzer">Analyzer</param>
        /// <param name="schema">Index Schema</param>
        public LuceneSearchProvider(LucUtil.Version ver, Directory indexDir, Analyzer analyzer, IFullTextIndexSchema schema)
            : this(ver, indexDir, analyzer, schema, true) { }

        /// <summary>
        /// Creates a new Lucene Search Provider
        /// </summary>
        /// <param name="ver">Version</param>
        /// <param name="indexDir">Directory</param>
        /// <param name="analyzer">Analyzer</param>
        /// <param name="autoSync">Whether to keep the search provider in sync with the index</param>
        /// <remarks>
        /// Uses the <see cref="DefaultIndexSchema">DefaultIndexSchema</see> as the schema
        /// </remarks>
        public LuceneSearchProvider(LucUtil.Version ver, Directory indexDir, Analyzer analyzer, bool autoSync)
            : this(ver, indexDir, analyzer, new DefaultIndexSchema(), autoSync) { }

        /// <summary>
        /// Creates a new Lucene Search Provider
        /// </summary>
        /// <param name="ver">Version</param>
        /// <param name="indexDir">Directory</param>
        /// <param name="analyzer">Analyzer</param>
        /// <remarks>
        /// Uses the <see cref="DefaultIndexSchema">DefaultIndexSchema</see> as the schema
        /// </remarks>
        public LuceneSearchProvider(LucUtil.Version ver, Directory indexDir, Analyzer analyzer)
            : this(ver, indexDir, analyzer, true) { }

        /// <summary>
        /// Creates a new Lucene Search Provider
        /// </summary>
        /// <param name="ver">Version</param>
        /// <param name="indexDir">Directory</param>
        /// <param name="schema">Index Schema</param>
        /// <param name="autoSync">Whether to keep the search provider in sync with the index</param>
        /// <remarks>
        /// Uses the <see cref="StandardAnalyzer">StandardAnalyzer</see> as the analyzer
        /// </remarks>
        public LuceneSearchProvider(LucUtil.Version ver, Directory indexDir, IFullTextIndexSchema schema, bool autoSync)
            : this(ver, indexDir, new StandardAnalyzer(ver), schema, autoSync) { }

        
        /// <summary>
        /// Creates a new Lucene Search Provider
        /// </summary>
        /// <param name="ver">Version</param>
        /// <param name="indexDir">Directory</param>
        /// <param name="schema">Index Schema</param>
        /// <remarks>
        /// Uses the <see cref="StandardAnalyzer">StandardAnalyzer</see> as the analyzer
        /// </remarks>
        public LuceneSearchProvider(LucUtil.Version ver, Directory indexDir, IFullTextIndexSchema schema)
            : this(ver, indexDir, schema, true) { }

        /// <summary>
        /// Creates a new Lucene Search Provider
        /// </summary>
        /// <param name="ver">Version</param>
        /// <param name="indexDir">Directory</param>
        /// <param name="autoSync">Whether to jeep the search provider in sync with the index</param>
        /// <remarks>
        /// Uses the <see cref="DefaultIndexSchema">DefaultIndexSchema</see> as the schema and the <see cref="StandardAnalyzer">StandardAnalyzer</see> as the analyzer
        /// </remarks>
        public LuceneSearchProvider(LucUtil.Version ver, Directory indexDir, bool autoSync)
            : this(ver, indexDir, new StandardAnalyzer(ver), new DefaultIndexSchema(), autoSync) { }

        /// <summary>
        /// Creates a new Lucene Search Provider
        /// </summary>
        /// <param name="ver">Version</param>
        /// <param name="indexDir">Directory</param>
        /// <remarks>
        /// Uses the <see cref="DefaultIndexSchema">DefaultIndexSchema</see> as the schema and the <see cref="StandardAnalyzer">StandardAnalyzer</see> as the analyzer
        /// </remarks>
        public LuceneSearchProvider(LucUtil.Version ver, Directory indexDir)
            : this(ver, indexDir, true) { }
    }
}
