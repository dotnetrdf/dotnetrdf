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
using System.IO;
using System.Linq;
using System.Text;
#if !NO_WEB
using System.Web.UI;
#endif
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Writing.Contexts;
using VDS.RDF.Writing.Formatting;

//TODO: Embed RDFa in the HTML Output

namespace VDS.RDF.Writing
{
    /// <summary>
    /// HTML Schema Writer is a HTML Writer which writes a human readable description of a Schema/Ontology
    /// </summary>
    public class HtmlSchemaWriter : BaseHtmlWriter, IRdfWriter
    {
        /// <summary>
        /// Saves the Graph to the given File as an XHTML Table with embedded RDFa
        /// </summary>
        /// <param name="g">Graph to save</param>
        /// <param name="filename">File to save to</param>
        public void Save(IGraph g, String filename)
        {
            StreamWriter output = new StreamWriter(filename, false, new UTF8Encoding(Options.UseBomForUtf8));
            this.Save(g, output);
        }

        /// <summary>
        /// Saves the Result Set to the given Stream as an XHTML Table with embedded RDFa
        /// </summary>
        /// <param name="g">Graph to save</param>
        /// <param name="output">Stream to save to</param>
        public void Save(IGraph g, TextWriter output)
        {
            try
            {
                HtmlWriterContext context = new HtmlWriterContext(g, output);
                this.GenerateOutput(context);
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
        /// Internal method which generates the HTML Output for the Graph
        /// </summary>
        /// <param name="context">Writer Context</param>
        private void GenerateOutput(HtmlWriterContext context)
        {
            Object results;

            //Add the Namespaces we want to use later on
            context.QNameMapper.AddNamespace("owl", new Uri(NamespaceMapper.OWL));
            context.QNameMapper.AddNamespace("rdf", new Uri(NamespaceMapper.RDF));
            context.QNameMapper.AddNamespace("rdfs", new Uri(NamespaceMapper.RDFS));
            context.QNameMapper.AddNamespace("dc", new Uri("http://purl.org/dc/elements/1.1/"));
            context.QNameMapper.AddNamespace("dct", new Uri("http://purl.org/dc/terms/"));
            context.QNameMapper.AddNamespace("vann", new Uri("http://purl.org/vocab/vann/"));
            context.QNameMapper.AddNamespace("vs", new Uri("http://www.w3.org/2003/06/sw-vocab-status/ns#"));

            //Find the Node that represents the Schema Ontology
            //Assumes there is exactly one thing given rdf:type owl:Ontology
            IUriNode ontology = context.Graph.CreateUriNode(new Uri(NamespaceMapper.OWL + "Ontology"));
            IUriNode rdfType = context.Graph.CreateUriNode(new Uri(RdfSpecsHelper.RdfType));
            IUriNode rdfsLabel = context.Graph.CreateUriNode(new Uri(NamespaceMapper.RDFS + "label"));
            INode ontoNode = context.Graph.GetTriplesWithPredicateObject(rdfType, ontology).Select(t => t.Subject).FirstOrDefault();
            INode ontoLabel = (ontoNode != null) ? context.Graph.GetTriplesWithSubjectPredicate(ontoNode, rdfsLabel).Select(t => t.Object).FirstOrDefault() : null;

            //Stuff for formatting
            //We'll use the Turtle Formatter to get nice QNames wherever possible
            context.NodeFormatter = new TurtleFormatter(context.QNameMapper);
            context.UriFormatter = (IUriFormatter)context.NodeFormatter;

            //Page Header
            context.HtmlWriter.Write("<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML+RDFa 1.0//EN\" \"http://www.w3.org/MarkUp/DTD/xhtml-rdfa-1.dtd\">");
            context.HtmlWriter.WriteLine();
            context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.Html);
            context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.Head);
            context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.Title);
            context.HtmlWriter.WriteEncodedText("Schema");
            if (ontoNode != null && ontoLabel != null)
            {
                context.HtmlWriter.WriteEncodedText(" - " + ontoLabel.ToSafeString());
            }
            else if (context.Graph.BaseUri != null)
            {
                context.HtmlWriter.WriteEncodedText(" - " + context.Graph.BaseUri.ToString());
            }
            context.HtmlWriter.RenderEndTag();
            if (!this.Stylesheet.Equals(String.Empty))
            {
                context.HtmlWriter.AddAttribute(HtmlTextWriterAttribute.Href, this.Stylesheet);
                context.HtmlWriter.AddAttribute(HtmlTextWriterAttribute.Type, "text/css");
                context.HtmlWriter.AddAttribute(HtmlTextWriterAttribute.Rel, "stylesheet");
                context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.Link);
                context.HtmlWriter.RenderEndTag();
            }
            context.HtmlWriter.RenderEndTag();
#if !NO_WEB
            context.HtmlWriter.WriteLine();
#endif

