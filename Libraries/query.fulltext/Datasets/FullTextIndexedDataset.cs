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
using VDS.RDF.Query.FullText.Indexing;

namespace VDS.RDF.Query.Datasets
{
    /// <summary>
    /// A Full Text Indexed Dataset is a wrapper around another dataset and provides automatic full text indexing of data that is added and removed
    /// </summary>
    public class FullTextIndexedDataset
        : WrapperDataset
    {
        private IFullTextIndexer _indexer;

        /// <summary>
        /// Creates a new Full Text Indexed Dataset
        /// </summary>
        /// <param name="dataset">Dataset to wrap</param>
        /// <param name="indexer">Indexer to use</param>
        /// <param name="indexNow">Whether the dataset provided should be indexed now, set to false if indexer is linked to an existing index for this data</param>
        public FullTextIndexedDataset(ISparqlDataset dataset, IFullTextIndexer indexer, bool indexNow)
            : base(dataset)
        {
            this._indexer = indexer;

            //Index Now if requested
            if (indexNow)
            {
                foreach (IGraph g in this.Graphs)
                {
                    this._indexer.Index(g);
                }
            }
        }

        public override void AddGraph(IGraph g)
        {
            this._indexer.Index(g);
            base.AddGraph(g);
        }

        public override void RemoveGraph(Uri graphUri)
        {
            if (this.HasGraph(graphUri))
            {
                this._indexer.Unindex(this[graphUri]);
            }
            base.RemoveGraph(graphUri);
        }

        public override IGraph GetModifiableGraph(Uri graphUri)
        {
            //Use Events to pick up Triple Level changes in the Modifiable Graph
            //because writing a Graph wrapper for this seems like overkill when
            //there are just events we can hook into
            IGraph g = base.GetModifiableGraph(graphUri);
            g.TripleAsserted += new TripleEventHandler(this.HandleTripleAdded);
            g.TripleRetracted += new TripleEventHandler(this.HandleTripleRemoved);
            return g;
        }

        private void HandleTripleAdded(Object sender, TripleEventArgs args)
        {
            this._indexer.Index(args.Triple);
        }

        private void HandleTripleRemoved(Object sender, TripleEventArgs args)
        {
            this._indexer.Unindex(args.Triple);
        }

        public override void Flush()
        {
            //Always flush the index in Flush because triple level index changes don't cause an automatic Flush()
            this._indexer.Flush();
            base.Flush();
        }

        public override void Discard()
        {
            //Always flush the index in Discard because triple level index changes don't cause an automatic Flush()
            this._indexer.Flush();
            base.Discard();
        }
    }
}
