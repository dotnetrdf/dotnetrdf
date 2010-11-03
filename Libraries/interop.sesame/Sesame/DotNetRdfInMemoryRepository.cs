using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dotSesame = org.openrdf.model;
using dotSesameRepo = org.openrdf.repository;
using dotSesameFormats = org.openrdf.rio;
using java.io;
using VDS.RDF.Parsing;
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

    public class DotNetRdfInMemoryRepositoryConnection : dotSesameRepo.RepositoryConnection
    {
        private DotNetRdfInMemoryRepository _repo;
        private IInMemoryQueryableStore _store;
        private DotNetRdfValueFactory _factory;
        private bool _autoCommit = false;

        public DotNetRdfInMemoryRepositoryConnection(DotNetRdfInMemoryRepository repository, IInMemoryQueryableStore store, DotNetRdfValueFactory factory)
        {
            this._repo = repository;
            this._store = store;
            this._factory = factory;
        }

        public void add(info.aduna.iteration.Iteration i, params org.openrdf.model.Resource[] rarr)
        {
            throw new NotImplementedException();
        }

        public void add(java.lang.Iterable i, params org.openrdf.model.Resource[] rarr)
        {
            throw new NotImplementedException();
        }

        public void add(org.openrdf.model.Statement s, params org.openrdf.model.Resource[] rarr)
        {
            throw new NotImplementedException();
        }

        public void add(Reader r, string str, org.openrdf.rio.RDFFormat rdff, params org.openrdf.model.Resource[] rarr)
        {
            throw new NotImplementedException();
        }

        public void add(InputStream @is, string str, org.openrdf.rio.RDFFormat rdff, params org.openrdf.model.Resource[] rarr)
        {
            throw new NotImplementedException();
        }

        public void add(org.openrdf.model.Resource r, org.openrdf.model.URI uri, org.openrdf.model.Value v, params org.openrdf.model.Resource[] rarr)
        {
            throw new NotImplementedException();
        }

        public void add(File f, string str, org.openrdf.rio.RDFFormat rdff, params org.openrdf.model.Resource[] rarr)
        {
            Object obj = SesameHelper.LoadFromFile(f, str, rdff);

            if (obj is ITripleStore)
            {
                foreach (IGraph x in ((ITripleStore)obj).Graphs)
                {
                    this._store.Add(x, true);
                }
            }
            else if (obj is IGraph)
            {
                if (!((IGraph)obj).IsEmpty)
                {
                    this._store.Add((IGraph)obj, true);
                }
            }
        }

        public void add(java.net.URL url, string str, org.openrdf.rio.RDFFormat rdff, params org.openrdf.model.Resource[] rarr)
        {
            Object obj = SesameHelper.LoadFromUri(url, str, rdff);

            if (obj is ITripleStore)
            {
                foreach (IGraph x in ((ITripleStore)obj).Graphs)
                {
                    this._store.Add(x, true);
                }
            }
            else if (obj is IGraph)
            {
                if (!((IGraph)obj).IsEmpty)
                {
                    this._store.Add((IGraph)obj, true);
                }
            }
        }

        public void clear(params org.openrdf.model.Resource[] rarr)
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

        public void clearNamespaces()
        {
            this._factory.Graph.NamespaceMap.Clear();
        }

        public void close()
        {
            //Nothing to do
        }

        public void commit()
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

        public org.openrdf.repository.RepositoryResult getContextIDs()
        {
            throw new NotImplementedException();
        }

        public string getNamespace(string str)
        {
            return this._factory.Graph.NamespaceMap.GetNamespaceUri(str).ToString();
        }

        public org.openrdf.repository.RepositoryResult getNamespaces()
        {
            throw new NotImplementedException();
        }

        public org.openrdf.repository.Repository getRepository()
        {
            return this._repo;
        }

        public org.openrdf.repository.RepositoryResult getStatements(org.openrdf.model.Resource r, org.openrdf.model.URI uri, org.openrdf.model.Value v, bool b, params org.openrdf.model.Resource[] rarr)
        {
            throw new NotImplementedException();
        }

        public org.openrdf.model.ValueFactory getValueFactory()
        {
            return this._factory;
        }

        public bool hasStatement(org.openrdf.model.Statement s, bool b, params org.openrdf.model.Resource[] rarr)
        {
            throw new NotImplementedException();
        }

        public bool hasStatement(org.openrdf.model.Resource r, org.openrdf.model.URI uri, org.openrdf.model.Value v, bool b, params org.openrdf.model.Resource[] rarr)
        {
            throw new NotImplementedException();
        }

        public bool isAutoCommit()
        {
            return this._autoCommit;
        }

        public bool isEmpty()
        {
            return !this._store.Triples.Any();
        }

        public bool isOpen()
        {
            return true;
        }

        public org.openrdf.query.BooleanQuery prepareBooleanQuery(org.openrdf.query.QueryLanguage ql, string str1, string str2)
        {
            throw new NotImplementedException();
        }

        public org.openrdf.query.BooleanQuery prepareBooleanQuery(org.openrdf.query.QueryLanguage ql, string str)
        {
            throw new NotImplementedException();
        }

        public org.openrdf.query.GraphQuery prepareGraphQuery(org.openrdf.query.QueryLanguage ql, string str1, string str2)
        {
            throw new NotImplementedException();
        }

        public org.openrdf.query.GraphQuery prepareGraphQuery(org.openrdf.query.QueryLanguage ql, string str)
        {
            throw new NotImplementedException();
        }

        public org.openrdf.query.Query prepareQuery(org.openrdf.query.QueryLanguage ql, string str1, string str2)
        {
            throw new NotImplementedException();
        }

        public org.openrdf.query.Query prepareQuery(org.openrdf.query.QueryLanguage ql, string str)
        {
            throw new NotImplementedException();
        }

        public org.openrdf.query.TupleQuery prepareTupleQuery(org.openrdf.query.QueryLanguage ql, string str1, string str2)
        {
            throw new NotImplementedException();
        }

        public org.openrdf.query.TupleQuery prepareTupleQuery(org.openrdf.query.QueryLanguage ql, string str)
        {
            throw new NotImplementedException();
        }

        public void remove(info.aduna.iteration.Iteration i, params org.openrdf.model.Resource[] rarr)
        {
            throw new NotImplementedException();
        }

        public void remove(java.lang.Iterable i, params org.openrdf.model.Resource[] rarr)
        {
            throw new NotImplementedException();
        }

        public void remove(org.openrdf.model.Statement s, params org.openrdf.model.Resource[] rarr)
        {
            throw new NotImplementedException();
        }

        public void remove(org.openrdf.model.Resource r, org.openrdf.model.URI uri, org.openrdf.model.Value v, params org.openrdf.model.Resource[] rarr)
        {
            throw new NotImplementedException();
        }

        public void removeNamespace(string str)
        {
            throw new NotImplementedException();
        }

        public void rollback()
        {
            throw new NotImplementedException();
        }

        public void setAutoCommit(bool b)
        {
            this._autoCommit = true;
        }

        public void setNamespace(string str1, string str2)
        {
            this._factory.Graph.NamespaceMap.AddNamespace(str1, new Uri(str2));
        }

        public long size(params org.openrdf.model.Resource[] rarr)
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