            //Start Body
            context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.Body);

            //Title
            context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.H2);
            context.HtmlWriter.WriteEncodedText("Schema");
            if (ontoNode != null && ontoLabel != null)
            {
                context.HtmlWriter.WriteEncodedText(" - " + ontoLabel.ToSafeString());
            }
            else if (context.Graph.BaseUri != null)
            {
                context.HtmlWriter.WriteEncodedText(" - " + context.Graph.BaseUri.ToString());
            }
            context.HtmlWriter.RenderEndTag();
#if !NO_WEB
            context.HtmlWriter.WriteLine();
#endif

            //Show the Description of the Schema (if any)
            if (ontoNode != null)
            {
                SparqlParameterizedString getOntoDescrip = new SparqlParameterizedString();
                getOntoDescrip.Namespaces = context.QNameMapper;
                getOntoDescrip.CommandText = "SELECT * WHERE { @onto a owl:Ontology . OPTIONAL { @onto rdfs:comment ?description } . OPTIONAL { @onto vann:preferredNamespacePrefix ?nsPrefix ; vann:preferredNamespaceUri ?nsUri } . OPTIONAL { @onto dc:creator ?creator . ?creator (foaf:name | rdfs:label) ?creatorName } }";
                getOntoDescrip.SetParameter("onto", ontoNode);

                try 
                {
                    results = context.Graph.ExecuteQuery(getOntoDescrip);
                    if (results is SparqlResultSet)
                    {
                        if (!((SparqlResultSet)results).IsEmpty)
                        {
                            SparqlResult ontoInfo = ((SparqlResultSet)results)[0];

                            //Show rdfs:comment on the Ontology
                            if (ontoInfo.HasValue("description"))
                            {
                                INode descrip = ontoInfo["description"];
                                if (descrip.NodeType == NodeType.Literal)
                                {
                                    context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.P);
                                    context.HtmlWriter.Write(((ILiteralNode)descrip).Value);
                                    context.HtmlWriter.RenderEndTag();
#if !NO_WEB
                                    context.HtmlWriter.WriteLine();
#endif
                                }
                            }

                            //Show Author Information
                            if (ontoInfo.HasValue("creator"))
                            {
                                INode author = ontoInfo["creator"];
                                INode authorName = ontoInfo["creatorName"];
                                context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.P);
                                context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.Em);
                                context.HtmlWriter.WriteEncodedText("Schema created by ");
                                if (author.NodeType == NodeType.Uri)
                                {
                                    context.HtmlWriter.AddAttribute("href", ((IUriNode)author).Uri.ToString());
                                    context.HtmlWriter.AddAttribute(HtmlTextWriterAttribute.Class, this.CssClassUri);
                                    context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.A);
                                }
                                switch (authorName.NodeType)
                                {
                                    case NodeType.Uri:
                                        context.HtmlWriter.WriteEncodedText(((IUriNode)authorName).Uri.ToString());
                                        break;
                                    case NodeType.Literal:
                                        context.HtmlWriter.WriteEncodedText(((ILiteralNode)authorName).Value);
                                        break;
                                    default:
                                        context.HtmlWriter.WriteEncodedText(authorName.ToString());
                                        break;
                                }
                                if (author.NodeType == NodeType.Uri) context.HtmlWriter.RenderEndTag();
                                context.HtmlWriter.RenderEndTag();
                                context.HtmlWriter.RenderEndTag();
#if !NO_WEB
                                context.HtmlWriter.WriteLine();
#endif
                            }

                            //Show the Namespace information for the Schema
                            if (ontoInfo.HasValue("nsPrefix"))
                            {
                                if (ontoInfo["nsPrefix"].NodeType == NodeType.Literal && ontoInfo["nsUri"].NodeType == NodeType.Uri)
                                {
                                    //Add this QName to the QName Mapper so we can get nice QNames later on
                                    String prefix = ((ILiteralNode)ontoInfo["nsPrefix"]).Value;
                                    context.QNameMapper.AddNamespace(prefix, ((IUriNode)ontoInfo["nsUri"]).Uri);

                                    context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.H4);
                                    context.HtmlWriter.WriteEncodedText("Preferred Namespace Definition");
                                    context.HtmlWriter.RenderEndTag();

#if !NO_WEB
                                    context.HtmlWriter.WriteLine();
#endif

                                    //Show human readable description of preferred Namespace Settings
                                    context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.P);
                                    context.HtmlWriter.WriteEncodedText("Preferred Namespace Prefix is ");
                                    context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.Strong);
                                    context.HtmlWriter.WriteEncodedText(prefix);
                                    context.HtmlWriter.RenderEndTag();
                                    context.HtmlWriter.WriteEncodedText(" and preferred Namespace URI is ");
                                    context.HtmlWriter.AddAttribute(HtmlTextWriterAttribute.Href, context.QNameMapper.GetNamespaceUri(prefix).ToString());
                                    context.HtmlWriter.AddAttribute(HtmlTextWriterAttribute.Class, this.CssClassUri);
                                    context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.A);
                                    context.HtmlWriter.WriteEncodedText(context.QNameMapper.GetNamespaceUri(prefix).ToString());
                                    context.HtmlWriter.RenderEndTag();
                                    context.HtmlWriter.RenderEndTag();

                                    //RDF/XML Syntax
                                    context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.H5);
                                    context.HtmlWriter.WriteEncodedText("RDF/XML Syntax");
                                    context.HtmlWriter.RenderEndTag();
