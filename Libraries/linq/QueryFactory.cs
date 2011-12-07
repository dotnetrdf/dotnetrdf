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
using System.Text;
using VDS.RDF.Linq.Sparql;

namespace VDS.RDF.Linq
{
    /// <summary>
    /// A class factory that create queries of type <see cref="IRdfQuery{T}"/>
    /// </summary>
    /// <typeparam name="T">The type of object being queried for</typeparam>
    public class QueryFactory<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryFactory{T}"/> class.
        /// </summary>
        /// <param name="queryType">the <see cref="LinqToRdf.QueryType"/> of the query.</param>
        /// <param name="context">The <see cref="IRdfContext"/> data context that will track results, connections and instances.</param>
        public QueryFactory(LinqQueryMethod queryType, IRdfContext context)
        {
            QueryType = queryType;
            DataContext = context;
            TypeTranslator = new XsdtTypeConverter();
        }

        /// <summary>
        /// The <see cref="IRdfContext"/> data context that will track results, connections and instances.
        /// </summary>
        private IRdfContext DataContext { get; set; }

        /// <summary>
        /// indicates what type of query this is for.
        /// </summary>
        /// <value>The type of the query.</value>
        public LinqQueryMethod QueryType { get; set; }

        /// <summary>
        /// the <see cref="ITypeTranslator"/> that will be used to convert results prior to insertion into result objects.
        /// </summary>
        /// <value>The type translator.</value>
        public ITypeTranslator TypeTranslator { get; private set; }

        /// <summary>
        /// Creates an expression translator that will be able to decode the results 
        /// returned from the triple store selected for the query.
        /// </summary>
        /// <returns>An <see cref="IQueryFormatTranslator"/> that will be able to decode 
        /// the results from the chosen triple store</returns>
        public IQueryFormatTranslator CreateExpressionTranslator()
        {
            switch (QueryType)
            {
                default:
                    var translator = new LinqToSparqlExpTranslator<T>(new StringBuilder());
                    translator.TypeTranslator = TypeTranslator;
                    return translator;
            }
        }

        /// <summary>
        /// Creates the query using <see cref="S"/> as the base type of the query.
        /// </summary>
        /// <typeparam name="S">the base type with the metadata needed to make the query</typeparam>
        /// <returns>an <see cref="IRdfQuery{T}"/> to help generate the query</returns>
        public IRdfQuery<S> CreateQuery<S>()
        {
            switch (QueryType)
            {
                default:
                    return new LinqToSparqlQuery<S>(DataContext);
            }
        }

        /// <summary>
        /// Creates the <see cref="IRdfConnection{T}"/> through which the communication with the triple store is performed.
        /// </summary>
        /// <param name="qry">The <see cref="IRdfQuery{T}"/> that contains details of the type of triple store to target.</param>
        /// <returns>An <see cref="IRdfConnection{T}"/> that can communicate with the triple store identified in the query</returns>
        /// <remarks>
        /// At present this is confined to Local and remote SPARQL stores. Local in-memory stores created by SemWeb do not get a connection (IIRC).
        /// </remarks>
        public IRdfConnection<T> CreateConnection(IRdfQuery<T> qry)
        {
            switch (QueryType)
            {
                default:
                    var sparqlConnection = new LinqToSparqlConnection<T>((LinqToSparqlQuery<T>) qry);
                    return sparqlConnection;
            }
        }
    }
}