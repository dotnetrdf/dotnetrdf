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

namespace VDS.RDF.Storage
{

    /// <summary>
    /// Structure for representing Triples that are waiting to be Batch written to the Database
    /// </summary>
    public struct BatchTriple
    {
        private Triple _t;
        private String _graphID;

        /// <summary>
        /// Creates a new Batch Triple
        /// </summary>
        /// <param name="t">Triple</param>
        /// <param name="graphID">Graph ID to store Triple for</param>
        public BatchTriple(Triple t, String graphID)
        {
            this._t = t;
            this._graphID = graphID;
        }

        /// <summary>
        /// Triple
        /// </summary>
        public Triple Triple
        {
            get
            {
                return this._t;
            }
        }

        /// <summary>
        /// Graph ID
        /// </summary>
        public String GraphID
        {
            get
            {
                return this._graphID;
            }
        }

        /// <summary>
        /// Equality for Batch Triples
        /// </summary>
        /// <param name="obj">Object to test</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is BatchTriple)
            {
                BatchTriple other = (BatchTriple)obj;
                return this._graphID == other.GraphID && this._t.Equals(other.Triple);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Hash Code for Batch Triples
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return (this._graphID + this._t.GetHashCode()).GetHashCode();
        }
    }
}