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

using System;
using VDS.RDF.Query;

namespace VDS.RDF
{
    /// <summary>
    /// Interface for Handlers which handle the SPARQL Results produced by parsers
    /// </summary>
    public interface ISparqlResultsHandler : INodeFactory
    {
        /// <summary>
        /// Starts the Handling of Results
        /// </summary>
        void StartResults();

        /// <summary>
        /// Ends the Handling of Results
        /// </summary>
        /// <param name="ok">Indicates whether parsing completed without error</param>
        void EndResults(bool ok);

        /// <summary>
        /// Handles a Boolean Result
        /// </summary>
        /// <param name="result">Result</param>
        void HandleBooleanResult(bool result);

        /// <summary>
        /// Handles a Variable Declaration
        /// </summary>
        /// <param name="var">Variable Name</param>
        /// <returns></returns>
        bool HandleVariable(String var);

        /// <summary>
        /// Handles a SPARQL Result
        /// </summary>
        /// <param name="result">Result</param>
        /// <returns></returns>
        bool HandleResult(SparqlResult result);
    }
}
