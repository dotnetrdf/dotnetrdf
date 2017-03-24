/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/

using System;
using VDS.RDF.Parsing;
using VDS.RDF.Storage;
using VDS.RDF.Update;

namespace VDS.RDF.Utilities.StoreManager.Tasks
{
    /// <summary>
    /// Task for running SPARQL updates
    /// </summary>
    public class UpdateTask 
        : NonCancellableTask<TaskResult>
    {
        private readonly IStorageProvider _manager;
        private readonly String _update;
        private SparqlUpdateCommandSet _cmds;

        /// <summary>
        /// Creates a new SPARQL Update task
        /// </summary>
        /// <param name="manager">Storage Provider</param>
        /// <param name="update">SPARQL Update</param>
        public UpdateTask(IStorageProvider manager, String update)
            : base("SPARQL Update")
        {
            this._manager = manager;
            this._update = update;
        }

        /// <summary>
        /// Runs the task
        /// </summary>
        /// <returns></returns>
        protected override TaskResult RunTaskInternal()
        {
            SparqlUpdateParser parser = new SparqlUpdateParser();
            this._cmds = parser.ParseFromString(this._update);
            GenericUpdateProcessor processor = new GenericUpdateProcessor(this._manager);
            processor.ProcessCommandSet(this._cmds);
            return new TaskResult(true);
        }

        /// <summary>
        /// Gets the Update Commands
        /// </summary>
        public SparqlUpdateCommandSet Updates
        {
            get
            {
                return this._cmds;
            }
        }
    }
}
