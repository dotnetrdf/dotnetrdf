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
using VDS.RDF.Parsing;

namespace VDS.RDF.Writing
{
#if !NO_XMLDOM

    /// <summary>
    /// Class for generating RDF/XML Concrete Syntax
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is a non-streaming Writer which generates an XML DOM Tree and then saves it to a Stream
    /// </para>
    /// <para>
    /// Since this Writer is non-streaming it may be slower particularly for large Graphs since it has to build the entire XML DOM Tree for the output before it can be saved to disk.  While this may be a slight disadvantage this writer is capable of applying more of the RDF/XML syntax compressions than the standard streaming <see cref="RdfXmlWriter">RdfXmlWriter</see> because it can alter the parts of the DOM Tree it has already generated.
    /// </para>
    /// </remarks>
    [Obsolete("This class is deprecated in favour of the FastRdfXmlWriter or the new streaming RdfXmlWriter", false)]
    public class RdfXmlTreeWriter : IRdfWriter, IPrettyPrintingWriter, ICompressingWriter
    {
        private NodeCollection _subjectsDone;
        private TripleCollection _triplesDone;
        private int _compressionlevel = (int)WriterCompressionLevel.Default;
        private bool _prettyprint = true;
        private IGraph _g;
        private UriNode _rdfType = new UriNode(null, new Uri(NamespaceMapper.RDF + "type"));
        private InternalXmlWriter _writer = new InternalXmlWriter();
        private int _nextNamespaceID = 0;
        private List<String> _tempNamespaceIDs = new List<string>();

        /// <summary>
        /// Creates a new RDF/XML Tree Writer
        /// </summary>
        public RdfXmlTreeWriter()
        {

        }

        /// <summary>
        /// Creates a new RDF/XML Tree Writer using the given Compression Level
        /// </summary>
        /// <param name="compressionLevel">Compression Level</param>
        public RdfXmlTreeWriter(int compressionLevel)
        {
            this._compressionlevel = compressionLevel;
        }

        /// <summary>
        /// Gets/Sets the Compression Level
        /// </summary>
        /// <remarks>Uses the same Compression Levels as the <see cref="RdfXmlWriter">RdfXmlWriter</see></remarks>
        public int CompressionLevel
        {
            get
            {
                return this._compressionlevel;
            }
            set
            {
                this._compressionlevel = value;
            }
        }

