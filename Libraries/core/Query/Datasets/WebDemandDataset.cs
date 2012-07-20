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

#if !SILVERLIGHT

using System;

namespace VDS.RDF.Query.Datasets
{
    /// <summary>
    /// Implementation of a dataset wrapper which can load additional graphs from the web on demand
    /// </summary>
    public class WebDemandDataset
        : BaseDemandDataset
    {
        /// <summary>
        /// Creates a new Web Demand Dataset
        /// </summary>
        /// <param name="dataset">Underlying Dataset</param>
        public WebDemandDataset(ISparqlDataset dataset)
            : base(dataset) { }

        /// <summary>
        /// Tries to load graphs from the web
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <param name="g">Graph</param>
        /// <returns></returns>
        protected override bool TryLoadGraph(Uri graphUri, out IGraph g)
        {
            try
            {
                g = new Graph();
                g.LoadFromUri(graphUri);
                return true;
            }
            catch
            {
                //Any error means we couldn't load on demand
                g = null;
                return false;
            }
        }
    }
}

#endif