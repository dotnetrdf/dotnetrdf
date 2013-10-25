/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using VDS.RDF.Graphs;
using VDS.RDF.Namespaces;
using VDS.RDF.Nodes;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
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
    public class PrettyRdfXmlWriter 
        : IRdfWriter, IPrettyPrintingWriter, ICompressingWriter, IDtdWriter,
        INamespaceWriter, IFormatterBasedWriter, IAttributeWriter
    {
        private bool _prettyprint = true;
        private int _compressionLevel = WriterCompressionLevel.High;
        private bool _useDTD = IOOptions.UseDtd;
        private bool _useAttributes = true;
        private INamespaceMapper _defaultNamespaces = new NamespaceMapper();

        /// <summary>
        /// Creates a new RDF/XML Writer
        /// </summary>
        public PrettyRdfXmlWriter()
        {

        }

        /// <summary>
        /// Creates a new RDF/XML Writer
        /// </summary>
        /// <param name="compressionLevel">Compression Level</param>
        public PrettyRdfXmlWriter(int compressionLevel)
            : this()
        {
            this._compressionLevel = compressionLevel;
        }

        /// <summary>
        /// Creates a new RDF/XML Writer
        /// </summary>
        /// <param name="compressionLevel">Compression Level</param>
        /// <param name="useDtd">Whether to use DTDs to further compress output</param>
        public PrettyRdfXmlWriter(int compressionLevel, bool useDtd)
            : this(compressionLevel)
        {
            this._useDTD = useDtd;
        }

        /// <summary>
        /// Creates a new RDF/XML Writer
        /// </summary>
        /// <param name="compressionLevel">Compression Level</param>
        /// <param name="useDtd">Whether to use DTDs to further compress output</param>
        /// <param name="useAttributes">Whether to use attributes to encode triples with simple literal objects where possible</param>
        public PrettyRdfXmlWriter(int compressionLevel, bool useDtd, bool useAttributes)
            : this(compressionLevel, useDtd)
        {
            this._useAttributes = useAttributes;
        }

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
                return this._compressionLevel;
            }
            set
            {
                this._compressionLevel = value;
            }
        }

        /// <summary>
        /// Gets/Sets whether DTDs are used in the output
        /// </summary>
        public bool UseDtd
        {
            get
            {
                return this._useDTD;
            }
            set
            {
                this._useDTD = value;
            }
        }

        /// <summary>
        /// Gets/Sets whether triples which have a literal object will be expressed as attributes rather than elements where possible (defaults to true)
        /// </summary>
        public bool UseAttributes
        {
            get
            {
                return this._useAttributes;
            }
            set
            {
                this._useAttributes = value;
            }
        }

        /// <summary>
        /// Gets/Sets the Default Namespaces that are always available
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
        /// Gets the type of the Triple Formatter used by the writer
        /// </summary>
        public Type TripleFormatterType
        {
            get
            {
                return typeof(RdfXmlFormatter);
            }
        }

#if !NO_FILE
        /// <summary>
        /// Saves a Graph in RDF/XML syntax to the given File
        /// </summary>
        /// <param name="g">Graph to save</param>
        /// <param name="filename">Filename to save to</param>
        public void Save(IGraph g, string filename)
        {
            StreamWriter output = new StreamWriter(filename, false, new UTF8Encoding(IOOptions.UseBomForUtf8));
            this.Save(g, output);
        }
