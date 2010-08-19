using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace VDS.RDF.Writing.Contexts
{
    public class RdfXmlWriterContext : IWriterContext, ICollectionCompressingWriterContext
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

        private int _nextNamespaceID = 0;

        private Dictionary<INode, OutputRDFCollection> _collections = new Dictionary<INode, OutputRDFCollection>();
        private TripleCollection _triplesDone = new TripleCollection();

        public RdfXmlWriterContext(IGraph g, TextWriter output)
        {
            this._g = g;
            this._output = output;
            this._writer = XmlWriter.Create(this._output, this.GetSettings());
            this._nsmapper.Import(this._g.NamespaceMap);
        }

        private XmlWriterSettings GetSettings()
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.ConformanceLevel = ConformanceLevel.Document;
            settings.CloseOutput = true;
            settings.Encoding = Encoding.UTF8;
            settings.Indent = this._prettyPrint;
#if SILVERLIGHT
            settings.NamespaceHandling = NamespaceHandling.OmitDuplicates;
#endif
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
                return this._g;
            }
        }

        /// <summary>
        /// Gets the TextWriter being written to
        /// </summary>
        public TextWriter Output
        {
            get
            {
                return this._output;
            }
        }

        /// <summary>
        /// Gets the XML Writer in use
        /// </summary>
        public XmlWriter Writer
        {
            get
            {
                return this._writer;
            }
        }

        /// <summary>
        /// Gets/Sets the Pretty Printing Mode used
        /// </summary>
        public bool PrettyPrint
        {
            get
            {
                return this._prettyPrint;
            }
            set
            {
                this._prettyPrint = value;
            }
        }

        public NestedNamespaceMapper NamespaceMap
        {
            get
            {
                return this._nsmapper;
            }
        }

        public bool HighSpeedModePermitted
        {
            get
            {
                return false;
            }
            set
            {
                //Do Nothing
            }
        }

        public int CompressionLevel
        {
            get
            {
                return WriterCompressionLevel.Default;
            }
            set
            {
                //Do Nothing
            }
        }

        public int NextNamespaceID
        {
            get
            {
                return this._nextNamespaceID;
            }
            set
            {
                this._nextNamespaceID = value;
            }
        }

        /// <summary>
        /// Represents the mapping from Blank Nodes to Collections
        /// </summary>
        public Dictionary<INode, OutputRDFCollection> Collections
        {
            get
            {
                return this._collections;
            }
        }

        /// <summary>
        /// Stores the Triples that should be excluded from standard output as they are part of collections
        /// </summary>
        public BaseTripleCollection TriplesDone
        {
            get
            {
                return this._triplesDone;
            }
        }

        public string FormatNode(INode n, NodeFormat format)
        {
            throw new NotImplementedException();
        }

        public string FormatUri(string u)
        {
            throw new NotImplementedException();
        }

        public string FormatUri(Uri u)
        {
            throw new NotImplementedException();
        }

        public string FormatChar(char c, NodeFormat format)
        {
            throw new NotImplementedException();
        }
    }
}
