/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Storage;
using VDS.RDF.Storage.Management;
using VDS.RDF.Storage.Management.Provisioning;

namespace VDS.RDF.Utilities.StoreManager.Tasks
{
    /// <summary>
    /// Task for creating a new Store
    /// </summary>
    public class CreateStoreTask
        : NonCancellableTask<TaskValueResult<bool>>
    {
        private IStorageServer _server;
        private IStoreTemplate _template;

        /// <summary>
        /// Creates a task for creating a Store
        /// </summary>
        /// <param name="server">Server</param>
        /// <param name="id"></param>
        public CreateStoreTask(IStorageServer server, IStoreTemplate template)
            : base("Create Store")
        {
            this._server = server;
            this._template = template;
        }

        /// <summary>
        /// Runs the task
        /// </summary>
        /// <returns></returns>
        protected override TaskValueResult<bool> RunTaskInternal()
        {
            this.Information = "Creating Store " + this._template.ID + "...";
            if (this._server != null)
            {
                return new TaskValueResult<bool>(this._server.CreateStore(this._template));
            }
            else
            {
                throw new RdfStorageException("Creating a store is unsupported");
            }
        }
    }
}
