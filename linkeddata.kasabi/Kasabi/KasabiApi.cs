using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;

namespace VDS.RDF.LinkedData.Kasabi
{
    public abstract class KasabiApi
        : BaseEndpoint
    {
        public const String KasabiBaseApiUri = "http://api.kasabi.com/dataset/";

        public const String KasabiParamOutputFormat = "output";

        private String _authKey;
        private String _datasetID, _apiName;
        private bool _requireKasabiAuth = true;

        public KasabiApi(String datasetID, String apiName, String authKey, bool requireAuth)
            : base(new Uri(KasabiBaseApiUri + datasetID + "/" + apiName))
        {
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

        protected HttpWebRequest CreateRequest(Dictionary<String, String> apiParams)
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
                if (!requestUri.EndsWith("&")) requestUri += "&";
                requestUri += "apikey=" + Uri.EscapeDataString(this._authKey);
            }

            //Now create the request
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUri);
            request.Method = this.HttpMode;
            request.Timeout = this.Timeout;
            request.Credentials = this.Credentials;
            request.Proxy = this.Proxy;

            //Check whether we want to use conneg or not?
            if (apiParams.ContainsKey(KasabiParamOutputFormat))
            {
                //If the output parameter is specified we'll set accept header to Any
                request.Accept = MimeTypesHelper.Any;
            }
            else
            {
                //If no output parameter do conneg so include RDF/SPARQL Results header
                request.Accept = MimeTypesHelper.HttpRdfOrSparqlAcceptHeader;
            }

#if DEBUG
            if (Options.HttpDebugging)
            {
                Tools.HttpDebugRequest(request);
            }
#endif

            return request;
        }

        public HttpWebResponse GetRawResponse(Dictionary<String, String> apiParams)
        {
            HttpWebRequest request = this.CreateRequest(apiParams);
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

        protected void GetGraphResponse(Dictionary<String, String> apiParams, IRdfHandler handler)
        {
            using (HttpWebResponse response = this.GetRawResponse(apiParams))
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

        protected IGraph GetGraphResponse(Dictionary<String, String> apiParams)
        {
            Graph g = new Graph();
            GraphHandler handler = new GraphHandler(g);
            this.GetGraphResponse(apiParams, handler);
            return g;
        }
    }
}
