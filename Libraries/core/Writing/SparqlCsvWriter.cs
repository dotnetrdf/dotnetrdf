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
using System.IO;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Writing
{
    /// <summary>
    /// Class for saving SPARQL Result Sets to CSV format (not a standardised format)
    /// </summary>
    public class SparqlCsvWriter
        : ISparqlResultsWriter
    {
        private CsvFormatter _formatter = new CsvFormatter();

        /// <summary>
        /// Saves a SPARQL Result Set to CSV format
        /// </summary>
        /// <param name="results">Result Set</param>
        /// <param name="filename">File to save to</param>
        public void Save(SparqlResultSet results, string filename)
        {
            this.Save(results, new StreamWriter(filename, false, new UTF8Encoding(Options.UseBomForUtf8)));
        }

        /// <summary>
        /// Saves a SPARQL Result Set to CSV format
        /// </summary>
        /// <param name="results">Result Set</param>
        /// <param name="output">Writer to save to</param>
        public void Save(SparqlResultSet results, TextWriter output)
        {
            try
            {
                if (results.ResultsType == SparqlResultsType.VariableBindings)
                {
                    //Output Variables first
                    String[] vars = results.Variables.ToArray();
                    for (int i = 0; i < vars.Length; i++)
                    {
                        output.Write(vars[i]);
                        if (i < vars.Length - 1) output.Write(',');
                    }
                    output.Write("\r\n");

                    foreach (SparqlResult result in results)
                    {
                        for (int i = 0; i < vars.Length; i++)
                        {
                            if (result.HasValue(vars[i]))
                            {
                                INode temp = result[vars[i]];
                                if (temp != null)
                                {
                                    switch (temp.NodeType)
                                    {
                                        case NodeType.Blank:
                                        case NodeType.Uri:
                                        case NodeType.Literal:
                                            output.Write(this._formatter.Format(temp));
                                            break;
                                        case NodeType.GraphLiteral:
                                            throw new RdfOutputException(WriterErrorMessages.GraphLiteralsUnserializable("SPARQL CSV"));
                                        default:
                                            throw new RdfOutputException(WriterErrorMessages.UnknownNodeTypeUnserializable("SPARQL CSV"));
                                    }
                                }
                            }
                            if (i < vars.Length - 1) output.Write(',');
                        }
                        output.Write("\r\n");
                    }
                }
                else
                {
                    output.Write(results.Result.ToString());
                }

                output.Close();
            }
            catch
            {
                try
                {
                    output.Close();
                }
                catch
                {
                    //No error handling, just trying to clean up
                }
                throw;
            }
        }

        /// <summary>
        /// Helper Method which raises the Warning event when a non-fatal issue with the SPARQL Results being written is detected
        /// </summary>
        /// <param name="message">Warning Message</param>
        private void RaiseWarning(String message)
        {
            SparqlWarning d = this.Warning;
            if (d != null)
            {
                d(message);
            }
        }

        /// <summary>
        /// Event raised when a non-fatal issue with the SPARQL Results being written is detected
        /// </summary>
        public event SparqlWarning Warning;

        /// <summary>
        /// Gets the String representation of the writer which is a description of the syntax it produces
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "SPARQL Results CSV";
        }
    }
}
