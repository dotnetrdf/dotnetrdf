/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using VDS.RDF.Parsing.Handlers;

namespace VDS.RDF.Parsing
{
    /// <summary>
    /// Static Helper Class for loading Graphs and Triple Stores from Embedded Resources
    /// </summary>
    public static class EmbeddedResourceLoader
    {
#if NETCORE
        private static string _currAsmName = GetAssemblyName(typeof(EmbeddedResourceLoader).GetTypeInfo().Assembly);
#else
        private static String _currAsmName = GetAssemblyName(Assembly.GetExecutingAssembly());
#endif

        /// <summary>
        /// Loads a Graph from an Embedded Resource
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="resource">Assembly Qualified Name of the Resource to load</param>
        /// <param name="parser">Parser to use (leave null for auto-selection)</param>
        public static void Load(IGraph g, String resource, IRdfReader parser)
        {
            if (g == null) throw new RdfParseException("Cannot read RDF into a null Graph");
            Load(new GraphHandler(g), resource, (IRdfReader)null);
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
                    // Resource is an external assembly
                    String assemblyName = resource.Substring(resource.IndexOf(',') + 1).TrimStart();
                    resourceName = resourceName.Substring(0, resource.IndexOf(',')).TrimEnd();

                    // Try to load this assembly
#if NETCORE
                    Assembly asm = assemblyName.Equals(_currAsmName)
                        ? typeof(EmbeddedResourceLoader).GetTypeInfo().Assembly
                        : Assembly.Load(new AssemblyName(assemblyName));
#else
                    Assembly asm = assemblyName.Equals(_currAsmName) ? Assembly.GetExecutingAssembly() : Assembly.Load(assemblyName);
#endif
                    if (asm != null)
                    {
                        // Resource is in the loaded assembly
                        LoadGraphInternal(handler, asm, resourceName, parser);
                    }
                    else
                    {
                        throw new RdfParseException("The Embedded Resource '" + resourceName + "' cannot be loaded as the required assembly '" + assemblyName + "' could not be loaded");
                    }
                }
                else
                {
                    // Resource is in dotNetRDF
#if NETCORE
                    LoadGraphInternal(handler, typeof(EmbeddedResourceLoader).GetTypeInfo().Assembly, resourceName, parser);
#else
                    LoadGraphInternal(handler, Assembly.GetExecutingAssembly(), resourceName, parser);
#endif
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
            Load(handler, resource, (IRdfReader)null);
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
            Load(g, resource, null);
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
            // Resource is in the given assembly
            using (Stream s = asm.GetManifestResourceStream(resource))
            {
                if (s == null)
                {
                    // Resource did not exist in this assembly
                    throw new RdfParseException("The Embedded Resource '" + resource + "' does not exist inside of " + GetAssemblyName(asm));
                }
                else
                {
                    // Resource exists

                    // Did we get a defined parser to use?
                    if (parser != null)
                    {
                        parser.Load(handler, new StreamReader(s));
                    }
                    else
                    {
                        // Need to select a Parser or use StringParser
                        String ext = MimeTypesHelper.GetTrueResourceExtension(resource);
                        MimeTypeDefinition def = MimeTypesHelper.GetDefinitionsByFileExtension(ext).FirstOrDefault(d => d.CanParseRdf);
                        if (def != null)
                        {
                            // Resource has an appropriate file extension and we've found a candidate parser for it
                            parser = def.GetRdfParser();
                            parser.Load(handler, new StreamReader(s));
                        }
                        else
                        {
                            // Resource did not have a file extension or we didn't have a parser associated with the extension
                            // Try using StringParser instead
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
            Load(new StoreHandler(store), resource, parser);
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
            Load(store, resource, null);
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
                    // Resource is an external assembly
                    String assemblyName = resource.Substring(resource.IndexOf(',') + 1).TrimStart();
                    resourceName = resourceName.Substring(0, resource.IndexOf(',')).TrimEnd();

                    // Try to load this assembly
#if NETCORE
                    Assembly asm = assemblyName.Equals(_currAsmName)
                        ? typeof(EmbeddedResourceLoader).GetTypeInfo().Assembly
                        : Assembly.Load(new AssemblyName(assemblyName));
#else
                    Assembly asm = (assemblyName.Equals(_currAsmName) ? Assembly.GetExecutingAssembly() : Assembly.Load(assemblyName)) as Assembly;
#endif
                    if (asm != null)
                    {
                        // Resource is in the loaded assembly
                        LoadDatasetInternal(handler, asm, resourceName, parser);
                    }
                    else
                    {
                        throw new RdfParseException("The Embedded Resource '" + resourceName + "' cannot be loaded as the required assembly '" + assemblyName + "' could not be loaded.  Please ensure that the assembly name is correct and that is is referenced/accessible in your application.");
                    }
                }
                else
                {
                    // Resource is in dotNetRDF
#if NETCORE
                    LoadDatasetInternal(handler,
                        typeof(EmbeddedResourceLoader).GetTypeInfo().Assembly, resourceName, parser);
#else
                    LoadDatasetInternal(handler, Assembly.GetExecutingAssembly(), resourceName, parser);
#endif
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
            Load(handler, resource, (IStoreReader)null);
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
            // Resource is in the given assembly
            using (Stream s = asm.GetManifestResourceStream(resource))
            {
                if (s == null)
                {
                    // Resource did not exist in this assembly
                    throw new RdfParseException("The Embedded Resource '" + resource + "' does not exist inside of " + GetAssemblyName(asm));
                }
                else
                {
                    // Resource exists
                    // Do we have a predefined Parser?
                    if (parser != null)
                    {
                        parser.Load(handler, new StreamReader(s));
                    }
                    else
                    {
                        // Need to select a Parser or use StringParser
                        String ext =  MimeTypesHelper.GetTrueResourceExtension(resource);
                        MimeTypeDefinition def = MimeTypesHelper.GetDefinitionsByFileExtension(ext).FirstOrDefault(d => d.CanParseRdfDatasets);
                        if (def != null)
                        {
                            // Resource has an appropriate file extension and we've found a candidate parser for it
                            parser = def.GetRdfDatasetParser();
                            parser.Load(handler, new StreamReader(s));
                        }
                        else
                        {
                            // See if the format was actually an RDF graph instead
                            def = MimeTypesHelper.GetDefinitionsByFileExtension(ext).FirstOrDefault(d => d.CanParseRdf);
                            if (def != null)
                            {
                                IRdfReader rdfParser = def.GetRdfParser();
                                rdfParser.Load(handler, new StreamReader(s));
                            }
                            else
                            {
                                // Resource did not have a file extension or we didn't have a parser associated with the extension
                                // Try using StringParser instead
                                String data;
                                using (StreamReader reader = new StreamReader(s))
                                {
                                    data = reader.ReadToEnd();
                                    reader.Close();
                                }
                                parser = StringParser.GetDatasetParser(data);
                                parser.Load(handler, new StringReader(data));
                            }
                        }
                    }
                }
            }
        }

        private static string GetAssemblyName(Assembly asm)
        {
            return asm.GetName().Name;
        }


    }
}
