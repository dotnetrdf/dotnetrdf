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
using System.Xml;
using VDS.RDF.Query;

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
#if !NO_RWLOCK
            this._g = new ThreadSafeGraph();
#else
            this._g = new Graph();
#endif
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
#if !NO_XMLDOM
                XmlDocument doc = new XmlDocument();
                doc.Load(input);
                this.Parse(doc, results);
#else
                XmlReader reader = XmlReader.Create(input, this.GetSettings());
                this.Parse(reader, results);
#endif
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

#if !NO_XMLDOM

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

#else
        /// <summary>
        /// Initialises the XML Reader settings
        /// </summary>
        /// <returns></returns>
        private XmlReaderSettings GetSettings()
        {
            XmlReaderSettings settings = new XmlReaderSettings();
#if SILVERLIGHT
            settings.DtdProcessing = DtdProcessing.Parse;
#else
            settings.ProhibitDtd = false;
#endif
            settings.ConformanceLevel = ConformanceLevel.Document;
            settings.IgnoreComments = true;
            settings.IgnoreProcessingInstructions = true;
            settings.IgnoreWhitespace = true;
            return settings;
        }

        /// <summary>
        /// Parses the XML Result Set format into a set of SPARQLResult objects
        /// </summary>
        /// <param name="xmlDoc">XML Document to parse from</param>
        /// <param name="results">Result Set to parse into</param>
        private void Parse(XmlReader reader, SparqlResultSet results)
        {
            try
            {
                results.SetEmpty(false);

                //Get the Document Element and check it's a Sparql element
                if (!reader.Read()) throw new RdfParseException("Unable to Parse a SPARQL Result Set as it was not possible to read a document element from the input");
                while (reader.NodeType != XmlNodeType.Element)
                {
                    if (!reader.Read()) throw new RdfParseException("Unable to Parse a SPARQL Result Set as it was not possible to read a document element from the input");
                }
                if (!reader.Name.Equals("sparql"))
                {
                    throw new RdfParseException("Unable to Parse a SPARQL Result Set from the provided XML since the Document Element is not a <sparql> element!");
                }

                //Go through it's attributes and check the Namespace is specified
                bool nsfound = false;
                if (reader.HasAttributes)
                {
                    for (int i = 0; i < reader.AttributeCount; i++)
                    {
                        reader.MoveToNextAttribute();
                        if (reader.Name.Equals("xmlns"))
                        {
                            if (!reader.Value.Equals(SparqlSpecsHelper.SparqlNamespace))
                            {
                                throw new RdfParseException("Unable to Parse a SPARQL Result Set since the <sparql> element has an incorrect Namespace!");
                            }
                            else
                            {
                                nsfound = true;
                            }
                        }
                    }
                }
                if (!nsfound)
                {
                    throw new RdfParseException("Unable to Parse a SPARQL Result Set since the <sparql> element fails to specify the SPARQL Namespace!");
                }

                //Get the Variables from the Header
                if (!reader.Read()) throw new RdfParseException("Unable to Parse a SPARQL Result Set as could not read a <head> element from the input");
                if (!reader.Name.Equals("head"))
                {
                    throw new RdfParseException("Unable to Parse a SPARQL Result Set since the first Child Node of the <sparql> element is not the required <head> element!");
                }
                while (reader.Read())
                {
                    //Stop reading when we hit the </head>
                    if (reader.NodeType == XmlNodeType.EndElement && reader.Name.Equals("head")) break;

                    //Looking for <variable> elements
                    if (reader.Name.Equals("variable"))
                    {
                        //Should only have 1 attribute
                        if (reader.AttributeCount != 1)
                        {
                            throw new RdfParseException("Unable to Parse a SPARQL Result Set since a <variable> element has too few/many attributes, only a 'name' attribute should be present!");
                        }
                        else
                        {
                            //Add the Variable to the list
                            reader.MoveToNextAttribute();
                            results.AddVariable(reader.Value);
                        }
                    }
                    else if (reader.Name.Equals("link"))
                    {
                        //Not bothered about <link> elements
                    }
                    else
                    {
                        //Some unexpected element
                        throw new RdfParseException("Unable to Parse a SPARQL Result Set since the <head> contains an unexpected element <" + reader.Name + ">!");
                    }
                }

                if (!reader.Name.Equals("head"))
                {
                    throw new RdfParseException("Unable to Parse a SPARQL Result Set as reached the end of the input before the closing </head> element was found");
                }

                //Look at the <results> or <boolean> element
                if (!reader.Read()) throw new RdfParseException("Unable to Parse a SPARQL Result Set as could not read a <results> element from the input");
                if (reader.Name.Equals("results"))
                {
                    while (reader.Read())
                    {
                        //Stop reading when we hit the </results>
                        if (reader.NodeType == XmlNodeType.EndElement && reader.Name.Equals("results")) break;

                        //Must be a <result> element
                        if (!reader.Name.Equals("result"))
                        {
                            throw new RdfParseException("Unable to Parse a SPARQL Result Set since the <results> element contains an unexpected element <" + reader.Name + ">!");
                        }

                        //Get the values of each Binding
                        String var;
                        INode value;
                        SparqlResult result = new SparqlResult();
                        while (reader.Read())
                        {
                            //Stop reading when we hit the </binding>
                            if (reader.NodeType == XmlNodeType.EndElement && reader.Name.Equals("result")) break;

                            //Must be a <binding> element
                            if (!reader.Name.Equals("binding"))
                            {
                                throw new RdfParseException("Unable to Parse a SPARQL Result Set since a <result> element contains an unexpected element <" + reader.Name + ">!");
                            }

                            //Must have only 1 attribute
                            if (reader.AttributeCount != 1)
                            {
                                throw new RdfParseException("Unable to Parse a SPARQL Result Set since a <binding> element has too few/many attributes, only a 'name' attribute should be present!");
                            }

                            //Get the Variable this is a binding for and its Value
                            reader.MoveToNextAttribute();
                            var = reader.Value;
                            if (!reader.Read()) throw new RdfParseException("Unable to Parse a SPARQL Result Set as reached the end of input when the contents of a <binding> element was expected");
                            value = this.ParseValue(reader);

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

                        if (!reader.Name.Equals("result"))
                        {
                            throw new RdfParseException("Unable to Parse a SPARQL Result Set as reached the end of the input before a closing </result> element was found");
                        }

                        //Add to results set
                        results.AddResult(result);
                    }

                    if (!reader.Name.Equals("results"))
                    {
                        throw new RdfParseException("Unable to Parse a SPARQL Result Set as reached the end of the input before the closing </results> element was found");
                    }
                }
                else if (reader.Name.Equals("boolean"))
                {
                    //Can't be any <variable> elements
                    if (results.Variables.Any())
                    {
                        throw new RdfParseException("Unable to Parse a SPARQL Result Set since the <boolean> element is specified but the <head> contained one/more <variable> elements which is not permitted!");
                    }

                    try
                    {
                        //Get the value of the <boolean> element as a Boolean
                        Boolean b = Boolean.Parse(reader.ReadInnerXml());
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
        private INode ParseValue(XmlReader reader)
        {
            if (reader.Name.Equals("uri"))
            {
                return this._g.CreateUriNode(new Uri(reader.ReadInnerXml()));
            }
            else if (reader.Name.Equals("literal"))
            {
                if (reader.AttributeCount == 0)
                {
                    //Literal with no Data Type/Language Specifier
                    return this._g.CreateLiteralNode(reader.ReadInnerXml());
                }
                else if (reader.AttributeCount == 1)
                {
                    reader.MoveToNextAttribute();
                    if (reader.Name.Equals("xml:lang"))
                    {
                        //Language is specified
                        String lang = reader.Value;
                        reader.MoveToContent();
                        return this._g.CreateLiteralNode(reader.ReadInnerXml(), lang);
                    }
                    else if (reader.Name.Equals("datatype"))
                    {
                        //Data Type is specified
                        String dt = reader.Value;
                        reader.MoveToContent();
                        return this._g.CreateLiteralNode(reader.ReadInnerXml(), new Uri(dt));
                    }
                    else
                    {
                        throw new RdfParseException("Unable to Parse a SPARQL Result Set since a <literal> element has an unknown attribute '" + reader.Name + "'!");
                    }
                }
                else
                {
                    throw new RdfParseException("Unable to Parse a SPARQL Result Set since a <literal> element has too many Attributes, only 1 of 'xml:lang' or 'datatype' may be specified!");
                }
            }
            else if (reader.Name.Equals("bnode"))
            {
                return this._g.CreateBlankNode(reader.ReadInnerXml());
            }
            else
            {
                throw new RdfParseException("Unable to Parse a SPARQL Result Set since a <binding> element contains an unexpected element <" + reader.Name + ">!");
            }
        }
#endif
    }
}
