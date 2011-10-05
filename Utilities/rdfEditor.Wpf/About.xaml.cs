using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

namespace VDS.RDF.Utilities.Editor.Wpf
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : Window
    {
        public About()
        {
            InitializeComponent();

            this.lblEditorVersion.Content = Assembly.GetExecutingAssembly().GetName().Version;
            this.lblEditorCoreVersion.Content = Assembly.GetAssembly(typeof(GlobalOptions)).GetName().Version;
            this.lblEditorCoreWpfVersion.Content = Assembly.GetAssembly(typeof(WpfEditorAdaptor)).GetName().Version;
            this.lblRdfVersion.Content = Assembly.GetAssembly(typeof(IGraph)).GetName().Version;
        }
    }
}
