using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.Common;

namespace VDS.RDF.Query.Datasets
{
    public abstract class BaseQuadDataset
        : ISparqlDataset
    {
        private readonly ThreadIsolatedReference<Stack<IEnumerable<Uri>>> _defaultGraphs;
        private readonly ThreadIsolatedReference<Stack<IEnumerable<Uri>>> _activeGraphs;
        private bool _unionDefaultGraph = true;
        private Uri _defaultGraphUri;

        public BaseQuadDataset()
        {
            this._defaultGraphs = new ThreadIsolatedReference<Stack<IEnumerable<Uri>>>(this.InitDefaultGraphStack);
            this._activeGraphs = new ThreadIsolatedReference<Stack<IEnumerable<Uri>>>(this.InitActiveGraphStack);
        }

        public BaseQuadDataset(bool unionDefaultGraph)
            : this()
        {
            this._unionDefaultGraph = unionDefaultGraph;
        }

        public BaseQuadDataset(Uri defaultGraphUri)
            : this(false)
        {
            this._defaultGraphUri = defaultGraphUri;
        }

        private Stack<IEnumerable<Uri>> InitDefaultGraphStack()
        {
            Stack<IEnumerable<Uri>> s = new Stack<IEnumerable<Uri>>();
            if (!this._unionDefaultGraph)
            {
                s.Push(new Uri[] { this._defaultGraphUri });
            }
            return s;
        }

        private Stack<IEnumerable<Uri>> InitActiveGraphStack()
        {
            return new Stack<IEnumerable<Uri>>();
        }

        #region Active and Default Graph management

        public void SetActiveGraph(IEnumerable<Uri> graphUris)
        {
            this._activeGraphs.Value.Push(graphUris.ToList());
        }

        public void SetActiveGraph(Uri graphUri)
        {
            if (graphUri == null)
            {
                this._activeGraphs.Value.Push(this.DefaultGraphUris);
            }
            else
            {
                this._activeGraphs.Value.Push(new Uri[] { graphUri });
            }
        }

        public void SetDefaultGraph(Uri graphUri)
        {
            this._defaultGraphs.Value.Push(new Uri[] { graphUri });
        }

        public void SetDefaultGraph(IEnumerable<Uri> graphUris)
        {
            this._defaultGraphs.Value.Push(graphUris.ToList());
        }

        public void ResetActiveGraph()
        {
            if (this._activeGraphs.Value.Count > 0)
            {
                this._activeGraphs.Value.Pop();
            }
            else
            {
                throw new RdfQueryException("Unable to reset the Active Graph since no previous Active Graphs exist");
            }
        }

        public void ResetDefaultGraph()
        {
            if (this._defaultGraphs.Value.Count > 0)
            {
                this._defaultGraphs.Value.Pop();
            }
            else
            {
                throw new RdfQueryException("Unable to reset the Default Graph since no previous Default Graphs exist");
            }
        }

        public IGraph DefaultGraph
        {
            get { throw new NotImplementedException(); }
        }

        public IEnumerable<Uri> DefaultGraphUris
        {
            get 
            {
                if (this._defaultGraphs.Value.Count > 0)
                {
                    return this._defaultGraphs.Value.Peek();
                }
                else
                {
                    return Enumerable.Empty<Uri>();
                }
            }
        }

        public IGraph ActiveGraph
        {
            get { throw new NotImplementedException(); }
        }

        public IEnumerable<Uri> ActiveGraphUris
        {
            get 
            {
                if (this._activeGraphs.Value.Count > 0)
                {
                    return this._activeGraphs.Value.Peek();
                }
                else
                {
                    return Enumerable.Empty<Uri>();
                }
            }
        }

        public bool UsesUnionDefaultGraph
        {
            get 
            {
                return this._unionDefaultGraph;
            }
        }

        #endregion

        public void AddGraph(IGraph g)
        {
            throw new NotSupportedException("This dataset is immutable");
        }

        public void RemoveGraph(Uri graphUri)
        {
            throw new NotSupportedException("This dataset is immutable");
        }

        public abstract bool HasGraph(Uri graphUri);

        public IEnumerable<IGraph> Graphs
        {
            get 
            { 
                throw new NotImplementedException(); 
            }
        }

        public abstract IEnumerable<Uri> GraphUris
        {
            get;
        }

        public IGraph this[Uri graphUri]
        {
            get 
            { 
                throw new NotImplementedException(); 
            }
        }

        public IGraph GetModifiableGraph(Uri graphUri)
        {
            throw new NotSupportedException("This dataset is immutable");
        }

        public bool HasTriples
        {
            get 
            {
                return this.Triples.Any();
            }
        }

        public bool ContainsTriple(Triple t)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Triple> Triples
        {
            get { throw new NotImplementedException(); }
        }

        public IEnumerable<Triple> GetTriplesWithSubject(INode subj)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Triple> GetTriplesWithPredicate(INode pred)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Triple> GetTriplesWithObject(INode obj)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Triple> GetTriplesWithSubjectPredicate(INode subj, INode pred)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Triple> GetTriplesWithSubjectObject(INode subj, INode obj)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Triple> GetTriplesWithPredicateObject(INode pred, INode obj)
        {
            throw new NotImplementedException();
        }

        public void Flush()
        {
            //Nothing to do
        }

        public void Discard()
        {
            //Nothing to do
        }
    }
}
