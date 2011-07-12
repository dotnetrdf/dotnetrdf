using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Storage;
using VDS.RDF.Update;

namespace VDS.RDF.Query.Optimisation
{
    public class AdoOptimiser<TConn, TCommand, TParameter, TAdaptor, TException> 
        : IAlgebraOptimiser
        where TConn : DbConnection
        where TCommand : DbCommand
        where TParameter : DbParameter
        where TAdaptor : DbDataAdapter
        where TException : DbException
    {
        private List<IAlgebraOptimiser> _optimisers = new List<IAlgebraOptimiser>();

        public AdoOptimiser(BaseAdoStore<TConn, TCommand, TParameter, TAdaptor, TException> manager)
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
}
