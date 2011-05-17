using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing;
using VDS.RDF.Storage;
using VDS.RDF.Update;

namespace VDS.RDF.Utilities.StoreManager.Tasks
{
    public class UpdateTask : NonCancellableTask<TaskResult>
    {
        private IGenericIOManager _manager;
        private String _update;
        private SparqlUpdateCommandSet _cmds;

        public UpdateTask(IGenericIOManager manager, String update)
            : base("SPARQL Update")
        {
            this._manager = manager;
            this._update = update;
        }

        protected override TaskResult RunTaskInternal()
        {
            SparqlUpdateParser parser = new SparqlUpdateParser();
            this._cmds = parser.ParseFromString(this._update);
            GenericUpdateProcessor processor = new GenericUpdateProcessor(this._manager);
            processor.ProcessCommandSet(this._cmds);
            return new TaskResult(true);
        }

        public SparqlUpdateCommandSet Updates
        {
            get
            {
                return this._cmds;
            }
        }
    }
}
