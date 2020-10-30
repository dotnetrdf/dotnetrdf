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

using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace VDS.RDF.Storage
{
    internal class GraphContent: HttpContent
    {
        private readonly IGraph _graph;
        private readonly IRdfWriter _writer;

        /// <summary>
        /// Get or set the text encoding to use when writing RDF data.
        /// </summary>
        public Encoding Encoding { get; set; } = Encoding.UTF8;

        /// <summary>
        /// Get or set whether the server requires Content-Length to be set.
        /// </summary>
        public bool ContentLengthRequired { get; set; } = false;

        private MemoryStream _buffer;

        public GraphContent(IGraph graph, string contentType)
        {
            _graph = graph;
            _writer = MimeTypesHelper.GetWriter(contentType);
            Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
        }

        public GraphContent(IGraph graph, IRdfWriter writer)
        {
            _graph = graph;
            _writer = writer;
            Headers.ContentType = MediaTypeHeaderValue.Parse(MimeTypesHelper.GetMimeType(writer));
        }

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            return Task.Run(() =>
            {
                if (_buffer != null)
                {
                    _buffer.Seek(0, SeekOrigin.Begin);
                    _buffer.CopyTo(stream);
                }
                else
                {
                    var streamWriter = new StreamWriter(stream, Encoding, 4096, true);
                    _writer.Save(_graph, streamWriter);
                }
            });
        }

        protected override bool TryComputeLength(out long length)
        {
            if (ContentLengthRequired)
            {
                // We will have to buffer the data locally before sending
                _buffer = new MemoryStream();
                _writer.Save(_graph, new StreamWriter(_buffer, Encoding, 4096, true));
                length = _buffer.Length;
                return true;
            }
            length = 0;
            return false;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _buffer?.Dispose();
                _buffer = null;
            }
            base.Dispose(disposing);
        }
    }
}
