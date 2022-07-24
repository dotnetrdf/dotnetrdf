# Storage Transactions

The Transactions API provides some degree of control over Transactions when working with a store that supports these.
Supporting stores implement the [ITransactionalStorage](xref:VDS.RDF.Storage.ITransactionalStorage) and/or [IAsyncTransactionalStorage](xref:VDS.RDF.Storage.IAsyncTransactionalStorage) interfaces.

# Basic Usage 

It is important to note that individual implementations are free to decide whether their transactions are global or somehow scoped (e.g. per-thread).  Please consult the documentation of an implementation to determine which is the case.

These interfaces are fairly rudimentary and provide three simple methods:

## Begin() 

The [`Begin()`](xref:VDS.RDF.Storage.ITransactionalStorage.Begin) method starts a new transaction.

## Commit() 

The [`Commit()`](xref:VDS.RDF.Storage.ITransactionalStorage.Commit) method commits the current transaction.

## Rollback() 

The [`Rollback()`](xref:VDS.RDF.Storage.ITransactionalStorage.Rollback) method rolls back the current transaction.