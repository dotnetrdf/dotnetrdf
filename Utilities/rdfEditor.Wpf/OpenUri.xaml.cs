/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

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
