using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Writing.Formatting;

namespace slv_tests
{
    public partial class MainPage : UserControl
    {
        private SparqlRemoteEndpoint _endpoint;

        public MainPage()
        {
            InitializeComponent();

            //Set up our test endpoint
            //this._endpoint = new SparqlRemoteEndpoint(new Uri("http://dbpedia.org/sparql"), "http://dbpedia.org");
            this._endpoint = new SparqlRemoteEndpoint(new Uri("http://localhost/demos/leviathan/"));
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

        #endregion

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
    }
}
