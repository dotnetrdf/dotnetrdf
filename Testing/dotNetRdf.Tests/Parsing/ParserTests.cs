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
using System.IO;
using System.Linq;
using System.Net;
using Xunit;
using VDS.RDF.Query;
using VDS.RDF.Writing;

namespace VDS.RDF.Parsing;

public class ParserTests
{
    [Fact]
    public void ParsingStringParser()
    {
            String[] someRDF = { "<http://example.org/subject> <http://example.org/predicate> <http://example.org/object>.",
                                 "@prefix : <http://example.org/>.:subject :predicate :object.",
                                 "@prefix : <http://example.org/>.@keywords.subject predicate object.",
                                 "@prefix : <http://example.org/>. {:subject :predicate :object}.",
                                 "<?xml version=\"1.0\"?><rdf:RDF xmlns=\"http://example.org/\" xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\"><rdf:Description rdf:about=\"http://example.org/subject\"><predicate rdf:resource=\"http://example.org/object\" /></rdf:Description></rdf:RDF>",
                                 "{ \"http://example.org/subject\" : { \"http://example.org/predicate\" : [ { \"type\" : \"uri\", \"value\" : \"http://example.org/object\" } ] } }",
                                 "some random junk which isn't RDF at all",
                               };

            bool[] parseExpected = { true, true, true, false, true, true, false };

            Console.WriteLine("Testing the StringParser with a bunch of strings which are either invalid RDF or all express the one same simple Triple");
            Console.WriteLine();

            var g = new Graph();
            for (var i = 0; i < someRDF.Length; i++)
            {
                try
                {
                    var rdf = someRDF[i];
                    Console.WriteLine("# Trying to parse the following");
                    Console.WriteLine(rdf);

                    if (parseExpected[i])
                    {
                        Console.WriteLine("# Expected Result = Parsed OK");
                    }
                    else
                    {
                        Console.WriteLine("# Expected Result = Parse Fails");
                    }

                    //Parse
                    VDS.RDF.Parsing.StringParser.Parse(g, rdf);

                    if (!parseExpected[i])
                    {
                        Assert.Fail("Expected Parsing to Fail but succeeded");
                    }
                }
                catch (RdfParseException parseEx)
                {
                    if (!parseExpected[i])
                    {
                        TestTools.ReportError("RDF Parsing Error", parseEx);
                    }  
                    else
                    {
                        throw;
                    }
                }
                finally
                {
                    Console.WriteLine();
                }
            }

            Console.WriteLine("# Final Graph Contents");
            foreach (Triple t in g.Triples)
            {
                Console.WriteLine(t.ToString());
            }
    }

    [Fact]
    public void ParsingRdfXmlNamespaceAttributes()
    {
        Assert.SkipUnless(TestConfigManager.GetSettingAsBoolean(TestConfigManager.UseRemoteParsing),
            "Test Config marks Remote Parsing as unavailable, test cannot be run");

        var g = new Graph();
        var request = (HttpWebRequest)WebRequest.Create("http://dbpedia.org/resource/Southampton");
        request.Method = "GET";
        request.Accept = MimeTypesHelper.HttpAcceptHeader;

        var response = (HttpWebResponse)request.GetResponse();
        IRdfReader parser = MimeTypesHelper.GetParser(response.ContentType);
        parser.Load(g, new StreamReader(response.GetResponseStream()));

        foreach (Triple t in g.Triples)
        {
            Console.WriteLine(t.ToString());
        }
    }

    [Fact]
    public void ParsingRdfXmlStreaming()
    {
        var parser = new RdfXmlParser(RdfXmlParserMode.Streaming)
        {
            TraceParsing = true
        };
        var g = new Graph();
            parser.Load(g, Path.Combine("resources", "example.rdf"));

            TestTools.ShowGraph(g);
    }

    [Fact]
    public void ParsingMalformedFileUris()
    {
        var malformedFileUriFragment = "@base <file:/path/to/somewhere>. @prefix ex: <file:/path/to/nowhere/> . <#this> a \"URI Resolved with a malformed Base URI\" . <this> a \"Another URI Resolved with a malformed Base URI\" . ex:this a \"QName Resolved with a malformed Namespace URI\" .";

        var g = new Graph();
        VDS.RDF.Parsing.StringParser.Parse(g, malformedFileUriFragment);
        foreach (Triple t in g.Triples)
        {
            Console.WriteLine(t.ToString());
        }
    }

    //[Fact]
    //public void JsonNTriplesParsing()
    //{
    //    Graph g = new Graph();
    //    FileLoader.Load(g, "resources\\InferenceTest.ttl");
    //    g.Assert(new Triple(g.CreateBlankNode(), g.CreateUriNode("rdf:type"), g.CreateLiteralNode("some text", "en")));

