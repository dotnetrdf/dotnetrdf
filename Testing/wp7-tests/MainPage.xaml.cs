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
using Microsoft.Phone.Controls;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Writing.Formatting;

namespace wp7_tests
{
    public partial class MainPage : PhoneApplicationPage
    {
        private SparqlRemoteEndpoint _endpoint;

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            //Set up our test endpoint
            this._endpoint = new SparqlRemoteEndpoint(new Uri("http://dbpedia.org/sparql"), "http://dbpedia.org");
        }

        #region Callbacks

        private void GraphCallback(IGraph g, Object state)
        {
            Dispatcher.BeginInvoke(() =>
                {
                    TurtleFormatter formatter = new TurtleFormatter(g.NamespaceMap);
                    this.ResultsText.Text = g.Triples.Count + " Triple(s) returned\n";
                    foreach (Triple t in g.Triples)
                    {
                        this.ResultsText.Text += t.ToString(formatter);
                    }
                });
        }

        private void SparqlResultsCallback(SparqlResultSet results, Object state)
        {
            Dispatcher.BeginInvoke(() =>
                {
                    SparqlFormatter formatter = new SparqlFormatter();
                    this.ResultsText.Text = results.Count + " Result(s) returned\n";

                    switch (results.ResultsType)
                    {
                        case SparqlResultsType.Boolean:
                            this.ResultsText.Text += formatter.FormatBooleanResult(results.Result);
                            break;
                        case SparqlResultsType.VariableBindings:
                            foreach (SparqlResult r in results)
                            {
                                this.ResultsText.Text += r.ToString(formatter) + "\n";
                            }
                            break;
                        default:
                            this.ResultsText.Text += "Unknown Results Type";
                            break;
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

        #endregion
    }
}