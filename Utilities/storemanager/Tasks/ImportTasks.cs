/*

Copyright Robert Vesse 2009-12
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing;
using VDS.RDF.Storage;

namespace VDS.RDF.Utilities.StoreManager.Tasks
{
    public class ImportFileTask : BaseImportTask
    {
        private String _file;

        public ImportFileTask(IStorageProvider manager, String file, Uri targetUri, int batchSize)
            : base("Import File", manager, targetUri, batchSize)
        {
            this._file = file;
        }

        protected override void ImportUsingHandler(IRdfHandler handler)
        {
            this.Information = "Importing from File " + this._file;
            try
            {
                //Assume a RDF Graph
                IRdfReader reader = MimeTypesHelper.GetParser(MimeTypesHelper.GetMimeTypes(MimeTypesHelper.GetTrueFileExtension(this._file)));
                FileLoader.Load(handler, this._file, reader);
            }
            catch (RdfParserSelectionException)
            {
                //Assume a RDF Dataset
                FileLoader.LoadDataset(handler, this._file);
            }
        }
    }

    public class ImportUriTask : BaseImportTask
    {
        private Uri _u;

        public ImportUriTask(IStorageProvider manager, Uri u, Uri targetUri, int batchSize)
            : base("Import URI", manager, targetUri, batchSize)
        {
            this._u = u;
        }

        protected override void ImportUsingHandler(IRdfHandler handler)
        {
            this.Information = "Importing from URI " + this._u.ToString();
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

        protected override Uri GetDefaultTargetUri()
        {
            return this._u;
        }
    }
}
