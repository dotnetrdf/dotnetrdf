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
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;

namespace VDS.RDF.Update;

/// <summary>
/// A class for connecting to a remote SPARQL update endpoint and executing updates against it using the System.Net.Http library.
/// </summary>
public class SparqlUpdateClient : IConfigurationSerializable
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Creates a new SPARQL Update client for the given endpoint URI.
    /// </summary>
    /// <param name="httpClient">The underlying HTTP client to use for all connections to the remote endpoint.</param>
    /// <param name="endpointUri">The URI of the remote endpoint to connect to.</param>
    public SparqlUpdateClient(HttpClient httpClient, Uri endpointUri)
    {
        _httpClient = httpClient;
        EndpointUri = endpointUri;
    }

    /// <summary>
    /// The URI of the SPARQL update endpoint.
    /// </summary>
    public Uri EndpointUri { get; }

    /// <summary>
    /// Makes an update request to the remote endpoint.
    /// </summary>
    /// <param name="sparqlUpdate">The SPARQL Update request.</param>
    /// <returns></returns>
    public async Task UpdateAsync(string sparqlUpdate)
    {
        await UpdateAsync(sparqlUpdate, CancellationToken.None);

    }

    /// <summary>
    /// Makes an update request to the remote endpoint.
    /// </summary>
    /// <param name="sparqlUpdate">The SPARQL Update request.</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns></returns>
    public async Task UpdateAsync(string sparqlUpdate, CancellationToken cancellationToken)
    {
        var content = new FormUrlEncodedContent(
            new[] {new KeyValuePair<string, string>("update", sparqlUpdate)}
        );
        HttpResponseMessage response = await _httpClient.PostAsync(EndpointUri, content, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new SparqlUpdateException($"Server returned {(int)response.StatusCode}: {response.ReasonPhrase}");
        }
    }

    /// <summary>
    /// Serializes configuration for the endpoint.
    /// </summary>
    /// <param name="context">Serialization Context.</param>
    public void SerializeConfiguration(ConfigurationSerializationContext context)
    {
        INode endpoint = context.NextSubject;
        INode endpointClass = context.Graph.CreateUriNode(context.UriFactory.Create(ConfigurationLoader.ClassSparqlUpdateClient));
        INode rdfType = context.Graph.CreateUriNode(context.UriFactory.Create(RdfSpecsHelper.RdfType));
        INode dnrType = context.Graph.CreateUriNode(context.UriFactory.Create(ConfigurationLoader.PropertyType));
        INode endpointUri = context.Graph.CreateUriNode(context.UriFactory.Create(ConfigurationLoader.PropertyUpdateEndpointUri));

        context.Graph.Assert(new Triple(endpoint, rdfType, endpointClass));
        context.Graph.Assert(new Triple(endpoint, dnrType, context.Graph.CreateLiteralNode(GetType().FullName)));
        context.Graph.Assert(new Triple(endpoint, endpointUri, context.Graph.CreateUriNode(EndpointUri)));
    }
}
