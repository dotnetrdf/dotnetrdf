/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Graphs;
using VDS.RDF.Nodes;
using VDS.RDF.Parsing;

namespace VDS.RDF.Collections
{
    /// <summary>
    /// A decorator for graph collections that allows for graphs to be loaded on demand if they don't exist in the underlying graph collection
    /// </summary>
    public abstract class BaseDemandGraphCollection
        : WrapperGraphCollection
    {
        /// <summary>
        /// Creates a new decorator
        /// </summary>
        protected BaseDemandGraphCollection()
            : base() { }

        /// <summary>
        /// Creates a new decorator over the given graph collection
        /// </summary>
        /// <param name="collection">Graph Collection</param>
        protected BaseDemandGraphCollection(BaseGraphCollection collection)
            : base(collection) { }

        /// <summary>
        /// Checks whether the collection contains a Graph invoking an on-demand load if not present in the underlying collection
        /// </summary>
        /// <param name="graphName">Graph name</param>
        /// <returns></returns>
        public override bool ContainsKey(INode graphName)
        {
            if (ReferenceEquals(graphName, null)) return base.ContainsKey(graphName);
            try
            {
                if (base.ContainsKey(graphName))
                {
                    return true;
                }
                else
                {
                    //Try to do on-demand loading
                    IGraph g = this.LoadOnDemand(graphName);

                    //Remember to set the Graph URI to the URI being asked for prior to adding it to the underlying collection
                    //in case the loading process sets it otherwise
                    base.Add(graphName, g);
                    return true;
                }
            }
            catch
            {
                //Any errors in checking if the Graph already exists or loading it on-demand leads to a return of false
                return false;
            }
        }

        /// <summary>
        /// Loads a Graph on demand
        /// </summary>
        /// <param name="graphName">Name of the graph to load</param>
        /// <returns>A Graph if it could be loaded and throws an error otherwise</returns>
        protected abstract IGraph LoadOnDemand(INode graphName);
    }

#if !SILVERLIGHT

    /// <summary>
    /// A decorator for graph collections where graphs not in the underlying graph collection can be loaded on-demand from the Web as needed
    /// </summary>
    public class WebDemandGraphCollection
        : BaseDemandGraphCollection
    {
        /// <summary>
        /// Creates a new Web Demand Graph Collection which loads Graphs from the Web on demand
        /// </summary>
        public WebDemandGraphCollection() { }

        /// <summary>
        /// Creates a new Web Demand Graph Collection which loads Graphs from the Web on demand
        /// </summary>
        /// <param name="collection">Collection to decorate</param>
        public WebDemandGraphCollection(BaseGraphCollection collection)
            : base(collection) { }

        /// <summary>
        /// Tries to load a Graph on demand from a URI
        /// </summary>
        /// <param name="graphName">Graph name</param>
        /// <returns></returns>
        protected override IGraph LoadOnDemand(INode graphName)
        {
            if (graphName.NodeType == NodeType.Uri)
            {
                try
                {
                    Graph g = new Graph();
                    UriLoader.Load(g, graphName.Uri);
                    return g;
                }
                catch
                {
                    throw new RdfException("The Graph with the URI " + graphName.Uri.AbsoluteUri + " does not exist in this collection");
                }
            }
            throw new RdfException("The Graph with the URI does not exist in this collection");
        }
    }

#endif

#if !NO_FILE
    /// <summary>
    /// A decorator for graph collection where graphs not in the underlying graph collection can be loaded on-demand from the Files on Disk as needed
    /// </summary>
    public class DiskDemandGraphCollection
        : BaseDemandGraphCollection
    {
        /// <summary>
        /// Creates a new Disk Demand Graph Collection which loads Graphs from the Web on demand
        /// </summary>
        public DiskDemandGraphCollection()
            : base() { }

        /// <summary>
        /// Creates a new Disk Demand Graph Collection
        /// </summary>
        /// <param name="collection">Collection to decorate</param>
        public DiskDemandGraphCollection(BaseGraphCollection collection)
            : base(collection) { }

        /// <summary>
        /// Tries to load a Graph on demand
        /// </summary>
        /// <param name="graphName">Graph name</param>
        /// <returns></returns>
        protected override IGraph LoadOnDemand(INode graphName)
        {
            if (graphName.NodeType == NodeType.Uri)
            {
                Uri graphUri = graphName.Uri;
#if SILVERLIGHT
            if (graphUri.IsFile())
#else
                if (graphUri.IsFile)
#endif
                {
                    try
                    {
                        Graph g = new Graph();
                        FileLoader.Load(g, graphUri.AbsoluteUri.Substring(8));

                        return g;
                    }
                    catch
                    {
                        throw new RdfException("The Graph with the URI " + graphUri.AbsoluteUri + " does not exist in this collection");
                    }
                }
                throw new RdfException("The Graph with the URI " + graphUri.AbsoluteUri + " does not exist in this collection");
            }
            throw new RdfException("The Graph with the URI does not exist in this collection");
        }
    }
#endif
}
