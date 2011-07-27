using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.OWL
{
    public interface IDataRange
    {
        long Arity { get; }
    }

    public abstract class BaseDataRange
    {
        public long Arity { get; protected set; }
    }
}
