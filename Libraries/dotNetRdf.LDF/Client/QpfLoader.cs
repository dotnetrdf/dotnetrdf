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
using System.Linq;
using VDS.RDF.Parsing;
using VDS.RDF.Query;

namespace VDS.RDF.LDF.Client;

internal class QpfLoader : TripleStore
{
    private static readonly NodeFactory factory = new();

    internal QpfLoader(Uri uri, IStoreReader reader = null, Loader loader = null)
    {
        this.LoadFromUri(uri, reader ?? new NQuadsParser(), loader);

        var r = new LeviathanQueryProcessor(this).ProcessQuery(Queries.SelectQpf) as SparqlResultSet;
        var controls = r.Single()["controls"] as IRefNode;

        Metadata = new TpfMetadataGraph(this[controls]);
        Remove(controls);
    }

    internal TpfMetadataGraph Metadata { get; private set; }
}
