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
using System.Windows.Forms;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Utilities.Sparql
{
    public partial class fclsInspect : Form
    {
        private SparqlFormatter _formatter = new SparqlFormatter();

        public fclsInspect(SparqlQuery query, long parseTime, String origQuery)
        {
            InitializeComponent();

            this.lblParseTime.Text = "Took " + parseTime + "ms to parse";
            this.txtQuery.Text = this._formatter.Format(query);
            this.txtAlgebra.Text = query.ToAlgebra().ToString();

            //Compute the actual syntax compatability
            SparqlQueryParser parser = new SparqlQueryParser();
            parser.SyntaxMode = SparqlQuerySyntax.Sparql_1_0;
            try
            {
                SparqlQuery q = parser.ParseFromString(origQuery);
                this.lblSyntaxCompatability.Text += " SPARQL 1.0 (Standard)";
            }
            catch
            {
                try
                {
                    parser.SyntaxMode = SparqlQuerySyntax.Sparql_1_1;
                    SparqlQuery q = parser.ParseFromString(origQuery);
                    this.lblSyntaxCompatability.Text += " SPARQL 1.1 (Current Working Draft Standard)";
                }
                catch
                {
                    parser.SyntaxMode = SparqlQuerySyntax.Extended;
                    SparqlQuery q = parser.ParseFromString(origQuery);
                    this.lblSyntaxCompatability.Text += " SPARQL 1.1 (Implementation specific extensions)";
                }
            }
        }
    }
}
