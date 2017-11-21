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
using System.IO;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Writing.Contexts;
using VDS.RDF.Writing.Formatting;
using System.Web.UI;

// TODO: Embed RDFa in the HTML Output

namespace VDS.RDF.Writing
{
    /// <summary>
    /// HTML Schema Writer is a HTML Writer which writes a human readable description of a Schema/Ontology
    /// </summary>
    public class HtmlSchemaWriter
        : BaseHtmlWriter, IRdfWriter
    {
        /// <summary>
        /// Saves the Graph to the given File as an XHTML Table with embedded RDFa
        /// </summary>
        /// <param name="g">Graph to save</param>
        /// <param name="filename">File to save to</param>
        public void Save(IGraph g, String filename)
        {
            using (var stream = File.Open(filename, FileMode.Create))
            {
                Save(g, new StreamWriter(stream, new UTF8Encoding(Options.UseBomForUtf8)));
            }
        }

        /// <summary>
        /// Saves the Result Set to the given Stream as an XHTML Table with embedded RDFa
        /// </summary>
        /// <param name="g">Graph to save</param>
        /// <param name="output">Stream to save to</param>
        public void Save(IGraph g, TextWriter output)
        {
            Save(g, output, false);
        }

        /// <inheritdoc/>
        public void Save(IGraph g, TextWriter output, bool leaveOpen)
        { 
            try
            {
                HtmlWriterContext context = new HtmlWriterContext(g, output);
                GenerateOutput(context);
                if (!leaveOpen) output.Close();
            }
            catch
            {
                if (!leaveOpen)
                {
                    try
                    {
                        output.Close();
                    }
                    catch
                    {
                        // No Catch Actions
                    }
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

            // Add the Namespaces we want to use later on
            context.QNameMapper.AddNamespace("owl", UriFactory.Create(NamespaceMapper.OWL));
            context.QNameMapper.AddNamespace("rdf", UriFactory.Create(NamespaceMapper.RDF));
            context.QNameMapper.AddNamespace("rdfs", UriFactory.Create(NamespaceMapper.RDFS));
            context.QNameMapper.AddNamespace("dc", UriFactory.Create("http://purl.org/dc/elements/1.1/"));
            context.QNameMapper.AddNamespace("dct", UriFactory.Create("http://purl.org/dc/terms/"));
            context.QNameMapper.AddNamespace("vann", UriFactory.Create("http://purl.org/vocab/vann/"));
            context.QNameMapper.AddNamespace("vs", UriFactory.Create("http://www.w3.org/2003/06/sw-vocab-status/ns#"));

            // Find the Node that represents the Schema Ontology
            // Assumes there is exactly one thing given rdf:type owl:Ontology
            IUriNode ontology = context.Graph.CreateUriNode(UriFactory.Create(NamespaceMapper.OWL + "Ontology"));
            IUriNode rdfType = context.Graph.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfType));
            IUriNode rdfsLabel = context.Graph.CreateUriNode(UriFactory.Create(NamespaceMapper.RDFS + "label"));
            INode ontoNode = context.Graph.GetTriplesWithPredicateObject(rdfType, ontology).Select(t => t.Subject).FirstOrDefault();
            INode ontoLabel = (ontoNode != null) ? context.Graph.GetTriplesWithSubjectPredicate(ontoNode, rdfsLabel).Select(t => t.Object).FirstOrDefault() : null;

            // Stuff for formatting
            // We'll use the Turtle Formatter to get nice QNames wherever possible
            context.NodeFormatter = new TurtleFormatter(context.QNameMapper);
            context.UriFormatter = (IUriFormatter)context.NodeFormatter;

            // Page Header
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
                context.HtmlWriter.WriteEncodedText(" - " + context.Graph.BaseUri.AbsoluteUri);
            }
            context.HtmlWriter.RenderEndTag();
            if (!Stylesheet.Equals(String.Empty))
            {
                context.HtmlWriter.AddAttribute(HtmlTextWriterAttribute.Href, Stylesheet);
                context.HtmlWriter.AddAttribute(HtmlTextWriterAttribute.Type, "text/css");
                context.HtmlWriter.AddAttribute(HtmlTextWriterAttribute.Rel, "stylesheet");
                context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.Link);
                context.HtmlWriter.RenderEndTag();
            }
            context.HtmlWriter.RenderEndTag();
            context.HtmlWriter.WriteLine();

