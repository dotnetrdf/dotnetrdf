using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.OWL.Term.Entity;

namespace VDS.OWL.Term.Axiom
{
    public interface IDataTypeDefinition
        : IAxiom
    {
        INamedDataType DataType
        {
            get;
        }

        IDataType Definition
        {
            get;
        }
    }
}
