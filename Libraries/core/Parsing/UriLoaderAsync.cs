/*

Copyright Robert Vesse 2009-11
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
using System.Net;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Storage.Params;

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
        /// In the event that the URI is a File URI the <see cref="FileLoader">FileLoader</see> will be used instead
        /// </para>
        /// <para>
        /// If the URI is a Data URI then the <see cref="DataUriLoader">DataUriLoader</see> will be used instead.
        /// </para>
        /// </remarks>
        public static void Load(IGraph g, Uri u, IRdfReader parser, GraphCallback callback, Object state)
        {
            if (g == null) throw new RdfParseException("Cannot read RDF into a null Graph");
            if (u == null) throw new RdfParseException("Cannot read RDF from a null URI");

#if SILVERLIGHT
            if (u.IsFile())
#else
            if (u.IsFile)
#endif
            {
                //Invoke FileLoader instead
                UriLoader.RaiseWarning("This is a file: URI so invoking the FileLoader instead");
                if (Path.DirectorySeparatorChar == '/')
                {
                    FileLoader.Load(g, u.ToString().Substring(7), parser);
                }
                else
                {
                    FileLoader.Load(g, u.ToString().Substring(8), parser);
                }
                //FileLoader.Load() will run synchronously so once this completes we can invoke the callback
                callback(g, state);
                return;
            }
            if (u.Scheme.Equals("data"))
            {
                //Invoke DataUriLoader instead
                RaiseWarning("This is a data: URI so invoking the DataUriLoader instead");
                DataUriLoader.Load(g, u);
                //After DataUriLoader.Load() has run (which happens synchronously) can invoke the callback
                callback(g, state);
                return;
            }

            //Set Base URI if necessary
            if (g.BaseUri == null && g.IsEmpty) g.BaseUri = u;

            UriLoader.Load(new GraphHandler(g), u, parser, (_,s) => callback(g, s), state);
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
        /// </remarks>
        public static void Load(IGraph g, Uri u, GraphCallback callback, Object state)
        {
            UriLoader.Load(g, u, null, callback, state);
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
        /// </remarks>
        public static void Load(IRdfHandler handler, Uri u, IRdfReader parser, RdfHandlerCallback callback, Object state)
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
                    //FileLoader.Load() will run synchronously so once this completes we can invoke the callback
                    callback(handler, state);
                    return;
                }
                if (u.Scheme.Equals("data"))
                {
                    //Invoke DataUriLoader instead
                    RaiseWarning("This is a data: URI so invoking the DataUriLoader instead");
                    DataUriLoader.Load(handler, u);
                    //After DataUriLoader.Load() has run (which happens synchronously) can invoke the callback
                    callback(handler, state);
                    return;
                }

                //Sanitise the URI to remove any Fragment ID
                u = Tools.StripUriFragment(u);

                //TODO: Add use of Cache into here, this is tricky because this code is primarily intended for Silverlight where we disable the cache purposefully

                //Setup the Request
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(u);

                //Want to ask for RDF formats
                if (parser != null)
                {
                    //If a non-null parser set up a HTTP Header that is just for the given parser
                    request.Accept = MimeTypesHelper.CustomHttpAcceptHeader(parser);
                }
                else
                {
                    request.Accept = MimeTypesHelper.HttpAcceptHeader;
                }

                //Use HTTP GET
                request.Method = "GET";
#if !SILVERLIGHT
                request.Timeout = Options.UriLoaderTimeout;
#endif
                if (_userAgent != null && !_userAgent.Equals(String.Empty))
                {
                    request.UserAgent = _userAgent;
                }

#if DEBUG
                //HTTP Debugging
                if (Options.HttpDebugging)
                {
                    Tools.HttpDebugRequest(request);
                }
#endif

                request.BeginGetResponse(result =>
                    {
                        using (HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(result))
                        {
#if DEBUG
                            if (Options.HttpDebugging)
                            {
                                Tools.HttpDebugResponse(response);
                            }
#endif

                            //Get a Parser and load the RDF
                            if (parser == null)
                            {
                                //Only need to auto-detect the parser if a specific one wasn't specified
                                parser = MimeTypesHelper.GetParser(response.ContentType);
                            }
                            parser.Warning += RaiseWarning;

                            parser.Load(handler, new StreamReader(response.GetResponseStream()));

                            //Finally can invoke the callback
                            callback(handler, state);
                        }
                    }, null);
            }
            catch (UriFormatException uriEx)
            {
                //URI Format Invalid
                throw new RdfParseException("Unable to load from the given URI '" + u.ToString() + "' since it's format was invalid, see inner exception for details", uriEx);
            }
            catch (WebException webEx)
            {
#if DEBUG
                if (Options.HttpDebugging)
                {
                    if (webEx.Response != null) Tools.HttpDebugResponse((HttpWebResponse)webEx.Response);
                }
#endif

                throw new WebException("A HTTP Error occurrred resolving the URI '" + u.ToString() + "', see inner exception for details", webEx);
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
        /// </remarks>
        public static void Load(IRdfHandler handler, Uri u, RdfHandlerCallback callback, Object state)
        {
            UriLoader.Load(handler, u, (IRdfReader)null, callback, state);
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
        /// </remarks>
        public static void Load(ITripleStore store, Uri u, IStoreReader parser, TripleStoreCallback callback, Object state)
        {
            if (store == null) throw new RdfParseException("Cannot read a RDF dataset into a null Triple Store");
            if (u == null) throw new RdfParseException("Cannot read a RDF dataset from a null URI");
            UriLoader.Load(new StoreHandler(store), u, parser, (_, s) => callback(store, s), state);
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
        /// </remarks>
        public static void Load(ITripleStore store, Uri u, TripleStoreCallback callback, Object state)
        {
            UriLoader.Load(store, u, null, callback, state);
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
        /// </remarks>
        public static void Load(IRdfHandler handler, Uri u, IStoreReader parser, RdfHandlerCallback callback, Object state)
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
                    //FileLoader.Load() will run synchronously so once this completes we can invoke the callback
                    callback(handler, state);
                    return;
                }
                if (u.Scheme.Equals("data"))
                {
                    //Invoke DataUriLoader instead
                    RaiseWarning("This is a data: URI so invoking the DataUriLoader instead");
                    DataUriLoader.Load(handler, u);
                    //After DataUriLoader.Load() has run (which happens synchronously) can invoke the callback
                    callback(handler, state);
                    return;
                }

                //Sanitise the URI to remove any Fragment ID
                u = Tools.StripUriFragment(u);

                //Setup the Request
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(u);

                //Want to ask for RDF dataset formats
                if (parser != null)
                {
                    //If a non-null parser set up a HTTP Header that is just for the given parser
                    request.Accept = MimeTypesHelper.CustomHttpAcceptHeader(parser);
                }
                else
                {
                    request.Accept = MimeTypesHelper.HttpAcceptHeader;
                }

                //Use HTTP GET
                request.Method = "GET";
#if !SILVERLIGHT
                request.Timeout = Options.UriLoaderTimeout;
#endif
                if (_userAgent != null && !_userAgent.Equals(String.Empty))
                {
                    request.UserAgent = _userAgent;
                }

#if DEBUG
                //HTTP Debugging
                if (Options.HttpDebugging)
                {
                    Tools.HttpDebugRequest(request);
                }
#endif

                request.BeginGetResponse(result =>
                {
                    using (HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(result))
                    {
#if DEBUG
                        if (Options.HttpDebugging)
                        {
                            Tools.HttpDebugResponse(response);
                        }
#endif

                        //Get a Parser and load the RDF
                        if (parser == null)
                        {
                            try
                            {
                                //Only need to auto-detect the parser if a specific one wasn't specified
                                parser = MimeTypesHelper.GetStoreParser(response.ContentType);
                                parser.Warning += RaiseWarning;
                                parser.Load(handler, new StreamParams(response.GetResponseStream()));
                            }
                            catch (RdfParserSelectionException selEx)
                            {
                                String data = new StreamReader(response.GetResponseStream()).ReadToEnd();
                                parser = StringParser.GetDatasetParser(data);
                                parser.Warning += RaiseStoreWarning;
                                parser.Load(handler, new TextReaderParams(new StringReader(data)));
                            }
                        }
                        else
                        {
                            parser.Warning += RaiseStoreWarning;
                            parser.Load(handler, new StreamParams(response.GetResponseStream()));
                        }

                        //Finally can invoke the callback
                        callback(handler, state);
                    }
                }, null);
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
        /// </remarks>
        public static void LoadDataset(IRdfHandler handler, Uri u, RdfHandlerCallback callback, Object state)
        {
            UriLoader.Load(handler, u, (IStoreReader)null, callback, state);
        }
    }
}
