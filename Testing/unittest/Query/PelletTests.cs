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
using VDS.RDF.Query;
using VDS.RDF.Query.Inference;
using VDS.RDF.Query.Inference.Pellet;
using VDS.RDF.Query.Inference.Pellet.Services;

namespace VDS.RDF.Test
{
    [TestClass]
    public class PelletTests
    {
        public const String PelletTestServer = "http://ps.clarkparsia.com/";

        [TestMethod]
        public void PelletKBList()
        {
                PelletServer server = new PelletServer(PelletTestServer);

                foreach (KnowledgeBase kb in server.KnowledgeBases)
                {
                    Console.WriteLine(kb.Name);
                    Console.WriteLine("Services:");
                    foreach (PelletService svc in kb.Services)
                    {
                        Console.WriteLine("  " + svc.Name);
                        Console.WriteLine("    Uri: " + svc.Endpoint.Uri);
                        Console.WriteLine("    HTTP Methods: " + String.Join(",", svc.Endpoint.HttpMethods.ToArray()));
                        Console.WriteLine("    Response MIME Types: " + String.Join(",", svc.MimeTypes.ToArray()));
                    }
                    Console.WriteLine();
                }
        }

        [TestMethod]
        public void PelletQuery()
        {
                PelletServer server = new PelletServer(PelletTestServer);
                Type svcType = typeof(QueryService);

                foreach (KnowledgeBase kb in server.KnowledgeBases)
                {
                    if (kb.SupportsService(svcType)) 
                    {
                        Console.WriteLine(kb.Name + " supports SPARQL Query");
                        QueryService q = (QueryService)kb.GetService(svcType);
                        Object results = q.Query("SELECT * WHERE {?s a ?type} LIMIT 10");
                        if (results is SparqlResultSet)
                        {
                            TestTools.ShowResults(results);
                        }
                        else
                        {
                            Console.WriteLine("Unexpected Result type from Query Service");
                        }
                    } 
                    else 
                    {
                        Console.WriteLine(kb.Name + " does not support the Query Service");
                    }
                    Console.WriteLine();
                }
        }

        [TestMethod]
        public void PelletRealize()
        {
                PelletServer server = new PelletServer(PelletTestServer);
                Type svcType = typeof(RealizeService);

                foreach (KnowledgeBase kb in server.KnowledgeBases)
                {
                    if (kb.SupportsService(svcType))
                    {
                        Console.WriteLine(kb.Name + " supports Realize");
                        RealizeService svc = (RealizeService)kb.GetService(svcType);
                        IGraph g = svc.Realize();
                        TestTools.ShowGraph(g);
                    }
                    else
                    {
                        Console.WriteLine(kb.Name + " does not support the Realize Service");
                    }
                    Console.WriteLine();
                }
        }

        [TestMethod]
        public void PelletConsistency()
        {
                PelletServer server = new PelletServer(PelletTestServer);
                Type svcType = typeof(ConsistencyService);

                foreach (KnowledgeBase kb in server.KnowledgeBases)
                {
                    if (kb.SupportsService(svcType))
                    {
                        Console.WriteLine(kb.Name + " supports Consistency");
                        ConsistencyService svc = (ConsistencyService)kb.GetService(svcType);
                        Console.WriteLine("Consistency: " + svc.IsConsistent().ToString());
                    }
                    else
                    {
                        Console.WriteLine(kb.Name + " does not support the Consistency Service");
                    }
                    Console.WriteLine();
                }
         }

        [TestMethod]
        public void PelletSearch()
        {
                PelletServer server = new PelletServer(PelletTestServer);
                Type svcType = typeof(SearchService);

                foreach (KnowledgeBase kb in server.KnowledgeBases)
                {
                    if (kb.SupportsService(svcType))
                    {
                        Console.WriteLine(kb.Name + " supports Search");
                        SearchService svc = (SearchService)kb.GetService(svcType);
                        foreach (SearchServiceResult result in svc.Search("cabernet"))
                        {
                            Console.WriteLine(result.ToString());
                        }
                    }
                    else
                    {
                        Console.WriteLine(kb.Name + " does not support the Search Service");
                    }
                    Console.WriteLine();
                }
        }

        [TestMethod]
        public void PelletNamespace()
        {
                PelletServer server = new PelletServer(PelletTestServer);
                Type svcType = typeof(NamespaceService);

                foreach (KnowledgeBase kb in server.KnowledgeBases)
                {
                    if (kb.SupportsService(svcType))
                    {
                        Console.WriteLine(kb.Name + " supports Namespaces");
                        NamespaceService svc = (NamespaceService)kb.GetService(svcType);
                        NamespaceMapper nsmap = svc.GetNamespaces();
                        foreach (String prefix in nsmap.Prefixes)
                        {
                            Console.WriteLine(prefix + ": " + nsmap.GetNamespaceUri(prefix).AbsoluteUri);
                        }
                    }
                    else
                    {
                        Console.WriteLine(kb.Name + " does not support the Search Service");
                    }
                    Console.WriteLine();
                }
        }

        [TestMethod]
        public void PelletIcv()
        {
                PelletServer server = new PelletServer(PelletTestServer);
                Type svcType = typeof(IntegrityConstraintValidationService);

                foreach (KnowledgeBase kb in server.KnowledgeBases)
                {
                    if (kb.SupportsService(svcType))
                    {
                        Console.WriteLine(kb.Name + " supports ICV");
                        IntegrityConstraintValidationService svc = (IntegrityConstraintValidationService)kb.GetService(svcType);

                        ITripleStore store = svc.Validate();
                        Console.WriteLine("ICV returned " + store.Graphs.Count + " with " + store.Graphs.Sum(g => g.Triples.Count) + " Triples");

                        foreach (Graph g in store.Graphs)
                        {
                            TestTools.ShowGraph(g);
                            Console.WriteLine();
                        }
                    }
                    else
                    {
                        Console.WriteLine(kb.Name + " does not support the ICV Service");
                    }
                    Console.WriteLine();
                }
        }

