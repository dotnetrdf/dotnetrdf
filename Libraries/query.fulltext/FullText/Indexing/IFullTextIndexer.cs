using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query.Datasets;

namespace VDS.RDF.Query.FullText.Indexing
{
    /// <summary>
    /// Interface for classes that provide full text indexing functionality
    /// </summary>
    public interface IFullTextIndexer
    {
        void Index(Triple t);

        void Index(IGraph g);

        void Index(ISparqlDataset dataset);

        void Unindex(Triple t);

        void Unindex(IGraph g);

        void Unindex(ISparqlDataset dataset);
    }
}
