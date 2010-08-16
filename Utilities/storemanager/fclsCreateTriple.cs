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
using System.ComponentModel;
using System.Windows.Forms;
using VDS.RDF;

namespace dotNetRDFStore
{
    public partial class fclsCreateTriple : Form
    {
        private IGraph _g;

        public fclsCreateTriple(IGraph g)
        {
            InitializeComponent();

            this._g = g;
        }

        private void mnuTriples_Opening(object sender, CancelEventArgs e)
        {
            if (this.lvwTriples.Items.Count == 0)
            {
                e.Cancel = true;
            }
            else if (this.lvwTriples.SelectedItems.Count == 0)
            {
                e.Cancel = true;
            }
        }

        private void NewTriple()
        {
            String[] triple = new String[] { String.Empty, String.Empty, String.Empty };
            ListViewItem item = new ListViewItem(triple);
            this.lvwTriples.Items.Add(item);

            foreach (ListViewItem i in this.lvwTriples.SelectedItems)
            {
                i.Selected = false;
            }
            item.Selected = true;
        }

        private void btnNewTriple_Click(object sender, EventArgs e)
        {
            this.NewTriple();
        }

        private INode NewNode()
        {
            INode n;

            if (this.radBaseURI.Checked)
            {
                n = this._g.CreateUriNode();
            }
            else if (this.radURI.Checked)
            {
                n = this._g.CreateUriNode(new Uri(this.txtURI.Text));
            }
            else if (this.radQName.Checked)
            {
                n = this._g.CreateUriNode(this.txtQName.Text);
            }
            else if (this.radPlainLiteral.Checked)
            {
                n = this._g.CreateLiteralNode(this.txtPlainLiteral.Text);
            }
            else
            {
                throw new NotImplementedException("Not yet implemented");
            }
            return n;
        }

        private void btnSubject_Click(object sender, EventArgs e)
        {

        }
    }
}
