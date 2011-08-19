using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query;
using VDS.RDF.Query.Inference.Pellet;
using VDS.RDF.Query.Inference.Pellet.Services;
using VDS.RDF.Update;
using VDS.RDF.Writing.Formatting;

namespace wp7_tests
{
    public partial class MainPage : PhoneApplicationPage
    {
        private SparqlRemoteEndpoint _endpoint;
        private SparqlRemoteUpdateEndpoint _updateEndpoint;

        public const String TestRemoteQueryEndpoint = "http://dbpedia.org/sparql",
                            TestRemoteQueryEndpointDefaultGraph = "http://dbpedia.org",
                            TestRemoteUpdateEndpoint = "http://localhost/demos/server/update",
                            TestPelletServer = "http://ps.clarkparsia.com";

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            //Set up our test endpoints
            this._endpoint = new SparqlRemoteEndpoint(new Uri(TestRemoteQueryEndpoint), TestRemoteQueryEndpointDefaultGraph);
            this._updateEndpoint = new SparqlRemoteUpdateEndpoint(new Uri(TestRemoteUpdateEndpoint));
        }

        #region Callbacks

        private void GraphCallback(IGraph g, Object state)
        {
            Dispatcher.BeginInvoke(() =>
                {
                    TurtleFormatter formatter = new TurtleFormatter(g.NamespaceMap);
                    this.ResultsSummary.Text = g.Triples.Count + " Triple(s) returned";
                    this.ResultsList.Items.Clear();
                    foreach (Triple t in g.Triples)
                    {
                        this.ResultsList.Items.Add(t.ToString(formatter));
                    }
                });
        }

        private void SparqlResultsCallback(SparqlResultSet results, Object state)
        {
            Dispatcher.BeginInvoke(() =>
                {
                    DateTime? start = state as DateTime?;
                    if (start != null)
                    {
                        TimeSpan elapsed = DateTime.Now - start.Value;
                        //Do what you want with the execution time...
                    }

                    SparqlFormatter formatter = new SparqlFormatter();
                    this.ResultsSummary.Text = results.Count + " Result(s) returned";
                    this.ResultsList.Items.Clear();

                    switch (results.ResultsType)
                    {
                        case SparqlResultsType.Boolean:
                            this.ResultsList.Items.Add(formatter.FormatBooleanResult(results.Result));
                            break;
                        case SparqlResultsType.VariableBindings:
                            foreach (SparqlResult r in results)
                            {
                                this.ResultsList.Items.Add(r.ToString(formatter));
                            }
                            break;
                        default:
                            this.ResultsList.Items.Add("Unknown Results Type");
                            break;
                    }
                });
            
        }

        private void RdfHandlerCallback(IRdfHandler handler, Object state)
        {
            Dispatcher.BeginInvoke(() =>
                {
                    this.ResultsList.Items.Clear();

                    if (handler is CountHandler)
                    {
                        this.ResultsSummary.Text = "Handler counted " + ((CountHandler)handler).Count + " Triple(s)";
                    }
                    else
                    {
                        this.ResultsSummary.Text = "Parsing with " + handler.GetType().Name + " Completed";
                    }
                });
        }

        private void TripleStoreCallback(ITripleStore store, Object state)
        {
            Dispatcher.BeginInvoke(() =>
                {
                    TurtleFormatter formatter = new TurtleFormatter(new NamespaceMapper());
                    this.ResultsSummary.Text = store.Graphs.Count + " Graph(s) with " + store.Graphs.Sum(g => g.Triples.Count) + " Triple(s)";
                    this.ResultsList.Items.Clear();
                    foreach (IGraph g in store.Graphs)
                    {
                        this.ResultsList.Items.Add("Graph " + formatter.FormatUri(g.BaseUri));
                        foreach (Triple t in g.Triples)
                        {
                            this.ResultsList.Items.Add(t.ToString(formatter));
                        }
                    }
                });
        }

        private void UpdateCallback(Object state)
        {
            Dispatcher.BeginInvoke(() =>
                {
                    this.ResultsList.Items.Clear();
                    this.ResultsSummary.Text = "Update Completed OK";
                });
        }

