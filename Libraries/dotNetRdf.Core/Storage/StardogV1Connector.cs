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

using System.Net;
using System.Net.Http;

namespace VDS.RDF.Storage;

/// <summary>
/// A Stardog Connector for connecting to Stardog version 1.* servers.
/// </summary>
public class StardogV1Connector
    : BaseStardogConnector
{
    /// <summary>
    /// Creates a new connection to a Stardog Store.
    /// </summary>
    /// <param name="baseUri">Base Uri of the Server.</param>
    /// <param name="kbID">Knowledge Base (i.e. Database) ID.</param>
    /// <param name="reasoning">Reasoning Mode.</param>
    public StardogV1Connector(string baseUri, string kbID, StardogReasoningMode reasoning)
        : this(baseUri, kbID, reasoning, null, null)
    {
    }

    /// <summary>
    /// Creates a new connection to a Stardog Store.
    /// </summary>
    /// <param name="baseUri">Base Uri of the Server.</param>
    /// <param name="kbID">Knowledge Base (i.e. Database) ID.</param>
    public StardogV1Connector(string baseUri, string kbID)
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
    public StardogV1Connector(string baseUri, string kbID, string username, string password)
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
    public StardogV1Connector(string baseUri, string kbID, StardogReasoningMode reasoning, string username,
        string password)
        : base(baseUri, kbID, reasoning, username, password)
    {
    }

    /// <summary>
    /// Creates a new connection to a Stardog Store.
    /// </summary>
    /// <param name="baseUri">Base Uri of the Server.</param>
    /// <param name="kbID">Knowledge Base (i.e. Database) ID.</param>
    /// <param name="reasoning">Reasoning Mode.</param>
    /// <param name="proxy">Proxy Server.</param>
    public StardogV1Connector(string baseUri, string kbID, StardogReasoningMode reasoning, IWebProxy proxy)
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
    public StardogV1Connector(string baseUri, string kbID, StardogReasoningMode reasoning, string username,
        string password, IWebProxy proxy)
        : base(baseUri, kbID, reasoning, username, password, proxy)
    {
    }

    /// <summary>
    /// Creates a new connection to a Stardog Store.
    /// </summary>
    /// <param name="baseUri">Base Uri of the Server.</param>
    /// <param name="kbID">Knowledge Base (i.e. Database) ID.</param>
    /// <param name="proxy">Proxy Server.</param>
    public StardogV1Connector(string baseUri, string kbID, IWebProxy proxy)
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
    public StardogV1Connector(string baseUri, string kbID, string username, string password, IWebProxy proxy)
        : this(baseUri, kbID, StardogReasoningMode.None, username, password, proxy)
    {
    }

    /// <summary>
    /// Creates a new connection to a Stardog store.
    /// </summary>
    /// <param name="baseUri"></param>
    /// <param name="kbId"></param>
    /// <param name="httpClientHandler"></param>
    public StardogV1Connector(string baseUri, string kbId, HttpClientHandler httpClientHandler) 
        : base(baseUri, kbId, StardogReasoningMode.None, httpClientHandler)
    {

    }
}