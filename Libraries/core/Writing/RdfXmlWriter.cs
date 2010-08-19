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
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Writing.Contexts;

namespace VDS.RDF.Writing
{
    /// <summary>
    /// Class for generating RDF/XML Concrete Syntax
    /// </summary>
    /// <remarks>
    /// <para>
    /// Use this if you require fast writing and are not so bothered about the readibility of syntax produced, if you prefer readable syntax and can live with the very slow speeds (around 300 Triple/second) use the <see cref="RdfXmlWriter">RdfXmlWriter</see> or the <see cref="RdfXmlTreeWriter">RdfXmlTreeWriter</see>.
    /// </para>
    /// <para>
    /// This is a fast context.Writer based on the fast writing technique used in the other non-RDF/XML context.Writers.  While it is significantly faster than the existing RDF/XML context.Writers achieving a speed of around 25,000 Triples/second the syntax produced is not the 'prettiest'.  It uses various syntax compressions but since it doesn't generate output in an explicitly striped manner it cannot produce the nice striped syntax
    /// </para>
    /// </remarks>
    public class RdfXmlWriter : IRdfWriter, IPrettyPrintingWriter
    {
        private bool _prettyprint = true;

        /// <summary>
        /// Gets/Sets Pretty Print Mode for the Writer
        /// </summary>
        public bool PrettyPrintMode
        {
            get
            {
                return this._prettyprint;
            }
            set
            {
                this._prettyprint = value;
            }
        }

        /// <summary>
        /// Saves a Graph in RDF/XML syntax to the given File
        /// </summary>
        /// <param name="g">Graph to save</param>
        /// <param name="filename">Filename to save to</param>
        public void Save(IGraph g, string filename)
        {
            StreamWriter output = new StreamWriter(filename, false, Encoding.UTF8);
            this.Save(g, output);
        }

        /// <summary>
        /// Saves a Graph to an arbitrary output stream
        /// </summary>
        /// <param name="g">Graph to save</param>
        /// <param name="output">Stream to save to</param>
        public void Save(IGraph g, TextWriter output)
        {
            try
            {
                this.GenerateOutput(g, output);
                output.Close();
            }
            catch
            {
                try
                {
                    //Close the Output Stream
                    output.Close();
                }
                catch
                {
                    //No Catch actions here
                }
                throw;
            }
        }

