using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDS.RDF
{
    public class TripleCollectionTests: AbstractTripleCollectionTests
    {
        protected override BaseTripleCollection GetInstance()
        {
            return new TripleCollection();
        }
    }
}
