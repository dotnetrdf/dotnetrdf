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
using VDS.RDF.Linq.Sparql;
using VDS.RDF.Query;
using VDS.RDF.Storage;


namespace VDS.RDF.Linq
{
    /// <summary>
    /// RdfDataContext is the source of all entities from a triple store. It includes a query results cache 
    /// and when used with a remote triple store, wraps a triple store connection. RdfDataContext also 
    /// includes a class factory method IRdfQuery<T> ForType<T>( ) that creates ontology query objects for 
    /// the type T.
    /// </summary>
    public class RdfDataContext : IRdfContext
    {
        private Dictionary<string, IEnumerable> resultsCache = new Dictionary<string, IEnumerable>();
        protected LinqTripleStore store;
        protected string defaultGraph;
        private Queue<IPendingLinqAction> pendingQueue = new Queue<IPendingLinqAction>();

        /// <summary>
        /// Creates a new Data Context that uses the given LINQ Triple Store
        /// </summary>
        /// <param name="store">LINQ Triple Store</param>
        public RdfDataContext(LinqTripleStore store)
        {
            this.store = store;
        }

        /// <summary>
        /// Gets/Sets the Results Cache
        /// </summary>
        public Dictionary<string, IEnumerable> ResultsCache
        {
            get 
            { 
                return resultsCache; 
            }
            set 
            { 
                resultsCache = value; 
            }
        }

        /// <summary>
        /// Gets/Sets the LINQ Triple Store used for queries and updates
        /// </summary>
        public LinqTripleStore Store
        {
            get 
            { 
                return store; 
            }
            set 
            { 
                store = value; 
            }
        }

        /// <summary>
        /// Submits all pending changes to the underlying Store
        /// </summary>
        public void SubmitChanges()
        {
            if (pendingQueue.Count == 0) return;
            if (!store.SupportsPersistence) throw new LinqToRdfException("The LinqTripleStore in use does not support Persistence");
            if (store.UpdateProcessor == null) throw new LinqToRdfException("The LinqTripleStore in use does not support Persistence");

            while (pendingQueue.Count > 0)
            {
                IPendingLinqAction action = pendingQueue.Dequeue();
                action.ProcessAction(store.UpdateProcessor);
            }
        }

        /// <summary>
        /// Gets/Sets the Default Graph used for queries and updates
        /// </summary>
        public string DefaultGraph 
        {
            get { return defaultGraph; }
            set { defaultGraph = value; } 
        }

        /// <summary>
        /// Discards all pending changes
        /// </summary>
        public void DiscardChanges()
        {
            // cut loose the old queue (which should then be GCd)
            pendingQueue = new Queue<IPendingLinqAction>();
        }

        /// <summary>
        /// Adds an Object to be added to the list of pending changes
        /// </summary>
        /// <typeparam name="T">Type of the Object</typeparam>
        /// <param name="entity">Object</param>
        public void Add<T>(T entity) where T : OwlInstanceSupertype
        {
            if (entity == null) throw new ArgumentNullException("entity", "Cannot persist a null Object");

            pendingQueue.Enqueue(new AdditionAction(entity, this.DefaultGraph));
        }

        /// <summary>
        /// Adds an Object to be deleted to the list of pending changes
        /// </summary>
        /// <typeparam name="T">Type of the Object</typeparam>
        /// <param name="entity">Object</param>
        public void Delete<T>(T entity) where T : OwlInstanceSupertype
        {
            this.Delete<T>(entity, LinqDeleteMode.DeleteValues);
        }

        /// <summary>
        /// Adds an Object to be deleted to the list of pending changes
        /// </summary>
        /// <typeparam name="T">Type of the Object</typeparam>
        /// <param name="entity">Object</param>
        /// <param name="mode">Deletion Mode</param>
        public void Delete<T>(T entity, LinqDeleteMode mode) where T : OwlInstanceSupertype
        {
            if (entity == null) throw new ArgumentNullException("entity", "Cannot delete a null Object");

            pendingQueue.Enqueue(new DeletionAction(entity, this.DefaultGraph, mode));
        }

        /// <summary>
        /// Creates a LINQ Query which returns all Objects with the given Type from the underlying Store
        /// </summary>
        /// <typeparam name="T">Desired Type</typeparam>
        /// <returns></returns>
        public IRdfQuery<T> ForType<T>()
        {
            QueryFactory<T> qf = new QueryFactory<T>(Store.QueryMethod, this);
            switch (Store.QueryMethod)
            {
                case LinqQueryMethod.InMemorySparql:
                case LinqQueryMethod.RemoteSparql:
                case LinqQueryMethod.CustomSparql:
                case LinqQueryMethod.NativeSparql:
                case LinqQueryMethod.GenericSparql:
                    LinqToSparqlQuery<T> tmp = (LinqToSparqlQuery<T>)qf.CreateQuery<T>();
                    tmp.TripleStore = this.Store;
                    tmp.QueryFactory = qf;
                    return tmp;
                default:
                    throw new LinqToRdfException("Unrecognised query method requested");
            }
        }
    }
}
