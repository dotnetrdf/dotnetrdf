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
using System.IO;
using System.Linq;
using System.Reflection;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Storage.Params;

namespace VDS.RDF.Parsing
{
    /// <summary>
    /// Static Helper Class for loading Graphs and Triple Stores from Embedded Resources
    /// </summary>
    public static class EmbeddedResourceLoader
    {
        private static String _currAsmName = Assembly.GetExecutingAssembly().GetName().Name;

        /// <summary>
        /// Loads a Graph from an Embedded Resource
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="resource">Assembly Qualified Name of the Resource to load</param>
        /// <param name="parser">Parser to use (leave null for auto-selection)</param>
        public static void Load(IGraph g, String resource, IRdfReader parser)
        {
            if (g == null) throw new RdfParseException("Cannot read RDF into a null Graph");
            EmbeddedResourceLoader.Load(new GraphHandler(g), resource, (IRdfReader)null);
        }

        /// <summary>
        /// Loads a Graph from an Embedded Resource
        /// </summary>
        /// <param name="handler">RDF Handler to use</param>
        /// <param name="resource">Assembly Qualified Name of the Resource to load</param>
        /// <param name="parser">Parser to use (leave null for auto-selection)</param>
        public static void Load(IRdfHandler handler, String resource, IRdfReader parser)
        {
            if (resource == null) throw new RdfParseException("Cannot read RDF from a null Resource");
            if (handler == null) throw new RdfParseException("Cannot read RDF using a null Handler");

            try
            {
                String resourceName = resource;

                if (resource.Contains(','))
                {
                    //Resource is an external assembly
                    String assemblyName = resource.Substring(resource.IndexOf(',') + 1).TrimStart();
                    resourceName = resourceName.Substring(0, resource.IndexOf(',')).TrimEnd();

                    //Try to load this assembly
                    Assembly asm = assemblyName.Equals(_currAsmName) ? Assembly.GetExecutingAssembly() : Assembly.Load(assemblyName);
                    if (asm != null)
                    {
                        //Resource is in the loaded assembly
                        EmbeddedResourceLoader.LoadGraphInternal(handler, asm, resourceName, parser);
                    }
                    else
                    {
                        throw new RdfParseException("The Embedded Resource '" + resourceName + "' cannot be loaded as the required assembly '" + assemblyName + "' could not be loaded");
                    }
                }
                else
                {
                    //Resource is in dotNetRDF
                    EmbeddedResourceLoader.LoadGraphInternal(handler, Assembly.GetExecutingAssembly(), resourceName, parser);
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
        /// Loads a Graph from an Embedded Resource
        /// </summary>
        /// <param name="handler">RDF Handler to use</param>
        /// <param name="resource">Assembly Qualified Name of the Resource to load</param>
        public static void Load(IRdfHandler handler, String resource)
        {
            EmbeddedResourceLoader.Load(handler, resource, (IRdfReader)null);
        }

        /// <summary>
        /// Loads a Graph from an Embedded Resource
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="resource">Assembly Qualified Name of the Resource to load</param>
        /// <remarks>
        /// Parser will be auto-selected
        /// </remarks>
        public static void Load(IGraph g, String resource)
        {
            EmbeddedResourceLoader.Load(g, resource, null);
        }

        /// <summary>
        /// Internal Helper method which does the actual loading of the Graph from the Resource
        /// </summary>
        /// <param name="handler">RDF Handler to use</param>
        /// <param name="asm">Assembly to get the resource stream from</param>
        /// <param name="resource">Full name of the Resource (without the Assembly Name)</param>
        /// <param name="parser">Parser to use (if null then will be auto-selected)</param>
        private static void LoadGraphInternal(IRdfHandler handler, Assembly asm, String resource, IRdfReader parser)
        {
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

                    //Did we get a defined parser to use?
                    if (parser != null)
                    {
                        parser.Load(handler, new StreamReader(s));
                    }
                    else
                    {
                        //Need to select a Parser or use StringParser
                        String ext = resource.Substring(resource.LastIndexOf("."));
                        MimeTypeDefinition def = MimeTypesHelper.GetDefinitions(MimeTypesHelper.GetMimeTypes(ext)).FirstOrDefault(d => d.CanParseRdf);
                        if (def != null)
                        {
                            //Resource has an appropriate file extension and we've found a candidate parser for it
                            parser = def.GetRdfParser();
                            parser.Load(handler, new StreamReader(s));
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
                            parser = StringParser.GetParser(data);
                            parser.Load(handler, new StringReader(data));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Loads a RDF Dataset from an Embedded Resource
        /// </summary>
        /// <param name="store">Store to load into</param>
        /// <param name="resource">Assembly Qualified Name of the Resource to load</param>
        /// <param name="parser">Parser to use (leave null for auto-selection)</param>
        public static void Load(ITripleStore store, String resource, IStoreReader parser)
        {
            if (store == null) throw new RdfParseException("Cannot read RDF Dataset into a null Store");
            EmbeddedResourceLoader.Load(new StoreHandler(store), resource, parser);
        }

        /// <summary>
        /// Loads a RDF Dataset from an Embedded Resource
        /// </summary>
        /// <param name="store">Store to load into</param>
        /// <param name="resource">Assembly Qualified Name of the Resource to load</param>
        /// <remarks>
        /// Parser will be auto-selected
        /// </remarks>
        public static void Load(ITripleStore store, String resource)
        {
            EmbeddedResourceLoader.Load(store, resource, null);
        }

        /// <summary>
        /// Loads a RDF Dataset from an Embedded Resource
        /// </summary>
        /// <param name="handler">RDF Handler to use</param>
        /// <param name="resource">Assembly Qualified Name of the Resource to load</param>
        /// <param name="parser">Parser to use (leave null for auto-selection)</param>
        public static void Load(IRdfHandler handler, String resource, IStoreReader parser)
        {
            if (resource == null) throw new RdfParseException("Cannot read a RDF Dataset from a null Resource");
            if (handler == null) throw new RdfParseException("Cannot read a RDF Dataset using a null Handler");

            try
            {
                String resourceName = resource;

                if (resource.Contains(','))
                {
                    //Resource is an external assembly
                    String assemblyName = resource.Substring(resource.IndexOf(',') + 1).TrimStart();
                    resourceName = resourceName.Substring(0, resource.IndexOf(',')).TrimEnd();

                    //Try to load this assembly
                    Assembly asm = (assemblyName.Equals(_currAsmName) ? Assembly.GetExecutingAssembly() : Assembly.Load(assemblyName)) as Assembly;
                    if (asm != null)
                    {
                        //Resource is in the loaded assembly
                        EmbeddedResourceLoader.LoadDatasetInternal(handler, asm, resourceName, parser);
                    }
                    else
                    {
                        throw new RdfParseException("The Embedded Resource '" + resourceName + "' cannot be loaded as the required assembly '" + assemblyName + "' could not be loaded.  Please ensure that the assembly name is correct and that is is referenced/accessible in your application.");
                    }
                }
                else
                {
                    //Resource is in dotNetRDF
                    EmbeddedResourceLoader.LoadDatasetInternal(handler, Assembly.GetExecutingAssembly(), resourceName, parser);
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
        /// Loads a RDF Dataset from an Embedded Resource
        /// </summary>
        /// <param name="handler">RDF Handler to use</param>
        /// <param name="resource">Assembly Qualified Name of the Resource to load</param>
        public static void LoadDataset(IRdfHandler handler, String resource)
        {
            EmbeddedResourceLoader.Load(handler, resource, (IStoreReader)null);
        }

        /// <summary>
        /// Internal Helper method which does the actual loading of the Triple Store from the Resource
        /// </summary>
        /// <param name="handler">RDF Handler to use</param>
        /// <param name="asm">Assembly to get the resource stream from</param>
        /// <param name="resource">Full name of the Resource (without the Assembly Name)</param>
        /// <param name="parser">Parser to use (if null will be auto-selected)</param>
        private static void LoadDatasetInternal(IRdfHandler handler, Assembly asm, String resource, IStoreReader parser)
        {
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
                    //Do we have a predefined Parser?
                    if (parser != null)
                    {
                        parser.Load(handler, new StreamParams(s));
                    }
                    else
                    {
                        //Need to select a Parser or use StringParser
                        String ext = resource.Substring(resource.LastIndexOf("."));
                        MimeTypeDefinition def = MimeTypesHelper.GetDefinitions(MimeTypesHelper.GetMimeTypes(ext)).FirstOrDefault(d => d.CanParseRdfDatasets);
                        if (def != null)
                        {
                            //Resource has an appropriate file extension and we've found a candidate parser for it
                            parser = def.GetRdfDatasetParser();
                            parser.Load(handler, new StreamParams(s));
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
                            parser = StringParser.GetDatasetParser(data);
                            parser.Load(handler, new TextReaderParams(new StringReader(data)));
                        }
                    }
                }
            }
        }
    }
}
