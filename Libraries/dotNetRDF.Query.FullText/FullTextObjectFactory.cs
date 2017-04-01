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

using System;
using System.Collections.Generic;
using DirInfo = System.IO.DirectoryInfo;
using System.Linq;
using System.Reflection;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Store;
using VDS.RDF.Query;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Query.FullText.Indexing;
using VDS.RDF.Query.FullText.Indexing.Lucene;
using VDS.RDF.Query.FullText.Schema;
using VDS.RDF.Query.FullText.Search;
using VDS.RDF.Query.FullText.Search.Lucene;
using VDS.RDF.Query.Optimisation;
using LucVersion = Lucene.Net.Util.Version;

namespace VDS.RDF.Configuration
{   
    /// <summary>
    /// An Object Factory that can load types from the Full Text Query library (<strong>dotNetRDF.Query.FullText.dll</strong>)
    /// </summary>
    public class FullTextObjectFactory
        : IObjectFactory
    {
        /// <summary>
        /// Constants for loadable Types
        /// </summary>
        private const String LuceneSubjectsIndexer = "VDS.RDF.Query.FullText.Indexing.Lucene.LuceneSubjectsIndexer",
                             LuceneObjectsIndexer = "VDS.RDF.Query.FullText.Indexing.Lucene.LuceneObjectsIndexer",
                             LucenePredicatesIndexer = "VDS.RDF.Query.FullText.Indexing.Lucene.LucenePredicatesIndexer",
                             DefaultIndexSchema = "VDS.RDF.Query.FullText.Schema.DefaultIndexSchema",
                             LuceneSearchProvider = "VDS.RDF.Query.FullText.Search.Lucene.LuceneSearchProvider",
                             FullTextOptimiser = "VDS.RDF.Query.Optimisation.FullTextOptimiser",
                             FullTextIndexedDataset = "VDS.RDF.Query.Datasets.FullTextIndexedDataset";

        private readonly Type _luceneAnalyzerType = typeof(Analyzer);
        private readonly Type _luceneDirectoryType = typeof(Directory);

        private const int DefaultVersion = 3000;


        /// <summary>
        /// Tries to load an object based on information from the Configuration Graph
        /// </summary>
        /// <param name="g">Configuration Graph</param>
        /// <param name="objNode">Object Node</param>
        /// <param name="targetType">Target Type</param>
        /// <param name="obj">Output Object</param>
        /// <returns></returns>
        public bool TryLoadObject(IGraph g, INode objNode, Type targetType, out object obj)
        {
            obj = null;

            INode index = g.CreateUriNode(UriFactory.Create(FullTextHelper.FullTextConfigurationNamespace + "index"));
            INode indexerProperty = g.CreateUriNode(UriFactory.Create(FullTextHelper.FullTextConfigurationNamespace + "indexer"));
            INode searcher = g.CreateUriNode(UriFactory.Create(FullTextHelper.FullTextConfigurationNamespace + "searcher"));
            INode analyzer = g.CreateUriNode(UriFactory.Create(FullTextHelper.FullTextConfigurationNamespace + "analyzer"));
            INode schema = g.CreateUriNode(UriFactory.Create(FullTextHelper.FullTextConfigurationNamespace + "schema"));
            INode version = g.CreateUriNode(UriFactory.Create(FullTextHelper.FullTextConfigurationNamespace + "version"));

            Object tempIndex, tempAnalyzer, tempSchema;
            int ver = DefaultVersion;
            //Always check for the version
            ver = ConfigurationLoader.GetConfigurationInt32(g, objNode, version, DefaultVersion);

            switch (targetType.FullName)
            {
                case DefaultIndexSchema:
                    obj = new DefaultIndexSchema();
                    break;

                case FullTextIndexedDataset:
                    //Need to get the inner dataset
                    INode datasetNode = ConfigurationLoader.GetConfigurationNode(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyUsingDataset)));
                    if (datasetNode == null) throw new DotNetRdfConfigurationException("Unable to load the Full Text Indexed Dataset specified by the Node '" + objNode.ToString() + "' as there was no value specified for the required dnr:usingDataset property");
                    Object tempDataset = ConfigurationLoader.LoadObject(g, datasetNode);
                    if (tempDataset is ISparqlDataset)
                    {
                        //Then load the indexer associated with the dataset
                        INode indexerNode = ConfigurationLoader.GetConfigurationNode(g, objNode, indexerProperty);
                        if (indexerNode == null) throw new DotNetRdfConfigurationException("Unable to load the Full Text Indexed Dataset specified by the Node '" + objNode.ToString() + " as there was no value specified for the required dnr-ft:indexer property");
                        Object tempIndexer = ConfigurationLoader.LoadObject(g, indexerNode);
                        if (tempIndexer is IFullTextIndexer)
                        {
                            bool indexNow = ConfigurationLoader.GetConfigurationBoolean(g, objNode, g.CreateUriNode(UriFactory.Create(FullTextHelper.PropertyIndexNow)), false);
                            obj = new FullTextIndexedDataset((ISparqlDataset)tempDataset, (IFullTextIndexer)tempIndexer, indexNow);
                        }
                        else
                        {
                            throw new DotNetRdfConfigurationException("Unable to load the Full Text Indexed Dataset specified by the Node '" + objNode.ToString() + "' as the value specified for the dnr-ft:indexer property pointed to an object which could not be loaded as a type that implements the required IFullTextIndexer interface");
                        }
                    }
                    else
                    {
                        throw new DotNetRdfConfigurationException("Unable to load the Full Text Indexed Dataset specified by the Node '" + objNode.ToString() + "' as the value specified for the dnr:usingDataset property pointed to an object which could not be loaded as a type that implements the required ISparqlDataset interface");
                    }
                    break;

                case FullTextOptimiser:
                    //Need to get the Search Provider
                    INode providerNode = ConfigurationLoader.GetConfigurationNode(g, objNode, searcher);
                    if (providerNode == null) throw new DotNetRdfConfigurationException("Unable to load the Full Text Optimiser specified by the Node '" + objNode.ToString() + "' as there was no value specified for the required dnr-ft:searcher property");
                    Object tempSearcher = ConfigurationLoader.LoadObject(g, providerNode);
                    if (tempSearcher is IFullTextSearchProvider)
                    {
                        obj = new FullTextOptimiser((IFullTextSearchProvider)tempSearcher);
                    }
                    else
                    {
                        throw new DotNetRdfConfigurationException("Unable to load the Full Text Optimiser specified by the Node '" + objNode.ToString() + "' as the value specified for the dnr-ft:searcher property pointed to an object which could not be loaded as a type that implements the required IFullTextSearchProvider interface");
                    }
                    break;

                case LuceneObjectsIndexer:
                case LucenePredicatesIndexer:
                case LuceneSubjectsIndexer:
                case LuceneSearchProvider:
                    //For any Lucene Indexer/Search Provider need to know the Index, Analyzer and Schema to be used

                    //Then get the Index
                    tempIndex = ConfigurationLoader.GetConfigurationNode(g, objNode, index);
                    if (tempIndex == null) throw new DotNetRdfConfigurationException("Unable to load the Lucene Indexer specified by the Node '" + objNode.ToString() + "' as there was no value specified for the required dnr-ft:index property");
                    tempIndex = ConfigurationLoader.LoadObject(g, (INode)tempIndex);
                    if (tempIndex is Directory)
                    {
                        //Next get the Analyzer (assume Standard if none specified)
                        tempAnalyzer = ConfigurationLoader.GetConfigurationNode(g, objNode, analyzer);
                        if (tempAnalyzer == null)
                        {
                            tempAnalyzer = new StandardAnalyzer(this.GetLuceneVersion(ver));
                        } 
                        else 
                        {
                            tempAnalyzer = ConfigurationLoader.LoadObject(g, (INode)tempAnalyzer);
                        }

                        if (tempAnalyzer is Analyzer)
                        {
                            //Finally get the Schema (assume Default if none specified)
                            tempSchema = ConfigurationLoader.GetConfigurationNode(g, objNode, schema);
                            if (tempSchema == null)
                            {
                                tempSchema = new DefaultIndexSchema();
                            }
                            else
                            {
                                tempSchema = ConfigurationLoader.LoadObject(g, (INode)tempSchema);
                            }

                            if (tempSchema is IFullTextIndexSchema)
                            {
                                //Now we can create the Object
                                switch (targetType.FullName)
                                {
                                    case LuceneObjectsIndexer:
                                        obj = new LuceneObjectsIndexer((Directory)tempIndex, (Analyzer)tempAnalyzer, (IFullTextIndexSchema)tempSchema);
                                        break;
                                    case LucenePredicatesIndexer:
                                        obj = new LucenePredicatesIndexer((Directory)tempIndex, (Analyzer)tempAnalyzer, (IFullTextIndexSchema)tempSchema);
                                        break;
                                    case LuceneSubjectsIndexer:
                                        obj = new LuceneSubjectsIndexer((Directory)tempIndex, (Analyzer)tempAnalyzer, (IFullTextIndexSchema)tempSchema);
                                        break;
                                    case LuceneSearchProvider:
                                        //Before the Search Provider has been loaded determine whether we need to carry out auto-indexing
                                        List<INode> sources = ConfigurationLoader.GetConfigurationData(g, objNode, g.CreateUriNode(UriFactory.Create(FullTextHelper.FullTextConfigurationNamespace + "buildIndexFor"))).ToList();
                                        if (sources.Count > 0)
                                        {
                                            //If there are sources to index ensure we have an indexer to index with
                                            INode indexerNode = ConfigurationLoader.GetConfigurationNode(g, objNode, g.CreateUriNode(UriFactory.Create(FullTextHelper.FullTextConfigurationNamespace + "buildIndexWith")));
                                            if (indexerNode == null) throw new DotNetRdfConfigurationException("Unable to load the Lucene Search Provider specified by the Node '" + objNode.ToString() + "' as there were values specified for the dnr-ft:buildIndexFor property but no dnr-ft:buildIndexWith property was found");
                                            IFullTextIndexer indexer = ConfigurationLoader.LoadObject(g, indexerNode) as IFullTextIndexer;
                                            if (indexer == null) throw new DotNetRdfConfigurationException("Unable to load the Lucene Search Provider specified by the Node '" + objNode.ToString() + "' as the value given for the dnr-ft:buildIndexWith property pointed to an Object which could not be loaded as a type that implements the required IFullTextIndexer interface");

                                            try 
                                            {
                                                //For Each Source load it and Index it
                                                foreach (INode sourceNode in sources)
                                                {
                                                    Object source = ConfigurationLoader.LoadObject(g, sourceNode);
                                                    if (source is ISparqlDataset)
                                                    {
                                                        indexer.Index((ISparqlDataset)source);
                                                    }
                                                    else if (source is ITripleStore)
                                                    {
                                                        foreach (IGraph graph in ((ITripleStore)source).Graphs)
                                                        {
                                                            indexer.Index(graph);
                                                        }
                                                    }
                                                    else if (source is IGraph)
                                                    {
                                                        indexer.Index((IGraph)source);
                                                    }
                                                    else
                                                    {
                                                        throw new DotNetRdfConfigurationException("Unable to load the Lucene Search Provider specified by the Node '" + objNode.ToString() + "' as a value given for the dnr-ft:buildIndexFor property ('" + sourceNode.ToString() + "') pointed to an Object which could not be loaded as a type that implements one of the required interfaces: IGraph, ITripleStore or ISparqlDataset");
                                                    }
                                                }
                                            } 
                                            finally 
                                            {
                                                indexer.Dispose();
                                            }
                                        }

                                        //Then we actually load the Search Provider
                                        bool autoSync = ConfigurationLoader.GetConfigurationBoolean(g, objNode, g.CreateUriNode(UriFactory.Create(FullTextHelper.PropertyIndexSync)), true);
                                        obj = new LuceneSearchProvider(this.GetLuceneVersion(ver), (Directory)tempIndex, (Analyzer)tempAnalyzer, (IFullTextIndexSchema)tempSchema, autoSync);
                                        break;
                                }
                            }
                            else
                            {
                                throw new DotNetRdfConfigurationException("Unable to load the Lucene Indexer specified by the Node '" + objNode.ToString() + "' as the value given for the dnr-ft:schema property pointed to an Object which could not be loaded as a type that implements the required IFullTextIndexSchema interface");
                            }
                        }
                        else
                        {
                            throw new DotNetRdfConfigurationException("Unable to load the Lucene Indexer specified by the Node '" + objNode.ToString() + "' as the value given for the dnr-ft:analyzer property pointed to an Object which could not be loaded as a type that derives from the required Lucene.Net.Analysis.Analyzer type");
                        }
                    }
                    else
                    {
                        throw new DotNetRdfConfigurationException("Unable to load the Lucene Indexer specified by the Node '" + objNode.ToString() + "' as the value given for the dnr-ft:index property pointed to an Object which could not be loaded as a type that derives from the required Lucene.Net.Store.Directory type");
                    }
                    break;

                default:
                    try
                    {
                        if (this._luceneAnalyzerType.IsAssignableFrom(targetType))
                        {
                            //Create an Analyzer
                            //Try to create passing Lucene Version wherever possible
                            if (targetType.GetConstructor(new Type[] { typeof(LucVersion) }) != null)
                            {
                                obj = Activator.CreateInstance(targetType, new Object[] { this.GetLuceneVersion(ver) });
                            }
                            else
                            {
                                obj = Activator.CreateInstance(targetType);
                            }
                        }
                        else if (this._luceneDirectoryType.IsAssignableFrom(targetType))
                        {
                            //Create a Directory aka a Lucene Index
                            String dir = ConfigurationLoader.GetConfigurationString(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyFromFile)));
                            if (dir != null)
                            {
                                try
                                {
                                    obj = Activator.CreateInstance(targetType, new Object[] { dir });
                                }
                                catch
                                {
                                    MethodInfo method = targetType.GetMethod("Open", new Type[] { typeof(DirInfo) });
                                    if (method != null)
                                    {
                                        obj = method.Invoke(null, new Object[] { new DirInfo(ConfigurationLoader.ResolvePath(dir)) });
                                    }
                                }
                            }
                            else
                            {
                                obj = Activator.CreateInstance(targetType);
                            }
                            //Ensure the Index if necessary
                            if (obj != null)
                            {
                                if (ConfigurationLoader.GetConfigurationBoolean(g, objNode, g.CreateUriNode(UriFactory.Create(FullTextHelper.FullTextConfigurationNamespace + "ensureIndex")), false))
                                {
                                    IndexWriter writer = new IndexWriter((Directory)obj, new StandardAnalyzer(this.GetLuceneVersion(ver)), IndexWriter.MaxFieldLength.UNLIMITED);
                                    writer.Dispose();
                                }
                            }
                        }
                    }
                    catch
                    {
                        //Since we know we don't allow loading of all analyzers and directories we allow for users to inject other object factories
                        //which may know how to load those specific instances
                        obj = null;
                    }
                    break;
            }

            return (obj != null);
        }

        /// <summary>
        /// Gets whether this Factory can load objects of the given Type
        /// </summary>
        /// <param name="t">Type</param>
        /// <returns></returns>
        public bool CanLoadObject(Type t)
        {
            switch (t.FullName)
            {
                case DefaultIndexSchema:
                case FullTextIndexedDataset:
                case FullTextOptimiser:
                case LuceneObjectsIndexer:
                case LucenePredicatesIndexer:
                case LuceneSearchProvider:
                case LuceneSubjectsIndexer:
                    return true;

                default:
                    if (this._luceneAnalyzerType.IsAssignableFrom(t)) return true;
                    if (this._luceneDirectoryType.IsAssignableFrom(t)) return true;
                    return false;
            }
        }

        /// <summary>
        /// Converts from an integer to a Lucene.Net Version
        /// </summary>
        /// <param name="ver">Version</param>
        /// <returns></returns>
        private LucVersion GetLuceneVersion(int ver)
        {
            switch (ver)
            {
                case 2000:
                    return LucVersion.LUCENE_20;
                case 2100:
                    return LucVersion.LUCENE_21;
                case 2200:
                    return LucVersion.LUCENE_22;
                case 2300:
                    return LucVersion.LUCENE_23;
                case 2400:
                    return LucVersion.LUCENE_24;
                case 2900:
                    return LucVersion.LUCENE_29;
                default:
                    return LucVersion.LUCENE_29;
            }
        }
    }
}
