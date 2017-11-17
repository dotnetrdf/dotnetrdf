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
using System.IO;
using System.Text;
using VDS.RDF.Query;
using VDS.RDF.Writing.Formatting;
using System.Web.UI;

namespace VDS.RDF.Writing
{
    /// <summary>
    /// Class for saving SPARQL Result Sets to a HTML Table format (this is not a standardised format)
    /// </summary>
    public class SparqlHtmlWriter 
        : BaseHtmlWriter, ISparqlResultsWriter, INamespaceWriter
    {
        private HtmlFormatter _formatter = new HtmlFormatter();
        private INamespaceMapper _namespaces = new NamespaceMapper();

        /// <summary>
        /// Gets/Sets the Default Namespaces used to pretty print URIs in the output
        /// </summary>
        public INamespaceMapper DefaultNamespaces
        {
            get
            {
                return _namespaces;
            }
            set
            {
                _namespaces = value;
            }
        }


        /// <summary>
        /// Saves the Result Set to the given File as a HTML Table
        /// </summary>
        /// <param name="results">Result Set to save</param>
        /// <param name="filename">File to save to</param>
        public void Save(SparqlResultSet results, String filename)
        {
            using (var stream = File.Open(filename, FileMode.Create))
            {
                Save(results, new StreamWriter(stream, new UTF8Encoding(Options.UseBomForUtf8)));
            }
        }

        /// <summary>
        /// Saves the Result Set to the given Stream as a HTML Table
        /// </summary>
        /// <param name="results">Result Set to save</param>
        /// <param name="output">Stream to save to</param>
        public void Save(SparqlResultSet results, TextWriter output)
        {
            try
            {
                GenerateOutput(results, output);
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
                    // No Catch Actions
                }
                throw;
            }
        }

        /// <summary>
        /// Internal method which generates the HTML Output for the Sparql Results
        /// </summary>
        /// <param name="results"></param>
        /// <param name="output"></param>
        private void GenerateOutput(SparqlResultSet results, TextWriter output)
        {
            HtmlTextWriter writer = new HtmlTextWriter(output);
            QNameOutputMapper qnameMapper = new QNameOutputMapper(_namespaces != null ? _namespaces : new NamespaceMapper(true));

            // Page Header
            writer.Write("<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">");
            writer.RenderBeginTag(HtmlTextWriterTag.Html);
            writer.RenderBeginTag(HtmlTextWriterTag.Head);
            writer.RenderBeginTag(HtmlTextWriterTag.Title);
            writer.WriteEncodedText("SPARQL Query Results");
            writer.RenderEndTag();
            if (!Stylesheet.Equals(String.Empty))
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Href, Stylesheet);
                writer.AddAttribute(HtmlTextWriterAttribute.Type, "text/css");
                writer.AddAttribute(HtmlTextWriterAttribute.Rel, "stylesheet");
                writer.RenderBeginTag(HtmlTextWriterTag.Link);
                writer.RenderEndTag();
            }
            // TODO: Add <meta> for charset?
            writer.RenderEndTag();

            // Start Body
            writer.RenderBeginTag(HtmlTextWriterTag.Body);

