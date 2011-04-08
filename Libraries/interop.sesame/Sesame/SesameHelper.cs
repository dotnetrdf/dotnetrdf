using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using dotSesame = org.openrdf.model;
using dotSesameFormats = org.openrdf.rio;
using java.io;
using VDS.RDF.Parsing;
using VDS.RDF.Storage.Params;

namespace VDS.RDF.Interop.Sesame
{
    public static class SesameHelper
    {
        public static Object LoadFromFile(java.io.File f, string baseUri, org.openrdf.rio.RDFFormat rdff)
        {
            Object obj;

            if (rdff == dotSesameFormats.RDFFormat.N3)
            {
                obj = new Graph();
                if (baseUri != null) ((IGraph)obj).BaseUri = new Uri(baseUri);
                FileLoader.Load((IGraph)obj, f.getPath(), new Notation3Parser());
            }
            else if (rdff == dotSesameFormats.RDFFormat.NTRIPLES)
            {
                obj = new Graph();
                if (baseUri != null) ((IGraph)obj).BaseUri = new Uri(baseUri);
                FileLoader.Load((IGraph)obj, f.getPath(), new NTriplesParser());
            }
            else if (rdff == dotSesameFormats.RDFFormat.RDFXML)
            {
                obj = new Graph();
                if (baseUri != null) ((IGraph)obj).BaseUri = new Uri(baseUri);
                FileLoader.Load((IGraph)obj, f.getPath(), new RdfXmlParser());
            }
            else if (rdff == dotSesameFormats.RDFFormat.TRIG)
            {
                obj = new TripleStore();
                TriGParser trig = new TriGParser();
                trig.Load((ITripleStore)obj, new StreamParams(f.getPath()));
            }
            else if (rdff == dotSesameFormats.RDFFormat.TRIX)
            {
                obj = new TripleStore();
                TriXParser trix = new TriXParser();
                trix.Load((ITripleStore)obj, new StreamParams(f.getPath()));
            }
            else if (rdff == dotSesameFormats.RDFFormat.TURTLE)
            {
                obj = new Graph();
                if (baseUri != null) ((IGraph)obj).BaseUri = new Uri(baseUri);
                FileLoader.Load((IGraph)obj, f.getPath(), new TurtleParser());
            }
            else
            {
                throw new RdfParserSelectionException("The given Input Format is not supported by dotNetRDF");
            }

            return obj;
        }

        public static Object LoadFromUri(java.net.URL url, string baseUri, org.openrdf.rio.RDFFormat rdff)
        {
            Object obj;
            Uri u = new Uri(url.toString());

            if (rdff == dotSesameFormats.RDFFormat.N3)
            {
                obj = new Graph();
                if (baseUri != null) ((IGraph)obj).BaseUri = new Uri(baseUri);
                UriLoader.Load((IGraph)obj, u, new Notation3Parser());
            }
            else if (rdff == dotSesameFormats.RDFFormat.NTRIPLES)
            {
                obj = new Graph();
                if (baseUri != null) ((IGraph)obj).BaseUri = new Uri(baseUri);
                UriLoader.Load((IGraph)obj, u, new NTriplesParser());
            }
            else if (rdff == dotSesameFormats.RDFFormat.RDFXML)
            {
                obj = new Graph();
                if (baseUri != null) ((IGraph)obj).BaseUri = new Uri(baseUri);
                UriLoader.Load((IGraph)obj, u, new RdfXmlParser());
            }
            else if (rdff == dotSesameFormats.RDFFormat.TRIG || rdff == dotSesameFormats.RDFFormat.TRIX)
            {
                obj = new TripleStore();
                UriLoader.Load((ITripleStore)obj, u);
            }
            else if (rdff == dotSesameFormats.RDFFormat.TURTLE)
            {
                obj = new Graph();
                if (baseUri != null) ((IGraph)obj).BaseUri = new Uri(baseUri);
                UriLoader.Load((IGraph)obj, u, new TurtleParser());
            }
            else
            {
                throw new RdfParserSelectionException("The given Input Format is not supported by dotNetRDF");
            }

            return obj;
        }

