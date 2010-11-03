using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using dotSesame = org.openrdf.model;
using dotSesameRepo = org.openrdf.repository;
using java.io;
using VDS.RDF.Parsing;
using VDS.RDF.Storage;
using VDS.RDF.Storage.Params;
using VDS.RDF.Writing;

namespace VDS.RDF.Interop.Sesame
{
    public class DotNetRdfGenericRepository : dotSesameRepo.Repository
    {
        private IGenericIOManager _manager;
        private DotNetRdfGenericRepositoryConnection _connection;
        private DotNetRdfValueFactory _factory = new DotNetRdfValueFactory(new Graph());
        
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
            throw new NotSupportedException("dotNetRDF Generic Repositories do not support setting the data directory");
        }

        public void shutDown()
        {
            if (this._manager != null) this._manager.Dispose();
            this._manager = null;
        }
    }

    public class DotNetRdfGenericRepositoryConnection : dotSesameRepo.RepositoryConnection
    {
        private DotNetRdfGenericRepository _repo;
        private IGenericIOManager _manager;
        private DotNetRdfValueFactory _factory;
        private bool _autoCommit = false;

        public DotNetRdfGenericRepositoryConnection(DotNetRdfGenericRepository repo, IGenericIOManager manager, DotNetRdfValueFactory factory)
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

        public void add(info.aduna.iteration.Iteration i, params org.openrdf.model.Resource[] rarr)
        {
            if (this._manager.IsReadOnly) throw NotWritableError("add");
        }

        public void add(java.lang.Iterable i, params org.openrdf.model.Resource[] rarr)
        {
            if (this._manager.IsReadOnly) throw NotWritableError("add");

        }

        public void add(org.openrdf.model.Statement s, params org.openrdf.model.Resource[] rarr)
        {
            if (this._manager.IsReadOnly) throw NotWritableError("add");

        }

        public void  add(Reader r, string str, org.openrdf.rio.RDFFormat rdff, params org.openrdf.model.Resource[] rarr)
        {
            if (this._manager.IsReadOnly) throw NotWritableError("add");

        }

        public void add(InputStream @is, string str, org.openrdf.rio.RDFFormat rdff, params org.openrdf.model.Resource[] rarr)
        {
            if (this._manager.IsReadOnly) throw NotWritableError("add");

        }

        public void add(org.openrdf.model.Resource r, org.openrdf.model.URI uri, org.openrdf.model.Value v, params org.openrdf.model.Resource[] rarr)
        {
            if (this._manager.IsReadOnly) throw NotWritableError("add");

        }

        public void add(File f, string str, org.openrdf.rio.RDFFormat rdff, params org.openrdf.model.Resource[] rarr)
        {
            if (this._manager.IsReadOnly) throw NotWritableError("add");

            Object obj = SesameHelper.LoadFromFile(f, str, rdff);

            if (obj is ITripleStore)
            {
                foreach (IGraph x in ((ITripleStore)obj).Graphs)
                {
                    this._manager.SaveGraph(x);
                }
            }
            else if (obj is IGraph)
            {
                if (!((IGraph)obj).IsEmpty)
                {
                    this._manager.SaveGraph((IGraph)obj);
                }
            }
        }

        public void add(java.net.URL url, string str, org.openrdf.rio.RDFFormat rdff, params org.openrdf.model.Resource[] rarr)
        {
            if (this._manager.IsReadOnly) throw NotWritableError("add");

            Object obj = SesameHelper.LoadFromUri(url, str, rdff);

            if (obj is ITripleStore)
            {
                foreach (IGraph x in ((ITripleStore)obj).Graphs)
                {
                    this._manager.SaveGraph(x);
                }
            }
            else if (obj is IGraph)
            {
                if (!((IGraph)obj).IsEmpty)
                {
                    this._manager.SaveGraph((IGraph)obj);
                }
            }
        }

        public void clear(params org.openrdf.model.Resource[] rarr)
        {
            if (this._manager.IsReadOnly) throw NotWritableError("add");
            throw NoClearRepository();
        }

        public void clearNamespaces()
        {
            throw NoNamespacesError();
        }

        public void close()
        {
 	        //Nothing to do
        }

        public void commit()
        {
            if (this._manager.IsReadOnly) throw NotWritableError("commit");

        }

        public void export(org.openrdf.rio.RDFHandler rdfh, params org.openrdf.model.Resource[] rarr)
        {
 	        throw new NotImplementedException();
        }

        public void exportStatements(org.openrdf.model.Resource r, org.openrdf.model.URI uri, org.openrdf.model.Value v, bool b, org.openrdf.rio.RDFHandler rdfh, params org.openrdf.model.Resource[] rarr)
        {
 	        throw new NotImplementedException();
        }

        public org.openrdf.repository.RepositoryResult  getContextIDs()
        {
 	        throw new NotImplementedException();
        }

        public string getNamespace(string str)
        {
            throw NoNamespacesError();
        }

        public org.openrdf.repository.RepositoryResult getNamespaces()
        {
            throw NoNamespacesError();
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
 	        throw new NotImplementedException();
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
            if (this._manager.IsReadOnly) throw NotWritableError("remove");

        }

        public void remove(java.lang.Iterable i, params org.openrdf.model.Resource[] rarr)
        {
            if (this._manager.IsReadOnly) throw NotWritableError("remove");
        }

        public void remove(org.openrdf.model.Statement s, params org.openrdf.model.Resource[] rarr)
        {
            if (this._manager.IsReadOnly) throw NotWritableError("remove");
        }

        public void remove(org.openrdf.model.Resource r, org.openrdf.model.URI uri, org.openrdf.model.Value v, params org.openrdf.model.Resource[] rarr)
        {
            if (this._manager.IsReadOnly) throw NotWritableError("remove");
        }

        public void removeNamespace(string str)
        {
            throw NoNamespacesError();
        }

        public void rollback()
        {
            if (this._manager.IsReadOnly) throw NotWritableError("rollback");
        }

        public void setAutoCommit(bool b)
        {
            this._autoCommit = b;
        }

        public void setNamespace(string str1, string str2)
        {
            throw NoNamespacesError();
        }

        public long size(params org.openrdf.model.Resource[] rarr)
        {
 	        throw new NotImplementedException();
        }
    }
}
