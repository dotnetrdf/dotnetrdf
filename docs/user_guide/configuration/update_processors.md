# Configuring Update Processors 

Update Processors are used to process SPARQL Update commands. Update Processors implement the [`ISparqlUpdateProcessor`](xref:VDS.RDF.Update.ISparqlUpdateProcessor) interface and the library provides 3 concrete implementations which can be configured using the Configuration API

# Basic Configuration 

Basic Configuration for a Update Processor looks like the following:

```turtle

@prefix dnr: <http://www.dotnetrdf.org/configuration#> .

_:proc a dnr:SparqlUpdateProcessor .
  dnr:type "VDS.RDF.Update.LeviathanUpdateProcessor" .
```

## Leviathan Update Processor 

The Leviathan Update Processor is used to process updates on in-memory stores using the library's Leviathan SPARQL Engine. It is configured quite simply by adding a dnr:usingStore property to the basic configuration, the object pointed to by this property must be a Triple Store which implements the [`IInMemoryQueryableStore`](xref:VDS.RDF.IInMemoryQueryableStore) interface e.g.

```turtle

@prefix dnr: <http://www.dotnetrdf.org/configuration#> .

_:proc a dnr:SparqlUpdateProcessor .
  dnr:type "VDS.RDF.Update.LeviathanUpdateProcessor" ;
  dnr:usingStore _:store .

_:store a dnr:TripleStore ;
  dnr:type "VDS.RDF.TripleStore" .
```

For information on how to configure Triple Stores see [Configuration API - Triple Stores](triple_stores.md).

Alternatively you may use the `dnr:usingDataset` property to connect it to a Dataset instead. See [Configuration API - Datasets](sparql_datasets.md) for details. If both `dnr:usingDataset` and `dnr:usingStore` are present then `dnr:usingDataset` has priority and the value for `dnr:usingStore` is ignored.

## Generic Update Processor 

The Generic Update Processor is used to process updates against some arbitrary store's SPARQL engine where the store you wish to connect to has an implementation of [`IStorageProvider`](xref:VDS.RDF.Storage.IStorageProvider).

How updates are actually processed depends on the exact concrete implementation of `IStorageProvider` being used, if the implementation also implements [`IUpdateableStorage`](xref:VDS.RDF.Storage.IUpdateableStorage) then the managers own SPARQL Update implementation is used. If this interface is not implemented then dotNetRDF will approximate SPARQL update implementation - in this case not all of SPARQL update may be supported or behave correctly depending on the capabilities of the `IStorageProvider` provided

To configure these handlers simply add a `dnr:storageProvider` property to the basic configuration like so:

```turtle

@prefix dnr: <http://www.dotnetrdf.org/configuration#> .

_:proc a dnr:SparqlUpdateProcessor .
  dnr:type "VDS.RDF.Update.GenericUpdateProcessor" ;
  dnr:genericManager _:manager .

_:manager a dnr:GenericIOManager ;
  dnr:type "VDS.RDF.Storage.AllegroGraphConnector" ;
  dnr:server "http://agraph.example.com" ;
  dnr:catalogID "catalog" ;
  dnr:storeID "store" .
```

The above specifies a Update Processor which applies the updates to the AllegroGraph repository store in the catalog catalog on the server `http://agraph.example.com`. See [Configuration API - Storage Providers](storage_providers.md) for more detail on configuring storage providers.

## Simple Update Processor 

Similar to the Generic Update Processor the Simple Update Processor passes updates to the `ExecuteUpdate()` method of a Triple Store that implements the [`IUpdateableTripleStore`](xref:VDS.RDF.IUpdateableTripleStore) interface. To configure this add a using Store property that points to a Triple Store that implements the relevant interface e.g.

```turtle

@prefix dnr: <http://www.dotnetrdf.org/configuration#> .

_:proc a dnr:SparqlUpdateProcessor .
  dnr:type "VDS.RDF.Update.SimpleUpdateProcessor" ;
  dnr:usingStore _:store .

_:store a dnr:TripleStore ;
  dnr:type "VDS.RDF.TripleStore" .
```

The above specifies a simple query processor that operates on an in-memory store. Note that both native and in-memory stores can be configured for use with this processor.