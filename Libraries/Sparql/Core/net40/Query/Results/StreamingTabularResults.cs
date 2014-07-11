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

        public ~StreamingTabularResults()
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