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

namespace VDS.RDF.Linq.Sparql
{
	public class LinqToSparqlConnection<T> : IRdfConnection<T>
	{
		public LinqToSparqlQuery<T> SparqlQuery
		{
			get { return sparqlQuery; }
		}

		private readonly LinqToSparqlQuery<T> sparqlQuery;

		public LinqTripleStore Store
		{
			get { return store; }
		}

		private readonly LinqTripleStore store;

		public LinqToSparqlConnection(LinqToSparqlQuery<T> sparqlQuery)
		{
			this.sparqlQuery = sparqlQuery;
			store = sparqlQuery.TripleStore;
		}

		public IRdfCommand<T> CreateCommand()
		{
			LinqToSparqlCommand<T> result = new LinqToSparqlCommand<T>();
			result.Connection = this;
			return result;
		}
	}
}