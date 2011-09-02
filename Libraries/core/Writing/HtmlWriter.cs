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
using VDS.RDF.Writing.Contexts;

namespace VDS.RDF.Writing
{
    /// <summary>
    /// Class for saving RDF Graphs to a XHTML Table format with the actual Triples embedded as RDFa
    /// </summary>
    /// <remarks>
    /// <para>
    /// Since not all Triples can be embedded into XHTML those Triples will not have RDFa generated for them but all Triples will be shown in a human readable format.  Triples that can be serialized are roughly equivalent to anything that can be serialized in Turtle i.e. URI/BNode subject, URI predicates and URI/BNode/Literal object.
    /// </para>
    /// <para>
    /// If you encode Triples which have values datatyped as XML Literals with this writer then round-trip Graph equality is not guaranteed as the RDFa parser will add appropriate Namespace declarations to elements as required by the specification
    /// </para>
    /// </remarks>
    public class HtmlWriter
        : BaseHtmlWriter, IRdfWriter, INamespaceWriter
    {
        private INamespaceMapper _defaultNamespaces = new NamespaceMapper();

        /// <summary>
        /// Gets/Sets the Default Namespaces to use for writers
        /// </summary>
        public INamespaceMapper DefaultNamespaces
        {
            get
            {
                return this._defaultNamespaces;
            }
            set
            {
                this._defaultNamespaces = value;
            }
        }

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
                g.NamespaceMap.Import(this._defaultNamespaces);
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
            //Page Header
            context.HtmlWriter.Write("<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML+RDFa 1.0//EN\" \"http://www.w3.org/MarkUp/DTD/xhtml-rdfa-1.dtd\">");
            context.HtmlWriter.WriteLine();
            context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.Html);
            context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.Head);
            context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.Title);
            context.HtmlWriter.WriteEncodedText("RDF Graph");
            if (context.Graph.BaseUri != null)
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
            //TODO: Add <meta> for charset?
            context.HtmlWriter.RenderEndTag();
#if !NO_WEB
            context.HtmlWriter.WriteLine();
#endif

            //Start Body
            context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.Body);

            //Title
            context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.H3);
            context.HtmlWriter.WriteEncodedText("RDF Graph");
            if (context.Graph.BaseUri != null)
            {
                context.HtmlWriter.WriteEncodedText(" - " + context.Graph.BaseUri.ToString());
            }
            context.HtmlWriter.RenderEndTag();
#if !NO_WEB
            context.HtmlWriter.WriteLine();
#endif

            //Create a Table for the Graph
            context.HtmlWriter.AddAttribute(HtmlTextWriterAttribute.Width, "100%");
            context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.Table);

            //Create a Table Header
            context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.Thead);
            context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.Tr);
            context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.Th);
            context.HtmlWriter.WriteEncodedText("Subject");
            context.HtmlWriter.RenderEndTag();
            context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.Th);
            context.HtmlWriter.WriteEncodedText("Predicate");
            context.HtmlWriter.RenderEndTag();
            context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.Th);
            context.HtmlWriter.WriteEncodedText("Object");
            context.HtmlWriter.RenderEndTag();
            context.HtmlWriter.RenderEndTag();
            context.HtmlWriter.RenderEndTag();
#if !NO_WEB
            context.HtmlWriter.WriteLine();
#endif

            //Create a Table Body for the Triple
            context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.Tbody);

            TripleCollection triplesDone = new TripleCollection();
            foreach (INode subj in context.Graph.Triples.SubjectNodes)
            {
                IEnumerable<Triple> ts = context.Graph.GetTriplesWithSubject(subj);

                //Start a Row
                context.HtmlWriter.AddAttribute(HtmlTextWriterAttribute.Valign, "top");
                context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.Tr);

                //Then a Column for the Subject which spans the correct number of Rows
                context.HtmlWriter.AddAttribute(HtmlTextWriterAttribute.Rowspan, ts.Count().ToString());

                context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.Td);
#if !NO_WEB
                context.HtmlWriter.WriteLine();
#endif
                //For each Subject add an anchor if it can be reduced to a QName
                if (subj.NodeType == NodeType.Uri)
                {
                    String qname;
                    if (context.QNameMapper.ReduceToQName(subj.ToString(), out qname))
                    {
                        if (!qname.EndsWith(":"))
                        {
                            qname = qname.Substring(qname.IndexOf(':') + 1);
                            context.HtmlWriter.AddAttribute(HtmlTextWriterAttribute.Name, qname);
                            context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.A);
                            context.HtmlWriter.RenderEndTag();
                        }
                    }
                }

                this.GenerateNodeOutput(context, subj);
#if !NO_WEB
                context.HtmlWriter.WriteLine();
#endif
                context.HtmlWriter.RenderEndTag();
#if !NO_WEB
                context.HtmlWriter.WriteLine();
