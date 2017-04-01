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

#if UNFINISHED

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Storage;

namespace VDS.RDF.Query.Datasets
{
    /// <summary>
    /// A Dataset implementation where we are wrapping some external <see cref="IQueryableStorage"/>'s methods
    /// </summary>
    public class ExternalDataset
        : BaseQuadDataset
    {
        protected IQueryableStorage _storage;

        public ExternalDataset(IQueryableStorage storage)
        {
            if (storage == null) throw new ArgumentNullException("storage");
            this._storage = storage;
        }

        protected internal override bool AddQuad(Uri graphUri, Triple t)
        {
            this._storage.UpdateGraph(graphUri, t.AsEnumerable(), null);
            return true;
        }

        public override bool RemoveGraph(Uri graphUri)
        {
            this._storage.DeleteGraph(graphUri);
            return true;
        }

        protected internal override bool RemoveQuad(Uri graphUri, Triple t)
        {
            this._storage.UpdateGraph(graphUri, null, t.AsEnumerable());
            return true;
        }

        protected override bool HasGraphInternal(Uri graphUri)
        {
            AnyHandler any = new AnyHandler();
            this._storage.LoadGraph(any, graphUri);
            return any.Any;
        }

        public override IEnumerable<Uri> GraphUris
        {
            get 
            {
                return this._storage.ListGraphs();
            }
        }

        protected override IGraph GetGraphInternal(Uri graphUri)
        {
            throw new NotImplementedException();
        }

        protected internal override bool ContainsQuad(Uri graphUri, Triple t)
        {
            throw new NotImplementedException();
        }

        protected internal override IEnumerable<Triple> GetQuads(Uri graphUri)
        {
            throw new NotImplementedException();
        }

        protected internal override IEnumerable<Triple> GetQuadsWithSubject(Uri graphUri, INode subj)
        {
            throw new NotImplementedException();
        }

        protected internal override IEnumerable<Triple> GetQuadsWithPredicate(Uri graphUri, INode pred)
        {
            throw new NotImplementedException();
        }

        protected internal override IEnumerable<Triple> GetQuadsWithObject(Uri graphUri, INode obj)
        {
            throw new NotImplementedException();
        }

        protected internal override IEnumerable<Triple> GetQuadsWithSubjectPredicate(Uri graphUri, INode subj, INode pred)
        {
            throw new NotImplementedException();
        }

        protected internal override IEnumerable<Triple> GetQuadsWithSubjectObject(Uri graphUri, INode subj, INode obj)
        {
            throw new NotImplementedException();
        }

        protected internal override IEnumerable<Triple> GetQuadsWithPredicateObject(Uri graphUri, INode pred, INode obj)
        {
            throw new NotImplementedException();
        }
    }
}

#endif