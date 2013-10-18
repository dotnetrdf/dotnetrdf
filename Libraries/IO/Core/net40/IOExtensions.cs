using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VDS.RDF.Graphs;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;

namespace VDS.RDF
{
    public static class IOExtensions
    {
        /// <summary>
        /// Method for Loading a Graph from some Concrete RDF Syntax via some arbitrary Stream
        /// </summary>
        /// <param name="g">Graph to load RDF into</param>
        /// <param name="input">The reader to read input from</param>
        /// <exception cref="RdfException">Thrown if the Parser tries to output something that is invalid RDF</exception>
        /// <exception cref="Parsing.RdfParseException">Thrown if the Parser cannot Parse the Input</exception>
        /// <exception cref="System.IO.IOException">Thrown if the Parser encounters an IO Error while trying to access/parse the Stream</exception>
        public static void Load(this IRdfReader parser, IGraph g, StreamReader input)
        {
            if (ReferenceEquals(parser, null)) throw new ArgumentNullException("parser");
            parser.Load(new GraphHandler(g), input);
        }

        /// <summary>
        /// Method for Loading a Graph from some Concrete RDF Syntax via some arbitrary Input
        /// </summary>
        /// <param name="g">Graph to load RDF into</param>
        /// <param name="input">The reader to read input from</param>
        /// <exception cref="RdfException">Thrown if the Parser tries to output something that is invalid RDF</exception>
        /// <exception cref="Parsing.RdfParseException">Thrown if the Parser cannot Parse the Input</exception>
        /// <exception cref="System.IO.IOException">Thrown if the Parser encounters an IO Error while trying to access/parse the Stream</exception>
        public static void Load(this IRdfReader parser, IGraph g, TextReader input)
        {
            if (ReferenceEquals(parser, null)) throw new ArgumentNullException("parser");
            parser.Load(new GraphHandler(g), input);
        }

#if !NO_FILE
        /// <summary>
        /// Method for Loading a Graph from some Concrete RDF Syntax from a given File
        /// </summary>
        /// <param name="g">Graph to load RDF into</param>
        /// <param name="filename">The Filename of the File to read from</param>
        /// <exception cref="RdfException">Thrown if the Parser tries to output something that is invalid RDF</exception>
        /// <exception cref="Parsing.RdfParseException">Thrown if the Parser cannot Parse the Input</exception>
        /// <exception cref="System.IO.IOException">Thrown if the Parser encounters an IO Error while trying to access/parse the File</exception>
        public static void Load(this IRdfReader parser, IGraph g, String filename)
        {
            if (ReferenceEquals(parser, null)) throw new ArgumentNullException("parser");
            parser.Load(new GraphHandler(g), filename);
        }
#endif


        /// <summary>
        /// Loads RDF data from a file into a Graph
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="file">File to load from</param>
        /// <param name="parser">Parser to use</param>
        /// <remarks>
        /// <para>
        /// This is just a shortcut to using the static <strong>Load()</strong> methods from the <see cref="FileLoader">FileLoader</see> class located in the <see cref="VDS.RDF.Parsing">Parsing</see> namespace
        /// </para>
        /// <para>
        /// <strong>Note:</strong> FileLoader will assign the Graph a file URI as it's Base URI unless the Graph already has a Base URI or is non-empty prior to attempting parsing.  Note that any Base URI specified in the RDF contained in the file will override this initial Base URI.  In some cases this may lead to invalid RDF being accepted and generating strange relative URIs, if you encounter this either set a Base URI prior to calling this method or create an instance of the relevant parser and invoke it directly.
        /// </para>
        /// <para>
        /// If a File URI is assigned it will always be an absolute URI for the file
        /// </para>
        /// </remarks>
        public static void LoadFromFile(this IGraph g, String file, IRdfReader parser)
        {
            FileLoader.Load(g, file, parser);
        }

