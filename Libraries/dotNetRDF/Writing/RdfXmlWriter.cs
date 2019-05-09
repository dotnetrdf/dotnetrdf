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
using System.Xml.Linq;
using VDS.RDF.Parsing;
using VDS.RDF.Writing.Contexts;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Writing
{
    /// <summary>
    /// Class for generating RDF/XML Concrete Syntax
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is a fast writer based on the fast writing technique used in the other non-RDF/XML Writers.
    /// </para>
    /// <para>
    /// <strong>Note:</strong> If the Graph to be serialized makes heavy use of collections it may result in a StackOverflowException.  To address this set the <see cref="RdfXmlWriter.CompressionLevel">CompressionLevel</see> property to &lt; 5
    /// </para>
    /// </remarks>
    public class RdfXmlWriter 
        : BaseRdfWriter, IPrettyPrintingWriter, ICompressingWriter, IDtdWriter, INamespaceWriter, IFormatterBasedWriter
    {
        private bool _prettyprint = true;
        private int _compressionLevel = WriterCompressionLevel.High;
        private bool _useDTD = Options.UseDtd;
        private INamespaceMapper _defaultNamespaces = new NamespaceMapper();

        /// <summary>
        /// Creates a new RDF/XML Writer
        /// </summary>
        public RdfXmlWriter()
        {

        }

        /// <summary>
        /// Creates a new RDF/XML Writer
        /// </summary>
        /// <param name="compressionLevel">Compression Level</param>
        public RdfXmlWriter(int compressionLevel)
            : this()
        {
            _compressionLevel = compressionLevel;
        }

        /// <summary>
        /// Creates a new RDF/XML Writer
        /// </summary>
        /// <param name="compressionLevel">Compression Level</param>
        /// <param name="useDtd">Whether to use DTDs to further compress output</param>
        public RdfXmlWriter(int compressionLevel, bool useDtd)
            : this(compressionLevel)
        {
            _useDTD = useDtd;
        }

        /// <summary>
        /// Gets/Sets Pretty Print Mode for the Writer
        /// </summary>
        public bool PrettyPrintMode
        {
            get
            {
                return _prettyprint;
            }
            set
            {
                _prettyprint = value;
            }
        }

        /// <summary>
        /// Gets/Sets the Compression Level in use
        /// </summary>
        /// <remarks>
        /// <para>
        /// Compression Level defaults to <see cref="WriterCompressionLevel.High">High</see> - if Compression Level is set to below <see cref="WriterCompressionLevel.More">More</see> i.e. &lt; 5 then Collections will not be compressed into more compact syntax
        /// </para>
        /// </remarks>
        public int CompressionLevel
        {
            get
            {
                return _compressionLevel;
            }
            set
            {
                _compressionLevel = value;
            }
        }

        /// <summary>
        /// Gets/Sets whether DTDs are used in the output
        /// </summary>
        public bool UseDtd
        {
            get
            {
                return _useDTD;
            }
            set
            {
                _useDTD = value;
            }
        }

        /// <summary>
        /// Gets/Sets the Default Namespaces that are always available
        /// </summary>
        public INamespaceMapper DefaultNamespaces
        {
            get
            {
                return _defaultNamespaces;
            }
            set
            {
                _defaultNamespaces = value;
            }
        }

        /// <summary>
        /// Gets the type of the Triple Formatter used by this writer
        /// </summary>
        public Type TripleFormatterType
        {
            get
            {
                return typeof(RdfXmlFormatter);
            }
        }

        /// <summary>
        /// Saves a Graph in RDF/XML syntax to the given File
        /// </summary>
        /// <param name="g">Graph to save</param>
        /// <param name="filename">Filename to save to</param>
        public override void Save(IGraph g, string filename)
        {
            using (var stream = File.Open(filename, FileMode.Create))
            {
                Save(g, new StreamWriter(stream, new UTF8Encoding(Options.UseBomForUtf8)));
            }
        }

        /// <summary>
        /// Saves a Graph to an arbitrary output stream
        /// </summary>
        /// <param name="g">Graph to save</param>
        /// <param name="output">Stream to save to</param>
        protected override void SaveInternal(IGraph g, TextWriter output)
        {
            GenerateOutput(g, output);
        }

        /// <summary>
        /// Internal method which generates the RDF/Json Output for a Graph
        /// </summary>
        /// <param name="g">Graph to save</param>
        /// <param name="output">Stream to save to</param>
        private void GenerateOutput(IGraph g, TextWriter output)
        {
            // Always force RDF Namespace to be correctly defined
            g.NamespaceMap.Import(_defaultNamespaces);
            g.NamespaceMap.AddNamespace("rdf", UriFactory.Create(NamespaceMapper.RDF));

            // Create our Writer Context and start the XML Document
            RdfXmlWriterContext context = new RdfXmlWriterContext(g, output);
            context.CompressionLevel = _compressionLevel;
            context.UseDtd = _useDTD;
            context.Writer.WriteStartDocument();

            if (context.UseDtd)
            {
                // Create the DOCTYPE declaration
                StringBuilder entities = new StringBuilder();
                String uri;
                entities.Append('\n');
                foreach (String prefix in context.NamespaceMap.Prefixes)
                {
                    uri = context.NamespaceMap.GetNamespaceUri(prefix).AbsoluteUri;
                    if (!uri.Equals(context.NamespaceMap.GetNamespaceUri(prefix).ToString()))
                    {
                        context.UseDtd = false;
                        break;
                    }
                    if (!prefix.Equals(String.Empty))
                    {
                        var escapedUri = WriterHelper.EncodeForXml(uri);
                        entities.AppendLine("\t<!ENTITY " + prefix + " '" + escapedUri + "'>");
                    }
                }
                if (context.UseDtd) context.Writer.WriteDocType("rdf:RDF", null, null, entities.ToString());
            }

            // Create the rdf:RDF element
            context.Writer.WriteStartElement("rdf", "RDF", NamespaceMapper.RDF);
            if (context.Graph.BaseUri != null)
            {
                context.Writer.WriteAttributeString("xml", "base", null, context.Graph.BaseUri.AbsoluteUri);//Uri.EscapeUriString(context.Graph.BaseUri.ToString()));
            }
            context.NamespaceMap.IncrementNesting();
            foreach (String prefix in context.NamespaceMap.Prefixes)
            {
                if (prefix.Equals("rdf")) continue;

                if (!prefix.Equals(String.Empty))
                {
                    context.Writer.WriteStartAttribute("xmlns", prefix, null);
                    // String nsRef = "&" + prefix + ";";
                    // context.Writer.WriteRaw(nsRef);
                    // context.Writer.WriteEntityRef(prefix);
                    context.Writer.WriteRaw(WriterHelper.EncodeForXml(context.NamespaceMap.GetNamespaceUri(prefix).AbsoluteUri));//Uri.EscapeUriString(WriterHelper.EncodeForXml(context.NamespaceMap.GetNamespaceUri(prefix).AbsoluteUri)));
                    context.Writer.WriteEndAttribute();
                }
                else
                {
                    context.Writer.WriteStartAttribute("xmlns");
                    context.Writer.WriteRaw(WriterHelper.EncodeForXml(context.NamespaceMap.GetNamespaceUri(prefix).AbsoluteUri));//Uri.EscapeUriString(WriterHelper.EncodeForXml(context.NamespaceMap.GetNamespaceUri(prefix).AbsoluteUri)));
                    context.Writer.WriteEndAttribute();
                }
            }

            // Find the Collections and Type References
            if (context.CompressionLevel >= WriterCompressionLevel.More)
            {
                WriterHelper.FindCollections(context, CollectionSearchMode.ImplicitOnly);
                // WriterHelper.FindCollections(context, CollectionSearchMode.All);
            }
            Dictionary<INode, String> typerefs = FindTypeReferences(context);

            // Get the Triples as a Sorted List
            List<Triple> ts = context.Graph.Triples.Where(t => !context.TriplesDone.Contains(t)).ToList();
            ts.Sort(new RdfXmlTripleComparer());

            // Variables we need to track our writing
            INode lastSubj, lastPred, lastObj;
            lastSubj = lastPred = lastObj = null;

            for (int i = 0; i < ts.Count; i++)
            {
                Triple t = ts[i];
                if (context.TriplesDone.Contains(t)) continue; //Skip if already done

                if (lastSubj == null || !t.Subject.Equals(lastSubj))
                {
                    // Start a new set of Triples
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

                    // Write out the Subject
                    // Validate Subject
                    // Use a Type Reference if applicable
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

                    // Apply appropriate attributes
                    switch (t.Subject.NodeType)
                    {
                        case NodeType.GraphLiteral:
                            throw new RdfOutputException(WriterErrorMessages.GraphLiteralsUnserializable("RDF/XML"));
                        case NodeType.Literal:
                            throw new RdfOutputException(WriterErrorMessages.LiteralSubjectsUnserializable("RDF/XML"));
                        case NodeType.Blank:
                            if (context.Collections.ContainsKey(t.Subject))
                            {
                                GenerateCollectionOutput(context, t.Subject);
                            }
                            else
                            {
                                context.Writer.WriteAttributeString("rdf", "nodeID", null, context.BlankNodeMapper.GetOutputID(((IBlankNode)t.Subject).InternalID));
                            }
                            break;
                        case NodeType.Uri:
                            GenerateUriOutput(context, (IUriNode)t.Subject, "rdf:about");
                            break;
                        default:
                            throw new RdfOutputException(WriterErrorMessages.UnknownNodeTypeUnserializable("RDF/XML"));
                    }

                    // Write the Predicate
                    context.NamespaceMap.IncrementNesting();
                    GeneratePredicateNode(context, t.Predicate);
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

                    // Write the Predicate
                    context.NamespaceMap.IncrementNesting();
                    GeneratePredicateNode(context, t.Predicate);
                    lastPred = t.Predicate;
                    lastObj = null;
                }

                // Write the Object
                if (lastObj != null)
                {
                    // Terminate the previous Predicate Node
                    context.NamespaceMap.DecrementNesting();
                    context.Writer.WriteEndElement();

                    // Start a new Predicate Node
                    context.NamespaceMap.DecrementNesting();
                    context.Writer.WriteEndElement();
                    context.NamespaceMap.IncrementNesting();
                    GeneratePredicateNode(context, t.Predicate);
                }
                // Create an Object for the Object
                switch (t.Object.NodeType)
                {
                    case NodeType.Blank:
                        if (context.Collections.ContainsKey(t.Object))
                        {
                            // Output a Collection
                            GenerateCollectionOutput(context, t.Object);
                        }
                        else
                        {
                            // Terminate the Blank Node triple by adding a rdf:nodeID attribute
                            context.Writer.WriteAttributeString("rdf", "nodeID", null, context.BlankNodeMapper.GetOutputID(((IBlankNode)t.Object).InternalID));
                        }

                        break;

                    case NodeType.GraphLiteral:
                        throw new RdfOutputException(WriterErrorMessages.GraphLiteralsUnserializable("RDF/XML"));

                    case NodeType.Literal:
                        ILiteralNode lit = (ILiteralNode)t.Object;
                        GenerateLiteralOutput(context, lit);

                        break;
                    case NodeType.Uri:
                        GenerateUriOutput(context, (IUriNode)t.Object, "rdf:resource");
                        break;
                    default:
                        throw new RdfOutputException(WriterErrorMessages.UnknownNodeTypeUnserializable("RDF/XML"));
                }
                lastObj = t.Object;

                // Force a new Predicate Node
                context.NamespaceMap.DecrementNesting();
                context.Writer.WriteEndElement();
                lastPred = null;

                context.TriplesDone.Add(t);
            }

            // Check we haven't failed to output any collections
            foreach (KeyValuePair<INode, OutputRdfCollection> pair in context.Collections)
            {
                if (pair.Value.Triples.Count > 0)
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

                        GenerateCollectionOutput(context, pair.Key);

                        context.Writer.WriteEndElement();
                    }
                    else
                    {
                        context.Writer.WriteStartElement("rdf", "Description", NamespaceMapper.RDF);
                        context.Writer.WriteAttributeString("rdf", "nodeID", NamespaceMapper.RDF, context.BlankNodeMapper.GetOutputID(((IBlankNode)pair.Key).InternalID));
                        GenerateCollectionOutput(context, pair.Key);
                        context.Writer.WriteEndElement();
                        // throw new RdfOutputException("Failed to output a Collection due to an unknown error");
                    }
                }
            }

            context.NamespaceMap.DecrementNesting();
            context.Writer.WriteEndDocument();

            // Save to the Output Stream
            context.Writer.Flush();
        }

        private void GenerateCollectionOutput(RdfXmlWriterContext context, INode key)
        {
            OutputRdfCollection c = context.Collections[key];
            if (!c.IsExplicit)
            {
                if (context.NamespaceMap.NestingLevel > 2)
                {
                    // Need to set the Predicate to have a rdf:parseType of Resource
                    context.Writer.WriteAttributeString("rdf", "parseType", NamespaceMapper.RDF, "Resource");
                }

                int length = c.Triples.Count;
                while (c.Triples.Count > 0)
                {
                    // Get the Next Item and generate the rdf:first element
                    INode next = c.Triples.First().Object;
                    c.Triples.RemoveAt(0);
                    context.NamespaceMap.IncrementNesting();
                    context.Writer.WriteStartElement("rdf", "first", NamespaceMapper.RDF);

                    // Set the value of the rdf:first Item
                    switch (next.NodeType)
                    {
                        case NodeType.Blank:
                            context.Writer.WriteAttributeString("rdf", "nodeID", NamespaceMapper.RDF, context.BlankNodeMapper.GetOutputID(((IBlankNode)next).InternalID));
                            break;
                        case NodeType.GraphLiteral:
                            throw new RdfOutputException(WriterErrorMessages.GraphLiteralsUnserializable("RDF/XML"));
                        case NodeType.Literal:
                            GenerateLiteralOutput(context, (ILiteralNode)next);
                            break;
                        case NodeType.Uri:
                            GenerateUriOutput(context, (IUriNode)next, "rdf:resource");
                            break;
                        default:
                            throw new RdfOutputException(WriterErrorMessages.UnknownNodeTypeUnserializable("RDF/XML"));
                    }

                    // Now generate the rdf:rest element
                    context.NamespaceMap.DecrementNesting();
                    context.Writer.WriteEndElement();
                    context.NamespaceMap.IncrementNesting();
                    context.Writer.WriteStartElement("rdf", "rest", NamespaceMapper.RDF);

                    if (c.Triples.Count >= 1)
                    {
                        // Set Parse Type to resource
                        context.Writer.WriteAttributeString("rdf", "parseType", NamespaceMapper.RDF, "Resource");
                    }
                    else
                    {
                        // Terminate list with an rdf:nil
                        context.Writer.WriteStartAttribute("rdf", "resource", NamespaceMapper.RDF);
                        if (context.UseDtd)
                        {
                            context.Writer.WriteRaw("&rdf;nil");
                        }
                        else
                        {
                            context.Writer.WriteRaw(NamespaceMapper.RDF + "nil");
                        }
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
                if (c.Triples.Count == 0)
                {
                    // Terminate the Blank Node triple by adding a rdf:nodeID attribute
                    context.Writer.WriteAttributeString("rdf", "nodeID", NamespaceMapper.RDF, context.BlankNodeMapper.GetOutputID(((IBlankNode)key).InternalID));
                }
                else
                {
                    // Need to set the Predicate to have a rdf:parseType of Resource
                    if (context.NamespaceMap.NestingLevel > 2)
                    {
                        // Need to set the Predicate to have a rdf:parseType of Resource
                        context.Writer.WriteAttributeString("rdf", "parseType", NamespaceMapper.RDF, "Resource");
                    }

                    // Output the Predicate Object list
                    while (c.Triples.Count > 0)
                    {
                        Triple t = c.Triples[0];
                        c.Triples.RemoveAt(0);
                        INode nextPred = t.Predicate;
                        INode nextObj = t.Object;

                        // Generate the predicate
                        GeneratePredicateNode(context, nextPred);

                        // Output the Object
                        switch (nextObj.NodeType)
                        {
                            case NodeType.Blank:
                                if (context.Collections.ContainsKey(nextObj))
                                {
                                    // Output a Collection
                                    GenerateCollectionOutput(context, nextObj);
                                }
                                else
                                {
                                    context.Writer.WriteAttributeString("rdf", "nodeID", NamespaceMapper.RDF, context.BlankNodeMapper.GetOutputID(((IBlankNode)key).InternalID));
                                }
                                break;
                            case NodeType.GraphLiteral:
                                throw new RdfOutputException(WriterErrorMessages.GraphLiteralsUnserializable("RDF/XML"));
                            case NodeType.Literal:
                                GenerateLiteralOutput(context, (ILiteralNode)nextObj);
                                break;
                            case NodeType.Uri:
                                GenerateUriOutput(context, (IUriNode)nextObj, "rdf:resource");
                                break;
                            default:
                                throw new RdfOutputException(WriterErrorMessages.UnknownNodeTypeUnserializable("RDF/XML"));
                        }

                        context.Writer.WriteEndElement();
                    }
                }
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
                    // OK
                    UriRefType rtype;
                    String predRef = GenerateUriRef(context, ((IUriNode)p).Uri, UriRefType.QName, out rtype);
                    String prefix, uri;
                    prefix = uri = null;
                    if (rtype != UriRefType.QName)
                    {
                        GenerateTemporaryNamespace(context, (IUriNode)p, out prefix, out uri);

                        predRef = GenerateUriRef(context, ((IUriNode)p).Uri, UriRefType.QName, out rtype);
                        if (rtype != UriRefType.QName)
                        {
                            throw new RdfOutputException(WriterErrorMessages.UnreducablePropertyURIUnserializable + " - '" + p.ToString() + "'");
                        }
                    }

                    GenerateElement(context, predRef);

                    // Add Temporary Namespace to current XML Element
                    // CORE-431: This is unecessary and causes malformed XML under monotouch
                    // if (prefix != null && uri != null)
                    // {
                    //    context.Writer.WriteStartAttribute("xmlns", prefix, null);
                    //    context.Writer.WriteRaw(Uri.EscapeUriString(WriterHelper.EncodeForXml(uri)));
                    //    context.Writer.WriteEndAttribute();
                    // }

                    break;
                default:
                    throw new RdfOutputException(WriterErrorMessages.UnknownNodeTypeUnserializable("RDF/XML"));
            }

            // Write the Predicate
        }

        private void GenerateLiteralOutput(RdfXmlWriterContext context, ILiteralNode lit)
        {
            if (!lit.Language.Equals(String.Empty))
            {
                context.Writer.WriteAttributeString("xml", "lang", null, lit.Language);
                context.Writer.WriteString(lit.Value);
            }
            else if (lit.DataType != null)
            {
                if (RdfSpecsHelper.RdfXmlLiteral.Equals(lit.DataType.AbsoluteUri))
                {
                    context.Writer.WriteAttributeString("rdf", "parseType", null, "Literal");
                    context.Writer.WriteRaw(lit.Value);
                }
                else
                {
                    UriRefType refType;
                    String dtUri = GenerateUriRef(context, lit.DataType, UriRefType.UriRef, out refType);
                    if (refType == UriRefType.Uri)
                    {
                        context.Writer.WriteAttributeString("rdf", "datatype", null, lit.DataType.AbsoluteUri);//Uri.EscapeUriString(lit.DataType.ToString()));
                    }
                    else if (refType == UriRefType.UriRef)
                    {
                        context.Writer.WriteStartAttribute("rdf", "datatype", null);
                        context.Writer.WriteRaw(dtUri);
                        context.Writer.WriteEndAttribute();
                    }
                    context.Writer.WriteString(lit.Value);
                }
            }
            else
            {
                // context.Writer.WriteRaw(WriterHelper.EncodeForXml(lit.Value));
                context.Writer.WriteString(lit.Value);
            }
        }

        private void GenerateUriOutput(RdfXmlWriterContext context, IUriNode u, String attribute)
        {
            // Get a Uri Reference if the Uri can be reduced
            UriRefType rtype;
            String uriref = GenerateUriRef(context, u.Uri, UriRefType.UriRef, out rtype);
            
            if (attribute.Contains(':'))
            {
                context.Writer.WriteStartAttribute(attribute.Substring(0, attribute.IndexOf(':')), attribute.Substring(attribute.IndexOf(':') + 1), NamespaceMapper.RDF);
                if (rtype == UriRefType.UriRef)
                {
                    context.Writer.WriteRaw(WriterHelper.EncodeForXml(uriref));//Uri.EscapeUriString(WriterHelper.EncodeForXml(uriref)));
                }
                else
                {
                    context.Writer.WriteString(uriref);//Uri.EscapeUriString(uriref));
                }
                context.Writer.WriteEndAttribute();
            } 
            else 
            {
                context.Writer.WriteStartAttribute(attribute);
                if (rtype == UriRefType.UriRef)
                {
                    context.Writer.WriteRaw(WriterHelper.EncodeForXml(uriref));//Uri.EscapeUriString(WriterHelper.EncodeForXml(uriref)));
                }
                else
                {
                    context.Writer.WriteString(uriref);//Uri.EscapeUriString(uriref));
                }
                context.Writer.WriteEndAttribute();
            }
        }

        private String GenerateUriRef(RdfXmlWriterContext context, Uri u, UriRefType type, out UriRefType outType)
        {
            String uriref, qname;

            if (context.NamespaceMap.ReduceToQName(u.AbsoluteUri, out qname) && (type != UriRefType.QName || RdfXmlSpecsHelper.IsValidQName(qname)))
            {
                // Reduced to QName OK
                uriref = qname;
                outType = UriRefType.QName;
            }
            else
            {
                // Just use the Uri
                uriref = u.AbsoluteUri;
                outType = UriRefType.Uri;
            }

            // Convert to a Uri Ref from a QName if required
            if (outType == UriRefType.QName && type == UriRefType.UriRef)
            {
                if (uriref.Contains(':') && !uriref.StartsWith(":"))
                {
                    String prefix = uriref.Substring(0, uriref.IndexOf(':'));
                    if (context.UseDtd && context.NamespaceMap.GetNestingLevel(prefix) == 0)
                    {
                        // Must have Use DTD enabled
                        // Can only use entities for non-temporary Namespaces as Temporary Namespaces won't have Entities defined
                        uriref = "&" + uriref.Replace(':', ';');
                        outType = UriRefType.UriRef;
                    }
                    else
                    {
                        uriref = context.NamespaceMap.GetNamespaceUri(prefix).AbsoluteUri + uriref.Substring(uriref.IndexOf(':') + 1);
                        outType = UriRefType.Uri;
                    }
                }
                else
                {
                    if (context.NamespaceMap.HasNamespace(String.Empty))
                    {
                        uriref = context.NamespaceMap.GetNamespaceUri(String.Empty).AbsoluteUri + uriref.Substring(1);
                        outType = UriRefType.Uri;
                    }
                    else
                    {
                        String baseUri = context.Graph.BaseUri.AbsoluteUri;
                        if (!baseUri.EndsWith("#")) baseUri += "#";
                        uriref = baseUri + uriref;
                        outType = UriRefType.Uri;
                    }
                }
            }

            return uriref;
        }

        private void GenerateTemporaryNamespace(RdfXmlWriterContext context, IUriNode u, out String tempPrefix, out String tempUri)
        {
            String uri = u.Uri.AbsoluteUri;
            String nsUri;
            if (uri.Contains("#"))
            {
                // Create a Hash Namespace Uri
                nsUri = uri.Substring(0, uri.LastIndexOf("#") + 1);
            }
            else
            {
                // Create a Slash Namespace Uri
                nsUri = uri.Substring(0, uri.LastIndexOf("/") + 1);
            }

            // Create a Temporary Namespace ID
            // Can't use an ID if already in the Namespace Map either at top level (nesting == 0) or at the current nesting
            while (context.NamespaceMap.HasNamespace("ns" + context.NextNamespaceID) && (context.NamespaceMap.GetNestingLevel("ns" + context.NextNamespaceID) == 0 || context.NamespaceMap.GetNestingLevel("ns" + context.NextNamespaceID) == context.NamespaceMap.NestingLevel))
            {
                context.NextNamespaceID++;
            }
            String prefix = "ns" + context.NextNamespaceID;
            context.NextNamespaceID++;
            context.NamespaceMap.AddNamespace(prefix, UriFactory.Create(nsUri));

            tempPrefix = prefix;
            tempUri = nsUri;

            RaiseWarning("Created a Temporary Namespace '" + prefix + "' with URI '" + nsUri + "'");
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
                    String ns = (context.NamespaceMap.GetNestingLevel(prefix) > 1) ? context.NamespaceMap.GetNamespaceUri(prefix).AbsoluteUri : null;
                    context.Writer.WriteStartElement(prefix, qname.Substring(prefix.Length + 1), ns);
                }
            }
            else
            {
                context.Writer.WriteStartElement(qname);
            }
        }

        private Dictionary<INode, String> FindTypeReferences(RdfXmlWriterContext context)
        {
            // LINQ query to find all Triples which define the rdf:type of a Uri/BNode as a Uri
            IUriNode rdfType = context.Graph.CreateUriNode(UriFactory.Create(NamespaceMapper.RDF + "type"));
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
                    typeref = GenerateUriRef(context, ((IUriNode)t.Object).Uri, UriRefType.QName, out rtype);
                    if (rtype != UriRefType.QName)
                    {
                        // Generate a Temporary Namespace for the QName Type Reference
                        String prefix, uri;
                        GenerateTemporaryNamespace(context, (IUriNode)t.Object, out prefix, out uri);

                        // Add to current XML Element
                        context.Writer.WriteStartAttribute("xmlns", prefix, null);
                        context.Writer.WriteRaw(Uri.EscapeUriString(WriterHelper.EncodeForXml(uri)));
                        context.Writer.WriteEndAttribute();

                        typeref = GenerateUriRef(context, ((IUriNode)t.Object).Uri, UriRefType.QName, out rtype);
                        if (rtype == UriRefType.QName)
                        {
                            // Got a QName Type Reference in the Temporary Namespace OK
                            typerefs.Add(t.Subject, typeref);
                            if (context.Graph.Triples.WithSubject(t.Subject).Count() > 1)
                            {
                                context.TriplesDone.Add(t);
                            }
                        }
                    }
                    else
                    {
                        // Got a QName Type Reference OK
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
            if (Warning != null)
            {
                Warning(message);
            }
        }

        /// <summary>
        /// Event which is raised when there is a non-fatal issue with the RDF being output
        /// </summary>
        public override event RdfWriterWarning Warning;

        /// <summary>
        /// Gets the String representation of the writer which is a description of the syntax it produces
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "RDF/XML (Streaming Writer)";
        }
    }

    class RdfXmlTripleComparer 
        : BaseTripleComparer, IComparer<INode>
    {
        public RdfXmlTripleComparer()
            : base(new FastNodeComparer()) { }

        public override int Compare(Triple x, Triple y)
        {
            int c = Compare(x.Subject, y.Subject);
            if (c == 0)
            {
                c = Compare(x.Predicate, y.Predicate);
                if (c == 0)
                {
                    c = Compare(x.Object, y.Object);
                }
            }
            return c;
        }

        public int Compare(INode x, INode y)
        {
            if (x.NodeType == y.NodeType)
            {
                return _nodeComparer.Compare(x, y);
            }
            else
            {
                switch (x.NodeType)
                {
                    case NodeType.Uri:
                        // URIs are less than everything
                        return -1;
                    case NodeType.Blank:
                        if (y.NodeType == NodeType.Uri)
                        {
                            // Blanks and greater than URIs
                            return 1;
                        }
                        else
                        {
                            // Blanks are less than everything else
                            return -1;
                        }
                    case NodeType.Literal:
                        if (y.NodeType == NodeType.Uri || y.NodeType == NodeType.Blank)
                        {
                            // Literals are greater than Blanks and URIs
                            return 1;
                        }
                        else
                        {
                            // Literals are less than than everything else
                            return -1;
                        }
                    default:
                        throw new RdfOutputException("Cannot output an RDF Graph containing non-standard Node types as RDF/XML");
                }
            }
        }
    }
}