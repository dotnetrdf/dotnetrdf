using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Storage;

namespace VDS.RDF.Utilities.StoreManager.Connections.BuiltIn
{
    public class InMemoryConnectionDefinition
        : BaseConnectionDefinition
    {
        public InMemoryConnectionDefinition()
            : base("In-Memory", "Create a temporary non-persistent in-memory store for testing and experimentation purposes") { }

        protected override IGenericIOManager OpenConnectionInternal()
        {
            return new InMemoryManager();
        }
    }
}
