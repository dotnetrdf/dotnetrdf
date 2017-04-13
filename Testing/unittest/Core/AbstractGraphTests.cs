/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2013 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

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
using Xunit;
using VDS.RDF.Parsing;

namespace VDS.RDF
{
    /// <summary>
    /// Abstract set of tests that can be used to test any <see cref="IGraph" /> implementation
    /// </summary>

    public abstract class AbstractGraphTests
    {
        /// <summary>
        /// Method which derived tests should implement to provide a fresh instance that can be used for testing
        /// </summary>
        /// <returns></returns>
        protected abstract IGraph GetInstance();

        [Fact]
        public void GraphIsEmpty01()
        {
            IGraph g = this.GetInstance();
            Assert.True(g.IsEmpty);
        }

        [Fact]
        public void GraphIsEmpty02()
        {
            IGraph g = this.GetInstance();

            g.Assert(new Triple(g.CreateBlankNode(), g.CreateBlankNode(), g.CreateBlankNode()));
            Assert.False(g.IsEmpty);
        }

        [Fact]
        public void GraphAssert01()
        {
            IGraph g = this.GetInstance();
            g.NamespaceMap.AddNamespace(String.Empty, UriFactory.Create("http://example/"));

            Triple t = new Triple(g.CreateUriNode(":s"), g.CreateUriNode(":p"), g.CreateBlankNode(":o"));
            g.Assert(t);
            Assert.False(g.IsEmpty);
            Assert.True(g.ContainsTriple(t));
        }

        [Fact]
        public void GraphRetract01()
        {
            IGraph g = this.GetInstance();
            g.NamespaceMap.AddNamespace(String.Empty, UriFactory.Create("http://example/"));

            Triple t = new Triple(g.CreateUriNode(":s"), g.CreateUriNode(":p"), g.CreateBlankNode(":o"));
            g.Assert(t);
            Assert.False(g.IsEmpty);
            Assert.True(g.ContainsTriple(t));

            g.Retract(t);
            Assert.True(g.IsEmpty);
            Assert.False(g.ContainsTriple(t));
        }

        [Fact]
        public void GraphRetract02()
        {
            IGraph g = this.GetInstance();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");

            Assert.False(g.IsEmpty);

            INode rdfType = g.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfType));
            Assert.True(g.GetTriplesWithPredicate(rdfType).Any());

            g.Retract(g.GetTriplesWithPredicate(rdfType).ToList());
            Assert.False(g.GetTriplesWithPredicate(rdfType).Any());
        }
    }

    public class GraphTests
        : AbstractGraphTests
    {
        protected override IGraph GetInstance()
        {
            return new Graph();
        }
    }


    public class ThreadSafeGraphTests
        : AbstractGraphTests
    {
        protected override IGraph GetInstance()
        {
            return new ThreadSafeGraph();
        }
    }


    public class NonIndexedGraphTests
        : AbstractGraphTests
    {
        protected override IGraph GetInstance()
        {
            return new NonIndexedGraph();
        }
    }

}
