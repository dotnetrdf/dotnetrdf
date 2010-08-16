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
using System.IO;
using System.Xml;
using VDS.RDF.Query;

//REQ: Implement an XmlReader based version of the SparqlXmlParser

namespace VDS.RDF.Parsing
{
    /// <summary>
    /// Parser for SPARQL Results XML Format
    /// </summary>
    public class SparqlXmlParser : ISparqlResultsReader
    {
        private IGraph _g;

        /// <summary>
        /// Creates a new SPARQL Results XML Parser
        /// </summary>
        public SparqlXmlParser()
        {
            //Initialise the Graph
            this._g = new ThreadSafeGraph();
            Uri sparql = new Uri(SparqlSpecsHelper.SparqlNamespace);
            this._g.BaseUri = sparql;
            this._g.NamespaceMap.AddNamespace("", sparql);
        }

        /// <summary>
        /// Loads a Result Set from an Input Stream
        /// </summary>
        /// <param name="results">Result Set to load into</param>
        /// <param name="input">Input Stream to read from</param>
        public void Load(SparqlResultSet results, StreamReader input)
        {
            //Ensure Empty Result Set
            if (!results.IsEmpty)
            {
                throw new RdfParseException("Cannot load a Result Set from a Stream into a non-empty Result Set");
            }

            try
            {
                //Parse the XML
                XmlDocument doc = new XmlDocument();
                doc.Load(input);
                this.Parse(doc, results);
            }
            catch
            {
                try
                {
                    input.Close();
                }
                catch
                {
                    //No catch actions - this is a cleanup attempt only
                }
                throw;
            }
        }

        /// <summary>
        /// Loads a Result Set from a File
        /// </summary>
        /// <param name="results">Result Set to load into</param>
        /// <param name="filename">File to load from</param>
        public void Load(SparqlResultSet results, string filename)
        {
            StreamReader input = new StreamReader(filename);
            this.Load(results, input);
        }

        /// <summary>
        /// Loads a Result Set from an XML Document
        /// </summary>
        /// <param name="results">Result Set to load into</param>
        /// <param name="doc">XML Document</param>
        public void Load(SparqlResultSet results, XmlDocument doc)
        {
            //Ensure Empty Result Set
            if (!results.IsEmpty)
            {
                throw new RdfParseException("Cannot load a Result Set from a Stream into a non-empty Result Set");
            }

            this.Parse(doc, results);
        }