        /// <summary>
        /// Gets/Sets whether the RDF/XML output should be pretty printed
        /// </summary>
        /// <remarks>Controls whether the RDF/XML produced is indented to be human readable, enabled by default</remarks>
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
        /// Saves a Graph as RDF/XML to the given File
        /// </summary>
        /// <param name="g">Graph to Save</param>
        /// <param name="filename">Filename of the File to save to</param>
        public void Save(IGraph g, string filename)
        {
            try
            {
                StreamWriter output = new StreamWriter(filename);
                this.Save(g, output);
            }
            catch (IOException ioEx)
            {
                throw new RdfOutputException("Unable to create a File at the specified location or some other IO Error occurred!", ioEx);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Saves a Graph as RDF/XML to the given Stream
        /// </summary>
        /// <param name="g">Graph to Save</param>
        /// <param name="output">Stream to save to</param>
        public void Save(IGraph g, TextWriter output)
        {
            try
            {
                this._g = g;
                XmlDocument tree = this.GenerateTree();
                this._writer.PrettyPrintMode = this._prettyprint;
                this._writer.Save(output, tree);
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
                    //No Catch actions
                }
                throw;
            }
        }

        private XmlDocument GenerateTree()
        {
            //Prepare for Writing
            this._subjectsDone = new NodeCollection();
            this._triplesDone = new TripleCollection();
            this._tempNamespaceIDs.Clear();
            this._nextNamespaceID = 0;

            //Create the XML Document
            XmlDocument doc = new XmlDocument();

            //Create the XML Declaration
            XmlDeclaration declaration = doc.CreateXmlDeclaration("1.0", "UTF-8", String.Empty);
            doc.AppendChild(declaration);

            //Create DOCTYPE
            String uri;
            StringBuilder entities = new StringBuilder();
            entities.Append('\n');
            foreach (String prefix in this._g.NamespaceMap.Prefixes)
            {
                if (!prefix.Equals(String.Empty))
                {
                    uri = this._g.NamespaceMap.GetNamespaceUri(prefix).ToString().Replace("'", "&pos;");
                    entities.AppendLine("\t<!ENTITY " + prefix + " '" + uri + "'>");
                }
            }
            XmlDocumentType doctype = doc.CreateDocumentType("rdf:RDF", null, null, entities.ToString());
            doc.AppendChild(doctype);

            //Create Root Element rdf:RDF
            XmlElement root = doc.CreateElement("rdf:RDF",NamespaceMapper.RDF);
            foreach (String prefix in this._g.NamespaceMap.Prefixes)
            {
                if (!prefix.Equals(String.Empty))
                {
                    XmlAttribute ns = doc.CreateAttribute("xmlns:" + prefix, "http://www.w3.org/2000/xmlns/");
                    ns.InnerXml = "&" + prefix + ";";
                    root.Attributes.Append(ns);
                }
                else
                {
                    XmlAttribute ns = doc.CreateAttribute("xmlns");
                    ns.Value = this._g.NamespaceMap.GetNamespaceUri(prefix).ToString();
                    root.Attributes.Append(ns);
                }
            }
            if (this._g.BaseUri != null)
            {
                XmlAttribute xmlbase = doc.CreateAttribute("xml:base");
                xmlbase.Value = this._g.BaseUri.ToString();
                root.Attributes.Append(xmlbase);
            }
            doc.AppendChild(root);

            //Generate Node Stripes
            foreach (INode s in this._g.Triples.SubjectNodes)
            {
                if (this._subjectsDone.Contains(s))
                {
                    continue;
                }

                this.GenerateNodeStripe(s, doc, root, true);
            }

            if (this._compressionlevel == WriterCompressionLevel.High)
            {
                if (this._triplesDone.Count < this._g.Triples.Count)
                {
                    //Due to the High Compression setting we've missed some stuff
                    this._compressionlevel = WriterCompressionLevel.More;

                    //Generate Node Stripes
                    foreach (INode s in this._g.Triples.SubjectNodes)
                    {
                        if (this._subjectsDone.Contains(s))
                        {
                            continue;
                        }

                        this.GenerateNodeStripe(s, doc, root, true);
                    }
                }
            }

            //Get rid of the Temporary Namespace
            foreach (String tempPrefix in this._tempNamespaceIDs)
            {
                this._g.NamespaceMap.RemoveNamespace(tempPrefix);
            }

            return doc;
        }

        private void GenerateNodeStripe(INode n, XmlDocument doc, XmlElement parent, bool toplevel)
        {
            UriRefType type;
            String uriref, typeref;
            XmlElement node;

            if (toplevel)
            {
                switch (n.NodeType)
                {
                    case NodeType.Blank:
                        if (toplevel)
                        {
                            //Skip for now if it occurs as the Object of any Triples
                            if (this._g.GetTriplesWithObject(n).Count() > 0) return;
                        }
                        if (this._compressionlevel >= WriterCompressionLevel.More)
                        {
                            typeref = this.FindTypeRef(n);
                            if (typeref != String.Empty)
                            {
                                node = this.GenerateElement(typeref, doc);
                                parent.AppendChild(node);
                                break;
                            }
                        }
                        node = doc.CreateElement("rdf:Description", NamespaceMapper.RDF);
                        parent.AppendChild(node);

                        break;

                    case NodeType.GraphLiteral:
                        throw new RdfOutputException(WriterErrorMessages.GraphLiteralsUnserializable("RDF/XML"));

                    case NodeType.Literal:
                        throw new RdfOutputException(WriterErrorMessages.LiteralSubjectsUnserializable("RDF/XML"));

                    case NodeType.Uri:
                        if (toplevel && this._compressionlevel >= WriterCompressionLevel.High)
                        {
                            //Skip for now
                            if (this._g.GetTriplesWithObject(n).Count() > 0) return;
                        }

                        uriref = this.GenerateUriRef((UriNode)n, UriRefType.UriRef, out type);
                        XmlAttribute about = doc.CreateAttribute("rdf:about", NamespaceMapper.RDF);
                        about.InnerXml = WriterHelper.EncodeForXml(uriref);

                        if (this._compressionlevel >= WriterCompressionLevel.More)
                        {
                            typeref = this.FindTypeRef(n);
                            if (typeref != String.Empty)
                            {
                                node = this.GenerateElement(typeref, doc);
                                node.Attributes.Append(about);
                                parent.AppendChild(node);
                                break;
                            }
                        }
                        node = doc.CreateElement("rdf:Description", NamespaceMapper.RDF);
                        node.Attributes.Append(about);
                        parent.AppendChild(node);

                        break;

                    default:
                        throw new RdfOutputException(WriterErrorMessages.UnknownNodeTypeUnserializable("RDF/XML"));
                }
            }
            else
            {
                node = parent;
            }

            this._subjectsDone.Add(n);

            foreach (Triple t in this._g.GetTriplesWithSubject(n))
            {
                this.GeneratePropertyStripe(t, doc, node);
            }
        }

        private void GeneratePropertyStripe(Triple t, XmlDocument doc, XmlElement parent)
        {
            if (this._triplesDone.Contains(t)) return;

            INode pred = t.Predicate;
            switch (pred.NodeType)
            {
                case NodeType.Blank:
                    throw new RdfOutputException(WriterErrorMessages.BlankPredicatesUnserializable("RDF/XML"));

                case NodeType.GraphLiteral:
                    throw new RdfOutputException(WriterErrorMessages.GraphLiteralsUnserializable("RDF/XML"));

                case NodeType.Literal:
                    throw new RdfOutputException(WriterErrorMessages.LiteralPredicatesUnserializable("RDF/XML"));

                case NodeType.Uri:

                    //Get Uri Ref for the Predicate
                    String predRef, typeref;
                    UriRefType type;
                    predRef = this.GenerateUriRef((UriNode)pred, UriRefType.QName, out type);
                    if (type != UriRefType.QName)
                    {
                        this.GenerateTemporaryNamespace((UriNode)pred, doc);
                        predRef = this.GenerateUriRef((UriNode)pred, UriRefType.QName, out type);
                        if (type != UriRefType.QName)
                        {
                            throw new RdfOutputException(WriterErrorMessages.UnreducablePropertyURIUnserializable + " - '" + pred.ToString() + "'");
                        }
                    }

                    //Create XML Node for the Property
                    XmlElement property = this.GenerateElement(predRef, doc);

                    //Get all the Triples with this Subject Predicate Pair
                    SubjectHasPropertySelector sel = new SubjectHasPropertySelector(t.Subject,t.Predicate);
                    List<Triple> ts = this._g.GetTriples(sel).ToList();

                    bool newPropertyNodeNeeded = true;
                    bool newPropertyThisLoop = false;
                    foreach (Triple t2 in ts)
                    {
                        if (newPropertyNodeNeeded)
                        {
                            parent.AppendChild(property);
                            newPropertyNodeNeeded = false;
                            newPropertyThisLoop = true;
                        }
                        else
                        {
                            newPropertyThisLoop = false;
                        }
                        this._triplesDone.Add(t2);

                        //Decide what to do next based on the Object type
                        INode obj = t2.Object;
                        switch (obj.NodeType)
                        {
                            case NodeType.Blank:
                                XmlElement node;
                                if (this._compressionlevel >= WriterCompressionLevel.More)
                                {
                                    typeref = this.FindTypeRef(obj);
                                    if (typeref != String.Empty)
                                    {
                                        node = this.GenerateElement(typeref, doc);
                                        property.AppendChild(node);

                                        this.GenerateNodeStripe(obj, doc, node, false);
                                        newPropertyNodeNeeded = true;
                                        break;
                                    }
                                }
                                this.GenerateNodeStripe(obj, doc, property, false);
                                if (property.HasChildNodes)
                                {
                                    XmlAttribute parseResource = doc.CreateAttribute("rdf:parseType", NamespaceMapper.RDF);
                                    parseResource.Value = "Resource";
                                    property.Attributes.Append(parseResource);
                                }
                                else
                                {
                                    XmlAttribute nodeID = doc.CreateAttribute("rdf:nodeID", NamespaceMapper.RDF);
                                    nodeID.Value = ((BlankNode)obj).InternalID;
                                    property.Attributes.Append(nodeID);
                                }
                                newPropertyNodeNeeded = true;
                                break;

                            case NodeType.GraphLiteral:
                                throw new RdfOutputException(WriterErrorMessages.GraphLiteralsUnserializable("RDF/XML"));

                            case NodeType.Literal:

                                LiteralNode lit = (LiteralNode)obj;
                                if (!lit.Language.Equals(String.Empty))
                                {
                                    XmlAttribute lang = doc.CreateAttribute("xml:lang");
                                    lang.Value = lit.Language;
                                    property.Attributes.Append(lang);
                                    property.InnerText = WriterHelper.EncodeForXml(lit.Value);
                                }
                                else if (lit.DataType != null)
                                {
                                    if (lit.DataType.ToString().Equals(RdfSpecsHelper.RdfXmlLiteral))
                                    {
                                        if (!newPropertyThisLoop)
                                        {
                                            property = this.GenerateElement(predRef, doc);
                                            parent.AppendChild(property);
                                        }

                                        //XML Literal
                                        XmlAttribute parseType = doc.CreateAttribute("rdf:parseType", NamespaceMapper.RDF);
                                        parseType.Value = "Literal";
                                        property.Attributes.Append(parseType);

                                        XmlDocumentFragment fragment = doc.CreateDocumentFragment();
                                        fragment.InnerXml = lit.Value;
                                        property.AppendChild(fragment);
                                    }
                                    else
                                    {
                                        //Other Typed Literal
                                        XmlAttribute dt = doc.CreateAttribute("rdf:datatype", NamespaceMapper.RDF);
                                        dt.Value = WriterHelper.EncodeForXml(lit.DataType.ToString());
                                        property.InnerText = WriterHelper.EncodeForXml(lit.Value);
                                        property.Attributes.Append(dt);
                                    }
                                }
                                else
                                {
                                    if (this._compressionlevel == WriterCompressionLevel.High)
                                    {
                                        XmlAttribute simple = this.GenerateAttribute(predRef, doc);
                                        simple.Value = WriterHelper.EncodeForXml(lit.Value);

                                        if (parent.Attributes.GetNamedItem(predRef) == null)
                                        {
                                            parent.RemoveChild(property);
                                            parent.Attributes.Append(simple);
                                        }
                                        else
                                        {
                                            property.InnerText = WriterHelper.EncodeForXml(lit.Value);
                                        }
                                    }
                                    else
                                    {
                                        property.InnerText = WriterHelper.EncodeForXml(lit.Value);
                                    }
                                }
                                newPropertyNodeNeeded = true;
                                break;

                            case NodeType.Uri:

                                XmlAttribute resource = doc.CreateAttribute("rdf:resource", NamespaceMapper.RDF);
                                resource.InnerXml = WriterHelper.EncodeForXml(this.GenerateUriRef((UriNode)obj, UriRefType.UriRef, out type));
                                property.Attributes.Append(resource);

                                newPropertyNodeNeeded = true;
                                break;

                            default:
                                throw new RdfOutputException(WriterErrorMessages.UnknownNodeTypeUnserializable("RDF/XML"));
                        }

                        //Generate a new Property Node for each Triple
                        if (newPropertyNodeNeeded)
                        {
                            property = this.GenerateElement(predRef, doc);
                        }
                    }

                    break;

                default:
                    throw new RdfOutputException(WriterErrorMessages.UnknownNodeTypeUnserializable("RDF/XML"));
            }
        }

        private String GenerateUriRef(UriNode u, UriRefType type, out UriRefType outType)
        {
            String uriref, qname;

            if (this._g.NamespaceMap.ReduceToQName(u.Uri.ToString(), out qname))
            {
                //Reducted to QName OK
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
                    if (!this._tempNamespaceIDs.Contains(prefix))
                    {
                        //Can only use entities for non-temporary Namespaces as Temporary Namespaces won't have Entities defined
                        uriref = "&" + uriref.Replace(':', ';');
                    }
                    else
                    {
                        uriref = this._g.NamespaceMap.GetNamespaceUri(prefix).ToString() + uriref.Substring(uriref.IndexOf(':') + 1);
                    }
                }
                else
                {
                    if (this._g.NamespaceMap.HasNamespace(String.Empty))
                    {
                        uriref = this._g.NamespaceMap.GetNamespaceUri(String.Empty).ToString() + uriref.Substring(1);
                    }
                    else
                    {
                        uriref = this._g.BaseUri.ToString() + uriref;
                    }
                }
                outType = UriRefType.UriRef;
            }

            return uriref;
        }

