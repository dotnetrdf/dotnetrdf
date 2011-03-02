using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using dotSesame = org.openrdf.model;
using dotSesameRepo = org.openrdf.repository;
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
    public class DotNetRdfGenericRepository : dotSesameRepo.Repository, IConfigurationSerializable
    {
        private IGenericIOManager _manager;
        private DotNetRdfGenericRepositoryConnection _connection;
        private DotNetRdfValueFactory _factory = new DotNetRdfValueFactory();
        
        public DotNetRdfGenericRepository(IGenericIOManager manager)
        {
            this._manager = manager;
        }

        public org.openrdf.repository.RepositoryConnection getConnection()
        {
            if (this._connection == null) this._connection = new DotNetRdfGenericRepositoryConnection(this, this._manager, this._factory);
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
            while (!this._manager.IsReady)
            {
                Thread.Sleep(50);
            }
        }

        public bool isWritable()
        {
            return !this._manager.IsReadOnly;
        }

        public void setDataDir(File f)
        {
            //Do Nothing
        }

        public void shutDown()
        {
            if (this._manager != null) this._manager.Dispose();
            this._manager = null;
        }

        public void SerializeConfiguration(ConfigurationSerializationContext context)
        {
            if (this._manager is IConfigurationSerializable)
            {
                ((IConfigurationSerializable)this._manager).SerializeConfiguration(context);
            }
            else
            {
                throw new dotSesameRepo.config.RepositoryConfigException("This Repository does not support serializing it's Configuration");
            }
        }
    }

    public class DotNetRdfGenericRepositoryConnection : BaseRepositoryConnection
    {
        private DotNetRdfGenericRepository _repo;
        private IGenericIOManager _manager;
        private DotNetRdfValueFactory _factory;

        public DotNetRdfGenericRepositoryConnection(DotNetRdfGenericRepository repo, IGenericIOManager manager, DotNetRdfValueFactory factory)
            : base(repo, factory)
        {
            this._repo = repo;
            this._manager = manager;
            this._factory = factory;
        }

        private dotSesameRepo.RepositoryReadOnlyException NotWritableError(String op)
        {
            return new dotSesameRepo.RepositoryReadOnlyException("Cannot perform the requested " + op + " operation as the Repository is Read Only");
        }

        private dotSesameRepo.RepositoryException NoNamespacesError()
        {
            return new dotSesameRepo.RepositoryException("dotNetRDF Generic Repositories do not support Namespaces");
        }

        private dotSesameRepo.RepositoryException NoClearRepository()
        {
            return new dotSesameRepo.RepositoryException("dotNetRDF Generic Repositories do not currently support the clear() operation");
        }

        private bool AddGraph(IGraph g)
        {
            if (this._manager.UpdateSupported)
            {
                this._manager.UpdateGraph(g.BaseUri, g.Triples, null);
            }
            else
            {
                Graph temp = new Graph();
                temp.BaseUri = g.BaseUri;
                this._manager.LoadGraph(temp, temp.BaseUri);
                temp.Assert(g.Triples);
                this._manager.SaveGraph(temp);
            }
            return true;
        }

        private bool AddGraphToContext(Uri u, IGraph g)
        {
            if (this._manager.UpdateSupported)
            {
                this._manager.UpdateGraph(u, g.Triples, null);
            }
            else
            {
                Graph temp = new Graph();
                temp.BaseUri = g.BaseUri;
                this._manager.LoadGraph(temp, temp.BaseUri);
                temp.Assert(g.Triples);
                this._manager.SaveGraph(temp);
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

        public override void add(info.aduna.iteration.Iteration i, params org.openrdf.model.Resource[] rarr)
        {
            if (this._manager.IsReadOnly) throw NotWritableError("add");

            base.add(i, rarr);
        }

        public override void add(java.lang.Iterable i, params org.openrdf.model.Resource[] rarr)
        {
            if (this._manager.IsReadOnly) throw NotWritableError("add");

            base.add(i, rarr);
        }

        public override void add(org.openrdf.model.Statement s, params org.openrdf.model.Resource[] rarr)
        {
            if (this._manager.IsReadOnly) throw NotWritableError("add");

            base.add(s, rarr);
        }

        public override void add(Reader r, string str, org.openrdf.rio.RDFFormat rdff, params org.openrdf.model.Resource[] rarr)
        {
            if (this._manager.IsReadOnly) throw NotWritableError("add");

            base.add(r, str, rdff, rarr);
        }

        public override void add(InputStream @is, string str, org.openrdf.rio.RDFFormat rdff, params org.openrdf.model.Resource[] rarr)
        {
            if (this._manager.IsReadOnly) throw NotWritableError("add");

            base.add(@is, str, rdff, rarr);
        }

        public override void add(org.openrdf.model.Resource r, org.openrdf.model.URI uri, org.openrdf.model.Value v, params org.openrdf.model.Resource[] rarr)
        {
            if (this._manager.IsReadOnly) throw NotWritableError("add");

            base.add(r, uri, v, rarr);
        }

        public override void add(File f, string str, org.openrdf.rio.RDFFormat rdff, params org.openrdf.model.Resource[] rarr)
        {
            if (this._manager.IsReadOnly) throw NotWritableError("add");

            base.add(f, str, rdff, rarr);
        }

        public override void add(java.net.URL url, string str, org.openrdf.rio.RDFFormat rdff, params org.openrdf.model.Resource[] rarr)
        {
            if (this._manager.IsReadOnly) throw NotWritableError("add");

            base.add(url, str, rdff, rarr);
        }

        public override void clear(params org.openrdf.model.Resource[] rarr)
        {
            if (this._manager.IsReadOnly) throw NotWritableError("add");
            throw NoClearRepository();
        }

        public override void clearNamespaces()
        {
            throw NoNamespacesError();
        }

        public override void close()
        {
 	        //Nothing to do
        }

        public override void commit()
        {
            if (this._manager.IsReadOnly) throw NotWritableError("commit");

            //HACK: Commit() in effect does nothing as no way of knowing whether the underlying Store supports transactions
            //If the underlying Store does support transactions then typically these will be used by the IGenericIOManager implementation
            //but this behaviour is not made publicly available to end users
            //throw new dotSesameRepo.RepositoryException("dotNetRDF Generic Repositories do not support the commit() operation");
        }

        public override org.openrdf.repository.RepositoryResult getContextIDs()
        {
            if (this._manager.ListGraphsSupported)
            {
                IEnumerable<dotSesame.Resource> resIter = from u in this._manager.ListGraphs()
                                                          select (dotSesame.Resource)this._mapping.ValueFactory.createURI(u.ToString());

                return new org.openrdf.repository.RepositoryResult(new DotNetAdunaIterationWrapper(resIter));
            }
            else if (this._manager is IQueryableGenericIOManager)
            {
                try 
                {
                    Object results = ((IQueryableGenericIOManager)this._manager).Query("SELECT DISTINCT ?g WHERE { GRAPH ?g { ?s ?p ?o }}");
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
            else
            {
                throw new dotSesameRepo.RepositoryException("This dotNetRDF Generic Repository does not support returning the Context IDs");
            }
        }

        public override string getNamespace(string str)
        {
            throw NoNamespacesError();
        }

        public override org.openrdf.repository.RepositoryResult getNamespaces()
        {
            throw NoNamespacesError();
        }

        protected override org.openrdf.repository.RepositoryResult GetStatementsInternal(string sparqlQuery, SesameMapping mapping)
        {
            if (this._manager is IQueryableGenericIOManager)
            {
                try
                {
                    Object results = ((IQueryableGenericIOManager)this._manager).Query(sparqlQuery);
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
            else
            {
                throw new dotSesameRepo.RepositoryException("This dotNetRDF Generic Repository does not support returning specific Statements");
            }
        }

        protected override bool HasTripleInternal(Triple t)
        {
            if (this._manager is IQueryableGenericIOManager)
            {
                SparqlParameterizedString queryString = new SparqlParameterizedString("ASK WHERE { @subject @predicate @object}");
                queryString.SetParameter("subject", t.Subject);
                queryString.SetParameter("predicate", t.Predicate);
                queryString.SetParameter("object", t.Object);

                Object results = ((IQueryableGenericIOManager)this._manager).Query(queryString.ToString());
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
            else
            {
                throw new dotSesameRepo.RepositoryException("This dotNetRDF Generic Repository does not support detecting whether a given Statement exists in the Store");
            }
        }

        protected override bool HasTriplesInternal(string askQuery)
        {
            if (this._manager is IQueryableGenericIOManager)
            {
                Object results = ((IQueryableGenericIOManager)this._manager).Query(askQuery);
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
            else
            {
                throw new dotSesameRepo.RepositoryException("This dotNetRDF Generic Repository does not support detecting whether certain Statements exist in the Store");
            }
        }

        public override bool isEmpty()
        {
            throw new dotSesameRepo.RepositoryException("dotNetRDF Generic Repositories do no support indicating whether they are empty");
        }

        public override bool isOpen()
        {
            return true;
        }

        public override org.openrdf.query.BooleanQuery prepareBooleanQuery(org.openrdf.query.QueryLanguage ql, string str)
        {
            if (!this.SupportsQueryLanguage(ql)) throw UnsupportedQueryLanguage(ql);
            if (this._manager is IQueryableGenericIOManager)
            {
                return new GenericBooleanQuery(str, (IQueryableGenericIOManager)this._manager);
            }
            else
            {
                throw new dotSesameRepo.RepositoryException("This dotNetRDF Generic Repository does not support SPARQL Queries");
            }
        }

        public override org.openrdf.query.GraphQuery prepareGraphQuery(org.openrdf.query.QueryLanguage ql, string str)
        {
            if (!this.SupportsQueryLanguage(ql)) throw UnsupportedQueryLanguage(ql);
            if (this._manager is IQueryableGenericIOManager)
            {
                return new GenericGraphQuery(str, (IQueryableGenericIOManager)this._manager);
            }
            else
            {
                throw new dotSesameRepo.RepositoryException("This dotNetRDF Generic Repository does not support SPARQL Queries");
            }
        }

        public override org.openrdf.query.Query prepareQuery(org.openrdf.query.QueryLanguage ql, string str)
        {
            if (!this.SupportsQueryLanguage(ql)) throw UnsupportedQueryLanguage(ql);

            if (!(this._manager is IQueryableGenericIOManager))
            {
                throw new dotSesameRepo.RepositoryException("This dotNetRDF Generic Repository does not support SPARQL Queries");
            }

            try
            {
                SparqlQueryParser parser = new SparqlQueryParser();
                SparqlQuery q = parser.ParseFromString(str);

                switch (q.QueryType)
                {
                    case SparqlQueryType.Ask:
                        return new GenericBooleanQuery(str, (IQueryableGenericIOManager)this._manager);
                    case SparqlQueryType.Construct:
                    case SparqlQueryType.Describe:
                    case SparqlQueryType.DescribeAll:
                        return new GenericGraphQuery(str, (IQueryableGenericIOManager)this._manager);
                    case SparqlQueryType.Select:
                    case SparqlQueryType.SelectAll:
                    case SparqlQueryType.SelectAllDistinct:
                    case SparqlQueryType.SelectAllReduced:
                    case SparqlQueryType.SelectDistinct:
                    case SparqlQueryType.SelectReduced:
                        return new GenericTupleQuery(str, (IQueryableGenericIOManager)this._manager);
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
            if (this._manager is IQueryableGenericIOManager)
            {
                return new GenericTupleQuery(str, (IQueryableGenericIOManager)this._manager);
            }
            else
            {
                throw new dotSesameRepo.RepositoryException("This dotNetRDF Generic Repository does not support SPARQL Queries");
            }
        }

        private bool RemoveGraph(IGraph g)
        {
            if (this._manager.UpdateSupported)
            {
                this._manager.UpdateGraph(g.BaseUri, null, g.Triples);
            }
            else
            {
                Graph temp = new Graph();
                temp.BaseUri = g.BaseUri;
                this._manager.LoadGraph(temp, temp.BaseUri);
                temp.Retract(g.Triples);
                this._manager.SaveGraph(temp);
            }
            return true;
        }

        private bool RemoveGraphFromContext(Uri u, IGraph g)
        {
            if (this._manager.UpdateSupported)
            {
                this._manager.UpdateGraph(u, null, g.Triples);
            }
            else
            {
                Graph temp = new Graph();
                temp.BaseUri = g.BaseUri;
                this._manager.LoadGraph(temp, temp.BaseUri);
                temp.Retract(g.Triples);
                this._manager.SaveGraph(temp);
            }
            return true;
        }

        protected override void RemoveInternal(object obj, IEnumerable<Uri> contexts)
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

        public override void remove(info.aduna.iteration.Iteration i, params org.openrdf.model.Resource[] rarr)
        {
            if (this._manager.IsReadOnly) throw NotWritableError("remove");

            base.remove(i, rarr);
        }

        public override void remove(java.lang.Iterable i, params org.openrdf.model.Resource[] rarr)
        {
            if (this._manager.IsReadOnly) throw NotWritableError("remove");

            base.remove(i, rarr);
        }

        public override void remove(org.openrdf.model.Statement s, params org.openrdf.model.Resource[] rarr)
        {
            if (this._manager.IsReadOnly) throw NotWritableError("remove");

            base.remove(s, rarr);
        }

        public override void remove(org.openrdf.model.Resource r, org.openrdf.model.URI uri, org.openrdf.model.Value v, params org.openrdf.model.Resource[] rarr)
        {
            if (this._manager.IsReadOnly) throw NotWritableError("remove");

            base.remove(r, uri, v, rarr);
        }

        public override void removeNamespace(string str)
        {
            throw NoNamespacesError();
        }

        public override void setNamespace(string str1, string str2)
        {
            throw NoNamespacesError();
        }

        public override long size(params org.openrdf.model.Resource[] rarr)
        {
            throw new dotSesameRepo.RepositoryException("dotNetRDF Generic Repositories do not support getting the size of the repository");
        }
    }
}
