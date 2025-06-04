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
using System.Globalization;
using System.IO;
using System.Linq;
using Xunit;
using VDS.RDF.Writing;
using VDS.RDF.Parsing;
using VDS.RDF.Query;

#pragma warning disable 252,253

#pragma warning disable xUnit2000 // Expected value should be first
#pragma warning disable xUnit2003 // Do not use Assert.Equal to check for null value
#pragma warning disable CS1718 // Comparison made to same variable
// ReSharper disable EqualExpressionComparison

namespace VDS.RDF;


public class BasicTests1 : BaseTest
{
    public BasicTests1(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public void NodesDistinct()
    {
        var g = new Graph();
        var test = new List<INode>()
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

    [Fact]
    public void GraphCreation1()
    {
        //Create a new Empty Graph
        var g = new Graph();
        Assert.NotNull(g);

        //Define Namespaces
        g.NamespaceMap.AddNamespace("vds", new Uri("http://www.vdesign-studios.com/dotNetRDF#"));
        g.NamespaceMap.AddNamespace("ecs", new Uri("http://id.ecs.soton.ac.uk/person/"));

        //Check we set the Namespace OK
        Assert.True(g.NamespaceMap.HasNamespace("vds"), "Failed to set a Namespace");

        //Set Base Uri
        g.BaseUri = g.NamespaceMap.GetNamespaceUri("vds");
        Assert.NotNull(g.BaseUri);
        Assert.Equal(g.NamespaceMap.GetNamespaceUri("vds"), g.BaseUri);

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
            var ttlwriter = new CompressingTurtleWriter();
            ttlwriter.Save(g, "graph_building_example.ttl");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);
        }
    }

    [Fact]
    public void GraphCreation2()
    {
        //Create a new Empty Graph
        var g = new Graph();
        Assert.NotNull(g);

        //Define Namespaces
        g.NamespaceMap.AddNamespace("pets", new Uri("http://example.org/pets"));
        Assert.True(g.NamespaceMap.HasNamespace("pets"));

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

        Assert.Equal(5, g.Triples.Count);
    }

    [Fact]
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

        for (var i = 0; i < baseUris.Length; i++)
        {
            Console.WriteLine("Resolving against Base URI " + baseUris[i]);

            var baseUri = new Uri(baseUris[i]);

            for (var j = 0; j < uriRefs.Length; j++)
            {
                Console.WriteLine("Resolving " + uriRefs[j]);

                String result, expectedResult;
                result = Tools.ResolveUri(uriRefs[j], baseUris[i]);
                expectedResult = expected[i][j];

                Console.WriteLine("Expected: " + expectedResult);
                Console.WriteLine("Actual: " + result);

                Assert.Equal(expectedResult, result);
            }

            Console.WriteLine();
        }

