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
using VDS.RDF.Storage;
using VDS.RDF.Parsing;

namespace VDS.RDF
{
    /// <summary>
    /// Class for representing Triple Stores which are collections of RDF Graphs
    /// </summary>
    /// <remarks>
    /// The 'Disk Demand' Triple Store is a Triple Store which automatically retrieves Graphs from the Disk based on the URIs of Graphs that you ask it for when those URIs are file:/// URIs
    /// </remarks>
    public class DiskDemandTripleStore : TripleStore
    {
        /// <summary>
        /// Creates a new Disk Demand Triple Store
        /// </summary>
        public DiskDemandTripleStore()
            : base(new DiskDemandGraphCollection()) { }
    }

    /// <summary>
    /// A Graph Collection where Graphs can be loaded on-demand from the Files on Disk as needed
    /// </summary>
    public class DiskDemandGraphCollection : GraphCollection, IEnumerable<IGraph>
    {
        /// <summary>
        /// Creates a new Web Demand Graph Collection which loads Graphs from the Web on demand
        /// </summary>
        public DiskDemandGraphCollection() { }

        /// <summary>
        /// Checks whether the Graph with the given Uri exists in this Graph Collection.  If it doesn't but is a file URI and can be retrieved successfully from disk it will be added to the collection
        /// </summary>
        /// <param name="graphUri">Graph Uri to test</param>
        /// <returns></returns>
        public override bool Contains(Uri graphUri)
        {
            if (base.Contains(graphUri))
            {
                return true;
            }
#if SILVERLIGHT
            if (graphUri.IsFile())
#else
            if (graphUri.IsFile)
#endif
            {
                try
                {
                    Graph g = new Graph();
                    FileLoader.Load(g, graphUri.ToString().Substring(8));

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
        /// Disposes of a Disk Demand Graph Collection
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
        }
    }
}
