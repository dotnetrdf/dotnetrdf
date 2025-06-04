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
using System.Collections.Generic;
using System.Text;
using System.IO;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Writing;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Parsing;

/// <summary>
/// Provides caching services to the <see cref="UriLoader">UriLoader</see> class.
/// </summary>
class UriLoaderCache
    : IUriLoaderCache
{
    private string _cacheDir;
    private TimeSpan _cacheDuration = new TimeSpan(1, 0, 0);
    private bool _canCacheGraphs = false, _canCacheETag = false;
    private string _graphDir;
    private string _etagFile;
    private Dictionary<int, string> _etags = new Dictionary<int, string>();
    private CompressingTurtleWriter _ttlwriter = new CompressingTurtleWriter(WriterCompressionLevel.Medium);
    private HashSet<string> _nocache = new HashSet<string>();
    private Type _formatterType = typeof(TurtleFormatter);

    /// <summary>
    /// Creates a new Cache which uses the system temporary directory as the cache location.
    /// </summary>
    public UriLoaderCache()
        : this(Path.GetTempPath()) { }

    /// <summary>
    /// Creates a new Cache which uses the given directory as the cache location.
    /// </summary>
    /// <param name="dir">Directory.</param>
    public UriLoaderCache(string dir)
    {
        if (Directory.Exists(dir))
        {
            _cacheDir = dir;
        }
        else
        {
            throw new DirectoryNotFoundException("Cannot use a non-existent directory as the cache directory");
        }
        Initialise();
    }

    /// <summary>
    /// Gets/Sets how long results should be cached.
    /// </summary>
    /// <remarks>
    /// This only applies to downloaded URIs where an ETag is not available, where ETags are available proper ETag based caching is used.
    /// </remarks>
    public TimeSpan CacheDuration
    {
        get
        {
            return _cacheDuration;
        }
        set
        {
            _cacheDuration = value;
        }
    }

    /// <summary>
    /// Gets/Sets the Cache Directory that is used.
    /// </summary>
    public string CacheDirectory
    {
        get
        {
            return _cacheDir;
        }
        set
        {
            if (!_cacheDir.Equals(value))
            {
                _cacheDir = value;

                // Reinitialise to set up the Graphs as required
                _canCacheETag = false;
                _canCacheGraphs = false;
                Initialise();
            }
        }
    }

    /// <summary>
    /// Initialises the Cache as required.
    /// </summary>
    private void Initialise()
    {
        _graphDir = Path.Combine(_cacheDir, "dotnetrdf-graphs\\");
        if (!Directory.Exists(_graphDir))
        {
            try
            {
                Directory.CreateDirectory(_graphDir);
                _canCacheGraphs = true;
            }
            catch
            {
                // If can't create directory we can't cache Graphs
            }
        }
        else
        {
            _canCacheGraphs = true;
        }

        _etagFile = Path.Combine(_cacheDir, "dotnetrdf-etags.txt");
        if (File.Exists(_etagFile))
        {
            try
            {
                // Read in the existing ETags
                using (var reader = new StreamReader(File.Open(_etagFile, FileMode.Open, FileAccess.Read), Encoding.UTF8))
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        try
                        {
                            var data = line.Split('\t');
                            var i = int.Parse(data[0]);
                            var etag = data[1];

                            if (!_etags.ContainsKey(i))
                            {
                                _etags.Add(i, etag);
                            }
                        }
                        catch
                        {
                            // Ignore this line and continue if we can
                        }
                    }
                }
                _canCacheETag = true;
            }
            catch
            {
                // If error then we can't cache ETags
            }
        }
        else
        {
            // Try and create ETags Cache File
            try
            {
                File.Create(_etagFile);
                _canCacheETag = true;
            }
            catch
            {
                // If can't create file we can't cache ETags
            }
        }
    }

    /// <summary>
    /// Clears the Cache.
    /// </summary>
    public void Clear()
    {
        // Clear the ETag Cache
        _etags.Clear();
        if (_canCacheETag && File.Exists(_etagFile))
        {
            File.WriteAllText(_etagFile, string.Empty, Encoding.UTF8);
        }

        // Clear the Graphs Cache
        if (_canCacheGraphs && Directory.Exists(_graphDir))
        {
            foreach (var file in Directory.GetFiles(_graphDir))
            {
                File.Delete(file);
            }
        }
    }

    /// <summary>
    /// Gets whether there is an ETag for the given URI.
    /// </summary>
    /// <param name="u">URI.</param>
    /// <returns></returns>
    public bool HasETag(Uri u)
    {
        if (_canCacheETag)
        {
            if (_nocache.Contains(u.GetSha256Hash())) return false;
            return _etags.ContainsKey(u.GetEnhancedHashCode()) && HasLocalCopy(u, false);
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Gets the ETag for the given URI.
    /// </summary>
    /// <param name="u">URI.</param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException">Thrown if there is no ETag for the given URI.</exception>
    public string GetETag(Uri u)
    {
        if (_canCacheETag)
        {
            if (_nocache.Contains(u.GetSha256Hash())) throw new KeyNotFoundException("No ETag was found for the URI " + u.AbsoluteUri);
            var id = u.GetEnhancedHashCode();
            if (_etags.ContainsKey(id))
            {
                return _etags[id];
            }
            else
            {
                throw new KeyNotFoundException("No ETag was found for the URI " + u.AbsoluteUri);
            }
        }
        else
        {
            throw new KeyNotFoundException("No ETag was found for the URI " + u.AbsoluteUri);
        }
    }

    /// <summary>
    /// Remove the ETag record for the given URI.
    /// </summary>
    /// <param name="u">URI.</param>
    public void RemoveETag(Uri u)
    {
        try
        {
            if (_canCacheETag)
            {
                if (_etags.ContainsKey(u.GetEnhancedHashCode()))
                {
                    _etags.Remove(u.GetEnhancedHashCode());
                    // If we did remove an ETag then we need to rewrite our ETag cache file
                    using (var writer = new StreamWriter(File.Open(_etagFile, FileMode.Create, FileAccess.Write), Encoding.UTF8))
                    {
                        foreach (KeyValuePair<int, string> etag in _etags)
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
            // If an IO Exception occurs ignore it, something went wrong with cache alteration
        }
    }

    /// <summary>
    /// Removes a locally cached copy of a URIs results from the Cache.
    /// </summary>
    /// <param name="u">URI.</param>
    public void RemoveLocalCopy(Uri u)
    {
        if (u == null) return;

        try
        {
            var graph = Path.Combine(_graphDir, u.GetSha256Hash());
            if (File.Exists(graph))
            {
                File.Delete(graph);
            }
        }
        catch
        {
            // If error add to the list of uncachable URIs
            _nocache.Add(u.GetSha256Hash());
        }
    }

    /// <summary>
    /// Is there a locally cached copy of the Graph from the given URI which is not expired.
    /// </summary>
    /// <param name="u">URI.</param>
    /// <param name="requireFreshness">Whether the local copy is required to meet the Cache Freshness (set by the Cache Duration).</param>
    /// <returns></returns>
    public bool HasLocalCopy(Uri u, bool requireFreshness)
    {
        try
        {
            if (_canCacheGraphs)
            {
                if (_nocache.Contains(u.GetSha256Hash())) return false;

                var graph = Path.Combine(_graphDir, u.GetSha256Hash());
                if (File.Exists(graph))
                {
                    if (requireFreshness)
                    {
                        // Check the freshness of the local copy
                        DateTime created = File.GetCreationTime(graph);
                        TimeSpan freshness = DateTime.Now - created;
                        if (freshness > _cacheDuration)
                        {
                            // Local copy has expired
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
            // If we get an error trying to detect if a URI is cached then it can't be in the cache
            return false;
        }
    }

    /// <summary>
    /// Gets the path to the locally cached copy of the Graph from the given URI.
    /// </summary>
    /// <param name="u">URI.</param>
    /// <returns></returns>
    /// <remarks>
    /// This method does not do any cache expiry calculations on the file.  This is due to the fact that we'll store local copies of Graphs for which we have ETags and when using ETags we rely on the servers knowledge of whether the resource described by the URI has changed rather than some arbitrary caching duration that we/the user has set to use.
    /// </remarks>
    public string GetLocalCopy(Uri u)
    {
        if (_canCacheGraphs)
        {
            if (_nocache.Contains(u.GetSha256Hash())) return null;

            var graph = Path.Combine(_graphDir, u.GetSha256Hash());
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

    public IRdfHandler ToCache(Uri requestUri, Uri responseUri, string etag)
    {
        IRdfHandler handler = null;
        try
        {
            var cacheTwice = !requestUri.AbsoluteUri.Equals(responseUri.AbsoluteUri, StringComparison.OrdinalIgnoreCase);

            // Cache the ETag if present
            if (_canCacheETag && etag != null && !etag.Equals(string.Empty))
            {
                var id = requestUri.GetEnhancedHashCode();
                var requireAdd = false;
                if (_etags.ContainsKey(id))
                {
                    if (!_etags[id].Equals(etag))
                    {
                        // If the ETag has changed remove it and then re-add it
                        RemoveETag(requestUri);
                        requireAdd = true;
                    }
                }
                else
                {
                    requireAdd = true;
                }

                if (requireAdd)
                {
                    // Add a New ETag
                    _etags.Add(id, etag);
                    using (var writer = new StreamWriter(File.Open(_etagFile, FileMode.Append, FileAccess.Write), Encoding.UTF8))
                    {
                        writer.WriteLine(id + "\t" + etag);
                        writer.Close();
                    }
                }

                // Cache under the Response URI as well if applicable
                if (cacheTwice)
                {
                    id = responseUri.GetEnhancedHashCode();
                    requireAdd = false;
                    if (_etags.ContainsKey(id))
                    {
                        if (!_etags[id].Equals(etag))
                        {
                            // If the ETag has changed remove it and then re-add it
                            RemoveETag(responseUri);
                            requireAdd = true;
                        }
                    }
                    else
                    {
                        requireAdd = true;
                    }

                    if (requireAdd)
                    {
                        using (var writer = new StreamWriter(File.Open(_etagFile, FileMode.Append, FileAccess.Write), Encoding.UTF8))
                        {
                            writer.WriteLine(id + "\t" + etag);
                            writer.Close();
                        }
                    }
                }
            }

            // Then if we are caching Graphs return WriteThroughHandlers to do the caching for us
            if (_canCacheGraphs)
            {
                var graph = Path.Combine(_graphDir, requestUri.GetSha256Hash());
                handler = new WriteThroughHandler(_formatterType, new StreamWriter(File.Open(graph, FileMode.Append)));

                if (cacheTwice)
                {
                    graph = Path.Combine(_graphDir, responseUri.GetSha256Hash());

                    // We should use the original handler in its capacity as node factory,
                    // otherwise there might be unexpected differences between its output
                    // and that of the MultiHandler's
                    handler = new MultiHandler(new IRdfHandler[] { handler, new WriteThroughHandler(_formatterType, new StreamWriter(File.Open(graph, FileMode.Append, FileAccess.Write)), true) }, handler);
                }
            }
        }
        catch (IOException)
        {
            // Ignore - if we get an IO Exception we failed to cache somehow
        }
        catch (RdfOutputException)
        {
            // Ignore - if we get an RDF Output Exception then we failed to cache
        }
        return handler;
    }
}