        private void GenerateTemporaryNamespace(UriNode u, XmlDocument doc)
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
            while (this._g.NamespaceMap.HasNamespace("ns" + this._nextNamespaceID))
            {
                this._nextNamespaceID++;
            }
            String prefix = "ns" + this._nextNamespaceID;
            this._nextNamespaceID++;
            this._g.NamespaceMap.AddNamespace(prefix, new Uri(nsUri));
            this._tempNamespaceIDs.Add(prefix);

            //Add to XML Document Element
            XmlAttribute ns = doc.CreateAttribute("xmlns:" + prefix, "http://www.w3.org/2000/xmlns/");
            ns.Value = nsUri;
            doc.DocumentElement.Attributes.Append(ns);

            this.RaiseWarning("Created a Temporary Namespace '" + prefix + "' with URI '" + nsUri + "'");
        }

        private XmlElement GenerateElement(String qname, XmlDocument doc)
        {
            if (qname.Contains(':'))
            {
                if (qname.StartsWith(":"))
                {
                    //No Namespace prefix
                    return doc.CreateElement(qname.Substring(1));
                }
                else
                {
                    //Valid Namespace prefix
                    return doc.CreateElement(qname, this._g.NamespaceMap.GetNamespaceUri(qname.Substring(0, qname.IndexOf(':'))).ToString());
                }
            }
            else
            {
                return doc.CreateElement(qname);
            }
        }

