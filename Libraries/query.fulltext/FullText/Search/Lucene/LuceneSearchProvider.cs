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
        public LuceneSearchProvider(LucUtil.Version ver, Directory indexDir, Analyzer analyzer, IFullTextIndexSchema schema)
            : base(ver, indexDir, analyzer, schema) { }

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
            : this(ver, indexDir, analyzer, new DefaultIndexSchema()) { }

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
            : this(ver, indexDir, new StandardAnalyzer(ver), schema) { }

        /// <summary>
        /// Creates a new Lucene Search Provider
        /// </summary>
        /// <param name="ver">Version</param>
        /// <param name="indexDir">Directory</param>
        /// <remarks>
        /// Uses the <see cref="DefaultIndexSchema">DefaultIndexSchema</see> as the schema and the <see cref="StandardAnalyzer">StandardAnalyzer</see> as the analyzer
        /// </remarks>
        public LuceneSearchProvider(LucUtil.Version ver, Directory indexDir)
            : this(ver, indexDir, new StandardAnalyzer(ver), new DefaultIndexSchema()) { }
    }
}