        /// <summary>
        /// Internal method which generates the RDF/Json Output for a Graph
        /// </summary>
        /// <param name="g">Graph to save</param>
        /// <param name="output">Stream to save to</param>
        private void GenerateOutput(IGraph g, TextWriter output)
        {
            TripleCollection triplesDone = new TripleCollection();

            //Always force RDF Namespace to be correctly defined
            g.NamespaceMap.AddNamespace("rdf", new Uri(NamespaceMapper.RDF));

            //Create our Writer Context and start the XML Document
            RdfXmlWriterContext context = new RdfXmlWriterContext(g, output);
            context.Writer.WriteStartDocument();

            //Create the DOCTYPE declaration
            StringBuilder entities = new StringBuilder();
            String uri;
            entities.Append('\n');
            foreach (String prefix in context.NamespaceMap.Prefixes)
            {
                uri = context.NamespaceMap.GetNamespaceUri(prefix).ToString();
                if (!prefix.Equals(String.Empty))
                {
                    entities.AppendLine("\t<!ENTITY " + prefix + " '" + uri + "'>");
                }
            }
            context.Writer.WriteDocType("rdf:RDF", null, null, entities.ToString());

            //Create the rdf:RDF element
            context.Writer.WriteStartElement("rdf", "RDF", NamespaceMapper.RDF);
            foreach (String prefix in context.NamespaceMap.Prefixes)
            {
                if (prefix.Equals("rdf")) continue;

                if (!prefix.Equals(String.Empty))
                {
                    context.Writer.WriteStartAttribute("xmlns", prefix, null);
                    context.Writer.WriteRaw("&" + prefix + ";");
                    context.Writer.WriteEndAttribute();
                }
                else
                {
                    context.Writer.WriteStartAttribute("xmlns");
                    context.Writer.WriteRaw(context.NamespaceMap.GetNamespaceUri(prefix).ToString());
                    context.Writer.WriteEndAttribute();
                }
            }

            //Find the Collections
            Dictionary<INode, OutputRDFCollection> collections = new Dictionary<INode, OutputRDFCollection>();// WriterHelper.FindCollections(g, triplesDone);

            //Find the Type References
            Dictionary<INode, String> typerefs = this.FindTypeReferences(context, triplesDone);

            //Get the Triples as a Sorted List
            List<Triple> ts = context.Graph.Triples.Where(t => !triplesDone.Contains(t)).ToList();
            ts.Sort();

            //Variables we need to track our writing
            INode lastSubj, lastPred, lastObj;
            lastSubj = lastPred = lastObj = null;

            for (int i = 0; i < ts.Count; i++)
            {
                Triple t = ts[i];
                if (triplesDone.Contains(t)) continue; //Skip if already done

                if (lastSubj == null || !t.Subject.Equals(lastSubj))
                {
                    //Start a new set of Triples
                    if (lastSubj != null)
                    {
                        context.NamespaceMap.DecrementNesting();
                        context.Writer.WriteEndElement();
                    }
                    if (lastPred != null)
                    {
                        context.NamespaceMap.DecrementNesting();
                        context.Writer.WriteEndElement();
                    }

                    //Write out the Subject
                    //Validate Subject
                    //Use a Type Reference if applicable
                    context.NamespaceMap.IncrementNesting();
                    if (typerefs.ContainsKey(t.Subject))
                    {
                        String tref = typerefs[t.Subject];
                        if (tref.StartsWith(":"))
                        {
                            context.Writer.WriteStartElement(tref.Substring(1));
                        }
                        else if (tref.Contains(":"))
                        {
                            context.Writer.WriteStartElement(tref.Substring(0, tref.IndexOf(':')), tref.Substring(tref.IndexOf(':') + 1), null);
                        }
                        else
                        {
                            context.Writer.WriteStartElement(tref);
                        }
                    }
                    else
                    {
                        context.Writer.WriteStartElement("rdf", "Description", NamespaceMapper.RDF);
                    }
                    lastSubj = t.Subject;

                    //Apply appropriate attributes
                    switch (t.Subject.NodeType)
                    {
                        case NodeType.GraphLiteral:
                            throw new RdfOutputException(WriterErrorMessages.GraphLiteralsUnserializable("RDF/XML"));
                        case NodeType.Literal:
                            throw new RdfOutputException(WriterErrorMessages.LiteralSubjectsUnserializable("RDF/XML"));
                        case NodeType.Blank:
                            if (collections.ContainsKey(t.Subject))
                            {
                                throw new NotImplementedException("Collection Output not yet implemented");
                                //this.GenerateCollectionOutput(g, collections, t.Subject, subj, ref nextNamespaceID, tempNamespaces, doc);
                            }
                            else
                            {
                                context.Writer.WriteAttributeString("rdf", "nodeID", null, ((BlankNode)t.Subject).InternalID);
                            }
                            break;
                        case NodeType.Uri:
                            this.GenerateUriOutput(context, (UriNode)t.Subject, "rdf:about");
                            break;
                        default:
                            throw new RdfOutputException(WriterErrorMessages.UnknownNodeTypeUnserializable("RDF/XML"));
                    }

                    //Write the Predicate
                    context.NamespaceMap.IncrementNesting();
                    this.GeneratePredicateNode(context, t.Predicate);
                    lastPred = t.Predicate;
                    lastObj = null;
                }
                else if (lastPred == null || !t.Predicate.Equals(lastPred))
                {
                    if (lastPred != null)
                    {
                        context.NamespaceMap.DecrementNesting();
                        context.Writer.WriteEndElement();
                    }

                    //Write the Predicate
                    context.NamespaceMap.IncrementNesting();
                    this.GeneratePredicateNode(context, t.Predicate);
                    lastPred = t.Predicate;
                    lastObj = null;
                }

                //Write the Object
                if (lastObj != null)
                {
                    context.NamespaceMap.DecrementNesting();
                    context.Writer.WriteEndElement();
                }
                //Create an Object for the Object
                switch (t.Object.NodeType)
                {
                    case NodeType.Blank:
                        if (lastObj != null)
                        {
                            //Require a new Predicate
                            context.NamespaceMap.DecrementNesting();
                            context.Writer.WriteEndElement();
                            context.NamespaceMap.IncrementNesting();
                            this.GeneratePredicateNode(context, t.Predicate);
                        }

                        if (collections.ContainsKey(t.Object))
                        {
                            //Output a Collection
                            throw new NotImplementedException("Collection Output is not yet implemented");
                            //this.GenerateCollectionOutput(g, collections, t.Object, pred, ref nextNamespaceID, tempNamespaces, doc);
                        }
                        else
                        {
                            //Terminate the Blank Node triple by adding a rdf:nodeID attribute
                            context.Writer.WriteAttributeString("rdf", "nodeID", null, ((BlankNode)t.Subject).InternalID);
                        }

                        break;

                    case NodeType.GraphLiteral:
                        throw new RdfOutputException(WriterErrorMessages.GraphLiteralsUnserializable("RDF/XML"));

                    case NodeType.Literal:
                        LiteralNode lit = (LiteralNode)t.Object;

                        if (lastObj != null)
                        {
                            //Require a new Predicate
                            context.NamespaceMap.DecrementNesting();
                            context.Writer.WriteEndElement();
                            context.NamespaceMap.IncrementNesting();
                            this.GeneratePredicateNode(context, t.Predicate);
                        }

                        this.GenerateLiteralOutput(context, lit);

                        break;
                    case NodeType.Uri:

                        this.GenerateUriOutput(context, (UriNode)t.Object, "rdf:resource");

                        break;
                    default:
                        throw new RdfOutputException(WriterErrorMessages.UnknownNodeTypeUnserializable("RDF/XML"));
                }
                lastObj = t.Object;

                //Force a new Predicate Node
                context.NamespaceMap.DecrementNesting();
                context.Writer.WriteEndElement();
                lastPred = null;

                triplesDone.Add(t);
            }

            ////Check we haven't failed to output any collections
            //foreach (KeyValuePair<INode, OutputRDFCollection> pair in collections)
            //{
            //    if (pair.Value.Count > 0)
            //    {
            //        if (typerefs.ContainsKey(pair.Key))
            //        {
            //            String tref = typerefs[pair.Key];
            //            String tprefix;
            //            if (tref.StartsWith(":")) 
            //            {
            //                tref = tref.Substring(1);
            //                tprefix = String.Empty;
            //            }
            //            else if (tref.Contains(":"))
            //            {
            //                tprefix = tref.Substring(0, tref.IndexOf(':'));
            //            }
            //            else
            //            {
            //                tprefix = String.Empty;
            //            }
            //            subj = doc.CreateElement(tref, g.NamespaceMap.GetNamespaceUri(tprefix).ToString());

            //            doc.DocumentElement.AppendChild(subj);

            //            this.GenerateCollectionOutput(g, collections, pair.Key, subj, ref nextNamespaceID, tempNamespaces, doc);
            //        }
            //        else
            //        {
            //            throw new RdfOutputException("Failed to output a Collection due to an unknown error");
            //        }
            //    }
            //}

            context.Writer.WriteEndDocument();

            //Save to the Output Stream
            context.Writer.Close();
        }

