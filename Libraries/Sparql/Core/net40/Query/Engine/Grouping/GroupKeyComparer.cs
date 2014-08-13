using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Query.Engine.Grouping
{
    public class GroupKeyComparer
        : IEqualityComparer<ISolution>
    {
        public bool Equals(ISolution x, ISolution y)
        {
            throw new NotImplementedException();
        }

        public int GetHashCode(ISolution obj)
        {
            throw new NotImplementedException();
        }
    }
}
