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
using VDS.RDF.LDF.Hydra;
using VDS.RDF.Nodes;
using VDS.RDF.Parsing;
using VDS.RDF.Query;

namespace VDS.RDF.LDF
{
    internal class LdfMetadataGraph : WrapperGraph
    {
        private const string selectSparql = """
            PREFIX hydra: <http://www.w3.org/ns/hydra/core#>
            PREFIX void:  <http://rdfs.org/ns/void#>

            SELECT DISTINCT ?page ?search
            WHERE {
            	?fragment ^void:subset   ?dataset .
            	?page     ^void:subset   ?fragment .
            	?search   ^hydra:search  ?dataset .
            	?mapping  ^hydra:mapping ?search .
            }
            """;
        private static readonly SparqlQuery select = new SparqlQueryParser().ParseFromString(selectSparql);

        internal LdfMetadataGraph(IGraph g) : base(g)
        {
            var sparqlResults = this.ExecuteQuery(select) as SparqlResultSet;
            if (sparqlResults.Count != 1) // TODO: Express invariants as SHACL?
            {
                throw new LdfException("Could not interpret metadata in TPF response graph");
            }

            var result = sparqlResults.Single();
            var page = new GraphWrapperNode(result["page"], this);

            NextPageUri = Vocabulary.Hydra.Next.ObjectsOf(page).Cast<IUriNode>().SingleOrDefault()?.Uri;
            TripleCount = Vocabulary.Void.Triples.ObjectsOf(page).SingleOrDefault()?.AsValuedNode().AsInteger();
            Search = new IriTemplate(result["search"], this);
        }

        internal IriTemplate Search { get; private set; }

        internal Uri NextPageUri { get; private set; }

        internal long? TripleCount { get; private set; }
    }
}
