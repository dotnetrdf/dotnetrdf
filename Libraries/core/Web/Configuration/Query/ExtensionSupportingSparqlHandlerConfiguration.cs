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
using System.Collections.Generic;
using System.Configuration;
using System.Web;
using VDS.RDF.Query;
using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Web.Configuration.Query
{
    /// <summary>
    /// Class for storing SPARQL Handler Configuration where custom expression factories are supported
    /// </summary>
    public class ExtensionSupportingSparqlHandlerConfiguration : BaseSparqlHandlerConfiguration
    {
        /// <summary>
        /// List of Custom Expression Factories which have been specified in the Handler Configuration
        /// </summary>
        protected List<ISparqlCustomExpressionFactory> _expressionFactories = new List<ISparqlCustomExpressionFactory>();

        /// <summary>
        /// Creates a new Extension Function Supporting Sparql Handler Configuration object which loads all the relevant settings from the AppSettings section of the in-scope config file
        /// </summary>
        /// <param name="context">HTPP Context</param>
        /// <param name="cacheKey">Cache Key for this Handler</param>
        /// <param name="configPrefix">Configuration Variable Name Prefix for this Handler</param>
        public ExtensionSupportingSparqlHandlerConfiguration(HttpContext context, String cacheKey, String configPrefix)
            : base(context, cacheKey, configPrefix)
        {
            //Are there any Expression Factories defined?
            int i = 1;
            while (ConfigurationManager.AppSettings[configPrefix + "ExpressionFactory" + i] != null)
            {
                String factoryType = ConfigurationManager.AppSettings[configPrefix + "ExpressionFactory" + i];

                //Use Reflection to create the Expression Factory
                try
                {
                    Object temp;

                    //Is the Factory from an external assembly?
                    if (ConfigurationManager.AppSettings[configPrefix + "ExpressionFactoryAssembly" + i] != null)
                    {
                        String assembly = ConfigurationManager.AppSettings[configPrefix + "ExpressionFactoryAssembly" + i];
                        temp = Activator.CreateInstance(assembly, factoryType).Unwrap();
                    }
                    else
                    {
                        if (!factoryType.Contains(".")) factoryType = "VDS.RDF.Query.Expressions." + factoryType;
                        temp = Activator.CreateInstance(null, factoryType).Unwrap();
                    }

                    //Ensure it's an Expression Factory and apply initialisation if applicable
                    if (temp is ISparqlCustomExpressionFactory)
                    {
                        this._expressionFactories.Add((ISparqlCustomExpressionFactory)temp);
                    }
                }
                catch (Exception ex)
                {
                    throw new RdfQueryException("SPARQL Handler Expression Factory Configuration was invalid", ex);
                }
                i++;
            }
        }

        /// <summary>
        /// Gets whether any Custom Expression Factories are registered in the Config for this Handler
        /// </summary>
        public bool HasExpressionFactories
        {
            get
            {
                return (this._expressionFactories.Count > 0);
            }
        }

        /// <summary>
        /// Gets the Custom Expression Factories which are in the Config for this Handler
        /// </summary>
        public IEnumerable<ISparqlCustomExpressionFactory> ExpressionFactories
        {
            get
            {
                return this._expressionFactories;
            }
        }
    }
}

#endif