        public static Object LoadFromStream(InputStream @is, string baseUri, org.openrdf.rio.RDFFormat rdff)
        {
            Object obj;

            if (rdff == dotSesameFormats.RDFFormat.N3)
            {
                obj = new Graph();
                if (baseUri != null) ((IGraph)obj).BaseUri = new Uri(baseUri);
                Notation3Parser parser = new Notation3Parser();
                parser.Load((IGraph)obj, @is.ToDotNetReadableStream());
            }
            else if (rdff == dotSesameFormats.RDFFormat.NTRIPLES)
            {
                obj = new Graph();
                if (baseUri != null) ((IGraph)obj).BaseUri = new Uri(baseUri);
                NTriplesParser parser = new NTriplesParser();
                parser.Load((IGraph)obj, @is.ToDotNetReadableStream());
            }
            else if (rdff == dotSesameFormats.RDFFormat.RDFXML)
            {
                obj = new Graph();
                if (baseUri != null) ((IGraph)obj).BaseUri = new Uri(baseUri);
                RdfXmlParser parser = new RdfXmlParser();
                parser.Load((IGraph)obj, @is.ToDotNetReadableStream());
            }
            else if (rdff == dotSesameFormats.RDFFormat.TRIG)
            {
                obj = new TripleStore();
                TriGParser trig = new TriGParser();
                trig.Load((ITripleStore)obj, new StreamParams(@is.ToDotNetReadableStream().BaseStream));
            }
            else if (rdff == dotSesameFormats.RDFFormat.TRIX)
            {
                obj = new TripleStore();
                TriXParser trix = new TriXParser();
                trix.Load((ITripleStore)obj, new StreamParams(@is.ToDotNetReadableStream().BaseStream));
            }
            else if (rdff == dotSesameFormats.RDFFormat.TURTLE)
            {
                obj = new Graph();
                if (baseUri != null) ((IGraph)obj).BaseUri = new Uri(baseUri);
                TurtleParser parser = new TurtleParser();
                parser.Load((IGraph)obj, @is.ToDotNetReadableStream());
            }
            else
            {
                throw new RdfParserSelectionException("The given Input Format is not supported by dotNetRDF");
            }

            return obj;
        }

        public static Object LoadFromReader(Reader r, string baseUri, org.openrdf.rio.RDFFormat rdff)
        {
            Object obj;

            if (rdff == dotSesameFormats.RDFFormat.N3)
            {
                obj = new Graph();
                if (baseUri != null) ((IGraph)obj).BaseUri = new Uri(baseUri);
                Notation3Parser parser = new Notation3Parser();
                parser.Load((IGraph)obj, r.ToDotNetReadableStream());
            }
            else if (rdff == dotSesameFormats.RDFFormat.NTRIPLES)
            {
                obj = new Graph();
                if (baseUri != null) ((IGraph)obj).BaseUri = new Uri(baseUri);
                NTriplesParser parser = new NTriplesParser();
                parser.Load((IGraph)obj, r.ToDotNetReadableStream());
            }
            else if (rdff == dotSesameFormats.RDFFormat.RDFXML)
            {
                obj = new Graph();
                if (baseUri != null) ((IGraph)obj).BaseUri = new Uri(baseUri);
                RdfXmlParser parser = new RdfXmlParser();
                parser.Load((IGraph)obj, r.ToDotNetReadableStream());
            }
            else if (rdff == dotSesameFormats.RDFFormat.TRIG)
            {
                obj = new TripleStore();
                TriGParser trig = new TriGParser();
                trig.Load((ITripleStore)obj, new StreamParams(r.ToDotNetReadableStream().BaseStream));
            }
            else if (rdff == dotSesameFormats.RDFFormat.TRIX)
            {
                obj = new TripleStore();
                TriXParser trix = new TriXParser();
                trix.Load((ITripleStore)obj, new StreamParams(r.ToDotNetReadableStream().BaseStream));
            }
            else if (rdff == dotSesameFormats.RDFFormat.TURTLE)
            {
                obj = new Graph();
                if (baseUri != null) ((IGraph)obj).BaseUri = new Uri(baseUri);
                TurtleParser parser = new TurtleParser();
                parser.Load((IGraph)obj, r.ToDotNetReadableStream());
            }
            else
            {
                throw new RdfParserSelectionException("The given Input Format is not supported by dotNetRDF");
            }

            return obj;
        }