        [TestMethod]
        public void PelletCluster()
        {
                PelletServer server = new PelletServer(PelletTestServer);
                Type svcType = typeof(ClusterService);

                foreach (KnowledgeBase kb in server.KnowledgeBases)
                {
                    if (kb.SupportsService(svcType))
                    {
                        Console.WriteLine(kb.Name + " supports Clustering");
                        ClusterService svc = (ClusterService)kb.GetService(svcType);
                        Console.WriteLine("Cluster=3");
                        List<List<INode>> clusters = svc.Cluster(3);
                        Console.WriteLine(clusters.Count + " Clusters returned");
                        for (int i = 0; i < clusters.Count; i++)
                        {
                            Console.WriteLine("Cluster " + (i + 1) + " contains " + clusters[i].Count + " Items");
                        }
                    }
                    else
                    {
                        Console.WriteLine(kb.Name + " does not support the Cluster Service");
                    }
                    Console.WriteLine();
                }
        }

        [TestMethod]
        public void PelletClusterWithType()
        {
                PelletServer server = new PelletServer(PelletTestServer);
                Type svcType = typeof(ClusterService);

                foreach (KnowledgeBase kb in server.KnowledgeBases)
                {
                    if (kb.SupportsService(svcType))
                    {
                        Console.WriteLine(kb.Name + " supports Clustering");
                        ClusterService svc = (ClusterService)kb.GetService(svcType);
                        Console.WriteLine("Cluster=3 and Type=wine:WineGrape");
                        List<List<INode>> clusters = svc.Cluster(3, "wine:WineGrape");
                        Console.WriteLine(clusters.Count + " Clusters returned");
                        for (int i = 0; i < clusters.Count; i++)
                        {
                            Console.WriteLine("Cluster " + (i + 1) + " contains " + clusters[i].Count + " Items");
                        }
                    }
                    else
                    {
                        Console.WriteLine(kb.Name + " does not support the Cluster Service");
                    }
                    Console.WriteLine();
                }
        }

        [TestMethod]
        public void PelletSimilarity()
        {
                PelletServer server = new PelletServer(PelletTestServer);
                Type svcType = typeof(SimilarityService);

                foreach (KnowledgeBase kb in server.KnowledgeBases)
                {
                    if (kb.SupportsService(svcType))
                    {
                        Console.WriteLine(kb.Name + " supports Similarity");
                        SimilarityService svc = (SimilarityService)kb.GetService(svcType);

                        IGraph g = svc.SimilarityRaw(5, "wine:Red");
                        TestTools.ShowGraph(g);
                    }
                    else
                    {
                        Console.WriteLine(kb.Name + " does not support the Similarity Service");
                    }
                    Console.WriteLine();
                }
        }

        [TestMethod]
        public void PelletSimilarity2()
        {
                PelletServer server = new PelletServer(PelletTestServer);
                Type svcType = typeof(SimilarityService);

                foreach (KnowledgeBase kb in server.KnowledgeBases)
                {
                    if (kb.SupportsService(svcType))
                    {
                        Console.WriteLine(kb.Name + " supports Similarity");
                        SimilarityService svc = (SimilarityService)kb.GetService(svcType);

                        List<KeyValuePair<INode, double>> results = svc.Similarity(5, "wine:Red");
                        foreach (KeyValuePair<INode, double> kvp in results)
                        {
                            Console.WriteLine(kvp.Key.ToString() + " (Similarity = " + kvp.Value + ")");
                        }
                    }
                    else
                    {
                        Console.WriteLine(kb.Name + " does not support the Similarity Service");
                    }
                    Console.WriteLine();
                }
        }

        [TestMethod]
        public void PelletPredict()
        {
                PelletServer server = new PelletServer(PelletTestServer);
                Type svcType = typeof(PredictService);

                foreach (KnowledgeBase kb in server.KnowledgeBases)
                {
                    if (kb.SupportsService(svcType))
                    {
                        Console.WriteLine(kb.Name + " supports Prediction");
                        PredictService svc = (PredictService)kb.GetService(svcType);

                        IGraph g = svc.PredictRaw("wine:DAnjou", "wine:locatedIn");
                        TestTools.ShowGraph(g);
                    }
                    else
                    {
                        Console.WriteLine(kb.Name + " does not support the Prediction Service");
                    }
                    Console.WriteLine();
                }
        }

        [TestMethod]
        public void PelletPredict2()
        {
                PelletServer server = new PelletServer(PelletTestServer);
                Type svcType = typeof(PredictService);

                foreach (KnowledgeBase kb in server.KnowledgeBases)
                {
                    if (kb.SupportsService(svcType))
                    {
                        Console.WriteLine(kb.Name + " supports Prediction");
                        PredictService svc = (PredictService)kb.GetService(svcType);

                        List<INode> predictions = svc.Predict("wine:DAnjou", "wine:locatedIn");
                        Console.WriteLine(predictions.Count + " Predictions");
                        foreach (INode obj in predictions)
                        {
                            Console.WriteLine(obj.ToString());
                        }
                    }
                    else
                    {
                        Console.WriteLine(kb.Name + " does not support the Prediction Service");
                    }
                    Console.WriteLine();
                }
        }
    }
}

