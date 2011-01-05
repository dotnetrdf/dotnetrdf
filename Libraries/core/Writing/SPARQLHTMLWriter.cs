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
#if !NO_WEB
using System.Web.UI;
#endif
using VDS.RDF.Query;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Writing
{
    /// <summary>
    /// Class for saving SPARQL Result Sets to a HTML Table format (this is not a standardised format)
    /// </summary>
    public class SparqlHtmlWriter : ISparqlResultsWriter, IHtmlWriter
    {
        private HtmlFormatter _formatter = new HtmlFormatter();

        #region IHtmlWriter Members

        private String _stylesheet = String.Empty;
        private String _uriClass = "uri",
                       _bnodeClass = "bnode",
                       _literalClass = "literal",
                       _datatypeClass = "datatype",
                       _langClass = "langspec";
        private String _uriPrefix = String.Empty;


        /// <summary>
        /// Gets/Sets a path to a Stylesheet which is used to format the Results
        /// </summary>
        public string Stylesheet
        {
            get
            {
                return this._stylesheet;
            }
            set
            {
                this._stylesheet = value;
            }
        }

        /// <summary>
        /// Gets/Sets the CSS class used for the anchor tags used to display the URIs of URI Nodes
        /// </summary>
        public string CssClassUri
        {
            get
            {
                return this._uriClass;
            }
            set
            {
                this._uriClass = value;
            }
        }

        /// <summary>
        /// Gets/Sets the CSS class used for the span tags used to display Blank Node IDs
        /// </summary>
        public string CssClassBlankNode
        {
            get
            {
                return this._bnodeClass;
            }
            set
            {
                this._bnodeClass = value;
            }
        }

        /// <summary>
        /// Gets/Sets the CSS class used for the span tags used to display Literals
        /// </summary>
        public string CssClassLiteral
        {
            get
            {
                return this._literalClass;
            }
            set
            {
                this._literalClass = value;
            }
        }

        /// <summary>
        /// Gets/Sets the CSS class used for the anchor tags used to display Literal datatypes
        /// </summary>
        public string CssClassDatatype
        {
            get
            {
                return this._datatypeClass;
            }
            set
            {
                this._datatypeClass = value;
            }
        }

        /// <summary>
        /// Gets/Sets the CSS class used for the span tags used to display Literal language specifiers
        /// </summary>
        public string CssClassLangSpec
        {
            get
            {
                return this._langClass;
            }
            set
            {
                this._langClass = value;
            }
        }

        /// <summary>
        /// Gets/Sets the Prefix applied to href attributes
        /// </summary>
        public String UriPrefix
        {
            get
            {
                return this._uriPrefix;
            }
            set
            {
                this._uriPrefix = value;
            }
        }

        #endregion

        /// <summary>
        /// Saves the Result Set to the given File as a HTML Table
        /// </summary>
        /// <param name="results">Result Set to save</param>
        /// <param name="filename">File to save to</param>
        public void Save(SparqlResultSet results, String filename)
        {
            StreamWriter output = new StreamWriter(filename, false, new UTF8Encoding(Options.UseBomForUtf8));
            this.Save(results, output);
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
                this.GenerateOutput(results, output);
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
                    //No Catch Actions
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

            //Page Header
            writer.Write("<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">");
            writer.RenderBeginTag(HtmlTextWriterTag.Html);
            writer.RenderBeginTag(HtmlTextWriterTag.Head);
            writer.RenderBeginTag(HtmlTextWriterTag.Title);
            writer.WriteEncodedText("SPARQL Query Results");
            writer.RenderEndTag();
            if (!this._stylesheet.Equals(String.Empty))
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Href, this._stylesheet);
                writer.AddAttribute(HtmlTextWriterAttribute.Type, "text/css");
                writer.AddAttribute(HtmlTextWriterAttribute.Rel, "stylesheet");
                writer.RenderBeginTag(HtmlTextWriterTag.Link);
                writer.RenderEndTag();
            }
            //TODO: Add <meta> for charset?
            writer.RenderEndTag();

            //Start Body
            writer.RenderBeginTag(HtmlTextWriterTag.Body);

            if (results.ResultsType == SparqlResultsType.VariableBindings)
            {
                //Create a Table for the results
                writer.AddAttribute(HtmlTextWriterAttribute.Width, "100%");
                writer.RenderBeginTag(HtmlTextWriterTag.Table);

                //Create a Table Header with the Variable Names
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
#if !NO_WEB
                writer.WriteLine();
#endif

                //Create a Table Body for the Results
                writer.RenderBeginTag(HtmlTextWriterTag.Tbody);

                //Create a Column for each Binding
                foreach (SparqlResult result in results)
                {
                    //Start Row
                    writer.RenderBeginTag(HtmlTextWriterTag.Tr);

                    foreach (String var in results.Variables)
                    {
                        //Start Column
                        writer.RenderBeginTag(HtmlTextWriterTag.Td);

                        if (result.HasValue(var))
                        {
                            INode value = result[var];

                            if (value != null)
                            {
                                switch (value.NodeType)
                                {
                                    case NodeType.Blank:
                                        writer.AddAttribute(HtmlTextWriterAttribute.Class, this._bnodeClass);
                                        writer.RenderBeginTag(HtmlTextWriterTag.Span);
                                        writer.WriteEncodedText(value.ToString());
                                        writer.RenderEndTag();
                                        break;

                                    case NodeType.Literal:
                                        LiteralNode lit = (LiteralNode)value;
                                        writer.AddAttribute(HtmlTextWriterAttribute.Class, this._literalClass);
                                        writer.RenderBeginTag(HtmlTextWriterTag.Span);
                                        if (lit.DataType != null)
                                        {
                                            writer.WriteEncodedText(lit.Value);
                                            writer.RenderEndTag();
                                            writer.WriteEncodedText("^^");
                                            writer.AddAttribute(HtmlTextWriterAttribute.Href, this._formatter.FormatUri(lit.DataType.ToString()));
                                            writer.AddAttribute(HtmlTextWriterAttribute.Class, this._datatypeClass);
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
                                                writer.AddAttribute(HtmlTextWriterAttribute.Class, this._langClass);
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
                                        //Error
                                        throw new RdfOutputException("Result Sets which contain Graph Literal Nodes cannot be serialized in the HTML Format");

                                    case NodeType.Uri:
                                        writer.AddAttribute(HtmlTextWriterAttribute.Class, this._uriClass);
                                        writer.AddAttribute(HtmlTextWriterAttribute.Href, this._formatter.FormatUri(this._uriPrefix + value.ToString()));
                                        writer.RenderBeginTag(HtmlTextWriterTag.A);
                                        writer.WriteEncodedText(value.ToString());
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

                        //End Column
                        writer.RenderEndTag();
                    }

                    //End Row
                    writer.RenderEndTag();
#if !NO_WEB
                    writer.WriteLine();
#endif
                }

                //End Table Body
                writer.RenderEndTag();

                //End Table
                writer.RenderEndTag();
            }
            else
            {
                //Show a Header and a Boolean value
                writer.RenderBeginTag(HtmlTextWriterTag.H3);
                writer.WriteEncodedText("ASK Query Result");
                writer.RenderEndTag();
                writer.RenderBeginTag(HtmlTextWriterTag.P);
                writer.WriteEncodedText(results.Result.ToString());
                writer.RenderEndTag();
            }

            //End of Page
            writer.RenderEndTag(); //End Body
            writer.RenderEndTag(); //End Html
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
    }
}
