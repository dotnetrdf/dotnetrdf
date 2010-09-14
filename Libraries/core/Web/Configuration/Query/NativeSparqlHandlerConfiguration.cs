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

#if !NO_WEB && !NO_ASP && !NO_STORAGE

using System;
using System.Configuration;
using System.Web;
using VDS.RDF.Query;
using VDS.RDF.Storage;

namespace VDS.RDF.Web.Configuration.Query
{
    /// <summary>
    /// Class for storing Native Sparql Handler Configuration
    /// </summary>
    [Obsolete("This class is obseleted and has been superseded by QueryHandlerConfiguration", true)]
    public class NativeSparqlHandlerConfiguration : BaseSparqlHandlerConfiguration
    {
        private INativelyQueryableStore _store;
        private HandlerStoreTypes _storetype;

        /// <summary>
        /// Creates a new Native Sparql Handler Configuration object which loads all the relevant settings from the AppSettings section of the in-scope config file
        /// </summary>
        /// <param name="context">HTPP Context</param>
        /// <param name="cacheKey">Cache Key for this Handler</param>
        /// <param name="configPrefix">Configuration Variable Name Prefix for this Handler</param>
        public NativeSparqlHandlerConfiguration(HttpContext context, String cacheKey, String configPrefix)
            : base(context, cacheKey, configPrefix)
        {
            this._supportsTimeout = false;
            this._supportsPartialResults = false;

            try
            {
                //Retrieve Store Configuration
                HandlerStoreTypes storetype;
                if (ConfigurationManager.AppSettings[configPrefix + "StoreType"] != null)
                {
                    storetype = (HandlerStoreTypes)Enum.Parse(typeof(HandlerStoreTypes), ConfigurationManager.AppSettings[configPrefix + "StoreType"]);
                }
                else
                {
                    throw new RdfQueryException("Required Store Type configuration setting for the Native SPARQL Handler was not found");
                }

                this._storetype = storetype;

                //Look for configuration variables appropriate to the Store
                String server, name, user, pwd, catalog;
                int port = 1111;
                switch (storetype)
                {
                    case HandlerStoreTypes.Talis:
                        //Use Talis Platform Store

                        name = ConfigurationManager.AppSettings[configPrefix + "StoreName"];
                        user = ConfigurationManager.AppSettings[configPrefix + "StoreUser"];
                        pwd = ConfigurationManager.AppSettings[configPrefix + "StorePassword"];

                        if (user == null || pwd == null || user.Equals(String.Empty) || pwd.Equals(String.Empty))
                        {
                            //Unauthenticated connection
                            this._store = new TalisTripleStore(name);
                        }
                        else
                        {
                            //Authenticated connection
                            this._store = new TalisTripleStore(name, user, pwd);
                        }
                        break;

                    case HandlerStoreTypes.Virtuoso:
                        //Use Virtuoso Universal Server Native Quad Store

                        if (ConfigurationManager.AppSettings[configPrefix + "StoreServer"] != null)
                        {
                            server = ConfigurationManager.AppSettings[configPrefix + "StoreServer"];
                        }
                        else
                        {
                            server = "localhost";
                        }
                        if (ConfigurationManager.AppSettings[configPrefix + "StoreName"] != null)
                        {
                            name = ConfigurationManager.AppSettings[configPrefix + "StoreName"];
                        }
                        else
                        {
                            //Virtuoso default database for the Native Quad Store
                            name = "DB";
                        }
                        user = ConfigurationManager.AppSettings[configPrefix + "StoreUser"];
                        pwd = ConfigurationManager.AppSettings[configPrefix + "StorePassword"];
                        if (ConfigurationManager.AppSettings[configPrefix + "StorePort"] != null)
                        {
                            Int32.TryParse(ConfigurationManager.AppSettings[configPrefix + "StorePort"], out port);
                        }

                        this._store = new VirtuosoTripleStore(server, port, name, user, pwd);
                        break;

                    case HandlerStoreTypes.FourStore:
                        //Use a 4store Server

                        if (ConfigurationManager.AppSettings[configPrefix + "StoreServer"] != null)
                        {
                            server = ConfigurationManager.AppSettings[configPrefix + "StoreServer"];
                        }
                        else
                        {
                            throw new RdfQueryException("Required Store Server configuration setting for using 4store with the Native SPARQL Handler was not found");
                        }

                        this._store = new NativeTripleStore(new FourStoreConnector(server));
                        break;

                    case HandlerStoreTypes.Sesame2HTTP:
                        //Use a Sesame 2 HTTP protocol supporting server

                        if (ConfigurationManager.AppSettings[configPrefix + "StoreServer"] != null)
                        {
                            server = ConfigurationManager.AppSettings[configPrefix + "StoreServer"];
                        }
                        else
                        {
                            throw new RdfQueryException("Required Store Server configuration setting for using a Sesame 2.0 HTTP Protocol supporting store with the Native SPARQL Handler was not found");
                        }
                        if (ConfigurationManager.AppSettings[configPrefix + "StoreName"] != null)
                        {
                            name = ConfigurationManager.AppSettings[configPrefix + "StoreName"];
                        }
                        else
                        {
                            throw new RdfQueryException("Required Store Name configuration setting for using a Sesame 2.0 HTTP Protocol supporting store with the Native SPARQL Handler was not found");
                        }
                        user = ConfigurationManager.AppSettings[configPrefix + "StoreUser"];
                        pwd = ConfigurationManager.AppSettings[configPrefix + "StorePassword"];

                        if (user != null && pwd != null)
                        {
                            this._store = new NativeTripleStore(new SesameHttpProtocolConnector(server, name, user, pwd));
                        }
                        else
                        {
                            this._store = new NativeTripleStore(new SesameHttpProtocolConnector(server, name));
                        }
                        break;

                    case HandlerStoreTypes.AllegroGraph:
                        //Use an AllegroGraph server

                        if (ConfigurationManager.AppSettings[configPrefix + "StoreServer"] != null)
                        {
                            server = ConfigurationManager.AppSettings[configPrefix + "StoreServer"];
                        }
                        else
                        {
                            throw new RdfQueryException("Required Store Server configuration setting for using an AllegroGraph store with the Native SPARQL Handler was not found");
                        }
                        if (ConfigurationManager.AppSettings[configPrefix + "StoreName"] != null)
                        {
                            name = ConfigurationManager.AppSettings[configPrefix + "StoreName"];
                        }
                        else
                        {
                            throw new RdfQueryException("Required Store Name configuration setting for using an AllegroGraph store with the Native SPARQL Handler was not found");
                        }
                        if (ConfigurationManager.AppSettings[configPrefix + "StoreCatalog"] != null)
                        {
                            catalog = ConfigurationManager.AppSettings[configPrefix + "StoreCatalog"];
                        }
                        else
                        {
                            throw new RdfQueryException("Required Store Catalog configuration setting for using an AllegroGraph store with the Native SPARQL Handler was not found");
                        }
                        user = ConfigurationManager.AppSettings[configPrefix + "StoreUser"];
                        pwd = ConfigurationManager.AppSettings[configPrefix + "StorePassword"];

                        if (user != null && pwd != null)
                        {
                            this._store = new NativeTripleStore(new AllegroGraphConnector(server, catalog, name, user, pwd));
                        }
                        else
                        {
                            this._store = new NativeTripleStore(new AllegroGraphConnector(server, catalog, name));
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                throw new RdfQueryException("Native SPARQL Handler Configuration could not be found/was invalid", ex);
            }

            //Check for null stores
            if (this._store == null)
            {
                throw new RdfQueryException("Failed to load a Store for use with the Native SPARQL Handler");
            }
        }

        /// <summary>
        /// Gets the Natively Queryable Store object used to service requests
        /// </summary>
        public INativelyQueryableStore TripleStore
        {
            get
            {
                return this._store;
            }
        }

        /// <summary>
        /// Gets the type of Triple Store used
        /// </summary>
        public HandlerStoreTypes StoreType
        {
            get
            {
                return this._storetype;
            }
        }
    }
}

#endif