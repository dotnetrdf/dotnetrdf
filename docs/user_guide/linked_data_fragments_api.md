# Linked Data Fragments API 

## Introduction

[Linked Data Fragments](https://linkeddatafragments.org/) presents "new ways of publishing Linked Data, in which the query workload is distributed between clients and servers", an alternative to SPARQL endpoints and data dumps.

dotNetRDF features a client for Triple Pattern Fragments, a flavor of Linked Data Fragments.

## Basic Example 

The following example prints the first one thousand triples from the [DBpedia LDF endpoint](https://fragments.dbpedia.org/2014/en):

```csharp
using var dbpedia = new TpfLiveGraph(new("https://fragments.dbpedia.org/2014/en"));
foreach (var t in dbpedia.Triples.Take(1000))
	Console.WriteLine("{0}", t);
```

> [!NOTE]
> The example above results in 11 network calls:
> - One to obtain the TPF metadata from the endpoint.
> - Ten more to retrieve ten pages of data, a hundred items each.

> [!IMPORTANT]
> Repeating the code above (i.e. two subsequent iteration of the same first 1000 items) results in twice the network calls because there is no caching. You can use a custom [`Loader`](xref:VDS.RDF.Parsing.Loader) to implement caching.

> [!WARNING]
> An unrestricted iteration over _all_ triples of the DBpedia TPF graph above (e.g. `foreach (var t in dbpedia.Triples)`) would result in **thousands** of network calls.

## Programmatic _triple patterns_

The following example retrieves the first 1000 subjects that are instances of the `Character` class from the [Between Our Worlds](https://betweenourworlds.org/) TPF endpoint:

```csharp
using var betweenOurWorlds = new TpfLiveGraph(new("https://data.betweenourworlds.org/latest"));

var a = betweenOurWorlds.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfType));
var character = betweenOurWorlds.CreateUriNode(UriFactory.Create("https://betweenourworlds.org/ontology/Character"));

var characters = betweenOurWorlds.GetTriplesWithPredicateObject(a, character)
    .Select(c => c.Subject)
    .Take(1000);

foreach (var c in characters)
	Console.WriteLine("{0}", c);
```

> [!NOTE]
> Observe the similarity between the invocation of `GetTriplesWithPredicateObject` method above and the resulting TPF query:
> ```HTTP
> GET /2014/en?predicate=http%3A%2F%2Fwww.w3.org%2F1999%2F02%2F22-rdf-syntax-ns%23type&object=http%3A%2F%2Fdbpedia.org%2Fclass%2Fyago%2FWorldWideWebConsortiumStandards
> Host: fragments.dbpedia.org
> ```

> [!NOTE]
> See [Selecting Triples](working_with_graphs.md#selecting-triples) for more details on programmatically working with graphs.

## SPARQL querying

The following example uses SPARQL against the DBpedia TPF endpoint to find the English labels of W3C standards:

```csharp
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
foreach (var r in betweenOurWorlds.ExecuteQuery(sparql) as SparqlResultSet)
	Console.WriteLine("{0}", r);
```

> [!IMPORTANT]
> Due to implementation details of dotNetRDF's query engine, `LIMIT` statements in SPARQL queries are executed only once all relevant triples have been obtained from the TPF endpoint, so they cannot be used for restricting the number of network requests. In some circumstances you can achieve the desired effect by using limited inline selects.

> [!NOTE]
> See [Querying with SPARQL](querying_with_sparql.md) for more details.