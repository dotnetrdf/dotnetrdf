# Debugging HTTP Communication 

Various features of dotNetRDF rely on HTTP communication to provide functionality, most of the time this works fine but occasionally you may encounter errors.  dotNetRDF provides some helpful functionality to help you debug these errors.

To get basic traces of HTTP requests and responses printed to the Console you can enable the global static option `HttpDebugging` found in the [Options](xref:VDS.RDF.Options) class e.g.

```csharp
Options.HttpDebugging = true;
```

## Full Response Traces 

While most of the time the above will be sufficient to diagnose a problem sometimes you may need to see the full HTTP response in order to see exactly what error messages the remote server is responding with.

To get full response traces for HTTP responses you must have enabled the `HttpDebugging` property and then also enable the `HttpFullDebugging` property e.g.

```csharp
Options.HttpDebugging = true;
Options.HttpFullDebugging = true;
```

**Note:** When full debugging is used the HTTP response stream is consumed, this may cause dotNetRDF to throw different errors to those normally seen because the stream the code expects has already been consumed.