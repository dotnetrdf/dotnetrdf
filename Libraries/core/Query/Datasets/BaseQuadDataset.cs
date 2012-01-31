/*

Copyright Robert Vesse 2009-12
rvesse@vdesign-studios.com

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.Common;

namespace VDS.RDF.Query.Datasets
{
    /// <summary>
    /// Abstract Base class of dataset designed around out of memory datasets where you rarely wish to load data into memory but simply wish to know which graph to look in for data
    /// </summary>
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

        public IEnumerable<Uri> DefaultGraphUris
        {
            get 
            {
                if (this._defaultGraphs.Value.Count > 0)
                {
                    return this._defaultGraphs.Value.Peek();
                }
                else if (this._unionDefaultGraph)
                {
                    return this.GraphUris;
                }
                else
                {
                    return Enumerable.Empty<Uri>();
                }
            }
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
                    return this.DefaultGraphUris;
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

        public abstract void AddGraph(IGraph g);

        /// <summary>
        /// Removes a Graph from the Dataset
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        public abstract void RemoveGraph(Uri graphUri);

        public abstract bool HasGraph(Uri graphUri);

        public virtual IEnumerable<IGraph> Graphs
        {
            get 
            {
                return (from u in this.GraphUris
                        select this[u]);
            }
        }

        public abstract IEnumerable<Uri> GraphUris
        {
            get;
        }

        public abstract IGraph this[Uri graphUri]
        {
            get;
        }

        public virtual IGraph GetModifiableGraph(Uri graphUri)
        {
            throw new NotSupportedException("This dataset is immutable");
        }

        public virtual bool HasTriples
        {
            get 
            {
                return this.Triples.Any();
            }
        }

        public bool ContainsTriple(Triple t)
        {
            return this.ActiveGraphUris.Any(u => this.ContainsQuad(u, t));
        }

        /// <summary>
        /// Gets whether a Triple exists in a specific Graph of the dataset
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <param name="t">Triple</param>
        /// <returns></returns>
        protected abstract bool ContainsQuad(Uri graphUri, Triple t);

        public IEnumerable<Triple> Triples
        {
            get 
            { 
                return (from u in this.ActiveGraphUris
                        from t in this.GetTriples(u)
                        select t);
            }
        }

        /// <summary>
        /// Gets all the Triples for a specific graph of the dataset
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <returns></returns>
        protected abstract IEnumerable<Triple> GetTriples(Uri graphUri);

        public IEnumerable<Triple> GetTriplesWithSubject(INode subj)
        {
            return (from u in this.ActiveGraphUris
                    from t in this.GetQuadsWithSubject(u, subj)
                    select t);
        }

        /// <summary>
        /// Gets all the Triples with a given subject from a specific graph of the dataset
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <param name="subj">Subject</param>
        /// <returns></returns>
        protected abstract IEnumerable<Triple> GetQuadsWithSubject(Uri graphUri, INode subj);

        public IEnumerable<Triple> GetTriplesWithPredicate(INode pred)
        {
            return (from u in this.ActiveGraphUris
                    from t in this.GetQuadsWithPredicate(u, pred)
                    select t);
        }

        /// <summary>
        /// Gets all the Triples with a given predicate from a specific graph of the dataset
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <param name="pred">Predicate</param>
        /// <returns></returns>
        protected abstract IEnumerable<Triple> GetQuadsWithPredicate(Uri graphUri, INode pred);

        public IEnumerable<Triple> GetTriplesWithObject(INode obj)
        {
            return (from u in this.ActiveGraphUris
                    from t in this.GetQuadsWithObject(u, obj)
                    select t);
        }

        /// <summary>
        /// Gets all the Triples with a given object from a specific graph of the dataset
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        protected abstract IEnumerable<Triple> GetQuadsWithObject(Uri graphUri, INode obj);

        public IEnumerable<Triple> GetTriplesWithSubjectPredicate(INode subj, INode pred)
        {
            return (from u in this.ActiveGraphUris
                    from t in this.GetQuadsWithSubjectPredicate(u, subj, pred)
                    select t);
        }

        /// <summary>
        /// Gets all the Triples with a given subject and predicate from a specific graph of the dataset
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <param name="subj">Subject</param>
        /// <param name="pred">Predicate</param>
        /// <returns></returns>
        protected abstract IEnumerable<Triple> GetQuadsWithSubjectPredicate(Uri graphUri, INode subj, INode pred);

        public IEnumerable<Triple> GetTriplesWithSubjectObject(INode subj, INode obj)
        {
            return (from u in this.ActiveGraphUris
                    from t in this.GetQuadsWithSubjectObject(u, subj, obj)
                    select t);
        }

        /// <summary>
        /// Gets all the Triples with a given subject and object from a specific graph of the dataset
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <param name="subj">Subject</param>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        protected abstract IEnumerable<Triple> GetQuadsWithSubjectObject(Uri graphUri, INode subj, INode obj);

        public IEnumerable<Triple> GetTriplesWithPredicateObject(INode pred, INode obj)
        {
            return (from u in this.ActiveGraphUris
                    from t in this.GetQuadsWithPredicateObject(u, pred, obj)
                    select t);
        }

        /// <summary>
        /// Gets all the Triples with a given predicate and object from a specific graph of the dataset
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <param name="obj">Predicate</param>
        /// <param name="subj">Object</param>
        /// <returns></returns>
        protected abstract IEnumerable<Triple> GetQuadsWithPredicateObject(Uri graphUri, INode pred, INode obj);

        public virtual void Flush()
        {
            //Nothing to do
        }

        public virtual void Discard()
        {
            //Nothing to do
        }
    }

    /// <summary>
    /// Abstract Base class for immutable quad datasets
    /// </summary>
    public abstract class BaseImmutableQuadDataset
        : BaseQuadDataset
    {
        public sealed override void AddGraph(IGraph g)
        {
            throw new NotSupportedException("This dataset is immutable");
        }

        public sealed override void RemoveGraph(Uri graphUri)
        {
            throw new NotSupportedException("This dataset is immutable");
        }

        public sealed override IGraph GetModifiableGraph(Uri graphUri)
        {
            throw new NotSupportedException("This dataset is immutable");
        }
    }
}
