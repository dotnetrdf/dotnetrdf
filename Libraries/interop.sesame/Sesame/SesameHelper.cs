using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dotSesameFormats = org.openrdf.rio;
using java.io;
using VDS.RDF.Parsing;
using VDS.RDF.Storage.Params;

namespace VDS.RDF.Interop.Sesame
{
    public static class SesameHelper
    {
        public static Object LoadFromFile(File f, string baseUri, org.openrdf.rio.RDFFormat rdff)
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
    }
}
