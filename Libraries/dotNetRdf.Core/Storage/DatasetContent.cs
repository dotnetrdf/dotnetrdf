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

using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace VDS.RDF.Storage;

internal class DatasetContent : HttpContent
{
    private readonly ITripleStore _store;
    private readonly IStoreWriter _writer;
    public DatasetContent(IGraph graph, string contentType)
    {
        _store = new SimpleTripleStore();
        _store.Add(graph);
        _writer = MimeTypesHelper.GetStoreWriter(contentType);
        Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
    }

    public DatasetContent(IGraph graph, IStoreWriter writer)
    {
        _store = new SimpleTripleStore();
        _store.Add(graph);
        _writer = writer;
        var contentType = MimeTypesHelper.GetDefinitionsByFileExtension(MimeTypesHelper.GetFileExtension(writer))
            .Select(x => x.CanonicalMimeType).FirstOrDefault();
        Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
    }

    public DatasetContent(ITripleStore store, IStoreWriter writer)
    {
        _store = store;
        _writer = writer;
        var contentType = MimeTypesHelper.GetDefinitionsByFileExtension(MimeTypesHelper.GetFileExtension(writer))
            .Select(x => x.CanonicalMimeType).FirstOrDefault();
        Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
    }

    public DatasetContent(ITripleStore store, string contentType)
    {
        _store = store;
        _writer = MimeTypesHelper.GetStoreWriter(contentType);
        Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
    }

    protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
    {
        return Task.Run(() =>
        {
            var streamWriter = new StreamWriter(stream, Encoding.UTF8, 4096, true);
            _writer.Save(_store, streamWriter);
        });
    }

    protected override bool TryComputeLength(out long length)
    {
        length = 0;
        return false;
    }
}