        //private void GenerateCollectionOutput(IGraph g, Dictionary<INode, OutputRDFCollection> collections, INode key, XmlElement pred, ref int nextNamespaceID, List<String> tempNamespaces, XmlDocument doc)
        //{
        //    OutputRDFCollection c = collections[key];
        //    if (!c.IsExplicit)
        //    {
        //        XmlAttribute parseType;
        //        if (pred.ParentNode != doc.DocumentElement)
        //        {
        //            //Need to set the Predicate to have a rdf:parseType of Resource
        //            parseType = doc.CreateAttribute("rdf:parseType", NamespaceMapper.RDF);
        //            parseType.Value = "Resource";
        //            pred.Attributes.Append(parseType);
        //        }

        //        XmlElement first, rest;
        //        while (c.Count > 0)
        //        {
        //            //Get the Next Item and generate rdf:first and rdf:rest Nodes
        //            INode next = c.Pop();
        //            first = doc.CreateElement("rdf:first", NamespaceMapper.RDF);
        //            rest = doc.CreateElement("rdf:rest", NamespaceMapper.RDF);

        //            pred.AppendChild(first);
        //            pred.AppendChild(rest);

        //            //Set the value of the rdf:first Item
        //            switch (next.NodeType)
        //            {
        //                case NodeType.Blank:
        //                    XmlAttribute nodeID = doc.CreateAttribute("rdf:nodeID", NamespaceMapper.RDF);
        //                    nodeID.Value = ((BlankNode)next).InternalID;
        //                    first.Attributes.Append(nodeID);
        //                    break;
        //                case NodeType.GraphLiteral:
        //                    throw new RdfOutputException(WriterErrorMessages.GraphLiteralsUnserializable("RDF/XML"));
        //                case NodeType.Literal:
        //                    this.GenerateLiteralOutput((LiteralNode)next, first, doc);
        //                    break;
        //                case NodeType.Uri:
        //                    this.GenerateUriOutput(g, (UriNode)next, "rdf:resource", tempNamespaces, first, doc);
        //                    break;
        //                default:
        //                    throw new RdfOutputException(WriterErrorMessages.UnknownNodeTypeUnserializable("RDF/XML"));
        //            }

