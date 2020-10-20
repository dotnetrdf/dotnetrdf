/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2020 dotNetRDF Project (http://dotnetrdf.org/)
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
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VDS.RDF.Parsing.Handlers;

namespace VDS.RDF.Parsing
{
    /// <summary>
    /// A class providing utility methods for loading RDF data from URIS.
    /// </summary>
    public class Loader
    {
        private HttpClient _httpClient;

        /// <summary>
        /// Get or set the client to use for making HTTP requests.
        /// </summary>
        /// <remarks>This property must not be set to null.</remarks>
        public HttpClient HttpClient { get => _httpClient; set => _httpClient = value ?? throw new ArgumentNullException(nameof(value), "The HttpClient property cannot be set to null."); }

        /// <summary>
        /// Create a new Loader instance with the specified HTTP client.
        /// </summary>
        /// <param name="httpClient">The HTTP client instance to use. Must not be null.</param>
        public Loader(HttpClient httpClient)
        {

            HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        /// <summary>
        /// Create a new Loader instance with a new default HTTP client.
        /// </summary>
        public Loader() : this(new HttpClient()){}

        /// <summary>
        /// Load RDF data from the specified URI into the specified graph.
        /// </summary>
        /// <param name="graph">The target graph to load RDF data into.</param>
        /// <param name="uri">The URI of the RDF data to be loaded.</param>
        /// <remarks>
        /// <para>If <paramref name="uri"/> is a data: URI, the <see cref="DataUriLoader"/> will be used to load the content.
        /// If <paramref name="uri"/> is a reference to a local file, the <see cref="FileLoader"/> will be used to load the content.
        /// Otherwise the loader will attempt to dereference the URI using the <see cref="System.Net.Http.HttpClient"/>.</para>
        /// <para>
        /// The loader will attempt to automatically determine the parser to be used from the Content Type header returned by the remote server (or by the file extension if the URI is a local file URI).
        /// </para>
        /// </remarks>
        public async Task LoadGraphAsync(IGraph graph, Uri uri)
        {
            await LoadGraphAsync(graph, uri, null, CancellationToken.None);
        }

        /// <summary>
        /// Load RDF data from the specified URI into the specified graph.
        /// </summary>
        /// <param name="graph">The target graph to load RDF data into.</param>
        /// <param name="uri">The URI of the RDF data to be loaded.</param>
        /// <remarks>
        /// <para>If <paramref name="uri"/> is a data: URI, the <see cref="DataUriLoader"/> will be used to load the content.
        /// If <paramref name="uri"/> is a reference to a local file, the <see cref="FileLoader"/> will be used to load the content.
        /// Otherwise the loader will attempt to dereference the URI using the <see cref="System.Net.Http.HttpClient"/>.</para>
        /// <para>
        /// The loader will attempt to automatically determine the parser to be used from the Content Type header returned by the remote server (or by the file extension if the URI is a local file URI).
        /// </para>
        /// </remarks>
        public void LoadGraph(IGraph graph, Uri uri)
        {
            LoadGraphAsync(graph, uri, null, CancellationToken.None).RunSynchronously();
        }

        /// <summary>
        /// Load RDF data from the specified URI into the specified graph.
        /// </summary>
        /// <param name="graph">The target graph to load RDF data into.</param>
        /// <param name="uri">The URI of the RDF data to be loaded.</param>
        /// <param name="parser">The parser to use when loading data.</param>
        /// <remarks>
        /// <para>If <paramref name="uri"/> is a data: URI, the <see cref="DataUriLoader"/> will be used to load the content.
        /// If <paramref name="uri"/> is a reference to a local file, the <see cref="FileLoader"/> will be used to load the content.
        /// Otherwise the loader will attempt to dereference the URI using the <see cref="System.Net.Http.HttpClient"/>.</para>
        /// <para>
        /// If <paramref name="parser"/> is null, the loader will attempt to automatically determine the parser to be used from the Content Type header returned by the remote server (or by the file extension if the URI is a local file URI).
        /// If <paramref name="parser"/> is not null, then this parser will always be used regardless of the Content Type header returned by the remote server.
        /// </para>
        /// </remarks>
        public async Task LoadGraphAsync(IGraph graph, Uri uri, IRdfReader parser)
        {
            await LoadGraphAsync(graph, uri, parser, CancellationToken.None);
        }

        /// <summary>
        /// Load RDF data from the specified URI into the specified graph.
        /// </summary>
        /// <param name="graph">The target graph to load RDF data into.</param>
        /// <param name="uri">The URI of the RDF data to be loaded.</param>
        /// <param name="parser">The parser to use when loading data.</param>
        /// <remarks>
        /// <para>If <paramref name="uri"/> is a data: URI, the <see cref="DataUriLoader"/> will be used to load the content.
        /// If <paramref name="uri"/> is a reference to a local file, the <see cref="FileLoader"/> will be used to load the content.
        /// Otherwise the loader will attempt to dereference the URI using the <see cref="System.Net.Http.HttpClient"/>.</para>
        /// <para>
        /// If <paramref name="parser"/> is null, the loader will attempt to automatically determine the parser to be used from the Content Type header returned by the remote server (or by the file extension if the URI is a local file URI).
        /// If <paramref name="parser"/> is not null, then this parser will always be used regardless of the Content Type header returned by the remote server.
        /// </para>
        /// </remarks>
        public void LoadGraph(IGraph graph, Uri uri, IRdfReader parser)
        {
            LoadGraphAsync(graph, uri, parser, CancellationToken.None).RunSynchronously();
        }

        /// <summary>
        /// Load RDF data from the specified URI into the specified graph.
        /// </summary>
        /// <param name="graph">The target graph to load RDF data into.</param>
        /// <param name="uri">The URI of the RDF data to be loaded.</param>
        /// <param name="parser">The parser to use when loading data.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <remarks>
        /// <para>If <paramref name="uri"/> is a data: URI, the <see cref="DataUriLoader"/> will be used to load the content.
        /// If <paramref name="uri"/> is a reference to a local file, the <see cref="FileLoader"/> will be used to load the content.
        /// Otherwise the loader will attempt to dereference the URI using the <see cref="System.Net.Http.HttpClient"/>.</para>
        /// <para>
        /// If <paramref name="parser"/> is null, the loader will attempt to automatically determine the parser to be used from the Content Type header returned by the remote server (or by the file extension if the URI is a local file URI).
        /// If <paramref name="parser"/> is not null, then this parser will always be used regardless of the Content Type header returned by the remote server.
        /// </para>
        /// </remarks>
        public async Task LoadGraphAsync(IGraph graph, Uri uri, IRdfReader parser, CancellationToken cancellationToken)
        {
            if (graph == null) throw new ArgumentNullException(nameof(graph), "Cannot read RDF into a null Graph.");
            if (uri == null) throw new ArgumentNullException(nameof(uri), "Cannot read RDF from a null URI");
            if (uri.IsFile)
            {
                // Pass through to the FileLoader
                RaiseWarning("This is a file: URI so invoking the FileLoader instead");
                if (Path.DirectorySeparatorChar == '/')
                {
                    FileLoader.Load(graph, uri.ToString().Substring(7), parser);
                }
                else
                {
                    FileLoader.Load(graph, uri.ToString().Substring(8), parser);
                }
                return;
            }

            if (uri.Scheme.Equals("data"))
            {
                // Invoke DataUriLoader instead
                RaiseWarning("This is a data: URI so invoking the DataUriLoader instead");
                DataUriLoader.Load(graph, uri);
                return;
            }

            // Set graph Base URI if necessary
            if (graph.BaseUri == null && graph.IsEmpty) graph.BaseUri = uri;
            await LoadGraphAsync(new GraphHandler(graph), uri, parser, cancellationToken);
        }

        /// <summary>
        /// Parse RDF data from the specified URI and pass the resulting triples to the specified handler.
        /// </summary>
        /// <param name="handler">The handler to receive the parsed RDF triples.</param>
        /// <param name="uri">The URI of the RDF data to be loaded.</param>
        /// <param name="parser">The parser to use when loading data.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <remarks>
        /// <para>If <paramref name="uri"/> is a data: URI, the <see cref="DataUriLoader"/> will be used to load the content.
        /// If <paramref name="uri"/> is a reference to a local file, the <see cref="FileLoader"/> will be used to load the content.
        /// Otherwise the loader will attempt to dereference the URI using the <see cref="System.Net.Http.HttpClient"/>.</para>
        /// <para>
        /// If <paramref name="parser"/> is null, the loader will attempt to automatically determine the parser to be used from the Content Type header returned by the remote server (or by the file extension if the URI is a local file URI).
        /// If <paramref name="parser"/> is not null, then this parser will always be used regardless of the Content Type header returned by the remote server.
        /// </para>
        /// </remarks>

        public async Task LoadGraphAsync(IRdfHandler handler, Uri uri, IRdfReader parser, CancellationToken cancellationToken)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler), "Cannot read RDF using a null RDF Handler");
            if (uri == null) throw new ArgumentNullException(nameof(uri), "Cannot read RDF from a null URI");

            try
            {
                if (uri.IsFile)
                {
                    // Invoke FileLoader instead
                    RaiseWarning("This is a file: URI so invoking the FileLoader instead");
                    FileLoader.Load(handler,
                        Path.DirectorySeparatorChar == '/' ? uri.ToString().Substring(7) : uri.ToString().Substring(8),
                        parser);

                    return;
                }

                if (uri.Scheme.Equals("data"))
                {
                    // Invoke DataUriLoader instead
                    RaiseWarning("This is a data: URI so invoking the DataUriLoader instead");
                    DataUriLoader.Load(handler, uri);
                    return;
                }

                uri = Tools.StripUriFragment(uri);
                var requestMessage = new HttpRequestMessage(HttpMethod.Get, uri);
                requestMessage.Headers.Add("Accept",
                    parser != null ? MimeTypesHelper.CustomHttpAcceptHeader(parser) : MimeTypesHelper.HttpAcceptHeader);

                using var httpResponse = await HttpClient.SendAsync(requestMessage, cancellationToken);
                AssertResponseSuccess(uri, httpResponse);

                parser ??= MimeTypesHelper.GetParser(httpResponse.Content.Headers.ContentType.MediaType);
                parser.Warning += RaiseWarning;

                // TODO: Replace with an async load method when available
                parser.Load(handler, new StreamReader(await httpResponse.Content.ReadAsStreamAsync()));
            }
            catch (UriFormatException ex)
            {
                throw new RdfParseException("Unable to load from the given URI '" + uri.AbsoluteUri + "' since it's format was invalid", ex);
            }
        }

        private static void AssertResponseSuccess(Uri uri, HttpResponseMessage httpResponse)
        {
            if (!httpResponse.IsSuccessStatusCode)
            {
                throw new RdfException(
                    $"Failed to load from the URI '{uri.AbsoluteUri}'. Server reports status {(int) httpResponse.StatusCode}: {httpResponse.ReasonPhrase}.");
            }
        }

        /// <summary>
        /// Attempts to load a RDF dataset from the given URI into the given Triple Store.
        /// </summary>
        /// <param name="store">Triple Store to load into.</param>
        /// <param name="uri">URI to attempt to get a RDF dataset from.</param>
        /// <remarks>
        /// <para>
        /// Attempts to select the relevant Store Parser based on the Content Type header returned in the HTTP Response.
        /// </para>
        /// </remarks>
        public async Task LoadDatasetAsync(ITripleStore store, Uri uri)
        {
            await LoadDatasetAsync(store, uri, null, CancellationToken.None);
        }

        /// <summary>
        /// Attempts to load a RDF dataset from the given URI into the given Triple Store.
        /// </summary>
        /// <param name="store">Triple Store to load into.</param>
        /// <param name="uri">URI to attempt to get a RDF dataset from.</param>
        /// <remarks>
        /// <para>
        /// Attempts to select the relevant Store Parser based on the Content Type header returned in the HTTP Response.
        /// </para>
        /// </remarks>
        public void LoadDataset(ITripleStore store, Uri uri)
        {
            LoadDatasetAsync(store, uri, null, CancellationToken.None).RunSynchronously();
        }

        /// <summary>
        /// Attempts to load a RDF dataset from the given URI using a RDF Handler.
        /// </summary>
        /// <param name="handler">RDF Handler to use.</param>
        /// <param name="u">URI to attempt to get a RDF dataset from.</param>
        /// <remarks>
        /// <para>
        /// Attempts to select the relevant Store Parser based on the Content Type header returned in the HTTP Response.
        /// </para>
        /// </remarks>
        public async Task LoadDatasetAsync(IRdfHandler handler, Uri u)
        {
            await LoadDatasetAsync(handler, u, (IStoreReader) null, CancellationToken.None);
        }

        /// <summary>
        /// Attempts to load a RDF dataset from the given URI into the given Triple Store.
        /// </summary>
        /// <param name="store">Triple Store to load into.</param>
        /// <param name="uri">URI to attempt to get a RDF dataset from.</param>
        /// <param name="parser">Parser to use to parse the RDF dataset.</param>
        /// <remarks>
        /// <para>
        /// If the <paramref name="parser"/> parameter is set to null then this method attempts to select the relevant Store Parser based on the Content Type header returned in the HTTP Response.
        /// </para>
        /// <para>
        /// If you know ahead of time the Content Type you can explicitly pass in the parser to use.
        /// </para>
        /// </remarks>
        public async Task LoadDatasetAsync(ITripleStore store, Uri uri, IStoreReader parser)
        {
            await LoadDatasetAsync(store, uri, parser, CancellationToken.None);
        }

        /// <summary>
        /// Attempts to load a RDF dataset from the given URI into the given Triple Store.
        /// </summary>
        /// <param name="store">Triple Store to load into.</param>
        /// <param name="uri">URI to attempt to get a RDF dataset from.</param>
        /// <param name="parser">Parser to use to parse the RDF dataset.</param>
        /// <remarks>
        /// <para>
        /// If the <paramref name="parser"/> parameter is set to null then this method attempts to select the relevant Store Parser based on the Content Type header returned in the HTTP Response.
        /// </para>
        /// <para>
        /// If you know ahead of time the Content Type you can explicitly pass in the parser to use.
        /// </para>
        /// </remarks>
        public void LoadDataset(ITripleStore store, Uri uri, IStoreReader parser)
        {
            LoadDatasetAsync(store, uri, parser, CancellationToken.None).RunSynchronously();
        }

        /// <summary>
        /// Attempts to load a RDF dataset from the given URI using a RDF Handler.
        /// </summary>
        /// <param name="handler">RDF Handler to use.</param>
        /// <param name="uri">URI to attempt to get a RDF dataset from.</param>
        /// <param name="parser">Parser to use to parse the RDF dataset.</param>
        /// <remarks>
        /// <para>
        /// If the <paramref name="parser"/> parameter is set to null then this method attempts to select the relevant Store Parser based on the Content Type header returned in the HTTP Response.
        /// </para>
        /// <para>
        /// If you know ahead of time the Content Type you can explicitly pass in the parser to use.
        /// </para>
        /// </remarks>
        public async Task LoadDatasetAsync(IRdfHandler handler, Uri uri, IStoreReader parser)
        {
            await LoadDatasetAsync(handler, uri, parser, CancellationToken.None);
        }


        /// <summary>
        /// Attempts to load a RDF dataset from the given URI using a RDF Handler.
        /// </summary>
        /// <param name="handler">RDF Handler to use.</param>
        /// <param name="uri">URI to attempt to get a RDF dataset from.</param>
        /// <param name="parser">Parser to use to parse the RDF dataset.</param>
        /// <remarks>
        /// <para>
        /// If the <paramref name="parser"/> parameter is set to null then this method attempts to select the relevant Store Parser based on the Content Type header returned in the HTTP Response.
        /// </para>
        /// <para>
        /// If you know ahead of time the Content Type you can explicitly pass in the parser to use.
        /// </para>
        /// </remarks>
        public void LoadDataset(IRdfHandler handler, Uri uri, IStoreReader parser)
        {
            LoadDatasetAsync(handler, uri, parser, CancellationToken.None).RunSynchronously();
        }

        /// <summary>
        /// Attempts to load a RDF dataset from the given URI into the given Triple Store.
        /// </summary>
        /// <param name="store">Triple Store to load into.</param>
        /// <param name="uri">URI to attempt to get a RDF dataset from.</param>
        /// <param name="parser">Parser to use to parse the RDF dataset.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <remarks>
        /// <para>
        /// If the <paramref name="parser"/> parameter is set to null then this method attempts to select the relevant Store Parser based on the Content Type header returned in the HTTP Response.
        /// </para>
        /// <para>
        /// If you know ahead of time the Content Type you can explicitly pass in the parser to use.
        /// </para>
        /// </remarks>
        public async Task LoadDatasetAsync(ITripleStore store, Uri uri, IStoreReader parser, CancellationToken cancellationToken)
        {
            if (store == null) throw new ArgumentNullException(nameof(store), "Cannot read an RDF dataset into a null Triple Store");
            if (uri == null) throw new ArgumentNullException(nameof(parser), "Cannot read an RDF dataset from a null URI");
            await LoadDatasetAsync(new StoreHandler(store), uri, parser, cancellationToken);
        }

        /// <summary>
        /// Attempts to load a RDF dataset from the given URI using a RDF Handler.
        /// </summary>
        /// <param name="handler">RDF Handler to use.</param>
        /// <param name="uri">URI to attempt to get a RDF dataset from.</param>
        /// <param name="parser">Parser to use to parse the RDF dataset.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <remarks>
        /// <para>
        /// If the <paramref name="parser"/> parameter is set to null then this method attempts to select the relevant Store Parser based on the Content Type header returned in the HTTP Response.
        /// </para>
        /// <para>
        /// If you know ahead of time the Content Type you can explicitly pass in the parser to use.
        /// </para>
        /// </remarks>
        public async Task LoadDatasetAsync(IRdfHandler handler, Uri uri, IStoreReader parser,
            CancellationToken cancellationToken)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler) ,"Cannot read an RDF dataset using a null RDF handler");
            if (uri == null) throw new ArgumentNullException(nameof(uri), "Cannot read an RDF dataset from a null URI");

            try
            {
                if (uri.IsFile)
                {
                    // Use the FileLoader instead
                    RaiseWarning("This is a file: URI so invoking the FileLoader instead");
                    if (Path.DirectorySeparatorChar == '/')
                    {
                        FileLoader.Load(handler, uri.AbsoluteUri.Substring(7), parser);
                    }
                    else
                    {
                        FileLoader.Load(handler, uri.AbsoluteUri.Substring(8), parser);
                    }

                    return;
                }

                if (uri.Scheme.Equals("data"))
                {
                    // Invoke DataUriLoader instead
                    RaiseWarning("This is a data: URI so invoking the DataUriLoader instead");
                    DataUriLoader.Load(handler, uri);
                    return;
                }

                // Sanitize request URI by removing any fragment ID
                uri = Tools.StripUriFragment(uri);

                // Set up the request
                var requestMessage = new HttpRequestMessage(HttpMethod.Get, uri);

                // Set Accept header
                requestMessage.Headers.Add("Accept",
                    parser != null
                        ? MimeTypesHelper.CustomHttpAcceptHeader(parser)
                        : MimeTypesHelper.HttpRdfDatasetAcceptHeader);

                using var responseMessage = await HttpClient.SendAsync(requestMessage, cancellationToken);
                AssertResponseSuccess(uri, responseMessage);

                if (parser == null)
                {
                    try
                    {
                        parser = MimeTypesHelper.GetStoreParser(responseMessage.Content.Headers.ContentType.MediaType);
                        parser.Warning += RaiseStoreWarning;
                        var stream = await responseMessage.Content.ReadAsStreamAsync();
                        cancellationToken.ThrowIfCancellationRequested();
                        parser.Load(handler, new StreamReader(stream));
                    }
                    catch (RdfParserSelectionException)
                    {
                        RaiseStoreWarning("Unable to select an RDF Dataset parser based on Content-Type: " +
                                          responseMessage.Content.Headers.ContentType +
                                          " - seeing if the content is an RDF Graph instead.");
                        try
                        {
                            var rdfParser = MimeTypesHelper.GetParser(responseMessage.Content.Headers.ContentType.MediaType);
                            var stream = await responseMessage.Content.ReadAsStreamAsync();
                            cancellationToken.ThrowIfCancellationRequested();
                            rdfParser.Load(handler, new StreamReader(stream));
                        }
                        catch (RdfParserSelectionException)
                        {
                            // Fall back to assuming a dataset and trying format guessing
                            RaiseStoreWarning("Unable to select and RDF Graph parser based on Content-Type: " +
                                              responseMessage.Content.Headers.ContentType +
                                              " - attempting to determine RDF Dataset format from content.");
                            var data = await responseMessage.Content.ReadAsStringAsync();
                            cancellationToken.ThrowIfCancellationRequested();
                            parser = StringParser.GetDatasetParser(data);
                            parser.Warning += RaiseStoreWarning;
                            parser.Load(handler, new StringReader(data));
                        }
                    }
                }
                else
                {
                    parser.Warning += RaiseStoreWarning;
                    parser.Load(handler, new StreamReader(await responseMessage.Content.ReadAsStreamAsync()));
                }
            }
            catch (UriFormatException uriEx)
            {
                throw new RdfException($"Unable to load from the given URI '" + uri.AbsoluteUri + "' since its format was invalid. See inner exception for details.", uriEx);
            }
        }

        #region Warning Events

        /// <summary>
        /// Raises warning messages.
        /// </summary>
        /// <param name="message">Warning Message.</param>
        private static void RaiseWarning(string message)
        {
            var d = Warning;
            d?.Invoke(message);
        }

        /// <summary>
        /// Raises store warning messages.
        /// </summary>
        /// <param name="message">Warning Message.</param>
        private static void RaiseStoreWarning(string message)
        {
            var d = StoreWarning;
            d?.Invoke(message);
        }

        /// <summary>
        /// Event which is raised when a parser that is invoked by the UriLoader notices a non-fatal issue with the RDF syntax
        /// </summary>
        public static event RdfReaderWarning Warning;

        /// <summary>
        /// Event which is raised when a store parser that is invoked by the UriLoader notices a non-fatal issue with the RDF dataset syntax
        /// </summary>
        public static event StoreReaderWarning StoreWarning;

        #endregion
    }
}
