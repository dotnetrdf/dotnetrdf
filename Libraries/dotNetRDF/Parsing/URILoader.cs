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
using System.IO;
using System.Net;
using VDS.RDF.Parsing.Handlers;

namespace VDS.RDF.Parsing
{
    /// <summary>
    /// Static Helper Class for dereferencing URIs and attempting to parse the results of a HTTP GET request to the URI into RDF
    /// </summary>
    /// <remarks>
    /// <h3>Caching</h3>
    /// <para>
    /// As of the 0.2.2 release the loader has support for caching retrieved data locally built into it (for Graphs only), caching is done using ETags where the remote server provides them or just by a user-defineable 'freshness' criteria (i.e. number of hours retrieved resources should be cached for).  By default this caching happens in the system temporary directory which means it is non-persistent i.e. if you run your application using dotNetRDF it may cache stuff during the session but once the application is closed the operating system may freely delete the cached data.  If you wish to have a persistent cache then you can use the <see cref="UriLoader.CacheDirectory">CacheDirectory</see> property to set your own cache directory.  Even when you set your own cache directory dotNetRDF will delete obsolete data from it over time though this will only happen when a new request invalidates previously cached data.
    /// </para>
    /// <para>
    /// If you wish to completely control the Cache you can implement your own <see cref="IUriLoaderCache">IUriLoaderCache</see> implementation and use it by setting the <see cref="UriLoader.Cache">Cache</see> property
    /// </para>
    /// </remarks>
    public static partial class UriLoader
    {
        private static String _userAgent;

        #region URI Caching
        private static IUriLoaderCache _cache = new UriLoaderCache();

        /// <summary>
        /// Gets/Sets the Directory used for caching Graphs loaded from URIs
        /// </summary>
        public static String CacheDirectory
        {
            get
            {
                return _cache.CacheDirectory;
            }
            set
            {
                _cache.CacheDirectory = value;
            }
        }

        /// <summary>
        /// Gets/Sets the amount of time Graphs are cached for
        /// </summary>
        /// <remarks>
        /// This duration only applies to URIs which don't return an ETag as part of the HTTP response when they are deferenced
        /// </remarks>
        public static TimeSpan CacheDuration
        {
            get
            {
                return _cache.CacheDuration;
            }
            set
            {
                _cache.CacheDuration = value;
            }
        }

        /// <summary>
        /// Gets/Sets the Cache that is in use
        /// </summary>
        /// <remarks>
        /// Setting the Cache to null does not disable it, to disable caching use the <see cref="Options.UriLoaderCaching">Options.UriLoaderCaching</see> property.
        /// </remarks>
        public static IUriLoaderCache Cache
        {
            get
            {
                return _cache;
            }
            set
            {
                if (value != null)
                {
                    _cache = value;
                }
            }
        }

        /// <summary>
        /// Determines whether the RDF behind the given URI is cached
        /// </summary>
        /// <param name="u">URI</param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// <strong>Note:</strong> This does not guarantee that the cached content will be used if you load from the URI using the UriLoader.  Whether the cached copy is used will depend on whether 
        /// </para>
        /// </remarks>
        public static bool IsCached(Uri u)
        {
            Uri temp = Tools.StripUriFragment(u);
            return _cache.HasLocalCopy(temp, false);
        }
        #endregion

        /// <summary>
        /// Gets/Sets an optional User Agent string that will be appended to HTTP Requests
        /// </summary>
        public static String UserAgent
        {
            get
            {
                return _userAgent;
            }
            set
            {
                _userAgent = value;
            }
        }