        /// <summary>
        /// Loads RDF data from a file into a Graph
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="file">File to load from</param>
        /// <remarks>
        /// <para>
        /// This is just a shortcut to using the static <strong>Load()</strong> methods from the <see cref="FileLoader">FileLoader</see> class located in the <see cref="VDS.RDF.Parsing">Parsing</see> namespace
        /// </para>
        /// <para>
        /// <strong>Note:</strong> FileLoader will assign the Graph a file URI as it's Base URI unless the Graph already has a Base URI or is non-empty prior to attempting parsing.  Note that any Base URI specified in the RDF contained in the file will override this initial Base URI.  In some cases this may lead to invalid RDF being accepted and generating strange relative URIs, if you encounter this either set a Base URI prior to calling this method or create an instance of the relevant parser and invoke it directly.
        /// </para>
        /// <para>
        /// If a File URI is assigned it will always be an absolute URI for the file
        /// </para>
        /// </remarks>
        public static void LoadFromFile(this IGraph g, String file)
        {
            FileLoader.Load(g, file);
        }

#if !SILVERLIGHT

        /// <summary>
        /// Loads RDF data from a URI into a Graph
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="u">URI to load from</param>
        /// <param name="parser">Parser to use</param>
        /// <remarks>
        /// <para>
        /// This is just a shortcut to using the static <strong>Load()</strong> methods from the <see cref="UriLoader">UriLoader</see> class located in the <see cref="VDS.RDF.Parsing">Parsing</see> namespace
        /// </para>
        /// <para>
        /// <strong>Note:</strong> UriLoader will assign the Graph the source URI as it's Base URI unless the Graph already has a Base URI or is non-empty prior to attempting parsing.  Note that any Base URI specified in the RDF contained in the file will override this initial Base URI.  In some cases this may lead to invalid RDF being accepted and generating strange relative URIs, if you encounter this either set a Base URI prior to calling this method or create an instance of the relevant parser and invoke it directly.
        /// </para>
        /// </remarks>
        public static void LoadFromUri(this IGraph g, Uri u, IRdfReader parser)
        {
            UriLoader.Load(g, u, parser);
        }

        /// <summary>
        /// Loads RDF data from a URI into a Graph
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="u">URI to load from</param>
        /// <remarks>
        /// <para>
        /// This is just a shortcut to using the static <strong>Load()</strong> methods from the <see cref="UriLoader">UriLoader</see> class located in the <see cref="VDS.RDF.Parsing">Parsing</see> namespace
        /// </para>
        /// <para>
        /// <strong>Note:</strong> UriLoader will assign the Graph the source URI as it's Base URI unless the Graph already has a Base URI or is non-empty prior to attempting parsing.  Note that any Base URI specified in the RDF contained in the file will override this initial Base URI.  In some cases this may lead to invalid RDF being accepted and generating strange relative URIs, if you encounter this either set a Base URI prior to calling this method or create an instance of the relevant parser and invoke it directly.
        /// </para>
        /// </remarks>
        public static void LoadFromUri(this IGraph g, Uri u)
        {
            UriLoader.Load(g, u);
        }

#endif

        // TODO: Add LoadFromUri extensions that do the loading asychronously for use on Silverlight/Windows Phone 7

        /// <summary>
        /// Loads RDF data from a String into a Graph
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="data">Data to load</param>
        /// <param name="parser">Parser to use</param>
        /// <remarks>
        /// This is just a shortcut to using the static <strong>Parse()</strong> methods from the <see cref="StringParser">StringParser</see> class located in the <see cref="VDS.RDF.Parsing">Parsing</see> namespace
        /// </remarks>
        public static void LoadFromString(this IGraph g, String data, IRdfReader parser)
        {
            StringParser.Parse(g, data, parser);
        }

        /// <summary>
        /// Loads RDF data from a String into a Graph
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="data">Data to load</param>
        /// <remarks>
        /// This is just a shortcut to using the static <strong>Parse()</strong> methods from the <see cref="StringParser">StringParser</see> class located in the <see cref="VDS.RDF.Parsing">Parsing</see> namespace
        /// </remarks>
        public static void LoadFromString(this IGraph g, String data)
        {
            StringParser.Parse(g, data);
        }

