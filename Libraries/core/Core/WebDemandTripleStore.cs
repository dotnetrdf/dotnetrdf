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

#if !SILVERLIGHT

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Storage;
using VDS.RDF.Parsing;

namespace VDS.RDF
{
    /// <summary>
    /// Class for representing Triple Stores which are collections of RDF Graphs
    /// </summary>
    /// <remarks>
    /// The 'Web Demand' Triple Store is a Triple Store which automatically retrieves Graphs from the Web based on the URIs of Graphs that you ask it for
    /// </remarks>
    public class WebDemandTripleStore : TripleStore
    {
        /// <summary>
        /// Creates an Web Demand Triple Store
        /// </summary>
        /// <param name="defaultGraphUri">A Uri for the Default Graph which should be loaded from the Web as the initial Graph</param>
        public WebDemandTripleStore(Uri defaultGraphUri)
            : this()
        {
            //Call Contains() which will try to load the Graph if it exists in the Store
            if (!this._graphs.Contains(defaultGraphUri))
            {
                throw new RdfException("Cannot load the requested Default Graph since a valid Graph with that URI could not be retrieved from the Web");
            }
        }

        /// <summary>
        /// Creates an Web Demand Triple Store
        /// </summary>
        /// <param name="defaultGraphFile">A Filename for the Default Graph which should be loaded from a local File as the initial Graph</param>
        public WebDemandTripleStore(String defaultGraphFile)
            : this()
        {
            try
            {
                Graph g = new Graph();
                FileLoader.Load(g, defaultGraphFile);
                this._graphs.Add(g, false);
            }
            catch (Exception)
            {
                throw new RdfException("Cannot load the requested Default Graph since a valid Graph could not be retrieved from the given File");
            }
        }

        /// <summary>
        /// Creates a new Web Demand Triple Store
        /// </summary>
        public WebDemandTripleStore()
            : base(new WebDemandGraphCollection()) { }
    }

    /// <summary>
    /// A Graph Collection where Graphs can be loaded on-demand from the Web as needed
    /// </summary>
    public class WebDemandGraphCollection : GraphCollection, IEnumerable<IGraph>
    {
        /// <summary>
        /// Creates a new Web Demand Graph Collection which loads Graphs from the Web on demand
        /// </summary>
        public WebDemandGraphCollection() { }

        /// <summary>
        /// Checks whether the Graph with the given Uri exists in this Graph Collection.  If it doesn't but can be successfully loaded from the Web it will be loaded into the Graph Collection
        /// </summary>
        /// <param name="graphUri">Graph Uri to test</param>
        /// <returns></returns>
        public override bool Contains(Uri graphUri)
        {
            if (base.Contains(graphUri))
            {
                return true;
            }
            else if (graphUri != null)
            {
                try
                {
                    Graph g = new Graph();
                    UriLoader.Load(g, graphUri);

                    this.Add(g, false);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Disposes of a Web Demand Graph Collection
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
        }
    }
}

#endif
