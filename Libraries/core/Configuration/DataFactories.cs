/*

Copyright Robert Vesse 2009-10
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
using System.Reflection;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Inference;
using VDS.RDF.Storage;

namespace VDS.RDF.Configuration
{
    /// <summary>
    /// Factory class for producing Graphs from Configuration Graphs
    /// </summary>
    public class GraphFactory : IObjectFactory
    {
        /// <summary>
        /// Tries to load a Graph based on information from the Configuration Graph
        /// </summary>
        /// <param name="g">Configuration Graph</param>
        /// <param name="objNode">Object Node</param>
        /// <param name="targetType">Target Type</param>
        /// <param name="obj">Output Object</param>
        /// <returns></returns>
        public bool TryLoadObject(IGraph g, INode objNode, Type targetType, out object obj)
        {
            obj = null;
            IGraph output;
            try
            {
                output = (IGraph)Activator.CreateInstance(targetType);
            }
            catch
            {
                //Any error means this loader can't load this type
                return false;
            }

            //Now we want to find out where the data for the Graph is coming from
            //Data Source loading order is Graphs, Files, Strings, Databases, Stores, URIs
            IEnumerable<INode> sources;

            //Load from Graphs
            sources = ConfigurationLoader.GetConfigurationData(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyFromGraph));
            foreach (INode source in sources)
            {
                ConfigurationLoader.CheckCircularReference(objNode, source, "dnr:fromGraph");

                Object graph = ConfigurationLoader.LoadObject(g, source);
                if (graph is IGraph)
                {
                    output.Merge((IGraph)graph);
                }
                else
                {
                    throw new DotNetRdfConfigurationException("Unable to load data from another Graph for the Graph identified by the Node '" + objNode.ToString() + "' as one of the values for the dnr:fromGraph property points to an Object that cannot be loaded as an object which implements the IGraph interface");
                }
            }

            //Load from Embedded Resources
            sources = ConfigurationLoader.GetConfigurationData(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyFromEmbedded));
            foreach (INode source in sources)
            {
                if (source.NodeType == NodeType.Literal)
                {
                    EmbeddedResourceLoader.Load(output, ((ILiteralNode)source).Value);
                }
                else
                {
                    throw new DotNetRdfConfigurationException("Unable to load data from an Embedded Resource for the Graph identified by the Node '" + objNode.ToString() + "' as one of the values for the dnr:fromEmbedded property is not a Literal Node as required");
                }
            }
            
            //Load from Files
            sources = ConfigurationLoader.GetConfigurationData(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyFromFile));
            foreach (INode source in sources)
            {
                if (source.NodeType == NodeType.Literal)
                {
                    FileLoader.Load(output, ConfigurationLoader.ResolvePath(((ILiteralNode)source).Value));
                }
                else
                {
                    throw new DotNetRdfConfigurationException("Unable to load data from a file for the Graph identified by the Node '" + objNode.ToString() + "' as one of the values for the dnr:fromFile property is not a Literal Node as required");
                }
            }

            //Load from Strings
            sources = ConfigurationLoader.GetConfigurationData(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyFromString));
            foreach (INode source in sources)
            {
                if (source.NodeType == NodeType.Literal)
                {
                    StringParser.Parse(output, ((ILiteralNode)source).Value);
                }
                else
                {
                    throw new DotNetRdfConfigurationException("Unable to load data from a string for the Graph identified by the Node '" + objNode.ToString() + "' as one of the values for the dnr:fromString property is not a Literal Node as required");
                }
            }

            IEnumerable<Object> connections;

#if !NO_DATA && !NO_STORAGE

            //Load from Databases
            IEnumerable<INode> dbs = ConfigurationLoader.GetConfigurationData(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyFromDatabase));
            dbs.All(db => !ConfigurationLoader.CheckCircularReference(objNode, db, "dnr:fromDatabase"));
            connections = dbs.Select(db => ConfigurationLoader.LoadObject(g, db));
            sources = ConfigurationLoader.GetConfigurationData(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyWithUri));
            foreach (Object db in connections)
            {
                if (db is ISqlIOManager)
                {
                    SqlReader reader = new SqlReader((ISqlIOManager)db);
                    foreach (INode source in sources)
                    {
                        if (source.NodeType == NodeType.Uri || source.NodeType == NodeType.Literal)
                        {
                            output.Merge(reader.Load(source.ToString()));
                        }
                        else
                        {
                            throw new DotNetRdfConfigurationException("Unable to load data from a database for the Graph identified by the Node '" + objNode.ToString() + "' as one of the values for the dnr:withUri property is not a URI/Literal Node as required");
                        }
                    }
                }
                else
                {
                    throw new DotNetRdfConfigurationException("Unable to load data from a database for the Graph identified by the Node '" + objNode.ToString() + "' as one of the values of the dnr:fromDatabase property points to an Object which cannot be loaded as an object which implements the ISqlIOManager interface");
                }
            }

#endif

#if !NO_STORAGE

            //Load from Stores
            IEnumerable<INode> stores = ConfigurationLoader.GetConfigurationData(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyFromStore));
            stores.All(s => !ConfigurationLoader.CheckCircularReference(objNode, s, "dnr:fromStore"));
            connections = stores.Select(s => ConfigurationLoader.LoadObject(g, s));
            sources = ConfigurationLoader.GetConfigurationData(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyWithUri));
            foreach (Object store in connections)
            {
                if (store is IGenericIOManager)
                {
                    foreach (INode source in sources)
                    {
                        if (source.NodeType == NodeType.Uri || source.NodeType == NodeType.Literal)
                        {
                            ((IGenericIOManager)store).LoadGraph(output, source.ToString());
                        } 
                        else 
                        {
                            throw new DotNetRdfConfigurationException("Unable to load data from a Generic Store for the Graph identified by the Node '" + objNode.ToString() + "' as one of the values for the dnr:withUri property is not a URI/Literal Node as required");
                        }
                    }
                }
                else if (store is ITripleStore)
                {
                    foreach (INode source in sources)
                    {
                        if (source.NodeType == NodeType.Uri)
                        {
                            output.Merge(((ITripleStore)store).Graph(((IUriNode)source).Uri));
                        }
                        else if (source.NodeType == NodeType.Literal)
                        {
                            output.Merge(((ITripleStore)store).Graph(new Uri(((ILiteralNode)source).Value)));
                        }
                        else
                        {
                            throw new DotNetRdfConfigurationException("Unable to load data from a Store for the Graph identified by the Node '" + objNode.ToString() + "' as one of the values for the dnr:withUri property is not a URI/Literal Node as required");
                        }
                    }
                }
                else
                {
                    throw new DotNetRdfConfigurationException("Unable to load data from a Store for the Graph identified by the Node '" + objNode.ToString() + "' as one of the values of the dnr:fromStore property points to an Object which cannot be loaded as an object which implements either the IGenericIOManager/ITripleStore interface");
                }
            }

#endif

            //Finally load from Remote URIs
            sources = ConfigurationLoader.GetConfigurationData(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyFromUri));
            foreach (INode source in sources)
            {
                if (source.NodeType == NodeType.Uri)
                {
#if !SILVERLIGHT
                    UriLoader.Load(output, ((IUriNode)source).Uri);
#else
                    throw new PlatformNotSupportedException("Loading Data into a Graph from a remote URI is not currently supported under Silverlight/Windows Phone 7");
#endif
                }
                else if (source.NodeType == NodeType.Literal)
                {
#if !SILVERLIGHT
                    UriLoader.Load(output, new Uri(((ILiteralNode)source).Value));
#else
                    throw new PlatformNotSupportedException("Loading Data into a Graph from a remote URI is not currently supported under Silverlight/Windows Phone 7");
#endif
                }
                else
                {
                    throw new DotNetRdfConfigurationException("Unable to load data from a URI for the Graph identified by the Node '" + objNode.ToString() + "' as one of the values for the dnr:fromUri property is not a URI/Literal Node as required");
                }
            }
            
            //Then are we assigning a Base URI to this Graph which overrides any existing Base URI?
            INode baseUri = ConfigurationLoader.GetConfigurationNode(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyAssignUri));
            if (baseUri != null)
            {
                if (baseUri.NodeType == NodeType.Uri)
                {
                    output.BaseUri = ((IUriNode)baseUri).Uri;
                }
                else if (baseUri.NodeType == NodeType.Literal)
                {
                    output.BaseUri = new Uri(((ILiteralNode)baseUri).Value);
                }
                else
                {
                    throw new DotNetRdfConfigurationException("Unable to assign a new Base URI for the Graph identified by the Node '" + objNode.ToString() + "' as the value for the dnr:assignUri property is not a URI/Literal Node as required");
                }
            }

            //Finally we'll apply any reasoners
            IEnumerable<INode> reasoners = ConfigurationLoader.GetConfigurationData(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyReasoner));
            foreach (INode reasoner in reasoners)
            {
                Object temp = ConfigurationLoader.LoadObject(g, reasoner);
                if (temp is IInferenceEngine)
                {
                    ((IInferenceEngine)temp).Apply(output);
                }
                else
                {
                    throw new DotNetRdfConfigurationException("Unable to apply a reasoner for the Graph identified by the Node '" + objNode.ToString() + "' as one of the values for the dnr:reasoner property points to an Object which cannot be loaded as an object which implements the IInferenceEngine interface");
                }
            }

            obj = output;
            return true;
        }

        /// <summary>
        /// Gets whether this Factory can load objects of the given Type
        /// </summary>
        /// <param name="t">Type</param>
        /// <returns></returns>
        public bool CanLoadObject(Type t)
        {
            Type igraph = typeof(IGraph);
            
            //We can load any object which implements IGraph and has a public unparameterized constructor
            if (t.GetInterfaces().Any(i => i.Equals(igraph)))
            {
                ConstructorInfo c = t.GetConstructor(System.Type.EmptyTypes);
                if (c != null)
                {
                    return c.IsPublic;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }
    }

    /// <summary>
    /// Factory class for producing Triple Stores from Configuration Graphs
    /// </summary>
    public class StoreFactory 
        : IObjectFactory
    {
        private const String TripleStore = "VDS.RDF.TripleStore",
                             SqlTripleStore = "VDS.RDF.SqlTripleStore",
                             ThreadedSqlTripleStore = "VDS.RDF.ThreadedSqlTripleStore",
                             OnDemandTripleStore = "VDS.RDF.OnDemandTripleStore",
#if !SILVERLIGHT
                             WebDemandTripleStore = "VDS.RDF.WebDemandTripleStore",
#endif
                             NativeTripleStore = "VDS.RDF.NativeTripleStore",
                             PersistentTripleStore = "VDS.RDF.PersistentTripleStore";


        /// <summary>
        /// Tries to load a Triple Store based on information from the Configuration Graph
        /// </summary>
        /// <param name="g">Configuration Graph</param>
        /// <param name="objNode">Object Node</param>
        /// <param name="targetType">Target Type</param>
        /// <param name="obj">Output Object</param>
        /// <returns></returns>
        public bool TryLoadObject(IGraph g, INode objNode, Type targetType, out object obj)
        {
            obj = null;

            ITripleStore store = null;
            INode subObj;
            bool isAsync;
            Object temp;

            //Get Property Nodes we need
            INode propSqlManager = ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertySqlManager),
                  propGenericManager = ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyGenericManager),
                  propAsync = ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyAsync);

            //Instantiate the Store Class
            switch (targetType.FullName)
            {
                case TripleStore:
                    store = new TripleStore();
                    break;

#if !SILVERLIGHT
                case WebDemandTripleStore:
                    store = new WebDemandTripleStore();
                    break;
#endif

#if !NO_DATA && !NO_STORAGE

                case SqlTripleStore:
                case ThreadedSqlTripleStore:
                case OnDemandTripleStore:
                    subObj = ConfigurationLoader.GetConfigurationNode(g, objNode, propSqlManager);
                    if (subObj == null) return false;

                    temp = ConfigurationLoader.LoadObject(g, subObj);
                    if (temp is ISqlIOManager)
                    {
                        if (targetType.FullName.Equals(SqlTripleStore))
                        {
                            store = new SqlTripleStore((ISqlIOManager)temp);
                        }
                        else if (targetType.FullName.Equals(OnDemandTripleStore))
                        {
                            store = new OnDemandTripleStore((ISqlIOManager)temp);
                        }
                        else if (temp is IThreadedSqlIOManager)
                        {
                            isAsync = ConfigurationLoader.GetConfigurationBoolean(g, objNode, propAsync, false);
                            store = new ThreadedSqlTripleStore((IThreadedSqlIOManager)temp, isAsync);
                        }
                        else
                        {
                            throw new DotNetRdfConfigurationException("Unable to load a SQL Triple Store identified by the Node '" + objNode.ToString() + "' as the value given for the dnr:sqlManager property points to an Object which could not be loaded as an object which implements either the ISqlIOManager/IThreadedSqlIOManager interface");
                        }
                    }
                    else
                    {
                        throw new DotNetRdfConfigurationException("Unable to load a SQL Triple Store identified by the Node '" + objNode.ToString() + "' as the value given for the dnr:sqlManager property points to an Object which could not be loaded as an object which implements either the ISqlIOManager interface");
                    }
                    break;

#endif

#if !NO_STORAGE

                case NativeTripleStore:
                    subObj = ConfigurationLoader.GetConfigurationNode(g, objNode, propGenericManager);
                    if (subObj == null) return false;

                    temp = ConfigurationLoader.LoadObject(g, subObj);
                    if (temp is IQueryableGenericIOManager)
                    {
                        store = new NativeTripleStore((IQueryableGenericIOManager)temp);
                    }
                    else
                    {
                        throw new DotNetRdfConfigurationException("Unable to load a Native Triple Store identified by the Node '" + objNode.ToString() + "' as the value given for the dnr:genericManager property points to an Object which could not be loaded as an object which implements the IQueryableGenericIOManager interface");
                    }
                    break;

                case PersistentTripleStore:
                    subObj = ConfigurationLoader.GetConfigurationNode(g, objNode, propGenericManager);
                    if (subObj == null) return false;

                    temp = ConfigurationLoader.LoadObject(g, subObj);
                    if (temp is IGenericIOManager)
                    {
                        store = new PersistentTripleStore((IGenericIOManager)temp);
                    }
                    else
                    {
                        throw new DotNetRdfConfigurationException("Unable to load a Persistent Triple Store identified by the Node '" + objNode.ToString() + "' as the value given the for dnr:genericManager property points to an Object which could not be loaded as an object which implements the IGenericIOManager interface");
                    }
                    break;

#endif

            }
            
            //Read in additional data to be added to the Store
            if (store != null)
            {
                IEnumerable<INode> sources = ConfigurationLoader.GetConfigurationData(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, "dnr:usingGraph"));

                //Read from Graphs
                foreach (INode source in sources)
                {
                    temp = ConfigurationLoader.LoadObject(g, source);
                    if (temp is IGraph)
                    {
                        store.Add((IGraph)temp);
                    }
                    else
                    {
                        throw new DotNetRdfConfigurationException("Unable to load data from a Graph for the Triple Store identified by the Node '" + objNode.ToString() + "' as one of the values for the dnr:usingGraph property points to an Object that cannot be loaded as an object which implements the IGraph interface");
                    }
                }

                //Load from Embedded Resources
                sources = ConfigurationLoader.GetConfigurationData(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyFromEmbedded));
                foreach (INode source in sources)
                {
                    if (source.NodeType == NodeType.Literal)
                    {
                        EmbeddedResourceLoader.Load(store, ((ILiteralNode)source).Value);
                    }
                    else
                    {
                        throw new DotNetRdfConfigurationException("Unable to load data from an Embedded Resource for the Graph identified by the Node '" + objNode.ToString() + "' as one of the values for the dnr:fromEmbedded property is not a Literal Node as required");
                    }
                }

                //Read from Files - we assume these files are Dataset Files
                sources = ConfigurationLoader.GetConfigurationData(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyFromFile));
                foreach (INode source in sources)
                {
                    if (source.NodeType == NodeType.Literal)
                    {
                        FileLoader.Load(store, ConfigurationLoader.ResolvePath(((ILiteralNode)source).Value));
                    }
                    else
                    {
                        throw new DotNetRdfConfigurationException("Unable to load data from a file for the Triple Store identified by the Node '" + objNode.ToString() + "' as one of the values for the dnr:fromFile property is not a Literal Node as required");
                    }
                }

                //Finally we'll apply any reasoners
                if (store is IInferencingTripleStore)
                {
                    IEnumerable<INode> reasoners = ConfigurationLoader.GetConfigurationData(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyReasoner));
                    foreach (INode reasoner in reasoners)
                    {
                        temp = ConfigurationLoader.LoadObject(g, reasoner);
                        if (temp is IInferenceEngine)
                        {
                            ((IInferencingTripleStore)store).AddInferenceEngine((IInferenceEngine)temp);
                        }
                        else
                        {
                            throw new DotNetRdfConfigurationException("Unable to apply a reasoner for the Graph identified by the Node '" + objNode.ToString() + "' as one of the values for the dnr:reasoner property points to an Object which cannot be loaded as an object which implements the IInferenceEngine interface");
                        }
                    }
                }

                //And as an absolute final step if the store is transactional we'll flush any changes we've made
                if (store is ITransactionalStore)
                {
                    ((ITransactionalStore)store).Flush();
                }
            }

            obj = store;
            return (store != null);
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
                case TripleStore:
                case SqlTripleStore:
                case ThreadedSqlTripleStore:
                case OnDemandTripleStore:
#if !SILVERLIGHT
                case WebDemandTripleStore:
#endif
                case NativeTripleStore:
                case PersistentTripleStore:
                     return true;
                default:
                    return false;
            }
        }
    }
}
