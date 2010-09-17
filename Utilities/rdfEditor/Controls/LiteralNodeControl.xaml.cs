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

namespace rdfEditor.Controls
{
    /// <summary>
    /// Interaction logic for LiteralNodeControl.xaml
    /// </summary>
    public partial class LiteralNodeControl : UserControl
    {
        private Uri _u;

        public LiteralNodeControl(LiteralNode n, INodeFormatter formatter)
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
