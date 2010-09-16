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
using System.Windows.Navigation;
using System.Windows.Shapes;
using VDS.RDF;
using VDS.RDF.Writing.Formatting;

namespace rdfEditor.Controls
{
    /// <summary>
    /// Interaction logic for TripleControl.xaml
    /// </summary>
    public partial class TripleControl : UserControl
    {
        private INodeFormatter _formatter;
        private Triple _t;

        public TripleControl(Triple t, INodeFormatter formatter)
        {
            InitializeComponent();

            this._t = t;
            this._formatter = formatter;

            this.RenderTriple();
        }

        public TripleControl(Triple t)
            : this(t, new NTriplesFormatter()) { }

        private void RenderTriple()
        {
            this.Subject.Content = this.RenderNode(this._t.Subject);
            this.Predicate.Content = this.RenderNode(this._t.Predicate);
            this.Object.Content = this.RenderNode(this._t.Object);
        }

        private Control RenderNode(INode n)
        {
            switch (n.NodeType)
            {
                case NodeType.Blank:
                    Label bnode = new Label();
                    bnode.Content = this._formatter.Format(n);
                    return bnode;

                case NodeType.GraphLiteral:
                    Label glit = new Label();
                    glit.Content = "{Graph Literals cannot be shown in this Viewer}";
                    return glit;

                case NodeType.Literal:
                    return new LiteralNodeControl((LiteralNode)n, this._formatter);

                case NodeType.Uri:
                    return new UriNodeControl((UriNode)n, this._formatter);

                default:
                    Label unknown = new Label();
                    unknown.Content = "{Unknown Node Types cannot be shown in this Viewer}";
                    return unknown;
            }
        }

        public INodeFormatter Formatter
        {
            get
            {
                return this._formatter;
            }
            set
            {
                this._formatter = value;
                this.RenderTriple();
            }
        }
    }
}
