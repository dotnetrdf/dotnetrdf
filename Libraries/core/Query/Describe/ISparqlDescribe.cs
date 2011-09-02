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

namespace VDS.RDF.Query.Describe
{
    /// <summary>
    /// Interface for classes that implement the DESCRIBE functionality of SPARQL
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is designed so that developers can introduce their own DESCRIBE algorithms as required
    /// </para>
    /// </remarks>
    public interface ISparqlDescribe
    {
        /// <summary>
        /// Generates a Graph which is the description of the resources resulting from the Query
        /// </summary>
        /// <param name="context">SPARQL Evaluation Context</param>
        /// <returns></returns>
        IGraph Describe(SparqlEvaluationContext context);

        /// <summary>
        /// Generates the Description Graph based on the Query Results from the given Evaluation Context passing the resulting Triples to the given RDF Handler
        /// </summary>
        /// <param name="handler">RDF Handler</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        void Describe(IRdfHandler handler, SparqlEvaluationContext context);
    }
}
