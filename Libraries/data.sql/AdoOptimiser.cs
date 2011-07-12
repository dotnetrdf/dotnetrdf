using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Storage;
using VDS.RDF.Update;

namespace VDS.RDF.Query.Optimisation
{
    public abstract class BaseAdoOptimiser<TConn, TCommand, TParameter, TAdaptor, TException> 
        : IAlgebraOptimiser
        where TConn : DbConnection
        where TCommand : DbCommand
        where TParameter : DbParameter
        where TAdaptor : DbDataAdapter
        where TException : DbException
    {
        private List<IAlgebraOptimiser> _optimisers = new List<IAlgebraOptimiser>();

        public BaseAdoOptimiser(BaseAdoStore<TConn, TCommand, TParameter, TAdaptor, TException> manager)
        {
            this._optimisers.Add(new SimpleVirtualAlgebraOptimiser(manager));
        }

        public ISparqlAlgebra Optimise(ISparqlAlgebra algebra)
        {
            foreach (IAlgebraOptimiser opt in this._optimisers)
            {
                algebra = opt.Optimise(algebra);
            }
            return algebra;
        }

        public bool IsApplicable(SparqlQuery q)
        {
            return true;
        }

        public bool IsApplicable(SparqlUpdateCommandSet cmds)
        {
            return true;
        }
    }

    public abstract class BaseAdoSqlClientOptimiser
        : BaseAdoOptimiser<SqlConnection, SqlCommand, SqlParameter, SqlDataAdapter, SqlException>
    {
        public BaseAdoSqlClientOptimiser(BaseAdoStore<SqlConnection, SqlCommand, SqlParameter, SqlDataAdapter, SqlException> manager)
            : base(manager) { }
    }

    public class MicrosoftAdoOptimiser
        : BaseAdoSqlClientOptimiser
    {
        public MicrosoftAdoOptimiser(MicrosoftAdoManager manager)
            : base(manager) { }
    }
}
