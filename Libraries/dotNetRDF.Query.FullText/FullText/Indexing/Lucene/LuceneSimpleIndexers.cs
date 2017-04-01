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
using Lucene.Net.Store;
using VDS.RDF.Query.FullText.Schema;

namespace VDS.RDF.Query.FullText.Indexing.Lucene
{
    /// <summary>
    /// A Lucene.Net based indexer which indexes the predicate from the triple with the full text of the literal object
    /// </summary>
    public class LucenePredicatesIndexer
        : BaseSimpleLuceneIndexer
    {
        /// <summary>
        /// Creates a Lucene Predicates Indexer
        /// </summary>
        /// <param name="indexDir">Directory</param>
        /// <param name="analyzer">Analyzer</param>
        /// <param name="schema">Index Schema</param>
        public LucenePredicatesIndexer(Directory indexDir, Analyzer analyzer, IFullTextIndexSchema schema)
            : base(indexDir, analyzer, schema, IndexingMode.Predicates) { }
    }

    /// <summary>
    /// A Lucene.Net based indexer which indexes the original object from the triple with the full text of that literal object
    /// </summary>
    public class LuceneObjectsIndexer
        : BaseSimpleLuceneIndexer
    {
        /// <summary>
        /// Creates a Lucene Objects Indexer
        /// </summary>
        /// <param name="indexDir">Directory</param>
        /// <param name="analyzer">Analyzer</param>
        /// <param name="schema">Index Schema</param>
        public LuceneObjectsIndexer(Directory indexDir, Analyzer analyzer, IFullTextIndexSchema schema)
            : base(indexDir, analyzer, schema, IndexingMode.Objects) { }
    }

    /// <summary>
    /// A Lucene.Net based indexer which indexes the original subject from the triple with the full text of the literal object
    /// </summary>
    public class LuceneSubjectsIndexer
        : BaseSimpleLuceneIndexer
    {
        /// <summary>
        /// Creates a Lucene Subjects Indexer
        /// </summary>
        /// <param name="indexDir">Directory</param>
        /// <param name="analyzer">Analyzer</param>
        /// <param name="schema">Index Schema</param>
        public LuceneSubjectsIndexer(Directory indexDir, Analyzer analyzer, IFullTextIndexSchema schema)
            : base(indexDir, analyzer, schema, IndexingMode.Subjects) { }
    }
}
