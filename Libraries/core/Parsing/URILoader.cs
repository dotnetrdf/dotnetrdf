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
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Storage.Params;

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
#if !NO_URICACHE
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
#endif
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

#if !SILVERLIGHT

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
        /// </remarks>
        public static void Load(IGraph g, Uri u)
        {
            UriLoader.Load(g, u, null);
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
        /// In the event that the URI is a File URI the <see cref="FileLoader">FileLoader</see> will be used instead
        /// </para>
        /// <para>
        /// If the URI is a Data URI then the <see cref="DataUriLoader">DataUriLoader</see> will be used instead.
        /// </para>
        /// </remarks>
        public static void Load(IGraph g, Uri u, IRdfReader parser)
        {
            if (g == null) throw new RdfParseException("Cannot read RDF into a null Graph");
            if (u == null) throw new RdfParseException("Cannot load RDF from a null URI");
#if SILVERLIGHT
            if (u.IsFile())
#else
            if (u.IsFile)
#endif
            {
                //Invoke FileLoader instead
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
                //Invoke DataUriLoader instead
                RaiseWarning("This is a data: URI so invoking the DataUriLoader instead");
                DataUriLoader.Load(g, u);
                return;
            }

            //Set Base Uri if necessary
            if (g.BaseUri == null && g.IsEmpty) g.BaseUri = u;

            UriLoader.Load(new GraphHandler(g), u, parser);
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
            UriLoader.Load(handler, u, (IRdfReader)null);
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
#if SILVERLIGHT
                if (u.IsFile())
#else
                if (u.IsFile)
#endif
                {
                    //Invoke FileLoader instead
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
                    //Invoke DataUriLoader instead
                    RaiseWarning("This is a data: URI so invoking the DataUriLoader instead");
                    DataUriLoader.Load(handler, u);
                    return;
                }

                //Sanitise the URI to remove any Fragment ID
                u = Tools.StripUriFragment(u);

#if !NO_URICACHE
                //Use Cache if possible
                String etag = String.Empty;
                String local = null;
                if (Options.UriLoaderCaching)
                {
                    if (_cache.HasETag(u))
                    {
                        //Get the ETag and then we'll include an If-None-Match header in our request
                        etag = _cache.GetETag(u);
                    }
                    else if (_cache.HasLocalCopy(u, true))
                    {
                        //Just try loading from the local copy
                        local = _cache.GetLocalCopy(u);
                        if (local != null)
                        {
                            try
                            {
                                FileLoader.Load(handler, local, new TurtleParser());
                            }
                            catch
                            {
                                //If we get an Exception we failed to access the file successfully
                                _cache.RemoveETag(u);
                                _cache.RemoveLocalCopy(u);
                                UriLoader.Load(handler, u, parser);
                            }
                            return;
                        }
                    }
                }
#endif

                //Set-up the Request
                HttpWebRequest httpRequest;
                httpRequest = (HttpWebRequest)WebRequest.Create(u);

                //Want to ask for RDF formats
                if (parser != null)
                {
                    //If a non-null parser set up a HTTP Header that is just for the given parser
                    httpRequest.Accept = MimeTypesHelper.CustomHttpAcceptHeader(parser);
                }
                else
                {
                    httpRequest.Accept = MimeTypesHelper.HttpAcceptHeader;
                }

#if !NO_URICACHE
                if (Options.UriLoaderCaching)
                {
                    if (!etag.Equals(String.Empty))
                    {
                        httpRequest.Headers.Add(HttpRequestHeader.IfNoneMatch, etag);
                    }
                }
#endif

                //Use HTTP GET
                httpRequest.Method = "GET";
#if !SILVERLIGHT
                httpRequest.Timeout = Options.UriLoaderTimeout;
#endif
                if (_userAgent != null && !_userAgent.Equals(String.Empty))
                {
                    httpRequest.UserAgent = _userAgent;
                }
#if DEBUG
                //HTTP Debugging
                if (Options.HttpDebugging)
                {
                    Tools.HttpDebugRequest(httpRequest);
                }
#endif

                using (HttpWebResponse httpResponse = (HttpWebResponse)httpRequest.GetResponse())
                {
#if DEBUG
                    //HTTP Debugging
                    if (Options.HttpDebugging)
                    {
                        Tools.HttpDebugResponse(httpResponse);
                    }
#endif

#if !NO_URICACHE
                    if (Options.UriLoaderCaching)
                    {
                        //Are we using ETag based caching?
                        if (!etag.Equals(String.Empty))
                        {
                            //Did we get a Not-Modified response?
                            if (httpResponse.StatusCode == HttpStatusCode.NotModified)
                            {
                                //If so then we need to load the Local Copy assuming it exists?
                                if (_cache.HasLocalCopy(u, false))
                                {
                                    local = _cache.GetLocalCopy(u);
                                    try
                                    {
                                        FileLoader.Load(handler, local, new TurtleParser());
                                    }
                                    catch
                                    {
                                        //If we get an Exception we failed to access the file successfully
                                        _cache.RemoveETag(u);
                                        _cache.RemoveLocalCopy(u);
                                        UriLoader.Load(handler, u, parser);
                                    }
                                    return;
                                }
                                else
                                {
                                    //If the local copy didn't exist then we need to redo the response without
                                    //the ETag as we've lost the cached copy somehow
                                    _cache.RemoveETag(u);
                                    UriLoader.Load(handler, u, parser);
                                    return;
                                }
                            }
                            //If we didn't get a Not-Modified response then we'll continue and parse the new response
                        }
                    }
#endif

                    //Get a Parser and Load the RDF
                    if (parser == null)
                    {
                        //Only need to auto-detect the parser if a specific one wasn't specified
                        parser = MimeTypesHelper.GetParser(httpResponse.ContentType);
                    }
                    parser.Warning += RaiseWarning;
#if !NO_URICACHE
                    //To do caching we ask the cache to give us a handler and then we tie it to
                    IRdfHandler cacheHandler = _cache.ToCache(u, Tools.StripUriFragment(httpResponse.ResponseUri), httpResponse.Headers["ETag"]);
                    if (cacheHandler != null)
                    {
                        //Note: We can ONLY use caching when we know that the Handler will accept all the data returned
                        //i.e. if the Handler may trim the data in some way then we shouldn't cache the data returned
                        if (handler.AcceptsAll)
                        {
                            handler = new MultiHandler(new IRdfHandler[] { handler, cacheHandler });
                        }
                        else
                        {
                            cacheHandler = null;
                        }
                    }
                    try
                    {
#endif
                        parser.Load(handler, new StreamReader(httpResponse.GetResponseStream()));

#if !NO_URICACHE
                    }
                    catch
                    {
                        //If we were trying to cache the response and something went wrong discard the cached copy
                        _cache.RemoveETag(u);
                        _cache.RemoveETag(Tools.StripUriFragment(httpResponse.ResponseUri));
                        _cache.RemoveLocalCopy(u);
                        _cache.RemoveLocalCopy(Tools.StripUriFragment(httpResponse.ResponseUri));
                    }
#endif
                }
            }
            catch (UriFormatException uriEx)
            {
                //Uri Format Invalid
                throw new RdfParseException("Unable to load from the given URI '" + u.ToString() + "' since it's format was invalid", uriEx);
            }
            catch (WebException webEx)
            {
#if DEBUG
                if (webEx.Response != null && Options.HttpDebugging)
                {
                    Tools.HttpDebugResponse((HttpWebResponse)webEx.Response);
                }
#endif

#if !NO_URICACHE
                if (webEx.Response != null)
                {
                    if (((HttpWebResponse)webEx.Response).StatusCode == HttpStatusCode.NotModified)
                    {
                        //If so then we need to load the Local Copy assuming it exists?
                        if (_cache.HasLocalCopy(u, false))
                        {
                            String local = _cache.GetLocalCopy(u);
                            try
                            {
                                FileLoader.Load(handler, local, new TurtleParser());
                            }
                            catch
                            {
                                //If we get an Exception we failed to access the file successfully
                                _cache.RemoveETag(u);
                                _cache.RemoveLocalCopy(u);
                                UriLoader.Load(handler, u, parser);
                            }
                            return;
                        }
                        else
                        {
                            //If the local copy didn't exist then we need to redo the response without
                            //the ETag as we've lost the cached copy somehow
                            _cache.RemoveETag(u);
                            UriLoader.Load(handler, u, parser);
                            return;
                        }
                    }
                }
#endif

                //Some sort of HTTP Error occurred
                throw new WebException("A HTTP Error occurred resolving the URI '" + u.ToString() + "'", webEx);
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
            UriLoader.Load(new StoreHandler(store), u, parser);
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
            UriLoader.Load(store, u, null);
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
#if SILVERLIGHT
                if (u.IsFile())
#else
                if (u.IsFile)
#endif

                {
                    //Invoke FileLoader instead
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

                //Sanitise the URI to remove any Fragment ID
                u = Tools.StripUriFragment(u);

                //Set-up the Request
                HttpWebRequest httpRequest;
                httpRequest = (HttpWebRequest)WebRequest.Create(u);

                //Want to ask for TriG, NQuads or TriX
                if (parser != null)
                {
                    //If a non-null parser set up a HTTP Header that is just for the given parser
                    httpRequest.Accept = MimeTypesHelper.CustomHttpAcceptHeader(parser);
                }
                else
                {
                    httpRequest.Accept = MimeTypesHelper.HttpRdfDatasetAcceptHeader;
                }

                //Use HTTP GET
                httpRequest.Method = "GET";
#if !SILVERLIGHT
                httpRequest.Timeout = Options.UriLoaderTimeout;
#endif
                if (_userAgent != null && !_userAgent.Equals(String.Empty))
                {
                    httpRequest.UserAgent = _userAgent;
                }

#if DEBUG
                //HTTP Debugging
                if (Options.HttpDebugging)
                {
                    Tools.HttpDebugRequest(httpRequest);
                }
#endif

                using (HttpWebResponse httpResponse = (HttpWebResponse)httpRequest.GetResponse())
                {
#if DEBUG
                    //HTTP Debugging
                    if (Options.HttpDebugging)
                    {
                        Tools.HttpDebugResponse(httpResponse);
                    }
#endif

                    //Get a Parser and Load the RDF
                    if (parser == null)
                    {
                        try
                        {
                            parser = MimeTypesHelper.GetStoreParser(httpResponse.ContentType);
                            parser.Warning += RaiseStoreWarning;
                            parser.Load(handler, new StreamParams(httpResponse.GetResponseStream()));
                        }
                        catch (RdfParserSelectionException selEx)
                        {
                            String data = new StreamReader(httpResponse.GetResponseStream()).ReadToEnd();
                            parser = StringParser.GetDatasetParser(data);
                            parser.Warning += RaiseStoreWarning;
                            parser.Load(handler, new TextReaderParams(new StringReader(data)));
                        }
                    }
                    else
                    {
                        parser.Warning += RaiseStoreWarning;
                        parser.Load(handler, new StreamParams(httpResponse.GetResponseStream()));
                    }
                }
            }
            catch (UriFormatException uriEx)
            {
                //Uri Format Invalid
                throw new RdfException("Unable to load from the given URI '" + u.ToString() + "' since it's format was invalid, see inner exception for details", uriEx);
            }
            catch (WebException webEx)
            {
#if DEBUG
                if (Options.HttpDebugging)
                {
                    if (webEx.Response != null) Tools.HttpDebugResponse((HttpWebResponse)webEx.Response);
                }
#endif
                //Some sort of HTTP Error occurred
                throw new WebException("A HTTP Error occurred resolving the URI '" + u.ToString() + "', see innner exception for details", webEx);
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
            UriLoader.Load(handler, u, (IStoreReader)null);
        }

#endif

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