#if !NO_WEB
                                    context.HtmlWriter.WriteLine();
#endif
                                    context.HtmlWriter.AddStyleAttribute(HtmlTextWriterStyle.Width, "90%");
                                    context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.Pre);
                                    context.HtmlWriter.WriteEncodedText("<?xml version=\"1.0\" charset=\"utf-8\"?>");
                                    context.HtmlWriter.WriteLine();
                                    context.HtmlWriter.WriteEncodedText("<rdf:RDF xmlns:rdf=\"" + NamespaceMapper.RDF + "\" xmlns:" + prefix + "=\"" + context.UriFormatter.FormatUri(context.QNameMapper.GetNamespaceUri(prefix)) + "\">");
                                    context.HtmlWriter.WriteLine();
                                    context.HtmlWriter.WriteEncodedText("   <!-- Your RDF here... -->");
                                    context.HtmlWriter.WriteLine();
                                    context.HtmlWriter.WriteEncodedText("</rdf:RDF>");
                                    context.HtmlWriter.RenderEndTag();
#if !NO_WEB
                                    context.HtmlWriter.WriteLine();
#endif

                                    //Turtle/N3 Syntax
                                    context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.H5);
                                    context.HtmlWriter.WriteEncodedText("Turtle/N3 Syntax");
                                    context.HtmlWriter.RenderEndTag();
#if !NO_WEB
                                    context.HtmlWriter.WriteLine();
#endif
                                    context.HtmlWriter.AddStyleAttribute(HtmlTextWriterStyle.Width, "90%");
                                    context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.Pre);
                                    context.HtmlWriter.WriteEncodedText("@prefix " + prefix + ": <" + context.UriFormatter.FormatUri(context.QNameMapper.GetNamespaceUri(prefix)) + "> .");
                                    context.HtmlWriter.RenderEndTag();
