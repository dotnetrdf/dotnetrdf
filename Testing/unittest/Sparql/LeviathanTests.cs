using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Functions;
using VDS.RDF.Query.Filters;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Test.Sparql
{
    [TestClass]
    public class LeviathanTests
    {
        [TestMethod]
        public void SparqlBgpEvaluation()
        {
            //Prepare the Store
            TripleStore store = new TripleStore();
            Graph g = new Graph();
            FileLoader.Load(g, "Turtle.ttl");
            store.Add(g);

            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(@"PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
SELECT * WHERE {?s ?p ?o . ?s rdfs:label ?label}");
            Object testResult = store.ExecuteQuery(q);

            ISparqlAlgebra testAlgebra = q.ToAlgebra();

            if (testResult is SparqlResultSet)
            {
                SparqlResultSet rset = (SparqlResultSet)testResult;
                Console.WriteLine(rset.Count + " Results");
                foreach (SparqlResult r in rset) 
                {
                    Console.WriteLine(r.ToString());
                }
                Console.WriteLine();
            }

            //Create some Triple Patterns
            TriplePattern t1 = new TriplePattern(new VariablePattern("?s"), new VariablePattern("?p"), new VariablePattern("?o"));
            TriplePattern t2 = new TriplePattern(new VariablePattern("?s"), new NodeMatchPattern(g.CreateUriNode("rdfs:label")), new VariablePattern("?label"));
            TriplePattern t3 = new TriplePattern(new VariablePattern("?x"), new VariablePattern("?y"), new VariablePattern("?z"));
            TriplePattern t4 = new TriplePattern(new VariablePattern("?s"), new NodeMatchPattern(g.CreateUriNode(":name")), new VariablePattern("?name"));

            //Build some BGPs
            Bgp selectNothing = new Bgp();
            Bgp selectAll = new Bgp(t1);
            Bgp selectLabelled = new Bgp(new List<ITriplePattern>() { t1, t2 });
            Bgp selectAllDisjoint = new Bgp(new List<ITriplePattern>() { t1, t3 });
            Bgp selectLabels = new Bgp(t2);
            Bgp selectNames = new Bgp(t4);
            //LeftJoin selectOptionalNamed = new LeftJoin(selectAll, new Optional(selectNames));
            LeftJoin selectOptionalNamed = new LeftJoin(selectAll, selectNames);
            Union selectAllUnion = new Union(selectAll, selectAll);
            Union selectAllUnion2 = new Union(selectAllUnion, selectAll);
            Filter selectAllUriObjects = new Filter(selectAll, new UnaryExpressionFilter(new IsUriFunction(new VariableExpressionTerm("o"))));

            //Test out the BGPs
            //Console.WriteLine("{}");
            //this.ShowMultiset(selectNothing.Evaluate(new SparqlEvaluationContext(null, store)));

            //Console.WriteLine("{?s ?p ?o}");
            //this.ShowMultiset(selectAll.Evaluate(new SparqlEvaluationContext(null, store)));

            //Console.WriteLine("{?s ?p ?o . ?s rdfs:label ?label}");
            //SparqlEvaluationContext context = new SparqlEvaluationContext(null, store);
            //this.ShowMultiset(selectLabelled.Evaluate(context));
            //SparqlResultSet lvnResult = new SparqlResultSet(context);

            //Console.WriteLine("{?s ?p ?o . ?x ?y ?z}");
            //this.ShowMultiset(selectAllDisjoint.Evaluate(new SparqlEvaluationContext(null, store)));

            //Console.WriteLine("{?s ?p ?o . OPTIONAL {?s :name ?name}}");
            //this.ShowMultiset(selectOptionalNamed.Evaluate(new SparqlEvaluationContext(null, store)));

            Console.WriteLine("{{?s ?p ?o} UNION {?s ?p ?o}}");
            this.ShowMultiset(selectAllUnion.Evaluate(new SparqlEvaluationContext(null, new InMemoryDataset(store))));

            Console.WriteLine("{{?s ?p ?o} UNION {?s ?p ?o} UNION {?s ?p ?o}}");
            this.ShowMultiset(selectAllUnion2.Evaluate(new SparqlEvaluationContext(null, new InMemoryDataset(store))));

            Console.WriteLine("{?s ?p ?o FILTER (ISURI(?o))}");
            this.ShowMultiset(selectAllUriObjects.Evaluate(new SparqlEvaluationContext(null, new InMemoryDataset(store))));
        }

        [TestMethod]
        public void SparqlMultisetLeftJoin()
        {
            //Create a load of Nodes to use in the tests
            Graph g = new Graph();
            g.NamespaceMap.AddNamespace(String.Empty, new Uri("http://example.org"));
            UriNode s1 = g.CreateUriNode(":s1");
            UriNode s2 = g.CreateUriNode(":s2");
            UriNode p1 = g.CreateUriNode(":p1");
            UriNode p2 = g.CreateUriNode(":p2");
            UriNode rdfsLabel = g.CreateUriNode("rdfs:label");
            LiteralNode o1 = g.CreateLiteralNode("Some Text");
            LiteralNode o2 = g.CreateLiteralNode("1", new Uri(XmlSpecsHelper.XmlSchemaDataTypeInteger));

            //Create an ID and Null Multiset
            IdentityMultiset id = new IdentityMultiset();
            NullMultiset nullset = new NullMultiset();

            //Create and Populate a Multiset
            Multiset m = new Multiset();
            Set s = new Set();
            s.Add("s", s1);
            s.Add("p", p1);
            s.Add("o", o1);
            m.Add(s);
            s = new Set();
            s.Add("s", s2);
            s.Add("p", p2);
            s.Add("o", o2);
            m.Add(s);

            //Create and Populate another Multiset
            Multiset n = new Multiset();
            s = new Set();
            s.Add("s", s1);
            s.Add("label", o1);
            n.Add(s);

            //Create and Populate another Multiset
            Multiset d = new Multiset();
            s = new Set();
            s.Add("s1", s1);
            s.Add("p1", p1);
            s.Add("o1", o1);
            d.Add(s);
            s = new Set();
            s.Add("s1", s2);
            s.Add("p1", p2);
            s.Add("o1", o2);
            d.Add(s);

            //Show the Sets
            Console.WriteLine("LHS");
            foreach (Set set in m.Sets)
            {
                Console.WriteLine(set.ToString());
            }
            Console.WriteLine();
            Console.WriteLine("RHS");
            foreach (Set set in n.Sets)
            {
                Console.WriteLine(set.ToString());
            }
            Console.WriteLine();
            Console.WriteLine("D");
            foreach (Set set in d.Sets)
            {
                Console.WriteLine(set.ToString());
            }
            Console.WriteLine();

            //Try a Join to Identity
            Console.WriteLine("Join ID-LHS");
            BaseMultiset join = id.Join(m);
            foreach (Set set in join.Sets)
            {
                Console.WriteLine(set.ToString());
            }
            Console.WriteLine();

            //Try a Join to Identity
            Console.WriteLine("Join LHS-ID");
            join = m.Join(id);
            foreach (Set set in join.Sets)
            {
                Console.WriteLine(set.ToString());
            }
            Console.WriteLine();

            //Try a Join to Null
            Console.WriteLine("Join NULL-LHS");
            join = nullset.Join(m);
            foreach (Set set in join.Sets)
            {
                Console.WriteLine(set.ToString());
            }
            Console.WriteLine();

            //Try a Join to Null
            Console.WriteLine("Join LHS-NULL");
            join = m.Join(nullset);
            foreach (Set set in join.Sets)
            {
                Console.WriteLine(set.ToString());
            }
            Console.WriteLine();

            //Try a LeftJoin
            Console.WriteLine("LeftJoin NULL-LHS");
            BaseMultiset leftjoin = nullset.LeftJoin(m, new BooleanExpressionTerm(true));
            foreach (Set set in leftjoin.Sets)
            {
                Console.WriteLine(set.ToString());
            }
            Console.WriteLine();

            //Try a LeftJoin
            Console.WriteLine("LeftJoin LHS-NULL");
            leftjoin = m.LeftJoin(nullset, new BooleanExpressionTerm(true));
            foreach (Set set in leftjoin.Sets)
            {
                Console.WriteLine(set.ToString());
            }
            Console.WriteLine();

            //Try a Join
            Console.WriteLine("Join LHS-RHS");
            join = m.Join(n);
            foreach (Set set in join.Sets)
            {
                Console.WriteLine(set.ToString());
            }
            Console.WriteLine();
           
            //Try a LeftOuterJoin
            Console.WriteLine("LeftJoin LHS-RHS");
            leftjoin = m.LeftJoin(n, new BooleanExpressionTerm(true));
            foreach (Set set in leftjoin.Sets)
            {
                Console.WriteLine(set.ToString());
            }
            Console.WriteLine();

            //Try a Produce
            Console.WriteLine("Product LHS-RHS");
            BaseMultiset product = m.Product(n);
            foreach (Set set in product.Sets)
            {
                Console.WriteLine(set.ToString());
            }
            Console.WriteLine();

            //Try a Join to Self
            Console.WriteLine("Product LHS-D");
            product = m.Product(d);
            foreach (Set set in product.Sets)
            {
                Console.WriteLine(set.ToString());
            }
            Console.WriteLine();

            //Try a Union
            Console.WriteLine("Union LHS-RHS");
            BaseMultiset union = m.Union(n);
            foreach (Set set in union.Sets)
            {
                Console.WriteLine(set.ToString());
            }
            Console.WriteLine();
        }

        [TestMethod]
        public void SparqlPropertyPathParser()
        {
            //Load our test data
            TripleStore store = new TripleStore();
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            store.Add(g);

            List<String> testQueries = new List<string>();
            String rdfsPrefix = "PREFIX rdfs: <" + NamespaceMapper.RDFS + ">\n";

            //Cardinality Paths
            testQueries.Add(rdfsPrefix + "SELECT * WHERE {?subclass rdfs:subClassOf* <http://example.org/vehicles/Vehicle>}");
            testQueries.Add(rdfsPrefix + "SELECT * WHERE {?subclass rdfs:subClassOf+ <http://example.org/vehicles/Vehicle>}");
            testQueries.Add(rdfsPrefix + "SELECT * WHERE {?subclass rdfs:subClassOf? <http://example.org/vehicles/Vehicle>}");
            testQueries.Add(rdfsPrefix + "SELECT * WHERE {?subclass rdfs:subClassOf{2,4} <http://example.org/vehicles/Vehicle>}");
            testQueries.Add(rdfsPrefix + "SELECT * WHERE {?subclass rdfs:subClassOf{2,} <http://example.org/vehicles/Vehicle>}");
            testQueries.Add(rdfsPrefix + "SELECT * WHERE {?subclass rdfs:subClassOf{,4} <http://example.org/vehicles/Vehicle>}");
            testQueries.Add(rdfsPrefix + "SELECT * WHERE {?subclass rdfs:subClassOf{2} <http://example.org/vehicles/Vehicle>}");

            //Simple Inverse Paths
            testQueries.Add(rdfsPrefix + "SELECT * WHERE {?type ^a ?entity}");

            //Sequence Paths
            testQueries.Add(rdfsPrefix + "SELECT * WHERE {?subclass rdfs:subClassOf / rdfs:subClassOf <http://example.org/vehicles/Vehicle>}");
            testQueries.Add(rdfsPrefix + "SELECT * WHERE {?subclass rdfs:subClassOf{2} / rdfs:subClassOf <http://example.org/vehicles/Vehicle>}");
            testQueries.Add(rdfsPrefix + "SELECT * WHERE {?subclass rdfs:subClassOf / rdfs:subClassOf{2} <http://example.org/vehicles/Vehicle>}");
            testQueries.Add(rdfsPrefix + "SELECT * WHERE {?subclass a / rdfs:subClassOf <http://example.org/vehicles/Plane>}");
            testQueries.Add(rdfsPrefix + "SELECT * WHERE {?vehicle a ^ rdfs:subClassOf <http://example.org/vehicles/Plane>}");
            testQueries.Add(rdfsPrefix + "SELECT * WHERE {?subclass a / ^ rdfs:subClassOf <http://example.org/vehicles/Vehicle>}");
            testQueries.Add(rdfsPrefix + "SELECT * WHERE {?entity a ^ a ?type}");
            testQueries.Add(rdfsPrefix + "SELECT * WHERE {?entity a ^ a / rdfs:subClassOf <http://example.org/vehicles/GroundVehicle>}");


            //Alternative Paths
            testQueries.Add(rdfsPrefix + "SELECT * WHERE {?subclass rdfs:subClassOf | a <http://example.org/vehicles/Vehicle>}");
            testQueries.Add(rdfsPrefix + "SELECT * WHERE {?subclass (rdfs:subClassOf | a) | rdfs:someProperty <http://example.org/vehicles/Vehicle>}");
            testQueries.Add(rdfsPrefix + "SELECT * WHERE {?subclass rdfs:subClassOf | a | rdfs:someProperty <http://example.org/vehicles/Vehicle>}");

            SparqlQueryParser parser = new SparqlQueryParser();

            foreach (String query in testQueries)
            {
                //Parse the Query and output to console
                SparqlQuery q = parser.ParseFromString(query);
                Console.WriteLine(q.ToString());

                //Now we'll try and evaluate it (if this is possible)
                try
                {
                    Object results = store.ExecuteQuery(q);

                    Console.WriteLine("Evaluated OK");
                    TestTools.ShowResults(results);
                    Console.WriteLine();
                }
                catch (RdfQueryException queryEx)
                {
                    Console.WriteLine("Unable to evaluate:");
                    Console.WriteLine(queryEx.Message);
                    Console.WriteLine(queryEx.StackTrace);
                }
            }
        }

        [TestMethod]
        public void SparqlSimplePropertyPathDuplicates()
        {
            QueryableGraph g = new QueryableGraph();
            FileLoader.Load(g, "property-path-duplicates.ttl");

            String foafPrefix = "PREFIX foaf: <http://xmlns.com/foaf/0.1/>\n";
            String twoStepQuery = foafPrefix + "SELECT ?x ?y WHERE { ?x foaf:knows ?z . ?z foaf:knows ?y }";
            String pathQuery = foafPrefix + "SELECT ?x ?y WHERE {?x foaf:knows{2} ?y}";

            Object resultsA = g.ExecuteQuery(twoStepQuery);
            Object resultsB = g.ExecuteQuery(pathQuery);

            Console.WriteLine("Two Step Query");
            Console.WriteLine();
            Console.WriteLine(twoStepQuery);
            TestTools.ShowResults(resultsA);
            Console.WriteLine();

            Console.WriteLine("Equivalent Path Query");
            Console.WriteLine();
            Console.WriteLine(pathQuery);
            TestTools.ShowResults(resultsB);
            Console.WriteLine();

            Assert.AreEqual(resultsA, resultsB, "Result Sets should have been equivalent");
        }

        [TestMethod]
        public void SparqlStreamingBgpAskEvaluation()
        {
            //Get the Data we want to query
            TripleStore store = new TripleStore();
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            store.Add(g);
            //g = new Graph();
            //FileLoader.Load(g, "noise.ttl");
            //store.Add(g);

            Console.WriteLine(store.Triples.Count() + " Triples in Store");

            //Create the Triple Pattern we want to query with
            UriNode fordFiesta = g.CreateUriNode(new Uri("http://example.org/vehicles/FordFiesta"));
            UriNode rdfType = g.CreateUriNode(new Uri(RdfSpecsHelper.RdfType));
            UriNode rdfsLabel = g.CreateUriNode(new Uri(NamespaceMapper.RDFS + "label"));
            UriNode speed = g.CreateUriNode(new Uri("http://example.org/vehicles/Speed"));
            UriNode carClass = g.CreateUriNode(new Uri("http://example.org/vehicles/Car"));

            TriplePattern allTriples = new TriplePattern(new VariablePattern("?s"), new VariablePattern("?p"), new VariablePattern("?o"));
            TriplePattern allTriples2 = new TriplePattern(new VariablePattern("?x"), new VariablePattern("?y"), new VariablePattern("?z"));
            TriplePattern tp1 = new TriplePattern(new VariablePattern("?s"), new NodeMatchPattern(rdfType), new NodeMatchPattern(carClass));
            TriplePattern tp2 = new TriplePattern(new VariablePattern("?s"), new NodeMatchPattern(speed), new VariablePattern("?speed"));
            TriplePattern tp3 = new TriplePattern(new VariablePattern("?s"), new NodeMatchPattern(rdfsLabel), new VariablePattern("?label"));
            TriplePattern novars = new TriplePattern(new NodeMatchPattern(fordFiesta), new NodeMatchPattern(rdfType), new NodeMatchPattern(carClass));
            TriplePattern novars2 = new TriplePattern(new NodeMatchPattern(fordFiesta), new NodeMatchPattern(rdfsLabel), new NodeMatchPattern(carClass));
            FilterPattern blankSubject = new FilterPattern(new UnaryExpressionFilter(new IsBlankFunction(new VariableExpressionTerm("?s"))));
            List<List<ITriplePattern>> tests = new List<List<ITriplePattern>>()
            {
                new List<ITriplePattern>() { },
                new List<ITriplePattern>() { allTriples },
                new List<ITriplePattern>() { allTriples, allTriples2 },
                new List<ITriplePattern>() { tp1 },
                new List<ITriplePattern>() { tp1, tp2 },
                new List<ITriplePattern>() { tp1, tp3 },
                new List<ITriplePattern>() { novars },
                new List<ITriplePattern>() { novars, tp1 },
                new List<ITriplePattern>() { novars, tp1, tp2 },
                new List<ITriplePattern>() { novars2 },
                new List<ITriplePattern>() { tp1, blankSubject }
            };

            foreach (List<ITriplePattern> tps in tests)
            {
                Console.WriteLine(tps.Count + " Triple Patterns in the Query");
                foreach (ITriplePattern tp in tps)
                {
                    Console.WriteLine(tp.ToString());
                }
                Console.WriteLine();

                ISparqlAlgebra ask = new Ask(new Bgp(tps));
                ISparqlAlgebra askOptimised = new Ask(new AskBgp(tps));

                //Evaluate with timings
                Stopwatch timer = new Stopwatch();
                TimeSpan unopt, opt;
                timer.Start();
                BaseMultiset results1 = ask.Evaluate(new SparqlEvaluationContext(null, new InMemoryDataset(store)));
                timer.Stop();
                unopt = timer.Elapsed;
                timer.Reset();
                timer.Start();
                BaseMultiset results2 = askOptimised.Evaluate(new SparqlEvaluationContext(null, new InMemoryDataset(store)));
                timer.Stop();
                opt = timer.Elapsed;

                Console.WriteLine("ASK = " + results1.GetType().ToString() + " in " + unopt.ToString());
                Console.WriteLine("ASK Optimised = " + results2.GetType().ToString() + " in " + opt.ToString());

                Assert.AreEqual(results1.GetType(), results2.GetType(), "Both ASK queries should have produced the same result");

                Console.WriteLine();
            }
        }

        [TestMethod]
        public void SparqlEvaluationGraphNonExistentUri()
        {
            String query = "SELECT * WHERE { GRAPH <http://example.org/noSuchGraph> { ?s ?p ?o } }";
            TripleStore store = new TripleStore();
            Object results = store.ExecuteQuery(query);

            if (results is SparqlResultSet)
            {
                TestTools.ShowResults(results);

                SparqlResultSet rset = (SparqlResultSet)results;
                Assert.IsTrue(rset.IsEmpty, "Result Set should be empty");
                Assert.AreEqual(3, rset.Variables.Count(), "Should still be 3 Variables even if no results");
            }
            else
            {
                Assert.Fail("Query should have returned a SPARQL Result Set");
            }
        }

        //[TestMethod]
        //public void StreamingBgpSelectEvaluation()
        //{
        //    //Get the Data we want to query
        //    TripleStore store = new TripleStore();
        //    Graph g = new Graph();
        //    FileLoader.Load(g, "InferenceTest.ttl");
        //    store.Add(g);
        //    //g = new Graph();
        //    //FileLoader.Load(g, "noise.ttl");
        //    //store.Add(g);

        //    Console.WriteLine(store.Triples.Count() + " Triples in Store");

        //    //Create the Triple Pattern we want to query with
        //    UriNode fordFiesta = g.CreateUriNode(new Uri("http://example.org/vehicles/FordFiesta"));
        //    UriNode rdfType = g.CreateUriNode(new Uri(RdfSpecsHelper.RdfType));
        //    UriNode rdfsLabel = g.CreateUriNode(new Uri(NamespaceMapper.RDFS + "label"));
        //    UriNode speed = g.CreateUriNode(new Uri("http://example.org/vehicles/Speed"));
        //    UriNode carClass = g.CreateUriNode(new Uri("http://example.org/vehicles/Car"));

        //    TriplePattern allTriples = new TriplePattern(new VariablePattern("?s"), new VariablePattern("?p"), new VariablePattern("?o"));
        //    TriplePattern allTriples2 = new TriplePattern(new VariablePattern("?x"), new VariablePattern("?y"), new VariablePattern("?z"));
        //    TriplePattern tp1 = new TriplePattern(new VariablePattern("?s"), new NodeMatchPattern(rdfType), new NodeMatchPattern(carClass));
        //    TriplePattern tp2 = new TriplePattern(new VariablePattern("?s"), new NodeMatchPattern(speed), new VariablePattern("?speed"));
        //    TriplePattern tp3 = new TriplePattern(new VariablePattern("?s"), new NodeMatchPattern(rdfsLabel), new VariablePattern("?label"));
        //    TriplePattern novars = new TriplePattern(new NodeMatchPattern(fordFiesta), new NodeMatchPattern(rdfType), new NodeMatchPattern(carClass));
        //    TriplePattern novars2 = new TriplePattern(new NodeMatchPattern(fordFiesta), new NodeMatchPattern(rdfsLabel), new NodeMatchPattern(carClass));
        //    FilterPattern blankSubject = new FilterPattern(new UnaryExpressionFilter(new IsBlankFunction(new VariableExpressionTerm("?s"))));
        //    List<List<ITriplePattern>> tests = new List<List<ITriplePattern>>()
        //    {
        //        new List<ITriplePattern>() { },
        //        new List<ITriplePattern>() { allTriples },
        //        new List<ITriplePattern>() { allTriples, allTriples2 },
        //        new List<ITriplePattern>() { tp1 },
        //        new List<ITriplePattern>() { tp1, tp2 },
        //        new List<ITriplePattern>() { tp1, tp3 },
        //        new List<ITriplePattern>() { novars },
        //        new List<ITriplePattern>() { novars, tp1 },
        //        new List<ITriplePattern>() { novars, tp1, tp2 },
        //        new List<ITriplePattern>() { novars2 },
        //        new List<ITriplePattern>() { tp1, blankSubject }
        //    };

        //    foreach (List<ITriplePattern> tps in tests)
        //    {
        //        Console.WriteLine(tps.Count + " Triple Patterns in the Query");
        //        foreach (ITriplePattern tp in tps)
        //        {
        //            Console.WriteLine(tp.ToString());
        //        }
        //        Console.WriteLine();

        //        ISparqlAlgebra select = new Bgp(tps);
        //        ISparqlAlgebra selectOptimised = new LazyBgp(tps, 10);

        //        //Evaluate with timings
        //        Stopwatch timer = new Stopwatch();
        //        TimeSpan unopt, opt;
        //        timer.Start();
        //        BaseMultiset results1 = select.Evaluate(new SparqlEvaluationContext(null, new InMemoryDataset(store)));
        //        timer.Stop();
        //        unopt = timer.Elapsed;
        //        timer.Reset();
        //        timer.Start();
        //        BaseMultiset results2 = selectOptimised.Evaluate(new SparqlEvaluationContext(null, new InMemoryDataset(store)));
        //        timer.Stop();
        //        opt = timer.Elapsed;

        //        Console.WriteLine("SELECT = " + results1.GetType().ToString() + " (" + results1.Count + " Results) in " + unopt.ToString());
        //        Console.WriteLine("SELECT Optimised = " + results2.GetType().ToString() + " (" + results2.Count + " Results) in " + opt.ToString());

        //        Console.WriteLine();
        //        Console.WriteLine("Optimised Results");
        //        foreach (Set s in results2.Sets)
        //        {
        //            Console.WriteLine(s.ToString());
        //        }

        //        Assert.IsTrue(results1.Count >= results2.Count, "Optimised Select should have produced as many/fewer results than Unoptimised Select");

        //        Console.WriteLine();
        //    }
        //}

        private void ShowMultiset(BaseMultiset multiset) 
        {
            Console.WriteLine(multiset.GetType().ToString());
            foreach (Set s in multiset.Sets)
            {
                Console.WriteLine(s.ToString());
            }
            Console.WriteLine();
        }
    }
}
