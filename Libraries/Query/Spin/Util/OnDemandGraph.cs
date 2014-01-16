using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Storage;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Spin.Util
{
    public class OnDemandGraph : ThreadSafeGraph
    {

        private IStorageProvider _storage;
        private HashSet<TriplePattern> _demands = new HashSet<TriplePattern>();

        public OnDemandGraph(IStorageProvider storage) 
            :base()
        {
            _storage = storage;
        }

    }
}