#if !NO_WEB
                                    context.HtmlWriter.WriteLine();
#endif

                                    //SPARQL Syntax
                                    context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.H5);
                                    context.HtmlWriter.WriteEncodedText("SPARQL Syntax");
                                    context.HtmlWriter.RenderEndTag();
#if !NO_WEB
                                    context.HtmlWriter.WriteLine();
#endif
                                    context.HtmlWriter.AddStyleAttribute(HtmlTextWriterStyle.Width, "90%");
                                    context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.Pre);
                                    context.HtmlWriter.WriteEncodedText("PREFIX " + prefix + ": <" + context.UriFormatter.FormatUri(context.QNameMapper.GetNamespaceUri(prefix)) + ">");
                                    context.HtmlWriter.RenderEndTag();
#if !NO_WEB
                                    context.HtmlWriter.WriteLine();
#endif
                                }
                            }
                        }
                    }
                    else
                    {
                        throw new RdfOutputException("Tried to make a SPARQL Query to determine Schema Information but an unexpected Query Result was returned");
                    }
                }
                catch (RdfQueryException queryEx)
                {
                    throw new RdfOutputException("Tried to make a SPARQL Query to determine Schema Information but a Query Error occurred", queryEx);
                }
            }

            //Show lists of all Classes and Properties in the Schema
            context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.H4);
            context.HtmlWriter.WriteEncodedText("Class and Property Summary");
            context.HtmlWriter.RenderEndTag();
#if !NO_WEB
            context.HtmlWriter.WriteLine();
#endif

            context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.P);
            context.HtmlWriter.WriteEncodedText("This Schema defines the following classes:");
            context.HtmlWriter.RenderEndTag();
#if !NO_WEB
            context.HtmlWriter.WriteLine();
#endif
            context.HtmlWriter.AddStyleAttribute("width", "90%");
            context.HtmlWriter.AddAttribute(HtmlTextWriterAttribute.Class, this.CssClassBox);
            context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.P);

            //Get the Classes and Display
            SparqlParameterizedString getClasses = new SparqlParameterizedString();
            getClasses.Namespaces = context.QNameMapper;
            getClasses.CommandText = "SELECT DISTINCT ?class WHERE { { ?class a rdfs:Class } UNION { ?class a owl:Class } FILTER(ISURI(?class)) } ORDER BY ?class";
            try
            {
                results = context.Graph.ExecuteQuery(getClasses);
                if (results is SparqlResultSet)
                {
                    SparqlResultSet rs = (SparqlResultSet)results;
                    for (int i = 0; i < rs.Count; i++)
                    {
                        SparqlResult r = rs[i];

                        //Get the QName and output a Link to an anchor that we'll generate later to let
                        //users jump to a Class/Property definition
                        String qname = context.NodeFormatter.Format(r["class"]);
                        context.HtmlWriter.AddAttribute("href", "#" + qname);
                        context.HtmlWriter.AddAttribute(HtmlTextWriterAttribute.Class, this.CssClassUri);
                        context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.A);
                        context.HtmlWriter.WriteEncodedText(qname);
                        context.HtmlWriter.RenderEndTag();

                        if (i < rs.Count - 1)
                        {
                            context.HtmlWriter.WriteEncodedText(" , ");
                        }
                    }
                }
                else
                {
                    throw new RdfOutputException("Tried to make a SPARQL Query to find Classes in the Schema but an unexpected Query Result was returned");
                }
            }
            catch (RdfQueryException queryEx)
            {
                throw new RdfOutputException("Tried to make a SPARQL Query to find Classes in the Schema but a Query Error occurred", queryEx);
            }

            context.HtmlWriter.RenderEndTag();
#if !NO_WEB
            context.HtmlWriter.WriteLine();
#endif

            context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.P);
            context.HtmlWriter.WriteEncodedText("This Schema defines the following properties:");
            context.HtmlWriter.RenderEndTag();
#if !NO_WEB
            context.HtmlWriter.WriteLine();
