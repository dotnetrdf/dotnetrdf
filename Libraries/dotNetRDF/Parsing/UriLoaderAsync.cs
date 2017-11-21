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
    public static partial class UriLoader
    {
        /// <summary>
        /// Attempts to load a RDF Graph from a URI asynchronously
        /// </summary>
        /// <param name="g">Graph to assert triple in</param>
        /// <param name="u">URI to load from</param>
        /// <param name="parser">Parser to use</param>
        /// <param name="callback">Callback to invoke when the operation completes</param>
        /// <param name="state">State to pass to the callback</param>
        /// <remarks>
        /// <para>
        /// Uses the supplied parser to attempt parsing regardless of the actual Content Type returned
        /// </para>
        /// <para>
        /// In the event that the URI is a File URI the <see cref="FileLoader">FileLoader</see> will be used instead. If the URI is a Data URI then the <see cref="DataUriLoader">DataUriLoader</see> will be used instead.
        /// </para>
        /// <para>
        /// <strong>Note:</strong> UriLoader will assign the Graph the source URI as it's Base URI unless the Graph already has a Base URI or is non-empty prior to attempting parsing.  Note that any Base URI specified in the RDF contained in the file will override this initial Base URI.  In some cases this may lead to invalid RDF being accepted and generating strange relative URIs, if you encounter this either set a Base URI prior to calling this method or create an instance of the relevant parser and invoke it directly.
        /// </para>
        /// <para>
        /// If the loading completes normally the callback will be invoked normally, if an error occurs it will be invoked and passed an instance of <see cref="AsyncError"/> as the state which contains details of the error and the original state.
        /// </para>
        /// </remarks>
        public static void Load(IGraph g, Uri u, IRdfReader parser, GraphCallback callback, Object state)
        {
            if (g == null) throw new RdfParseException("Cannot read RDF into a null Graph");
            if (u == null) throw new RdfParseException("Cannot read RDF from a null URI");

            if (u.IsFile)
            {
                // Invoke FileLoader instead
                RaiseWarning("This is a file: URI so invoking the FileLoader instead");
                if (Path.DirectorySeparatorChar == '/')
                {
                    FileLoader.Load(g, u.AbsoluteUri.Substring(7), parser);
                }
                else
                {
                    FileLoader.Load(g, u.AbsoluteUri.Substring(8), parser);
                }
                // FileLoader.Load() will run synchronously so once this completes we can invoke the callback
                callback(g, state);
                return;
            }
            if (u.Scheme.Equals("data"))
            {
                // Invoke DataUriLoader instead
                RaiseWarning("This is a data: URI so invoking the DataUriLoader instead");
                DataUriLoader.Load(g, u);
                // After DataUriLoader.Load() has run (which happens synchronously) can invoke the callback
                callback(g, state);
                return;
            }

            // Set Base URI if necessary
            if (g.BaseUri == null && g.IsEmpty) g.BaseUri = u;

            Load(new GraphHandler(g), u, parser, (_, s) => callback(g, s), state);
        }

        /// <summary>
        /// Attempts to load a RDF Graph from a URI asynchronously
        /// </summary>
        /// <param name="g">Graph to assert triple in</param>
        /// <param name="u">URI to load from</param>
        /// <param name="callback">Callback to invoke when the operation completes</param>
        /// <param name="state">State to pass to the callback</param>
        /// <remarks>
        /// <para>
        /// Will attempt to autodetect the format of the RDF based on the Content-Type header of the HTTP response
        /// </para>
        /// <para>
        /// In the event that the URI is a File URI the <see cref="FileLoader">FileLoader</see> will be used instead. If the URI is a Data URI then the <see cref="DataUriLoader">DataUriLoader</see> will be used instead.
        /// </para>
        /// <para>
        /// <strong>Note:</strong> UriLoader will assign the Graph the source URI as it's Base URI unless the Graph already has a Base URI or is non-empty prior to attempting parsing.  Note that any Base URI specified in the RDF contained in the file will override this initial Base URI.  In some cases this may lead to invalid RDF being accepted and generating strange relative URIs, if you encounter this either set a Base URI prior to calling this method or create an instance of the relevant parser and invoke it directly.
        /// </para>
        /// <para>
        /// If the loading completes normally the callback will be invoked normally, if an error occurs it will be invoked and passed an instance of <see cref="AsyncError"/> as the state which contains details of the error and the original state.
        /// </para>
        /// </remarks>
        public static void Load(IGraph g, Uri u, GraphCallback callback, Object state)
        {
            Load(g, u, null, callback, state);
        }

        /// <summary>
        /// Attempts to load a RDF Graph from a URI asynchronously using an RDF Handler
        /// </summary>
        /// <param name="handler">RDF Handler to use</param>
        /// <param name="u">URI to load from</param>
        /// <param name="parser">Parser to use</param>
        /// <param name="callback">Callback to invoke when the operation completes</param>
        /// <param name="state">State to pass to the callback</param>
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
        /// <para>
        /// If the loading completes normally the callback will be invoked normally, if an error occurs it will be invoked and passed an instance of <see cref="AsyncError"/> as the state which contains details of the error and the original state.
        /// </para>
        /// </remarks>
        public static void Load(IRdfHandler handler, Uri u, IRdfReader parser, RdfHandlerCallback callback, Object state)
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
                        FileLoader.Load(handler, u.AbsoluteUri.Substring(7), parser);
                    }
                    else
                    {
                        FileLoader.Load(handler, u.AbsoluteUri.Substring(8), parser);
                    }
                    // FileLoader.Load() will run synchronously so once this completes we can invoke the callback
                    callback(handler, state);
                    return;
                }
                if (u.Scheme.Equals("data"))
                {
                    // Invoke DataUriLoader instead
                    RaiseWarning("This is a data: URI so invoking the DataUriLoader instead");
                    DataUriLoader.Load(handler, u);
                    // After DataUriLoader.Load() has run (which happens synchronously) can invoke the callback
                    callback(handler, state);
                    return;
                }

                // Sanitise the URI to remove any Fragment ID
                u = Tools.StripUriFragment(u);

                // TODO: Add use of Cache into here, this is tricky because this code is primarily intended for Silverlight where we disable the cache purposefully

                // Setup the Request
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(u);

                // Want to ask for RDF formats
                if (parser != null)
                {
                    // If a non-null parser set up a HTTP Header that is just for the given parser
                    request.Accept = MimeTypesHelper.CustomHttpAcceptHeader(parser);
                }
                else
                {
                    request.Accept = MimeTypesHelper.HttpAcceptHeader;
                }

                // Use HTTP GET
                request.Method = "GET";
