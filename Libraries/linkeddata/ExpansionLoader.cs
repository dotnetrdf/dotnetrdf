using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using VDS.RDF.LinkedData.Profiles;
using VDS.RDF.Parsing;
using VDS.RDF.Query;

namespace VDS.RDF.LinkedData
{
    /// <summary>
    /// Loader which loads Linked Data starting from a given URI and following/discovering Links according to the given Expansion Profile
    /// </summary>
    public class ExpansionLoader
    {
        private ExpansionCache _cache;
        private int _threadsToUse = 4;
        private const int ThreadPollingInterval = 250;
        private const int HttpRequestInterval = 1000;

        /// <summary>
        /// Creates a new Expansion Loader which uses the default Cache directory
        /// </summary>
        public ExpansionLoader()
        {
            this._cache = new ExpansionCache("expansion_cache\\");
            UriLoader.UserAgent = "Expander/dotNetRDF " + Assembly.GetAssembly(typeof(IGraph)).GetName().Version + "/.Net Framework " + Environment.Version + " (rav08r@ecs.soton.ac.uk)";
        }

        /// <summary>
        /// Creates a new Expansion Loader which uses the specified Cache directory
        /// </summary>
        /// <param name="cacheDir">Cache Directory</param>
        public ExpansionLoader(String cacheDir)
        {
            this._cache = new ExpansionCache(cacheDir);
            UriLoader.UserAgent = "Expander/dotNetRDF " + Assembly.GetAssembly(typeof(IGraph)).GetName().Version + "/.Net Framework " + Environment.Version + " (rav08r@ecs.soton.ac.uk)";
        }

        /// <summary>
        /// Creates a new Expansion Loader which uses the specified Cache directory and Cache freshness
        /// </summary>
        /// <param name="cacheDir">Cache Directory</param>
        /// <param name="freshness">Cache Freshness</param>
        public ExpansionLoader(String cacheDir, int freshness)
        {
            this._cache = new ExpansionCache(cacheDir, freshness);
            UriLoader.UserAgent = "Expander/dotNetRDF " + Assembly.GetAssembly(typeof(IGraph)).GetName().Version + "/.Net Framework " + Environment.Version + " (rav08r@ecs.soton.ac.uk)";
        }

        /// <summary>
        /// Creates a new Expansion Loader which uses the specified settings
        /// </summary>
        /// <param name="cacheDir">Cache Directory</param>
        /// <param name="freshness">Cache Freshness</param>
        /// <param name="threads">Number of Threads to use</param>
        public ExpansionLoader(String cacheDir, int freshness, int threads)
        {
            this._cache = new ExpansionCache(cacheDir, freshness);
            this._threadsToUse = threads;
            UriLoader.UserAgent = "Expander/dotNetRDF " + Assembly.GetAssembly(typeof(IGraph)).GetName().Version + "/.Net Framework " + Environment.Version + " (rav08r@ecs.soton.ac.uk)";
        }

        /// <summary>
        /// Gets/Sets whether Caching is Disabled
        /// </summary>
        public bool DisableCaching
        {
            get
            {
                return this._cache.DisableCaching;
            }
            set
            {
                this._cache.DisableCaching = value;
            }
        }

        /// <summary>
        /// Loads Linked Data starting from the given URI and using the Default Expansion Profile
        /// </summary>
        /// <param name="u">URI to start from</param>
        /// <returns></returns>
        public IInMemoryQueryableStore Load(Uri u) 
        {
            if (this._cache.IsExpansionCached(u))
            {
                return this._cache.GetExpansion(u);
            }
            else
            {
                IInMemoryQueryableStore temp = LoadInternal(u, new ExpansionProfile());
                this._cache.Add(u, temp);
                return temp;
            }
        }

