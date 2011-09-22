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
                             FullTextOptimiser = "VDS.RDF.Query.Optimisation.FullTextOptimiser";

        private readonly Type _luceneAnalyzerType = typeof(Analyzer);
        private readonly Type _luceneDirectoryType = typeof(Directory);


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

            INode index = g.CreateUriNode(new Uri(FullTextHelper.FullTextConfigurationNamespace + "index"));
            //INode indexer = g.CreateUriNode(new Uri(FullTextHelper.FullTextConfigurationNamespace + "indexer"));
            INode searcher = g.CreateUriNode(new Uri(FullTextHelper.FullTextConfigurationNamespace + "searcher"));
            INode analyzer = g.CreateUriNode(new Uri(FullTextHelper.FullTextConfigurationNamespace + "analyzer"));
            INode schema = g.CreateUriNode(new Uri(FullTextHelper.FullTextConfigurationNamespace + "schema"));
            INode version = g.CreateUriNode(new Uri(FullTextHelper.FullTextConfigurationNamespace + "version"));

            Object tempIndex, tempAnalyzer, tempSchema;
            int ver = 2900;
            //Always check for the version
            ver = ConfigurationLoader.GetConfigurationInt32(g, objNode, version, 2900);

            switch (targetType.FullName)
            {
                case DefaultIndexSchema:
                    obj = new DefaultIndexSchema();
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
                                        List<INode> sources = ConfigurationLoader.GetConfigurationData(g, objNode, g.CreateUriNode(new Uri(FullTextHelper.FullTextConfigurationNamespace + "buildIndexFor"))).ToList();
                                        if (sources.Count > 0)
                                        {
                                            //If there are sources to index ensure we have an indexer to index with
                                            INode indexerNode = ConfigurationLoader.GetConfigurationNode(g, objNode, g.CreateUriNode(new Uri(FullTextHelper.FullTextConfigurationNamespace + "buildIndexWith")));
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
                                        obj = new LuceneSearchProvider(this.GetLuceneVersion(ver), (Directory)tempIndex, (Analyzer)tempAnalyzer, (IFullTextIndexSchema)tempSchema);
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
                            String dir = ConfigurationLoader.GetConfigurationString(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyFromFile));
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
                                if (ConfigurationLoader.GetConfigurationBoolean(g, objNode, g.CreateUriNode(new Uri(FullTextHelper.FullTextConfigurationNamespace + "ensureIndex")), false))
                                {
                                    IndexWriter writer = new IndexWriter((Directory)obj, new StandardAnalyzer(this.GetLuceneVersion(ver)));
                                    writer.Close();
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
