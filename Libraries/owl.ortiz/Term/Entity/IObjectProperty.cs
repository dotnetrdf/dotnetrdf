using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.OWL.Term.Axiom;
using VDS.OWL.Term.Query;

namespace VDS.OWL.Term.Entity
{
    public interface IObjectProperty
        : IProperty
    {
        IAsymmetric CreateAsymmetric();

        IDirectSubObjectPropertyOf CreateDirectSubPropertyOf(IObjectProperty p);

        IDisjointObjectProperties CreateDisjointWith(IObjectProperty p);

        IObjectPropertyDomain CreateDomain(IClassExpression domain);

        IEquivalentObjectProperties CreateEquivalentTo(IObjectProperty p);

        IObjectCardinality CreateExactCardinality(int cardinality);

        IObjectCardinality CreateExactCardinality(int cardinality, IDataType t);

        IObjectFunctional CreateFunctional();

        IObjectProperty CreateInverse();

        IInverseFunctional CreateInverseFunctional();

        IInverseProperties CreateInverseOf(IObjectProperty p);

        IIrreflexive CreateIrreflexive();

        bool IsInverse
        {
            get;
        }

        IObjectMax CreateMaxCardinality(int cardinality);

        IObjectMax CreateMaxCardinality(int cardinality, IDataType t);

        IObjectMin CreateMinCardinality(int cardinality);

        IObjectMin CreateMinCardinality(int cardinality, IDataType t);

        IObjectAll CreateOnly(IClassExpression c);

        IObjectPropertyRange CreateRange(IClassExpression range);

        IReflexive CreateReflexive();

        IObjectSelf CreateSelf();

        IObjectSome CreateSome(IClassExpression c);

        IStrictSubObjectPropertyOf CreateStrictSubPropertyOf(IObjectProperty p);

        ISubObjectPropertyOf CreateSubPropertyOf(IObjectProperty p);

        ISymmetric CreateSymmetric();

        ITransitive CreateTransitive();

        IObjectHasValue CreateValue(IIndividualExpression i);
    }

    public interface IInverseObjectProperty
        : IObjectProperty
    {
        IObjectProperty Property
        {
            get;
        }
    }

    public interface IObjectPropertyVariable
        : INamedTerm, IObjectProperty, IVariable
    {

    }
}
