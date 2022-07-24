# SPARQL Datasets 

A SPARQL dataset refers to the dataset notion from the SPARQL specification, within dotNetRDF it may refer to the concrete [`ISparqlDataset`](xref:VDS.RDF.Query.Datasets.ISparqlDataset) interface which represents datasets for in-memory queries or it may refer to the dataset description associated with a query/update.

## Dataset Description 

The dataset for a SPARQL Query or Update consists of a default graph and zero or more named graphs, the default graph is often also referred to as the unnamed graph.

SPARQL does not mandate what the default graph is so this can and often is dataset specific, see the later sections of this page for how to construct different types of dataset for use with the libraries in-memory SPARLQ engine.

### Query Datasets

The dataset description of a query consists of the `FROM` and `FROM NAMED` clauses present in the query, these indicate to a query engine which graph(s) to use and where to use them when answering queries.

Graphs specified in the `FROM` clause are used to form the default graph, this is the graph that queries operate over **except** when a `GRAPH` clause is encountered.

Graphs specified in the `FROM NAMED` clauses are named graphs that may be accessed using a `GRAPH` clause in your query.

### Update Datasets

The `INSERT/DELETE` command in SPARQL Update allows the dataset to be specified in several ways:

* The `WITH` clause specifies a default graph for the `INSERT` and `DELETE` portions of the query
* The `USING` clauses specifies the graphs that form the default graph for the `WHERE` portion of the update.  If there are no `USING` clauses the `WITH` clause specifies the default graph, if there is no `WITH` then the default graph is dataset specific.
* The `USING NAMED` clause specifies the named graphs for the `WHERE` portion of the update.

## ISparqlDataset 

The [`ISparqlDataset`](xref:VDS.RDF.Query.Datasets.ISparqlDataset) interface is used to represent a dataset over which queries and updates are applied.
dotNetRDF includes a number of implementations of this primarily for wrapping a [`IInMemoryQueryableStore`](xref:VDS.RDF.IInMemoryQueryableStore) i.e. an in-memory [`ITripleStore`](xref:VDS.RDF.ITripleStore) so that it can be queried.

How you construct your dataset can affect the results of your query due to SPARQL dataset descriptions as already discussed on this page.
For our examples here we will use the [`InMemoryDataset`](xref:VDS.RDF.Query.Datasets.InMemoryDataset) which may be constructed in a number of ways.

If your queries involve a lot of named graphs then you may get better performance by using an [`InMemoryQuadDataset`](xref:VDS.RDF.Query.Datasets.InMemoryQuadDataset) instead. 
This has identical constructors to the ones shown in the following examples.

### Default Behavior 

The default behavior of dotNetRDF is to treat the unnamed graph as the default graph and all other graphs as named graphs.  You get this type of dataset when constructing like so:

```csharp

//Construct a fresh dataset
ISparqlDataset ds = new InMemoryDataset();
```

Or if you construct from an existing triple store:

```csharp

TripleStore store = new TripleStore();

//Assume it gets filled with data from somewhere...

//Construct a dataset
ISparqlDataset ds = new InMemoryDataset(store);
```

### Specific Default Graph 

If you have a specific graph that you wish to have treated as the default graph you can create a dataset like so:

```csharp

TripleStore store = new TripleStore();

//Assume it gets filled with data from somewhere

//Construct a dataset using a specific graph as the default
ISparqlDataset ds = new InMemoryDataset(store, new Uri("http://example.org/default-graph"));
```

### Union Default Graph 

Union default graph is a special behavior whereby the default graphs acts as if it were the union of all the graphs in the store, it may be constructed like so:

```csharp

TripleStore store = new TripleStore();

//Assume it gets filled with data from somewhere

//Construct a dataset using a union default graph
ISparqlDataset ds = new InMemoryDataset(store, true);
```