using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Inference;
using VDS.RDF.Writing;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Test.Parsing
{
    [TestClass]
    public class VariableNodeTests
    {
        [TestMethod]
        public void ParsingN3Variables()
        {
            String TestFragment = "@prefix rdfs: <" + NamespaceMapper.RDFS + ">. { ?s a ?type } => { ?s rdfs:label \"This has a type\" } .";
            Notation3Parser parser = new Notation3Parser();
            Graph g = new Graph();
            StringParser.Parse(g, TestFragment, parser);

            foreach (Triple t in g.Triples)
            {
                Console.WriteLine(t.ToString());
            }

            StringWriter.Write(g, new Notation3Writer());
        }

        [TestMethod]
        public void ParsingN3GraphLiterals()
        {
            String TestFragment = "{ :a :b :c . :d :e :f } a \"Graph Literal\" .";
            Graph g = new Graph();
            g.BaseUri = new Uri("http://example.org/n3/graph-literals");
            StringParser.Parse(g, TestFragment, new Notation3Parser());

            Assert.IsTrue(g.Triples.Count == 1, "Should be 1 Triple");
            Assert.IsTrue(((GraphLiteralNode)g.Triples.First().Subject).SubGraph.Triples.Count == 2, "Should be 2 Triples in the Graph Literal");
        }

        [TestMethod]
        public void ParsingN3VariableContexts()
        {
            String prefixes = "@prefix rdf: <" + NamespaceMapper.RDF + ">. @prefix rdfs: <" + NamespaceMapper.RDFS + ">.";
            List<String> tests = new List<string>()
            {
                prefixes + "@forAll :x :type . { :x a :type } => {:x rdfs:label \"This has a type\" } .",
                prefixes + "@forSome :x :type . { :x a :type } => {:x rdfs:label \"This has a type\" } .",
                prefixes + "@forAll :h . @forSome :g . :g :loves :h .",
                prefixes + "@forSome :h . @forAll :g . :g :loves :h .",
                prefixes + "{@forSome :a . :Joe :home :a } a :Formula . :Joe :phone \"555-1212\" ."
            };

            Notation3Parser parser = new Notation3Parser();
            Notation3Writer writer = new Notation3Writer();
            foreach (String test in tests)
            {
                Graph g = new Graph();
                g.BaseUri = new Uri("http://example.org/n3rules");
                StringParser.Parse(g, test, parser);
                Console.WriteLine(StringWriter.Write(g, writer));

                Console.WriteLine();
            }
        }

        [TestMethod]
        public void ParsingN3Reasoner()
        {
            String rules = "@prefix rdfs: <" + NamespaceMapper.RDFS + "> . { ?s rdfs:subClassOf ?class } => { ?s a ?class } .";

            Graph rulesGraph = new Graph();
            StringParser.Parse(rulesGraph, rules, new Notation3Parser());

            Graph data = new Graph();
            FileLoader.Load(data, "InferenceTest.ttl");

            Console.WriteLine("Original Graph - " + data.Triples.Count + " Triples");
            int origCount = data.Triples.Count;
            foreach (Triple t in data.Triples)
            {
                Console.WriteLine(t.ToString());
            }

            SimpleN3RulesReasoner reasoner = new SimpleN3RulesReasoner();
            reasoner.Initialise(rulesGraph);

            reasoner.Apply(data);

            Console.WriteLine("Graph after Reasoner application - " + data.Triples.Count + " Triples");
            foreach (Triple t in data.Triples)
            {
                Console.WriteLine(t.ToString());
            }

            Assert.IsTrue(data.Triples.Count > origCount, "Number of Triples should have increased after the reasoner was run");
        }

        [TestMethod]
        public void ParsingN3ReasonerWithForAll()
        {
            String rules = "@prefix rdfs: <" + NamespaceMapper.RDFS + "> . @forAll :x . { :x rdfs:subClassOf ?class } => { :x a ?class } .";

            Graph rulesGraph = new Graph();
            rulesGraph.BaseUri = new Uri("http://example.org/rules");
            StringParser.Parse(rulesGraph, rules, new Notation3Parser());

            Graph data = new Graph();
            FileLoader.Load(data, "InferenceTest.ttl");

            Console.WriteLine("Original Graph - " + data.Triples.Count + " Triples");
            int origCount = data.Triples.Count;
            foreach (Triple t in data.Triples)
            {
                Console.WriteLine(t.ToString());
            }

            SimpleN3RulesReasoner reasoner = new SimpleN3RulesReasoner();
            reasoner.Initialise(rulesGraph);

            reasoner.Apply(data);

            Console.WriteLine("Graph after Reasoner application - " + data.Triples.Count + " Triples");
            foreach (Triple t in data.Triples)
            {
                Console.WriteLine(t.ToString());
            }

            Assert.IsTrue(data.Triples.Count > origCount, "Number of Triples should have increased after the reasoner was run");
        }
    }
}
