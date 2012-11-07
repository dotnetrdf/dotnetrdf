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
