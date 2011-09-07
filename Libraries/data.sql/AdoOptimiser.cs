/*

Copyright Robert Vesse 2009-11
rvesse@vdesign-studios.com

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Storage;
using VDS.RDF.Update;

namespace VDS.RDF.Query.Optimisation
{
    /// <summary>
    /// An abstract algebra optimiser implementation that optimises the algebra to use Virtual Node terms in place of Node terms where the Virtual Nodes originate from an ADO Store
    /// </summary>
    /// <typeparam name="TConn">Connection Type</typeparam>
    /// <typeparam name="TCommand">Command Type</typeparam>
    /// <typeparam name="TParameter">Parameter Type</typeparam>
    /// <typeparam name="TAdaptor">Adaptor Type</typeparam>
    /// <typeparam name="TException">Exception Type</typeparam>
    public abstract class BaseAdoOptimiser<TConn, TCommand, TParameter, TAdaptor, TException> 
        : IAlgebraOptimiser
        where TConn : DbConnection
        where TCommand : DbCommand
        where TParameter : DbParameter
        where TAdaptor : DbDataAdapter
        where TException : DbException
    {
        private List<IAlgebraOptimiser> _optimisers = new List<IAlgebraOptimiser>();

        /// <summary>
        /// Creates a new Base ADO Optimiser
        /// </summary>
        /// <param name="manager">ADO Store Manager</param>
        public BaseAdoOptimiser(BaseAdoStore<TConn, TCommand, TParameter, TAdaptor, TException> manager)
        {
            this._optimisers.Add(new SimpleVirtualAlgebraOptimiser(manager));
        }

        /// <summary>
        /// Optimises the Algebra for evaluation against an ADO Store
        /// </summary>
        /// <param name="algebra">Algebra</param>
        /// <returns></returns>
        public ISparqlAlgebra Optimise(ISparqlAlgebra algebra)
        {
            foreach (IAlgebraOptimiser opt in this._optimisers)
            {
                algebra = opt.Optimise(algebra);
            }
            return algebra;
        }

        /// <summary>
        /// Returns that the optimiser is applicable to all queries
        /// </summary>
        /// <param name="q">Query</param>
        /// <returns></returns>
        public bool IsApplicable(SparqlQuery q)
        {
            return true;
        }

        /// <summary>
        /// Returns that the optimiser is applicable to all updates
        /// </summary>
        /// <param name="cmds">Updates</param>
        /// <returns></returns>
        public bool IsApplicable(SparqlUpdateCommandSet cmds)
        {
            return true;
        }
    }

    /// <summary>
    /// An abstract implementation of the ADO Optimiser for System.Data.SqlClient based ADO Store implementations
    /// </summary>
    public abstract class BaseAdoSqlClientOptimiser
        : BaseAdoOptimiser<SqlConnection, SqlCommand, SqlParameter, SqlDataAdapter, SqlException>
    {
        /// <summary>
        /// Creates a new Base ADO SQL Client Optimiser
        /// </summary>
        /// <param name="manager">ADO SQL Client Store Manager</param>
        public BaseAdoSqlClientOptimiser(BaseAdoStore<SqlConnection, SqlCommand, SqlParameter, SqlDataAdapter, SqlException> manager)
            : base(manager) { }
    }

    /// <summary>
    /// An implementation of the ADO Optimiser for Microsoft SQL Server based ADO Store implementations
    /// </summary>
    public class MicrosoftAdoOptimiser
        : BaseAdoSqlClientOptimiser
    {
        /// <summary>
        /// Creates a new Microsoft SQL Server ADO Optimiser
        /// </summary>
        /// <param name="manager">Microsoft SQL Server ADO Store Manager</param>
        public MicrosoftAdoOptimiser(MicrosoftAdoManager manager)
            : base(manager) { }
    }
}
