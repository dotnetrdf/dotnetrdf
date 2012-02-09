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

namespace VDS.RDF.Query.Datasets
{
    /// <summary>
    /// An abstract dataset wrapper that can be used to wrap another dataset and just modify some functionality i.e. provides a decorator over an existing dataset
    /// </summary>
    public abstract class WrapperDataset
        : ISparqlDataset
    {
        /// <summary>
        /// Underlying Dataset
        /// </summary>
        protected ISparqlDataset _dataset;

        /// <summary>
        /// Creates a new wrapped dataset
        /// </summary>
        /// <param name="dataset">Dataset</param>
        public WrapperDataset(ISparqlDataset dataset)
        {
            if (dataset == null) throw new ArgumentNullException("dataset");
            this._dataset = dataset;
        }

        #region ISparqlDataset Members

        public virtual void SetActiveGraph(IEnumerable<Uri> graphUris)
        {
            this._dataset.SetActiveGraph(graphUris);
        }

        public virtual void SetActiveGraph(Uri graphUri)
        {
            this._dataset.SetActiveGraph(graphUri);
        }

        public virtual void SetDefaultGraph(Uri graphUri)
        {
            this._dataset.SetDefaultGraph(graphUri);
        }

        public virtual void SetDefaultGraph(IEnumerable<Uri> graphUris)
        {
            this._dataset.SetDefaultGraph(graphUris);
        }

        public virtual void ResetActiveGraph()
        {
            this._dataset.ResetActiveGraph();
        }

        public virtual void ResetDefaultGraph()
        {
            this._dataset.ResetDefaultGraph();
        }

        public virtual IEnumerable<Uri> DefaultGraphUris
        {
            get
            {
                return this._dataset.DefaultGraphUris;
            }
        }

        public virtual IEnumerable<Uri> ActiveGraphUris
        {
            get
            {
                return this._dataset.ActiveGraphUris;
            }
        }

        public virtual bool UsesUnionDefaultGraph
        {
            get
            {
                return this._dataset.UsesUnionDefaultGraph;
            }
        }

        public virtual void AddGraph(IGraph g)
        {
            this._dataset.AddGraph(g);
        }

        public virtual void RemoveGraph(Uri graphUri)
        {
            this._dataset.RemoveGraph(graphUri);
        }

        public virtual bool HasGraph(Uri graphUri)
        {
            return this._dataset.HasGraph(graphUri);
        }

        public virtual IEnumerable<IGraph> Graphs
        {
            get 
            {
                return this._dataset.Graphs;
            }
        }

        public virtual IEnumerable<Uri> GraphUris
        {
            get 
            {
                return this._dataset.GraphUris;
            }
        }

        public virtual IGraph this[Uri graphUri]
        {
            get
            {
                return this._dataset[graphUri];
            }
        }

        public virtual IGraph GetModifiableGraph(Uri graphUri)
        {
            return this._dataset.GetModifiableGraph(graphUri);
        }

        public virtual bool HasTriples
        {
            get 
            {
                return this._dataset.HasTriples; 
            }
        }

        public virtual bool ContainsTriple(Triple t)
        {
            return this._dataset.ContainsTriple(t);
        }

        public virtual IEnumerable<Triple> Triples
        {
            get
            {
                return this._dataset.Triples;
            }
        }

        public virtual IEnumerable<Triple> GetTriplesWithSubject(INode subj)
        {
            return this._dataset.GetTriplesWithSubject(subj);
        }

        public virtual IEnumerable<Triple> GetTriplesWithPredicate(INode pred)
        {
            return this._dataset.GetTriplesWithPredicate(pred);
        }

        public virtual IEnumerable<Triple> GetTriplesWithObject(INode obj)
        {
            return this._dataset.GetTriplesWithObject(obj);
        }

        public virtual IEnumerable<Triple> GetTriplesWithSubjectPredicate(INode subj, INode pred)
        {
            return this._dataset.GetTriplesWithSubjectPredicate(subj, pred);
        }

        public virtual IEnumerable<Triple> GetTriplesWithSubjectObject(INode subj, INode obj)
        {
            return this._dataset.GetTriplesWithSubjectObject(subj, obj);
        }

        public virtual IEnumerable<Triple> GetTriplesWithPredicateObject(INode pred, INode obj)
        {
            return this._dataset.GetTriplesWithPredicateObject(pred, obj);
        }

        public virtual void Flush()
        {
            this._dataset.Flush();
        }

        public virtual void Discard()
        {
            this._dataset.Discard();
        }

        #endregion
    }
}
