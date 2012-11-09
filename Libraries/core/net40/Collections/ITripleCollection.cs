using System;
using System.Collections.Generic;
using VDS.RDF;

namespace VDS.RDF.Collections
{
    /// <summary>
    /// Interface for Triple Collections
    /// </summary>
    public interface ITripleCollection
        : IRdfCollection<Triple>
    {
        IEnumerable<INode> ObjectNodes { get; }

        IEnumerable<INode> PredicateNodes { get; }

        IEnumerable<INode> SubjectNodes { get; }

        event TripleEventHandler TripleAdded;

        event TripleEventHandler TripleRemoved;

        IEnumerable<Triple> WithObject(INode obj);

        IEnumerable<Triple> WithPredicate(INode pred);

        IEnumerable<Triple> WithPredicateObject(INode pred, INode obj);

        IEnumerable<Triple> WithSubject(INode subj);

        IEnumerable<Triple> WithSubjectObject(INode subj, INode obj);

        IEnumerable<Triple> WithSubjectPredicate(INode subj, INode pred);
    }
}
