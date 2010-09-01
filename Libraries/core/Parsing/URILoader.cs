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
using VDS.RDF.Storage.Params;

//SLV: Implement a version of the UriLoader that makes the requests asynchronously

namespace VDS.RDF.Parsing
{
    /// <summary>
    /// A Class for dereferencing URIs and attempting to parse the results of a HTTP GET request to the Uri into RDF
    /// </summary>
    /// <remarks>
    /// <para>
    /// As of the 0.2.2 release the loader has support for caching retrieved data locally built into it (for Graphs only), caching is done using ETags where the remote server provides them or just by a user-defineable 'freshness' criteria (i.e. number of hours retrieved resources should be cached for).  By default this caching happens in the system temporary directory which means it is non-persistent i.e. if you run your application using dotNetRDF it may cache stuff during the session but once the application is closed the operating system may freely delete the cached data.  If you wish to have a persistent cache then you can use the <see cref="UriLoader.CacheDirectory">CacheDirectory</see> property to set your own cache directory.  Even when you set your own cache directory dotNetRDF will delete obsolete data from it over time though this will only happen when a new request invalidates previously cached data.
    /// </para>
    /// </remarks>
    public static class UriLoader
    {
#if !NO_URICACHE
        private static UriLoaderCache _cache = new UriLoaderCache();

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
#endif

        /// <summary>
        /// Attempts to load a RDF Graph from the given URI into the given Graph
        /// </summary>
        /// <param name="g">Graph to assert Triples in</param>
        /// <param name="u">Uri to attempt to get RDF from</param>
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
        /// Attempts to load a RDF Graph from the given Uri into the given Graph
        /// </summary>
        /// <param name="g">Graph to assert Triples in</param>
        /// <param name="u">Uri to attempt to get RDF from</param>
        /// <param name="parser">Parser to use</param>
        /// <remarks>
        /// <para>
        /// Uses the supplied parser to attempt parsing regardless of the actual Content Type returned
        /// </para>
        /// <para>
        /// In the event that the Uri is a File Uri the <see cref="FileLoader">FileLoader</see> will be used instead
        /// </para>
        /// <para>
        /// If the URI is a Data URI then the <see cref="DataUriLoader">DataUriLoader</see> will be used instead.
        /// </para>
        /// </remarks>
        public static void Load(IGraph g, Uri u, IRdfReader parser)
        {
            try
            {
                if (u == null) throw new RdfParseException("Cannot load RDF from a null URI");
#if SILVERLIGHT
                if (u.IsFile())
#else
                if (u.IsFile)
#endif

                {
                    //Invoke FileLoader instead
                    OnWarning("This is a file: URI so invoking the FileLoader instead");
                    FileLoader.Load(g, u.ToString().Substring(8));
                    return;
                }
                if (u.Scheme.Equals("data"))
                {
                    //Invoke DataUriLoader instead
                    OnWarning("This is a data: URI so invoking the DataUriLoader instead");
                    DataUriLoader.Load(g, u);
                    return;
                }

                //Set Base Uri if necessary
                if (g.BaseUri == null) g.BaseUri = u;

                //Use Cache if possible
                String etag = String.Empty;
                String local = null;
                bool cacheable = g.IsEmpty;
#if !NO_URICACHE
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
                                FileLoader.Load(g, local, new TurtleParser());
                            }
                            catch
                            {
                                //If we get an Exception we failed to access the file successfully
                                _cache.RemoveETag(u);
                                _cache.RemoveLocalCopy(u);
                                UriLoader.Load(g, u);
                            }
                            return;
                        }
                    }
                }
#endif

                //Set-up the Request
                HttpWebRequest httpRequest;
                httpRequest = (HttpWebRequest)WebRequest.Create(u);

                //Want to ask for RDF/XML, Turtle, Notation 3 or NTriples
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
                httpRequest.Timeout = 15000;
