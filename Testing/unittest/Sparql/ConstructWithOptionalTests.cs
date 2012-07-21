/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Update;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Test.Sparql
{

    [TestClass]
    public class ConstructWithOptionalTests
    {
        private SparqlQueryParser _parser = new SparqlQueryParser();
        private SparqlUpdateParser _updateParser = new SparqlUpdateParser();

        private ISparqlDataset AsDataset(IInMemoryQueryableStore store)
        {
            if (store.Graphs.Count == 1)
            {
                return new InMemoryDataset(store, store.Graphs.First().BaseUri);
            }
            else
            {
                return new InMemoryDataset(store);
            }
        }

        private void TestConstruct(IGraph data, IGraph expected, String query)
        {
            TripleStore store = new TripleStore();
            store.Add(data);

            this.TestConstruct(store, expected, query);
        }

        private void TestConstruct(IInMemoryQueryableStore store, IGraph expected, String query)
        {
            SparqlQuery q = this._parser.ParseFromString(query);

            LeviathanQueryProcessor processor = new LeviathanQueryProcessor(AsDataset(store));
            Object results = processor.ProcessQuery(q);
            if (results is IGraph)
            {
                IGraph result = (IGraph)results;

                NTriplesFormatter formatter = new NTriplesFormatter();
                Console.WriteLine("Result Data");
                foreach (Triple t in result.Triples)
                {
                    Console.WriteLine(t.ToString(formatter));
                }
                Console.WriteLine();

                Console.WriteLine("Expected Data");
                foreach (Triple t in expected.Triples)
                {
                    Console.WriteLine(t.ToString(formatter));
                }
                Console.WriteLine();

                Assert.AreEqual(expected, result, "Graphs should be equal");
            }
            else
            {
                Assert.Fail("Did not get a Graph as expected");
            }
        }

        private void TestUpdate(IGraph data, IGraph expected, String update)
        {
            TripleStore store = new TripleStore();
            store.Add(data);

            this.TestUpdate(store, expected, update);
        }

        private void TestUpdate(IInMemoryQueryableStore store, IGraph expected, String update)
        {
            SparqlUpdateCommandSet cmds = this._updateParser.ParseFromString(update);

            LeviathanUpdateProcessor processor = new LeviathanUpdateProcessor(store);
            processor.ProcessCommandSet(cmds);

            Assert.IsTrue(store.HasGraph(null), "Store should have a default unnamed Graph");
            IGraph result = store.Graph(null);
            
            NTriplesFormatter formatter = new NTriplesFormatter();
            Console.WriteLine("Result Data");
            foreach (Triple t in result.Triples)
            {
                Console.WriteLine(t.ToString(formatter));
            }
            Console.WriteLine();

            Console.WriteLine("Expected Data");
            foreach (Triple t in expected.Triples)
            {
                Console.WriteLine(t.ToString(formatter));
            }
            Console.WriteLine();

            Assert.AreEqual(expected, result, "Graphs should be equal");
        }

        [TestMethod]
        public void SparqlConstructWithOptional()
        {
            Graph g = new Graph();
            g.LoadFromFile("InferenceTest.ttl");

            Graph expected = new Graph();
            expected.Assert(g.GetTriplesWithPredicate(g.CreateUriNode("rdf:type")));
            expected.Assert(g.GetTriplesWithPredicate(g.CreateUriNode("eg:Speed")));

            String query = "PREFIX ex: <http://example.org/vehicles/> CONSTRUCT { ?s a ?type . ?s ex:Speed ?speed } WHERE { ?s a ?type . OPTIONAL { ?s ex:Speed ?speed } }";

            this.TestConstruct(g, expected, query);
        }

        [TestMethod]
        public void SparqlUpdateInsertWithOptional()
        {
            Graph g = new Graph();
            g.LoadFromFile("InferenceTest.ttl");
            g.BaseUri = new Uri("http://example.org/vehicles/");

            Graph expected = new Graph();
            expected.Assert(g.GetTriplesWithPredicate(g.CreateUriNode("rdf:type")));
            expected.Assert(g.GetTriplesWithPredicate(g.CreateUriNode("eg:Speed")));

            String update = "PREFIX ex: <http://example.org/vehicles/> INSERT { ?s a ?type . ?s ex:Speed ?speed } USING <http://example.org/vehicles/> WHERE { ?s a ?type . OPTIONAL { ?s ex:Speed ?speed } }";

            this.TestUpdate(g, expected, update);
        }

        [TestMethod]
        public void SparqlUpdateDeleteWithOptional()
        {
            Graph g = new Graph();
            g.LoadFromFile("InferenceTest.ttl");
            g.BaseUri = new Uri("http://example.org/vehicles/");

            Graph def = new Graph();
            def.Merge(g);

            TripleStore store = new TripleStore();
            store.Add(g);
            store.Add(def);

            Graph expected = new Graph();
            expected.NamespaceMap.Import(g.NamespaceMap);
            expected.Merge(g);
            expected.Retract(expected.GetTriplesWithPredicate(expected.CreateUriNode("rdf:type")).ToList());
            expected.Retract(expected.GetTriplesWithPredicate(expected.CreateUriNode("eg:Speed")).ToList());

            String update = "PREFIX ex: <http://example.org/vehicles/> DELETE { ?s a ?type . ?s ex:Speed ?speed } USING <http://example.org/vehicles/> WHERE { ?s a ?type . OPTIONAL { ?s ex:Speed ?speed } }";

            this.TestUpdate(store, expected, update);
        }

        [TestMethod]
        public void SparqlUpdateModifyWithOptional()
        {
            Graph g = new Graph();
            g.LoadFromFile("InferenceTest.ttl");
            g.BaseUri = new Uri("http://example.org/vehicles/");

            Graph expected = new Graph();
            expected.Assert(g.GetTriplesWithPredicate(g.CreateUriNode("rdf:type")));
            expected.Assert(g.GetTriplesWithPredicate(g.CreateUriNode("eg:Speed")));

            String update = "PREFIX ex: <http://example.org/vehicles/> DELETE { } INSERT { ?s a ?type . ?s ex:Speed ?speed } USING <http://example.org/vehicles/> WHERE { ?s a ?type . OPTIONAL { ?s ex:Speed ?speed } }";

            this.TestUpdate(g, expected, update);
        }

        [TestMethod]
        public void SparqlUpdateModifyWithOptional2()
        {
            Graph g = new Graph();
            g.LoadFromFile("InferenceTest.ttl");
            g.BaseUri = new Uri("http://example.org/vehicles/");

            Graph def = new Graph();
            def.Merge(g);

            TripleStore store = new TripleStore();
            store.Add(g);
            store.Add(def);

            Graph expected = new Graph();
            expected.NamespaceMap.Import(g.NamespaceMap);
            expected.Merge(g);
            expected.Retract(expected.GetTriplesWithPredicate(expected.CreateUriNode("rdf:type")).ToList());
            expected.Retract(expected.GetTriplesWithPredicate(expected.CreateUriNode("eg:Speed")).ToList());

            String update = "PREFIX ex: <http://example.org/vehicles/> DELETE { ?s a ?type . ?s ex:Speed ?speed } INSERT { } USING <http://example.org/vehicles/> WHERE { ?s a ?type . OPTIONAL { ?s ex:Speed ?speed } }";

            this.TestUpdate(store, expected, update);
        }
    }
}