        var mailto = new Uri("mailto:example@example.org");
        var rel = new Uri("/some/folder", UriKind.Relative);
        var res = new Uri(mailto, rel);
        Console.WriteLine(res.ToString());
    }

    [Fact]
    public void UriResolutionWithGraphBase()
    {
        IGraph g = new Graph
        {
            BaseUri = new Uri("http://example.org/")
        };

        var expected = new Uri("http://example.org/relative/path");
        IUriNode actual = g.CreateUriNode(new Uri("relative/path", UriKind.Relative));
        Assert.Equal(expected, actual.Uri);
    }

    [Fact]
    public void UriResolutionUriProvidedToQNameMethod()
    {
        IGraph g = new Graph();
        var ex = Assert.Throws<RdfException>(() => g.CreateUriNode("http://example.org"));
     
        TestTools.ReportError("Error", ex);
    }

    [Fact]
    public void UriHashCodes()
    {
        //Quick Test to see if how the Uri classes Hash Codes behave
        var test1 = new Uri("http://example.org/test#one");
        var test2 = new Uri("http://example.org/test#two");
        var test3 = new Uri("http://example.org/test#three");

        Console.WriteLine("Three identical URIs with different Fragment IDs, .Net ignores the Fragments in creating Hash Codes");
        Console.WriteLine("URI 1 has Hash Code " + test1.GetHashCode());
        Console.WriteLine("URI 2 has Hash Code " + test2.GetHashCode());
        Console.WriteLine("URI 3 has Hash Code " + test3.GetHashCode());

        Assert.Equal(test1.GetHashCode(), test2.GetHashCode());
        Assert.Equal(test2.GetHashCode(), test3.GetHashCode());
        Assert.Equal(test1.GetHashCode(), test3.GetHashCode());
    }

    [Fact]
    public void NodesHashCodes()
    {
        Console.WriteLine("Tests that Literal and URI Nodes produce different Hashes");
        Console.WriteLine();

        //Create the Nodes
        var g = new Graph();
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

        Assert.NotEqual(u.GetHashCode(), l.GetHashCode());
        Assert.NotSame(u, l);
        //Assert.NotEqual(u, l);

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

        Assert.NotEqual(plain.GetHashCode(), typed.GetHashCode());
        Assert.NotEqual(plain, typed);

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

        Assert.NotEqual(t1.GetHashCode(), t2.GetHashCode());
        Assert.NotEqual(t1, t2);

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

        Assert.NotEqual(t1.GetHashCode(), t2.GetHashCode());
        Assert.NotEqual(t1, t2);

    }

    [Fact]
    public void NodesUriNodeEquality()
    {
        //Create the Nodes
        var g = new Graph();
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

    [Fact]
    public void NodesBlankNodeEquality()
    {
        var g = new Graph
        {
            BaseUri = new Uri("http://example.org/BlankNodeEquality")
        };
        var h = new Graph
        {
            BaseUri = new Uri("http://example.org/BlankNodeEqualityTwo")
        };
        var i = new Graph
        {
            BaseUri = new Uri("http://example.org/BlankNodeEquality")
        };

        IBlankNode b = g.CreateBlankNode();
        IBlankNode c = g.CreateBlankNode();
        IBlankNode d = h.CreateBlankNode();
        IBlankNode e = i.CreateBlankNode();

        //Shouldn't be equal
        Assert.NotEqual(b, c);
        Assert.NotEqual(c, d);
        Assert.NotEqual(b, d);
        Assert.NotEqual(d, e);
        Assert.NotEqual(b, e);

        //Should be equal
        Assert.Equal(b, b);
        Assert.Equal(c, c);
        Assert.Equal(d, d);
        Assert.Equal(e, e);

        //Named Nodes
        // Named nodes are equal if they have the same ID
        IBlankNode one = g.CreateBlankNode("one");
        IBlankNode two = h.CreateBlankNode("one");
        IBlankNode three = i.CreateBlankNode("one");

        Assert.Equal(one, three);
        Assert.Equal(one, two);
        Assert.Equal(two, three);
    }

    [Fact]
    public void NodesLiteralNodeEquality()
    {
        try
        {
            var g = new Graph();

            //Strict Mode Tests
            Console.WriteLine("Doing a load of Strict Literal Equality Tests");
            EqualityHelper.LiteralEqualityMode = LiteralEqualityMode.Strict;

            //Test Literals with Language Tags
            ILiteralNode hello, helloEn, helloEnUS, helloAgain;
            hello = g.CreateLiteralNode("hello");
            helloEn = g.CreateLiteralNode("hello", "en");
            helloEnUS = g.CreateLiteralNode("hello", "en-US");
            helloAgain = g.CreateLiteralNode("hello");

            Assert.NotEqual(hello, helloEn);
            Assert.NotEqual(hello, helloEnUS);
            Assert.NotEqual(helloEn, helloEnUS);
            Assert.NotEqual(helloEn, helloAgain);
            Assert.NotEqual(helloEnUS, helloAgain);

            Assert.Equal(hello, helloAgain);

            //Test Plain Literals
            ILiteralNode plain1, plain2, plain3, plain4;
            plain1 = g.CreateLiteralNode("plain literal");
            plain2 = g.CreateLiteralNode("another plain literal");
            plain3 = g.CreateLiteralNode("Plain Literal");
            plain4 = g.CreateLiteralNode("plain literal");

            Assert.NotEqual(plain1, plain2);
            Assert.NotEqual(plain1, plain3);
            Assert.Equal(plain1, plain4);
            Assert.NotEqual(plain2, plain3);
            Assert.NotEqual(plain2, plain4);
            Assert.NotEqual(plain3, plain4);

            //Typed Literals
            var intType = new Uri(XmlSpecsHelper.XmlSchemaDataTypeInteger);
            var boolType = new Uri(XmlSpecsHelper.XmlSchemaDataTypeBoolean);

            ILiteralNode one1, one2, one3, one4;
            one1 = g.CreateLiteralNode("1");
            one2 = g.CreateLiteralNode("1", intType);
            one3 = g.CreateLiteralNode("0001", intType);
            one4 = g.CreateLiteralNode("1", intType);

            Assert.NotEqual(one1, one2);
            Assert.NotEqual(one1, one3);
            Assert.NotEqual(one1, one4);
            Assert.NotEqual(one2, one3);
            Assert.Equal(one2, one4);
            Assert.NotEqual(one3, one4);

            Assert.NotEqual(0, one1.CompareTo(one2));
            Assert.Equal(0, one2.CompareTo(one3));
            Assert.Equal(0, one3.CompareTo(one2));
            Assert.Equal(0, one3.CompareTo(one4));

            ILiteralNode t, f, one5;
            t = g.CreateLiteralNode("true", boolType);
            f = g.CreateLiteralNode("false", boolType);
            one5 = g.CreateLiteralNode("1", boolType);

            Assert.NotEqual(t, f);
            Assert.Equal(t, t);
            Assert.Equal(f, f);

            Assert.NotEqual(t, one5);

            //Loose Mode Tests
            Console.WriteLine("Doing a load of Loose Equality Tests");
            EqualityHelper.LiteralEqualityMode = LiteralEqualityMode.Loose;

            Assert.Equal(one2, one3);
            Assert.Equal(one3, one4);
            Assert.NotEqual(t, one5);

        }
        finally
        {
            //Reset Literal Equality Mode
            EqualityHelper.LiteralEqualityMode = LiteralEqualityMode.Strict;
        }
    }

    [Fact]
    public void NodesSorting()
    {
        //Stream for Output
        Console.WriteLine("## Sorting Test");
        Console.WriteLine("NULLs < Blank Nodes < URI Nodes < Untyped Literals < Typed Literals");
        Console.WriteLine();

        //Create a Graph
        var g = new Graph
        {
            BaseUri = new Uri("http://example.org/")
        };
        g.NamespaceMap.AddNamespace("", new Uri("http://example.org/"));

        //Create a list of various Nodes
        var nodes = new List<INode>();
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

        for (var i = 0; i < 32; i++)
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

    [Fact]
    public void NodesSortingSparqlOrder()
    {
        var comparer = new SparqlOrderingComparer(CultureInfo.InvariantCulture, CompareOptions.Ordinal);

        //Stream for Output
        Console.WriteLine("## Sorting Test");
        Console.WriteLine("NULLs < Blank Nodes < URI Nodes < Untyped Literals < Typed Literals");
        Console.WriteLine();

        //Create a Graph
        var g = new Graph
        {
            BaseUri = new Uri("http://example.org/")
        };
        g.NamespaceMap.AddNamespace("", new Uri("http://example.org/"));

        //Create a list of various Nodes
        var nodes = new List<INode>();
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

    [Fact]
    public void NodesNullNodeEquality()
    {
        UriNode nullUri = null;
        LiteralNode nullLiteral = null;
        BlankNode nullBNode = null;

        var g = new Graph();
        IUriNode someUri = g.CreateUriNode(new Uri("http://example.org"));
        ILiteralNode someLiteral = g.CreateLiteralNode("A Literal");
        IBlankNode someBNode = g.CreateBlankNode();

        Assert.Equal(nullUri, nullUri);
        Assert.Equal(nullUri, null);
        Assert.Equal(null, nullUri);
        Assert.True(nullUri == nullUri, "Null URI Node should be equal to self");
        Assert.False(nullUri != nullUri, "Null URI Node should be equal to self");
        Assert.True(nullUri == null, "Null URI Node should be equal to a null");
        Assert.True(null == nullUri, "Null should be equal to a Null URI Node");
        Assert.False(nullUri != null, "Null URI Node should be equal to a null");
        Assert.False(null != nullUri, "Null should be equal to a Null URI Node");
        Assert.NotEqual(nullUri, someUri);
        Assert.NotEqual(someUri, nullUri);
        Assert.False(nullUri == someUri, "Null URI Node should not be equal to an actual URI Node");
        Assert.False(someUri == nullUri, "Null URI Node should not be equal to an actual URI Node");

        Assert.Equal(nullLiteral, nullLiteral);
        Assert.Equal(nullLiteral, null);
        Assert.Equal(null, nullLiteral);
        Assert.True(nullLiteral == nullLiteral, "Null Literal Node should be equal to self");
        Assert.True(nullLiteral == null, "Null Literal Node should be equal to a null");
        Assert.True(null == nullLiteral, "Null should be equal to a Null Literal Node");
        Assert.False(nullLiteral != nullLiteral, "Null Literal Node should be equal to self");
        Assert.False(nullLiteral != null, "Null Literal Node should be equal to a null");
        Assert.False(null != nullLiteral, "Null should be equal to a Null Literal Node");
        Assert.NotEqual(nullLiteral, someLiteral);
        Assert.NotEqual(someLiteral, nullLiteral);
        Assert.False(nullLiteral == someLiteral, "Null Literal Node should not be equal to an actual Literal Node");
        Assert.False(someLiteral == nullLiteral, "Null Literal Node should not be equal to an actual Literal Node");

        Assert.Equal(nullBNode, nullBNode);
        Assert.Equal(nullBNode, null);
        Assert.Equal(null, nullBNode);
        Assert.True(nullBNode == nullBNode, "Null BNode Node should be equal to self");
        Assert.True(nullBNode == null, "Null BNode Node should be equal to a null");
        Assert.True(null == nullBNode, "Null should be equal to a Null BNode Node");
        Assert.False(nullBNode != nullBNode, "Null BNode Node should be equal to self");
        Assert.False(nullBNode != null, "Null BNode Node should be equal to a null");
        Assert.False(null != nullBNode, "Null should be equal to a Null BNode Node");
        Assert.NotEqual(nullBNode, someBNode);
        Assert.NotEqual(someBNode, nullBNode);
        Assert.False(nullBNode == someBNode, "Null BNode Node should not be equal to an actual BNode Node");
        Assert.False(someBNode == nullBNode, "Null BNode Node should not be equal to an actual BNode Node");
    }

    [Fact]
    public void GraphMerging()
    {
        //Load the Test RDF
        var parser = new TurtleParser();
        var g = new Graph();
        var h = new Graph();
        Assert.NotNull(g);
        Assert.NotNull(h);
        parser.Load(g, Path.Combine("resources", "MergePart1.ttl"));
        parser.Load(h, Path.Combine("resources", "MergePart2.ttl"));
        g.Merge(h);
        Assert.Equal(8, g.Triples.Count);

        //Same merge into an Empty Graph
        var i = new Graph();

        //Need to reload g from disk
        g = new Graph();
        parser.Load(g,  Path.Combine("resources", "MergePart1.ttl"));

        //Do the actual merge
        i.Merge(g);
        i.Merge(h);

        Assert.Equal(8, i.Triples.Count);
    }

    [Fact]
    public void GraphTripleCreation()
    {
        //Create two Graphs
        var g = new Graph();
        var h = new Graph();

        g.NamespaceMap.AddNamespace("ex", new Uri("http://example.org/"));
        h.NamespaceMap.AddNamespace("ex", new Uri("http://example.org/"));

        //Create a Triple in First Graph
        g.Assert(g.CreateBlankNode(), g.CreateUriNode("rdf:type"), g.CreateUriNode("ex:Triple"));
        Assert.Equal(1, g.Triples.Count);

        //Create a Triple in Second Graph
        h.Assert(h.CreateBlankNode(), h.CreateUriNode("rdf:type"), h.CreateUriNode("ex:Triple"));
        Assert.Equal(1, h.Triples.Count);
    }


    [Fact]
    public void UriPathAndQuery()
    {
        var u = new Uri("http://example.org/some/path/with?query=some&param=values");

        var pathAndQuery = u.PathAndQuery;
        var absPathPlusQuery = u.AbsolutePath + u.Query;

        Console.WriteLine("PathAndQuery - " + pathAndQuery);
        Console.WriteLine("AbsolutePath + Query - " + absPathPlusQuery);

        Assert.Equal(pathAndQuery, absPathPlusQuery);
    }

    [Fact]
    public void UriQuery()
    {
        var withQuery = new Uri("http://example.org/with?some=query");
        var withoutQuery = new Uri("http://example.org/without");

        Assert.NotEqual(String.Empty, withQuery.Query);
        Assert.Equal(String.Empty, withoutQuery.Query);
    }

    //[Fact]
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
