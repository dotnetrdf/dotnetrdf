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
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;
using VDS.RDF.Query;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Utilities.Sparql
{
    public partial class fclsExplanation : Form
    {
        private SparqlFormatter _formatter = new SparqlFormatter();

        public fclsExplanation(SparqlQuery query, long parseTime)
        {
            InitializeComponent();

            this.lblParseTime.Text = "Took " + parseTime + "ms to parse";
            this.txtQuery.Text = this._formatter.Format(query);

            //First need to create the Explanation
            using (StreamWriter writer = new StreamWriter("explain.temp", false, Encoding.UTF8))
            {
                ExplainQueryProcessor processor = new ExplainQueryProcessor(new TripleStore());
                processor.ExplanationLevel = (ExplanationLevel.OutputToTrace | ExplanationLevel.ShowAll | ExplanationLevel.AnalyseAll | ExplanationLevel.Simulate) ^ ExplanationLevel.ShowThreadID ^ ExplanationLevel.ShowTimings ^ ExplanationLevel.ShowIntermediateResultCount;

                TextWriterTraceListener listener = new TextWriterTraceListener(writer, "SparqlGUI");
                Trace.Listeners.Add(listener);
                try
                {
                    Object results = processor.ProcessQuery(query);
                }
                catch
                {
                    throw;
                }
                finally
                {
                    Trace.Listeners.Remove(listener);
                }

                writer.Close();
            }

            //Then need to display it
            using (StreamReader reader = new StreamReader("explain.temp"))
            {
                this.txtExplanation.Text = reader.ReadToEnd();
                reader.Close();
            }
        }
    }
}
