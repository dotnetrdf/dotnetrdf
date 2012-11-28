/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
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
        /// <param name="template">Template</param>
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
