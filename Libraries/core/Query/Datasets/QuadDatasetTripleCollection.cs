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

namespace VDS.RDF.Query.Datasets
{
    /// <summary>
    /// A Triple Collection which is a thin wrapper around a <see cref="BaseQuadDataset">BaseQuadDataset</see> to reduce much of the complexity for <see cref="ISparqlDataset">ISparqlDataset</see> implementors around returning of Graphs
    /// </summary>
    class QuadDatasetTripleCollection
        : BaseTripleCollection
    {
        private BaseQuadDataset _dataset;
        private Uri _graphUri;

        public QuadDatasetTripleCollection(BaseQuadDataset dataset, Uri graphUri)
        {
            this._dataset = dataset;
            this._graphUri = graphUri;
        }

        protected internal override void Add(Triple t)
        {
            this._dataset.AddQuad(this._graphUri, t);
        }

        public override bool Contains(Triple t)
        {
            return this._dataset.ContainsQuad(this._graphUri, t);
        }

        public override int Count
        {
            get 
            {
                return this._dataset.GetQuads(this._graphUri).Count();
            }
        }

        protected internal override void Delete(Triple t)
        {
            this._dataset.RemoveQuad(this._graphUri, t);
        }

        public override Triple this[Triple t]
        {
            get 
            {
                if (this._dataset.ContainsQuad(this._graphUri, t))
                {
                    return t;
                }
                else
                {
                    throw new KeyNotFoundException("Given Triple does not exist in the Graph");
                }
            }
        }

        public override IEnumerable<INode> ObjectNodes
        {
            get 
            {
                return this._dataset.GetQuads(this._graphUri).Select(t => t.Object).Distinct();
            }
        }

        public override IEnumerable<INode> PredicateNodes
        {
            get 
            {
                return this._dataset.GetQuads(this._graphUri).Select(t => t.Predicate).Distinct();
            }
        }

        public override IEnumerable<INode> SubjectNodes
        {
            get 
            {
                return this._dataset.GetQuads(this._graphUri).Select(t => t.Subject).Distinct(); 
            }
        }

        public override void Dispose()
        {
            //No dispose actions needed
        }

        public override IEnumerator<Triple> GetEnumerator()
        {
            return this._dataset.GetQuads(this._graphUri).GetEnumerator();
        }

        public override IEnumerable<Triple> WithObject(INode obj)
        {
            return this._dataset.GetQuadsWithObject(this._graphUri, obj);
        }

        public override IEnumerable<Triple> WithPredicate(INode pred)
        {
            return this._dataset.GetQuadsWithPredicate(this._graphUri, pred);
        }

        public override IEnumerable<Triple> WithPredicateObject(INode pred, INode obj)
        {
            return this._dataset.GetQuadsWithPredicateObject(this._graphUri, pred, obj);
        }

        public override IEnumerable<Triple> WithSubject(INode subj)
        {
            return this._dataset.GetQuadsWithSubject(this._graphUri, subj);
        }

        public override IEnumerable<Triple> WithSubjectObject(INode subj, INode obj)
        {
            return this._dataset.GetQuadsWithSubjectObject(this._graphUri, subj, obj);
        }

        public override IEnumerable<Triple> WithSubjectPredicate(INode subj, INode pred)
        {
            return this._dataset.GetQuadsWithSubjectPredicate(this._graphUri, subj, pred);
        }
    }
}
