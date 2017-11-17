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
using System.Collections.Generic;
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
            output.AppendLine(GetBaseHeader());
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
            output.AppendLine(GetBaseHeader());
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
                                    output.Append(" datatype=\"" + WriterHelper.EncodeForXml(lit.DataType.AbsoluteUri) + "\">" + WriterHelper.EncodeForXml(lit.Value) + "</literal>");
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
