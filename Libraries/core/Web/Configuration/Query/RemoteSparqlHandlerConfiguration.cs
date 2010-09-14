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

#if !NO_WEB && !NO_ASP

using System;
using System.Configuration;
using System.Web;
using VDS.RDF.Query;

namespace VDS.RDF.Web.Configuration.Query
{
    /// <summary>
    /// Class for storing Remote Sparql Handler Configuration
    /// </summary>
    [Obsolete("This class is obseleted and has been superseded by QueryHandlerConfiguration", true)]
    class RemoteSparqlHandlerConfiguration : BaseSparqlHandlerConfiguration
    {
        private Uri _endpointURI = null;

        /// <summary>
        /// Creates a new Remote Sparql Handler Configuration object which loads all the relevant settings from the AppSettings section of the in-scope config file
        /// </summary>
        /// <param name="context">HTPP Context</param>
        /// <param name="cacheKey">Cache Key for this Handler</param>
        /// <param name="configPrefix">Configuration Variable Name Prefix for this Handler</param>
        public RemoteSparqlHandlerConfiguration(HttpContext context, String cacheKey, String configPrefix)
            : base(context, cacheKey, configPrefix)
        {
            //Try to get Configuration
            Uri endpointURI;
            bool supportsTimeout, supportsPartialResults;
            
            try
            {
                //Sparql Query Default Config
                if (Boolean.TryParse(ConfigurationManager.AppSettings[configPrefix + "SupportsTimeout"], out supportsTimeout))
                {
                    this._supportsTimeout = supportsTimeout;
                }
                if (ConfigurationManager.AppSettings[configPrefix + "TimeoutField"] != null)
                {
                    this._timeoutField = ConfigurationManager.AppSettings[configPrefix + "TimeoutField"];
                }
                if (Boolean.TryParse(ConfigurationManager.AppSettings[configPrefix + "SupportsPartialResults"], out supportsPartialResults))
                {
                    this._supportsPartialResults = supportsPartialResults;
                }
                if (ConfigurationManager.AppSettings[configPrefix + "PartialResultsField"] != null)
                {
                    this._partialResultsField = ConfigurationManager.AppSettings[configPrefix + "PartialResultsField"];
                }

                //Handler Config
                if (ConfigurationManager.AppSettings[configPrefix + "EndpointURI"] != null)
                {
                    endpointURI = new Uri(ConfigurationManager.AppSettings[configPrefix + "EndpointURI"]);
                    this._endpointURI = endpointURI;
                }
                else
                {
                    throw new RdfException("Required Endpoint URI configuration variable for the Remote SPARQL Handler was not found");
                }
            }
            catch
            {
                throw new RdfQueryException("SPARQL Handler Configuration could not be found/was invalid");
            }
        }

        /// <summary>
        /// Uri of the Remote Endpoint
        /// </summary>
        public Uri EndpointURI
        {
            get
            {
                return this._endpointURI;
            }
        }
    }
}

#endif