#endif

                bool firstPred = true;
                foreach (Triple t in ts)
                {
                    if (triplesDone.Contains(t)) continue;
                    if (!firstPred)
                    {
                        //If not the first Triple start a new row
                        context.HtmlWriter.AddAttribute(HtmlTextWriterAttribute.Valign, "top");
                        context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.Tr);
                    }

                    //Then a Column for the Predicate
                    IEnumerable<Triple> predTriples = context.Graph.GetTriplesWithSubjectPredicate(t.Subject, t.Predicate);
                    context.HtmlWriter.AddAttribute(HtmlTextWriterAttribute.Rowspan, predTriples.Count().ToString());
                    context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.Td);
#if !NO_WEB
                    context.HtmlWriter.WriteLine();
#endif
                    this.GenerateNodeOutput(context, t.Predicate);
#if !NO_WEB
                    context.HtmlWriter.WriteLine();
#endif
                    context.HtmlWriter.RenderEndTag();
#if !NO_WEB
                    context.HtmlWriter.WriteLine();
#endif

                    //Then we write out all the Objects
                    bool firstObj = true;
                    foreach (Triple predTriple in predTriples)
                    {
                        if (triplesDone.Contains(predTriple)) continue;
                        if (!firstObj)
                        {
                            //If not the first Triple start a new row
                            context.HtmlWriter.AddAttribute(HtmlTextWriterAttribute.Valign, "top");
                            context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.Tr);
                        }

                        //Object Column
                        context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.Td);
#if !NO_WEB
                        context.HtmlWriter.WriteLine();
#endif
                        this.GenerateNodeOutput(context, predTriple.Object, predTriple);
#if !NO_WEB
                        context.HtmlWriter.WriteLine();
#endif
                        context.HtmlWriter.RenderEndTag();
#if !NO_WEB
                        context.HtmlWriter.WriteLine();
#endif

                        //End of Row
                        context.HtmlWriter.RenderEndTag();
#if !NO_WEB
                        context.HtmlWriter.WriteLine();
