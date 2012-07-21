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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Ontology;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Inference;


namespace VDS.RDF.Test.Ontology
{
    [TestClass]
    public class OntologyTests
    {
        [TestMethod]
        public void OntologyClassBasic()
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

        [TestMethod]
        public void OntologyIndividualCreation()
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

        [TestMethod]
        public void OntologyPropertyBasic()
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

        [TestMethod]
        public void OntologyReasonerGraph()
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
            IUriNode fiesta = g.CreateUriNode(new Uri("http://example.org/vehicles/FordFiesta"));
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

        [TestMethod]
        public void OntologyResourceCasting()
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

        [TestMethod]
        public void OntologyDomainAndRangeOfClassProperties()
        {
            OntologyGraph g = new OntologyGraph();
            FileLoader.Load(g, "InferenceTest.ttl");

            //Get the Class of interest
            OntologyClass cls = g.CreateOntologyClass(new Uri("http://example.org/vehicles/Vehicle"));

            //Find Triples where Predicate is rdfs:range or rdfs:domain and the Object is the Class
            List<OntologyProperty> ranges = cls.IsRangeOf.ToList();
            List<OntologyProperty> domains = cls.IsDomainOf.ToList();

            //Do whatever you want with the Ranges and Domains...

            Console.WriteLine("Ranges");
            foreach (OntologyProperty range in ranges)
            {
                Console.WriteLine(range.ToString());
            }
            Console.WriteLine();
            Console.WriteLine("Domains");
            foreach (OntologyProperty domain in domains)
            {
                Console.WriteLine(domain.ToString());
            }
        }

        [TestMethod]
        public void OntologyDomainAndRangeOfClassManual()
        {
            OntologyGraph g = new OntologyGraph();
            FileLoader.Load(g, "InferenceTest.ttl");

            //Get the Class of interest
            OntologyClass cls = g.CreateOntologyClass(new Uri("http://example.org/vehicles/Vehicle"));

            //Find Triples where Predicate is rdfs:range or rdfs:domain and the Object is the Class
            IUriNode rdfsRange = g.CreateUriNode(new Uri(NamespaceMapper.RDFS + "range"));
            IUriNode rdfsDomain = g.CreateUriNode(new Uri(NamespaceMapper.RDFS + "domain"));
            List<OntologyProperty> ranges = new List<OntologyProperty>();
            List<OntologyProperty> domains = new List<OntologyProperty>();
            foreach (Triple t in cls.TriplesWithObject)
            {
                if (t.Predicate.Equals(rdfsRange))
                {
                    ranges.Add(new OntologyProperty(t.Subject, g));
                }
                else if (t.Predicate.Equals(rdfsDomain))
                {
                    domains.Add(new OntologyProperty(t.Subject, g));
                }
            }

            //Do whatever you want with the Ranges and Domains...

            Console.WriteLine("Ranges");
            foreach (OntologyProperty range in ranges)
            {
                Console.WriteLine(range.ToString());
            }
            Console.WriteLine();
            Console.WriteLine("Domains");
            foreach (OntologyProperty domain in domains)
            {
                Console.WriteLine(domain.ToString());
            }
        }
    }
}
