/*

Copyright Robert Vesse 2009-10
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

#if !NO_STORAGE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Storage.Params
{
    /// <summary>
    /// Store Parameters for Readers &amp; Writers which communicate with arbitrary stores which are connected to using an <see cref="IGenericIOManager">IGenericIOManager</see>
    /// </summary>
    public class GenericIOParams : IStoreParams
    {
        /// <summary>
        /// Input/Output Manager for the target underlying store
        /// </summary>
        protected IGenericIOManager _manager;
        private int _threads = 1;

        /// <summary>
        /// Creates a new set of Generic IO Parameters using the given <see cref="IGenericIOManager">IGenericIOManager</see>
        /// </summary>
        /// <param name="manager">IO Manager for the underlying Store</param>
        public GenericIOParams(IGenericIOManager manager)
            : this(manager, 1) { }

        /// <summary>
        /// Creates a new set of Generic IO Parameters using the given <see cref="IGenericIOManager">IGenericIOManager</see>
        /// </summary>
        /// <param name="manager">IO Manager for the underlying Store</param>
        /// <param name="threads">Number of Threads to use for operations which can be multi-threaded</param>
        public GenericIOParams(IGenericIOManager manager, int threads)
        {
            this._manager = manager;
            this._threads = 1;
        }

        /// <summary>
        /// Input/Output Manager for communicating with the underlying store
        /// </summary>
        public IGenericIOManager Manager
        {
            get
            {
                return this._manager;
            }
        }

        /// <summary>
        /// Threads to use for multi-threadable operations
        /// </summary>
        public int Threads
        {
            get
            {
                return this._threads;
            }
        }
    }

    /// <summary>
    /// Store Parameters for Readers &amp; Writers which communicate with arbitrary stores which are connected to using an <see cref="IQueryableGenericIOManager">IQueryableGenericIOManager</see>
    /// </summary>
    public class QueryableGenericIOParams : GenericIOParams
    {
        /// <summary>
        /// Creates a new set of Generic IO Parameters using the given <see cref="IQueryableGenericIOManager">IQueryableGenericIOManager</see>
        /// </summary>
        /// <param name="manager">Queryable IO Manager for the underlying Store</param>
        public QueryableGenericIOParams(IQueryableGenericIOManager manager)
            : base(manager) { }

        /// <summary>
        /// Creates a new set of Generic IO Parameters using the given <see cref="IQueryableGenericIOManager">IQueryableGenericIOManager</see>
        /// </summary>
        /// <param name="manager">Queryable IO Manager for the underlying Store</param>
        /// <param name="threads">Number of Threads to use for operations which can be multi-threaded</param>
        public QueryableGenericIOParams(IQueryableGenericIOManager manager, int threads)
            : base(manager, threads) { }

        /// <summary>
        /// Gets the Queryable Input/Output Manager for communicating with the underlying store
        /// </summary>
        public IQueryableGenericIOManager QueryableManager
        {
            get
            {
                return (IQueryableGenericIOManager)this._manager;
            }
        }
    }
}

#endif