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

namespace rdfEditor
{
    /// <summary>
    /// Interaction logic for OpenUri.xaml
    /// </summary>
    public partial class OpenUri : Window
    {
        private Uri _u;
        private String _data;
        private IRdfReader _parser;

        public OpenUri()
        {
            InitializeComponent();
        }

        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Uri u = new Uri(this.txtUri.Text);

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(u.ToString());
                request.Accept = MimeTypesHelper.HttpAcceptHeader + ",*.*";
                String data;
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    data = new StreamReader(response.GetResponseStream()).ReadToEnd();
                    try
                    {
                        this._parser = MimeTypesHelper.GetParser(response.ContentType);
                        if (this._parser is NTriplesParser)
                        {
                            if (!MimeTypesHelper.NTriples.Contains(response.ContentType))
                            {
                                this._parser = null;
                            }
                        }
                    }
                    catch (RdfParserSelectionException)
                    {
                        //Ignore here as we'll try and set the parser in another way next
                    }
                    response.Close();
                }

                this._data = data;
                if (this._parser == null)
                {
                    this._parser = StringParser.GetParser(data);
                    if (this._parser is NTriplesParser) this._parser = null;
                }
                this._u = u;

                this.DialogResult = true;
                this.Close();
            }
            catch (UriFormatException)
            {
                MessageBox.Show("You have failed to enter a valid URI", "Invalid URI");
            }
            catch (WebException webEx)
            {
                MessageBox.Show("A HTTP error occurred opening the URI: " + webEx.Message, "Open URI Failed");
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while opening the URI: " + ex.Message, "Open URI Failed");
            }
        }

        public Uri OpenedUri
        {
            get
            {
                return this._u;
            }
        }

        public String RetrievedData
        {
            get
            {
                return this._data;
            }
        }

        public IRdfReader Parser
        {
            get
            {
                return this._parser;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.txtUri.Focus();
            this.txtUri.SelectAll();
        }


    }
}