    //    String temp = StringWriter.Write(g, new JsonNTriplesWriter());
    //    Console.WriteLine(temp);
    //    Console.WriteLine();

    //    Graph h = new Graph();
    //    VDS.RDF.Parsing.StringParser.Parse(h, temp, new JsonNTriplesParser());

    //    foreach (Triple t in h.Triples)
    //    {
    //        Console.WriteLine(t.ToString());
    //    }

    //    Assert.Equal(g, h);
    //}

    //[Fact]
    //public void JsonNTriplesEscaping()
    //{
    //    Graph g = new Graph();
    //    String[] testStrings = new String[]
    //    {
    //        "newline \\n ",
    //        "newline 2 \\r ",
    //        "double quote \\\" ",
    //        "tab \\t ",
    //        "backslash \\\\ ",
    //        "unicode \u00E9 "
    //    };

    //    UriNode subj = g.CreateUriNode("rdf:subject");
    //    UriNode pred = g.CreateUriNode("rdf:predicate");

    //    foreach (String test in testStrings)
    //    {
    //        g.Assert(new Triple(subj, pred, g.CreateLiteralNode(test)));
    //    }

    //    String temp = StringWriter.Write(g, new JsonNTriplesWriter());
    //    Console.WriteLine(temp);
    //    Console.WriteLine();

    //    Console.WriteLine("Original Graph");
    //    foreach (Triple t in g.Triples)
    //    {
    //        Console.WriteLine(t.ToString());
    //    }
    //    Console.WriteLine();

    //    Graph h = new Graph();
    //    Stopwatch timer = new Stopwatch();
    //    timer.Start();
    //    VDS.RDF.Parsing.StringParser.Parse(h, temp, new JsonNTriplesParser());
    //    timer.Stop();
    //    Console.WriteLine("Took " + timer.Elapsed + " to parse");

    //    Console.WriteLine("Serialized then Parsed Graph");
    //    foreach (Triple t in h.Triples)
    //    {
    //        Console.WriteLine(t.ToString());
    //    }

    //    Assert.Equal(g, h);
    //}

    [Fact]
    public void ParsingRdfXmlEmptyElement()
    {
        var fragment = @"<?xml version='1.0'?><rdf:RDF xmlns:rdf='" + NamespaceMapper.RDF + @"' xmlns='http://example.org/'><rdf:Description rdf:about='http://example.org/subject'><predicate rdf:resource='http://example.org/object' /></rdf:Description></rdf:RDF>";

        var g = new Graph();
        var parser = new RdfXmlParser(RdfXmlParserMode.Streaming)
        {
            TraceParsing = true
        };
        VDS.RDF.Parsing.StringParser.Parse(g, fragment, parser);
        foreach (Triple t in g.Triples)
        {
            Console.WriteLine(t.ToString());
        }
    }

    [Fact]
    public void ParsingTurtleWithAndWithoutBOM()
    {
        var parser = new TurtleParser();
        var g = new Graph();
        FileLoader.Load(g, Path.Combine("resources", "ttl-with-bom.ttl"));

        var h = new Graph();
        FileLoader.Load(h, Path.Combine("resources", "ttl-without-bom.ttl"));

        Assert.Equal(g, h);
    }

    [Fact]
    public void ParsingMalformedSparqlXml()
    {
        var results = new SparqlResultSet();
        var parser = new SparqlXmlParser();

        Assert.Throws<RdfParseException>(() => parser.Load(results, Path.Combine("resources", "bad_srx.srx")));
    }

    [Fact]
    public void ParsingTurtleDBPediaMalformedData()
    {
        var g = new Graph();
        var parser = new TurtleParser(TurtleSyntax.Original, false);

        Assert.Throws<RdfParseException>(() => parser.Load(g, Path.Combine("resources", "dbpedia_malformed.ttl")));
    }

    [Fact]
    public void ParsingDefaultPrefixFallbackTurtle1()
    {
        var data = @"@base <http://base/> . :subj :pred :obj .";
        IRdfReader parser = new TurtleParser();
        var g = new Graph();

        Assert.Throws<RdfParseException>(() => parser.Load(g, new StringReader(data)));
    }

    [Fact]
    public void ParsingDefaultPrefixFallbackTurtle2()
    {
        var data = @"@prefix : <http://default/ns#> . :subj :pred :obj .";
        IRdfReader parser = new TurtleParser();
        var g = new Graph();
        parser.Load(g, new StringReader(data));
        Assert.False(g.IsEmpty);
        Assert.Equal(1, g.Triples.Count);
    }

