using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing;
using VDS.RDF.Storage;

namespace VDS.RDF.Utilities.StoreManager.Tasks
{
    public class ImportFileTask : BaseImportTask
    {
        private String _file;

        public ImportFileTask(IGenericIOManager manager, String file)
            : base("Import File", manager)
        {
            this._file = file;
        }

        protected override void ImportUsingHandler(IRdfHandler handler)
        {
            FileLoader.Load(handler, this._file);
        }

        protected override IGraph ImportUsingGraph()
        {
            Graph g = new Graph();
            FileLoader.Load(g, this._file);
            return g;
        }
    }

    public class ImportUriTask : BaseImportTask
    {
        private Uri _u;

        public ImportUriTask(IGenericIOManager manager, Uri u)
            : base("Import URI", manager)
        {
            this._u = u;
        }

        protected override void ImportUsingHandler(IRdfHandler handler)
        {
            UriLoader.Load(handler, this._u);
        }

        protected override IGraph ImportUsingGraph()
        {
            Graph g = new Graph();
            UriLoader.Load(g, this._u);
            return g;
        }
    }

}
