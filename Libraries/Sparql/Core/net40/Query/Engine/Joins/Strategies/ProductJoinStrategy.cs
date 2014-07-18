using System.Collections.Generic;
using VDS.RDF.Query.Engine.Joins.Workers;

namespace VDS.RDF.Query.Engine.Joins.Strategies
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
