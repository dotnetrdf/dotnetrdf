/* 
 * Copyright (C) 2007, Andrew Matthews http://aabs.wordpress.com/
 *
 * This file is Free Software and part of LinqToRdf http://code.google.com/fromName/linqtordf/
 *
 * It is licensed under the following license:
 *   - Berkeley License, V2.0 or any newer version
 *
 * You may not use this file except in compliance with the above license.
 *
 * See http://code.google.com/fromName/linqtordf/ for the complete text of the license agreement.
 *
 */

using VDS.RDF.Storage;

namespace VDS.RDF.Linq
{
	/// <summary>
	/// Indicates the method of Querying which is in-use
	/// </summary>
	public enum LinqQueryMethod
	{
        /// <summary>
        /// Uses dotNetRDF's in-memory SPARQL engine
        /// </summary>
		InMemorySparql,	
        /// <summary>
        /// Uses the SPARQL implementation of an <see cref="IQueryableGenericIOManager">IQueryableGenericIOManager</see> implementation
        /// </summary>
	    GenericSparql,
        /// <summary>
        /// Uses the SPARQL implementation of an <see cref="INativelyQueryableStore">INativelyQueryableStore</see> implementation
        /// </summary>
        NativeSparql,
        /// <summary>
        /// Uses a <see cref="SparqlRemoteEndpoint">SparqlRemoteEndpoint</see>
        /// </summary>
		RemoteSparql,
        /// <summary>
        /// Uses a custom <see cref="ISparqlQueryProcessor">ISparqlQueryProcessor</see> implementation
        /// </summary>
        CustomSparql
	}
}