#endif

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
                                        FileLoader.Load(g, local, new TurtleParser());
                                    }
                                    catch
                                    {
                                        //If we get an Exception we failed to access the file successfully
                                        _cache.RemoveETag(u);
                                        _cache.RemoveLocalCopy(u);
                                        UriLoader.Load(g, u);
                                    }
                                    return;
                                }
                                else
                                {
                                    //If the local copy didn't exist then we need to redo the response without
                                    //the ETag as we've lost the cached copy somehow
                                    _cache.RemoveETag(u);
                                    UriLoader.Load(g, u);
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
                    parser.Warning += OnWarning;
                    parser.Load(g, new StreamReader(httpResponse.GetResponseStream()));

#if !NO_URICACHE
                    if (Options.UriLoaderCaching && cacheable)
                    {
                        //Cache the response only if caching is enabled and the Graph we parsed into was empty to start with
                        //If it was non-empty then we can't cache as this Graph doesn't just represent the URI we just retrieved
                        _cache.ToCache(u, g, httpResponse.Headers["ETag"]);
                    }
#endif
                }
            }
            catch (UriFormatException uriEx)
            {
                //Uri Format Invalid
                throw new RdfException("Unable to load from the given URI '" + u.ToString() + "' since it's format was invalid", uriEx);
            }
            catch (WebException webEx)
            {
                if (webEx.Response != null && Options.HttpDebugging)
                {
                    Tools.HttpDebugResponse((HttpWebResponse)webEx.Response);
                }

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
                                FileLoader.Load(g, local, new TurtleParser());
                            }
                            catch
                            {
                                //If we get an Exception we failed to access the file successfully
                                _cache.RemoveETag(u);
                                _cache.RemoveLocalCopy(u);
                                UriLoader.Load(g, u);
                            }
                            return;
                        }
                        else
                        {
                            //If the local copy didn't exist then we need to redo the response without
                            //the ETag as we've lost the cached copy somehow
                            _cache.RemoveETag(u);
                            UriLoader.Load(g, u);
                            return;
                        }
                    }
                }