        /// <summary>
        /// Loads RDF data from an Embedded Resource into a Graph
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="resource">Assembly qualified name of the resource to load from</param>
        /// <remarks>
        /// This is just a shortcut to using the static <strong>Load()</strong> methods from the <see cref="EmbeddedResourceLoader">EmbeddedResourceLoader</see> class located in the <see cref="VDS.RDF.Parsing">Parsing</see> namespace
        /// </remarks>
        public static void LoadFromEmbeddedResource(this IGraph g, String resource)
        {
            EmbeddedResourceLoader.Load(g, resource);
        }

        /// <summary>
        /// Loads RDF data from an Embedded Resource into a Graph
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="resource">Assembly qualified name of the resource to load from</param>
        /// <param name="parser">Parser to use</param>
        /// <remarks>
        /// This is just a shortcut to using the static <strong>Load()</strong> methods from the <see cref="EmbeddedResourceLoader">EmbeddedResourceLoader</see> class located in the <see cref="VDS.RDF.Parsing">Parsing</see> namespace
        /// </remarks>
        public static void LoadFromEmbeddedResource(this IGraph g, String resource, IRdfReader parser)
        {
            EmbeddedResourceLoader.Load(g, resource, parser);
        }

        /// <summary>
        /// Saves a Graph to a File
        /// </summary>
        /// <param name="g">Graph to save</param>
        /// <param name="file">File to save to</param>
        /// <param name="writer">Writer to use</param>
        public static void SaveToFile(this IGraph g, String file, IRdfWriter writer)
        {
            if (writer == null)
            {
                g.SaveToFile(file);
            }
            else
            {
                writer.Save(g, file);
            }
        }

        /// <summary>
        /// Saves a Graph to a File
        /// </summary>
        /// <param name="g">Graph to save</param>
        /// <param name="file">File to save to</param>
        public static void SaveToFile(this IGraph g, String file)
        {
            IRdfWriter writer = IOManager.GetWriterByFileExtension(IOManager.GetTrueFileExtension(file));
            writer.Save(g, file);
        }

        /// <summary>
        /// Loads an RDF dataset from a file into a Triple Store
        /// </summary>
        /// <param name="store">Triple Store to load into</param>
        /// <param name="file">File to load from</param>
        /// <param name="parser">Parser to use</param>
        /// <remarks>
        /// This is just a shortcut to using the static <strong>Load()</strong> methods from the <see cref="FileLoader">FileLoader</see> class located in the <see cref="VDS.RDF.Parsing">Parsing</see> namespace
        /// </remarks>
        public static void LoadFromFile(this IGraphStore store, String file, IRdfReader parser)
        {
            FileLoader.Load(store, file, parser);
        }

        /// <summary>
        /// Loads an RDF dataset from a file into a Triple Store
        /// </summary>
        /// <param name="store">Triple Store to load into</param>
        /// <param name="file">File to load from</param>
        /// <remarks>
        /// This is just a shortcut to using the static <strong>Load()</strong> methods from the <see cref="FileLoader">FileLoader</see> class located in the <see cref="VDS.RDF.Parsing">Parsing</see> namespace
        /// </remarks>
        public static void LoadFromFile(this IGraphStore store, String file)
        {
            FileLoader.Load(store, file);
        }

#if !SILVERLIGHT

        /// <summary>
        /// Loads an RDF dataset from a URI into a Triple Store
        /// </summary>
        /// <param name="store">Triple Store to load into</param>
        /// <param name="u">URI to load from</param>
        /// <param name="parser">Parser to use</param>
        /// <remarks>
        /// This is just a shortcut to using the static <strong>Load()</strong> methods from the <see cref="UriLoader">UriLoader</see> class located in the <see cref="VDS.RDF.Parsing">Parsing</see> namespace
        /// </remarks>
        public static void LoadFromUri(this IGraphStore store, Uri u, IRdfReader parser)
        {
            UriLoader.Load(store, u, parser);
        }

