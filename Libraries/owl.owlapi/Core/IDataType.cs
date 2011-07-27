using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.OWL
{
    public interface IDataType : IEntity
    {

    }

    public class DataType : BaseDataRange, IDataType
    {
        public Iri EntityIri { get; protected set; }
    }
}