            if (results.ResultsType == SparqlResultsType.VariableBindings)
            {
                // Create a Table for the results
                writer.AddAttribute(HtmlTextWriterAttribute.Width, "100%");
                writer.RenderBeginTag(HtmlTextWriterTag.Table);

                // Create a Table Header with the Variable Names
                writer.RenderBeginTag(HtmlTextWriterTag.Thead);
                writer.RenderBeginTag(HtmlTextWriterTag.Tr);

                foreach (String var in results.Variables)
                {
                    writer.RenderBeginTag(HtmlTextWriterTag.Th);
                    writer.WriteEncodedText(var);
                    writer.RenderEndTag();
                }

                writer.RenderEndTag();
                writer.RenderEndTag();
                writer.WriteLine();

                // Create a Table Body for the Results
                writer.RenderBeginTag(HtmlTextWriterTag.Tbody);

                // Create a Column for each Binding
                foreach (SparqlResult result in results)
                {
                    // Start Row
                    writer.RenderBeginTag(HtmlTextWriterTag.Tr);

                    foreach (String var in results.Variables)
                    {
                        // Start Column
                        writer.RenderBeginTag(HtmlTextWriterTag.Td);

                        if (result.HasValue(var))
                        {
                            INode value = result[var];

                            if (value != null)
                            {
                                switch (value.NodeType)
                                {
                                    case NodeType.Blank:
                                        writer.AddAttribute(HtmlTextWriterAttribute.Class, CssClassBlankNode);
                                        writer.RenderBeginTag(HtmlTextWriterTag.Span);
                                        writer.WriteEncodedText(value.ToString());
                                        writer.RenderEndTag();
                                        break;

                                    case NodeType.Literal:
                                        ILiteralNode lit = (ILiteralNode)value;
                                        writer.AddAttribute(HtmlTextWriterAttribute.Class, CssClassLiteral);
                                        writer.RenderBeginTag(HtmlTextWriterTag.Span);
                                        if (lit.DataType != null)
                                        {
                                            writer.WriteEncodedText(lit.Value);
                                            writer.RenderEndTag();
                                            writer.WriteEncodedText("^^");
                                            writer.AddAttribute(HtmlTextWriterAttribute.Href, _formatter.FormatUri(lit.DataType.AbsoluteUri));
                                            writer.AddAttribute(HtmlTextWriterAttribute.Class, CssClassDatatype);
                                            writer.RenderBeginTag(HtmlTextWriterTag.A);
                                            writer.WriteEncodedText(lit.DataType.ToString());
                                            writer.RenderEndTag();
                                        }
                                        else
                                        {
                                            writer.WriteEncodedText(lit.Value);
                                            if (!lit.Language.Equals(String.Empty))
                                            {
                                                writer.RenderEndTag();
                                                writer.WriteEncodedText("@");
                                                writer.AddAttribute(HtmlTextWriterAttribute.Class, CssClassLangSpec);
                                                writer.RenderBeginTag(HtmlTextWriterTag.Span);
                                                writer.WriteEncodedText(lit.Language);
                                                writer.RenderEndTag();
                                            }
                                            else
                                            {
                                                writer.RenderEndTag();
                                            }
                                        }
                                        break;

                                    case NodeType.GraphLiteral:
                                        // Error
                                        throw new RdfOutputException("Result Sets which contain Graph Literal Nodes cannot be serialized in the HTML Format");

                                    case NodeType.Uri:
                                        writer.AddAttribute(HtmlTextWriterAttribute.Class, CssClassUri);
                                        writer.AddAttribute(HtmlTextWriterAttribute.Href, _formatter.FormatUri(UriPrefix + value.ToString()));
                                        writer.RenderBeginTag(HtmlTextWriterTag.A);

                                        String qname;
                                        if (qnameMapper.ReduceToQName(value.ToString(), out qname))
                                        {
                                            writer.WriteEncodedText(qname);
                                        }
                                        else
                                        {
                                            writer.WriteEncodedText(value.ToString());
                                        }
                                        writer.RenderEndTag();
                                        break;

                                    default:
                                        throw new RdfOutputException("Result Sets which contain Unknown Node Types cannot be serialized in the HTML Format");
                                }
                            }
                            else
                            {
                                writer.WriteEncodedText(" ");
                            }
                        }
                        else
                        {
                            writer.WriteEncodedText(" ");
                        }

                        // End Column
                        writer.RenderEndTag();
                    }

                    // End Row
                    writer.RenderEndTag();
                    writer.WriteLine();
                }

                // End Table Body
                writer.RenderEndTag();

                // End Table
                writer.RenderEndTag();
            }
            else
            {
                // Show a Header and a Boolean value
                writer.RenderBeginTag(HtmlTextWriterTag.H3);
                writer.WriteEncodedText("ASK Query Result");
                writer.RenderEndTag();
                writer.RenderBeginTag(HtmlTextWriterTag.P);
                writer.WriteEncodedText(results.Result.ToString());
                writer.RenderEndTag();
            }

            // End of Page
            writer.RenderEndTag(); //End Body
            writer.RenderEndTag(); //End Html
        }

        /// <summary>
        /// Helper Method which raises the Warning event when a non-fatal issue with the SPARQL Results being written is detected
        /// </summary>
        /// <param name="message">Warning Message</param>
        private void RaiseWarning(String message)
        {
            SparqlWarning d = Warning;
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
            return "SPARQL Results HTML";
        }
    }
}
