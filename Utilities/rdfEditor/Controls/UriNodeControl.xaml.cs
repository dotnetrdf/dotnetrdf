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

namespace VDS.RDF.Utilities.Editor.Controls
{
    /// <summary>
    /// Interaction logic for UriNodeControl.xaml
    /// </summary>
    public partial class UriNodeControl : UserControl
    {
        private Uri _u;

        public UriNodeControl(UriNode u, INodeFormatter formatter)
        {
            InitializeComponent();

            this.lnkUri.Text = formatter.Format(u);
            this._u = u.Uri;
        }

        public UriNodeControl(UriNode u)
            : this(u, new NTriplesFormatter()) { }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
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