        private void PelletServerReadyCallback(PelletServer server, Object state)
        {
            Dispatcher.BeginInvoke(() =>
                {
                    this.ResultsSummary.Text = server.KnowledgeBases.Count() + " Knowledge Bases available";
                    this.ResultsList.Items.Clear();
                    foreach (KnowledgeBase kb in server.KnowledgeBases)
                    {
                        this.ResultsList.Items.Add(kb.Name);
                        foreach (PelletService svc in kb.Services)
                        {
                            this.ResultsList.Items.Add(kb.Name + "/" + svc.Name);
                        }
                    }
                });
        }

        private void NamespaceCallback(INamespaceMapper nsmap, Object state)
        {
            Dispatcher.BeginInvoke(() =>
                {
                    this.ResultsSummary.Text = nsmap.Prefixes.Count() + " Namespaces defined";
                    this.ResultsList.Items.Clear();
                    foreach (String prefix in nsmap.Prefixes)
                    {
                        this.ResultsList.Items.Add(prefix + ": <" + nsmap.GetNamespaceUri(prefix).ToString() + ">");
                    }
                });
        }

        private void NodeListCallback(List<INode> nodes, Object state)
        {
            Dispatcher.BeginInvoke(() =>
                {
                    this.ResultsSummary.Text = nodes.Count + " Node(s) returned";
                    this.ResultsList.Items.Clear();
                    NTriplesFormatter formatter = new NTriplesFormatter();
                    foreach (INode n in nodes)
                    {
                        this.ResultsList.Items.Add(n.ToString(formatter));
                    }
                });
        }

        private void PelletSearchCallback(List<SearchServiceResult> results, Object state)
        {
            Dispatcher.BeginInvoke(() =>
                {
                    this.ResultsSummary.Text = results.Count + " Result(s) returned";
                    this.ResultsList.Items.Clear();
                    NTriplesFormatter formatter = new NTriplesFormatter();
                    foreach (SearchServiceResult res in results)
                    {
                        this.ResultsList.Items.Add("Score " + res.Score + " " + res.Node.ToString(formatter));
                    }
                });
        }

        #endregion

        #region Test Methods

        private void RemoteSparqlTest1_Click(object sender, RoutedEventArgs e)
        {
            this._endpoint.QueryWithResultSet("SELECT ?type WHERE { ?s a ?type } LIMIT 10", this.SparqlResultsCallback, null);
        }

        private void RemoteSparqlTest2_Click(object sender, RoutedEventArgs e)
        {
            this._endpoint.QueryWithResultGraph("CONSTRUCT { ?s a ?type } WHERE { ?s a ?type } LIMIT 10", this.GraphCallback, null);
        }

        private void UriLoaderTest1_Click(object sender, RoutedEventArgs e)
        {
            UriLoader.Load(new Graph(), new Uri("http://dbpedia.org/resource/Ilkeston"), this.GraphCallback, null);
        }

        private void UriLoaderTest2_Click(object sender, RoutedEventArgs e)
        {
            CountHandler handler = new CountHandler();
            UriLoader.Load(handler, new Uri("http://dbpedia.org/resource/Ilkeston"), this.RdfHandlerCallback, null);
        }

        private void UriLoaderTest3_Click(object sender, RoutedEventArgs e)
        {
            UriLoader.Load(new TripleStore(), new Uri("http://localhost/demos/sampleDataset"), this.TripleStoreCallback, null);
        }

        private void UriLoaderTest4_Click(object sender, RoutedEventArgs e)
        {
            CountHandler handler = new CountHandler();
            UriLoader.LoadDataset(handler, new Uri("http://localhost/demos/sampleDataset"), this.RdfHandlerCallback, null);
        }

        private void RemoteSparqlUpdateTest1_Click(object sender, RoutedEventArgs e)
        {
            this._updateEndpoint.Update("LOAD <http://dbpedia.org/resource/Ilkeston> INTO GRAPH <http://example.org/ilson>", this.UpdateCallback, null);
        }

        #endregion

        private void PelletServerTest_Click(object sender, RoutedEventArgs e)
        {
            PelletServer.Connect(TestPelletServer, this.PelletServerReadyCallback, null);
        }

        private void PelletClassifyTest_Click(object sender, RoutedEventArgs e)
        {
            PelletServer.Connect(TestPelletServer, (svr,_) =>
                {
                    ClassifyService svc = svr.KnowledgeBases.First(kb => kb.SupportsService<ClassifyService>()).GetService<ClassifyService>();
                    svc.Classify(this.GraphCallback, null);
                }, null);
        }

