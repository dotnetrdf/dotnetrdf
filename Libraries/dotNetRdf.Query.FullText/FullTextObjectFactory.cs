/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2025 dotNetRDF Project (http://dotnetrdf.org/)
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
using DirInfo = System.IO.DirectoryInfo;
using System.Linq;
using System.Reflection;
using Lucene.Net.Analysis;
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
using Lucene.Net.Util;
using Lucene.Net.Analysis.Standard;

namespace VDS.RDF.Configuration;

/// <summary>
/// An Object Factory that can load types from the Full Text Query library (<strong>dotNetRDF.Query.FullText.dll</strong>).
/// </summary>
public class FullTextObjectFactory
    : IObjectFactory
{
    /// <summary>
    /// Constants for loadable Types.
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
    /// Tries to load an object based on information from the Configuration Graph.
    /// </summary>
    /// <param name="g">Configuration Graph.</param>
    /// <param name="objNode">Object Node.</param>
    /// <param name="targetType">Target Type.</param>
    /// <param name="obj">Output Object.</param>
    /// <returns></returns>
    public bool TryLoadObject(IGraph g, INode objNode, Type targetType, out object obj)
    {
        obj = null;

        INode index = g.CreateUriNode(g.UriFactory.Create(FullTextHelper.FullTextConfigurationNamespace + "index"));
        INode indexerProperty = g.CreateUriNode(g.UriFactory.Create(FullTextHelper.FullTextConfigurationNamespace + "indexer"));
        INode searcher = g.CreateUriNode(g.UriFactory.Create(FullTextHelper.FullTextConfigurationNamespace + "searcher"));
        INode analyzer = g.CreateUriNode(g.UriFactory.Create(FullTextHelper.FullTextConfigurationNamespace + "analyzer"));
        INode schema = g.CreateUriNode(g.UriFactory.Create(FullTextHelper.FullTextConfigurationNamespace + "schema"));
        INode version = g.CreateUriNode(g.UriFactory.Create(FullTextHelper.FullTextConfigurationNamespace + "version"));

        Object tempIndex, tempAnalyzer, tempSchema;
        var ver = DefaultVersion;
        //Always check for the version
        ver = ConfigurationLoader.GetConfigurationInt32(g, objNode, version, DefaultVersion);

        switch (targetType.FullName)
        {
            case DefaultIndexSchema:
                obj = new DefaultIndexSchema();
                break;

            case FullTextIndexedDataset:
                //Need to get the inner dataset
                INode datasetNode = ConfigurationLoader.GetConfigurationNode(g, objNode, g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyUsingDataset)));
                if (datasetNode == null) throw new DotNetRdfConfigurationException("Unable to load the Full Text Indexed Dataset specified by the Node '" + objNode.ToString() + "' as there was no value specified for the required dnr:usingDataset property");
                var tempDataset = ConfigurationLoader.LoadObject(g, datasetNode);
                if (tempDataset is ISparqlDataset sparqlDataset)
                {
                    //Then load the indexer associated with the dataset
                    INode indexerNode = ConfigurationLoader.GetConfigurationNode(g, objNode, indexerProperty);
                    if (indexerNode == null) throw new DotNetRdfConfigurationException("Unable to load the Full Text Indexed Dataset specified by the Node '" + objNode.ToString() + " as there was no value specified for the required dnr-ft:indexer property");
                    var tempIndexer = ConfigurationLoader.LoadObject(g, indexerNode);
                    if (tempIndexer is IFullTextIndexer fullTextIndexer)
                    {
                        var indexNow = ConfigurationLoader.GetConfigurationBoolean(g, objNode, g.CreateUriNode(g.UriFactory.Create(FullTextHelper.PropertyIndexNow)), false);
                        obj = new FullTextIndexedDataset(sparqlDataset, fullTextIndexer, indexNow);
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
                var tempSearcher = ConfigurationLoader.LoadObject(g, providerNode);
                if (tempSearcher is IFullTextSearchProvider fullTextSearchProvider)
                {
                    obj = new FullTextOptimiser(fullTextSearchProvider);
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
                if (tempIndex is Directory directory)
                {
                    //Next get the Analyzer (assume Standard if none specified)
                    tempAnalyzer = ConfigurationLoader.GetConfigurationNode(g, objNode, analyzer);
                    tempAnalyzer = tempAnalyzer == null ? new StandardAnalyzer(GetLuceneVersion(ver)) : ConfigurationLoader.LoadObject(g, (INode)tempAnalyzer);

                    if (tempAnalyzer is Analyzer indexAnalyzer)
                    {
                        //Finally get the Schema (assume Default if none specified)
                        tempSchema = ConfigurationLoader.GetConfigurationNode(g, objNode, schema);
                        tempSchema = tempSchema == null ? new DefaultIndexSchema() : ConfigurationLoader.LoadObject(g, (INode)tempSchema);

                        if (tempSchema is IFullTextIndexSchema fullTextIndexSchema)
                        {
                            //Now we can create the Object
                            switch (targetType.FullName)
                            {
                                case LuceneObjectsIndexer:
                                    obj = new LuceneObjectsIndexer(directory, indexAnalyzer, fullTextIndexSchema);
                                    break;
                                case LucenePredicatesIndexer:
                                    obj = new LucenePredicatesIndexer(directory, indexAnalyzer, fullTextIndexSchema);
                                    break;
                                case LuceneSubjectsIndexer:
                                    obj = new LuceneSubjectsIndexer(directory, indexAnalyzer, fullTextIndexSchema);
                                    break;
                                case LuceneSearchProvider:
                                    //Before the Search Provider has been loaded determine whether we need to carry out auto-indexing
                                    var sources = ConfigurationLoader.GetConfigurationData(g, objNode, g.CreateUriNode(g.UriFactory.Create(FullTextHelper.FullTextConfigurationNamespace + "buildIndexFor"))).ToList();
                                    if (sources.Count > 0)
                                    {
                                        //If there are sources to index ensure we have an indexer to index with
                                        INode indexerNode = ConfigurationLoader.GetConfigurationNode(g, objNode, g.CreateUriNode(g.UriFactory.Create(FullTextHelper.FullTextConfigurationNamespace + "buildIndexWith")));
                                        if (indexerNode == null) throw new DotNetRdfConfigurationException("Unable to load the Lucene Search Provider specified by the Node '" + objNode.ToString() + "' as there were values specified for the dnr-ft:buildIndexFor property but no dnr-ft:buildIndexWith property was found");
                                        var indexer = ConfigurationLoader.LoadObject(g, indexerNode) as IFullTextIndexer;
                                        if (indexer == null) throw new DotNetRdfConfigurationException("Unable to load the Lucene Search Provider specified by the Node '" + objNode.ToString() + "' as the value given for the dnr-ft:buildIndexWith property pointed to an Object which could not be loaded as a type that implements the required IFullTextIndexer interface");

                                        try 
                                        {
                                            //For Each Source load it and Index it
                                            foreach (INode sourceNode in sources)
                                            {
                                                var source = ConfigurationLoader.LoadObject(g, sourceNode);
                                                switch (source)
                                                {
                                                    case ISparqlDataset dataset:
                                                        indexer.Index(dataset);
                                                        break;
                                                    case ITripleStore store:
                                                        {
                                                            foreach (IGraph graph in store.Graphs)
                                                            {
                                                                indexer.Index(graph);
                                                            }
                                                            break;
                                                        }
                                                    case IGraph graph:
                                                        indexer.Index(graph);
                                                        break;
                                                    default:
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
                                    var autoSync = ConfigurationLoader.GetConfigurationBoolean(g, objNode, g.CreateUriNode(g.UriFactory.Create(FullTextHelper.PropertyIndexSync)), true);
                                    obj = new LuceneSearchProvider(GetLuceneVersion(ver), directory, indexAnalyzer, fullTextIndexSchema, autoSync);
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
                    if (_luceneAnalyzerType.IsAssignableFrom(targetType))
                    {
                        //Create an Analyzer
                        //Try to create passing Lucene Version wherever possible
                        if (targetType.GetConstructor(new Type[] { typeof(LuceneVersion) }) != null)
                        {
                            obj = Activator.CreateInstance(targetType, new Object[] { GetLuceneVersion(ver) });
                        }
                        else
                        {
                            obj = Activator.CreateInstance(targetType);
                        }
                    }
                    else if (_luceneDirectoryType.IsAssignableFrom(targetType))
                    {
                        //Create a Directory aka a Lucene Index
                        var dir = ConfigurationLoader.GetConfigurationString(g, objNode, g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyFromFile)));
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
                            if (ConfigurationLoader.GetConfigurationBoolean(g, objNode, g.CreateUriNode(g.UriFactory.Create(FullTextHelper.FullTextConfigurationNamespace + "ensureIndex")), false))
                            {
                                var conf = new IndexWriterConfig(GetLuceneVersion(ver), new StandardAnalyzer(GetLuceneVersion(ver)));
                                var writer = new IndexWriter((Directory)obj, conf);
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
    /// Gets whether this Factory can load objects of the given Type.
    /// </summary>
    /// <param name="t">Type.</param>
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
                if (_luceneAnalyzerType.IsAssignableFrom(t)) return true;
                if (_luceneDirectoryType.IsAssignableFrom(t)) return true;
                return false;
        }
    }

    /// <summary>
    /// Converts from an integer to a Lucene.Net Version.
    /// </summary>
    /// <param name="ver">Version.</param>
    /// <returns></returns>
    private LuceneVersion GetLuceneVersion(int ver)
    {
        switch (ver)
        {
#pragma warning disable CS0618
            case 3000:
                return LuceneVersion.LUCENE_30;
            case 3100:
                return LuceneVersion.LUCENE_31;
            case 3200:
                return LuceneVersion.LUCENE_32;
            case 3300:
                return LuceneVersion.LUCENE_33;
            case 3400:
                return LuceneVersion.LUCENE_34;
            case 3500:
                return LuceneVersion.LUCENE_35;
            case 3600:
                return LuceneVersion.LUCENE_36;
            case 4000:
                return LuceneVersion.LUCENE_40;
            case 4100:
                return LuceneVersion.LUCENE_41;
            case 4200:
                return LuceneVersion.LUCENE_42;
            case 4300:
                return LuceneVersion.LUCENE_43;
            case 4400:
                return LuceneVersion.LUCENE_44;
            case 4500:
                return LuceneVersion.LUCENE_45;
            case 4600:
                return LuceneVersion.LUCENE_46;
            case 4700:
                return LuceneVersion.LUCENE_47;
#pragma warning restore CS0618
            case 4800:
                return LuceneVersion.LUCENE_48;
            default:
                return LuceneVersion.LUCENE_48;
        }
    }
}
