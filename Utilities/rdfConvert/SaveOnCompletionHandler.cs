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
using System.IO;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Storage.Params;

namespace VDS.RDF.Utilities.Convert
{
    class SaveOnCompletionHandler : GraphHandler
    {
        private IRdfWriter _writer;
        private String _file;
        private TextWriter _textWriter;

        public SaveOnCompletionHandler(IRdfWriter writer, String file)
            : base(new Graph())
        {
            if (writer == null) throw new ArgumentNullException("writer", "Must specify a RDF Writer to use when the Handler completes RDF handling");
            if (file == null) throw new ArgumentNullException("file", "Cannot save RDF to a null file");
            this._writer = writer;
            this._file = file;
        }

        public SaveOnCompletionHandler(IRdfWriter writer, TextWriter textWriter)
            : base(new Graph())
        {
            if (writer == null) throw new ArgumentNullException("writer", "Must specify a RDF Writer to use when the Handler completes RDF handling");
            if (textWriter == null) throw new ArgumentNullException("textWriter", "Cannot save RDF to a null TextWriter");
            this._writer = writer;
            this._textWriter = textWriter;            
        }

        protected override void EndRdfInternal(bool ok)
        {
            base.EndRdfInternal(ok);

            if (ok)
            {
                if (this._textWriter != null)
                {
                    this._writer.Save(this.Graph, this._textWriter);
                }
                else
                {
                    this._writer.Save(this.Graph, this._file);
                }
            }
        }
    }

    class SaveStoreOnCompletionHandler : StoreHandler
    {
        private IStoreWriter _writer;
        private String _file;
        private TextWriter _textWriter;

        public SaveStoreOnCompletionHandler(IStoreWriter writer, String file)
            : base(new TripleStore())
        {
            if (writer == null) throw new ArgumentNullException("writer", "Must specify a RDF Dataset Writer to use when the Handler completes RDF handling");
            if (file == null) throw new ArgumentNullException("file", "Cannot save RDF to a null file");
            this._writer = writer;
            this._file = file;
        }

        public SaveStoreOnCompletionHandler(IStoreWriter writer, TextWriter textWriter)
            : base(new TripleStore())
        {
            if (writer == null) throw new ArgumentNullException("writer", "Must specify a RDF Dataset Writer to use when the Handler completes RDF handling");
            if (textWriter == null) throw new ArgumentNullException("textWriter", "Cannot save RDF to a null TextWriter");
            this._writer = writer;
            this._textWriter = textWriter;            
        }

        protected override void EndRdfInternal(bool ok)
        {
            base.EndRdfInternal(ok);

            if (ok)
            {
                if (this._textWriter != null)
                {
                    this._writer.Save(this.Store, this._textWriter);
                }
                else
                {
                    this._writer.Save(this.Store, this._file);
                }
            }
        }
    }
}
