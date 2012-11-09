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
using VDS.RDF.Query;
using VDS.RDF.Writing.Formatting;
using VDS.RDF.Utilities.Editor.Wpf.Controls;

namespace VDS.RDF.Utilities.Editor.Wpf
{
    /// <summary>
    /// Interaction logic for ResultSetWindow.xaml
    /// </summary>
    public partial class ResultSetWindow : Window
    {
        private INodeFormatter _formatter;
        private Grid _grid;

        public ResultSetWindow(SparqlResultSet results)
        {
            InitializeComponent();
            this._formatter = new SparqlFormatter();
            this._grid = this.gridResults;

            this.RenderResultSet(results);
        }


        private void RenderResultSet(SparqlResultSet results)
        {
            //First Create the Header Row
            RowDefinition rowDef = new RowDefinition();
            rowDef.Height = new GridLength(27);
            this._grid.RowDefinitions.Add(rowDef);

            //Create the appropriate number of Columns
            int c = 0;
            List<GridSplitter> columnSplitters = new List<GridSplitter>();
            foreach (String var in results.Variables)
            {
                //Create the Column for the Variable
                ColumnDefinition colDef = new ColumnDefinition();
                colDef.Width = new GridLength(100, GridUnitType.Star);
                this._grid.ColumnDefinitions.Add(colDef);

                //Create a Label for the Variable
                Label varLabel = new Label();
                varLabel.Content = var;
                varLabel.Background = Brushes.LightGray;
                varLabel.HorizontalContentAlignment = HorizontalAlignment.Center;
                varLabel.VerticalContentAlignment = VerticalAlignment.Center;
                this._grid.Children.Add(varLabel);
                Grid.SetColumn(varLabel, c);
                Grid.SetRow(varLabel, 0);

                c++;

                //Create a Column for a Splitter
                colDef = new ColumnDefinition();
                colDef.Width = new GridLength(1);
                this._grid.ColumnDefinitions.Add(colDef);

                //Add the Spliiter
                GridSplitter splitter = new GridSplitter();
                splitter.ResizeDirection = GridResizeDirection.Columns;
                splitter.ResizeBehavior = GridResizeBehavior.PreviousAndNext;
                splitter.Width = 1;
                splitter.Background = Brushes.Black;
                splitter.Foreground = Brushes.Black;
                this._grid.Children.Add(splitter);
                Grid.SetColumn(splitter, c);
                Grid.SetRow(splitter, 0);
                columnSplitters.Add(splitter);

                c++;
            }

            int row = 1;
            c = 0;

            foreach (SparqlResult result in results)
            {
                //Create a new Row
                rowDef = new RowDefinition();
                rowDef.Height = new GridLength(27, GridUnitType.Star);
                this._grid.RowDefinitions.Add(rowDef);

                //Create Controls for each Value in the appropriate Columns
                //The increment is two because we're skipping the column splitter columns
                foreach (String var in results.Variables)
                {
                    if (result.HasValue(var))
                    {
                        if (result[var] != null)
                        {
                            Control value = this.RenderNode(result[var]);
                            Grid.SetRow(value, row);
                            Grid.SetColumn(value, c);
                            this._grid.Children.Add(value);
                        }
                    }
                    c += 2;
                }


                //Add the Splitter Row
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
                Grid.SetColumnSpan(rowSplitter, this._grid.ColumnDefinitions.Count);
                this._grid.Children.Add(rowSplitter);

                //Increment Row and Reset Column
                row++;
                c = 0;
            }

            foreach (GridSplitter splitter in columnSplitters)
            {
                Grid.SetRowSpan(splitter, this._grid.RowDefinitions.Count);
            }
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
    }
}
