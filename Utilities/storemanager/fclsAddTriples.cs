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
using System.Windows.Forms;
using VDS.RDF;
using VDS.RDF.Parsing;

namespace dotNetRDFStore
{
    public partial class fclsAddTriples : Form
    {
        public fclsAddTriples()
        {
            InitializeComponent();

            this.cboFormat.SelectedIndex = 0;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void btnAddTriples_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        public String RDF
        {
            get
            {
                return this.txtRDF.Text;
            }
        }

        public IRdfReader Parser
        {
            get
            {
                switch (this.cboFormat.SelectedIndex)
                {
                    case 0:
                        return null;
                    case 1:
                        return new NTriplesParser();
                    case 2:
                        return new TurtleParser();
                    case 3:
                        return new Notation3Parser();
                    case 4:
                        return new RdfXmlParser();
                    case 5:
                        return new RdfJsonParser();
                    default:
                        return null;
                }
            }
        }
    }
}
