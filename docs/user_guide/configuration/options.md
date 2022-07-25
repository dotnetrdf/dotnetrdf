# Configuring Static Options 

The Configuration API includes the capability to automatically configure any static property of any class provided the .Net type of that property is one of the following:

* `Int32`
* `Int64`
* `Boolean`
* `String`
* `Uri`
* An Enum

Static options may be automatically configured when using the `LoadConfiguration()` methods with the `autoConfigure` argument set to `true` or by calling `AutoConfigureStaticOptions(IGraph g)`.  This is useful when you want to ensure that some static options are always set in your environment.

Configuration for static options is defined like so:

```turtle

@prefix dnr: <http://www.dotnetrdf.org/configuration#> .

<dotnetrdf-configure:VDS.RDF.Options#UsePLinqEvaluation> dnr:configure false .
```

The above example will turn off PLINQ Evaluation for queries.

Note that the subject must use a special URI of the form `<dotnetrdf-configure:Class#Property>` and the predicate must be `dnr:configure`
