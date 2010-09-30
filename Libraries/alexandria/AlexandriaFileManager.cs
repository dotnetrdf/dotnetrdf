using System;
using System.Collections.Generic;
using System.Security.Cryptography;
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
    public class AlexandriaFileManager : AlexandriaSha256HashingManager
    {
        /// <summary>
        /// Set of Default Indices, if you don't specify your indices this is what you get - gives best query performance
        /// </summary>
        public static TripleIndexType[] DefaultIndices = new TripleIndexType[]
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
        /// Partial Indexes for best data import/query compromise
        /// </summary>
        public static TripleIndexType[] PartialIndices = new TripleIndexType[]
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
