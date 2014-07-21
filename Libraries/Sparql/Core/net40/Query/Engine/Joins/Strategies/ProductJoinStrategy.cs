using System.Collections.Generic;
using VDS.RDF.Query.Engine.Joins.Workers;

namespace VDS.RDF.Query.Engine.Joins.Strategies
{
    public class ProductJoinStrategy
        : BaseJoinStrategy
    {
        public override IJoinWorker PrepareWorker(IEnumerable<ISolution> rhs)
        {
            return new ProductJoinWorker(rhs);
        }
    }
}