        /// <summary>
        /// Loads Linked Data starting from the given URI and using the Expansion Profile retrieved from the given URI
        /// </summary>
        /// <param name="u">URI to start from</param>
        /// <param name="profile">Expansion Profile URI</param>
        /// <returns></returns>
        public IInMemoryQueryableStore Load(Uri u, Uri profile)
        {
            if (this._cache.IsExpansionCached(u, profile))
            {
                return this._cache.GetExpansion(u, profile);
            }
            else
            {
                IInMemoryQueryableStore temp = LoadInternal(u, new ExpansionProfile(profile));
                this._cache.Add(u, profile, temp);
                return temp;
            }
        }

        /// <summary>
        /// Loads Linked Data starting from the given URI and using the merge of the Expansion Profiles retrieved from the given URIs
        /// </summary>
        /// <param name="u">URI to start from</param>
        /// <param name="profiles">Expansion Profile URIs</param>
        /// <returns></returns>
        public IInMemoryQueryableStore Load(Uri u, IEnumerable<Uri> profiles)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Loads Linked Data starting from the given URI and using the given Expansion Profile
        /// </summary>
        /// <param name="u">URI to start from</param>
        /// <param name="profile">Expansion Profile</param>
        /// <returns></returns>
        public IInMemoryQueryableStore Load(Uri u, ExpansionProfile profile)
        {
            return this.LoadInternal(u, profile);
        }

        /// <summary>
        /// Loads Linked Data starting from the given URI and using the merge of the given Expansion Profiles
        /// </summary>
        /// <param name="u">URI to start from</param>
        /// <param name="profiles">Expansion Profiles</param>
        /// <returns></returns>
        public IInMemoryQueryableStore Load(Uri u, IEnumerable<ExpansionProfile> profiles)
        {
            throw new NotImplementedException();
        }

        private IInMemoryQueryableStore LoadInternal(Uri u, ExpansionProfile profile)
        {
            ExpansionContext context = new ExpansionContext(profile);
            context.Uris.Enqueue(new UriToExpand(u, 0));

            Expand(context);

            return context.Store;
        }

        private void Expand(ExpansionContext context)
        {
            while (context.Uris.Count > 0)
            {
                //Get the next URI to expand upon and check it's not above the max expansion depth
                //Or that it's already a Graph in the Store
                UriToExpand u = context.GetNextUri();//context.Uris.Dequeue();
                if (u == null) return;

                Debug.WriteLine("Expanding URI <" + u.Uri.ToString() + "> at Depth " + u.Depth);
                Debug.WriteLine(context.Uris.Count + " remaining to expand");
                Debug.WriteLine("Got " + context.Store.Graphs.Count + " Graphs so far");

                if (u.Depth > context.Profile.MaxExpansionDepth) continue;
                if (context.Store.HasGraph(u.Uri)) continue;

                //Try and retrieve RDF from the next URI
                Graph g = new Graph();
                try
                {
                    UriLoader.Load(g, u.Uri);
                }
                catch (RdfException rdfEx)
                {
                    //Ignore
                    this.DebugErrors("Error: Tried to expand URI <" + u.Uri.ToString() + "> but an RDF Error occurred", rdfEx);
                }
                catch (WebException webEx)
                {
                    //Ignore
                    this.DebugErrors("Error: Tried to expand URI <" + u.Uri.ToString() + "> but a HTTP Error occurred", webEx); 
                }

                ExpandGraph(u, g, context);

                //If we've got any URIs to expand and we're not already multi-threading then spawn some
                //threads to share out the work
                if (!context.MultiThreading && context.Uris.Count > 0)
                {
                    //REQ: Convert to an IAsyncResult pattern
                    context.MultiThreading = true;
                    List<Thread> threads = new List<Thread>();
                    for (int i = 0; i < Math.Min(context.Uris.Count, this._threadsToUse); i++)
                    {
                        threads.Add(new Thread(new ThreadStart(delegate { this.Expand(context); })));
                    }
                    threads.ForEach(t => t.Start());
                    while (threads.Any(t => t.ThreadState == System.Threading.ThreadState.Running)) 
                    {
                        Thread.Sleep(ThreadPollingInterval);
                    }
                    context.MultiThreading = false;
                }
            }
        }

