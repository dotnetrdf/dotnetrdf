using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using VDS.RDF.Parsing.Handlers;

namespace VDS.RDF.Parsing
{
    public static partial class UriLoader
    {
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

        public static void Load(IGraph g, Uri u, GraphCallback callback, Object state)
        {
            UriLoader.Load(g, u, null, callback, state);
        }

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
                throw new RdfParseException("Unable to load from the given URI '" + u.ToString() + "' since it's format was invalid", uriEx);
            }
        }

        public static void Load(IRdfHandler handler, Uri u, RdfHandlerCallback callback, Object state)
        {
            UriLoader.Load(handler, u, (IRdfReader)null, callback, state);
        }

        public static void Load(ITripleStore store, Uri u, IStoreReader parser, TripleStoreCallback callback, Object state)
        {
            throw new NotImplementedException();
        }

        public static void Load(ITripleStore store, Uri u, TripleStoreCallback callback, Object state)
        {
            UriLoader.Load(store, u, null, callback, state);
        }

        public static void Load(IRdfHandler handler, Uri u, IStoreReader parser, RdfHandlerCallback callback, Object state)
        {
            throw new NotImplementedException();
        }

        public static void LoadDataset(IRdfHandler handler, Uri u, RdfHandlerCallback callback, Object state)
        {
            UriLoader.Load(handler, u, (IStoreReader)null, callback, state);
        }
    }
}
