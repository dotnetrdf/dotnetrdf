using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.OWL
{
    public interface ILiteral : IAnnotationValue
    {
        IDataType DataType
        {
            get;
        }

        String Value
        {
            get;
        }
    }

    public abstract class BaseLiteral
    {
        public IDataType DataType { get; protected set; }

        public String Value { get; protected set; }
    }


}
