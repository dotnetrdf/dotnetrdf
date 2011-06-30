using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF;

namespace VDS.Alexandria.Documents.Adaptors
{
    /// <summary>
    /// An Adaptor which coverts between Data (Graphs and Triples) and Documents
    /// </summary>
    public interface IDataAdaptor<TReader, TWriter>
    {
        /// <summary>
        /// Converts the contents of a Document into a Graph
        /// </summary>
        /// <param name="g">Graph to convert into</param>
        /// <param name="document">Document to convert from</param>
        void ToGraph(IGraph g, IDocument<TReader,TWriter> document);

        void ToHandler(IRdfHandler handler, IDocument<TReader, TWriter> document);

        /// <summary>
        /// Converts the contents of a Graph into a Document
        /// </summary>
        /// <param name="g">Graph to convert from</param>
        /// <param name="document">Document to convert to</param>
        void ToDocument(IGraph g, IDocument<TReader,TWriter> document);

        /// <summary>
        /// Appends Triples to a Document
        /// </summary>
        /// <param name="ts">Triples</param>
        /// <param name="document">Document</param>
        void AppendTriples(IEnumerable<Triple> ts, IDocument<TReader,TWriter> document);

        /// <summary>
        /// Deletes Triples from a Document
        /// </summary>
        /// <param name="ts">Triples</param>
        /// <param name="document">Document</param>
        void DeleteTriples(IEnumerable<Triple> ts, IDocument<TReader,TWriter> document);
    }
}