        /// <summary>
        /// Loads an RDF dataset from a URI into a Triple Store
        /// </summary>
        /// <param name="store">Triple Store to load into</param>
        /// <param name="u">URI to load from</param>
        /// <remarks>
        /// This is just a shortcut to using the static <strong>Load()</strong> methods from the <see cref="UriLoader">UriLoader</see> class located in the <see cref="VDS.RDF.Parsing">Parsing</see> namespace
        /// </remarks>
        public static void LoadFromUri(this IGraphStore store, Uri u)
        {
            UriLoader.Load(store, u);
        }

#endif

        /// <summary>
        /// Loads an RDF dataset from a String into a Triple Store
        /// </summary>
        /// <param name="store">Triple Store to load into</param>
        /// <param name="data">Data to load</param>
        /// <param name="parser">Parser to use</param>
        /// <remarks>
        /// This is just a shortcut to using the static <strong>ParseDataset()</strong> methods from the <see cref="StringParser">StringParser</see> class located in the <see cref="VDS.RDF.Parsing">Parsing</see> namespace
        /// </remarks>
        public static void LoadFromString(this IGraphStore store, String data, IRdfReader parser)
        {
            StringParser.ParseDataset(store, data, parser);
        }

        /// <summary>
        /// Loads an RDF dataset from a String into a Triple Store
        /// </summary>
        /// <param name="store">Triple Store to load into</param>
        /// <param name="data">Data to load</param>
        /// <remarks>
        /// This is just a shortcut to using the static <strong>ParseDataset()</strong> methods from the <see cref="StringParser">StringParser</see> class located in the <see cref="VDS.RDF.Parsing">Parsing</see> namespace
        /// </remarks>
        public static void LoadFromString(this IGraphStore store, String data)
        {
            StringParser.ParseDataset(store, data);
        }

        /// <summary>
        /// Loads an RDF dataset from an Embedded Resource into a Triple Store
        /// </summary>
        /// <param name="store">Triple Store to load into</param>
        /// <param name="resource">Assembly Qualified Name of the Embedded Resource to load from</param>
        /// <param name="parser">Parser to use</param>
        /// <remarks>
        /// This is just a shortcut to using the static <strong>Load()</strong> methods from the <see cref="EmbeddedResourceLoader">EmbeddedResourceLoader</see> class located in the <see cref="VDS.RDF.Parsing">Parsing</see> namespace
        /// </remarks>
        public static void LoadFromEmbeddedResource(this IGraphStore store, String resource, IRdfReader parser)
        {
            EmbeddedResourceLoader.Load(store, resource, parser);
        }

        /// <summary>
        /// Loads an RDF dataset from an Embedded Resource into a Triple Store
        /// </summary>
        /// <param name="store">Triple Store to load into</param>
        /// <param name="resource">Assembly Qualified Name of the Embedded Resource to load from</param>
        /// <remarks>
        /// This is just a shortcut to using the static <strong>Load()</strong> methods from the <see cref="EmbeddedResourceLoader">EmbeddedResourceLoader</see> class located in the <see cref="VDS.RDF.Parsing">Parsing</see> namespace
        /// </remarks>
        public static void LoadFromEmbeddedResource(this IGraphStore store, String resource)
        {
            EmbeddedResourceLoader.Load(store, resource);
        }

        /// <summary>
        /// Saves a Triple Store to a file
        /// </summary>
        /// <param name="store">Triple Store to save</param>
        /// <param name="file">File to save to</param>
        /// <param name="writer">Writer to use</param>
        public static void SaveToFile(this IGraphStore store, String file, IRdfWriter writer)
        {
            if (writer == null)
            {
                store.SaveToFile(file);
            }
            else
            {
                writer.Save(store, file);
            }
        }

        /// <summary>
        /// Saves a Triple Store to a file
        /// </summary>
        /// <param name="store">Triple Store to save</param>
        /// <param name="file">File to save to</param>
        public static void SaveToFile(this IGraphStore store, String file)
        {
            IRdfWriter writer = IOManager.GetStoreWriterByFileExtension(IOManager.GetTrueFileExtension(file));
            writer.Save(store, file);
        }
    }
}
