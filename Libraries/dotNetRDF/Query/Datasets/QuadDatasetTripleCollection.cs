/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
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
            _dataset = dataset;
            _graphUri = graphUri;
        }

        protected internal override bool Add(Triple t)
        {
            return _dataset.AddQuad(_graphUri, t);
        }

        public override bool Contains(Triple t)
        {
            return _dataset.ContainsQuad(_graphUri, t);
        }

        public override int Count
        {
            get 
            {
                return _dataset.GetQuads(_graphUri).Count();
            }
        }

        protected internal override bool Delete(Triple t)
        {
            return _dataset.RemoveQuad(_graphUri, t);
        }

        public override Triple this[Triple t]
        {
            get 
            {
                if (_dataset.ContainsQuad(_graphUri, t))
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
                return _dataset.GetQuads(_graphUri).Select(t => t.Object).Distinct();
            }
        }

        public override IEnumerable<INode> PredicateNodes
        {
            get 
            {
                return _dataset.GetQuads(_graphUri).Select(t => t.Predicate).Distinct();
            }
        }

        public override IEnumerable<INode> SubjectNodes
        {
            get 
            {
                return _dataset.GetQuads(_graphUri).Select(t => t.Subject).Distinct(); 
            }
        }

        public override void Dispose()
        {
            // No dispose actions needed
        }

        public override IEnumerator<Triple> GetEnumerator()
        {
            return _dataset.GetQuads(_graphUri).GetEnumerator();
        }

        public override IEnumerable<Triple> WithObject(INode obj)
        {
            return _dataset.GetQuadsWithObject(_graphUri, obj);
        }

        public override IEnumerable<Triple> WithPredicate(INode pred)
        {
            return _dataset.GetQuadsWithPredicate(_graphUri, pred);
        }

        public override IEnumerable<Triple> WithPredicateObject(INode pred, INode obj)
        {
            return _dataset.GetQuadsWithPredicateObject(_graphUri, pred, obj);
        }

        public override IEnumerable<Triple> WithSubject(INode subj)
        {
            return _dataset.GetQuadsWithSubject(_graphUri, subj);
        }

        public override IEnumerable<Triple> WithSubjectObject(INode subj, INode obj)
        {
            return _dataset.GetQuadsWithSubjectObject(_graphUri, subj, obj);
        }

        public override IEnumerable<Triple> WithSubjectPredicate(INode subj, INode pred)
        {
            return _dataset.GetQuadsWithSubjectPredicate(_graphUri, subj, pred);
        }
    }
}
