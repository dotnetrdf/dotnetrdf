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
    class BaseQuery : dotSesameQuery.Query
    {
        private static SparqlQueryParser _parser;

        private SparqlParameterizedString _queryString;

        public BaseQuery(String query)
        {
            this._queryString = new SparqlParameterizedString(query);
        }

        private static SparqlQuery ParseQuery(String query)
        {
            if (_parser == null) _parser = new SparqlQueryParser();

            return _parser.ParseFromString(query);
        }

        #region Query Members

        public void clearBindings()
        {
            throw new NotImplementedException();
        }

        public org.openrdf.query.BindingSet getBindings()
        {
            throw new NotImplementedException();
        }

        public org.openrdf.query.Dataset getDataset()
        {
            throw new NotImplementedException();
        }

        public bool getIncludeInferred()
        {
            throw new NotImplementedException();
        }

        public int getMaxQueryTime()
        {
            throw new NotImplementedException();
        }

        public void removeBinding(string str)
        {
            throw new NotImplementedException();
        }

        public void setBinding(string str, org.openrdf.model.Value v)
        {
            throw new NotImplementedException();
        }

        public void setDataset(org.openrdf.query.Dataset d)
        {
            throw new NotImplementedException();
        }

        public void setIncludeInferred(bool b)
        {
            throw new NotImplementedException();
        }

        public void setMaxQueryTime(int i)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
