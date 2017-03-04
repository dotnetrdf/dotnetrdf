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
using System.Reflection;
using System.Text;
using System.Net;
using System.Web;
using NUnit.Framework;
using VDS.RDF.Writing;
using VDS.RDF.Parsing;
using VDS.RDF.Query;

namespace VDS.RDF
{
    [TestFixture]
    public class BasicTests1 : BaseTest
    {
        [Test]
        public void NodesDistinct()
        {
            Graph g = new Graph();
            List<INode> test = new List<INode>()
            {
                g.CreateUriNode("rdf:type"),
                g.CreateUriNode(new Uri("http://example.org")),
                g.CreateBlankNode(),
                g.CreateBlankNode(),
                null,
                g.CreateBlankNode("test"),
                g.CreateLiteralNode("Test text"),
                g.CreateLiteralNode("Test text", "en"),
                g.CreateLiteralNode("Test text", new Uri(XmlSpecsHelper.XmlSchemaDataTypeString)),
                g.CreateUriNode("rdf:type"),
                null,
                g.CreateUriNode(new Uri("http://example.org#test")),
                g.CreateUriNode(new Uri("http://example.org"))
            };

            foreach (INode n in test.Distinct())
            {
                if (n != null)
                {
                    Console.WriteLine(n.ToString());
                }
                else
                {
                    Console.WriteLine("null");
                }
            }
        }

