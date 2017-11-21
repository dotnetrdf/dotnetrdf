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

using System.Collections.Generic;
using System.IO;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Writing.Contexts
{
    /// <summary>
    /// Interface for Writer Contexts
    /// </summary>
    public interface IWriterContext
    {
        /// <summary>
        /// Gets the Graph being written
        /// </summary>
        IGraph Graph
        {
            get;
        }

        /// <summary>
        /// Gets the TextWriter being written to
        /// </summary>
        TextWriter Output
        {
            get;
        }

        /// <summary>
        /// Gets/Sets the Pretty Printing Mode used
        /// </summary>
        bool PrettyPrint
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets the High Speed Mode used
        /// </summary>
        bool HighSpeedModePermitted
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets the Compression Level used
        /// </summary>
        int CompressionLevel
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets the Node Formatter used
        /// </summary>
        INodeFormatter NodeFormatter
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets the URI Formatter used
        /// </summary>
        IUriFormatter UriFormatter
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Interface for Writer Contexts which store collection compression data
    /// </summary>
    public interface ICollectionCompressingWriterContext : IWriterContext
    {
        /// <summary>
        /// Gets the mapping from Blank Nodes to Collections
        /// </summary>
        Dictionary<INode, OutputRdfCollection> Collections
        {
            get;
        }

        /// <summary>
        /// Gets the Triples that should be excluded from standard output as they are part of collections
        /// </summary>
        BaseTripleCollection TriplesDone
        {
            get;
        }
    }

    /// <summary>
    /// Base Class for Writer Context Objects
    /// </summary>
    /// <remarks>
    /// This is not an abstract class since some writers will require only this information or possibly less
    /// </remarks>
    public class BaseWriterContext 
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
        /// Graph being written
        /// </summary>
        private IGraph _g;
        /// <summary>
        /// TextWriter being written to
        /// </summary>
        private TextWriter _output;
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
        /// <param name="g">Graph being written</param>
        /// <param name="output">TextWriter being written to</param>
        public BaseWriterContext(IGraph g, TextWriter output)
        {
            _g = g;
            _output = output;
            _qnameMapper = new QNameOutputMapper(_g.NamespaceMap);
        }

        /// <summary>
        /// Creates a new Base Writer Context with custom settings
        /// </summary>
        /// <param name="g">Graph being written</param>
        /// <param name="output">TextWriter being written to</param>
        /// <param name="compressionLevel">Compression Level</param>
        public BaseWriterContext(IGraph g, TextWriter output, int compressionLevel)
            : this(g, output)
        {
            _compressionLevel = compressionLevel;
        }

        /// <summary>
        /// Creates a new Base Writer Context with custom settings
        /// </summary>
        /// <param name="g">Graph being written</param>
        /// <param name="output">TextWriter being written to</param>
        /// <param name="compressionLevel">Compression Level</param>
        /// <param name="prettyPrint">Pretty Print Mode</param>
        /// <param name="hiSpeedAllowed">High Speed Mode</param>
        public BaseWriterContext(IGraph g, TextWriter output, int compressionLevel, bool prettyPrint, bool hiSpeedAllowed)
            : this(g, output)
        {
            _compressionLevel = compressionLevel;
            _prettyPrint = prettyPrint;
            _hiSpeedAllowed = hiSpeedAllowed;
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
        /// Gets the QName Output Mapper in use
        /// </summary>
        public QNameOutputMapper QNameMapper
        {
            get
            {
                return _qnameMapper;
            }
        }

        /// <summary>
        /// Gets/Sets the Compression Level used
        /// </summary>
        public virtual int CompressionLevel
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
        /// Gets/Sets the High Speed Mode used
        /// </summary>
        public bool HighSpeedModePermitted
        {
            get
            {
                return _hiSpeedAllowed;
            }
            set
            {
                _hiSpeedAllowed = value;
            }
        }

        /// <summary>
        /// Gets/Sets the Node Formatter in use
        /// </summary>
        public INodeFormatter NodeFormatter
        {
            get
            {
                return _formatter;
            }
            set
            {
                _formatter = value;
            }
        }

        /// <summary>
        /// Gets/Sets the URI Formatter in use
        /// </summary>
        public IUriFormatter UriFormatter
        {
            get
            {
                if (_uriFormatter == null)
                {
                    // If no URI Formatter set but the Node Formatter used is also a URI Formatter return that instead
                    if (_formatter is IUriFormatter)
                    {
                        return (IUriFormatter)_formatter;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return _uriFormatter;
                }
            }
            set
            {
                _uriFormatter = value;
            }
        }
    }
}