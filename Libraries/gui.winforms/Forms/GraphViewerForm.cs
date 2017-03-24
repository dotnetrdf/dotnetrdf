/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/

using System;
using System.Windows.Forms;

namespace VDS.RDF.GUI.WinForms.Forms
{
    /// <summary>
    /// A Form that displays a Graph using a DataGridView
    /// </summary>
    public partial class GraphViewerForm : Form
    {
        /// <summary>
        /// Displays the given Graph
        /// </summary>
        /// <param name="g">Graph to display</param>
        public GraphViewerForm(IGraph g)
        {
            InitializeComponent();
            if (Constants.WindowIcon != null)
            {
                this.Icon = Constants.WindowIcon;
            }
            this.Text = String.Format("Graph Viewer - {0} Triple(s)", g.Triples.Count);

            this.graphViewer.UriClicked += (sender, uri) => this.RaiseUriClicked(uri);
            this.Load += (sender, args) => this.graphViewer.DisplayGraph(g);
        }

        /// <summary>
        /// Displays the given Graph and prefixes the Form Title with the given Title
        /// </summary>
        /// <param name="g">Graph to display</param>
        /// <param name="title">Title</param>
        public GraphViewerForm(IGraph g, String title)
            : this(g)
        {
            this.Text = String.Format("{0} - Graph Viewer - {1} Triple(s)", title, g.Triples.Count);
        }

        /// <summary>
        /// Event which is raised when a User clicks a URI
        /// </summary>
        public event UriClickedEventHandler UriClicked;

        private void RaiseUriClicked(Uri u)
        {
            UriClickedEventHandler d = this.UriClicked;
            if (d != null)
            {
                d(this, u);
            }
        }
    }
}