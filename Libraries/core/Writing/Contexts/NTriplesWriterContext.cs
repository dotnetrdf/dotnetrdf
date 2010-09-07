using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Writing.Contexts
{
    public class NTriplesWriterContext : BaseWriterContext
    {
        /// <summary>
        /// Creates a new NTriples Writer Context with default settings
        /// </summary>
        /// <param name="g">Graph to write</param>
        /// <param name="output">TextWriter to write to</param>
        public NTriplesWriterContext(IGraph g, TextWriter output)
            : base(g, output) 
        {
            this._formatter = new NTriplesFormatter();
            this._uriFormatter = (IUriFormatter)this._formatter;
        }

        /// <summary>
        /// Creates a new NTriples Writer Context with custom settings
        /// </summary>
        /// <param name="g">Graph to write</param>
        /// <param name="output">TextWriter to write to</param>
        /// <param name="prettyPrint">Pretty Print Mode</param>
        /// <param name="hiSpeed">High Speed Mode</param>
        public NTriplesWriterContext(IGraph g, TextWriter output, bool prettyPrint, bool hiSpeed)
            : base(g, output, WriterCompressionLevel.Default, prettyPrint, hiSpeed) 
        {
            this._formatter = new NTriplesFormatter();
            this._uriFormatter = (IUriFormatter)this._formatter;
        }
    }
}
