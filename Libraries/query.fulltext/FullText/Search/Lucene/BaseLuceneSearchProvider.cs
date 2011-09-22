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

using System;
using System.Collections.Generic;
using System.Linq;
using Lucene.Net.Analysis;
using Lucene.Net.QueryParsers;
using LucSearch = Lucene.Net.Search;
using Lucene.Net.Store;
using LucUtil = Lucene.Net.Util;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
using VDS.RDF.Query.FullText.Schema;

namespace VDS.RDF.Query.FullText.Search.Lucene
{
    /// <summary>
    /// Abstract Base Implementation of a Full Text Search Provider using Lucene.Net
    /// </summary>
    /// <remarks>
    /// Derived Implementations may only need to call the base constructor as the <see cref="LuceneSearchProvider">LuceneSearchProvider</see> does but if you've implemented custom indexing you may need to extend more of this class
    /// </remarks>
    public abstract class BaseLuceneSearchProvider
        : IFullTextSearchProvider, IConfigurationSerializable
    {
        private Directory _indexDir;
        private LucSearch.IndexSearcher _searcher;
        private QueryParser _parser;
        private LucUtil.Version _version;
        private Analyzer _analyzer;
        private IFullTextIndexSchema _schema;

        /// <summary>
        /// Creates a new Base Lucene Search Provider
        /// </summary>
        /// <param name="ver">Lucene Version</param>
        /// <param name="indexDir">Directory</param>
        /// <param name="analyzer">Analyzer</param>
        /// <param name="schema">Index Schema</param>
        public BaseLuceneSearchProvider(LucUtil.Version ver, Directory indexDir, Analyzer analyzer, IFullTextIndexSchema schema)
        {
            this._version = ver;
            this._indexDir = indexDir;
            this._analyzer = analyzer;
            this._schema = schema;

            //Create necessary objects
            this._searcher = new LucSearch.IndexSearcher(this._indexDir, true);
            this._parser = new QueryParser(this._version, this._schema.IndexField, this._analyzer);
        }

        /// <summary>
        /// Destructor which ensures that the Search Provider is properly disposed of
        /// </summary>
        ~BaseLuceneSearchProvider()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Gets results that match the given query with the score threshold and limit applied
        /// </summary>
        /// <param name="text">Search Query</param>
        /// <param name="scoreThreshold">Score Threshold</param>
        /// <param name="limit">Result Limit</param>
        /// <returns></returns>
        public virtual IEnumerable<IFullTextSearchResult> Match(string text, double scoreThreshold, int limit)
        {
            LucSearch.Query q = this._parser.Parse(text);
            LucSearch.TopDocs docs = this._searcher.Search(q, limit);
            return (from doc in docs.scoreDocs
                    where doc.score > scoreThreshold
                    select this._searcher.Doc(doc.doc).ToResult(doc.score, this._schema));
        }

        /// <summary>
        /// Gets results that match the given query with the score threshold applied
        /// </summary>
        /// <param name="text">Search Query</param>
        /// <param name="scoreThreshold">Score Threshold</param>
        /// <returns></returns>
        public virtual IEnumerable<IFullTextSearchResult> Match(string text, double scoreThreshold)
        {
            LucSearch.Query q = this._parser.Parse(text);
            DocCollector collector = new DocCollector(scoreThreshold);
            this._searcher.Search(q, collector);
            return (from doc in collector.Documents
                    select this._searcher.Doc(doc.Key).ToResult(doc.Value, this._schema));
        }

        /// <summary>
        /// Gets results that match the given query with a limit applied
        /// </summary>
        /// <param name="text">Search Query</param>
        /// <param name="limit">Result Limit</param>
        /// <returns></returns>
        public virtual IEnumerable<IFullTextSearchResult> Match(string text, int limit)
        {
            LucSearch.Query q = this._parser.Parse(text);
            LucSearch.TopDocs docs = this._searcher.Search(q, limit);
            return (from doc in docs.scoreDocs
                    select this._searcher.Doc(doc.doc).ToResult(doc.score, this._schema));
        }

        /// <summary>
        /// Gets results that match the given query
        /// </summary>
        /// <param name="text">Search Query</param>
        /// <returns></returns>
        public virtual IEnumerable<IFullTextSearchResult> Match(string text)
        {
            LucSearch.Query q = this._parser.Parse(text);
            DocCollector collector = new DocCollector();
            this._searcher.Search(q, collector);
            return (from doc in collector.Documents
                    select this._searcher.Doc(doc.Key).ToResult(doc.Value, this._schema));
        }

        /// <summary>
        /// Disposes of the Search Provider
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
        }

        /// <summary>
        /// Disposes of the Search Provider
        /// </summary>
        /// <param name="disposing">Whether this was called by the Dispose method</param>
        private void Dispose(bool disposing)
        {
            if (disposing) GC.SuppressFinalize(this);

            this.DisposeInternal();

            if (this._searcher != null) this._searcher.Close();
        }

        /// <summary>
        /// Virtual method that can be overridden to add implementation specific dispose logic
        /// </summary>
        protected virtual void DisposeInternal() { }

        /// <summary>
        /// Serializes Configuration of this Provider
        /// </summary>
        /// <param name="context">Serialization Context</param>
        public virtual void SerializeConfiguration(ConfigurationSerializationContext context)
        {
            context.EnsureObjectFactory(typeof(FullTextObjectFactory));

            INode searcherObj = context.NextSubject;
            INode rdfType = context.Graph.CreateUriNode(new Uri(RdfSpecsHelper.RdfType));
            INode dnrType = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyType);
            INode searcherClass = context.Graph.CreateUriNode(new Uri(FullTextHelper.ClassSearcher));
            INode index = context.Graph.CreateUriNode(new Uri(FullTextHelper.PropertyIndex));
            INode schema = context.Graph.CreateUriNode(new Uri(FullTextHelper.PropertySchema));
            INode analyzer = context.Graph.CreateUriNode(new Uri(FullTextHelper.PropertyAnalyzer));

            //Basic Properties
            context.Graph.Assert(searcherObj, rdfType, searcherClass);
            context.Graph.Assert(searcherObj, dnrType, context.Graph.CreateLiteralNode(this.GetType().FullName + ", dotNetRDF.Query.FullText"));

            //Serialize and link the Index
            INode indexObj = context.Graph.CreateBlankNode();
            context.NextSubject = indexObj;
            this._indexDir.SerializeConfiguration(context);
            context.Graph.Assert(searcherObj, index, indexObj);

            //Serialize and link the Schema
            INode schemaObj = context.Graph.CreateBlankNode();
            context.NextSubject = schemaObj;
            if (this._schema is IConfigurationSerializable)
            {
                ((IConfigurationSerializable)this._schema).SerializeConfiguration(context);
            }
            else
            {
                throw new DotNetRdfConfigurationException("Unable to serialize configuration for this Lucene Search Provider as the IFullTextSchema used does not implement the IConfigurationSerializable interface");
            }
            context.Graph.Assert(searcherObj, schema, schemaObj);

            //Serialize and link the Analyzer
            INode analyzerObj = context.Graph.CreateBlankNode();
            context.NextSubject = analyzerObj;
            this._analyzer.SerializeConfiguration(context);
            context.Graph.Assert(searcherObj, index, analyzerObj);
        }
    }
}
