using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Writing;

namespace VDS.RDF.Test.Parsing
{
    [TestClass]
    public class ParserTests
    {
        [TestMethod]
        public void ParsingStringParser()
        {
            try
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
                            Assert.Fail("Expected Parsing to Fail but succeeded");
                        }
                    }
                    catch (RdfParseException parseEx)
                    {
                        TestTools.ReportError("RDF Parsing Error", parseEx, parseExpected[i]);
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
            catch (Exception ex)
            {
                TestTools.ReportError("Other Error", ex, true);
            }
        }

        [TestMethod]
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
                new RdfXmlTreeWriter(),
                new FastRdfXmlWriter(),
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

                    Assert.AreEqual(expectedTriples[s], g.Triples.Count, "Should have produced " + expectedTriples[s] + " Triples");
                    Assert.AreEqual(expectedSubjects[s], g.Triples.SubjectNodes.Distinct().Count(), "Should have produced " + expectedSubjects[s] + " distinct subjects");

                    //Try outputting with each of the available writers
                    for (int i = 0; i < writers.Count; i++)
                    {
                        String temp = StringWriter.Write(g, writers[i]);
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

                        Assert.AreEqual(expectedTriples[s], h.Triples.Count, "Should have produced " + expectedTriples[s] + " Triples");
                        Assert.AreEqual(expectedSubjects[s], h.Triples.SubjectNodes.Distinct().Count(), "Should have produced " + expectedSubjects[s] + " distinct subjects");

                        //Do full equality Test
                        Dictionary<INode, INode> mapping;
                        bool equals = g.Equals(h, out mapping);
                        if (!equals)
                        {
                            Console.WriteLine(writers[i].GetType().ToString() + " failed");
                            Console.WriteLine(temp);
                        }
                        Assert.IsTrue(equals, "Graphs should be equal");
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

        [TestMethod]
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
                new RdfXmlTreeWriter(),
                new FastRdfXmlWriter(),
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

                    Assert.AreEqual(expectedTriples[s], g.Triples.Count, "Should have produced " + expectedTriples[s] + " Triples");
                    Assert.AreEqual(expectedSubjects[s], g.Triples.SubjectNodes.Distinct().Count(), "Should have produced " + expectedSubjects[s] + " distinct subjects");

                    //Try outputting with each of the available writers
                    for (int i = 0; i < writers.Count; i++)
                    {
                        String temp = StringWriter.Write(g, writers[i]);
                        Graph h = new Graph();
                        VDS.RDF.Parsing.StringParser.Parse(h, temp, readers[i]);

                        Console.WriteLine("Trying " + writers[i].GetType().ToString());
                        Assert.AreEqual(expectedTriples[s], h.Triples.Count, "Should have produced " + expectedTriples[s] + " Triples");
                        Assert.AreEqual(expectedSubjects[s], h.Triples.SubjectNodes.Distinct().Count(), "Should have produced " + expectedSubjects[s] + " distinct subjects");

                        Dictionary<INode, INode> mapping;
                        bool equals = g.Equals(h, out mapping);
                        Assert.IsTrue(equals, "Graphs should have been equal");
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

        [TestMethod]
        public void ParsingRdfXmlNamespaceAttributes()
        {
            try
            {
                Graph g = new Graph();
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://dbpedia.org/resource/Southampton");
                request.Method = "GET";
                request.Accept = MimeTypesHelper.HttpAcceptHeader;

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                IRdfReader parser = MimeTypesHelper.GetParser(response.ContentType);
                parser.Load(g, new BlockingStreamReader(response.GetResponseStream()));

                foreach (Triple t in g.Triples)
                {
                    Console.WriteLine(t.ToString());
                }

            }
            catch (Exception ex)
            {
                TestTools.ReportError("Error", ex, true);
            }
        }

        [TestMethod]
        public void ParsingMalformedRdfA()
        {
            Console.WriteLine("Tests how the RDFa Parser handles RDFa from the web which is embedded in malformed HTML and is known to contain malformed RDFa");
            Console.WriteLine("For this we use MySpace RDFa");
            Console.WriteLine();

            RdfAParser parser = new RdfAParser();
            parser.Warning += new RdfReaderWarning(TestTools.WarningPrinter);

            List<Uri> testUris = new List<Uri>()
            {
                new Uri("http://www.myspace.com/coldplay"),
                new Uri("http://www.myspace.com/fashionismylife10")
            };

            foreach (Uri u in testUris) 
            {
                Console.WriteLine("Testing URI " + u.ToString());
                Graph g = new Graph();
                UriLoader.Load(g, u, parser);

                foreach (Triple t in g.Triples)
                {
                    Console.WriteLine(t.ToString());
                }
                Console.WriteLine();
            }
        }

        [TestMethod]
        public void ParsingRdfXmlStreaming()
        {
            try
            {
                RdfXmlParser parser = new RdfXmlParser(RdfXmlParserMode.Streaming);
                parser.TraceParsing = true;
                Graph g = new Graph();
                parser.Load(g, "example.rdf");

                TestTools.ShowGraph(g);
            }
            catch (Exception ex)
            {
                TestTools.ReportError("Error", ex, true);
            }
        }

        [TestMethod]
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

        //[TestMethod]
        //public void JsonNTriplesParsing()
        //{
        //    Graph g = new Graph();
        //    FileLoader.Load(g, "InferenceTest.ttl");
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

        //    Assert.AreEqual(g, h, "Graphs should be equal before and after serialization");
        //}

        //[TestMethod]
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

        //    Assert.AreEqual(g, h, "Graphs should have been equal");
        //}

        [TestMethod]
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

        [TestMethod]
        public void ParsingTurtleWithAndWithoutBOM()
        {
            TurtleParser parser = new TurtleParser();
            Graph g = new Graph();
            FileLoader.Load(g, "ttl-with-bom.ttl");

            Graph h = new Graph();
            FileLoader.Load(h, "ttl-without-bom.ttl");

            Assert.AreEqual(g, h, "Graphs should be equal as presence (or lack thereof) of UTF-8 BOM should make no difference");
        }

        [TestMethod, ExpectedException(typeof(RdfParseException))]
        public void ParsingMalformedSparqlXml()
        {
            SparqlResultSet results = new SparqlResultSet();
            SparqlXmlParser parser = new SparqlXmlParser();
            parser.Load(results, "bad_srx.srx");
        }
    }
}
