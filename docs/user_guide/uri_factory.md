# URI Factory

Working with RDF data can involve a lot of URI manipulation.
Fortunately .NET provides us with a lot of "out of the box" support for URIs in the form of the `System.Uri` class.
However, this comes at the cost of some memory overhead, particularly when instantiating a lot of duplicate URIs as can often be the case when parsing or processing RDF.
The [`IUriFactory`](xref:VDS.RDF.IUriFactory) interface and [`CachingUriFactory`](xref:VDS.RDF.CachingUriFactory) class have been designed to try to reduce this overhead and help reduce the overall memory footprint of RDF processing at the cost of a little extra run-time lookup.

A URI factory is effectively a cache. When you use the factory to create a URI that URI is cached by the factory. When you call the factory to create a URI it already has in the cache, the factory returns the cached URI rather than create a new instance. This process is also referred to as 'interning URIs'.

Of course, over time the URI factory itself gathers up lots of URIs which may no longer be in use. 
Rather than attempt to define complex cache invalidation strategies, the [`CachingUriFactory`](xref:VDS.RDF.CachingUriFactory) class is instead designed to be hierarchically composable.
A new `CachingUriFactory` instance can be created with a parent `IUriFactory`. 
When asked to create a URI, the factory now checks if it can satisfy the request from its own cache.
If not, it then checks if its parent can provide the URI from its cache (and so on up the chain).
If any of the factories in the chain have the URI cached, that URI is returned with no further processing.
If the URI is nor present in any cache, the factory that was originally requested to make the URI will create it and add it to its own cache without affecting the caches of the factories higher up in the chain.
In this way you can make a nest of scoped URI factories. This can be particularly useful in stateless applications such as web servers as it allows you to scope new URI creation to each individual request while still sharing a set of session or application level URIs.

## IUriFactory

The API for using a factory is defined by the [`IUriFactory`](xref:VDS.RDF.IUriFactory) interface.
The method [`Create(string)`](xref:VDS.RDF.IUriFactory.Create(System.String)) is used to create a new URI, and the method [`Create(Uri, string)`](xref:VDS.RDF.IUriFactory.Create(System.Uri,System.String)) is used to create a new URI by resolving the string value against the specified base URI.

To check if a factory has a specific URI in its cache, use the [`TryGetUri`](xref:VDS.RDF.IUriFactory.TryGetUri(System.String,System.Uri@)) method.

To disable/enable the caching of URIs, you can set the [`InternUris`](xref:VDS.RDF.IUriFactory.InternUris) property. This might sometimes be useful if you expect a process to be about to create a large number of short-lived and unique URIs.

To clear the current cache, you can use the [`Clear()`](xref:VDS.RDF.IUriFactory.Clear) method.

## UriFactory

The [UriFactory](xref:VDS.RDF.UriFactory) static class is provided primarily to provide backwards compatibility with code written for dotNetRDF 2.x and older. It wraps a statically defined [`Root`](xref:VDS.RDF.UriFactory.Root) URI factory and provides a static implementation of the basic `Create(string)` method that creates URIs using that root factory.

> [!NOTE]
> In dotNetRDF 3.0, many class constructors and some methods have been updated to allow an `IUriFactory` instance to be injected. Typically those arguments have been made optional to preserve backwards compatibility and will default to using the root factory defined by [`UriFactory.Root`](xref:VDS.RDF.UriFactory.Root)

## CachingUriFactory

The [CachingUriFactory](xref:VDS.RDF.UriFactory) class is the default implementation of [`IUriFactory`](xref:VDS.RDF.IUriFactory) provided by the library. It implements an in-memory cache and the hierarchical composition strategy described above.