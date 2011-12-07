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
using System;
using System.Collections;
using System.Collections.Generic;

namespace VDS.RDF.Linq
{
    /// <summary>
    /// Implemented by classes which define a query context class.
    /// </summary>
	public interface IRdfContext
    {
		/// <summary>
		/// Maps from the hashcode of ontology query object to the collection of objects (if any) that it retrieved
		/// </summary>
		Dictionary<string, IEnumerable> ResultsCache 
        { 
            get;
            set; 
        }

		/// <summary>
		/// Allows changes to be tracked and written back to the triple store
		/// </summary>
        void SubmitChanges();

		/// <summary>
		/// Class factory method that creates an ontology query object for the type T
		/// </summary>
		/// <typeparam name="T">The type for which we'll be querying.</typeparam>
		/// <returns>An object of type IRdfQuery that will do the querying.</returns>
		/// <remarks>The results when found will be written back to ResultsCache for later re-iteration.</remarks>
    	IRdfQuery<T> ForType<T>();

        /// <summary>
        /// Default graph URI, without angle brackets. The default graph is 
        /// used in the FROM clause of any SPARQL query used to retrieve
        /// entities for this context.
        /// </summary>
        string DefaultGraph 
        { 
            get; 
            set; 
        }
    }
}
