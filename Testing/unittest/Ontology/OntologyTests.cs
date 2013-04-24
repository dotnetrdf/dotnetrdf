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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Ontology;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Inference;


namespace VDS.RDF.Ontology
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

        [TestMethod]
        public void OntologyClassSubClasses()
        {
            //Load Test Data
            Console.WriteLine("Loading in the standard test data InferenceTest.ttl");
            OntologyGraph g = new OntologyGraph();
            FileLoader.Load(g, "InferenceTest.ttl");

            //Get the class of Ground Vehicles
            OntologyClass groundVehicle = g.CreateOntologyClass(new Uri("http://example.org/vehicles/GroundVehicle"));
            Console.WriteLine("Got the class of Ground Vehicles");

            //Check counts of super classes
            Assert.AreEqual(1, groundVehicle.SuperClasses.Count());
            Assert.AreEqual(1, groundVehicle.DirectSuperClasses.Count());
            Assert.AreEqual(0, groundVehicle.IndirectSuperClasses.Count());

            //Check counts of sub-classes
            Assert.AreEqual(5, groundVehicle.SubClasses.Count());
            Assert.AreEqual(3, groundVehicle.DirectSubClasses.Count());
            Assert.AreEqual(2, groundVehicle.IndirectSubClasses.Count());
        }

        [TestMethod]
        public void OntologyClassSiblings()
        {
            //Load Test Data
            Console.WriteLine("Loading in the standard test data InferenceTest.ttl");
            OntologyGraph g = new OntologyGraph();
            FileLoader.Load(g, "InferenceTest.ttl");

            //Get the class of Cars
            OntologyClass car = g.CreateOntologyClass(new Uri("http://example.org/vehicles/Car"));

            //Get siblings
            List<OntologyClass> siblings = car.Siblings.ToList();
            Assert.AreEqual(2, siblings.Count);
            Assert.IsFalse(siblings.Contains(car));
        }

        [TestMethod]
        public void OntologyClassTopAndBottom()
        {
            //Load Test Data
            Console.WriteLine("Loading in the standard test data InferenceTest.ttl");
            OntologyGraph g = new OntologyGraph();
            FileLoader.Load(g, "InferenceTest.ttl");

            //Get the class of Vehicles
            OntologyClass vehicle = g.CreateOntologyClass(new Uri("http://example.org/vehicles/Vehicle"));
            Assert.IsTrue(vehicle.IsTopClass);
            Assert.IsFalse(vehicle.IsBottomClass);

            //Get the class of cars
            OntologyClass car = g.CreateOntologyClass(new Uri("http://example.org/vehicles/Car"));
            Assert.IsFalse(car.IsTopClass);
            Assert.IsFalse(car.IsBottomClass);

            //Get the class of sports cars
            OntologyClass sportsCar = g.CreateOntologyClass(new Uri("http://example.org/vehicles/SportsCar"));
            Assert.IsFalse(sportsCar.IsTopClass);
            Assert.IsTrue(sportsCar.IsBottomClass);
        }

        [TestMethod]
        public void OntologyPropertySubProperties()
        {
            //Load Test Data
            Console.WriteLine("Loading in the standard test data InferenceTest.ttl");
            OntologyGraph g = new OntologyGraph();
            FileLoader.Load(g, "InferenceTest.ttl");

            //Get the property of Ground Speed
            OntologyProperty groundSpeed = g.CreateOntologyProperty(new Uri("http://example.org/vehicles/GroundSpeed"));

            //Check counts of super properties
            Assert.AreEqual(1, groundSpeed.SuperProperties.Count());
            Assert.AreEqual(1, groundSpeed.DirectSuperProperties.Count());
            Assert.AreEqual(0, groundSpeed.IndirectSuperProperty.Count());

            //Check counts of sub-properties
            OntologyProperty speed = g.CreateOntologyProperty(new Uri("http://example.org/vehicles/Speed"));
            Assert.AreEqual(3, speed.SubProperties.Count());
            Assert.AreEqual(3, speed.DirectSubProperties.Count());
            Assert.AreEqual(0, speed.IndirectSubProperties.Count());
        }

        [TestMethod]
        public void OntologyPropertyTopAndBottom()
        {
            //Load Test Data
            Console.WriteLine("Loading in the standard test data InferenceTest.ttl");
            OntologyGraph g = new OntologyGraph();
            FileLoader.Load(g, "InferenceTest.ttl");

            //Get the property Speed
            OntologyProperty speed = g.CreateOntologyProperty(new Uri("http://example.org/vehicles/Speed"));
            Assert.IsTrue(speed.IsTopProperty);
            Assert.IsFalse(speed.IsBottomProperty);

            //Get the property AirSpeed
            OntologyProperty airSpeed = g.CreateOntologyProperty(new Uri("http://example.org/vehicles/AirSpeed"));
            Assert.IsFalse(airSpeed.IsTopProperty);
            Assert.IsTrue(airSpeed.IsBottomProperty);
        }

        [TestMethod]
        public void OntologyPropertySiblings()
        {
            //Load Test Data
            Console.WriteLine("Loading in the standard test data InferenceTest.ttl");
            OntologyGraph g = new OntologyGraph();
            FileLoader.Load(g, "InferenceTest.ttl");

            //Get the property LimitedSpeed
            OntologyProperty limitedSpeed = g.CreateOntologyProperty(new Uri("http://example.org/vehicles/LimitedSpeed"));

            //Get siblings
            List<OntologyProperty> siblings = limitedSpeed.Siblings.ToList();
            Assert.AreEqual(2, siblings.Count);
            Assert.IsFalse(siblings.Contains(limitedSpeed));
        }

        [TestMethod]
        public void OntologyProperties()
        {
            OntologyGraph g = new OntologyGraph();
            g.LoadFromFile("ontology.ttl");

            //Check Property Counts
            Assert.AreEqual(1, g.RdfProperties.Count());
            Assert.AreEqual(1, g.OwlAnnotationProperties.Count());
            Assert.AreEqual(1, g.OwlDatatypeProperties.Count());
            Assert.AreEqual(1, g.OwlObjectProperties.Count());
            Assert.AreEqual(3, g.OwlProperties.Count());
            Assert.AreEqual(4, g.AllProperties.Count());
        }

        [TestMethod]
        public void OntologyClasses()
        {
            OntologyGraph g = new OntologyGraph();
            g.LoadFromFile("ontology.ttl");

            //Check Class Counts
            Assert.AreEqual(1, g.RdfClasses.Count());
            Assert.AreEqual(1, g.OwlClasses.Count());
            Assert.AreEqual(2, g.AllClasses.Count());
        }

        [TestMethod]
        public void OntologyClassCount1()
        {
            OntologyGraph g = new OntologyGraph();
            g.LoadFromFile("swrc.owl");
            Assert.IsFalse(g.IsEmpty);

            //Count classes, raw and distinct count should be same
            int count = g.OwlClasses.Count();
            int distinctCount = g.OwlClasses.Select(c => c.Resource).Distinct().Count();

            Console.WriteLine("Count = " + count);
            Console.WriteLine("Distinct Count = " + distinctCount);

            Assert.IsTrue(count == distinctCount, "Expected raw and distinct counts to be the same, got " + count + " and " + distinctCount);
        }

        [TestMethod]
        public void OntologyClassCount2()
        {
            OntologyGraph g = new OntologyGraph();
            g.LoadFromFile("swrc.owl");
            Assert.IsFalse(g.IsEmpty);

            OntologyClass classOfClasses = g.CreateOntologyClass(g.CreateUriNode("owl:Class"));
            int count = 0;
            HashSet<INode> resources = new HashSet<INode>();

            //This iterates over the things that are a class
            foreach (OntologyResource c in classOfClasses.Instances)
            {
                count++;
                resources.Add(c.Resource);
            }

            Console.WriteLine("Count = " + count);
            Console.WriteLine("Distinct Count = " + resources.Count);

            Assert.AreEqual(resources.Count, count);
        }
    }
}
