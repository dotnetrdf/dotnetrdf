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
            context.NamespaceMap.IncrementNesting();
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
                    context.Writer.WriteRaw(WriterHelper.EncodeForXml(context.NamespaceMap.GetNamespaceUri(prefix).ToString()));
                    context.Writer.WriteEndAttribute();
                }
            }

            //Find the Collections and Type References
            WriterHelper.FindCollections(context, CollectionSearchMode.ImplicitOnly);
            Dictionary<INode, String> typerefs = this.FindTypeReferences(context);

            //Get the Triples as a Sorted List
            List<Triple> ts = context.Graph.Triples.Where(t => !context.TriplesDone.Contains(t)).ToList();
            ts.Sort();

            //Variables we need to track our writing
            INode lastSubj, lastPred, lastObj;
            lastSubj = lastPred = lastObj = null;

            for (int i = 0; i < ts.Count; i++)
            {
                Triple t = ts[i];
                if (context.TriplesDone.Contains(t)) continue; //Skip if already done

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
                            if (context.Collections.ContainsKey(t.Subject))
                            {
                                this.GenerateCollectionOutput(context, t.Subject);
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

                        if (context.Collections.ContainsKey(t.Object))
                        {
                            //Output a Collection
                            this.GenerateCollectionOutput(context, t.Object);
                        }
                        else
                        {
                            //Terminate the Blank Node triple by adding a rdf:nodeID attribute
                            context.Writer.WriteAttributeString("rdf", "nodeID", null, ((BlankNode)t.Object).InternalID);
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

                context.TriplesDone.Add(t);
            }

            //Check we haven't failed to output any collections
            foreach (KeyValuePair<INode, OutputRDFCollection> pair in context.Collections)
            {
                if (pair.Value.Count > 0)
                {
                    if (typerefs.ContainsKey(pair.Key))
                    {
                        String tref = typerefs[pair.Key];
                        context.NamespaceMap.IncrementNesting();
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

                        this.GenerateCollectionOutput(context, pair.Key);
                    }
                    else
                    {
                        throw new RdfOutputException("Failed to output a Collection due to an unknown error");
                    }
                }
            }

            context.NamespaceMap.DecrementNesting();
            context.Writer.WriteEndDocument();

            //Save to the Output Stream
            context.Writer.Close();
        }

        private void GenerateCollectionOutput(RdfXmlWriterContext context, INode key)
        {
            OutputRDFCollection c = context.Collections[key];
            if (!c.IsExplicit)
            {
                if (context.NamespaceMap.NestingLevel > 2)
                {
                    //Need to set the Predicate to have a rdf:parseType of Resource
                    context.Writer.WriteAttributeString("rdf", "parseType", NamespaceMapper.RDF, "Resource");
                }

                int length = c.Count;
                while (c.Count > 0)
                {
                    //Get the Next Item and generate the rdf:first element
                    INode next = c.Pop();
                    context.NamespaceMap.IncrementNesting();
                    context.Writer.WriteStartElement("rdf", "first", NamespaceMapper.RDF);

                    //Set the value of the rdf:first Item
                    switch (next.NodeType)
                    {
                        case NodeType.Blank:
                            context.Writer.WriteAttributeString("rdf", "nodeID", NamespaceMapper.RDF, ((BlankNode)next).InternalID);
                            break;
                        case NodeType.GraphLiteral:
                            throw new RdfOutputException(WriterErrorMessages.GraphLiteralsUnserializable("RDF/XML"));
                        case NodeType.Literal:
                            this.GenerateLiteralOutput(context, (LiteralNode)next);
                            break;
                        case NodeType.Uri:
                            this.GenerateUriOutput(context, (UriNode)next, "rdf:resource");
                            break;
                        default:
                            throw new RdfOutputException(WriterErrorMessages.UnknownNodeTypeUnserializable("RDF/XML"));
                    }

                    //Now generate the rdf:rest element
                    context.NamespaceMap.DecrementNesting();
                    context.Writer.WriteEndElement();
                    context.NamespaceMap.IncrementNesting();
                    context.Writer.WriteStartElement("rdf", "rest", NamespaceMapper.RDF);

                    if (c.Count >= 1)
                    {
                        //Set Parse Type to resource
                        context.Writer.WriteAttributeString("rdf", "parseType", NamespaceMapper.RDF, "Resource");
                    }
                    else
                    {
                        //Terminate list with an rdf:nil
                        context.Writer.WriteStartAttribute("rdf", "resource", NamespaceMapper.RDF);
                        context.Writer.WriteRaw("&rdf;nil");
                        context.Writer.WriteEndAttribute();
                    }
                }
                for (int i = 0; i < length; i++)
                {
                    context.NamespaceMap.DecrementNesting();
                    context.Writer.WriteEndElement();
                }
            }
            else
            {
                //if (c.Count == 0)
                //{
                //    //Terminate the Blank Node triple by adding a rdf:nodeID attribute
                //    context.Writer.WriteAttributeString("rdf", "nodeID", NamespaceMapper.RDF, ((BlankNode)key).InternalID);
                //}
                //else
                //{
                //    //Need to set the Predicate to have a rdf:parseType of Resource
                //    if (pred.Name != "rdf:Description" && pred.ParentNode != doc.DocumentElement)
                //    {
                //        XmlAttribute parseType = doc.CreateAttribute("rdf:parseType", NamespaceMapper.RDF);
                //        parseType.Value = "Resource";
                //        pred.Attributes.Append(parseType);
                //    }

                //    //Output the Predicate Object list
                //    while (c.Count > 0)
                //    {
                //        INode nextPred = c.Pop();
                //        INode nextObj = c.Pop();

                //        XmlElement p;

                //        //Generate the predicate
                //        p = this.GeneratePredicateNode(g, nextPred, ref nextNamespaceID, tempNamespaces, doc, pred);

                //        //Output the Object
                //        switch (nextObj.NodeType)
                //        {
                //            case NodeType.Blank:
                //                if (collections.ContainsKey(nextObj))
                //                {
                //                    //Output a Collection
                //                    this.GenerateCollectionOutput(g, collections, nextObj, p, ref nextNamespaceID, tempNamespaces, doc);
                //                }
                //                else
                //                {
                //                    XmlAttribute nodeID = doc.CreateAttribute("rdf:nodeID", NamespaceMapper.RDF);
                //                    nodeID.Value = ((BlankNode)nextObj).InternalID;
                //                    p.Attributes.Append(nodeID);
                //                }
                //                break;
                //            case NodeType.GraphLiteral:
                //                throw new RdfOutputException(WriterErrorMessages.GraphLiteralsUnserializable("RDF/XML"));
                //            case NodeType.Literal:
                //                this.GenerateLiteralOutput((LiteralNode)nextObj, p, doc);
                //                break;
                //            case NodeType.Uri:
                //                this.GenerateUriOutput(g, (UriNode)nextObj, "rdf:resource", tempNamespaces, p, doc);
                //                break;
                //            default:
                //                throw new RdfOutputException(WriterErrorMessages.UnknownNodeTypeUnserializable("RDF/XML"));
                //        }
                //    }
                //}
            }
        }

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
                    String predRef = this.GenerateUriRef(context, ((UriNode)p).Uri, UriRefType.QName, out rtype);
                    String prefix, uri;
                    prefix = uri = null;
                    if (rtype != UriRefType.QName)
                    {
                        this.GenerateTemporaryNamespace(context, (UriNode)p, out prefix, out uri);

                        predRef = this.GenerateUriRef(context, ((UriNode)p).Uri, UriRefType.QName, out rtype);
                        if (rtype != UriRefType.QName)
                        {
                            throw new RdfOutputException(WriterErrorMessages.UnreducablePropertyURIUnserializable + " - '" + p.ToString() + "'");
                        }
                    }

                    this.GenerateElement(context, predRef);

                    //Add Temporary Namespace to current XML Element
                    if (prefix != null && uri != null)
                    {
                        context.Writer.WriteStartAttribute("xmlns", prefix, null);
                        context.Writer.WriteRaw(WriterHelper.EncodeForXml(uri));
                        context.Writer.WriteEndAttribute();
                    }

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
                context.Writer.WriteRaw(WriterHelper.EncodeForXml(lit.Value));
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
                    UriRefType refType;
                    String dtUri = this.GenerateUriRef(context, lit.DataType, UriRefType.UriRef, out refType);
                    if (refType == UriRefType.Uri)
                    {
                        context.Writer.WriteAttributeString("rdf", "datatype", null, WriterHelper.EncodeForXml(lit.DataType.ToString()));
                    }
                    else if (refType == UriRefType.UriRef)
                    {
                        context.Writer.WriteStartAttribute("rdf", "datatype", null);
                        context.Writer.WriteRaw(dtUri);
                        context.Writer.WriteEndAttribute();
                    }
                    context.Writer.WriteRaw(lit.Value);
                }
            }
            else
            {
                context.Writer.WriteRaw(WriterHelper.EncodeForXml(lit.Value));
            }
        }

        private void GenerateUriOutput(RdfXmlWriterContext context, UriNode u, String attribute)
        {
            //Get a Uri Reference if the Uri can be reduced
            UriRefType rtype;
            String uriref = this.GenerateUriRef(context, u.Uri, UriRefType.UriRef, out rtype);
            
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

        private String GenerateUriRef(RdfXmlWriterContext context, Uri u, UriRefType type, out UriRefType outType)
        {
            String uriref, qname;

            if (context.NamespaceMap.ReduceToQName(u.ToString(), out qname))
            {
                //Reduced to QName OK
                uriref = qname;
                outType = UriRefType.QName;
            }
            else
            {
                //Just use the Uri
                uriref = u.ToString();
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

            this.RaiseWarning("Created a Temporary Namespace '" + prefix + "' with URI '" + nsUri + "'");
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
                    String prefix = qname.Substring(0, qname.IndexOf(':'));
                    String ns = (context.NamespaceMap.GetNestingLevel(prefix) > 1) ? context.NamespaceMap.GetNamespaceUri(prefix).ToString() : null;
                    context.Writer.WriteStartElement(prefix, qname.Substring(prefix.Length + 1), ns);
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

        private Dictionary<INode, String> FindTypeReferences(RdfXmlWriterContext context)
        {
            //LINQ query to find all Triples which define the rdf:type of a Uri/BNode as a Uri
            UriNode rdfType = context.Graph.CreateUriNode(new Uri(NamespaceMapper.RDF + "type"));
            IEnumerable<Triple> ts = from t in context.Graph.Triples
                                     where (t.Subject.NodeType == NodeType.Blank || t.Subject.NodeType == NodeType.Uri)
                                            && t.Predicate.Equals(rdfType) && t.Object.NodeType == NodeType.Uri
                                            && !context.TriplesDone.Contains(t)
                                     select t;

            Dictionary<INode, String> typerefs = new Dictionary<INode, string>();
            foreach (Triple t in ts)
            {
                if (!typerefs.ContainsKey(t.Subject))
                {
                    String typeref;
                    UriRefType rtype;
                    typeref = this.GenerateUriRef(context, ((UriNode)t.Object).Uri, UriRefType.QName, out rtype);
                    if (rtype != UriRefType.QName)
                    {
                        //Generate a Temporary Namespace for the QName Type Reference
                        String prefix, uri;
                        this.GenerateTemporaryNamespace(context, (UriNode)t.Object, out prefix, out uri);

                        //Add to current XML Element
                        context.Writer.WriteStartAttribute("xmlns", prefix, null);
                        context.Writer.WriteRaw(WriterHelper.EncodeForXml(uri));
                        context.Writer.WriteEndAttribute();

                        typeref = this.GenerateUriRef(context, ((UriNode)t.Object).Uri, UriRefType.QName, out rtype);
                        if (rtype == UriRefType.QName)
                        {
                            //Got a QName Type Reference in the Temporary Namespace OK
                            typerefs.Add(t.Subject, typeref);
                            if (context.Graph.Triples.WithSubject(t.Subject).Count() > 1)
                            {
                                context.TriplesDone.Add(t);
                            }
                        }
                    }
                    else
                    {
                        //Got a QName Type Reference OK
                        typerefs.Add(t.Subject, typeref);
                        if (context.Graph.Triples.WithSubject(t.Subject).Count() > 1)
                        {
                           context.TriplesDone.Add(t);
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
        private void RaiseWarning(String message)
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