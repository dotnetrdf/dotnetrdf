# Allegro Graph 

dotNetRDF supports [Franz AllegroGraph](http://www.franz.com/agraph/) 3.x and 4.x via the [AllegroGraphConnector](xref:VDS.RDF.Storage.AllegroGraphConnector) class.

## Supported Capabilities 

Currently the connector supports the following capabilities:

* Load, Save, Update, Delete and List Graphs
* SPARQL Query
* SPARQL Update (if using dotNetRDF 1.0.0 and higher)

## Creating a Connection 

To connect to Allegro Graph you will need to know the Base URI of your server, the catalog and store ID.  Optionally you can also provide HTTP authentication credentials and a HTTP proxy.

For 3.x you can create a connection like so:

```csharp

//Create an unauthenticated connection
AllegroGraphConnector agraph = new AllegroGraphConnector("http://localhost:9875", "catalog", "store");
```

Or provide authentication credentials like so:

```csharp

//Create an authenticated connection
AllegroGraphConnector agraph = new AllegroGraphConnector("http://localhost:9875", "catalog", "store", "user", "password");

```

The 4.x release introduced the notion of a root catalog which allows you to omit the Catalog ID for stores in this catalog, you can either set the catalog to `null` or use the constructors which omit the argument e.g.

```csharp

//Create an unauthenticated connection to a store in the root catalog
AllegroGraphConnector agraph = new AllegroGraphConnector("http://localhost:9875",  "store");
```

# Managing a Server 

We support managing a server via the [AllegroGraphServer](xref:VDS.RDF.Storage.Management.AllegroGraphServer) class which implements our [Servers API](storage_servers.md)

Managing a server allows you to manage a single catalog at a time, a `AllegroGraphConnector` provides access to its associated server via the `ParentServer` or `AsyncParentServer` property.

## Connecting to a Server 

You can also create connect directly to a server by creating an instance of the [AllegroGraphServer](xref:VDS.RDF.Storage.Management.AllegroGraphServer) class.  This requires you to know the Base URI and catalog ID you want to manage.

For 3.x you will always need the catalog ID:

```csharp

//Create an unauthenticated connection
AllegroGraphServer agraph = new AllegroGraphServer("http://localhost:9875", "catalog");
```

For 4.x you can manage the root catalog like so:

```csharp

//Create an unauthenticated connection to the root catalog
AllegroGraphServer agraph = new AllegroGraphServer("http://localhost:9875");
```

## Creating Stores 

When creating a store the `AllegroGraphServer` will provide only simple [StoreTemplate](xref:VDS.RDF.Storage.Management.Provisioning.StoreTemplate) instances as templates.
No extra settings may currently be configured when creating stores on a Allegro Graph server.