# Sesame 

We support the [Sesame 2.0 HTTP Protocol](http://www.openrdf.org/doc/sesame2/system/ch08.html) which allows us to communicate with any [Sesame](http://www.openrdf.org) based store exposed via a Sesame HTTP server.  This means we can support Sesame's own store implementations as well as others such as [OWLIM](http://www.ontotext.com/owlim) and [BigData](http://www.systap.com/bigdata.htm) which may be exposed via Sesame.

Connectivity with Sesame is done via a number of classes:

| Provider | Description |
| --- | --- |
| [SesameHttpProtocolConnector](xref:VDS.RDF.Storage.SesameHttpProtocolConnector) | Connect to a Sesame server that uses the latest version of the Sesame protocol |
| [SesameHttpProtocolVersion6Connector](xref:VDS.RDF.Storage.SesameHttpProtocolVersion6Connector) | Connect to a Sesame server that uses Version 6 (the current version) of the Sesame protocol |
| [SesameHttpProtocolVersion5Connector](xref:VDS.RDF.Storage.SesameHttpProtocolVersion5Connector) | Connect to a Sesame server that uses Version 5 of the Sesame protocol.  Use this if you are using an older version of Sesame prior to the addition of SPARQL Update support. |

Typically you will most likely just use the `SesameHttpProtocolConnector` since that will always reflect the current version of the Sesame protocol.

## Supported Capabilities 

* Load, Save, Delete, Update and List Graphs
* SPARQL Query
* SPARQL Update (if using Version 6 connector or higher)

## Creating a Connection 

To create a connection to Sesame you need to know the Base URI of the server and the repository ID e.g.

```csharp

SesameHttpProtocolConnector sesame = new SesameHttpProtocolConnector("http://localhost:8080/openrdf-sesame/", "example");
```

If you are using HTTP authentication then you can optionally supply user credentials:

```csharp

SesameHttpProtocolConnector sesame = new SesameHttpProtocolConnector("http://localhost:8080/openrdf-sesame/", "example", "username", "password");
```

Additionally there are overloads for supplying a proxy server if necessary.

# Managing a Server 

We support managing a server via the [SesameServer](xref:VDS.RDF.Storage.Management.SesameServer) class which implements our [Servers API](storage_servers.md)

Managing a server allows you to manage a single catalog at a time, a Sesame connection provides access to its associated server via the `ParentServer` or `AsyncParentServer` property.

## Connecting to a Server 

You can also create connect directly to a server by creating an instance of the [SesameServer](xref:VDS.RDF.Storage.Management.SesameServer) class.  This requires you to know the Base URI of the server:

```csharp

SesameServer sesame = new SesameServer("http://localhost:8080/openrdf-sesame/");
```

## Creating Stores 

When creating a store the `SesameServer` will use one of the following templates:

| Template | Description |
| --- | --- |
| [SesameMemTemplate](xref:VDS.RDF.Storage.Management.Provisioning.Sesame.SesameMemTemplate) | Used to create Sesame memory stores |
| [SesameNativeTemplate](xref:VDS.RDF.Storage.Management.Provisioning.Sesame.SesameNativeTemplate) | Used to create Sesame native stores |
| [SesameHttpTemplate](xref:VDS.RDF.Storage.Management.Provisioning.Sesame.SesameHttpTemplate) | Used to create Sesame HTTP stores i.e. pointers to other remote Sesame repositories |

Each template has various properties for configuring store type specific settings.