        /// <summary>
        /// Attempts to load a RDF Graph from the given URI into the given Graph
        /// </summary>
        /// <param name="g">Graph to assert Triples in</param>
        /// <param name="u">URI to attempt to get RDF from</param>
        /// <remarks>
        /// <para>
        /// Attempts to select the relevant Parser based on the Content Type header returned in the HTTP Response.
        /// </para>
        /// <para>
        /// If you know ahead of time the Content Type you can just open a HTTP Stream yourself and pass it to an instance of the correct Parser.
        /// </para>
        /// <para>
        /// In the event that the URI is a File URI the <see cref="FileLoader">FileLoader</see> will be used instead.  If the URI is a Data URI then the <see cref="DataUriLoader">DataUriLoader</see> will be used instead.
        /// </para>
        /// <para>
        /// <strong>Note:</strong> UriLoader will assign the Graph the source URI as it's Base URI unless the Graph already has a Base URI or is non-empty prior to attempting parsing.  Note that any Base URI specified in the RDF contained in the file will override this initial Base URI.  In some cases this may lead to invalid RDF being accepted and generating strange relative URIs, if you encounter this either set a Base URI prior to calling this method or create an instance of the relevant parser and invoke it directly.
        /// </para>
        /// </remarks>
        public static void Load(IGraph g, Uri u)
        {
            Load(g, u, null);
        }

        /// <summary>
        /// Attempts to load a RDF Graph from the given URI into the given Graph
        /// </summary>
        /// <param name="g">Graph to assert Triples in</param>
        /// <param name="u">URI to attempt to get RDF from</param>
        /// <param name="parser">Parser to use</param>
        /// <remarks>
        /// <para>
        /// Uses the supplied parser to attempt parsing regardless of the actual Content Type returned
        /// </para>
        /// <para>
        /// In the event that the URI is a File URI the <see cref="FileLoader">FileLoader</see> will be used instead.  If the URI is a Data URI then the <see cref="DataUriLoader">DataUriLoader</see> will be used instead.
        /// </para>
        /// <para>
        /// <strong>Note:</strong> UriLoader will assign the Graph the source URI as it's Base URI unless the Graph already has a Base URI or is non-empty prior to attempting parsing.  Note that any Base URI specified in the RDF contained in the file will override this initial Base URI.  In some cases this may lead to invalid RDF being accepted and generating strange relative URIs, if you encounter this either set a Base URI prior to calling this method or create an instance of the relevant parser and invoke it directly.
        /// </para>
        /// </remarks>
        public static void Load(IGraph g, Uri u, IRdfReader parser)
        {
            if (g == null) throw new RdfParseException("Cannot read RDF into a null Graph");
            if (u == null) throw new RdfParseException("Cannot load RDF from a null URI");
            if (u.IsFile)
            {
                // Invoke FileLoader instead
                RaiseWarning("This is a file: URI so invoking the FileLoader instead");
                if (Path.DirectorySeparatorChar == '/')
                {
                    FileLoader.Load(g, u.ToString().Substring(7), parser);
                }
                else
                {
                    FileLoader.Load(g, u.ToString().Substring(8), parser);
                }
                return;
            }
            if (u.Scheme.Equals("data"))
            {
                // Invoke DataUriLoader instead
                RaiseWarning("This is a data: URI so invoking the DataUriLoader instead");
                DataUriLoader.Load(g, u);
                return;
            }

            // Set Base Uri if necessary
            if (g.BaseUri == null && g.IsEmpty) g.BaseUri = u;

            Load(new GraphHandler(g), u, parser);
        }

        /// <summary>
        /// Attempts to load a RDF Graph from the given URI using a RDF Handler
        /// </summary>
        /// <param name="handler">RDF Handler to use</param>
        /// <param name="u">URI to attempt to get RDF from</param>
        /// <remarks>
        /// <para>
        /// Attempts to select the relevant Parser based on the Content Type header returned in the HTTP Response.
        /// </para>
        /// <para>
        /// If you know ahead of time the Content Type you can just open a HTTP Stream yourself and pass it to an instance of the correct Parser.
        /// </para>
        /// <para>
        /// In the event that the URI is a File URI the <see cref="FileLoader">FileLoader</see> will be used instead.  If the URI is a Data URI then the <see cref="DataUriLoader">DataUriLoader</see> will be used instead.
        /// </para>
        /// </remarks>
        public static void Load(IRdfHandler handler, Uri u)
        {
            Load(handler, u, (IRdfReader)null);
        }

