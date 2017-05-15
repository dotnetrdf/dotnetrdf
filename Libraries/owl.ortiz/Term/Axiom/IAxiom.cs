using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.OWL.Term.Entity;

namespace VDS.OWL.Term.Axiom
{
    public interface IAxiom : ITerm
    {
    }

    public interface IAsymmetric
        : IUnaryPropertyAxiom<IObjectProperty>
    {

    }

    public interface IIrreflexive
        : IUnaryPropertyAxiom<IObjectProperty>
    {

    }

    public interface IReflexive
        : IUnaryPropertyAxiom<IObjectProperty>
    {

    }

    public interface ISymmetric
        : IUnaryPropertyAxiom<IObjectProperty>
    {

    }

    public interface ITransitive
        : IUnaryPropertyAxiom<IObjectProperty>
    {

    }
}