        //            if (c.Count >= 1)
        //            {
        //                //Set Parse Type to resource
        //                parseType = doc.CreateAttribute("rdf:parseType", NamespaceMapper.RDF);
        //                parseType.Value = "Resource";
        //                rest.Attributes.Append(parseType);

        //                pred = rest;
        //            }
        //            else
        //            {
        //                //Terminate list with an rdf:nil
        //                XmlAttribute res = doc.CreateAttribute("rdf:resource");
        //                res.InnerXml = "&rdf;nil";
        //                rest.Attributes.Append(res);
        //            }
        //        }
        //    }
        //    else
        //    {
        //        if (c.Count == 0)
        //        {
        //            //Terminate the Blank Node triple by adding a rdf:nodeID attribute
        //            XmlAttribute nodeID = doc.CreateAttribute("rdf:nodeID", NamespaceMapper.RDF);
        //            nodeID.Value = ((BlankNode)key).InternalID;
        //            pred.Attributes.Append(nodeID);
        //        }
        //        else
        //        {
        //            //Need to set the Predicate to have a rdf:parseType of Resource
        //            if (pred.Name != "rdf:Description" && pred.ParentNode != doc.DocumentElement)
        //            {
        //                XmlAttribute parseType = doc.CreateAttribute("rdf:parseType", NamespaceMapper.RDF);
        //                parseType.Value = "Resource";
        //                pred.Attributes.Append(parseType);
        //            }

        //            //Output the Predicate Object list
        //            while (c.Count > 0)
        //            {
        //                INode nextPred = c.Pop();
        //                INode nextObj = c.Pop();

        //                XmlElement p;

