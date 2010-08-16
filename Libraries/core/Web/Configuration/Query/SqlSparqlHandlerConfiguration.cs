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

#if !NO_WEB && !NO_ASP && !NO_DATA && !NO_STORAGE

using System;
using System.Configuration;
using System.Web;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Inference;
using VDS.RDF.Storage;
using VDS.RDF.Web.Configuration.Query;

namespace VDS.RDF.Web
{
    /// <summary>
    /// Class for Storing Sparql Handler Configuration
    /// </summary>
    class SqlSparqlHandlerConfiguration : InferencingSparqlHandlerConfiguration
    {
        private SparqlLoadMode _loadMode = SparqlLoadMode.OnDemand;
        private ISqlIOManager _manager = null;
        private IInMemoryQueryableStore _uncachedStore = null;

        /// <summary>
        /// Creates a new Sparql Handler Configuration object which loads all the relevant settings from the AppSettings section of the in-scope config file
        /// </summary>
        /// <param name="context">HTPP Context</param>
        /// <param name="cacheKey">Cache Key for this Handler</param>
        /// <param name="configPrefix">Configuration Variable Name Prefix for this Handler</param>
        public SqlSparqlHandlerConfiguration(HttpContext context, String cacheKey, String configPrefix)
            : base(context, cacheKey, configPrefix)
        {
            //Try to get Configuration
            String dbserver, dbname, dbuser, dbpassword;
            int dbport = 1111;
            HandlerDBTypes dbtype = HandlerDBTypes.MSSQL;
            SparqlLoadMode mode = SparqlLoadMode.OnDemand;

            //Set that Timeout and Partial Results are supported
            this._supportsPartialResults = true;
            this._supportsTimeout = true;

            //Set Full Triple Indexing
            Options.FullTripleIndexing = this.FullTripleIndexing;

            try
            {
                if (ConfigurationManager.AppSettings[configPrefix + "LoadMode"] != null)
                {
                    mode = (SparqlLoadMode)Enum.Parse(typeof(SparqlLoadMode), ConfigurationManager.AppSettings[configPrefix + "LoadMode"]);
                    this._loadMode = mode;
                }

                //SQL Backed Store Config
                dbserver = ConfigurationManager.AppSettings[configPrefix + "DBServer"];
                dbname = ConfigurationManager.AppSettings[configPrefix + "DBName"];
                dbuser = ConfigurationManager.AppSettings[configPrefix + "DBUser"];
                dbpassword = ConfigurationManager.AppSettings[configPrefix + "DBPassword"];
                Int32.TryParse(ConfigurationManager.AppSettings[configPrefix + "DBPort"], out dbport);
                if (ConfigurationManager.AppSettings[configPrefix + "DBType"] != null)
                {
                    dbtype = (HandlerDBTypes)Enum.Parse(typeof(HandlerDBTypes), ConfigurationManager.AppSettings[configPrefix + "DBType"]);
                }
            }
            catch (Exception ex)
            {
                throw new RdfQueryException("SPARQL Handler Configuration could not be found/was invalid", ex);
            }

            //Select the Store Manager and load the Store
            if (context.Cache[cacheKey] == null)
            {
                IInMemoryQueryableStore store;
                IThreadedSqlIOManager manager = null;
                switch (dbtype)
                {
                    case HandlerDBTypes.MySQL:
                        manager = new MySqlStoreManager(dbserver, dbname, dbuser, dbpassword);
                        break;
                    case HandlerDBTypes.Virtuoso:
                        manager = new NonNativeVirtuosoManager(dbserver, dbport, dbname, dbuser, dbpassword);
                        break;
                    case HandlerDBTypes.MSSQL:
                    default:
                        manager = new MicrosoftSqlStoreManager(dbserver, dbname, dbuser, dbpassword);
                        break;
                }
                switch (mode) 
                {
                    case SparqlLoadMode.PreloadAll:
                        store = new ThreadedSqlTripleStore(manager);
                        break;

                    case SparqlLoadMode.PreloadAllAsync:
                        store = new ThreadedSqlTripleStore(manager, true);
                        break;

                    case SparqlLoadMode.OnDemand:
                    case SparqlLoadMode.OnDemandAggressive:
                    case SparqlLoadMode.OnDemandEnhanced:
                    default:
                        //Use an On Demand Triple Store
                        if (!this._defaultGraph.Equals(String.Empty))
                        {
                            store = new OnDemandTripleStore(manager, new Uri(this._defaultGraph));
                        }
                        else
                        {
                            store = new OnDemandTripleStore(manager);
                        }
                        this._manager = manager;
                        break;
                }

                //Load Reasoners for the Store if the Store can support them
                if (store is IInferencingTripleStore)
                {
                    try
                    {
                        while (this.HasReasoners)
                        {
                            IInferenceEngine reasoner = this._reasoners.Dequeue();
                            Uri rulesGraphUri = this._ruleGraphs.Dequeue();

                            //Initialise the Reasoner with a Rules Graph if configured
                            if (rulesGraphUri != null)
                            {
                                if (store.Graphs.Contains(rulesGraphUri))
                                {
                                    //Just use the Graph that's already in the Store
                                    reasoner.Initialise(store.Graphs[rulesGraphUri]);
                                }
                                else
                                {
                                    //Need to load this Graph from the Store before we can use it to initialise the Reasoner
                                    SqlReader sqlreader = new SqlReader(this._manager);
                                    Graph rulesGraph = sqlreader.Load(rulesGraphUri);
                                    reasoner.Initialise(rulesGraph);
                                }
                            }

                            //Add the Reasoner
                            ((IInferencingTripleStore)store).AddInferenceEngine(reasoner);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new RdfQueryException("SPARQL Handler Configuration could not be found/was invalid", ex);
                    }
                }

                //Add to Cache
                context.Cache.Add(cacheKey, store, null, System.Web.Caching.Cache.NoAbsoluteExpiration, new TimeSpan(0, this._cacheDuration, 0), System.Web.Caching.CacheItemPriority.AboveNormal, null);
            }
        }

        /// <summary>
        /// Gets the Load Mode that controls how the Handler loads the Store into memory
        /// </summary>
        public SparqlLoadMode LoadMode
        {
            get
            {
                return this._loadMode;
            }
        }

        /// <summary>
        /// Gets the Manager used for the Database
        /// </summary>
        public ISqlIOManager Manager
        {
            get
            {
                return this._manager;
            }
        }
    }
}

#endif