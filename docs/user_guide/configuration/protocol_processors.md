# Configuring Protocol Processors 

Protocol Processors are classes that can process SPARQL Graph Store HTTP Protocol requests and return appropriate responses. Protocol Processors implement the [`ISparqlHttpProtocolProcessor`](xref:VDS.RDF.Update.Protocol.ISparqlHttpProtocolProcessor) interface and the library provides 3 concrete implementations of this all of which can be configured using the Configuration API.

# Basic Configuration 

Basic Configuration for a Protocol Processor looks like the following:

```turtle

@prefix dnr: <http://www.dotnetrdf.org/configuration#> .

_:proc a dnr:SparqlHttpProtocolProcessor .
  dnr:type "VDS.RDF.Update.Protocol.LeviathanProtocolProcessor" .
```

## Leviathan Protocol Processor 

The Leviathan Protocol Processor is used to process protocol requests on in-memory stores using the library's Leviathan SPARQL Engine. It is configured quite simply by adding a `dnr:usingStore` property to the basic configuration, the object pointed to by this property must be a Triple Store which implements the [`IInMemoryQueryableStore`](xref:VDS.RDF.IInMemoryQueryableStore) interface e.g.

```turtle

@prefix dnr: <http://www.dotnetrdf.org/configuration#> .

_:proc a dnr:SparqlHttpProtocolProcessor .
  dnr:type "VDS.RDF.Update.Protocol.LeviathanProtocolProcessor" ;
  dnr:usingStore _:store .

_:store a dnr:TripleStore ;
  dnr:type "VDS.RDF.TripleStore" .
```

For information on how to configure Triple Stores see [Configuration API - Triple Stores](triple_stores.md).

Alternatively you may use the `dnr:usingDataset` property to connect it to a Dataset instead. See [Configuration API - Datasets](sparql_datasets.md) for details. If both `dnr:usingDataset` and `dnr:usingStore` are present then `dnr:usingDataset` has priority and the value of `dnr:usingStore` is ignored.

## Generic Protocol Processor 

The Generic Protocol Processor is used to process protocol requests against some arbitrary store where the store you wish to connect to has an implementation of [`IStorageProvider`](xref:VDS.RDF.Storage.IStorageProvider).

Not all features of the protocol may be supported or behave correctly depending on the capabilities of the `IStorageProvider` used.

To configure these handlers simply add a `dnr:storageProvider` property to the basic configuration like so:

```turtle

@prefix dnr: <http://www.dotnetrdf.org/configuration#> .

_:proc a dnr:SparqlHttpProtocolProcessor .
  dnr:type "VDS.RDF.Update.Protocol.GenericProtocolProcessor" ;
  dnr:storageProvider _:manager .

_:manager a dnr:StorageProvider ;
  dnr:type "VDS.RDF.Storage.VirtuosoManager" ;
  dnr:server "http://virtuoso.example.com" ;
  dnr:user "username" ;
  dnr:password "password" .
```

The above configures a Generic Protocol Processor which processes requests using a Virtuoso quad store.

## Protocol to Update Processor 

The Protocol to Update Processor is a processor which operates using the supplied Query and Update processors, see [Configuration API - Query Processors](query_processors.md) and [Configuration API - Update Processors](update_processors.md) for details on configuring these.

```turtle

@prefix dnr: <http://www.dotnetrdf.org/configuration#> .

_:proc a dnr:SparqlHttpProtocolProcessor .
  dnr:type "VDS.RDF.Update.Protocol.ProtocolToUpdateProcessor" ;
  dnr:queryProcessor _:qProc ;
  dnr:updateProcessor _:uProc .

_:qProc a dnr:SparqlQueryProcessor ;
  dnr:type "VDS.RDF.Query.LeviathanQueryProcessor" ;
  dnr:usingStore _:store .

_:uProc a dnr:SparqlUpdateProcessor ;
  dnr:type "VDS.RDF.Update.LeviathanUpdateProcessor" ;
  dnr:usingStore _:store .

_:store a dnr:TripleStore ;
  dnr:type "VDS.RDF.TripleStore" .
```

Note that the above is in effect identical to the example given for the Leviathan Protocol Processor but much more complex configurations are possible than with the plain Leviathan processor.