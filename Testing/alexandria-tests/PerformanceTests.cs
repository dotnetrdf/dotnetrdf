using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Storage;
using Alexandria;

namespace alexandria_tests
{
    [TestClass]
    public class PerformanceTests
    {
        [TestMethod]
        public void LargeLoadUsingWriteOnlyGraph()
        {
            //Open an Alexandria Store and save the Graph
            AlexandriaFileManager manager = new AlexandriaFileManager("test");

            //Load in our Test Graph
            WriteOnlyStoreGraph g = new WriteOnlyStoreGraph((Uri)null, manager);
            FileLoader.Load(g, "dataset_50.ttl");
            g.Dispose();

            manager.Dispose();
        }

        [TestMethod]
        public void LargeLoadUsingSaveGraph()
        {
            Graph g = new Graph();
            FileLoader.Load(g, "dataset_50.ttl");

            AlexandriaFileManager manager = new AlexandriaFileManager("test");
            manager.SaveGraph(g);

            manager.Dispose();
        }

        [TestMethod]
        public void LargeLoadUsingSaveGraphAndPartialIndices()
        {
            Graph g = new Graph();
            FileLoader.Load(g, "dataset_50.ttl");

            AlexandriaFileManager manager = new AlexandriaFileManager("test", AlexandriaFileManager.PartialIndices);
            manager.SaveGraph(g);

            manager.Dispose();
        }

        [TestMethod]
        public void LargeLoadUsingSaveGraphAndSimpleIndices()
        {
            Graph g = new Graph();
            FileLoader.Load(g, "dataset_50.ttl");

            AlexandriaFileManager manager = new AlexandriaFileManager("test", AlexandriaFileManager.SimpleIndices);
            manager.SaveGraph(g);

            manager.Dispose();
        }

        [TestMethod]
        public void LargeLoadUsingSaveGraphAndNoIndices()
        {
            Graph g = new Graph();
            FileLoader.Load(g, "dataset_50.ttl");

            AlexandriaFileManager manager = new AlexandriaFileManager("test", null);
            manager.SaveGraph(g);

            manager.Dispose();
        }

        [TestMethod]
        public void LargeLoadUsingSaveGraphAndNoIndices2()
        {
            Graph g = new Graph();
            FileLoader.Load(g, "dataset_50.ttl");

            NonIndexedAlexandriaFileManager manager = new NonIndexedAlexandriaFileManager("test");
            manager.SaveGraph(g);

            manager.Dispose();
        }
    }
}