#if !NETCORE
                request.Timeout = Options.UriLoaderTimeout;
#endif
                if (_userAgent != null && !_userAgent.Equals(String.Empty))
                {
#if NETCORE
                    request.Headers[HttpRequestHeader.UserAgent] = _userAgent;
#else
                    request.UserAgent = _userAgent;
#endif
                }

                Tools.HttpDebugRequest(request);

                try
                {
                    request.BeginGetResponse(result =>
                        {
                            try
                            {
                                using (HttpWebResponse response = (HttpWebResponse) request.EndGetResponse(result))
                                {
                                    Tools.HttpDebugResponse(response);

                                    // Get a Parser and load the RDF
                                    if (parser == null)
                                    {
                                        // Only need to auto-detect the parser if a specific one wasn't specified
                                        parser = MimeTypesHelper.GetParser(response.ContentType);
                                    }
                                    parser.Warning += RaiseWarning;

                                    parser.Load(handler, new StreamReader(response.GetResponseStream()));

                                    // Finally can invoke the callback
                                    callback(handler, state);
                                }
                            }
                            catch (WebException webEx)
                            {
                                if (webEx.Response != null) Tools.HttpDebugResponse((HttpWebResponse)webEx.Response);
                                callback(handler, new AsyncError(new RdfParseException("A HTTP Error occurred loading the URI '" + u.AbsoluteUri + "' asynchronously, see inner exeption for details", webEx), state));
                            }
                            catch (Exception ex)
                            {
                                callback(handler, new AsyncError(new RdfParseException("Unexpected error while loading the URI '" + u.AbsoluteUri + "' asynchronously, see inner exception for details", ex), state));
                            }
                        }, null);
                }
                catch (WebException webEx)
                {
                    if (webEx.Response != null) Tools.HttpDebugResponse((HttpWebResponse) webEx.Response);
                    callback(handler, new AsyncError(new RdfParseException("A HTTP Error occurred loading the URI '" + u.AbsoluteUri + "' asynchronously, see inner exeption for details", webEx), state));
                }
                catch (Exception ex)
                {
                    callback(handler, new AsyncError(new RdfParseException("Unexpected error while loading the URI '" + u.AbsoluteUri + "' asynchronously, see inner exception for details", ex), state));
                }
            }
            catch (UriFormatException uriEx)
            {
                // URI Format Invalid
                throw new RdfParseException("Unable to load from the given URI '" + u.AbsoluteUri + "' since it's format was invalid, see inner exception for details", uriEx);
            }
        }

        /// <summary>
        /// Attempts to load a RDF Graph from a URI asynchronously using an RDF Handler
        /// </summary>
        /// <param name="handler">RDF Handler to use</param>
        /// <param name="u">URI to load from</param>
        /// <param name="callback">Callback to invoke when the operation completes</param>
        /// <param name="state">State to pass to the callback</param>
        /// <remarks>
        /// <para>
        /// Attempts to autodetect the RDF format based on the Content-Type header of the HTTP response
        /// </para>
        /// <para>
        /// If the loading completes normally the callback will be invoked normally, if an error occurs it will be invoked and passed an instance of <see cref="AsyncError"/> as the state which contains details of the error and the original state.
        /// </para>
        /// </remarks>
        public static void Load(IRdfHandler handler, Uri u, RdfHandlerCallback callback, Object state)
        {
            Load(handler, u, (IRdfReader)null, callback, state);
        }

        /// <summary>
        /// Attempts to load a RDF dataset asynchronously from the given URI into the given Triple Store
        /// </summary>
        /// <param name="store">Triple Store to load into</param>
        /// <param name="u">URI to attempt to get a RDF dataset from</param>
        /// <param name="parser">Parser to use to parse the RDF dataset</param>
        /// <param name="callback">Callback to invoke when the operation completes</param>
        /// <param name="state">State to pass to the callback</param>
        /// <remarks>
        /// <para>
        /// If the <paramref name="parser"/> parameter is set to null then this method attempts to select the relevant Store Parser based on the Content Type header returned in the HTTP Response.
        /// </para>
        /// <para>
        /// If you know ahead of time the Content Type you can explicitly pass in the parser to use.
        /// </para>
        /// <para>
        /// If the loading completes normally the callback will be invoked normally, if an error occurs it will be invoked and passed an instance of <see cref="AsyncError"/> as the state which contains details of the error and the original state.
        /// </para>
        /// </remarks>
        public static void Load(ITripleStore store, Uri u, IStoreReader parser, TripleStoreCallback callback, Object state)
        {
            if (store == null) throw new RdfParseException("Cannot read a RDF dataset into a null Triple Store");
            if (u == null) throw new RdfParseException("Cannot read a RDF dataset from a null URI");
            Load(new StoreHandler(store), u, parser, (_, s) => callback(store, s), state);
        }

        /// <summary>
        /// Attempts to load a RDF dataset asynchronously from the given URI into the given Triple Store
        /// </summary>
        /// <param name="store">Triple Store to load into</param>
        /// <param name="u">URI to attempt to get a RDF dataset from</param>
        /// <param name="callback">Callback to invoke when the operation completes</param>
        /// <param name="state">State to pass to the callback</param>
        /// <remarks>
        /// <para>
        /// Attempts to select the relevant Store Parser based on the Content Type header returned in the HTTP Response.
        /// </para>
        /// <para>
        /// If the loading completes normally the callback will be invoked normally, if an error occurs it will be invoked and passed an instance of <see cref="AsyncError"/> as the state which contains details of the error and the original state.
        /// </para>
        /// </remarks>
        public static void Load(ITripleStore store, Uri u, TripleStoreCallback callback, Object state)
        {
            Load(store, u, null, callback, state);
        }

        /// <summary>
        /// Attempts to load a RDF dataset asynchronously from the given URI using a RDF Handler
        /// </summary>
        /// <param name="handler">RDF Handler to use</param>
        /// <param name="u">URI to attempt to get a RDF dataset from</param>
        /// <param name="parser">Parser to use to parse the RDF dataset</param>
        /// <param name="callback">Callback to invoke when the operation completes</param>
        /// <param name="state">State to pass to the callback</param>
        /// <remarks>
        /// <para>
        /// If the <paramref name="parser"/> parameter is set to null then this method attempts to select the relevant Store Parser based on the Content Type header returned in the HTTP Response.
        /// </para>
        /// <para>
        /// If you know ahead of time the Content Type you can explicitly pass in the parser to use.
        /// </para>
        /// <para>
        /// If the loading completes normally the callback will be invoked normally, if an error occurs it will be invoked and passed an instance of <see cref="AsyncError"/> as the state which contains details of the error and the original state.
        /// </para>
        /// </remarks>
        public static void Load(IRdfHandler handler, Uri u, IStoreReader parser, RdfHandlerCallback callback, Object state)
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
                    // FileLoader.Load() will run synchronously so once this completes we can invoke the callback
                    callback(handler, state);
                    return;
                }
                if (u.Scheme.Equals("data"))
                {
                    // Invoke DataUriLoader instead
                    RaiseWarning("This is a data: URI so invoking the DataUriLoader instead");
                    DataUriLoader.Load(handler, u);
                    // After DataUriLoader.Load() has run (which happens synchronously) can invoke the callback
                    callback(handler, state);
                    return;
                }

                // Sanitise the URI to remove any Fragment ID
                u = Tools.StripUriFragment(u);

                // Setup the Request
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(u);

                // Want to ask for RDF dataset formats
                if (parser != null)
                {
                    // If a non-null parser set up a HTTP Header that is just for the given parser
                    request.Accept = MimeTypesHelper.CustomHttpAcceptHeader(parser);
                }
                else
                {
                    request.Accept = MimeTypesHelper.HttpAcceptHeader;
                }

                // Use HTTP GET
                request.Method = "GET";
