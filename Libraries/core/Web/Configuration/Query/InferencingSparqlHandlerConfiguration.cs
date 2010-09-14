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
using System.Linq;
using System.Text;
using System.Web;
using System.Configuration;
using System.Reflection;
using VDS.RDF.Query;
using VDS.RDF.Query.Inference;

namespace VDS.RDF.Web.Configuration.Query
{
    /// <summary>
    /// Class for storing Sparql Handler configuration where Reasoners are supported
    /// </summary>
    [Obsolete("This class is obseleted and has been superseded by QueryHandlerConfiguration", true)]
    public class InferencingSparqlHandlerConfiguration : ExtensionSupportingSparqlHandlerConfiguration
    {
        /// <summary>
        /// Queue of Reasoners that should be applied to the Store
        /// </summary>
        protected Queue<IInferenceEngine> _reasoners = new Queue<IInferenceEngine>();
        /// <summary>
        /// Queue of URIs for associated Rules Graph used to intialise the Reasoners
        /// </summary>
        /// <remarks>
        /// Nulls are used for Reasoners that have no associated Rules Graph
        /// </remarks>
        protected Queue<Uri> _ruleGraphs = new Queue<Uri>();

        /// <summary>
        /// Creates a new Inferencing Sparql Handler Configuration object which loads all the relevant settings from the AppSettings section of the in-scope config file
        /// </summary>
        /// <param name="context">HTPP Context</param>
        /// <param name="cacheKey">Cache Key for this Handler</param>
        /// <param name="configPrefix">Configuration Variable Name Prefix for this Handler</param>
        public InferencingSparqlHandlerConfiguration(HttpContext context, String cacheKey, String configPrefix)
            : base(context, cacheKey, configPrefix)
        {
            //Are there any Reasoners defined?
            int i = 1;
            while (ConfigurationManager.AppSettings[configPrefix + "Reasoner" + i] != null)
            {
                String reasonerType = ConfigurationManager.AppSettings[configPrefix + "Reasoner" + i];

                //Use Reflection to create the Reasoner
                try
                {
                    Object temp;

                    //Is the Reasoner from an external assembly?
                    if (ConfigurationManager.AppSettings[configPrefix + "ReasonerAssembly" + i] != null)
                    {
                        String assembly = ConfigurationManager.AppSettings[configPrefix + "ReasonerAssembly" + i];
                        temp = Activator.CreateInstance(assembly, reasonerType).Unwrap();
                    }
                    else
                    {
                        temp = Activator.CreateInstance(null, reasonerType).Unwrap();
                    }

                    //Ensure it's a Reasoner and apply initialisation if applicable
                    if (temp is IInferenceEngine)
                    {
                        this._reasoners.Enqueue((IInferenceEngine)temp);

                        //Was an Rules Graph specified for the Reasoner?
                        if (ConfigurationManager.AppSettings[configPrefix + "ReasonerRulesGraph" + i] != null)
                        {
                            try
                            {
                                Uri rulesGraphUri = new Uri(ConfigurationManager.AppSettings[configPrefix + "ReasonerRulesGraph" + i]);
                                this._ruleGraphs.Enqueue(rulesGraphUri);
                            }
                            catch (UriFormatException)
                            {
                                //Malformed Uri so can't use the Rule Graph
                                this._ruleGraphs.Enqueue(null);
                            }
                        }
                        else
                        {
                            //No Rules Graph
                            this._ruleGraphs.Enqueue(null);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new RdfQueryException("SPARQL Handler Reasoner Configuration was invalid", ex);
                }
                i++;
            }
        }

        /// <summary>
        /// Gets whether there are any Reasoners loaded in the Configuration
        /// </summary>
        public bool HasReasoners
        {
            get
            {
                return (this._reasoners.Count > 0);
            }
        }
    }
}

#endif