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
using VDS.RDF.Utilities.Editor.Wpf.Controls;

namespace VDS.RDF.Utilities.Editor.Wpf
{
    /// <summary>
    /// Interaction logic for TriplesWindow.xaml
    /// </summary>
    public partial class TriplesWindow : Window
    {
        private INodeFormatter _formatter;
        private IGraph _g;
        private Grid _grid;

        public TriplesWindow(IGraph g, INodeFormatter formatter)
        {
            InitializeComponent();
            this._formatter = formatter;
            this._g = g;
            this._grid = this.gridTriples;

            this.RenderTriples();
        }

        public TriplesWindow(IGraph g)
            : this(g, new NTriplesFormatter()) { }


        private void RenderTriples()
        {
            if (this._grid == null) return;
            if (this._grid.RowDefinitions.Count > 1)
            {
                this._grid.RowDefinitions.RemoveRange(1, this._grid.RowDefinitions.Count - 1);
                this._grid.Children.RemoveRange(5, this._grid.Children.Count - 5);
            }

            int row = 1;
            foreach (Triple t in this._g.Triples)
            {
                RowDefinition def = new RowDefinition();
                def.Height = GridLength.Auto;
                this._grid.RowDefinitions.Add(def);

                Control s = this.RenderNode(t.Subject);
                this._grid.Children.Add(s);
                Grid.SetColumn(s, 0);
                Grid.SetRow(s, row);

                Control p = this.RenderNode(t.Predicate);
                this._grid.Children.Add(p);
                Grid.SetColumn(p, 2);
                Grid.SetRow(p, row);

                Control o = this.RenderNode(t.Object);
                this._grid.Children.Add(o);
                Grid.SetColumn(o, 4);
                Grid.SetRow(o, row);
                row++;

                this._grid.RowDefinitions.Add(new RowDefinition());
                GridSplitter rowSplitter = new GridSplitter();
                rowSplitter.HorizontalAlignment = HorizontalAlignment.Stretch;
                rowSplitter.Height = 1;
                rowSplitter.ResizeDirection = GridResizeDirection.Rows;
                rowSplitter.ResizeBehavior = GridResizeBehavior.PreviousAndNext;
                rowSplitter.Background = Brushes.Black;
                rowSplitter.Foreground = Brushes.Black;
                Grid.SetColumn(rowSplitter, 0);
                Grid.SetRow(rowSplitter, row);
                Grid.SetColumnSpan(rowSplitter, 5);
                this._grid.Children.Add(rowSplitter);
                row++;
            }

            Grid.SetRowSpan(this.split1, row + 1);
            Grid.SetRowSpan(this.split2, row + 1);

            this._grid.InvalidateVisual();
            ((ScrollViewer)this.FindName("scroll")).InvalidateScrollInfo();
        }

        private Control RenderNode(INode n)
        {
            switch (n.NodeType)
            {
                case NodeType.Blank:
                    Label bnode = new Label();
                    bnode.Content = this._formatter.Format(n);
                    bnode.Padding = new Thickness(2);
                    return bnode;

                case NodeType.GraphLiteral:
                    Label glit = new Label();
                    glit.Content = "{Graph Literals cannot be shown in this Viewer}";
                    glit.Padding = new Thickness(2);
                    return glit;

                case NodeType.Literal:
                    return new LiteralNodeControl((ILiteralNode)n, this._formatter);

                case NodeType.Uri:
                    return new UriNodeControl((IUriNode)n, this._formatter);

                default:
                    Label unknown = new Label();
                    unknown.Content = "{Unknown Node Types cannot be shown in this Viewer}";
                    unknown.Padding = new Thickness(2);
                    return unknown;
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.cboFormatter.SelectedIndex > -1)
            {
                switch (this.cboFormatter.SelectedIndex)
                {
                    case 0:
                        this._formatter = new NTriplesFormatter();
                        break;
                    case 1:
                        this._formatter = new TurtleFormatter(this._g);
                        break;
                    case 2:
                        this._formatter = new UncompressedTurtleFormatter();
                        break;
                    case 3:
                        this._formatter = new Notation3Formatter(this._g);
                        break;
                    case 4:
                        this._formatter = new UncompressedNotation3Formatter();
                        break;
                    case 5:
                        this._formatter = new CsvFormatter();
                        break;
                    case 6:
                        this._formatter = new TsvFormatter();
                        break;
                }

                this.RenderTriples();
            }
        }
    }
}
