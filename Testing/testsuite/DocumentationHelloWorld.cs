using System;
using System.Collections.Generic;
using VDS.RDF;
using VDS.RDF.Writing;

namespace dotNetRDFTest 
{

public class HelloWorld 
{

    public static void Main(String[] args) 
    {
	    //Fill in the code shown on this page here to build your hello world application
        Graph g = new Graph();

        IUriNode dotNetRDF = g.CreateUriNode(new Uri("http://www.dotnetrdf.org"));
        IUriNode says = g.CreateUriNode(new Uri("http://example.org/says"));
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

    }
}
}

