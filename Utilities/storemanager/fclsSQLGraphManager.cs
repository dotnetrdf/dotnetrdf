/*

Copyright Robert Vesse 2009-10
rvesse@vdesign-studios.com

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

If this license is not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Storage;

namespace dotNetRDFStore
{
    public partial class fclsSQLGraphManager : Form
    {
        private ISqlIOManager _manager;
        private Uri _graphUri;
        private String _graphID;
        private Graph _g;

        public fclsSQLGraphManager(Uri graphUri, ISqlIOManager manager)
        {
            InitializeComponent();

            this._manager = manager;
            this._graphUri = graphUri;

            if (this._manager.Exists(this._graphUri))
            {
                this._graphID = this._manager.GetGraphID(this._graphUri);
            }
            else
            {
                throw new RdfStorageException("The specified Graph does not exist in this dotNetRDF Store");
            }
        }


        private void fclsGraphManager_Load(object sender, System.EventArgs e)
        {
            //Set Window Title
            this.Text = "Graph - " + this._graphUri.ToString();

            //Load Graph and Populate Form Fields
            this.LoadInternal();
        }

        private void LoadInternal()
        {
            //Load Graph
            SqlReader reader = new SqlReader(this._manager);
            Stopwatch timer = new Stopwatch();
            timer.Start();
            this._g = reader.Load(this._graphUri);
            timer.Stop();

            //Show Load Statistics
            this.lblLoadInfo.Text = this._g.Triples.Count + " Triples Loaded in ";
            if (timer.ElapsedMilliseconds > 1000)
            {
                this.lblLoadInfo.Text += (((double)timer.ElapsedMilliseconds) / 1000d).ToString("F2") + " Seconds";
            }
            else
            {
                this.lblLoadInfo.Text += timer.ElapsedMilliseconds + " Milliseconds";
            }
            this.lblLoadInfo.Text += " at a Speed of ";
            double speed = ((double)this._g.Triples.Count) / (((double)timer.ElapsedMilliseconds) / 1000d);
            this.lblLoadInfo.Text += speed.ToString("N0") + " Triples/Second";

            //Show Graph Uri
            this.lnkGraphURI.Text = this._graphUri.ToString();

            //Show Triples
            this.lvwTriples.BeginUpdate();
            this.lvwTriples.Items.Clear();
            ListViewItem item;
            String[] triple;
            foreach (Triple t in this._g.Triples)
            {
                triple = new String[] { t.Subject.ToString(), t.Predicate.ToString(), t.Object.ToString() };
                item = new ListViewItem(triple);
                this.lvwTriples.Items.Add(item);
            }
            this.lvwTriples.EndUpdate();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            this.LoadInternal();
        }

        private void btnAddTriples_Click(object sender, EventArgs e)
        {
            fclsAddTriples addTriples = new fclsAddTriples();
            if (addTriples.ShowDialog() == DialogResult.OK)
            {
                //Parse into an Empty Graph which has the same Base Uri and Namespace as the Target Graph
                Graph h = new Graph();
                h.BaseUri = this._g.BaseUri;
                h.NamespaceMap.Import(this._g.NamespaceMap);

                try
                {
                    IRdfReader parser = addTriples.Parser;
                    if (parser == null)
                    {
                        StringParser.Parse(h, addTriples.RDF);
                    }
                    else
                    {
                        StringParser.Parse(h, addTriples.RDF, parser);
                    }

                    //Save each Triple to the Graph using the Manager
                    foreach (Triple t in h.Triples)
                    {
                        this._manager.SaveTriple(t, this._graphID);
                    }

                    //Wait for the Manager to finish writing
                    while (!this._manager.HasCompleted)
                    {
                        Thread.Sleep(100);
                        Application.DoEvents();
                    }

                    MessageBox.Show("Succesfully added your Triples to the Graph!", "Add Triples Completed", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    //Reload Graph
                    this.LoadInternal();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An Error occurred while attempting to Add Triples to the Graph:\n" + ex.Message, "Add Triples Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            fclsExportGraph exporter = new fclsExportGraph();
            if (exporter.ShowDialog() == DialogResult.OK)
            {
                IRdfWriter writer = exporter.Writer;
                String file = exporter.File;

                try
                {
                    writer.Save(this._g, file);

                    MessageBox.Show("Successfully exported the Graph to the file '" + file + "'", "Graph Export Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred attempting to export the Graph:\n" + ex.Message, "Graph Export Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
