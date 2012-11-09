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
using System.IO;
using VDS.RDF.Parsing.Handlers;

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
