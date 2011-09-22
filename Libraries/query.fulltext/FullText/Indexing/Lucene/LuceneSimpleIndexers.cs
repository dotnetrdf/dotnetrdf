/*

Copyright Robert Vesse 2009-11
rvesse@vdesign-studios.com

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
