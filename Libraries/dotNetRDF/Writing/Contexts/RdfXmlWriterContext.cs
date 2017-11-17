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
using System.Text;
using System.Xml;
using VDS.RDF.Parsing;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Writing.Contexts
{
    /// <summary>
    /// Writer Context for RDF/XML Writers
    /// </summary>
    public class RdfXmlWriterContext 
        : IWriterContext, ICollectionCompressingWriterContext
    {
        /// <summary>
        /// Pretty Printing Mode setting
        /// </summary>
        protected bool _prettyPrint = true;
        /// <summary>
        /// Graph being written
        /// </summary>
        private IGraph _g;
        /// <summary>
        /// TextWriter being written to
        /// </summary>
        private TextWriter _output;
        /// <summary>
        /// XmlWriter being written to
        /// </summary>
        private XmlWriter _writer;
        /// <summary>
        /// Nested Namespace Mapper
        /// </summary>
        private NestedNamespaceMapper _nsmapper = new NestedNamespaceMapper(true);
        private bool _useDTD = Options.UseDtd;
        private bool _useAttributes = true;
        private int _compressionLevel = WriterCompressionLevel.Default;
        private int _nextNamespaceID = 0;
        private BlankNodeOutputMapper _bnodeMapper = new BlankNodeOutputMapper(XmlSpecsHelper.IsName);
        private Dictionary<INode, OutputRdfCollection> _collections = new Dictionary<INode, OutputRdfCollection>();
        private TripleCollection _triplesDone = new TripleCollection();

        /// <summary>
        /// Creates a new RDF/XML Writer Context
        /// </summary>
        /// <param name="g">Graph</param>
        /// <param name="output">Output destination</param>
        public RdfXmlWriterContext(IGraph g, TextWriter output)
        {
            _g = g;
            _output = output;
            _writer = XmlWriter.Create(_output, GetSettings());
            _nsmapper.Import(_g.NamespaceMap);
        }

        /// <summary>
        /// Generates the required settings for the <see cref="XmlWriter">XmlWriter</see>
        /// </summary>
        /// <returns></returns>
        private XmlWriterSettings GetSettings()
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.ConformanceLevel = ConformanceLevel.Document;
            settings.CloseOutput = true;
            settings.Encoding = new UTF8Encoding(Options.UseBomForUtf8);
            settings.Indent = _prettyPrint;
            settings.NewLineHandling = NewLineHandling.None;
            settings.OmitXmlDeclaration = false;

            return settings;
        }

        /// <summary>
        /// Gets the Graph being written
        /// </summary>
        public IGraph Graph
        {
            get
            {
                return _g;
            }
        }

        /// <summary>
        /// Gets the TextWriter being written to
        /// </summary>
        public TextWriter Output
        {
            get
            {
                return _output;
            }
        }

        /// <summary>
        /// Gets the XML Writer in use
        /// </summary>
        public XmlWriter Writer
        {
            get
            {
                return _writer;
            }
        }

        /// <summary>
        /// Gets/Sets the Pretty Printing Mode used
        /// </summary>
        public bool PrettyPrint
        {
            get
            {
                return _prettyPrint;
            }
            set
            {
                _prettyPrint = value;
            }
        }

        /// <summary>
        /// Gets/Sets the Node Formatter
        /// </summary>
        /// <remarks>
        /// Node Formatters are not used for RDF/XML output
        /// </remarks>
        public INodeFormatter NodeFormatter
        {
            get
            {
                return null;
            }
            set
            {
                throw new NotSupportedException("Node Formatters are not used for RDF/XML output");
            }
        }

        /// <summary>
        /// Gets/Sets the URI Formatter
        /// </summary>
        /// <remarks>
        /// URI Formatters are not used for RDF/XML output
        /// </remarks>
        public IUriFormatter UriFormatter
        {
            get
            {
                return null;
            }
            set
            {
                throw new NotSupportedException("URI Formatters are not used for RDF/XML output");
            }
        }

        /// <summary>
        /// Gets the Namespace Map in use
        /// </summary>
        public NestedNamespaceMapper NamespaceMap
        {
            get
            {
                return _nsmapper;
            }
        }

        /// <summary>
        /// Gets the Blank Node map in use
        /// </summary>
        public BlankNodeOutputMapper BlankNodeMapper
        {
            get
            {
                return _bnodeMapper;
            }
        }

        /// <summary>
        /// Gets/Sets whether High Speed Mode is permitted
        /// </summary>
        /// <remarks>
        /// Not currently supported
        /// </remarks>
        public bool HighSpeedModePermitted
        {
            get
            {
                return false;
            }
            set
            {
                // Do Nothing
            }
        }

        /// <summary>
        /// Gets/Sets the Compression Level used
        /// </summary>
        /// <remarks>
        /// Not currently supported
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
        /// Gets/Sets the next ID to use for issuing Temporary Namespaces
        /// </summary>
        public int NextNamespaceID
        {
            get
            {
                return _nextNamespaceID;
            }
            set
            {
                _nextNamespaceID = value;
            }
        }

        /// <summary>
        /// Gets/Sets whether a DTD is used
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
        /// Gets/Sets whether attributes are used to encode the predicates and objects of triples with simple literal properties
        /// </summary>
        public bool UseAttributes
        {
            get
            {
                return _useAttributes;
            }
            set
            {
                _useAttributes = value;
            }
        }

        /// <summary>
        /// Represents the mapping from Blank Nodes to Collections
        /// </summary>
        public Dictionary<INode, OutputRdfCollection> Collections
        {
            get
            {
                return _collections;
            }
        }

        /// <summary>
        /// Stores the Triples that should be excluded from standard output as they are part of collections
        /// </summary>
        public BaseTripleCollection TriplesDone
        {
            get
            {
                return _triplesDone;
            }
        }
    }
}
