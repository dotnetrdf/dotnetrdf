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
using System.IO;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing;
using VDS.RDF.Storage;

namespace VDS.RDF.Utilities.StoreManager.Tasks
{
    /// <summary>
    /// Task for importing data from a file
    /// </summary>
    public class ImportFileTask 
        : BaseImportTask
    {
        private String _file;

        /// <summary>
        /// Creates a new Import File task
        /// </summary>
        /// <param name="manager">Storage Provider</param>
        /// <param name="file">File to import from</param>
        /// <param name="targetUri">Target Graph URI</param>
        /// <param name="batchSize">Import Batch Size</param>
        public ImportFileTask(IStorageProvider manager, String file, Uri targetUri, int batchSize)
            : base("Import File", manager, targetUri, batchSize)
        {
            this._file = file;
        }

        /// <summary>
        /// Implements the import
        /// </summary>
        /// <param name="handler">Handler</param>
        protected override void ImportUsingHandler(IRdfHandler handler)
        {
            this.Information = "Importing from File " + this._file;
            try
            {
                //Assume a RDF Graph
                IRdfReader reader = MimeTypesHelper.GetParserByFileExtension(MimeTypesHelper.GetTrueFileExtension(this._file));
                FileLoader.Load(handler, this._file, reader);
            }
            catch (RdfParserSelectionException)
            {
                //Assume a RDF Dataset
                FileLoader.LoadDataset(handler, this._file);
            }
        }
    }

    /// <summary>
    /// Task for importing data from a URI
    /// </summary>
    public class ImportUriTask
        : BaseImportTask
    {
        private Uri _u;

        /// <summary>
        /// Creates a new Import URI Task
        /// </summary>
        /// <param name="manager">Storage Provider</param>
        /// <param name="u">URI to import from</param>
        /// <param name="targetUri">Target Graph URI</param>
        /// <param name="batchSize">Import Batch Size</param>
        public ImportUriTask(IStorageProvider manager, Uri u, Uri targetUri, int batchSize)
            : base("Import URI", manager, targetUri, batchSize)
        {
            this._u = u;
        }

        /// <summary>
        /// Implements the import
        /// </summary>
        /// <param name="handler">Handler</param>
        protected override void ImportUsingHandler(IRdfHandler handler)
        {
            this.Information = "Importing from URI " + this._u.AbsoluteUri;
            try
            {
                //Assume a RDF Graph
                UriLoader.Load(handler, this._u);
            }
            catch (RdfParserSelectionException)
            {
                //Assume a RDF Dataset
                UriLoader.LoadDataset(handler, this._u);
            }
        }

        /// <summary>
        /// Gets the Default Target Graph URI
        /// </summary>
        /// <returns></returns>
        protected override Uri GetDefaultTargetUri()
        {
            return this._u;
        }
    }
}