        //                //Generate the predicate
        //                p = this.GeneratePredicateNode(g, nextPred, ref nextNamespaceID, tempNamespaces, doc, pred);

        //                //Output the Object
        //                switch (nextObj.NodeType)
        //                {
        //                    case NodeType.Blank:
        //                        if (collections.ContainsKey(nextObj))
        //                        {
        //                            //Output a Collection
        //                            this.GenerateCollectionOutput(g, collections, nextObj, p, ref nextNamespaceID, tempNamespaces, doc);
        //                        }
        //                        else
        //                        {
        //                            XmlAttribute nodeID = doc.CreateAttribute("rdf:nodeID", NamespaceMapper.RDF);
        //                            nodeID.Value = ((BlankNode)nextObj).InternalID;
        //                            p.Attributes.Append(nodeID);
        //                        }
        //                        break;
        //                    case NodeType.GraphLiteral:
        //                        throw new RdfOutputException(WriterErrorMessages.GraphLiteralsUnserializable("RDF/XML"));
        //                    case NodeType.Literal:
        //                        this.GenerateLiteralOutput((LiteralNode)nextObj, p, doc);
        //                        break;
        //                    case NodeType.Uri:
        //                        this.GenerateUriOutput(g, (UriNode)nextObj, "rdf:resource", tempNamespaces, p, doc);
        //                        break;
        //                    default:
        //                        throw new RdfOutputException(WriterErrorMessages.UnknownNodeTypeUnserializable("RDF/XML"));
        //                }
        //            }
        //        }
        //    }
        //}

        private void GeneratePredicateNode(RdfXmlWriterContext context, INode p)
        {
            switch (p.NodeType)
            {
                case NodeType.GraphLiteral:
                    throw new RdfOutputException(WriterErrorMessages.GraphLiteralsUnserializable("RDF/XML"));
                case NodeType.Blank:
                    throw new RdfOutputException(WriterErrorMessages.BlankPredicatesUnserializable("RDF/XML"));
                case NodeType.Literal:
                    throw new RdfOutputException(WriterErrorMessages.LiteralPredicatesUnserializable("RDF/XML"));
                case NodeType.Uri:
                    //OK
                    UriRefType rtype;
                    String predRef = this.GenerateUriRef(context, (UriNode)p, UriRefType.QName, out rtype);
                    if (rtype != UriRefType.QName)
                    {
                        String prefix, uri;
                        this.GenerateTemporaryNamespace(context, (UriNode)p, out prefix, out uri);

                        //Add to current XML Element
                        context.Writer.WriteStartAttribute("xmlns", prefix, null);
                        context.Writer.WriteRaw(WriterHelper.EncodeForXml(uri));
                        context.Writer.WriteEndAttribute();

                        predRef = this.GenerateUriRef(context, (UriNode)p, UriRefType.QName, out rtype);
                        if (rtype != UriRefType.QName)
                        {
                            throw new RdfOutputException(WriterErrorMessages.UnreducablePropertyURIUnserializable + " - '" + p.ToString() + "'");
                        }
                    }

                    this.GenerateElement(context, predRef);
                    break;
                default:
                    throw new RdfOutputException(WriterErrorMessages.UnknownNodeTypeUnserializable("RDF/XML"));
            }

            //Write the Predicate
        }

        private void GenerateLiteralOutput(RdfXmlWriterContext context, LiteralNode lit)
        {
            if (!lit.Language.Equals(String.Empty))
            {
                context.Writer.WriteAttributeString("xml", "lang", null, lit.Language);
                context.Writer.WriteString(lit.Value);
            }
            else if (lit.DataType != null)
            {
                if (RdfSpecsHelper.RdfXmlLiteral.Equals(lit.DataType.ToString()))
                {
                    context.Writer.WriteAttributeString("rdf", "parseType", null, "Literal");
                    context.Writer.WriteRaw(lit.Value);
                }
                else
                {
                    context.Writer.WriteAttributeString("rdf", "datatype", null, lit.DataType.ToString());
                    context.Writer.WriteRaw(lit.Value);
                }
            }
        }

