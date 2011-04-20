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

#if !NO_URICACHE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Writing;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Parsing
{
    /// <summary>
    /// Provides caching services to the <see cref="UriLoader">UriLoader</see> class
    /// </summary>
    class UriLoaderCache : IUriLoaderCache
    {
        private String _cacheDir;
        private TimeSpan _cacheDuration = new TimeSpan(1, 0, 0);
        private bool _canCacheGraphs = false, _canCacheETag = false;
        private String _graphDir;
        private String _etagFile;
        private Dictionary<int, String> _etags = new Dictionary<int, string>();
        private CompressingTurtleWriter _ttlwriter = new CompressingTurtleWriter(WriterCompressionLevel.Medium);
        private HashSet<String> _nocache = new HashSet<string>();
        private Type _formatterType = typeof(TurtleFormatter);

        /// <summary>
        /// Creates a new Cache which uses the system temporary directory as the cache location
        /// </summary>
        public UriLoaderCache()
            : this(Path.GetTempPath()) { }

        /// <summary>
        /// Creates a new Cache which uses the given directory as the cache location
        /// </summary>
        /// <param name="dir">Directory</param>
        public UriLoaderCache(String dir)
        {
            if (Directory.Exists(dir))
            {
                this._cacheDir = dir;
            }
            else
            {
                throw new DirectoryNotFoundException("Cannot use a non-existent directory as the cache directory");
            }
            this.Initialise();
        }

        /// <summary>
        /// Gets/Sets how long results should be cached
        /// </summary>
        /// <remarks>
        /// This only applies to downloaded URIs where an ETag is not available, where ETags are available proper ETag based caching is used
        /// </remarks>
        public TimeSpan CacheDuration
        {
            get
            {
                return this._cacheDuration;
            }
            set
            {
                this._cacheDuration = value;
            }
        }

        /// <summary>
        /// Gets/Sets the Cache Directory that is used
        /// </summary>
        public String CacheDirectory
        {
            get
            {
                return this._cacheDir;
            }
            set
            {
                if (!this._cacheDir.Equals(value))
                {
                    this._cacheDir = value;

                    //Reinitialise to set up the Graphs as required
                    this._canCacheETag = false;
                    this._canCacheGraphs = false;
                    this.Initialise();
                }
            }
        }

        /// <summary>
        /// Initialises the Cache as required
        /// </summary>
        private void Initialise()
        {
            this._graphDir = Path.Combine(this._cacheDir, "dotnetrdf-graphs\\");
            if (!Directory.Exists(this._graphDir))
            {
                try
                {
                    Directory.CreateDirectory(this._graphDir);
                    this._canCacheGraphs = true;
                }
                catch
                {
                    //If can't create directory we can't cache Graphs
                }
            }
            else
            {
                this._canCacheGraphs = true;
            }

            this._etagFile = Path.Combine(this._cacheDir, "dotnetrdf-etags.txt");
            if (File.Exists(this._etagFile))
            {
                try
                {
                    //Read in the existing ETags
                    using (StreamReader reader = new StreamReader(this._etagFile, Encoding.UTF8))
                    {
                        while (!reader.EndOfStream)
                        {
                            String line = reader.ReadLine();
                            try
                            {
                                String[] data = line.Split('\t');
                                int i = Int32.Parse(data[0]);
                                String etag = data[1];

                                if (!this._etags.ContainsKey(i))
                                {
                                    this._etags.Add(i, etag);
                                }
                            }
                            catch
                            {
                                //Ignore this line and continue if we can
                            }
                        }
                    }
                    this._canCacheETag = true;
                }
                catch
                {
                    //If error then we can't cache ETags
                }
            }
            else
            {
                //Try and create ETags Cache File
                try
                {
                    File.Create(this._etagFile);
                    this._canCacheETag = true;
                }
                catch
                {
                    //If can't create file we can't cache ETags
                }
            }
        }

        /// <summary>
        /// Clears the Cache
        /// </summary>
        public void Clear()
        {
            //Clear the ETag Cache
            this._etags.Clear();
            if (this._canCacheETag && File.Exists(this._etagFile))
            {
                File.WriteAllText(this._etagFile, String.Empty, Encoding.UTF8);
            }

            //Clear the Graphs Cache
            if (this._canCacheGraphs && Directory.Exists(this._graphDir))
            {
                foreach (String file in Directory.GetFiles(this._graphDir))
                {
                    File.Delete(file);
                }
            }
        }

        /// <summary>
        /// Gets whether there is an ETag for the given URI
        /// </summary>
        /// <param name="u">URI</param>
        /// <returns></returns>
        public bool HasETag(Uri u)
        {
            if (this._canCacheETag)
            {
                if (this._nocache.Contains(u.GetSha256Hash())) return false;
                return this._etags.ContainsKey(u.GetEnhancedHashCode()) && this.HasLocalCopy(u, false);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the ETag for the given URI
        /// </summary>
        /// <param name="u">URI</param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException">Thrown if there is no ETag for the given URI</exception>
        public String GetETag(Uri u)
        {
            if (this._canCacheETag)
            {
                if (this._nocache.Contains(u.GetSha256Hash())) throw new KeyNotFoundException("No ETag was found for the URI " + u.ToString());
                int id = u.GetEnhancedHashCode();
                if (this._etags.ContainsKey(id))
                {
                    return this._etags[id];
                }
                else
                {
                    throw new KeyNotFoundException("No ETag was found for the URI " + u.ToString());
                }
            }
            else
            {
                throw new KeyNotFoundException("No ETag was found for the URI " + u.ToString());
            }
        }

        /// <summary>
        /// Remove the ETag record for the given URI
        /// </summary>
        /// <param name="u">URI</param>
        public void RemoveETag(Uri u)
        {
            try
            {
                if (this._canCacheETag)
                {
                    if (this._etags.ContainsKey(u.GetEnhancedHashCode()))
                    {
                        this._etags.Remove(u.GetEnhancedHashCode());
                        //If we did remove an ETag then we need to rewrite our ETag cache file
                        using (StreamWriter writer = new StreamWriter(this._etagFile, false, Encoding.UTF8))
                        {
                            foreach (KeyValuePair<int, String> etag in this._etags)
                            {
                                writer.WriteLine(etag.Key + "\t" + etag.Value);
                            }
                            writer.Close();
                        }
                    }
                }
            }
            catch (IOException)
            {
                //If an IO Exception occurs ignore it, something went wrong with cache alteration
            }
        }

        /// <summary>
        /// Removes a locally cached copy of a URIs results from the Cache
        /// </summary>
        /// <param name="u">URI</param>
        public void RemoveLocalCopy(Uri u)
        {
            if (u == null) return;

            try
            {
                String graph = Path.Combine(this._graphDir, u.GetSha256Hash());
                if (File.Exists(graph))
                {
                    File.Delete(graph);
                }
            }
            catch
            {
                //If error add to the list of uncachable URIs
                this._nocache.Add(u.GetSha256Hash());
            }
        }

        /// <summary>
        /// Is there a locally cached copy of the Graph from the given URI which is not expired
        /// </summary>
        /// <param name="u">URI</param>
        /// <param name="requireFreshness">Whether the local copy is required to meet the Cache Freshness (set by the Cache Duration)</param>
        /// <returns></returns>
        public bool HasLocalCopy(Uri u, bool requireFreshness)
        {
            try
            {
                if (this._canCacheGraphs)
                {
                    if (this._nocache.Contains(u.GetSha256Hash())) return false;

                    String graph = Path.Combine(this._graphDir, u.GetSha256Hash());
                    if (File.Exists(graph))
                    {
                        if (requireFreshness)
                        {
                            //Check the freshness of the local copy
                            DateTime created = File.GetCreationTime(graph);
                            TimeSpan freshness = DateTime.Now - created;
                            if (freshness > this._cacheDuration)
                            {
                                //Local copy has expired
                                File.Delete(graph);
                                return false;
                            }
                            else
                            {
                                return true;
                            }
                        }
                        else
                        {
                            return true;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                //If we get an error trying to detect if a URI is cached then it can't be in the cache
                return false;
            }
        }

        /// <summary>
        /// Gets the path to the locally cached copy of the Graph from the given URI
        /// </summary>
        /// <param name="u">URI</param>
        /// <returns></returns>
        /// <remarks>
        /// This method does not do any cache expiry calculations on the file.  This is due to the fact that we'll store local copies of Graphs for which we have ETags and when using ETags we rely on the servers knowledge of whether the resource described by the URI has changed rather than some arbitrary caching duration that we/the user has set to use.
        /// </remarks>
        public String GetLocalCopy(Uri u)
        {
            if (this._canCacheGraphs)
            {
                if (this._nocache.Contains(u.GetSha256Hash())) return null;

                String graph = Path.Combine(this._graphDir, u.GetSha256Hash());
                if (File.Exists(graph))
                {
                    return graph;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public IRdfHandler ToCache(Uri requestUri, Uri responseUri, String etag)
        {
            IRdfHandler handler = null;
            try
            {
                bool cacheTwice = !requestUri.ToString().Equals(responseUri.ToString(), StringComparison.OrdinalIgnoreCase);

                //Cache the ETag if present
                if (this._canCacheETag && etag != null && !etag.Equals(String.Empty))
                {
                    int id = requestUri.GetEnhancedHashCode();
                    bool requireAdd = false;
                    if (this._etags.ContainsKey(id))
                    {
                        if (!this._etags[id].Equals(etag))
                        {
                            //If the ETag has changed remove it and then re-add it
                            this.RemoveETag(requestUri);
                            requireAdd = true;
                        }
                    }
                    else
                    {
                        requireAdd = true;
                    }

                    if (requireAdd)
                    {
                        //Add a New ETag
                        this._etags.Add(id, etag);
                        using (StreamWriter writer = new StreamWriter(this._etagFile, true, Encoding.UTF8))
                        {
                            writer.WriteLine(id + "\t" + etag);
                            writer.Close();
                        }
                    }

                    //Cache under the Response URI as well if applicable
                    if (cacheTwice)
                    {
                        id = responseUri.GetEnhancedHashCode();
                        requireAdd = false;
                        if (this._etags.ContainsKey(id))
                        {
                            if (!this._etags[id].Equals(etag))
                            {
                                //If the ETag has changed remove it and then re-add it
                                this.RemoveETag(responseUri);
                                requireAdd = true;
                            }
                        }
                        else
                        {
                            requireAdd = true;
                        }

                        if (requireAdd)
                        {
                            using (StreamWriter writer = new StreamWriter(this._etagFile, true, Encoding.UTF8))
                            {
                                writer.WriteLine(id + "\t" + etag);
                                writer.Close();
                            }
                        }
                    }
                }

                //Then if we are caching Graphs return WriteThroughHandlers to do the caching for us
                if (this._canCacheGraphs)
                {
                    String graph = Path.Combine(this._graphDir, requestUri.GetSha256Hash());
                    handler = new WriteThroughHandler(this._formatterType, new StreamWriter(graph), true);

                    if (cacheTwice)
                    {
                        graph = Path.Combine(this._graphDir, responseUri.GetSha256Hash());
                        handler = new MultiHandler(new IRdfHandler[] { handler, new WriteThroughHandler(this._formatterType, new StreamWriter(graph), true) });
                    }
                }
            }
            catch (IOException)
            {
                //Ignore - if we get an IO Exception we failed to cache somehow
            }
            catch (RdfOutputException)
            {
                //Ignore - if we get an RDF Output Exception then we failed to cache
            }
            return handler;
        }

        /// <summary>
        /// Caches a Graph in the Cache
        /// </summary>
        /// <param name="requestUri">URI from which the Graph was requested</param>
        /// <param name="responseUri">The actual URI which responded to the request</param>
        /// <param name="g">Graph</param>
        /// <param name="etag">ETag</param>
        [Obsolete("This form of the ToCache() method is obsolete and should not be used", true)]
        public void ToCache(Uri requestUri, Uri responseUri, IGraph g, String etag)
        {
            //Cache a local copy of the Graph
            try
            {
                bool cacheTwice = !requestUri.ToString().Equals(responseUri.ToString() , StringComparison.OrdinalIgnoreCase);

                if (this._canCacheGraphs)
                {
                    String graph = Path.Combine(this._graphDir, requestUri.GetSha256Hash());
                    this._ttlwriter.Save(g, graph);

                    //If applicable also cache under the responseUri
                    if (cacheTwice)
                    {
                        graph = Path.Combine(this._graphDir, responseUri.GetSha256Hash());
                        this._ttlwriter.Save(g, graph);
                    }
                }

                //Cache the ETag if present
                if (this._canCacheETag && etag != null && !etag.Equals(String.Empty))
                {
                    int id = requestUri.GetEnhancedHashCode();
                    bool requireAdd = false;
                    if (this._etags.ContainsKey(id))
                    {
                        if (!this._etags[id].Equals(etag))
                        {
                            //If the ETag has changed remove it and then re-add it
                            this.RemoveETag(requestUri);
                            requireAdd = true;
                        }
                    }
                    else
                    {
                        requireAdd = true;
                    }

                    if (requireAdd)
                    {
                        //Add a New ETag
                        this._etags.Add(id, etag);
                        using (StreamWriter writer = new StreamWriter(this._etagFile, true, Encoding.UTF8))
                        {
                            writer.WriteLine(id + "\t" + etag);
                            writer.Close();
                        }
                    }

                    //Cache under the Response URI as well if applicable
                    if (cacheTwice)
                    {
                        id = responseUri.GetEnhancedHashCode();
                        requireAdd = false;
                        if (this._etags.ContainsKey(id))
                        {
                            if (!this._etags[id].Equals(etag))
                            {
                                //If the ETag has changed remove it and then re-add it
                                this.RemoveETag(responseUri);
                                requireAdd = true;
                            }
                        }
                        else
                        {
                            requireAdd = true;
                        }

                        if (requireAdd)
                        {
                            using (StreamWriter writer = new StreamWriter(this._etagFile, true, Encoding.UTF8))
                            {
                                writer.WriteLine(id + "\t" + etag);
                                writer.Close();
                            }
                        }
                    }
                }
            }
            catch (IOException)
            {
                //Ignore - if we get an IO Exception we failed to cache somehow
            }
            catch (RdfOutputException)
            {
                //Ignore - if we get an RDF Output Exception then we failed to cache
            }
        }
    }
}

#endif