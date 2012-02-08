using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF;

namespace VDS.Alexandria.Indexing
{
    /// <summary>
    /// Interface for Index Manager
    /// </summary>
    public interface IIndexManager : IDisposable
    {
        IEnumerable<Triple> GetTriplesWithSubject(INode subj);

        IEnumerable<Triple> GetTriplesWithPredicate(INode pred);

        IEnumerable<Triple> GetTriplesWithObject(INode obj);

        IEnumerable<Triple> GetTriplesWithSubjectPredicate(INode subj, INode pred);

        IEnumerable<Triple> GetTriplesWithPredicateObject(INode pred, INode obj);

        IEnumerable<Triple> GetTriplesWithSubjectObject(INode subj, INode obj);

        IEnumerable<Triple> GetTriples(Triple t);

        void AddToIndex(Triple t);

        void AddToIndex(IEnumerable<Triple> ts);

        void RemoveFromIndex(Triple t);

        void RemoveFromIndex(IEnumerable<Triple> ts);

        void Flush();
    }
}
