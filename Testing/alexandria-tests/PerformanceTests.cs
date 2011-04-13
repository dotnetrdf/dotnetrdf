using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Storage;
using VDS.Alexandria;
using VDS.RDF.Parsing.Handlers;

namespace alexandria_tests
{
    [TestClass]
    public class PerformanceTests
    {
        [TestMethod]
        public void FSLargeLoadUsingWriteToStoreHandlerAndFullIndices()
        {
            //Open an Alexandria Store and save the Graph
            AlexandriaFileManager manager = new AlexandriaFileManager(TestTools.GetNextStoreID(), AlexandriaFileManager.FullIndices);

            //Load in our Test Graph
            WriteToStoreHandler handler = new WriteToStoreHandler(manager);
            TurtleParser parser = new TurtleParser();
            parser.Load(handler, "dataset_50.ttl");

            manager.Dispose();
        }

        [TestMethod]
        public void FSLargeLoadUsingWriteToStoreHandlerAndOptimalIndices()
        {
            //Open an Alexandria Store and save the Graph
            AlexandriaFileManager manager = new AlexandriaFileManager(TestTools.GetNextStoreID(), AlexandriaFileManager.OptimalIndices);

            //Load in our Test Graph
            WriteToStoreHandler handler = new WriteToStoreHandler(manager);
            TurtleParser parser = new TurtleParser();
            parser.Load(handler, "dataset_50.ttl");

            manager.Dispose();
        }

        [TestMethod]
        public void FSLargeLoadUsingWriteToStoreHandlerAndSimpleIndices()
        {
            //Open an Alexandria Store and save the Graph
            AlexandriaFileManager manager = new AlexandriaFileManager(TestTools.GetNextStoreID(), AlexandriaFileManager.SimpleIndices);

            //Load in our Test Graph
            WriteToStoreHandler handler = new WriteToStoreHandler(manager);
            TurtleParser parser = new TurtleParser();
            parser.Load(handler, "dataset_50.ttl");

            manager.Dispose();
        }

        [TestMethod]
        public void FSLargeLoadUsingWriteToStoreHandlerAndNoIndices()
        {
            //Open an Alexandria Store and save the Graph
            AlexandriaFileManager manager = new AlexandriaFileManager(TestTools.GetNextStoreID(), null);

            //Load in our Test Graph
            WriteToStoreHandler handler = new WriteToStoreHandler(manager);
            TurtleParser parser = new TurtleParser();
            parser.Load(handler, "dataset_50.ttl");

            manager.Dispose();
        }

        [TestMethod]
        public void FSLargeLoadUsingWriteToStoreHandlerAndNoIndices2()
        {
            //Open an Alexandria Store and save the Graph
            NonIndexedAlexandriaFileManager manager = new NonIndexedAlexandriaFileManager(TestTools.GetNextStoreID());

            //Load in our Test Graph
            WriteToStoreHandler handler = new WriteToStoreHandler(manager);
            TurtleParser parser = new TurtleParser();
            parser.Load(handler, "dataset_50.ttl");

            manager.Dispose();
        }

        [TestMethod]
        public void FSLargeLoadUsingSaveGraphAndFullIndices()
        {
            Graph g = new Graph();
            FileLoader.Load(g, "dataset_50.ttl");

            AlexandriaFileManager manager = new AlexandriaFileManager(TestTools.GetNextStoreID(), AlexandriaFileManager.FullIndices);
            manager.SaveGraph(g);

            manager.Dispose();
        }

        [TestMethod]
        public void FSLargeLoadUsingSaveGraphAndOptimalIndices()
        {
            Graph g = new Graph();
            FileLoader.Load(g, "dataset_50.ttl");

            AlexandriaFileManager manager = new AlexandriaFileManager(TestTools.GetNextStoreID(), AlexandriaFileManager.OptimalIndices);
            manager.SaveGraph(g);

            manager.Dispose();
        }

        [TestMethod]
        public void FSLargeLoadUsingSaveGraphAndSimpleIndices()
        {
            Graph g = new Graph();
            FileLoader.Load(g, "dataset_50.ttl");

            AlexandriaFileManager manager = new AlexandriaFileManager(TestTools.GetNextStoreID(), AlexandriaFileManager.SimpleIndices);
            manager.SaveGraph(g);

            manager.Dispose();
        }

        [TestMethod]
        public void FSLargeLoadUsingSaveGraphAndNoIndices()
        {
            Graph g = new Graph();
            FileLoader.Load(g, "dataset_50.ttl");

            AlexandriaFileManager manager = new AlexandriaFileManager(TestTools.GetNextStoreID(), null);
            manager.SaveGraph(g);

            manager.Dispose();
        }

        [TestMethod]
        public void FSLargeLoadUsingSaveGraphAndNoIndices2()
        {
            Graph g = new Graph();
            FileLoader.Load(g, "dataset_50.ttl");

            NonIndexedAlexandriaFileManager manager = new NonIndexedAlexandriaFileManager(TestTools.GetNextStoreID());
            manager.SaveGraph(g);

            manager.Dispose();
        }
    }
}
