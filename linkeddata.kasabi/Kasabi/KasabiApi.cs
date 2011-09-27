using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query;

namespace VDS.RDF.LinkedData.Kasabi
{
    public abstract class KasabiApi
        : BaseEndpoint
    {
        public const String KasabiBaseApiUri = "http://api.kasabi.com/dataset/";

        public const String KasabiParamOutputFormat = "output";
        public const String KasabiParamApiKey = "apikey";

        private String _authKey;
        private String _datasetID, _apiName;
        private bool _requireKasabiAuth = true;

        public KasabiApi(Uri customUri, String datasetID, String apiName, String authKey, bool requireAuth)
            : base(customUri)
        {
            this._datasetID = datasetID;
            this._apiName = apiName;
            this._authKey = authKey;
            this._requireKasabiAuth = requireAuth;
        }

        public KasabiApi(String datasetID, String apiName, String authKey, bool requireAuth)
            : base(new Uri(KasabiBaseApiUri + datasetID + "/apis/" + apiName))
        {
            if (authKey == null && requireAuth) throw new ArgumentNullException("authKey", "API Key is required for the " + apiName + " API");
            this._datasetID = datasetID;
            this._apiName = apiName;
            this._authKey = authKey;
            this._requireKasabiAuth = requireAuth;
        }

        public KasabiApi(String datasetID, String apiName, String authKey)
            : this(datasetID, apiName, authKey, true) { }

        public KasabiApi(String datasetID, String apiName)
            : this(datasetID, apiName, null, false) { }

        public String DatasetID
        {
            get
            {
                return this._datasetID;
            }
        }

        public String Api
        {
            get
            {
                return this._apiName;
            }
        }

        public String AuthenticationKey
        {
            internal get
            {
                return this._authKey;
            }
            set
            {
                this._authKey = value;
            }
        }

        #region HTTP Request and Response Related Methods

        protected HttpWebRequest CreateRequest(Dictionary<String, String> apiParams, Dictionary<String,String> postParams)
        {
            //Build up the Request URI
            String requestUri = this.Uri.ToString();
            if (apiParams.Count > 0)
            {
                requestUri += "?";
                foreach (String param in apiParams.Keys)
                {
                    requestUri += param + "=" + Uri.EscapeDataString(apiParams[param]);
                    requestUri += "&";
                }
            }
            //Include API Key if required
            if (this._requireKasabiAuth && !String.IsNullOrEmpty(this._authKey))
            {
                if (apiParams.Count == 0)
                {
                    requestUri += "?";
                }
                else if (!requestUri.EndsWith("&"))
                {
                    requestUri += "&";
                }
                requestUri += KasabiParamApiKey + "=" + Uri.EscapeDataString(this._authKey);
            }

            //Now create the request
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUri);
            request.Method = this.HttpMode;
            request.Timeout = this.Timeout;
            request.Credentials = this.Credentials;
            request.Proxy = this.Proxy;
            request.UserAgent = "dotNetRDF Kasabi Client (rvesse@dotnetrdf.org)";

            //Check whether we want to use conneg or not?
            if (apiParams.ContainsKey(KasabiParamOutputFormat))
            {
                //If the output parameter is specified we'll set accept header to Any
                request.Accept = MimeTypesHelper.Any;
            }
            else
            {
                //If no output parameter do conneg so include RDF/SPARQL Results header
                //Make sure to exclude HTML
                request.Accept = MimeTypesHelper.CustomHttpAcceptHeader(from def in MimeTypesHelper.Definitions
                                                                        where !def.SyntaxName.Equals("HTML") && (def.CanParseRdf || def.CanParseSparqlResults)
                                                                        from t in def.MimeTypes
                                                                        select t);
            }

            //Add in POST data if necessary
            if ((postParams.Count > 0 || this._requireKasabiAuth) && this.HttpMode.Equals("POST"))
            {
                using (StreamWriter writer = new StreamWriter(request.GetRequestStream()))
                {
                    int i = 1;
                    foreach (String param in postParams.Keys)
                    {
                        writer.Write(param + "=" + Uri.EscapeDataString(postParams[param]));
                        if (i > postParams.Count)
                        {
                            writer.Write("&");
                        }
                        i++;
                    }

                    //if (this._requireKasabiAuth && !String.IsNullOrEmpty(this._authKey))
                    //{
                    //    if (postParams.Count > 0) writer.Write("&");
                    //    writer.Write(KasabiParamApiKey + "=" + Uri.EscapeDataString(this._authKey));
                    //}

                    writer.Close();
                }
            }


#if DEBUG
            if (Options.HttpDebugging)
            {
                Tools.HttpDebugRequest(request);
            }
#endif

            return request;
        }

