using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Storage;
using Alexandria;
using Alexandria.Documents;
using Alexandria.Documents.GraphRegistry;

namespace alexandria_tests
{
    [TestClass]
    public class GraphRegistryTests
    {
        [TestMethod]
        public void FSGraphRegistryEnumerate()
        {
            //Load in our Test Graph
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");

            //Open an Alexandria Store and save the Graph
            AlexandriaFileManager manager = new AlexandriaFileManager(TestTools.GetNextStoreID());
            manager.SaveGraph(g);

            //Now load another Graph and add it
            Graph i = new Graph();
            FileLoader.Load(i, "test.nt");

            //Save it to the Store which should overwrite it and then load it back
            manager.SaveGraph(i);

            //Now enumerate the Graph URIs and Document Names
            TestWrapper<StreamReader,TextWriter> wrapper = new TestWrapper<StreamReader,TextWriter>(manager);
            IGraphRegistry registry = wrapper.DocumentManager.GraphRegistry;
            Console.WriteLine("Graph URIs");
            foreach (String uri in registry.GraphUris)
            {
                Console.WriteLine(uri);
            }
            Console.WriteLine();
            Console.WriteLine("Document Names");
            foreach (String name in registry.DocumentNames)
            {
                Console.WriteLine(name);
            }
            Console.WriteLine();
            Console.WriteLine("Document -> Graph Mappings");
            foreach (KeyValuePair<String, String> kvp in registry.DocumentToGraphMappings)
            {
                Console.WriteLine(kvp.Key + " -> " + kvp.Value);
            }
            Console.WriteLine();
            Console.WriteLine("Graph -> Document Mappins");
            foreach (KeyValuePair<String, String> kvp in registry.GraphToDocumentMappings)
            {
                Console.WriteLine(kvp.Key + " -> " + kvp.Value);
            }
            Console.WriteLine();

            manager.Dispose();
        }

        [TestMethod]
        public void FSGraphRegistryEnumerateWithDefaultGraph()
        {
            //Load in our Test Graph
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            g.BaseUri = null;

            //Open an Alexandria Store and save the Graph
            AlexandriaFileManager manager = new AlexandriaFileManager(TestTools.GetNextStoreID());
            manager.SaveGraph(g);

            //Now load another Graph and add it
            Graph i = new Graph();
            FileLoader.Load(i, "test.nt");

            //Save it to the Store which should overwrite it and then load it back
            manager.SaveGraph(i);

            //Now enumerate the Graph URIs and Document Names
            TestWrapper<StreamReader,TextWriter> wrapper = new TestWrapper<StreamReader,TextWriter>(manager);
            IGraphRegistry registry = wrapper.DocumentManager.GraphRegistry;
            Console.WriteLine("Graph URIs");
            foreach (String uri in registry.GraphUris)
            {
                Console.WriteLine(uri);
            }
            Console.WriteLine();
            Console.WriteLine("Document Names");
            foreach (String name in registry.DocumentNames)
            {
                Console.WriteLine(name);
            }
            Console.WriteLine();
            Console.WriteLine("Document -> Graph Mappings");
            foreach (KeyValuePair<String, String> kvp in registry.DocumentToGraphMappings)
            {
                Console.WriteLine(kvp.Key + " -> " + kvp.Value);
            }
            Console.WriteLine();
            Console.WriteLine("Graph -> Document Mappins");
            foreach (KeyValuePair<String, String> kvp in registry.GraphToDocumentMappings)
            {
                Console.WriteLine(kvp.Key + " -> " + kvp.Value);
            }
            Console.WriteLine();

            manager.Dispose();
        }

        [TestMethod]
        public void FSGraphRegistryLookup()
        {
            //Load in our Test Graph
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");

            //Open an Alexandria Store and save the Graph
            AlexandriaFileManager manager = new AlexandriaFileManager(TestTools.GetNextStoreID());
            manager.SaveGraph(g);

            TestWrapper<StreamReader,TextWriter> wrapper = new TestWrapper<StreamReader,TextWriter>(manager);
            String graphUri = (g.BaseUri != null) ? g.BaseUri.ToString() : String.Empty;
            String name = wrapper.DocumentManager.GraphRegistry.GetDocumentName(graphUri);

            String retrievedDocumentName = wrapper.DocumentManager.GraphRegistry.GetDocumentName(graphUri);
            Assert.AreEqual(name, retrievedDocumentName, "Document Names should have been equal");
            String retrievedGraphUri = wrapper.DocumentManager.GraphRegistry.GetGraphUri(name);
            Assert.AreEqual(graphUri, retrievedGraphUri, "Graph URIs should have been equal");

            manager.Dispose();
        }

        [TestMethod]
        public void FSGraphRegisterUnregister()
        {
            //Load in our Test Graph
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            g.BaseUri = null;

            //Open an Alexandria Store and save the Graph
            AlexandriaFileManager manager = new AlexandriaFileManager(TestTools.GetNextStoreID());
            manager.SaveGraph(g);

            //Now load another Graph and add it
            Graph i = new Graph();
            FileLoader.Load(i, "test.nt");

            //Save it to the Store which should overwrite it and then load it back
            manager.SaveGraph(i);

            //Now enumerate the Graph URIs and Document Names
            TestWrapper<StreamReader,TextWriter> wrapper = new TestWrapper<StreamReader,TextWriter>(manager);
            IGraphRegistry registry = wrapper.DocumentManager.GraphRegistry;
            Console.WriteLine("Graph URIs");
            foreach (String uri in registry.GraphUris)
            {
                Console.WriteLine(uri);
            }
            Assert.IsTrue(registry.GraphUris.Count() == 2, "Should have been 2 Graph URIs");
            Console.WriteLine();
            Console.WriteLine("Document Names");
            foreach (String name in registry.DocumentNames)
            {
                Console.WriteLine(name);
            }
            Assert.IsTrue(registry.DocumentNames.Count() == 2, "Should have been 2 Document Names");

            //Now delete one of the Graphs
            manager.DeleteGraph(String.Empty);

            //Again enumerate the Graph URIs and Document Names
            Console.WriteLine("Graph URIs");
            foreach (String uri in registry.GraphUris)
            {
                Console.WriteLine(uri);
            }
            Assert.IsTrue(registry.GraphUris.Count() == 1, "Should have been only 1 Graph URIs");
            Console.WriteLine();
            Console.WriteLine("Document Names");
            foreach (String name in registry.DocumentNames)
            {
                Console.WriteLine(name);
            }
            Assert.IsTrue(registry.DocumentNames.Count() == 1, "Should have been only 1 Document Names");

            manager.Dispose();
        }
    }
}
