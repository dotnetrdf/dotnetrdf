/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using Xunit;
using VDS.RDF.Parsing;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Writing;

public class OwlOneOf
{
    [Fact]
    public void WritingSerializeOwnOneOf()
    {
        //Create the Graph for the Test and Generate a List of URIs
        var g = new Graph();
        var nodes = new List<IUriNode>();
        for (var i = 1; i <= 10; i++)
        {
            nodes.Add(g.CreateUriNode(new Uri("http://example.org/Class" + i)));
        }

        //Use the thingOneOf to generate the Triples
        thingOneOf(g, nodes.ToArray());

        //Dump as NTriples to the Console
        var formatter = new NTriplesFormatter();
        foreach (Triple t in g.Triples)
        {
            Console.WriteLine(t.ToString(formatter));
        }

        Console.WriteLine();

        //Now try to save as RDF/XML
        IRdfWriter writer = new RdfXmlWriter();
        writer.Save(g, "owl-one-of.rdf");
        Console.WriteLine("Saved OK using RdfXmlWriter");
        Console.WriteLine();

        writer = new PrettyRdfXmlWriter();
        writer.Save(g, "owl-one-of-pretty.rdf");
        Console.WriteLine("Saved OK using PrettyRdfXmlWriter");
        Console.WriteLine();

        //Now check that the Graphs are all equivalent
        var h = new Graph();
        h.LoadFromFile("owl-one-of.rdf");
        Assert.Equal(g, h);
        Console.WriteLine("RdfXmlWriter serialization was OK");
        Console.WriteLine();

        var j = new Graph();
        j.LoadFromFile("owl-one-of-pretty.rdf");
        Assert.Equal(g, j);
        Console.WriteLine("PrettyRdfXmlWriter serialization was OK");
    }

    [Fact(Skip ="Extremely resource heavy")]
    public void WritingSerializeOwnOneOfVeryLarge()
    {
            //Create the Graph for the Test and Generate a List of URIs
            var g = new Graph();
            var nodes = new List<IUriNode>();
            for (var i = 1; i <= 10000; i++)
            {
                nodes.Add(g.CreateUriNode(new Uri("http://example.org/Class" + i)));
            }

            //Use the thingOneOf to generate the Triples
            thingOneOf(g, nodes.ToArray());

            //Dump as NTriples to the Console
            var formatter = new NTriplesFormatter();
            foreach (Triple t in g.Triples)
            {
                Console.WriteLine(t.ToString(formatter));
            }

            Console.WriteLine();

            //Now try to save as RDF/XML
            IRdfWriter writer = new RdfXmlWriter();
            writer.Save(g, "owl-one-of.rdf");
            
            Console.WriteLine("Saved OK using RdfXmlWriter");
            Console.WriteLine();

            writer = new PrettyRdfXmlWriter();
            ((ICompressingWriter)writer).CompressionLevel = WriterCompressionLevel.Medium;
            writer.Save(g, "owl-one-of-pretty.rdf");
            Console.WriteLine("Saved OK using PrettyRdfXmlWriter");
            Console.WriteLine();

            //Now check that the Graphs are all equivalent
            var h = new Graph();
            h.LoadFromFile("owl-one-of.rdf");
            Assert.Equal(g, h);
            Console.WriteLine("RdfXmlWriter serialization was OK");
            Console.WriteLine();

            var j = new Graph();
            j.LoadFromFile("owl-one-of-pretty.rdf");
            Assert.Equal(g, j);
            Console.WriteLine("PrettyRdfXmlWriter serialization was OK");
    }

    protected static void thingOneOf(IGraph graph, IUriNode[] listInds)
    {
        IBlankNode oneOfNode = graph.CreateBlankNode();
        IBlankNode chainA = graph.CreateBlankNode();
        IUriNode rdfType = graph.CreateUriNode(new Uri(RdfSpecsHelper.RdfType));
        IUriNode rdfFirst = graph.CreateUriNode(new Uri(RdfSpecsHelper.RdfListFirst));
        IUriNode rdfRest = graph.CreateUriNode(new Uri(RdfSpecsHelper.RdfListRest));
        IUriNode rdfNil = graph.CreateUriNode(new Uri(RdfSpecsHelper.RdfListNil));
        IUriNode owlClass = graph.CreateUriNode(new Uri(NamespaceMapper.OWL + "Class"));
        IUriNode owlOneOf = graph.CreateUriNode(new Uri(NamespaceMapper.OWL + "oneOf"));
        IUriNode owlThing = graph.CreateUriNode(new Uri(NamespaceMapper.OWL + "Thing"));
        IUriNode owlEquivClass = graph.CreateUriNode(new Uri(NamespaceMapper.OWL + "equivalentClass"));

        graph.Assert(new Triple(oneOfNode, rdfType, owlClass));
        graph.Assert(new Triple(oneOfNode, owlOneOf, chainA));
        graph.Assert(new Triple(owlThing, owlEquivClass, oneOfNode));

        for (var i = 0; i < listInds.Length; i++)
        {
            graph.Assert(new Triple(chainA, rdfFirst, listInds[i]));
            IBlankNode chainB = graph.CreateBlankNode();

            if (i < listInds.Length - 1)
            {
                graph.Assert(new Triple(chainA, rdfRest, chainB));
                chainA = chainB;
            }
        }
        graph.Assert(new Triple(chainA, rdfRest, rdfNil));
    }
}