        /// <summary>
        /// Attempts to load a RDF Graph from the given URI using a RDF Handler
        /// </summary>
        /// <param name="handler">RDF Handler to use</param>
        /// <param name="u">URI to attempt to get RDF from</param>
        /// <param name="parser">Parser to use</param>
        /// <remarks>
        /// <para>
        /// Uses the supplied parser to attempt parsing regardless of the actual Content Type returned
        /// </para>
        /// <para>
        /// In the event that the URI is a File URI the <see cref="FileLoader">FileLoader</see> will be used instead
        /// </para>
        /// <para>
        /// If the URI is a Data URI then the <see cref="DataUriLoader">DataUriLoader</see> will be used instead.
        /// </para>
        /// </remarks>
        public static void Load(IRdfHandler handler, Uri u, IRdfReader parser)
        {
            if (handler == null) throw new RdfParseException("Cannot read RDF using a null RDF Handler");
            if (u == null) throw new RdfParseException("Cannot load RDF from a null URI");
            try
            {
                if (u.IsFile)
                {
                    // Invoke FileLoader instead
                    RaiseWarning("This is a file: URI so invoking the FileLoader instead");
                    if (Path.DirectorySeparatorChar == '/')
                    {
                        FileLoader.Load(handler, u.ToString().Substring(7), parser);
                    }
                    else
                    {
                        FileLoader.Load(handler, u.ToString().Substring(8), parser);
                    }
                    return;
                }
                if (u.Scheme.Equals("data"))
                {
                    // Invoke DataUriLoader instead
                    RaiseWarning("This is a data: URI so invoking the DataUriLoader instead");
                    DataUriLoader.Load(handler, u);
                    return;
                }

                // Sanitise the URI to remove any Fragment ID
                u = Tools.StripUriFragment(u);

                // Use Cache if possible
                String etag = String.Empty;
                String local = null;
                if (Options.UriLoaderCaching)
                {
                    if (_cache.HasETag(u))
                    {
                        // Get the ETag and then we'll include an If-None-Match header in our request
                        etag = _cache.GetETag(u);
                    }
                    else if (_cache.HasLocalCopy(u, true))
                    {
                        // Just try loading from the local copy
                        local = _cache.GetLocalCopy(u);
                        if (local != null)
                        {
                            try
                            {
                                FileLoader.Load(handler, local, new TurtleParser());
                            }
                            catch
                            {
                                // If we get an Exception we failed to access the file successfully
                                _cache.RemoveETag(u);
                                _cache.RemoveLocalCopy(u);
                                Load(handler, u, parser);
                            }
                            return;
                        }
                    }
                }

                // Set-up the Request
                HttpWebRequest httpRequest;
                httpRequest = (HttpWebRequest)WebRequest.Create(u);

                // Want to ask for RDF formats
                if (parser != null)
                {
                    // If a non-null parser set up a HTTP Header that is just for the given parser
                    httpRequest.Accept = MimeTypesHelper.CustomHttpAcceptHeader(parser);
                }
                else
                {
                    httpRequest.Accept = MimeTypesHelper.HttpAcceptHeader;
                }

                if (Options.UriLoaderCaching)
                {
                    if (!etag.Equals(String.Empty))
                    {
                        httpRequest.Headers[HttpRequestHeader.IfNoneMatch] = etag;
                    }
                }

                // Use HTTP GET
                httpRequest.Method = "GET";
#if !NETCORE
                httpRequest.Timeout = Options.UriLoaderTimeout;
#endif
                if (_userAgent != null && !_userAgent.Equals(String.Empty))
                {
#if NETCORE
                    httpRequest.Headers[HttpRequestHeader.UserAgent] = _userAgent;
#else
                    httpRequest.UserAgent = _userAgent;
#endif
                }

                Tools.HttpDebugRequest(httpRequest);

                using (HttpWebResponse httpResponse = (HttpWebResponse)httpRequest.GetResponse())
                {
                    Tools.HttpDebugResponse(httpResponse);

                    if (Options.UriLoaderCaching)
                    {
                        // Are we using ETag based caching?
                        if (!etag.Equals(String.Empty))
                        {
                            // Did we get a Not-Modified response?
                            if (httpResponse.StatusCode == HttpStatusCode.NotModified)
                            {
                                // If so then we need to load the Local Copy assuming it exists?
                                if (_cache.HasLocalCopy(u, false))
                                {
                                    local = _cache.GetLocalCopy(u);
                                    try
                                    {
                                        FileLoader.Load(handler, local, new TurtleParser());
                                    }
                                    catch
                                    {
                                        // If we get an Exception we failed to access the file successfully
                                        _cache.RemoveETag(u);
                                        _cache.RemoveLocalCopy(u);
                                        Load(handler, u, parser);
                                    }
                                    return;
                                }
                                else
                                {
                                    // If the local copy didn't exist then we need to redo the response without
                                    // the ETag as we've lost the cached copy somehow
                                    _cache.RemoveETag(u);
                                    Load(handler, u, parser);
                                    return;
                                }
                            }
                            // If we didn't get a Not-Modified response then we'll continue and parse the new response
                        }
                    }

                    // Get a Parser and Load the RDF
                    if (parser == null)
                    {
                        // Only need to auto-detect the parser if a specific one wasn't specified
                        parser = MimeTypesHelper.GetParser(httpResponse.ContentType);
                    }
                    parser.Warning += RaiseWarning;
                    // To do caching we ask the cache to give us a handler and then we tie it to
                    if (Options.UriLoaderCaching)
                    {
                        IRdfHandler cacheHandler = _cache.ToCache(u, Tools.StripUriFragment(httpResponse.ResponseUri), httpResponse.Headers["ETag"]);
                        if (cacheHandler != null)
                        {
                            // Note: We can ONLY use caching when we know that the Handler will accept all the data returned
                            // i.e. if the Handler may trim the data in some way then we shouldn't cache the data returned
                            if (handler.AcceptsAll)
                            {
                                // We should use the original handler in its capacity as node factory,
                                // otherwise there might be unexpected differences between its output
                                // and that of the MultiHandler's
                                handler = new MultiHandler(new IRdfHandler[] { handler, cacheHandler }, handler);
                            }
                            else
                            {
                                cacheHandler = null;
                            }
                        }
                    }
                    try
                    {
                        parser.Load(handler, new StreamReader(httpResponse.GetResponseStream()));
                    }
                    catch
                    {
                        // If we were trying to cache the response and something went wrong discard the cached copy
                        if (Options.UriLoaderCaching)
                        {
                            _cache.RemoveETag(u);
                            _cache.RemoveETag(Tools.StripUriFragment(httpResponse.ResponseUri));
                            _cache.RemoveLocalCopy(u);
                            _cache.RemoveLocalCopy(Tools.StripUriFragment(httpResponse.ResponseUri));
                        }
                        throw;
                    }
                }
            }
            catch (UriFormatException uriEx)
            {
                // Uri Format Invalid
                throw new RdfParseException("Unable to load from the given URI '" + u.AbsoluteUri + "' since it's format was invalid", uriEx);
            }
            catch (WebException webEx)
            {
                if (webEx.Response != null)
                {
                    Tools.HttpDebugResponse((HttpWebResponse)webEx.Response);
                }

                if (Options.UriLoaderCaching)
                {
                    if (webEx.Response != null)
                    {
                        if (((HttpWebResponse)webEx.Response).StatusCode == HttpStatusCode.NotModified)
                        {
                            // If so then we need to load the Local Copy assuming it exists?
                            if (_cache.HasLocalCopy(u, false))
                            {
                                String local = _cache.GetLocalCopy(u);
                                try
                                {
                                    FileLoader.Load(handler, local, new TurtleParser());
                                }
                                catch
                                {
                                    // If we get an Exception we failed to access the file successfully
                                    _cache.RemoveETag(u);
                                    _cache.RemoveLocalCopy(u);
                                    Load(handler, u, parser);
                                }
                                return;
                            }
                            else
                            {
                                // If the local copy didn't exist then we need to redo the response without
                                // the ETag as we've lost the cached copy somehow
                                _cache.RemoveETag(u);
                                Load(handler, u, parser);
                                return;
                            }
                        }
                    }
                }

                // Some sort of HTTP Error occurred
                throw new WebException("A HTTP Error occurred resolving the URI '" + u.AbsoluteUri + "'", webEx);
            }
        }