    [Fact]
    public void ParsingDefaultPrefixFallbackNotation3_1()
    {
        var data = @"@base <http://base/> . :subj :pred :obj .";
        IRdfReader parser = new Notation3Parser();
        var g = new Graph();
        parser.Load(g, new StringReader(data));
        Assert.False(g.IsEmpty);
        Assert.Equal(1, g.Triples.Count);
    }

    [Fact]
    public void ParsingDefaultPrefixFallbackNotation3_2()
    {
        var data = @"@prefix : <http://default/ns#> . :subj :pred :obj .";
        IRdfReader parser = new Notation3Parser();
        var g = new Graph();
        parser.Load(g, new StringReader(data));
        Assert.False(g.IsEmpty);
        Assert.Equal(1, g.Triples.Count);
    }

    [Fact]
    public void ParsingBlankNodeIDs()
    {
        var parsersToTest = new List<IRdfReader>()
        {
            new TurtleParser(),
            new Notation3Parser()
        };

        var samples = new String[] {
            "@prefix ex: <http://example.org>. [] a ex:bNode. _:autos1 a ex:bNode. _:autos1 a ex:another.",
            "@prefix ex: <http://example.org>. _:autos1 a ex:bNode. [] a ex:bNode. _:autos1 a ex:another.",
            "@prefix : <http://example.org/>. [] a :BlankNode ; :firstProperty :a ; :secondProperty :b .",
            "@prefix : <http://example.org/>. (:first :second) a :Collection .",
            "@prefix : <http://example.org/>. [a :bNode ; :connectsTo [a :bNode ; :connectsTo []]] a [].",
            "@prefix : <http://example.org/>. [a :bNode ; :connectsTo [a :bNode ; :connectsTo []]] a []. [] a :another ; a [a :yetAnother] ."
        };

        var expectedTriples = new int[] {
            3,
            3,
            3,
            5,
            5,
            8
        };

        var expectedSubjects = new int[] {
            2,
            2,
            1,
            2,
            2,
            4
        };

        Console.WriteLine("Tests Blank Node ID assignment in Parsing and Serialization as well as Graph Equality");
        Console.WriteLine();

        var writers = new List<IRdfWriter>() {
            new NTriplesWriter(),
            new CompressingTurtleWriter(),
            new Notation3Writer(),
            new RdfXmlWriter(),
            new PrettyRdfXmlWriter(),
            new RdfJsonWriter()
        };

        var readers = new List<IRdfReader>() {
            new NTriplesParser(),
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

            var s = 0;
            foreach (var sample in samples)
            {
                Console.WriteLine();
                Console.WriteLine("Sample:");
                Console.WriteLine(sample);
                Console.WriteLine();

                var g = new Graph();
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
                for (var i = 0; i < writers.Count; i++)
                {
                    var temp = VDS.RDF.Writing.StringWriter.Write(g, writers[i]);
                    var h = new Graph();
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
                    var equals = g.Equals(h, out mapping);
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
        var parsersToTest = new List<IRdfReader>()
        {
            new TurtleParser(),
            new Notation3Parser()
        };

        var samples = new String[] {
            "@prefix ex: <http://example.com/>. (\"one\" \"two\") a ex:Collection .",
            "@prefix ex: <http://example.com/>. (\"one\" \"two\" \"three\") a ex:Collection .",
            "@prefix ex: <http://example.com/>. (1) ex:someProp \"Value\"."
        };

        var expectedTriples = new int[] {
            5,
            7,
            3
        };

        var expectedSubjects = new int[] {
            2,
            3,
            1
        };

        var writers = new List<IRdfWriter>() {
            new NTriplesWriter(),
            new CompressingTurtleWriter(),
            new Notation3Writer(),
            new RdfXmlWriter(),
            new PrettyRdfXmlWriter(),
            new RdfJsonWriter()
        };

        var readers = new List<IRdfReader>() {
            new NTriplesParser(),
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

            var s = 0;
            foreach (var sample in samples)
            {
                Console.WriteLine();
                Console.WriteLine("Sample:");
                Console.WriteLine(sample);
                Console.WriteLine();

                var g = new Graph();
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
                for (var i = 0; i < writers.Count; i++)
                {
                    Console.WriteLine("Trying " + writers[i].GetType().ToString());
                    var temp = VDS.RDF.Writing.StringWriter.Write(g, writers[i]);
                    Console.WriteLine(temp);
                    var h = new Graph();
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
                    var equals = g.Equals(h, out mapping);
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