        private XmlAttribute GenerateAttribute(String qname, XmlDocument doc)
        {
            if (qname.Contains(':'))
            {
                return doc.CreateAttribute(qname, this._g.NamespaceMap.GetNamespaceUri(qname.Substring(0, qname.IndexOf(':'))).ToString());
            }
            else
            {
                return doc.CreateAttribute(qname);
            }
        }

        private String FindTypeRef(INode n)
        {
            SubjectHasPropertySelector sel = new SubjectHasPropertySelector(n, this._rdfType);
            IEnumerable<Triple> ts = this._g.GetTriples(sel).Where(t => !this._triplesDone.Contains(t));
            if (ts.Count() >= 1)
            {
                Triple t = ts.First();
                if (t.Object.NodeType == NodeType.Uri) {
                    UriRefType type;
                    String typeref = this.GenerateUriRef((UriNode)t.Object, UriRefType.QName, out type);

                    if (type == UriRefType.QName)
                    {
                        this._triplesDone.Add(t);
                        return typeref;
                    }
                    else
                    {
                        return String.Empty;
                    }
                } else {
                    return String.Empty;
                }
            }
            else
            {
                return String.Empty;
            }
        }

        /// <summary>
        /// Internal Helper method for handling raising of the <see cref="RdfXmlTreeWriter.Warning">Warning</see> event
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
        /// Event which is raised when the Writer detects a non-fatal issue with the Graph being written
        /// </summary>
        public event RdfWriterWarning Warning;
    }

#endif
}
