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

#if !NO_WEB && !NO_ASP

using VDS.RDF.Query;
using VDS.RDF.Query.Datasets;

namespace VDS.RDF.Update.Protocol
{
    /// <summary>
    /// A processor for the SPARQL Graph Store HTTP Protocol which operates by using the libraries in-memory Leviathan SPARQL engine and converting protocol actions to SPARQL Query/Update commands as appropriate
    /// </summary>
    public class LeviathanProtocolProcessor : ProtocolToUpdateProcessor
    {
        /// <summary>
        /// Creates a new Leviathan Protocol Processor
        /// </summary>
        /// <param name="store">Triple Store</param>
        public LeviathanProtocolProcessor(IInMemoryQueryableStore store)
            : this(new InMemoryDataset(store)) { }

        /// <summary>
        /// Creates a new Leviathan Protocol Processor
        /// </summary>
        /// <param name="dataset">SPARQL Dataset</param>
        public LeviathanProtocolProcessor(ISparqlDataset dataset)
            : base(new LeviathanQueryProcessor(dataset), new LeviathanUpdateProcessor(dataset)) { }
    }
}

#endif