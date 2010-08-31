using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using rdfEditor.Syntax;
using rdfEditor.AutoComplete;

namespace rdfEditor
{
    /// <summary>
    /// Interaction logic for OpenQueryResults.xaml
    /// </summary>
    public partial class OpenQueryResults : Window
    {
        private EditorManager _manager;
        private String _data;
        private ISparqlResultsReader _parser;

        public OpenQueryResults()
        {
            InitializeComponent();

            this._manager = new EditorManager(queryEditor);
            this._manager.SetHighlighter(SyntaxManager.GetHighlighter("SparqlQuery11"));
            this._manager.SetAutoCompleter(new SparqlAutoCompleter(SparqlQuerySyntax.Sparql_1_1));
        }

        private void btnOpenQueryResults_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Uri u = new Uri(this.txtEndpoint.Text);
                String defGraph = this.txtDefaultGraph.Text;
                SparqlRemoteEndpoint endpoint;
                if (defGraph.Equals(String.Empty))
                {
                    endpoint = new SparqlRemoteEndpoint(u);
                }
                else
                {
                    endpoint = new SparqlRemoteEndpoint(u, defGraph);
                }

                String data;
                using (HttpWebResponse response = endpoint.QueryRaw(queryEditor.Text))
                {
                    data = new StreamReader(response.GetResponseStream()).ReadToEnd();
                    try
                    {
                        this._parser = MimeTypesHelper.GetSparqlParser(response.ContentType);
                    }
                    catch (RdfParserSelectionException)
                    {
                        //Ignore here we'll try other means of getting a parser after this
                    }
                    response.Close();
                }

                this._data = data;
                if (this._parser == null)
                {
                    try
                    {
                        this._parser = StringParser.GetResultSetParser(this._data);
                    }
                    catch (RdfParserSelectionException)
                    {
                        this._parser = null;
                    }
                }

                this.DialogResult = true;
                this.Close();
            }
            catch (UriFormatException)
            {
                MessageBox.Show("You have failed to enter a valid Endpoint URI", "Invalid URI");
            }
            catch (WebException webEx)
            {
                MessageBox.Show("A HTTP error occurred making the Query: " + webEx.Message, "Open Query Results Failed");
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while making the Query: " + ex.Message, "Open Query Results Failed");
            }
        }

        public String RetrievedData
        {
            get
            {
                return this._data;
            }
        }

        public ISparqlResultsReader Parser
        {
            get
            {
                return this._parser;
            }
        }
    }
}