#endif
            context.HtmlWriter.AddStyleAttribute("width", "90%");
            context.HtmlWriter.AddAttribute(HtmlTextWriterAttribute.Class, this.CssClassBox);
            context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.P);

            //Get the Properties and Display
            SparqlParameterizedString getProperties = new SparqlParameterizedString();
            getProperties.Namespaces = context.QNameMapper;
            getProperties.CommandText = "SELECT DISTINCT ?property WHERE { { ?property a rdf:Property } UNION { ?property a owl:DatatypeProperty } UNION { ?property a owl:ObjectProperty } FILTER(ISURI(?property)) } ORDER BY ?property";
            try
            {
                results = context.Graph.ExecuteQuery(getProperties);
                if (results is SparqlResultSet)
                {
                    SparqlResultSet rs = (SparqlResultSet)results;
                    for (int i = 0; i < rs.Count; i++)
                    {
                        SparqlResult r = rs[i];

                        //Get the QName and output a Link to an anchor that we'll generate later to let
                        //users jump to a Class/Property definition
                        String qname = context.NodeFormatter.Format(r["property"]);
                        context.HtmlWriter.AddAttribute("href", "#" + qname);
                        context.HtmlWriter.AddAttribute(HtmlTextWriterAttribute.Class, this.CssClassUri);
                        context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.A);
                        context.HtmlWriter.WriteEncodedText(qname);
                        context.HtmlWriter.RenderEndTag();

                        if (i < rs.Count - 1)
                        {
                            context.HtmlWriter.WriteEncodedText(" , ");
                        }
                    }
                }
                else
                {
                    throw new RdfOutputException("Tried to make a SPARQL Query to find Properties in the Schema but an unexpected Query Result was returned");
                }
            }
            catch (RdfQueryException queryEx)
            {
                throw new RdfOutputException("Tried to make a SPARQL Query to find Properties in the Schema but a Query Error occurred", queryEx);
            }

            context.HtmlWriter.RenderEndTag();
#if !NO_WEB
            context.HtmlWriter.WriteLine();
#endif

            //Show details for each class
            context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.H3);
            context.HtmlWriter.WriteEncodedText("Classes");
            context.HtmlWriter.RenderEndTag();
#if !NO_WEB
            context.HtmlWriter.WriteLine();
