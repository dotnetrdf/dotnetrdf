using System;
using System.Collections.Generic;
using System.Linq;
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
using VDS.RDF.Writing.Formatting;
using rdfEditor.Controls;

namespace rdfEditor
{
    /// <summary>
    /// Interaction logic for TriplesWindow.xaml
    /// </summary>
    public partial class TriplesWindow : Window
    {
        public TriplesWindow(IGraph g, INodeFormatter formatter)
        {
            InitializeComponent();

            foreach (Triple t in g.Triples)
            {
                TripleControl control = new TripleControl(t, formatter);
                this.stkTriples.Children.Add(control);
            }
        }

        public TriplesWindow(IGraph g)
            : this(g, new NTriplesFormatter()) { }
    }
}
