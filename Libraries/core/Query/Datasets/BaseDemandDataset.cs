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
            if (!this._dataset.HasGraph(graphUri))
            {
                //If the underlying dataset doesn't have the Graph can we load it on demand
                IGraph g;
                if (this.TryLoadGraph(graphUri, out g))
                {
                    g.BaseUri = graphUri;
                    this.AddGraph(g);
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