            // Start Body
            context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.Body);

            // Title
            context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.H2);
            context.HtmlWriter.WriteEncodedText("Schema");
            if (ontoNode != null && ontoLabel != null)
            {
                context.HtmlWriter.WriteEncodedText(" - " + ontoLabel.ToSafeString());
            }
            else if (context.Graph.BaseUri != null)
            {
                context.HtmlWriter.WriteEncodedText(" - " + context.Graph.BaseUri.AbsoluteUri);
            }
            context.HtmlWriter.RenderEndTag();
            context.HtmlWriter.WriteLine();

            // Show the Description of the Schema (if any)
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

                            // Show rdfs:comment on the Ontology
                            if (ontoInfo.HasValue("description"))
                            {
                                INode descrip = ontoInfo["description"];
                                if (descrip.NodeType == NodeType.Literal)
                                {
                                    context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.P);
                                    context.HtmlWriter.Write(((ILiteralNode)descrip).Value);
                                    context.HtmlWriter.RenderEndTag();
                                    context.HtmlWriter.WriteLine();
                                }
                            }

                            // Show Author Information
                            if (ontoInfo.HasValue("creator"))
                            {
                                INode author = ontoInfo["creator"];
                                INode authorName = ontoInfo["creatorName"];
                                context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.P);
                                context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.Em);
                                context.HtmlWriter.WriteEncodedText("Schema created by ");
                                if (author.NodeType == NodeType.Uri)
                                {
                                    context.HtmlWriter.AddAttribute("href", ((IUriNode)author).Uri.AbsoluteUri);
                                    context.HtmlWriter.AddAttribute(HtmlTextWriterAttribute.Class, CssClassUri);
                                    context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.A);
                                }
                                switch (authorName.NodeType)
                                {
                                    case NodeType.Uri:
                                        context.HtmlWriter.WriteEncodedText(((IUriNode)authorName).Uri.AbsoluteUri);
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
                                context.HtmlWriter.WriteLine();
                            }

                            // Show the Namespace information for the Schema
                            if (ontoInfo.HasValue("nsPrefix"))
                            {
                                if (ontoInfo["nsPrefix"].NodeType == NodeType.Literal && ontoInfo["nsUri"].NodeType == NodeType.Uri)
                                {
                                    // Add this QName to the QName Mapper so we can get nice QNames later on
                                    String prefix = ((ILiteralNode)ontoInfo["nsPrefix"]).Value;
                                    context.QNameMapper.AddNamespace(prefix, ((IUriNode)ontoInfo["nsUri"]).Uri);

                                    context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.H4);
                                    context.HtmlWriter.WriteEncodedText("Preferred Namespace Definition");
                                    context.HtmlWriter.RenderEndTag();
                                    context.HtmlWriter.WriteLine();

                                    // Show human readable description of preferred Namespace Settings
                                    context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.P);
                                    context.HtmlWriter.WriteEncodedText("Preferred Namespace Prefix is ");
                                    context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.Strong);
                                    context.HtmlWriter.WriteEncodedText(prefix);
                                    context.HtmlWriter.RenderEndTag();
                                    context.HtmlWriter.WriteEncodedText(" and preferred Namespace URI is ");
                                    context.HtmlWriter.AddAttribute(HtmlTextWriterAttribute.Href, context.QNameMapper.GetNamespaceUri(prefix).AbsoluteUri);
                                    context.HtmlWriter.AddAttribute(HtmlTextWriterAttribute.Class, CssClassUri);
                                    context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.A);
                                    context.HtmlWriter.WriteEncodedText(context.QNameMapper.GetNamespaceUri(prefix).AbsoluteUri);
                                    context.HtmlWriter.RenderEndTag();
                                    context.HtmlWriter.RenderEndTag();

                                    // RDF/XML Syntax
                                    context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.H5);
                                    context.HtmlWriter.WriteEncodedText("RDF/XML Syntax");
                                    context.HtmlWriter.RenderEndTag();
                                    context.HtmlWriter.WriteLine();
                                    context.HtmlWriter.AddStyleAttribute(HtmlTextWriterStyle.Width, "90%");
                                    context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.Pre);
                                    int currIndent = context.HtmlWriter.Indent;
                                    context.HtmlWriter.Indent = 0;
                                    context.HtmlWriter.WriteEncodedText("<?xml version=\"1.0\" charset=\"utf-8\"?>");
                                    context.HtmlWriter.WriteLine();
                                    context.HtmlWriter.WriteEncodedText("<rdf:RDF xmlns:rdf=\"" + NamespaceMapper.RDF + "\" xmlns:" + prefix + "=\"" + context.UriFormatter.FormatUri(context.QNameMapper.GetNamespaceUri(prefix)) + "\">");
                                    context.HtmlWriter.WriteLine();
                                    context.HtmlWriter.WriteEncodedText("   <!-- Your RDF here... -->");
                                    context.HtmlWriter.WriteLine();
                                    context.HtmlWriter.WriteEncodedText("</rdf:RDF>");
                                    context.HtmlWriter.Indent = currIndent;
                                    context.HtmlWriter.RenderEndTag();
                                    context.HtmlWriter.WriteLine();

                                    // Turtle/N3 Syntax
                                    context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.H5);
                                    context.HtmlWriter.WriteEncodedText("Turtle/N3 Syntax");
                                    context.HtmlWriter.RenderEndTag();
                                    context.HtmlWriter.WriteLine();
                                    context.HtmlWriter.AddStyleAttribute(HtmlTextWriterStyle.Width, "90%");
                                    context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.Pre);
                                    currIndent = context.HtmlWriter.Indent;
                                    context.HtmlWriter.Indent = 0;
                                    context.HtmlWriter.WriteEncodedText("@prefix " + prefix + ": <" + context.UriFormatter.FormatUri(context.QNameMapper.GetNamespaceUri(prefix)) + "> .");
                                    context.HtmlWriter.Indent = currIndent;
                                    context.HtmlWriter.RenderEndTag();
                                    context.HtmlWriter.WriteLine();

                                    // SPARQL Syntax
                                    context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.H5);
                                    context.HtmlWriter.WriteEncodedText("SPARQL Syntax");
                                    context.HtmlWriter.RenderEndTag();
                                    context.HtmlWriter.WriteLine();
                                    context.HtmlWriter.AddStyleAttribute(HtmlTextWriterStyle.Width, "90%");
                                    context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.Pre);
                                    currIndent = context.HtmlWriter.Indent;
                                    context.HtmlWriter.Indent = 0;
                                    context.HtmlWriter.WriteEncodedText("PREFIX " + prefix + ": <" + context.UriFormatter.FormatUri(context.QNameMapper.GetNamespaceUri(prefix)) + ">");
                                    context.HtmlWriter.Indent = currIndent;
                                    context.HtmlWriter.RenderEndTag();
                                    context.HtmlWriter.WriteLine();
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

            SparqlParameterizedString getPropertyRanges = new SparqlParameterizedString();
            getPropertyRanges.Namespaces = new NamespaceMapper();
            getPropertyRanges.Namespaces.AddNamespace("owl", UriFactory.Create(NamespaceMapper.OWL));
            getPropertyRanges.CommandText = "SELECT ?range WHERE { { @property rdfs:range ?range . FILTER(ISURI(?range)) } UNION { @property rdfs:range ?union . ?union owl:unionOf ?ranges . { ?ranges rdf:first ?range } UNION { ?ranges rdf:rest+/rdf:first ?range } } }";
            SparqlParameterizedString getPropertyDomains = new SparqlParameterizedString();
            getPropertyDomains.Namespaces = getPropertyRanges.Namespaces;
            getPropertyDomains.CommandText = "SELECT ?domain WHERE { { @property rdfs:domain ?domain . FILTER(ISURI(?domain)) } UNION { @property rdfs:domain ?union . ?union owl:unionOf ?domains . { ?domains rdf:first ?domain } UNION { ?domains rdf:rest+/rdf:first ?domain } } }";

            // Show lists of all Classes and Properties in the Schema
            context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.H4);
            context.HtmlWriter.WriteEncodedText("Class and Property Summary");
            context.HtmlWriter.RenderEndTag();
            context.HtmlWriter.WriteLine();

            context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.P);
            context.HtmlWriter.WriteEncodedText("This Schema defines the following classes:");
            context.HtmlWriter.RenderEndTag();
            context.HtmlWriter.WriteLine();
            context.HtmlWriter.AddStyleAttribute("width", "90%");
            context.HtmlWriter.AddAttribute(HtmlTextWriterAttribute.Class, CssClassBox);
            context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.P);

            // Get the Classes and Display
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

                        // Get the QName and output a Link to an anchor that we'll generate later to let
                        // users jump to a Class/Property definition
                        String qname = context.NodeFormatter.Format(r["class"]);
                        context.HtmlWriter.AddAttribute("href", "#" + qname);
                        context.HtmlWriter.AddAttribute(HtmlTextWriterAttribute.Class, CssClassUri);
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
            context.HtmlWriter.WriteLine();

            context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.P);
            context.HtmlWriter.WriteEncodedText("This Schema defines the following properties:");
            context.HtmlWriter.RenderEndTag();
            context.HtmlWriter.WriteLine();
            context.HtmlWriter.AddStyleAttribute("width", "90%");
            context.HtmlWriter.AddAttribute(HtmlTextWriterAttribute.Class, CssClassBox);
            context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.P);

            // Get the Properties and Display
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

                        // Get the QName and output a Link to an anchor that we'll generate later to let
                        // users jump to a Class/Property definition
                        String qname = context.NodeFormatter.Format(r["property"]);
                        context.HtmlWriter.AddAttribute("href", "#" + qname);
                        context.HtmlWriter.AddAttribute(HtmlTextWriterAttribute.Class, CssClassUri);
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
            context.HtmlWriter.WriteLine();

            // Show details for each class
            context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.H3);
            context.HtmlWriter.WriteEncodedText("Classes");
            context.HtmlWriter.RenderEndTag();
            context.HtmlWriter.WriteLine();

            // Now create the URI Nodes we need for the next stage of Output
            IUriNode rdfsDomain = context.Graph.CreateUriNode(UriFactory.Create(NamespaceMapper.RDFS + "domain"));
            IUriNode rdfsRange = context.Graph.CreateUriNode(UriFactory.Create(NamespaceMapper.RDFS + "range"));
            IUriNode rdfsSubClassOf = context.Graph.CreateUriNode(UriFactory.Create(NamespaceMapper.RDFS + "subClassOf"));
            IUriNode rdfsSubPropertyOf = context.Graph.CreateUriNode(UriFactory.Create(NamespaceMapper.RDFS + "subPropertyOf"));
            IUriNode owlDisjointClass = context.Graph.CreateUriNode(UriFactory.Create(NamespaceMapper.OWL + "disjointWith"));
            IUriNode owlEquivalentClass = context.Graph.CreateUriNode(UriFactory.Create(NamespaceMapper.OWL + "equivalentClass"));
            IUriNode owlEquivalentProperty = context.Graph.CreateUriNode(UriFactory.Create(NamespaceMapper.OWL + "equivalentProperty"));
            IUriNode owlInverseProperty = context.Graph.CreateUriNode(UriFactory.Create(NamespaceMapper.OWL + "inverseOf"));

            // Alter our previous getClasses query to get additional details
            getClasses.CommandText = "SELECT ?class (SAMPLE(?label) AS ?classLabel) (SAMPLE(?description) AS ?classDescription) WHERE { { ?class a rdfs:Class } UNION { ?class a owl:Class } FILTER(ISURI(?class)) OPTIONAL { ?class rdfs:label ?label } OPTIONAL { ?class rdfs:comment ?description } } GROUP BY ?class ORDER BY ?class";
            try
            {
                results = context.Graph.ExecuteQuery(getClasses);
                if (results is SparqlResultSet)
                {
                    foreach (SparqlResult r in (SparqlResultSet)results)
                    {
                        if (!r.HasValue("class")) continue;
                        String qname = context.NodeFormatter.Format(r["class"]);

                        // Use a <div> for each Class
                        context.HtmlWriter.AddAttribute(HtmlTextWriterAttribute.Class, CssClassBox);
                        context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.Div);

                        // Add the Anchor to which earlier Class summary links to
                        context.HtmlWriter.AddAttribute(HtmlTextWriterAttribute.Name, qname);
                        context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.A);
                        context.HtmlWriter.RenderEndTag();

                        // Show Basic Class Information
                        context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.H4);
                        context.HtmlWriter.WriteEncodedText("Class: " + qname);
                        context.HtmlWriter.RenderEndTag();

                        // Show "Local Name - Label"
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
                                context.HtmlWriter.WriteEncodedText(temp.Segments.Last());
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
                        context.HtmlWriter.WriteLine();
      
                        // Output further information about the class
                        IEnumerable<Triple> ts;

                        // Output any Subclasses
                        ts = context.Graph.GetTriplesWithSubjectPredicate(rdfsSubClassOf, r["class"]);
                        GenerateCaptionedInformation(context, "Has Sub Classes", ts, t => t.Object);

                        // Output Properties which have this as domain/range
                        ts = context.Graph.GetTriplesWithPredicateObject(rdfsDomain, r["class"]);
                        GenerateCaptionedInformation(context, "Properties Include", ts, t => t.Subject);
                        ts = context.Graph.GetTriplesWithPredicateObject(rdfsRange, r["class"]);
                        GenerateCaptionedInformation(context, "Used With", ts, t => t.Subject);

                        // Output any Equivalent Classes
                        ts = context.Graph.GetTriplesWithSubjectPredicate(r["class"], owlEquivalentClass).Concat(context.Graph.GetTriplesWithPredicateObject(owlEquivalentClass, r["class"]));
                        GenerateCaptionedInformation(context, "Equivalent Classes", ts, t => t.Subject.Equals(r["class"]) ? t.Object : t.Subject);
                        // Output any Disjoint Classes
                        ts = context.Graph.GetTriplesWithSubjectPredicate(r["class"], owlDisjointClass).Concat(context.Graph.GetTriplesWithPredicateObject(owlDisjointClass, r["class"]));
                        GenerateCaptionedInformation(context, "Disjoint Classes", ts, t => t.Subject.Equals(r["class"]) ? t.Object : t.Subject);

                        // Show the Class Description
                        if (r.HasValue("classDescription"))
                        {
                            if (r["classDescription"] != null && r["classDescription"].NodeType == NodeType.Literal)
                            {
                                context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.P);
                                context.HtmlWriter.Write(((ILiteralNode)r["classDescription"]).Value);
                                context.HtmlWriter.RenderEndTag();
                            }
                        }

                        // End the </div> for the Class
                        context.HtmlWriter.RenderEndTag();
                        context.HtmlWriter.WriteLine();
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

            // Show details for each property
            context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.H3);
            context.HtmlWriter.WriteEncodedText("Properties");
            context.HtmlWriter.RenderEndTag();
            context.HtmlWriter.WriteLine();

            // Alter our previous getProperties query to get additional details
            getProperties.CommandText = "SELECT ?property (SAMPLE(?label) AS ?propertyLabel) (SAMPLE(?description) AS ?propertyDescription) WHERE { { ?property a rdf:Property } UNION { ?property a owl:ObjectProperty } UNION { ?property a owl:DatatypeProperty } FILTER(ISURI(?property)) OPTIONAL { ?property rdfs:label ?label } OPTIONAL { ?property rdfs:comment ?description } } GROUP BY ?property ORDER BY ?property";
            try
            {
                results = context.Graph.ExecuteQuery(getProperties);
                if (results is SparqlResultSet)
                {
                    foreach (SparqlResult r in (SparqlResultSet)results)
                    {
                        if (!r.HasValue("property")) continue;
                        String qname = context.NodeFormatter.Format(r["property"]);

                        // Use a <div> for each Property
                        context.HtmlWriter.AddAttribute(HtmlTextWriterAttribute.Class, CssClassBox);
                        context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.Div);

                        // Add the Anchor to which earlier Property summary links to
                        context.HtmlWriter.AddAttribute(HtmlTextWriterAttribute.Name, qname);
                        context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.A);
                        context.HtmlWriter.RenderEndTag();

                        // Show Basic Property Information
                        context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.H4);
                        context.HtmlWriter.WriteEncodedText("Property: " + qname);
                        context.HtmlWriter.RenderEndTag();

                        // Show "Local Name - Label"
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
                                context.HtmlWriter.WriteEncodedText(temp.Segments.Last());
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
                        context.HtmlWriter.WriteLine();

                        // Output further information about the property
                        IEnumerable<Triple> ts;

                        // Output any Subproperties
                        ts = context.Graph.GetTriplesWithSubjectPredicate(rdfsSubPropertyOf, r["property"]);
                        GenerateCaptionedInformation(context, "Has Sub Properties", ts, t => t.Object);

                        // Output Domain and Range
                        // ts = context.Graph.GetTriplesWithSubjectPredicate(r["property"], rdfsDomain);
                        // this.GenerateCaptionedInformation(context, "Has Domain", ts, t => t.Object);
                        // ts = context.Graph.GetTriplesWithSubjectPredicate(r["property"], rdfsRange);
                        // this.GenerateCaptionedInformation(context, "Has Range", ts, t => t.Object);
                        getPropertyDomains.SetParameter("property", r["property"]);
                        GenerateCaptionedInformation(context, "Has Domain", context.Graph.ExecuteQuery(getPropertyDomains) as SparqlResultSet, "domain");
                        getPropertyRanges.SetParameter("property", r["property"]);
                        GenerateCaptionedInformation(context, "Has Range", context.Graph.ExecuteQuery(getPropertyRanges) as SparqlResultSet, "range");

                        // Output any Equivalent Properties
                        ts = context.Graph.GetTriplesWithSubjectPredicate(r["property"], owlEquivalentProperty).Concat(context.Graph.GetTriplesWithPredicateObject(owlEquivalentProperty, r["property"]));
                        GenerateCaptionedInformation(context, "Equivalent Properties", ts, t => t.Subject.Equals(r["property"]) ? t.Object : t.Subject);
                        // Output any Disjoint Classes
                        ts = context.Graph.GetTriplesWithSubjectPredicate(r["property"], owlInverseProperty).Concat(context.Graph.GetTriplesWithPredicateObject(owlInverseProperty, r["property"]));
                        GenerateCaptionedInformation(context, "Inverse Property", ts, t => t.Subject.Equals(r["property"]) ? t.Object : t.Subject);

                        // Show the Property Description
                        if (r.HasValue("propertyDescription"))
                        {
                            if (r["propertyDescription"] != null && r["propertyDescription"].NodeType == NodeType.Literal)
                            {
                                context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.P);
                                context.HtmlWriter.Write(((ILiteralNode)r["propertyDescription"]).Value);
                                context.HtmlWriter.RenderEndTag();
                            }
                        }

                        // End the </div> for the Property
                        context.HtmlWriter.RenderEndTag();
                        context.HtmlWriter.WriteLine();
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


            // End of Page
            context.HtmlWriter.RenderEndTag(); //End Body
            context.HtmlWriter.RenderEndTag(); //End Html
        }

        private void GenerateCaptionedInformation(HtmlWriterContext context, String caption, IEnumerable<Triple> ts, Func<Triple,INode> displaySelector)
        {
            GenerateCaptionedInformation(context, caption, ts.Select(t => displaySelector(t)));
        }

        private void GenerateCaptionedInformation(HtmlWriterContext context, String caption, SparqlResultSet results, String var)
        {
            if (results == null) return;
            GenerateCaptionedInformation(context, caption, results.Select(r => r[var]).Where(n => n != null));
        }

        private void GenerateCaptionedInformation(HtmlWriterContext context, String caption, IEnumerable<INode> ns)
        {
            if (ns.Any())
            {
                context.HtmlWriter.AddStyleAttribute("width", "90%");
                context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.P);
                context.HtmlWriter.WriteLine();
                context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.Strong);
                context.HtmlWriter.WriteEncodedText(caption + ": ");
                context.HtmlWriter.RenderEndTag();
                context.HtmlWriter.WriteLine();
                foreach (INode n in ns.OrderBy(x => x))
                {
                    String qname = context.NodeFormatter.Format(n);
                    context.HtmlWriter.AddAttribute(HtmlTextWriterAttribute.Href, "#" + qname);
                    context.HtmlWriter.AddAttribute(HtmlTextWriterAttribute.Class, CssClassUri);
                    context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.A);
                    context.HtmlWriter.WriteEncodedText(qname);
                    context.HtmlWriter.RenderEndTag();
                    context.HtmlWriter.Write(' ');
                }
                context.HtmlWriter.RenderEndTag();
                context.HtmlWriter.WriteLine();
            }
        }

        /// <summary>
        /// Helper method for raising the <see cref="Warning">Warning</see> event
        /// </summary>
        /// <param name="message">Warning Message</param>
        private void RaiseWarning(String message)
        {
            RdfWriterWarning d = Warning;
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
