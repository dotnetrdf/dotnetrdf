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
using System.Linq;
using NUnit.Framework;
using VDS.RDF.Graphs;
using VDS.RDF.Parsing;
using VDS.RDF.Specifications;

namespace VDS.RDF.Nodes
{
    [TestFixture]
    public class NodeTests
        : BaseTest
    {
        [Test]
        public void NodeDistinct()
        {
            Graph g = new Graph();
            List<INode> test = new List<INode>()
            {
                g.CreateUriNode("rdf:type"),
                g.CreateUriNode(new Uri("http://example.org")),
                g.CreateBlankNode(),
                g.CreateBlankNode(), // Will be distinct from previous CreateBlankNode()
                null,
                g.CreateBlankNode(Guid.NewGuid()),
                g.CreateLiteralNode("Test text"),
                g.CreateLiteralNode("Test text", "en"), // Will be distinct from CreateLiteralNode("Test text") because it has a languate type
                g.CreateLiteralNode("Test text", new Uri(XmlSpecsHelper.XmlSchemaDataTypeString)), // Non-distinct - Equivalent to CreateLiteralNode("Test text") due to RDF 1.1 implicit typing to xsd:string
                g.CreateUriNode("rdf:type"), // Non-distinct
                null, // Non-distinct
                g.CreateUriNode(new Uri("http://example.org#test")),
                g.CreateUriNode(new Uri("http://example.org")) // Non-distinct
            };
            const int totalNonDistinct = 4;

            foreach (INode n in test.Distinct())
            {
                Console.WriteLine(n != null ? n.ToString() : "null");
            }
            Assert.AreEqual(test.Count - totalNonDistinct, test.Distinct().Count());
        }
   
        [Test]
        public void NodeHashCodes()
        {
            Console.WriteLine("Tests that Literal and URI Nodes produce different Hashes");
            Console.WriteLine();

            //Create the Nodes
            Graph g = new Graph();
            INode u = g.CreateUriNode(new Uri("http://www.google.com"));
            INode l = g.CreateLiteralNode("http://www.google.com/");

            Console.WriteLine("Created a URI and Literal Node both referring to 'http://www.google.com'");
            Console.WriteLine("String form of URI Node is:");
            Console.WriteLine(u.ToString());
            Console.WriteLine("String form of Literal Node is:");
            Console.WriteLine(l.ToString());
            Console.WriteLine("Hash Code of URI Node is " + u.GetHashCode());
            Console.WriteLine("Hash Code of Literal Node is " + l.GetHashCode());
            Console.WriteLine("Hash Codes are Equal? " + u.GetHashCode().Equals(l.GetHashCode()));
            Console.WriteLine("Nodes are equal? " + u.Equals(l));

            Assert.AreNotEqual(u.GetHashCode(), l.GetHashCode());
            Assert.AreNotEqual(u, l);

            //Create some plain and typed literals which may have colliding Hash Codes
            INode plain = g.CreateLiteralNode("test^^http://example.org/type");
            INode typed = g.CreateLiteralNode("test", new Uri("http://example.org/type"));

            Console.WriteLine();
            Console.WriteLine("Created a Plain and Typed Literal where the String representations are identical");
            Console.WriteLine("Plain Literal String form is:");
            Console.WriteLine(plain.ToString());
            Console.WriteLine("Typed Literal String from is:");
            Console.WriteLine(typed.ToString());
            Console.WriteLine("Hash Code of Plain Literal is " + plain.GetHashCode());
            Console.WriteLine("Hash Code of Typed Literal is " + typed.GetHashCode());
            Console.WriteLine("Hash Codes are Equal? " + plain.GetHashCode().Equals(typed.GetHashCode()));
            Console.WriteLine("Nodes are equal? " + plain.Equals(typed));

            Assert.AreNotEqual(plain.GetHashCode(), typed.GetHashCode());
            Assert.AreNotEqual(plain, typed);

            //Create Triples
            INode b = g.CreateBlankNode();
            INode type = g.CreateUriNode("rdf:type");
            Triple t1 = new Triple(b, type, u);
            Triple t2 = new Triple(b, type, l);

            Console.WriteLine();
            Console.WriteLine("Created two Triples stating a Blank Node has rdf:type of the URI Nodes created earlier");
            Console.WriteLine("String form of Triple 1 (using URI Node) is:");
            Console.WriteLine(t1.ToString());
            Console.WriteLine("String form of Triple 2 (using Literal Node) is:");
            Console.WriteLine(t2.ToString());
            Console.WriteLine("Hash Code of Triple 1 is " + t1.GetHashCode());
            Console.WriteLine("Hash Code of Triple 2 is " + t2.GetHashCode());
            Console.WriteLine("Hash Codes are Equal? " + t1.GetHashCode().Equals(t2.GetHashCode()));
            Console.WriteLine("Triples are Equal? " + t1.Equals(t2));

            Assert.AreNotEqual(t1.GetHashCode(), t2.GetHashCode());
            Assert.AreNotEqual(t1, t2);

            //Create Triples from the earlier Literal Nodes
            t1 = new Triple(b, type, plain);
            t2 = new Triple(b, type, typed);

            Console.WriteLine();
            Console.WriteLine("Created two Triples stating a Blank Node has rdf:type of the Literal Nodes created earlier");
            Console.WriteLine("String form of Triple 1 (using Plain Literal) is:");
            Console.WriteLine(t1.ToString());
            Console.WriteLine("String form of Triple 2 (using Typed Literal) is:");
            Console.WriteLine(t2.ToString());
            Console.WriteLine("Hash Code of Triple 1 is " + t1.GetHashCode());
            Console.WriteLine("Hash Code of Triple 2 is " + t2.GetHashCode());
            Console.WriteLine("Hash Codes are Equal? " + t1.GetHashCode().Equals(t2.GetHashCode()));
            Console.WriteLine("Triples are Equal? " + t1.Equals(t2));

            Assert.AreNotEqual(t1.GetHashCode(), t2.GetHashCode());
            Assert.AreNotEqual(t1, t2);

        }

        [Test]
        public void NodeUriNodeEquality()
        {
            //Create the Nodes
            Graph g = new Graph();
            Console.WriteLine("Creating two URIs referring to google - one lowercase, one uppercase - which should be equivalent");
            INode a = g.CreateUriNode(new Uri("http://www.google.com"));
            INode b = g.CreateUriNode(new Uri("http://www.GOOGLE.com/"));

            TestTools.CompareNodes(a, b, true);

            Console.WriteLine("Creating two URIs with the same Fragment ID but differing in case and thus are different since Fragment IDs are case sensitive => not equals");
            INode c = g.CreateUriNode(new Uri("http://www.google.com/#Test"));
            INode d = g.CreateUriNode(new Uri("http://www.GOOGLE.com/#test"));

            TestTools.CompareNodes(c, d, false);

            Console.WriteLine("Creating two identical URIs with unusual characters in them");
            INode e = g.CreateUriNode(new Uri("http://www.google.com/random,_@characters"));
            INode f = g.CreateUriNode(new Uri("http://www.google.com/random,_@characters"));

            TestTools.CompareNodes(e, f, true);

            Console.WriteLine("Creating two URIs with similar paths that differ in case");
            INode h = g.CreateUriNode(new Uri("http://www.google.com/path/test/case"));
            INode i = g.CreateUriNode(new Uri("http://www.google.com/path/Test/case"));

            TestTools.CompareNodes(h, i, false);

            Console.WriteLine("Creating three URIs with equivalent relative paths");
            INode j = g.CreateUriNode(new Uri("http://www.google.com/relative/test/../example.html"));
            INode k = g.CreateUriNode(new Uri("http://www.google.com/relative/test/monkey/../../example.html"));
            INode l = g.CreateUriNode(new Uri("http://www.google.com/relative/./example.html"));

            TestTools.CompareNodes(j, k, true);
            TestTools.CompareNodes(k, l, true);
        }

        [Test]
        public void NodeBlankNodeEquality()
        {
            Graph g = new Graph();
            Graph h = new Graph();
            Graph i = new Graph();

            Console.WriteLine("Doing some Blank Node Equality Testing");

            INode b = g.CreateBlankNode();
            INode c = g.CreateBlankNode();
            INode d = h.CreateBlankNode();
            INode e = i.CreateBlankNode();

            //Shouldn't be equal
            Assert.AreNotEqual(b, c, "Two Anonymous Blank Nodes created by a Graph should be non-equal");
            Assert.AreNotEqual(c, d, "Anonymous Blank Nodes created by different Graphs should be non-equal");
            Assert.AreNotEqual(b, d, "Anonymous Blank Nodes created by different Graphs should be non-equal");
            Assert.AreNotEqual(d, e, "Anonymous Blank Nodes created by different Graphs should be non-equal");

            //Should be equal
            Assert.AreEqual(b, b, "A Blank Node should be equal to itself");
            Assert.AreEqual(c, c, "A Blank Node should be equal to itself");
            Assert.AreEqual(d, d, "A Blank Node should be equal to itself");
            Assert.AreEqual(e, e, "A Blank Node should be equal to itself");

            //Named Nodes
            Guid guid = Guid.NewGuid();
            INode one = g.CreateBlankNode(guid);
            INode two = h.CreateBlankNode(guid);
            INode three = i.CreateBlankNode(guid);

            Assert.AreEqual(one, three, "Blank nodes with same ID are equal regardless of graph");
            Assert.AreEqual(one, two, "Blank nodes with same ID are equal regardless of graph");
            Assert.AreEqual(two, three, "Blank nodes with same ID are equal regardless of graph");
        }

        [Test]
        public void NodeLiteralNodeEquality()
        {
            try
            {
                IGraph g = new Graph();

                //Strict Mode Tests
                Console.WriteLine("Doing a load of Strict Literal Equality Tests");
                Options.LiteralEqualityMode = LiteralEqualityMode.Strict;

                //Test Literals with Language Tags
                INode hello, helloEn, helloEnUS, helloAgain;
                hello = g.CreateLiteralNode("hello");
                helloEn = g.CreateLiteralNode("hello", "en");
                helloEnUS = g.CreateLiteralNode("hello", "en-US");
                helloAgain = g.CreateLiteralNode("hello");

                Assert.AreNotEqual(hello, helloEn, "Identical Literals with differing Language Tags are non-equal");
                Assert.AreNotEqual(hello, helloEnUS, "Identical Literals with differing Language Tags are non-equal");
                Assert.AreNotEqual(helloEn, helloEnUS, "Identical Literals with differing Language Tags are non-equal");
                Assert.AreNotEqual(helloEn, helloAgain, "Identical Literals with differing Language Tags are non-equal");
                Assert.AreNotEqual(helloEnUS, helloAgain, "Identical Literals with differing Language Tags are non-equal");

                Assert.AreEqual(hello, helloAgain, "Identical Literals with no Language Tag are equal");

                //Test Plain Literals
                INode plain1, plain2, plain3, plain4;
                plain1 = g.CreateLiteralNode("plain literal");
                plain2 = g.CreateLiteralNode("another plain literal");
                plain3 = g.CreateLiteralNode("Plain Literal");
                plain4 = g.CreateLiteralNode("plain literal");

                Assert.AreNotEqual(plain1, plain2, "Literals with non-identical lexical values are non-equal");
                Assert.AreNotEqual(plain1, plain3, "Literals with non-identical lexical values are non-equal even if they differ only in case");
                Assert.AreEqual(plain1, plain4, "Literals with identical lexical values are equal");
                Assert.AreNotEqual(plain2, plain3, "Literals with non-identical lexical values are non-equal");
                Assert.AreNotEqual(plain2, plain4, "Literals with non-identical lexical values are non-equal");
                Assert.AreNotEqual(plain3, plain4, "Literals with non-identical lexical values are non-equal even if they differ only in case");

                //Typed Literals
                Uri intType = new Uri(XmlSpecsHelper.XmlSchemaDataTypeInteger);
                Uri boolType = new Uri(XmlSpecsHelper.XmlSchemaDataTypeBoolean);

                INode one1, one2, one3, one4;
                one1 = g.CreateLiteralNode("1");
                one2 = g.CreateLiteralNode("1", intType);
                one3 = g.CreateLiteralNode("0001", intType);
                one4 = g.CreateLiteralNode("1", intType);

                Assert.AreNotEqual(one1, one2, "Literals with identical lexical values but non-identical data types are non-equal");
                Assert.AreNotEqual(one1, one3, "Literals with identical lexical values but non-identical data types are non-equal");
                Assert.AreNotEqual(one1, one4, "Literals with identical lexical values but non-identical data types are non-equal");
                Assert.AreNotEqual(one2, one3, "Literals with equivalent values represented as different lexical values are non-equal even when they're data types are equal");
                Assert.AreEqual(one2, one4, "Literals with identical lexical values and identical data types are equal");
                Assert.AreNotEqual(one3, one4, "Literals with equivalent values represented as different lexical values are non-equal even when they're data types are equal");

                Assert.AreNotEqual(0, one1.CompareTo(one2), "Using the Comparer for Literal Nodes which is used for sorting Literals with identical lexical values but non-identical data types are still non-equal");
                Assert.AreEqual(0, one2.CompareTo(one3), "Using the Comparer for Literal Nodes which is used for sorting Literals with equivalent non-identical lexical values are considered equal when their data types are equal");
                Assert.AreEqual(0, one3.CompareTo(one2), "Using the Comparer for Literal Nodes which is used for sorting Literals with equivalent non-identical lexical values are considered equal when their data types are equal");
                Assert.AreEqual(0, one3.CompareTo(one4), "Using the Comparer for Literal Nodes which is used for sorting Literals with equivalent non-identical lexical values are considered equal when their data types are equal");

                INode t, f, one5;
                t = g.CreateLiteralNode("true", boolType);
                f = g.CreateLiteralNode("false", boolType);
                one5 = g.CreateLiteralNode("1", boolType);

                Assert.AreNotEqual(t, f, "Literals with different lexical values but identical data types are non-equal");
                Assert.AreEqual(t, t, "Literals with identical lexical values and identical data types are equal");
                Assert.AreEqual(f, f, "Literals with identical lexical values and identical data types are equal");

                Assert.AreNotEqual(t, one5, "Literals with different data types are non-equal even if their lexical values when cast to that type may be equivalent");

                //Loose Mode Tests
                Console.WriteLine("Doing a load of Loose Equality Tests");
                Options.LiteralEqualityMode = LiteralEqualityMode.Loose;

                Assert.AreEqual(one2, one3, "Literals with equivalent lexical values and identical data types can be considered equal under Loose Equality Mode");
                Assert.AreEqual(one3, one4, "Literals with equivalent lexical values and identical data types can be considered equal under Loose Equality Mode");
                Assert.AreNotEqual(t, one5, "Literals with equivalent lexical values (but which are not in the recognized lexical space of the type i.e. require a cast) and identical data types are still non-equal under Loose Equality Mode");

            }
            finally
            {
                //Reset Literal Equality Mode
                Options.LiteralEqualityMode = LiteralEqualityMode.Strict;
            }
        }

        [Test]
        public void NodeSorting()
        {
            //Stream for Output
            Console.WriteLine("## Sorting Test");
            Console.WriteLine("NULLs < Blank Nodes < URI Nodes < Untyped Literals < Typed Literals");
            Console.WriteLine();

            //Create a Graph
            Graph g = new Graph();
            g.Namespaces.AddNamespace("", new Uri("http://example.org/"));

            //Create a list of various Nodes
            List<INode> nodes = new List<INode>();
            nodes.Add(g.CreateUriNode(":someUri"));
            nodes.Add(g.CreateBlankNode());
            nodes.Add(null);
            nodes.Add(g.CreateBlankNode());
            nodes.Add(g.CreateLiteralNode("cheese"));
            nodes.Add(g.CreateLiteralNode("aardvark"));
            nodes.Add(g.CreateLiteralNode(DateTime.Now.AddDays(-25).ToString(XmlSpecsHelper.XmlSchemaDateTimeFormat), new Uri(XmlSpecsHelper.XmlSchemaDataTypeDateTime)));
            nodes.Add(g.CreateLiteralNode("duck"));
            nodes.Add(g.CreateUriNode(":otherUri"));
            nodes.Add(g.CreateLiteralNode("1.5", new Uri(XmlSpecsHelper.XmlSchemaDataTypeDouble)));
            nodes.Add(g.CreateUriNode(new Uri("http://www.google.com")));
            nodes.Add(g.CreateLiteralNode(DateTime.Now.AddYears(3).ToString(XmlSpecsHelper.XmlSchemaDateTimeFormat), new Uri(XmlSpecsHelper.XmlSchemaDataTypeDateTime)));
            nodes.Add(g.CreateLiteralNode("23", new Uri(XmlSpecsHelper.XmlSchemaDataTypeInteger)));
            nodes.Add(g.CreateLiteralNode("M43d", new Uri(XmlSpecsHelper.XmlSchemaDataTypeBase64Binary)));
            nodes.Add(g.CreateUriNode(new Uri("http://www.dotnetrdf.org")));
            nodes.Add(g.CreateLiteralNode("12", new Uri(XmlSpecsHelper.XmlSchemaDataTypeInteger)));
            nodes.Add(g.CreateBlankNode(Guid.NewGuid()));
            nodes.Add(g.CreateBlankNode());
            nodes.Add(g.CreateLiteralNode("chaese"));
            nodes.Add(g.CreateLiteralNode("1.0456345", new Uri(XmlSpecsHelper.XmlSchemaDataTypeDouble)));
            nodes.Add(g.CreateLiteralNode("cheese"));
            nodes.Add(g.CreateLiteralNode(Convert.ToBase64String(new byte[] { Byte.Parse("32") }), new Uri(XmlSpecsHelper.XmlSchemaDataTypeBase64Binary)));
            nodes.Add(g.CreateLiteralNode("TA==", new Uri(XmlSpecsHelper.XmlSchemaDataTypeBase64Binary)));
            nodes.Add(g.CreateLiteralNode("-45454", new Uri(XmlSpecsHelper.XmlSchemaDataTypeInteger)));
            nodes.Add(g.CreateLiteralNode(DateTime.Now.ToString(XmlSpecsHelper.XmlSchemaDateTimeFormat), new Uri(XmlSpecsHelper.XmlSchemaDataTypeDateTime)));
            nodes.Add(g.CreateLiteralNode("-3", new Uri(XmlSpecsHelper.XmlSchemaDataTypeInteger)));
            nodes.Add(g.CreateLiteralNode("242344.3456435", new Uri(XmlSpecsHelper.XmlSchemaDataTypeDouble)));
            nodes.Add(g.CreateLiteralNode("true", new Uri(XmlSpecsHelper.XmlSchemaDataTypeBoolean)));
            nodes.Add(g.CreateUriNode(":what"));
            nodes.Add(null);
            nodes.Add(g.CreateLiteralNode("false", new Uri(XmlSpecsHelper.XmlSchemaDataTypeBoolean)));
            nodes.Add(g.CreateLiteralNode("invalid-value", new Uri(XmlSpecsHelper.XmlSchemaDataTypeInteger)));

            for (int i = 0; i < 32; i++)
            {
                nodes.Add(g.CreateLiteralNode(i.ToString("x"), new Uri(XmlSpecsHelper.XmlSchemaDataTypeHexBinary)));
            }

            for (byte b = 50; b < 77; b++)
            {
                nodes.Add(g.CreateLiteralNode(Convert.ToBase64String(new byte[] { b }), new Uri(XmlSpecsHelper.XmlSchemaDataTypeBase64Binary)));
            }

            nodes.Sort();

            //Output the Results
            foreach (INode n in nodes)
            {
                Console.WriteLine(n == null ? "NULL" : n.ToString());
            }

            Console.WriteLine();
            Console.WriteLine("Now in reverse...");
            Console.WriteLine();

            nodes.Reverse();

            //Output the Results
            foreach (INode n in nodes)
            {
                Console.WriteLine(n == null ? "NULL" : n.ToString());
            }
        }

        //[Test]
        //public void NodesSortingSparqlOrder()
        //{
        //    SparqlOrderingComparer comparer = new SparqlOrderingComparer();

        //    //Stream for Output
        //    Console.WriteLine("## Sorting Test");
        //    Console.WriteLine("NULLs < Blank Nodes < URI Nodes < Untyped Literals < Typed Literals");
        //    Console.WriteLine();

        //    //Create a Graph
        //    Graph g = new Graph();
        //    g.BaseUri = new Uri("http://example.org/");
        //    g.Namespaces.AddNamespace("", new Uri("http://example.org/"));

        //    //Create a list of various Nodes
        //    List<INode> nodes = new List<INode>();
        //    nodes.Add(g.CreateUriNode(":someUri"));
        //    nodes.Add(g.CreateBlankNode());
        //    nodes.Add(null);
        //    nodes.Add(g.CreateBlankNode());
        //    nodes.Add(g.CreateLiteralNode("cheese"));
        //    nodes.Add(g.CreateLiteralNode("aardvark"));
        //    nodes.Add(g.CreateLiteralNode(DateTime.Now.AddDays(-25).ToString(XmlSpecsHelper.XmlSchemaDateTimeFormat), new Uri(XmlSpecsHelper.XmlSchemaDataTypeDateTime)));
        //    nodes.Add(g.CreateLiteralNode("duck"));
        //    nodes.Add(g.CreateUriNode(":otherUri"));
        //    nodes.Add(g.CreateLiteralNode("1.5", new Uri(XmlSpecsHelper.XmlSchemaDataTypeDouble)));
        //    nodes.Add(g.CreateUriNode(new Uri("http://www.google.com")));
        //    nodes.Add(g.CreateLiteralNode(DateTime.Now.AddYears(3).ToString(XmlSpecsHelper.XmlSchemaDateTimeFormat), new Uri(XmlSpecsHelper.XmlSchemaDataTypeDateTime)));
        //    nodes.Add(g.CreateLiteralNode("23", new Uri(XmlSpecsHelper.XmlSchemaDataTypeInteger)));
        //    nodes.Add(g.CreateLiteralNode("M43d", new Uri(XmlSpecsHelper.XmlSchemaDataTypeBase64Binary)));
        //    nodes.Add(g.CreateUriNode(new Uri("http://www.dotnetrdf.org")));
        //    nodes.Add(g.CreateLiteralNode("12", new Uri(XmlSpecsHelper.XmlSchemaDataTypeInteger)));
        //    nodes.Add(g.CreateBlankNode("monkey"));
        //    nodes.Add(g.CreateBlankNode());
        //    nodes.Add(g.CreateLiteralNode("chaese"));
        //    nodes.Add(g.CreateLiteralNode("1.0456345", new Uri(XmlSpecsHelper.XmlSchemaDataTypeDouble)));
        //    nodes.Add(g.CreateLiteralNode("cheese"));
        //    nodes.Add(g.CreateLiteralNode(Convert.ToBase64String(new byte[] { Byte.Parse("32") }), new Uri(XmlSpecsHelper.XmlSchemaDataTypeBase64Binary)));
        //    nodes.Add(g.CreateLiteralNode("TA==", new Uri(XmlSpecsHelper.XmlSchemaDataTypeBase64Binary)));
        //    nodes.Add(g.CreateLiteralNode("-45454", new Uri(XmlSpecsHelper.XmlSchemaDataTypeInteger)));
        //    nodes.Add(g.CreateLiteralNode(DateTime.Now.ToString(XmlSpecsHelper.XmlSchemaDateTimeFormat), new Uri(XmlSpecsHelper.XmlSchemaDataTypeDateTime)));
        //    nodes.Add(g.CreateLiteralNode("-3", new Uri(XmlSpecsHelper.XmlSchemaDataTypeInteger)));
        //    nodes.Add(g.CreateLiteralNode("242344.3456435", new Uri(XmlSpecsHelper.XmlSchemaDataTypeDouble)));
        //    nodes.Add(g.CreateLiteralNode("true", new Uri(XmlSpecsHelper.XmlSchemaDataTypeBoolean)));
        //    nodes.Add(g.CreateUriNode(":what"));
        //    nodes.Add(null);
        //    nodes.Add(g.CreateLiteralNode("false", new Uri(XmlSpecsHelper.XmlSchemaDataTypeBoolean)));
        //    nodes.Add(g.CreateLiteralNode("invalid-value", new Uri(XmlSpecsHelper.XmlSchemaDataTypeInteger)));

        //    nodes.Sort(comparer);

        //    //Output the Results
        //    foreach (INode n in nodes)
        //    {
        //        if (n == null)
        //        {
        //            Console.WriteLine("NULL");
        //        }
        //        else
        //        {
        //            Console.WriteLine(n.ToString());
        //        }
        //    }

        //    Console.WriteLine();
        //    Console.WriteLine("Now in reverse...");
        //    Console.WriteLine();

        //    nodes.Reverse();

        //    //Output the Results
        //    foreach (INode n in nodes)
        //    {
        //        if (n == null)
        //        {
        //            Console.WriteLine("NULL");
        //        }
        //        else
        //        {
        //            Console.WriteLine(n.ToString());
        //        }
        //    }
        //}

        [Test]
        public void NodeNullNodeEquality()
        {
            const UriNode nullUri = null;
            const LiteralNode nullLiteral = null;
            const BlankNode nullBNode = null;

            Graph g = new Graph();
            INode someUri = g.CreateUriNode(new Uri("http://example.org"));
            INode someLiteral = g.CreateLiteralNode("A Literal");
            INode someBNode = g.CreateBlankNode();

            // ReSharper disable EqualExpressionComparison
            // ReSharper disable CSharpWarnings::CS0252
            Assert.AreEqual(nullUri, nullUri, "Null URI Node should be equal to self");
            Assert.AreEqual(nullUri, null, "Null URI Node should be equal to a null");
            Assert.AreEqual(null, nullUri, "Null should be equal to a Null URI Node");
            Assert.IsTrue(nullUri == nullUri, "Null URI Node should be equal to self");
            Assert.IsTrue(nullUri == null, "Null URI Node should be equal to a null");
            Assert.IsTrue(null == nullUri, "Null should be equal to a Null URI Node");
            Assert.IsFalse(nullUri != nullUri, "Null URI Node should be equal to self");
            Assert.IsFalse(nullUri != null, "Null URI Node should be equal to a null");
            Assert.IsFalse(null != nullUri, "Null should be equal to a Null URI Node");
            Assert.AreNotEqual(nullUri, someUri, "Null URI Node should not be equal to an actual URI Node");
            Assert.AreNotEqual(someUri, nullUri, "Null URI Node should not be equal to an actual URI Node");
            Assert.IsFalse(nullUri == someUri, "Null URI Node should not be equal to an actual URI Node");
            Assert.IsFalse(someUri == nullUri, "Null URI Node should not be equal to an actual URI Node");

            Assert.AreEqual(nullLiteral, nullLiteral, "Null Literal Node should be equal to self");
            Assert.AreEqual(nullLiteral, null, "Null Literal Node should be equal to a null");
            Assert.AreEqual(null, nullLiteral, "Null should be equal to a Null Literal Node");
            Assert.IsTrue(nullLiteral == nullLiteral, "Null Literal Node should be equal to self");
            Assert.IsTrue(nullLiteral == null, "Null Literal Node should be equal to a null");
            Assert.IsTrue(null == nullLiteral, "Null should be equal to a Null Literal Node");
            Assert.IsFalse(nullLiteral != nullLiteral, "Null Literal Node should be equal to self");
            Assert.IsFalse(nullLiteral != null, "Null Literal Node should be equal to a null");
            Assert.IsFalse(null != nullLiteral, "Null should be equal to a Null Literal Node");
            Assert.AreNotEqual(nullLiteral, someLiteral, "Null Literal Node should not be equal to an actual Literal Node");
            Assert.AreNotEqual(someLiteral, nullLiteral, "Null Literal Node should not be equal to an actual Literal Node");
            Assert.IsFalse(nullLiteral == someLiteral, "Null Literal Node should not be equal to an actual Literal Node");
            Assert.IsFalse(someLiteral == nullLiteral, "Null Literal Node should not be equal to an actual Literal Node");

            Assert.AreEqual(nullBNode, nullBNode, "Null BNode Node should be equal to self");
            Assert.AreEqual(nullBNode, null, "Null BNode Node should be equal to a null");
            Assert.AreEqual(null, nullBNode, "Null should be equal to a Null BNode Node");
            Assert.IsTrue(nullBNode == nullBNode, "Null BNode Node should be equal to self");
            Assert.IsTrue(nullBNode == null, "Null BNode Node should be equal to a null");
            Assert.IsTrue(null == nullBNode, "Null should be equal to a Null BNode Node");
            Assert.IsFalse(nullBNode != nullBNode, "Null BNode Node should be equal to self");
            Assert.IsFalse(nullBNode != null, "Null BNode Node should be equal to a null");
            Assert.IsFalse(null != nullBNode, "Null should be equal to a Null BNode Node");
            Assert.AreNotEqual(nullBNode, someBNode, "Null BNode Node should not be equal to an actual BNode Node");
            Assert.AreNotEqual(someBNode, nullBNode, "Null BNode Node should not be equal to an actual BNode Node");
            Assert.IsFalse(nullBNode == someBNode, "Null BNode Node should not be equal to an actual BNode Node");
            Assert.IsFalse(someBNode == nullBNode, "Null BNode Node should not be equal to an actual BNode Node");
            // ReSharper restore CSharpWarnings::CS0252
            // ReSharper restore EqualExpressionComparison
        }
    }
}