        private void PelletClusterTest_Click(object sender, RoutedEventArgs e)
        {
            PelletServer.Connect(TestPelletServer, (svr, _) =>
                {
                    ClusterService svc = svr.KnowledgeBases.First(kb => kb.SupportsService<ClusterService>()).GetService<ClusterService>();
                    svc.ClusterRaw(3, this.GraphCallback, null);
                }, null);
        }

        private void PelletConsistencyTest_Click(object sender, RoutedEventArgs e)
        {
            PelletServer.Connect(TestPelletServer, (svr, _) =>
                {
                    ConsistencyService svc = svr.KnowledgeBases.First(kb => kb.SupportsService<ConsistencyService>()).GetService<ConsistencyService>();
                    svc.IsConsistent((ok, s) =>
                        {
                            Dispatcher.BeginInvoke(() =>
                                {
                                    this.ResultsList.Items.Clear();
                                    this.ResultsSummary.Text = "Knowledge Base is " + (ok ? "consistent" : "NOT consistent");
                                });
                        }, null);
                }, null);
        }

        private void PelletExplainTest_Click(object sender, RoutedEventArgs e)
        {
            PelletServer.Connect(TestPelletServer, (svr, _) =>
                {
                    ExplainService svc = svr.KnowledgeBases.First(kb => kb.SupportsService<ExplainService>()).GetService<ExplainService>();
                    svc.Explain("SELECT * WHERE { ?s a ?type }", this.GraphCallback, null);
                }, null);
        }

        private void PelletICVTest_Click(object sender, RoutedEventArgs e)
        {
            PelletServer.Connect(TestPelletServer, (svr, _) =>
                {
                    IntegrityConstraintValidationService svc = svr.KnowledgeBases.First(kb => kb.SupportsService<IntegrityConstraintValidationService>()).GetService<IntegrityConstraintValidationService>();
                    svc.Validate(this.TripleStoreCallback, null);
                }, null);
        }

        private void PelletNamespaceTest_Click(object sender, RoutedEventArgs e)
        {
            PelletServer.Connect(TestPelletServer, (svr, _) =>
            {
                NamespaceService svc = svr.KnowledgeBases.First(kb => kb.SupportsService<NamespaceService>()).GetService<NamespaceService>();
                svc.GetNamespaces(this.NamespaceCallback, null);
            }, null);
        }

        private void PelletPredictTest_Click(object sender, RoutedEventArgs e)
        {
            PelletServer.Connect(TestPelletServer, (svr, _) =>
                {
                    PredictService svc = svr.KnowledgeBases.First(kb => kb.SupportsService<PredictService>()).GetService<PredictService>();
                    svc.Predict("wine:DAnjou", "wine:locatedIn", this.NodeListCallback, null);
                }, null);
        }

        private void PelletQueryTest_Click(object sender, RoutedEventArgs e)
        {
            PelletServer.Connect(TestPelletServer, (svr, _) =>
                {
                    QueryService svc = svr.KnowledgeBases.First(kb => kb.SupportsService<QueryService>()).GetService<QueryService>();
                    svc.Query("SELECT * WHERE { ?s a ?type }", this.GraphCallback, this.SparqlResultsCallback, null);
                }, null);
        }

        private void PelletRealizeTest_Click(object sender, RoutedEventArgs e)
        {
            PelletServer.Connect(TestPelletServer, (svr, _) =>
                {
                    RealizeService svc = svr.KnowledgeBases.First(kb => kb.SupportsService<RealizeService>()).GetService<RealizeService>();
                    svc.Realize(this.GraphCallback, null);
                }, null);
        }

        private void PelletSearchTest_Click(object sender, RoutedEventArgs e)
        {
            PelletServer.Connect(TestPelletServer, (svr, _) =>
                {
                    SearchService svc = svr.KnowledgeBases.First(kb => kb.SupportsService<SearchService>()).GetService<SearchService>();
                    svc.Search("cabernet", this.PelletSearchCallback, null);
                }, null);
        }

        private void PelletSimilarityTest_Click(object sender, RoutedEventArgs e)
        {
            PelletServer.Connect(TestPelletServer, (svr, _) =>
                {
                    SimilarityService svc = svr.KnowledgeBases.First(kb => kb.SupportsService<SimilarityService>()).GetService<SimilarityService>();
                    svc.SimilarityRaw(5, "wine:Red", this.GraphCallback, null);
                }, null);
        }
    }
}