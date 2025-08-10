/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using Xunit;

namespace VDS.RDF.LDF.Client;

[SuppressMessage("Usage", "xUnit1004:Test methods should not be skipped", Justification = BecauseThisIsExampleCode)]
public class Examples(ITestOutputHelper output)
{
    private const string BecauseThisIsExampleCode = "Skipping example code";

    [Fact(DisplayName = "First 1000 triples from DBpedia", Skip = BecauseThisIsExampleCode)]
    public void Example1()
    {
        using var dbpedia = new TpfLiveGraph(new("https://fragments.dbpedia.org/2014/en"));
        foreach (var t in dbpedia.Triples.Take(1000))
            output.WriteLine("{0}", t);
    }

    [Fact(DisplayName = "First 1000 characters from Between Our Worlds", Skip = BecauseThisIsExampleCode)]
    public void Example2()
    {
        using var betweenOurWorlds = new TpfLiveGraph(new("https://data.betweenourworlds.org/latest"));

        var a = betweenOurWorlds.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfType));
        var character = betweenOurWorlds.CreateUriNode(UriFactory.Create("https://betweenourworlds.org/ontology/Character"));

        var characters = betweenOurWorlds.GetTriplesWithPredicateObject(a, character)
            .Select(c => c.Subject)
            .Take(1000);

        foreach (var c in characters)
            output.WriteLine("{0}", c);
    }

    [Fact(DisplayName = "SPARQL query over DBpedia TPF", Skip = BecauseThisIsExampleCode)]
    public void Example3()
    {
        const string sparql = """
            PREFIX yago: <http://dbpedia.org/class/yago/>
            PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

            SELECT ?label
            WHERE {
                ?standard
                    a yago:WorldWideWebConsortiumStandards ;
                    rdfs:label ?label .

                FILTER (LANG(?label) = 'en')
            }
            """;

        using var betweenOurWorlds = new TpfLiveGraph(new("https://fragments.dbpedia.org/2014/en"));
        foreach (var result in betweenOurWorlds.ExecuteQuery(sparql) as SparqlResultSet)
            output.WriteLine("{0}", result);
    }

    [Fact(DisplayName = "SPARQL inline select for paging", Skip = BecauseThisIsExampleCode)]
    public void Example4()
    {
        const string sparql = """
            PREFIX yago: <http://dbpedia.org/class/yago/>
            PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

            SELECT ?label
            WHERE {
            	{
            		SELECT * {
            			?standard a yago:WorldWideWebConsortiumStandards .
            		}
            		LIMIT 10
            	}

            	?standard rdfs:label ?label .
            	FILTER (LANG(?label) = 'en')
            }
            """;

        using var betweenOurWorlds = new TpfLiveGraph(new("https://fragments.dbpedia.org/2014/en"));
        foreach (var result in betweenOurWorlds.ExecuteQuery(sparql) as SparqlResultSet)
            output.WriteLine("{0}", result);
    }

    [Fact(DisplayName = "Caching with custom loader", Skip = BecauseThisIsExampleCode)]
    public void Example5()
    {
        using var dbpedia = new TpfLiveGraph(new("https://fragments.dbpedia.org/2014/en"), loader: new CachingLoader());
        foreach (var t in dbpedia.Triples.Take(1000)) output.WriteLine("First pass: {0}", t);
        foreach (var t in dbpedia.Triples.Take(1000)) output.WriteLine("Second pass: {0}", t);
    }

    // A naive implementation of a caching Loader.
    // Caution: Do not use in production!
    class CachingLoader() : Loader(new HttpClient(new CachingHandler()))
    {
        class CachingHandler : HttpClientHandler
        {
            private static readonly Dictionary<Uri, HttpResponseMessage> cache = [];

            protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
            {
                if (!cache.TryGetValue(request.RequestUri, out var cached))
                    cache.Add(request.RequestUri, cached = await Clone(await base.SendAsync(request, ct)));

                return await Clone(cached);
            }

            private async static Task<HttpResponseMessage> Clone(HttpResponseMessage original)
            {
                var stream = new MemoryStream();
                await original.Content.CopyToAsync(stream);

                stream.Position = 0;
                return new HttpResponseMessage(original.StatusCode) { Content = new StreamContent(stream) };
            }
        }
    }
}
