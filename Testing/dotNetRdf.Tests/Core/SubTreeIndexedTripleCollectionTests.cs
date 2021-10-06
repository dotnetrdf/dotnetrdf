using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDS.RDF
{
    public class SubTreeIndexedTripleCollectionTests : AbstractTripleCollectionTests
    {
        protected override BaseTripleCollection GetInstance()
        {
            return new SubTreeIndexedTripleCollection();
        }
    }
}
