using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.OWL.Term.Entity;

namespace VDS.OWL.Term.Axiom
{
    public interface IDataFunctional
        : IFunctional<IDataProperty>
    {

    }

    public interface IObjectFunctional
        : IFunctional<IObjectProperty>
    {

    }
}
