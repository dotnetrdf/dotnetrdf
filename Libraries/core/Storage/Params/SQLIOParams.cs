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

#if !NO_DATA && !NO_STORAGE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Storage.Params
{
    /// <summary>
    /// Interface for Store Parameters where the Parameter is a <see cref="ISqlIOManager">ISqlIOManager</see>
    /// </summary>
    [Obsolete("The legacy SQL store format and related classes are officially deprecated - please see http://www.dotnetrdf.org?content.asp?pageID=dotNetRDF%20Store#migration for details on upgrading to the new ADO store format", false)]
    public interface ISQLIOParams : IStoreParams 
    {
        /// <summary>
        /// Gets the Manager for the Store
        /// </summary>
        ISqlIOManager Manager {
            get;
        }
    }

    /// <summary>
    /// Store Parameters for Readers &amp; Writers which use SQL based storage where the main Parameter is a <see cref="ISqlIOManager">ISqlIOManager</see>
    /// </summary>
    [Obsolete("The legacy SQL store format and related classes are officially deprecated - please see http://www.dotnetrdf.org?content.asp?pageID=dotNetRDF%20Store#migration for details on upgrading to the new ADO store format", false)]
    public class SqlIOParams : ISQLIOParams
    {
        /// <summary>
        /// A SQL IO Manager
        /// </summary>
        protected ISqlIOManager _manager;
        /// <summary>
        /// Variable indicating the writers overwrite behaviour
        /// </summary>
        protected bool _clearIfExists = false;

        /// <summary>
        /// Creates a new set of Parameters
        /// </summary>
        /// <param name="manager">SQL IO Manager</param>
        public SqlIOParams(ISqlIOManager manager)
        {
            this._manager = manager;
        }

        /// <summary>
        /// Creates a new set of Parameters
        /// </summary>
        /// <param name="manager">SQL IO Manager</param>
        /// <param name="clearIfExists">Whether existing Graphs of the same Uri should be removed from the Store before saving a Graph</param>
        public SqlIOParams(ISqlIOManager manager, bool clearIfExists) : this(manager)
        {
            this._clearIfExists = clearIfExists;
        }

        /// <summary>
        /// Gets the SQL IO Manager
        /// </summary>
        public ISqlIOManager Manager
        {
            get
            {
                return this._manager;
            }
        }

        /// <summary>
        /// Gets/Sets whether Graphs from the Store being saved should completely overwrite an existing Graph of the same Uri in the Store being written to
        /// </summary>
        public bool ClearIfExists
        {
            get
            {
                return this._clearIfExists;
            }
            set
            {
                this._clearIfExists = value;
            }
        }
    }

    /// <summary>
    /// Store Parameters for multi-threaded Readers &amp; Writers which use SQL based storage where the main Parameter is a <see cref="IThreadedSqlIOManager">IThreadedSqlIOManager</see>
    /// </summary>
    [Obsolete("The legacy SQL store format and related classes are officially deprecated - please see http://www.dotnetrdf.org?content.asp?pageID=dotNetRDF%20Store#migration for details on upgrading to the new ADO store format", false)]
    public class ThreadedSqlIOParams : SqlIOParams
    {
        private IThreadedSqlIOManager _threadedmanager;
        private int _threads = 8;

        /// <summary>
        /// Creates a new set of Parameters
        /// </summary>
        /// <param name="manager">Threaded SQL IO Manager</param>
        public ThreadedSqlIOParams(IThreadedSqlIOManager manager)
            : base(manager)
        {
            this._threadedmanager = manager;
        }

        /// <summary>
        /// Creates a new set of Parameters
        /// </summary>
        /// <param name="manager">Threaded SQL IO Manager</param>
        /// <param name="threads">Number of Threads to use</param>
        public ThreadedSqlIOParams(IThreadedSqlIOManager manager, int threads)
            : this(manager)
        {
            if (threads > 0) this._threads = threads;
        }

        /// <summary>
        /// Gets the Threaded SQL IO Manager
        /// </summary>
        public IThreadedSqlIOManager ThreadedManager
        {
            get
            {
                return this._threadedmanager;
            }
        }

        /// <summary>
        /// Gets the number of Threads to use
        /// </summary>
        public int Threads
        {
            get
            {
                return this._threads;
            }
        }
    }
}

#endif