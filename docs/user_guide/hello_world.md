# Hello World with dotNetRDF

So now we're going to go ahead and build a simple Hello World application using dotNetRDF. We'll start with an outline class as follows. The rest of the code in this example will be placed inside the `Main()` method

```csharp

using System;
using System.Collections.Generic;
using VDS.RDF;
using VDS.RDF.Writing;

public class HelloWorld 
{
	public static void Main(String[] args) 
	{
		//Fill in the code shown on this page here to build your hello world application
	}
}
```

First thing we want to do is create a graph in which we do as follows:

```csharp
IGraph g = new Graph();
```

We're using the [`Graph`](xref:VDS.RDF.Graph) class here which is the most commonly used implementation of the [`IGraph`](xref:VDS.RDF.IGraph) interface.

Next we want to create some Nodes and assert some Triples in the Graph like so:

```csharp
IUriNode dotNetRDF = g.CreateUriNode(UriFactory.Create("http://www.dotnetrdf.org"));
IUriNode says = g.CreateUriNode(UriFactory.Create("http://example.org/says"));
ILiteralNode helloWorld = g.CreateLiteralNode("Hello World");
ILiteralNode bonjourMonde = g.CreateLiteralNode("Bonjour tout le Monde", "fr");

g.Assert(new Triple(dotNetRDF, says, helloWorld));
g.Assert(new Triple(dotNetRDF, says, bonjourMonde));
```

As noted in the Library Overview we use the [`UriFactory.Create()`](xref:VDS.RDF.UriFactory.Create(System.String)) method to create URIs since this will intern URIs for us which both reduces memory usage and speeds up equality comparisons.

Once we've done this we can output this to the console to see the Triples:

```csharp
foreach (Triple t in g.Triples) 
{
	Console.WriteLine(t.ToString());
}
Console.ReadLine();
```

This will give us output like the following:

```
http://www.dotnetrdf.org , http://example.org/says , Hello World
http://www.dotnetrdf.org , http://example.org/says , Bonjour tout le Monde@fr
```

Those of you who know RDF will notice that the above is not a valid RDF syntax - this is just an ultra simple representation of the Triples and is primarily intended for debugging. If we want to actually output RDF syntax then we need to use one of the classes from the [`VDS.RDF.Writing`](xref:VDS.RDF.Writing) namespace. First we'll output the above in NTriples syntax:

```csharp
NTriplesWriter ntwriter = new NTriplesWriter();
ntwriter.Save(g, "HelloWorld.nt");
```

This will save the contents of the Graph to the file `HelloWorld.nt` using NTriples syntax, this gives us output like the following:

```
<http://www.dotnetrdf.org> <http://example.org/says> "Hello World".
<http://www.dotnetrdf.org> <http://example.org/says> "Bonjour tout le Monde"@fr.
```

If we want to save the Graph to another RDF syntax we could do that as well e.g.

```csharp
RdfXmlWriter rdfxmlwriter = new RdfXmlWriter();
rdfxmlwriter.Save(g, "HelloWorld.rdf");
```

This will result in something like the following output:

```xml
<?xml version="1.0" encoding="utf-8"?>
<!DOCTYPE rdf:RDF[
	<!ENTITY rdf 'http://www.w3.org/1999/02/22-rdf-syntax-ns#'>
	<!ENTITY rdfs 'http://www.w3.org/2000/01/rdf-schema#'>
	<!ENTITY xsd 'http://www.w3.org/2001/XMLSchema#'>
]>
<rdf:RDF xmlns:rdf="&rdf;" xmlns:rdfs="&rdfs;" xmlns:xsd="&xsd;" xmlns:ns0="http://example.org/">
	<rdf:Description rdf:about="http://www.dotnetrdf.org/">
		<ns0:says>Hello World</ns0:says>
		<ns0:says xml:lang="fr">Bonjour tout le Monde</ns0:says>
	</rdf:Description>
</rdf:RDF>
```

Notice that the RDF/XML Writer already knew about the RDF, RDFS and XML Schema Namespaces. These Namespaces are provided by default for most implementations of [`IGraph`](xref:VDS.RDF.IGraph). The Writer also generates temporary namespaces of the form `nsX` for URIs that it otherwise couldn't represent in RDF/XML.

Here's the complete class:

```csharp
using System;
using System.Collections.Generic;
using VDS.RDF;
using VDS.RDF.Writing;

public class HelloWorld 
{
    public static void Main(String[] args) 
    {
	//Fill in the code shown on this page here to build your hello world application
        Graph g = new Graph();

        IUriNode dotNetRDF = g.CreateUriNode(UriFactory.Create("http://www.dotnetrdf.org"));
        IUriNode says = g.CreateUriNode(UriFactory.Create("http://example.org/says"));
        ILiteralNode helloWorld = g.CreateLiteralNode("Hello World");
        ILiteralNode bonjourMonde = g.CreateLiteralNode("Bonjour tout le Monde", "fr");

        g.Assert(new Triple(dotNetRDF, says, helloWorld));
        g.Assert(new Triple(dotNetRDF, says, bonjourMonde));

        foreach (Triple t in g.Triples)
        {
            Console.WriteLine(t.ToString());
        }

        NTriplesWriter ntwriter = new NTriplesWriter();
        ntwriter.Save(g, "HelloWorld.nt");

        RdfXmlWriter rdfxmlwriter = new RdfXmlWriter();
        rdfxmlwriter.Save(g, "HelloWorld.rdf");

        Console.ReadLine();
    }
}
```

## Compiler/Runtime Errors

If the simple example above gives you compiler/runtime errors then it is likely you haven't set up your environment correctly. Please refer to the [Getting Started](getting_started.md) document for more detail.

If you really can't get this working you can ask for [Support](/support.md) but please try the obvious things first.