        internal static void ModifyStore(Object obj, Func<IGraph,bool> modFunc)
        {
            if (obj is IGraph)
            {
                IGraph g = (IGraph)obj;
                if (!g.IsEmpty)
                {
                    modFunc(g);
                }
            }
            else if (obj is ITripleStore)
            {
                ITripleStore store = (ITripleStore)obj;
                foreach (IGraph g in store.Graphs)
                {
                    if (!g.IsEmpty)
                    {
                        modFunc(g);
                    }
                }
            }
        }

        internal static void ModifyStore(Object obj, Func<Uri, IGraph, bool> modFunc, IEnumerable<Uri> contexts)
        {
            foreach (Uri context in contexts)
            {
                if (obj is IGraph)
                {
                    IGraph g = (IGraph)obj;
                    if (!g.IsEmpty)
                    {
                        modFunc(context, g);
                    }
                }
                else if (obj is ITripleStore)
                {
                    ITripleStore store = (ITripleStore)obj;
                    foreach (IGraph g in store.Graphs)
                    {
                        if (!g.IsEmpty)
                        {
                            modFunc(context, g);
                        }
                    }
                }
            }
        }

        internal static IEnumerable<Uri> ToContexts(this dotSesame.Resource[] contexts, SesameMapping mapping)
        {
            if (contexts == null)
            {
                return Enumerable.Empty<Uri>();
            }
            else if (contexts.Length == 0)
            {
                return Enumerable.Empty<Uri>();
            }
            else
            {
                List<Uri> results = new List<Uri>();
                foreach (dotSesame.Resource r in contexts)
                {
                    if (r != null && r is dotSesame.URI)
                    {
                        results.Add(((IUriNode)SesameConverter.FromSesameResource(r, mapping)).Uri);
                    }
                }
                return results;
            }
        }

        internal static IEnumerable<Triple> ToTriples(this info.aduna.iteration.Iteration iter, SesameMapping mapping)
        {
            while (iter.hasNext())
            {
                yield return SesameConverter.FromSesame((dotSesame.Statement)iter.next(), mapping);
            }
        }

        private static StreamReader ToDotNetReadableStream(this InputStream stream)
        {
            MemoryStream mem = new MemoryStream();
            StreamWriter writer = new StreamWriter(mem);

            try
            {
                InputStreamReader buffer = new InputStreamReader(stream);
                BufferedReader reader = new BufferedReader(buffer);
                while (reader.ready())
                {
                    String line = reader.readLine();
                    if (line != null)
                    {
                        writer.WriteLine(line);
                    }
                    else
                    {
                        break;
                    }
                }
            } 
            catch
            {
                throw new RdfParseException("Failed to convert the Java Input Stream into a .Net Stream successfully");
            }
            finally
            {
                stream.close();
            }
            writer.Flush();
            mem.Seek(0, SeekOrigin.Begin);
            return new StreamReader(mem);
        }

        private static StreamReader ToDotNetReadableStream(this Reader r)
        {
            MemoryStream mem = new MemoryStream();
            StreamWriter writer = new StreamWriter(mem);

            try
            {
                BufferedReader reader = new BufferedReader(r);
                while (reader.ready())
                {
                    String line = reader.readLine();
                    if (line != null)
                    {
                        writer.WriteLine(line);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            catch
            {
                throw new RdfParseException("Failed to convert the Java Reader into a .Net Stream successfully");
            }
            finally
            {
                r.close();
            }
            writer.Flush();
            mem.Seek(0, SeekOrigin.Begin);
            return new StreamReader(mem);
        }   
    }
}