#endif

            //Now create the URI Nodes we need for the next stage of Output
            IUriNode rdfsDomain = context.Graph.CreateUriNode(new Uri(NamespaceMapper.RDFS + "domain"));
            IUriNode rdfsRange = context.Graph.CreateUriNode(new Uri(NamespaceMapper.RDFS + "range"));
            IUriNode rdfsSubClassOf = context.Graph.CreateUriNode(new Uri(NamespaceMapper.RDFS + "subClassOf"));
            IUriNode rdfsSubPropertyOf = context.Graph.CreateUriNode(new Uri(NamespaceMapper.RDFS + "subPropertyOf"));
            IUriNode owlDisjointClass = context.Graph.CreateUriNode(new Uri(NamespaceMapper.OWL + "disjointWith"));
            IUriNode owlEquivalentClass = context.Graph.CreateUriNode(new Uri(NamespaceMapper.OWL + "equivalentClass"));
            IUriNode owlEquivalentProperty = context.Graph.CreateUriNode(new Uri(NamespaceMapper.OWL + "equivalentProperty"));
            IUriNode owlInverseProperty = context.Graph.CreateUriNode(new Uri(NamespaceMapper.OWL + "inverseOf"));

            //Alter our previous getClasses query to get additional details
            getClasses.CommandText = "SELECT ?class (SAMPLE(?label) AS ?classLabel) (SAMPLE(?description) AS ?classDescription) WHERE { { ?class a rdfs:Class } UNION { ?class a owl:Class } FILTER(ISURI(?class)) OPTIONAL { ?class rdfs:label ?label } OPTIONAL { ?class rdfs:comment ?description } } GROUP BY ?class ORDER BY ?class";
            try
            {
                results = context.Graph.ExecuteQuery(getClasses);
                if (results is SparqlResultSet)
                {
                    foreach (SparqlResult r in (SparqlResultSet)results)
                    {
                        String qname = context.NodeFormatter.Format(r["class"]);

                        //Use a <div> for each Class
                        context.HtmlWriter.AddAttribute(HtmlTextWriterAttribute.Class, this.CssClassBox);
                        context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.Div);

                        //Add the Anchor to which earlier Class summary links to
                        context.HtmlWriter.AddAttribute(HtmlTextWriterAttribute.Name, qname);
                        context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.A);
                        context.HtmlWriter.RenderEndTag();

                        //Show Basic Class Information
                        context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.H4);
                        context.HtmlWriter.WriteEncodedText("Class: " + qname);
                        context.HtmlWriter.RenderEndTag();

                        //Show "Local Name - Label"
                        context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.Em);
                        if (TurtleSpecsHelper.IsValidQName(qname))
                        {
                            context.HtmlWriter.WriteEncodedText(qname);
                        }
                        else
                        {
                            Uri temp = new Uri(qname, UriKind.RelativeOrAbsolute);
                            if (!temp.Fragment.Equals(String.Empty))
                            {
                                context.HtmlWriter.WriteEncodedText(temp.Fragment);
                            } 
                            else 
                            {
#if !SILVERLIGHT
                                context.HtmlWriter.WriteEncodedText(temp.Segments.Last());
#else
                                context.HtmlWriter.WriteEncodedText(temp.Segments().Last());
#endif
                            }
                        }
                        context.HtmlWriter.RenderEndTag();
                        if (r.HasValue("classLabel"))
                        {
                            if (r["classLabel"] != null && r["classLabel"].NodeType == NodeType.Literal)
                            {
                                context.HtmlWriter.WriteEncodedText(" - ");
                                context.HtmlWriter.WriteEncodedText(((ILiteralNode)r["classLabel"]).Value);
                            }
                        }
                        context.HtmlWriter.WriteLine();
                        context.HtmlWriter.WriteBreak();
#if !NO_WEB
                        context.HtmlWriter.WriteLine();
#endif
                        //Output further information about the class
                        IEnumerable<Triple> ts;

                        //Output any Subclasses
                        ts = context.Graph.GetTriplesWithSubjectPredicate(rdfsSubClassOf, r["class"]);
                        this.GenerateCaptionedInformation(context, "Has Sub Classes", ts, t => t.Object);

                        //Output Properties which have this as domain/range
                        ts = context.Graph.GetTriplesWithPredicateObject(rdfsDomain, r["class"]);
                        this.GenerateCaptionedInformation(context, "Properties Include", ts, t => t.Subject);
                        ts = context.Graph.GetTriplesWithPredicateObject(rdfsRange, r["class"]);
                        this.GenerateCaptionedInformation(context, "Used With", ts, t => t.Subject);

                        //Output any Equivalent Classes
                        ts = context.Graph.GetTriplesWithSubjectPredicate(r["class"], owlEquivalentClass).Concat(context.Graph.GetTriplesWithPredicateObject(owlEquivalentClass, r["class"]));
                        this.GenerateCaptionedInformation(context, "Equivalent Classes", ts, t => t.Subject.Equals(r["class"]) ? t.Object : t.Subject);
                        //Output any Disjoint Classes
                        ts = context.Graph.GetTriplesWithSubjectPredicate(r["class"], owlDisjointClass).Concat(context.Graph.GetTriplesWithPredicateObject(owlDisjointClass, r["class"]));
                        this.GenerateCaptionedInformation(context, "Disjoint Classes", ts, t => t.Subject.Equals(r["class"]) ? t.Object : t.Subject);

                        //Show the Class Description
                        if (r.HasValue("classDescription"))
                        {
                            if (r["classDescription"] != null && r["classDescription"].NodeType == NodeType.Literal)
                            {
                                context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.P);
                                context.HtmlWriter.Write(((ILiteralNode)r["classDescription"]).Value);
                                context.HtmlWriter.RenderEndTag();
                            }
                        }

                        //End the </div> for the Class
                        context.HtmlWriter.RenderEndTag();