#endif

                //Some sort of HTTP Error occurred
                throw new WebException("A HTTP Error occurred resolving the URI '" + u.ToString() + "'", webEx);
            }
            catch (RdfException)
            {
                //Some problem with the RDF or Parsing thereof
                throw;
            }
            catch (Exception)
            {
                //Other Exception
                throw;
            }
        }

        /// <summary>
        /// Attempts to load a RDF dataset from the given Uri into the given Triple Store
        /// </summary>
        /// <param name="store">Triple Store to load into</param>
        /// <param name="u">Uri to attempt to get a RDF dataset from</param>
        /// <remarks>
        /// <para>
        /// Attempts to select the relevant Store Parser based on the Content Type header returned in the HTTP Response.
        /// </para>
        /// <para>
        /// If you know ahead of time the Content Type you can just open a HTTP Stream yourself and pass it to an instance of the correct Store Parser.
        /// </para>
        /// </remarks>
        public static void Load(ITripleStore store, Uri u)
        {
            try
            {
                //Set-up the Request
                HttpWebRequest httpRequest;
                httpRequest = (HttpWebRequest)WebRequest.Create(u);

                //Want to ask for TriG, NQuads or TriX
                httpRequest.Accept = MimeTypesHelper.HttpRdfDatasetAcceptHeader;

                //Use HTTP GET
                httpRequest.Method = "GET";
#if !SILVERLIGHT
                httpRequest.Timeout = 15000;
#endif

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
                    IStoreReader parser = MimeTypesHelper.GetStoreParser(httpResponse.ContentType);
                    parser.Warning += OnStoreWarning;
                    parser.Load(store, new StreamParams(httpResponse.GetResponseStream()));
                }
            }
            catch (UriFormatException uriEx)
            {
                //Uri Format Invalid
                throw new RdfException("Unable to load from the given URI '" + u.ToString() + "' since it's format was invalid", uriEx);
            }
            catch (WebException webEx)
            {
                //Some sort of HTTP Error occurred
                throw new WebException("A HTTP Error occurred resolving the URI '" + u.ToString() + "'", webEx);
            }
            catch (RdfException)
            {
                //Some problem with the RDF or Parsing thereof
                throw;
            }
            catch (Exception)
            {
                //Other Exception
                throw;
            }
        }

        /// <summary>
        /// Raises warning messages
        /// </summary>
        /// <param name="message">Warning Message</param>
        static void OnWarning(String message)
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
        static void OnStoreWarning(String message)
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
    }

    /// <summary>
    /// A Class for parsing RDF data from Data URIs
    /// </summary>
    /// <remarks>
    /// <para>
    /// Data URIs use the data: scheme and are defined by the IETF in <a href="http://tools.ietf.org/html/rfc2397">RFC 2397</a> and provide a means to embed data directly in a URI either in Base64 or ASCII encoded format.  This class can extract the data from such URIs and attempt to parse it as RDF using the <see cref="StringParser">StringParser</see>
    /// </para>
    /// <para>
    /// The parsing process for data: URIs involves first extracting and decoding the data embedded in the URI - this may either be in Base64 or ASCII encoding - and then using the <see cref="StringParser">StringParser</see> to actually parse the data string.  If the data: URI defines a MIME type then a parser is selected (if one exists for the given MIME type) and that is used to parse the data, in the event that no MIME type is given or the one given does not have a corresponding parser then the <see cref="StringParser">StringParser</see> will use its basic heuristics to attempt to auto-detect the format and select an appropriate parser.
    /// </para>
    /// <para>
    /// If you attempt to use this loader for non data: URIs then the standard <see cref="UriLoader">UriLoader</see> is used instead.
    /// </para>
    /// </remarks>
    public static class DataUriLoader
    {
        /// <summary>
        /// Loads RDF data into a Graph from a data: URI
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="u">URI to load from</param>
        /// <remarks>
        /// Invokes the normal <see cref="UriLoader">UriLoader</see> instead if a the URI provided is not a data: URI
        /// </remarks>
        /// <exception cref="UriFormatException">Thrown if the metadata portion of the URI which indicates the MIME Type, Character Set and whether Base64 encoding is used is malformed</exception>
        public static void Load(IGraph g, Uri u)
        {
            if (u == null) throw new RdfParseException("Cannot load RDF from a null URI");
            if (!u.Scheme.Equals("data"))
            {
                //Invoke the normal URI Loader if not a data: URI
                UriLoader.Load(g, u);
                return;
            }

            String mimetype = "text/plain";
            bool mimeSet = false;
            bool base64 = false;
            String[] uri = u.AbsolutePath.Split(',');
            String metadata = uri[0];
            String data = uri[1];

            //Process the metadata
            if (metadata.Equals(String.Empty))
            {
                //Nothing to do
            }
            else if (metadata.Contains(';'))
            {
                if (metadata.StartsWith(";")) metadata = metadata.Substring(1);
                String[] meta = metadata.Split(';');
                for (int i = 0; i < meta.Length; i++)
                {
                    if (meta[i].StartsWith("charset="))
                    {
                        //OPT: Do we need to process the charset parameter here at all?
                        //String charset = meta[i].Substring(meta[i].IndexOf('=') + 1);
                    }
                    else if (meta[i].Equals("base64"))
                    {
                        base64 = true;
                    }
                    else if (meta[i].Contains('/'))
                    {
                        mimetype = meta[i];
                        mimeSet = true;
                    }
                    else
                    {
                        throw new UriFormatException("This data: URI appears to be malformed as encountered the parameter value '" + meta[i] + "' in the metadata section of the URI");
                    }
                }
            }
            else
            {
                if (metadata.StartsWith("charset="))
                {
                    //OPT: Do we need to process the charset parameter here at all?
                }
                else if (metadata.Equals(";base64"))
                {
                    base64 = true;
                }
                else if (metadata.Contains('/'))
                {
                    mimetype = metadata;
                    mimeSet = true;
                }
                else
                {
                    throw new UriFormatException("This data: URI appears to be malformed as encountered the parameter value '" + metadata + "' in the metadata section of the URI");
                }
            }

            //Process the data
            if (base64)
            {
                StringWriter temp = new StringWriter();
                foreach (byte b in Convert.FromBase64String(data))
                {
                    temp.Write((char)((int)b));
                }
                data = temp.ToString();
            }
            else
            {
                data = Uri.UnescapeDataString(data);
            }

            //Now either select a parser based on the MIME type or let StringParser guess the format
            try
            {
                if (mimeSet)
                {
                    //If the MIME Type was explicitly set then we'll try and get a parser and use it
                    IRdfReader reader = MimeTypesHelper.GetParser(mimetype);
                    StringParser.Parse(g, data, reader);
                }
                else
                {
                    //If the MIME Type was not explicitly set we'll let the StringParser guess the format
                    StringParser.Parse(g, data);
                }
            }
            catch (RdfParserSelectionException)
            {
                //If we fail to get a parser then we'll let the StringParser guess the format
                StringParser.Parse(g, data);
            }
        }
    }
}
