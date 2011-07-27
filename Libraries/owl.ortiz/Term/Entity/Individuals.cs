using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.OWL.Term.Entity
{
    public interface IAnonymousIndividual
        : IIndividual
    {

    }

    public interface IAnonymousIndividualVariable
        : IIndividualExpression, IVariable
    {

    }

    public interface INamedIndividual
        : IIndividual
    {

    }
}
