# Configuring Object Factories 

Object Factories are classes that are used to extend the [`ConfigurationLoader`](xref:VDS.RDF.Configuration.ConfigurationLoader) so it can load more complex or user defined objects. These classes must implement the [`IObjectFactory`](xref:VDS.RDF.Configuration.IObjectFactory) interface and have a public unparameterized constructor. They can be configured as follows:

```csharp

@prefix dnr: <http://www.dotnetrdf.org/configuration#> .

_:factory a dnr:ObjectFactory ;
  dnr:type "YourNamespace.YourType, YourAssembly" .
```

This configures an object factory from an external assembly. Object factories can be registered as described on the [Configuration API](index.md) page.

> [!NOTE]
> When a configuration graph is used in an ASP.Net application the `AutoConfigureObjectFactories()` method will be called automatically before attempting to load configuration so any custom objects you define will be loadable.