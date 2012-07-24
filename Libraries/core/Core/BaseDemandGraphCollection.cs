using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing;

namespace VDS.RDF
{
    /// <summary>
    /// A decorator for graph collections that allows for graphs to be loaded on demand if they don't exist in the underlying graph collection
    /// </summary>
    public abstract class BaseDemandGraphCollection
        : WrapperGraphCollection
    {
        public BaseDemandGraphCollection()
            : base() { }

        public BaseDemandGraphCollection(BaseGraphCollection collection)
            : base(collection) { }

        public override bool Contains(Uri graphUri)
        {
            try
            {
                if (base.Contains(graphUri))
                {
                    return true;
                }
                else
                {
                    //Try to do on-demand loading
                    IGraph g = this.LoadOnDemand(graphUri);

                    //Remember to set the Graph URI to the URI being asked for prior to adding it to the underlying collection
                    //in case the loading process sets it otherwise
                    g.BaseUri = graphUri;
                    base.Add(g, false);
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
        /// <param name="graphUri">URI of the Graph to load</param>
        /// <returns>A Graph if it could be loaded and throws an error otherwise</returns>
        protected abstract IGraph LoadOnDemand(Uri graphUri);
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

        protected override IGraph LoadOnDemand(Uri graphUri)
        {
            if (graphUri != null)
            {
                try
                {
                    Graph g = new Graph();
                    UriLoader.Load(g, graphUri);
                    return g;
                }
                catch
                {
                    throw new RdfException("The Graph with the URI " + graphUri.AbsoluteUri + " does not exist in this collection");
                }
            }
            else
            {
                throw new RdfException("The Graph with the URI does not exist in this collection");
            }
        }
    }

#endif

    /// <summary>
    /// A decorator for graph collection where graphs not in the underlying graph collection can be loaded on-demand from the Files on Disk as needed
    /// </summary>
    public class DiskDemandGraphCollection
        : BaseDemandGraphCollection
    {
        /// <summary>
        /// Creates a new Web Demand Graph Collection which loads Graphs from the Web on demand
        /// </summary>
        public DiskDemandGraphCollection()
            : base() { }

        /// <summary>
        /// Creates 
        /// </summary>
        /// <param name="collection"></param>
        public DiskDemandGraphCollection(BaseGraphCollection collection)
            : base(collection) { }

        protected override IGraph LoadOnDemand(Uri graphUri)
        {
            if (graphUri == null) throw new RdfException("The Graph with the given URI does not exist in this collection");
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
            else
            {
                throw new RdfException("The Graph with the URI " + graphUri.AbsoluteUri + " does not exist in this collection");
            }
        }
    }
}
