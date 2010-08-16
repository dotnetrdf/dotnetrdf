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
using VDS.RDF.Ontology;

namespace VDS.RDF.Query.Inference
{
    /// <summary>
    /// Proposed interface for OWL Reasoners - currently incomplete
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Important:</strong> This interface is specifically designed so that it does not require the provision of a Graph to methods unless the method does not make sense without taking an <see cref="IGraph">IGraph</see> as a parameter.  This is because we envisage the use of this interface for connecting to reasoners which have their own access to the data over which they are reasoning and do not need it providing explicitly to them.
    /// </para>
    /// <para>
    /// Reasoner implementations may throw <see cref="NotSupportedException">NotSupportedException</see> for operations they don't support and may throw any other appropriate exceptions as appropriate for operations that encounter errors.
    /// </para>
    /// </remarks>
    public interface IOwlReasoner
    {
        /// <summary>
        /// Adds a Graph to the reasoners knowledge base
        /// </summary>
        /// <param name="g">Graph</param>
        /// <remarks>
        /// <para>
        /// A reasoner may choose to do nothing in this method if that reasoner especially if it operates using some pre-defined, remote or otherwise immutable knowledge base.
        /// </para>
        /// </remarks>
        /// <exception cref="NotSupportedException">May be thrown if the Reasoner does not support such an operation</exception>
        void Add(IGraph g);

        /// <summary>
        /// Extract a reasoning enhanced sub-graph from the given Graph rooted at the given Node
        /// </summary>
        /// <param name="g">Graph</param>
        /// <param name="n">Root Node</param>
        /// <returns></returns>
        IGraph Extract(IGraph g, INode n);

        /// <summary>
        /// Extracts all possible triples using the given extraction mode
        /// </summary>
        /// <param name="mode">Extraction Mode</param>
        /// <returns></returns>
        /// <remarks>
        /// The <paramref name="mode">mode</paramref> permits for the specification of an extraction mode for reasoners that can extract specific subsets of reasoning.  Where this is not supported the reasoner should simply extract all triples that can be inferred by reasoning
        /// </remarks>
        /// <exception cref="NotSupportedException">May be thrown if the Reasoner does not support such an operation</exception>
        IEnumerable<Triple> Extract(String mode);

        /// <summary>
        /// Extracts all possible triples using the given extraction modes
        /// </summary>
        /// <param name="modes">Extraction Modes</param>
        /// <returns></returns>
        /// <remarks>
        /// The <paramref name="modes">modes</paramref> permits for the specification of an extraction mode for reasoners that can extract specific subsets of reasoning.  Where this is not supported the reasoner should simply extract all triples that can be inferred by reasoning
        /// </remarks>
        /// <exception cref="NotSupportedException">May be thrown if the Reasoner does not support such an operation</exception>
        IEnumerable<Triple> Extract(IEnumerable<String> modes);

        /// <summary>
        /// Extracts the triples which comprise the class hierarchy
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotSupportedException">May be thrown if the Reasoner does not support such an operation</exception>
        IEnumerable<Triple> Classify();

        /// <summary>
        /// Extracts the triples which comprise the class hierarchy and individuals of those classes
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotSupportedException">May be thrown if the Reasoner does not support such an operation</exception>
        IEnumerable<Triple> Realize();

        /// <summary>
        /// Returns whether the underlying knowledge base is consistent
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotSupportedException">May be thrown if the Reasoner does not support such an operation</exception>
        bool IsConsistent();

        /// <summary>
        /// Returns whether the given Graph is consistent with the underlying knowledge base
        /// </summary>
        /// <param name="g">Graph</param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException">May be thrown if the Reasoner does not support such an operation</exception>
        bool IsConsistent(IGraph g);

        /// <summary>
        /// Returns the enumeration of unsatisfiable classes
        /// </summary>
        /// <exception cref="NotSupportedException">May be thrown if the Reasoner does not support such an operation</exception>
        IEnumerable<OntologyResource> Unsatisfiable
        {
            get;
        }
    }

    /// <summary>
    /// Interface for OWL Reasoners which have access to their own SPARQL implementations
    /// </summary>
    public interface IQueryableOwlReasoner : IOwlReasoner
    {
        /// <summary>
        /// Executes a SPARQL Query using the reasoners SPARQL implementation
        /// </summary>
        /// <param name="sparqlQuery">SPARQL Query</param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException">May be thrown if the Reasoner does not support such an operation</exception>
        Object ExecuteQuery(String sparqlQuery);
    }
}
