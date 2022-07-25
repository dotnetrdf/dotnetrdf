# Full Text Querying with SPARQL 

Full Text SPARQL is a non-standard extension to our SPARQL engine provided in the separate **dotNetRDF.Query.FullText.dll** library. This library is included in the standard distribution from the 0.6.0 release onwards.

It uses [Lucene.Net](http://lucenenet.apache.org) to build and query full text indexes and allows you to leverage this capability directly from SPARQL queries. This document will guide you through the process of creating and querying a full text index.

# General Usage 

To use the library you'll need to add a reference to **dotNetRDF.Query.FullText.dll** (`dotNetRDF.Query.FullText` on NuGet) into your project (or install it via NuGet) and you should ensure that Lucene.Net is included in your project as well as this provides the actual indexing and query functionality.
Using NuGet is the preferred way to install since it will sort out dependencies and framework versions for you.

The majority of the classes provided by this library can be found in the `VDS.RDF.Query.FullText` namespace, the only other class you'll typically need is the [`FullTextOptimiser`](xref:VDS.RDF.Query.Optimisation.FullTextOptimiser) which is located in the [`VDS.RDF.Query.Optimisation`](xref:VDS.RDF.Query.Optimisation) namespace.

## Creating an Index 

Before you can perform full text queries you must first build an index from your RDF data. To do this you will use an instance of the [`IFullTextIndexer`](xref:VDS.RDF.Query.FullText.Indexing.IFullTextIndexer) interface, an indexer provides the means to index Triples, Graphs and Datasets and builds an index which relates the full text of literal objects to one of the nodes of each Triple.

Currently the following implementations are available:

| Indexer | Description |
| --- | --- |
| [LuceneSubjectsIndexer](xref:VDS.RDF.Query.FullText.Indexing.Lucene.LuceneSubjectsIndexer) | Relates the Subject of the Triple to the full text of the Literal Object |
| [LucenePredicatesIndexer](xref:VDS.RDF.Query.FullText.Indexing.Lucene.LucenePredicatesIndexer) | Relates the Predicate of the Triple to the full text of the Literal Object |
| [LuceneObjectsIndexer](xref:VDS.RDF.Query.FullText.Indexing.Lucene.LuceneObjectsIndexer) | Relates the Object of the Triple to its own full text |

So let's look at an example of building an index:

```csharp

using System;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Store;
using VDS.RDF;
using VDS.RDF.Query.FullText;
using VDS.RDF.Query.FullText.Indexing;
using VDS.RDF.Query.FullText.Indexing.Lucene;
using VDS.RDF.Query.FullText.Schema;

public class FullTextIndexingExample
{
	public static void Main(String[] args)
	{
		IFullTextIndexer indexer = null;
		try
		{
			//First get a Graph we want to Index
			Graph g = new Graph();
			g.LoadFromFile("example.ttl");

			//Then create an indexer and index the data
			indexer = new LuceneSubjectsIndexer(FSDirectory.Open("example"), new StandardAnalyzer(), new DefaultIndexSchema());
			indexer.Index(g);
		}
		catch (Exception ex)
		{
			//Handle any errors that occurred during Indexing
		}
		finally
		{
			//Always dispose of your index when it's built to ensure that indexed data is persisted to the index
			if (indexer != null) indexer.Dispose();
		}
	}
}
```

Note that when we created the indexer we passed in a Lucene.Net `Directory` and an `Analyzer` - you can use whatever implementations of these you like with our indexers. The [`DefaultIndexSchema`](xref:VDS.RDF.Query.FullText.Schema.DefaultIndexSchema) is a schema used to control how the indexed data is stored onto fields on the documents in the index, for most use cases you will only ever need to use this default implementation but you can implement your own if you are an advanced user.

## Querying an Index 

To query an index you use a [`IFullTextSearchProvider`](xref:VDS.RDF.Query.FullText.Search.IFullTextSearchProvider) instance, currently there is a single implementation [`LuceneSearchProvider`](xref:VDS.RDF.Query.FullText.Search.Lucene.LuceneSearchProvider). The following example demonstrates its usage:

```csharp

using System;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Store;
using VDS.RDF;
using VDS.RDF.Query.FullText;
using VDS.RDF.Query.FullText.Search;
using VDS.RDF.Query.FullText.Search.Lucene;
using VDS.RDF.Query.FullText.Schema;
using VDS.RDF.Writing.Formatting;

public class FullTextSearchExample
{
	public static void Main(String[] args)
	{
		//This example assumes we've already created our index in a folder called example

		IFullTextSearchProvider provider = null;
		try
		{
			//Get a Lucene Search Provider
			provider = new LuceneSearchProvider(Lucene.Util.Version.LUCENE_30, FSDirectory.Open("example"), new StandardAnalyzer(), new DefaultIndexSchema());

			//Use it to make a search and print the results
			NTriplesFormatter formatter = new NTriplesFormatter();
			foreach (IFullTextSearchResult result in provider.Match("text"))
			{
				Console.WriteLine("Node: " + result.Node.ToString(formatter) + " - Score: " + results.Score.ToString());
			}
		}
		catch (Exception ex)
		{
			//Handle any exceptions that occur during querying
		}
		finally
		{
			//Always dispose of a search provider when done as not doing so may cause problems with other code accesing your index
			if (provider != null) provider.Dispose();
		}
	}
}
```

As with our previous example the `LuceneSearchProvider` takes a Lucene.Net `Directory` and `Analyzer` plus a [`IFullTextIndexSchema`](xref:VDS.RDF.Query.FullText.Schema.IFullTextIndexSchema).

Note: This constructor allows you to omit either/both of the Analyzer or Schema, in this case this uses the default Lucene.Net `StandardAnalyzer` and/or the `DefaultIndexSchema`

# Full Text Querying with SPARQL 

So now that you've seen how to build and query an index programatically lets look at how you go about making a full text query via SPARQL. To do this you will need to create an instance of the `FullTextOptimiser` and attach it to your SPARQL Queries as an Algebra Optimiser.

The following example shows how to do this:

```csharp

using System;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Store;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.FullText;
using VDS.RDF.Query.FullText.Search;
using VDS.RDF.Query.FullText.Search.Lucene;
using VDS.RDF.Query.Optimisation;
using VDS.RDF.Writing.Formatting;

public class FullTextSparqlExample
{
	public static void Main(String[] args)
	{
		//This example assumes we've already created our index in a folder called example

		IFullTextSearchProvider provider = null;
		try
		{
			//Create our dataset
			InMemoryDataset dataset = new InMemoryDataset();
			//Assume we load it with data from somewhere...

			//Create and parse our query
			SparqlParameterizedString queryString = new SparqlParameterizedString();
			queryString.Namespaces.Add("pf", new Uri(FullTextHelper.FullTextMatchNamespace));
			queryString.CommandText = "SELECT * WHERE { ?match pf:textMatch 'text' }";

			SparqlQueryParser parser = new SparqlQueryParser();
			SparqlQuery query = parser.ParseFromString(queryString);

			//Get a Lucene Search Provider
			//For simplicity I've used the short constructor which assume StandardAnalyzer and DefaultIndexSchema
			provider = new LuceneSearchProvider(Lucene.Util.Version.LUCENE_29, FSDirectory.Open("example"));

			//Create the Full Text Optimiser and attach it to the query
			FullTextOptimiser optimiser = new FullTextOptimiser(provider);
			query.AlgebraOptimisers = new IAlgebraOptimiser[] { optimiser };

			//Now we can go ahead and run our query
			SparqlResultSet results = query.Evaluate(dataset) as SparqlResultSet;
			if (results != null)
			{
				NTriplesFormatter formatter = new NTriplesFormatter();
				foreach (SparqlResult result in results)
				{
					Console.WriteLine(result.ToString(formatter));
				}
			}
		}
		catch (Exception ex)
		{
			//Handle any exceptions that occur during querying
		}
		finally
		{
			//Always dispose of a search provider when done as not doing so may cause problems with other code accesing your index
			if (provider != null) provider.Dispose();
		}
	}
}
```

Those of you who may be familiar with [LARQ](http://jena.apache.org/documentation/larq/index.html) will notice that the query syntax for full text query is identical to that.

So you can do things like get scores for matches:

```sparql

# Get matches with scores
PREFIX pf: <http://jena.hpl.hp.com/ARQ/property#>

SELECT * WHERE { (?match ?score) pf:textMatch "text" . }
```

Or apply a limit on the results:

```sparql

# Get up to 10 matches
PREFIX pf: <http://jena.hpl.hp.com/ARQ/property#>

SELECT * WHERE { ?match pf:textMatch ( "text" 10) . }
```

Or apply a score threshold to the results:

```sparql

# Apply a Score Threshold of 0.75
PREFIX pf: <http://jena.hpl.hp.com/ARQ/property#>

SELECT * WHERE { ?match pf:textMatch ( "text" 0.75) . }
```

> [!NOTE]
> You can apply either a limit/threshold on their own, in this case a threshold must be a decimal/double while a limit must be an integer.
> If you wish to apply both a threshold and a limit the threshold is assumed to always appear first.
> This is in line with the LARQ syntax for full text query.

## Use with SPARQL Endpoints 

You can use Full Text Querying with SPARQL Endpoints by configuring it via the [Configuration API](configuration/index.md), see [Configuration API - Full Text Query](configuration/full_text_query.md) for more details.

# Keeping an index in sync with Datasets 

If your dataset is mutable then you may wish to keep your full text index in sync with your dataset as it changes. To do this you can use the [`FullTextIndexedDataset`](xref:VDS.RDF.Query.Datasets.FullTextIndexedDataset) which is a decorator that can be applied over another [`ISparqlDataset`](xref:VDS.RDF.Query.Datasets.ISparqlDataset) and will automatically keep your index in sync with changes made to the dataset.