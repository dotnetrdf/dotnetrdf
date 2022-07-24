# Formatting API 

The Formatting API is an collection of APIs found in the [`VDS.RDF.Writing.Formatting`](xref:VDS.RDF.Writing.Formatting) namespace, it is concerned with turning objects like nodes, triples and SPARQL results into strings for display.  The formatting API underpins the writers already seen in the basic tutorial on the [Writing RDF](writing_rdf.md) page.

The API consists of a number of interfaces:

| Interface | Formatting Capabilities |
| --- | --- |
| [`ICharFormatter`](xref:VDS.RDF.Writing.Formatting.ICharFormatter) | Formats individual characters |
| [`IUriFormatter`](xref:VDS.RDF.Writing.Formatting.IUriFormatter) | Formats URIs |
| [`IBaseUriFormatter`](xref:VDS.RDF.Writing.Formatting.IBaseUriFormatter) | Formats Base URI declarations |
| [`INamespaceFormatter`](xref:VDS.RDF.Writing.Formatting.INamespaceFormatter) | Formats namespace declarations |
| [`INodeFormatter`](xref:VDS.RDF.Writing.Formatting.INodeFormatter) | Formats [INode](xref:VDS.RDF.INode) instances |
| [`ITripleFormatter`](xref:VDS.RDF.Writing.Formatting.ITripleFormatter) | Formats [Triple](xref:VDS.RDF.Triple) instances |
| [`IResultFormatter`](xref:VDS.RDF.Writing.Formatting.IResultFormatter) | Formats [SparqlResult](xref:VDS.RDF.Query.SparqlResult) instances |

# Basic Usage 

Generally you will only want to use one of the higher level interfaces such as [`INodeFormatter`](xref:VDS.RDF.Writing.Formatting.INodeFormatter) or [`ITripleFormatter`](xref:VDS.RDF.Writing.Formatting.ITripleFormatter).  Both these interfaces define `Format(…)` methods which take either a `Triple` or an `INode` and return a string representation of them. You can also call `ToString(…)` overloads on `Triple` and `INode` which take in a formatter and return the String representation as formatted by that formatter.

In general any formatter usually provides one or more `Format()` or `FormatX()` methods which are used to format specific things.  These methods take the thing to be formatted and return a string.

## Example 1 

For example we can format specific nodes:

```csharp

//Assumes that we already have a Graph in the variable g
NTriplesFormatter formatter = new NTriplesFormatter();

//Want to get only the triples defining types - assumes rdf: prefix is appropriately defined for this Graph
UriNode rdfType = g.CreateUriNode("rdf:type");

//This prints only the subjects of the Triples we find with
//the predicate rdf:type using NTriples formatting
foreach (Triple t in g.GetTriplesWithPredicate(rdfType))
{
	Console.WriteLine(t.Subject.ToString(formatter));
}
```

## Standard Implementations 

Currently the library has the following formatters available but you can easily define your own:

| Formatter | Format Produced |
| --- | --- |
| [`CsvFormatter`](xref:VDS.RDF.Writing.Formatting.CsvFormatter) | CSV |
| [`Notation3Formatter`](xref:VDS.RDF.Writing.Formatting.Notation3Formatter) | Notation 3 |
| [`NQuadsFormatter`](xref:VDS.RDF.Writing.Formatting.NQuadsFormatter) | NQuads |
| [`NTriplesFormatter`](xref:VDS.RDF.Writing.Formatting.NTriplesFormatter) | NTriples |
| [`SparqlFormatter`](xref:VDS.RDF.Writing.Formatting.SparqlFormatter) | SPARQL style, can also format queries |
| [`TsvFormatter`](xref:VDS.RDF.Writing.Formatting.TsvFormatter) | TSV |
| [`TurtleFormatter`](xref:VDS.RDF.Writing.Formatting.TurtleFormatter) | Turtle |
| [`UncompressedNotation3Formatter`](xref:VDS.RDF.Writing.Formatting.UncompressedNotation3Formatter) | Uncompressed Notation 3 |
| [`UncompressedTurtleFormatter`](xref:VDS.RDF.Writing.Formatting.UncompressedTurtleFormatter) | Uncompressed Turtle |

## Example 2 

Here's another example of formatting Triples for display on the console:

```csharp

using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF;
using VDS.RDF.Writing.Formatting;

public class FormattingTriplesExample
{
  public static void Main(String[] args)
  {
    IGraph g = new Graph();

    //Assume we fill our graph with data from somewhere...

    //Create a formatter
    ITripleFormatter formatter = new TurtleFormatter(g);

    //Print triples with this formatter
    foreach (Triple t in g.Triples)
    {
      Console.WriteLine(t.ToString(formatter));
    }
  }
}
```