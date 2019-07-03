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
using System.Text;
using System.Net;
using Xunit;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Writing.Formatting;

#pragma warning disable CS1718 // Comparison made to same variable

namespace VDS.RDF
{
    public partial class BasicTests2 : BaseTest
    {
        [Fact]
        public void GraphWithBNodeEquality()
        {
            Console.WriteLine("Testing Graph Equality when the Graphs have Blank Nodes");
            Graph g = new Graph();
            Graph h = new Graph();

            TurtleParser ttlparser = new TurtleParser();
            ttlparser.Load(g, "resources\\MergePart1.ttl");
            ttlparser.Load(h, "resources\\MergePart1.ttl");

            Assert.Equal(g.BaseUri, h.BaseUri);
            //TestTools.CompareGraphs(g, h, true);
            Dictionary<INode, INode> mapping;
            bool equals = g.Equals(h, out mapping);
            Assert.True(@equals, "Graphs should have been equal");
            if (mapping != null)
            {
                Console.WriteLine("Blank Node Mapping was:");
                foreach (KeyValuePair<INode, INode> pair in mapping)
                {
                    Console.WriteLine(pair.Key.ToString() + " => " + pair.Value.ToString());
                }
            }
        }

        [Fact]
        public void NodesEqualityOperator()
        {
            Console.WriteLine("Testing that the overridden operators for Nodes work as expected");

            Graph g = new Graph();
            IBlankNode a = g.CreateBlankNode();
            IBlankNode b = g.CreateBlankNode();

            Console.WriteLine("Testing using Equals() method");
            Assert.False(a.Equals(b), "Two different Blank Nodes should be non-equal");
            Assert.True(a.Equals(a), "A Blank Node should be equal to itself");
            Assert.True(b.Equals(b), "A Blank Node should be equal to itself");
            Console.WriteLine("OK");

            Console.WriteLine();

            Console.WriteLine("Testing using == operator");
            Assert.False(a == b, "Two different Blank Nodes should be non-equal");
            Assert.True(a == a, "A Blank Node should be equal to itself");
            Assert.True(b == b, "A Blank Node should be equal to itself");
            Console.WriteLine("OK");

            Console.WriteLine();

            //Test typed as INode
            INode c = g.CreateBlankNode();
            INode d = g.CreateBlankNode();

            Console.WriteLine("Now testing with typed as INode using Equals()");
            Assert.False(c.Equals(d), "Two different Nodes should be non-equal");
            Assert.True(c.Equals(c), "A Node should be equal to itself");
            Assert.True(d.Equals(d), "A Node should be equal to itself");
            Console.WriteLine("OK");

            Console.WriteLine();

            Console.WriteLine("Now testing with typed as INode using == operator");
            Assert.False(c == d, "Two different Nodes should be non-equal");
            Assert.True(c == c, "A Node should be equal to itself");
            Assert.True(d == d, "A Node should be equal to itself");
            Console.WriteLine("OK");
        }

        [Fact]
        public void GraphPersistenceWrapperNodeCreation()
        {
            Graph g = new Graph();
            GraphPersistenceWrapper wrapper = new GraphPersistenceWrapper(g);

            INode s = wrapper.CreateBlankNode();
            INode p = wrapper.CreateUriNode("rdf:type");
            INode o = wrapper.CreateUriNode("rdfs:Class");

            wrapper.Assert(s, p, o);
        }
    }
}
