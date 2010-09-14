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
using System.IO;
using System.Web;
using VDS.RDF.Query;
using VDS.RDF.Query.Inference;

namespace VDS.RDF.Web.Configuration.Query
{
    /// <summary>
    /// Class for storing File Sparql Handler Configuration
    /// </summary>
    [Obsolete("This class is obseleted and has been superseded by QueryHandlerConfiguration", true)]
    public class FileSparqlHandlerConfiguration : InferencingSparqlHandlerConfiguration
    {
        private IInMemoryQueryableStore _store = null;
        private String _cacheKey;

        /// <summary>
        /// Creates a new File Sparql Handler Configuration object which loads all the relevant settings from the AppSettings section of the in-scope config file
        /// </summary>
        /// <param name="context">HTPP Context</param>
        /// <param name="cacheKey">Cache Key for this Handler</param>
        /// <param name="configPrefix">Configuration Variable Name Prefix for this Handler</param>
        public FileSparqlHandlerConfiguration(HttpContext context, String cacheKey, String configPrefix)
            : base(context, cacheKey, configPrefix)
        {
            this._cacheKey = cacheKey;

            //Timeout and Partial Results are supported
            this._supportsTimeout = true;
            this._supportsPartialResults = true;

            try
            {
                IInMemoryQueryableStore store;
                bool fixedDataset;
                if (ConfigurationManager.AppSettings[configPrefix + "FixedDataset"] != null)
                {
                    fixedDataset = Boolean.Parse(ConfigurationManager.AppSettings[configPrefix + "FixedDataset"]);
                    if (fixedDataset)
                    {
                        store = new TripleStore();
                    }
                    else
                    {
                        store = new WebDemandTripleStore();
                    }
                }
                else
                {
                    fixedDataset = true;
                    store = new TripleStore();
                }

                //Retrieve Store Configuration
                if (ConfigurationManager.AppSettings[configPrefix + "DataFolder"] != null)
                {
                    //Get the Folder
                    String folder = context.Server.MapPath(ConfigurationManager.AppSettings[configPrefix + "DataFolder"]);
                    if (Directory.Exists(folder))
                    {
                        foreach (String file in Directory.GetFiles(folder))
                        {
                            try
                            {
                                String ext = Path.GetExtension(file);
                                IRdfReader parser = MimeTypesHelper.GetParser(MimeTypesHelper.GetMimeType(ext));
                                Graph g = new Graph();
                                parser.Load(g, file);

                                if (g.BaseUri == null)
                                {
                                    g.BaseUri = new Uri("file:///" + file);
                                }

                                store.Add(g);
                            }
                            catch (RdfException)
                            {
                                //Ignore errors that occur with the RDF
                            }
                        }
                    }

                } 
                else if (ConfigurationManager.AppSettings[configPrefix + "DataFile"] != null)
                {
                    //Get the File and pick a Parser based on the File Extension
                    String file = context.Server.MapPath(ConfigurationManager.AppSettings[configPrefix + "DataFile"]);
                    String ext = Path.GetExtension(file);
                    IRdfReader parser = MimeTypesHelper.GetParser(MimeTypesHelper.GetMimeType(ext));
                    Graph g = new Graph();

                    try
                    {
                        parser.Load(g, file);

                        if (g.BaseUri == null)
                        {
                            g.BaseUri = new Uri("file:///" + file);
                        }

                        store.Add(g);
                    }
                    catch (RdfException)
                    {
                        //Ignore errors that occur with the RDF
                    }
                }
                else if (!fixedDataset)
                {
                    //Don't need to specify initial data if not using a Fixed Dataset
                }
                else
                {
                    throw new RdfQueryException("Required Data Folder/File configuration setting for the File SPARQL Handler was not found");
                }

                //Apply any reasoners
                while (this.HasReasoners)
                {
                    IInferenceEngine reasoner = this._reasoners.Dequeue();
                    Uri rulesGraphUri = this._ruleGraphs.Dequeue();

                    //Initialise the Reasoner with a Rules Graph if configured
                    if (rulesGraphUri != null)
                    {
                        if (store.Graphs.Contains(rulesGraphUri))
                        {
                            //Use the Graph that's already in the Store
                            reasoner.Initialise(store.Graphs[rulesGraphUri]);
                        }
                        //If we don't have this Graph in the Store we can't use it
                    }

                    //Add the Reasoner
                    ((IInferencingTripleStore)store).AddInferenceEngine(reasoner);
                }

                this._store = store;

            }
            catch (Exception ex)
            {
                throw new RdfQueryException("File SPARQL Handler Configuration could not be found/was invalid", ex);
            }

            //Check for null stores
            if (this._store == null)
            {
                throw new RdfQueryException("Failed to load a Store for use with the File SPARQL Handler");
            }
        }

        /// <summary>
        /// Gets the Triple Store that queries are executed against
        /// </summary>
        public IInMemoryQueryableStore TripleStore
        {
            get
            {
                return this._store;
            }
        }
    }
}

#endif