#if !NETCORE
                request.Timeout = Options.UriLoaderTimeout;
#endif
                if (_userAgent != null && !_userAgent.Equals(String.Empty))
                {
#if NETCORE
                    request.Headers[HttpRequestHeader.UserAgent] = _userAgent;
#else
                    request.UserAgent = _userAgent;
#endif
                }

                Tools.HttpDebugRequest(request);

                try
                {
                    request.BeginGetResponse(result =>
                        {
                            try
                            {
                                using (HttpWebResponse response = (HttpWebResponse) request.EndGetResponse(result))
                                {
                                    Tools.HttpDebugResponse(response);

                                    // Get a Parser and load the RDF
                                    if (parser == null)
                                    {
                                        try
                                        {
                                            // Only need to auto-detect the parser if a specific one wasn't specified
                                            parser = MimeTypesHelper.GetStoreParser(response.ContentType);
                                            parser.Warning += RaiseWarning;
                                            parser.Load(handler, new StreamReader(response.GetResponseStream()));
                                        }
                                        catch (RdfParserSelectionException)
                                        {
                                            RaiseStoreWarning("Unable to select a RDF Dataset parser based on Content-Type: " + response.ContentType + " - seeing if the content is an RDF Graph instead");

                                            try
                                            {
                                                // If not a RDF Dataset format see if it is a Graph
                                                IRdfReader rdfParser = MimeTypesHelper.GetParser(response.ContentType);
                                                rdfParser.Load(handler, new StreamReader(response.GetResponseStream()));
                                            }
                                            catch (RdfParserSelectionException)
                                            {
                                                String data = new StreamReader(response.GetResponseStream()).ReadToEnd();
                                                parser = StringParser.GetDatasetParser(data);
                                                parser.Warning += RaiseStoreWarning;
                                                parser.Load(handler, new StringReader(data));
                                            }
                                        }
                                    }
                                    else
                                    {
                                        parser.Warning += RaiseStoreWarning;
                                        parser.Load(handler, new StreamReader(response.GetResponseStream()));
                                    }

                                    // Finally can invoke the callback
                                    callback(handler, state);
                                }
                            }
                            catch (WebException webEx)
                            {
                                if (webEx.Response != null) Tools.HttpDebugResponse((HttpWebResponse)webEx.Response);
                                callback(handler, new AsyncError(new RdfParseException("A HTTP Error occurred loading the URI '" + u.AbsoluteUri + "' asynchronously, see inner exeption for details", webEx), state));
                            }
                            catch (Exception ex)
                            {
                                callback(handler, new AsyncError(new RdfParseException("Unexpected error while loading the URI '" + u.AbsoluteUri + "' asynchronously, see inner exception for details", ex), state));
                            }
                        }, null);
                }
                catch (WebException webEx)
                {
                    if (webEx.Response != null) Tools.HttpDebugResponse((HttpWebResponse)webEx.Response);
                    callback(handler, new AsyncError(new RdfParseException("A HTTP Error occurred loading the URI '" + u.AbsoluteUri + "' asynchronously, see inner exeption for details", webEx), state));
                }
                catch (Exception ex)
                {
                    callback(handler, new AsyncError(new RdfParseException("Unexpected error while loading the URI '" + u.AbsoluteUri + "' asynchronously, see inner exception for details", ex), state));
                }
            }
            catch (UriFormatException uriEx)
            {
                // Uri Format Invalid
                throw new RdfException("Unable to load from the given URI '" + u.AbsoluteUri + "' since it's format was invalid, see inner exception for details", uriEx);
            }
        }

        /// <summary>
        /// Attempts to load a RDF dataset asynchronously from the given URI using a RDF Handler
        /// </summary>
        /// <param name="handler">RDF Handler to use</param>
        /// <param name="u">URI to attempt to get a RDF dataset from</param>
        /// <param name="callback">Callback to invoke when the operation completes</param>
        /// <param name="state">State to pass to the callback</param>
        /// <remarks>
        /// <para>
        /// Attempts to select the relevant Store Parser based on the Content Type header returned in the HTTP Response.
        /// </para>
        /// <para>
        /// If the loading completes normally the callback will be invoked normally, if an error occurs it will be invoked and passed an instance of <see cref="AsyncError"/> as the state which contains details of the error and the original state.
        /// </para>
        /// </remarks>
        public static void LoadDataset(IRdfHandler handler, Uri u, RdfHandlerCallback callback, Object state)
        {
            Load(handler, u, (IStoreReader)null, callback, state);
        }
    }
}
