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
using System.IO;
using System.Net;
using System.Security;
using System.Text;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
using System.Web;

namespace VDS.RDF.Update;

/// <summary>
/// A Class for connecting to a remote SPARQL Update endpoint and executing Updates against it.
/// </summary>
[Obsolete("This class is obsolete and will be removed in a future release. Replaced by VDS.RDF.Update.SparqlUpdateClient.")]
public class SparqlRemoteUpdateEndpoint 
    : BaseEndpoint
{
    const int LongUpdateLength = 2048;

    /// <summary>
    /// Creates a new SPARQL Update Endpoint for the given URI.
    /// </summary>
    /// <param name="endpointUri">Endpoint URI.</param>
    public SparqlRemoteUpdateEndpoint(Uri endpointUri)
        : base(endpointUri) 
    {
        HttpMode = "POST";
    }

    /// <summary>
    /// Creates a new SPARQL Update Endpoint for the given URI.
    /// </summary>
    /// <param name="endpointUri">Endpoint URI.</param>
    public SparqlRemoteUpdateEndpoint(string endpointUri)
        : this(UriFactory.Root.Create(endpointUri)) { }

    /// <summary>
    /// Gets/Sets the HTTP Method used for requests.
    /// </summary>
    /// <remarks>
    /// The SPARQL 1.1 Protocol specification mandates that Update requests may only be POSTed, attempting to alter the HTTP Mode to anything other than POST will result in a <see cref="SparqlUpdateException">SparqlUpdateException</see>.
    /// </remarks>
    public override string HttpMode
    {
        get
        {
            return base.HttpMode;
        }
        set
        {
            if (value.Equals("POST"))
            {
                base.HttpMode = value;
            }
            else
            {
                throw new SparqlUpdateException("The SPARQL 1.1 Protocol specification requires SPARQL Updates to be POSTed, you cannot set the HTTP Method to a value other than POST");
            }
        }
    }

    /// <summary>
    /// Makes an update request to the remote endpoint.
    /// </summary>
    /// <param name="sparqlUpdate">SPARQL Update.</param>
    public void Update(string sparqlUpdate)
    {
        try
        {
            // Build the Request URI and POST Data
            var requestUri = new StringBuilder();
            requestUri.Append(Uri.AbsoluteUri);
            var postData = new StringBuilder();
            var longUpdate = false;
            if (!HttpMode.Equals("POST") && sparqlUpdate.Length <= LongUpdateLength)
            {
                if (!Uri.Query.Equals(string.Empty))
                {
                    requestUri.Append("&update=");
                }
                else
                {
                    requestUri.Append("?update=");
                }
                requestUri.Append(HttpUtility.UrlEncode(sparqlUpdate));
            }
            else
            {
                longUpdate = true;
                postData.Append("update=");
                postData.Append(HttpUtility.UrlEncode(sparqlUpdate));
            }

            // Make the request
            var request = (HttpWebRequest)WebRequest.Create(requestUri.ToString());
            ApplyRequestOptions(request);
            if (longUpdate)
            {
                request.Method = "POST";
                request.ContentType = MimeTypesHelper.Utf8WWWFormURLEncoded;
                using (var writer = new StreamWriter(request.GetRequestStream(), new UTF8Encoding(false)))
                {
                    writer.Write(postData);
                    writer.Close();
                }
            }
            else
            {
                request.Method = HttpMode;
            }
            request.Accept = MimeTypesHelper.Any;
            using (var response = (HttpWebResponse)request.GetResponse())
            {
                // If we don't get an error then we should be fine
                response.Close();
            }

        }
        catch (WebException webEx)
        {
            if (webEx.Response != null)
            {
            }

            // Some sort of HTTP Error occurred
            throw new SparqlUpdateException("A HTTP Error occurred when trying to make the SPARQL Update", webEx);
        }
    }

    /// <summary>
    /// Makes an update request asynchronously to the remote endpoint.
    /// </summary>
    /// <param name="sparqlUpdate">SPARQL Update.</param>
    /// <param name="callback">Callback to invoke when the update completes.</param>
    /// <param name="state">State to pass to the callback.</param>
    public void Update(string sparqlUpdate, UpdateCallback callback, object state)
    {
        var request = (HttpWebRequest)WebRequest.Create(Uri);
        request.Method = "POST";
        request.ContentType = MimeTypesHelper.Utf8WWWFormURLEncoded;
        request.Accept = MimeTypesHelper.Any;
        ApplyRequestOptions(request);

        try
        {
            request.BeginGetRequestStream(result =>
                {
                    try
                    {
                        Stream stream = request.EndGetRequestStream(result);
                        using (var writer = new StreamWriter(stream, new UTF8Encoding(false)))
                        {
                            writer.Write("update=");
                            writer.Write(HttpUtility.UrlEncode(sparqlUpdate));

                            writer.Close();
                        }

                        request.BeginGetResponse(innerResult =>
                            {
                                try
                                {
                                    using (var response = (HttpWebResponse) request.EndGetResponse(innerResult))
                                    {
                                        response.Close();
                                        callback(state);
                                    }
                                }
                                catch (SecurityException secEx)
                                {
                                    callback(new AsyncError(new SparqlUpdateException("Calling code does not have permission to access the specified remote endpoint, see inner exception for details", secEx), state));
                                }
                                catch (WebException webEx)
                                {
                                    if (webEx.Response != null)
                                    {
                                    }

                                    callback(new AsyncError(new SparqlUpdateException("A HTTP error occurred while making an asynchronous update, see inner exception for details", webEx), state));
                                }
                                catch (Exception ex)
                                {
                                    callback(new AsyncError(new SparqlUpdateException("Unexpected error while making an asynchronous update, see inner exception for details", ex), state));
                                }
                            }, null);
                    }
                    catch (SecurityException secEx)
                    {
                        callback(new AsyncError(new SparqlUpdateException("Calling code does not have permission to access the specified remote endpoint, see inner exception for details", secEx), state));
                    }
                    catch (WebException webEx)
                    {
                        if (webEx.Response != null)
                        {
                        }

                        callback(new AsyncError(new SparqlUpdateException("A HTTP error occurred while making an asynchronous update, see inner exception for details", webEx), state));
                    }
                    catch (Exception ex)
                    {
                        callback(new AsyncError(new SparqlUpdateException("Unexpected error while making an asynchronous update, see inner exception for details", ex), state));
                    }
                }, null);
        }
        catch (Exception ex)
        {
            callback(new AsyncError(new SparqlUpdateException("Unexpected error while making an asynchronous update, see inner exception for details", ex), state));
        }
    }

    /// <summary>
    /// Serializes configuration for the endpoint.
    /// </summary>
    /// <param name="context">Serialization Context.</param>
    public override void SerializeConfiguration(ConfigurationSerializationContext context)
    {
        INode endpoint = context.NextSubject;
        INode endpointClass = context.Graph.CreateUriNode(context.UriFactory.Create(ConfigurationLoader.ClassSparqlUpdateEndpoint));
        INode rdfType = context.Graph.CreateUriNode(context.UriFactory.Create(RdfSpecsHelper.RdfType));
        INode dnrType = context.Graph.CreateUriNode(context.UriFactory.Create(ConfigurationLoader.PropertyType));
        INode endpointUri = context.Graph.CreateUriNode(context.UriFactory.Create(ConfigurationLoader.PropertyUpdateEndpointUri));

        context.Graph.Assert(new Triple(endpoint, rdfType, endpointClass));
        context.Graph.Assert(new Triple(endpoint, dnrType, context.Graph.CreateLiteralNode(GetType().FullName)));
        context.Graph.Assert(new Triple(endpoint, endpointUri, context.Graph.CreateUriNode(Uri)));

        context.NextSubject = endpoint;
        base.SerializeConfiguration(context);
    }
}
