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
using System.Collections.Generic;
using System.Linq.Expressions;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Algebra;

namespace VDS.RDF.Linq.Sparql
{
	public class LinqToSparqlCommand<T> : IRdfCommand<T>
	{
        private SparqlQueryParser _parser = new SparqlQueryParser(SparqlQuerySyntax.Sparql_1_1);

		protected Logger Logger = new Logger(typeof(LinqToSparqlCommand<T>));
		public IRdfConnection<T> Connection { get; set; }

		#region IRdfCommand<T> Members

		public string CommandText { get; set; }
		public string InstanceName { get; set; }
		public IEnumerator<T> ExecuteQuery()
		{
			using (var loggingScope = new LoggingScope("SparqlCommand<T>.ExecuteQuery()"))
			{
				#region Tracing
#line hidden
				if (Logger.IsDebugEnabled)
				{
					Logger.Debug("{0}.", CommandText);
				}
#line default
				#endregion
				MethodCallExpression e = null;
				IList<T> results = new List<T>();
				switch (Connection.Store.QueryMethod)
				{
                    case LinqQueryMethod.CustomSparql:
                    case LinqQueryMethod.GenericSparql:
                    case LinqQueryMethod.InMemorySparql:
                    case LinqQueryMethod.NativeSparql:
                    case LinqQueryMethod.RemoteSparql:
                        //Set up the Connection and the Results Sink
                        LinqToSparqlConnection<T> connection = (LinqToSparqlConnection<T>)Connection;
                        if (connection.SparqlQuery.Expressions.ContainsKey("Select"))
                        {
                            e = connection.SparqlQuery.Expressions["Select"];
                        }
                        ObjectDeserialiserQuerySink sink = new ObjectDeserialiserQuerySink(connection.SparqlQuery.OriginalType, typeof(T), InstanceName, ElideDuplicates, e, (RdfDataContext)connection.SparqlQuery.DataContext);

                        //Compile and Parse the Query
                        DateTime beforeQueryCompilation = DateTime.Now;
                        SparqlQuery query = this._parser.ParseFromString(CommandText);
                        DateTime afterQueryCompilation = DateTime.Now;
                        DateTime beforeQueryRun = DateTime.Now;

                        //Run the Query
                        connection.Store.QueryProcessor.Run(query, sink);

                        //Extract and Register Results
                        DateTime afterQueryRun = DateTime.Now;
                        ExtractResultsIntoList(results, sink);
                        RegisterResults(connection.SparqlQuery, results);

                        break;
					default:
						break;
				}
				return results.GetEnumerator();
			}
		}

		public bool ElideDuplicates
		{
			get;
			set;
		}

		#endregion

		private void RegisterResults(LinqToSparqlQuery<T> query, IEnumerable<T> results)
		{
			string queryHashCode = query.GetHashCode().ToString();
			foreach (T t in results)
			{
				var i = t as OwlInstanceSupertype;
				if (i != null)
				{
					i.DataContext = query.DataContext;
				}
			}

			//discard any old results (not sure whether this will ever get invoked)
			if (query.CachedResults != null)
			{
				query.DataContext.ResultsCache.Remove(queryHashCode);
			}
			query.CachedResults = results;
		}

		private void ExtractResultsIntoList(IList<T> list, ObjectDeserialiserQuerySink querySink)
		{
			if (querySink.IncomingResults != null)
			{
				foreach (T t in querySink.IncomingResults)
				{
					list.Add(t);
				}
			}
		}
	}
}