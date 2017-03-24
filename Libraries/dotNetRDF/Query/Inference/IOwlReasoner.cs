/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/

using System;
using System.Collections.Generic;
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
