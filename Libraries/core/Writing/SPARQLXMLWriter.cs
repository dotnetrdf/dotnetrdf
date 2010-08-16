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

#if !NO_XMLDOM

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using VDS.RDF.Query;

//REQ: Implement an XmlWriter based version of this for non-XML DOM supporting platforms

namespace VDS.RDF.Writing
{
    /// <summary>
    /// Class for saving Sparql Result Sets to the Sparql Results XML Format
    /// </summary>
    public class SparqlXmlWriter : ISparqlResultsWriter
    {
        /// <summary>
        /// Saves the Result Set to the given File in the Sparql Results XML Format
        /// </summary>
        /// <param name="results">Result Set to save</param>
        /// <param name="filename">File to save to</param>
        public virtual void Save(SparqlResultSet results, String filename)
        {
            StreamWriter output = new StreamWriter(filename, false, Encoding.UTF8);
            this.Save(results, output);
        }

        /// <summary>
        /// Saves the Result Set to the given Stream in the Sparql Results XML Format
        /// </summary>
        /// <param name="results"></param>
        /// <param name="output"></param>
        public virtual void Save(SparqlResultSet results, TextWriter output)
        {
            try
            {
                XmlDocument doc = this.GenerateOutput(results);
                doc.Save(output);
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
        /// Method which generates the Sparql Query Results XML Format serialization of the Result Set
        /// </summary>
        /// <returns></returns>
        protected XmlDocument GenerateOutput(SparqlResultSet resultSet)
        {
            XmlDocument xmlDoc = new XmlDocument();

            //XML Declaration
            XmlDeclaration xmlDec = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", String.Empty);
            xmlDoc.AppendChild(xmlDec);

            //<sparql> element
            XmlElement sparql = xmlDoc.CreateElement("sparql");
            XmlAttribute sparqlns = xmlDoc.CreateAttribute("xmlns");
            sparqlns.Value = SparqlSpecsHelper.SparqlNamespace;
            sparql.Attributes.Append(sparqlns);
            xmlDoc.AppendChild(sparql);

            //<head> element
            XmlElement head = xmlDoc.CreateElement("head");
            sparql.AppendChild(head);

            //Variables in the Header?
            if (resultSet.ResultsType == SparqlResultsType.VariableBindings)
            {
                foreach (String var in resultSet.Variables)
                {
                    //<variable> element
                    XmlElement varEl = xmlDoc.CreateElement("variable");
                    XmlAttribute varAttr = xmlDoc.CreateAttribute("name");
                    varAttr.Value = var;
                    varEl.Attributes.Append(varAttr);
                    head.AppendChild(varEl);
                }

                //<results> Element
                XmlElement results = xmlDoc.CreateElement("results");
                sparql.AppendChild(results);

                foreach (SparqlResult r in resultSet.Results)
                {
                    //<result> Element
                    XmlElement result = xmlDoc.CreateElement("result");
                    results.AppendChild(result);

                    foreach (String var in resultSet.Variables)
                    {
                        if (r.HasValue(var))
                        {
                            //<binding> Element
                            XmlElement binding = xmlDoc.CreateElement("binding");
                            XmlAttribute name = xmlDoc.CreateAttribute("name");
                            name.Value = var;
                            binding.Attributes.Append(name);

                            INode n = r.Value(var);
                            if (n == null) continue; //NULLs don't get serialized in the XML Format
                            switch (n.NodeType)
                            {
                                case NodeType.Blank:
                                    //<bnode> element
                                    XmlElement bnode = xmlDoc.CreateElement("bnode");
                                    bnode.InnerText = ((BlankNode)n).InternalID;
                                    binding.AppendChild(bnode);
                                    break;

                                case NodeType.GraphLiteral:
                                    //Error!
                                    throw new RdfOutputException("Result Sets which contain Graph Literal Nodes cannot be serialized in the SPARQL Query Results XML Format");

                                case NodeType.Literal:
                                    //<literal> element
                                    XmlElement lit = xmlDoc.CreateElement("literal");
                                    LiteralNode l = (LiteralNode)n;
                                    lit.InnerText = l.Value;

                                    if (!l.Language.Equals(String.Empty))
                                    {
                                        XmlAttribute lang = xmlDoc.CreateAttribute("xml:lang");
                                        lang.Value = l.Language;
                                        lit.Attributes.Append(lang);
                                    }
                                    else if (l.DataType != null)
                                    {
                                        XmlAttribute dt = xmlDoc.CreateAttribute("datatype");
                                        dt.Value = l.DataType.ToString();
                                        lit.Attributes.Append(dt);
                                    }

                                    binding.AppendChild(lit);
                                    break;

                                case NodeType.Uri:
                                    //<uri> element
                                    XmlElement uri = xmlDoc.CreateElement("uri");
                                    uri.InnerText = ((UriNode)n).StringUri;
                                    binding.AppendChild(uri);
                                    break;

                                default:
                                    throw new RdfOutputException("Result Sets which contain Nodes of unknown Type cannot be serialized in the SPARQL Query Results XML Format");

                            }

                            result.AppendChild(binding);
                        }
                    }
                }
            }
            else
            {
                XmlElement boolRes = xmlDoc.CreateElement("boolean");
                boolRes.InnerText = resultSet.Result.ToString().ToLower();
                sparql.AppendChild(boolRes);
            }

            return xmlDoc;
        }
    }
}

#endif