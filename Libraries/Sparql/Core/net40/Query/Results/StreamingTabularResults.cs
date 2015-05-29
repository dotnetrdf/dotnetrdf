/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2015 dotNetRDF Project (dotnetrdf-develop@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections;
using System.Collections.Generic;

namespace VDS.RDF.Query.Results
{
    /// <summary>
    /// Represents a set of tabular results that are streamed
    /// </summary>
    public class StreamingTabularResults
        : ITabularResults
    {
        private readonly IResultStream _stream;
        private bool _used, _disposed;

        public StreamingTabularResults(IResultStream stream)
        {
            if (stream == null) throw new ArgumentNullException("stream");
            this._stream = stream;
        }

        ~StreamingTabularResults()
        {
            this.Dispose(false);
        }

        public IEnumerator<IResultRow> GetEnumerator()
        {
            lock (this._stream)
            {
                if (this._used) throw new RdfQueryException("Streamed results may only be enumerated once");
                this._used = true;
                return this._stream;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool IsStreaming
        {
            get { return true; }
        }

        public IEnumerable<string> Variables
        {
            get { return this._stream.Variables; }
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (this._disposed) return;
            if (disposing) GC.SuppressFinalize(this);

            lock (this._stream)
            {
                if (this._disposed) return;
                this._stream.Dispose();
                this._disposed = true;
            }
        }
    }
}