        /// <summary>
        /// Attempts to load a RDF dataset from the given URI into the given Triple Store
        /// </summary>
        /// <param name="store">Triple Store to load into</param>
        /// <param name="u">URI to attempt to get a RDF dataset from</param>
        /// <param name="parser">Parser to use to parse the RDF dataset</param>
        /// <remarks>
        /// <para>
        /// If the <paramref name="parser"/> parameter is set to null then this method attempts to select the relevant Store Parser based on the Content Type header returned in the HTTP Response.
        /// </para>
        /// <para>
        /// If you know ahead of time the Content Type you can explicitly pass in the parser to use.
        /// </para>
        /// </remarks>
        public static void Load(ITripleStore store, Uri u, IStoreReader parser)
        {
            if (store == null) throw new RdfParseException("Cannot read a RDF dataset into a null Triple Store");
            if (u == null) throw new RdfParseException("Cannot read a RDF dataset from a null URI");
            Load(new StoreHandler(store), u, parser);
        }

        /// <summary>
        /// Attempts to load a RDF dataset from the given URI into the given Triple Store
        /// </summary>
        /// <param name="store">Triple Store to load into</param>
        /// <param name="u">URI to attempt to get a RDF dataset from</param>
        /// <remarks>
        /// <para>
        /// Attempts to select the relevant Store Parser based on the Content Type header returned in the HTTP Response.
        /// </para>
        /// </remarks>
        public static void Load(ITripleStore store, Uri u)
        {
            Load(store, u, null);
        }

