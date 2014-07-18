using System.Collections.Generic;
using VDS.RDF.Query.Engine.Join.Workers;

namespace VDS.RDF.Query.Engine.Join.Strategies
{
    public class ProductJoinStrategy
        : BaseJoinStrategy
    {
        public override IJoinWorker PrepareWorker(IEnumerable<ISet> rhs)
        {
            return new ProductJoinWorker(rhs);
        }
    }
}
