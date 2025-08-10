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

using System;
using System.Linq;
using VDS.RDF.LDF.Hydra;
using VDS.RDF.Nodes;
using VDS.RDF.Query;

namespace VDS.RDF.LDF.Client;

internal class TpfMetadataGraph : WrapperGraph
{
    private readonly ISparqlResult _result;

    internal TpfMetadataGraph(IGraph g) : base(g)
    {
        var sparqlResults = this.ExecuteQuery(Queries.Select) as SparqlResultSet;
        if (sparqlResults == null || sparqlResults.Count != 1)
        {
            throw new LdfException("Could not interpret metadata in TPF response graph");
        }

        _result = sparqlResults.Single();
    }

    internal IriTemplate Search => new(_result["search"], this);

    internal Uri NextPageUri => Vocabulary.Hydra.Next.ObjectsOf(Page).Cast<IUriNode>().SingleOrDefault()?.Uri;

    internal long? TripleCount => Vocabulary.Void.Triples.ObjectsOf(Page).SingleOrDefault()?.AsValuedNode().AsInteger();

    private GraphWrapperNode Page => new(_result["page"], this);
}
