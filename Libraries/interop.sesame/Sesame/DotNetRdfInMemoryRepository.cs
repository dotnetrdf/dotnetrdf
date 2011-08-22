using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dotSesame = org.openrdf.model;
using dotSesameRepo = org.openrdf.repository;
using dotSesameFormats = org.openrdf.rio;
using dotSesameQuery = org.openrdf.query;
using java.io;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Storage;
using VDS.RDF.Storage.Params;
using VDS.RDF.Writing;

namespace VDS.RDF.Interop.Sesame
{
    public class DotNetRdfInMemoryRepository : dotSesameRepo.Repository, IConfigurationSerializable
    {
        private IInMemoryQueryableStore _store;
        private DotNetRdfInMemoryRepositoryConnection _connection;
        private DotNetRdfValueFactory _factory = new DotNetRdfValueFactory();

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
            //Do Nothing
        }

        public void shutDown()
        {
            if (this._store != null)
            {
                if (this._store is ITransactionalStore)
                {
                    ((ITransactionalStore)this._store).Flush();
                }
                this._store.Dispose();
            }
            this._store = null;
        }

        public void SerializeConfiguration(ConfigurationSerializationContext context)
        {
            INode subj = context.NextSubject;
            context.Graph.Assert(subj, context.Graph.CreateUriNode(new Uri(RdfSpecsHelper.RdfType)), ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.ClassTripleStore));
            context.Graph.Assert(subj, ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyType), context.Graph.CreateLiteralNode(this._store.GetType().FullName));
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
                foreach (Uri u in rarr.ToContexts(this._mapping))
                {
                    if (this._store.HasGraph(u))
                    {
                        this._store.Remove(u);
                    }
                }
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
            if (this._store is ITransactionalStore)
            {
                ((ITransactionalStore)this._store).Flush();
            }
        }

        public override org.openrdf.repository.RepositoryResult getContextIDs()
        {
            try
            {
                Object results = this._store.ExecuteQuery("SELECT DISTINCT ?g WHERE { GRAPH ?g { ?s ?p ?o }}");
                if (results is SparqlResultSet)
                {
                    IEnumerable<dotSesame.Resource> resIter = from result in (SparqlResultSet)results
                                                              where result.HasValue("g") && result["g"] != null
                                                              select SesameConverter.ToSesameResource(result["g"], this._mapping);
                    return new org.openrdf.repository.RepositoryResult(new DotNetAdunaIterationWrapper(resIter));
                }
                else
                {
                    throw new dotSesameRepo.RepositoryException("Unable to return the Context IDs from this repository as the repository returned an unexpected result");
                }
            }
            catch (Exception ex)
            {
                throw new dotSesameRepo.RepositoryException("Unable to return the Context IDs from this repository due to the following error: " + ex.Message);
            }
        }

        public override string getNamespace(string str)
        {
            return this._factory.Graph.NamespaceMap.GetNamespaceUri(str).ToString();
        }

        public override org.openrdf.repository.RepositoryResult getNamespaces()
        {
            IEnumerable<dotSesame.impl.NamespaceImpl> ns = from prefix in this._factory.Graph.NamespaceMap.Prefixes.Distinct()
                                                           select new dotSesame.impl.NamespaceImpl(prefix, this._factory.Graph.NamespaceMap.GetNamespaceUri(prefix).ToString());
            return new dotSesameRepo.RepositoryResult(new DotNetAdunaIterationWrapper(ns));
        }

        protected override org.openrdf.repository.RepositoryResult GetStatementsInternal(string sparqlQuery, SesameMapping mapping)
        {
            try
            {
                Object results = this._store.ExecuteQuery(sparqlQuery);
                if (results is SparqlResultSet)
                {
                    IEnumerable<dotSesame.Statement> stmts = from result in (SparqlResultSet)results
                                                             select this._factory.createStatement(SesameConverter.ToSesameResource(result["subj"], mapping), SesameConverter.ToSesameUri(result["pred"], mapping), SesameConverter.ToSesameValue(result["obj"], mapping));
                    return new dotSesameRepo.RepositoryResult(new DotNetAdunaIterationWrapper(stmts));
                }
                else
                {
                    throw new dotSesameRepo.RepositoryException("Unable to return Statements from this repository as the repository returned an unexpected result");
                }
            }
            catch (Exception ex)
            {
                throw new dotSesameRepo.RepositoryException("Unable to return Statements from this repository due to the following error: " + ex.Message);
            }
        }

        protected override bool HasTripleInternal(Triple t)
        {
            return this._store.Contains(t);
        }

        protected override bool HasTriplesInternal(string askQuery)
        {
            Object results = this._store.ExecuteQuery(askQuery);
            if (results is SparqlResultSet)
            {
                SparqlResultSet rset = (SparqlResultSet)results;
                return rset.ResultsType == SparqlResultsType.Boolean && rset.Result;
            }
            else
            {
                return false;
            }
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
            this._factory.Graph.NamespaceMap.RemoveNamespace(str);
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
                List<Uri> contexts = rarr.ToContexts(this._mapping).Distinct().ToList();
                int sum = 0;
                foreach (Uri u in contexts)
                {
                    if (this._store.HasGraph(u))
                    {
                        sum += this._store.Graphs[u].Triples.Count;
                    }
                }
                return sum;
            }
        }
    }
}