#endif
                        firstObj = false;

                        triplesDone.Add(predTriple);
                    }
                    firstPred = false;
                }
            }


            //End Table Body
            context.HtmlWriter.RenderEndTag();

            //End Table
            context.HtmlWriter.RenderEndTag();

            //End of Page
            context.HtmlWriter.RenderEndTag(); //End Body
            context.HtmlWriter.RenderEndTag(); //End Html
        }

        /// <summary>
        /// Generates Output for a given Node
        /// </summary>
        /// <param name="context">Writer Context</param>
        /// <param name="n">Node</param>
        private void GenerateNodeOutput(HtmlWriterContext context, INode n)
        {
            this.GenerateNodeOutput(context, n, null);
        }

        /// <summary>
        /// Generates Output for a given Node
        /// </summary>
        /// <param name="context">Writer Context</param>
        /// <param name="n">Node</param>
        /// <param name="t">Triple being written</param>
        private void GenerateNodeOutput(HtmlWriterContext context, INode n, Triple t)
        {                
            //Embed RDFa on the Node Output
            bool rdfASerializable = false;
            if (t != null)
            {
                if (t.Predicate.NodeType == NodeType.Uri)
                {
                    //Use @about to specify the Subject
                    if (t.Subject.NodeType == NodeType.Uri)
                    {
                        rdfASerializable = true;
                        context.HtmlWriter.AddAttribute("about", context.UriFormatter.FormatUri(t.Subject.ToString()));
                    }
                    else if (t.Subject.NodeType == NodeType.Blank)
                    {
                        rdfASerializable = true;
                        context.HtmlWriter.AddAttribute("about", "[" + t.Subject.ToString() + "]");
                    }
                    else
                    {
                        this.RaiseWarning("Cannot serialize a Triple since the Subject is not a URI/Blank Node: " + t.Subject.ToString());
                    }

                    //Then if we can serialize this Triple we serialize the Predicate
                    if (rdfASerializable)
                    {
                        //Get the CURIE for the Predicate
                        String curie;
                        String tempNamespace;
                        if (context.QNameMapper.ReduceToQName(t.Predicate.ToString(), out curie, out tempNamespace))
                        {
                            //Extract the Namespace and make sure it's registered on this Attribute
                            String ns = curie.Substring(0, curie.IndexOf(':'));
                            context.HtmlWriter.AddAttribute("xmlns:" + ns, context.UriFormatter.FormatUri(context.QNameMapper.GetNamespaceUri(ns)));
                        }
                        else
                        {
                            this.RaiseWarning("Cannot serialize a Triple since the Predicate cannot be reduced to a QName: " + t.Predicate.ToString());
                            rdfASerializable = false;
                        }

                        if (rdfASerializable)
                        {
                            switch (t.Object.NodeType)
                            {
                                case NodeType.Blank:
                                case NodeType.Uri:
                                    //If the Object is a URI or a Blank then we specify the predicate with @rel
                                    context.HtmlWriter.AddAttribute("rel", curie);
                                    break;

                                case NodeType.Literal:
                                    //If the Object is a Literal we specify the predicate with @property
                                    context.HtmlWriter.AddAttribute("property", curie);
                                    break;
                                default:
                                    this.RaiseWarning("Cannot serialize a Triple since the Object is not a URI/Blank/Literal Node: " + t.Object.ToString());
                                    rdfASerializable = false;
                                    break;
                            }
                        }
                    }
                }
                else
                {
                    this.RaiseWarning("Cannot serialize a Triple since the Predicate is not a URI Node: " + t.Predicate.ToString());
                }
            }

            String qname;
            switch (n.NodeType)
            {
                case NodeType.Blank:
                    if (rdfASerializable)
                    {
                        //Need to embed the CURIE for the BNode in the @resource attribute
                        context.HtmlWriter.AddAttribute("resource", "[" + n.ToString() + "]");
                    }

                    context.HtmlWriter.AddAttribute(HtmlTextWriterAttribute.Class, this.CssClassBlankNode);
                    context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.Span);
                    context.HtmlWriter.WriteEncodedText(n.ToString());
                    context.HtmlWriter.RenderEndTag();
                    break;

                case NodeType.Literal:
                    ILiteralNode lit = (ILiteralNode)n;
                    if (lit.DataType != null)
                    {
                        if (rdfASerializable)
                        {
                            //Need to embed the datatype in the @datatype attribute
                            String dtcurie, dtnamespace;
                            if (context.QNameMapper.ReduceToQName(lit.DataType.ToString(), out dtcurie, out dtnamespace))
                            {
                                //Extract the Namespace and make sure it's registered on this Attribute
                                String ns = dtcurie.Substring(0, dtcurie.IndexOf(':'));
                                context.HtmlWriter.AddAttribute("xmlns:" + ns, context.UriFormatter.FormatUri(context.QNameMapper.GetNamespaceUri(ns)));
                                context.HtmlWriter.AddAttribute("datatype", dtcurie);
                            }
                        }

                        context.HtmlWriter.AddAttribute(HtmlTextWriterAttribute.Class, this.CssClassLiteral);
                        context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.Span);
                        if (lit.DataType.ToString().Equals(Parsing.RdfSpecsHelper.RdfXmlLiteral))
                        {
                            context.HtmlWriter.Write(lit.Value);
                        }
                        else
                        {
                            context.HtmlWriter.WriteEncodedText(lit.Value);
                        }
                        context.HtmlWriter.RenderEndTag();

                        //Output the Datatype
                        context.HtmlWriter.WriteEncodedText("^^");
                        context.HtmlWriter.AddAttribute(HtmlTextWriterAttribute.Href, lit.DataType.ToString());
                        context.HtmlWriter.AddAttribute(HtmlTextWriterAttribute.Class, this.CssClassDatatype);
                        context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.A);
                        if (context.QNameMapper.ReduceToQName(lit.DataType.ToString(), out qname))
                        {
                            context.HtmlWriter.WriteEncodedText(qname);
                        }
                        else
                        {
                            context.HtmlWriter.WriteEncodedText(lit.DataType.ToString());
                        }
                        context.HtmlWriter.RenderEndTag();
                    }
                    else
                    {
                        if (rdfASerializable)
                        {
                            if (!lit.Language.Equals(String.Empty))
                            {
                                //Need to add the language as an xml:lang attribute
                                context.HtmlWriter.AddAttribute("xml:lang", lit.Language);
                            }
                        }
                        context.HtmlWriter.AddAttribute(HtmlTextWriterAttribute.Class, this.CssClassLiteral);
                        context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.Span);
                        context.HtmlWriter.WriteEncodedText(lit.Value);
                        context.HtmlWriter.RenderEndTag();
                        if (!lit.Language.Equals(String.Empty))
                        {
                            context.HtmlWriter.WriteEncodedText("@");
                            context.HtmlWriter.AddAttribute(HtmlTextWriterAttribute.Class, this.CssClassLangSpec);
                            context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.Span);
                            context.HtmlWriter.WriteEncodedText(lit.Language);
                            context.HtmlWriter.RenderEndTag();
                        }
                    }
                    break;

                case NodeType.GraphLiteral:
                    //Error
                    throw new RdfOutputException(WriterErrorMessages.GraphLiteralsUnserializable("HTML"));

                case NodeType.Uri:
                    if (rdfASerializable && !this.UriPrefix.Equals(String.Empty))
                    {
                        //If the URIs are being prefixed with something then we need to set the original
                        //URI in the resource attribute to generate the correct triple
                        context.HtmlWriter.AddAttribute("resource", n.ToString());
                    }

                    context.HtmlWriter.AddAttribute(HtmlTextWriterAttribute.Class, this.CssClassUri);
                    context.HtmlWriter.AddAttribute(HtmlTextWriterAttribute.Href, this.UriPrefix + n.ToString());
                    context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.A);
                    if (context.QNameMapper.ReduceToQName(n.ToString(), out qname))
                    {
                        context.HtmlWriter.WriteEncodedText(qname);
                    }
                    else
                    {
                        context.HtmlWriter.WriteEncodedText(n.ToString());
                    }
                    context.HtmlWriter.RenderEndTag();
                    break;

                default:
                    throw new RdfOutputException(WriterErrorMessages.UnknownNodeTypeUnserializable("HTML"));
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
            return "XHTML+RDFa";
        }
    }
}
