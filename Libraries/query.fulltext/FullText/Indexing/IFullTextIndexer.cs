using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query.Datasets;

namespace VDS.RDF.Query.FullText.Indexing
{
    /// <summary>
    /// Indicates the Indexing Mode used by an Indexer to index literals
    /// </summary>
    public enum IndexingMode
    {
        /// <summary>
        /// Indicates that the Indexer stores the original Object Literal whose text is indexed
        /// </summary>
        Objects,
        /// <summary>
        /// Indicates that the Indexer stores the Subject (Blank Node/URI) associated with the Literal whose text is indexed
        /// </summary>
        Subjects,
        /// <summary>
        /// Indicates that the Indexer uses some other custom indexing strategy
        /// </summary>
        Custom
    }

    /// <summary>
    /// Interface for classes that provide full text indexing functionality
    /// </summary>
    public interface IFullTextIndexer
        : IDisposable
    {
        IndexingMode IndexingMode
        {
            get;
        }

        void Index(Triple t);

        void Index(IGraph g);

        void Index(ISparqlDataset dataset);

        void Unindex(Triple t);

        void Unindex(IGraph g);

        void Unindex(ISparqlDataset dataset);

        void Flush();
    }
}
