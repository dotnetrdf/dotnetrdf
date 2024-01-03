/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2023 dotNetRDF Project (http://dotnetrdf.org/)
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
    private readonly IRdfReader reader;
    private readonly Loader loader;
    private Uri nextPage;
    private Triple current;
    private IEnumerator<Triple> underlyingTriples;

    internal TpfEnumerator(Uri firstPage, IRdfReader reader = null, Loader loader = null)
    {
        nextPage = firstPage ?? throw new ArgumentNullException(nameof(firstPage));
        this.reader = reader;
        this.loader = loader;
    }

    Triple IEnumerator<Triple>.Current => current;

    object IEnumerator.Current => (this as IEnumerator<Triple>).Current;

    bool IEnumerator.MoveNext()
    {
        if (underlyingTriples is null)
        {
            InitializeCurrentPage();
        }

        if (underlyingTriples.MoveNext())
        {
            current = underlyingTriples.Current;
            return true;
        }

        if (nextPage is null)
        {
            current = null;
            return false;
        }

        return AdvanceToNextPage();
    }

    void IDisposable.Dispose() => underlyingTriples?.Dispose();

    void IEnumerator.Reset() => throw new NotSupportedException("This enumerator cannot be reset.");

    private void InitializeCurrentPage()
    {
        using var fragment = new TpfLoader(nextPage, reader, loader);

        underlyingTriples = fragment.Data.Triples.GetEnumerator();
        nextPage = fragment.Metadata.NextPageUri;
    }

    private bool AdvanceToNextPage()
    {
        underlyingTriples.Dispose();
        underlyingTriples = null;

        return (this as IEnumerator).MoveNext();
    }
}