        private void GenerateUriOutput(RdfXmlWriterContext context, UriNode u, String attribute)
        {
            //Get a Uri Reference if the Uri can be reduced
            UriRefType rtype;
            String uriref = this.GenerateUriRef(context, u, UriRefType.UriRef, out rtype);
            
            if (attribute.Contains(':'))
            {
                context.Writer.WriteStartAttribute(attribute.Substring(0, attribute.IndexOf(':')), attribute.Substring(attribute.IndexOf(':') + 1), null);
                context.Writer.WriteRaw(WriterHelper.EncodeForXml(uriref));
                context.Writer.WriteEndAttribute();
            } 
            else 
            {
                context.Writer.WriteStartAttribute(attribute);
                context.Writer.WriteRaw(WriterHelper.EncodeForXml(uriref));
                context.Writer.WriteEndAttribute();
            }
        }

        private String GenerateUriRef(RdfXmlWriterContext context, UriNode u, UriRefType type, out UriRefType outType)
        {
            String uriref, qname;

            if (context.NamespaceMap.ReduceToQName(u.Uri.ToString(), out qname))
            {
                //Reduced to QName OK
                uriref = qname;
                outType = UriRefType.QName;
            }
            else
            {
                //Just use the Uri
                uriref = u.Uri.ToString();
                outType = UriRefType.Uri;
            }

            //Convert to a Uri Ref from a QName if required
            if (outType == UriRefType.QName && type == UriRefType.UriRef)
            {
                if (uriref.Contains(':') && !uriref.StartsWith(":"))
                {
                    String prefix = uriref.Substring(0, uriref.IndexOf(':'));
                    if (context.NamespaceMap.GetNestingLevel(prefix) == 0)
                    {
                        //Can only use entities for non-temporary Namespaces as Temporary Namespaces won't have Entities defined
                        uriref = "&" + uriref.Replace(':', ';');
                    }
                    else
                    {
                        uriref = context.NamespaceMap.GetNamespaceUri(prefix).ToString() + uriref.Substring(uriref.IndexOf(':') + 1);
                    }
                }
                else
                {
                    if (context.NamespaceMap.HasNamespace(String.Empty))
                    {
                        uriref = context.NamespaceMap.GetNamespaceUri(String.Empty).ToString() + uriref.Substring(1);
                    }
                    else
                    {
                        String baseUri = context.Graph.BaseUri.ToString();
                        if (!baseUri.EndsWith("#")) baseUri += "#";
                        uriref = baseUri + uriref;
                    }
                }
                outType = UriRefType.UriRef;
            }

            return uriref;
        }

        private void GenerateTemporaryNamespace(RdfXmlWriterContext context, UriNode u, out String tempPrefix, out String tempUri)
        {
            String uri = u.Uri.ToString();
            String nsUri;
            if (uri.Contains("#"))
            {
                //Create a Hash Namespace Uri
                nsUri = uri.Substring(0, uri.LastIndexOf("#") + 1);
            }
            else
            {
                //Create a Slash Namespace Uri
                nsUri = uri.Substring(0, uri.LastIndexOf("/") + 1);
            }

            //Create a Temporary Namespace ID
            while (context.NamespaceMap.HasNamespace("ns" + context.NextNamespaceID) && context.NamespaceMap.GetNestingLevel("ns" + context.NextNamespaceID) == context.NamespaceMap.NestingLevel)
            {
                context.NextNamespaceID++;
            }
            String prefix = "ns" + context.NextNamespaceID;
            context.NextNamespaceID++;
            context.NamespaceMap.AddNamespace(prefix, new Uri(nsUri));

            tempPrefix = prefix;
            tempUri = nsUri;

            this.OnWarning("Created a Temporary Namespace '" + prefix + "' with URI '" + nsUri + "'");
        }

