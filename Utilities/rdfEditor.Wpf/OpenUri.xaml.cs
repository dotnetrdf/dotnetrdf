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

namespace VDS.RDF.Utilities.Editor.Wpf
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

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(u.AbsoluteUri);
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
                            if (!response.ContentType.Equals("text/plain"))
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
