using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace VDS.Alexandria.Documents
{
    public class FileDocument : BaseDocument<StreamReader, TextWriter>
    {
        private String _filename;

        public FileDocument(String filename, String name, IDocumentManager<StreamReader,TextWriter> manager)
            : base(name, manager)
        {
            this._filename = filename;
        }

        public override bool Exists
        {
            get 
            {
                return File.Exists(this._filename); 
            }
        }

        protected override TextWriter BeginWriteInternal(bool append)
        {
            if (append)
            {
                return new StreamWriter(File.Open(this._filename, FileMode.Append));
            }
            else
            {
                return new StreamWriter(File.Open(this._filename, FileMode.Create));
            }
        }

        protected override StreamReader BeginReadInternal()
        {
            return new StreamReader(File.OpenRead(this._filename));
        }
    }
}