#if !NO_WEB
                        context.HtmlWriter.WriteLine();
#endif
                    }
                }
                else
                {
                    throw new RdfOutputException("Tried to make a SPARQL Query to get Class Information from the Schema but an unexpected Query Result was returned");
                }
            }
            catch (RdfQueryException queryEx)
            {
                throw new RdfOutputException("Tried to make a SPARQL Query to get Class Information from the Schema but a Query Error occurred", queryEx);
            }

            //Show details for each property
            context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.H3);
            context.HtmlWriter.WriteEncodedText("Properties");
            context.HtmlWriter.RenderEndTag();
#if !NO_WEB
            context.HtmlWriter.WriteLine();
#endif

            //Alter our previous getClasses query to get additional details
            getProperties.CommandText = "SELECT ?property (SAMPLE(?label) AS ?propertyLabel) (SAMPLE(?description) AS ?propertyDescription) WHERE { { ?property a rdf:Property } UNION { ?property a owl:ObjectProperty } UNION { ?property a owl:DatatypeProperty } FILTER(ISURI(?property)) OPTIONAL { ?property rdfs:label ?label } OPTIONAL { ?property rdfs:comment ?description } } GROUP BY ?property ORDER BY ?property";
            try
            {
                results = context.Graph.ExecuteQuery(getProperties);
                if (results is SparqlResultSet)
                {
                    foreach (SparqlResult r in (SparqlResultSet)results)
                    {
                        String qname = context.NodeFormatter.Format(r["property"]);

                        //Use a <div> for each Property
                        context.HtmlWriter.AddAttribute(HtmlTextWriterAttribute.Class, this.CssClassBox);
                        context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.Div);

                        //Add the Anchor to which earlier Property summary links to
                        context.HtmlWriter.AddAttribute(HtmlTextWriterAttribute.Name, qname);
                        context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.A);
                        context.HtmlWriter.RenderEndTag();

                        //Show Basic Property Information
                        context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.H4);
                        context.HtmlWriter.WriteEncodedText("Property: " + qname);
                        context.HtmlWriter.RenderEndTag();

                        //Show "Local Name - Label"
                        context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.Em);
                        if (TurtleSpecsHelper.IsValidQName(qname))
                        {
                            context.HtmlWriter.WriteEncodedText(qname);
                        }
                        else
                        {
                            Uri temp = new Uri(qname, UriKind.RelativeOrAbsolute);
                            if (!temp.Fragment.Equals(String.Empty))
                            {
                                context.HtmlWriter.WriteEncodedText(temp.Fragment);
                            }
                            else
                            {
#if !SILVERLIGHT
                                context.HtmlWriter.WriteEncodedText(temp.Segments.Last());
#else
                                context.HtmlWriter.WriteEncodedText(temp.Segments().Last());
#endif                        
                            }
                        }
                        context.HtmlWriter.RenderEndTag();
                        if (r.HasValue("propertyLabel"))
                        {
                            if (r["propertyLabel"] != null && r["propertyLabel"].NodeType == NodeType.Literal)
                            {
                                context.HtmlWriter.WriteEncodedText(" - ");
                                context.HtmlWriter.WriteEncodedText(((ILiteralNode)r["propertyLabel"]).Value);
                            }
                        }
                        context.HtmlWriter.WriteLine();
                        context.HtmlWriter.WriteBreak();
#if !NO_WEB
                        context.HtmlWriter.WriteLine();
