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