        /// <summary>
        /// Attempts to load a RDF dataset from the given URI using a RDF Handler
        /// </summary>
        /// <param name="handler">RDF Handler to use</param>
        /// <param name="u">URI to attempt to get a RDF dataset from</param>
        /// <param name="parser">Parser to use to parse the RDF dataset</param>
        /// <remarks>
        /// <para>
        /// If the <paramref name="parser"/> parameter is set to null then this method attempts to select the relevant Store Parser based on the Content Type header returned in the HTTP Response.
        /// </para>
        /// <para>
        /// If you know ahead of time the Content Type you can explicitly pass in the parser to use.
        /// </para>
        /// </remarks>
        public static void Load(IRdfHandler handler, Uri u, IStoreReader parser)
        {
            if (u == null) throw new RdfParseException("Cannot read a RDF dataset from a null URI");
            if (handler == null) throw new RdfParseException("Cannot read a RDF dataset using a null RDF handler");

            try
            {
                if (u.IsFile)
                {
                    // Invoke FileLoader instead
                    RaiseWarning("This is a file: URI so invoking the FileLoader instead");
                    if (Path.DirectorySeparatorChar == '/')
                    {
                        FileLoader.Load(handler, u.AbsoluteUri.Substring(7), parser);
                    }
                    else
                    {
                        FileLoader.Load(handler, u.AbsoluteUri.Substring(8), parser);
                    }
                    return;
                }

                // Sanitise the URI to remove any Fragment ID
                u = Tools.StripUriFragment(u);

                // Set-up the Request
                HttpWebRequest httpRequest;
                httpRequest = (HttpWebRequest)WebRequest.Create(u);

                // Want to ask for TriG, NQuads or TriX
                if (parser != null)
                {
                    // If a non-null parser set up a HTTP Header that is just for the given parser
                    httpRequest.Accept = MimeTypesHelper.CustomHttpAcceptHeader(parser);
                }
                else
                {
                    httpRequest.Accept = MimeTypesHelper.HttpRdfDatasetAcceptHeader;
                }

                // Use HTTP GET
                httpRequest.Method = "GET";
#if !NETCORE
                httpRequest.Timeout = Options.UriLoaderTimeout;
#endif
                if (_userAgent != null && !_userAgent.Equals(String.Empty))
                {
#if NETCORE
                    httpRequest.Headers[HttpRequestHeader.UserAgent] = _userAgent;
#else
                    httpRequest.UserAgent = _userAgent;
#endif
                }

                // HTTP Debugging
                Tools.HttpDebugRequest(httpRequest);

                using (HttpWebResponse httpResponse = (HttpWebResponse)httpRequest.GetResponse())
                {
                    Tools.HttpDebugResponse(httpResponse);

                    // Get a Parser and Load the RDF
                    if (parser == null)
                    {
                        try
                        {
                            parser = MimeTypesHelper.GetStoreParser(httpResponse.ContentType);
                            parser.Warning += RaiseStoreWarning;
                            parser.Load(handler, new StreamReader(httpResponse.GetResponseStream()));
                        }
                        catch (RdfParserSelectionException)
                        {
                            RaiseStoreWarning("Unable to select a RDF Dataset parser based on Content-Type: " + httpResponse.ContentType + " - seeing if the content is an RDF Graph instead");

                            try
                            {
                                // If not a RDF Dataset format see if it is a Graph
                                IRdfReader rdfParser = MimeTypesHelper.GetParser(httpResponse.ContentType);
                                rdfParser.Load(handler, new StreamReader(httpResponse.GetResponseStream()));
                            }
                            catch (RdfParserSelectionException)
                            {
                                // Finally fall back to assuming a dataset and trying format guessing
                                String data = new StreamReader(httpResponse.GetResponseStream()).ReadToEnd();
                                parser = StringParser.GetDatasetParser(data);
                                parser.Warning += RaiseStoreWarning;
                                parser.Load(handler, new StringReader(data));
                            }
                        }
                    }
                    else
                    {
                        parser.Warning += RaiseStoreWarning;
                        parser.Load(handler, new StreamReader(httpResponse.GetResponseStream()));
                    }
                }
            }
            catch (UriFormatException uriEx)
            {
                // Uri Format Invalid
                throw new RdfException("Unable to load from the given URI '" + u.AbsoluteUri + "' since it's format was invalid, see inner exception for details", uriEx);
            }
            catch (WebException webEx)
            {
                if (webEx.Response != null) Tools.HttpDebugResponse((HttpWebResponse)webEx.Response);

                // Some sort of HTTP Error occurred
                throw new WebException("A HTTP Error occurred resolving the URI '" + u.AbsoluteUri + "', see innner exception for details", webEx);
            }
        }

