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
using VDS.RDF.Storage;

namespace VDS.RDF.Utilities.StoreManager.Tasks
{
    /// <summary>
    /// Task for deleting a Graph from a Store
    /// </summary>
    public class DeleteGraphTask
        : NonCancellableTask<TaskResult>
    {
        private readonly IStorageProvider _manager;
        private readonly String _graphUri;

        /// <summary>
        /// Creates a new Delete Graph task
        /// </summary>
        /// <param name="manager">Storage Provider</param>
        /// <param name="graphUri">Graph URI</param>
        public DeleteGraphTask(IStorageProvider manager, String graphUri)
            : base("Delete Graph")
        {
            this._manager = manager;
            this._graphUri = graphUri;
        }

        /// <summary>
        /// Runs the Task
        /// </summary>
        /// <returns></returns>
        protected override TaskResult RunTaskInternal()
        {
            if (this._graphUri != null && !this._graphUri.Equals(String.Empty))
            {
                this.Information = "Deleting Graph " + this._graphUri + "...";
            }
            else
            {
                this.Information = "Deleting Default Graph...";
            }
            this._manager.DeleteGraph(this._graphUri);
            if (this._graphUri != null && !this._graphUri.Equals(String.Empty))
            {
                this.Information = "Deleted Graph " + this._graphUri + " OK";
            }
            else
            {
                this.Information = "Deleted Default Graph OK";
            }

            return new TaskResult(true);
        }
    }
}