        private void ExpandGraph(UriToExpand u, IGraph g, ExpansionContext context)
        {
            //Can ignore empty Graphs
            //if (g.IsEmpty) return;

            //If the Graph with the Base URI doesn't already exist we add it to the store
            if (context.Store.HasGraph(g.BaseUri)) return;
            context.Store.Add(g, true);

            //If it didn't already exist we find URIs to expand upon from this Graph
            this.FindExpandableUris(u, g, context);

            //Then expand by Datasets
            this.ExpandByDatasets(u, context);
        }

        private void FindExpandableUris(UriToExpand u, IGraph g, ExpansionContext context)
        {
            if (u.Depth == context.Profile.MaxExpansionDepth) return;

            try
            {
                IUriNode current = g.CreateUriNode(u.Uri);

                //Find owl:sameAs and rdfs:seeAlso links
                foreach (Triple t in g.GetTriplesWithSubjectPredicate(current, context.SameAs))
                {
                    if (t.Object.NodeType == NodeType.Uri)
                    {
                        context.Uris.Enqueue(new UriToExpand(((IUriNode)t.Object).Uri, u.Depth + 1));
                        context.LinkGraph.Assert(t.CopyTriple(context.LinkGraph));
                    }
                }
                foreach (Triple t in g.GetTriplesWithPredicateObject(context.SameAs, current))
                {
                    if (t.Subject.NodeType == NodeType.Uri)
                    {
                        context.Uris.Enqueue(new UriToExpand(((IUriNode)t.Subject).Uri, u.Depth + 1));
                        context.LinkGraph.Assert(t.CopyTriple(context.LinkGraph));
                    }
                }
                foreach (Triple t in g.GetTriplesWithSubjectPredicate(current, context.SeeAlso))
                {
                    if (t.Object.NodeType == NodeType.Uri)
                    {
                        context.Uris.Enqueue(new UriToExpand(((IUriNode)t.Object).Uri, u.Depth + 1));
                        context.LinkGraph.Assert(t.CopyTriple(context.LinkGraph));
                    }
                }

                //Then apply Linksets
                foreach (ExpansionLinkset linkset in context.Profile.ExpansionLinksets)
                {
                    //Check if Linkset is ignored
                    if (linkset.Ignore) continue;

                    //Check if either Dataset is ignored
                    if (context.Profile.GetExpansionDataset(linkset.SubjectsTarget).Ignore) return;
                    if (context.Profile.GetExpansionDataset(linkset.ObjectsTarget).Ignore) return;

                    foreach (INode pred in linkset.Predicates)
                    {
                        //Find links from the current Graph Base URI
                        foreach (Triple t in g.GetTriplesWithSubjectPredicate(current, pred))
                        {
                            if (t.Object.NodeType == NodeType.Uri)
                            {
                                context.Uris.Enqueue(new UriToExpand(((IUriNode)t.Object).Uri, u.Depth + 1));
                                context.LinkGraph.Assert(t.CopyTriple(context.LinkGraph));
                            }
                            if (!linkset.IsDirected)
                            {
                                //If linkset is not directed then follow the link in the reverse direction
                                if (t.Subject.NodeType == NodeType.Uri)
                                {
                                    context.Uris.Enqueue(new UriToExpand(((IUriNode)t.Subject).Uri, u.Depth + 1));
                                    context.LinkGraph.Assert(t.CopyTriple(context.LinkGraph));
                                }
                            }
                        }

                        //Look for any links from other Graph URIs
                        foreach (IGraph graph in context.Store.Graphs)
                        {
                            INode curr = graph.CreateUriNode();
                            foreach (Triple t in g.GetTriplesWithSubjectPredicate(curr, pred))
                            {
                                if (t.Object.NodeType == NodeType.Uri)
                                {
                                    context.Uris.Enqueue(new UriToExpand(((IUriNode)t.Object).Uri, u.Depth + 1));
                                    context.LinkGraph.Assert(t.CopyTriple(context.LinkGraph));
                                }
                                if (!linkset.IsDirected)
                                {
                                    //If linkset is not directed then follow the link in the reverse direction
                                    if (t.Subject.NodeType == NodeType.Uri)
                                    {
                                        context.Uris.Enqueue(new UriToExpand(((IUriNode)t.Subject).Uri, u.Depth + 1));
                                        context.LinkGraph.Assert(t.CopyTriple(context.LinkGraph));
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.DebugErrors("Error: Trying to find links to follow from Graph '" + g.BaseUri.ToString() + "' when an error occurred", ex);
            }
        }

        private void ExpandByDatasets(UriToExpand u, ExpansionContext context)
        {
            if (u.Depth == context.Profile.MaxExpansionDepth) return;

            foreach (ExpansionDataset dataset in context.Profile.ExpansionDatasets)
            {
                this.ExpandByDataset(u, context, dataset);
            }
        }

        private void ExpandByDataset(UriToExpand u, ExpansionContext context, ExpansionDataset dataset)
        {
            if (u.Depth == context.Profile.MaxExpansionDepth) return;
            if (dataset.Ignore) return;

            foreach (Uri endpoint in dataset.SparqlEndpoints)
            {
                Thread.Sleep(HttpRequestInterval);
                SparqlRemoteEndpoint sparqlEndpoint = new SparqlRemoteEndpoint(endpoint);
                try
                {
                    SparqlParameterizedString queryString = new SparqlParameterizedString("DESCRIBE @uri");
                    queryString.SetUri("uri", u.Uri);
                    Object temp = sparqlEndpoint.QueryWithResultGraph(queryString.ToString());
                    if (temp is Graph)
                    {
                        Graph g = (Graph)temp;
                        this.ExpandGraph(u, g, context);
                    }
                }
                catch (RdfException rdfEx)
                {
                    this.DebugErrors("Error: Tried to DESCRIBE <" + u.Uri.ToString() + "> against the SPARQL Endpoint <" + endpoint.ToString() + "> but an error occurred:", rdfEx);
                }
                catch (WebException webEx)
                {
                    this.DebugErrors("Error: Tried to DESCRIBE <" + u.Uri.ToString() + "> against the SPARQL Endpoint <" + endpoint.ToString() + "> but an error occurred:", webEx);
                }
            }

            foreach (Uri endpoint in dataset.UriLookupEndpoints)
            {
                this.ExpandByUriLookup(u, context, endpoint);
            }

            foreach (Uri endpoint in dataset.UriDiscoveryEndpoints)
            {
                this.ExpandByUriDiscovery(u, context, endpoint);
            }
        }

        private void ExpandByUriDiscovery(UriToExpand u, ExpansionContext context, Uri discoveryEndpoint)
        {
            if (u.Depth == context.Profile.MaxExpansionDepth) return;

            StringBuilder requestUri = new StringBuilder();
            requestUri.Append(discoveryEndpoint.ToString());
            requestUri.Append(Uri.EscapeDataString(u.Uri.ToString()));

            Graph g = new Graph();
            Thread.Sleep(HttpRequestInterval);
            UriLoader.Load(g, new Uri(requestUri.ToString()));

            this.FindExpandableUris(u, g, context);
        }

        private void ExpandByUriLookup(UriToExpand u, ExpansionContext context, Uri lookupEndpoint)
        {
            if (u.Depth == context.Profile.MaxExpansionDepth) return;

            StringBuilder requestUri = new StringBuilder();
            requestUri.Append(lookupEndpoint.ToString());
            requestUri.Append(Uri.EscapeDataString(u.Uri.ToString()));

            Graph g = new Graph();
            Thread.Sleep(HttpRequestInterval);
            UriLoader.Load(g, new Uri(requestUri.ToString()));

            this.ExpandGraph(u, g, context);
        }

        private void DebugErrors(String message, Exception ex)
        {
#if DEBUG
            Debug.WriteLine(message);
            Debug.WriteLine(ex.Message);
            Debug.WriteLine(ex.StackTrace);

            Exception inner = ex.InnerException;
            while (inner != null)
            {
                Debug.WriteLine(String.Empty);
                Debug.WriteLine(inner.Message);
                Debug.WriteLine(inner.StackTrace);
                inner = inner.InnerException;
            }
#endif
        }
    }
}
