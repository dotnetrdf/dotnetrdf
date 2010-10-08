using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Query.Datasets
{
    public interface ISparqlDataset
    {

        #region Active and Default Graph Management

        void SetActiveGraph(IEnumerable<Uri> graphUris);

        void SetActiveGraph(Uri graphUri);

        void SetActiveGraph(IGraph g);

        void SetDefaultGraph(IGraph g);

        void ResetActiveGraph();

        void ResetDefaultGraph();

        #endregion

        #region Graph Existence and Retrieval

        void AddGraph(IGraph g);

        void RemoveGraph(Uri graphUri);

        bool HasGraph(Uri graphUri);

        IEnumerable<IGraph> Graphs
        {
            get;
        }

        IEnumerable<Uri> GraphUris
        {
            get;
        }

        IGraph this[Uri graphUri]
        {
            get;
        }

        #endregion

        #region Triple Existence and Retrieval

        bool HasTriples
        {
            get;
        }

        bool ContainsTriple(Triple t);

        IEnumerable<Triple> Triples
        {
            get;
        }

        IEnumerable<Triple> GetTriplesWithSubject(INode subj);

        IEnumerable<Triple> GetTriplesWithPredicate(INode pred);

        IEnumerable<Triple> GetTriplesWithObject(INode obj);

        IEnumerable<Triple> GetTriplesWithSubjectPredicate(INode subj, INode pred);

        IEnumerable<Triple> GetTriplesWithSubjectObject(INode subj, INode obj);

        IEnumerable<Triple> GetTriplesWithPredicateObject(INode pred, INode obj);

        #endregion

        void Flush();
    }
}
