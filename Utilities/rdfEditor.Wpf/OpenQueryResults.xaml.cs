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
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ICSharpCode.AvalonEdit;
using VDS.RDF.Parsing;
using VDS.RDF.Query;

namespace VDS.RDF.Utilities.Editor.Wpf
{
    /// <summary>
    /// Interaction logic for OpenQueryResults.xaml
    /// </summary>
    public partial class OpenQueryResults : Window
    {
        private Editor<TextEditor, FontFamily, Color> _editor;
        private String _data;
        private ISparqlResultsReader _parser;

        public OpenQueryResults(VisualOptions<FontFamily, Color> options)
        {
            InitializeComponent();

            this._editor = new Editor<TextEditor, FontFamily, Color>(new WpfEditorFactory());
            this._editor.DocumentManager.VisualOptions = options;
            Document<TextEditor> doc = this._editor.DocumentManager.New(true);
            doc.Syntax = "SparqlQuery11";
            Grid.SetRow(doc.TextEditor.Control, 2);
            Grid.SetColumn(doc.TextEditor.Control, 1);
            this.gridContent.Children.Add(doc.TextEditor.Control);

            doc.TextEditor.Control.TabIndex = 3;
            this.btnOpenQueryResults.TabIndex = 4;
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
                using (HttpWebResponse response = endpoint.QueryRaw(this._editor.DocumentManager.ActiveDocument.Text))
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

        public String Query
        {
            get
            {
                return this._editor.DocumentManager.ActiveDocument.Text;
            }
            set
            {
                this._editor.DocumentManager.ActiveDocument.Text = value;
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.txtEndpoint.Focus();
            this.txtEndpoint.SelectAll();
        }
    }
}
