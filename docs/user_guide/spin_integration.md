# SPIN integration

The [`dotNetRdf.Query.Spin`](https://www.nuget.org/packages/dotNetRdf.Query.Spin)
package provides [`SpinWrappedDataset`](xref:VDS.RDF.Query.Spin.SpinWrappedDataset)
over an `IUpdateableStorage` provider. Its public
`ExecuteQuery(string)` and `ExecuteUpdate(string)` methods take SPARQL text.
The package has no public entry point that takes a SPIN RDF command node and
its graph. It also has no public operation to run `spin:rule` declarations or
check `spin:constraint` declarations explicitly.

## Run a stored SPARQL query on local data

A [SPIN query resource](https://www.w3.org/submissions/spin-sparql/#sp-Queries)
can carry the original SPARQL query as an optional `sp:text` literal. If you
already have that literal, you can pass its text to the core SPARQL parser and
run it against an application-owned in-memory dataset. Supply any prefix and
base declarations the query text needs. This example assumes that you trust the query and have
verified that it uses local data only, without a `SERVICE` clause.

```csharp
using System.Linq;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Datasets;

static object QueryLocalGraph(IGraph commands, INode command, InMemoryDataset data)
{
    IUriNode textProperty = commands.CreateUriNode(
        commands.UriFactory.Create("http://spinrdf.org/sp#text"));
    var text = (ILiteralNode)commands.GetTriplesWithSubjectPredicate(command, textProperty)
        .Single().Object;

    SparqlQuery query = new SparqlQueryParser().ParseFromString(text.Value);
    using var processor = new LeviathanQueryProcessor(data);
    return processor.ProcessQuery(query);
}
```

This example requires exactly one `sp:text` literal. If a command is supplied
only as structured SPIN RDF, the package provides no public API to convert it
to SPARQL text. The core engine returns the Boolean result of an `ASK` query
or the graph produced by a `CONSTRUCT` query. The caller still needs a SPIN
engine to discover and apply rules or check constraints.

## Graph imports and offline use

`SpinWrappedDataset` can resolve class-library graphs by URI during
initialization. A local storage provider alone cannot prevent that
resolution. There is no public option to restrict class-library resolution
to caller-supplied graphs. The local query example above uses
`LeviathanQueryProcessor` directly and does not initialize the SPIN
class-library loader.
