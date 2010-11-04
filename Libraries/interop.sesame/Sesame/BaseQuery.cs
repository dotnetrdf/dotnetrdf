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

        private int _timeout = -1;

        public BaseQuery(String query)
        {
            this._queryString = new SparqlParameterizedString(query);
        }

        private static SparqlQuery ParseQuery(String query)
        {
            if (_parser == null) _parser = new SparqlQueryParser();

            return _parser.ParseFromString(query);
        }

        internal int QueryTimeout
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

        public abstract int getMaxQueryTime();

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
            this._timeout = i;
        }

        #endregion
    }

    public abstract class GraphQuery : BaseQuery, dotSesameQuery.GraphQuery
    {
        public GraphQuery(String query)
            : base(query) { }

        public override int getMaxQueryTime()
        {
            throw new NotImplementedException();
        }

        #region GraphQuery Members

        protected abstract IGraph EvaluateQuery();

        public void evaluate(org.openrdf.rio.RDFHandler rdfh)
        {
            throw new NotImplementedException();
        }

        public org.openrdf.query.GraphQueryResult evaluate()
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public abstract class BooleanQuery : BaseQuery, dotSesameQuery.BooleanQuery
    {
        public BooleanQuery(String query)
            : base(query) { }

        public override int getMaxQueryTime()
        {
            throw new NotImplementedException();
        }

        #region BooleanQuery Members

        public bool evaluate()
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public abstract class TupleQuery : BaseQuery, dotSesameQuery.TupleQuery
    {
        public TupleQuery(String query)
            : base(query) { }

        public override int getMaxQueryTime()
        {
            throw new NotImplementedException();
        }

        #region TupleQuery Members

        public void evaluate(org.openrdf.query.TupleQueryResultHandler tqrh)
        {
            throw new NotImplementedException();
        }

        public org.openrdf.query.TupleQueryResult evaluate()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
