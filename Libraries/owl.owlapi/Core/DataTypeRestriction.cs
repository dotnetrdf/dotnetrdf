using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.OWL
{
    public class DataTypeRestriction : BaseDataRange
    {
        public IDataType DataType { get; protected set; }

        public IEnumerable<FacetRestriction> Restrictions { get; protected set; }
    }
}
