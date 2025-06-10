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
using System.IO;
using System.Linq;
using Xunit;
using VDS.RDF.Parsing;


namespace VDS.RDF.Ontology;


public class OntologyTests
{
    private readonly ITestOutputHelper _output;
    public OntologyTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void OntologyClassBasic()
    {
        //Load Test Data
        _output.WriteLine("Loading in the standard test data InferenceTest.ttl");
        var g = new OntologyGraph();
        g.LoadFromFile(Path.Combine("resources", "InferenceTest.ttl"));

        //Get the class of Ground Vehicles
        OntologyClass groundVehicle = g.CreateOntologyClass(new Uri("http://example.org/vehicles/GroundVehicle"));
        _output.WriteLine("Got the class of Ground Vehicles");
        foreach (OntologyClass c in groundVehicle.SuperClasses)
        {
            _output.WriteLine("Super Class: " + c.Resource.ToString());
        }
        foreach (OntologyClass c in groundVehicle.SubClasses)
        {
            _output.WriteLine("Sub Class: " + c.Resource.ToString());
        }
        _output.WriteLine(string.Empty);

        //Get the class of Cars
        OntologyClass car = g.CreateOntologyClass(new Uri("http://example.org/vehicles/Car"));
        _output.WriteLine("Got the class of Cars");
        foreach (OntologyClass c in car.SuperClasses)
        {
            _output.WriteLine("Super Class: " + c.Resource.ToString());
        }
        foreach (OntologyClass c in car.SubClasses)
        {
            _output.WriteLine("Sub Class: " + c.Resource.ToString());
        }
        foreach (OntologyResource r in car.Instances)
        {
            _output.WriteLine("Instance: " + r.Resource.ToString());
        }
    }

    [Fact]
    public void OntologyIndividualCreation()
    {
        //Load Test Data
        _output.WriteLine("Loading in the standard test data InferenceTest.ttl");
        var g = new OntologyGraph();
        FileLoader.Load(g, Path.Combine("resources", "InferenceTest.ttl"));

        //Try and get a non-existent individual, should fail
        try
        {
            Individual i = g.CreateIndividual(new Uri("http://example.org/noSuchThing"));
            Assert.Fail("Attempting to create a none-existent Individual should fail");
        }
        catch (RdfOntologyException)
        {
            //This is what should happen
            _output.WriteLine("Errored when trying to get non-existent Individual - OK");
        }
        _output.WriteLine(string.Empty);

        //Try and get an actual individual
        try
        {
            Individual i = g.CreateIndividual(new Uri("http://example.org/vehicles/FordFiesta"));
            _output.WriteLine("Got an existing individual OK");
            foreach (Triple t in i.Triples)
            {
                _output.WriteLine(t.ToString());
            }
        }
        catch (RdfOntologyException)
        {
            Assert.Fail("Should be able to get an existing Individual");
        }
        _output.WriteLine(string.Empty);

        //Try and create a new individual
        try
        {
            Individual i = g.CreateIndividual(new Uri("http://example.org/vehicles/MazdaMX5"), new Uri("http://example.org/vehicles/Car"));
            _output.WriteLine("Created a new individual OK");
            _output.WriteLine("Graph now contains the following");
            foreach (Triple t in g.Triples)
            {
                _output.WriteLine(t.ToString());
            }
        }
        catch (RdfOntologyException)
        {
            Assert.Fail("Should be able to create new Individuals");
        }
    }

    [Fact]
    public void OntologyPropertyBasic()
    {
        //Load Test Data
        _output.WriteLine("Loading in the standard test data InferenceTest.ttl");
        var g = new OntologyGraph();
        FileLoader.Load(g, Path.Combine("resources", "InferenceTest.ttl"));

        OntologyProperty speed = g.CreateOntologyProperty(new Uri("http://example.org/vehicles/Speed"));
        _output.WriteLine("Ranges");
        foreach (OntologyClass c in speed.Ranges)
        {
            _output.WriteLine(c.Resource.ToString());
        }
        _output.WriteLine("Domains");
        foreach (OntologyClass c in speed.Domains)
        {
            _output.WriteLine(c.Resource.ToString());
        }
        _output.WriteLine("Sub-properties");
        foreach (OntologyProperty p in speed.SubProperties)
        {
            _output.WriteLine(p.Resource.ToString());
        }
        _output.WriteLine("Super-properties");
        foreach (OntologyProperty p in speed.SuperProperties)
        {
            _output.WriteLine(p.Resource.ToString());
        }
        _output.WriteLine(string.Empty);
        _output.WriteLine("Used By");
        foreach (OntologyResource r in speed.UsedBy)
        {
            _output.WriteLine(r.Resource.ToString());
        }
    }



