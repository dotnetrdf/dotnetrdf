using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.IO;
using System.Linq;
using System.Text;
using VDS.RDF;
using Alexandria.Documents;
using Alexandria.Indexing;

namespace Alexandria
{
    /// <summary>
    /// Manages an Alexandria Store which is stored in a directory in the filesystem
    /// </summary>
    public class AlexandriaFileManager : AlexandriaDocumentStoreManager<StreamReader,TextWriter>
    {
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
    }

    public class NonIndexedAlexandriaFileManager : AlexandriaFileManager
    {
        public NonIndexedAlexandriaFileManager(String directory)
            : base(directory, null) { }
    }
}
