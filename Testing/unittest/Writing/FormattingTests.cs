using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Test.Writing
{
    [TestClass]
    public class FormattingTests
    {
        [TestMethod]
        public void WritingFormattingTriples()
        {
            try
            {
                //Create the Graph and define an additional namespace
                Graph g = new Graph();
                g.NamespaceMap.AddNamespace("ex", new Uri("http://example.org/"));

                //Create URIs used for datatypes
                Uri dtInt = new Uri(XmlSpecsHelper.XmlSchemaDataTypeInteger);
                Uri dtFloat = new Uri(XmlSpecsHelper.XmlSchemaDataTypeFloat);
                Uri dtDouble = new Uri(XmlSpecsHelper.XmlSchemaDataTypeDouble);
                Uri dtDecimal = new Uri(XmlSpecsHelper.XmlSchemaDataTypeDecimal);
                Uri dtBoolean = new Uri(XmlSpecsHelper.XmlSchemaDataTypeBoolean);
                Uri dtUnknown = new Uri("http://example.org/unknownType");
                Uri dtXmlLiteral = new Uri(RdfSpecsHelper.RdfXmlLiteral);

                //Create Nodes used for our test Triples
                IBlankNode subjBnode = g.CreateBlankNode();
                IUriNode subjUri = g.CreateUriNode(new Uri("http://example.org/subject"));
                IUriNode predUri = g.CreateUriNode(new Uri("http://example.org/vocab#predicate"));
                IUriNode predType = g.CreateUriNode(new Uri(RdfSpecsHelper.RdfType));
                IBlankNode objBnode = g.CreateBlankNode();
                IUriNode objUri = g.CreateUriNode(new Uri("http://example.org/object"));
                ILiteralNode objString = g.CreateLiteralNode("This is a literal");
                ILiteralNode objStringLang = g.CreateLiteralNode("This is a literal with a language specifier", "en");
                ILiteralNode objInt = g.CreateLiteralNode("123", dtInt);
                ILiteralNode objFloat = g.CreateLiteralNode("12.3e4", dtFloat);
                ILiteralNode objDouble = g.CreateLiteralNode("12.3e4", dtDouble);
                ILiteralNode objDecimal = g.CreateLiteralNode("12.3", dtDecimal);
                ILiteralNode objTrue = g.CreateLiteralNode("true", dtBoolean);
                ILiteralNode objFalse = g.CreateLiteralNode("false", dtBoolean);
                ILiteralNode objUnknown = g.CreateLiteralNode("This is a literal with an unknown type", dtUnknown);
                ILiteralNode objXmlLiteral = g.CreateLiteralNode("<strong>XML Literal</strong>", dtXmlLiteral);

                List<ITripleFormatter> formatters = new List<ITripleFormatter>()
                {
                    new NTriplesFormatter(),
                    new UncompressedTurtleFormatter(),
                    new UncompressedNotation3Formatter(),
                    new TurtleFormatter(g),
                    new Notation3Formatter(g),
                    new CsvFormatter(),
                    new TsvFormatter(),
                    new RdfXmlFormatter()
                };
                List<INode> subjects = new List<INode>()
                {
                    subjBnode,
                    subjUri
                };
                List<INode> predicates = new List<INode>()
                {
                    predUri,
                    predType
                };
                List<INode> objects = new List<INode>()
                {
                    objBnode,
                    objUri,
                    objString,
                    objStringLang,
                    objInt,
                    objFloat,
                    objDouble,
                    objDecimal,
                    objTrue,
                    objFalse,
                    objUnknown,
                    objXmlLiteral
                };
                List<Triple> testTriples = new List<Triple>();
                foreach (INode s in subjects)
                {
                    foreach (INode p in predicates)
                    {
                        foreach (INode o in objects)
                        {
                            testTriples.Add(new Triple(s, p, o));
                        }
                    }
                }

                foreach (Triple t in testTriples)
                {
                    Console.WriteLine("Raw Triple:");
                    Console.WriteLine(t.ToString());
                    Console.WriteLine();
                    foreach (ITripleFormatter f in formatters)
                    {
                        Console.WriteLine(f.GetType().ToString());
                        Console.WriteLine(f.Format(t));
                        Console.WriteLine();
                    }
                    Console.WriteLine();
                }
            }
            catch (Exception ex)
            {
                TestTools.ReportError("Error", ex, true);
            }       
        }

        [TestMethod]
        public void WritingFormattingGraphs()
        {
            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");

            List<IGraphFormatter> formatters = new List<IGraphFormatter>()
            {
                new RdfXmlFormatter()
            };

            List<IRdfReader> parsers = new List<IRdfReader>()
            {
                new RdfXmlParser()
            };

            Console.WriteLine("Using Formatter " + formatters.GetType().ToString());
            for (int i = 0; i < formatters.Count; i++)
            {
                IGraphFormatter formatter = formatters[i];

                StringBuilder output = new StringBuilder();
                output.AppendLine(formatter.FormatGraphHeader(g));
                foreach (Triple t in g.Triples)
                {
                    output.AppendLine(formatter.Format(t));
                }
                output.AppendLine(formatter.FormatGraphFooter());

                Console.WriteLine(output.ToString());

                //Try parsing to check it round trips
                Graph h = new Graph();
                IRdfReader parser = parsers[i];
                parser.Load(h, new StringReader(output.ToString()));

                GraphDiffReport diff = g.Difference(h);
                if (!diff.AreEqual)
                {
                    TestTools.ShowDifferences(diff);
                }

                Assert.AreEqual(g, h, "Graphs should be equal after round tripping");
            }
            Console.WriteLine();
        }
    }
}
