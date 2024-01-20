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
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Parsing;

namespace VDS.RDF.LDF.Client;

public class QpfLiveTripleStore(Uri baseUri, IStoreReader reader = null, Loader loader = null) : ITripleStore
{
    [Obsolete("Replaced by this[IRefNode]")]
    public IGraph this[Uri graphUri] => throw new NotImplementedException();

    public IGraph this[IRefNode graphName] => throw new NotImplementedException();

    public bool IsEmpty => Quads.Any();

    public BaseGraphCollection Graphs => throw new NotImplementedException();

    public IEnumerable<Triple> Triples => Quads.Select(q => q.Triple).Distinct();

    public IEnumerable<Quad> Quads => new QpfEnumerable(baseUri, reader, loader);

    public IUriFactory UriFactory => throw new NotImplementedException();

    public event TripleStoreEventHandler GraphAdded;
    public event TripleStoreEventHandler GraphRemoved;
    public event TripleStoreEventHandler GraphChanged;
    public event TripleStoreEventHandler GraphCleared;
    public event TripleStoreEventHandler GraphMerged;

    public void Dispose() { }

    [Obsolete("Replaced by HasGraph(IRefNode)")]
    public bool HasGraph(Uri graphUri) => throw new NotImplementedException();

    public bool HasGraph(IRefNode graphName) => throw new NotImplementedException();











    public bool Add(IGraph g) => throw new NotSupportedException();

    public bool Add(IGraph g, bool mergeIfExists) => throw new NotSupportedException();

    public bool AddFromUri(Uri graphUri) => throw new NotSupportedException();

    public bool AddFromUri(Uri graphUri, bool mergeIfExists) => throw new NotSupportedException();

    public bool AddFromUri(Uri graphUri, bool mergeIfExists, Loader loader) => throw new NotSupportedException();

    [Obsolete("Replaced by Remove(IRefNode)")]
    public bool Remove(Uri graphUri) => throw new NotSupportedException();

    public bool Remove(IRefNode graphName) => throw new NotSupportedException();
}
