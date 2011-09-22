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
using System.Security.Cryptography;
using System.Text;
using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Lucene.Net.Util;
using LucSearch = Lucene.Net.Search;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
using VDS.RDF.Query.FullText.Schema;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query.FullText.Indexing.Lucene
{
    /// <summary>
    /// Abstract Implementation of a simple Full Text Indexer using Lucene.Net which indexes the full text of literal objects and associated a specific Node with that full text
    /// </summary>
    public abstract class BaseSimpleLuceneIndexer
        : BaseSimpleFullTextIndexer, IConfigurationSerializable
    {
        private IndexingMode _mode;
        private Directory _indexDir;
        private Analyzer _analyzer;
        private IndexWriter _writer;
        private IFullTextIndexSchema _schema;
        private NTriplesFormatter _formatter = new NTriplesFormatter();
        private LucSearch.IndexSearcher _searcher;

        /// <summary>
        /// Creates a new Simple Lucene Indexer
        /// </summary>
        /// <param name="indexDir">Directory</param>
        /// <param name="analyzer">Analyzer</param>
        /// <param name="schema">Index Schema</param>
        /// <param name="mode">Indexing Mode</param>
        public BaseSimpleLuceneIndexer(Directory indexDir, Analyzer analyzer, IFullTextIndexSchema schema, IndexingMode mode)
        {
            if (this._mode == IndexingMode.Custom) throw new ArgumentException("Cannot use IndexingMode.Custom with the BaseSimpleLuceneIndexer");
            this._mode = mode;
            this._indexDir = indexDir;
            this._analyzer = analyzer;
            this._schema = schema;
            this._writer = new IndexWriter(indexDir, analyzer);
            this._searcher = new LucSearch.IndexSearcher(this._indexDir, true);
        }

        /// <summary>
        /// Gets the Indexing Mode used
        /// </summary>
        public override IndexingMode IndexingMode
        {
            get
            {
                return this._mode;
            }
        }

        /// <summary>
        /// Indexes a Node and some full text as a Lucene document
        /// </summary>
        /// <param name="n">Node</param>
        /// <param name="text">Full Text</param>
        protected override void Index(INode n, string text)
        {
            Document doc = this.CreateDocument(n, text);
            this._writer.AddDocument(doc);
        }

        /// <summary>
        /// Unindexes a Node and some full text
        /// </summary>
        /// <param name="n">Node</param>
        /// <param name="text">Full Text</param>
        protected override void Unindex(INode n, string text)
        {
            LucSearch.TermQuery query = new LucSearch.TermQuery(new Term(this._schema.HashField, this.GetHash(n, text)));
            this._writer.DeleteDocuments(query);
        }

        /// <summary>
        /// Creates a Lucene document to add to the index
        /// </summary>
        /// <param name="n">Node</param>
        /// <param name="text">Full Text</param>
        /// <returns></returns>
        /// <remarks>
        /// May be overridden by derived classes that wish to implement custom indexing behaviour
        /// </remarks>
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

        /// <summary>
        /// Gets the hash that should be included as part of a document so that it can be unindexed if desired
        /// </summary>
        /// <param name="n">Node</param>
        /// <param name="text">Full Text</param>
        /// <returns></returns>
        protected String GetHash(INode n, String text)
        {
            String hashInput = this._formatter.Format(n);
            hashInput += text;
            return hashInput.GetSha256Hash();
        }

        /// <summary>
        /// Flushes any changes to the index
        /// </summary>
        public override void Flush()
        {
            this._writer.Commit();
        }

        /// <summary>
        /// Lucene dispose logic that ensures changes to the index are discarded
        /// </summary>
        protected override void DisposeInternal()
        {
            this._writer.Commit();
            this._searcher.Close();
            if (this._indexDir.isOpen_ForNUnit) this._writer.Close();
        }

        /// <summary>
        /// Serializes the Configuration of the Indexer
        /// </summary>
        /// <param name="context">Serialization Context</param>
        public virtual void SerializeConfiguration(ConfigurationSerializationContext context)
        {
            context.EnsureObjectFactory(typeof(FullTextObjectFactory));

            INode indexerObj = context.NextSubject;
            INode rdfType = context.Graph.CreateUriNode(new Uri(RdfSpecsHelper.RdfType));
            INode dnrType = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyType);
            INode indexerClass = context.Graph.CreateUriNode(new Uri(FullTextHelper.ClassIndexer));
            INode index = context.Graph.CreateUriNode(new Uri(FullTextHelper.PropertyIndex));
            INode schema = context.Graph.CreateUriNode(new Uri(FullTextHelper.PropertySchema));
            INode analyzer = context.Graph.CreateUriNode(new Uri(FullTextHelper.PropertyAnalyzer));

            //Basic Properties
            context.Graph.Assert(indexerObj, rdfType, indexerClass);
            context.Graph.Assert(indexerObj, dnrType, context.Graph.CreateLiteralNode(this.GetType().FullName + ", dotNetRDF.Query.FullText"));

            //Serialize and link the Index
            INode indexObj = context.Graph.CreateBlankNode();
            context.NextSubject = indexObj;
            this._indexDir.SerializeConfiguration(context);
            context.Graph.Assert(indexerObj, index, indexObj);

            //Serialize and link the Schema
            INode schemaObj = context.Graph.CreateBlankNode();
            context.NextSubject = schemaObj;
            if (this._schema is IConfigurationSerializable)
            {
                ((IConfigurationSerializable)this._schema).SerializeConfiguration(context);
            }
            else
            {
                throw new DotNetRdfConfigurationException("Unable to serialize configuration for this Lucene Indexer as the IFullTextSchema used does not implement the IConfigurationSerializable interface");
            }
            context.Graph.Assert(indexerObj, schema, schemaObj);

            //Serialize and link the Analyzer
            INode analyzerObj = context.Graph.CreateBlankNode();
            context.NextSubject = analyzerObj;
            this._analyzer.SerializeConfiguration(context);
            context.Graph.Assert(indexerObj, index, analyzerObj);
        }
    }
}
