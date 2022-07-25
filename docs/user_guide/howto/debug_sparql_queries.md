# Debugging SPARQL Queries 

## In-Memory Queries 

If you are using our in-memory SPARQL engine then you can attempt to debug why your queries are not working by using the [ExplainQueryProcessor](xref:VDS.RDF.Query.ExplainQueryProcessor) to try and figure out what is happening with your queries.

It is important to understand that explaining query evaluation will **substantially** impact performance so if you are trying to debug performance issues this is not the way to do so.

You can create this like so:

```csharp
//Assuming you already have a dataset you wish to use in the variable ds
ISparqlQueryProcessor processor = new ExplainQueryProcessor(ds, ExplanationLevel.Full);

```

Once you have the processor you run your query like you would normally with a query processor i.e.

```csharp
//Assuming you have a query ready to run in variable q
Object results = processor.ProcessQuery(q);
```

This will result in your query being evaluated but also explanations of the query level being printed to Console Standard Output and Debug Output.  These explanations will include useful information like how many results were found at each stage of the query and can help you see why you have no results

## Remote/Native Queries 

Debugging queries against remote endpoints or native SPARQL provided by a [IQueryableStorage](xref:VDS.RDF.Storage.IQueryableStorage) is somewhat more difficult.

For `IQueryableStorage` check the documentation of the store you are using to see if they provide any query debugging tools that you can use.

If not the best way to debug queries is to follow the following method:

1. Break your query down into its constituent parts i.e. individual triple patterns
1. Run each triple pattern to verify that it will return results
1. Start to build your query back up in small chunks i.e. individual graph patterns
1. Run each graph pattern to verify that it will return results
1. Start adding back in the `FILTER` and joins to your query
1. Run each form of the query to verify that it will return results

The general idea is to verify whether the blocks of your query actually return results and identify what block(s) are preventing your query from returning a result.  Once you have done this you can start to adjust your query to produce some results.