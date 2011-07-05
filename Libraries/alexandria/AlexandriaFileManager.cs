using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.IO;
using System.Linq;
using System.Text;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.Alexandria.Datasets;
using VDS.Alexandria.Documents;
using VDS.Alexandria.Indexing;

namespace VDS.Alexandria
{
    /// <summary>
    /// Manages an Alexandria Store which is stored in a directory in the filesystem
    /// </summary>
    public class AlexandriaFileManager : AlexandriaDocumentStoreManager<StreamReader,TextWriter>
    {
        private LeviathanQueryProcessor _processor;
        private SparqlQueryParser _parser;

        /// <summary>
        /// Set of All Indices - gives best query performance but poorer import performance
        /// </summary>
        public static TripleIndexType[] FullIndices = new TripleIndexType[]
        {
            TripleIndexType.NoVariables,
            TripleIndexType.Object,
            TripleIndexType.Predicate,
            TripleIndexType.PredicateObject,
            TripleIndexType.Subject,
            TripleIndexType.SubjectObject,
            TripleIndexType.SubjectPredicate
        };

        /// <summary>
        /// Optimal Indexes for best data import/query compromise (this is the Default)
        /// </summary>
        public static TripleIndexType[] OptimalIndices = new TripleIndexType[]
        {
            TripleIndexType.Object,
            TripleIndexType.Predicate,
            TripleIndexType.Subject,
            TripleIndexType.SubjectPredicate
        };

        /// <summary>
        /// Simple Indexes for fastest data import times
        /// </summary>
        public static TripleIndexType[] SimpleIndices = new TripleIndexType[]
        {
            TripleIndexType.Predicate,
            TripleIndexType.Subject,
        };

        public AlexandriaFileManager(FileDocumentManager manager)
            : base(manager, new FileIndexManager(manager)) { }

        public AlexandriaFileManager(FileDocumentManager manager, IEnumerable<TripleIndexType> indices)
            : base(manager, new FileIndexManager(manager, indices)) { }

        public AlexandriaFileManager(String directory)
            : this(new FileDocumentManager(directory)) { }

        public AlexandriaFileManager(String directory, IEnumerable<TripleIndexType> indices)
            : this(new FileDocumentManager(directory), indices) { }

        public override object Query(string sparqlQuery)
        {
            if (this._processor == null)
            {
                AlexandriaFileDataset dataset = new AlexandriaFileDataset(this);
                this._processor = new LeviathanQueryProcessor(dataset);
            }
            if (this._parser == null) this._parser = new SparqlQueryParser();
            return this._processor.ProcessQuery(this._parser.ParseFromString(sparqlQuery));
        }

        public override void Query(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, string sparqlQuery)
        {
            if (this._processor == null)
            {
                AlexandriaFileDataset dataset = new AlexandriaFileDataset(this);
                this._processor = new LeviathanQueryProcessor(dataset);
            }
            if (this._parser == null) this._parser = new SparqlQueryParser();
            this._processor.ProcessQuery(rdfHandler, resultsHandler, this._parser.ParseFromString(sparqlQuery));
        }
    }

    public class NonIndexedAlexandriaFileManager : AlexandriaFileManager
    {
        public NonIndexedAlexandriaFileManager(String directory)
            : base(directory, null) { }
    }
}
