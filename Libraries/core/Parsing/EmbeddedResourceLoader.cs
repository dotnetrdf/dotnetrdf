/*

Copyright Robert Vesse 2009-11
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using VDS.RDF.Storage.Params;

namespace VDS.RDF.Parsing
{
    /// <summary>
    /// Static Helper class for loading Graphs and Triple Stores from Embedded Resources
    /// </summary>
    public static class EmbeddedResourceLoader
    {
        /// <summary>
        /// Loads a Graph from an Embedded Resource
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="resource">Assembly Qualified Name of the Resource to load</param>
        public static void Load(IGraph g, String resource)
        {
            IGraph target;
            if (g.IsEmpty)
            {
                target = g;
            } 
            else 
            {
                target = new Graph();
            }

            try
            {
                String resourceName = resource;

                if (resource.Contains(','))
                {
                    //Resource is an external assembly
                    String assemblyName = resource.Substring(resource.IndexOf(',') + 1).TrimStart();
                    resourceName = resourceName.Substring(0, resource.IndexOf(',')).TrimEnd();

                    //Try to load this assembly
                    Assembly asm = Assembly.Load(assemblyName);
                    if (asm != null)
                    {
                        //Resource is in the loaded assembly
                        EmbeddedResourceLoader.LoadInternal(target, asm, resourceName);
                    }
                    else
                    {
                        throw new RdfParseException("The Embedded Resource '" + resourceName + "' cannot be loaded as the required assembly '" + assemblyName + "' could not be loaded");
                    }
                }
                else
                {
                    //Resource is in dotNetRDF
                    EmbeddedResourceLoader.LoadInternal(target, Assembly.GetExecutingAssembly(), resourceName);
                }
            }
            catch (RdfParseException)
            {
                    throw;
            }
            catch (Exception ex)
            {
                throw new RdfParseException("Unable to load the Embedded Resource '" + resource + "' as an unexpected error occurred", ex);
            }

            if (!ReferenceEquals(g, target))
            {
                g.Merge(target);
            }
        }

        /// <summary>
        /// Internal Helper method which does the actual loading of the Graph from the Resource
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="asm">Assembly to get the resource stream from</param>
        /// <param name="resource">Full name of the Resource (without the Assembly Name)</param>
        private static void LoadInternal(IGraph g, Assembly asm, String resource)
        {
            IRdfReader parser;

            //Resource is in the given assembly
            using (Stream s = asm.GetManifestResourceStream(resource))
            {
                if (s == null)
                {
                    //Resource did not exist in this assembly
                    throw new RdfParseException("The Embedded Resource '" + resource + "' does not exist inside of " + asm.GetName().Name);
                }
                else
                {
                    //Resource exists
                    String ext = resource.Substring(resource.LastIndexOf("."));
                    MimeTypeDefinition def = MimeTypesHelper.GetDefinitions(MimeTypesHelper.GetMimeType(ext)).FirstOrDefault(d => d.CanParseRdf);
                    if (def != null)
                    {
                        //Resource has an appropriate file extension and we've found a candidate parser for it
                        parser = def.GetRdfParser();
                        parser.Load(g, new StreamReader(s));
                    }
                    else
                    {
                        //Resource did not have a file extension or we didn't have a parser associated with the extension
                        //Try using StringParser instead
                        String data;
                        using (StreamReader reader = new StreamReader(s))
                        {
                            data = reader.ReadToEnd();
                            reader.Close();
                        }

                        StringParser.Parse(g, data);
                    }
                }
            }
        }

        /// <summary>
        /// Loads a Triple Store from an Embedded Resource
        /// </summary>
        /// <param name="store">Store to load into</param>
        /// <param name="resource">Assembly Qualified Name of the Resource to load</param>
        public static void Load(ITripleStore store, String resource)
        {
            try
            {
                String resourceName = resource;

                if (resource.Contains(','))
                {
                    //Resource is an external assembly
                    String assemblyName = resource.Substring(resource.IndexOf(',') + 1).TrimStart();
                    resourceName = resourceName.Substring(0, resource.IndexOf(',')).TrimEnd();

                    //Try to load this assembly
                    Assembly asm = Assembly.Load(assemblyName);
                    if (asm != null)
                    {
                        //Resource is in the loaded assembly
                        EmbeddedResourceLoader.LoadInternal(store, asm, resourceName);
                    }
                    else
                    {
                        throw new RdfParseException("The Embedded Resource '" + resourceName + "' cannot be loaded as the required assembly '" + assemblyName + "' could not be loaded");
                    }
                }
                else
                {
                    //Resource is in dotNetRDF
                    EmbeddedResourceLoader.LoadInternal(store, Assembly.GetExecutingAssembly(), resourceName);
                }
            }
            catch (RdfParseException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new RdfParseException("Unable to load the Embedded Resource '" + resource + "' as an unexpected error occurred", ex);
            }
        }

        /// <summary>
        /// Internal Helper method which does the actual loading of the Triple Store from the Resource
        /// </summary>
        /// <param name="store">Store to load into</param>
        /// <param name="asm">Assembly to get the resource stream from</param>
        /// <param name="resource">Full name of the Resource (without the Assembly Name)</param>
        private static void LoadInternal(ITripleStore store, Assembly asm, String resource)
        {
            IStoreReader parser;

            //Resource is in the given assembly
            using (Stream s = asm.GetManifestResourceStream(resource))
            {
                if (s == null)
                {
                    //Resource did not exist in this assembly
                    throw new RdfParseException("The Embedded Resource '" + resource + "' does not exist inside of " + asm.GetName().Name);
                }
                else
                {
                    //Resource exists
                    String ext = resource.Substring(resource.LastIndexOf("."));
                    MimeTypeDefinition def = MimeTypesHelper.GetDefinitions(MimeTypesHelper.GetMimeType(ext)).FirstOrDefault(d => d.CanParseRdfDatasets);
                    if (def != null)
                    {
                        //Resource has an appropriate file extension and we've found a candidate parser for it
                        parser = def.GetRdfDatasetParser();
                        StreamParams parameters = new StreamParams(s);
                        parser.Load(store, parameters);
                    }
                    else
                    {
                        //Resource did not have a file extension or we didn't have a parser associated with the extension
                        //Try using StringParser instead
                        String data;
                        using (StreamReader reader = new StreamReader(s))
                        {
                            data = reader.ReadToEnd();
                            reader.Close();
                        }

                        StringParser.ParseDataset(store, data);
                    }
                }
            }
        }
    }
}
