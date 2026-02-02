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
using System.Collections;
using System.Collections.Generic;
using VDS.RDF.Parsing;

namespace VDS.RDF.LDF.Client;

internal class TpfEnumerator : IEnumerator<Triple>
{
    private readonly IRdfReader _reader;
    private readonly Loader _loader;
    private Uri _nextPage;
    private Triple _current;
    private TpfLoader _fragment;
    private IEnumerator<Triple> _underlyingTriples;

    internal TpfEnumerator(Uri firstPage, IRdfReader reader = null, Loader loader = null)
    {
        _nextPage = firstPage ?? throw new ArgumentNullException(nameof(firstPage));
        this._reader = reader;
        this._loader = loader;
    }

    Triple IEnumerator<Triple>.Current => _current;

    object IEnumerator.Current => (this as IEnumerator<Triple>).Current;

    bool IEnumerator.MoveNext()
    {
        if (_underlyingTriples is null)
        {
            InitializeCurrentPage();
        }

        if (_underlyingTriples != null && _underlyingTriples.MoveNext())
        {
            _current = _underlyingTriples.Current;
            return true;
        }

        if (_nextPage is null)
        {
            _current = null;
            return false;
        }

        return AdvanceToNextPage();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _underlyingTriples?.Dispose();
            _underlyingTriples = null;
            _fragment?.Dispose();
            _fragment = null;
        }
    }

    void IEnumerator.Reset() => throw new NotSupportedException("This enumerator cannot be reset.");

    private void InitializeCurrentPage()
    {
        _fragment = new TpfLoader(_nextPage, _reader, _loader);

        _underlyingTriples = _fragment.Data.Triples.GetEnumerator();
        _nextPage = _fragment.Metadata.NextPageUri;
    }

    private bool AdvanceToNextPage()
    {
        _underlyingTriples.Dispose();
        _underlyingTriples = null;
        _fragment.Dispose();
        _fragment = null;

        return (this as IEnumerator).MoveNext();
    }
}
