using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;

namespace rdfMetal
{
    public class MetadataSource
    {
        private ISparqlQueryProcessor _processor;
        private SparqlQueryParser _parser = new SparqlQueryParser(SparqlQuerySyntax.Sparql_1_1);

        public MetadataSource(Options opts)
        {
            if (!String.IsNullOrEmpty(opts.EndpointUri))
            {
                if (String.IsNullOrEmpty(opts.DefaultGraphUri))
                {
                    this._processor = new RemoteQueryProcessor(new SparqlRemoteEndpoint(new Uri(opts.EndpointUri)));
                }
                else
                {
                    this._processor = new RemoteQueryProcessor(new SparqlRemoteEndpoint(new Uri(opts.EndpointUri), opts.DefaultGraphUri));
                }
            }
            else if (!String.IsNullOrEmpty(opts.SourceFile))
            {
                TripleStore store = new TripleStore();
                Graph g = new Graph();
                FileLoader.Load(g, opts.SourceFile);
                store.Add(g);
                this._processor = new LeviathanQueryProcessor(store);
            }
            else
            {
                throw new Exception("Must specify an endpoint or a file to query");
            }
        }

        public SparqlResultSet QueryWithResultSet(String sparqlQuery)
        {
            SparqlQuery q = this._parser.ParseFromString(sparqlQuery);
            Object temp = this._processor.ProcessQuery(q);
            if (temp is SparqlResultSet)
            {
                return (SparqlResultSet)temp;
            }
            else
            {
                throw new RdfQueryException("Did not retrieved a SPARQL Result Set as expected");
            }
        }

    }
}
