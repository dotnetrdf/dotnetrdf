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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Ordering;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query
{
    [TestClass]
    public class SparqlOrderByTests
    {
        [TestMethod]
        public void SparqlOrderByWithRawVariableName()
        {
            // when
            var ordering = new OrderByVariable("name");

            // then
            Assert.AreEqual("name", ordering.Variables.Single());
        }

        [TestMethod]
        public void SparqlOrderByWithVariableNameWithDollarSign()
        {
            // when
            var ordering = new OrderByVariable("$name");

            // then
            Assert.AreEqual("name", ordering.Variables.Single());
        }

        [TestMethod]
        public void SparqlOrderByWithVariableNameWithQuestionMark()
        {
            // when
            var ordering = new OrderByVariable("?name");

            // then
            Assert.AreEqual("name", ordering.Variables.Single());
        }

        [TestMethod]
        public void SparqlOrderByDescendingScope()
        {
            //Test Case for CORE-350

            IGraph g = new Graph();
            g.NamespaceMap.AddNamespace(String.Empty, UriFactory.Create("http://example/"));
            INode a = g.CreateUriNode(":a");
            INode b = g.CreateUriNode(":b");
            INode c = g.CreateUriNode(":c");

            List<INode> nodes = new List<INode>() { a, b, c };
            List<Triple> ts = new List<Triple>();
            foreach (INode s in nodes)
            {
                foreach (INode p in nodes.OrderByDescending(x => x))
                {
                    foreach (INode o in nodes)
                    {
                        ts.Add(new Triple(s, p, o));
                    }
                }
            }
            g.Assert(ts);
            Assert.AreEqual(27, g.Triples.Count);

            String query = @"SELECT * WHERE { ?s ?p ?o } ORDER BY ?s DESC(?p) ?o";
            SparqlQuery q = new SparqlQueryParser().ParseFromString(query);

            SparqlResultSet results = g.ExecuteQuery(q) as SparqlResultSet;
            Assert.IsNotNull(results);
            Assert.AreEqual(27, results.Count);

            for (int i = 0; i < ts.Count; i++)
            {
                Triple t = new Triple(results[i]["s"], results[i]["p"], results[i]["o"]);
                Assert.AreEqual(ts[i], t, "Element at position " + i + " is not as expected");
            }
        }
    }
}