    [Fact]
    public void OntologyResourceCasting()
    {
        //Load Test Data
        _output.WriteLine("Loading in the standard test data InferenceTest.ttl");
        var g = new OntologyGraph();
        FileLoader.Load(g, Path.Combine("resources", "InferenceTest.ttl"));

        //Get the eg:FordFiesta resource
        OntologyResource resource = g.CreateOntologyResource(new Uri("http://example.org/vehicles/FordFiesta"));
        IGraph h = (Graph)resource;
        foreach (Triple t in h.Triples)
        {
            _output.WriteLine(t.ToString());
        }
    }

    [Fact]
    public void OntologyDomainAndRangeOfClassProperties()
    {
        var g = new OntologyGraph();
        FileLoader.Load(g, Path.Combine("resources", "InferenceTest.ttl"));

        //Get the Class of interest
        OntologyClass cls = g.CreateOntologyClass(new Uri("http://example.org/vehicles/Vehicle"));

        //Find Triples where Predicate is rdfs:range or rdfs:domain and the Object is the Class
        var ranges = cls.IsRangeOf.ToList();
        var domains = cls.IsDomainOf.ToList();

        //Do whatever you want with the Ranges and Domains...

        _output.WriteLine("Ranges");
        foreach (OntologyProperty range in ranges)
        {
            _output.WriteLine(range.ToString());
        }
        _output.WriteLine(string.Empty);
        _output.WriteLine("Domains");
        foreach (OntologyProperty domain in domains)
        {
            _output.WriteLine(domain.ToString());
        }
    }

    [Fact]
    public void OntologyDomainAndRangeOfClassManual()
    {
        var g = new OntologyGraph();
        FileLoader.Load(g, Path.Combine("resources", "InferenceTest.ttl"));

        //Get the Class of interest
        OntologyClass cls = g.CreateOntologyClass(new Uri("http://example.org/vehicles/Vehicle"));

        //Find Triples where Predicate is rdfs:range or rdfs:domain and the Object is the Class
        IUriNode rdfsRange = g.CreateUriNode(new Uri(NamespaceMapper.RDFS + "range"));
        IUriNode rdfsDomain = g.CreateUriNode(new Uri(NamespaceMapper.RDFS + "domain"));
        var ranges = new List<OntologyProperty>();
        var domains = new List<OntologyProperty>();
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

        _output.WriteLine("Ranges");
        foreach (OntologyProperty range in ranges)
        {
            _output.WriteLine(range.ToString());
        }
        _output.WriteLine(string.Empty);
        _output.WriteLine("Domains");
        foreach (OntologyProperty domain in domains)
        {
            _output.WriteLine(domain.ToString());
        }
    }

    [Fact]
    public void OntologyClassSubClasses()
    {
        //Load Test Data
        _output.WriteLine("Loading in the standard test data InferenceTest.ttl");
        var g = new OntologyGraph();
        FileLoader.Load(g, Path.Combine("resources", "InferenceTest.ttl"));

        //Get the class of Ground Vehicles
        OntologyClass groundVehicle = g.CreateOntologyClass(new Uri("http://example.org/vehicles/GroundVehicle"));
        _output.WriteLine("Got the class of Ground Vehicles");

        //Check counts of super classes
        Assert.Single(groundVehicle.SuperClasses);
        Assert.Single(groundVehicle.DirectSuperClasses);
        Assert.Empty(groundVehicle.IndirectSuperClasses);

        //Check counts of sub-classes
        Assert.Equal(5, groundVehicle.SubClasses.Count());
        Assert.Equal(3, groundVehicle.DirectSubClasses.Count());
        Assert.Equal(2, groundVehicle.IndirectSubClasses.Count());
    }

    [Fact]
    public void OntologyClassSiblings()
    {
        //Load Test Data
        _output.WriteLine("Loading in the standard test data InferenceTest.ttl");
        var g = new OntologyGraph();
        FileLoader.Load(g, Path.Combine("resources", "InferenceTest.ttl"));

        //Get the class of Cars
        OntologyClass car = g.CreateOntologyClass(new Uri("http://example.org/vehicles/Car"));

        //Get siblings
        var siblings = car.Siblings.ToList();
        Assert.Equal(2, siblings.Count);
        Assert.DoesNotContain(car, siblings);
    }

    [Fact]
    public void OntologyClassTopAndBottom()
    {
        //Load Test Data
        _output.WriteLine("Loading in the standard test data InferenceTest.ttl");
        var g = new OntologyGraph();
        FileLoader.Load(g, Path.Combine("resources", "InferenceTest.ttl"));

        //Get the class of Vehicles
        OntologyClass vehicle = g.CreateOntologyClass(new Uri("http://example.org/vehicles/Vehicle"));
        Assert.True(vehicle.IsTopClass);
        Assert.False(vehicle.IsBottomClass);

        //Get the class of cars
        OntologyClass car = g.CreateOntologyClass(new Uri("http://example.org/vehicles/Car"));
        Assert.False(car.IsTopClass);
        Assert.False(car.IsBottomClass);

        //Get the class of sports cars
        OntologyClass sportsCar = g.CreateOntologyClass(new Uri("http://example.org/vehicles/SportsCar"));
        Assert.False(sportsCar.IsTopClass);
        Assert.True(sportsCar.IsBottomClass);
    }

