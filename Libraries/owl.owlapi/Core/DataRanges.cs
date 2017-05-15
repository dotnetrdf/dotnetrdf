using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.OWL
{
    public class DataComplementOf : BaseDataRange
    {
        public IDataRange ComplementOf { get; protected set; }
    }

    public class DataUnionOf : BaseDataRange
    {
        public IEnumerable<IDataRange> UnionOf { get; protected set; }
    }

    public class DataOneOf : BaseDataRange
    {
        public IEnumerable<ILiteral> Values { get; protected set; }
    }

    public class DataInsersectionOf : BaseDataRange
    {
        public IEnumerable<IDataRange> IntersectionOf { get; protected set; }
    }
}
