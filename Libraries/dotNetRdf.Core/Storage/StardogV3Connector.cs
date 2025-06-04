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
using VDS.RDF.Storage.Management;

namespace VDS.RDF.Storage;

/// <summary>
/// A Stardog Connector for connecting to Stardog version 3.* servers.
/// </summary>
public class StardogV3Connector
    : StardogV2Connector,
        IUpdateableStorage,
        IAsyncUpdateableStorage
{
    /// <summary>
    /// Creates a new connection to a Stardog Store.
    /// </summary>
    /// <param name="baseUri">Base Uri of the Server.</param>
    /// <param name="kbID">Knowledge Base (i.e. Database) ID.</param>
    public StardogV3Connector(string baseUri, string kbID)
        : this(baseUri, kbID, null, null)
    {
    }

    /// <summary>
    /// Creates a new connection to a Stardog Store.
    /// </summary>
    /// <param name="baseUri">Base Uri of the Server.</param>
    /// <param name="kbID">Knowledge Base (i.e. Database) ID.</param>
    /// <param name="username">Username.</param>
    /// <param name="password">Password.</param>
    public StardogV3Connector(string baseUri, string kbID, string username, string password)
        : base(baseUri, kbID, StardogReasoningMode.DatabaseControlled, username, password)
    {
        Server = new StardogV2Server(baseUri, username, password);
    }

    /// <summary>
    /// Creates a new connection to a Stardog Store.
    /// </summary>
    /// <param name="baseUri">Base Uri of the Server.</param>
    /// <param name="kbID">Knowledge Base (i.e. Database) ID.</param>
    /// <param name="proxy">Proxy Server.</param>
    public StardogV3Connector(string baseUri, string kbID, IWebProxy proxy)
        : this(baseUri, kbID, null, null, proxy)
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
    public StardogV3Connector(string baseUri, string kbID, string username, string password, IWebProxy proxy)
        : base(baseUri, kbID, StardogReasoningMode.DatabaseControlled, username, password, proxy)
    {
        Server = new StardogV2Server(baseUri, username, password, proxy);
    }


    /// <summary>
    /// Creates a new connection to a Stardog store.
    /// </summary>
    /// <param name="baseUri">Base URI of the server.</param>
    /// <param name="kbId">Knowledge base ID.</param>
    /// <param name="httpClientHandler">Handler to configure outgoing HTTP requests.</param>
    public StardogV3Connector(string baseUri, string kbId, HttpClientHandler httpClientHandler) 
        : base(baseUri, kbId, httpClientHandler)
    {
    }

    /// <inheritdoc />
    public override StardogReasoningMode Reasoning
    {
        get => StardogReasoningMode.DatabaseControlled;
        set => throw new RdfStorageException(
            "Stardog 3.x does not support configuring reasoning mode at the connection level, reasoning is instead controlled at the database level ");
    }

    /// <summary>
    /// Adds Stardog specific request headers.
    /// </summary>
    /// <param name="request"></param>
    [Obsolete("This method is obsolete and will be removed in a future release")]
    protected override void AddStardogHeaders(HttpWebRequest request)
    {
        // No special headers needed for V3
    }

    /// <summary>
    /// Adds Stardog specific request headers.
    /// </summary>
    /// <param name="request"></param>
    protected override void AddStardogHeaders(HttpRequestMessage request)
    {
        // No special headers needed for V3
    }
}