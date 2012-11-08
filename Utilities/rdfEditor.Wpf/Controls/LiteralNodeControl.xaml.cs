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
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VDS.RDF;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Utilities.Editor.Wpf.Controls
{
    /// <summary>
    /// Interaction logic for LiteralNodeControl.xaml
    /// </summary>
    public partial class LiteralNodeControl : UserControl
    {
        private Uri _u;

        public LiteralNodeControl(ILiteralNode n, INodeFormatter formatter)
        {
            InitializeComponent();

            String data = formatter.Format(n);
            if (data.Contains("\"^^"))
            {
                String value = data.Substring(0, data.IndexOf("\"^^") + 3);
                String dt = data.Substring(data.IndexOf("\"^^") + 4);
                this.txtValue.Content = value;
                this.lnkDatatypeUri.Text = dt;
                this._u = n.DataType;
            }
            else
            {
                this.txtValue.Content = data;
            }
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            if (this._u != null)
            {
                try
                {
                    Process.Start(this._u.AbsoluteUri);
                }
                catch
                {
                    //Supress errors
                }
            }
        }
    }
}
