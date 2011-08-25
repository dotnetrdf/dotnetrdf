using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using java.io;
using dotSesame = org.openrdf.model;
using dotSesameRepo = org.openrdf.repository;
using VDS.RDF.Query;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Interop.Sesame
{
    public abstract class BaseRepositoryConnection : dotSesameRepo.RepositoryConnection
    {
        private bool _autoCommit = false;
        private dotSesameRepo.Repository _repo;
        private DotNetRdfValueFactory _factory;
        protected SesameMapping _mapping;
        private SparqlFormatter _formatter = new SparqlFormatter();

        public BaseRepositoryConnection(dotSesameRepo.Repository repository, DotNetRdfValueFactory factory)
        {
            this._repo = repository;
            this._mapping = factory.Mapping;
            this._factory = factory;
        }

        protected org.openrdf.query.UnsupportedQueryLanguageException UnsupportedQueryLanguage(org.openrdf.query.QueryLanguage ql)
        {
            throw new org.openrdf.query.UnsupportedQueryLanguageException("dotNetRDF Repository Connections do not support the " + ql.getName() + " Query Language");
        }

        protected bool SupportsQueryLanguage(org.openrdf.query.QueryLanguage ql)
        {
            return ql.getName().ToUpper().Equals("SPARQL");
        }



        protected abstract void AddInternal(Object obj, IEnumerable<Uri> contexts);

        public virtual void add(info.aduna.iteration.Iteration i, params org.openrdf.model.Resource[] rarr)
        {
            IEnumerable<Uri> contexts = rarr.ToContexts(this._mapping);
            Graph g = new Graph();
            g.Assert(i.ToTriples(this._mapping));
            this.AddInternal(g, contexts);
        }

        public virtual void add(java.lang.Iterable i, params org.openrdf.model.Resource[] rarr)
        {
            IEnumerable<Uri> contexts = rarr.ToContexts(this._mapping);
            Graph g = new Graph();
            g.Assert(new JavaIteratorWrapper<dotSesame.Statement>(i.iterator()).Select(s => SesameConverter.FromSesame(s, this._mapping)));
            this.AddInternal(g, contexts);
        }

        public virtual void add(org.openrdf.model.Statement s, params org.openrdf.model.Resource[] rarr)
        {
            IEnumerable<Uri> contexts = rarr.ToContexts(this._mapping);
            Graph g = new Graph();
            g.Assert(SesameConverter.FromSesame(s, this._mapping));
            this.AddInternal(g, contexts);
        }

        public virtual void add(Reader r, string str, org.openrdf.rio.RDFFormat rdff, params org.openrdf.model.Resource[] rarr)
        {
            Object obj = SesameHelper.LoadFromReader(r, str, rdff);
            IEnumerable<Uri> contexts = rarr.ToContexts(this._mapping);
            this.AddInternal(obj, contexts);
        }

        public virtual void add(InputStream @is, string str, org.openrdf.rio.RDFFormat rdff, params org.openrdf.model.Resource[] rarr)
        {
            Object obj = SesameHelper.LoadFromStream(@is, str, rdff);
            IEnumerable<Uri> contexts = rarr.ToContexts(this._mapping);
            this.AddInternal(obj, contexts);
        }

        public virtual void add(org.openrdf.model.Resource r, org.openrdf.model.URI uri, org.openrdf.model.Value v, params org.openrdf.model.Resource[] rarr)
        {
            IEnumerable<Uri> contexts = rarr.ToContexts(this._mapping);
            Graph g = new Graph();
            g.Assert(SesameConverter.FromSesameResource(r, this._mapping), SesameConverter.FromSesameUri(uri, this._mapping), SesameConverter.FromSesameValue(v, this._mapping));
            this.AddInternal(g, contexts);
        }

        public virtual void add(File f, string str, org.openrdf.rio.RDFFormat rdff, params org.openrdf.model.Resource[] rarr)
        {
            Object obj = SesameHelper.LoadFromFile(f, str, rdff);
            IEnumerable<Uri> contexts = rarr.ToContexts(this._mapping);
            this.AddInternal(obj, contexts);
        }

        public virtual void add(java.net.URL url, string str, org.openrdf.rio.RDFFormat rdff, params org.openrdf.model.Resource[] rarr)
        {
            Object obj = SesameHelper.LoadFromUri(url, str, rdff);
            IEnumerable<Uri> contexts = rarr.ToContexts(this._mapping);
            this.AddInternal(obj, contexts);
        }

        public abstract void clear(params org.openrdf.model.Resource[] rarr);

        public abstract void clearNamespaces();

        public abstract void close();

        public abstract void commit();

        public void export(org.openrdf.rio.RDFHandler rdfh, params org.openrdf.model.Resource[] rarr)
        {
            this.exportStatements(null, null, null, false, rdfh, rarr);
        }

        public void exportStatements(org.openrdf.model.Resource r, org.openrdf.model.URI uri, org.openrdf.model.Value v, bool b, org.openrdf.rio.RDFHandler rdfh, params org.openrdf.model.Resource[] rarr)
        {
            dotSesameRepo.RepositoryResult results = this.getStatements(r, uri, v, b, rarr);
            rdfh.startRDF();
            while (results.hasNext())
            {
                rdfh.handleStatement((dotSesame.Statement)results.next());
            }
            rdfh.endRDF();
        }

        public abstract org.openrdf.repository.RepositoryResult getContextIDs();

        public abstract string getNamespace(string str);

        public abstract org.openrdf.repository.RepositoryResult getNamespaces();

        public org.openrdf.repository.Repository getRepository()
        {
            return this._repo;
        }

        protected abstract dotSesameRepo.RepositoryResult GetStatementsInternal(String sparqlQuery, SesameMapping mapping);

        public org.openrdf.repository.RepositoryResult getStatements(org.openrdf.model.Resource r, org.openrdf.model.URI uri, org.openrdf.model.Value v, bool b, params org.openrdf.model.Resource[] rarr)
        {
            SparqlParameterizedString queryString = new SparqlParameterizedString();
            IEnumerable<Uri> contexts = rarr.ToContexts(this._mapping);
            if (contexts.Any())
            {
                queryString.CommandText = "SELECT (?s AS ?subj) (?p AS ?pred) (?o AS ?obj)\n";
                foreach (Uri u in contexts)
                {
                    queryString.CommandText += "FROM <" + this._formatter.FormatUri(u) + ">\n";
                }
                queryString.CommandText += "WHERE { ?s ?p ?o }";
            }
            else
            {
                queryString.CommandText = "SELECT (?s AS ?subj) (?p AS ?pred) (?o AS ?obj) WHERE { ?s ?p ?o }";
            }
            if (r != null) queryString.SetVariable("s", SesameConverter.FromSesameResource(r, this._mapping));
            if (uri != null) queryString.SetVariable("p", SesameConverter.FromSesameUri(uri, this._mapping));
            if (v != null) queryString.SetVariable("o", SesameConverter.FromSesameValue(v, this._mapping));

            return this.GetStatementsInternal(queryString.ToString(), this._mapping);
        }

        public org.openrdf.model.ValueFactory getValueFactory()
        {
            return this._factory;
        }

        protected abstract bool HasTripleInternal(Triple t);

        protected abstract bool HasTriplesInternal(String askQuery);

        public bool hasStatement(org.openrdf.model.Statement s, bool b, params org.openrdf.model.Resource[] rarr)
        {
            Triple t = SesameConverter.FromSesame(s, this._mapping);
            return HasTripleInternal(t);
        }

        public bool hasStatement(org.openrdf.model.Resource r, org.openrdf.model.URI uri, org.openrdf.model.Value v, bool b, params org.openrdf.model.Resource[] rarr)
        {
            SparqlParameterizedString queryString = new SparqlParameterizedString();
            queryString.CommandText = "ASK";
            foreach (Uri u in rarr.ToContexts(this._mapping))
            {
                queryString.CommandText += "\nFROM <" + this._formatter.FormatUri(u) + ">";
            }
            queryString.CommandText += "\nWHERE { ?subject ?predicate ?object}";
            if (r != null) queryString.SetVariable("subject", SesameConverter.FromSesameResource(r, this._mapping));
            if (uri != null) queryString.SetVariable("predicate", SesameConverter.FromSesameUri(uri, this._mapping));
            if (v != null) queryString.SetVariable("object", SesameConverter.FromSesameValue(v, this._mapping));

            return this.HasTriplesInternal(queryString.ToString());
        }

        public bool isAutoCommit()
        {
            return this._autoCommit;
        }

        public abstract bool isEmpty();

        public abstract bool isOpen();

        public org.openrdf.query.BooleanQuery prepareBooleanQuery(org.openrdf.query.QueryLanguage ql, string str1, string str2)
        {
            if (!this.SupportsQueryLanguage(ql)) throw UnsupportedQueryLanguage(ql);
            String query = "BASE <" + this._formatter.FormatUri(str2) + ">\n" + str1;
            return this.prepareBooleanQuery(ql, query);
        }

        public abstract org.openrdf.query.BooleanQuery prepareBooleanQuery(org.openrdf.query.QueryLanguage ql, string str);

        public org.openrdf.query.GraphQuery prepareGraphQuery(org.openrdf.query.QueryLanguage ql, string str1, string str2)
        {
            if (!this.SupportsQueryLanguage(ql)) throw UnsupportedQueryLanguage(ql);
            String query = "BASE <" + this._formatter.FormatUri(str2) + ">\n" + str1;
            return this.prepareGraphQuery(ql, query);
        }

        public abstract org.openrdf.query.GraphQuery prepareGraphQuery(org.openrdf.query.QueryLanguage ql, string str);

        public org.openrdf.query.Query prepareQuery(org.openrdf.query.QueryLanguage ql, string str1, string str2)
        {
            if (!this.SupportsQueryLanguage(ql)) throw UnsupportedQueryLanguage(ql);
            String query = "BASE <" + this._formatter.FormatUri(str2) + ">\n" + str1;
            return this.prepareQuery(ql, query);
        }

        public abstract org.openrdf.query.Query prepareQuery(org.openrdf.query.QueryLanguage ql, string str);

        public org.openrdf.query.TupleQuery prepareTupleQuery(org.openrdf.query.QueryLanguage ql, string str1, string str2)
        {
            if (!this.SupportsQueryLanguage(ql)) throw UnsupportedQueryLanguage(ql);
            String query = "BASE <" + this._formatter.FormatUri(str2) + ">\n" + str1;
            return this.prepareTupleQuery(ql, query);
        }

        public abstract org.openrdf.query.TupleQuery prepareTupleQuery(org.openrdf.query.QueryLanguage ql, string str);

        protected abstract void RemoveInternal(Object obj, IEnumerable<Uri> contexts);

        public virtual void remove(info.aduna.iteration.Iteration i, params org.openrdf.model.Resource[] rarr)
        {
            IEnumerable<Uri> contexts = rarr.ToContexts(this._mapping);
            Graph g = new Graph();
            g.Assert(i.ToTriples(this._mapping));
            this.RemoveInternal(g, contexts);
        }

        public virtual void remove(java.lang.Iterable i, params org.openrdf.model.Resource[] rarr)
        {
            IEnumerable<Uri> contexts = rarr.ToContexts(this._mapping);
            Graph g = new Graph();
            g.Assert(new JavaIteratorWrapper<dotSesame.Statement>(i.iterator()).Select(s => SesameConverter.FromSesame(s, this._mapping)));
            this.RemoveInternal(g, contexts);
        }

        public virtual void remove(org.openrdf.model.Statement s, params org.openrdf.model.Resource[] rarr)
        {
            IEnumerable<Uri> contexts = rarr.ToContexts(this._mapping);
            Graph g = new Graph();
            g.Assert(SesameConverter.FromSesame(s, this._mapping));
            this.RemoveInternal(g, contexts);
        }

        public virtual void remove(org.openrdf.model.Resource r, org.openrdf.model.URI uri, org.openrdf.model.Value v, params org.openrdf.model.Resource[] rarr)
        {
            throw new NotImplementedException();
        }

        public abstract void removeNamespace(string str);

        public virtual void rollback()
        {
            //Does Nothing
        }

        public void setAutoCommit(bool b)
        {
            this._autoCommit = b;
        }

        public abstract void setNamespace(string str1, string str2);

        public abstract long size(params org.openrdf.model.Resource[] rarr);
    }
}
