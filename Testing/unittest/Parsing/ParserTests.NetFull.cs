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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Net;
using Xunit;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Writing;
using VDS.RDF.XunitExtensions;
#pragma warning disable 618

namespace VDS.RDF.Parsing
{
    public partial class ParserTests
    {
        [Fact]
        public void ParsingBlankNodeIDs()
        {
            List<IRdfReader> parsersToTest = new List<IRdfReader>()
            {
                new TurtleParser(),
                new Notation3Parser()
            };

            String[] samples = new String[] {
                "@prefix ex: <http://example.org>. [] a ex:bNode. _:autos1 a ex:bNode. _:autos1 a ex:another.",
                "@prefix ex: <http://example.org>. _:autos1 a ex:bNode. [] a ex:bNode. _:autos1 a ex:another.",
                "@prefix : <http://example.org/>. [] a :BlankNode ; :firstProperty :a ; :secondProperty :b .",
                "@prefix : <http://example.org/>. (:first :second) a :Collection .",
                "@prefix : <http://example.org/>. [a :bNode ; :connectsTo [a :bNode ; :connectsTo []]] a [].",
                "@prefix : <http://example.org/>. [a :bNode ; :connectsTo [a :bNode ; :connectsTo []]] a []. [] a :another ; a [a :yetAnother] ."
            };

            int[] expectedTriples = new int[] {
                3,
                3,
                3,
                5,
                5,
                8
            };

            int[] expectedSubjects = new int[] {
                2,
                2,
                1,
                2,
                2,
                4
            };

            Console.WriteLine("Tests Blank Node ID assignment in Parsing and Serialization as well as Graph Equality");
            Console.WriteLine();

            List<IRdfWriter> writers = new List<IRdfWriter>() {
                new NTriplesWriter(),
                new TurtleWriter(),
                new CompressingTurtleWriter(),
                new Notation3Writer(),
                new RdfXmlWriter(),
                new PrettyRdfXmlWriter(),
                new RdfJsonWriter()
            };

            List<IRdfReader> readers = new List<IRdfReader>() {
                new NTriplesParser(),
                new TurtleParser(),
                new TurtleParser(),
                new Notation3Parser(),
                new RdfXmlParser(),
                new RdfXmlParser(),
                new RdfJsonParser()
            };

            foreach (IRdfReader parser in parsersToTest)
            {
                Console.WriteLine("Testing " + parser.GetType().ToString());
                //parser.TraceTokeniser = true;
                //parser.TraceParsing = true;

                int s = 0;
                foreach (String sample in samples)
                {
                    Console.WriteLine();
                    Console.WriteLine("Sample:");
                    Console.WriteLine(sample);
                    Console.WriteLine();

                    Graph g = new Graph();
                    VDS.RDF.Parsing.StringParser.Parse(g, sample, parser);
                    Console.WriteLine("Original Graph");
                    Console.WriteLine(g.Triples.Count + " Triples produced");
                    foreach (Triple t in g.Triples)
                    {
                        Console.WriteLine(t.ToString());
                    }
                    Console.WriteLine();

                    Assert.Equal(expectedTriples[s], g.Triples.Count);
                    Assert.Equal(expectedSubjects[s], g.Triples.SubjectNodes.Distinct().Count());

                    //Try outputting with each of the available writers
                    for (int i = 0; i < writers.Count; i++)
                    {
                        String temp = VDS.RDF.Writing.StringWriter.Write(g, writers[i]);
                        Graph h = new Graph();
                        VDS.RDF.Parsing.StringParser.Parse(h, temp, readers[i]);

                        Console.WriteLine("Trying " + writers[i].GetType().ToString());

                        Console.WriteLine("Graph after Serialization and Parsing");
                        Console.WriteLine(h.Triples.Count + " Triples produced");
                        foreach (Triple t in h.Triples)
                        {
                            Console.WriteLine(t.ToString());
                        }
                        Console.WriteLine();

                        if (expectedTriples[s] != h.Triples.Count || expectedSubjects[s] != h.Triples.SubjectNodes.Distinct().Count())
                        {
                            Console.WriteLine(writers[i].GetType().ToString() + " failed");
                            Console.WriteLine(temp);
                        }

                        Assert.Equal(expectedTriples[s], h.Triples.Count);
                        Assert.Equal(expectedSubjects[s], h.Triples.SubjectNodes.Distinct().Count());

                        //Do full equality Test
                        Dictionary<INode, INode> mapping;
                        bool equals = g.Equals(h, out mapping);
                        if (!equals)
                        {
                            Console.WriteLine(writers[i].GetType().ToString() + " failed");
                            Console.WriteLine(temp);
                        }
                        Assert.True(equals, "Graphs should be equal");
                        Console.WriteLine("Node Mapping was:");
                        foreach (KeyValuePair<INode, INode> pair in mapping)
                        {
                            Console.WriteLine(pair.Key.ToString() + " => " + pair.Value.ToString());
                        }
                        Console.WriteLine();
                    }
                    Console.WriteLine("All writers OK");
                    s++;
                }

                Console.WriteLine();
                Console.WriteLine();
            }
        }