    [Fact]
    public void OntologyPropertySubProperties()
    {
        //Load Test Data
        _output.WriteLine("Loading in the standard test data InferenceTest.ttl");
        var g = new OntologyGraph();
        FileLoader.Load(g, Path.Combine("resources", "InferenceTest.ttl"));

        //Get the property of Ground Speed
        OntologyProperty groundSpeed = g.CreateOntologyProperty(new Uri("http://example.org/vehicles/GroundSpeed"));

        //Check counts of super properties
        Assert.Single(groundSpeed.SuperProperties);
        Assert.Single(groundSpeed.DirectSuperProperties);
        Assert.Empty(groundSpeed.IndirectSuperProperty);

        //Check counts of sub-properties
        OntologyProperty speed = g.CreateOntologyProperty(new Uri("http://example.org/vehicles/Speed"));
        Assert.Equal(3, speed.SubProperties.Count());
        Assert.Equal(3, speed.DirectSubProperties.Count());
        Assert.Empty(speed.IndirectSubProperties);
    }

    [Fact]
    public void OntologyPropertyTopAndBottom()
    {
        //Load Test Data
        _output.WriteLine("Loading in the standard test data InferenceTest.ttl");
        var g = new OntologyGraph();
        FileLoader.Load(g, Path.Combine("resources", "InferenceTest.ttl"));

        //Get the property Speed
        OntologyProperty speed = g.CreateOntologyProperty(new Uri("http://example.org/vehicles/Speed"));
        Assert.True(speed.IsTopProperty);
        Assert.False(speed.IsBottomProperty);

        //Get the property AirSpeed
        OntologyProperty airSpeed = g.CreateOntologyProperty(new Uri("http://example.org/vehicles/AirSpeed"));
        Assert.False(airSpeed.IsTopProperty);
        Assert.True(airSpeed.IsBottomProperty);
    }

    [Fact]
    public void OntologyPropertySiblings()
    {
        //Load Test Data
        _output.WriteLine("Loading in the standard test data InferenceTest.ttl");
        var g = new OntologyGraph();
        FileLoader.Load(g, Path.Combine("resources", "InferenceTest.ttl"));

        //Get the property LimitedSpeed
        OntologyProperty limitedSpeed = g.CreateOntologyProperty(new Uri("http://example.org/vehicles/LimitedSpeed"));

        //Get siblings
        var siblings = limitedSpeed.Siblings.ToList();
        Assert.Equal(2, siblings.Count);
        Assert.DoesNotContain(limitedSpeed, siblings);
    }

    [Fact]
    public void OntologyProperties()
    {
        var g = new OntologyGraph();
        g.LoadFromFile(Path.Combine("resources", "ontology.ttl"));

        //Check Property Counts
        Assert.Single(g.RdfProperties);
        Assert.Single(g.OwlAnnotationProperties);
        Assert.Single(g.OwlDatatypeProperties);
        Assert.Single(g.OwlObjectProperties);
        Assert.Equal(3, g.OwlProperties.Count());
        Assert.Equal(4, g.AllProperties.Count());
    }

    [Fact]
    public void OntologyClasses()
    {
        var g = new OntologyGraph();
        g.LoadFromFile(Path.Combine("resources", "ontology.ttl"));

        //Check Class Counts
        Assert.Single(g.RdfClasses);
        Assert.Single(g.OwlClasses);
        Assert.Equal(2, g.AllClasses.Count());
    }

    [Fact]
    public void OntologyClassCount1()
    {
        var g = new OntologyGraph();
        g.LoadFromFile(Path.Combine("resources", "swrc.owl"));
        Assert.False(g.IsEmpty);

        //Count classes, raw and distinct count should be same
        var count = g.OwlClasses.Count();
        var distinctCount = g.OwlClasses.Select(c => c.Resource).Distinct().Count();

        _output.WriteLine("Count = " + count);
        _output.WriteLine("Distinct Count = " + distinctCount);

        Assert.True(count == distinctCount, "Expected raw and distinct counts to be the same, got " + count + " and " + distinctCount);
    }

    [Fact]
    public void OntologyClassCount2()
    {
        var g = new OntologyGraph();
        g.LoadFromFile(Path.Combine("resources", "swrc.owl"));
        Assert.False(g.IsEmpty);

        OntologyClass classOfClasses = g.CreateOntologyClass(g.CreateUriNode("owl:Class"));
        var count = 0;
        var resources = new HashSet<INode>();

        //This iterates over the things that are a class
        foreach (OntologyResource c in classOfClasses.Instances)
        {
            count++;
            resources.Add(c.Resource);
        }

        _output.WriteLine("Count = " + count);
        _output.WriteLine("Distinct Count = " + resources.Count);

        Assert.Equal(resources.Count, count);
    }
}
