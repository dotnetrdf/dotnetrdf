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

If this license is not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

#if !NO_WEB && !NO_ASP && !NO_DATA && !NO_STORAGE

using System;
using System.Configuration;
using System.Web;
using VDS.RDF.Storage;

namespace VDS.RDF.Web.Configuration.Resource
{
    class SqlResourceHandlerConfiguration : BaseResourceHandlerConfiguration
    {
        private String _cacheKey = String.Empty;
        private ISqlIOManager _manager = null;
        private SQLResourceLookupMode _lookupMode = SQLResourceLookupMode.Graph;

        public SqlResourceHandlerConfiguration(HttpContext context, String cacheKey, String configPrefix)
        {
            this._cacheKey = cacheKey;

            String dbserver, dbname, dbuser, dbpassword;
            HandlerDBTypes dbtype = HandlerDBTypes.MSSQL;
            SQLResourceLookupMode mode = SQLResourceLookupMode.Graph;
            int dbport = 1111;

            try
            {
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

                //Handler Config
                if (ConfigurationManager.AppSettings[configPrefix + "LookupMode"] != null)
                {
                    mode = (SQLResourceLookupMode)Enum.Parse(typeof(SQLResourceLookupMode), ConfigurationManager.AppSettings[configPrefix + "LookupMode"]);
                    this._lookupMode = mode;
                }
            }
            catch
            {
                throw new RdfException("SQL Resource Handler Configuration could not be found/was invalid");
            }

            //Create the SQL Reader
            ISqlIOManager manager;
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
            manager.PreserveState = true;
            this._manager = manager;
        }

        public String CacheKey
        {
            get
            {
                return this._cacheKey;
            }
        }

        public ISqlIOManager Manager
        {
            get
            {
                return this._manager;
            }
        }

        public SQLResourceLookupMode LookupMode
        {
            get
            {
                return this._lookupMode;
            }
        }
    }
}

#endif