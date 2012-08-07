/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

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
