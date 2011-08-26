/*

Copyright Robert Vesse 2009-11
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

namespace VDS.RDF.Parsing.Handlers
{
    /// <summary>
    /// A RDF Handler which simply counts the Triples
    /// </summary>
    public class CountHandler 
        : BaseRdfHandler
    {
        private int _counter = 0;

        /// <summary>
        /// Creates a Handler which counts Triples
        /// </summary>
        public CountHandler()
            : base(new MockNodeFactory())
        { }

        /// <summary>
        /// Resets the current count to zero
        /// </summary>
        protected override void StartRdfInternal()
        {
            this._counter = 0;
        }

        /// <summary>
        /// Handles the Triple by incrementing the Triple count
        /// </summary>
        /// <param name="t">Triple</param>
        /// <returns></returns>
        protected override bool HandleTripleInternal(Triple t)
        {
            this._counter++;
            return true;
        }

        /// <summary>
        /// Gets the Count of Triples handled in the most recent parsing operation
        /// </summary>
        /// <remarks>
        /// Note that each time you reuse the handler the count is reset to 0
        /// </remarks>
        public int Count
        {
            get
            {
                return this._counter;
            }
        }

        /// <summary>
        /// Gets that the Handler accepts all Triples
        /// </summary>
        public override bool AcceptsAll
        {
            get 
            {
                return true; 
            }
        }
    }
}