        [Test]
        public void GraphCreation1()
        {
            //Create a new Empty Graph
            Graph g = new Graph();
            Assert.IsNotNull(g);

            //Define Namespaces
            g.NamespaceMap.AddNamespace("vds", new Uri("http://www.vdesign-studios.com/dotNetRDF#"));
            g.NamespaceMap.AddNamespace("ecs", new Uri("http://id.ecs.soton.ac.uk/person/"));

            //Check we set the Namespace OK
            Assert.IsTrue(g.NamespaceMap.HasNamespace("vds"), "Failed to set a Namespace");

            //Set Base Uri
            g.BaseUri = g.NamespaceMap.GetNamespaceUri("vds");
            Assert.IsNotNull(g.BaseUri);
            Assert.AreEqual(g.NamespaceMap.GetNamespaceUri("vds"), g.BaseUri);

            //Create Uri Nodes
            IUriNode rav08r, wh, lac, hcd;
            rav08r = g.CreateUriNode("ecs:11471");
            wh = g.CreateUriNode("ecs:1650");
            hcd = g.CreateUriNode("ecs:46");
            lac = g.CreateUriNode("ecs:60");

            //Create Uri Nodes for some Predicates
            IUriNode supervises, collaborates, advises, has;
            supervises = g.CreateUriNode("vds:supervises");
            collaborates = g.CreateUriNode("vds:collaborates");
            advises = g.CreateUriNode("vds:advises");
            has = g.CreateUriNode("vds:has");

            //Create some Literal Nodes
            ILiteralNode singleLine = g.CreateLiteralNode("Some string");
            ILiteralNode multiLine = g.CreateLiteralNode("This goes over\n\nseveral\n\nlines");
            ILiteralNode french = g.CreateLiteralNode("Bonjour", "fr");
            ILiteralNode number = g.CreateLiteralNode("12", new Uri(g.NamespaceMap.GetNamespaceUri("xsd") + "integer"));

            g.Assert(new Triple(wh, supervises, rav08r));
            g.Assert(new Triple(lac, supervises, rav08r));
            g.Assert(new Triple(hcd, advises, rav08r));
            g.Assert(new Triple(wh, collaborates, lac));
            g.Assert(new Triple(wh, collaborates, hcd));
            g.Assert(new Triple(lac, collaborates, hcd));
            g.Assert(new Triple(rav08r, has, singleLine));
            g.Assert(new Triple(rav08r, has, multiLine));
            g.Assert(new Triple(rav08r, has, french));
            g.Assert(new Triple(rav08r, has, number));

            //Now print all the Statements
            Console.WriteLine("All Statements");
            foreach (Triple t in g.Triples)
            {
                Console.WriteLine(t.ToString());
            }

            //Get statements about Rob Vesse
            Console.WriteLine();
            Console.WriteLine("Statements about Rob Vesse");
            foreach (Triple t in g.GetTriples(rav08r))
            {
                Console.WriteLine(t.ToString());
            }

            //Get Statements about Collaboration
            Console.WriteLine();
            Console.WriteLine("Statements about Collaboration");
            foreach (Triple t in g.GetTriples(collaborates))
            {
                Console.WriteLine(t.ToString());
            }

            //Attempt to output Turtle for this Graph
            try
            {
                Console.WriteLine("Writing Turtle file graph_building_example.ttl");
                TurtleWriter ttlwriter = new TurtleWriter();
                ttlwriter.Save(g, "graph_building_example.ttl");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        [Test]
        public void GraphCreation2()
        {
            //Create a new Empty Graph
            Graph g = new Graph();
            Assert.IsNotNull(g);

            //Define Namespaces
            g.NamespaceMap.AddNamespace("pets", new Uri("http://example.org/pets"));
            Assert.IsTrue(g.NamespaceMap.HasNamespace("pets"));

            //Create Uri Nodes
            IUriNode dog, fido, rob, owner, name, species, breed, lab;
            dog = g.CreateUriNode("pets:Dog");
            fido = g.CreateUriNode("pets:abc123");
            rob = g.CreateUriNode("pets:def456");
            owner = g.CreateUriNode("pets:hasOwner");
            name = g.CreateUriNode("pets:hasName");
            species = g.CreateUriNode("pets:isAnimal");
            breed = g.CreateUriNode("pets:isBreed");
            lab = g.CreateUriNode("pets:Labrador");

            //Assert Triples
            g.Assert(new Triple(fido, species, dog));
            g.Assert(new Triple(fido, owner, rob));
            g.Assert(new Triple(fido, name, g.CreateLiteralNode("Fido")));
            g.Assert(new Triple(rob, name, g.CreateLiteralNode("Rob")));
            g.Assert(new Triple(fido, breed, lab));

            Assert.IsTrue(g.Triples.Count == 5);
        }

        [Test]
        public void UriResolution()
        {
            String[] baseUris = { "http://www.bbc.co.uk",
                                  "http://www.bbc.co.uk/",
                                  "http://www.bbc.co.uk/test.txt",
                                  "http://www.bbc.co.uk/test",
                                  "http://www.bbc.co.uk/test/",
                                  "http://www.bbc.co.uk/test/subdir",
                                  "http://www.bbc.co.uk/really/really/long/path",
                                  "http://www.bbc.co.uk#fragment",
                                  "http://www.bbc.co.uk/test.txt#fragment"//,
                                };
            String[] uriRefs = { "test2.txt",
                                 "test2",
                                 "/test2",
                                 "test2/subdir",
                                 "/test2/subdir",
                                 "../test2",
                                 "#fragment2"
                                };
            String[][] expected = { new String[] {"http://www.bbc.co.uk/test2.txt","http://www.bbc.co.uk/test2","http://www.bbc.co.uk/test2","http://www.bbc.co.uk/test2/subdir","http://www.bbc.co.uk/test2/subdir","http://www.bbc.co.uk/test2","http://www.bbc.co.uk/#fragment2"},
                                    new String[] {"http://www.bbc.co.uk/test2.txt","http://www.bbc.co.uk/test2","http://www.bbc.co.uk/test2","http://www.bbc.co.uk/test2/subdir","http://www.bbc.co.uk/test2/subdir","http://www.bbc.co.uk/test2","http://www.bbc.co.uk/#fragment2"},
                                    new String[] {"http://www.bbc.co.uk/test2.txt","http://www.bbc.co.uk/test2","http://www.bbc.co.uk/test2","http://www.bbc.co.uk/test2/subdir","http://www.bbc.co.uk/test2/subdir","http://www.bbc.co.uk/test2","http://www.bbc.co.uk/test.txt#fragment2"},
                                    new String[] {"http://www.bbc.co.uk/test2.txt","http://www.bbc.co.uk/test2","http://www.bbc.co.uk/test2","http://www.bbc.co.uk/test2/subdir","http://www.bbc.co.uk/test2/subdir","http://www.bbc.co.uk/test2","http://www.bbc.co.uk/test#fragment2"},
                                    new String[] {"http://www.bbc.co.uk/test/test2.txt","http://www.bbc.co.uk/test/test2","http://www.bbc.co.uk/test2","http://www.bbc.co.uk/test/test2/subdir","http://www.bbc.co.uk/test2/subdir","http://www.bbc.co.uk/test2","http://www.bbc.co.uk/test/#fragment2"},
                                    new String[] {"http://www.bbc.co.uk/test/test2.txt","http://www.bbc.co.uk/test/test2","http://www.bbc.co.uk/test2","http://www.bbc.co.uk/test/test2/subdir","http://www.bbc.co.uk/test2/subdir","http://www.bbc.co.uk/test2","http://www.bbc.co.uk/test/subdir#fragment2"},
                                    new String[] {"http://www.bbc.co.uk/really/really/long/test2.txt","http://www.bbc.co.uk/really/really/long/test2","http://www.bbc.co.uk/test2","http://www.bbc.co.uk/really/really/long/test2/subdir","http://www.bbc.co.uk/test2/subdir","http://www.bbc.co.uk/really/really/test2","http://www.bbc.co.uk/really/really/long/path#fragment2"},
                                    new String[] {"http://www.bbc.co.uk/test2.txt","http://www.bbc.co.uk/test2","http://www.bbc.co.uk/test2","http://www.bbc.co.uk/test2/subdir","http://www.bbc.co.uk/test2/subdir","http://www.bbc.co.uk/test2","http://www.bbc.co.uk/#fragment2"},
                                    new String[] {"http://www.bbc.co.uk/test2.txt","http://www.bbc.co.uk/test2","http://www.bbc.co.uk/test2","http://www.bbc.co.uk/test2/subdir","http://www.bbc.co.uk/test2/subdir","http://www.bbc.co.uk/test2","http://www.bbc.co.uk/test.txt#fragment2"}
                                  };

            try
            {
                for (int i = 0; i < baseUris.Length; i++)
                {
                    Console.WriteLine("Resolving against Base URI " + baseUris[i]);

                    Uri baseUri = new Uri(baseUris[i]);

                    for (int j = 0; j < uriRefs.Length; j++)
                    {
                        Console.WriteLine("Resolving " + uriRefs[j]);

                        String result, expectedResult;
                        result = Tools.ResolveUri(uriRefs[j], baseUris[i]);
                        expectedResult = expected[i][j];

                        Console.WriteLine("Expected: " + expectedResult);
                        Console.WriteLine("Actual: " + result);

                        Assert.AreEqual(expectedResult, result);
                    }

                    Console.WriteLine();
                }

                Uri mailto = new Uri("mailto:example@example.org");
                Uri rel = new Uri("/some/folder", UriKind.Relative);
                Uri res = new Uri(mailto, rel);
                Console.WriteLine(res.ToString());
            }
            catch (UriFormatException uriEx)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [Test]
        public void UriResolutionWithGraphBase()
        {
            IGraph g = new Graph();
            g.BaseUri = new Uri("http://example.org/");

            Uri expected = new Uri("http://example.org/relative/path");
            IUriNode actual = g.CreateUriNode(new Uri("relative/path", UriKind.Relative));
            Assert.AreEqual(expected, actual.Uri);
        }

        [Test]
        public void UriResolutionUriProvidedToQNameMethod()
        {
            IGraph g = new Graph();
            var ex = Assert.Throws<RdfException>(() => g.CreateUriNode("http://example.org"));
         
            TestTools.ReportError("Error", ex);
        }

        [Test]
        public void UriHashCodes()
        {
            //Quick Test to see if how the Uri classes Hash Codes behave
            Uri test1 = new Uri("http://example.org/test#one");
            Uri test2 = new Uri("http://example.org/test#two");
            Uri test3 = new Uri("http://example.org/test#three");

            Console.WriteLine("Three identical URIs with different Fragment IDs, .Net ignores the Fragments in creating Hash Codes");
            Console.WriteLine("URI 1 has Hash Code " + test1.GetHashCode());
            Console.WriteLine("URI 2 has Hash Code " + test2.GetHashCode());
            Console.WriteLine("URI 3 has Hash Code " + test3.GetHashCode());

            Assert.AreEqual(test1.GetHashCode(), test2.GetHashCode());
            Assert.AreEqual(test2.GetHashCode(), test3.GetHashCode());
            Assert.AreEqual(test1.GetHashCode(), test3.GetHashCode());
        }

        [Test]
        public void NodesHashCodes()
        {
            Console.WriteLine("Tests that Literal and URI Nodes produce different Hashes");
            Console.WriteLine();

            //Create the Nodes
            Graph g = new Graph();
            IUriNode u = g.CreateUriNode(new Uri("http://www.google.com"));
            ILiteralNode l = g.CreateLiteralNode("http://www.google.com/");

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
            ILiteralNode plain = g.CreateLiteralNode("test^^http://example.org/type");
            ILiteralNode typed = g.CreateLiteralNode("test", new Uri("http://example.org/type"));

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
            IBlankNode b = g.CreateBlankNode();
            IUriNode type = g.CreateUriNode("rdf:type");
            Triple t1, t2;
            t1 = new Triple(b, type, u);
            t2 = new Triple(b, type, l);

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
        public void NodesUriNodeEquality()
        {
            //Create the Nodes
            Graph g = new Graph();
            Console.WriteLine("Creating two URIs referring to google - one lowercase, one uppercase - which should be equivalent");
            IUriNode a = g.CreateUriNode(new Uri("http://www.google.com"));
            IUriNode b = g.CreateUriNode(new Uri("http://www.GOOGLE.com/"));

            TestTools.CompareNodes(a, b, true);

            Console.WriteLine("Creating two URIs with the same Fragment ID but differing in case and thus are different since Fragment IDs are case sensitive => not equals");
            IUriNode c = g.CreateUriNode(new Uri("http://www.google.com/#Test"));
            IUriNode d = g.CreateUriNode(new Uri("http://www.GOOGLE.com/#test"));

            TestTools.CompareNodes(c, d, false);

            Console.WriteLine("Creating two identical URIs with unusual characters in them");
            IUriNode e = g.CreateUriNode(new Uri("http://www.google.com/random,_@characters"));
            IUriNode f = g.CreateUriNode(new Uri("http://www.google.com/random,_@characters"));

            TestTools.CompareNodes(e, f, true);

            Console.WriteLine("Creating two URIs with similar paths that differ in case");
            IUriNode h = g.CreateUriNode(new Uri("http://www.google.com/path/test/case"));
            IUriNode i = g.CreateUriNode(new Uri("http://www.google.com/path/Test/case"));

            TestTools.CompareNodes(h, i, false);

            Console.WriteLine("Creating three URIs with equivalent relative paths");
            IUriNode j = g.CreateUriNode(new Uri("http://www.google.com/relative/test/../example.html"));
            IUriNode k = g.CreateUriNode(new Uri("http://www.google.com/relative/test/monkey/../../example.html"));
            IUriNode l = g.CreateUriNode(new Uri("http://www.google.com/relative/./example.html"));

            TestTools.CompareNodes(j, k, true);
            TestTools.CompareNodes(k, l, true);
        }

        [Test]
        public void NodesBlankNodeEquality()
        {
            try
            {
                Graph g = new Graph();
                g.BaseUri = new Uri("http://example.org/BlankNodeEquality");
                Graph h = new Graph();
                h.BaseUri = new Uri("http://example.org/BlankNodeEqualityTwo");
                Graph i = new Graph();
                i.BaseUri = new Uri("http://example.org/BlankNodeEquality");

                Console.WriteLine("Doing some Blank Node Equality Testing");
                Console.WriteLine("Blank Nodes are equal if they have the same ID and come from the same Graph which is established by Reference Equality between the two Graphs");

                IBlankNode b = g.CreateBlankNode();
                IBlankNode c = g.CreateBlankNode();
                IBlankNode d = h.CreateBlankNode();
                IBlankNode e = i.CreateBlankNode();

                //Shouldn't be equal
                Assert.AreNotEqual(b, c, "Two Anonymous Blank Nodes created by a Graph should be non-equal");
                Assert.AreNotEqual(c, d, "Anonymous Blank Nodes created by different Graphs (even with same ID) should be non-equal");
                Assert.AreNotEqual(b, d, "Anonymous Blank Nodes created by different Graphs (even with same ID) should be non-equal");
                Assert.AreNotEqual(d, e, "Anonymous Blank Nodes created by different Graphs (even with same ID) should be non-equal");

                //Should be equal
                Assert.AreEqual(b, b, "A Blank Node should be equal to itself");
                Assert.AreEqual(c, c, "A Blank Node should be equal to itself");
                Assert.AreEqual(d, d, "A Blank Node should be equal to itself");
                Assert.AreEqual(e, e, "A Blank Node should be equal to itself");
                Assert.AreNotEqual(b, e, "First Anonymous Blank Nodes generated by two Graphs with same Graph URI should have same ID but are still not equal");

                //Named Nodes
                IBlankNode one = g.CreateBlankNode("one");
                IBlankNode two = h.CreateBlankNode("one");
                IBlankNode three = i.CreateBlankNode("one");

                Assert.AreNotEqual(one, three, "Two User defined Blank Nodes with identical IDs from two Graphs with the same Graph URI should be non-equal");
                Assert.AreNotEqual(one, two, "Two User defined Blank Nodes with identical IDs from two Graphs with different Graph URIs should be non-equal");
                Assert.AreNotEqual(two, three, "Two User defined Blank Nodes with identical IDs from two Graphs with different Graph URIs should be non-equal");
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [Test]
        public void NodesLiteralNodeEquality()
        {
            try
            {
                Graph g = new Graph();

                //Strict Mode Tests
                Console.WriteLine("Doing a load of Strict Literal Equality Tests");
                Options.LiteralEqualityMode = LiteralEqualityMode.Strict;

                //Test Literals with Language Tags
                ILiteralNode hello, helloEn, helloEnUS, helloAgain;
                hello = g.CreateLiteralNode("hello");
                helloEn = g.CreateLiteralNode("hello", "en");
                helloEnUS = g.CreateLiteralNode("hello", "en-US");
                helloAgain = g.CreateLiteralNode("hello");

                Assert.AreNotEqual(hello, helloEn, "Identical Literals with differing Language Tags are non-equal");
                Assert.AreNotEqual(hello, helloEnUS, "Identical Literals with differing Language Tags are non-equal");
                Assert.AreNotEqual(helloEn, helloEnUS, "Identical Literals with differing Language Tags are non-equal");
                Assert.AreNotEqual(helloEn, helloAgain, "Identical Literals with differing Language Tags are non-equal");
                Assert.AreNotEqual(helloEnUS, helloAgain, "Identical Literals with differing Language Tags are non-equal");

                Assert.AreEqual(hello, helloAgain, "Identical Literals with the same Language Tag are equal");

                //Test Plain Literals
                ILiteralNode plain1, plain2, plain3, plain4;
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

                ILiteralNode one1, one2, one3, one4;
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

                ILiteralNode t, f, one5;
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
            catch (Exception ex)
            {
                //Reset Literal Equality Mode
                Options.LiteralEqualityMode = LiteralEqualityMode.Strict;

                throw;
            }
            finally
            {
                //Reset Literal Equality Mode
                Options.LiteralEqualityMode = LiteralEqualityMode.Strict;
            }
        }

        [Test]
        public void NodesSorting()
        {
            //Stream for Output
            Console.WriteLine("## Sorting Test");
            Console.WriteLine("NULLs < Blank Nodes < URI Nodes < Untyped Literals < Typed Literals");
            Console.WriteLine();

            //Create a Graph
            Graph g = new Graph();
            g.BaseUri = new Uri("http://example.org/");
            g.NamespaceMap.AddNamespace("", new Uri("http://example.org/"));

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
            nodes.Add(g.CreateBlankNode("monkey"));
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
                if (n == null)
                {
                    Console.WriteLine("NULL");
                }
                else
                {
                    Console.WriteLine(n.ToString());
                }
            }

            Console.WriteLine();
            Console.WriteLine("Now in reverse...");
            Console.WriteLine();

            nodes.Reverse();

            //Output the Results
            foreach (INode n in nodes)
            {
                if (n == null)
                {
                    Console.WriteLine("NULL");
                }
                else
                {
                    Console.WriteLine(n.ToString());
                }
            }
        }

        [Test]
        public void NodesSortingSparqlOrder()
        {
            SparqlOrderingComparer comparer = new SparqlOrderingComparer();

            //Stream for Output
            Console.WriteLine("## Sorting Test");
            Console.WriteLine("NULLs < Blank Nodes < URI Nodes < Untyped Literals < Typed Literals");
            Console.WriteLine();

            //Create a Graph
            Graph g = new Graph();
            g.BaseUri = new Uri("http://example.org/");
            g.NamespaceMap.AddNamespace("", new Uri("http://example.org/"));

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
            nodes.Add(g.CreateBlankNode("monkey"));
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

            nodes.Sort(comparer);

            //Output the Results
            foreach (INode n in nodes)
            {
                if (n == null)
                {
                    Console.WriteLine("NULL");
                }
                else
                {
                    Console.WriteLine(n.ToString());
                }
            }

            Console.WriteLine();
            Console.WriteLine("Now in reverse...");
            Console.WriteLine();

            nodes.Reverse();

            //Output the Results
            foreach (INode n in nodes)
            {
                if (n == null)
                {
                    Console.WriteLine("NULL");
                }
                else
                {
                    Console.WriteLine(n.ToString());
                }
            }
        }

        [Test]
        public void NodesNullNodeEquality()
        {
            UriNode nullUri = null;
            LiteralNode nullLiteral = null;
            BlankNode nullBNode = null;

            Graph g = new Graph();
            IUriNode someUri = g.CreateUriNode(new Uri("http://example.org"));
            ILiteralNode someLiteral = g.CreateLiteralNode("A Literal");
            IBlankNode someBNode = g.CreateBlankNode();

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
        }

        [Test]
        public void GraphMerging()
        {
            try
            {
                //Load the Test RDF
                TurtleParser ttlparser = new TurtleParser();
                Graph g = new Graph();
                Graph h = new Graph();
                Assert.IsNotNull(g);
                Assert.IsNotNull(h);
                ttlparser.Load(g, "..\\resources\\MergePart1.ttl");
                ttlparser.Load(h, "..\\resources\\MergePart2.ttl");

                Console.WriteLine("Merge Test Data Loaded OK");
                Console.WriteLine();

                Console.WriteLine("Graph 1 Contains");
                foreach (Triple t in g.Triples)
                {
                    Console.WriteLine(t.ToString());
                }

                Console.WriteLine();
                Console.WriteLine("Graph 2 Contains");
                foreach (Triple t in h.Triples)
                {
                    Console.WriteLine(t.ToString());
                }

                Console.WriteLine();

                Console.WriteLine("Attempting Graph Merge");
                g.Merge(h);
                Console.WriteLine();

                foreach (Triple t in g.Triples)
                {
                    Console.WriteLine(t.ToString());
                }

                Assert.AreEqual(8, g.Triples.Count, "Expected 8 Triples after the Merge");

                //Same merge into an Empty Graph
                Console.WriteLine();
                Console.WriteLine("Combining the two Graphs with two Merge operations into an Empty Graph");
                Graph i = new Graph();

                //Need to reload g from disk
                g = new Graph();
                ttlparser.Load(g, "..\\resources\\MergePart1.ttl");

                //Do the actual merge
                i.Merge(g);
                i.Merge(h);
                Console.WriteLine();

                foreach (Triple t in i.Triples)
                {
                    Console.WriteLine(t.ToString());
                }

                Assert.AreEqual(8, i.Triples.Count, "Expected 8 Triples after the Merge");
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [Test]
        public void GraphTripleCreation()
        {
            //Create two Graphs
            Graph g = new Graph();
            Graph h = new Graph();

            g.NamespaceMap.AddNamespace("ex", new Uri("http://example.org/"));
            h.NamespaceMap.AddNamespace("ex", new Uri("http://example.org/"));

            //Create a Triple in First Graph
            g.Assert(g.CreateBlankNode(), g.CreateUriNode("rdf:type"), g.CreateUriNode("ex:Triple"));
            Assert.AreEqual(1, g.Triples.Count, "Should have 1 Triple in the Graph");

            //Create a Triple in Second Graph
            h.Assert(h.CreateBlankNode(), h.CreateUriNode("rdf:type"), h.CreateUriNode("ex:Triple"));
            Assert.AreEqual(1, h.Triples.Count, "Should have 1 Triple in the Graph");
        }

#if PORTABLE
        [TestCase("The following String needs URL Encoding <node>Test</node> 100% not a percent encode", "The%20following%20String%20needs%20URL%20Encoding%20%3Cnode%3ETest%3C%2Fnode%3E%20100%25%20not%20a%20percent%20encode")]
        [TestCase("This string contains UTF-8 納豆 characters", "This%20string%20contains%20UTF-8%20%E7%B4%8D%E8%B1%86%20characters")]
        [TestCase("This string contains UTF-8 ç´è± characters", "This%20string%20contains%20UTF-8%20%C3%A7%C2%B4%C2%8D%C3%A8%C2%B1%C2%86%20characters")]
        [TestCase("This string has safe characters -._~", "This%20string%20has%20safe%20characters%20-._~")]
        public void UriEncoding(string test, string expectedEncoded)
        {
            string encoded = HttpUtility.UrlEncode(test);
            string encodedTwice = HttpUtility.UrlEncode(encoded); 
            string decoded = HttpUtility.UrlDecode(encoded);
            string decodedTwice = HttpUtility.UrlDecode(decoded);

            Console.WriteLine("Encoded once:  {0}", encoded);
            Console.WriteLine("Encoded twice: {0}", encodedTwice);
            Console.WriteLine("Decoded once:  {0}", decoded);
            Console.WriteLine("Decoded twice: {0}", decodedTwice);

            Assert.That(encoded, Is.EqualTo(expectedEncoded));
            Assert.That(encodedTwice, Is.EqualTo(expectedEncoded));
            Assert.That(decoded, Is.EqualTo(test));
            Assert.That(decodedTwice, Is.EqualTo(test));
        }
#endif

        [Test]
        public void UriPathAndQuery()
        {
            Uri u = new Uri("http://example.org/some/path/with?query=some&param=values");

            String pathAndQuery = u.PathAndQuery;
            String absPathPlusQuery = u.AbsolutePath + u.Query;

            Console.WriteLine("PathAndQuery - " + pathAndQuery);
            Console.WriteLine("AbsolutePath + Query - " + absPathPlusQuery);

            Assert.AreEqual(pathAndQuery, absPathPlusQuery);
        }

        [Test]
        public void UriQuery()
        {
            Uri withQuery = new Uri("http://example.org/with?some=query");
            Uri withoutQuery = new Uri("http://example.org/without");

            Assert.AreNotEqual(String.Empty, withQuery.Query);
            Assert.AreEqual(String.Empty, withoutQuery.Query);
        }

        //[Test]
        //public void UriTrailingDot()
        //{
        //    Uri u = new Uri("http://example.org/path.");
        //    Console.WriteLine(u.ToString());
        //    Console.WriteLine("Is IRI? " + IriSpecsHelper.IsIri("http://example.org/path.").ToString());

        //    foreach (PropertyInfo info in u.GetType().GetProperties())
        //    {
        //        Console.WriteLine(info.Name + " = " + info.GetValue(u, null));
        //    }
        //    Console.WriteLine();

        //    MethodInfo getSyntax = typeof(UriParser).GetMethod("GetSyntax", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
        //    FieldInfo flagsField = typeof(UriParser).GetField("m_Flags", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        //    if (getSyntax != null && flagsField != null)
        //    {
        //        foreach (string scheme in new[] { "http", "https" })
        //        {
        //            UriParser parser = (UriParser)getSyntax.Invoke(null, new object[] { scheme });
        //            if (parser != null)
        //            {
        //                int flagsValue = (int)flagsField.GetValue(parser);
        //                // Clear the CanonicalizeAsFilePath attribute
        //                if ((flagsValue & 0x1000000) != 0)
        //                    flagsField.SetValue(parser, flagsValue & ~0x1000000);
        //            }
        //        }
        //    }
        //    Uri v = new Uri("http://example.org/path.");
        //    Console.WriteLine(v.ToString());

        //    foreach (PropertyInfo info in v.GetType().GetProperties())
        //    {
        //        Console.WriteLine(info.Name + " = " + info.GetValue(v, null));
        //    }
        //}
    }
}
