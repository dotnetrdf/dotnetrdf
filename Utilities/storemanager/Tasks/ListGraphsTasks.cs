using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Storage;

namespace VDS.RDF.Utilities.StoreManager.Tasks
{
    public class ListGraphsTasks : NonCancellableTask<IEnumerable<Uri>>
    {
        private IGenericIOManager _manager;

        public ListGraphsTasks(IGenericIOManager manager)
            : base("List Graphs")
        {
            this._manager = manager;
        }

        protected override IEnumerable<Uri> RunTaskInternal()
        {
            if (this._manager.ListGraphsSupported)
            {
                return this._manager.ListGraphs();
            }
            else
            {
                return Enumerable.Empty<Uri>();
            }
        }
    }
}
