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
using System.Text;
using Xunit;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Writing;


public class FormattingTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public FormattingTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void WritingFormattingTriples()
    {
        //Create the Graph and define an additional namespace
        var g = new Graph();
        g.NamespaceMap.AddNamespace("ex", new Uri("http://example.org/"));

        //Create URIs used for datatypes
        var dtInt = new Uri(XmlSpecsHelper.XmlSchemaDataTypeInteger);
        var dtFloat = new Uri(XmlSpecsHelper.XmlSchemaDataTypeFloat);
        var dtDouble = new Uri(XmlSpecsHelper.XmlSchemaDataTypeDouble);
        var dtDecimal = new Uri(XmlSpecsHelper.XmlSchemaDataTypeDecimal);
        var dtBoolean = new Uri(XmlSpecsHelper.XmlSchemaDataTypeBoolean);
        var dtUnknown = new Uri("http://example.org/unknownType");
        var dtXmlLiteral = new Uri(RdfSpecsHelper.RdfXmlLiteral);

        //Create Nodes used for our test Triples
        IBlankNode subjBnode = g.CreateBlankNode();
        IUriNode subjUri = g.CreateUriNode(new Uri("http://example.org/subject"));
        IUriNode subjUri2 = g.CreateUriNode(new Uri("http://example.org/123"));
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

        var formatters = new List<ITripleFormatter>()
        {
            new NTriplesFormatter(NTriplesSyntax.Original),
            new NTriplesFormatter(NTriplesSyntax.Rdf11),
            new UncompressedTurtleFormatter(),
            new UncompressedNotation3Formatter(),
            new TurtleFormatter(g),
            new TurtleW3CFormatter(g),
            new Notation3Formatter(g),
            new CsvFormatter(),
            new TsvFormatter(),
            new RdfXmlFormatter()
        };
        var subjects = new List<INode>()
        {
            subjBnode,
            subjUri,
            subjUri2
        };
        var predicates = new List<INode>()
        {
            predUri,
            predType
        };
        var objects = new List<INode>()
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
        var testTriples = (from s in subjects from p in predicates from o in objects select new Triple(s, p, o)).ToList();

        foreach (Triple t in testTriples)
        {
            _testOutputHelper.WriteLine("Raw Triple:");
            _testOutputHelper.WriteLine(t.ToString());
            _testOutputHelper.WriteLine();
            foreach (ITripleFormatter f in formatters)
            {
                _testOutputHelper.WriteLine(f.GetType().ToString());
                _testOutputHelper.WriteLine(f.Format(t));
                _testOutputHelper.WriteLine();
            }
            _testOutputHelper.WriteLine();
        }
    }

    [Fact]
    public void WritingFormattingGraphs()
    {
        var graphs = new List<IGraph>();
        var g = new Graph();
        g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
        graphs.Add(g);
        g = new Graph();
        g.LoadFromFile(Path.Combine("resources", "InferenceTest.ttl"));
        graphs.Add(g);
        g = new Graph();
        g.LoadFromFile(Path.Combine("resources", "cyrillic.rdf"));
        graphs.Add(g);
        g = new Graph();
        g.LoadFromFile(Path.Combine("resources", "complex-collections.nt"));
        graphs.Add(g);
        g.LoadFromFile(Path.Combine("resources", "issue-522", "urn-uuid.nt"));
        graphs.Add(g);

        var formatters = new List<IGraphFormatter>()
        {
            new RdfXmlFormatter()
        };

        var parsers = new List<IRdfReader>()
        {
            new RdfXmlParser()
        };

        for (var i = 0; i < formatters.Count; i++)
        {
            IGraphFormatter formatter = formatters[i];
            _testOutputHelper.WriteLine("Using Formatter " + formatter.GetType());

            foreach (IGraph graph in graphs)
            {
                //Console.WriteLine("Testing Graph " + (graph.BaseUri != null ? graph.BaseUri.ToString() : String.Empty));
                var output = new StringBuilder();
                output.AppendLine(formatter.FormatGraphHeader(graph));
                foreach (Triple t in graph.Triples)
                {
                    output.AppendLine(formatter.Format(t));
                }
                output.AppendLine(formatter.FormatGraphFooter());

                _testOutputHelper.WriteLine(output.ToString());

                //Try parsing to check it round trips
                var h = new Graph();
                IRdfReader parser = parsers[i];
                parser.Load(h, new StringReader(output.ToString()));

                GraphDiffReport diff = graph.Difference(h);
                if (!diff.AreEqual)
                {
                    TestTools.ShowDifferences(diff);
                }

                Assert.Equal(graph, h);
            }
        }
        _testOutputHelper.WriteLine();
    }

    [Fact]
    public void WritingFormattingResultSets()
    {
        var g = new Graph();
        g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
        var expected = g.ExecuteQuery("SELECT * WHERE { ?s a ?type }") as SparqlResultSet;
        Assert.NotNull(expected);

        var formatters = new List<IResultSetFormatter>()
        {
            new SparqlXmlFormatter()
        };

        var parsers = new List<ISparqlResultsReader>()
        {
            new SparqlXmlParser()
        };

        _testOutputHelper.WriteLine("Using Formatter " + formatters.GetType());
        for (var i = 0; i < formatters.Count; i++)
        {
            IResultSetFormatter formatter = formatters[i];

            var output = new StringBuilder();
            output.AppendLine(formatter.FormatResultSetHeader(expected.Variables));
            foreach (ISparqlResult r in expected)
            {
                output.AppendLine(formatter.Format(r));
            }
            output.AppendLine(formatter.FormatResultSetFooter());

            _testOutputHelper.WriteLine(output.ToString());

            //Try parsing to check it round trips
            var actual = new SparqlResultSet();
            ISparqlResultsReader parser = parsers[i];
            parser.Load(actual, new StringReader(output.ToString()));

            Assert.Equal(expected, actual);
        }
        _testOutputHelper.WriteLine();
    }
}
