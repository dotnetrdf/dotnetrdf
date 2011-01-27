using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Interop;
using VDS.RDF.Interop.Jena;
using VDS.RDF.Parsing;
using VDS.RDF.Storage;
using com.hp.hpl.jena.rdf.model;

namespace VDS.RDF.Test.Interop
{
    [TestClass]
    public class JenaInteropTests
    {
        //[TestMethod]
        //public void GraphConversionForJena()
        //{
        //    Graph g = new Graph();
        //    FileLoader.Load(g, "Turtle.ttl");

        //    Console.WriteLine("Initial Graph");
        //    TestTools.ShowGraph(g);

        //    Model m = ModelFactory.createDefaultModel();
        //    JenaConverter.ToJena(g, m);

        //    Graph h = new Graph();
        //    JenaConverter.FromJena(m, h);

        //    Console.WriteLine("Graph after conversion to and from Jena");
        //    TestTools.ShowGraph(h);

        //    Assert.AreEqual(g, h, "1 - Graphs should have been equal");

        //    Model m2 = ModelFactory.createMemModelMaker().createFreshModel();
        //    JenaConverter.ToJena(h, m2);

        //    Graph i = new Graph();
        //    JenaConverter.FromJena(m2, i);

        //    Console.WriteLine("Graph after further conversion to and from Jena");
        //    TestTools.ShowGraph(i);

        //    Assert.AreEqual(h, i, "2 - Graphs should have been equal");
        //}
    }
}
