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
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VDS.RDF.Storage.Management;

namespace VDS.RDF.Storage;

/// <summary>
/// A Stardog Connector for connecting to Stardog version 2.* servers.
/// </summary>
public class StardogV2Connector
    : StardogV1Connector, IUpdateableStorage, IAsyncUpdateableStorage
{
    /// <summary>
    /// Creates a new connection to a Stardog Store.
    /// </summary>
    /// <param name="baseUri">Base Uri of the Server.</param>
    /// <param name="kbID">Knowledge Base (i.e. Database) ID.</param>
    /// <param name="reasoning">Reasoning Mode.</param>
    public StardogV2Connector(string baseUri, string kbID, StardogReasoningMode reasoning)
        : this(baseUri, kbID, reasoning, null, null)
    {
    }

    /// <summary>
    /// Creates a new connection to a Stardog Store.
    /// </summary>
    /// <param name="baseUri">Base Uri of the Server.</param>
    /// <param name="kbID">Knowledge Base (i.e. Database) ID.</param>
    public StardogV2Connector(string baseUri, string kbID)
        : this(baseUri, kbID, StardogReasoningMode.None)
    {
    }

    /// <summary>
    /// Creates a new connection to a Stardog Store.
    /// </summary>
    /// <param name="baseUri">Base Uri of the Server.</param>
    /// <param name="kbID">Knowledge Base (i.e. Database) ID.</param>
    /// <param name="username">Username.</param>
    /// <param name="password">Password.</param>
    public StardogV2Connector(string baseUri, string kbID, string username, string password)
        : this(baseUri, kbID, StardogReasoningMode.None, username, password)
    {
    }

    /// <summary>
    /// Creates a new connection to a Stardog Store.
    /// </summary>
    /// <param name="baseUri">Base Uri of the Server.</param>
    /// <param name="kbID">Knowledge Base (i.e. Database) ID.</param>
    /// <param name="username">Username.</param>
    /// <param name="password">Password.</param>
    /// <param name="reasoning">Reasoning Mode.</param>
    public StardogV2Connector(string baseUri, string kbID, StardogReasoningMode reasoning, string username,
        string password)
        : base(baseUri, kbID, reasoning, username, password)
    {
        Server = new StardogV2Server(baseUri, username, password);
    }

    /// <summary>
    /// Creates a new connection to a Stardog Store.
    /// </summary>
    /// <param name="baseUri">Base Uri of the Server.</param>
    /// <param name="kbID">Knowledge Base (i.e. Database) ID.</param>
    /// <param name="reasoning">Reasoning Mode.</param>
    /// <param name="proxy">Proxy Server.</param>
    public StardogV2Connector(string baseUri, string kbID, StardogReasoningMode reasoning, IWebProxy proxy)
        : this(baseUri, kbID, reasoning, null, null, proxy)
    {
    }

    /// <summary>
    /// Creates a new connection to a Stardog Store.
    /// </summary>
    /// <param name="baseUri">Base Uri of the Server.</param>
    /// <param name="kbID">Knowledge Base (i.e. Database) ID.</param>
    /// <param name="username">Username.</param>
    /// <param name="password">Password.</param>
    /// <param name="reasoning">Reasoning Mode.</param>
    /// <param name="proxy">Proxy Server.</param>
    public StardogV2Connector(string baseUri, string kbID, StardogReasoningMode reasoning, string username,
        string password, IWebProxy proxy)
        : base(baseUri, kbID, reasoning, username, password, proxy)
    {
        Server = new StardogV2Server(baseUri, username, password, proxy);
    }

    /// <summary>
    /// Creates a new connection to a Stardog Store.
    /// </summary>
    /// <param name="baseUri">Base Uri of the Server.</param>
    /// <param name="kbID">Knowledge Base (i.e. Database) ID.</param>
    /// <param name="proxy">Proxy Server.</param>
    public StardogV2Connector(string baseUri, string kbID, IWebProxy proxy)
        : this(baseUri, kbID, StardogReasoningMode.None, proxy)
    {
    }

    /// <summary>
    /// Creates a new connection to a Stardog Store.
    /// </summary>
    /// <param name="baseUri">Base Uri of the Server.</param>
    /// <param name="kbID">Knowledge Base (i.e. Database) ID.</param>
    /// <param name="username">Username.</param>
    /// <param name="password">Password.</param>
    /// <param name="proxy">Proxy Server.</param>
    public StardogV2Connector(string baseUri, string kbID, string username, string password, IWebProxy proxy)
        : this(baseUri, kbID, StardogReasoningMode.None, username, password, proxy)
    {
    }

    /// <summary>
    /// Creates a new connection to a Stardog store.
    /// </summary>
    /// <param name="baseUri"></param>
    /// <param name="kbId"></param>
    /// <param name="httpClientHandler"></param>
    public StardogV2Connector(string baseUri, string kbId, HttpClientHandler httpClientHandler):base(baseUri, kbId, httpClientHandler){}

    /// <summary>
    /// Adds Stardog specific request headers.
    /// </summary>
    /// <param name="request"></param>
    [Obsolete("This method is obsolete and will be removed in a future release")]
    protected override void AddStardogHeaders(HttpWebRequest request)
    {
        var reasoning = GetReasoningParameter();
#if !NETCORE
        request.Headers.Add("SD-Connection-String", reasoning);
        // Only reasoning parameter needed in Stardog 2.0, but < 2.2
#else
        request.Headers["SD-Connection-String"] = reasoning;
#endif
    }

    /// <summary>
    /// Adds Stardog specific request headers.
    /// </summary>
    /// <param name="request"></param>
    protected override void AddStardogHeaders(HttpRequestMessage request)
    {
        var reasoning = GetReasoningParameter();
        request.Headers.Add("SD-Connection-String", reasoning);
        // Only reasoning parameter needed in Stardog 2.0, but < 2.2
    }

    /// <summary>
    /// Get the query string parameter that specifies the current reasoning mode.
    /// </summary>
    /// <returns></returns>
    protected override string GetReasoningParameter()
    {
        return Reasoning switch
        {
            StardogReasoningMode.QL => "reasoning=QL",
            StardogReasoningMode.EL => "reasoning=EL",
            StardogReasoningMode.RL => "reasoning=RL",
            StardogReasoningMode.DL => "reasoning=DL",
            StardogReasoningMode.RDFS => "reasoning=RDFS",
            StardogReasoningMode.SL => "reasoning=SL",
            StardogReasoningMode.DatabaseControlled => string.Empty,
            StardogReasoningMode.None => string.Empty,
            _ => string.Empty
        };
    }

    /// <summary>
    /// Executes a SPARQL Update against the Stardog store.
    /// </summary>
    /// <param name="sparqlUpdate">SPARQL Update.</param>
    /// <remarks>
    /// Stardog executes SPARQL update requests in their own self contained transactions which do not interact with normal Stardog transactions that may be managed via this API.  In some cases this can lead to unexpected behaviour, for example if you call <see cref="BaseStardogConnector.Begin()"/>, make an update and then call <see cref="BaseStardogConnector.Rollback()"/> the updates will not be rolled back.
    /// </remarks>
    public void Update(string sparqlUpdate)
    {
        try
        {
            // NB - Updates don't run inside a transaction rather they use their own self-contained transaction

            // Create the Request
            HttpRequestMessage request = CreateRequest(KbId + "/update", MimeTypesHelper.Any, HttpMethod.Post, null);

            // Build the Post Data and add to the Request Body
            request.Content = new StringContent(sparqlUpdate, Encoding.UTF8, MimeTypesHelper.SparqlUpdate);

            // Check the response
            using HttpResponseMessage response = HttpClient.SendAsync(request).Result;
            if (!response.IsSuccessStatusCode)
            {
                throw StorageHelper.HandleHttpError(response, "executing a SPARQL update against");
            }
        }
        catch (Exception ex)
        {
            throw StorageHelper.HandleError(ex, "executing a SPARQL update against");
        }
    }

    /// <summary>
    /// Executes a SPARQL Update against the Stardog store.
    /// </summary>
    /// <param name="sparqlUpdates">SPARQL Update.</param>
    /// <param name="callback">Callback.</param>
    /// <param name="state">State to pass to callback.</param>
    /// <remarks>
    /// Stardog executes SPARQL update requests in their own self contained transactions which do not interact with normal Stardog transactions that may be managed via this API.  In some cases this can lead to unexpected behaviour, for example if you call <see cref="BaseStardogConnector.Begin(AsyncStorageCallback, object)"/>, make an update and then call <see cref="BaseStardogConnector.Rollback(AsyncStorageCallback, object)"/> the updates will not be rolled back.
    /// </remarks>
    public void Update(string sparqlUpdates, AsyncStorageCallback callback, object state)
    {
        // NB - Updates don't run inside a transaction rather they use their own self-contained transaction

        // Create the Request, for simplicity async requests are always POST
        HttpRequestMessage request = CreateRequest(KbId + "/update", MimeTypesHelper.Any, HttpMethod.Post, null);

        // Create the request body
        request.Content = new StringContent(sparqlUpdates, Encoding.UTF8, MimeTypesHelper.SparqlUpdate);
        HttpClient.SendAsync(request).ContinueWith(requestTask =>
        {
            if (requestTask.IsCanceled || requestTask.IsFaulted)
            {
                callback(this,
                    new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlUpdate,
                        requestTask.IsCanceled
                            ? new RdfStorageException("The operation was cancelled.")
                            : StorageHelper.HandleError(requestTask.Exception,
                                "executing a SPARQL update against")),
                    state);
            }
            else
            {
                HttpResponseMessage response = requestTask.Result;
                if (!response.IsSuccessStatusCode)
                {
                    callback(this,
                        new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlUpdate,
                            StorageHelper.HandleHttpError(response, "executing a SPARQL update against")),
                        state);
                }
                else
                {
                    callback(this,
                        new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlUpdate, sparqlUpdates),
                        state);
                }
            }
        });

    }

    /// <inheritdoc />
    public async Task UpdateAsync(string sparqlUpdates, CancellationToken cancellationToken)
    {
        try
        {
            HttpRequestMessage request =
                CreateRequest(KbId + "/update", MimeTypesHelper.Any, HttpMethod.Post, null);
            request.Content = new StringContent(sparqlUpdates, Encoding.UTF8, MimeTypesHelper.SparqlUpdate);
            HttpResponseMessage response = await HttpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw StorageHelper.HandleHttpError(response, "executing a SPARQL update against");
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (RdfStorageException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw StorageHelper.HandleError(ex, "executing a SPARQL update against");
        }
    }
}