#endif

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
            g.NamespaceMap.Import(this._defaultNamespaces);
            g.NamespaceMap.AddNamespace("rdf", UriFactory.Create(NamespaceMapper.RDF));

            //Create our Writer Context and start the XML Document
            RdfXmlWriterContext context = new RdfXmlWriterContext(g, output);
            context.CompressionLevel = this._compressionLevel;
            context.UseDtd = this._useDTD;
            context.UseAttributes = this._useAttributes;
            context.Writer.WriteStartDocument();

            if (context.UseDtd)
            {
                //Create the DOCTYPE declaration
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
                        entities.AppendLine("\t<!ENTITY " + prefix + " '" + uri + "'>");
                    }
                }
                if (context.UseDtd) context.Writer.WriteDocType("rdf:RDF", null, null, entities.ToString());
            }

            //Create the rdf:RDF element
            context.Writer.WriteStartElement("rdf", "RDF", NamespaceMapper.RDF);
            if (context.Graph.BaseUri != null)
            {
                context.Writer.WriteAttributeString("xml", "base", null, context.Graph.BaseUri.AbsoluteUri);
            }

            //Add all the existing Namespace Definitions here
            context.NamespaceMap.IncrementNesting();
            foreach (String prefix in context.NamespaceMap.Prefixes)
            {
                if (prefix.Equals("rdf")) continue;

                if (!prefix.Equals(String.Empty))
                {
                    context.Writer.WriteStartAttribute("xmlns", prefix, null);
                    context.Writer.WriteString(context.NamespaceMap.GetNamespaceUri(prefix).AbsoluteUri);//Uri.EscapeUriString(context.NamespaceMap.GetNamespaceUri(prefix).AbsoluteUri));
                    context.Writer.WriteEndAttribute();
                }
                else
                {
                    context.Writer.WriteStartAttribute("xmlns");
                    context.Writer.WriteString(context.NamespaceMap.GetNamespaceUri(prefix).AbsoluteUri);//Uri.EscapeUriString(context.NamespaceMap.GetNamespaceUri(prefix).AbsoluteUri));
                    context.Writer.WriteEndAttribute();
                }
            }

            //Find the Collections and Type References
            if (context.CompressionLevel >= WriterCompressionLevel.More)
            {
                WriterHelper.FindCollections(context, CollectionSearchMode.All);
            }

            //Get the Triples as a Sorted List
            List<Triple> ts = context.Graph.Triples.Where(t => !context.TriplesDone.Contains(t)).ToList();
            ts.Sort(new RdfXmlTripleComparer());


            INode lastSubj = null;
            List<Triple> sameSubject = new List<Triple>();
            for (int i = 0; i < ts.Count; i++)
            {
                //Find the first group of Triples with the same subject
                if (lastSubj == null)
                {
                    //Start of new set of Triples with the same subject
                    lastSubj = ts[i].Subject;
                    sameSubject.Add(ts[i]);

                    if (lastSubj.NodeType == NodeType.GraphLiteral)
                    {
                        throw new RdfOutputException(WriterErrorMessages.GraphLiteralsUnserializable("RDF/XML"));
                    }
                    else if (lastSubj.NodeType == NodeType.Variable)
                    {
                        throw new RdfOutputException(WriterErrorMessages.VariableNodesUnserializable("RDF/XML"));
                    }
                }
                else
                {
                    if (ts[i].Subject.Equals(lastSubj))
                    {
                        //Still finding Triples with same subject
                        sameSubject.Add(ts[i]);
                    }
                    else
                    {
                        //Found the end of current set of Triples with same subject
                        this.GenerateSubjectOutput(context, sameSubject, true);

                        //Reset so we'll start from a new subject on next iteration
                        sameSubject.Clear();
                        lastSubj = null;
                        i--;
                    }
                }
            }
            //Ensure last set of Triples with same subject gets written
            if (sameSubject.Count > 0)
            {
                this.GenerateSubjectOutput(context, sameSubject, true);
            }

            //Take care of any collections that weren't yet written
            foreach (KeyValuePair<INode, OutputRdfCollection> kvp in context.Collections)
            {
                if (!kvp.Value.HasBeenWritten)
                {
                    //Generate a rdf:Description node and then write the collection
                    context.Writer.WriteStartElement("rdf", "Description", NamespaceMapper.RDF);
                    if (kvp.Value.Triples.Count > 0 || !kvp.Value.IsExplicit) context.Writer.WriteAttributeString("rdf", "nodeID", NamespaceMapper.RDF, context.BlankNodeMapper.GetOutputID((kvp.Key).AnonID));
                    this.GenerateCollectionOutput(context, kvp.Key);
                    context.Writer.WriteEndElement();
                }
            }

            context.NamespaceMap.DecrementNesting();
            context.Writer.WriteEndDocument();

            //Save to the Output Stream
            context.Writer.Flush();
            context.Writer.Close();
        }

        private void GenerateSubjectOutput(RdfXmlWriterContext context, List<Triple> ts, bool allowRdfDescription)
        {
            //If nothing to do return
            if (ts.Count == 0) return;

            context.NamespaceMap.IncrementNesting();

            //First off determine what the XML Element should be
            //If there is a rdf:type triple then create a typed node
            //Otherwise create a rdf:Description node
            INode rdfType = context.Graph.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfType));
            Triple typeTriple = ts.FirstOrDefault(t => t.Predicate.Equals(rdfType) && t.Object.NodeType == NodeType.Uri);
            INode subj;
            if (typeTriple != null)
            {
                //Create Typed Node
                subj = typeTriple.Subject;

                //Generate the Type Reference creating a temporary namespace if necessary
                UriRefType outType;
                IUriNode typeNode = (IUriNode)typeTriple.Object;
                String uriref = this.GenerateUriRef(context, typeNode.Uri, UriRefType.QName, out outType);
                if (outType != UriRefType.QName)
                {
                    //Need to generate a temporary namespace and try generating a QName again
                    String tempPrefix, tempUri;
                    this.GenerateTemporaryNamespace(context, typeNode, out tempPrefix, out tempUri);
 
                    uriref = this.GenerateUriRef(context, typeNode.Uri, UriRefType.QName, out outType);
                    if (outType != UriRefType.QName)
                    {
                        if (allowRdfDescription)
                        {
                            //Still couldn't generate a QName so fall back to rdf:Description
                            //Note that in this case we don't remove the typeTriple from those to be written as we still need to
                            //write it later
                            context.Writer.WriteStartElement("rdf", "Description", NamespaceMapper.RDF);
                        }
                        else
                        {
                            throw new RdfOutputException(WriterErrorMessages.UnreducablePropertyUriUnserializable);
                        }
                    }
                    else
                    {
                        if (uriref.Contains(':'))
                        {
                            //Type Node in relevant namespace
                            context.Writer.WriteStartElement(uriref.Substring(0, uriref.IndexOf(':')), uriref.Substring(uriref.IndexOf(':') + 1), tempUri);
                        }
                        else
                        {
                            //Type Node in default namespace
                            context.Writer.WriteStartElement(uriref);
                        }
                        ts.Remove(typeTriple);
                        context.TriplesDone.Add(typeTriple);
                    }

                    //Remember to define the temporary namespace on the current element
                    context.Writer.WriteAttributeString("xmlns", tempPrefix, null, Uri.EscapeUriString(tempUri));
                }
                else
                {
                    //Generated a valid QName
                    if (uriref.Contains(':'))
                    {
                        //Create an element with appropriate namespace
                        String ns = context.NamespaceMap.GetNamespaceUri(uriref.Substring(0, uriref.IndexOf(':'))).AbsoluteUri;
                        context.Writer.WriteStartElement(uriref.Substring(0, uriref.IndexOf(':')), uriref.Substring(uriref.IndexOf(':') + 1), ns);
                    }
                    else
                    {
                        //Create an element in default namespace
                        context.Writer.WriteStartElement(uriref);
                    }

                    context.TriplesDone.Add(typeTriple);
                    ts.Remove(typeTriple);
                }
            }
            else
            {
                subj = ts.First().Subject;
                if (allowRdfDescription)
                {
                    //Create rdf:Description Node
                    context.Writer.WriteStartElement("rdf", "Description", NamespaceMapper.RDF);
                }
                else
                {
                    throw new RdfOutputException(WriterErrorMessages.UnreducablePropertyUriUnserializable);
                }
            }

            //Always remember to add rdf:about or rdf:nodeID as appropriate
            if (subj.NodeType == NodeType.Uri)
            {
                context.Writer.WriteAttributeString("rdf", "about", NamespaceMapper.RDF, subj.ToString());//Uri.EscapeUriString(subj.ToString()));
            }
            else
            {
                //Can omit the rdf:nodeID if nesting level is > 2 i.e. not a top level subject node
                if (context.NamespaceMap.NestingLevel <= 2)
                {
                    context.Writer.WriteAttributeString("rdf", "nodeID", NamespaceMapper.RDF, context.BlankNodeMapper.GetOutputID((subj).AnonID));
                }
            }

            //If use of attributes is enabled we'll encode triples with simple literal objects
            //as attributes on the subject node directly
            if (context.UseAttributes)
            {
                //Next find any simple literals we can attach directly to the Subject Node
                List<Triple> simpleLiterals = new List<Triple>();
                HashSet<INode> simpleLiteralPredicates = new HashSet<INode>();
                foreach (Triple t in ts)
                {
                    if (t.Object.NodeType == NodeType.Literal)
                    {
                        // TODO When RDF 1.1 is supported this will never be true thus will need a IsSimpleLiteral property
                        if (!t.Object.HasDataType && !t.Object.HasLanguage)
                        {
                            if (!simpleLiteralPredicates.Contains(t.Predicate))
                            {
                                simpleLiteralPredicates.Add(t.Predicate);
                                simpleLiterals.Add(t);
                            }
                        }
                    }
                }

                //Now go ahead and attach these to the Subject Node as attributes
                this.GenerateSimpleLiteralAttributes(context, simpleLiterals);
                simpleLiterals.ForEach(t => context.TriplesDone.Add(t));
                simpleLiterals.ForEach(t => ts.Remove(t));
            }

            //Then generate Predicate Output for each remaining Triple
            foreach (Triple t in ts)
            {
                this.GeneratePredicateOutput(context, t);
                context.TriplesDone.Add(t);
            }

            //Also check for the rare case where the subject is the key to a collection
            if (context.Collections.ContainsKey(subj))
            {
                OutputRdfCollection collection = context.Collections[subj];
                if (!collection.IsExplicit)
                {
                    this.GenerateCollectionItemOutput(context, collection);
                    collection.HasBeenWritten = true;
                }
            }

            context.Writer.WriteEndElement();
            context.NamespaceMap.DecrementNesting();
        }

        private void GenerateSimpleLiteralAttributes(RdfXmlWriterContext context, List<Triple> ts)
        {
            //If nothing to do then return
            if (ts.Count == 0) return;

            //Otherwise attach each Simple Literal directly to the Subject
            foreach (Triple t in ts)
            {
                UriRefType outType;
                IUriNode p = (IUriNode)t.Predicate;
                String uriref = this.GenerateUriRef(context, p.Uri, UriRefType.QName, out outType);
                if (outType != UriRefType.QName)
                {
                    //Need to generate a temporary namespace
                    String tempPrefix, tempUri;
                    this.GenerateTemporaryNamespace(context, p, out tempPrefix, out tempUri);
                    context.Writer.WriteAttributeString("xmlns", tempPrefix, null, Uri.EscapeUriString(tempUri));
                    uriref = this.GenerateUriRef(context, p.Uri, UriRefType.QName, out outType);
                    if (outType != UriRefType.QName) throw new RdfOutputException(WriterErrorMessages.UnreducablePropertyUriUnserializable);
                }
                
                //Output Literal Attribute using the resulting QName
                if (uriref.Contains(':'))
                {
                    //Create an attribute in appropriate namespace
                    String ns = context.NamespaceMap.GetNamespaceUri(uriref.Substring(0, uriref.IndexOf(':'))).AbsoluteUri;
                    context.Writer.WriteAttributeString(uriref.Substring(0, uriref.IndexOf(':')), uriref.Substring(uriref.IndexOf(':') + 1), ns, t.Object.ToString());
                }
                else
                {
                    //Create an attribute in the default namespace
                    context.Writer.WriteAttributeString(uriref, t.Object.ToString());
                }
            }
        }

        private void GeneratePredicateOutput(RdfXmlWriterContext context, Triple t)
        {
            context.NamespaceMap.IncrementNesting();

            //Must ensure a URI predicate
            switch (t.Predicate.NodeType)
            {
                case NodeType.Blank:
                    throw new RdfOutputException(WriterErrorMessages.BlankPredicatesUnserializable("RDF/XML"));
                case NodeType.GraphLiteral:
                    throw new RdfOutputException(WriterErrorMessages.GraphLiteralsUnserializable("RDF/XML"));
                case NodeType.Literal:
                    throw new RdfOutputException(WriterErrorMessages.LiteralPredicatesUnserializable("RDF/XML"));
                case NodeType.Variable:
                    throw new RdfOutputException(WriterErrorMessages.VariableNodesUnserializable("RDF/XML"));
            }
            IUriNode p = (IUriNode)t.Predicate;

            //First generate the Predicate Node
            UriRefType outType;
            String uriref = this.GenerateUriRef(context, p.Uri, UriRefType.QName, out outType);
            String tempPrefix = null, tempUri = null;
            if (outType != UriRefType.QName)
            {
                //Need to generate a temporary namespace
                this.GenerateTemporaryNamespace(context, p, out tempPrefix, out tempUri);
                uriref = this.GenerateUriRef(context, p.Uri, UriRefType.QName, out outType);
                if (outType != UriRefType.QName) throw new RdfOutputException(WriterErrorMessages.UnreducablePropertyUriUnserializable + " - '" + p.Uri + "'");
            }
            //Use the QName for the Node
            if (uriref.Contains(':'))
            {
                //Create an element in the appropriate namespace
                String ns = context.NamespaceMap.GetNamespaceUri(uriref.Substring(0, uriref.IndexOf(':'))).AbsoluteUri;
                context.Writer.WriteStartElement(uriref.Substring(0, uriref.IndexOf(':')), uriref.Substring(uriref.IndexOf(':') + 1), ns);
            }
            else
            {
                //Create an element in the default namespace
                context.Writer.WriteStartElement(uriref);
            }
            if (tempPrefix != null && tempUri != null)
            {
                context.Writer.WriteAttributeString("xmlns", tempPrefix, null, Uri.EscapeUriString(tempUri));
            }

            //Then generate the Object Output
            this.GenerateObjectOutput(context, t);

            context.Writer.WriteEndElement();
            context.NamespaceMap.DecrementNesting();
        }

        private void GenerateObjectOutput(RdfXmlWriterContext context, Triple t)
        {
            //Take different actions depending on the Node Type to be written
            switch (t.Object.NodeType)
            {
                case NodeType.Blank:
                    if (context.Collections.ContainsKey(t.Object))
                    {
                        //Blank Node has a collection associated with it
                        this.GenerateCollectionOutput(context, t.Object);
                    }
                    else
                    {
                        //Isolated Blank Node so use nodeID
                        context.Writer.WriteAttributeString("rdf", "nodeID", NamespaceMapper.RDF, context.BlankNodeMapper.GetOutputID((t.Object).AnonID));
                    }
                    break;

                case NodeType.GraphLiteral:
                    throw new RdfOutputException(WriterErrorMessages.GraphLiteralsUnserializable("RDF/XML"));

                case NodeType.Literal:
                    //Write as content of the current element
                    ILiteralNode lit = (ILiteralNode)t.Object;
                    if (lit.DataType != null)
                    {
                        if (lit.DataType.AbsoluteUri.Equals(RdfSpecsHelper.RdfXmlLiteral))
                        {
                            //XML Literal
                            context.Writer.WriteAttributeString("rdf", "parseType", NamespaceMapper.RDF, "Literal");
                            context.Writer.WriteRaw(lit.Value);
                        }
                        else
                        {
                            //Datatyped Literal
                            context.Writer.WriteAttributeString("rdf", "datatype", NamespaceMapper.RDF, lit.DataType.AbsoluteUri);//Uri.EscapeUriString(lit.DataType.ToString()));
                            context.Writer.WriteString(lit.Value);
                        }
                    }
                    else if (!lit.Language.Equals(String.Empty))
                    {
                        //Language specified Literal
                        context.Writer.WriteAttributeString("xml", "lang", null, lit.Language);
                        context.Writer.WriteString(lit.Value);
                    }
                    else
                    {
                        //Simple Literal
                        context.Writer.WriteString(lit.Value);
                    }
                    break;

                case NodeType.Uri:
                    //Simple rdf:resource
                    //TODO: Compress this into UriRef where possible
                    context.Writer.WriteAttributeString("rdf", "resource", NamespaceMapper.RDF, t.Object.ToString());//Uri.EscapeUriString(t.Object.ToString()));
                    break;

                case NodeType.Variable:
                    throw new RdfOutputException(WriterErrorMessages.VariableNodesUnserializable("RDF/XML"));
            }
        }

        private void GenerateCollectionOutput(RdfXmlWriterContext context, INode key)
        {
            OutputRdfCollection c = context.Collections[key];
            c.HasBeenWritten = true;

            if (c.IsExplicit)
            {
                if (c.Triples.Count == 0)
                {
                    //If No Triples then an isolated blank node so add rdf:nodeID and return
                    context.Writer.WriteAttributeString("rdf", "nodeID", NamespaceMapper.RDF, context.BlankNodeMapper.GetOutputID((key).AnonID));
                    return;
                }

                //First see if there is a typed triple available (only applicable if we have more than one triple)
                INode rdfType = context.Graph.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfType));
                Triple typeTriple = c.Triples.FirstOrDefault(t => t.Predicate.Equals(rdfType) && t.Object.NodeType == NodeType.Uri);
                if (typeTriple != null)
                {
                    //Should be safe to invoke GenerateSubjectOutput but we can't allow rdf:Description
                    this.GenerateSubjectOutput(context, c.Triples, false);
                }
                else
                {
                    //Otherwise we invoke GeneratePredicateOutput (and use rdf:parseType="Resource" if there was more than 1 triple)
                    context.Writer.WriteAttributeString("rdf", "parseType", NamespaceMapper.RDF, "Resource");
                    foreach (Triple t in c.Triples)
                    {
                        this.GeneratePredicateOutput(context, t);
                        context.TriplesDone.Add(t);
                    }
                }
            }
            else
            {
                //If No Triples then use rdf:about rdf:nil and return
                if (c.Triples.Count == 0)
                {
                    context.Writer.WriteAttributeString("rdf", "about", NamespaceMapper.RDF, RdfSpecsHelper.RdfListNil);
                    return;
                }

                //Going to need rdf:parseType="Resource" on current predicate
                context.Writer.WriteAttributeString("rdf", "parseType", NamespaceMapper.RDF, "Resource");

                this.GenerateCollectionItemOutput(context, c);
            }
        }

        private void GenerateCollectionItemOutput(RdfXmlWriterContext context, OutputRdfCollection c)
        {
            //Then output the elements of the Collection
            int toClose = c.Triples.Count;
            while (c.Triples.Count > 0)
            {
                Triple t = c.Triples[0];
                c.Triples.RemoveAt(0);

                //rdf:first Node
                context.Writer.WriteStartElement("rdf", "first", NamespaceMapper.RDF);
                this.GenerateObjectOutput(context, t);

                context.TriplesDone.Add(t);
                context.Writer.WriteEndElement();

                //rdf:rest Node
                context.Writer.WriteStartElement("rdf", "rest", NamespaceMapper.RDF);
            }
            //Terminate the list and close all the open rdf:rest elements
            context.Writer.WriteAttributeString("rdf", "resource", NamespaceMapper.RDF, RdfSpecsHelper.RdfListNil);
            for (int i = 0; i < toClose; i++)
            {
                context.Writer.WriteEndElement();
            }
        }

        private String GenerateUriRef(RdfXmlWriterContext context, Uri u, UriRefType type, out UriRefType outType)
        {
            String uriref, qname;

            if (context.NamespaceMap.ReduceToQName(u.AbsoluteUri, out qname) && RdfXmlSpecsHelper.IsValidQName(qname))
            {
                //Reduced to QName OK
                uriref = qname;
                outType = UriRefType.QName;
            }
            else
            {
                //Just use the Uri
                uriref = u.AbsoluteUri;
                outType = UriRefType.Uri;
            }

            //Convert to a Uri Ref from a QName if required
            if (outType == UriRefType.QName && type == UriRefType.UriRef)
            {
                if (uriref.Contains(':') && !uriref.StartsWith(":"))
                {
                    String prefix = uriref.Substring(0, uriref.IndexOf(':'));
                    if (context.UseDtd && context.NamespaceMap.GetNestingLevel(prefix) == 0)
                    {
                        //Muse have used a DTD to generate this style of URI Reference
                        //Can only use entities for non-temporary Namespaces as Temporary Namespaces won't have Entities defined
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
                //Create a Hash Namespace Uri
                nsUri = uri.Substring(0, uri.LastIndexOf("#") + 1);
            }
            else
            {
                //Create a Slash Namespace Uri
                nsUri = uri.Substring(0, uri.LastIndexOf("/") + 1);
            }

            //Create a Temporary Namespace ID
            //Can't use an ID if already in the Namespace Map either at top level (nesting == 0) or at the current nesting
            while (context.NamespaceMap.HasNamespace("ns" + context.NextNamespaceID) && (context.NamespaceMap.GetNestingLevel("ns" + context.NextNamespaceID) == 0 || context.NamespaceMap.GetNestingLevel("ns" + context.NextNamespaceID) == context.NamespaceMap.NestingLevel))
            {
                context.NextNamespaceID++;
            }
            String prefix = "ns" + context.NextNamespaceID;
            context.NextNamespaceID++;
            context.NamespaceMap.AddNamespace(prefix, UriFactory.Create(nsUri));

            tempPrefix = prefix;
            tempUri = nsUri;

            this.RaiseWarning("Created a Temporary Namespace '" + prefix + "' with URI '" + nsUri + "'");
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

        /// <summary>
        /// Gets the String representation of the writer which is a description of the syntax it produces
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "RDF/XML (Pretty Writer)";
        }
    }
}