        /// <summary>
        /// Parses the XML Result Set format into a set of SPARQLResult objects
        /// </summary>
        /// <param name="xmlDoc">XML Document to parse from</param>
        /// <param name="results">Result Set to parse into</param>
        private void Parse(XmlDocument xmlDoc, SparqlResultSet results)
        {
            try
            {
                XmlElement xmlDocEl;
                XmlNode xmlEl;

                results.SetEmpty(false);

                //Get the Document Element and check it's a Sparql element
                xmlDocEl = xmlDoc.DocumentElement;
                if (!xmlDocEl.Name.Equals("sparql"))
                {
                    throw new RdfParseException("Unable to Parse a SPARQL Result Set from the provided XML since the Document Element is not a <sparql> element!");
                }
                //Go through it's attributes and check the Namespace is specified
                bool nsfound = false;
                foreach (XmlAttribute a in xmlDocEl.Attributes)
                {
                    if (a.Name.Equals("xmlns"))
                    {
                        if (!a.Value.Equals(SparqlSpecsHelper.SparqlNamespace))
                        {
                            throw new RdfParseException("Unable to Parse a SPARQL Result Set since the <sparql> element has an incorrect Namespace!");
                        }
                        else
                        {
                            nsfound = true;
                        }
                    }
                }
                if (!nsfound)
                {
                    throw new RdfParseException("Unable to Parse a SPARQL Result Set since the <sparql> element fails to specify the SPARQL Namespace!");
                }

                //Check Number of Child Nodes
                if (xmlDocEl.ChildNodes.Count < 2)
                {
                    //Not enough child nodes, should be a <head> followed by a <results> or <boolean>
                    throw new RdfParseException("Unable to Parse a SPARQL Result Set since there are insufficient Child Nodes of the <sparql> element present.  A <sparql> element must contain a <head> element followed by a <results> or <boolean> element!");
                }

                //Get the Variables from the Header
                xmlEl = xmlDocEl.ChildNodes[0];
                if (!xmlEl.Name.Equals("head"))
                {
                    throw new RdfParseException("Unable to Parse a SPARQL Result Set since the first Child Node of the <sparql> element is not the required <head> element!");
                }
                foreach (XmlNode n in xmlEl.ChildNodes)
                {
                    //Looking for <variable> elements
                    if (n.Name.Equals("variable"))
                    {
                        //Should only have 1 attribute
                        if (n.Attributes.Count != 1)
                        {
                            throw new RdfParseException("Unable to Parse a SPARQL Result Set since a <variable> element has too few/many attributes, only a 'name' attribute should be present!");
                        }
                        else
                        {
                            //Add the Variable to the list
                            results.AddVariable(n.Attributes[0].Value);
                        }
                    }
                    else if (n.Name.Equals("link"))
                    {
                        //Not bothered about <link> elements
                    }
                    else
                    {
                        //Some unexpected element
                        throw new RdfParseException("Unable to Parse a SPARQL Result Set since the <head> contains an unexpected element <" + n.Name + ">!");
                    }
                }

                //Look at the <results> or <boolean> element
                xmlEl = xmlDocEl.ChildNodes[1];
                if (xmlEl.Name.Equals("results"))
                {
                    foreach (XmlNode res in xmlEl.ChildNodes)
                    {
                        //Must be a <result> element
                        if (!res.Name.Equals("result"))
                        {
                            throw new RdfParseException("Unable to Parse a SPARQL Result Set since the <results> element contains an unexpected element <" + res.Name + ">!");
                        }

                        //Get the values of each Binding
                        String var;
                        INode value;
                        SparqlResult result = new SparqlResult();
                        foreach (XmlNode binding in res.ChildNodes)
                        {
                            //Must be a <binding> element
                            if (!binding.Name.Equals("binding"))
                            {
                                throw new RdfParseException("Unable to Parse a SPARQL Result Set since a <result> element contains an unexpected element <" + res.Name + ">!");
                            }
                            //Must have only 1 attribute
                            if (binding.Attributes.Count != 1)
                            {
                                throw new RdfParseException("Unable to Parse a SPARQL Result Set since a <binding> element has too few/many attributes, only a 'name' attribute should be present!");
                            }

                            //Get the Variable this is a binding for and its Value
                            var = binding.Attributes[0].Value;
                            value = this.ParseValue(binding.ChildNodes[0]);

                            //Check that the Variable was defined in the Header
                            if (!results.Variables.Contains(var))
                            {
                                throw new RdfParseException("Unable to Parse a SPARQL Result Set since a <binding> element attempts to bind a value to the variable '" + var + "' which is not defined in the <head> with a <variable> element!");
                            }

                            //Set the Variable to the Value
                            result.SetValue(var, value);

                            //Check that all Variables are bound for a given result binding nulls where appropriate
                            foreach (String v in results.Variables)
                            {
                                if (!result.HasValue(v))
                                {
                                    result.SetValue(v, null);
                                }
                            }
                        }

                        //Add to results set
                        results.AddResult(result);
                    }
                }
                else if (xmlEl.Name.Equals("boolean"))
                {
                    //Can't be any <variable> elements
                    if (results.Variables.Any())
                    {
                        throw new RdfParseException("Unable to Parse a SPARQL Result Set since the <boolean> element is specified but the <head> contained one/more <variable> elements which is not permitted!");
                    }

                    try
                    {
                        //Get the value of the <boolean> element as a Boolean
                        Boolean b = Boolean.Parse(xmlEl.InnerText);
                        results.SetResult(b);
                    }
                    catch (Exception)
                    {
                        throw new RdfParseException("Unable to Parse a SPARQL Result Set since the <boolean> element contained a value that could not be understood as a Boolean value!");
                    }
                }
                else
                {
                    throw new RdfParseException("Unable to Parse a SPARQL Result Set since the second Child Node of the <sparql> element is not the required <results> or <boolean> element!");
                }
            }
            catch (XmlException xmlEx)
            {
                //Error processing the XML itself
                throw new RdfParseException("Unable to Parse a SPARQL Result Set due to an error in System.Xml:\n" + xmlEx.Message, xmlEx);
            }
            catch (Exception)
            {
                //Some other Error
                throw;
            }
        }

        /// <summary>
        /// Internal Helper method which parses the child element of a &lt;binding&gt; element into an <see cref="INode">INode</see>
        /// </summary>
        /// <param name="valueNode">An XML Node representing the value bound to a Variable for a given Binding</param>
        /// <returns></returns>
        private INode ParseValue(XmlNode valueNode)
        {
            if (valueNode.Name.Equals("uri"))
            {
                return this._g.CreateUriNode(new Uri(valueNode.InnerText));
            }
            else if (valueNode.Name.Equals("literal"))
            {
                if (valueNode.Attributes.Count == 0)
                {
                    //Literal with no Data Type/Language Specifier
                    return this._g.CreateLiteralNode(valueNode.InnerText);
                }
                else if (valueNode.Attributes.Count == 1)
                {
                    XmlAttribute attr = valueNode.Attributes[0];
                    if (attr.Name.Equals("xml:lang"))
                    {
                        //Language is specified
                        return this._g.CreateLiteralNode(valueNode.InnerText, attr.Value);
                    }
                    else if (attr.Name.Equals("datatype"))
                    {
                        //Data Type is specified
                        return this._g.CreateLiteralNode(valueNode.InnerText, new Uri(attr.Value));
                    }
                    else
                    {
                        throw new RdfParseException("Unable to Parse a SPARQL Result Set since a <literal> element has an unknown attribute '" + attr.Name + "'!");
                    }
                }
                else
                {
                    throw new RdfParseException("Unable to Parse a SPARQL Result Set since a <literal> element has too many Attributes, only 1 of 'xml:lang' or 'datatype' may be specified!");
                }
            }
            else if (valueNode.Name.Equals("bnode"))
            {
                return this._g.CreateBlankNode(valueNode.InnerText);
            }
            else
            {
                throw new RdfParseException("Unable to Parse a SPARQL Result Set since a <binding> element contains an unexpected element <" + valueNode.Name + ">!");
            }
        }
    }
}

#endif