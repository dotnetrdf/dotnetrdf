/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2026 dotNetRDF Project (http://dotnetrdf.org/)
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
using System.Text;
using System.Threading.Tasks;

namespace VDS.RDF.Utils;

/// <summary>
/// An implementation of the HttpContent class which can be used to send RDF Graphs as HTTP Content.
/// </summary>
public class RdfGraphContent: HttpContent
{
    private readonly IGraph _graph;
    private readonly IRdfWriter _writer;
    private readonly Encoding _encoding;

    /// <summary>
    /// Initializes a new instance of the <see cref="RdfGraphContent"/> class.
    /// </summary>
    /// <param name="graph">The graph whose content is to be sent in the HTTP request.</param>
    /// <param name="writer">The RDF writer used to serialize the graph.</param>
    /// <param name="encoding">The encoding to use for the serialized content. Defaults to UTF-8 if not specified.</param>
    public RdfGraphContent(IGraph graph, IRdfWriter writer, Encoding encoding = null)
    {
        _graph = graph ?? throw new ArgumentNullException(nameof(graph));
        _writer = writer ?? throw new ArgumentNullException(nameof(writer));
        _encoding = encoding ?? Encoding.UTF8;
        Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(MimeTypesHelper.GetMimeType(_writer));
    }

    /// <summary>
    /// Serialize the graph to the given stream.
    /// </summary>
    /// <param name="stream">The stream to which the graph will be serialized.</param>
    /// <param name="context">The transport context.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    protected override async Task SerializeToStreamAsync(Stream stream, TransportContext context)
    {
        using var writer = new StreamWriter(stream, _encoding, -1, leaveOpen: true);
        _writer.Save(_graph, writer);
        await writer.FlushAsync().ConfigureAwait(false);
    }

    /// <inheritdoc/>
    protected override bool TryComputeLength(out long length)
    {
        length = 0;
        return false;
    }
}