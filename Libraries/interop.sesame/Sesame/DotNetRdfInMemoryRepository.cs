using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dotSesame = org.openrdf.model;
using dotSesameRepo = org.openrdf.repository;
using dotSesameFormats = org.openrdf.rio;
using dotSesameQuery = org.openrdf.query;
using java.io;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Storage;
using VDS.RDF.Storage.Params;
using VDS.RDF.Writing;

namespace VDS.RDF.Interop.Sesame
{
    public class DotNetRdfInMemoryRepository : dotSesameRepo.Repository
    {
        private IInMemoryQueryableStore _store;
        private DotNetRdfInMemoryRepositoryConnection _connection;
        private DotNetRdfValueFactory _factory = new DotNetRdfValueFactory(new Graph());

        public DotNetRdfInMemoryRepository()
            : this(new TripleStore()) { }

        public DotNetRdfInMemoryRepository(IInMemoryQueryableStore store)
        {
            this._store = store;
        }

        public org.openrdf.repository.RepositoryConnection getConnection()
        {
            if (this._connection == null) this._connection = new DotNetRdfInMemoryRepositoryConnection(this, this._store, this._factory);
            return this._connection;
        }

        public File getDataDir()
        {
            return null;
        }

        public org.openrdf.model.ValueFactory getValueFactory()
        {
            return this._factory;
        }

        public void initialize()
        {
            //Nothing to do
        }

        public bool isWritable()
        {
            return true;
        }

        public void setDataDir(File f)
        {
            throw new NotSupportedException("dotNetRDF In-Memory Repositories do not support settings the data directory");
        }

        public void shutDown()
        {
            if (this._store != null)
            {
                if (this._store is IFlushableStore)
                {
                    ((IFlushableStore)this._store).Flush();
                }
                this._store.Dispose();
            }
            this._store = null;
        }
    }

    public class DotNetRdfInMemoryRepositoryConnection : BaseRepositoryConnection
    {
        private DotNetRdfInMemoryRepository _repo;
        private IInMemoryQueryableStore _store;
        private DotNetRdfValueFactory _factory;
        private bool _autoCommit = false;

        public DotNetRdfInMemoryRepositoryConnection(DotNetRdfInMemoryRepository repository, IInMemoryQueryableStore store, DotNetRdfValueFactory factory)
            : base(repository, factory)
        {
            this._repo = repository;
            this._store = store;
            this._factory = factory;
        }

        private bool AddGraph(IGraph g)
        {
            if (this._store.HasGraph(g.BaseUri))
            {
                IGraph target = this._store.Graphs[g.BaseUri];
                target.Assert(g.Triples.Select(t => t.CopyTriple(target)));
            }
            else 
            {
                this._store.Add(g);
            }
            return true;
        }

        private bool AddGraphToContext(Uri u, IGraph g)
        {
            if (this._store.HasGraph(u))
            {
                IGraph target = this._store.Graphs[u];
                target.Assert(g.Triples.Select(t => t.CopyTriple(target)));
            }
            else
            {
                Graph copy = new Graph();
                copy.BaseUri = u;
                copy.Assert(g.Triples.Select(t => t.CopyTriple(copy)));
                this.AddGraph(copy);
            }
            return true;
        }

        protected override void AddInternal(Object obj, IEnumerable<Uri> contexts)
        {
            if (contexts.Any())
            {
                SesameHelper.ModifyStore(obj, this.AddGraphToContext, contexts);
            }
            else
            {
                SesameHelper.ModifyStore(obj, this.AddGraph);
            }
        }

