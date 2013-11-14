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

using System.IO;
using VDS.RDF.Graphs;
using VDS.RDF.Namespaces;
using VDS.RDF.Nodes;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Writing.Contexts
{
    /// <summary>
    /// Base class for writer contextx
    /// </summary>
    /// <remarks>
    /// This is not an abstract class since some writers will require only this information or possibly less
    /// </remarks>
    public abstract class BaseWriterContext
        : IWriterContext
    {
        /// <summary>
        /// Compression Level to be used
        /// </summary>
        protected int _compressionLevel = WriterCompressionLevel.Default;

        /// <summary>
        /// Pretty Printing Mode setting
        /// </summary>
        protected bool _prettyPrint = true;

        /// <summary>
        /// High Speed Mode setting
        /// </summary>
        protected bool _hiSpeedAllowed = true;

        /// <summary>
        /// TextWriter being written to
        /// </summary>
        private readonly TextWriter _output;

        /// <summary>
        /// QName Output Mapper
        /// </summary>
        protected QNameOutputMapper _qnameMapper;

        /// <summary>
        /// Node Formatter
        /// </summary>
        protected INodeFormatter _formatter;

        /// <summary>
        /// URI Formatter
        /// </summary>
        protected IUriFormatter _uriFormatter;

        /// <summary>
        /// Creates a new Base Writer Context with default settings
        /// </summary>
        /// <param name="namespaces">Namespaces to use</param>
        /// <param name="output">TextWriter being written to</param>
        public BaseWriterContext(INamespaceMapper namespaces, TextWriter output)
        {
            this._output = output;
            this._qnameMapper = new QNameOutputMapper(namespaces);
        }

        /// <summary>
        /// Creates a new Base Writer Context with custom settings
        /// </summary>
        /// <param name="namespaces">Namespaces to use</param>
        /// <param name="output">TextWriter being written to</param>
        /// <param name="compressionLevel">Compression Level</param>
        public BaseWriterContext(INamespaceMapper namespaces, TextWriter output, int compressionLevel)
            : this(namespaces, output)
        {
            this._compressionLevel = compressionLevel;
        }

        /// <summary>
        /// Creates a new Base Writer Context with custom settings
        /// </summary>
        /// <param name="namespaces">Namespaces to use</param>
        /// <param name="output">TextWriter being written to</param>
        /// <param name="compressionLevel">Compression Level</param>
        /// <param name="prettyPrint">Pretty Print Mode</param>
        /// <param name="hiSpeedAllowed">High Speed Mode</param>
        public BaseWriterContext(INamespaceMapper namespaces, TextWriter output, int compressionLevel, bool prettyPrint, bool hiSpeedAllowed)
            : this(namespaces, output)
        {
            this._compressionLevel = compressionLevel;
            this._prettyPrint = prettyPrint;
            this._hiSpeedAllowed = hiSpeedAllowed;
        }

        /// <summary>
        /// Gets the TextWriter being written to
        /// </summary>
        public TextWriter Output
        {
            get { return this._output; }
        }

        /// <summary>
        /// Gets the QName Output Mapper in use
        /// </summary>
        public QNameOutputMapper QNameMapper
        {
            get { return this._qnameMapper; }
        }

        /// <summary>
        /// Gets/Sets the Compression Level used
        /// </summary>
        public virtual int CompressionLevel
        {
            get { return this._compressionLevel; }
            set { this._compressionLevel = value; }
        }

        /// <summary>
        /// Gets/Sets the Pretty Printing Mode used
        /// </summary>
        public bool PrettyPrint
        {
            get { return this._prettyPrint; }
            set { this._prettyPrint = value; }
        }

        /// <summary>
        /// Gets/Sets the High Speed Mode used
        /// </summary>
        public bool HighSpeedModePermitted
        {
            get { return this._hiSpeedAllowed; }
            set { this._hiSpeedAllowed = value; }
        }

        /// <summary>
        /// Gets/Sets the Node Formatter in use
        /// </summary>
        public INodeFormatter NodeFormatter
        {
            get { return this._formatter; }
            set { this._formatter = value; }
        }

        /// <summary>
        /// Gets/Sets the URI Formatter in use
        /// </summary>
        public IUriFormatter UriFormatter
        {
            get
            {
                if (this._uriFormatter == null)
                {
                    //If no URI Formatter set but the Node Formatter used is also a URI Formatter return that instead
                    if (this._formatter is IUriFormatter)
                    {
                        return (IUriFormatter) this._formatter;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return this._uriFormatter;
                }
            }
            set { this._uriFormatter = value; }
        }
    }

    /// <summary>
    /// Base Class for graph writer contexts
    /// </summary>
    public abstract class BaseGraphWriterContext
        : BaseWriterContext, IGraphWriterContext
    {
        protected BaseGraphWriterContext(IGraph g, TextWriter output)
            : base(g.Namespaces, output)
        {
            this.Graph = g;
        }

        protected BaseGraphWriterContext(IGraph g, TextWriter output, int compressionLevel)
            : base(g.Namespaces, output, compressionLevel)
        {
            this.Graph = g;
        }

        protected BaseGraphWriterContext(IGraph g, TextWriter output, int compressionLevel, bool prettyPrint, bool hiSpeedAllowed)
            : base(g.Namespaces, output, compressionLevel, prettyPrint, hiSpeedAllowed)
        {
            this.Graph = g;
        }

        /// <summary>
        /// Gets the Graph being written
        /// </summary>
        public IGraph Graph { get; private set; }
    }

    /// <summary>
    /// Base class for graph store writer contexts
    /// </summary>
    public abstract class BaseGraphStoreWriterContext
        : BaseWriterContext, IGraphStoreWriterContext
    {

        protected BaseGraphStoreWriterContext(IGraphStore graphStore, INamespaceMapper namespaces, TextWriter output)
            : base(namespaces, output)
        {
            this.GraphStore = graphStore;
        }

        protected BaseGraphStoreWriterContext(IGraphStore graphStore, INamespaceMapper namespaces, TextWriter output, int compressionLevel)
            : base(namespaces, output, compressionLevel)
        {
            this.GraphStore = graphStore;
        }

        protected BaseGraphStoreWriterContext(IGraphStore graphStore, INamespaceMapper namespaces, TextWriter output, int compressionLevel, bool prettyPrint, bool hiSpeedAllowed)
            : base(namespaces, output, compressionLevel, prettyPrint, hiSpeedAllowed)
        {
            this.GraphStore = graphStore;
        }

        public IGraphStore GraphStore { get; private set; }

        public IGraph CurrentGraph { get; set; }

        public INode CurrentGraphName { get; set; }
    }
}