        private void GenerateElement(RdfXmlWriterContext context, String qname)
        {
            if (qname.Contains(':'))
            {
                if (qname.StartsWith(":"))
                {
                    context.Writer.WriteStartElement(qname.Substring(1));
                }
                else
                {
                    context.Writer.WriteStartElement(qname.Substring(0, qname.IndexOf(':')), qname.Substring(qname.IndexOf(':')+1), null);
                }
            }
            else
            {
                context.Writer.WriteStartElement(qname);
            }
        }

        //private XmlAttribute GenerateAttribute(IGraph g, String qname, XmlDocument doc)
        //{
        //    if (qname.Contains(':'))
        //    {
        //        if (qname.StartsWith(":"))
        //        {
        //            return doc.CreateAttribute(qname.Substring(1));
        //        }
        //        else
        //        {
        //            return doc.CreateAttribute(qname, g.NamespaceMap.GetNamespaceUri(qname.Substring(0, qname.IndexOf(':'))).ToString());
        //        }
        //    }
        //    else
        //    {
        //        return doc.CreateAttribute(qname);
        //    }
        //}

        private Dictionary<INode, String> FindTypeReferences(RdfXmlWriterContext context, TripleCollection triplesDone)
        {
            //LINQ query to find all Triples which define the rdf:type of a Uri/BNode as a Uri
            UriNode rdfType = context.Graph.CreateUriNode(new Uri(NamespaceMapper.RDF + "type"));
            IEnumerable<Triple> ts = from t in context.Graph.Triples
                                     where (t.Subject.NodeType == NodeType.Blank || t.Subject.NodeType == NodeType.Uri)
                                            && t.Predicate.Equals(rdfType) && t.Object.NodeType == NodeType.Uri
                                            && !triplesDone.Contains(t)
                                     select t;

            Dictionary<INode, String> typerefs = new Dictionary<INode, string>();
            foreach (Triple t in ts)
            {
                if (!typerefs.ContainsKey(t.Subject))
                {
                    String typeref;
                    UriRefType rtype;
                    typeref = this.GenerateUriRef(context, (UriNode)t.Object, UriRefType.QName, out rtype);
                    if (rtype != UriRefType.QName)
                    {
                        //Generate a Temporary Namespace for the QName Type Reference
                        String prefix, uri;
                        this.GenerateTemporaryNamespace(context, (UriNode)t.Object, out prefix, out uri);

                        //Add to current XML Element
                        context.Writer.WriteStartAttribute("xmlns", prefix, null);
                        context.Writer.WriteRaw(WriterHelper.EncodeForXml(uri));
                        context.Writer.WriteEndAttribute();

                        typeref = this.GenerateUriRef(context, (UriNode)t.Object, UriRefType.QName, out rtype);
                        if (rtype == UriRefType.QName)
                        {
                            //Got a QName Type Reference in the Temporary Namespace OK
                            typerefs.Add(t.Subject, typeref);
                            if (context.Graph.Triples.WithSubject(t.Subject).Count() > 1)
                            {
                                triplesDone.Add(t);
                            }
                        }
                    }
                    else
                    {
                        //Got a QName Type Reference OK
                        typerefs.Add(t.Subject, typeref);
                        if (context.Graph.Triples.WithSubject(t.Subject).Count() > 1)
                        {
                            triplesDone.Add(t);
                        }
                    }
                }
            }

            return typerefs;
        }

        /// <summary>
        /// Internal Helper method for raising the Warning event
        /// </summary>
        /// <param name="message">Warning Message</param>
        private void OnWarning(String message)
        {
            if (this.Warning != null)
            {
                this.Warning(message);
            }
        }

        /// <summary>
        /// Event which is raised when there is a non-fatal issue with the RDF being output
        /// </summary>
        public event RdfWriterWarning Warning;
    }
}