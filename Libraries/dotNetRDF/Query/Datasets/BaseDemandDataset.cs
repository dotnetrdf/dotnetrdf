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

namespace VDS.RDF.Query.Datasets
{
    /// <summary>
    /// Abstract Dataset wrapper implementation for datasets that can load graphs on demand
    /// </summary>
    public abstract class BaseDemandDataset
        : WrapperDataset
    {
        /// <summary>
        /// Creates a new Demand Dataset
        /// </summary>
        /// <param name="dataset">Underlying Dataset</param>
        public BaseDemandDataset(ISparqlDataset dataset)
            : base(dataset) { }

        /// <summary>
        /// Sees if the underlying dataset has a graph and if not tries to load it on demand
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <returns></returns>
        public override bool HasGraph(Uri graphUri)
        {
            if (!_dataset.HasGraph(graphUri))
            {
                // If the underlying dataset doesn't have the Graph can we load it on demand
                IGraph g;
                if (TryLoadGraph(graphUri, out g))
                {
                    g.BaseUri = graphUri;
                    AddGraph(g);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Method to be implemented by derived classes which implements the loading of graphs on demand
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <param name="g">Graph</param>
        /// <returns></returns>
        protected abstract bool TryLoadGraph(Uri graphUri, out IGraph g);
    }
}
