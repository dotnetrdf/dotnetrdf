using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Utilities.Convert.Inputs
{
    abstract class BaseInput : IConversionInput
    {
        public BaseInput()
        { }

        public BaseInput(IRdfHandler handler)
        {
            this.ConversionHandler = handler;
        }

        public IRdfHandler ConversionHandler
        {
            get; set;
        }

        public abstract void Convert();
    }
}
