/*

Copyright Robert Vesse 2009-10
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

namespace VDS.RDF.Query.Datasets
{
    public class DatasetGraphCollection : BaseGraphCollection
    {
        private ISparqlDataset _dataset;

        public DatasetGraphCollection(ISparqlDataset dataset)
        {
            this._dataset = dataset;
        }

        public override bool Contains(Uri graphUri)
        {
            return this._dataset.HasGraph(graphUri);
        }

        protected internal override void Add(IGraph g, bool mergeIfExists)
        {
            if (this.Contains(g.BaseUri))
            {
                if (mergeIfExists)
                {
                    IGraph temp = this._dataset.GetModifiableGraph(g.BaseUri);
                    temp.Merge(g);
                    temp.Dispose();
                    this._dataset.Flush();
                }
                else
                {
                    throw new RdfException("Cannot add this Graph as a Graph with the URI '" + g.BaseUri.ToSafeString() + "' already exists in the Collection and mergeIfExists was set to false");
                }
            }
            else
            {
                //Safe to add a new Graph
                this._dataset.AddGraph(g);
                this._dataset.Flush();
                this.RaiseGraphAdded(g);
            }
        }

        protected internal override void Remove(Uri graphUri)
        {
            if (this.Contains(graphUri))
            {
                IGraph temp = this._dataset[graphUri];
                this._dataset.RemoveGraph(graphUri);
                this._dataset.Flush();
                this.RaiseGraphRemoved(temp);
                temp.Dispose();
            }
        }

        public override int Count
        {
            get 
            {
                return this._dataset.GraphUris.Count(); 
            }
        }

        public override IEnumerable<Uri> GraphUris
        {
            get 
            {
                return this._dataset.GraphUris;
            }
        }

        public override IGraph this[Uri graphUri]
        {
            get 
            {
                return this._dataset[graphUri]; 
            }
        }

        public override void Dispose()
        {
            this._dataset.Flush();
        }

        public override IEnumerator<IGraph> GetEnumerator()
        {
            return this._dataset.Graphs.GetEnumerator();
        }
    }
}
