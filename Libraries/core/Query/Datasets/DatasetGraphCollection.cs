/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Query.Datasets
{
    /// <summary>
    /// A Graph Collection which wraps an <see cref="ISparqlDataset">ISparqlDataset</see> implementation so it can be used as if it was a Graph Collection
    /// </summary>
    public class DatasetGraphCollection
        : BaseGraphCollection
    {
        private ISparqlDataset _dataset;

        /// <summary>
        /// Creates a new Dataset Graph collection
        /// </summary>
        /// <param name="dataset">SPARQL Dataset</param>
        public DatasetGraphCollection(ISparqlDataset dataset)
        {
            this._dataset = dataset;
        }

        /// <summary>
        /// Gets whether the Collection contains a Graph with the given URI
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <returns></returns>
        public override bool Contains(Uri graphUri)
        {
            return this._dataset.HasGraph(graphUri);
        }

        /// <summary>
        /// Adds a Graph to the Collection
        /// </summary>
        /// <param name="g">Graph to add</param>
        /// <param name="mergeIfExists">Whether to merge the given Graph with any existing Graph with the same URI</param>
        /// <exception cref="RdfException">Thrown if a Graph with the given URI already exists and the <paramref name="mergeIfExists">mergeIfExists</paramref> is set to false</exception>
        protected internal override bool Add(IGraph g, bool mergeIfExists)
        {
            if (this.Contains(g.BaseUri))
            {
                if (mergeIfExists)
                {
                    IGraph temp = this._dataset.GetModifiableGraph(g.BaseUri);
                    temp.Merge(g);
                    temp.Dispose();
                    this._dataset.Flush();
                    return true;
                }
                else
                {
                    throw new RdfException("Cannot add this Graph as a Graph with the URI '" + g.BaseUri.ToSafeString() + "' already exists in the Collection and mergeIfExists was set to false");
                }
            }
            else
            {
                //Safe to add a new Graph
                if (this._dataset.AddGraph(g))
                {
                    this._dataset.Flush();
                    this.RaiseGraphAdded(g);
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Removes a Graph from the Collection
        /// </summary>
        /// <param name="graphUri">URI of the Graph to removed</param>
        protected internal override bool Remove(Uri graphUri)
        {
            if (this.Contains(graphUri))
            {
                IGraph temp = this._dataset[graphUri];
                bool removed = this._dataset.RemoveGraph(graphUri);
                this._dataset.Flush();
                this.RaiseGraphRemoved(temp);
                temp.Dispose();
                return removed;
            }
            return false;
        }

        /// <summary>
        /// Gets the number of Graphs in the Collection
        /// </summary>
        public override int Count
        {
            get 
            {
                return this._dataset.GraphUris.Count(); 
            }
        }

        /// <summary>
        /// Gets the URIs of Graphs in the Collection
        /// </summary>
        public override IEnumerable<Uri> GraphUris
        {
            get 
            {
                return this._dataset.GraphUris;
            }
        }

        /// <summary>
        /// Gets the Graph with the given URI
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <returns></returns>
        public override IGraph this[Uri graphUri]
        {
            get 
            {
                if (this._dataset.HasGraph(graphUri))
                {
                    return this._dataset[graphUri];
                }
                else
                {
                    throw new RdfException("The Graph with the given URI does not exist in this Graph Collection");
                }
            }
        }

        /// <summary>
        /// Disposes of the Graph Collection
        /// </summary>
        public override void Dispose()
        {
            this._dataset.Flush();
        }

        /// <summary>
        /// Gets the enumeration of Graphs in this Collection
        /// </summary>
        /// <returns></returns>
        public override IEnumerator<IGraph> GetEnumerator()
        {
            return this._dataset.Graphs.GetEnumerator();
        }
    }
}
