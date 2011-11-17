/*

Copyright Robert Vesse 2009-11
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
using VDS.RDF.Query;

namespace VDS.RDF.Writing.Formatting
{
    /// <summary>
    /// A Result Format that formats using the official SPARQL XML Results format
    /// </summary>
    public class SparqlXmlFormatter 
        : IResultSetFormatter
    {
        private String GetBaseHeader()
        {
            return @"<?xml version=""1.0"" encoding=""utf-8""?>
<sparql xmlns=""" + SparqlSpecsHelper.SparqlNamespace + "\">";
        }

        /// <summary>
        /// Formats the Header for a SPARQL Result Set
        /// </summary>
        /// <param name="variables">Variables</param>
        /// <returns></returns>
        public string FormatResultSetHeader(IEnumerable<string> variables)
        {
            StringBuilder output = new StringBuilder();
            output.AppendLine(this.GetBaseHeader());
            output.AppendLine("<head>");
            foreach (String var in variables)
            {
                output.AppendLine(" <variable name=\"" + var + "\" />"); 
            }
            output.AppendLine("</head>");
            output.Append("<results>");
            return output.ToString();
        }

        /// <summary>
        /// Formats the Header for a SPARQL Result Set
        /// </summary>
        /// <returns></returns>
        public string FormatResultSetHeader()
        {
            StringBuilder output = new StringBuilder();
            output.AppendLine(this.GetBaseHeader());
            output.AppendLine("<head />");
            output.Append("<results>");
            return output.ToString();
        }

        /// <summary>
        /// Formats the Footer for a SPARQL Result Set
        /// </summary>
        /// <returns></returns>
        public string FormatResultSetFooter()
        {
            return "</results>\n</sparql>";
        }

        /// <summary>
        /// Formats a SPARQL Result
        /// </summary>
        /// <param name="result">SPARQL Result</param>
        /// <returns></returns>
        public string Format(SparqlResult result)
        {
            StringBuilder output = new StringBuilder();
            output.AppendLine(" <result>");
            foreach (String var in result.Variables)
            {
                if (result.HasValue(var))
                {
                    INode value = result[var];
                    if (value != null)
                    {
                        output.Append("  <binding name=\"" + var + "\">");
                        switch (value.NodeType)
                        {
                            case NodeType.Blank:
                                output.Append("<bnode>" + ((IBlankNode)value).InternalID + "</bnode>");
                                break;
                            case NodeType.GraphLiteral:
                                throw new RdfOutputException(WriterErrorMessages.GraphLiteralsUnserializable("SPARQL XML Results"));
                            case NodeType.Literal:
                                ILiteralNode lit = (ILiteralNode)value;
                                output.Append("<literal");
                                if (lit.DataType != null)
                                {
                                    output.Append(" datatype=\"" + WriterHelper.EncodeForXml(lit.DataType.ToString()) + "\">" + WriterHelper.EncodeForXml(lit.Value) + "</literal>");
                                }
                                else if (!lit.Language.Equals(String.Empty))
                                {
                                    output.Append(" xml:lang=\"" + lit.Language + "\">" + lit.Value + "</literal>");
                                }
                                else
                                {
                                    output.Append(">" + WriterHelper.EncodeForXml(lit.Value) + "</literal>");
                                }
                                break;
                            case NodeType.Uri:
                                output.Append("<uri>" + WriterHelper.EncodeForXml(value.ToString()) + "</uri>");
                                break;
                            case NodeType.Variable:
                                throw new RdfOutputException(WriterErrorMessages.VariableNodesUnserializable("SPARQL XML Results"));
                            default:
                                throw new RdfOutputException(WriterErrorMessages.UnknownNodeTypeUnserializable("SPARQL XML Results"));
                        }
                        output.AppendLine("</binding>");
                    }
                }
            }
            output.Append(" </result>");
            return output.ToString();
        }

        /// <summary>
        /// Formats a Boolean Result
        /// </summary>
        /// <param name="result">Boolean Result</param>
        /// <returns></returns>
        public string FormatBooleanResult(bool result)
        {
            return " <boolean>" + result.ToString().ToLower() + "</boolean>";
        }

        /// <summary>
        /// Gets the string representation of the formatter
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "SPARQL XML Results";
        }
    }
}