        protected HttpWebRequest CreateRequest(Dictionary<String, String> apiParams)
        {
            return this.CreateRequest(apiParams, KasabiClient.EmptyParams);
        }

        public HttpWebResponse GetRawResponse(Dictionary<String, String> apiParams, Dictionary<String,String> postParams)
        {
            HttpWebRequest request = this.CreateRequest(apiParams, postParams);
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
#if DEBUG
                if (Options.HttpDebugging)
                {
                    Tools.HttpDebugResponse(response);
                }
#endif
                return response;
            }
            catch (WebException webEx)
            {
                if (webEx.Response != null)
                {
#if DEBUG
                    if (Options.HttpDebugging)
                    {
                        Tools.HttpDebugResponse((HttpWebResponse)webEx.Response);
                    }
#endif
                    switch (((HttpWebResponse)webEx.Response).StatusCode)
                    {
                        case HttpStatusCode.BadRequest:
                            throw new KasabiException("Kasabi reports that your request was malformed, please see the inner exception for details", webEx);
                        case HttpStatusCode.Forbidden:
                            throw new KasabiException("Kasabi reports that your request is forbidden.  Please check that you set your API Key correctly and that your API Key is subscribed to this API", webEx);
                        case HttpStatusCode.NotFound:
                            throw new KasabiException("Kasabi reports that the requested API was not found.  Please check that the Dataset ID and API name are correct", webEx);
                        case HttpStatusCode.InternalServerError:
                            throw new KasabiException("Kasabi reports an internal error with this API, please see the inner exception for details", webEx);
                        case HttpStatusCode.ServiceUnavailable:
                            throw new KasabiException("Kasabi reports that the requested API is temporarily unavailable, please try again later", webEx);
                        default:
                            throw new KasabiException("Kasabi returned an unexpected error code, please see inner exception for details", webEx);
                    }
                }
                else
                {
                    throw new KasabiException("Error occurred while communicating over HTTP with Kasabi, please see inner exception for details", webEx);
                }
            }
        }

        public HttpWebResponse GetRawResponse(Dictionary<String, String> apiParams)
        {
            return this.GetRawResponse(apiParams, KasabiClient.EmptyParams);
        }

        #region Specific Data Format Response Methods

        protected void GetGraphResponse(Dictionary<String, String> apiParams, Dictionary<String, String> postParams, IRdfHandler handler)
        {
            using (HttpWebResponse response = this.GetRawResponse(apiParams, postParams))
            {
                try
                {
                    //Select a Parser and Parse the Response
                    IRdfReader parser = MimeTypesHelper.GetParser(response.ContentType);
                    parser.Load(handler, new StreamReader(response.GetResponseStream()));
                }
                catch (KasabiException)
                {
                    throw;
                }
                catch (RdfParserSelectionException selEx)
                {
                    throw new KasabiException("Unable to parse the response from Kasabi as a Graph as the API returned an unsupported format, please see inner exception for details", selEx);
                }
                catch (RdfParseException parseEx)
                {
                    throw new KasabiException("Unable to parse the response from Kasabi due to a parsing error, please see the inner exception for details", parseEx);
                }
                catch (RdfException rdfEx)
                {
                    throw new KasabiException("Unable to parse the response from Kasabi due to a RDF error, please see the inner exception for details", rdfEx);
                }

                response.Close();
            }
        }

        protected void GetGraphResponse(Dictionary<String, String> apiParams, IRdfHandler handler)
        {
            this.GetGraphResponse(apiParams, KasabiClient.EmptyParams, handler);
        }

        protected IGraph GetGraphResponse(Dictionary<String, String> apiParams, Dictionary<String,String> postParams)
        {
            Graph g = new Graph();
            GraphHandler handler = new GraphHandler(g);
            this.GetGraphResponse(apiParams, postParams, handler);
            return g;
        }

        protected IGraph GetGraphResponse(Dictionary<String, String> apiParams)
        {
            return this.GetGraphResponse(apiParams, KasabiClient.EmptyParams);
        }

        protected void GetSparqlResultsResponse(Dictionary<String, String> apiParams, Dictionary<String, String> postParams, ISparqlResultsHandler handler)
        {
            using (HttpWebResponse response = this.GetRawResponse(apiParams, postParams))
            {
                try
                {
                    //Select a Parser and Parse the Response
                    ISparqlResultsReader parser = MimeTypesHelper.GetSparqlParser(response.ContentType);
                    parser.Load(handler, new StreamReader(response.GetResponseStream()));
                }
                catch (KasabiException)
                {
                    throw;
                }
                catch (RdfParserSelectionException selEx)
                {
                    throw new KasabiException("Unable to parse the response from Kasabi as a SPARQL Result Set as the API returned an unsupported format, please see inner exception for details", selEx);
                }
                catch (RdfParseException parseEx)
                {
                    throw new KasabiException("Unable to parse the response from Kasabi due to a parsing error, please see the inner exception for details", parseEx);
                }
                catch (RdfException rdfEx)
                {
                    throw new KasabiException("Unable to parse the response from Kasabi due to a RDF error, please see the inner exception for details", rdfEx);
                }

                response.Close();
            }
        }

        protected void GetSparqlResultsResponse(Dictionary<String, String> apiParams, ISparqlResultsHandler handler)
        {
            this.GetSparqlResultsResponse(apiParams, KasabiClient.EmptyParams, handler);
        }

        protected SparqlResultSet GetSparqlResultsResponse(Dictionary<String, String> apiParams, Dictionary<String, String> postParams)
        {
            SparqlResultSet results = new SparqlResultSet();
            this.GetSparqlResultsResponse(apiParams, postParams, new ResultSetHandler(results));
            return results;
        }

        protected SparqlResultSet GetSparqlResultsResponse(Dictionary<String, String> apiParams)
        {
            return this.GetSparqlResultsResponse(apiParams, KasabiClient.EmptyParams);
        }

        protected String GetStringResponse(Dictionary<String, String> apiParams, Dictionary<String, String> postParams)
        {
            String result;
            using (HttpWebResponse response = this.GetRawResponse(apiParams, postParams))
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    result = reader.ReadToEnd();
                    reader.Close();
                }

                response.Close();
            }
            return result;
        }

        protected String GetStringResponse(Dictionary<String, String> apiParams)
        {
            return this.GetStringResponse(apiParams, KasabiClient.EmptyParams);
        }

        protected JToken GetJsonResponse(Dictionary<String, String> apiParams, Dictionary<String, String> postParams)
        {
            String data = String.Empty;
            using (HttpWebResponse response = this.GetRawResponse(apiParams, postParams))
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    data = reader.ReadToEnd();
                    reader.Close();
                }
                response.Close();
            }
            return JToken.Parse(data);
        }

        protected JToken GetJsonResponse(Dictionary<String, String> apiParams)
        {
            return this.GetJsonResponse(apiParams, KasabiClient.EmptyParams);
        }

        #endregion

        #endregion
    }
}
