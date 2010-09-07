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
using VDS.RDF.Parsing;
using VDS.RDF.Query;

//REQ: Implement a Linq to XML based RDF/XML writer for use on non-XML DOM supporting platforms

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
    /// This is a fast writer based on the fast writing technique used in the other non-RDF/XML writers.  While it is significantly faster than the existing RDF/XML writers achieving a speed of around 25,000 Triples/second the syntax produced is not the 'prettiest'.  It uses various syntax compressions but since it doesn't generate output in an explicitly striped manner it cannot produce the nice striped syntax
    /// </para>
    /// </remarks>
    public class FastRdfXmlWriter : IRdfWriter, IPrettyPrintingWriter
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
            //Create required variables
            int nextNamespaceID = 0;
            List<String> tempNamespaces = new List<String>();
            TripleCollection triplesDone = new TripleCollection();

            //Always force RDF Namespace to be correctly defined
            g.NamespaceMap.AddNamespace("rdf", new Uri(NamespaceMapper.RDF));

            //Create an XML Document
            XmlDocument doc = new XmlDocument();
            XmlDeclaration decl = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            doc.AppendChild(decl);

            //Create the DOCTYPE declaration and the rdf:RDF element
            StringBuilder entities = new StringBuilder();
            XmlElement rdf = doc.CreateElement("rdf:RDF", NamespaceMapper.RDF);
            XmlAttribute ns;
            String uri;
            entities.Append('\n');
            foreach (String prefix in g.NamespaceMap.Prefixes)
            {
                uri = g.NamespaceMap.GetNamespaceUri(prefix).ToString();
                if (!prefix.Equals(String.Empty))
                {
                    entities.AppendLine("\t<!ENTITY " + prefix + " '" + uri + "'>");
                    ns = doc.CreateAttribute("xmlns:" + prefix);
                    ns.Value = uri.Replace("'", "&apos;");
                }
                else
                {
                    ns = doc.CreateAttribute("xmlns");
                    ns.Value = uri;
                }
                rdf.Attributes.Append(ns);
            }
            XmlDocumentType doctype = doc.CreateDocumentType("rdf:RDF", null, null, entities.ToString());
            doc.AppendChild(doctype);
            doc.AppendChild(rdf);

            //Find the Collections
            Dictionary<INode, OutputRDFCollection> collections = WriterHelper.FindCollections(g, triplesDone);

            //Find the Type References
            Dictionary<INode, String> typerefs = this.FindTypeReferences(g, ref nextNamespaceID, tempNamespaces, doc, triplesDone);

            //Get the Triples as a Sorted List
            List<Triple> ts = g.Triples.Where(t => !triplesDone.Contains(t)).ToList();
            ts.Sort();

            //Variables we need to track our writing
            INode lastSubj, lastPred;
            lastSubj = lastPred = null;
            XmlElement subj, pred;

            //Initialise stuff to keep the compiler happy
            subj = doc.CreateElement("rdf:Description", NamespaceMapper.RDF);
            pred = doc.CreateElement("temp");

            for (int i = 0; i < ts.Count; i++)
            {
                Triple t = ts[i];
                if (triplesDone.Contains(t)) continue; //Skip if already done

                if (lastSubj == null || !t.Subject.Equals(lastSubj))
                {
                    //Start a new set of Triples
                    //Validate Subject
                    //Use a Type Reference if applicable
                    if (typerefs.ContainsKey(t.Subject))
                    {
                        String tref = typerefs[t.Subject];
                        String tprefix;
                        if (tref.StartsWith(":"))
                        {
                            tprefix = String.Empty;
                        }
                        else if (tref.Contains(":"))
                        {
                            tprefix = tref.Substring(0, tref.IndexOf(':'));
                        }
                        else
                        {
                            tprefix = String.Empty;
                        } 
                        subj = doc.CreateElement(tref, g.NamespaceMap.GetNamespaceUri(tprefix).ToString());
                    }
                    else
                    {
                        subj = doc.CreateElement("rdf:Description", NamespaceMapper.RDF);
                    }

                    //Write out the Subject
                    doc.DocumentElement.AppendChild(subj);
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
                                this.GenerateCollectionOutput(g, collections, t.Subject, subj, ref nextNamespaceID, tempNamespaces, doc);
                            }
                            else
                            {
                                XmlAttribute nodeID = doc.CreateAttribute("rdf:nodeID", NamespaceMapper.RDF);
                                nodeID.Value = ((BlankNode)t.Subject).InternalID;
                                subj.Attributes.Append(nodeID);
                            }
                            break;
                        case NodeType.Uri:
                            this.GenerateUriOutput(g, (UriNode)t.Subject, "rdf:about", tempNamespaces, subj, doc);
                            break;
                        default:
                            throw new RdfOutputException(WriterErrorMessages.UnknownNodeTypeUnserializable("RDF/XML"));
                    }

                    //Write the Predicate
                    pred = this.GeneratePredicateNode(g, t.Predicate, ref nextNamespaceID, tempNamespaces, doc, subj);
                    subj.AppendChild(pred);
                    lastPred = t.Predicate;
                }
                else if (lastPred == null || !t.Predicate.Equals(lastPred))
                {
                    //Write the Predicate
                    pred = this.GeneratePredicateNode(g, t.Predicate, ref nextNamespaceID, tempNamespaces, doc, subj);
                    subj.AppendChild(pred);
                    lastPred = t.Predicate;
                }

                //Write the Object
                //Create an Object for the Object
                switch (t.Object.NodeType)
                {
                    case NodeType.Blank:
                        if (pred.HasChildNodes || pred.HasAttributes)
                        {
                            //Require a new Predicate
                            pred = this.GeneratePredicateNode(g, t.Predicate, ref nextNamespaceID, tempNamespaces, doc, subj);
                            subj.AppendChild(pred);
                        }

                        if (collections.ContainsKey(t.Object))
                        {
                            //Output a Collection
                            this.GenerateCollectionOutput(g, collections, t.Object, pred, ref nextNamespaceID, tempNamespaces, doc);
                        }
                        else
                        {
                            //Terminate the Blank Node triple by adding a rdf:nodeID attribute
                            XmlAttribute nodeID = doc.CreateAttribute("rdf:nodeID", NamespaceMapper.RDF);
                            nodeID.Value = ((BlankNode)t.Object).InternalID;
                            pred.Attributes.Append(nodeID);
                        }

                        //Force a new Predicate after Blank Nodes
                        lastPred = null;

                        break;

                    case NodeType.GraphLiteral:
                        throw new RdfOutputException(WriterErrorMessages.GraphLiteralsUnserializable("RDF/XML"));

                    case NodeType.Literal:
                        LiteralNode lit = (LiteralNode)t.Object;

                        if (pred.HasChildNodes || pred.HasAttributes)
                        {
                            //Require a new Predicate
                            pred = this.GeneratePredicateNode(g, t.Predicate, ref nextNamespaceID, tempNamespaces, doc, subj);
                            subj.AppendChild(pred);
                        }

                        this.GenerateLiteralOutput(lit, pred, doc);

                        //Force a new Predicate Node after Literals
                        lastPred = null;

                        break;
                    case NodeType.Uri:

                        this.GenerateUriOutput(g, (UriNode)t.Object, "rdf:resource", tempNamespaces, pred, doc);

                        //Force a new Predicate Node after URIs
                        lastPred = null;

                        break;
                    default:
                        throw new RdfOutputException(WriterErrorMessages.UnknownNodeTypeUnserializable("RDF/XML"));
                }

                triplesDone.Add(t);
            }

            //Check we haven't failed to output any collections
            foreach (KeyValuePair<INode, OutputRDFCollection> pair in collections)
            {
                if (pair.Value.Count > 0)
                {
                    if (typerefs.ContainsKey(pair.Key))
                    {
                        String tref = typerefs[pair.Key];
                        String tprefix;
                        if (tref.StartsWith(":")) 
                        {
                            tref = tref.Substring(1);
                            tprefix = String.Empty;
                        }
                        else if (tref.Contains(":"))
                        {
                            tprefix = tref.Substring(0, tref.IndexOf(':'));
                        }
                        else
                        {
                            tprefix = String.Empty;
                        }
                        subj = doc.CreateElement(tref, g.NamespaceMap.GetNamespaceUri(tprefix).ToString());

                        doc.DocumentElement.AppendChild(subj);

                        this.GenerateCollectionOutput(g, collections, pair.Key, subj, ref nextNamespaceID, tempNamespaces, doc);
                    }
                    else
                    {
                        throw new RdfOutputException("Failed to output a Collection due to an unknown error");
                    }
                }
            }

            //Save to the Output Stream
            InternalXmlWriter writer = new InternalXmlWriter();
            writer.Save(output, doc);

            //Get rid of the Temporary Namespace
            foreach (String tempPrefix in tempNamespaces)
            {
                g.NamespaceMap.RemoveNamespace(tempPrefix);
            }
        }

        private void GenerateCollectionOutput(IGraph g, Dictionary<INode, OutputRDFCollection> collections, INode key, XmlElement pred, ref int nextNamespaceID, List<String> tempNamespaces, XmlDocument doc)
        {
            OutputRDFCollection c = collections[key];
            if (!c.IsExplicit)
            {
                XmlAttribute parseType;
                if (pred.ParentNode != doc.DocumentElement)
                {
                    //Need to set the Predicate to have a rdf:parseType of Resource
                    parseType = doc.CreateAttribute("rdf:parseType", NamespaceMapper.RDF);
                    parseType.Value = "Resource";
                    pred.Attributes.Append(parseType);
                }

                XmlElement first, rest;
                while (c.Count > 0)
                {
                    //Get the Next Item and generate rdf:first and rdf:rest Nodes
                    INode next = c.Pop();
                    first = doc.CreateElement("rdf:first", NamespaceMapper.RDF);
                    rest = doc.CreateElement("rdf:rest", NamespaceMapper.RDF);

                    pred.AppendChild(first);
                    pred.AppendChild(rest);

                    //Set the value of the rdf:first Item
                    switch (next.NodeType)
                    {
                        case NodeType.Blank:
                            XmlAttribute nodeID = doc.CreateAttribute("rdf:nodeID", NamespaceMapper.RDF);
                            nodeID.Value = ((BlankNode)next).InternalID;
                            first.Attributes.Append(nodeID);
                            break;
                        case NodeType.GraphLiteral:
                            throw new RdfOutputException(WriterErrorMessages.GraphLiteralsUnserializable("RDF/XML"));
                        case NodeType.Literal:
                            this.GenerateLiteralOutput((LiteralNode)next, first, doc);
                            break;
                        case NodeType.Uri:
                            this.GenerateUriOutput(g, (UriNode)next, "rdf:resource", tempNamespaces, first, doc);
                            break;
                        default:
                            throw new RdfOutputException(WriterErrorMessages.UnknownNodeTypeUnserializable("RDF/XML"));
                    }

                    if (c.Count >= 1)
                    {
                        //Set Parse Type to resource
                        parseType = doc.CreateAttribute("rdf:parseType", NamespaceMapper.RDF);
                        parseType.Value = "Resource";
                        rest.Attributes.Append(parseType);

                        pred = rest;
                    }
                    else
                    {
                        //Terminate list with an rdf:nil
                        XmlAttribute res = doc.CreateAttribute("rdf:resource");
                        res.InnerXml = "&rdf;nil";
                        rest.Attributes.Append(res);
                    }
                }
            }
            else
            {
                if (c.Count == 0)
                {
                    //Terminate the Blank Node triple by adding a rdf:nodeID attribute
                    XmlAttribute nodeID = doc.CreateAttribute("rdf:nodeID", NamespaceMapper.RDF);
                    nodeID.Value = ((BlankNode)key).InternalID;
                    pred.Attributes.Append(nodeID);
                }
                else
                {
                    //Need to set the Predicate to have a rdf:parseType of Resource
                    if (pred.Name != "rdf:Description" && pred.ParentNode != doc.DocumentElement)
                    {
                        XmlAttribute parseType = doc.CreateAttribute("rdf:parseType", NamespaceMapper.RDF);
                        parseType.Value = "Resource";
                        pred.Attributes.Append(parseType);
                    }

                    //Output the Predicate Object list
                    while (c.Count > 0)
                    {
                        INode nextPred = c.Pop();
                        INode nextObj = c.Pop();

                        XmlElement p;

                        //Generate the predicate
                        p = this.GeneratePredicateNode(g, nextPred, ref nextNamespaceID, tempNamespaces, doc, pred);

                        //Output the Object
                        switch (nextObj.NodeType)
                        {
                            case NodeType.Blank:
                                if (collections.ContainsKey(nextObj))
                                {
                                    //Output a Collection
                                    this.GenerateCollectionOutput(g, collections, nextObj, p, ref nextNamespaceID, tempNamespaces, doc);
                                }
                                else
                                {
                                    XmlAttribute nodeID = doc.CreateAttribute("rdf:nodeID", NamespaceMapper.RDF);
                                    nodeID.Value = ((BlankNode)nextObj).InternalID;
                                    p.Attributes.Append(nodeID);
                                }
                                break;
                            case NodeType.GraphLiteral:
                                throw new RdfOutputException(WriterErrorMessages.GraphLiteralsUnserializable("RDF/XML"));
                            case NodeType.Literal:
                                this.GenerateLiteralOutput((LiteralNode)nextObj, p, doc);
                                break;
                            case NodeType.Uri:
                                this.GenerateUriOutput(g, (UriNode)nextObj, "rdf:resource", tempNamespaces, p, doc);
                                break;
                            default:
                                throw new RdfOutputException(WriterErrorMessages.UnknownNodeTypeUnserializable("RDF/XML"));
                        }
                    }
                }
            }
        }

        private XmlElement GeneratePredicateNode(IGraph g, INode p, ref int nextNamespaceID, List<String> tempNamespaces, XmlDocument doc, XmlElement subj)
        {
            XmlElement pred;

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
                    String predRef = this.GenerateUriRef(g, (UriNode)p, UriRefType.QName, tempNamespaces, out rtype);
                    if (rtype != UriRefType.QName)
                    {
                        this.GenerateTemporaryNamespace(g, (UriNode)p, ref nextNamespaceID, tempNamespaces, doc);
                        predRef = this.GenerateUriRef(g, (UriNode)p, UriRefType.QName, tempNamespaces, out rtype);
                        if (rtype != UriRefType.QName)
                        {
                            throw new RdfOutputException(WriterErrorMessages.UnreducablePropertyURIUnserializable + " - '" + p.ToString() + "'");
                        }
                    }

                    pred = this.GenerateElement(g, predRef, doc);
                    break;
                default:
                    throw new RdfOutputException(WriterErrorMessages.UnknownNodeTypeUnserializable("RDF/XML"));
            }

            //Write the Predicate
            subj.AppendChild(pred);
            return pred;
        }

        private void GenerateLiteralOutput(LiteralNode lit, XmlElement pred, XmlDocument doc)
        {
            pred.InnerText = WriterHelper.EncodeForXml(lit.Value);

            if (!lit.Language.Equals(String.Empty))
            {
                XmlAttribute lang = doc.CreateAttribute("xml:lang");
                lang.Value = lit.Language;
                pred.Attributes.Append(lang);
            }
            else if (lit.DataType != null)
            {
                if (RdfSpecsHelper.RdfXmlLiteral.Equals(lit.DataType.ToString()))
                {
                    XmlAttribute parseType = doc.CreateAttribute("rdf:parseType");
                    parseType.Value = "Literal";
                    pred.Attributes.Append(parseType);

                    pred.InnerText = String.Empty;
                    XmlDocumentFragment fragment = doc.CreateDocumentFragment();
                    fragment.InnerXml = lit.Value;
                    pred.AppendChild(fragment);
                }
                else
                {
                    XmlAttribute dt = doc.CreateAttribute("rdf:datatype");
                    dt.Value = WriterHelper.EncodeForXml(lit.DataType.ToString());
                    pred.Attributes.Append(dt);
                }
            }
        }

        private void GenerateUriOutput(IGraph g, UriNode u, String attribute, List<String> tempNamespaceIDs, XmlElement node, XmlDocument doc)
        {
            //Create an attribute
            XmlAttribute attr = doc.CreateAttribute(attribute, NamespaceMapper.RDF);
            //Get a Uri Reference if the Uri can be reduced
            UriRefType rtype;
            String uriref = this.GenerateUriRef(g, u, UriRefType.UriRef, tempNamespaceIDs, out rtype);
            attr.InnerXml = WriterHelper.EncodeForXml(uriref);
            //Append the attribute
            node.Attributes.Append(attr);
        }

        private String GenerateUriRef(IGraph g, UriNode u, UriRefType type, List<String> tempNamespaceIDs, out UriRefType outType)
        {
            String uriref, qname;

            if (g.NamespaceMap.ReduceToQName(u.Uri.ToString(), out qname))
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
                    if (!tempNamespaceIDs.Contains(prefix))
                    {
                        //Can only use entities for non-temporary Namespaces as Temporary Namespaces won't have Entities defined
                        uriref = "&" + uriref.Replace(':', ';');
                    }
                    else
                    {
                        uriref = g.NamespaceMap.GetNamespaceUri(prefix).ToString() + uriref.Substring(uriref.IndexOf(':') + 1);
                    }
                }
                else
                {
                    if (g.NamespaceMap.HasNamespace(String.Empty))
                    {
                        uriref = g.NamespaceMap.GetNamespaceUri(String.Empty).ToString() + uriref.Substring(1);
                    }
                    else
                    {
                        String baseUri = g.BaseUri.ToString();
                        if (!baseUri.EndsWith("#")) baseUri += "#";
                        uriref = baseUri + uriref;
                    }
                }
                outType = UriRefType.UriRef;
            }

            return uriref;
        }

        private void GenerateTemporaryNamespace(IGraph g, UriNode u, ref int nextNamespaceID, List<String> tempNamespaceIDs, XmlDocument doc)
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
            while (g.NamespaceMap.HasNamespace("ns" + nextNamespaceID))
            {
                nextNamespaceID++;
            }
            String prefix = "ns" + nextNamespaceID;
            nextNamespaceID++;
            g.NamespaceMap.AddNamespace(prefix, new Uri(nsUri));
            tempNamespaceIDs.Add(prefix);

            //Add to XML Document Element
            XmlAttribute ns = doc.CreateAttribute("xmlns:" + prefix, "http://www.w3.org/2000/xmlns/");
            ns.Value = nsUri;
            doc.DocumentElement.Attributes.Append(ns);

            this.RaiseWarning("Created a Temporary Namespace '" + prefix + "' with URI '" + nsUri + "'");
        }

        private XmlElement GenerateElement(IGraph g, String qname, XmlDocument doc)
        {
            if (qname.Contains(':'))
            {
                if (qname.StartsWith(":"))
                {
                    return doc.CreateElement(qname.Substring(1));
                }
                else
                {
                    return doc.CreateElement(qname, g.NamespaceMap.GetNamespaceUri(qname.Substring(0, qname.IndexOf(':'))).ToString());
                }
            }
            else
            {
                return doc.CreateElement(qname);
            }
        }

        private XmlAttribute GenerateAttribute(IGraph g, String qname, XmlDocument doc)
        {
            if (qname.Contains(':'))
            {
                if (qname.StartsWith(":"))
                {
                    return doc.CreateAttribute(qname.Substring(1));
                }
                else
                {
                    return doc.CreateAttribute(qname, g.NamespaceMap.GetNamespaceUri(qname.Substring(0, qname.IndexOf(':'))).ToString());
                }
            }
            else
            {
                return doc.CreateAttribute(qname);
            }
        }

        private Dictionary<INode, String> FindTypeReferences(IGraph g, ref int nextNamespaceID, List<String> tempNamespaceIDs, XmlDocument doc, TripleCollection triplesDone)
        {
            //LINQ query to find all Triples which define the rdf:type of a Uri/BNode as a Uri
            UriNode rdfType = g.CreateUriNode(new Uri(NamespaceMapper.RDF + "type"));
            IEnumerable<Triple> ts = from t in g.Triples
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
                    typeref = this.GenerateUriRef(g, (UriNode)t.Object, UriRefType.QName, tempNamespaceIDs, out rtype);
                    if (rtype != UriRefType.QName)
                    {
                        //Generate a Temporary Namespace for the QName Type Reference
                        this.GenerateTemporaryNamespace(g, (UriNode)t.Object, ref nextNamespaceID, tempNamespaceIDs, doc);
                        typeref = this.GenerateUriRef(g, (UriNode)t.Object, UriRefType.QName, tempNamespaceIDs, out rtype);
                        if (rtype == UriRefType.QName)
                        {
                            //Got a QName Type Reference in the Temporary Namespace OK
                            typerefs.Add(t.Subject, typeref);
                            if (g.Triples.WithSubject(t.Subject).Count() > 1)
                            {
                                triplesDone.Add(t);
                            }
                        }
                    }
                    else
                    {
                        //Got a QName Type Reference OK
                        typerefs.Add(t.Subject, typeref);
                        if (g.Triples.WithSubject(t.Subject).Count() > 1)
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

#endif