        public override void clear(params org.openrdf.model.Resource[] rarr)
        {
            if (rarr == null || rarr.Length == 0)
            {
                foreach (IGraph g in this._store.Graphs)
                {
                    this._store.Remove(g.BaseUri);
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public override void clearNamespaces()
        {
            this._factory.Graph.NamespaceMap.Clear();
        }

        public override void close()
        {
            //Nothing to do
        }

        public override void commit()
        {
            if (this._store is IFlushableStore)
            {
                ((IFlushableStore)this._store).Flush();
            }
        }

        public void export(org.openrdf.rio.RDFHandler rdfh, params org.openrdf.model.Resource[] rarr)
        {
            throw new NotImplementedException();
        }

        public void exportStatements(org.openrdf.model.Resource r, org.openrdf.model.URI uri, org.openrdf.model.Value v, bool b, org.openrdf.rio.RDFHandler rdfh, params org.openrdf.model.Resource[] rarr)
        {
            throw new NotImplementedException();
        }

        public override org.openrdf.repository.RepositoryResult getContextIDs()
        {
            throw new NotImplementedException();
        }

        public override string getNamespace(string str)
        {
            return this._factory.Graph.NamespaceMap.GetNamespaceUri(str).ToString();
        }

        public override org.openrdf.repository.RepositoryResult getNamespaces()
        {
            throw new NotImplementedException();
        }

        protected override org.openrdf.repository.RepositoryResult GetStatementsInternal(string sparqlQuery)
        {
            throw new NotImplementedException();
        }

        protected override bool HasTripleInternal(Triple t)
        {
            return this._store.Contains(t);
        }

        public override bool isEmpty()
        {
            return !this._store.Triples.Any();
        }

        public override bool isOpen()
        {
            return true;
        }

        public override org.openrdf.query.BooleanQuery prepareBooleanQuery(org.openrdf.query.QueryLanguage ql, string str)
        {
            if (!this.SupportsQueryLanguage(ql)) throw UnsupportedQueryLanguage(ql);
            return new InMemoryBooleanQuery(str, this._store);
        }

        public override org.openrdf.query.GraphQuery prepareGraphQuery(org.openrdf.query.QueryLanguage ql, string str)
        {
            if (!this.SupportsQueryLanguage(ql)) throw UnsupportedQueryLanguage(ql);
            return new InMemoryGraphQuery(str, this._store);
        }

        public override org.openrdf.query.Query prepareQuery(org.openrdf.query.QueryLanguage ql, string str)
        {
            if (!this.SupportsQueryLanguage(ql)) throw UnsupportedQueryLanguage(ql);
            try
            {
                SparqlQueryParser parser = new SparqlQueryParser();
                SparqlQuery q = parser.ParseFromString(str);

                switch (q.QueryType)
                {
                    case SparqlQueryType.Ask:
                        return new InMemoryBooleanQuery(str, this._store);
                    case SparqlQueryType.Construct:
                    case SparqlQueryType.Describe:
                    case SparqlQueryType.DescribeAll:
                        return new InMemoryGraphQuery(str, this._store);
                    case SparqlQueryType.Select:
                    case SparqlQueryType.SelectAll:
                    case SparqlQueryType.SelectAllDistinct:
                    case SparqlQueryType.SelectAllReduced:
                    case SparqlQueryType.SelectDistinct:
                    case SparqlQueryType.SelectReduced:
                        return new InMemoryTupleQuery(str, this._store);
                    case SparqlQueryType.Unknown:
                    default:
                        throw new dotSesameQuery.MalformedQueryException("Unable to parse the given Query into a valid SPARQL Query as the Query Type is unknown");
                }
            }
            catch (RdfParseException parseEx)
            {
                throw new dotSesameQuery.MalformedQueryException("Unable to parse the given Query into a valid SPARQL Query due to the following error: " + parseEx.Message);
            }
        }

        public override org.openrdf.query.TupleQuery prepareTupleQuery(org.openrdf.query.QueryLanguage ql, string str)
        {
            if (!this.SupportsQueryLanguage(ql)) throw UnsupportedQueryLanguage(ql);
            return new InMemoryTupleQuery(str, this._store);
        }

        private bool RemoveGraph(IGraph g)
        {
            if (this._store.HasGraph(g.BaseUri))
            {
                IGraph target = this._store.Graphs[g.BaseUri];
                target.Retract(g.Triples.Select(t => t.CopyTriple(target)));
            }
            return true;
        }

        private bool RemoveGraphFromContext(Uri u, IGraph g)
        {
            g.BaseUri = u;
            this.AddGraph(g);
            return true;
        }

        protected override void RemoveInternal(Object obj, IEnumerable<Uri> contexts)
        {
            if (contexts.Any())
            {
                SesameHelper.ModifyStore(obj, this.RemoveGraphFromContext, contexts);
            }
            else
            {
                SesameHelper.ModifyStore(obj, this.RemoveGraph);
            }
        }

        public override void removeNamespace(string str)
        {
            throw new NotImplementedException();
        }

        public override void rollback()
        {
            throw new NotImplementedException();
        }

        public override void setNamespace(string str1, string str2)
        {
            this._factory.Graph.NamespaceMap.AddNamespace(str1, new Uri(str2));
        }

        public override long size(params org.openrdf.model.Resource[] rarr)
        {
            if (rarr == null || rarr.Length == 0)
            {
                return this._store.Triples.Count();
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