        [Fact]
        public void ParsingCollections()
        {
            List<IRdfReader> parsersToTest = new List<IRdfReader>()
            {
                new TurtleParser(),
                new Notation3Parser()
            };

            String[] samples = new String[] {
                "@prefix ex: <http://example.com/>. (\"one\" \"two\") a ex:Collection .",
                "@prefix ex: <http://example.com/>. (\"one\" \"two\" \"three\") a ex:Collection .",
                "@prefix ex: <http://example.com/>. (1) ex:someProp \"Value\"."
            };

            int[] expectedTriples = new int[] {
                5,
                7,
                3
            };

            int[] expectedSubjects = new int[] {
                2,
                3,
                1
            };

            List<IRdfWriter> writers = new List<IRdfWriter>() {
                new NTriplesWriter(),
                new TurtleWriter(),
                new CompressingTurtleWriter(),
                new Notation3Writer(),
                new RdfXmlWriter(),
                new PrettyRdfXmlWriter(),
                new RdfJsonWriter()
            };

            List<IRdfReader> readers = new List<IRdfReader>() {
                new NTriplesParser(),
                new TurtleParser(),
                new TurtleParser(),
                new Notation3Parser(),
                new RdfXmlParser(),
                new RdfXmlParser(),
                new RdfJsonParser()
            };

            foreach (IRdfReader parser in parsersToTest)
            {
                Console.WriteLine("Testing " + parser.GetType().ToString());
                //parser.TraceTokeniser = true;
                //parser.TraceParsing = true;

                int s = 0;
                foreach (String sample in samples)
                {
                    Console.WriteLine();
                    Console.WriteLine("Sample:");
                    Console.WriteLine(sample);
                    Console.WriteLine();

                    Graph g = new Graph();
                    VDS.RDF.Parsing.StringParser.Parse(g, sample, parser);
                    Console.WriteLine(g.Triples.Count + " Triples produced");
                    foreach (Triple t in g.Triples)
                    {
                        Console.WriteLine(t.ToString());
                    }
                    Console.WriteLine();

                    Assert.Equal(expectedTriples[s], g.Triples.Count);
                    Assert.Equal(expectedSubjects[s], g.Triples.SubjectNodes.Distinct().Count());

                    //Try outputting with each of the available writers
                    for (int i = 0; i < writers.Count; i++)
                    {
                        Console.WriteLine("Trying " + writers[i].GetType().ToString());
                        String temp = VDS.RDF.Writing.StringWriter.Write(g, writers[i]);
                        Console.WriteLine(temp);
                        Graph h = new Graph();
                        try
                        {
                            VDS.RDF.Parsing.StringParser.Parse(h, temp, readers[i]);
                        }
                        catch (RdfParseException)
                        {
                            Console.WriteLine(temp);
                            throw;
                        }

                        Assert.Equal(expectedTriples[s], h.Triples.Count);
                        Assert.Equal(expectedSubjects[s], h.Triples.SubjectNodes.Distinct().Count());

                        Dictionary<INode, INode> mapping;
                        bool equals = g.Equals(h, out mapping);
                        Assert.True(equals, "Graphs should have been equal");
                        Console.WriteLine("Node mapping was:");
                        foreach (KeyValuePair<INode, INode> pair in mapping)
                        {
                            Console.WriteLine(pair.Key.ToString() + " => " + pair.Value.ToString());
                        }
                        Console.WriteLine();
                    }
                    Console.WriteLine("All writers OK");
                    s++;
                }

                Console.WriteLine();
                Console.WriteLine();
            }
        }
    }
}
