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
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using VDS.RDF.Parsing;
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
    public class BaseWriterContext : IWriterContext
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
            this._g = g;
            this._output = output;
            this._qnameMapper = new QNameOutputMapper(this._g.NamespaceMap);
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
            this._compressionLevel = compressionLevel;
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
            this._compressionLevel = compressionLevel;
            this._prettyPrint = prettyPrint;
            this._hiSpeedAllowed = hiSpeedAllowed;
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
        /// Gets the QName Output Mapper in use
        /// </summary>
        public QNameOutputMapper QNameMapper
        {
            get
            {
                return this._qnameMapper;
            }
        }

        /// <summary>
        /// Gets/Sets the Compression Level used
        /// </summary>
        public virtual int CompressionLevel
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

        /// <summary>
        /// Gets/Sets the High Speed Mode used
        /// </summary>
        public bool HighSpeedModePermitted
        {
            get
            {
                return this._hiSpeedAllowed;
            }
            set
            {
                this._hiSpeedAllowed = value;
            }
        }

        /// <summary>
        /// Gets/Sets the Node Formatter in use
        /// </summary>
        public INodeFormatter NodeFormatter
        {
            get
            {
                return this._formatter;
            }
            set
            {
                this._formatter = value;
            }
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
                        return (IUriFormatter)this._formatter;
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
            set
            {
                this._uriFormatter = value;
            }
        }
    }
}