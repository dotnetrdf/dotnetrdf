using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dotSesame = org.openrdf.model;
using dotSesameQuery = org.openrdf.query;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Interop.Sesame
{
    public abstract class BaseQuery : dotSesameQuery.Query
    {
        private static SparqlQueryParser _parser;

        private SparqlParameterizedString _queryString;
        private SesameMapping _mapping = new SesameMapping(new Graph(), new dotSesame.impl.GraphImpl());

        private long _timeout = 0;
        protected long _evalTime = 0;

        public BaseQuery(String query)
        {
            this._queryString = new SparqlParameterizedString(query);
        }

        protected static SparqlQuery ParseQuery(String query)
        {
            if (_parser == null) _parser = new SparqlQueryParser();
            return _parser.ParseFromString(query);
        }

        protected String SparqlQuery
        {
            get
            {
                return this._queryString.ToString();
            }
        }

        protected long QueryTimeout
        {
            get
            {
                return this._timeout;
            }
        }

        #region Query Members

        public void clearBindings()
        {
            this._queryString.ClearVariables();
        }

        public org.openrdf.query.BindingSet getBindings()
        {
            dotSesameQuery.impl.MapBindingSet s = new org.openrdf.query.impl.MapBindingSet();

            foreach (KeyValuePair<String, INode> kvp in this._queryString.Variables)
            {
                s.addBinding(kvp.Key, SesameConverter.ToSesameValue(kvp.Value, this._mapping));
            }

            return s;
        }

        public org.openrdf.query.Dataset getDataset()
        {
            return null;
        }

        public bool getIncludeInferred()
        {
            return false;
        }

        public int getMaxQueryTime()
        {
            return (int)(this._evalTime / 1000);
        }

        public void removeBinding(string str)
        {
            this._queryString.UnsetVariable(str);
        }

        public void setBinding(string str, org.openrdf.model.Value v)
        {
            this._queryString.SetVariable(str, SesameConverter.FromSesameValue(v, this._mapping));
        }

        public void setDataset(org.openrdf.query.Dataset d)
        {
            //Do Nothing
        }

        public void setIncludeInferred(bool b)
        {
            //DOes Nothing
        }

        public void setMaxQueryTime(int i)
        {
            this._timeout = (long)(i*1000);
        }

        #endregion
    }

    public abstract class BaseGraphQuery : BaseQuery, dotSesameQuery.GraphQuery
    {
        public BaseGraphQuery(String query)
            : base(query) { }

        #region GraphQuery Members

        protected abstract IGraph EvaluateQuery();

        public void evaluate(org.openrdf.rio.RDFHandler rdfh)
        {
            IGraph g = this.EvaluateQuery();
            SesameMapping mapping = new SesameMapping(g, new dotSesame.impl.GraphImpl());

            
            rdfh.startRDF();
            foreach (String prefix in g.NamespaceMap.Prefixes)
            {
                rdfh.handleNamespace(prefix, g.NamespaceMap.GetNamespaceUri(prefix).ToString());
            }
            foreach (Triple t in g.Triples)
            {
                rdfh.handleStatement(SesameConverter.ToSesame(t, mapping));
            }
            rdfh.endRDF();
        }

        public org.openrdf.query.GraphQueryResult evaluate()
        {
            IGraph g = this.EvaluateQuery();
            SesameMapping mapping = new SesameMapping(g, new dotSesame.impl.GraphImpl());
            IEnumerable<dotSesame.Statement> stmts = from t in g.Triples 
                                                     select SesameConverter.ToSesame(t, mapping);

            DotNetEnumerableWrapper wrapper = new DotNetEnumerableWrapper(stmts);
            java.util.HashMap map = new java.util.HashMap();
            foreach (String prefix in g.NamespaceMap.Prefixes)
            {
                map.put(prefix, g.NamespaceMap.GetNamespaceUri(prefix).ToString());
            }
            dotSesameQuery.impl.GraphQueryResultImpl results = new org.openrdf.query.impl.GraphQueryResultImpl(map, wrapper);
            return results;
        }

        #endregion
    }

    public abstract class BaseBooleanQuery : BaseQuery, dotSesameQuery.BooleanQuery
    {
        public BaseBooleanQuery(String query)
            : base(query) { }

        #region BooleanQuery Members

        protected abstract SparqlResultSet EvaluateQuery();

        public bool evaluate()
        {
            SparqlResultSet rset = this.EvaluateQuery();
            return (rset.ResultsType == SparqlResultsType.Boolean && rset.Result);
        }

        #endregion
    }

    public abstract class BaseTupleQuery : BaseQuery, dotSesameQuery.TupleQuery
    {
        public BaseTupleQuery(String query)
            : base(query) { }

        #region TupleQuery Members

        protected abstract SparqlResultSet EvaluateQuery();

        public void evaluate(org.openrdf.query.TupleQueryResultHandler tqrh)
        {
            SparqlResultSet rset = this.EvaluateQuery();

            java.util.ArrayList vars = new java.util.ArrayList();
            foreach (String var in rset.Variables)
            {
                vars.add(var);
            }

            tqrh.startQueryResult(vars);
            SesameMapping mapping = new SesameMapping(new Graph(), new dotSesame.impl.GraphImpl());
            foreach (SparqlResult r in rset)
            {
                dotSesameQuery.impl.MapBindingSet binding = new org.openrdf.query.impl.MapBindingSet();
                foreach (String var in r.Variables)
                {
                    binding.addBinding(var, SesameConverter.ToSesameValue(r[var], mapping));
                }
                tqrh.handleSolution(binding);
            }

            tqrh.endQueryResult();
        }

        public org.openrdf.query.TupleQueryResult evaluate()
        {
            dotSesameQuery.impl.TupleQueryResultBuilder builder = new org.openrdf.query.impl.TupleQueryResultBuilder();
            this.evaluate(builder);

            return builder.getQueryResult();
        }

        #endregion
    }
}