        /// <summary>
        /// Attempts to load a RDF dataset from the given URI using a RDF Handler
        /// </summary>
        /// <param name="handler">RDF Handler to use</param>
        /// <param name="u">URI to attempt to get a RDF dataset from</param>
        /// <remarks>
        /// <para>
        /// Attempts to select the relevant Store Parser based on the Content Type header returned in the HTTP Response.
        /// </para>
        /// </remarks>
        public static void LoadDataset(IRdfHandler handler, Uri u)
        {
            Load(handler, u, (IStoreReader)null);
        }

        #region Warning Events

        /// <summary>
        /// Raises warning messages
        /// </summary>
        /// <param name="message">Warning Message</param>
        static void RaiseWarning(String message)
        {
            RdfReaderWarning d = Warning;
            if (d != null)
            {
                d(message);
            }
        }

        /// <summary>
        /// Raises store warning messages
        /// </summary>
        /// <param name="message">Warning Message</param>
        static void RaiseStoreWarning(String message)
        {
            StoreReaderWarning d = StoreWarning;
            if (d != null)
            {
                d(message);
            }
        }

        /// <summary>
        /// Event which is raised when a parser that is invoked by the UriLoader notices a non-fatal issue with the RDF syntax
        /// </summary>
        public static event RdfReaderWarning Warning;

        /// <summary>
        /// Event which is raised when a store parser that is invoked by the UriLoader notices a non-fatal issue with the RDF dataset syntax
        /// </summary>
        public static event StoreReaderWarning StoreWarning;

        #endregion
    }
}