#endif
                        //Output further information about the property
                        IEnumerable<Triple> ts;

                        //Output any Subproperties
                        ts = context.Graph.GetTriplesWithSubjectPredicate(rdfsSubPropertyOf, r["property"]);
                        this.GenerateCaptionedInformation(context, "Has Sub Properties", ts, t => t.Object);

                        //Output Domain and Range
                        ts = context.Graph.GetTriplesWithSubjectPredicate(r["property"], rdfsDomain);
                        this.GenerateCaptionedInformation(context, "Has Domain", ts, t => t.Object);
                        ts = context.Graph.GetTriplesWithSubjectPredicate(r["property"], rdfsRange);
                        this.GenerateCaptionedInformation(context, "Has Range", ts, t => t.Object);

                        //Output any Equivalent Properties
                        ts = context.Graph.GetTriplesWithSubjectPredicate(r["property"], owlEquivalentProperty).Concat(context.Graph.GetTriplesWithPredicateObject(owlEquivalentProperty, r["property"]));
                        this.GenerateCaptionedInformation(context, "Equivalent Properties", ts, t => t.Subject.Equals(r["property"]) ? t.Object : t.Subject);
                        //Output any Disjoint Classes
                        ts = context.Graph.GetTriplesWithSubjectPredicate(r["property"], owlInverseProperty).Concat(context.Graph.GetTriplesWithPredicateObject(owlInverseProperty, r["property"]));
                        this.GenerateCaptionedInformation(context, "Inverse Property", ts, t => t.Subject.Equals(r["property"]) ? t.Object : t.Subject);

                        //Show the Property Description
                        if (r.HasValue("propertyDescription"))
                        {
                            if (r["propertyDescription"] != null && r["propertyDescription"].NodeType == NodeType.Literal)
                            {
                                context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.P);
                                context.HtmlWriter.Write(((ILiteralNode)r["propertyDescription"]).Value);
                                context.HtmlWriter.RenderEndTag();
                            }
                        }

                        //End the </div> for the Property
                        context.HtmlWriter.RenderEndTag();
#if !NO_WEB
                        context.HtmlWriter.WriteLine();
#endif
                    }
                }
                else
                {
                    throw new RdfOutputException("Tried to make a SPARQL Query to get Property Information from the Schema but an unexpected Query Result was returned");
                }
            }
            catch (RdfQueryException queryEx)
            {
                throw new RdfOutputException("Tried to make a SPARQL Query to get Property Information from the Schema but a Query Error occurred", queryEx);
            }


            //End of Page
            context.HtmlWriter.RenderEndTag(); //End Body
            context.HtmlWriter.RenderEndTag(); //End Html
        }

        private void GenerateCaptionedInformation(HtmlWriterContext context, String caption, IEnumerable<Triple> ts, Func<Triple,INode> displaySelector)
        {
            if (ts.Any())
            {
                context.HtmlWriter.AddStyleAttribute("width", "90%");
                context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.P);
                context.HtmlWriter.WriteLine();
                context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.Strong);
                context.HtmlWriter.WriteEncodedText(caption + ": ");
                context.HtmlWriter.RenderEndTag();
                context.HtmlWriter.WriteLine();
                foreach (Triple t in ts.OrderBy(displaySelector))
                {
                    String qname = context.NodeFormatter.Format(displaySelector(t));
                    context.HtmlWriter.AddAttribute(HtmlTextWriterAttribute.Href, "#" + qname);
                    context.HtmlWriter.AddAttribute(HtmlTextWriterAttribute.Class, this.CssClassUri);
                    context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.A);
                    context.HtmlWriter.WriteEncodedText(qname);
                    context.HtmlWriter.RenderEndTag();
                    context.HtmlWriter.Write(' ');
                }
                context.HtmlWriter.RenderEndTag();
#if !NO_WEB
                context.HtmlWriter.WriteLine();
#endif
            }
        }

        /// <summary>
        /// Helper method for raising the <see cref="Warning">Warning</see> event
        /// </summary>
        /// <param name="message">Warning Message</param>
        private void RaiseWarning(String message)
        {
            RdfWriterWarning d = this.Warning;
            if (d != null)
            {
                d(message);
            }
        }

        /// <summary>
        /// Event which is raised if there is a non-fatal error with the RDF being output
        /// </summary>
        public event RdfWriterWarning Warning;

        /// <summary>
        /// Gets the String representation of the writer which is a description of the syntax it produces
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "XHTML (Human Readable Schema for Ontologies/Vocabularies)";
        }
    }
}
