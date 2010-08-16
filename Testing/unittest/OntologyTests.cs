using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Ontology;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Inference;


namespace VDS.RDF.Test
{
    [TestClass()]
    public class OntologyTests
    {
        [TestMethod()]
        public void ClassBasicTest()
        {
            //Load Test Data
            Console.WriteLine("Loading in the standard test data InferenceTest.ttl");
            OntologyGraph g = new OntologyGraph();
            FileLoader.Load(g, "InferenceTest.ttl");

            //Get the class of Ground Vehicles
            OntologyClass groundVehicle = g.CreateOntologyClass(new Uri("http://example.org/vehicles/GroundVehicle"));
            Console.WriteLine("Got the class of Ground Vehicles");
            foreach (OntologyClass c in groundVehicle.SuperClasses)
            {
                Console.WriteLine("Super Class: " + c.Resource.ToString());
            }
            foreach (OntologyClass c in groundVehicle.SubClasses)
            {
                Console.WriteLine("Sub Class: " + c.Resource.ToString());
            }
            Console.WriteLine();

            //Get the class of Cars
            OntologyClass car = g.CreateOntologyClass(new Uri("http://example.org/vehicles/Car"));
            Console.WriteLine("Got the class of Cars");
            foreach (OntologyClass c in car.SuperClasses)
            {
                Console.WriteLine("Super Class: " + c.Resource.ToString());
            }
            foreach (OntologyClass c in car.SubClasses)
            {
                Console.WriteLine("Sub Class: " + c.Resource.ToString());
            }
            foreach (OntologyResource r in car.Instances)
            {
                Console.WriteLine("Instance: " + r.Resource.ToString());
            }
        }

        [TestMethod()]
        public void IndividualCreationTest()
        {
            //Load Test Data
            Console.WriteLine("Loading in the standard test data InferenceTest.ttl");
            OntologyGraph g = new OntologyGraph();
            FileLoader.Load(g, "InferenceTest.ttl");

            //Try and get a non-existent individual, should fail
            try
            {
                Individual i = g.CreateIndividual(new Uri("http://example.org/noSuchThing"));
                Assert.Fail("Attempting to create a none-existent Individual should fail");
            }
            catch (RdfOntologyException)
            {
                //This is what should happen
                Console.WriteLine("Errored when trying to get non-existent Individual - OK");
            }
            Console.WriteLine();

            //Try and get an actual individual
            try
            {
                Individual i = g.CreateIndividual(new Uri("http://example.org/vehicles/FordFiesta"));
                Console.WriteLine("Got an existing individual OK");
                foreach (Triple t in i.Triples)
                {
                    Console.WriteLine(t.ToString());
                }
            }
            catch (RdfOntologyException)
            {
                Assert.Fail("Should be able to get an existing Individual");
            }
            Console.WriteLine();

            //Try and create a new individual
            try
            {
                Individual i = g.CreateIndividual(new Uri("http://example.org/vehicles/MazdaMX5"), new Uri("http://example.org/vehicles/Car"));
                Console.WriteLine("Created a new individual OK");
                Console.WriteLine("Graph now contains the following");
                foreach (Triple t in g.Triples)
                {
                    Console.WriteLine(t.ToString());
                }
            }
            catch (RdfOntologyException)
            {
                Assert.Fail("Should be able to create new Individuals");
            }
        }

        [TestMethod()]
        public void PropertyBasicTest()
        {
            //Load Test Data
            Console.WriteLine("Loading in the standard test data InferenceTest.ttl");
            OntologyGraph g = new OntologyGraph();
            FileLoader.Load(g, "InferenceTest.ttl");

            OntologyProperty speed = g.CreateOntologyProperty(new Uri("http://example.org/vehicles/Speed"));
            Console.WriteLine("Ranges");
            foreach (OntologyClass c in speed.Ranges)
            {
                Console.WriteLine(c.Resource.ToString());
            }
            Console.WriteLine("Domains");
            foreach (OntologyClass c in speed.Domains)
            {
                Console.WriteLine(c.Resource.ToString());
            }
            Console.WriteLine("Sub-properties");
            foreach (OntologyProperty p in speed.SubProperties)
            {
                Console.WriteLine(p.Resource.ToString());
            }
            Console.WriteLine("Super-properties");
            foreach (OntologyProperty p in speed.SuperProperties)
            {
                Console.WriteLine(p.Resource.ToString());
            }
            Console.WriteLine();
            Console.WriteLine("Used By");
            foreach (OntologyResource r in speed.UsedBy)
            {
                Console.WriteLine(r.Resource.ToString());
            }
        }

        [TestMethod()]
        public void ReasonerGraphTest()
        {
            //Load Test Data
            Console.WriteLine("Loading in the standard test data InferenceTest.ttl");
            OntologyGraph g = new OntologyGraph();
            FileLoader.Load(g, "InferenceTest.ttl");

            OntologyClass c = g.CreateOntologyClass(new Uri("http://example.org/vehicles/Car"));
            Console.WriteLine("Things which are cars in an Ontology Graph");
            foreach (OntologyResource r in c.Instances)
            {
                Console.WriteLine(r.Resource.ToString());
            }
            Console.WriteLine();

            ReasonerGraph g2 = new ReasonerGraph(g, new RdfsReasoner());
            OntologyClass c2 = g2.CreateOntologyClass(new Uri("http://example.org/vehicles/Car"));
            Console.WriteLine("Things which are cars in a Reasoner Graph using an RDFS Reasoner");
            foreach (OntologyResource r in c2.Instances)
            {
                Console.WriteLine(r.Resource.ToString());
            }
            Console.WriteLine();

            Console.WriteLine("Original Graph has " + g.Triples.Count + " Triples");
            Console.WriteLine("Reasoner Graph has " + g2.Triples.Count + " Triples");
            Assert.IsTrue(g2.Triples.Count > g.Triples.Count, "Reasoner Graph should have more Triples");
            Assert.AreEqual(g, g2.BaseGraph, "Original Graph should be equal to the Reasoner Graphs BaseGraph");

            Console.WriteLine();

            Console.WriteLine("Going to do a GetTriplesWithSubject() call on both Graphs to see if ReasonerGraph behaves as expected");
            UriNode fiesta = g.CreateUriNode(new Uri("http://example.org/vehicles/FordFiesta"));
            Console.WriteLine("Original Graph:");
            foreach (Triple t in g.GetTriplesWithSubject(fiesta))
            {
                Console.WriteLine(t.ToString());
            }
            Console.WriteLine();
            Console.WriteLine("Reasoner Graph:");
            foreach (Triple t in g2.GetTriplesWithSubject(fiesta))
            {
                Console.WriteLine(t.ToString());
            }
        }

        [TestMethod()]
        public void ResourceCastingTest()
        {
            //Load Test Data
            Console.WriteLine("Loading in the standard test data InferenceTest.ttl");
            OntologyGraph g = new OntologyGraph();
            FileLoader.Load(g, "InferenceTest.ttl");

            //Get the eg:FordFiesta resource
            OntologyResource resource = g.CreateOntologyResource(new Uri("http://example.org/vehicles/FordFiesta"));
            IGraph h = (Graph)resource;
            foreach (Triple t in h.Triples)
            {
                Console.WriteLine(t.ToString());
            }
        }
    }
}
