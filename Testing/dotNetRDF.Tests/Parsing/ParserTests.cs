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

namespace VDS.RDF.Parsing
{
    public partial class ParserTests
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

                Graph g = new Graph();
                for (int i = 0; i < someRDF.Length; i++)
                {
                    try
                    {
                        String rdf = someRDF[i];
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
                            Assert.True(false, "Expected Parsing to Fail but succeeded");
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

        [SkippableFact]
        public void ParsingRdfXmlNamespaceAttributes()
        {
            if (!TestConfigManager.GetSettingAsBoolean(TestConfigManager.UseRemoteParsing))
            {
                throw new SkipTestException("Test Config marks Remote Parsing as unavailable, test cannot be run");
            }

            Graph g = new Graph();
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://dbpedia.org/resource/Southampton");
            request.Method = "GET";
            request.Accept = MimeTypesHelper.HttpAcceptHeader;

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
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
                RdfXmlParser parser = new RdfXmlParser(RdfXmlParserMode.Streaming);
                parser.TraceParsing = true;
                Graph g = new Graph();
                parser.Load(g, "resources\\example.rdf");

                TestTools.ShowGraph(g);
        }

        [Fact]
        public void ParsingMalformedFileUris()
        {
            String malformedFileUriFragment = "@base <file:/path/to/somewhere>. @prefix ex: <file:/path/to/nowhere/> . <#this> a \"URI Resolved with a malformed Base URI\" . <this> a \"Another URI Resolved with a malformed Base URI\" . ex:this a \"QName Resolved with a malformed Namespace URI\" .";

            Graph g = new Graph();
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
            String fragment = @"<?xml version='1.0'?><rdf:RDF xmlns:rdf='" + NamespaceMapper.RDF + @"' xmlns='http://example.org/'><rdf:Description rdf:about='http://example.org/subject'><predicate rdf:resource='http://example.org/object' /></rdf:Description></rdf:RDF>";

            Graph g = new Graph();
            RdfXmlParser parser = new RdfXmlParser(RdfXmlParserMode.Streaming);
            parser.TraceParsing = true;
            VDS.RDF.Parsing.StringParser.Parse(g, fragment, parser);
            foreach (Triple t in g.Triples)
            {
                Console.WriteLine(t.ToString());
            }
        }

        [Fact]
        public void ParsingTurtleWithAndWithoutBOM()
        {
            TurtleParser parser = new TurtleParser();
            Graph g = new Graph();
            FileLoader.Load(g, "resources\\ttl-with-bom.ttl");

            Graph h = new Graph();
            FileLoader.Load(h, "resources\\ttl-without-bom.ttl");

            Assert.Equal(g, h);
        }

        [Fact]
        public void ParsingMalformedSparqlXml()
        {
            SparqlResultSet results = new SparqlResultSet();
            SparqlXmlParser parser = new SparqlXmlParser();

            Assert.Throws<RdfParseException>(() => parser.Load(results, "resources\\bad_srx.srx"));
        }

        [Fact]
        public void ParsingTurtleDBPediaMalformedData()
        {
            Graph g = new Graph();
            TurtleParser parser = new TurtleParser(TurtleSyntax.Original);

            Assert.Throws<RdfParseException>(() => parser.Load(g, "resources\\dbpedia_malformed.ttl"));
        }

        [Fact]
        public void ParsingDefaultPrefixFallbackTurtle1()
        {
            String data = @"@base <http://base/> . :subj :pred :obj .";
            IRdfReader parser = new TurtleParser();
            Graph g = new Graph();

            Assert.Throws<RdfParseException>(() => parser.Load(g, new StringReader(data)));
        }

        [Fact]
        public void ParsingDefaultPrefixFallbackTurtle2()
        {
            String data = @"@prefix : <http://default/ns#> . :subj :pred :obj .";
            IRdfReader parser = new TurtleParser();
            Graph g = new Graph();
            parser.Load(g, new StringReader(data));
            Assert.False(g.IsEmpty);
            Assert.Equal(1, g.Triples.Count);
        }

        [Fact]
        public void ParsingDefaultPrefixFallbackNotation3_1()
        {
            String data = @"@base <http://base/> . :subj :pred :obj .";
            IRdfReader parser = new Notation3Parser();
            Graph g = new Graph();
            parser.Load(g, new StringReader(data));
            Assert.False(g.IsEmpty);
            Assert.Equal(1, g.Triples.Count);
        }

        [Fact]
        public void ParsingDefaultPrefixFallbackNotation3_2()
        {
            String data = @"@prefix : <http://default/ns#> . :subj :pred :obj .";
            IRdfReader parser = new Notation3Parser();
            Graph g = new Graph();
            parser.Load(g, new StringReader(data));
            Assert.False(g.IsEmpty);
            Assert.Equal(1, g.Triples.Count);
        }
    }
}
